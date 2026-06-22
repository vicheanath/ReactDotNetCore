#!/usr/bin/env node
import { startFromEnv } from "./node";

startFromEnv().catch((err) => {
  console.error(err);
  process.exit(1);
});
