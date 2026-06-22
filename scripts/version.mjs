#!/usr/bin/env node
// Lockstep version bump for the whole monorepo: sets the same version on both publishable packages.
//   npm   -> packages/react-dotnetcore-runtime/package.json  ("version")
//   NuGet -> Directory.Build.props                           (<Version>)
//
// Usage:
//   node scripts/version.mjs <patch|minor|major|X.Y.Z[-prerelease]>
//   npm run bump minor
//   npm run bump 0.2.0
import { readFileSync, writeFileSync } from "node:fs";
import { execSync } from "node:child_process";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const pkgPath = join(root, "packages/react-dotnetcore-runtime/package.json");
const propsPath = join(root, "Directory.Build.props");

const arg = process.argv[2];
if (!arg) {
  console.error("Usage: npm run bump <patch|minor|major|X.Y.Z>");
  process.exit(1);
}

const current = JSON.parse(readFileSync(pkgPath, "utf8")).version;
const m = current.match(/^(\d+)\.(\d+)\.(\d+)/);
if (!m) {
  console.error(`Cannot parse current version "${current}".`);
  process.exit(1);
}
const [maj, min, pat] = m.slice(1).map(Number);

let next;
if (/^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$/.test(arg)) next = arg; // explicit, optionally pre-release
else if (arg === "major") next = `${maj + 1}.0.0`;
else if (arg === "minor") next = `${maj}.${min + 1}.0`;
else if (arg === "patch") next = `${maj}.${min}.${pat + 1}`;
else {
  console.error(`Invalid version or bump type: "${arg}". Use patch | minor | major | X.Y.Z`);
  process.exit(1);
}

// Targeted string replaces preserve each file's exact formatting (minimal diffs).
const pkgRaw = readFileSync(pkgPath, "utf8").replace(/("version":\s*")[^"]*(")/, `$1${next}$2`);
writeFileSync(pkgPath, pkgRaw);

const propsRaw = readFileSync(propsPath, "utf8");
if (!/<Version>[^<]*<\/Version>/.test(propsRaw)) {
  console.error("Could not find <Version> in Directory.Build.props.");
  process.exit(1);
}
writeFileSync(propsPath, propsRaw.replace(/<Version>[^<]*<\/Version>/, `<Version>${next}</Version>`));

// Keep package-lock.json in sync with the new workspace version — otherwise `npm ci` (in CI) fails
// with "package.json and package-lock.json ... are out of sync".
console.log("Updating package-lock.json...");
execSync("npm install --package-lock-only", { cwd: root, stdio: "inherit" });

console.log(`\nBumped ${current} -> ${next}`);
console.log("  npm:   @react-dotnetcore/runtime");
console.log("  nuget: ReactDotNetCore.AspNetCore");
console.log("  lock:  package-lock.json");
console.log("\nNext (pushing the tag auto-creates the GitHub Release and publishes both packages):");
console.log(`  git commit -am "release: v${next}"`);
console.log(`  git tag v${next}`);
console.log("  git push --follow-tags");
