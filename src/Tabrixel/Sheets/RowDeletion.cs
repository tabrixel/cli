namespace Tabrixel.Sheets;

/// <summary>
/// Half-open grid row range for deletion: Start inclusive, End exclusive
/// (as in DeleteDimension). 0-based grid rows: header is 0, first record is 1.
/// </summary>
public sealed record GridRowRange(int Start, int End);

public static class RowDeletion
{
    /// <summary>
    /// Ranges are ordered bottom-up so deleting one range does not shift the
    /// addressing of the ranges not yet deleted.
    /// </summary>
    public static IReadOnlyList<GridRowRange> ToRanges(IReadOnlyList<int> rowIndices) =>
        rowIndices
            .Distinct()
            .OrderByDescending(i => i)
            .Select(i => new GridRowRange(i, i + 1))
            .ToList();
}
