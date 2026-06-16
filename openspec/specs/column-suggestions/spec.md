# column-suggestions

## Purpose

The `did_you_mean` suggestion algorithm shared by all column-name lookups (`--where`, `--columns`, the JSON record, `--set`): what counts as a "close name" and how a suggestion is chosen.

## Requirements

### Requirement: Column lookup with did_you_mean suggestion
Every column-name lookup (`--where`, `--columns`, the JSON record, `--set`) SHALL resolve names by exact, case-sensitive match. On a miss the command SHALL fail with `ColumnNotFound`; when a close name exists, the error message SHALL include a `did you mean '<name>'?` hint and the JSON error details SHALL carry it as `did_you_mean`. When no close name exists, the error SHALL contain no suggestion.

#### Scenario: Unknown column with a close name
- **WHEN** a lookup names `Stats` and the header contains `Status`
- **THEN** the command fails with `ColumnNotFound` and `did_you_mean` is `Status`

#### Scenario: Unknown column with no close name
- **WHEN** a lookup names `Frobnicate` and the header contains `Name` and `Status`
- **THEN** the command fails with `ColumnNotFound` and no `did_you_mean` is present

### Requirement: Case-insensitive match is suggested first
When the looked-up name matches a header column ignoring letter case, that column SHALL be suggested regardless of edit distance (the leftmost such column when several match).

#### Scenario: Case-only mismatch
- **WHEN** a lookup names `name` and the header contains `Name`
- **THEN** the command fails with `ColumnNotFound` and `did_you_mean` is `Name`

### Requirement: Closeness is OSA edit distance within a length-scaled threshold
A header column SHALL count as close when its Optimal String Alignment distance to the looked-up name — single-character insertions, deletions, substitutions, and adjacent-character transpositions each costing 1 — is at most `max(1, len/3)` where `len` is the length of the looked-up name. Among several close names the one with the smallest distance SHALL be suggested; among equally distant names, the leftmost in header order.

#### Scenario: Adjacent transposition is one edit
- **WHEN** a lookup names `Nmae` and the header contains `Name`
- **THEN** the command fails with `ColumnNotFound` and `did_you_mean` is `Name`

#### Scenario: Single-character typo
- **WHEN** a lookup names `Nam` and the header contains `Name`
- **THEN** the command fails with `ColumnNotFound` and `did_you_mean` is `Name`

#### Scenario: Distance beyond the threshold
- **WHEN** a lookup names `Nmea` (OSA distance 2 from `Name`) and the header contains only `Name`
- **THEN** the command fails with `ColumnNotFound` and no `did_you_mean` is present

#### Scenario: Tie broken by header order
- **WHEN** a lookup names `Nme` and the header contains `Name` followed by `Nye`, both at OSA distance 1
- **THEN** the command fails with `ColumnNotFound` and `did_you_mean` is `Name`
