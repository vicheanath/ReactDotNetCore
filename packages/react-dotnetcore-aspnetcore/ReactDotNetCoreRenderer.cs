using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReactDotNetCore;

/// <summary>
/// Assembles a complete HTML document for a React view: serialize model -> props, run SSR via the
/// sidecar, then wire up client hydration. In production it links the hashed Vite assets from the
/// manifest; in development it loads modules + HMR from the Vite dev server origin.
/// </summary>
public sealed class ReactDotNetCoreRenderer
{
    private readonly ReactSsrClient _ssr;
    private readonly ViteManifestProvider _manifest;
    private readonly ReactDotNetCoreOptions _options;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ReactDotNetCoreRenderer> _logger;

    public ReactDotNetCoreRenderer(
        ReactSsrClient ssr,
        ViteManifestProvider manifest,
        ReactDotNetCoreOptions options,
        IWebHostEnvironment env,
        ILogger<ReactDotNetCoreRenderer> logger)
    {
        _ssr = ssr;
        _manifest = manifest;
        _options = options;
        _env = env;
        _logger = logger;
    }

    private bool IsDev => _options.DevMode ?? _env.IsDevelopment();

    public async Task<string> RenderPageAsync(string component, object? model, string? title, CancellationToken ct = default)
    {
        var propsJson = JsonSerializer.Serialize(model, _options.JsonOptions);

        string ssrHtml;
        try
        {
            var result = await _ssr.RenderAsync(component, propsJson, ct);
            ssrHtml = result.Html;
        }
        catch (Exception ex)
        {
            // Log full detail server-side; the exception propagates so the app's configured error
            // handling decides what (if anything) the client sees — no SSR internals leak in prod.
            _logger.LogError(ex, "React SSR failed for component {Component}.", component);
            throw;
        }

        var sb = new StringBuilder(propsJson.Length + ssrHtml.Length + 1024);
        sb.Append("<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n");
        sb.Append("<meta charset=\"utf-8\" />\n");
        sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />\n");
        sb.Append("<title>").Append(HtmlEncoder.Default.Encode(title ?? component)).Append("</title>\n");

        if (!IsDev)
        {
            var assets = _manifest.ResolveEntry();
            var basePath = _options.PublicBasePath.TrimEnd('/');
            foreach (var css in assets.Css)
                sb.Append("<link rel=\"stylesheet\" href=\"").Append(basePath).Append('/').Append(css).Append("\" />\n");
            sb.Append("</head>\n<body>\n");
            AppendRoot(sb, component, ssrHtml, propsJson);
            sb.Append("<script type=\"module\" src=\"").Append(basePath).Append('/').Append(assets.File).Append("\"></script>\n");
        }
        else
        {
            // Link the dev CSS as a real (render-blocking) stylesheet so first paint is styled —
            // avoids the flash of unstyled content from Vite's JS-injected CSS. Vite serves the
            // compiled CSS via its ?direct query.
            var origin = _options.DevServerOrigin;
            foreach (var sheet in _options.DevStylesheets)
                sb.Append("<link rel=\"stylesheet\" href=\"").Append(origin).Append('/')
                  .Append(sheet.TrimStart('/')).Append("?direct\" />\n");
            sb.Append("</head>\n<body>\n");
            AppendRoot(sb, component, ssrHtml, propsJson);
            AppendDevScripts(sb);
        }

        sb.Append("</body>\n</html>");
        return sb.ToString();
    }

    private static void AppendRoot(StringBuilder sb, string component, string ssrHtml, string propsJson)
    {
        sb.Append("<div id=\"react-dotnetcore-root\" data-react-dotnetcore-component=\"")
          .Append(HtmlEncoder.Default.Encode(component)).Append("\">");
        sb.Append(ssrHtml);
        sb.Append("</div>\n");
        sb.Append("<script id=\"react-dotnetcore-props\" type=\"application/json\">")
          .Append(EscapeForScript(propsJson))
          .Append("</script>\n");
    }

    /// <summary>Inject the Vite client + React Fast Refresh preamble + entry from the dev origin.</summary>
    private void AppendDevScripts(StringBuilder sb)
    {
        var origin = _options.DevServerOrigin;
        sb.Append("<script type=\"module\">\n")
          .Append("import RefreshRuntime from \"").Append(origin).Append("/@react-refresh\";\n")
          .Append("RefreshRuntime.injectIntoGlobalHook(window);\n")
          .Append("window.$RefreshReg$ = () => {};\n")
          .Append("window.$RefreshSig$ = () => (type) => type;\n")
          .Append("window.__vite_plugin_react_preamble_installed__ = true;\n")
          .Append("</script>\n");
        sb.Append("<script type=\"module\" src=\"").Append(origin).Append("/@vite/client\"></script>\n");
        sb.Append("<script type=\"module\" src=\"").Append(origin).Append('/').Append(_options.ClientEntryKey).Append("\"></script>\n");
    }

    /// <summary>
    /// Neutralize characters that could terminate the host script element or break JS parsing,
    /// emitting JSON-valid \uXXXX escapes. These characters only occur inside JSON string values,
    /// so the output stays valid JSON and round-trips through JSON.parse.
    /// </summary>
    private static string EscapeForScript(string json)
    {
        var sb = new StringBuilder(json.Length);
        foreach (var ch in json)
        {
            switch (ch)
            {
                case '<': sb.Append("\\u003c"); break;
                case '>': sb.Append("\\u003e"); break;
                case '&': sb.Append("\\u0026"); break;
                case '\u2028': sb.Append("\\u2028"); break;
                case '\u2029': sb.Append("\\u2029"); break;
                default: sb.Append(ch); break;
            }
        }
        return sb.ToString();
    }
}
