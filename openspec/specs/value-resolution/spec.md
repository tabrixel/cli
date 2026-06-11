# value-resolution

## Purpose

Defining the precedence chain (command line → environment variable → project config → global config) for resolving `spreadsheet-id`, `sheet`, and `credentials`, including empty-value semantics, sheet auto-selection, and source diagnostics in `auth check`.

## Requirements

### Requirement: Precedence chain per key
For each of `spreadsheet-id`, `sheet`, and `credentials`, the CLI SHALL resolve the effective value top-down, taking the first level where the value is present: command line (positional argument where supported, then flag) → environment variable → project config → global config. The environment variables SHALL be `TBXL_SPREADSHEET_ID` and `TBXL_SHEET`; credentials use `GOOGLE_APPLICATION_CREDENTIALS`.

#### Scenario: Flag beats env and config
- **WHEN** `--spreadsheet-id A` is passed while `TBXL_SPREADSHEET_ID=B` and the config sets `spreadsheet-id = "C"`
- **THEN** `A` is used

#### Scenario: Env beats config
- **WHEN** no `--sheet` flag is passed, `TBXL_SHEET=Env` is set, and the config sets `sheet = "Conf"`
- **THEN** `Env` is used

#### Scenario: Config as the weakest source
- **WHEN** neither flag nor env provides `spreadsheet-id` and the project config sets it
- **THEN** the config value is used

#### Scenario: Nothing set anywhere
- **WHEN** `spreadsheet-id` is absent at every level and a command requires it
- **THEN** the command fails with an `invalid_arguments` error explaining all the ways to set it (argument, flag, env variable, config)

### Requirement: Presence stops the chain, including empty values
At every level, presence SHALL be determined by the value being non-null; an explicitly provided empty value (after trim) SHALL count as present and stop the chain at that level. The implementation SHALL distinguish `null` (absent) from `string.Empty` (deliberately set empty) and SHALL NOT fall through on empty values.

#### Scenario: Empty flag overrides config
- **WHEN** `--spreadsheet-id ""` is passed and the config sets `spreadsheet-id = "C"`
- **THEN** the empty value is used and resolution does not fall through to the config; the command fails as if no usable spreadsheet ID were available, without silently substituting `C`

#### Scenario: Empty config value stops at config level
- **WHEN** the project config contains `sheet = ""` and the global config contains `sheet = "Fallback"`
- **THEN** resolution stops at the project level with the empty value and the global value is not used

### Requirement: Sheet resolution order with config and auto-select
For commands that operate on a single sheet, the sheet name SHALL resolve as: `--sheet` flag → `TBXL_SHEET` → project config → global config → auto-select when the document has exactly one sheet → `sheet_ambiguous` error. A config-provided sheet SHALL be used even when the document has multiple sheets; auto-select applies only when no level provides a value. The `describe` command is a whole-document overview: its optional sheet filter SHALL honor only the explicit `--sheet` flag, so env/config defaults never hide the rest of the document.

#### Scenario: Config sheet preempts auto-select
- **WHEN** the document has three sheets, no flag or env is set, and the config sets `sheet = "Data"`
- **THEN** the sheet `Data` is used without a `sheet_ambiguous` error

#### Scenario: Auto-select as final fallback
- **WHEN** no level provides a sheet name and the document has exactly one sheet
- **THEN** that sheet is selected automatically

#### Scenario: Ambiguity only when truly unset
- **WHEN** no level provides a sheet name and the document has multiple sheets
- **THEN** the command fails with a `sheet_ambiguous` error

#### Scenario: Describe ignores configured sheet defaults
- **WHEN** the config sets `sheet = "Data"` and the user runs `tbxl describe` without `--sheet`
- **THEN** the overview covers all sheets of the document, not only `Data`

### Requirement: Source diagnostics in auth check
The `auth check` command SHALL report, for each of the three keys, the resolved value (for credentials: the path, never key contents) and its source — flag, env, project config, global config, or none — and SHALL report the resolved project and global `config.toml` paths with whether each file was found. The diagnostics SHALL appear in both text and JSON output.

#### Scenario: Sources shown per value
- **WHEN** `auth check` runs with `--spreadsheet-id` passed as a flag, credentials coming from `GOOGLE_APPLICATION_CREDENTIALS`, and `sheet` coming from the project config
- **THEN** the output attributes each value to its source (flag / env / project config) and shows which `config.toml` paths were consulted

#### Scenario: Diagnostics in JSON
- **WHEN** `auth check --output json` runs
- **THEN** the JSON payload includes the per-key sources and config file paths in snake_case
