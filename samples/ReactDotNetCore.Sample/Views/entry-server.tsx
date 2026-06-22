import { createRegistry } from "@react-dotnetcore/runtime";
import { createServerRenderer } from "@react-dotnetcore/runtime/server";

// Auto-discover every React view in Views/*.tsx and expose render(component, props) for SSR.
const registry = createRegistry(import.meta.glob(["./*.tsx", "!./entry-client.tsx", "!./entry-server.tsx"], { eager: true }));

export const render = createServerRenderer(registry);
