using System.ServiceProcess;
using System.Windows.Forms;

namespace AutoDeleter;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // When registered as a Windows Service, sc.exe launches us with --service
        if (args.Length > 0 && args[0] == "--service")
        {
            ServiceBase.Run(new AutoDeleterService());
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
