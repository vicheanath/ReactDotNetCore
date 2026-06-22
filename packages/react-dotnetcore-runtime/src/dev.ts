import http from "node:http";

export interface DevServerOptions {
  /** Vite root (the folder containing entry-server.tsx / entry-client.tsx). */
  root: string;
  port: number;
  host?: string;
  /** SSR entry module id, resolved against root. Default "/entry-server.tsx". */
  serverEntry?: string;
  onListen?: (port: number) => void;
}

/**
 * Development SSR sidecar backed by a Vite dev server. Renders fresh on every request
 * (via `ssrLoadModule`) and serves client modules + HMR over the same HTTP server, so a
 * `.tsx` edit hot-reloads without a rebuild. `vite` is an optional peer dependency.
 */
export async function createDevServer(opts: DevServerOptions) {
  let createServer: typeof import("vite").createServer;
  try {
    ({ createServer } = await import("vite"));
  } catch {
    throw new Error("[react-dotnetcore] Dev mode requires 'vite' to be installed (peer dependency).");
  }

  const host = opts.host ?? "127.0.0.1";
  const serverEntry = opts.serverEntry ?? "/entry-server.tsx";
  const httpServer = http.createServer();

  const vite = await createServer({
    root: opts.root,
    appType: "custom",
    server: { middlewareMode: true, hmr: { server: httpServer }, cors: true },
  });

  httpServer.on("request", (req, res) => {
    if (req.method === "GET" && req.url === "/health") {
      res.writeHead(200, { "content-type": "text/plain" });
      res.end("ok");
      return;
    }
    if (req.method === "POST" && req.url === "/render") {
      let body = "";
      req.on("data", (c) => (body += c));
      req.on("end", async () => {
        try {
          const { component, props } = JSON.parse(body);
          const mod = (await vite.ssrLoadModule(serverEntry)) as {
            render: (c: string, p: unknown) => string;
          };
          const html = mod.render(component, props);
          res.writeHead(200, { "content-type": "application/json" });
          res.end(JSON.stringify({ html }));
        } catch (err) {
          vite.ssrFixStacktrace?.(err as Error);
          res.writeHead(500, { "content-type": "application/json" });
          res.end(JSON.stringify({ error: String((err as Error)?.stack ?? err) }));
        }
      });
      return;
    }
    // Everything else (client modules, /@vite/client, /@react-refresh, HMR) -> Vite.
    vite.middlewares(req, res);
  });

  httpServer.listen(opts.port, host, () => opts.onListen?.(opts.port));

  const shutdown = async () => {
    await vite.close();
    httpServer.close(() => process.exit(0));
  };
  process.on("SIGTERM", shutdown);
  process.on("SIGINT", shutdown);

  return { httpServer, vite };
}
