# json-output

## Purpose

JSON serialization conventions for all CLI output produced with `--output json` (data on stdout, warnings and errors on stderr).

## Requirements

### Requirement: Enums serialize as snake_case strings in JSON output
All JSON emitted by the CLI (data on stdout, warnings and errors on stderr) SHALL serialize enum values as the snake_case string form of the enum member name, never as the underlying numeric value.

#### Scenario: Error code is a string
- **WHEN** a command fails with `ErrorCode.ConfirmationRequired` and `--output json` is set
- **THEN** the error JSON on stderr contains `"code": "confirmation_required"`

#### Scenario: Any error code round-trips by name
- **WHEN** a command fails with any `ErrorCode` value under `--output json`
- **THEN** the `code` field equals the snake_case form of that enum member name (e.g. `AuthFailed` → `"auth_failed"`), and no numeric enum value appears in the payload

### Requirement: JSON property names use snake_case
JSON payloads SHALL use snake_case property names (e.g. `spreadsheet_id`, not `spreadsheetId`).

#### Scenario: Error envelope property names
- **WHEN** a `CliError` is rendered with `--output json`
- **THEN** its properties appear as `code`, `message`, and `details` in snake_case
