# rows-delete

## Purpose

Deleting rows matched by `--where` conditions via the `rows delete` command.

## Requirements

### Requirement: Delete matched rows with confirmation
The CLI SHALL provide a `rows delete` command that deletes whole rows matched by repeatable `--where 'Column=value'` conditions (same syntax and errors as `rows list`). The command SHALL require `--yes`: without it, it SHALL fail with `ConfirmationRequired` before any API access (the sheet is neither read nor modified). Rows below deleted rows move up; deletion ranges SHALL be applied bottom-up so earlier deletions do not shift later ones. An invalid header SHALL fail with `HeaderInvalid` before any deletion.

#### Scenario: Delete without --yes
- **WHEN** `tbxl rows delete --spreadsheet-id <id> --where 'Status=Done'` is run without `--yes`
- **THEN** the command fails with `ConfirmationRequired` and no API call is made

#### Scenario: Delete a single matching row
- **WHEN** `--yes` is given and the conditions match exactly one row
- **THEN** that row is deleted and the command reports 1 affected record

### Requirement: Match-count semantics (--all / --first)
When more than one row matches and neither `--all` nor `--first` is given, the command SHALL fail with `AmbiguousMatch` (details include the match count). `--first` SHALL delete only the first match; `--all` every match. Passing both `--all` and `--first` SHALL fail with `InvalidArguments`.

#### Scenario: Delete all matches
- **WHEN** `--yes --all` is given and 3 rows match
- **THEN** all 3 rows are deleted in one batch request and the affected count is 3

### Requirement: Zero matches is a warning, exit code 2
When the conditions match 0 rows the command SHALL write a warning to stderr, delete nothing, and exit with code 2 (`NoMatch`) — not an error.

#### Scenario: Nothing matched
- **WHEN** `--yes` is given but the conditions match no rows
- **THEN** stderr carries a warning and the exit code is 2

### Requirement: Mutation result output
On success the command SHALL report the affected count and a receipt of the deleted rows. With `--json`, stdout carries a payload with `matched`, `affected`, `dry_run`, a `rows` array, and the `truncated`/`returned` fields defined by `rows-mutation-receipts`. Each `rows` element SHALL carry `row` (1-based sheet row number per `rows-mutation-receipts`, as it was before the deletion) and `data` — the **full** record of the deleted row in `rows list` shape, serving as a restore receipt. Text output SHALL print the confirmation message plus a per-row summary of the same receipt, capped by the same truncation rule.

#### Scenario: JSON result carries deleted records
- **WHEN** `tbxl rows delete --spreadsheet-id <id> --where 'Status=Archived' --yes --json` matches one row at sheet row 7 with values `Name=Anna`, `Email=anna@x.ru`, `Status=Archived`
- **THEN** stdout contains `"affected": 1` and a `rows` element with `"row": 7` and `data` of `{"Name": "Anna", "Email": "anna@x.ru", "Status": "Archived"}`

#### Scenario: Receipt includes all columns
- **WHEN** a delete succeeds and the matched row has empty cells in some columns
- **THEN** `data` contains every header column, with empty cells as `""`
