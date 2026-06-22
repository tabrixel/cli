namespace Tabrixel.Infrastructure;

public sealed record CliError(
    ErrorCode Code, 
    string Message, 
    IReadOnlyDictionary<string, object?>? Details = null);
    