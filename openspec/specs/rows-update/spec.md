# rows-update

## Purpose

Updating cells of rows matched by `--where` conditions via the `rows update` command.

## Requirements

### Requirement: Update matched rows with --set
The CLI SHALL provide a `rows update` command that updates cells of rows matched by repeatable `--where 'Column=value'` conditions (same syntax and errors as `rows list`), assigning values from repeatable, required `--set 'Column=value'` options. `'Column='` in `--set` SHALL clear the cell. Writes SHALL be RAW and touch only the assigned cells. `--set` SHALL be validated before any read/write: unknown columns → `ColumnNotFound`; the same column assigned more than once → `InvalidArguments`; missing `--set` → `InvalidArguments`. An invalid header SHALL fail with `HeaderInvalid` before any write.

#### Scenario: Update a single matching row
- **WHEN** `tbxl rows update --spreadsheet-id <id> --where 'Name=John' --set 'Status=Done'` matches exactly one row
- **THEN** only that row's `Status` cell is updated and the command reports 1 affected record

#### Scenario: Duplicate --set column
- **WHEN** two `--set` options assign the same column
- **THEN** the command fails with `InvalidArguments` and nothing is written

### Requirement: Match-count semantics (--all / --first)
When more than one row matches and neither `--all` nor `--first` is given, the command SHALL fail with `AmbiguousMatch` (details include the match count). `--first` SHALL apply the update to the first match only; `--all` to every match. Passing both `--all` and `--first` SHALL fail with `InvalidArguments`.

#### Scenario: Ambiguous match
- **WHEN** the conditions match 3 rows and no flag is given
- **THEN** the command fails with `AmbiguousMatch` and no row is updated

#### Scenario: Update all matches
- **WHEN** `--all` is given and 3 rows match
- **THEN** all 3 rows are updated and the affected count is 3

### Requirement: Zero matches is a warning, exit code 2
When the conditions match 0 rows the command SHALL write a warning to stderr, write nothing to the sheet, and exit with code 2 (`NoMatch`) — not an error.

#### Scenario: Nothing matched
- **WHEN** the conditions match no rows
- **THEN** stderr carries a warning, stdout carries no affected-result, and the exit code is 2

### Requirement: Mutation result output
On success the command SHALL report the affected count and a receipt of the affected rows. With `--json`, stdout carries a payload with `matched`, `affected`, `dry_run`, a `rows` array, and the `truncated`/`returned` fields defined by `rows-mutation-receipts`. Each `rows` element SHALL carry `row` (1-based sheet row number per `rows-mutation-receipts`) plus `before` and `after` objects containing exactly the union of the columns assigned by `--set` and the columns referenced by `--where`, in header column order — not the full record. `before` SHALL hold the cell values from the pre-write snapshot; `after` the values as written (a cleared cell appears as `""`). Text output SHALL print the confirmation message plus a per-row summary of the same receipt (row number, `before` → `after` per assigned column), capped by the same truncation rule.

#### Scenario: JSON result carries before/after
- **WHEN** `tbxl rows update --spreadsheet-id <id> --where 'Email=a@b.c' --set 'Status=Done' --json` matches one row at sheet row 5 whose `Status` was `Open`
- **THEN** stdout contains `"affected": 1` and a `rows` element with `"row": 5`, `before` of `{"Email": "a@b.c", "Status": "Open"}`, and `after` of `{"Email": "a@b.c", "Status": "Done"}`

#### Scenario: Cleared cell in after
- **WHEN** an update with `--set 'Status='` succeeds with `--json`
- **THEN** the matched row's `after` object contains `"Status": ""`

#### Scenario: Unassigned columns are not in the receipt
- **WHEN** the sheet has columns `Name`, `Email`, `Status` and the update uses `--where 'Email=a@b.c' --set 'Status=Done'`
- **THEN** `before`/`after` contain only `Email` and `Status`, not `Name`
