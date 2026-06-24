# console-encoding

## Purpose

Defines how the CLI encodes console output so that non-ASCII characters are preserved verbatim and machine-readable output stays clean, regardless of the host console's default code page.

## Requirements

### Requirement: Console output is encoded as UTF-8

The CLI SHALL write all output as UTF-8, independent of the host console's default code page, so that non-ASCII characters (e.g. Cyrillic) are preserved verbatim. This SHALL apply to every output path — text data, JSON data, warnings, and errors — on both stdout and stderr. The encoding SHALL be configured once at process startup, before any output is produced.

#### Scenario: Non-ASCII value survives in text output

- **WHEN** a command renders a record containing the value `Профиль` without `--json`
- **THEN** the output contains `Профиль` and no `?` substitution characters

#### Scenario: Non-ASCII value survives in JSON output

- **WHEN** a command renders a record containing the value `Профиль` with `--json`
- **THEN** the JSON on stdout contains `"ru": "Профиль"` and no `?` substitution characters

#### Scenario: Non-ASCII error and warning text survives on stderr

- **WHEN** a command emits an error or warning message containing non-ASCII characters
- **THEN** those characters are preserved verbatim on stderr in both text and `--json` output

### Requirement: UTF-8 output carries no byte-order mark

The UTF-8 output written by the CLI SHALL NOT begin with a byte-order mark (BOM), so that `--json` output remains directly parseable by standard JSON consumers and piped output is not prefixed with stray bytes.

#### Scenario: JSON output is not prefixed with a BOM

- **WHEN** a command produces `--json` output and the bytes are captured (e.g. piped to a file or another process)
- **THEN** the first bytes are the JSON content itself, with no leading `EF BB BF` BOM sequence
