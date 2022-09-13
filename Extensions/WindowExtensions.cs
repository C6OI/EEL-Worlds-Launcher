using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace EELauncher.Extensions; 

public static class WindowExtensions {
    public static IBrush RandomBackground() {
        IAssetLoader assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!; 
        
        string[] backgrounds = {
            @"avares://EELauncher/Assets/Background_1.png",
            @"avares://EELauncher/Assets/Background_2.png",
            @"avares://EELauncher/Assets/Background_3.png"
        };

        Random random = new();
        string bgUri = backgrounds[random.Next(0, backgrounds.Length)];
            
        ImageBrush bg = new(new Bitmap(assets.Open(new Uri(bgUri)))) {
            Stretch = Stretch.Fill
        };

        return bg;
    }
}
