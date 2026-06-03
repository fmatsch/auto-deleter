namespace AutoDeleter;

public static class DeleteWorker
{
    public static (int deleted, int skipped) DeleteFolderContents(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return (0, 0);

        int deleted = 0, skipped = 0;

        foreach (var file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
        {
            try
            {
                if (IsFileLocked(file)) { skipped++; continue; }
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
                deleted++;
            }
            catch { skipped++; }
        }

        // Remove empty subdirectories bottom-up
        foreach (var dir in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories)
                     .OrderByDescending(d => d.Length))
        {
            try
            {
                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                    Directory.Delete(dir);
            }
            catch { }
        }

        return (deleted, skipped);
    }

    public static void CheckAndDeleteDue(AppConfig config)
    {
        bool changed = false;
        var now = DateTime.Now;

        foreach (var entry in config.Folders)
        {
            if (!ShouldRun(entry, now)) continue;
            DeleteFolderContents(entry.Path);
            entry.LastRun = now;
            changed = true;
        }

        if (changed) config.Save();
    }

    private static bool ShouldRun(FolderEntry entry, DateTime now) => entry.Interval switch
    {
        DeleteInterval.Hourly => (now - entry.LastRun).TotalHours >= 1,
        DeleteInterval.Daily  => (now - entry.LastRun).TotalDays  >= 1,
        DeleteInterval.Weekly => (now - entry.LastRun).TotalDays  >= 7,
        _                     => false,
    };

    private static bool IsFileLocked(string path)
    {
        try
        {
            using var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch (IOException)              { return true; }
        catch (UnauthorizedAccessException) { return true; }
    }
}
