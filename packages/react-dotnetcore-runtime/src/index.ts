export { createRegistry, registeredNames } from "./registry";
export type { ComponentRegistry, GlobModules } from "./types";

/** Attribute the server sets on the hydration root to name the component. */
export const COMPONENT_ATTR = "reactDotnetcoreComponent";
/** Default element ids used by the server-rendered page. */
export const ROOT_ID = "react-dotnetcore-root";
export const PROPS_ID = "react-dotnetcore-props";
