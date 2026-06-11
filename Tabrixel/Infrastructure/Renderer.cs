using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace Tabrixel.Infrastructure;

public static class Renderer
{
    // DictionaryKeyPolicy must stay unset: dictionary keys are data (sheet
    // column names in rows-list records and mutation receipts) and must
    // round-trip verbatim, while property names are snake_cased.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };

    private static IAnsiConsole DataConsole { get; } = AnsiConsole.Console;
    
    private static IAnsiConsole ErrorConsole { get; } = AnsiConsole.Create(new AnsiConsoleSettings
    {
        Out = new AnsiConsoleOutput(Console.Error),
    });

    public static void Data(object jsonPayload, Action<IAnsiConsole> render, OutputFormat format)
    {
        if (format == OutputFormat.Json)
        {
            var json = JsonSerializer.Serialize(jsonPayload, JsonOptions);
            DataConsole.Profile.Out.Writer.WriteLine(json);
        }
        else
        {
            render(DataConsole);
        }
    }
    
    public static void Warning(object jsonPayload, string message, OutputFormat format)
    {
        if (format == OutputFormat.Json)
        {
            var json = JsonSerializer.Serialize(jsonPayload, JsonOptions);
            ErrorConsole.Profile.Out.Writer.WriteLine(json);
        }
        else
        {
            ErrorConsole.MarkupLine($"[yellow]Warning:[/] {Markup.Escape(message)}");
        }
    }

    public static void Error(CliError error, OutputFormat format)
    {
        if (format == OutputFormat.Json)
        {
            var json = JsonSerializer.Serialize(error, JsonOptions);
            ErrorConsole.Profile.Out.Writer.WriteLine(json);
        }
        else
        {
            ErrorConsole.MarkupLine($"[red]Error ({error.Code.ToString()})[/]: {Markup.Escape(error.Message)}");;
        }
    }
}
