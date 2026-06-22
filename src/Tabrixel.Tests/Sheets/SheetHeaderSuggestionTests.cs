using Tabrixel.Infrastructure;
using Tabrixel.Sheets;

namespace Tabrixel.Tests.Sheets;

public class SheetHeaderSuggestionTests
{
    private static CliException FindColumnError(string name, params string?[] header)
    {
        var parsed = HeaderParser.Parse(header);
        return Assert.Throws<CliException>(() => parsed.FindColumn(name));
    }

    private static string? DidYouMean(CliException exception) =>
        exception.Error.Details?.GetValueOrDefault("did_you_mean") as string;

    [Fact]
    public void ExactMatchReturnsColumn()
    {
        var header = HeaderParser.Parse(["Name", "Status"]);

        var column = header.FindColumn("Status");

        Assert.Equal("Status", column.Name);
        Assert.Equal(1, column.Index);
    }

    [Fact]
    public void AdjacentTranspositionIsSuggested()
    {
        var error = FindColumnError("Nmae", "Name", "Status");

        Assert.Equal(ErrorCode.ColumnNotFound, error.Error.Code);
        Assert.Equal("Name", DidYouMean(error));
        Assert.Contains("did you mean 'Name'?", error.Message);
    }

    [Fact]
    public void SingleEditTypoIsSuggested()
    {
        var error = FindColumnError("Nam", "Name", "Status");

        Assert.Equal(ErrorCode.ColumnNotFound, error.Error.Code);
        Assert.Equal("Name", DidYouMean(error));
    }

    [Fact]
    public void CaseInsensitiveMatchIsSuggested()
    {
        var error = FindColumnError("name", "Name", "Status");

        Assert.Equal(ErrorCode.ColumnNotFound, error.Error.Code);
        Assert.Equal("Name", DidYouMean(error));
    }

    [Fact]
    public void UnrelatedNameGetsNoSuggestion()
    {
        var error = FindColumnError("Frobnicate", "Name", "Status");

        Assert.Equal(ErrorCode.ColumnNotFound, error.Error.Code);
        Assert.Null(DidYouMean(error));
        Assert.DoesNotContain("did you mean", error.Message);
    }

    [Fact]
    public void DistanceBeyondThresholdGetsNoSuggestion()
    {
        // OSA(Nmea, Name) = 2: the rotation is not a single adjacent swap.
        var error = FindColumnError("Nmea", "Name");

        Assert.Equal(ErrorCode.ColumnNotFound, error.Error.Code);
        Assert.Null(DidYouMean(error));
    }

    [Fact]
    public void TieIsBrokenByHeaderOrder()
    {
        // Both at OSA distance 1 from "Nme"; the leftmost column wins.
        var error = FindColumnError("Nme", "Name", "Nye");

        Assert.Equal(ErrorCode.ColumnNotFound, error.Error.Code);
        Assert.Equal("Name", DidYouMean(error));
    }
}
