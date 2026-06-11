using System.ComponentModel;
using Spectre.Console.Cli;
using Tabrixel.Configuration;

namespace Tabrixel.Infrastructure;

public class GlobalSettings : CommandSettings
{
    [CommandOption("--output <FORMAT>")]
    [Description("Output format: text (default) or json.")]
    [DefaultValue(OutputFormat.Text)]
    public OutputFormat Format { get; set; }

    [CommandOption("--credentials <PATH>")]
    [Description("Path to the Google service account JSON key. " +
                 "Defaults to the GOOGLE_APPLICATION_CREDENTIALS environment variable, " +
                 "then the 'credentials' config key.")]
    public string? CredentialsPath { get; set; }

    [CommandOption("--spreadsheet-id <ID>")]
    [Description("Google Spreadsheets document ID. " +
                 "Defaults to the TBXL_SPREADSHEET_ID environment variable, " +
                 "then the 'spreadsheet-id' config key.")]
    public string? SpreadsheetId { get; set; }

    [CommandOption("--sheet <NAME>")]
    [Description("Sheet name. Defaults to the TBXL_SHEET environment variable, " +
                 "then the 'sheet' config key. Can be omitted if the document has exactly one sheet.")]
    public string? SheetName { get; set; }

    private ValueResolver? _resolver;

    public ValueResolver Resolver => _resolver ??= ValueResolver.Create();

    public ResolvedValue ResolveCredentialsPath() =>
        Resolver.Resolve(ConfigKeys.Credentials, Consts.EnvGoogleCredentials, CredentialsPath);

    public ResolvedValue ResolveSpreadsheetId() =>
        Resolver.Resolve(ConfigKeys.SpreadsheetId, Consts.EnvSpreadsheetId, SpreadsheetId);

    public ResolvedValue ResolveSheetName() =>
        Resolver.Resolve(ConfigKeys.Sheet, Consts.EnvSheet, SheetName);
}
