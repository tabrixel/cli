# rows-list

## Purpose

Reading sheet rows as records, with filtering, limiting, and column projection, via the `rows list` command.

## Requirements

### Requirement: Read rows as records
The CLI SHALL provide a `rows list` command that reads rows below the header as records keyed by column names (in column order). Missing or null cells SHALL read as empty strings; cells beyond the last header column SHALL be ignored. Sheet and spreadsheet-ID resolution follow the same rules as `columns`. An invalid header SHALL fail the command with `HeaderInvalid`.

#### Scenario: List all records
- **WHEN** `tbxl rows list <id> --sheet Tasks` is run
- **THEN** every row below the header is output as a record with one key per header column

### Requirement: Filtering with --where
The command SHALL accept repeatable `--where 'Column=value'` conditions combined with AND, matched exactly (case-sensitive, no type coercion); `'Column='` SHALL match empty cells. The filter SHALL apply before `--limit`. A condition without `=` or with an empty column name SHALL fail with `InvalidArguments`; an unknown column name SHALL fail with `ColumnNotFound` (with a `did_you_mean` suggestion when a close name exists). Zero matches is valid: empty output, exit code 0.

#### Scenario: Filtered listing
- **WHEN** `--where 'Status=Done'` is passed
- **THEN** only records whose `Status` cell equals `Done` exactly are returned

#### Scenario: Zero matches
- **WHEN** no row matches the conditions
- **THEN** the command outputs an empty result and exits 0

#### Scenario: Unknown column
- **WHEN** `--where 'Stats=Done'` names a non-existent column similar to `Status`
- **THEN** the command fails with `ColumnNotFound` suggesting `Status`

### Requirement: Limiting with --limit
`--limit N` SHALL return at most the first N (matching) records remaining after `--offset` is applied, and SHALL default to 100 when not specified. A non-positive `--limit` SHALL fail with `InvalidArguments`.

#### Scenario: Default limit applied
- **WHEN** `tbxl rows list <id>` is run without `--limit` against a sheet with 250 data rows
- **THEN** the first 100 records are returned and `total` reports 250

#### Scenario: Limit applied after filter
- **WHEN** `--where` matches 5 records and `--limit 2` is passed
- **THEN** the first 2 matching records are returned and `total` reports 5

#### Scenario: Limit applied after offset
- **WHEN** `--where` matches 10 records and `--offset 2 --limit 3` is passed
- **THEN** at most 3 records are returned, starting from the 3rd match

#### Scenario: Invalid limit
- **WHEN** `--limit 0` is passed
- **THEN** the command fails with `InvalidArguments`

### Requirement: Skipping with --offset
`--offset N` SHALL skip the first N matching records before `--limit` is applied; it SHALL default to `0` (skip nothing) when not specified. A negative `--offset` SHALL fail with `InvalidArguments`. The skip SHALL apply after `--where` filtering and before `--limit`, so the effective window is the records from position N up to N + the limit. The `total` field SHALL continue to report the count of records matching `--where` before `--offset` and `--limit` are applied, so a caller can detect remaining pages. An offset at or beyond the matched count SHALL yield empty output and exit code 0.

#### Scenario: Offset skips leading records
- **WHEN** `--where` matches 10 records and `--offset 3` is passed without `--limit`
- **THEN** records starting at the 4th match are returned and `total` reports 10

#### Scenario: Offset combined with limit pages the result
- **WHEN** `--where` matches 10 records and `--offset 2 --limit 3` is passed
- **THEN** the 3rd, 4th, and 5th matching records are returned and `total` reports 10

#### Scenario: Offset beyond matches
- **WHEN** `--where` matches 4 records and `--offset 4` (or larger) is passed
- **THEN** the command outputs an empty result and exits 0

#### Scenario: Default offset preserves current behavior
- **WHEN** `--offset` is not passed
- **THEN** no records are skipped, identical to listing without offset

#### Scenario: Negative offset
- **WHEN** `--offset -1` is passed
- **THEN** the command fails with `InvalidArguments`

### Requirement: Column projection with --columns
The command SHALL accept `--columns <NAME,NAME,...>` (comma-separated, exact case-sensitive names) selecting which columns appear in the output, in the order given. Projection SHALL apply to both JSON records and the text table. An unknown column name SHALL fail with `ColumnNotFound` (with a `did_you_mean` suggestion when a close name exists). An empty list, an empty entry, or a duplicate name SHALL fail with `InvalidArguments`. `--where` conditions SHALL keep validating against the full header, so filtering on a column that is not selected is allowed. The `total` field SHALL be unaffected by `--columns`.

#### Scenario: Projected listing
- **WHEN** `tbxl rows list <id> --columns Имя,Статус` is run against a sheet with columns `Имя`, `Статус`, `Заметки`
- **THEN** each output record contains only the keys `Имя` and `Статус`, in that order

#### Scenario: Filter on a non-selected column
- **WHEN** `--columns Имя --where 'Статус=Done'` is passed
- **THEN** records are filtered by `Статус` but the output contains only the `Имя` column

#### Scenario: Unknown column in projection
- **WHEN** `--columns Статсу` names a non-existent column similar to `Статус`
- **THEN** the command fails with `ColumnNotFound` suggesting `Статус`

#### Scenario: Duplicate column in projection
- **WHEN** `--columns Имя,Имя` is passed
- **THEN** the command fails with `InvalidArguments`

### Requirement: Dual output format
With `--output json` the command SHALL emit to stdout exactly one JSON object with two fields: `rows` — an array of record objects (record per row, keys = selected column names verbatim) — and `total` — the number of records matching the `--where` filter (all records when no filter) before `--limit` is applied. With text output it SHALL render a table with one column per selected header column, followed by a summary line reporting the returned record count and `total`.

#### Scenario: JSON output
- **WHEN** `tbxl rows list <id> --output json` is run
- **THEN** stdout contains exactly one JSON object with a `rows` array of flat string-valued objects and an integer `total`

#### Scenario: Truncation is detectable
- **WHEN** the sheet has more matching records than the effective limit
- **THEN** `total` exceeds the length of `rows`, signaling that more data exists
