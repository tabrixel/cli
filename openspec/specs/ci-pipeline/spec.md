# ci-pipeline

## Purpose

Automated build-and-test validation of the Tabrixel solution on every push and pull request, run via GitHub Actions, so regressions are caught before merge.

## Requirements

### Requirement: Build and test on push and pull request
The CI pipeline SHALL build the solution and run the full xUnit test suite on every push to the default branch and on every pull request targeting it. The pipeline SHALL resolve the solution from its location under the `src/` directory (`src/Tabrixel.slnx`) rather than relying on solution auto-discovery in the repository root. The pipeline SHALL fail (non-zero result) when the build fails or any test fails.

#### Scenario: Pull request with passing tests
- **WHEN** a pull request is opened or updated against the default branch and all tests pass
- **THEN** the CI workflow restores, builds, and runs the test suite for the `src/Tabrixel.slnx` solution
- **AND** the workflow reports success

#### Scenario: Push with a failing test
- **WHEN** a commit is pushed and at least one test fails
- **THEN** the CI workflow runs the test suite and reports failure

#### Scenario: Build error
- **WHEN** the code does not compile
- **THEN** the CI workflow fails at the build step before tests run

#### Scenario: Solution located under src
- **WHEN** the CI workflow runs against the relocated layout where the solution lives at `src/Tabrixel.slnx`
- **THEN** the restore, build, and test steps locate the solution and projects under `src/` and do not fail with a "no project or solution found" error

### Requirement: Pinned .NET 10 toolchain
The CI pipeline SHALL provision the .NET 10 SDK required by the projects' `net10.0` target framework, using a pinned SDK setup so builds are reproducible.

#### Scenario: SDK provisioned
- **WHEN** the CI workflow starts a job
- **THEN** it installs the .NET 10 SDK before any `dotnet` command runs
