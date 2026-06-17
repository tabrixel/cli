# columns

List the column names of the target sheet, taken from its header row (row 1).
Use it to learn the exact, case-sensitive column names before filtering with
`--where`, projecting with `--columns`, or building a JSON record for
`rows add` / `rows upsert`.

```sh
tbxl columns [options]
```

## Options

The [global options](/commands/#global-options) apply. The relevant one:

| Option | Effect |
| ------ | ------ |
| `--sheet <NAME>` | The sheet whose columns to list. Resolves from `--sheet`, then `TBXL_SHEET`, then the `sheet` config key. Omittable if the document has exactly one sheet. |

## Examples

```sh
# Columns of the sheet resolved from env/config
tbxl columns --spreadsheet-id <ID>

# Columns of a specific sheet
tbxl columns --spreadsheet-id <ID> --sheet Users

# Machine-readable output
tbxl columns --spreadsheet-id <ID> --sheet Users --json
```

If the document has more than one sheet and no sheet is resolved, the command
fails with `sheet_ambiguous`. For an overview of every sheet at once, use
[`describe`](/commands/describe).
