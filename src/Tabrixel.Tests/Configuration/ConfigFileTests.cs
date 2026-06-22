using Tabrixel.Configuration;
using Tabrixel.Infrastructure;

namespace Tabrixel.Tests.Configuration;

public class ConfigFileTests
{
    [Fact]
    public void SaveWriteFailureThrowsIOErrorNamingPath()
    {
        // 3.1: a filesystem failure while persisting surfaces as IOError naming
        // the config path, not as an Internal error with a raw message.
        // Force the failure by placing the config under a path whose parent is a
        // regular file, so Directory.CreateDirectory cannot create the directory.
        var blocker = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        File.WriteAllText(blocker, "");
        try
        {
            var configPath = Path.Combine(blocker, "config.toml");
            var config = ConfigFile.Load(configPath);
            config.SetValue("spreadsheet-id", "1AbC");

            var error = Assert.Throws<CliException>(() => config.Save());

            Assert.Equal(ErrorCode.IOError, error.Error.Code);
            Assert.NotNull(error.Error.Details);
            Assert.Equal(configPath, error.Error.Details!["path"]);
        }
        finally
        {
            File.Delete(blocker);
        }
    }

    [Fact]
    public void SaveCreatesDirectoryAndWritesValue()
    {
        // 3.2: the happy path still creates the directory and persists the value.
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var configPath = Path.Combine(dir, "config.toml");
        try
        {
            var config = ConfigFile.Load(configPath);
            config.SetValue("spreadsheet-id", "1AbC");

            config.Save();

            Assert.True(File.Exists(configPath));
            Assert.Equal("1AbC", ConfigFile.Load(configPath).GetValue("spreadsheet-id"));
        }
        finally
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive: true);
            }
        }
    }
}
