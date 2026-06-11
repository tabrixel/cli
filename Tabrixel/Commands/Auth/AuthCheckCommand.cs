using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Spectre.Console;
using Spectre.Console.Cli;
using Tabrixel.Configuration;
using Tabrixel.Infrastructure;
using Tabrixel.Sheets;

namespace Tabrixel.Commands.Auth;

public class AuthCheckCommand : CliCommand<GlobalSettings>
{
    protected override ExitCodes ExecuteCommand(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        var client = SheetsServiceFactory.Create(settings);

        try
        {
            ((ITokenAccess)client.Credential)
                .GetAccessTokenForRequestAsync(cancellationToken: cancellationToken)
                .GetAwaiter()
                .GetResult();
        }
        catch (TokenResponseException ex)
        {
            throw new CliException(ErrorCode.AuthFailed,
                $"Google rejected the service account key: {ex.Error?.ErrorDescription ?? ex.Error?.Error ?? ex.Message}",
                new Dictionary<string, object?> { ["service_account"] = client.ServiceAccountEmail });
        }

        var spreadsheetId = settings.ResolveSpreadsheetId();

        if (!string.IsNullOrEmpty(spreadsheetId.Value))
        {
            try
            {
                var request = client.Service.Spreadsheets.Get(spreadsheetId.Value);
                request.Fields = "spreadsheetId";
                request.Execute();
            }
            catch (GoogleApiException ex)
            {
                throw ex.ToSpreadsheetAccessError(spreadsheetId.Value, client.ServiceAccountEmail);
            }
        }

        var result = new Payload(client.ServiceAccountEmail, spreadsheetId.Value,
            BuildSources(settings, spreadsheetId));

        Renderer.Data(result,
            console =>
            {
                console.MarkupLine($"[green]✓[/] service account: [blue]{Markup.Escape(result.ServiceAccountEmail)}[/]");
                console.MarkupLine(!string.IsNullOrEmpty(result.SpreadsheetId)
                    ? $"[green]✓[/] spreadsheet accessible: [blue]{Markup.Escape(result.SpreadsheetId)}[/]"
                    : "[yellow]![/] spreadsheet not provided, add [yellow]--spreadsheet-id <ID>[/]");
                RenderSources(console, result.Sources);
            }, settings.Format);

        return ExitCodes.Success;
    }

    private static SourceDiagnostics BuildSources(GlobalSettings settings, ResolvedValue spreadsheetId)
    {
        var resolver = settings.Resolver;
        var sheet = settings.ResolveSheetName();
        var credentials = settings.ResolveCredentialsPath();

        var projectPath = resolver.ProjectConfig?.FilePath
                          ?? ConfigLocator.DefaultProjectPath(Directory.GetCurrentDirectory());

        return new SourceDiagnostics(
            new ValueDiagnostic(spreadsheetId.Value, spreadsheetId.Source),
            new ValueDiagnostic(sheet.Value, sheet.Source),
            new ValueDiagnostic(credentials.Value, credentials.Source),
            new ConfigFileDiagnostic(projectPath, resolver.ProjectConfig is not null),
            new ConfigFileDiagnostic(resolver.GlobalConfig.FilePath, resolver.GlobalConfig.Exists));
    }

    private static void RenderSources(IAnsiConsole console, SourceDiagnostics sources)
    {
        console.MarkupLine("sources:");
        RenderValue(console, "spreadsheet-id", sources.SpreadsheetId);
        RenderValue(console, "sheet", sources.Sheet);
        RenderValue(console, "credentials", sources.Credentials);
        RenderFile(console, "project config", sources.ProjectConfig);
        RenderFile(console, "global config", sources.GlobalConfig);
    }

    private static void RenderValue(IAnsiConsole console, string name, ValueDiagnostic value) =>
        console.MarkupLine(value.Source == ValueSource.None
            ? $"  {name} [grey](not set)[/]"
            : $"  {name} ← {value.Source.Label()}: [blue]{Markup.Escape(value.Value ?? string.Empty)}[/]");

    private static void RenderFile(IAnsiConsole console, string label, ConfigFileDiagnostic file) =>
        console.MarkupLine($"  {label}: {Markup.Escape(file.Path)} " +
                           (file.Found ? "[green](found)[/]" : "[grey](not found)[/]"));

    private record ValueDiagnostic(string? Value, ValueSource Source);

    private record ConfigFileDiagnostic(string Path, bool Found);

    private record SourceDiagnostics(
        ValueDiagnostic SpreadsheetId,
        ValueDiagnostic Sheet,
        ValueDiagnostic Credentials,
        ConfigFileDiagnostic ProjectConfig,
        ConfigFileDiagnostic GlobalConfig);

    private record Payload(
        string ServiceAccountEmail,
        string? SpreadsheetId,
        SourceDiagnostics Sources,
        string Status = "OK");
}
