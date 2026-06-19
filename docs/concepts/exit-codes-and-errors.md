---
description: The three process exit codes (0, 1, 2) and the stable error code values tbxl reports on failure.
---

# Exit codes & errors

`tbxl` is designed to be scripted. Its process exit code tells you the outcome at
a glance, and failures carry a stable, machine-readable error `code`.

## Exit codes

| Code | Meaning |
| ---- | ------- |
| `0` | **Success.** The command completed. |
| `1` | **Failure.** Something went wrong; an error with a `code` was reported. |
| `2` | **Matched nothing.** The command ran correctly but affected zero rows. |

Exit code `2` applies to `rows update`, `rows delete`, and the update path of
`rows upsert` when `--where` matches no rows. It is **not** a failure — the
command worked, there was just nothing to act on. By contrast, `rows list` with
no results is ordinary success (`0`). See
[Matching & safety](/concepts/matching-and-safety#zero-matches-is-not-an-error).

```sh
tbxl rows update --where "Id=999" --set "IsActive=0"
echo $?   # 2 — nothing matched
```

## Error output

On failure, `tbxl` writes an error. With `--json` it goes to **stderr** as an
object with a snake_case `code`, a human `message`, and optional `details`:

```json
{ "code": "ambiguous_match", "message": "...", "details": null }
```

Keeping errors on stderr means stdout stays clean for parsing the success
payload. See [Output formats](/concepts/output-formats).

## Error codes

| `code` | When it happens |
| ------ | --------------- |
| `internal` | An unexpected error not otherwise classified. |
| `auth_failed` | The service account key is missing or invalid, or auth was rejected (HTTP 401). |
| `not_found` | The spreadsheet or sheet doesn't exist or isn't accessible (HTTP 403/404). |
| `sheet_ambiguous` | The document has multiple sheets and no sheet was resolved. |
| `header_invalid` | The header row (row 1) is missing or malformed. |
| `column_not_found` | A `--where`/`--set`/`--columns` or JSON column doesn't exist (may include a `did_you_mean`). |
| `ambiguous_match` | More than one row matched a mutation without `--all`/`--first`. |
| `confirmation_required` | `rows delete` was run without `--yes`. |
| `invalid_arguments` | Bad flags or JSON: e.g. a non-scalar/empty JSON record, or a required value resolved empty. |
| `io_error` | A local file/IO failure, such as writing the config file. |
