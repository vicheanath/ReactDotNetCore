# Deployment

Production needs three things on the server: the **.NET app**, **Node.js** (for the SSR sidecar), and
the **built frontend assets**.

## Build

```bash
npm ci
npm run build                 # client bundle + manifest → wwwroot/react-dotnetcore/client
                              # SSR bundle               → dist/server/entry-server.js
dotnet publish -c Release -o ./publish
```

Make sure the published output includes:

- `wwwroot/react-dotnetcore/client/**` (hashed JS/CSS + `.vite/manifest.json`)
- `dist/server/entry-server.js` (the SSR bundle)
- `ssr-server.mjs` and the `node_modules` needed by it (at minimum `@react-dotnetcore/runtime`,
  `react`, `react-dom`)

> Tip: run `npm run build` **before** `dotnet publish`, and ensure your `.csproj` doesn't exclude the
> built `dist/` and `wwwroot/react-dotnetcore/` outputs from publish.

## Runtime requirements

- **Node.js 18+** must be on the server. If `node` isn't on `PATH`, set `options.NodePath` to its
  absolute path.
- The app spawns and supervises the sidecar automatically — no separate service to manage by default.
- The sidecar listens on `127.0.0.1` only and is never exposed; don't open its port.

## Docker

Use a base image (or install steps) that has **both** the .NET runtime and Node.js. Sketch:

```dockerfile
# --- build ---
FROM node:20 AS frontend
WORKDIR /src
COPY . .
RUN npm ci && npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet
WORKDIR /src
COPY --from=frontend /src .
RUN dotnet publish -c Release -o /app

# --- runtime: needs .NET + Node ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y nodejs npm && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=dotnet /app .
# include the node_modules the sidecar needs (or `npm ci --omit=dev` here)
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

The key constraint: the **runtime image must contain Node** and the sidecar's `node_modules`.

## Health & startup

The app gates startup on the sidecar's health check (`SidecarStartupTimeout`, default 30s). If the
sidecar can't start (missing Node, missing bundle), startup fails fast with a clear error — wire your
orchestrator's readiness probe to the app's own health endpoint as usual.

## Scaling

Each app instance runs its own sidecar on an auto-selected loopback port, so horizontal scaling (more
replicas) needs no coordination. Within an instance, the sidecar handles concurrent `/render` calls.

## Checklist

- [ ] `npm run build` ran and `wwwroot/react-dotnetcore/client` + `dist/server/entry-server.js` exist
- [ ] Node.js present in the runtime environment (`NodePath` set if not on `PATH`)
- [ ] `ssr-server.mjs` and its `node_modules` shipped alongside the app
- [ ] `app.UseStaticFiles()` is enabled (serves the client assets)
- [ ] Running as `Production` (or `DevMode = false`) so the built bundle is used
