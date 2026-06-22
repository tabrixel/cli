namespace Tabrixel.Infrastructure;

public class CliException(CliError error) : Exception(error.Message)
{
    public CliError Error { get; } = error;
    
    public CliException(ErrorCode code, string message, IReadOnlyDictionary<string, object?>? details = null) 
        : this(new CliError(code, message, details))
    {
    }
}
