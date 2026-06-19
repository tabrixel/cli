---
name: tbxl-google-sheets
description: Use when reading from or writing to Google Sheets from the command line, or when a task mentions the tbxl CLI, a Google spreadsheet ID, setting up Google service-account access to a sheet (sharing a document with the service account), treating sheet rows as records (list/add/update/upsert/delete), or the GOOGLE_APPLICATION_CREDENTIALS, TBXL_SPREADSHEET_ID, or TBXL_SHEET environment variables.
---

# Driving the tbxl Google Sheets CLI

## Overview

`tbxl` is a globally-installed CLI that treats a Google Sheet as a table of records: row 1 is the header, every row below is a record keyed by column name. Use it to inspect a document and to read/add/update/upsert/delete rows by column conditions.

**Core principle:** the tool already knows its surface — don't re-derive it by trial and error. Learn the exact sheet/column names from `describe`/`columns` first, then act. Column names, sheet names, and `--where`/`--set` values are **exact, case-sensitive strings with no type coercion**.

## Setup & access (the #1 cause of failures)

`tbxl` is a global CLI — confirm it's available with `tbxl --version`. It authenticates as a **Google service account**, which can only see spreadsheets that have been **explicitly shared with its email** — sharing happens in the Google Sheets *Share* dialog, not through `tbxl`.

- Get the service account's address from `tbxl auth check --json` → `service_account_email`.
- In Google Sheets, **Share** the target document with that email: **Viewer** to read, **Editor** to add/update/delete.
- A correct spreadsheet id that simply hasn't been shared still fails with `not_found` (403/404), not a clearer "access denied" — see the error notes below.

## Critical conventions (read before running anything)

- **`--json` switches every command to machine output:** **data → stdout, warnings/errors → stderr**, snake_case keys. Default (no flag) is human tables.
- **Everything is a string.** `IsActive=0` writes the text `"0"`; rows come back as `"0"`/`"1"`. No quoting/escaping of numbers, no booleans-as-bools.
- **Exact, case-sensitive matching.** A miss on a column name → `column_not_found` (with a `did_you_mean` suggestion). A miss on a sheet name → `not_found`. `Sheet1` ≠ `sheet1`.
- **`row` in mutation receipts is the 1-based sheet row** (header is row 1), NOT the record's Id and NOT a 0-based offset. First data row is `row` 2.
- **Three inputs, each with a precedence chain** (first present wins; an explicit empty value stops the chain). The ALL-CAPS names below — `GOOGLE_APPLICATION_CREDENTIALS`, `TBXL_SPREADSHEET_ID`, `TBXL_SHEET` — are environment variables:
  - credentials: `--credentials <path>` → `GOOGLE_APPLICATION_CREDENTIALS` → config `credentials`
  - spreadsheet: `--spreadsheet-id` → `TBXL_SPREADSHEET_ID` → config `spreadsheet-id`
  - sheet: `--sheet <name>` → `TBXL_SHEET` → config `sheet` (omittable only if the doc has exactly one sheet; otherwise `sheet_ambiguous`)
- **Config is discovered by walking UP from the current directory** (like `.git`): project `.tabrixel/config.toml`, then global `~/.tabrixel/config.toml`. A config in a *subdirectory* of cwd is NOT found. For repeated work, persist with `tbxl config set <key> <value>` (`--global` for the global store) or pass flags every call.
- No command takes the spreadsheet id positionally — always give it via `--spreadsheet-id`/`TBXL_SPREADSHEET_ID`/config. The sole positional argument is the **JSON record** of `rows add`/`rows upsert` (e.g. `rows add '{…}'`).

## Command quick reference

| Command | Purpose | Key options |
|---|---|---|
| `tbxl auth check` | Validate the key; report resolved config + sources | `--spreadsheet-id` also checks access |
| `tbxl describe` | All sheets, their columns, record counts | `--sheet` filters to one (ignores env/config sheet default) |
| `tbxl columns` | Column names of the target sheet | `--sheet` |
| `tbxl rows list` | Read records below the header | `--where`, `--columns`, `--limit` (def 100), `--offset` (def 0) |
| `tbxl rows add '<json>'` | Append one record | unknown fields rejected; missing fields left empty |
| `tbxl rows update` | Set columns on matched rows | `--where` (repeatable, AND), `--set` (≥1, repeatable), `--all`/`--first` |
| `tbxl rows upsert '<json>'` | Update matches, else insert | `--where` required, `--all`/`--first` |
| `tbxl rows delete` | Delete matched rows | `--where`, **`--yes` required**, `--all`/`--first` |
| `tbxl config set/get/list` | Manage stored defaults | `set --global` for global store |

- `rows list --columns` only chooses which columns appear in the **output**; you can still `--where`-filter on a column you left out of `--columns`.
- `config get`/`config list` report only the **config layer**. To see a value's full resolution across flag → env → config (and which level won), use `auth check`.

Run `tbxl <command> --help` for the full, authoritative flag list — don't guess flags.

## Matching, safety, and exit codes (mutations)

- `--where 'Column=value'` is repeatable and AND-combined. `'Column='` (empty value) matches **empty cells**. Split is on the first `=`, so values may contain `=`.
- **If more than one row matches, update/upsert/delete fail with `ambiguous_match`** unless you pass `--all` (every match) or `--first` (just the first). `--all` and `--first` together is an error. Choose deliberately — don't add `--all` reflexively.
- **`delete` does nothing without `--yes`** (fails `confirmation_required`). `--dry-run` previews without `--yes`.
- **`--dry-run`** validates and matches against the live sheet but writes nothing; output shows what *would* change and uses the same exit codes. Prefer it before any irreversible mutation.
- **0 rows matched is not an error**: update/delete/upsert-update return **exit code 2** (`NoMatch`), the warning/payload going to **stderr**. (`rows list` with 0 results is normal exit 0.)
- Exit codes overall: **0** success · **1** failure (any `code` error) · **2** matched nothing.

