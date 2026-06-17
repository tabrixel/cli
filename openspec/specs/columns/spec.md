# columns

## Purpose

Listing the column names of a sheet via the `columns` command.

## Requirements

### Requirement: List sheet columns
The CLI SHALL provide a `columns` command that lists the column names of the target sheet, parsed from its first row. The spreadsheet ID SHALL come from `--spreadsheet-id`, the `TBXL_SPREADSHEET_ID` environment variable, or config (per `value-resolution`; else `InvalidArguments`). The target sheet SHALL be resolved by `--sheet`; when `--sheet` is omitted and the document has exactly one sheet, that sheet is used; with multiple sheets the command SHALL fail with `SheetAmbiguous` listing available sheets.

#### Scenario: Columns of the single sheet
- **WHEN** `tbxl columns --spreadsheet-id <id>` is run on a document with exactly one sheet
- **THEN** the column names of that sheet are printed in column order

#### Scenario: Ambiguous sheet
- **WHEN** `tbxl columns --spreadsheet-id <id>` is run without `--sheet` on a document with several sheets
- **THEN** the command fails with `SheetAmbiguous` and the error lists the available sheet titles

#### Scenario: Invalid header
- **WHEN** the target sheet's first row is empty, has gaps, or duplicate names
- **THEN** the command fails with `HeaderInvalid` describing the problem

### Requirement: Dual output format
With `--json` the command SHALL emit a JSON array of column name strings to stdout; with text output it SHALL render a table of column letter (A1) and name.

#### Scenario: JSON output
- **WHEN** `tbxl columns --spreadsheet-id <id> --json` is run
- **THEN** stdout contains exactly one JSON array of strings
