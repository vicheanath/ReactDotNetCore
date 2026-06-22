import { createElement } from "react";
import { renderToString } from "react-dom/server";
import type { ComponentRegistry } from "./types";
import { registeredNames } from "./registry";

/** A function that renders a registered component + props to an HTML string. */
export type ServerRenderer = (component: string, props: unknown) => string;

/**
 * Create the SSR `render(component, props)` function used by the Node sidecar.
 * Throws a descriptive error if the component is not registered.
 */
export function createServerRenderer(registry: ComponentRegistry): ServerRenderer {
  return (component, props) => {
    const Comp = registry[component];
    if (!Comp) {
      throw new Error(
        `[react-dotnetcore] Component "${component}" is not registered. Known components: ${registeredNames(registry).join(", ")}`,
      );
    }
    return renderToString(createElement(Comp, props as Record<string, unknown>));
  };
}
