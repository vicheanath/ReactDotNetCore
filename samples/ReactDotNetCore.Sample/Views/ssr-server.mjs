// The SSR sidecar the ASP.NET host spawns. All logic lives in @react-dotnetcore/runtime;
// mode (prod bundle vs. Vite HMR dev) and port are selected via environment variables.
import { startFromEnv } from "@react-dotnetcore/runtime/node";

startFromEnv().catch((err) => {
  console.error(err);
  process.exit(1);
});
