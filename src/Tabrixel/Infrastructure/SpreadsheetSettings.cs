using Tabrixel.Configuration;

namespace Tabrixel.Infrastructure;

public class SpreadsheetSettings : GlobalSettings
{
    // The spreadsheet ID resolves from --spreadsheet-id, the env variable, or
    // config; no command accepts it as a positional argument.
    public string RequireSpreadsheetId() =>
        Require(
            Resolver.Resolve(ConfigKeys.SpreadsheetId, Consts.EnvSpreadsheetId, SpreadsheetId),
            "spreadsheet ID is not set: pass it with --spreadsheet-id, " +
            $"via the {Consts.EnvSpreadsheetId} environment variable, " +
            "or with 'config set spreadsheet-id <id>'");

    // Shared null/empty handling: an explicit empty value stops the chain and
    // renders a distinct domain error.
    protected static string Require(ResolvedValue resolved, string notSetMessage) =>
        resolved.Value switch
        {
            null => throw new CliException(ErrorCode.InvalidArguments, notSetMessage),
            "" => throw new CliException(ErrorCode.InvalidArguments,
                $"spreadsheet ID is empty (set at the {resolved.Source.Label()} level)"),
            var id => id
        };
}
