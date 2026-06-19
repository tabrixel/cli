---
outline: deep
description: How rows are selected with --where and the mutation guardrails — --all/--first, --yes, --dry-run, and what row means in a receipt.
---

# Matching & safety

The `rows update`, `rows upsert`, and `rows delete` commands change live data, so
`tbxl` is deliberate about which rows they touch and refuses to guess. This page
explains how rows are selected and the guardrails around mutations.

## Selecting rows with `--where`

A condition is `Column=value`:

- **Exact and case-sensitive**, with no type coercion. `Status=Done` does not
  match `done`, and `Count=5` matches the text `"5"`.
- **Repeatable and AND-combined.** Passing `--where "A=1" --where "B=2"` matches
  rows where both hold.
- **Empty value matches empty cells.** `--where "Notes="` matches rows whose
  `Notes` cell is blank.
- **Split on the first `=`.** Values may themselves contain `=`.

A condition naming a column that doesn't exist fails with `column_not_found`,
often with a `did_you_mean` suggestion.

## Assigning values with `--set`

For `rows update`, `--set Column=value` assigns a new value:

- Repeatable; **at least one `--set` is required**.
- `Column=` (empty value) **clears** the cell.
- Unknown columns are rejected.

## Resolving multiple matches: `--all` / `--first`

If exactly one row matches, the mutation just applies. If **more than one** row
matches and you specify neither flag, the command **fails with
`ambiguous_match`** rather than changing rows you didn't intend.

| Flag | Behavior |
| ---- | -------- |
| `--first` | Apply to the first matching row only. |
| `--all` | Apply to every matching row. |

Passing both is an error. Choose deliberately — don't reach for `--all` just to
make the error go away, because it can rewrite many rows at once.

## Confirming deletes: `--yes`

`rows delete` does nothing without `--yes`; without it the command fails with
`confirmation_required`. You can still **preview** a delete without `--yes` by
using `--dry-run`.

## Previewing with `--dry-run`

Available on every mutation command (`add`, `update`, `upsert`, `delete`),
`--dry-run` validates the request and matches against the **live** sheet but
writes nothing. The output shows what *would* change and uses the **same exit
codes** as a real run, so it's a faithful preview. Prefer it before any
irreversible mutation.

## What `row` means

In every mutation receipt, `row` is the **1-based sheet row** — the header
occupies row 1, so the first data row is `row` 2. It is **not** the record's `Id`
and **not** a 0-based offset.

## Zero matches is not an error

For `update`, `delete`, and the update path of `upsert`, matching **zero** rows
is reported with exit code `2` (not `1`), and the warning/payload goes to stderr.
The command ran correctly; it simply had nothing to act on. (`rows list` with no
results is ordinary success, exit code `0`.) See
[Exit codes & errors](/concepts/exit-codes-and-errors).

## The recommended workflow

1. **Learn the names** with [`describe`](/commands/describe) /
   [`columns`](/commands/columns).
2. **Preview** with `--dry-run` and read the receipt.
3. **Commit** the change.
4. **Verify** with [`rows list`](/commands/rows#list).

The full worked example is on the [rows](/commands/rows#the-safe-mutation-workflow)
page.
