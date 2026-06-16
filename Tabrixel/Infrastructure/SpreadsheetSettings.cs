using Tabrixel.Configuration;

namespace Tabrixel.Infrastructure;

public class SpreadsheetSettings : GlobalSettings
{
    // No positional spreadsheet ID here: commands that accept one derive from
    // PositionalSpreadsheetSettings, which overrides RequireSpreadsheetId to
    // inject the argument. Commands without a positional id (rows add/upsert,
    // where the positional slot carries the JSON record) use this base, so the
    // ID resolves from --spreadsheet-id, the env variable, or config.
    public virtual string RequireSpreadsheetId() =>
        Require(
            Resolver.Resolve(ConfigKeys.SpreadsheetId, Consts.EnvSpreadsheetId, SpreadsheetId),
            "spreadsheet ID is not set: pass it with --spreadsheet-id, " +
            $"via the {Consts.EnvSpreadsheetId} environment variable, " +
            "or with 'config set spreadsheet-id <id>'");

    // Shared null/empty handling so the positional override and this base render
    // identical domain errors (an explicit empty value stops the chain).
    protected static string Require(ResolvedValue resolved, string notSetMessage) =>
        resolved.Value switch
        {
            null => throw new CliException(ErrorCode.InvalidArguments, notSetMessage),
            "" => throw new CliException(ErrorCode.InvalidArguments,
                $"spreadsheet ID is empty (set at the {resolved.Source.Label()} level)"),
            var id => id
        };
}
