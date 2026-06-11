# Spec: Project README

## Purpose

Requirements for the repository's top-level `README.md` — what sections it must contain and what accuracy guarantees it must uphold (documented commands, flags, and env vars must match the implementation).

## Requirements

### Requirement: Repository has a root README
The repository SHALL contain a `README.md` file at the repository root that introduces Tabrixel (`tbxl`) as a CLI for working with Google Sheets and states the target framework (.NET 10).

#### Scenario: New visitor opens the repository
- **WHEN** a user opens the repository root
- **THEN** `README.md` exists and its opening section explains what the tool is and what it is for

### Requirement: README documents build and publish
The README SHALL document how to build the project, run it during development, and produce a self-contained publish, including the list of supported RIDs (win-x64, win-arm64, linux-x64, osx-arm64).

#### Scenario: User builds from source
- **WHEN** a user follows the README build instructions
- **THEN** the documented commands (`dotnet build`, `dotnet run --project Tabrixel -- ...`, `dotnet publish Tabrixel -r <rid> -c Release`) match the project layout and succeed on a machine with the .NET 10 SDK

### Requirement: README documents authentication
The README SHALL explain that commands require a Google service account JSON key and SHALL list all ways to supply it: the `--credentials` flag, the `GOOGLE_APPLICATION_CREDENTIALS` environment variable, and the `credentials` config key, including their precedence order.

#### Scenario: User sets up credentials
- **WHEN** a user reads the Authentication section
- **THEN** all three credential sources are listed in precedence order and `tbxl auth check` is shown as the verification command

### Requirement: README documents configuration
The README SHALL document the config store: project `.tabrixel/config.toml` (discovered walking up from cwd) and global `~/.tabrixel/config.toml`, the available keys (`spreadsheet-id`, `sheet`, `credentials`), the `config set/get/list` commands, the `TBXL_SPREADSHEET_ID` and `TBXL_SHEET` environment variables, and the full value precedence chain (CLI flag → environment → project config → global config), including that an explicitly empty value stops the chain.

#### Scenario: User looks up value precedence
- **WHEN** a user reads the Configuration section
- **THEN** the precedence chain is stated in resolution order and matches the behavior implemented in `ValueResolver`

### Requirement: README documents every CLI command
The README SHALL list every command registered in `Program.cs` (`auth check`, `config set/get/list`, `describe`, `columns`, `rows list/add/update/upsert/delete`) with a one-line description consistent with the CLI help text and at least one usage example per command group.

#### Scenario: Documented commands match the implementation
- **WHEN** the README command reference is compared against the commands registered in `Program.cs`
- **THEN** every registered command appears in the README and no documented command is absent from the implementation

#### Scenario: User copies an example
- **WHEN** a user copies a README example for a command group
- **THEN** the example's flags and arguments are accepted by the CLI parser

### Requirement: README documents output formats and exit codes
The README SHALL document the `--output text|json` global option, that JSON uses snake_case property names, that data goes to stdout and errors to stderr, and that the process exit code is non-zero on failure.

#### Scenario: User scripts against JSON output
- **WHEN** a user reads the Output section
- **THEN** it states how to request JSON, the casing convention, and the stdout/stderr split so output can be machine-parsed
