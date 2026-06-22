# ReactDotNetCore Documentation

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

## Contents

1. [Getting Started](getting-started.md) — set up a new project from scratch
2. [Writing Views](writing-views.md) — components, model → props, markers, the `ReactView` API
3. [Configuration](configuration.md) — every `ReactDotNetCoreOptions` setting
4. [Development Mode (HMR)](development-mode.md) — hot reload with Vite
5. [Layouts & Shared Components](layouts-and-components.md) — shared UI, Tailwind, shadcn/ui
6. [Architecture](architecture.md) — how SSR, the sidecar, and hydration fit together
7. [Deployment](deployment.md) — building and shipping to production
8. [API Reference](api-reference.md) — C# and JavaScript surface
9. [Migrating from Razor](migration-from-razor.md) — incremental migration strategy
10. [Troubleshooting](troubleshooting.md) — common issues and fixes

## The two packages

| Package | Registry | Responsibility |
| --- | --- | --- |
| **ReactDotNetCore.AspNetCore** | NuGet | The .NET engine: `ReactView<T>()`, the page renderer, the Vite manifest reader, and the managed Node SSR sidecar. |
| **@react-dotnetcore/runtime** | npm | The JS runtime: component registry, SSR renderer, client hydration, and the sidecar (production + Vite HMR dev). |

```bash
dotnet add package ReactDotNetCore.AspNetCore
npm install @react-dotnetcore/runtime react react-dom
npm install -D vite @vitejs/plugin-react typescript @types/react @types/react-dom
```

## Requirements

- **.NET 6, 7, 8, 9, or 10** — the engine multi-targets `net6.0` through `net10.0`, so it works on any of these runtimes.
- Node.js 18+ (tested on Node 24 LTS) for the SSR sidecar.
