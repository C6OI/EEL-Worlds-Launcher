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
using Newtonsoft.Json;
using Serilog;

namespace EELauncher.Views; 

public partial class MainWindow : Window {
    static readonly ILogger Logger = Log.Logger.ForType<MainWindow>();
    readonly EELauncherPath _pathToMinecraft = new();
    readonly FabricVersionLoader _fabricLoader = new();
    readonly CMLauncher _launcher;
    readonly MSession _session;
    readonly List<Control> _disabled;
    readonly List<Control> _progressBars;
    LauncherData _launcherData;
    Process? _minecraftProcess;

    public MainWindow() {
        AppDomain.CurrentDomain.DomainUnload += Unloading;

        string optionsFile = Path.Combine(_pathToMinecraft.BasePath, "eelauncherOptions.json");
        string injector = Path.Combine(_pathToMinecraft.BasePath, "authlib-injector-1.2.1.jar");

        Background bg = WindowExtensions.RandomBackground();

        Activated += GetLauncherData;

        Initialized += (_, _) => {
            Background = bg.Brush;
            
#if DEBUG
            Logger.Debug($"Installed new background: {bg.BrushName}");
#endif
        };

        MessageBoxStandardParams notFound = new() {
            ContentTitle = "404 Not Found",
            WindowIcon = Icon,
            Icon = MessageBox.Avalonia.Enums.Icon.Error,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        _launcher = new CMLauncher(_pathToMinecraft);
        
        if (!File.Exists(optionsFile)) {
            OptionsData options = new() {
                Memory = 2048,
                FullScreen = true,
                JVMArguments = new[] { $"-javaagent:{injector}=ely.by" }
            };
            
            StaticData.Options = options;
            
            File.Create(optionsFile).Close();
            File.WriteAllText(optionsFile, JsonConvert.SerializeObject(options, Formatting.Indented));
        } else StaticData.Options = JsonConvert.DeserializeObject<OptionsData>(File.ReadAllText(optionsFile)) ?? new OptionsData();

        ElybyAuthData data = StaticData.Data;
        SelectedProfile profile = data.SelectedProfile;

        _session = new MSession(profile.Name, data.AccessToken, profile.Id) { ClientToken = data.ClientToken };

        ServicePointManager.DefaultConnectionLimit = 256;

        InitializeComponent();

        Header.PointerPressed += (_, e) => { if (e.Pointer.IsPrimary) BeginMoveDrag(e); };
        
        NewsImage.PointerPressed += (_, _) => "https://eelworlds.ml/news/".OpenUrl();

        PlayButton.PointerEntered += (_, _) => PlayButton.ChangeSvgContent("Play_Button_Pressed.svg");
        PlayButton.PointerExited += (_, _) => PlayButton.ChangeSvgContent("Play_Button_Normal.svg");
        
        MinimizeButton.PointerEntered += (_, _) => MinimizeButton.ChangeSvgContent("Minimize_Pressed.svg");
        MinimizeButton.PointerExited += (_, _) => MinimizeButton.ChangeSvgContent("Minimize_Normal.svg");
        MinimizeButton.Click += (_, _) => WindowState = WindowState.Minimized;
        
        CloseButton.PointerEntered += (_, _) => CloseButton.ChangeSvgContent("Close_Pressed.svg");
        CloseButton.PointerExited += (_, _) => CloseButton.ChangeSvgContent("Close_Normal.svg");
        CloseButton.Click += (_, _) => Close();
        
        SettingsButton.Click += (_, _) => new SettingsWindow(optionsFile).ShowDialog(this);
        OpenLauncherFolder.Click += (_, _) => Process.Start(new ProcessStartInfo { FileName = _pathToMinecraft.BasePath, UseShellExecute = true }); // may not work on linux/macos
        
        SiteButton.Click += (_, _) => "https://eelworlds.ml/".OpenUrl();
        DiscordButton.Click += (_, _) => "https://discord.gg/Nt9chgHxQ6".OpenUrl();
        
        YouTubeButton.Click += async (_, _) => {
            notFound.ContentMessage = "We don't have an YouTube channel yet";
            await MessageBoxManager.GetMessageBoxStandardWindow(notFound).ShowDialog(this);
        };
        
        VkButton.Click += async (_, _) => {
            notFound.ContentMessage = "We don't have an VK group yet";
            await MessageBoxManager.GetMessageBoxStandardWindow(notFound).ShowDialog(this);
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

    async void GetLauncherData(object? _, EventArgs __) {
        Activated -= GetLauncherData;
        
        if (!Updater.CanConnectToServer()) {
            await MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBoxStandardParams {
                    Icon = MessageBox.Avalonia.Enums.Icon.Error,
                    WindowIcon = Icon,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ShowInCenter = true,
                    ContentTitle = "Нет доступа к сети",
                    ContentMessage = "Невозможно проверить обновления.\n" +
                                     "Проверьте ваш антивирус/фаервол и подключение к интернету."
                }).ShowDialog(this);
            
            Environment.Exit(0);
            return;
        }
        
        BaseData launcherDataResponse = await UrlExtensions.JsonHttpRequest("https://mods.eelworlds.ml/eelauncher-data.json", HttpMethod.Get, null);
        
        if (!launcherDataResponse.IsOk) {
            ErrorData error = JsonConvert.DeserializeObject<ErrorData>(await launcherDataResponse.Data.ReadAsStringAsync())!;

            Logger.Error(error.ToString());
            
            Environment.Exit(0);
            return;
        }
        
        _launcherData = JsonConvert.DeserializeObject<LauncherData>(await launcherDataResponse.Data.ReadAsStringAsync())!;

        Updater.CheckForUpdates(_launcherData.LastLauncherVersion, this);
    }

    async void PlayButton_OnClick(object? s, RoutedEventArgs e) {
        _disabled.ForEach(c => c.IsEnabled = false);
        _progressBars.ForEach(c => c.IsVisible = true);
        
        MVersionCollection versions = await _fabricLoader.GetVersionMetadatasAsync();
        MVersionMetadata version = versions.GetVersionMetadata(_launcherData.FabricVersion);

        await version.SaveAsync(_pathToMinecraft);
        await _launcher.GetAllVersionsAsync();
        
        Logger.Verbose("Minecraft files downloaded");

        CrawlPage("https://mods.eelworlds.ml");
        
        Logger.Verbose("Custom resources downloaded");

        OptionsData options = StaticData.Options;

        // todo: validating AccessToken
        _minecraftProcess = await _launcher.CreateProcessAsync(_launcherData.FabricVersion, new MLaunchOption {
            Session = _session,
            GameLauncherName = "EELauncher",
            GameLauncherVersion = "1.2 Beta",
            ServerIp = "minecraft.eelworlds.ml",
            ServerPort = 8080,
            MinimumRamMb = 512,
            MaximumRamMb = options.Memory,
            JVMArguments = options.JVMArguments, 
            FullScreen = options.FullScreen,
            ScreenWidth = options.Width,
            ScreenHeight = options.Height,
            JavaPath = options.JavaPath
        });
        
        Logger.Verbose("New Minecraft process created");
            
        _minecraftProcess.EnableRaisingEvents = true;
        _progressBars.ForEach(c => c.IsVisible = false);

        _minecraftProcess.Exited += (_, _) => {
            Dispatcher.UIThread.InvokeAsync(() => {
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

    //todo: rewrite this shitcode
    async void CheckAndDownloadFile(HyperLink hyperLink) {
        Uri link = new(hyperLink.HrefValue.AbsoluteUri);
        string fileName = Path.GetFileName(link.ToString());

        if (fileName == "") return;
        if (fileName.StartsWith("eelauncher-data.json")) return;

        if (fileName.EndsWith(".jar")) {
            string filePath = Path.Combine(_pathToMinecraft.Mods, fileName);

            if (fileName.StartsWith("authlib-injector")) filePath = Path.Combine(_pathToMinecraft.BasePath, fileName);

            if (File.Exists(filePath)) return;

            await UrlExtensions.DownloadFile(link, filePath);
        } else if (fileName.EndsWith(".png") || fileName.EndsWith(".json")) {
            string filePath = Path.Combine(_pathToMinecraft.Emotes, fileName);
            if (File.Exists(filePath)) return;

            await UrlExtensions.DownloadFile(link, filePath);
        } else if (fileName == "servers.dat") {
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