# Commands

`tbxl` is organized into a few top-level commands and two branches (`config` and
`rows`). Run `tbxl <command> --help` for the authoritative, exhaustive flag list.

## Overview

| Command | Purpose |
| ------- | ------- |
| [`tbxl auth check`](/commands/auth-check) | Validate the key and report the resolved configuration + sources |
| [`tbxl config set`](/commands/config) | Set a config key (project, or `--global`) |
| [`tbxl config get`](/commands/config) | Show the effective value of a config key |
| [`tbxl config list`](/commands/config) | List all keys with values, scopes, and file paths |
| [`tbxl describe`](/commands/describe) | Overview of the document: sheets, columns, and record counts |
| [`tbxl columns`](/commands/columns) | List column names of the target sheet |
| [`tbxl rows list`](/commands/rows#list) | Read rows below the header as records |
| [`tbxl rows add`](/commands/rows#add) | Add one record from a JSON object |
| [`tbxl rows update`](/commands/rows#update) | Update fields of rows matched by `--where` |
| [`tbxl rows upsert`](/commands/rows#upsert) | Update matched rows, or insert when nothing matches |
| [`tbxl rows delete`](/commands/rows#delete) | Delete rows matched by `--where` (requires `--yes`) |

## Global options

These options are available on every command:

| Option | Description |
| ------ | ----------- |
| `--json` | Output machine-readable JSON instead of human text. See [Output formats](/concepts/output-formats). |
| `--credentials <PATH>` | Path to the service account JSON key. Falls back to `GOOGLE_APPLICATION_CREDENTIALS`, then the `credentials` config key. |
| `--spreadsheet-id <ID>` | The Google Spreadsheet document ID. Falls back to `TBXL_SPREADSHEET_ID`, then the `spreadsheet-id` config key. |
| `--sheet <NAME>` | The sheet (tab) name. Falls back to `TBXL_SHEET`, then the `sheet` config key. Omittable if the document has exactly one sheet. |

See [Configuration](/guide/configuration) for the full precedence chain.

::: tip The spreadsheet ID is never positional
No command takes the spreadsheet ID as a positional argument — always supply it
via `--spreadsheet-id`, `TBXL_SPREADSHEET_ID`, or config. The only positional
argument anywhere is the **JSON record** of `rows add` and `rows upsert`.
:::

## Conventions shared by every command

- **Exact, case-sensitive matching** of sheet names, column names, and values —
  no type coercion. `Sheet1` ≠ `sheet1`.
- **Everything is a string.** Values are written and returned verbatim as text.
- **Two output formats.** Human tables by default; `--json` switches to JSON
  with data on stdout and errors on stderr.
- **Exit codes:** `0` success · `1` failure · `2` ran fine but matched zero rows.
  See [Exit codes & errors](/concepts/exit-codes-and-errors).
