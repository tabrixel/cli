using System.Globalization;
using System.Text.Json;
using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

/// <summary>
/// Parses the --json value for rows add: a JSON object mapping column names to
/// values becomes a cell row laid out by header column indices. Strict mode:
/// shape errors (invalid JSON, non-object, empty object, duplicate field, nested
/// values) → InvalidArguments; an unknown field name → ColumnNotFound.
/// </summary>
public static class JsonRecordParser
{
    /// <summary>
    /// The result has one cell per header column: fields missing from the JSON
    /// stay empty. Values are stringified; numbers never use scientific notation.
    /// </summary>
    public static IReadOnlyList<string> Parse(string json, SheetHeader header)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            throw Invalid("not a valid JSON document");
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw Invalid($"expected a JSON object of column values, got {Describe(root.ValueKind)}");
            }

            // Header column indices are contiguous from zero (HeaderParser rejects holes).
            var row = new string[header.Columns.Count];
            Array.Fill(row, string.Empty);

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in root.EnumerateObject())
            {
                if (!seen.Add(property.Name))
                {
                    throw Invalid($"duplicate field '{property.Name}'");
                }

                var column = header.FindColumn(property.Name);
                row[column.Index] = ToCellValue(property.Name, property.Value);
            }

            if (seen.Count == 0)
            {
                throw Invalid("the record has no fields");
            }

            return row;
        }
    }

    /// <summary>
    /// Scalars only: string as-is, null → empty cell, booleans → JSON literals.
    /// Nested objects/arrays are an error: the model is flat, a cell is a string.
    /// </summary>
    private static string ToCellValue(string fieldName, JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Number => NumberToString(fieldName, value),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null => string.Empty,
        _ => throw Invalid(
            $"field '{fieldName}' has {Describe(value.ValueKind)} value; cell values must be scalars"),
    };

    /// <summary>
    /// Invariant culture and never scientific notation (the raw JSON text is not
    /// good enough: '1e3' must become '1000').
    /// </summary>
    private static string NumberToString(string fieldName, JsonElement value)
    {
        if (value.TryGetInt64(out var integer))
        {
            return integer.ToString(CultureInfo.InvariantCulture);
        }

        if (value.TryGetDecimal(out var dec))
        {
            // decimal.ToString never uses an exponent.
            return dec.ToString(CultureInfo.InvariantCulture);
        }

        var dbl = value.GetDouble();
        if (!double.IsFinite(dbl))
        {
            throw Invalid($"number in field '{fieldName}' is out of range");
        }

        // Fixed-point format without an exponent (17 significant fraction digits).
        return dbl.ToString("0.#################", CultureInfo.InvariantCulture);
    }

    private static string Describe(JsonValueKind kind) => kind switch
    {
        JsonValueKind.Object => "an object",
        JsonValueKind.Array => "an array",
        JsonValueKind.String => "a string",
        JsonValueKind.Number => "a number",
        JsonValueKind.True or JsonValueKind.False => "a boolean",
        JsonValueKind.Null => "null",
        _ => kind.ToString().ToLowerInvariant(),
    };

    private static CliException Invalid(string reason) => new(ErrorCode.InvalidArguments,
        $"invalid --json value: {reason}; expected a JSON object like '{{\"Column\":\"value\"}}'");
}
