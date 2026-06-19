---
description: Resolve credentials, spreadsheet ID, and sheet from flags, environment variables, and TOML config, and the precedence chain between them.
---

# Configuration

Three inputs drive almost every command — the **credentials path**, the
**spreadsheet ID**, and the **sheet name**. Rather than passing them as flags
every time, you can resolve them from environment variables or from TOML config
files.

## The value precedence chain

For each of the three inputs, sources are tried top-down and the **first present
(non-null) value wins**:

1. **CLI flag** — `--credentials`, `--spreadsheet-id`, `--sheet`
2. **Environment variable** — `GOOGLE_APPLICATION_CREDENTIALS`, `TBXL_SPREADSHEET_ID`, `TBXL_SHEET`
3. **Project config** — `.tabrixel/config.toml`
4. **Global config** — `~/.tabrixel/config.toml`

| Input          | Flag                | Environment variable             | Config key       |
| -------------- | ------------------- | -------------------------------- | ---------------- |
| Credentials    | `--credentials`     | `GOOGLE_APPLICATION_CREDENTIALS` | `credentials`    |
| Spreadsheet ID | `--spreadsheet-id`  | `TBXL_SPREADSHEET_ID`            | `spreadsheet-id` |
| Sheet name     | `--sheet`           | `TBXL_SHEET`                     | `sheet`          |

::: warning An empty value stops the chain
An explicitly empty value is *present*. Passing `--sheet ""` (or setting
`sheet = ""` in config) deliberately blanks the value and stops the search — the
chain does **not** fall through to the next level. This is how you override a
configured default back to "unset" for a single command.
:::

The sheet name is special: it can be omitted entirely if the document has exactly
one sheet. If the document has multiple sheets and no sheet is resolved, commands
fail with `sheet_ambiguous`.

## Config files

Config is stored as [TOML](https://toml.io/). There are two scopes:

- **Project** config: `.tabrixel/config.toml`, discovered by walking **up** from
  the current directory (the same way Git finds `.git`).
- **Global** config: `~/.tabrixel/config.toml` in your home directory.

Project config overrides global config **per key**. Available keys are
`spreadsheet-id`, `sheet`, and `credentials`:

```toml
# .tabrixel/config.toml
spreadsheet-id = "1AbC...xyz"
sheet = "Users"
credentials = "./key.json"
```

::: tip Config is found walking up, not down
A `.tabrixel/` directory in a *subdirectory* of your current location is **not**
found. If `tbxl` reports a value as unset, make sure you're running from inside
the directory tree that contains the config — or `cd` into it.
:::

## Managing config

Use the [`config`](/commands/config) commands to read and write the store:

```sh
# Write to the project config (current directory tree)
tbxl config set spreadsheet-id 1AbC...xyz
tbxl config set sheet Users

# Write to the global config instead
tbxl config set credentials ~/keys/key.json --global

# Read the effective value (project overrides global)
tbxl config get sheet

# List every key with its value, scope, and the file paths in use
tbxl config list
```

## Seeing what resolved

[`tbxl auth check`](/commands/auth-check) reports the final resolved value for
each input **and the source** it came from — flag, environment, project config,
or global config. It's the quickest way to debug "why is `tbxl` using that
value?"
