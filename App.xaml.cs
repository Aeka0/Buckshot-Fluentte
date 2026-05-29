using System.Windows;
using Wpf.Ui.Appearance;

namespace BuckshotFluentte;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        ApplicationThemeManager.ApplySystemTheme();

        base.OnStartup(e);

        MainWindow = new MainWindow();
        MainWindow.Show();
    }
}
