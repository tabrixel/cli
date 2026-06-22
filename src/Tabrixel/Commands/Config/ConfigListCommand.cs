using Spectre.Console;
using Spectre.Console.Cli;
using Tabrixel.Configuration;
using Tabrixel.Infrastructure;

namespace Tabrixel.Commands.Config;

public class ConfigListCommand : CliCommand<GlobalSettings>
{
    protected override ExitCodes ExecuteCommand(CommandContext context, GlobalSettings settings,
        CancellationToken cancellationToken)
    {
        var resolver = settings.Resolver;

        var entries = ConfigKeys.All
            .Select(key =>
            {
                var resolved = resolver.ResolveConfig(key);
                return new Entry(key, resolved.Value, resolved.Source switch
                {
                    ValueSource.ProjectConfig => "project",
                    ValueSource.GlobalConfig => "global",
                    _ => null
                });
            })
            .ToList();

        // When no project config was discovered, show where one would be created.
        var projectPath = resolver.ProjectConfig?.FilePath
                          ?? ConfigLocator.DefaultProjectPath(Directory.GetCurrentDirectory());

        var payload = new Payload(
            entries,
            new FileInfoPayload(projectPath, resolver.ProjectConfig is not null),
            new FileInfoPayload(resolver.GlobalConfig.FilePath, resolver.GlobalConfig.Exists));

        Renderer.Data(payload, console => Render(console, payload), settings.Format);

        return ExitCodes.Success;
    }

    private static void Render(IAnsiConsole console, Payload payload)
    {
        var table = new Table();
        table.AddColumn("Key");
        table.AddColumn("Value");
        table.AddColumn("Scope");

        foreach (var entry in payload.Keys)
        {
            table.AddRow(
                Markup.Escape(entry.Key),
                entry.Scope is null ? "[grey]—[/]" : Markup.Escape(entry.Value ?? string.Empty),
                entry.Scope is null ? "[grey]unset[/]" : Markup.Escape(entry.Scope));
        }

        console.Write(table);
        console.MarkupLine(FormatFile("project config", payload.ProjectConfig));
        console.MarkupLine(FormatFile("global config", payload.GlobalConfig));
    }

    private static string FormatFile(string label, FileInfoPayload file) =>
        $"{label}: {Markup.Escape(file.Path)} " +
        (file.Exists ? "[green](found)[/]" : "[grey](not found)[/]");

    private record Entry(string Key, string? Value, string? Scope);

    private record FileInfoPayload(string Path, bool Exists);

    private record Payload(
        IReadOnlyList<Entry> Keys,
        FileInfoPayload ProjectConfig,
        FileInfoPayload GlobalConfig);
}
