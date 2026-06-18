# Tabrixel

**Tabrixel** (`tbxl`) is a command-line tool that treats a **Google Sheet as a table of records**: inspect a document's structure, read rows as records, and add, update, upsert, or delete rows by column-based conditions. It ships as a cross-platform **.NET 10 global tool** and speaks both human-readable text (default) and machine-parseable JSON (`--json`).

## 📦 Installation

```
dotnet tool install --global Tabrixel
```

## 🚀 Quick Start

Point `tbxl` at a spreadsheet and a service-account key (via flags, environment variables, or saved config — see the Authentication and Configuration sections below), then:

```sh
# Verify credentials and show the resolved configuration
tbxl auth check

# See the document's sheets, columns, and record counts
tbxl describe

# List the target sheet's column names
tbxl columns

# Read records, filtered and projected
tbxl rows list --where "Status=Done" --columns "Name,Status" --limit 10

# Add a record from a JSON object
tbxl rows add '{"Name":"John","Age":30}'

# Update matched rows
tbxl rows update --where "Email=a@b.c" --set "Status=Done"

# Update a matched row, or insert it if nothing matches
tbxl rows upsert --where "Email=a@b.c" '{"Status":"Done"}'

# Delete matched rows (confirmation required)
tbxl rows delete --where "Status=Obsolete" --yes
```

`--where` takes repeatable, case-sensitive `Column=value` conditions combined with AND. Add `--json` to any command for machine-parseable output.

## 📋 Commands

| Command | Description |
| --- | --- |
| `tbxl auth check` | Validate credentials and report the resolved configuration |
| `tbxl config set <key> <value>` | Set a config key in the project store (`--global` for `~/.tabrixel`) |
| `tbxl config get <key>` | Show the effective value of a config key |
| `tbxl config list` | List all config keys with values, scopes, and file paths |
| `tbxl describe` | Overview of the document: sheets, columns, and record counts |
| `tbxl columns` | List the target sheet's column names (from its first row) |
| `tbxl rows list` | Read rows below the header as records (`--where`, `--columns`, `--limit`, `--offset`) |
| `tbxl rows add <json>` | Add one record laid out by column names from a JSON object |
| `tbxl rows update` | Update fields (`--set`) of rows matched by `--where` |
| `tbxl rows upsert` | Update matched rows, or insert a new record when nothing matches |
| `tbxl rows delete` | Delete rows matched by `--where` (requires `--yes`) |

Run `tbxl <command> --help` for the full list of options.

## 🔑 Authentication

Tabrixel authenticates with a **Google service account JSON key**. Create a service account, enable the Google Sheets API, download its JSON key, and share the target spreadsheet with the service account's email.

Supply the key in any of these ways (first present wins):

1. `--credentials <path>` flag
2. `GOOGLE_APPLICATION_CREDENTIALS` environment variable
3. `credentials` config key

Then verify your setup:

```
tbxl auth check --credentials path/to/key.json
```

## ⚙️ Configuration

Save defaults so you don't repeat them on every command. Config lives in TOML files:

- **Project** — `.tabrixel/config.toml`, discovered by walking up from the current directory (like `.git`)
- **Global** — `~/.tabrixel/config.toml`

```
tbxl config set spreadsheet-id 1A2B3C...   # project store
tbxl config set sheet Sheet1 --global      # global store
tbxl config list                           # values, scopes, and file paths
```

Keys: `spreadsheet-id`, `sheet`, `credentials`. Each value resolves through this chain, first present wins:

**CLI flag → environment → project config → global config**

Environment variables: `TBXL_SPREADSHEET_ID`, `TBXL_SHEET`, and `GOOGLE_APPLICATION_CREDENTIALS`. An explicitly empty value (`--flag ""` or `key = ""`) is treated as present and stops the chain.

## 🧾 Output

Commands print human-readable text by default. Pass the global `--json` flag for machine-parseable JSON: properties use snake_case, data goes to **stdout**, and errors go to **stderr**, so output stays parseable in scripts. The process exit code is non-zero on failure.

## 📖 Documentation & Support

- **Full documentation:** [tabrixel.com](https://tabrixel.com)
- **Source & issues:** [github.com/tabrixel/cli](https://github.com/tabrixel/cli)

## 📄 License

Apache-2.0
