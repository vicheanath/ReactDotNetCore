# Architecture

## Request flow

```mermaid
sequenceDiagram
    autonumber
    participant B as Browser
    participant C as MVC Controller
    participant R as ReactDotNetCoreRenderer
    participant S as Node SSR sidecar
    B->>C: GET /Users/Detail/7
    Note over C: _userService.GetUser(7) — your existing code
    C->>R: ReactView&lt;UserProfile&gt;(model)
    Note over R: serialize model → camelCase props
    R->>S: POST /render { component, props }
    Note over S: renderToString(UserProfile)
    S-->>R: { html }
    Note over R: resolve client assets (Vite manifest, prod)
    R-->>B: HTML — SSR markup + props + hydration script
    Note over B: hydrate with same component + props
```

## Components

| Piece | Package | Role |
| --- | --- | --- |
| `ReactView<T>()` / `ReactView("Name", model)` | NuGet | Controller extension returning a `ReactDotNetCoreResult`. |
| `ReactDotNetCoreRenderer` | NuGet | Serializes props, calls the sidecar, assembles the HTML page. |
| `ReactSsrClient` | NuGet | Typed HTTP client to the sidecar (`/health`, `/render`). |
| `ViteManifestProvider` | NuGet | Resolves the client entry → hashed JS + transitive CSS (production). |
| `SidecarHostedService` | NuGet | Spawns/supervises the Node sidecar; health-gates startup; auto-restarts. |
| Node SSR sidecar | npm | Renders components to HTML. Production: imports the built bundle. Dev: runs Vite. |
| `createRegistry` / `mount` / `createServerRenderer` | npm | Registry, hydration, and SSR glue used by your entry files. |

## Why a Node sidecar?

React's server renderer (`react-dom/server`) is JavaScript. Rather than embed a JS engine in the
.NET process, ReactDotNetCore runs a small **loopback-only** Node HTTP server that the .NET host
spawns and supervises. This keeps full compatibility with the React/Vite ecosystem (including HMR in
dev) and isolates rendering from your web process.

The sidecar exposes exactly two endpoints:

- `GET /health` → readiness probe (used to gate app startup)
- `POST /render` → `{ component, props }` ⇒ `{ html }` (or `{ error }`)

It binds to `127.0.0.1` only and is never exposed publicly.

## The page contract

Every rendered page contains:

```html
<div id="react-dotnetcore-root" data-react-dotnetcore-component="UserProfile">…SSR HTML…</div>
<script id="react-dotnetcore-props" type="application/json">{…props…}</script>
<!-- production: -->
<link rel="stylesheet" href="/react-dotnetcore/client/assets/entry-client-XXXX.css" />
<script type="module" src="/react-dotnetcore/client/assets/entry-client-XXXX.js"></script>
```

The client entry (`mount`) reads the component name from the root element's
`data-react-dotnetcore-component`, parses the embedded props, looks the component up in the registry,
and calls `hydrateRoot`. The **same JSON bytes** drive SSR and hydration, guaranteeing a match.

Props are escaped for safe embedding inside `<script>` (`<`, `>`, `&`, U+2028/U+2029 → `\uXXXX`),
which stays valid JSON and round-trips through `JSON.parse`.

## Production vs development

| | Production | Development |
| --- | --- | --- |
| SSR source | Built bundle (`dist/server/entry-server.js`) | Vite `ssrLoadModule` (fresh each request) |
| Client assets | Hashed files from the Vite **manifest** + `<link>` CSS | Modules loaded from the Vite dev server origin |
| Reload | Rebuild + restart | **HMR** (Fast Refresh) |
| Selected by | `IsDevelopment() == false` | `IsDevelopment() == true` (override with `DevMode`) |

## Environment contract

The host passes these to the sidecar process (you only need them if you run the sidecar yourself):

| Variable | Mode | Meaning |
| --- | --- | --- |
| `REACTDOTNETCORE_PORT` | both | Loopback port to listen on (default `5174`). |
| `REACTDOTNETCORE_MODE` | both | `prod` (default) or `dev`. |
| `REACTDOTNETCORE_BUNDLE` | prod | Path to the built SSR bundle exporting `render`. |
| `REACTDOTNETCORE_ROOT` | dev | Vite root containing the entry modules. |

## Reliability features

- **Free-port auto-selection** (`SidecarPort = 0`) avoids clashes.
- **Health-gated startup** — the app waits for the sidecar before serving.
- **Crash auto-restart** — bounded by `MaxSidecarRestarts`.
- **Render timeout** — `RenderTimeout` bounds each SSR call.
- **Graceful shutdown** — the sidecar is terminated (process tree) on app stop.
- **No leak in Production** — SSR errors are logged server-side and propagate to your configured error
  handling; internals aren't written to the response.
