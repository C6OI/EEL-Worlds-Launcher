using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EELauncher.Views; 

public partial class SettingsWindow : Window {
    public SettingsWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}

