# config-store

## Purpose

Storing default values for `spreadsheet-id`, `sheet`, and `credentials` in project (`.tabrixel/config.toml`, discovered up the directory tree) and global (`~/.tabrixel/config.toml`) TOML config files, managed via the `tbxl config set`, `config get`, and `config list` commands.

## Requirements

### Requirement: TOML config file with three keys
The CLI SHALL store default values in a TOML file named `config.toml` supporting exactly three keys: `spreadsheet-id`, `sheet`, and `credentials`. Unknown keys present in the file SHALL be ignored when reading and preserved when writing. A file that fails to parse as TOML SHALL produce an `invalid_arguments` error naming the file path, not be treated as absent.

#### Scenario: Known keys are read
- **WHEN** `.tabrixel/config.toml` contains `spreadsheet-id = "1AbC"`, `sheet = "Лист1"`, and `credentials = "/keys/sa.json"`
- **THEN** all three values are available as config-level defaults

#### Scenario: Unknown key is ignored on read and survives a write
- **WHEN** the config file contains an unrecognized key `foo = "bar"` and the user runs `tbxl config set sheet "Data"`
- **THEN** commands resolve values normally, and after the write the file still contains `foo = "bar"`

#### Scenario: Malformed config fails loudly
- **WHEN** `.tabrixel/config.toml` contains invalid TOML and any command needs config-level resolution
- **THEN** the command fails with an `invalid_arguments` error that names the config file path

### Requirement: Project and global config locations
The CLI SHALL support two config locations: a project config at `.tabrixel/config.toml` discovered by walking up the directory tree from the current working directory (first hit wins, like `.git`), and a global config at `~/.tabrixel/config.toml` in the user's home directory. Values SHALL merge per key, with the project config overriding the global config.

#### Scenario: Project config discovered up the tree
- **WHEN** the current directory is `repo/src/app` and `repo/.tabrixel/config.toml` exists
- **THEN** `repo/.tabrixel/config.toml` is used as the project config

#### Scenario: Per-key merge of project and global
- **WHEN** the project config sets only `spreadsheet-id` and the global config sets only `credentials`
- **THEN** resolution uses the project `spreadsheet-id` and the global `credentials`

#### Scenario: Project value beats global value
- **WHEN** both the project and global configs set `sheet`
- **THEN** the project value is used

### Requirement: config set command
The CLI SHALL provide `tbxl config set <key> <value>` writing one key to the project config by default: into the discovered project config if one exists up the tree, otherwise into a newly created `.tabrixel/config.toml` in the current working directory (creating the `.tabrixel` directory). With `--global`, the CLI SHALL write to `~/.tabrixel/config.toml` instead, creating the directory and file as needed. Setting an unknown key SHALL fail with `invalid_arguments`.

#### Scenario: First project write creates the file
- **WHEN** no `.tabrixel/config.toml` exists anywhere up the tree and the user runs `tbxl config set spreadsheet-id 1AbC`
- **THEN** `.tabrixel/config.toml` is created in the current working directory with `spreadsheet-id = "1AbC"`

#### Scenario: Existing project config is updated in place
- **WHEN** `repo/.tabrixel/config.toml` exists, the current directory is `repo/src`, and the user runs `tbxl config set sheet "Data"`
- **THEN** the key is written to `repo/.tabrixel/config.toml`, not to a new file under `repo/src`

#### Scenario: Global write
- **WHEN** the user runs `tbxl config set credentials /keys/sa.json --global`
- **THEN** the value is written to `~/.tabrixel/config.toml`

#### Scenario: Unknown key rejected
- **WHEN** the user runs `tbxl config set color red`
- **THEN** the command fails with an `invalid_arguments` error listing the supported keys

### Requirement: config get command
The CLI SHALL provide `tbxl config get <key>` printing the merged config-level value (project overriding global) together with the scope it came from. Flags and environment variables SHALL NOT be consulted. When the key is set in neither config file, the command SHALL fail with a `not_found` error.

#### Scenario: Merged value with scope
- **WHEN** `sheet` is set only in the global config and the user runs `tbxl config get sheet`
- **THEN** the global value is printed along with its scope (global)

#### Scenario: Unset key
- **WHEN** `credentials` is set in neither config file and the user runs `tbxl config get credentials`
- **THEN** the command fails with a `not_found` error

### Requirement: config list command
The CLI SHALL provide `tbxl config list` showing all supported keys with their merged values and source scope (project / global / unset), and the resolved paths of both config files including whether each exists.

#### Scenario: Full listing
- **WHEN** the user runs `tbxl config list` with a project config setting `spreadsheet-id` and a global config setting `credentials`
- **THEN** the output shows `spreadsheet-id` (project), `credentials` (global), `sheet` (unset), and both config file paths

### Requirement: config commands follow output conventions
The `config set`, `config get`, and `config list` commands SHALL support `--output text|json` like every other command, with data on stdout and errors on stderr.

#### Scenario: JSON output
- **WHEN** the user runs `tbxl config list --output json`
- **THEN** stdout contains a single JSON document with snake_case properties
