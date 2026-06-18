# Spec: Project README

## Purpose

Requirements for the repository's top-level `README.md` — what sections it must contain and what accuracy guarantees it must uphold (documented commands, flags, and env vars must match the implementation). The README is reoriented to NuGet end-users and also serves as the package overview (`PackageReadmeFile`).

## Requirements

### Requirement: Repository has a root README
The repository SHALL contain a `README.md` file at the repository root whose opening leads with a concise, user-facing value proposition: it SHALL identify Tabrixel (`tbxl`) as a command-line tool that treats a Google Sheet as a table of records, state in one or two sentences what a user can do with it (inspect a document; read rows as records; add, update, upsert, and delete rows by column conditions), and note that it is distributed as a .NET global tool (.NET 10). The opening SHALL be scannable (no prose longer than a short paragraph before the first actionable section). Because `Tabrixel/Tabrixel.csproj` declares `README.md` as the `PackageReadmeFile`, this same file SHALL serve as the NuGet package overview.

#### Scenario: New visitor opens the repository
- **WHEN** a user opens the repository root or the NuGet package page
- **THEN** the opening section states what the tool is, what it does for the user, and that it installs as a .NET global tool, without requiring the reader to follow a link first

#### Scenario: README is the NuGet package readme
- **WHEN** the package is built
- **THEN** the same root `README.md` is packed as the `PackageReadmeFile` and renders as a self-contained overview on the package page

### Requirement: README provides an example-first Quick Start
The README SHALL include a Quick Start section, placed near the top after Installation, containing several copy-pasteable example commands that cover the common flows: verifying credentials (`tbxl auth check`), inspecting a document (`tbxl describe`, `tbxl columns`), reading records (`tbxl rows list` with `--where`/`--limit`/`--columns`), and mutating records (`tbxl rows add`, `tbxl rows update --set`, `tbxl rows upsert`, `tbxl rows delete --yes`). Every example's flags and arguments SHALL be accepted by the CLI parser.

#### Scenario: User copies a Quick Start example
- **WHEN** a user copies any command from the Quick Start section
- **THEN** its flags and arguments are accepted by the CLI parser and match the corresponding command's settings

#### Scenario: User understands the tool without leaving the README
- **WHEN** a prospective user reads only the Installation and Quick Start sections
- **THEN** they can install the tool and run a working read and a working write command against a sheet they have access to

### Requirement: README documents authentication
The README SHALL explain that commands require a Google service account JSON key and SHALL list all ways to supply it: the `--credentials` flag, the `GOOGLE_APPLICATION_CREDENTIALS` environment variable, and the `credentials` config key, including their precedence order.

#### Scenario: User sets up credentials
- **WHEN** a user reads the Authentication section
- **THEN** all three credential sources are listed in precedence order and `tbxl auth check` is shown as the verification command

### Requirement: README documents configuration
The README SHALL document the config store: project `.tabrixel/config.toml` (discovered walking up from cwd) and global `~/.tabrixel/config.toml`, the available keys (`spreadsheet-id`, `sheet`, `credentials`), the `config set/get/list` commands, the `TBXL_SPREADSHEET_ID` and `TBXL_SHEET` environment variables, and the full value precedence chain (CLI flag → environment → project config → global config), including that an explicitly empty value stops the chain.

#### Scenario: User looks up value precedence
- **WHEN** a user reads the Configuration section
- **THEN** the precedence chain is stated in resolution order and matches the behavior implemented in `ValueResolver`

### Requirement: README documents every CLI command
The README SHALL present every command registered in `Program.cs` (`auth check`, `config set/get/list`, `describe`, `columns`, `rows list/add/update/upsert/delete`) in a compact, scannable reference (a table or equivalently terse list) with a one-line description per command consistent with the CLI help text, and SHALL direct readers to `tbxl <command> --help` for full options.

#### Scenario: Documented commands match the implementation
- **WHEN** the README command reference is compared against the commands registered in `Program.cs`
- **THEN** every registered command appears in the reference and no documented command is absent from the implementation

#### Scenario: Reference is scannable
- **WHEN** a user skims the command reference
- **THEN** each command and its one-line description can be read at a glance without prose paragraphs, and a pointer to `tbxl <command> --help` is present

### Requirement: README documents output formats and exit codes
The README SHALL document the global `--json` flag (text output is the default; `--json` switches to JSON), that JSON uses snake_case property names, that data goes to stdout and errors to stderr, and that the process exit code is non-zero on failure.

#### Scenario: User scripts against JSON output
- **WHEN** a user reads the Output section
- **THEN** it states how to request JSON with `--json`, the casing convention, and the stdout/stderr split so output can be machine-parsed

### Requirement: README links to the project website for further documentation
The README SHALL include a Documentation & Support section that links the project website (`https://tabrixel.com`) for deeper documentation and the repository (`https://github.com/tabrixel/cli`) for source and issues, so that the README stays compact while pointing readers to full docs when they need more.

#### Scenario: User wants more than the README provides
- **WHEN** a user reaches the end of the README needing more detail than it contains
- **THEN** a clearly labeled link to the project website is present, alongside a link to the repository for reporting issues
