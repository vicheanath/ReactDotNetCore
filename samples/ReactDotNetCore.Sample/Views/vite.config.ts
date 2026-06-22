import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { resolve } from "node:path";

// Two builds share this config:
//   vite build                          -> client bundle + manifest -> wwwroot/react-dotnetcore/client
//   vite build --ssr entry-server.tsx   -> SSR bundle               -> react/dist/server
export default defineConfig(({ command, isSsrBuild }) => ({
  root: __dirname,
  plugins: [react()],
  // Only the production client build is served from this path by ASP.NET static files.
  // In dev the Vite server serves modules from its origin root, so base must be "/".
  base: command === "build" && !isSsrBuild ? "/react-dotnetcore/client/" : "/",
  build: isSsrBuild
    ? {
        ssr: true,
        outDir: "dist/server",
        emptyOutDir: true,
        rollupOptions: {
          input: resolve(__dirname, "entry-server.tsx"),
        },
      }
    : {
        manifest: true,
        outDir: "../wwwroot/react-dotnetcore/client",
        emptyOutDir: true,
        rollupOptions: {
          input: resolve(__dirname, "entry-client.tsx"),
        },
      },
}));
