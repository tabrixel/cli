---
outline: deep
description: Read, add, update, upsert, and delete sheet rows as records, matched by exact column conditions.
---

# rows

Work with sheet rows as records. The `rows` branch is the heart of `tbxl`: read
records, append them, and change or remove them by column conditions.

All matching is **exact and case-sensitive with no type coercion**, and every
value is a string. Before reading the subcommands, it's worth understanding
[Matching & safety](/concepts/matching-and-safety) — `--where`, `--all`/`--first`,
`--yes`, `--dry-run`, and what `row` means in a receipt.

[[toc]]

## list

Read rows below the header as records.

```sh
tbxl rows list [options]
```

| Option | Description |
| ------ | ----------- |
| `--where <CONDITION>` | Filter by exact match `Column=value`. Repeatable; conditions are AND-combined. `Column=` matches empty cells. |
| `--columns <NAMES>` | Comma-separated column names to include, in that order. Other columns are omitted (but `--where` can still filter on them). |
| `--limit <N>` | Return at most N matching records. Default `100`. |
| `--offset <N>` | Skip the first N matching records before `--limit` is applied. Default `0`. |

```sh
tbxl rows list --limit 10
tbxl rows list --offset 10 --limit 10
tbxl rows list --where "Status=Done" --columns "Name,Status"
```

The JSON response carries a `total` field — the number of records that matched
**before** `--offset`/`--limit` were applied, which is what you page against.

::: tip Zero results is success
`rows list` returning no rows is a normal result (exit code `0`). Zero *matches*
is only treated specially for the mutation commands below (exit code `2`).
:::

## add

Append one record. Values are laid out by column name from a JSON object passed
as the single positional argument.

```sh
tbxl rows add '<json>' [options]
```

- Column names in the JSON are case-sensitive; **unknown fields are rejected**.
- Fields **missing** from the object are left empty in the new row.

```sh
tbxl rows add '{"Name":"John","Age":30}'
tbxl rows add '{"Id":"7","Name":"Sam","IsActive":"1"}' --sheet Users
```

The receipt reports `row` — the 1-based sheet row where the record was appended.

## update

Set new values on the columns of rows matched by `--where`.

```sh
tbxl rows update --where <CONDITION> --set <ASSIGNMENT> [options]
```

| Option | Description |
| ------ | ----------- |
| `--where <CONDITION>` | Select rows by exact match `Column=value`. Repeatable; AND-combined. `Column=` matches empty cells. |
| `--set <ASSIGNMENT>` | New value as `Column=value`. Repeatable; **at least one required**. `Column=` clears the cell. Unknown columns rejected. |
| `--all` | Update every matching row when more than one matches. |
| `--first` | Update only the first matching row when more than one matches. |
| `--dry-run` | Validate and match against the live sheet, but write nothing. |

```sh
tbxl rows update --where "Name=Max" --set "IsActive=0"
tbxl rows update --where "Status=New" --set "Status=Done" --all
```

If more than one row matches and you pass neither `--all` nor `--first`, the
command fails with `ambiguous_match` rather than guessing. Passing both `--all`
and `--first` is an error.

## upsert

Update the rows matched by `--where`, or insert a new record when nothing
matches. The JSON object supplies the fields to write.

```sh
tbxl rows upsert '<json>' --where <CONDITION> [options]
```

| Option | Description |
| ------ | ----------- |
| `--where <CONDITION>` | **Required.** Select rows by exact match `Column=value`. Repeatable; AND-combined. When nothing matches, the condition values become cells of the inserted row. |
| `--all` | Update every matching row when more than one matches. |
| `--first` | Update only the first matching row when more than one matches. |
| `--dry-run` | Validate and match against the live sheet, but write nothing. |

```sh
tbxl rows upsert '{"Status":"Done"}' --where "Email=a@b.c"
tbxl rows upsert '{"IsActive":"0"}' --where "Id=7" --sheet Users
```

- On **update**, only the columns in the JSON object are touched.
- On **insert**, the JSON fields plus the `--where` condition values form the new
  row. A column named in both must carry the same value.
- The receipt adds an `action` field: `"inserted"` or `"updated"`.

## delete

Delete rows matched by `--where`. Because this is destructive, it requires
explicit confirmation.

```sh
tbxl rows delete --where <CONDITION> --yes [options]
```

| Option | Description |
| ------ | ----------- |
| `--where <CONDITION>` | Select rows by exact match `Column=value`. Repeatable; AND-combined. `Column=` matches empty cells. |
| `--yes` | **Required.** Without it the command does nothing and fails with `confirmation_required`. |
| `--all` | Delete every matching row when more than one matches. |
| `--first` | Delete only the first matching row when more than one matches. |
| `--dry-run` | Validate and match against the live sheet, but write nothing. Previews without `--yes`. |

```sh
# Preview first — no --yes needed for a dry run
tbxl rows delete --where "Id=7" --dry-run

# Commit the deletion
tbxl rows delete --where "Id=7" --yes
```

## The safe mutation workflow

Treat mutations deliberately: learn the names, preview the change, commit it,
then verify against the live sheet.

```sh
# 1. Learn the real sheet/column names — never assume them.
tbxl describe --spreadsheet-id <ID>
tbxl columns --spreadsheet-id <ID> --sheet Users

# 2. Dry-run the change and read the receipt (row = 1-based sheet row).
tbxl rows update --spreadsheet-id <ID> --sheet Users \
  --where "Name=Max" --set "IsActive=0" --dry-run

# 3. Commit it (single match → no --all/--first needed).
tbxl rows update --spreadsheet-id <ID> --sheet Users \
  --where "Name=Max" --set "IsActive=0"

# 4. Verify against the live sheet.
tbxl rows list --spreadsheet-id <ID> --sheet Users --where "Name=Max"
```

See [Output formats](/concepts/output-formats) for the JSON receipt shapes and
[Exit codes & errors](/concepts/exit-codes-and-errors) for what each exit status
means.
