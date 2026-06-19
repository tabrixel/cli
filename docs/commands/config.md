---
description: Set, get, and list the stored defaults — spreadsheet ID, sheet, and credentials — in project or global config.
---

# config

Manage default values stored in `.tabrixel/config.toml`. Persisting the
spreadsheet ID, sheet, and credentials path means you don't have to repeat them
on every command. See [Configuration](/guide/configuration) for the storage model
and precedence rules.

The available keys are `spreadsheet-id`, `sheet`, and `credentials`.

## config set

Set a config key. By default it writes to the **project** config (the
`.tabrixel/config.toml` found by walking up from the current directory, created
if missing). Pass `--global` to write to `~/.tabrixel/config.toml` instead.

```sh
tbxl config set <key> <value> [--global]
```

```sh
tbxl config set credentials ~/path/to/key.json
tbxl config set spreadsheet-id 1234567890
tbxl config set sheet Sheet1

# Write to the global store
tbxl config set credentials ~/keys/key.json --global
```

## config get

Show the **effective** value of a key — i.e. the value that commands would
actually use, with project config overriding global config.

```sh
tbxl config get <key>
```

```sh
tbxl config get sheet
```

## config list

Show all config keys with their values, the scope each value comes from, and the
config file paths in use.

```sh
tbxl config list
tbxl config list --json
```

::: tip
`config get`/`list` report only the config layer. To see how a value resolves
across flags and environment variables too, use
[`auth check`](/commands/auth-check).
:::
