using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using EELauncher.Extensions;

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
        WireControls();
        
        ClientSize = new Size(720, 480);
        LauncherName.Text = Tag!.ToString();
    }

    void WireControls() {
        Header = this.FindControl<Grid>("Header");
        LauncherName = this.FindControl<TextBlock>("LauncherName");
        CloseButton = this.FindControl<Button>("CloseButton");
    }

    void CloseButton_OnClick(object? s, RoutedEventArgs e) => Close();

    void MinimizeButton_OnClick(object? s, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    void Header_OnPointerPressed(object? s, PointerPressedEventArgs e) { if (e.Pointer.IsPrimary) BeginMoveDrag(e); }

    void MinimizeButton_OnPointerLeave(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Minimize_Normal.svg");
        
    void MinimizeButton_OnPointerEnter(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Minimize_Pressed.svg");
        
    void CloseButton_OnPointerEnter(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Close_Pressed.svg");

    void CloseButton_OnPointerLeave(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Close_Normal.svg");
}

