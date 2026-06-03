using System.Diagnostics;
using System.ServiceProcess;

namespace AutoDeleter;

public static class ServiceManager
{
    public const string ServiceName = "AutoDeleter";
    private const string DisplayName = "Auto Deleter";

    public static bool IsInstalled()
    {
        try
        {
            using var sc = new ServiceController(ServiceName);
            _ = sc.Status; // throws if not installed
            return true;
        }
        catch { return false; }
    }

    public static bool IsRunning()
    {
        try
        {
            using var sc = new ServiceController(ServiceName);
            return sc.Status == ServiceControllerStatus.Running;
        }
        catch { return false; }
    }

    public static bool Install(string exePath)
    {
        // Register the service; the --service flag tells the exe to run in service mode
        string binPath = $"\"{exePath}\" --service";
        if (!RunSc($"create {ServiceName} binPath= {binPath} start= auto DisplayName= \"{DisplayName}\""))
            return false;

        // Set a description for the service
        RunSc($"description {ServiceName} \"Löscht Ordnerinhalte automatisch nach konfigurierten Intervallen.\"");

        // Start immediately
        RunSc($"start {ServiceName}");
        return true;
    }

    public static bool Uninstall()
    {
        RunSc($"stop {ServiceName}");
        return RunSc($"delete {ServiceName}");
    }

    private static bool RunSc(string args)
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo("sc", args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            })!;
            p.WaitForExit(10_000);
            return p.ExitCode == 0;
        }
        catch { return false; }
    }
}
