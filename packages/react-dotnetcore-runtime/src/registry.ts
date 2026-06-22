import type { ComponentRegistry, GlobModules } from "./types";

/**
 * Build a component registry from a Vite glob import. The key is the file's base name
 * (e.g. `../Views/UserProfile.tsx` -> `UserProfile`), which is what the server sends.
 *
 * @example
 * const registry = createRegistry(import.meta.glob("./Views/*.tsx", { eager: true }));
 */
export function createRegistry(modules: GlobModules): ComponentRegistry {
  const registry: ComponentRegistry = {};
  for (const path in modules) {
    const mod = modules[path];
    const def = mod?.default;
    if (!def) continue;
    const base = path.split("/").pop() ?? path;
    const name = base.replace(/\.[jt]sx?$/, "");
    registry[name] = def;
  }
  return registry;
}

/** Names of all registered components (useful for diagnostics). */
export function registeredNames(registry: ComponentRegistry): string[] {
  return Object.keys(registry);
}
