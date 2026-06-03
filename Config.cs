using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoDeleter;

public enum DeleteInterval { Hourly, Daily, Weekly }

public class FolderEntry
{
    public string Path { get; set; } = "";
    public DeleteInterval Interval { get; set; } = DeleteInterval.Daily;
    public DateTime LastRun { get; set; } = DateTime.MinValue;
}

public class AppConfig
{
    public List<FolderEntry> Folders { get; set; } = new();

    private static readonly string ConfigDir = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "AutoDeleter");

    public static string ConfigPath => System.IO.Path.Combine(ConfigDir, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
                return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigPath)) ?? new AppConfig();
        }
        catch { }
        return new AppConfig();
    }

    public void Save()
    {
        Directory.CreateDirectory(ConfigDir);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this, JsonOptions));
    }
}
