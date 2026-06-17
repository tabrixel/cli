# describe

Print an overview of the whole document: every sheet, its columns, and how many
records each contains. This is the command to run first when you don't yet know
the sheet and column names — matching elsewhere is exact and case-sensitive, so
start from the real names.

```sh
tbxl describe [options]
```

## What it does

For each sheet in the document, reports its name, the columns from its header
row, and the number of records below the header.

## Options

The [global options](/commands/#global-options) apply. In addition:

| Option | Effect |
| ------ | ------ |
| `--sheet <NAME>` | Filter the overview to a single sheet. |

::: warning `--sheet` here is an explicit filter only
Unlike other commands, `describe` ignores the `TBXL_SHEET` environment variable
and the `sheet` config default. `--sheet` narrows the document overview only when
you pass it explicitly on the command line.
:::

## Examples

```sh
# Overview of the whole document
tbxl describe --spreadsheet-id <ID>

# Narrow to one sheet
tbxl describe --spreadsheet-id <ID> --sheet Users

# Machine-readable output
tbxl describe --spreadsheet-id <ID> --json
```

Once you know a sheet's name, use [`columns`](/commands/columns) for just its
column list, or [`rows list`](/commands/rows#list) to read its records.
