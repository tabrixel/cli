using System.ComponentModel;
using Spectre.Console.Cli;
using Tabrixel.Configuration;

namespace Tabrixel.Infrastructure;

/// <summary>
/// The argument is optional at the parser level so that a missing ID produces
/// a domain error rendered per --output, not Spectre's parse-error text.
/// </summary>
public class SpreadsheetSettings : GlobalSettings
{
    [CommandArgument(0, "[SPREADSHEET_ID]")]
    [Description("Google Spreadsheets document ID. Takes precedence over --spreadsheet-id.")]
    public string? SpreadsheetIdArgument { get; set; }

    public string RequireSpreadsheetId()
    {
        var resolved = Resolver.Resolve(ConfigKeys.SpreadsheetId, Consts.EnvSpreadsheetId,
            SpreadsheetId, SpreadsheetIdArgument);

        return resolved.Value switch
        {
            null => throw new CliException(ErrorCode.InvalidArguments,
                "spreadsheet ID is not set: pass it as an argument, with --spreadsheet-id, " +
                $"via the {Consts.EnvSpreadsheetId} environment variable, " +
                "or with 'config set spreadsheet-id <id>'"),
            "" => throw new CliException(ErrorCode.InvalidArguments,
                $"spreadsheet ID is empty (set at the {resolved.Source.Label()} level)"),
            var id => id
        };
    }
}
