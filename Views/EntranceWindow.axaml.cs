using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using EELauncher.Data;
using EELauncher.Extensions;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using Newtonsoft.Json;

namespace EELauncher.Views; 

public partial class EntranceWindow : Window {
    public EntranceWindow() {
        InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
    }

    void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
        WireControls();
        
        ClientSize = new Size(960, 540);
        NicknameField.Text = "Nickname";
        PasswordField.Text = "Password";
    }

    void WireControls() {
        Header = this.FindControl<Grid>("Header");
        LauncherName = this.FindControl<TextBlock>("LauncherName");
        MinimizeButton = this.FindControl<Button>("MinimizeButton");
        CloseButton = this.FindControl<Button>("CloseButton");
        Login = this.FindControl<Grid>("Login");
        NicknameField = this.FindControl<TextBox>("NicknameField");
        PasswordField = this.FindControl<TextBox>("PasswordField");
        LoginButton = this.FindControl<Button>("LoginButton");
        ForgotButton = this.FindControl<Button>("ForgotButton");
    }

    readonly IAssetLoader _assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
    
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
        MessageBoxStandardParams mBoxParams = new() {
            WindowIcon = Icon,
            Icon = MessageBox.Avalonia.Enums.Icon.Question,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        
        if (NicknameField?.Text is null or "Nickname" || PasswordField?.Text is null or "Password") {
            mBoxParams.ContentTitle = "Ошибка";
            mBoxParams.ContentMessage = "Заполните все поля";
            
            MessageBoxManager.GetMessageBoxStandardWindow(mBoxParams).Show();
            return;
        }
        
        List<KeyValuePair<string, string>> authData = new() {
            KeyValuePair.Create<string, string>("username", NicknameField.Text),
            KeyValuePair.Create<string, string>("password", PasswordField.Text),
            KeyValuePair.Create<string, string>("clientToken", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            KeyValuePair.Create<string, string>("requestUser", "true")
        };

        ElybyAuthData data = JsonConvert.DeserializeObject<ElybyAuthData>(UrlExtensions.PostRequest("https://authserver.ely.by/auth/authenticate", authData));
        StaticData.Data = data;

        if (data.accessToken == null || data.selectedProfile.name == null) {
            mBoxParams.ContentTitle = "Ошибка";
            mBoxParams.ContentMessage = "Неверные данные";

            MessageBoxManager.GetMessageBoxStandardWindow(mBoxParams).Show();
            return;
        }
        
        new MainWindow().Show();
        Close();
    }

    void ForgotButton_OnClick(object? sender, RoutedEventArgs e) {
        "https://account.ely.by/forgot-password".OpenUrl();
    }

    void NotRegistered_OnClick(object? sender, RoutedEventArgs e) {
        "https://account.ely.by/register".OpenUrl();
    }
}

