using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Abot2.Crawler;
using Abot2.Poco;
using Avalonia.Controls;
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
using MessageBox.Avalonia.Extensions;
using Newtonsoft.Json;
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
        
        Background bg = WindowExtensions.RandomBackground();

        Initialized += (_, _) => {
            Background = bg.Brush;

#if DEBUG
            Logger.Debug($"Installed new background: {bg.BrushName}");
#endif
        };
        
        _notFound = new MessageBoxStandardParams {
            ContentTitle = "404 Not Found",
            WindowIcon = Icon,
            Icon = MessageBox.Avalonia.Enums.Icon.Error,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        _launcher = new CMLauncher(_pathToMinecraft);
        _injector = Path.Combine(_pathToMinecraft.BasePath, "authlib-injector-1.2.1.jar");

        ElybyAuthData data = StaticData.Data;
        SelectedProfile profile = data.SelectedProfile;

        _session = new MSession(profile.Name, data.AccessToken, profile.Id) { ClientToken = data.ClientToken };
            
        MVersionCollection versions = _fabricLoader.GetVersionMetadatas();
        _version = versions.GetVersionMetadata(FabricVersion);
            
        ServicePointManager.DefaultConnectionLimit = 256;

        InitializeComponent();
        
        Header.PointerPressed += (_, e) => { if (e.Pointer.IsPrimary) BeginMoveDrag(e); };
        
        NewsImage.PointerPressed += (_, _) => "https://eelworlds.ml/news/".OpenUrl();

        PlayButton.PointerEnter += (_, _) => PlayButton.ChangeSvgContent("Play_Button_Pressed.svg");
        PlayButton.PointerLeave += (_, _) => PlayButton.ChangeSvgContent("Play_Button_Normal.svg");
        
        MinimizeButton.PointerEnter += (_, _) => MinimizeButton.ChangeSvgContent("Minimize_Pressed.svg");
        MinimizeButton.PointerLeave += (_, _) => MinimizeButton.ChangeSvgContent("Minimize_Normal.svg");
        MinimizeButton.Click += (_, _) => WindowState = WindowState.Minimized;
        
        CloseButton.PointerEnter += (_, _) => CloseButton.ChangeSvgContent("Close_Pressed.svg");
        CloseButton.PointerLeave += (_, _) => CloseButton.ChangeSvgContent("Close_Normal.svg");
        CloseButton.Click += (_, _) => Close();
        
        SettingsButton.Click += (_, _) => new SettingsWindow().ShowDialog(this);
        OpenLauncherFolder.Click += (_, _) => Process.Start(new ProcessStartInfo { FileName = _pathToMinecraft.BasePath, UseShellExecute = true }); // may not work on linux/macos
        
        SiteButton.Click += (_, _) => "https://eelworlds.ml/".OpenUrl();
        DiscordButton.Click += (_, _) => "https://discord.gg/Nt9chgHxQ6".OpenUrl();
        
        YouTubeButton.Click += async (_, _) => {
            _notFound.ContentMessage = "We don't have an YouTube channel yet";
            await MessageBoxManager.GetMessageBoxStandardWindow(_notFound).ShowDialog(this);
        };
        
        VkButton.Click += async (_, _) => {
            _notFound.ContentMessage = "We don't have an VK group yet";
            await MessageBoxManager.GetMessageBoxStandardWindow(_notFound).ShowDialog(this);
        };

        _launcher.FileChanged += a => {
            DownloadProgress.Maximum = a.TotalFileCount;
            DownloadProgress.Value = a.ProgressedFileCount;

            DownloadInfo.Text = a.FileName != "" ? $"Скачивается: {a.FileName}" : $"{a.ProgressedFileCount}/{a.TotalFileCount}";

#if DEBUG
            Logger.Debug($"Downloading file {a.FileName}, {a.ProgressedFileCount}/{a.TotalFileCount}");
#endif
        };
        
        _disabled = new List<Control>(2) { PlayButton, SettingsButton };
        _progressBars = new List<Control>(2) { DownloadProgress, DownloadInfo };
        
        LauncherName.Text = $"{Tag!}: Вы вошли как {StaticData.Data.SelectedProfile.Name}";
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

    async void LogoutButton_OnClick(object? s, RoutedEventArgs e) {
        Logger.Information("Logging out...");

        Dictionary<string, string> data = new() {
            { "username", StaticData.Data.SelectedProfile.Name },
            { "password", StaticData.Password }
        };

        BaseData response = await UrlExtensions.JsonHttpRequest("https://authserver.ely.by/auth/signout", HttpMethod.Post, data);

        if (!response.IsOk) {
            ErrorData error = JsonConvert.DeserializeObject<ErrorData>(await response.Data.ReadAsStringAsync())!;
            Logger.Error(error.ToString());
            return;
        }
        
        Logger.Information("Logged out");
        
        new EntranceWindow().Show();
        Close();
    }

    async void CrawlPage(string uri) {
        CrawlConfiguration config = new() { MaxPagesToCrawl = 10, MinCrawlDelayPerDomainMilliSeconds = 500 };
        
        PoliteWebCrawler crawler = new(config);
        crawler.PageCrawlCompleted += (_, e) => e.CrawledPage.ParsedLinks?.ToList().ForEach(CheckAndDownloadFile);

        await crawler.CrawlAsync(new Uri(uri));
    }

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
        
    async void Unloading(object? s, EventArgs e) {
        Dictionary<string, string> data = new() {
            { "accessToken", StaticData.Data.AccessToken },
            { "clientToken", StaticData.Data.ClientToken }
        };

        BaseData response = await UrlExtensions.JsonHttpRequest("https://authserver.ely.by/auth/invalidate", HttpMethod.Post, data);
        
        if (!response.IsOk) {
            ErrorData error = JsonConvert.DeserializeObject<ErrorData>(await response.Data.ReadAsStringAsync())!;
            Logger.Error(error.ToString());
            return;
        }

        try {
            _minecraftProcess!.Close();
            Program.ReleaseMemory();
        } catch { /**/ }
    }
}