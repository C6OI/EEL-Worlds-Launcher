using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EELauncher.ViewModels;
using EELauncher.Views;

namespace EELauncher
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // main window changed
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new EntranceWindow {
                    DataContext = new EntranceWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}