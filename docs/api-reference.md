# API Reference

## C# — `ReactDotNetCore.AspNetCore`

Namespace: `ReactDotNetCore`.

### Controller extensions

```csharp
// Strongly typed (TComponent : IReactComponent). Component name = type name.
ReactDotNetCoreResult ReactView<TComponent>(this ControllerBase controller, object? model = null)

// String based.
ReactDotNetCoreResult ReactView(this ControllerBase controller, string component, object? model = null)
```

### `IReactComponent`

Empty marker interface. Implement it with a type named after a view to enable `ReactView<T>()`:

```csharp
public sealed class Dashboard : IReactComponent;
```

### `ReactDotNetCoreResult : ActionResult`

| Member | Type | Notes |
| --- | --- | --- |
| `Component` | `string` | The view/component name. |
| `Model` | `object?` | The model serialized to props. |
| `Title` | `string?` | Optional `<title>`; defaults to the component name. |

### `AddReactDotNetCore`

```csharp
IServiceCollection AddReactDotNetCore(
    this IServiceCollection services,
    Action<ReactDotNetCoreOptions>? configure = null)
```

Registers options, the manifest provider, the renderer, the typed SSR client, and (unless
`LaunchSidecar = false`) the sidecar hosted service. Auto-selects a free port when `SidecarPort = 0`.

### `ReactDotNetCoreOptions`

See [Configuration](configuration.md) for the full table. Key members: `SidecarPort`,
`RenderTimeout`, `MaxSidecarRestarts`, `DevMode`, `PublicBasePath`, `ClientManifest`, `JsonOptions`.

### Services (resolvable from DI)

| Type | Use |
| --- | --- |
| `ReactDotNetCoreRenderer` | `RenderPageAsync(component, model, title, ct)` → full HTML string. |
| `ReactSsrClient` | `RenderAsync(component, propsJson, ct)`, `WaitForReadyAsync(timeout, ct)`. |
| `ViteManifestProvider` | `ResolveEntry(entryKey?)` → `{ File, Css[] }`. |

---

## JavaScript — `@react-dotnetcore/runtime`

ESM, with subpath exports.

### `@react-dotnetcore/runtime`

```ts
function createRegistry(modules: GlobModules): ComponentRegistry
function registeredNames(registry: ComponentRegistry): string[]

const ROOT_ID: "react-dotnetcore-root"
const PROPS_ID: "react-dotnetcore-props"
const COMPONENT_ATTR: "reactDotnetcoreComponent"

type ComponentRegistry = Record<string, React.ComponentType<any>>
type GlobModules = Record<string, { default?: React.ComponentType<any> }>
```

`createRegistry` takes a Vite `import.meta.glob(..., { eager: true })` result and keys components by
file base name.

### `@react-dotnetcore/runtime/server`

```ts
type ServerRenderer = (component: string, props: unknown) => string
function createServerRenderer(registry: ComponentRegistry): ServerRenderer
```

Returns the `render(component, props)` used by `entry-server.tsx`. Throws (with the list of known
components) if a name isn't registered.

### `@react-dotnetcore/runtime/client`

```ts
interface MountOptions { rootId?: string; propsId?: string }
function mount(registry: ComponentRegistry, opts?: MountOptions): void
function readProps(propsId?: string): unknown
```

`mount` reads the root element, parses embedded props, and calls `hydrateRoot`. No-ops if the root is
absent.

### `@react-dotnetcore/runtime/node`

```ts
function startFromEnv(): Promise<void>   // reads REACTDOTNETCORE_* env vars; used by ssr-server.mjs
function createSsrServer(opts: SsrServerOptions): http.Server          // production sidecar
function createDevServer(opts: DevServerOptions): Promise<{ httpServer, vite }>  // Vite HMR sidecar

interface SsrServerOptions { render: ServerRenderer; port: number; host?: string;
  maxBodyBytes?: number; onListen?: (port: number) => void }
interface DevServerOptions { root: string; port: number; host?: string;
  serverEntry?: string; onListen?: (port: number) => void }
```

### Binary

`react-dotnetcore-ssr` — runs `startFromEnv()`. Equivalent to the two-line `ssr-server.mjs`. Same
[environment contract](architecture.md#environment-contract).

### Sidecar HTTP endpoints

| Method | Path | Body | Response |
| --- | --- | --- | --- |
| `GET` | `/health` | — | `200 ok` |
| `POST` | `/render` | `{ component, props }` | `{ html }` or `{ error }` |
