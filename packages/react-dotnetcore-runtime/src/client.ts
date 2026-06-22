import { createElement } from "react";
import { hydrateRoot } from "react-dom/client";
import type { ComponentRegistry } from "./types";

export interface MountOptions {
  /** Id of the SSR root element. Default: "react-dotnetcore-root". */
  rootId?: string;
  /** Id of the embedded props <script>. Default: "react-dotnetcore-props". */
  propsId?: string;
}

/** Read and parse the props embedded by the server. */
export function readProps(propsId = "react-dotnetcore-props"): unknown {
  if (typeof document === "undefined") return {};
  const el = document.getElementById(propsId);
  if (!el?.textContent) return {};
  try {
    return JSON.parse(el.textContent);
  } catch {
    console.error("[react-dotnetcore] Failed to parse embedded props JSON.");
    return {};
  }
}

/**
 * Hydrate the server-rendered markup using the same component + props the server used.
 * Safe to call unconditionally; it no-ops if the root element is absent.
 */
export function mount(registry: ComponentRegistry, opts: MountOptions = {}): void {
  if (typeof document === "undefined") return;
  const root = document.getElementById(opts.rootId ?? "react-dotnetcore-root");
  if (!root) return;

  const name = root.dataset.reactDotnetcoreComponent ?? "";
  const Comp = registry[name];
  if (!Comp) {
    console.error(`[react-dotnetcore] Component "${name}" is not registered on the client.`);
    return;
  }
  const props = readProps(opts.propsId);
  hydrateRoot(root, createElement(Comp, props as Record<string, unknown>));
}
