import type { ComponentType } from "react";

/** A map of component name -> React component. */
export type ComponentRegistry = Record<string, ComponentType<any>>;

/** The shape returned by Vite's `import.meta.glob(..., { eager: true })`. */
export type GlobModules = Record<string, { default?: ComponentType<any> } | undefined>;
