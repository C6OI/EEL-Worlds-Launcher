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
    }

    void CloseButton_OnClick(object? sender, RoutedEventArgs e) => Close();

    void MinimizeButton_OnClick(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    void Header_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        if (e.Pointer.IsPrimary) BeginMoveDrag(e);
    }

    void MinimizeButton_OnPointerLeave(object? sender, PointerEventArgs e) =>
        ((Button)sender!).ChangeSvgContent("avares://EELauncher/Assets/Minimize_Normal.svg");
        
    void MinimizeButton_OnPointerEnter(object? sender, PointerEventArgs e) =>
        ((Button)sender!).ChangeSvgContent("avares://EELauncher/Assets/Minimize_Pressed.svg");
        
    void CloseButton_OnPointerEnter(object? sender, PointerEventArgs e) =>
        ((Button)sender!).ChangeSvgContent("avares://EELauncher/Assets/Close_Pressed.svg");

    void CloseButton_OnPointerLeave(object? sender, PointerEventArgs e) =>
        ((Button)sender!).ChangeSvgContent("avares://EELauncher/Assets/Close_Normal.svg");
}

