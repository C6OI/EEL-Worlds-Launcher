using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using EELauncher.Data;
using EELauncher.Extensions;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Extensions;
using Newtonsoft.Json;
using Serilog;

namespace EELauncher.Views;

public partial class EntranceWindow : Window {
    static readonly ILogger Logger = Log.Logger.ForType<EntranceWindow>();
    readonly MessageBoxStandardParams _mBoxParams;
    bool _loginHandled;
    
    public EntranceWindow() {
        _mBoxParams = new MessageBoxStandardParams {
            ContentTitle = "Ошибка",
            WindowIcon = Icon,
            Icon = MessageBox.Avalonia.Enums.Icon.Question,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        
        Background bg = WindowExtensions.RandomBackground();

        Initialized += (_, _) => {
            Background = bg.Brush;

#if DEBUG
            Logger.Debug($"Installed new background: {bg.BrushName}");
#endif
        };
        
        Activated += (_, _) => NicknameField.Focus();

        KeyDown += (_, e) => { if (e.Key is Key.Enter or Key.Return) LoginButton_OnClick(this, new RoutedEventArgs()); };
        
        InitializeComponent();
        
        Header.PointerPressed += (_, e) => { if (e.Pointer.IsPrimary) BeginMoveDrag(e); };

        CloseButton.PointerEnter += (_, _) => CloseButton.ChangeSvgContent("Close_Pressed.svg");
        CloseButton.PointerLeave += (_, _) => CloseButton.ChangeSvgContent("Close_Normal.svg");
        CloseButton.Click += (_, _) => Close();
        
        MinimizeButton.PointerEnter += (_, _) => MinimizeButton.ChangeSvgContent("Minimize_Pressed.svg");
        MinimizeButton.PointerLeave += (_, _) => MinimizeButton.ChangeSvgContent("Minimize_Normal.svg");
        MinimizeButton.Click += (_, _) => WindowState = WindowState.Minimized;

        TogglePasswordView.Click += (_, _) => PasswordField.RevealPassword = !PasswordField.RevealPassword;
        ForgotButton.Click += (_, _) => "https://account.ely.by/forgot-password".OpenUrl();
        NotRegistered.Click += (_, _) => "https://account.ely.by/register".OpenUrl();
        
        LauncherName.Text = Tag!.ToString();
    }
    
    async void LoginButton_OnClick(object? s, RoutedEventArgs e) {
        if (_loginHandled) return;
        _loginHandled = true;
        
        if (NicknameField?.Text is null or "" || PasswordField?.Text is null or "") {
            Logger.Error("Authorization error: Nickname/Password fields are null or empty");
            
            _mBoxParams.ContentMessage = "Заполните все поля";
            await MessageBoxManager.GetMessageBoxStandardWindow(_mBoxParams).ShowDialog(this);
            
            _loginHandled = false;
            return;
        }

        Dictionary<string, string> authData = new() {
            { "username", NicknameField.Text },
            { "password", PasswordField.Text },
            { "clientToken", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            { "requestUser", "true" }
        };

        BaseData response = await UrlExtensions.JsonHttpRequest("https://authserver.ely.by/auth/authenticate", HttpMethod.Post, authData);

        if (!response.IsOk) {
            ErrorData error = JsonConvert.DeserializeObject<ErrorData>(await response.Data.ReadAsStringAsync())!;
            Logger.Error(error.ToString());
            
            _mBoxParams.ContentMessage = "Неверные данные";
            await MessageBoxManager.GetMessageBoxStandardWindow(_mBoxParams).ShowDialog(this);
            _loginHandled = false;
            return;
        }
        
        StaticData.Data = JsonConvert.DeserializeObject<ElybyAuthData>(await response.Data.ReadAsStringAsync());
        StaticData.Password = PasswordField.Text;
        
        Logger.Information($"Authorization successful. Selected profile: {StaticData.Data.SelectedProfile}");
        
        new MainWindow().Show();
        Close();
    }

    void OnClosing(object? s, CancelEventArgs e) {
        if (!StaticData.Data.Equals(default(ElybyAuthData))) return;
        try { Program.ReleaseMemory(); } finally { Environment.Exit(0); }
    }
}

