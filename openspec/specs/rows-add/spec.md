# rows-add

## Purpose

Appending a record to a sheet from a JSON object via the `rows add` command.

## Requirements

### Requirement: Append one record from --json
The CLI SHALL provide a `rows add` command that appends one row after the last non-empty row of the target sheet, laying values out by column names from a required JSON record passed as the command's first positional argument. The `rows add` command SHALL NOT accept a positional spreadsheet ID; its spreadsheet ID SHALL be resolved from `--spreadsheet-id`, the `TBXL_SPREADSHEET_ID` environment variable, or config. Values SHALL be written RAW (no Sheets interpretation). Fields missing from the object SHALL be left as empty cells. An invalid header SHALL fail with `HeaderInvalid` before any write. Omitting the positional JSON record SHALL fail with `InvalidArguments` rendered per the output format (not as a parser error).

#### Scenario: Successful add
- **WHEN** `tbxl rows add '{"Name":"John","Age":30}'` is run against a valid sheet (the spreadsheet ID resolved from `--spreadsheet-id`, env, or config)
- **THEN** one row is appended with `Name` and `Age` cells set, other columns empty, and the command reports 1 affected record

#### Scenario: Missing JSON record
- **WHEN** `rows add` is run with no positional JSON record
- **THEN** the command fails with `InvalidArguments` rendered per the output format

### Requirement: Strict --json validation
The JSON record MUST be a flat JSON object: invalid JSON, a non-object root, an empty object, duplicate fields, nested objects/arrays, or non-finite numbers SHALL fail with `InvalidArguments`; an unknown field name SHALL fail with `ColumnNotFound` (with `did_you_mean` when applicable). Scalar conversion: strings as-is, `null` → empty cell, booleans → `true`/`false`, numbers in invariant culture without scientific notation (e.g. `1e3` → `1000`).

#### Scenario: Unknown field
- **WHEN** the JSON record contains a field that is not a header column
- **THEN** the command fails with `ColumnNotFound` and nothing is written

#### Scenario: Nested value rejected
- **WHEN** a field value is an object or array
- **THEN** the command fails with `InvalidArguments` and nothing is written

### Requirement: Mutation result output
On success the command SHALL exit 0 and report the affected count and the destination row: with `--json`, stdout carries the payload `{"affected": 1, "dry_run": <bool>, "row": N}` where `row` is the 1-based sheet row number the record landed in (per `rows-mutation-receipts`), taken from the append response on real runs. On `--dry-run`, `row` SHALL be the predicted append position (one past the last non-empty row of the snapshot). Text output prints a human-readable confirmation that includes the row number.

#### Scenario: JSON result
- **WHEN** `rows add` succeeds with `--json` and the record lands in sheet row 12
- **THEN** stdout contains `"affected": 1`, `"dry_run": false`, and `"row": 12`

#### Scenario: Text result names the row
- **WHEN** `rows add` succeeds with text output
- **THEN** the confirmation message includes the destination row number

#### Scenario: Dry run predicts the row
- **WHEN** `rows add '{…}' --dry-run --json` runs against a sheet whose last non-empty row is 11
- **THEN** no write occurs and stdout contains `"row": 12` and `"dry_run": true`
