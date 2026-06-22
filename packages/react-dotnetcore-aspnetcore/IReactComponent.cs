namespace ReactDotNetCore;

/// <summary>
/// Marker interface for a strongly-typed React component reference. A C# type implementing
/// this interface maps to a React component of the same name in the component registry, enabling
/// the <c>ReactDotNetCore&lt;UserProfile&gt;(model)</c> developer experience.
/// </summary>
/// <remarks>
/// In the walking-skeleton these markers are written by hand. A future source generator can emit
/// them automatically (one per <c>*.tsx</c> view) so the set always matches the registry.
/// </remarks>
public interface IReactComponent
{
}
