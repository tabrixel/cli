---
# https://vitepress.dev/reference/default-theme-home-page
layout: home
description: Tabrixel (tbxl) is a command-line tool that treats a Google Sheet as a table of records you can inspect, read, and safely mutate.

hero:
  name: "Tabrixel"
  text: "Google Sheets from the command line"
  tagline: A CLI (tbxl) that treats a Google Sheet as a table of records — inspect a document, read rows, and add, update, upsert, or delete them by column conditions.
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: Introduction
      link: /guide/introduction
    - theme: alt
      text: Commands
      link: /commands/

features:
  - title: Sheets as records
    details: Row 1 is the header; every row below is a record keyed by column name. List, add, update, upsert, and delete rows by exact column conditions.
  - title: Safe by default
    details: Mutations need a single unambiguous match (or explicit --all/--first), delete refuses without --yes, and --dry-run previews any change before it touches the sheet.
  - title: Text or JSON output
    details: Human-readable tables by default, or pass --json for snake_case JSON with data on stdout and errors on stderr — ready to pipe into scripts.
  - title: Layered configuration
    details: Resolve credentials, spreadsheet ID, and sheet from flags, environment variables, or project/global TOML config, with a clear precedence chain.
---
