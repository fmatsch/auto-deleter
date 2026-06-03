using System.ServiceProcess;
using System.Timers;
using Timer = System.Timers.Timer;

namespace AutoDeleter;

public class AutoDeleterService : ServiceBase
{
    private Timer? _timer;

    public AutoDeleterService()
    {
        ServiceName = ServiceManager.ServiceName;
        CanStop = true;
        CanPauseAndContinue = false;
        AutoLog = true;
    }

    protected override void OnStart(string[] args)
    {
        _timer = new Timer(60_000); // tick every minute
        _timer.Elapsed += OnTick;
        _timer.AutoReset = true;
        _timer.Start();
    }

    protected override void OnStop()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    private static void OnTick(object? sender, ElapsedEventArgs e)
    {
        // Reload config every tick so GUI changes are picked up without restart
        var config = AppConfig.Load();
        DeleteWorker.CheckAndDeleteDue(config);
    }
}
