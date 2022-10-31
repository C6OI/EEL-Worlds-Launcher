using System.Linq;

namespace EELauncher.Data; 

public class OptionsData {
    public string? JavaPath { get; set; }
    public string[] JVMArguments { get; set; } = null!;
    public int Width { get; set; }
    public int Height { get; set; }
    public int Memory { get; set; }
    public bool FullScreen { get; set; }

#pragma warning disable CS0659
    public override bool Equals(object? obj) {
        if (obj is not OptionsData options) return false;

        if (JavaPath != options.JavaPath ||
            Width != options.Width ||
            Height != options.Height ||
            Memory != options.Memory ||
            FullScreen != options.FullScreen) return false;

        return !JVMArguments.Where((t, i) => t != options.JVMArguments[i]).Any();
    }
#pragma warning restore CS0659
}
