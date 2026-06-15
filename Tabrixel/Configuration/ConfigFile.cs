using System.Globalization;
using Tabrixel.Infrastructure;
using Tomlyn;
using Tomlyn.Model;

namespace Tabrixel.Configuration;

/// <summary>
/// One config.toml: unknown keys round-trip through load/save untouched;
/// comments and formatting are not preserved (accepted trade-off).
/// </summary>
public sealed class ConfigFile
{
    private readonly TomlTable _table;

    public string FilePath { get; }
    public bool Exists { get; }

    private ConfigFile(string filePath, bool exists, TomlTable table)
    {
        FilePath = filePath;
        Exists = exists;
        _table = table;
    }

    public static ConfigFile Load(string path)
    {
        if (!File.Exists(path))
        {
            return new ConfigFile(path, false, []);
        }

        try
        {
            var table = TomlSerializer.Deserialize<TomlTable>(File.ReadAllText(path),
                new TomlSerializerOptions { SourceName = path })!;

            return new ConfigFile(path, true, table);
        }
        catch (TomlException ex)
        {
            // A broken config must fail loudly, not act as an absent one.
            throw new CliException(ErrorCode.InvalidArguments,
                $"failed to parse {path}: {ex.Message}",
                new Dictionary<string, object?> { ["path"] = path });
        }
    }

    /// <summary>Null means the key is absent; an empty string is a present value.</summary>
    public string? GetValue(string key) =>
        _table.TryGetValue(key, out var value)
            ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
            : null;

    public void SetValue(string key, string value) => _table[key] = value;

    public void Save()
    {
        try
        {
            if (Path.GetDirectoryName(FilePath) is { Length: > 0 } directory)
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(FilePath, TomlSerializer.Serialize(_table));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A write failure (no permission, full disk, read-only path) is an
            // expected, user-facing error — surface it as such, not as Internal.
            throw new CliException(ErrorCode.IOError,
                $"failed to write config to {FilePath}: {ex.Message}",
                new Dictionary<string, object?> { ["path"] = FilePath });
        }
    }
}
