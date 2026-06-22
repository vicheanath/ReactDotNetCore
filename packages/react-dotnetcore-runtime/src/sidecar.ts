import http from "node:http";
import type { ServerRenderer } from "./server";

export interface SsrServerOptions {
  render: ServerRenderer;
  port: number;
  host?: string;
  /** Max accepted request body in bytes (props payload guard). Default 8 MiB. */
  maxBodyBytes?: number;
  onListen?: (port: number) => void;
}

function readBody(req: http.IncomingMessage, max: number): Promise<string> {
  return new Promise((resolve, reject) => {
    let size = 0;
    const chunks: Buffer[] = [];
    req.on("data", (c: Buffer) => {
      size += c.length;
      if (size > max) {
        reject(new Error(`Request body exceeds ${max} bytes`));
        req.destroy();
        return;
      }
      chunks.push(c);
    });
    req.on("end", () => resolve(Buffer.concat(chunks).toString("utf8")));
    req.on("error", reject);
  });
}

/**
 * Production SSR sidecar: a loopback HTTP server exposing
 *   GET  /health  -> "ok"
 *   POST /render  -> { component, props } => { html } | { error }
 */
export function createSsrServer(opts: SsrServerOptions): http.Server {
  const host = opts.host ?? "127.0.0.1";
  const maxBody = opts.maxBodyBytes ?? 8 * 1024 * 1024;

  const server = http.createServer(async (req, res) => {
    try {
      if (req.method === "GET" && req.url === "/health") {
        res.writeHead(200, { "content-type": "text/plain" });
        res.end("ok");
        return;
      }
      if (req.method === "POST" && req.url === "/render") {
        const { component, props } = JSON.parse(await readBody(req, maxBody));
        const html = opts.render(component, props);
        res.writeHead(200, { "content-type": "application/json" });
        res.end(JSON.stringify({ html }));
        return;
      }
      res.writeHead(404, { "content-type": "text/plain" });
      res.end("not found");
    } catch (err) {
      res.writeHead(500, { "content-type": "application/json" });
      res.end(JSON.stringify({ error: String((err as Error)?.stack ?? err) }));
    }
  });

  server.listen(opts.port, host, () => opts.onListen?.(opts.port));

  const shutdown = () => server.close(() => process.exit(0));
  process.on("SIGTERM", shutdown);
  process.on("SIGINT", shutdown);

  return server;
}
