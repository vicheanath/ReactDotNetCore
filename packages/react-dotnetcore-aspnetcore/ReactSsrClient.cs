using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ReactDotNetCore;

/// <summary>Result of a server-side render performed by the Node sidecar.</summary>
public sealed record SsrRenderResult(string Html);

/// <summary>
/// Typed HTTP client that asks the Node SSR sidecar to render a component to HTML.
/// The props are sent as raw, pre-serialized JSON so that the bytes used for SSR are byte-identical
/// to the bytes embedded in the page for hydration.
/// </summary>
public sealed class ReactSsrClient
{
    private readonly HttpClient _http;

    public ReactSsrClient(HttpClient http, ReactDotNetCoreOptions options)
    {
        _http = http;
        _http.BaseAddress ??= options.SidecarBaseAddress;
    }

    /// <summary>Render <paramref name="component"/> with the given pre-serialized props JSON.</summary>
    public async Task<SsrRenderResult> RenderAsync(string component, string propsJson, CancellationToken ct = default)
    {
        // Build {"component": "...", "props": <raw propsJson>} without re-serializing the props.
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("component", component);
            writer.WritePropertyName("props");
            using (var doc = JsonDocument.Parse(propsJson))
                doc.RootElement.WriteTo(writer);
            writer.WriteEndObject();
        }

        var content = new ByteArrayContent(buffer.ToArray());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

        using var response = await _http.PostAsync("render", content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new ReactSsrException($"SSR sidecar returned {(int)response.StatusCode} for component '{component}': {body}");

        using var resultDoc = JsonDocument.Parse(body);
        var root = resultDoc.RootElement;
        if (root.TryGetProperty("error", out var err))
            throw new ReactSsrException($"SSR failed for component '{component}': {err.GetString()}");

        var html = root.GetProperty("html").GetString() ?? "";
        return new SsrRenderResult(html);
    }

    /// <summary>Poll the sidecar health endpoint until it responds or the deadline passes.</summary>
    public async Task<bool> WaitForReadyAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                using var resp = await _http.GetAsync("health", ct);
                if (resp.IsSuccessStatusCode) return true;
            }
            catch (HttpRequestException) { /* sidecar not up yet */ }
            await Task.Delay(200, ct);
        }
        return false;
    }
}

public sealed class ReactSsrException : Exception
{
    public ReactSsrException(string message) : base(message) { }
}
