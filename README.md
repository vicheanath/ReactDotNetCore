# ReactDotNetCore for ASP.NET Core

Use **React components as ASP.NET Core MVC views** — server-side rendered and hydrated — without a
separate Web API, Next.js app, or SPA. Keep your existing controllers, services, repositories, and
view models; replace only the view layer.

```csharp
public IActionResult Detail(int id)
{
    var model = _userService.GetUser(id);     // unchanged domain code
    return this.ReactView<UserProfile>(model); // React instead of a Razor view
}
```

```tsx
// Views/UserProfile.tsx
export default function UserProfile(props: UserDto) {
  return <h1>{props.name}</h1>;
}
```

## Documentation

Full guides live in [`docs/`](docs/README.md):

- [Getting Started](docs/getting-started.md) — set up a new project from scratch
- [Writing Views](docs/writing-views.md) · [Configuration](docs/configuration.md) · [Development Mode (HMR)](docs/development-mode.md)
- [Layouts & Components](docs/layouts-and-components.md) · [Architecture](docs/architecture.md) · [Deployment](docs/deployment.md)
- [API Reference](docs/api-reference.md) · [Migrating from Razor](docs/migration-from-razor.md) · [Troubleshooting](docs/troubleshooting.md)

## Packages

| Package | Registry | What it is |
| --- | --- | --- |
| **ReactDotNetCore.AspNetCore** | NuGet | The .NET engine: `ReactView<T>()`, the result, the page renderer, the Vite manifest reader, and the managed Node SSR sidecar. |
| **@react-dotnetcore/runtime** | npm | The JS runtime: component registry, SSR renderer, client hydration, and the sidecar (production + Vite HMR dev). |

```bash
dotnet add package ReactDotNetCore.AspNetCore
npm install @react-dotnetcore/runtime react react-dom
npm install -D vite @vitejs/plugin-react   # vite enables dev/HMR mode
```

## How it works

```
Browser ─GET /Users/Detail/7─► MVC Controller ──► ReactView<UserProfile>(model)
                                                      │ serialize model → camelCase props
                                                      ▼
                                ReactDotNetCoreRenderer ──HTTP──► Node SSR sidecar
                                                      ◄── html ── renderToString(<UserProfile …/>)
                                                      ▼
                       HTML: SSR markup + embedded props + hydration script
                                                      ▼
                              Browser hydrates with the same component + props
```

- **Production** — the sidecar imports the Vite-built SSR bundle; the page links hashed assets from
  the Vite manifest.
- **Development** — the sidecar runs a Vite dev server and renders on the fly with **HMR**: edit a
  `.tsx` and the browser hot-reloads, no rebuild. The engine auto-selects this mode from
  `IWebHostEnvironment.IsDevelopment()`.

## Production hardening

- Auto free-port selection for the sidecar (`SidecarPort = 0`)
- Health-gated startup, crash **auto-restart** (bounded), graceful shutdown
- Per-render timeout; loopback-only sidecar; request body size guard
- Safe `<script>` props escaping; **no SSR stack-trace leakage in Production**
- camelCase model→props; single client bundle hydrates any view

## Repository layout

```
ReactDotNetCore.sln
Directory.Build.props                     shared .NET metadata
package.json                              npm workspaces root
packages/
  react-dotnetcore-aspnetcore/                   ► NuGet: ReactDotNetCore.AspNetCore
    ReactDotNetCoreControllerExtensions.cs        this.ReactView<T>(model) / ("Name", model)
    ReactDotNetCoreResult.cs · ReactDotNetCoreRenderer.cs
    ReactSsrClient.cs · ViteManifest.cs
    SidecarHostedService.cs · ServiceCollectionExtensions.cs
  react-dotnetcore-runtime/                      ► npm: @react-dotnetcore/runtime
    src/{registry,server,client,sidecar,dev,node,bin}.ts
samples/ReactDotNetCore.Sample/                 every page is a React view
  Controllers/ Models/ Services/ *.cs       unchanged MVC backend (project root = .NET only)
  Program.cs                                AddReactDotNetCore(o => o.ViteRoot = "Views")
  Views/                                    ← all frontend lives here
    Home.tsx Dashboard.tsx Users.tsx …      the React views
    components/ lib/ styles/                shared Layout + shadcn/ui + Tailwind
    entry-client.tsx entry-server.tsx       hydration / SSR entries
    ssr-server.mjs vite.config.ts package.json tailwind.config.cjs
  wwwroot/react-dotnetcore/client/          built client assets (generated)
.github/workflows/{ci,publish}.yml
```

## Run the sample

```bash
npm install            # installs all workspaces, links @react-dotnetcore/runtime
npm run build          # builds the runtime, then the sample client + SSR bundles
dotnet run --project samples/ReactDotNetCore.Sample
```

Open — every page is React:

- `/` Home · `/Dashboard` shadcn/ui dashboard · `/Users` list · `/Users/Detail/7` profile · `/Privacy`

For **HMR dev**, just run in the Development environment (`ASPNETCORE_ENVIRONMENT=Development`); the
sidecar boots a Vite dev server automatically and the client loads modules + HMR from it.

## Build & publish

```bash
npm run build                                   # runtime + sample
dotnet build ReactDotNetCore.sln -c Release
dotnet pack packages/react-dotnetcore-aspnetcore/... -o artifacts   # → .nupkg + .snupkg
npm pack -w @react-dotnetcore/runtime --pack-destination artifacts  # → .tgz
```

CI (`.github/workflows/ci.yml`) builds, typechecks, and packs both on every push/PR.
Publishing (`.github/workflows/publish.yml`) pushes to npm + NuGet on a GitHub Release
(needs `NPM_TOKEN` and `NUGET_API_KEY` secrets).

## License

MIT
