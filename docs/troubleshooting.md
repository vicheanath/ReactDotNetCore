# Troubleshooting

## Startup

### `SSR sidecar failed to start within the configured timeout`
The Node sidecar didn't become healthy in time. Common causes:

- **Node not found** — `node` isn't on `PATH`. Set `options.NodePath` to its absolute path.
- **Missing SSR bundle (production)** — `dist/server/entry-server.js` doesn't exist. Run
  `npm run build` before starting.
- **Sidecar crashed on load** — check the app log for `[ssr]` lines; the error is printed there.
- Increase `options.SidecarStartupTimeout` on slow/cold environments.

### `SSR bundle not found at '…/dist/server/entry-server.js'`
You're in production mode but haven't built the SSR bundle. Run `npm run build`, or run in
`Development` to use Vite (no build needed).

### `Vite client manifest not found at '…/.vite/manifest.json'`
The client bundle wasn't built. Run `npm run build` (which runs `vite build` with `manifest: true`).

## Rendering

### `Component "X" is not registered`
- The component file isn't under `Views/` (the default glob is `./Views/*.tsx`).
- The file name doesn't match the name you requested. `ReactView<Dashboard>()` needs
  `Views/Dashboard.tsx` with a `export default`.
- For subfolders, widen the glob to `./Views/**/*.tsx` in both entry files.

### Hydration mismatch warnings in the browser console
The server and client rendered different markup. Usually caused by:

- Reading `Date.now()`, `Math.random()`, or `window`/`document` **during render**. Move them into
  `useEffect` (client-only) so SSR and first client render match.
- Locale-dependent formatting that differs between server and browser.

### Styles missing / flash of unstyled content
- **Production** — ensure CSS is imported from `entry-client.tsx` (so it lands in the manifest) and
  that `app.UseStaticFiles()` is enabled. Confirm a `<link>` to `…/client/assets/*.css` is in the page.
- **Development** — Vite injects styles via JS; a brief unstyled flash on first paint is expected in
  dev only.

## Tailwind / PostCSS

### `The 'border-border' class does not exist` (or other tokens) in dev
Tailwind can't find `tailwind.config.*` and your theme tokens, so `@apply border-border` fails.
This happens when the sidecar's working directory isn't the Vite root.

- Make sure `options.ViteRoot` points at the folder containing `vite.config`, `tailwind.config`, and
  the entries. The engine runs the sidecar with its working directory there, so Tailwind resolves its
  config and content globs correctly.
- If you run the build yourself, run it from the Vite root (where `package.json`/`tailwind.config`
  live), or pin the config path in `postcss.config.cjs`:
  `tailwindcss: { config: require("path").join(__dirname, "tailwind.config.cjs") }`.

### CSS imports 404 in dev (e.g. `Cannot GET /your-base/styles/globals.css`)
Your Vite `base` is applied in dev, prefixing module URLs. Set `base` only for the production client
build so dev uses `/`:

```ts
base: command === "build" && !isSsrBuild ? "/react-dotnetcore/client/" : "/",
```

## Development / HMR

### Flash of unstyled content (a "blink") when navigating in dev
Vite injects CSS via JavaScript after first paint, so the server-rendered HTML shows briefly
unstyled. Link the CSS as a render-blocking stylesheet in dev:

```csharp
o.DevStylesheets = new[] { "styles/globals.css" }; // relative to ViteRoot
```

(Production already links the hashed CSS from the manifest, so it doesn't flash.) Navigating between
views is still a full-page load — ReactDotNetCore renders independent MVC views, not an SPA.

### Edits don't hot-reload
- Confirm `ASPNETCORE_ENVIRONMENT=Development` and you didn't set `DevMode = false`.
- The startup log should say `dev/HMR mode`. If it says `production`, the environment isn't
  Development.

### CORS or 404 on `/@vite/client` in dev
- Ensure `vite` and `@vitejs/plugin-react` are installed (dev/HMR needs Vite).
- Don't override the dev server CORS settings; Vite enables CORS by default so the browser can load
  modules from the sidecar's origin.

## Assets / URLs

### Client JS/CSS return 404 in production
- Verify the files exist under `wwwroot/react-dotnetcore/client/…` after `npm run build`.
- If you changed `base` in `vite.config.ts` or the client `outDir`, mirror it in
  `options.PublicBasePath` and `options.ClientManifest` (see [Configuration](configuration.md)).
- Make sure `app.UseStaticFiles()` is present.

## Ports

### `Address already in use`
- The sidecar uses an auto-selected free port by default (`SidecarPort = 0`). If you pinned a port,
  it may be taken — switch back to `0` or pick another.

## Build

### `Rollup failed to resolve import "…"` from a view
- A bare import (e.g. `lucide-react`) can't be resolved from `Views/`. Ensure the package is installed
  and that `node_modules` is resolvable from the Vite root (the entry files and `Views/` should share
  one `node_modules`, i.e. the Vite root is your project root).

## Still stuck?

Check the app log: sidecar output is prefixed with `[ssr]`, and SSR failures are logged with the
component name. In production the error detail stays server-side (it isn't written to the response).
