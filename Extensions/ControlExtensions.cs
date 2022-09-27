using System;
using Avalonia.Controls;
using Avalonia.Svg.Skia;

namespace EELauncher.Extensions; 

public static class ControlExtensions {
    public static void ChangeSvgContent(this Button button, string file) {
        string path = $"avares://EELauncher/Assets/{file}"; 
            
        Image newContent = new() {
            Source = new SvgImage {
                Source = SvgSource.Load<SvgSource>(path, new Uri(path))
            }
        };

        button.Content = newContent;
    }
}
