using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace ReactDotNetCore;

/// <summary>A single entry in a Vite build manifest.</summary>
public sealed class ViteManifestChunk
{
    [JsonPropertyName("file")] public string File { get; set; } = "";
    [JsonPropertyName("css")] public string[] Css { get; set; } = Array.Empty<string>();
    [JsonPropertyName("imports")] public string[] Imports { get; set; } = Array.Empty<string>();
    [JsonPropertyName("isEntry")] public bool IsEntry { get; set; }
}

/// <summary>Resolved assets (entry script + transitive CSS) for a client entry.</summary>
public sealed record ViteEntryAssets(string File, IReadOnlyList<string> Css);

/// <summary>
/// Loads and caches the Vite client build manifest, resolving an entry key to its hashed
/// output file plus the full set of CSS files reachable through its import graph.
/// </summary>
public sealed class ViteManifestProvider
{
    private readonly ReactDotNetCoreOptions _options;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ViteManifestProvider> _logger;
    private readonly object _gate = new();

    private Dictionary<string, ViteManifestChunk>? _cache;
    private DateTime _loadedWriteTimeUtc;

    public ViteManifestProvider(ReactDotNetCoreOptions options, IWebHostEnvironment env, ILogger<ViteManifestProvider> logger)
    {
        _options = options;
        _env = env;
        _logger = logger;
    }

    private string ManifestPath => Path.Combine(_env.ContentRootPath, _options.ClientManifest);

    private Dictionary<string, ViteManifestChunk> Load()
    {
        var path = ManifestPath;
        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Vite client manifest not found at '{path}'. Run the frontend build (npm run build) before starting the app.",
                path);

        var writeTime = File.GetLastWriteTimeUtc(path);
        lock (_gate)
        {
            if (_cache is not null && !_options.ReloadManifestPerRequest && writeTime == _loadedWriteTimeUtc)
                return _cache;

            var json = File.ReadAllText(path);
            var parsed = JsonSerializer.Deserialize<Dictionary<string, ViteManifestChunk>>(json)
                         ?? throw new InvalidOperationException($"Vite manifest at '{path}' could not be parsed.");
            _cache = parsed;
            _loadedWriteTimeUtc = writeTime;
            _logger.LogDebug("Loaded Vite manifest with {Count} chunks from {Path}", parsed.Count, path);
            return parsed;
        }
    }

    /// <summary>Resolve the entry's output file and all CSS reachable via its imports.</summary>
    public ViteEntryAssets ResolveEntry(string? entryKey = null)
    {
        var key = entryKey ?? _options.ClientEntryKey;
        var manifest = Load();
        if (!manifest.TryGetValue(key, out var entry))
            throw new KeyNotFoundException(
                $"Entry '{key}' not found in Vite manifest. Available keys: {string.Join(", ", manifest.Keys)}");

        var css = new List<string>();
        var seen = new HashSet<string>();
        CollectCss(key, manifest, css, seen);
        return new ViteEntryAssets(entry.File, css);
    }

    private static void CollectCss(string key, Dictionary<string, ViteManifestChunk> manifest, List<string> css, HashSet<string> seen)
    {
        if (!seen.Add(key) || !manifest.TryGetValue(key, out var chunk)) return;
        foreach (var c in chunk.Css)
            if (!css.Contains(c)) css.Add(c);
        foreach (var imp in chunk.Imports)
            CollectCss(imp, manifest, css, seen);
    }
}
