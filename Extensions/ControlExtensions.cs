using System;
using Avalonia.Controls;
using Avalonia.Svg.Skia;

namespace EELauncher.Extensions; 

public static class ControlExtensions {
    public static void ChangeSvgContent(this Button button, string path) {
        Image newContent = new() {
            Source = new SvgImage {
                Source = SvgSource.Load<SvgSource>("", new Uri(path))
            }
        };

        button.Content = newContent;
    }
}
