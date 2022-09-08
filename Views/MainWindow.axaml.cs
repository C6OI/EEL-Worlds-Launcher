using System;
using System.Diagnostics;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CmlLib.Core;
using CmlLib.Core.Auth;
using EELauncher.Extensions;

namespace EELauncher.Views {
    public partial class MainWindow : Window {
        readonly IAssetLoader _assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!; 
        readonly MinecraftPath _pathToMinecraft = new();
        CMLauncher _launcher;
        
        public MainWindow() {
            _launcher = new CMLauncher(_pathToMinecraft);
            
            InitializeComponent();
            ClientSize = new Size(960, 540);
            ServicePointManager.DefaultConnectionLimit = 256;
        }

        public void OnInitialized(object? sender, EventArgs e) {
            string[] backgrounds = {
                @"avares://EELauncher/Assets/Background_1.png",
                @"avares://EELauncher/Assets/Background_2.png"
            };

            Random random = new();
            string bgUri = backgrounds[random.Next(0, backgrounds.Length)];
            
            ImageBrush bg = new(new Bitmap(_assets.Open(new Uri(bgUri))));
            
            Background = bg;
        }
        
        public void NewsDescription_OnInitialized(object? sender, EventArgs e) {
            TextBlock textBlock = (TextBlock)sender!;
            
            textBlock.Text = "Раздача на спувне бесплатные шалкеры awawa!!\n" +
                             "Вчера я съел угря\n" +
                             "Что такое galnet и где кошкодевочки\n";

            textBlock.Text = _launcher.MinecraftPath.BasePath;
        }
        
        void NewsImage_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
            "http://eelworlds.ml/news/coming-soon".OpenUrl();
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

        async void PlayButton_OnClick(object? sender, RoutedEventArgs e) {
            Process process = await _launcher.CreateProcessAsync("1.19.2", new MLaunchOption {
                MaximumRamMb = 2048,
                Session = MSession.GetOfflineSession(NicknameField.Text),
            });

            process.Start();
        }

        void CloseButton_OnClick(object? sender, RoutedEventArgs e) {
            Close();
        }

        void MinimizeButton_OnClick(object? sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }
    }
}