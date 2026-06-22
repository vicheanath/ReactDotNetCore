import "./styles/globals.css";
import { createRegistry } from "@react-dotnetcore/runtime";
import { mount } from "@react-dotnetcore/runtime/client";

// Hydrate the server-rendered markup using the same component + props the server used.
const registry = createRegistry(import.meta.glob(["./*.tsx", "!./entry-client.tsx", "!./entry-server.tsx"], { eager: true }));
mount(registry);
