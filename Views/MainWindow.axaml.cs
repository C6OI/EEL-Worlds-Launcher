using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Abot2.Crawler;
using Abot2.Poco;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.FabricMC;
using CmlLib.Core.Version;
using EELauncher.Data;
using EELauncher.Extensions;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using Serilog;

namespace EELauncher.Views; 

public partial class MainWindow : Window {
    static readonly ILogger Logger = Log.Logger.ForType<MainWindow>();
    const string FabricVersion = "fabric-loader-0.14.9-1.19.2";
    readonly string _injector;
    readonly EELauncherPath _pathToMinecraft = new();
    readonly FabricVersionLoader _fabricLoader = new();
    readonly MessageBoxStandardParams _notFound;
    readonly CMLauncher _launcher;
    readonly MSession _session;
    readonly MVersionMetadata _version;
    readonly List<Control> _disabled;
    readonly List<Control> _progressBars;
    Process? _minecraftProcess;
        
    public MainWindow() {
        AppDomain.CurrentDomain.DomainUnload += Unloading;
            
        _launcher = new CMLauncher(_pathToMinecraft);
        _injector = Path.Combine(_pathToMinecraft.BasePath, "authlib-injector-1.2.1.jar");

        ElybyAuthData data = StaticData.Data;
        SelectedProfile profile = data.SelectedProfile;

        _session = new MSession(profile.Name, data.AccessToken, profile.Id) { ClientToken = data.ClientToken };
            
        MVersionCollection versions = _fabricLoader.GetVersionMetadatas();
        _version = versions.GetVersionMetadata(FabricVersion);
            
        ServicePointManager.DefaultConnectionLimit = 256;

        InitializeComponent();
            
        _disabled = new List<Control> { PlayButton, SettingsButton };
        _progressBars = new List<Control> { DownloadProgress, DownloadInfo };
            
        ClientSize = new Size(960, 540);
        LauncherName.Text = $"{Tag!}: Вы вошли как {StaticData.Data.SelectedProfile.Name}";
            
        _launcher.FileChanged += a => {
            DownloadProgress.Maximum = a.TotalFileCount;
            DownloadProgress.Value = a.ProgressedFileCount;

            DownloadInfo.Text = a.FileName != "" ? $"Скачивается: {a.FileName}" : $"{a.ProgressedFileCount}/{a.TotalFileCount}";

#if DEBUG
            Logger.Debug($"Downloading file {fileName}, {progressedFileCount}/{totalFileCount}");
#endif
        };

        _notFound = new MessageBoxStandardParams {
            ContentTitle = "404 Not Found",
            WindowIcon = Icon,
            Icon = MessageBox.Avalonia.Enums.Icon.Error,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
    }
        
    void OnInitialized(object? s, EventArgs e) {
        Background bg = WindowExtensions.RandomBackground();
        Background = bg.Brush;

#if DEBUG
        Logger.Debug($"Installed new background: {bg.BrushName}");
#endif
    }

    void NewsDescription_OnInitialized(object? s, EventArgs e) => ((TextBlock)s!).Text = "Скоро здесь будут новости с нашего сайта...";

    void NewsImage_OnPointerPressed(object? s, PointerPressedEventArgs e) => "https://eelworlds.ml/news/coming-soon".OpenUrl();

    void PlayButtonEnter(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Play_Button_Pressed.svg");
        
    void PlayButtonLeave(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Play_Button_Normal.svg");

    void CloseButton_OnClick(object? s, RoutedEventArgs e) => Close();

    void MinimizeButton_OnClick(object? s, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    void Header_OnPointerPressed(object? s, PointerPressedEventArgs e) { if (e.Pointer.IsPrimary) BeginMoveDrag(e); }

    void MinimizeButton_OnPointerLeave(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Minimize_Normal.svg");
        
    void MinimizeButton_OnPointerEnter(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Minimize_Pressed.svg");
        
    void CloseButton_OnPointerEnter(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Close_Pressed.svg");

    void CloseButton_OnPointerLeave(object? s, PointerEventArgs e) => ((Button)s!).ChangeSvgContent("Close_Normal.svg");

    void SettingsButton_OnClick(object? s, RoutedEventArgs e) => new SettingsWindow().Show(this);

    void SiteButton_OnClick(object? s, RoutedEventArgs e) => "https://eelworlds.ml/".OpenUrl();

    void DiscordButton_OnClick(object? s, RoutedEventArgs e) => "https://discord.gg/Nt9chgHxQ6".OpenUrl();
        
    void YouTubeButton_OnClick(object? s, RoutedEventArgs e) {
        _notFound.ContentMessage = "We don't have an YouTube channel yet";
        MessageBoxManager.GetMessageBoxStandardWindow(_notFound).Show();
    }

    void VkButton_OnClick(object? s, RoutedEventArgs e) {
        _notFound.ContentMessage = "We don't have an VK group yet";
        MessageBoxManager.GetMessageBoxStandardWindow(_notFound).Show();
    }

    async void PlayButton_OnClick(object? s, RoutedEventArgs e) {
        _disabled.ForEach(c => c.IsEnabled = false);
        _progressBars.ForEach(c => c.IsVisible = true);
        
        Logger.Verbose("Actions with controls completed");

        await _version.SaveAsync(_pathToMinecraft);
        await _launcher.GetAllVersionsAsync();
        
        Logger.Verbose("Minecraft files downloaded");

        CrawlPage("https://mods.eelworlds.ml");
        
        Logger.Verbose("Custom resources downloaded");

        // todo: validating AccessToken
            
        string[] jvmArguments = {
            $"-javaagent:{_injector}=ely.by"
        };

        _minecraftProcess = await _launcher.CreateProcessAsync(FabricVersion, new MLaunchOption {
            MaximumRamMb = 2048,
            Session = _session,
            GameLauncherName = "EELauncher",
            GameLauncherVersion = "1.2 Beta",
            ServerIp = "minecraft.eelworlds.ml",
            ServerPort = 8080,
            JVMArguments = jvmArguments, 
            FullScreen = true
        });
        
        Logger.Verbose("New Minecraft process created");
            
        _minecraftProcess.EnableRaisingEvents = true;
            
        _minecraftProcess.Exited += (_, _) => {
            Dispatcher.UIThread.InvokeAsync(() => {
                DownloadProgress.Value = 0;
                DownloadInfo.Text = "";
                _progressBars.ForEach(c => c.IsVisible = false);
                _disabled.ForEach(c => c.IsEnabled = true);
                Logger.Information("Minecraft exited");
                Show();
            });
        };
            
        Hide();
        _minecraftProcess.Start();
        
        Logger.Verbose("Minecraft started");
    }

    void LogoutButton_OnClick(object? s, RoutedEventArgs e) {
        Logger.Information("Logging out...");
        
        List<KeyValuePair<string, string>> data = new() {
            KeyValuePair.Create<string, string>("username", StaticData.Data.SelectedProfile.Name),
            KeyValuePair.Create<string, string>("password", StaticData.Password)
        };

        UrlExtensions.PostRequest("https://authserver.ely.by/auth/signout", data);
        
        Logger.Information("Logged out");
        
        new EntranceWindow().Show();
        Close();
    }

    async void CrawlPage(string uri) {
        CrawlConfiguration config = new() { MaxPagesToCrawl = 10, MinCrawlDelayPerDomainMilliSeconds = 500 };
        
        PoliteWebCrawler crawler = new(config);
        crawler.PageCrawlCompleted += CrawlCompleted;

        await crawler.CrawlAsync(new Uri(uri));
    }

    void CrawlCompleted(object? s, PageCrawlCompletedArgs e) => e.CrawledPage.ParsedLinks?.ToList().ForEach(CheckAndDownloadFile);

    async void CheckAndDownloadFile(HyperLink hyperLink) {
        Uri link = new(hyperLink.HrefValue.AbsoluteUri);
        string fileName = Path.GetFileName(link.ToString());

        if (fileName == "") return;

        if (fileName.EndsWith(".jar")) {
            string filePath = Path.Combine(_pathToMinecraft.Mods, fileName);

            if (fileName.StartsWith("authlib-injector")) filePath = Path.Combine(_pathToMinecraft.BasePath, fileName);

            if (File.Exists(filePath)) return;

            await UrlExtensions.DownloadFile(link, filePath);
        }
        else if (fileName.EndsWith(".png") || fileName.EndsWith(".json")) {
            string filePath = Path.Combine(_pathToMinecraft.Emotes, fileName);
            if (File.Exists(filePath)) return;

            await UrlExtensions.DownloadFile(link, filePath);
        }
        else if (fileName == "servers.dat") {
            string filePath = Path.Combine(_pathToMinecraft.BasePath, fileName);
            if (File.Exists(filePath)) return;

            await UrlExtensions.DownloadFile(link, filePath);
        }
    }
        
    void Unloading(object? s, EventArgs e) {
        List<KeyValuePair<string, string>> data = new() {
            KeyValuePair.Create<string, string>("accessToken", StaticData.Data.AccessToken),
            KeyValuePair.Create<string, string>("clientToken", StaticData.Data.ClientToken)
        };

        UrlExtensions.PostRequest("https://authserver.ely.by/auth/invalidate", data);

        try {
            _minecraftProcess!.Close();
            Program.ReleaseMemory();
        } catch { /**/ }
    }
}