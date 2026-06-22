namespace Tabrixel.Configuration;

public static class ConfigLocator
{
    public const string DirectoryName = ".tabrixel";
    public const string FileName = "config.toml";

    public static string GlobalPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            DirectoryName, FileName);

    /// <summary>
    /// Finds the project config by walking up from <paramref name="startDirectory"/>,
    /// like .git discovery; the first existing .tabrixel/config.toml wins.
    /// The global config file is never treated as a project one, even when the
    /// walk passes through the home directory.
    /// </summary>
    public static string? FindProjectPath(string startDirectory)
    {
        var globalPath = Path.GetFullPath(GlobalPath);
        var comparison = OperatingSystem.IsLinux()
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        for (var dir = new DirectoryInfo(startDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, DirectoryName, FileName);
            if (File.Exists(candidate) &&
                !string.Equals(Path.GetFullPath(candidate), globalPath, comparison))
            {
                return candidate;
            }
        }

        return null;
    }

    public static string DefaultProjectPath(string workingDirectory) =>
        Path.Combine(workingDirectory, DirectoryName, FileName);
}
