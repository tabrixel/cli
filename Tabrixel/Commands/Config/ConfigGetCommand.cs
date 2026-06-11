using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Tabrixel.Configuration;
using Tabrixel.Infrastructure;

namespace Tabrixel.Commands.Config;

public class ConfigGetSettings : GlobalSettings
{
    [CommandArgument(0, "<KEY>")]
    [Description("Config key: spreadsheet-id, sheet, or credentials.")]
    public string Key { get; set; } = string.Empty;
}

public class ConfigGetCommand : CliCommand<ConfigGetSettings>
{
    protected override ExitCodes ExecuteCommand(CommandContext context, ConfigGetSettings settings,
        CancellationToken cancellationToken)
    {
        var key = ConfigKeys.Require(settings.Key);

        // Config-level view only (project beats global): flags and env are
        // intentionally not consulted — full-chain diagnostics live in auth check.
        var resolved = settings.Resolver.ResolveConfig(key);

        if (!resolved.IsSet)
        {
            throw new CliException(ErrorCode.NotFound,
                $"config key '{key}' is not set in any config file; " +
                $"set it with 'config set {key} <value>'");
        }

        var scope = resolved.Source == ValueSource.ProjectConfig ? "project" : "global";
        var path = resolved.Source == ValueSource.ProjectConfig
            ? settings.Resolver.ProjectConfig!.FilePath
            : settings.Resolver.GlobalConfig.FilePath;

        Renderer.Data(new Payload(key, resolved.Value!, scope, path),
            console => console.MarkupLine(
                $"{Markup.Escape(resolved.Value!)} [grey]({scope}: {Markup.Escape(path)})[/]"),
            settings.Format);

        return ExitCodes.Success;
    }

    private record Payload(string Key, string Value, string Scope, string Path);
}
