using System.Net;
using ReactDotNetCore.Tests.TestSupport;

namespace ReactDotNetCore.Tests;

public class ReactSsrClientTests
{
    private static ReactSsrClient Client(StubHttpMessageHandler handler) =>
        new(handler.CreateClient(), new ReactDotNetCoreOptions());

    [Fact]
    public async Task RenderAsync_posts_to_render_and_returns_html()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            StubHttpMessageHandler.Json(HttpStatusCode.OK, """{ "html": "<p>ok</p>" }"""));
        var client = Client(handler);

        var result = await client.RenderAsync("Card", """{"a":1}""");

        Assert.Equal("<p>ok</p>", result.Html);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/render", handler.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task RenderAsync_sends_component_and_raw_props()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            StubHttpMessageHandler.Json(HttpStatusCode.OK, """{ "html": "" }"""));
        var client = Client(handler);

        await client.RenderAsync("Card", """{"a":1,"b":"x"}""");

        Assert.Contains("\"component\":\"Card\"", handler.LastBody);
        Assert.Contains("\"a\":1", handler.LastBody);
        Assert.Contains("\"b\":\"x\"", handler.LastBody);
    }

    [Fact]
    public async Task RenderAsync_throws_on_non_success_status()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            StubHttpMessageHandler.Json(HttpStatusCode.InternalServerError, """{ "error": "kaboom" }"""));
        var client = Client(handler);

        var ex = await Assert.ThrowsAsync<ReactSsrException>(() => client.RenderAsync("Card", "{}"));
        Assert.Contains("Card", ex.Message);
    }

    [Fact]
    public async Task RenderAsync_throws_when_body_contains_an_error()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            StubHttpMessageHandler.Json(HttpStatusCode.OK, """{ "error": "render failed" }"""));
        var client = Client(handler);

        var ex = await Assert.ThrowsAsync<ReactSsrException>(() => client.RenderAsync("Card", "{}"));
        Assert.Contains("render failed", ex.Message);
    }

    [Fact]
    public async Task WaitForReadyAsync_returns_true_when_health_is_ok()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            StubHttpMessageHandler.Text(HttpStatusCode.OK, "ok"));
        var client = Client(handler);

        Assert.True(await client.WaitForReadyAsync(TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public async Task WaitForReadyAsync_returns_false_when_never_healthy()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            throw new HttpRequestException("connection refused"));
        var client = Client(handler);

        Assert.False(await client.WaitForReadyAsync(TimeSpan.FromMilliseconds(400)));
    }
}
