using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

/// <summary>
/// Match-count semantics for mutating commands (rows update, rows delete).
/// Zero matches is not special here: an empty list is returned, and the
/// consuming command decides "0 rows → warning + exit code 2".
/// </summary>
public static class MatchSemantics
{
    public static IReadOnlyList<int> Select(IReadOnlyList<int> matched, bool all, bool first)
    {
        if (all && first)
        {
            throw new CliException(ErrorCode.InvalidArguments,
                "--all and --first cannot be used together");
        }

        if (matched.Count > 1 && !all && !first)
        {
            throw new CliException(ErrorCode.AmbiguousMatch,
                $"{matched.Count} rows match the condition; narrow --where or use --all / --first",
                new Dictionary<string, object?> { ["matched"] = matched.Count });
        }

        return first ? matched.Take(1).ToList() : matched;
    }
}
