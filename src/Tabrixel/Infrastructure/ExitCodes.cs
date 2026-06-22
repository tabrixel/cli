namespace Tabrixel.Infrastructure;

public enum ExitCodes
{
    Success = 0,
    Failure = 1,
    /// <summary>The condition worked correctly but affected 0 rows (rows update/delete).</summary>
    NoMatch = 2
}
