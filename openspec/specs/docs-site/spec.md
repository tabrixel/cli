# Spec: Documentation Site

## Purpose

Requirements for the VitePress documentation site under `docs/` — that it contains no scaffolded template content, introduces Tabrixel (`tbxl`) on its home page, and provides accurate guide, configuration, command-reference, and concepts content whose documented commands, flags, env vars, precedence, and exit codes match the implementation. Also governs the VitePress configuration (`nav`/`sidebar`/`socialLinks`) and retention of the `vitepress-plugin-llms` plugin.

## Requirements

### Requirement: Documentation site contains no default template content
The VitePress site under `docs/` SHALL NOT contain any of the scaffolded template content. The placeholder home hero (`tagline: My great project tagline`), the `Feature A/B/C` feature cards, and the template pages `markdown-examples.md` and `api-examples.md` SHALL be removed or replaced with real Tabrixel content.

#### Scenario: Build output contains no scaffold text
- **WHEN** the docs are inspected after this change
- **THEN** the strings "My great project tagline", "Feature A", "Runtime API Examples", and "Markdown Extension Examples" SHALL NOT appear anywhere under `docs/` source files
- **AND** `docs/markdown-examples.md` and `docs/api-examples.md` SHALL NOT exist

### Requirement: Home page introduces Tabrixel
The home page (`docs/index.md`) SHALL use the VitePress `layout: home` hero to present Tabrixel (`tbxl`) as a CLI for working with Google Sheets as record tables, with a tagline describing its real purpose and feature cards summarizing genuine capabilities (e.g. records model, safe mutations, dual text/JSON output, configuration precedence).

#### Scenario: Visitor opens the site root
- **WHEN** a user opens the documentation home page
- **THEN** the hero name is "Tabrixel", the tagline describes working with Google Sheets from the command line, and the feature cards describe real features rather than `Lorem ipsum` placeholders
- **AND** the primary hero action links to a getting-started or guide page that exists in the site

### Requirement: Site has a guide covering setup
The site SHALL contain guide pages covering: an introduction to what Tabrixel is and the records model (row 1 is the header, rows below are records keyed by column name), installation (`dotnet tool install --global Tabrixel`), authentication setup with a Google service account JSON key, and configuration.

#### Scenario: User follows the getting-started path
- **WHEN** a user reads the guide from introduction through authentication
- **THEN** it explains that a Google service account JSON key is required, lists all three credential sources (`--credentials` flag, `GOOGLE_APPLICATION_CREDENTIALS` env var, `credentials` config key) in precedence order, and shows `tbxl auth check` as the verification command

### Requirement: Site documents configuration and value precedence
The site SHALL document the config store — project `.tabrixel/config.toml` (discovered by walking up from the current directory) and global `~/.tabrixel/config.toml`, the keys (`spreadsheet-id`, `sheet`, `credentials`), the `config set/get/list` commands, the `TBXL_SPREADSHEET_ID` and `TBXL_SHEET` environment variables, and the full value precedence chain (CLI flag → environment → project config → global config), including that an explicitly empty value stops the chain.

#### Scenario: User looks up value precedence
- **WHEN** a user reads the configuration page
- **THEN** the precedence chain is stated in resolution order and matches the behavior implemented in `ValueResolver`
- **AND** the page states that project config overrides global config per key and that an explicitly empty value (`--flag ""` or `key = ""`) stops the chain

### Requirement: Site documents every CLI command
The site SHALL provide a command reference that lists every command registered in `Program.cs` (`auth check`, `config set`, `config get`, `config list`, `describe`, `columns`, `rows list`, `rows add`, `rows update`, `rows upsert`, `rows delete`), each with a description consistent with the CLI help text, its options, and at least one usage example.

#### Scenario: Documented commands match the implementation
- **WHEN** the command reference is compared against the commands registered in `Program.cs`
- **THEN** every registered command appears in the reference and no documented command is absent from the implementation

#### Scenario: User copies a documented example
- **WHEN** a user copies an example from the command reference
- **THEN** the example's flags and arguments are accepted by the CLI parser (e.g. the spreadsheet id is supplied via `--spreadsheet-id`/env/config and never positionally; the only positional argument is the JSON record of `rows add`/`rows upsert`)

### Requirement: Site documents output formats, matching, safety, and exit codes
The site SHALL document the global `--json` flag (text is the default; `--json` switches to JSON with snake_case keys, data on stdout and errors on stderr), the `--where`/`--set` matching rules (exact, case-sensitive, string-typed; `--where` repeatable and AND-combined), the mutation safety rules (`ambiguous_match` resolved with `--all`/`--first`, `delete` requiring `--yes`, `--dry-run` previewing), the meaning of `row` in receipts (1-based sheet row, header is row 1), and the exit codes (0 success, 1 failure, 2 matched nothing) together with the error `code` values.

#### Scenario: User scripts against the CLI
- **WHEN** a user reads the output/concepts pages
- **THEN** it states how to request JSON with `--json`, the snake_case casing, the stdout/stderr split, and the three exit codes
- **AND** it explains that `delete` requires `--yes`, that more than one match fails with `ambiguous_match` unless `--all`/`--first` is given, and that exit code 2 means the command ran but matched zero rows

### Requirement: VitePress configuration reflects the real site
`docs/.vitepress/config.mts` SHALL define `nav`, `sidebar`, and `socialLinks` entries that point only to pages that exist in the new site, with no links to the removed template pages, and SHALL retain the `vitepress-plugin-llms` plugin so `llms.txt`/`llms-full.txt` continue to be generated.

#### Scenario: Site builds with no broken internal links
- **WHEN** `vitepress build docs` is run
- **THEN** the build succeeds, every `nav`/`sidebar` link resolves to an existing page, and no link references `markdown-examples` or `api-examples`

### Requirement: Installation guide documents the global-tool install only

The installation guide (`docs/guide/installation.md`) SHALL document the .NET global-tool install flow (`dotnet tool install --global Tabrixel`, update, and verify steps) and SHALL NOT contain a "Supported platforms" section or enumerate self-contained single-file build runtime identifiers (e.g. `win-x64`, `win-arm64`, `linux-x64`, `osx-arm64`). Generated site output under `docs/.vitepress/dist/` and the generated `llms.txt`/`llms-full.txt` SHALL likewise be free of that section once rebuilt.

#### Scenario: Installation page omits the platforms section

- **WHEN** the installation guide is inspected after this change
- **THEN** the heading "Supported platforms" SHALL NOT appear in `docs/guide/installation.md`
- **AND** the runtime identifiers `win-x64`, `win-arm64`, `linux-x64`, and `osx-arm64` SHALL NOT appear on that page

#### Scenario: Install, update, and verify steps remain

- **WHEN** a user reads the installation guide
- **THEN** it still shows `dotnet tool install --global Tabrixel`, the `dotnet tool update --global Tabrixel` update step, and the `tbxl --version` verify step

#### Scenario: Rebuilt output is free of the section

- **WHEN** the docs are rebuilt with `vitepress build docs`
- **THEN** the generated installation page under `docs/.vitepress/dist/` and the generated `llms.txt`/`llms-full.txt` SHALL NOT contain the string "Supported platforms"
