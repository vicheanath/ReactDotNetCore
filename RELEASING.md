# Releasing

Both packages ship **in lockstep** — the same version, released together:

| Package | Registry | Version source |
| --- | --- | --- |
| `@react-dotnetcore/runtime` | npm | `packages/react-dotnetcore-runtime/package.json` |
| `ReactDotNetCore.AspNetCore` | NuGet | `Directory.Build.props` (`<Version>`) |

They're a matched pair (the .NET engine and its JS runtime), so they always carry the same version.
The `npm run bump` command keeps both files in sync.

## Release steps

```bash
# 1. Bump both packages to the same new version
npm run bump minor          # or: patch | major | an explicit version like 0.3.0

# 2. Commit, tag (vX.Y.Z), and push the tag — that's the whole release
git commit -am "release: v0.2.0"
git tag v0.2.0
git push --follow-tags
```

Pushing the tag fires two workflows in parallel:

- **`release.yml`** — creates the GitHub Release for `v0.2.0` with auto-generated notes.
- **`publish.yml`** — `verify` (tag matches both versions) → `npm` and `nuget` publish via OIDC
  Trusted Publishing (+ provenance/attestation).

No tokens are stored — both registries authenticate with short-lived OIDC credentials. See
[`docs/deployment.md`](docs/deployment.md) for the publishing-environment details.

> **Why the tag (not the GitHub Release) triggers publishing:** a release created by a workflow with
> the default `GITHUB_TOKEN` does **not** trigger other workflows (GitHub anti-recursion). So
> publishing listens for the **tag push** directly, and `release.yml` creates the release alongside it.

### One-time environment setting

The publish jobs deploy to the `production` environment. In **Settings → Environments → production →
Deployment branches and tags**, make sure tags are allowed (either "All branches and tags", or add a
tag rule like `v*`). Otherwise a tag-triggered deployment is blocked. If you want a manual approval
gate before publishing, add **required reviewers** to that environment — the publish jobs will pause
for approval (the release is still created immediately).

## Rules of thumb

- **Bump before every release.** npm and NuGet both reject re-publishing an existing version; the
  `verify` job catches a forgotten bump before anything is published.
- **One tag, one version.** The tag (`vX.Y.Z`) must equal the version in the files.
- **SemVer.** `patch` = fixes, `minor` = backwards-compatible features, `major` = breaking changes.
- **Pre-releases** are supported: `npm run bump 0.3.0-rc.1` (publishes under the `latest` npm tag —
  adjust the workflow if you want a separate dist-tag).

## Why lockstep (and not independent versions)?

The engine and runtime are designed and tested as one unit at a matching version, so independent
versioning would only create confusing compatibility matrices. If you later split them or add more
packages that version independently, consider a tool like
[Changesets](https://github.com/changesets/changesets) (npm) or
[release-please](https://github.com/googleapis/release-please) (multi-ecosystem).
