using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace EELauncher.Views {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            ClientSize = new Size(960, 540);
        }

        public void OnInitialized(object? sender, EventArgs e) {
            IAssetLoader assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
            
            string[] backgrounds = {
                @"avares://EELauncher/Assets/Background_1.png",
                @"avares://EELauncher/Assets/Background_2.png"
            };

            Random random = new();
            string bgUri = backgrounds[random.Next(0, backgrounds.Length)];
            
            ImageBrush bg = new(new Bitmap(assets.Open(new Uri(bgUri))));
            
            Background = bg;
        }
    }
}