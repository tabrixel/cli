# auth check

Validate the service account key and report the resolved configuration, along
with where each value came from. This is the command to run first when setting up
or debugging access.

```sh
tbxl auth check [options]
```

## What it does

- Loads the service account JSON key and confirms it is valid.
- Resolves the credentials path, spreadsheet ID, and sheet name through the full
  [precedence chain](/guide/configuration#the-value-precedence-chain) and reports
  the **source** of each (flag, environment, project config, or global config).
- If `--spreadsheet-id` resolves to a value, also checks that the service account
  can access that document.

## Options

Only the [global options](/commands/#global-options) apply. The most useful here:

| Option | Effect |
| ------ | ------ |
| `--credentials <PATH>` | The key to validate (or rely on env/config). |
| `--spreadsheet-id <ID>` | If set, also verifies access to that document. |
| `--json` | Emit the result as JSON. |

## Examples

```sh
# Validate the key resolved from env/config
tbxl auth check

# Validate a specific key and confirm access to a document
tbxl auth check --credentials ./key.json --spreadsheet-id <ID>

# Machine-readable output
tbxl auth check --json
```

## JSON output

```json
{
  "service_account_email": "my-bot@my-project.iam.gserviceaccount.com",
  "spreadsheet_id": "1AbC...xyz",
  "status": "OK",
  "sources": { "credentials": "flag", "spreadsheet_id": "config", "sheet": "env" }
}
```

The `service_account_email` is the address you must share the spreadsheet with.
See [Getting Started](/guide/getting-started) for the sharing step.
