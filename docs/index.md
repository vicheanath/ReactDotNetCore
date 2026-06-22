---
layout: home

hero:
  name: ReactDotNetCore
  text: React components as ASP.NET Core MVC views
  tagline: Server-side rendered and hydrated — no Web API, no Next.js, no SPA. Keep your controllers, services, and view models; replace only the view layer.
  actions:
    - theme: brand
      text: Get Started
      link: /getting-started
    - theme: alt
      text: Architecture
      link: /architecture
    - theme: alt
      text: GitHub
      link: https://github.com/vicheanath/ReactDotNetCore

features:
  - title: React as a view engine
    details: "return this.ReactView<UserProfile>(model); — your existing controllers, services, and view models are untouched."
  - title: SSR + hydration
    details: A managed Node sidecar renders React to HTML on the server; the browser hydrates the same component with the same props.
  - title: Dev mode with HMR
    details: In Development the sidecar runs Vite — edit a .tsx and the browser hot-reloads, no rebuild.
  - title: Production-hardened
    details: Auto free-port selection, health-gated startup, crash auto-restart, render timeouts, loopback-only sidecar, safe props escaping.
  - title: Incremental migration
    details: Razor and React views coexist. Convert one view at a time; the rest of the app keeps working.
  - title: Two packages
    details: ReactDotNetCore.AspNetCore (NuGet) for the engine, @react-dotnetcore/runtime (npm) for the SSR/hydration runtime.
---

## Quick look

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

Install both packages and you're ready:

```bash
dotnet add package ReactDotNetCore.AspNetCore
npm install @react-dotnetcore/runtime react react-dom
```

Head to [Getting Started](/getting-started) for a full project setup.
