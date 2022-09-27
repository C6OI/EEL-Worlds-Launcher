using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using EELauncher.Data;
using EELauncher.Extensions;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using Newtonsoft.Json;

namespace EELauncher.Views; 

public partial class EntranceWindow : Window {
    const string Nickname = "Никнейм";
    const string Password = "Пароль";
    readonly MessageBoxStandardParams _mBoxParams;
    
    public EntranceWindow() {
        _mBoxParams = new MessageBoxStandardParams {
            ContentTitle = "Ошибка",
            WindowIcon = Icon,
            Icon = MessageBox.Avalonia.Enums.Icon.Question,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        
        InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
    }

    void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
        WireControls();
        
        ClientSize = new Size(960, 540);
        LauncherName.Text = Tag!.ToString();
        NicknameField.Text = Nickname;
        PasswordField.Text = Password;
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
    
    void OnInitialized(object? s, EventArgs e) => Background = WindowExtensions.RandomBackground();

    void OnActivated(object? s, EventArgs e) => NicknameField.Focus();
    
    void OnKeyDown(object? s, KeyEventArgs e) { if (e.Key == (Key.Enter | Key.Return)) LoginButton_OnClick(this, new RoutedEventArgs()); }
    
    void Header_OnPointerPressed(object? s, PointerPressedEventArgs e) { if (e.Pointer.IsPrimary) BeginMoveDrag(e); }

    void MinimizeButton_OnClick(object? s, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    void MinimizeButton_OnPointerEnter(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Minimize_Pressed.svg");
    
    void MinimizeButton_OnPointerLeave(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Minimize_Normal.svg");

    void CloseButton_OnClick(object? s, RoutedEventArgs e) => Close();
    
    void CloseButton_OnPointerEnter(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Close_Pressed.svg");

    void CloseButton_OnPointerLeave(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Close_Normal.svg");

    void NicknameField_OnGotFocus(object? s, GotFocusEventArgs e) => ((TextBox)s!).RemovePlaceholder(Nickname, false);

    void NicknameField_OnLostFocus(object? s, RoutedEventArgs e) => ((TextBox)s!).AddPlaceholder(Nickname, false);

    void PasswordField_OnGotFocus(object? s, GotFocusEventArgs e) => ((TextBox)s!).RemovePlaceholder(Password, true);

    void PasswordField_OnLostFocus(object? s, RoutedEventArgs e) => ((TextBox)s!).AddPlaceholder(Password, true);

    void LoginButton_OnClick(object? s, RoutedEventArgs e) {
        if (NicknameField?.Text is null or Nickname || PasswordField?.Text is null or Password) {
            _mBoxParams.ContentMessage = "Заполните все поля";
            
            MessageBoxManager.GetMessageBoxStandardWindow(_mBoxParams).Show();
            return;
        }
        
        List<KeyValuePair<string, string>> authData = new() {
            KeyValuePair.Create<string, string>("username", NicknameField.Text),
            KeyValuePair.Create<string, string>("password", PasswordField.Text),
            KeyValuePair.Create<string, string>("clientToken", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            KeyValuePair.Create<string, string>("requestUser", "true")
        };

        StaticData.Data = JsonConvert.DeserializeObject<ElybyAuthData>(UrlExtensions.PostRequest("https://authserver.ely.by/auth/authenticate", authData));

        if (StaticData.Data.Equals(default(ElybyAuthData))) {
            _mBoxParams.ContentMessage = "Неверные данные";

            MessageBoxManager.GetMessageBoxStandardWindow(_mBoxParams).Show();
            return;
        }
        
        StaticData.Password = PasswordField.Text;
        
        new MainWindow().Show();
        Close();
    }

    void ForgotButton_OnClick(object? s, RoutedEventArgs e) => "https://account.ely.by/forgot-password".OpenUrl();

    void NotRegistered_OnClick(object? s, RoutedEventArgs e) => "https://account.ely.by/register".OpenUrl();

    void OnClosing(object? s, CancelEventArgs e) {
        if (!StaticData.Data.Equals(default(ElybyAuthData))) return;
        try { Program.ReleaseMemory(); } finally { Environment.Exit(0); }
    }
}

