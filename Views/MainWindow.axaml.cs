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

namespace EELauncher.Views {
    public partial class MainWindow : Window {
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
            SelectedProfile profile = data.selectedProfile;

            _session = new MSession(profile.name, data.accessToken, profile.id) { ClientToken = data.clientToken };
            
            MVersionCollection versions = _fabricLoader.GetVersionMetadatas();
            _version = versions.GetVersionMetadata(FabricVersion);
            
            ServicePointManager.DefaultConnectionLimit = 256;

            InitializeComponent();
            
            _disabled = new List<Control> { PlayButton, SettingsButton };
            _progressBars = new List<Control> { DownloadProgress, DownloadInfo };
            
            ClientSize = new Size(960, 540);
            LauncherName.Text = $"{Tag!}: Вы вошли как {StaticData.Data.selectedProfile.name}";
            
            _launcher.FileChanged += a => {
                DownloadProgress.Maximum = a.TotalFileCount;
                DownloadProgress.Value = a.ProgressedFileCount;

                DownloadInfo.Text = a.FileName != "" ? $"Скачивается: {a.FileName}" : $"{a.ProgressedFileCount}/{a.TotalFileCount}";
            };

            _notFound = new MessageBoxStandardParams {
                ContentTitle = "404 Not Found",
                WindowIcon = Icon,
                Icon = MessageBox.Avalonia.Enums.Icon.Error,
                ShowInCenter = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
        }
        
        void OnInitialized(object? sender, EventArgs e) => Background = WindowExtensions.RandomBackground();

        void Unloading(object? sender, EventArgs e) {
            List<KeyValuePair<string, string>> data = new() {
                KeyValuePair.Create<string, string>("accessToken", StaticData.Data.accessToken),
                KeyValuePair.Create<string, string>("clientToken", StaticData.Data.clientToken)
            };

            UrlExtensions.PostRequest("https://authserver.ely.by/auth/invalidate", data);

            try {
                _minecraftProcess!.Close();
                Program.ReleaseMemory();
            } catch { /**/ }
        }
        
        void NewsDescription_OnInitialized(object? sender, EventArgs e) =>
            ((TextBlock)sender!).Text = "Скоро здесь будут новости с нашего сайта...";

        void NewsImage_OnPointerPressed(object? sender, PointerPressedEventArgs e) => 
            "https://eelworlds.ml/news/coming-soon".OpenUrl();

        void PlayButtonEnter(object? sender, PointerEventArgs e) =>
            ((Button)sender!).ChangeSvgContent("avares://EELauncher/Assets/Play_Button_Pressed.svg");
        
        void PlayButtonLeave(object? sender, PointerEventArgs e) =>
            ((Button)sender!).ChangeSvgContent("avares://EELauncher/Assets/Play_Button_Normal.svg");

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

        void SettingsButton_OnClick(object? sender, RoutedEventArgs e) => new SettingsWindow().Show(this);

        void SiteButton_OnClick(object? sender, RoutedEventArgs e) => "https://eelworlds.ml/".OpenUrl();

        void DiscordButton_OnClick(object? sender, RoutedEventArgs e) => "https://discord.gg/Nt9chgHxQ6".OpenUrl();
        
        void YouTubeButton_OnClick(object? sender, RoutedEventArgs e) {
            _notFound.ContentMessage = "We don't have an YouTube channel yet";
            MessageBoxManager.GetMessageBoxStandardWindow(_notFound).Show();
        }

        void VkButton_OnClick(object? sender, RoutedEventArgs e) {
            _notFound.ContentMessage = "We don't have an VK group yet";
            MessageBoxManager.GetMessageBoxStandardWindow(_notFound).Show();
        }

        async void PlayButton_OnClick(object? sender, RoutedEventArgs e) {
            _disabled.ForEach(c => c.IsEnabled = false);
            _progressBars.ForEach(c => c.IsVisible = true);

            await _version.SaveAsync(_pathToMinecraft);
            await _launcher.GetAllVersionsAsync();
            
            CrawlPage("https://mods.eelworlds.ml");

            // todo: validating accessToken
            
            string[] jvmArguments = {
                $"-javaagent:{_injector}=ely.by"
            };

            _minecraftProcess = await _launcher.CreateProcessAsync(FabricVersion, new MLaunchOption {
                MaximumRamMb = 2048,
                Session = _session,
                GameLauncherName = "EELauncher",
                GameLauncherVersion = "1.1",
                ServerIp = "minecraft.eelworlds.ml",
                ServerPort = 8080,
                JVMArguments = jvmArguments, 
                FullScreen = true
            });
            
            _minecraftProcess.EnableRaisingEvents = true;
            
            _minecraftProcess.Exited += (_, _) => {
                Dispatcher.UIThread.InvokeAsync(() => {
                    DownloadProgress.Value = 0;
                    DownloadInfo.Text = "";
                    _progressBars.ForEach(c => c.IsVisible = false);
                    _disabled.ForEach(c => c.IsEnabled = true);
                    Show();
                });
            };
            
            Hide();
            _minecraftProcess.Start();
        }

        void LogoutButton_OnClick(object? sender, RoutedEventArgs e) {
            List<KeyValuePair<string, string>> data = new() {
                KeyValuePair.Create<string, string>("username", StaticData.Data.selectedProfile.name),
                KeyValuePair.Create<string, string>("password", StaticData.Password)
            };

            UrlExtensions.PostRequest("https://authserver.ely.by/auth/signout", data);
            
            new EntranceWindow().Show();
            Close();
        }

        async void CrawlPage(string uri) {
            CrawlConfiguration config = new() {
                MaxPagesToCrawl = 10,
                MinCrawlDelayPerDomainMilliSeconds = 500
            };
        
            PoliteWebCrawler crawler = new(config);
            crawler.PageCrawlCompleted += CrawlCompleted;

            await crawler.CrawlAsync(new Uri(uri));
        }

        void CrawlCompleted(object? sender, PageCrawlCompletedArgs e) {
            List<HyperLink>? links = e.CrawledPage.ParsedLinks?.ToList();
            
            links?.ForEach(CheckAndDownloadFile);
        }

        async void CheckAndDownloadFile(HyperLink f) {
            Uri link = new(f.HrefValue.AbsoluteUri);
            string fileName = Path.GetFileName(link.ToString());

            if (fileName == "") return;

            if (fileName.EndsWith(".jar")) {
                string file = Path.Combine(_pathToMinecraft.Mods, fileName);

                if (fileName.StartsWith("authlib-injector")) file = Path.Combine(_pathToMinecraft.BasePath, fileName);

                if (File.Exists(file)) return;

                await UrlExtensions.DownloadFile(link, file);
            }
            else if (fileName.EndsWith(".png") || fileName.EndsWith(".json")) {
                string file = Path.Combine(_pathToMinecraft.Emotes, fileName);
                if (File.Exists(file)) return;

                await UrlExtensions.DownloadFile(link, file);
            }
            else if (fileName == "servers.dat") {
                string file = Path.Combine(_pathToMinecraft.BasePath, fileName);
                if (File.Exists(file)) return;

                await UrlExtensions.DownloadFile(link, file);
            }
        }
    }
}