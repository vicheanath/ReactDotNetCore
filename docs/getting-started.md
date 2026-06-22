# Getting Started

This guide sets up ReactDotNetCore in a **new ASP.NET Core MVC app** from scratch. If you already
have an MVC app, skip step 1 and apply the rest.

::: tip Learn by reading working code
The `samples/ReactDotNetCore.Sample` project in the repository contains every file referenced below.
:::

## 1. Create the project

```bash
dotnet new mvc -n MyApp
cd MyApp
dotnet add package ReactDotNetCore.AspNetCore
```

## 2. Add the frontend toolchain

Create a `package.json` at the project root:

```json
{
  "name": "myapp-frontend",
  "private": true,
  "type": "module",
  "scripts": {
    "build": "npm run build:client && npm run build:server",
    "build:client": "vite build",
    "build:server": "vite build --ssr entry-server.tsx"
  },
  "dependencies": {
    "@react-dotnetcore/runtime": "^0.1.0",
    "react": "^18.3.1",
    "react-dom": "^18.3.1"
  },
  "devDependencies": {
    "@vitejs/plugin-react": "^4.3.4",
    "@types/react": "^18.3.12",
    "@types/react-dom": "^18.3.1",
    "typescript": "^5.6.3",
    "vite": "^5.4.11"
  }
}
```

```bash
npm install
```

## 3. Add the build + entry files

**`vite.config.ts`** — one config drives both the client and SSR builds:

```ts
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { resolve } from "node:path";

export default defineConfig(({ command, isSsrBuild }) => ({
  root: __dirname,
  plugins: [react()],
  // Only the production client build is served from this path by ASP.NET. In dev the Vite
  // server serves modules from its origin root, so base must stay "/".
  base: command === "build" && !isSsrBuild ? "/react-dotnetcore/client/" : "/",
  build: isSsrBuild
    ? {
        ssr: true,
        outDir: "dist/server",
        emptyOutDir: true,
        rollupOptions: { input: resolve(__dirname, "entry-server.tsx") },
      }
    : {
        manifest: true,
        outDir: "wwwroot/react-dotnetcore/client",
        emptyOutDir: true,
        rollupOptions: { input: resolve(__dirname, "entry-client.tsx") },
      },
}));
```

**`tsconfig.json`** (frontend):

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "moduleResolution": "bundler",
    "jsx": "react-jsx",
    "strict": true,
    "skipLibCheck": true,
    "noEmit": true,
    "isolatedModules": true,
    "types": ["vite/client", "node"]
  },
  "include": ["entry-client.tsx", "entry-server.tsx", "Views/**/*.tsx"]
}
```

The three glue files — SSR entry, hydration entry, and the sidecar bootstrap:

::: code-group

```tsx [entry-server.tsx]
// SSR entry — exports render()
import { createRegistry } from "@react-dotnetcore/runtime";
import { createServerRenderer } from "@react-dotnetcore/runtime/server";

const registry = createRegistry(import.meta.glob("./Views/*.tsx", { eager: true }));
export const render = createServerRenderer(registry);
```

```tsx [entry-client.tsx]
// Hydration entry
import { createRegistry } from "@react-dotnetcore/runtime";
import { mount } from "@react-dotnetcore/runtime/client";

const registry = createRegistry(import.meta.glob("./Views/*.tsx", { eager: true }));
mount(registry);
```

```js [ssr-server.mjs]
// The sidecar the .NET host launches — logic lives in the runtime
import { startFromEnv } from "@react-dotnetcore/runtime/node";
startFromEnv().catch((err) => { console.error(err); process.exit(1); });
```

:::

## 4. Wire up ASP.NET Core

In `Program.cs`:

```csharp
using ReactDotNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddReactDotNetCore();   // ← register the engine

var app = builder.Build();
app.UseStaticFiles();                     // serves the built client assets from wwwroot
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
```

## 5. Write your first view

**`Views/UserProfile.tsx`**:

```tsx
export interface UserProfileProps {
  name: string;
  email: string;
}

export default function UserProfile(props: UserProfileProps) {
  return (
    <main>
      <h1>{props.name}</h1>
      <p>{props.email}</p>
    </main>
  );
}
```

**A marker type** so you can use `ReactView<UserProfile>()` (one per view):

```csharp
// ReactComponents.cs
using ReactDotNetCore;
namespace MyApp;

public sealed class UserProfile : IReactComponent;
```

**Return it from a controller**:

```csharp
using ReactDotNetCore;

public class UsersController : Controller
{
    public IActionResult Detail(int id)
        => this.ReactView<UserProfile>(new { name = $"User {id}", email = $"user{id}@example.com" });
}
```

## 6. Build and run

```bash
npm run build          # builds the client bundle (+ manifest) and the SSR bundle
dotnet run
```

Open `https://localhost:<port>/Users/Detail/1`. View source: the HTML is server-rendered (the data
is in the markup, not an empty `<div>`), and the page hydrates on load.

During development you can skip the rebuild — see [Development Mode (HMR)](development-mode.md).

## Next steps

- [Writing Views](writing-views.md) — passing data, the `ReactView` API, naming rules
- [Configuration](configuration.md) — ports, timeouts, paths, JSON options
- [Layouts & Shared Components](layouts-and-components.md) — share a layout, add Tailwind/shadcn