## JSON shapes you'll parse

```jsonc
// rows list
{"rows":[{"Id":"4","Name":"Max","IsActive":"1"}],"total":4}   // total = matches before --offset/--limit

// rows update / delete  (rows[] capped at 50; truncated=true if more)
{"matched":1,"affected":1,"dry_run":false,
 "rows":[{"row":3,"before":{"IsActive":"0"},"after":{"IsActive":"1"}}],
 "truncated":false,"returned":1}                              // delete uses "data" (full record) instead of before/after

// rows add
{"affected":1,"dry_run":false,"row":7}                        // row = where it was appended

// rows upsert — adds "action":"inserted"|"updated".
//   insert: {"matched":0,"affected":1,"action":"inserted","dry_run":false,"row":7}
//   update: like rows update + "action":"updated"

// auth check
{"service_account_email":"...","spreadsheet_id":"...","status":"OK","sources":{...}}

// error (stderr), code is a snake_case string
{"code":"invalid_arguments","message":"...","details":null}
```

Error `code` values: `internal`, `auth_failed`, `not_found`, `sheet_ambiguous`, `header_invalid`, `column_not_found`, `ambiguous_match`, `confirmation_required`, `invalid_arguments`, `io_error`. Notable causes: `auth_failed` = key missing, invalid, or unauthorized; `not_found` = wrong/inaccessible spreadsheet id or sheet name, **or the document isn't shared with the service account**; `header_invalid` = the header row (row 1) is missing or malformed.

## Worked example (the safe mutation workflow)

```sh
# Replace <ID> with the spreadsheet id and <KEY> with the path to the service-account JSON.
# 1. Learn the real names — never assume them.
tbxl describe --spreadsheet-id <ID> --credentials <KEY> --json
tbxl columns --spreadsheet-id <ID> --sheet Users --credentials <KEY> --json
# 2. Dry-run the change and read the receipt (row = 1-based sheet row).
tbxl rows update --spreadsheet-id <ID> --sheet Users --where "Name=Max" --set "IsActive=0" --dry-run --credentials <KEY> --json
# 3. Commit it (single match → no --all/--first needed).
tbxl rows update --spreadsheet-id <ID> --sheet Users --where "Name=Max" --set "IsActive=0" --credentials <KEY> --json
# 4. Verify against the live sheet.
tbxl rows list --spreadsheet-id <ID> --sheet Users --where "Name=Max" --credentials <KEY> --json
```

`rows add`/`upsert` examples:
```sh
tbxl rows add '{"Id":"7","Name":"Sam","IsActive":"1"}' --spreadsheet-id <ID> --sheet Users --credentials <KEY> --json
tbxl rows upsert '{"IsActive":"0"}' --where "Id=7" --spreadsheet-id <ID> --sheet Users --credentials <KEY> --json
```
**Shell note (win/linux/osx):** forward-slash paths work everywhere — .NET accepts them on Windows too. Single-quoted JSON works in bash/zsh and PowerShell; Windows `cmd.exe` needs different quoting, so use PowerShell there. To skip repeating `--credentials`/`--spreadsheet-id` on every call, persist them once with `tbxl config set`.

JSON record rules: a flat object of `column → scalar`. Scalars are coerced to text — a number becomes its plain string (`{"Age":30}` → `"30"`, `1e3` → `"1000"`), `true`/`false` become `"true"`/`"false"`, and `null` clears the cell. **Objects/arrays, an empty `{}`, or a duplicate field** → `invalid_arguments`; unknown column → `column_not_found`. `upsert` requires `--where`, and a column named in both `--where` and the record must carry the same value.

## Common mistakes

| Mistake | Reality |
|---|---|
| Running `tbxl` from the repo root and finding "config unset" | Config is found walking *up* from cwd; a `.tabrixel/` in a subdir is invisible. `cd` into the dir, or pass flags/env. |
| Reading `row` as the Id or a 0-based offset | `row` is the 1-based sheet row (header = row 1). |
| Adding `--all` to make `ambiguous_match` go away | Decide: `--first` (one row) vs `--all` (every match). `--all` can rewrite many rows. |
| Treating exit code 2 as failure | Exit 2 = the command ran fine but matched 0 rows. |
| Guessing column/sheet names or case | Run `describe`/`columns` first; matching is exact and case-sensitive. |
| `delete` "did nothing" | It needs `--yes`; without it, it refuses (`confirmation_required`). |
| Expecting numbers/booleans back | All cells are strings (`"0"`, `"true"`). |
| Expecting auth to "stick" after `auth check` | No session state. Pass `--credentials` + the spreadsheet id on **every** call, or persist them via `config set`/env. |
| `not_found` even though the spreadsheet id is correct | The document isn't shared with the service account. Get the email from `auth check` and share it in Google Sheets — a distinct cause from a wrong id or sheet name. |

## Don't re-derive the surface

A capable agent without this skill spends ~9 calls probing `--help` and still misreads `row`. You have the map above: check `describe`/`columns` for names, `--help` only for an exhaustive flag list, then run the real command.

## Further reading

The full reference — every command, flag, concept, and JSON receipt shape — is published as an LLM-friendly index at <https://tabrixel.com/llms.txt>. Reach for it only when this skill and `tbxl <command> --help` don't settle a detail.
