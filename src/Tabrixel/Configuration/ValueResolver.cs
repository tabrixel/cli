namespace Tabrixel.Configuration;

public sealed record ResolvedValue(string? Value, ValueSource Source)
{
    public bool IsSet => Source != ValueSource.None;
}

/// <summary>
/// Implements the precedence chain: flag → env → project config → global config.
/// Presence at a level is value being non-null; values are trimmed, and an empty
/// value still stops the chain (deliberate override).
/// </summary>
public sealed class ValueResolver
{
    /// <summary>Null when no .tabrixel/config.toml was found up the tree.</summary>
    public ConfigFile? ProjectConfig { get; }

    public ConfigFile GlobalConfig { get; }

    private ValueResolver(ConfigFile? projectConfig, ConfigFile globalConfig)
    {
        ProjectConfig = projectConfig;
        GlobalConfig = globalConfig;
    }

    public static ValueResolver Create()
    {
        var projectPath = ConfigLocator.FindProjectPath(Directory.GetCurrentDirectory());

        return new ValueResolver(
            projectPath is null ? null : ConfigFile.Load(projectPath),
            ConfigFile.Load(ConfigLocator.GlobalPath));
    }

    public ResolvedValue Resolve(string configKey, string? environmentVariable,
        string? flagValue)
    {
        if (flagValue is not null)
        {
            return new ResolvedValue(flagValue.Trim(), ValueSource.Flag);
        }

        if (environmentVariable is not null &&
            Environment.GetEnvironmentVariable(environmentVariable) is { } env)
        {
            return new ResolvedValue(env.Trim(), ValueSource.Env);
        }

        return ResolveConfig(configKey);
    }

    /// <summary>Config-level resolution only: project beats global, per key.</summary>
    public ResolvedValue ResolveConfig(string configKey)
    {
        if (ProjectConfig?.GetValue(configKey) is { } project)
        {
            return new ResolvedValue(project.Trim(), ValueSource.ProjectConfig);
        }

        if (GlobalConfig.GetValue(configKey) is { } global)
        {
            return new ResolvedValue(global.Trim(), ValueSource.GlobalConfig);
        }

        return new ResolvedValue(null, ValueSource.None);
    }
}
