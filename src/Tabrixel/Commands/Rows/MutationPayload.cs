using Spectre.Console;
using Tabrixel.Sheets;

namespace Tabrixel.Commands.Rows;

/// <summary>
/// Shared JSON payload of rows update/delete (and the upsert update path):
/// Matched counts the --where hits, Affected the rows actually selected for the
/// write (0 on no match, less than Matched under --first). Rows carries the
/// per-row receipts, capped at MutationReceipt.MaxRows; Truncated/Returned are
/// always present so the schema is stable for parsers.
/// </summary>
public record MutationPayload<TRow>(
    int Matched, int Affected, bool DryRun, IReadOnlyList<TRow> Rows, bool Truncated)
{
    public int Returned => Rows.Count;
}

/// <summary>
/// Update receipt: Row is the 1-based sheet row (header is row 1, as in the
/// Sheets UI); Before/After hold the assigned + --where key columns only, not
/// the full record (token economy on wide tables).
/// </summary>
public sealed record UpdatedRowReceipt(
    int Row,
    IReadOnlyDictionary<string, string> Before,
    IReadOnlyDictionary<string, string> After);

/// <summary>
/// Delete receipt: Data is the full record (rows list shape) — the response is
/// the only place an irreversibly deleted row can be restored from.
/// </summary>
public sealed record DeletedRowReceipt(int Row, IReadOnlyDictionary<string, string> Data);

public static class MutationReceipt
{
    /// <summary>
    /// Receipts past this count are cut from the payload so mass mutations
    /// (--all over hundreds of rows) cannot blow up the response.
    /// </summary>
    public const int MaxRows = 50;

    /// <summary>
    /// Update receipts for the selected snapshot rows, capped at MaxRows.
    /// Each receipt covers the union of the assigned columns and the --where
    /// key columns, in header column order: Before from the snapshot, After
    /// with the assignments applied. Snapshot index i is sheet row i + 1.
    /// </summary>
    public static IReadOnlyList<UpdatedRowReceipt> BuildUpdateReceipts(
        SheetHeader header,
        IReadOnlyList<IReadOnlyList<string?>> values,
        IReadOnlyList<int> selected,
        IReadOnlyDictionary<int, string> assignmentsByColumnIndex,
        IEnumerable<WhereCondition> conditions)
    {
        var indices = assignmentsByColumnIndex.Keys
            .Concat(conditions.Select(c => header.FindColumn(c.ColumnName).Index))
            .ToHashSet();
        var columns = header.Columns.Where(c => indices.Contains(c.Index)).ToList();

        return selected
            .Take(MaxRows)
            .Select(i =>
            {
                var row = values[i];
                var before = new Dictionary<string, string>(columns.Count);
                var after = new Dictionary<string, string>(columns.Count);
                foreach (var column in columns)
                {
                    var cell = column.Index < row.Count
                        ? row[column.Index] ?? string.Empty
                        : string.Empty;
                    before[column.Name] = cell;
                    after[column.Name] =
                        assignmentsByColumnIndex.TryGetValue(column.Index, out var assigned)
                            ? assigned
                            : cell;
                }

                return new UpdatedRowReceipt(i + 1, before, after);
            })
            .ToList();
    }

    /// <summary>
    /// Delete receipts (full records) for the selected snapshot rows, capped
    /// at MaxRows. Snapshot index i is sheet row i + 1 — the number the row
    /// had before the deletion shifted anything.
    /// </summary>
    public static IReadOnlyList<DeletedRowReceipt> BuildDeleteReceipts(
        SheetHeader header,
        IReadOnlyList<IReadOnlyList<string?>> values,
        IReadOnlyList<int> selected)
    {
        var capped = selected.Take(MaxRows).ToList();
        var records = RecordReader.Read(header, values, capped);
        return capped
            .Select((rowIndex, i) => new DeletedRowReceipt(rowIndex + 1, records[i]))
            .ToList();
    }

    /// <summary>
    /// Per-row text summary of the update receipt: one column per receipt
    /// column, changed cells as "before → after", unchanged (the --where keys)
    /// as-is. Shared by rows update and the upsert update path.
    /// </summary>
    public static void RenderUpdateReceipts(
        IAnsiConsole console, IReadOnlyList<UpdatedRowReceipt> receipts, int affected)
    {
        var table = new Table();
        table.AddColumn("Row");
        var columnNames = receipts[0].Before.Keys.ToList();
        foreach (var name in columnNames)
        {
            table.AddColumn(Markup.Escape(name));
        }

        foreach (var receipt in receipts)
        {
            var cells = new List<string> { receipt.Row.ToString() };
            foreach (var name in columnNames)
            {
                var before = receipt.Before[name];
                var after = receipt.After[name];
                cells.Add(before == after
                    ? Markup.Escape(before)
                    : $"{Markup.Escape(before)} → {Markup.Escape(after)}");
            }

            table.AddRow(cells.ToArray());
        }

        console.Write(table);
        if (receipts.Count < affected)
        {
            console.WriteLine($"Showing first {receipts.Count} of {affected} affected row(s).");
        }
    }
}
