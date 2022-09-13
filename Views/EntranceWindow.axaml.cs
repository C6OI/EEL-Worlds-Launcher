using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using EELauncher.Extensions;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace EELauncher.Views; 

public partial class EntranceWindow : Window {
    readonly IAssetLoader _assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!; 
    
    public EntranceWindow() {
        InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
    }

    void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
        
        ClientSize = new Size(960, 540);
    }

    void OnInitialized(object? sender, EventArgs e) {
        Background = WindowExtensions.RandomBackground();
    }

    void Header_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        if (e.Pointer.IsPrimary)
            BeginMoveDrag(e);
    }

    void MinimizeButton_OnClick(object? sender, RoutedEventArgs e) {
        WindowState = WindowState.Minimized;
    }

    void MinimizeButton_OnPointerLeave(object? sender, PointerEventArgs e) {
        Button button = (Button)sender!;

        Image enterButtonImage = new() {
            Source = new Bitmap(_assets.Open(new Uri(@"avares://EELauncher/Assets/Minimize_Normal.png")))
        };

        button.Content = enterButtonImage;
    }

    void MinimizeButton_OnPointerEnter(object? sender, PointerEventArgs e) {
        Button button = (Button)sender!;

        Image enterButtonImage = new() {
            Source = new Bitmap(_assets.Open(new Uri(@"avares://EELauncher/Assets/Minimize_Pressed.png")))
        };

        button.Content = enterButtonImage;
    }

    void CloseButton_OnClick(object? sender, RoutedEventArgs e) {
        Close();
    }

    void CloseButton_OnPointerEnter(object? sender, PointerEventArgs e) {
        Button button = (Button)sender!;

        Image enterButtonImage = new() {
            Source = new Bitmap(_assets.Open(new Uri(@"avares://EELauncher/Assets/Close_Pressed.png")))
        };

        button.Content = enterButtonImage;
    }

    void CloseButton_OnPointerLeave(object? sender, PointerEventArgs e) {
        Button button = (Button)sender!;

        Image enterButtonImage = new() {
            Source = new Bitmap(_assets.Open(new Uri(@"avares://EELauncher/Assets/Close_Normal.png")))
        };

        button.Content = enterButtonImage;
    }

    void NicknameField_OnGotFocus(object? sender, GotFocusEventArgs e) {
        ((TextBox)sender!).RemovePlaceholder("Nickname");
    }

    void NicknameField_OnLostFocus(object? sender, RoutedEventArgs e) {
        ((TextBox)sender!).AddPlaceholder("Nickname");
    }

    void PasswordField_OnGotFocus(object? sender, GotFocusEventArgs e) {
        ((TextBox)sender!).RemovePlaceholder("Password");
    }

    void PasswordField_OnLostFocus(object? sender, RoutedEventArgs e) {
        ((TextBox)sender!).AddPlaceholder("Password");
    }
    
    void LoginButton_OnClick(object? sender, RoutedEventArgs e) {
        MainWindow launcher = new();
        launcher.Show();
        Close();
    }

    void ForgorButton_OnClick(object? sender, RoutedEventArgs e) {
        MessageBoxStandardParams mBoxParams = new() {
            ContentTitle = "жалк",
            ContentMessage = "вспоминай",
            WindowIcon = Icon,
            Icon = MessageBox.Avalonia.Enums.Icon.Question,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
            
        IMsBoxWindow<ButtonResult>? mBox = MessageBoxManager.GetMessageBoxStandardWindow(mBoxParams);

        mBox.Show();
    }
}

