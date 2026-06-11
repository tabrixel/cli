# credential-resolution

## Purpose

Resolving the Google service account JSON key path from the `--credentials` option with fallback to the standard `GOOGLE_APPLICATION_CREDENTIALS` environment variable and the `credentials` key of the project and global config files.

## Requirements

### Requirement: Credentials path resolution
The CLI SHALL resolve the Google service account JSON key path top-down, taking the first level where the value is present: the `--credentials` option, then the `GOOGLE_APPLICATION_CREDENTIALS` environment variable, then the `credentials` key of the project config, then the `credentials` key of the global config. The CLI SHALL NOT read the `TBXL_GOOGLE_CREDENTIALS` environment variable. The resolved path SHALL always be passed to the Google SDK explicitly; the SDK SHALL NOT be left to read `GOOGLE_APPLICATION_CREDENTIALS` on its own.

#### Scenario: Explicit option wins
- **WHEN** a command is invoked with `--credentials <path>` while `GOOGLE_APPLICATION_CREDENTIALS` is also set
- **THEN** the key is loaded from the `--credentials` path

#### Scenario: Environment variable fallback
- **WHEN** a command is invoked without `--credentials` and `GOOGLE_APPLICATION_CREDENTIALS` points to a service account JSON key
- **THEN** the key is loaded from the path in `GOOGLE_APPLICATION_CREDENTIALS`

#### Scenario: Config fallback
- **WHEN** a command is invoked without `--credentials`, `GOOGLE_APPLICATION_CREDENTIALS` is not set, and the project or global config sets `credentials`
- **THEN** the key is loaded from the config-provided path (project config preferred over global)

#### Scenario: Legacy variable is ignored
- **WHEN** a command is invoked without `--credentials`, `TBXL_GOOGLE_CREDENTIALS` is set, `GOOGLE_APPLICATION_CREDENTIALS` is not set, and no config provides `credentials`
- **THEN** the command fails with an `auth_failed` error indicating credentials are missing

#### Scenario: Nothing set anywhere
- **WHEN** a command is invoked without `--credentials`, without `GOOGLE_APPLICATION_CREDENTIALS`, and with no `credentials` key in either config
- **THEN** the command fails with an `auth_failed` error explaining all the ways to provide the key (option, env variable, config)

### Requirement: Documentation references the standard variable
The `--credentials` option help text and project documentation SHALL reference `GOOGLE_APPLICATION_CREDENTIALS` as the fallback environment variable and SHALL NOT mention `TBXL_GOOGLE_CREDENTIALS`.

#### Scenario: Help text names the variable
- **WHEN** a user views command help for any command (e.g., `tbxl auth check --help`)
- **THEN** the `--credentials` description states it defaults to the `GOOGLE_APPLICATION_CREDENTIALS` environment variable
