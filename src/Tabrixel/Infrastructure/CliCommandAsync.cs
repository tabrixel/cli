using Spectre.Console.Cli;

namespace Tabrixel.Infrastructure;

public abstract class CliCommandAsync<TSettings> : AsyncCommand<TSettings> where TSettings : GlobalSettings
{
    protected override async Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            return (int)await ExecuteCommand(context, settings, cancellationToken);
        }
        catch (CliException e)
        {
            Renderer.Error(e.Error, settings.Format);
            return (int)ExitCodes.Failure;
        }
        catch (Exception e)
        {
            Renderer.Error(new CliError(ErrorCode.Internal, e.Message), settings.Format);
            return (int)ExitCodes.Failure;
        }
    }
    
    protected abstract Task<ExitCodes> ExecuteCommand(CommandContext context, TSettings settings, CancellationToken cancellationToken);
}
