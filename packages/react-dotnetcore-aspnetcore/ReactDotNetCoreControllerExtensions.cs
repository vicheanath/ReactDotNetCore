using Microsoft.AspNetCore.Mvc;

namespace ReactDotNetCore;

/// <summary>
/// Controller-facing entry points for rendering React views, mirroring the built-in
/// <c>View(...)</c> helpers so migration from Razor stays a one-line change.
/// </summary>
public static class ReactDotNetCoreControllerExtensions
{
    /// <summary>Render the React component named <paramref name="component"/> with <paramref name="model"/>.</summary>
    public static ReactDotNetCoreResult ReactView(this ControllerBase controller, string component, object? model = null)
        => new(component, model);

    /// <summary>
    /// Render the React component identified by the marker type <typeparamref name="TComponent"/>
    /// (component name = type name), e.g. <c>ReactDotNetCore&lt;UserProfile&gt;(model)</c>.
    /// </summary>
    public static ReactDotNetCoreResult ReactView<TComponent>(this ControllerBase controller, object? model = null)
        where TComponent : IReactComponent
        => new(typeof(TComponent).Name, model);
}
