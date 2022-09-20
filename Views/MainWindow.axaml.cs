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
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.FabricMC;
using CmlLib.Core.Version;
using EELauncher.Data;
using EELauncher.Extensions;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace EELauncher.Views {
    public partial class MainWindow : Window {
        const string FabricVersion = "fabric-loader-0.14.9-1.19.2";
        readonly EELauncherPath _pathToMinecraft = new();
        readonly CMLauncher _launcher;
        readonly FabricVersionLoader _fabricLoader = new();
        Process _minecraftProcess = null!;
        MessageBoxStandardParams _notFound;
        
        public MainWindow() {
            AppDomain.CurrentDomain.DomainUnload += Unloading;
            _launcher = new CMLauncher(_pathToMinecraft);

            InitializeComponent();
            ClientSize = new Size(960, 540);
            ServicePointManager.DefaultConnectionLimit = 256;
            
            _notFound = new MessageBoxStandardParams {
                ContentTitle = "404 Not Found",
                WindowIcon = Icon,
                Icon = MessageBox.Avalonia.Enums.Icon.Error,
                ShowInCenter = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            LauncherName.Text = $"{Tag!}: Вы вошли как {StaticData.Data.selectedProfile.name}";
        }

        void OnInitialized(object? sender, EventArgs e) => Background = WindowExtensions.RandomBackground();

        void Unloading(object? sender, EventArgs e) {
            List<KeyValuePair<string, string>> data = new() {
                KeyValuePair.Create<string, string>("accessToken", StaticData.Data.accessToken),
                KeyValuePair.Create<string, string>("clientToken", StaticData.Data.clientToken)
            };

            UrlExtensions.PostRequest("https://authserver.ely.by/auth/invalidate", data);

            try {
                _minecraftProcess.Close();
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
            List<Control> disabled = new() { PlayButton, SettingsButton };
            disabled.ForEach(c => c.IsEnabled = false);

            _launcher.FileChanged += a => {
                DownloadProgress.Maximum = a.TotalFileCount;
                DownloadProgress.Value = a.ProgressedFileCount;

                DownloadInfo.Text = a.FileName != "" ? $"Скачивается: {a.FileName}" : $"{a.ProgressedFileCount}/{a.TotalFileCount}";
            };

            MVersionCollection versions = await _fabricLoader.GetVersionMetadatasAsync();
            MVersionMetadata version = versions.GetVersionMetadata(FabricVersion);
            
            await version.SaveAsync(_pathToMinecraft);
            await _launcher.GetAllVersionsAsync();
            
            CrawlPage("https://mods.eelworlds.ml");

            // todo: validating accessToken
            /*List<KeyValuePair<string, string>> validateData = new() {
                KeyValuePair.Create<string, string>("accessToken", StaticData.Data.accessToken)
            };

            string response = UrlExtensions.PostRequest("https://authserver.ely.by/auth/validate", validateData);*/

            ElybyAuthData data = StaticData.Data;
            SelectedProfile profile = data.selectedProfile;

            MSession session = new(profile.name, data.accessToken, profile.id) { ClientToken = data.clientToken };

            string injector = Path.Combine(_pathToMinecraft.BasePath, "authlib-injector-1.2.1.jar");
            
            string[] jvmArguments = {
                $"-javaagent:{injector}=ely.by"
            };

            _minecraftProcess = await _launcher.CreateProcessAsync(FabricVersion, new MLaunchOption {
                MaximumRamMb = 2048,
                Session = session,
                GameLauncherName = "EELauncher",
                GameLauncherVersion = "1.1",
                ServerIp = "minecraft.eelworlds.ml",
                ServerPort = 8080,
                JVMArguments = jvmArguments, 
                FullScreen = true
            });
            
            Hide();

            _minecraftProcess.EnableRaisingEvents = true;
            _minecraftProcess.Start();

            _minecraftProcess.Exited += (s, a) => {
                Dispatcher.UIThread.InvokeAsync(() => {
                    DownloadProgress.Value = 0;
                    DownloadInfo.Text = "";
                    disabled.ForEach(c => c.IsEnabled = true);
                    Show();
                });
            };
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
            
            links?.ForEach(async f => {
                Uri link = new(f.HrefValue.AbsoluteUri);
                string fileName = Path.GetFileName(link.ToString());

                if (fileName == "") return;

                if (fileName.EndsWith(".jar")) {
                    string file = Path.Combine(_pathToMinecraft.Mods, fileName);

                    if (fileName.StartsWith("authlib-injector"))
                        file = Path.Combine(_pathToMinecraft.BasePath, fileName);
                    
                    if (File.Exists(file)) return;

                    await UrlExtensions.DownloadFile(link, file);
                } else if (fileName.EndsWith(".png") || fileName.EndsWith(".json")) {
                    string file = Path.Combine(_pathToMinecraft.Emotes, fileName);
                    if (File.Exists(file)) return;
                    
                    await UrlExtensions.DownloadFile(link, file);
                } else if (fileName == "servers.dat") {
                    string file = Path.Combine(_pathToMinecraft.BasePath, fileName);
                    if (File.Exists(file)) return;

                    await UrlExtensions.DownloadFile(link, file);
                }
            });
        }
    }
}