using System.Windows;

namespace PostizInstaller;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}
