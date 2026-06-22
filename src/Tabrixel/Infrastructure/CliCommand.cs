using Spectre.Console.Cli;

namespace Tabrixel.Infrastructure;

public abstract class CliCommand<TSettings> : Command<TSettings> where TSettings : GlobalSettings
{
    protected override int Execute(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            return (int)ExecuteCommand(context, settings, cancellationToken);
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

    protected abstract ExitCodes ExecuteCommand(CommandContext context, TSettings settings,
        CancellationToken cancellationToken);
}
