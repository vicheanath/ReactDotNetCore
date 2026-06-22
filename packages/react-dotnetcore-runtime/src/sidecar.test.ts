import { describe, it, expect, afterEach } from "vitest";
import type { Server } from "node:http";
import type { AddressInfo } from "node:net";
import { createSsrServer } from "./sidecar";
import type { ServerRenderer } from "./server";

let server: Server | undefined;

afterEach(() => {
  server?.close();
  server = undefined;
});

async function start(render: ServerRenderer, maxBodyBytes?: number): Promise<number> {
  server = createSsrServer({ render, port: 0, maxBodyBytes });
  await new Promise<void>((resolve) => server!.on("listening", () => resolve()));
  return (server!.address() as AddressInfo).port;
}

describe("createSsrServer", () => {
  it("answers the health probe", async () => {
    const port = await start(() => "<p>x</p>");
    const res = await fetch(`http://127.0.0.1:${port}/health`);
    expect(res.status).toBe(200);
    expect(await res.text()).toBe("ok");
  });

  it("renders a component via POST /render", async () => {
    const port = await start((component, props) => `<p>${component}:${(props as { n: number }).n}</p>`);
    const res = await fetch(`http://127.0.0.1:${port}/render`, {
      method: "POST",
      body: JSON.stringify({ component: "Card", props: { n: 42 } }),
    });
    expect(res.status).toBe(200);
    expect(await res.json()).toEqual({ html: "<p>Card:42</p>" });
  });

  it("returns { error } with status 500 when the renderer throws", async () => {
    const port = await start(() => {
      throw new Error("boom");
    });
    const res = await fetch(`http://127.0.0.1:${port}/render`, {
      method: "POST",
      body: JSON.stringify({ component: "X", props: {} }),
    });
    expect(res.status).toBe(500);
    expect((await res.json()).error).toContain("boom");
  });

  it("returns 404 for unknown routes", async () => {
    const port = await start(() => "<p>x</p>");
    const res = await fetch(`http://127.0.0.1:${port}/nope`);
    expect(res.status).toBe(404);
  });

  it("guards against oversized request bodies", async () => {
    const port = await start(() => "<p>x</p>", 16);
    let guarded = false;
    try {
      const res = await fetch(`http://127.0.0.1:${port}/render`, {
        method: "POST",
        body: JSON.stringify({ component: "X", props: { big: "x".repeat(500) } }),
      });
      guarded = res.status >= 400; // rejected with an error status...
    } catch {
      guarded = true; // ...or the connection was cut. Either is "guarded".
    }
    expect(guarded).toBe(true);
  });
});
