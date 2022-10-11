using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EELauncher.Extensions;
using Hardware.Info;
using Serilog;

namespace EELauncher.Views; 

public partial class SettingsWindow : Window {
    static readonly ILogger Logger = Log.Logger.ForType<SettingsWindow>();
    static readonly IHardwareInfo HardwareInfo = new HardwareInfo();

    public SettingsWindow() {
        HardwareInfo.RefreshMemoryStatus();
        MemoryStatus memoryStatus = HardwareInfo.MemoryStatus;
        uint ramMiB = (uint)(memoryStatus.TotalPhysical / 1024 / 1024);

        if (ramMiB % 2 != 0) ramMiB += 1;

        AvaloniaXamlLoader.Load(this);
        InitializeComponent();
        
        Header.PointerPressed += (_, e) => { if (e.Pointer.IsPrimary) BeginMoveDrag(e); };
        
        CloseButton.PointerEnter += (_, _) => CloseButton.ChangeSvgContent("Close_Pressed.svg");
        CloseButton.PointerLeave += (_, _) => CloseButton.ChangeSvgContent("Close_Normal.svg");
        CloseButton.Click += (_, _) => Close();
        
        LauncherName.Text = Tag!.ToString();
    }
}

