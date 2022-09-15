using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CmlLib.Core;
using CmlLib.Core.Auth;
using EELauncher.Data;
using EELauncher.Extensions;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace EELauncher.Views {
    public partial class MainWindow : Window {
        readonly IAssetLoader _assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!; 
        readonly MinecraftPath _pathToMinecraft = new();
        readonly CMLauncher _launcher;
        readonly string _appData =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                    Environment.SpecialFolderOption.Create), "EELauncher");
        Process _minecraftProcess = null!;
        
        public MainWindow() {
            _launcher = new CMLauncher(_pathToMinecraft);

            InitializeComponent();
            ClientSize = new Size(960, 540);
            ServicePointManager.DefaultConnectionLimit = 256;

            LauncherName.Text = $"EELauncher 1.0: Вы вошли как {StaticData.Data.selectedProfile.name}";
        }

        void OnInitialized(object? sender, EventArgs e) {
            Background = WindowExtensions.RandomBackground();
        }
        
        void NewsDescription_OnInitialized(object? sender, EventArgs e) {
            TextBlock textBlock = (TextBlock)sender!;
            
            textBlock.Text = "Раздача на спувне бесплатные шалкеры awawa!!\n" +
                             "Вчера я съел угря\n" +
                             "Что такое galnet и где кошкодевочки\n";

            textBlock.Text = _launcher.MinecraftPath.BasePath;

            textBlock.Text = _appData;
        }
        
        void NewsImage_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
            "https://eelworlds.ml/news/coming-soon".OpenUrl();
        }
        
        void PlayButtonEnter(object? sender, PointerEventArgs e) {
            Button button = (Button)sender!;
            
            Image enterButtonImage = new() {
                Source = new Bitmap(_assets.Open(new Uri(@"avares://EELauncher/Assets/Play_Button_Pressed.png")))
            };

            button.Content = enterButtonImage;
        }

        void PlayButtonLeave(object? sender, PointerEventArgs e) {
            Button button = (Button)sender!;

            Image leaveButtonImage = new() {
                Source = new Bitmap(_assets.Open(new Uri(@"avares://EELauncher/Assets/Play_Button_Normal.png")))
            };

            button.Content = leaveButtonImage;
        }

        void CloseButton_OnClick(object? sender, RoutedEventArgs e) {
            try { _minecraftProcess.Close(); } finally { Close(); }
        }

        void MinimizeButton_OnClick(object? sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        void Header_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
            if (e.Pointer.IsPrimary)
                BeginMoveDrag(e);
        }

        void MinimizeButton_OnPointerEnter(object? sender, PointerEventArgs e) {
            Button button = (Button)sender!;

            Image enterButtonImage = new() {
                Source = new Bitmap(_assets.Open(new Uri(@"avares://EELauncher/Assets/Minimize_Pressed.png")))
            };

            button.Content = enterButtonImage;
        }

        void MinimizeButton_OnPointerLeave(object? sender, PointerEventArgs e) {
            Button button = (Button)sender!;

            Image enterButtonImage = new() {
                Source = new Bitmap(_assets.Open(new Uri(@"avares://EELauncher/Assets/Minimize_Normal.png")))
            };

            button.Content = enterButtonImage;
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

        void SiteButton_OnClick(object? sender, RoutedEventArgs e) {
            "https://eelworlds.ml/".OpenUrl();
        }


        void DiscordButton_OnClick(object? sender, RoutedEventArgs e) {
            "https://discord.gg/Nt9chgHxQ6".OpenUrl();
        }

        void YouTubeButton_OnClick(object? sender, RoutedEventArgs e) {
            MessageBoxStandardParams mBoxParams = new() {
                ContentTitle = "404 Not Found",
                ContentMessage = "We aren't have an YouTube channel yet",
                WindowIcon = Icon,
                Icon = MessageBox.Avalonia.Enums.Icon.Error,
                ShowInCenter = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            
            IMsBoxWindow<ButtonResult>? mBox = MessageBoxManager.GetMessageBoxStandardWindow(mBoxParams);

            mBox.Show();
        }

        void VkButton_OnClick(object? sender, RoutedEventArgs e) {
            MessageBoxStandardParams mBoxParams = new() {
                ContentTitle = "404 Not Found",
                ContentMessage = "We aren't have an VK group yet",
                WindowIcon = Icon,
                Icon = MessageBox.Avalonia.Enums.Icon.Error,
                ShowInCenter = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            
            IMsBoxWindow<ButtonResult>? mBox = MessageBoxManager.GetMessageBoxStandardWindow(mBoxParams);

            mBox.Show();
        }

        async void PlayButton_OnClick(object? sender, RoutedEventArgs e) {
            Hide();
            
            List<Control> disabled = new() { PlayButton, SettingsButton };
            disabled.ForEach(c => c.IsEnabled = false);
            
            // todo: validating accessToken
            /*List<KeyValuePair<string, string>> validateData = new() {
                KeyValuePair.Create<string, string>("accessToken", StaticData.Data.accessToken)
            };

            string response = UrlExtensions.PostRequest("https://authserver.ely.by/auth/validate", validateData);*/

            ElybyAuthData data = StaticData.Data;
            SelectedProfile profile = data.selectedProfile;

            MSession session = new(profile.name, data.accessToken, profile.id) {
                ClientToken = data.clientToken
            };

            _minecraftProcess = await _launcher.CreateProcessAsync("1.19.2", new MLaunchOption {
                MaximumRamMb = 2048,
                Session = session
            });
            
            _minecraftProcess.EnableRaisingEvents = true;
            _minecraftProcess.Start();

            _minecraftProcess.Exited += (s, a) => {
                disabled.ForEach(c => c.IsEnabled = true);
                Show();
            };
        }

        void LogoutButton_OnClick(object? sender, RoutedEventArgs e) {
            List<KeyValuePair<string, string>> data = new() {
                KeyValuePair.Create<string, string>("username", StaticData.Data.selectedProfile.name),
                KeyValuePair.Create<string, string>("password", StaticData.Password)
            };

            UrlExtensions.PostRequest("https://authserver.ely.by/auth/signout", data);
            
            try { _minecraftProcess.Close(); }
            finally { new EntranceWindow().Show(); Close(); }
        }

        void OnClosing(object? sender, CancelEventArgs e) {
            List<KeyValuePair<string, string>> data = new() {
                KeyValuePair.Create<string, string>("accessToken", StaticData.Data.accessToken),
                KeyValuePair.Create<string, string>("clientToken", StaticData.Data.clientToken)
            };

            UrlExtensions.PostRequest("https://authserver.ely.by/auth/invalidate", data);
            
            try { _minecraftProcess.Close(); Program.ReleaseMemory(); } catch { /**/ }
        }
    }
}