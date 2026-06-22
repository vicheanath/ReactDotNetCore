# Configuration

Configure the engine by passing a lambda to `AddReactDotNetCore`:

```csharp
builder.Services.AddReactDotNetCore(options =>
{
    options.SidecarPort = 5174;       // pin a port instead of auto-selecting
    options.RenderTimeout = TimeSpan.FromSeconds(10);
    options.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
```

All relative paths are resolved against the app's `ContentRootPath`.

## `ReactDotNetCoreOptions`

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `NodePath` | `string` | `"node"` | Command used to launch Node for the sidecar. Set to an absolute path if `node` isn't on `PATH`. |
| `SidecarScript` | `string` | `"ssr-server.mjs"` | Sidecar script the host spawns (relative to ContentRoot). |
| `ViteRoot` | `string` | `"."` | Vite project root (where `vite.config` + entries live), relative to ContentRoot. Used as the dev server root. |
| `ServerBundle` | `string` | `"dist/server/entry-server.js"` | Built SSR bundle the sidecar imports in production. |
| `SidecarPort` | `int` | `0` | Loopback port for the sidecar. `0` = auto-select a free port at startup (recommended). |
| `LaunchSidecar` | `bool` | `true` | Whether the host spawns and supervises the Node sidecar. Set `false` to manage it yourself. |
| `SidecarStartupTimeout` | `TimeSpan` | `30s` | How long to wait for the sidecar health check at startup. |
| `RenderTimeout` | `TimeSpan` | `30s` | Per-render timeout for the SSR request. |
| `MaxSidecarRestarts` | `int` | `5` | Max automatic restarts after an unexpected sidecar exit. |
| `DevMode` | `bool?` | `null` | Force dev (Vite HMR) rendering on/off. `null` = follow `IWebHostEnvironment.IsDevelopment()`. |
| `DevServerHost` | `string` | `"localhost"` | Browser-facing host for the dev server origin (module + HMR loading). |
| `DevStylesheets` | `IList<string>` | empty | CSS entry paths (relative to `ViteRoot`) to link as render-blocking stylesheets in dev, preventing the flash of unstyled content. e.g. `{ "styles/globals.css" }`. |
| `ClientManifest` | `string` | `"wwwroot/react-dotnetcore/client/.vite/manifest.json"` | Path to the Vite client manifest (production). |
| `ClientEntryKey` | `string` | `"entry-client.tsx"` | Manifest key / dev entry module for the client bundle. |
| `PublicBasePath` | `string` | `"/react-dotnetcore/client"` | Public URL base where built client assets are served. |
| `ReloadManifestPerRequest` | `bool` | `false` | Re-read the manifest from disk on every request (useful while iterating). |
| `JsonOptions` | `JsonSerializerOptions` | Web defaults (camelCase) | Controls model → props serialization. |

## Common scenarios

### Pin the sidecar port

Auto-selection (`0`) avoids clashes and is best for most apps. Pin it only if a firewall or tooling
needs a known port:

```csharp
options.SidecarPort = 5174;
```

### Change where assets are served

If you change `base` in `vite.config.ts` and the client `outDir`, mirror it here:

```csharp
options.PublicBasePath = "/assets/rdc";
options.ClientManifest = "wwwroot/assets/rdc/.vite/manifest.json";
```

### Customize props serialization

```csharp
options.JsonOptions.Converters.Add(new JsonStringEnumConverter());
options.JsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
```

### Run the sidecar yourself

Set `LaunchSidecar = false` and start `ssr-server.mjs` (or `react-dotnetcore-ssr`) as a separate
process/container. Provide the matching environment variables — see
[Architecture › Environment contract](architecture.md#environment-contract). Point the engine at it
with `SidecarPort`.

### Keep all the frontend in one folder

By default the Vite tooling sits at the project root. To group the entire frontend under a subfolder
(e.g. `Views/`, keeping the project root .NET-only), set the three path options together:

```csharp
builder.Services.AddReactDotNetCore(o =>
{
    o.ViteRoot = "Views";                               // dev server root (vite.config + entries)
    o.SidecarScript = "Views/ssr-server.mjs";           // the sidecar script
    o.ServerBundle = "Views/dist/server/entry-server.js"; // production SSR bundle
});
```

Point the client build's `outDir` at `wwwroot/react-dotnetcore/client` (so `ClientManifest` and
`PublicBasePath` stay at their defaults). The `samples/ReactDotNetCore.Sample` project is set up
exactly this way.

### Force dev or prod rendering

```csharp
options.DevMode = false; // always use the built bundle + manifest, even under Development
```
