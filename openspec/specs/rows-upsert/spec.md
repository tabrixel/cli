# rows-upsert

## Purpose

Idempotent "update or insert" of a record via the `rows upsert` command: rows matched by `--where` are updated with the fields of the positional JSON record; when nothing matches, a new row is inserted from the union of both.

## Requirements

### Requirement: Upsert a record matched by --where
The CLI SHALL provide a `rows upsert` command that matches rows by repeatable `--where 'Column=value'` conditions (same syntax and errors as `rows list`) and applies the fields of a required JSON record passed as the command's first positional argument. The `rows upsert` command SHALL NOT accept a positional spreadsheet ID; its spreadsheet ID SHALL be resolved from `--spreadsheet-id`, the `TBXL_SPREADSHEET_ID` environment variable, or config. When at least one row is selected, the command SHALL update the selected rows, writing one cell per record field (RAW), leaving columns absent from the record untouched. When no row matches, the command SHALL append one new row whose cells are the union of the `--where` condition values and the record fields, laid out by column names like `rows add`. At least one `--where` condition is required and the positional JSON record is required; omitting either SHALL fail with `InvalidArguments`. An invalid header SHALL fail with `HeaderInvalid` before any write.

#### Scenario: Existing record is updated
- **WHEN** `tbxl rows upsert --where 'Email=a@b.c' '{"Status":"Done"}'` matches exactly one row
- **THEN** only that row's `Status` cell is updated and the command reports the update outcome

#### Scenario: Missing record is inserted
- **WHEN** `tbxl rows upsert --where 'Email=a@b.c' '{"Name":"John"}'` matches no rows
- **THEN** one row is appended with `Email` set to `a@b.c`, `Name` set to `John`, other columns empty, and the command exits with code 0

#### Scenario: Missing --where
- **WHEN** `rows upsert` is run without any `--where` condition
- **THEN** the command fails with `InvalidArguments` rendered per the output format and nothing is written

#### Scenario: Re-running is idempotent
- **WHEN** the same `rows upsert` invocation runs twice against the same sheet with no concurrent writers
- **THEN** the second run updates the row inserted by the first instead of appending a duplicate

### Requirement: --json validation and --where conflict rule
The JSON record MUST pass the same strict validation as `rows add` (flat object, no nested values, known columns with `ColumnNotFound`/`did_you_mean`, identical scalar conversion). A column referenced by both a `--where` condition and a record field MUST carry the same cell value after scalar conversion; differing values SHALL fail with `InvalidArguments` naming the column, before any write. Two `--where` conditions on the same column with differing values SHALL likewise fail with `InvalidArguments`: no row could ever match such conditions, so every run would insert a fresh duplicate, breaking idempotency.

#### Scenario: Key repeated in the record with equal value
- **WHEN** `--where 'Email=a@b.c'` is combined with the record `'{"Email":"a@b.c","Name":"John"}'`
- **THEN** the command succeeds; on insert the `Email` cell is written once with `a@b.c`

#### Scenario: Conflicting value between --where and the record
- **WHEN** `--where 'Email=a@b.c'` is combined with the record `'{"Email":"x@y.z"}'`
- **THEN** the command fails with `InvalidArguments` and nothing is written

#### Scenario: Conflicting --where conditions on one column
- **WHEN** `--where 'Status=Open' --where 'Status=Done'` are both given
- **THEN** the command fails with `InvalidArguments` and nothing is written

### Requirement: Match-count semantics (--all / --first)
When more than one row matches and neither `--all` nor `--first` is given, the command SHALL fail with `AmbiguousMatch` (details include the match count) and write nothing. `--first` SHALL update only the first match; `--all` every match. Passing both `--all` and `--first` SHALL fail with `InvalidArguments`. Zero matches SHALL NOT be treated as `NoMatch`: it is the insert path and exits with code 0.

#### Scenario: Ambiguous match
- **WHEN** the conditions match 3 rows and neither `--all` nor `--first` is given
- **THEN** the command fails with `AmbiguousMatch` and no row is updated or inserted

#### Scenario: Update all matches
- **WHEN** `--all` is given and 3 rows match
- **THEN** all 3 rows are updated and the affected count is 3

### Requirement: Upsert result output
On success the command SHALL report the outcome: with `--json`, stdout carries a single payload object with the fields `matched` (rows matching `--where`), `affected` (rows written: the selected count on update, `1` on insert), `action` (`"updated"` or `"inserted"`), and `dry_run`. On the update path the payload SHALL additionally carry a `rows` array shaped like the `rows update` receipt — each element with `row` and `before`/`after` objects containing the union of the record's field columns and the `--where` key columns — plus the `truncated`/`returned` fields defined by `rows-mutation-receipts`. On the insert path the payload SHALL instead carry `row`: the 1-based sheet row the record landed in, taken from the append response. Text output SHALL print a confirmation naming the action and the affected count; the insert confirmation includes the row number, the update confirmation is followed by the per-row receipt summary. The exit code SHALL be 0 for both actions.

#### Scenario: JSON result for an update
- **WHEN** an upsert with `--where 'Email=a@b.c' '{"Status":"Done"}'` updates 1 matched row at sheet row 5 with `--json`
- **THEN** stdout contains `"matched": 1`, `"affected": 1`, `"action": "updated"`, `"dry_run": false`, and a `rows` element with `"row": 5` and `before`/`after` covering `Email` and `Status`

#### Scenario: JSON result for an insert
- **WHEN** an upsert matching no rows appends its record to sheet row 12 with `--json`
- **THEN** stdout contains `"matched": 0`, `"affected": 1`, `"action": "inserted"`, `"dry_run": false`, and `"row": 12`

### Requirement: Dry-run reports the would-be action
With `--dry-run`, the command SHALL perform all reads, validation, and matching but write nothing, and SHALL report the action a real run would have taken: the JSON payload carries `"dry_run": true` with the same `matched`/`affected`/`action` values and the same receipt fields as the real run — `rows` with the would-be `before`/`after` on the update path, `row` on the insert path, where `row` is the predicted append position (one past the last non-empty row of the snapshot). Text output uses `Would update` / `Would insert` phrasing. The exit code SHALL match the real run's.

#### Scenario: Dry-run insert
- **WHEN** `tbxl rows upsert --where 'Email=a@b.c' '{"Name":"John"}' --dry-run --json` matches no rows and the sheet's last non-empty row is 11
- **THEN** no write request is sent, stdout contains `"action": "inserted"`, `"affected": 1`, `"dry_run": true`, and `"row": 12`, and the exit code is 0

#### Scenario: Dry-run update carries the would-be receipt
- **WHEN** a dry-run upsert matches one row and would change `Status` from `Open` to `Done`
- **THEN** the `rows` element shows `before` with `"Status": "Open"` and `after` with `"Status": "Done"`, and no write request is sent
