# describe

## Purpose

Reporting the structure of a spreadsheet (sheets, columns, record counts) via the `describe` command.

## Requirements

### Requirement: Document overview
The CLI SHALL provide a `describe` command that reports, for each sheet of the document, the sheet name, its column names (parsed from the first row), and the number of records (rows below the header). The spreadsheet ID SHALL be resolved from `--spreadsheet-id`, the `TBXL_SPREADSHEET_ID` environment variable, or config (per `value-resolution`); if none is set the command SHALL fail with `InvalidArguments`.

#### Scenario: Describe all sheets
- **WHEN** `tbxl describe --spreadsheet-id <id>` is run without `--sheet` on a document with multiple sheets
- **THEN** every sheet is described (no `SheetAmbiguous` error), each with its name, columns, and record count

#### Scenario: Describe a single sheet
- **WHEN** `tbxl describe --spreadsheet-id <id> --sheet Tasks` is run and the sheet exists
- **THEN** only that sheet is described

#### Scenario: Named sheet not found
- **WHEN** `--sheet` names a sheet that does not exist
- **THEN** the command fails with `NotFound` and the error lists the available sheet titles

#### Scenario: Missing spreadsheet ID
- **WHEN** no spreadsheet ID is provided by `--spreadsheet-id`, env, or config
- **THEN** the command fails with `InvalidArguments` rendered per the output format (exit code 1)

### Requirement: Invalid header does not fail describe
A sheet whose first row is an invalid header (empty, gaps before the last non-empty cell, or duplicate names) SHALL be reported inline with a `HeaderInvalid` error entry instead of failing the whole command; the command SHALL still exit 0.

#### Scenario: One sheet has a broken header
- **WHEN** a document contains a sheet with duplicate header names among valid sheets
- **THEN** that sheet's entry carries the `HeaderInvalid` code and message, other sheets are described normally, and the exit code is 0

### Requirement: Dual output format
With `--json` the command SHALL emit a single JSON document `{"spreadsheet_id": ..., "sheets": [{"name", "columns", "records"} | {"name", "error": {"code", "message", ...}}]}` to stdout; with the default text output it SHALL render a table with Sheet/Records/Columns columns.

#### Scenario: JSON output
- **WHEN** `tbxl describe --spreadsheet-id <id> --json` is run
- **THEN** stdout contains exactly one JSON document with `spreadsheet_id` and a `sheets` array
