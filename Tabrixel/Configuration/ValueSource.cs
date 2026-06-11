namespace Tabrixel.Configuration;

public enum ValueSource
{
    None,
    Argument,
    Flag,
    Env,
    ProjectConfig,
    GlobalConfig
}

public static class ValueSourceExtensions
{
    public static string Label(this ValueSource source) => source switch
    {
        ValueSource.Argument => "argument",
        ValueSource.Flag => "flag",
        ValueSource.Env => "env",
        ValueSource.ProjectConfig => "project config",
        ValueSource.GlobalConfig => "global config",
        _ => "none"
    };
}
