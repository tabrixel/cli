# Tabrixel

**Tabrixel** (`tbxl`) is a command-line tool for working with Google Sheets as record tables: inspect a document's structure, read rows as records, and add, update, upsert, or delete rows by column-based conditions. Output is human-readable text by default, or machine-parseable JSON (snake_case properties, data on stdout, errors on stderr) when the global `--json` flag is passed.

## Installation

```
dotnet tool install --global Tabrixel
```

## Available Commands

- **tbxl auth check** — Validate credentials and report the resolved configuration
- **tbxl config set \<key\> \<value\>** — Set a config key (`--global` for the global store)
- **tbxl config get \<key\>** — Show the effective value of a config key
- **tbxl config list** — List all config keys with values, scopes, and file paths
- **tbxl describe** — Overview of the document: sheets, columns, and record counts
- **tbxl columns** — List column names of the target sheet
- **tbxl rows list** — Read rows below the header as records
- **tbxl rows add** — Add one record from a JSON object argument (`tbxl rows add '{"Name":"John"}'`)
- **tbxl rows update** — Update fields of rows matched by `--where`
- **tbxl rows upsert** — Update matched rows, or insert a new record when nothing matches
- **tbxl rows delete** — Delete rows matched by `--where` (requires `--yes`)

Run `tbxl <command> --help` for the full list of options.

## Getting Started

Tabrixel authenticates with a **Google service account JSON key**. Create a service account, enable the Google Sheets API, download its JSON key, and share the target spreadsheet with the service account's email. Then point Tabrixel at the key via the `--credentials` flag, the `GOOGLE_APPLICATION_CREDENTIALS` environment variable, or the `credentials` config key, and verify your setup:

```
tbxl auth check --credentials path/to/key.json
```

## Support

If you encounter a bug or have a question, please open an issue on the [project repository](https://github.com/tabrixel/cli).
