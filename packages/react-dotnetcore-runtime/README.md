# @react-dotnetcore/runtime

The JavaScript runtime for [ReactDotNetCore.AspNetCore](https://www.nuget.org/packages/ReactDotNetCore.AspNetCore) —
use React components as ASP.NET Core MVC views with SSR + hydration.

It provides the component registry, the SSR renderer, the client hydration bootstrap, and the Node
SSR sidecar (production build mode + Vite HMR dev mode) that the .NET host spawns.

## Install

```bash
npm install @react-dotnetcore/runtime react react-dom
npm install -D vite @vitejs/plugin-react   # vite is optional, needed for dev/HMR mode
```

## Usage

**`entry-server.tsx`** (SSR bundle):

```tsx
import { createRegistry } from "@react-dotnetcore/runtime";
import { createServerRenderer } from "@react-dotnetcore/runtime/server";

const registry = createRegistry(import.meta.glob("./Views/*.tsx", { eager: true }));
export const render = createServerRenderer(registry);
```

**`entry-client.tsx`** (hydration):

```tsx
import "./styles/globals.css";
import { createRegistry } from "@react-dotnetcore/runtime";
import { mount } from "@react-dotnetcore/runtime/client";

const registry = createRegistry(import.meta.glob("./Views/*.tsx", { eager: true }));
mount(registry);
```

**`ssr-server.mjs`** (the sidecar the .NET host launches):

```js
import { startFromEnv } from "@react-dotnetcore/runtime/node";
startFromEnv();
```

`startFromEnv()` reads environment variables set by the host:

| Variable           | Mode | Meaning                                            |
| ------------------ | ---- | -------------------------------------------------- |
| `REACTDOTNETCORE_PORT`   | both | Loopback port to listen on (default `5174`)        |
| `REACTDOTNETCORE_MODE`   | both | `prod` (default) or `dev`                          |
| `REACTDOTNETCORE_BUNDLE` | prod | Path to the built SSR bundle exporting `render`    |
| `REACTDOTNETCORE_ROOT`   | dev  | Vite root for on-the-fly SSR + HMR                 |

You can also run the bundled CLI directly: `react-dotnetcore-ssr` (same env contract).

## Exports

- `@react-dotnetcore/runtime` — `createRegistry`, ids/constants, types (safe everywhere)
- `@react-dotnetcore/runtime/server` — `createServerRenderer` (SSR)
- `@react-dotnetcore/runtime/client` — `mount`, `readProps` (browser)
- `@react-dotnetcore/runtime/node` — `createSsrServer`, `createDevServer`, `startFromEnv` (Node only)

## License

MIT
