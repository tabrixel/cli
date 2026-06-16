# json-output

## Purpose

JSON serialization conventions for all CLI output produced with the `--json` flag (data on stdout, warnings and errors on stderr).

## Requirements

### Requirement: JSON output is selected by the --json flag
The CLI SHALL emit machine-readable JSON instead of human-readable text when, and only when, the global boolean `--json` flag is present; text output SHALL be the default when `--json` is absent. The flag SHALL be a global option available on every command, and SHALL select JSON for both data (stdout) and warnings/errors (stderr). The CLI SHALL NOT expose an `--output` option.

#### Scenario: --json selects JSON output
- **WHEN** any command is run with `--json`
- **THEN** its data payload is serialized as JSON to stdout and any error or warning is serialized as JSON to stderr

#### Scenario: Text is the default
- **WHEN** a command is run without `--json`
- **THEN** it renders human-readable text and emits no JSON

#### Scenario: --output is no longer accepted
- **WHEN** a command is run with `--output json`
- **THEN** the CLI fails with a parse error because `--output` is not a recognized option

### Requirement: Enums serialize as snake_case strings in JSON output
All JSON emitted by the CLI (data on stdout, warnings and errors on stderr) SHALL serialize enum values as the snake_case string form of the enum member name, never as the underlying numeric value.

#### Scenario: Error code is a string
- **WHEN** a command fails with `ErrorCode.ConfirmationRequired` and `--json` is set
- **THEN** the error JSON on stderr contains `"code": "confirmation_required"`

#### Scenario: Any error code round-trips by name
- **WHEN** a command fails with any `ErrorCode` value under `--json`
- **THEN** the `code` field equals the snake_case form of that enum member name (e.g. `AuthFailed` → `"auth_failed"`), and no numeric enum value appears in the payload

### Requirement: JSON property names use snake_case
JSON payloads SHALL use snake_case property names (e.g. `spreadsheet_id`, not `spreadsheetId`).

#### Scenario: Error envelope property names
- **WHEN** a `CliError` is rendered with `--json`
- **THEN** its properties appear as `code`, `message`, and `details` in snake_case
