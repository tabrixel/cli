using Tabrixel.Infrastructure;
using Tabrixel.Sheets;

namespace Tabrixel.Tests.Sheets;

public class RowWindowTests
{
    // Matched row indices below the header; ten matches at positions 1..10.
    private static IReadOnlyList<int> TenMatches() => Enumerable.Range(1, 10).ToList();

    [Fact]
    public void OffsetSkipsLeadingMatches()
    {
        // 3.1: --offset 3 without an effective limit skips the first 3 matches;
        // total (captured by the caller before Apply) is unaffected.
        var matched = TenMatches();

        var window = RowWindow.Apply(matched, offset: 3, limit: 100);

        Assert.Equal(new[] { 4, 5, 6, 7, 8, 9, 10 }, window);
    }

    [Fact]
    public void OffsetCombinedWithLimitPagesResult()
    {
        // 3.2: --offset 2 --limit 3 returns the 3rd, 4th, and 5th matches.
        var window = RowWindow.Apply(TenMatches(), offset: 2, limit: 3);

        Assert.Equal(new[] { 3, 4, 5 }, window);
    }

    [Theory]
    [InlineData(4)] // exactly at the matched count
    [InlineData(7)] // beyond the matched count
    public void OffsetAtOrBeyondMatchesYieldsEmpty(int offset)
    {
        // 3.3: an offset at or past the matched count produces no records.
        var matched = Enumerable.Range(1, 4).ToList();

        var window = RowWindow.Apply(matched, offset, limit: 100);

        Assert.Empty(window);
    }

    [Fact]
    public void DefaultOffsetMatchesLimitOnlyBehavior()
    {
        // 3.4: offset 0 is identical to applying the limit alone.
        var matched = TenMatches();

        var window = RowWindow.Apply(matched, offset: 0, limit: 100);

        Assert.Equal(matched, window);
    }

    [Fact]
    public void NegativeOffsetFailsValidation()
    {
        // 3.5: --offset -1 fails with InvalidArguments.
        var error = Assert.Throws<CliException>(() => RowWindow.Validate(offset: -1, limit: 100));

        Assert.Equal(ErrorCode.InvalidArguments, error.Error.Code);
        Assert.Contains("--offset", error.Message);
    }

    [Fact]
    public void NonPositiveLimitFailsValidation()
    {
        var error = Assert.Throws<CliException>(() => RowWindow.Validate(offset: 0, limit: 0));

        Assert.Equal(ErrorCode.InvalidArguments, error.Error.Code);
        Assert.Contains("--limit", error.Message);
    }
}
