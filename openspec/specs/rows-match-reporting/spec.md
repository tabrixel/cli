# rows-match-reporting

## Purpose

The JSON output contract of `rows update` and `rows delete`: a uniform payload reporting `matched`, `affected`, and `dry_run` for both the matched and 0-match outcomes, so scripts always get a machine-parseable result.

## Requirements

### Requirement: Uniform mutation payload for rows update and rows delete
With `--json`, `rows update` and `rows delete` SHALL report their outcome as a single payload object with the fields `matched`, `affected`, `dry_run`, `rows`, `truncated`, and `returned`, for both the matched and the 0-match outcomes. `matched` MUST be the number of rows matching the `--where` conditions; `affected` MUST be the number of rows actually selected for the write (`0` when nothing matched; it MAY be less than `matched` under `--first`); `dry_run` MUST reflect the `--dry-run` flag; `rows`, `truncated`, and `returned` MUST follow `rows-mutation-receipts` and the per-command receipt shape. When at least one row matched, the payload is data and MUST be written to stdout.

#### Scenario: successful update reports matched and affected
- **WHEN** `tbxl rows update --spreadsheet-id <id> --where 'Status=Open' --set 'Status=Done' --first --json` matches 3 rows
- **THEN** stdout contains `"matched": 3`, `"affected": 1`, `"dry_run": false`, and a `rows` array with exactly 1 receipt element

#### Scenario: successful delete reports matched and affected
- **WHEN** `tbxl rows delete --spreadsheet-id <id> --where 'Status=Done' --all --yes --json` matches 2 rows
- **THEN** stdout contains `"matched": 2`, `"affected": 2`, `"dry_run": false`, and a `rows` array with 2 receipt elements

### Requirement: 0-match outcome is machine-parseable in JSON mode
When `rows update` or `rows delete` matches 0 rows and `--json` is set, the command SHALL write the payload `{"matched": 0, "affected": 0, "dry_run": <bool>, "rows": [], "truncated": false, "returned": 0}` to stderr — the warning channel, consistent with JSON errors — and MUST write nothing to stdout. The exit code MUST remain `NoMatch` (2) in both output formats.

#### Scenario: no-match update emits JSON payload on stderr
- **WHEN** `tbxl rows update --spreadsheet-id <id> --where 'Status=Nope' --set 'Status=Done' --all --json` matches no rows
- **THEN** stderr contains `"matched": 0`, `"affected": 0`, `"dry_run": false`, and `"rows": []`, stdout is empty, and the command exits with code 2

#### Scenario: no-match delete in dry run emits JSON payload on stderr
- **WHEN** `tbxl rows delete --spreadsheet-id <id> --where 'Status=Nope' --all --dry-run --json` matches no rows
- **THEN** stderr contains `"matched": 0`, `"affected": 0`, `"dry_run": true`, and `"rows": []`, stdout is empty, and the command exits with code 2

### Requirement: 0-match warning stays on stderr in text mode
When `rows update` or `rows delete` matches 0 rows and the output format is text, the command SHALL write the existing human-readable warning to stderr and nothing to stdout.

#### Scenario: text-mode no-match warning unchanged
- **WHEN** `tbxl rows delete --spreadsheet-id <id> --where 'Status=Nope' --all` matches no rows with text output
- **THEN** stderr contains a warning that 0 rows matched, stdout is empty, and the command exits with code 2
