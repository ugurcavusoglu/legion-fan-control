using LegionFanControl.Views;

namespace LegionFanControl;

public partial class App : System.Windows.Application
{
    private TrayIcon? _trayIcon;
    private MainWindow? _mainWindow;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        _mainWindow = new MainWindow();
        _trayIcon = new TrayIcon(_mainWindow);
        _mainWindow.Show();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
