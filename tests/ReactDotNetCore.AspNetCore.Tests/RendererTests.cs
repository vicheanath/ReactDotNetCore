using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using ReactDotNetCore.Tests.TestSupport;

namespace ReactDotNetCore.Tests;

public class RendererTests : IDisposable
{
    private readonly string _root;

    public RendererTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "rdc-render-" + Guid.NewGuid().ToString("N"));
        var manifestPath = Path.Combine(_root, "wwwroot", "react-dotnetcore", "client", ".vite", "manifest.json");
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        File.WriteAllText(manifestPath, """
            { "entry-client.tsx": { "file": "assets/app.js", "css": ["assets/app.css"], "isEntry": true } }
            """);
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
    }

    private ReactDotNetCoreRenderer Renderer(ReactDotNetCoreOptions options, bool development)
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            StubHttpMessageHandler.Json(HttpStatusCode.OK, """{ "html": "<main>SSR-CONTENT</main>" }"""));
        var ssr = new ReactSsrClient(handler.CreateClient(), options);
        var env = new FakeWebHostEnvironment
        {
            ContentRootPath = _root,
            EnvironmentName = development ? "Development" : "Production",
        };
        var manifest = new ViteManifestProvider(options, env, NullLogger<ViteManifestProvider>.Instance);
        return new ReactDotNetCoreRenderer(ssr, manifest, options, env, NullLogger<ReactDotNetCoreRenderer>.Instance);
    }

    [Fact]
    public async Task Production_page_embeds_ssr_props_and_hashed_assets()
    {
        var options = new ReactDotNetCoreOptions { SidecarPort = 5174 };
        var html = await Renderer(options, development: false)
            .RenderPageAsync("Widget", new { n = 5 }, title: null);

        Assert.Contains("id=\"react-dotnetcore-root\"", html);
        Assert.Contains("data-react-dotnetcore-component=\"Widget\"", html);
        Assert.Contains("<main>SSR-CONTENT</main>", html);
        Assert.Contains("id=\"react-dotnetcore-props\"", html);
        Assert.Contains("/react-dotnetcore/client/assets/app.css", html);
        Assert.Contains("/react-dotnetcore/client/assets/app.js", html);
        Assert.DoesNotContain("@vite/client", html);
        Assert.Contains("<title>Widget</title>", html); // defaults to component name
    }

    [Fact]
    public async Task Props_are_escaped_for_safe_script_embedding()
    {
        var options = new ReactDotNetCoreOptions { SidecarPort = 5174 };
        var html = await Renderer(options, development: false)
            .RenderPageAsync("Widget", new { label = "<b>x</b>", amp = "a&b" }, title: null);

        // The angle brackets and ampersand must not appear literally inside the props <script>;
        // they are emitted as \uXXXX escapes (System.Text.Json uses uppercase hex, our extra pass
        // lowercase — either is valid JSON, so compare case-insensitively).
        Assert.Contains("\\u003cb\\u003ex", html, StringComparison.OrdinalIgnoreCase); // "<b>x"
        Assert.Contains("a\\u0026b", html, StringComparison.OrdinalIgnoreCase);        // "a&b"
    }

    [Fact]
    public async Task Development_page_uses_vite_dev_server_and_hmr()
    {
        var options = new ReactDotNetCoreOptions { SidecarPort = 5174 };
        var html = await Renderer(options, development: true)
            .RenderPageAsync("Widget", new { n = 1 }, title: "MyTitle");

        Assert.Contains("<title>MyTitle</title>", html);
        Assert.Contains("http://localhost:5174/@vite/client", html);
        Assert.Contains("/@react-refresh", html);
        Assert.Contains("http://localhost:5174/entry-client.tsx", html);
        Assert.Contains("<main>SSR-CONTENT</main>", html);
        Assert.DoesNotContain("rel=\"stylesheet\"", html); // no manifest assets in dev
        Assert.DoesNotContain("/react-dotnetcore/client/assets", html);
    }

    [Fact]
    public async Task DevMode_option_overrides_environment()
    {
        var options = new ReactDotNetCoreOptions { SidecarPort = 5174, DevMode = true };
        // Environment is Production, but DevMode=true forces the dev path.
        var html = await Renderer(options, development: false)
            .RenderPageAsync("Widget", new { n = 1 }, title: null);

        Assert.Contains("@vite/client", html);
    }

    [Fact]
    public async Task Dev_stylesheets_are_linked_in_head_to_prevent_fouc()
    {
        var options = new ReactDotNetCoreOptions
        {
            SidecarPort = 5174,
            DevStylesheets = { "styles/globals.css" },
        };
        var html = await Renderer(options, development: true)
            .RenderPageAsync("Widget", new { n = 1 }, title: null);

        // A render-blocking stylesheet pointing at Vite's ?direct CSS, before <body>.
        Assert.Contains("<link rel=\"stylesheet\" href=\"http://localhost:5174/styles/globals.css?direct\" />", html);
        Assert.True(html.IndexOf("rel=\"stylesheet\"", StringComparison.Ordinal) < html.IndexOf("<body", StringComparison.Ordinal));
    }
}
