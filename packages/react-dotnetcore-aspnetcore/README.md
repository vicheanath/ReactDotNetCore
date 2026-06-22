# ReactDotNetCore.AspNetCore

Use **React components as ASP.NET Core MVC views** — server-side rendered and hydrated — without a
separate Web API, Next.js app, or SPA. Keep your existing controllers, services, repositories, and
view models; replace only the view layer.

```csharp
public IActionResult Detail(int id)
{
    var model = _userService.GetUser(id);   // unchanged domain code
    return this.ReactView<UserProfile>(model);
}
```

```tsx
// Views/UserProfile.tsx
export default function UserProfile(props: UserDto) {
  return <h1>{props.name}</h1>;
}
```

## How it works

A controller returns a `ReactDotNetCoreResult`. The engine serializes the model to camelCase JSON props,
asks a managed **Node SSR sidecar** to render the component to HTML, and returns a full page with
the markup, the embedded props, and the hydration script. The browser hydrates with the same
component + props.

- **Production:** the sidecar imports the Vite-built SSR bundle; the page links hashed assets from
  the Vite manifest.
- **Development:** the sidecar runs a Vite dev server, rendering on the fly with **HMR** — edit a
  `.tsx` and the browser hot-reloads, no rebuild.

The companion npm package [`@react-dotnetcore/runtime`](https://www.npmjs.com/package/@react-dotnetcore/runtime)
provides the JS side (registry, SSR renderer, hydration, sidecar).

## Setup

```csharp
builder.Services.AddReactDotNetCore();   // options: dev mode, ports, timeouts, paths, JSON
app.UseStaticFiles();              // serves the built client assets from wwwroot
```

Production hardening built in: auto free-port selection, sidecar health-gating, crash auto-restart,
render timeouts, graceful shutdown, loopback-only sidecar, safe `<script>` props escaping, and no
SSR stack-trace leakage in Production.

See the repository README for the full walkthrough, project structure, and the sample app.

## License

MIT
