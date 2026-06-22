# Development Mode (HMR)

In the `Development` environment, ReactDotNetCore renders through a **Vite dev server** instead of a
pre-built bundle. Editing a `.tsx` file hot-reloads the browser instantly — no `npm run build`, no
app restart.

## How to use it

Just run in the Development environment:

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

That's it. The engine detects `IsDevelopment()` and starts the sidecar in dev mode. Edit a view,
save, and the browser updates via Fast Refresh (component state is preserved where possible).

You do **not** need to run `npm run build` or `vite` yourself in development — the host spawns and
supervises the Vite-backed sidecar for you.

## What happens under the hood

- The sidecar boots a Vite dev server (Vite is an optional peer dependency).
- **SSR** is done per request via Vite's `ssrLoadModule("/entry-server.tsx")`, so server output
  always reflects your latest edit.
- The page the host returns loads the client modules **directly from the dev server origin**:
  the React Fast Refresh preamble, `/@vite/client` (HMR socket), and `/entry-client.tsx`.
- No production manifest or `<link>` is used; Vite injects styles and handles HMR.

```
Browser ──► ASP.NET app ──/render──► sidecar (Vite ssrLoadModule) ──► fresh SSR HTML
   │                                     ▲
   └── loads /@vite/client + entry ──────┘  (modules + HMR straight from Vite)
```

## Avoiding the flash of unstyled content (FOUC)

In dev, Vite injects CSS with JavaScript *after* first paint, so a full-page navigation can briefly
show unstyled content. Link your CSS as a real (render-blocking) stylesheet in dev with
`DevStylesheets` — the engine points it at Vite's `?direct` CSS, so first paint is already styled
(just like production), while HMR keeps working through the normal JS import:

```csharp
builder.Services.AddReactDotNetCore(o =>
{
    o.DevStylesheets = new[] { "styles/globals.css" }; // relative to ViteRoot
});
```

> This removes the unstyled flash. Note that navigating between views is still a normal full-page
> load (the same as production and Razor) — ReactDotNetCore renders independent server-rendered MVC
> views, not a single-page app, so there is no client-side router.

## Requirements

- `vite` and `@vitejs/plugin-react` installed (they already are if you followed
  [Getting Started](getting-started.md)).
- The Vite dev server enables CORS by default, so the browser can load its modules from a different
  port than the .NET app. No extra setup needed.

## Switching to production behavior locally

To test the production path (built bundle + hashed assets + manifest) on your machine:

```bash
npm run build
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

Or force it regardless of environment:

```csharp
builder.Services.AddReactDotNetCore(o => o.DevMode = false);
```

## Troubleshooting HMR

- **Styles/scripts 404 or CORS errors** — confirm `vite` is installed and the sidecar started in dev
  mode (the log line reads `dev/HMR mode`).
- **Edits don't reload** — make sure you're in `Development` and didn't set `DevMode = false`.
- **Want HMR off but stay in Development** — set `o.DevMode = false`.

See [Troubleshooting](troubleshooting.md) for more.
