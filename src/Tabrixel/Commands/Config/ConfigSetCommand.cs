using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Tabrixel.Configuration;
using Tabrixel.Infrastructure;

namespace Tabrixel.Commands.Config;

public class ConfigSetSettings : GlobalSettings
{
    [CommandArgument(0, "<KEY>")]
    [Description("Config key: spreadsheet-id, sheet, or credentials.")]
    public string Key { get; set; } = string.Empty;

    [CommandArgument(1, "<VALUE>")]
    [Description("Value to store.")]
    public string Value { get; set; } = string.Empty;

    [CommandOption("--global")]
    [Description("Write to the global ~/.tabrixel/config.toml instead of the project config.")]
    public bool Global { get; set; }
}

public class ConfigSetCommand : CliCommand<ConfigSetSettings>
{
    protected override ExitCodes ExecuteCommand(CommandContext context, ConfigSetSettings settings,
        CancellationToken cancellationToken)
    {
        var key = ConfigKeys.Require(settings.Key);

        // Project scope writes to the discovered config (like git config writes
        // to the enclosing repo); only first-time use creates one in the cwd.
        var path = settings.Global
            ? ConfigLocator.GlobalPath
            : ConfigLocator.FindProjectPath(Directory.GetCurrentDirectory())
              ?? ConfigLocator.DefaultProjectPath(Directory.GetCurrentDirectory());

        var file = ConfigFile.Load(path);
        file.SetValue(key, settings.Value);
        file.Save();

        var scope = settings.Global ? "global" : "project";
        var payload = new Payload(key, settings.Value, scope, file.FilePath);

        Renderer.Data(payload,
            console => console.MarkupLine(
                $"[green]✓[/] {Markup.Escape(key)} = {Markup.Escape(settings.Value)} " +
                $"({scope}: {Markup.Escape(file.FilePath)})"),
            settings.Format);

        return ExitCodes.Success;
    }

    private record Payload(string Key, string Value, string Scope, string Path);
}
