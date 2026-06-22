using Microsoft.Extensions.Logging.Abstractions;
using ReactDotNetCore.Tests.TestSupport;

namespace ReactDotNetCore.Tests;

public class ViteManifestProviderTests : IDisposable
{
    private readonly string _root;

    public ViteManifestProviderTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "rdc-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
    }

    private ViteManifestProvider Provider(string? manifestJson)
    {
        var options = new ReactDotNetCoreOptions
        {
            ClientManifest = "wwwroot/react-dotnetcore/client/.vite/manifest.json",
        };
        if (manifestJson is not null)
        {
            var path = Path.Combine(_root, options.ClientManifest.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, manifestJson);
        }
        var env = new FakeWebHostEnvironment { ContentRootPath = _root };
        return new ViteManifestProvider(options, env, NullLogger<ViteManifestProvider>.Instance);
    }

    [Fact]
    public void ResolveEntry_returns_file_and_transitive_css()
    {
        var provider = Provider("""
            {
              "entry-client.tsx": { "file": "assets/app.js", "css": ["assets/app.css"], "imports": ["_shared.js"], "isEntry": true },
              "_shared.js": { "file": "assets/shared.js", "css": ["assets/shared.css"] }
            }
            """);

        var assets = provider.ResolveEntry();

        Assert.Equal("assets/app.js", assets.File);
        Assert.Contains("assets/app.css", assets.Css);
        Assert.Contains("assets/shared.css", assets.Css); // pulled in via imports
    }

    [Fact]
    public void ResolveEntry_throws_when_the_entry_key_is_missing()
    {
        var provider = Provider("""{ "other.tsx": { "file": "assets/other.js" } }""");
        Assert.Throws<KeyNotFoundException>(() => provider.ResolveEntry());
    }

    [Fact]
    public void ResolveEntry_throws_when_the_manifest_file_is_missing()
    {
        var provider = Provider(manifestJson: null); // no file written
        Assert.Throws<FileNotFoundException>(() => provider.ResolveEntry());
    }
}
