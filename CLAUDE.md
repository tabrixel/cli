# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Tabrixel is a .NET 10 CLI application (binary name: `tbxl`) for working with Google Sheets, built on Spectre.Console.Cli. Two projects: `Tabrixel` (the CLI) and `Tabrixel.Tests` (xUnit).

## Commands

```powershell
dotnet build                                        # build
dotnet run --project Tabrixel -- auth check         # run a CLI command
dotnet run --project Tabrixel -- auth check --json
dotnet publish Tabrixel -r win-x64 -c Release       # self-contained single-file publish
```

Supported RIDs: win-x64, win-arm64, linux-x64, osx-arm64.

Running commands for real requires a Google service account JSON key, passed via `--credentials <path>`, the `GOOGLE_APPLICATION_CREDENTIALS` environment variable, or the `credentials` config key (`tbxl config set credentials <path>`).

## Architecture

Commands are registered in `Tabrixel/Program.cs` using Spectre.Console.Cli branches (e.g. `auth` branch → `auth check`). `Tabrixel/Infrastructure/` defines the conventions every command must follow; `Tabrixel/Sheets/` holds the Google Sheets integration.

### Command conventions (`Tabrixel/Infrastructure/`)

- **Command base class**: commands inherit `CliCommand<TSettings>` (not Spectre's `Command<T>` directly), where `TSettings : GlobalSettings`. Implement `ExecuteCommand(...)` returning an `ExitCodes` value. The base class converts it to the process exit code and handles all exceptions.

- **Dual output format**: every command supports a global boolean `--json` flag (human-readable text is the default) exposed as `GlobalSettings.Format` (`OutputFormat.Json`/`Text`), backed by the `GlobalSettings.JsonOutput` option. All command output must go through `Renderer.Data(jsonPayload, renderCallback, format)` — pass both a JSON-serializable payload (commands define private `record` payload types) and a Spectre render callback; `Renderer` picks one based on the format. JSON is serialized with snake_case property names. Never write to the console directly.

- **Error handling**: throw `CliException(ErrorCode.X, message, optionalDetailsDictionary)` for expected failures. The `CliCommand` base catches it and renders via `Renderer.Error` (respects `--json`, writes to stderr). Unexpected exceptions are wrapped as `ErrorCode.Internal`.

- Data output goes to stdout, errors to stderr, so JSON output stays machine-parseable.

- **Global options**: `GlobalSettings` defines `--json`, `--credentials`, `--spreadsheet-id`, and `--sheet`. Never read the raw option properties or env variables for these in command code — resolve through `GlobalSettings.ResolveCredentialsPath()` / `ResolveSpreadsheetId()` / `ResolveSheetName()` (or `SpreadsheetSettings.RequireSpreadsheetId()`), which implement the full precedence chain.

### Configuration layer (`Tabrixel/Configuration/`)

- Values resolve top-down, first present (non-null) level wins: CLI argument → flag → env (`TBXL_SPREADSHEET_ID`, `TBXL_SHEET`, `GOOGLE_APPLICATION_CREDENTIALS`) → project config → global config. An explicitly empty value (`--flag ""` or `key = ""`) is *present* and stops the chain — `ValueResolver` distinguishes `null` (absent) from `""` (deliberate). The chain is implemented once in `ValueResolver`; do not re-implement fallbacks elsewhere.
- Config files are TOML (`Tomlyn`), keys: `spreadsheet-id`, `sheet`, `credentials` (defined in `ConfigKeys`). Project config `.tabrixel/config.toml` is discovered walking up from the cwd (like `.git`); global is `~/.tabrixel/config.toml`; project overrides global per key. `ConfigLocator` owns path logic, `ConfigFile` owns load/save.
- `config set/get/list` manage the store (`Tabrixel/Commands/Config/`); `auth check` reports the source of every resolved value.
- Exception: `describe` ignores env/config sheet defaults — its `--sheet` is a filter over a whole-document overview, honored only as an explicit flag.

### Google Sheets layer (`Tabrixel/Sheets/`)

- `SheetsServiceFactory.Create(settings)` loads the service account key and returns a `SheetsClient` record bundling the `SheetsService`, the credential, and the service account email. It throws `CliException(ErrorCode.AuthFailed, ...)` for missing/invalid keys.
- `GoogleApiErrorMapper.ToSpreadsheetAccessError(...)` is the extension to convert a caught `GoogleApiException` into the appropriate `CliException` (401 → `AuthFailed`, 403/404 → `NotFound`, else `Internal`). Use it instead of mapping HTTP status codes inline.

Application name/version and the `TBXL_` env-var prefix live in `Tabrixel/Consts.cs` — new environment variables should be built from `Consts.EnvVariablePrefix`.
