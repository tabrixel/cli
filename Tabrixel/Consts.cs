namespace Tabrixel;

public static class Consts
{
    public static readonly string ApplicationName = "tbxl";
    public static readonly string ApplicationVersion = "0.3.1";
    
    public static readonly string EnvVariablePrefix = "TBXL_";

    // Standard Google ADC variable, intentionally not TBXL_-prefixed.
    public static readonly string EnvGoogleCredentials = "GOOGLE_APPLICATION_CREDENTIALS";

    public static readonly string EnvSpreadsheetId = EnvVariablePrefix + "SPREADSHEET_ID";
    public static readonly string EnvSheet = EnvVariablePrefix + "SHEET";
}
