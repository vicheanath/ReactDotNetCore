using System.Text.Json;

namespace ReactDotNetCore;

/// <summary>
/// Configuration for the ReactDotNetCore engine. Relative paths are resolved against the
/// application's ContentRootPath at runtime.
/// </summary>
public sealed class ReactDotNetCoreOptions
{
    /// <summary>Path (or command) used to launch Node for the SSR sidecar.</summary>
    public string NodePath { get; set; } = "node";

    /// <summary>Path to the Node SSR sidecar script, relative to ContentRoot.</summary>
    public string SidecarScript { get; set; } = "ssr-server.mjs";

    /// <summary>
    /// The Vite project root (where vite.config and the entry modules live), relative to ContentRoot.
    /// Used as the dev server root in development. Defaults to "." (the content root).
    /// </summary>
    public string ViteRoot { get; set; } = ".";

    /// <summary>Path to the built SSR bundle the sidecar imports (production mode), relative to ContentRoot.</summary>
    public string ServerBundle { get; set; } = "dist/server/entry-server.js";

    /// <summary>
    /// TCP port the SSR sidecar listens on (loopback only). Use <c>0</c> to auto-select a free port
    /// at startup (recommended for production to avoid clashes).
    /// </summary>
    public int SidecarPort { get; set; } = 0;

    /// <summary>Whether the host should spawn and manage the Node sidecar process.</summary>
    public bool LaunchSidecar { get; set; } = true;

    /// <summary>How long to wait for the sidecar health check before giving up.</summary>
    public TimeSpan SidecarStartupTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Per-render timeout for the SSR request to the sidecar.</summary>
    public TimeSpan RenderTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Max times the host will auto-restart the sidecar after an unexpected exit.</summary>
    public int MaxSidecarRestarts { get; set; } = 5;

    /// <summary>
    /// Force development (Vite HMR) rendering on/off. When null, the engine uses
    /// <c>IWebHostEnvironment.IsDevelopment()</c>.
    /// </summary>
    public bool? DevMode { get; set; }

    /// <summary>Browser-facing host for the dev server origin (module + HMR loading).</summary>
    public string DevServerHost { get; set; } = "localhost";

    /// <summary>
    /// CSS entry paths (relative to the Vite root) to link as render-blocking stylesheets in
    /// development, e.g. "styles/globals.css". This prevents the flash of unstyled content that
    /// otherwise occurs because Vite injects CSS via JavaScript after first paint. The styles are
    /// served by Vite as real CSS via its <c>?direct</c> query. In production the manifest provides
    /// the stylesheet links instead, so this is ignored.
    /// </summary>
    public IList<string> DevStylesheets { get; set; } = new List<string>();

    /// <summary>Path to the Vite client build manifest (production), relative to ContentRoot.</summary>
    public string ClientManifest { get; set; } = "wwwroot/react-dotnetcore/client/.vite/manifest.json";

    /// <summary>The Vite manifest key / dev entry module for the client entry.</summary>
    public string ClientEntryKey { get; set; } = "entry-client.tsx";

    /// <summary>Public URL base under which the client build assets are served (production).</summary>
    public string PublicBasePath { get; set; } = "/react-dotnetcore/client";

    /// <summary>Re-read the manifest from disk on every request (useful in Development).</summary>
    public bool ReloadManifestPerRequest { get; set; }

    /// <summary>JSON options used to serialize the model into React props.</summary>
    public JsonSerializerOptions JsonOptions { get; set; } = new(JsonSerializerDefaults.Web);

    /// <summary>Loopback base address the host uses to call the sidecar.</summary>
    public Uri SidecarBaseAddress => new($"http://127.0.0.1:{SidecarPort}/");

    /// <summary>Browser-facing origin of the dev server (Vite modules + HMR) in development.</summary>
    public string DevServerOrigin => $"http://{DevServerHost}:{SidecarPort}";
}
