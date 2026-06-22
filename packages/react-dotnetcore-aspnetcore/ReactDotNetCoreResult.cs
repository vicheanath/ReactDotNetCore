using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ReactDotNetCore;

/// <summary>
/// An <see cref="ActionResult"/> that renders a React component as the response: the model is
/// serialized to props, server-rendered via the Node sidecar, and returned as a hydratable HTML page.
/// </summary>
public sealed class ReactDotNetCoreResult : ActionResult
{
    public string Component { get; }
    public object? Model { get; }
    public string? Title { get; set; }

    public ReactDotNetCoreResult(string component, object? model)
    {
        if (string.IsNullOrWhiteSpace(component))
            throw new ArgumentException("Component name must be provided.", nameof(component));
        Component = component;
        Model = model;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var http = context.HttpContext;
        var renderer = http.RequestServices.GetRequiredService<ReactDotNetCoreRenderer>();

        var html = await renderer.RenderPageAsync(Component, Model, Title, http.RequestAborted);

        http.Response.ContentType = "text/html; charset=utf-8";
        await http.Response.WriteAsync(html, http.RequestAborted);
    }
}
