# rows-dry-run

## Purpose

Previewing mutating row commands (`rows add`, `rows update`, `rows delete`, `rows upsert`) without writing to the spreadsheet, via the `--dry-run` flag.

## Requirements

### Requirement: Mutating commands accept --dry-run
The commands `rows add`, `rows update`, `rows delete`, and `rows upsert` SHALL accept a `--dry-run` flag. When set, the command MUST perform all reads and validation exactly as a real run — resolve the sheet, load and validate the header, parse `--json`/`--set`/`--where`, and match rows — but MUST NOT issue any write request to the Google Sheets API.

#### Scenario: rows add dry run writes nothing
- **WHEN** `tbxl rows add <id> --json '{"Name":"John"}' --dry-run` is executed against a sheet with a valid header
- **THEN** no append request is sent to the API, and the command exits with code 0

#### Scenario: rows update dry run writes nothing
- **WHEN** `tbxl rows update <id> --where 'Status=Open' --set 'Status=Done' --all --dry-run` matches at least one row
- **THEN** no cell-update request is sent to the API, and the command exits with code 0

#### Scenario: rows delete dry run writes nothing
- **WHEN** `tbxl rows delete <id> --where 'Status=Done' --all --dry-run` matches at least one row
- **THEN** no delete request is sent to the API, and the command exits with code 0

#### Scenario: rows upsert dry run writes nothing
- **WHEN** `tbxl rows upsert <id> --where 'Email=a@b.c' --json '{"Name":"John"}' --dry-run` is executed against a sheet with a valid header
- **THEN** neither an update nor an append request is sent to the API, and the command exits with code 0

#### Scenario: validation errors still fail in dry run
- **WHEN** a mutating command is executed with `--dry-run` and invalid input (e.g. unknown column in `--set`, malformed `--json`, broken header)
- **THEN** the command fails with the same error code and message as a real run

### Requirement: Dry-run output reports the would-be effect
A dry run SHALL report the same affected-row count and the same receipt fields the real run would have produced: the payload shape is identical to the real run's so previews can be diffed against actual results. The JSON payload MUST include a `dry_run` field (`true` on dry runs, `false` on real runs); for `rows update`/`rows delete` it MUST also include the `matched` field defined by `rows-match-reporting` and the `rows`/`truncated`/`returned` receipt fields defined by `rows-mutation-receipts`, where `before`/`after` (update) show what *would* change and `data` (delete) what *would* be removed. For `rows add`, `row` MUST hold the predicted append position. Text output MUST use "Would …" phrasing instead of past tense.

#### Scenario: JSON output marks dry run
- **WHEN** `tbxl rows update … --dry-run --output json` matches 3 rows
- **THEN** stdout contains `"matched": 3`, `"affected": 3`, `"dry_run": true`, and a `rows` array of 3 receipt elements with `before`/`after`

#### Scenario: dry-run delete carries the would-be receipts
- **WHEN** `tbxl rows delete … --all --dry-run --output json` matches 2 rows
- **THEN** stdout contains a `rows` array with the 2 records under `data`, and no delete request is sent

#### Scenario: text output uses conditional phrasing
- **WHEN** `tbxl rows delete … --dry-run` matches 2 rows with text output
- **THEN** the message reads "Would delete 2 record(s) …" rather than "Deleted 2 record(s) …"

#### Scenario: real runs keep a stable schema
- **WHEN** a mutating command runs without `--dry-run` and `--output json`
- **THEN** the payload contains `"dry_run": false` and the same receipt fields as the dry run

### Requirement: Dry run preserves exit-code semantics
A dry run SHALL exit with the same code the real run would have: `Success` (0) when the write would happen, `NoMatch` (2) when `--where` matches zero rows for `rows update`/`rows delete`. With `--output json`, the zero-match case MUST additionally emit the payload `{"matched": 0, "affected": 0, "dry_run": true, "rows": [], "truncated": false, "returned": 0}` on stderr per `rows-match-reporting`.

#### Scenario: zero matches in dry run
- **WHEN** `tbxl rows delete <id> --where 'Status=Nope' --all --dry-run` matches no rows with text output
- **THEN** a warning is written to stderr and the command exits with code 2

#### Scenario: zero matches in dry run with JSON output
- **WHEN** `tbxl rows delete <id> --where 'Status=Nope' --all --dry-run --output json` matches no rows
- **THEN** stderr contains `"matched": 0`, `"affected": 0`, `"dry_run": true`, and `"rows": []`, stdout is empty, and the command exits with code 2

### Requirement: rows delete dry run does not require --yes
`rows delete --dry-run` SHALL NOT require the `--yes` confirmation flag, because no deletion occurs. Without `--dry-run`, the `--yes` requirement is unchanged.

#### Scenario: preview delete without confirmation
- **WHEN** `tbxl rows delete <id> --where 'Status=Done' --all --dry-run` is executed without `--yes`
- **THEN** the command succeeds and reports the would-be deletion count

#### Scenario: real delete still requires confirmation
- **WHEN** `tbxl rows delete <id> --where 'Status=Done' --all` is executed without `--yes` and without `--dry-run`
- **THEN** the command fails with `ConfirmationRequired` and modifies nothing
