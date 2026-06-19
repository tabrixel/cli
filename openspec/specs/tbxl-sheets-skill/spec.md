# Spec: Tbxl Google Sheets Skill

## Purpose

Requirements on the content of the `tbxl-google-sheets` agent skill (`.claude/skills/tbxl-google-sheets/SKILL.md`) — the guidance that lets an agent drive the `tbxl` CLI without trial and error. It governs that the skill teaches the service-account access model, makes access errors (`not_found`/`auth_failed`) actionable, and accurately describes column projection, JSON scalar coercion, the config-layer-only reporting of `config get`/`list`, and `header_invalid`, with a pointer to the full documentation. Every fact the skill asserts about behavior reflects the current implementation and the published `docs/`.

## Requirements

### Requirement: Skill documents the service-account access model and setup
The `tbxl-google-sheets` skill (`.claude/skills/tbxl-google-sheets/SKILL.md`) SHALL explain that the `tbxl` CLI can be verified with `tbxl --version` (assuming it is already installed) and that a Google service account can only access spreadsheets explicitly shared with its email address. It SHALL tell the agent how to discover that email (`tbxl auth check --json` → `service_account_email`) and that the target document must be shared with it (Editor to mutate, Viewer to read). The skill SHALL NOT include installation instructions.

#### Scenario: Agent reads the setup guidance before first use
- **WHEN** an agent reads the skill to set up access to a new spreadsheet
- **THEN** the skill states that the service account only sees documents shared with its email
- **AND** it shows that the email comes from `auth check` (the `service_account_email` field)
- **AND** it notes the CLI is verifiable with `tbxl --version` and does not include installation instructions

### Requirement: Skill connects access errors to recovery
The skill SHALL document that `not_found` can mean the document is not shared with the service account (in addition to a wrong/inaccessible spreadsheet ID or sheet name), and that `auth_failed` means the key is missing, invalid, or unauthorized. This guidance SHALL appear in the skill's error/common-mistakes guidance, not only in the bare error-code list.

#### Scenario: Agent hits not_found with a correct spreadsheet ID
- **WHEN** an agent encounters a `not_found` error and consults the skill
- **THEN** the skill directs it to verify the document is shared with the service account email (from `auth check`) as a distinct cause from a wrong ID or sheet name

#### Scenario: Agent hits auth_failed
- **WHEN** an agent encounters an `auth_failed` error and consults the skill
- **THEN** the skill states the cause is a missing, invalid, or unauthorized service account key

### Requirement: Skill accurately describes column projection
The skill SHALL state that `rows list --columns` selects which columns appear in the output, while `--where` can still filter on columns that are omitted from `--columns`.

#### Scenario: Agent wants to filter on a column it does not need in output
- **WHEN** an agent reads how `--columns` and `--where` interact
- **THEN** the skill makes clear that a column can be used in `--where` without being listed in `--columns`

### Requirement: Skill accurately describes JSON scalar coercion
The skill SHALL state that values in the `rows add`/`rows upsert` JSON record may be JSON scalars (strings, numbers, booleans), that non-string scalars are coerced to text (e.g. `{"Age":30}` is written as `"30"`), and that only non-scalar values (objects or arrays) or an empty object are rejected with `invalid_arguments`.

#### Scenario: Agent builds a JSON record with a numeric field
- **WHEN** an agent reads the JSON-record rules
- **THEN** the skill shows that a bare number or boolean is accepted and stored as text, and that objects/arrays/empty objects are rejected

### Requirement: Skill states config get/list report only the config layer
The skill SHALL state that `tbxl config get` and `tbxl config list` report only the stored config layer, and that `tbxl auth check` is the command that reports the full resolution across flag, environment, and config along with each value's source.

#### Scenario: Agent debugs which value tbxl will use
- **WHEN** an agent needs to know the effective value of a setting and its source across all layers
- **THEN** the skill points it to `auth check` rather than `config get`/`config list`

### Requirement: Skill explains the header_invalid error
The skill SHALL state that the `header_invalid` error means the sheet's header row (row 1) is missing or malformed.

#### Scenario: Agent encounters header_invalid
- **WHEN** an agent sees a `header_invalid` error and consults the skill
- **THEN** the skill explains it as a missing or malformed header row (row 1)

### Requirement: Skill points to the full documentation
The skill SHALL include a brief pointer to the canonical Tabrixel documentation source so an agent can drill into detail beyond the skill and `--help`.

#### Scenario: Agent needs more detail than the skill provides
- **WHEN** an agent has exhausted the skill and `tbxl <command> --help`
- **THEN** the skill names where the full documentation lives (the published LLM-friendly index at <https://tabrixel.com/llms.txt>)
