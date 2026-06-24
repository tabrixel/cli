# nuget-release

## Purpose

Tag-gated packaging and publishing of the `tbxl` tool to NuGet.org via GitHub Actions, gated on a passing test run and a `vX.Y.Z` tag, using NuGet.org Trusted Publishing (OIDC) instead of a long-lived API key.

## Requirements

### Requirement: Release triggered only by version tags
The release workflow SHALL trigger only on git tags matching the pattern `v[0-9]+.[0-9]+.[0-9]+` (e.g. `v0.0.0`, `v1.2.3`). It SHALL NOT publish on branch pushes, pull requests, or tags that do not match this pattern.

#### Scenario: Matching version tag pushed
- **WHEN** a tag of the form `vX.Y.Z` is pushed
- **THEN** the release workflow is triggered

#### Scenario: Non-version tag pushed
- **WHEN** a tag that does not match `vX.Y.Z` is pushed (e.g. `nightly`, `v1.2`)
- **THEN** the release workflow does not run

#### Scenario: Ordinary branch push
- **WHEN** a commit is pushed to a branch without a matching tag
- **THEN** the release workflow does not run

### Requirement: Publish gated on passing tests
The release workflow SHALL run the full test suite before publishing and SHALL publish to NuGet only if every test passes. If any test fails, the workflow SHALL NOT pack or push the package.

#### Scenario: Tests pass
- **WHEN** the release workflow runs on a version tag and all tests pass
- **THEN** the workflow proceeds to pack and push the package to NuGet

#### Scenario: Tests fail
- **WHEN** the release workflow runs on a version tag and a test fails
- **THEN** the workflow stops and does not push any package to NuGet

### Requirement: Package version derived from the tag
The release workflow SHALL set the published package version from the triggering tag, stripping the leading `v`, overriding the `<Version>` value declared in `Tabrixel.csproj`.

#### Scenario: Version matches tag
- **WHEN** the workflow runs for tag `v0.0.0`
- **THEN** the produced `.nupkg` has package version `0.0.0`

### Requirement: Publish the tbxl tool package to NuGet.org via Trusted Publishing
The release workflow SHALL pack the `Tabrixel` project (the packable `tbxl` .NET tool) at its location under the `src/` directory (`src/Tabrixel/Tabrixel.csproj`) and push the resulting `.nupkg` to NuGet.org using NuGet.org Trusted Publishing (OIDC), NOT a long-lived API key secret. The publish job SHALL be granted `id-token: write` permission and SHALL exchange the GitHub OIDC token for a short-lived NuGet API key (via the `NuGet/login` action) immediately before pushing. Publishing SHALL be idempotent with respect to already-published versions (a re-run for an existing version SHALL NOT fail the workflow).

#### Scenario: Successful publish
- **WHEN** tests pass and a matching Trusted Publishing policy exists on nuget.org
- **THEN** the workflow obtains a short-lived API key via OIDC and packs `src/Tabrixel/Tabrixel.csproj`, then pushes the `Tabrixel` package to NuGet.org

#### Scenario: No matching trusted publishing policy
- **WHEN** no nuget.org Trusted Publishing policy matches the repository owner, repository, and `release.yml` workflow file
- **THEN** the OIDC token exchange fails and the workflow fails before pushing

#### Scenario: Short-lived key requested before push
- **WHEN** the workflow needs to authenticate to NuGet.org
- **THEN** it requests the temporary API key immediately before the push step (the key is valid for one hour)

#### Scenario: Version already published
- **WHEN** the package version produced from the tag already exists on NuGet.org
- **THEN** the push step skips the duplicate without failing the workflow

#### Scenario: Project located under src
- **WHEN** the publish job runs against the relocated layout where the project lives at `src/Tabrixel/Tabrixel.csproj`
- **THEN** the pack step resolves the project under `src/` and does not fail with a missing-project error
