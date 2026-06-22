using Tabrixel.Infrastructure;

namespace Tabrixel.Configuration;

public static class ConfigKeys
{
    public const string SpreadsheetId = "spreadsheet-id";
    public const string Sheet = "sheet";
    public const string Credentials = "credentials";

    public static readonly IReadOnlyList<string> All = [SpreadsheetId, Sheet, Credentials];

    public static string Require(string key) =>
        All.Contains(key, StringComparer.Ordinal)
            ? key
            : throw new CliException(ErrorCode.InvalidArguments,
                $"unknown config key '{key}'; supported keys: {string.Join(", ", All)}",
                new Dictionary<string, object?> { ["supported_keys"] = All });
}
