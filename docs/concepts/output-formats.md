---
outline: deep
---

# Output formats

Every command supports two output formats. Human-readable text is the default;
the global `--json` flag switches to machine-readable JSON.

## Text vs JSON

```sh
tbxl rows list --sheet Users          # human-readable table
tbxl rows list --sheet Users --json   # JSON for scripting
```

When `--json` is set:

- **Property names are snake_case** (e.g. `spreadsheet_id`, `service_account_email`).
- **Data goes to stdout; warnings and errors go to stderr.** This split keeps
  stdout cleanly parseable even when something goes wrong — see
  [Exit codes & errors](/concepts/exit-codes-and-errors).
- **Values are strings.** Cells are never coerced to numbers or booleans;
  `IsActive` comes back as `"0"` / `"1"`.

## JSON shapes by command

### rows list

`total` is the number of records that matched before `--offset`/`--limit`.

```json
{ "rows": [ { "Id": "4", "Name": "Max", "IsActive": "1" } ], "total": 4 }
```

### rows add

`row` is the 1-based sheet row where the record was appended.

```json
{ "affected": 1, "dry_run": false, "row": 7 }
```

### rows update

The `rows` array is capped at 50 entries; `truncated` is `true` when more rows
were affected than returned.

```json
{
  "matched": 1,
  "affected": 1,
  "dry_run": false,
  "rows": [ { "row": 3, "before": { "IsActive": "0" }, "after": { "IsActive": "1" } } ],
  "truncated": false,
  "returned": 1
}
```

### rows delete

Same shape as update, but each row carries the full deleted record under `data`
instead of `before`/`after`.

```json
{
  "matched": 1,
  "affected": 1,
  "dry_run": false,
  "rows": [ { "row": 3, "data": { "Id": "7", "Name": "Sam" } } ],
  "truncated": false,
  "returned": 1
}
```

### rows upsert

Like `rows update` plus an `action` of `"updated"` or `"inserted"`.

```json
// insert
{ "matched": 0, "affected": 1, "action": "inserted", "dry_run": false, "row": 7 }
```

### auth check

```json
{
  "service_account_email": "my-bot@my-project.iam.gserviceaccount.com",
  "spreadsheet_id": "1AbC...xyz",
  "status": "OK",
  "sources": { "credentials": "flag", "spreadsheet_id": "config", "sheet": "env" }
}
```

### Errors

Errors are emitted on **stderr** with a snake_case `code`:

```json
{ "code": "invalid_arguments", "message": "...", "details": null }
```

The full list of `code` values is on
[Exit codes & errors](/concepts/exit-codes-and-errors).

::: tip `row` is the sheet row, not the Id
In every receipt, `row` is the **1-based sheet row** — the header is row 1, so
the first data row is `row` 2. It is not the record's `Id` and not a 0-based
offset.
:::
