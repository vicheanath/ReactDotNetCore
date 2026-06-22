import { pathToFileURL } from "node:url";
import { createSsrServer } from "./sidecar";
import { createDevServer } from "./dev";

export { createSsrServer } from "./sidecar";
export type { SsrServerOptions } from "./sidecar";
export { createDevServer } from "./dev";
export type { DevServerOptions } from "./dev";

/**
 * Start the SSR sidecar driven by environment variables (the entry point the .NET host spawns):
 *   REACTDOTNETCORE_PORT    loopback port to listen on (default 5174)
 *   REACTDOTNETCORE_MODE    "prod" (default) | "dev"
 *   REACTDOTNETCORE_BUNDLE  (prod) path to the built SSR bundle exporting `render`
 *   REACTDOTNETCORE_ROOT    (dev)  Vite root containing the entry modules
 */
export async function startFromEnv(): Promise<void> {
  const port = Number.parseInt(process.env.REACTDOTNETCORE_PORT ?? "5174", 10);
  const mode = process.env.REACTDOTNETCORE_MODE ?? "prod";

  if (mode === "dev") {
    const root = process.env.REACTDOTNETCORE_ROOT ?? process.cwd();
    await createDevServer({
      root,
      port,
      onListen: (p) => console.log(`[react-dotnetcore] dev SSR server (Vite HMR) on http://127.0.0.1:${p}`),
    });
    return;
  }

  const bundle = process.env.REACTDOTNETCORE_BUNDLE;
  if (!bundle) {
    console.error("[react-dotnetcore] REACTDOTNETCORE_BUNDLE is required in prod mode.");
    process.exit(1);
  }
  const mod = await import(pathToFileURL(bundle).href);
  if (typeof mod.render !== "function") {
    console.error('[react-dotnetcore] SSR bundle must export a "render" function.');
    process.exit(1);
  }
  createSsrServer({
    render: mod.render,
    port,
    onListen: (p) => console.log(`[react-dotnetcore] SSR server on http://127.0.0.1:${p}`),
  });
}
