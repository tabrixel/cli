# Introduction

**Tabrixel** (`tbxl`) is a command-line tool for working with Google Sheets as
record tables. It lets you inspect a document's structure, read rows as records,
and add, update, upsert, or delete rows by column-based conditions — without
opening a browser.

## The records model

`tbxl` treats a single sheet as a table of records:

- **Row 1 is the header.** Each cell in the first row is a column name.
- **Every row below the header is a record**, keyed by those column names.

So a sheet like this:

| Id | Name | IsActive |
| -- | ---- | -------- |
| 1  | Max  | 1        |
| 2  | Sam  | 0        |

is read as two records: `{Id: "1", Name: "Max", IsActive: "1"}` and
`{Id: "2", Name: "Sam", IsActive: "0"}`.

::: tip Everything is a string
Cells have no types. `IsActive=0` writes the text `"0"`, and rows come back as
`"0"` / `"1"`. There is no quoting of numbers and no booleans-as-bools — values
are exact strings in and exact strings out.
:::

## Core principle: learn the names first

Column names, sheet names, and the values you match on are **exact,
case-sensitive strings with no type coercion**. `Sheet1` is not `sheet1`, and
`Name` is not `name`.

Rather than guessing, learn the real names before you act:

```sh
tbxl describe   # every sheet, its columns, and record counts
tbxl columns    # column names of the target sheet
```

Then run `rows list` to read, or a mutation command to change data. A miss on a
column name returns a `column_not_found` error — often with a `did_you_mean`
suggestion.

## What's next

- [Installation](/guide/installation) — install the global tool.
- [Getting Started](/guide/getting-started) — set up a service account and verify access.
- [Configuration](/guide/configuration) — persist defaults so you don't repeat flags.
- [Commands](/commands/) — the full command reference.
