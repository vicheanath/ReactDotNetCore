import { defineConfig } from "vitest/config";

export default defineConfig({
  test: {
    // Default to Node; DOM-dependent specs opt in with `// @vitest-environment jsdom`.
    environment: "node",
    include: ["src/**/*.test.ts", "src/**/*.test.tsx"],
  },
});
