using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Tabrixel.Configuration;
using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

public static class SheetsServiceFactory
{
    public static SheetsClient Create(GlobalSettings settings)
    {
        // The resolved path is always handed to the SDK explicitly; the SDK is
        // never left to read GOOGLE_APPLICATION_CREDENTIALS on its own.
        var resolved = settings.ResolveCredentialsPath();
        var path = resolved.Value;

        if (path is null)
        {
            throw new CliException(ErrorCode.AuthFailed,
                "service account JSON key path is not set: pass --credentials, " +
                $"set the {Consts.EnvGoogleCredentials} environment variable, " +
                "or use 'config set credentials <path>'");
        }

        if (!File.Exists(path))
        {
            throw new CliException(ErrorCode.AuthFailed,
                $"file not found: {path}",
                new Dictionary<string, object?>
                {
                    ["path"] = path,
                    ["source"] = resolved.Source.Label()
                });
        }

        ServiceAccountCredential serviceAccount;
        try
        {
            serviceAccount = CredentialFactory.FromFile<ServiceAccountCredential>(path);
        }
        catch (Exception ex)
        {
            throw new CliException(ErrorCode.AuthFailed,
                $"failed to load the service account key: {ex.Message}",
                new Dictionary<string, object?> { ["path"] = path });
        }

        var scoped = serviceAccount.ToGoogleCredential()
            .CreateScoped(SheetsService.ScopeConstants.Spreadsheets);

        var service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = scoped,
            ApplicationName = $"{Consts.ApplicationName} {Consts.ApplicationVersion}"
        });

        return new SheetsClient(service, scoped, serviceAccount.Id);
    }
}
