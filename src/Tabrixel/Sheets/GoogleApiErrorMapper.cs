using System.Net;
using Google;
using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

public static class GoogleApiErrorMapper
{
    public static CliException ToSpreadsheetAccessError(
        this GoogleApiException exception,
        string spreadsheetId,
        string serviceAccountEmail) => exception.HttpStatusCode switch
    {
        HttpStatusCode.Unauthorized => new CliException(ErrorCode.AuthFailed,
            "Google API rejected the credentials: the key may be revoked or invalid"),
        HttpStatusCode.Forbidden or HttpStatusCode.NotFound => new CliException(
            ErrorCode.NotFound,
            $"spreadsheet '{spreadsheetId}' not found or not shared with the service account '{serviceAccountEmail}'",
            new Dictionary<string, object?>
            {
                ["spreadsheet_id"] = spreadsheetId,
                ["service_account"] = serviceAccountEmail,
            }),
        _ => new CliException(ErrorCode.Internal,
            "Google Sheets API request failed",
            new Dictionary<string, object?> { ["http_status"] = (int)exception.HttpStatusCode }),
    };
}
