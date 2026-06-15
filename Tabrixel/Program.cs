using Spectre.Console.Cli;
using Tabrixel;
using Tabrixel.Commands;
using Tabrixel.Commands.Auth;
using Tabrixel.Commands.Config;
using Tabrixel.Commands.Rows;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName(Consts.ApplicationName);
    config.SetApplicationVersion(Consts.ApplicationVersion);
    config.Settings.StrictParsing = true;

    config.AddBranch("auth", auth =>
    {
        auth.SetDescription("Authenticate using a service account key file");
        auth.AddCommand<AuthCheckCommand>("check").WithDescription("Check if the credentials are valid");
    });

    config.AddBranch("config", cfg =>
    {
        cfg.SetDescription("Manage default values stored in .tabrixel/config.toml (project or global)");

        cfg.AddCommand<ConfigSetCommand>("set")
            .WithDescription("Set a config key in the project config, or in ~/.tabrixel with --global")
            .WithExample("config", "set", "credentials", "~/path/to/key.json")
            .WithExample("config", "set", "spreadsheet-id", "1234567890")
            .WithExample("config", "set", "sheet", "Sheet1");

        cfg.AddCommand<ConfigGetCommand>("get")
            .WithDescription("Show the effective config value of a key (project overrides global)");

        cfg.AddCommand<ConfigListCommand>("list")
            .WithDescription("Show all config keys with values, scopes, and config file paths");
    });

    config.AddCommand<DescribeCommand>("describe")
        .WithDescription("Overview of the document: sheets, their columns and record counts.");

    config.AddCommand<ColumnsCommand>("columns")
        .WithDescription("List column names of the target sheet (from its first row).");

    config.AddBranch("rows", rows =>
    {
        rows.SetDescription("Work with sheet rows as records.");

        rows.AddCommand<RowsListCommand>("list")
            .WithDescription("Read rows below the header as records.");

        rows.AddCommand<RowsAddCommand>("add")
            .WithDescription("Add one record; values are laid out by column names from --json.");

        rows.AddCommand<RowsUpdateCommand>("update")
            .WithDescription("Update fields of rows matched by --where; --set assigns new column values.");

        rows.AddCommand<RowsUpsertCommand>("upsert")
            .WithDescription("Update rows matched by --where with --json fields, or insert a new record when nothing matches.");

        rows.AddCommand<RowsDeleteCommand>("delete")
            .WithDescription("Delete rows matched by --where; requires --yes.");
    });
    
#if DEBUG
    // Development-only settings
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

return app.Run(args);
