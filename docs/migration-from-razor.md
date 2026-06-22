# Migrating from Razor

ReactDotNetCore is designed for **incremental** migration. You replace views one at a time; Razor and
React views coexist in the same app, sharing the same controllers, services, and routing.

## The migration unit: one view

A Razor action like this:

```csharp
public IActionResult Detail(int id)
{
    var model = _userService.GetUser(id);
    return View(model);          // Views/Users/Detail.cshtml
}
```

becomes:

```csharp
public IActionResult Detail(int id)
{
    var model = _userService.GetUser(id);
    return this.ReactView<UserProfile>(model);   // Views/UserProfile.tsx
}
```

Nothing else changes — same controller, same service, same model.

## Step-by-step

1. **Add the packages and toolchain** once — see [Getting Started](getting-started.md) steps 2–4.
2. **Pick a leaf view** to convert first (a detail/show page with simple data is ideal).
3. **Translate the `.cshtml` to a `.tsx`** under `Views/`. Razor `@Model.Name` becomes `props.name`.
4. **Add a marker** (`public sealed class UserProfile : IReactComponent;`) or use the string overload.
5. **Switch the action** from `View(model)` to `this.ReactView<UserProfile>(model)`.
6. **Repeat.** Each converted action is independent; the rest of the app keeps using Razor.

## Translating common Razor patterns

| Razor | React view |
| --- | --- |
| `@Model.Name` | `props.name` |
| `@foreach (var x in Model.Items) { … }` | `props.items.map((x) => …)` |
| `@if (Model.IsAdmin) { … }` | `{props.isAdmin && …}` |
| `<a asp-controller="Users" asp-action="Detail" asp-route-id="7">` | `<a href="/Users/Detail/7">` |
| `@Html.Raw(...)` | `<div dangerouslySetInnerHTML={ { __html: ... } } />` (sanitize first) |
| Shared `_Layout.cshtml` | a `Layout.tsx` component you import — see [Layouts](layouts-and-components.md) |
| `@section Scripts { … }` | put interactivity in the component; it hydrates automatically |

## What stays the same

- Controllers, services, repositories, view models — **unchanged**.
- Routing, model binding, filters, dependency injection — **unchanged**.
- Authentication/authorization — `[Authorize]` etc. work as before; the action only runs if allowed.
- `RedirectToAction`, status codes, `NotFound()` — return them as usual.

## Passing existing view models

Your existing C# view models serialize straight to props (camelCase). You usually don't need to
change them — just type the matching `props` interface in the `.tsx`. See
[Writing Views › Model → props](writing-views.md#model-props) for date/collection notes.

## When to convert vs. keep Razor

Good early candidates: data-heavy or interactive pages that would benefit from React (dashboards,
tables, forms). Leave simple static pages on Razor until you're ready — there's no deadline, and the
two render side by side indefinitely.
