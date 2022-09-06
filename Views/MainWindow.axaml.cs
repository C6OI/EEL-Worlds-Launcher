using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using EELauncher.Extensions;

namespace EELauncher.Views {
    public partial class MainWindow : Window {
        readonly IAssetLoader _assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;

        public MainWindow() {
            InitializeComponent();
            ClientSize = new Size(960, 540);
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
        
        public void NewsShortBlock_OnInitialized(object? sender, EventArgs e) {
            TextBlock textBlock = (TextBlock)sender!;
            
            textBlock.Text = "Раздача на спувне бесплатные шалкеры awawa!!\n" +
                             "Вчера я съел угря\n" +
                             "Что такое galnet и где кошкодевочки\n";
        }
        
        void NewsImage_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
            Image image = (Image)sender!;

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
    }
}