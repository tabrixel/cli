# rows-mutation-receipts

## Purpose

The shared receipt contract of mutating row commands: mutations return the affected records themselves (not just counters), so an agent can verify what changed without a follow-up `rows list`. Covers row numbering, payload truncation, and column-name fidelity.

## Requirements

### Requirement: Receipt row numbers are 1-based sheet rows
Wherever a mutation payload reports a row number (`row` fields of receipts), the number SHALL be the 1-based sheet row as shown in the Google Sheets UI: the header is row 1, the first record is row 2. Consumers MUST be able to use the number directly in A1 ranges and browser links without translation.

#### Scenario: First record reports row 2
- **WHEN** a mutation affects the first record below the header
- **THEN** its receipt reports `"row": 2`

### Requirement: Receipts are truncated past 50 rows
When a mutation affects more than 50 rows, the `rows` receipt array SHALL contain only the first 50 affected rows in selection order. Payloads carrying a `rows` array MUST always include `truncated` (boolean, `true` only when rows were cut) and `returned` (the number of elements actually present in `rows`), in both truncated and non-truncated outcomes, so the payload schema is stable. `matched` and `affected` SHALL keep counting all rows regardless of truncation.

#### Scenario: Mass mutation is truncated
- **WHEN** `rows update … --all --json` affects 320 rows
- **THEN** stdout contains `"affected": 320`, a `rows` array of 50 elements, `"truncated": true`, and `"returned": 50`

#### Scenario: Small mutation keeps the stable schema
- **WHEN** a mutation affects 2 rows with `--json`
- **THEN** the payload contains a `rows` array of 2 elements, `"truncated": false`, and `"returned": 2`

### Requirement: Column names in receipts are verbatim
The keys of receipt objects (`before`, `after`, `data`) SHALL be the sheet's column names exactly as they appear in the header — never case-converted or otherwise transformed by the JSON serializer — and SHALL be ordered by header column order. Cell values follow `rows list` conventions: strings, with empty/missing cells as `""`.

#### Scenario: Mixed-case column survives serialization
- **WHEN** a sheet has a column named `UserEmail` and a mutation receipt includes it
- **THEN** the JSON key is exactly `UserEmail`, not `user_email`
