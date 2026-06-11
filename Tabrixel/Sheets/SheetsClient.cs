using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;

namespace Tabrixel.Sheets;

public sealed record SheetsClient(SheetsService Service, GoogleCredential Credential, string ServiceAccountEmail);
