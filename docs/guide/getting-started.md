# Getting Started

Tabrixel authenticates with a **Google service account JSON key**. This page
walks through creating one, granting it access to a spreadsheet, pointing `tbxl`
at the key, and verifying the setup.

## 1. Create a service account and key

In the [Google Cloud Console](https://console.cloud.google.com/):

1. Create (or select) a project.
2. **Enable the Google Sheets API** for that project.
3. Create a **service account**.
4. Create a **JSON key** for the service account and download it. This file is
   your credential — keep it secret.

## 2. Share the spreadsheet

A service account is its own identity with its own email address (something like
`my-bot@my-project.iam.gserviceaccount.com`). It can only see spreadsheets that
have been shared with it.

Open your target spreadsheet in Google Sheets, click **Share**, and add the
service account's email — as **Viewer** for read-only commands, or **Editor** if
you intend to add, update, or delete rows.

## 3. Point Tabrixel at the key

There are three ways to supply the credential. They are tried in this order, and
the **first one present wins**:

1. The `--credentials <path>` flag.
2. The `GOOGLE_APPLICATION_CREDENTIALS` environment variable.
3. The `credentials` config key (`tbxl config set credentials <path>`).

```sh
# 1. Per-command flag
tbxl auth check --credentials ./key.json

# 2. Environment variable
export GOOGLE_APPLICATION_CREDENTIALS=./key.json   # PowerShell: $env:GOOGLE_APPLICATION_CREDENTIALS = "./key.json"
tbxl auth check

# 3. Persisted config (see the Configuration guide)
tbxl config set credentials ./key.json
tbxl auth check
```

See [Configuration](/guide/configuration) for the full precedence chain and how
to persist the spreadsheet ID and sheet too.

## 4. Verify your setup

`tbxl auth check` validates the key and reports the resolved configuration and
where each value came from. Add `--spreadsheet-id` to also confirm the service
account can reach a specific document:

```sh
tbxl auth check --spreadsheet-id <SPREADSHEET_ID>
```

The spreadsheet ID is the long token in the document URL:
`https://docs.google.com/spreadsheets/d/`**`<SPREADSHEET_ID>`**`/edit`.

::: warning No session state
There is no login that "sticks." Every command resolves credentials and the
spreadsheet ID independently, so supply them on each call — or persist them with
[`config set`](/commands/config) or environment variables.
:::

## Next steps

```sh
tbxl describe --spreadsheet-id <ID>          # see all sheets and their columns
tbxl rows list --spreadsheet-id <ID> --sheet Users
```

Continue to the [Commands reference](/commands/) for everything `tbxl` can do.
