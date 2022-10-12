using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using EELauncher.Data;
using EELauncher.Extensions;
using Hardware.Info;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Models;
using Newtonsoft.Json;
using Serilog;

namespace EELauncher.Views;

public partial class SettingsWindow : Window {
    static readonly ILogger Logger = Log.Logger.ForType<SettingsWindow>();
    static readonly IHardwareInfo HardwareInfo = new HardwareInfo();
    static string _optionsFile = null!;
    static OptionsData _options = StaticData.Options;
    bool _saveClick;
    bool _withoutSaving;

    public SettingsWindow() => InitializeComponent();

    public SettingsWindow(string optionsFile) {
        _optionsFile = optionsFile;
        
        HardwareInfo.RefreshMemoryStatus();
        MemoryStatus memoryStatus = HardwareInfo.MemoryStatus;
        uint ramMiB = (uint)(memoryStatus.TotalPhysical / 1024 / 1024);
        
        if (ramMiB % 2 != 0) ramMiB += 1;
        
        InitializeComponent();

        MemoryAllocate.Value = _options.Memory;
        MemoryAllocate.Maximum = ramMiB;

        IsFullScreen.IsChecked = _options.FullScreen;

        ScreenWidth.Text = _options.Width == 0 ? "" : _options.Width.ToString();
        ScreenHeight.Text = _options.Height == 0 ? "" : _options.Height.ToString();

        if (_options.JVMArguments != null) {
            List<string> arguments = _options.JVMArguments.ToList();
            arguments.RemoveAt(0);

            JVMArguments.Text = string.Join(Environment.NewLine, arguments.ToArray());
        }

        Header.PointerPressed += (_, e) => { if (e.Pointer.IsPrimary) BeginMoveDrag(e); };
        
        CloseButton.PointerEnter += (_, _) => CloseButton.ChangeSvgContent("Close_Pressed.svg");
        CloseButton.PointerLeave += (_, _) => CloseButton.ChangeSvgContent("Close_Normal.svg");
        CloseButton.Click += (_, _) => Close();

        Save.Click += (_, _) => {
            _saveClick = true;
            Close();
        };

        Cancel.Click += (_, _) => {
            _withoutSaving = true;
            Close();
        };

        Closing += (_, e) => {
            if (_withoutSaving) return;
            SaveChanges(e);
        };

        Closed += (_, _) => {
            _withoutSaving = false;
            _saveClick = false;
        };
        
        LauncherName.Text = Tag!.ToString();
    }
    
    async void SaveChanges(CancelEventArgs e) {
        //todo: JVMArguments, JavaPath, JavaVersion
        OptionsData newOptions = new() {
            Height = ScreenHeight.Text == "" ? 0 : int.Parse(ScreenHeight.Text),
            Width = ScreenHeight.Text == "" ? 0 : int.Parse(ScreenWidth.Text),
            Memory = int.Parse(MemoryAllocate.Text),
            FullScreen = IsFullScreen.IsChecked ?? false,
            JVMArguments = _options.JVMArguments,
            JavaPath = _options.JavaPath,
            JavaVersion = _options.JavaVersion
        };

        if (!_saveClick) {
            if (!newOptions.Equals(_options)) {
                e.Cancel = true;
                _withoutSaving = true;
                
                MessageBoxCustomParams changedWarningParams = new() {
                    ContentTitle = "Настройки не сохранены",
                    ContentMessage = "Вы не сохранили настройки. Сохранить?",
                    WindowIcon = Icon,
                    Icon = MessageBox.Avalonia.Enums.Icon.Warning,
                    ShowInCenter = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ButtonDefinitions = new[] {
                        new ButtonDefinition {
                            Name = "Да",
                            IsDefault = true,
                            IsCancel = false
                        },
                        
                        new ButtonDefinition {
                            Name = "Нет",
                            IsDefault = false,
                            IsCancel = false
                        },
                        
                        new ButtonDefinition {
                            Name = "Отмена",
                            IsDefault = false,
                            IsCancel = true
                        }
                    },
                };

                string result = await MessageBoxManager.GetMessageBoxCustomWindow(changedWarningParams)
                    .ShowDialog(this);

                switch (result) {
                    case "Да":
                        Save();
                        Close();
                        break;

                    case "Нет":
                        Close();
                        break;

                    case "Отмена":
                        e.Cancel = true;
                        break;
                }

                _withoutSaving = false;
                _saveClick = false;
                
                return;
            }
        }
        
        Save();

        async void Save() {
            await using (StreamWriter streamWriter = File.CreateText(_optionsFile))
                await streamWriter.WriteAsync(JsonConvert.SerializeObject(newOptions, Formatting.Indented));
            
            StaticData.Options = newOptions;
            _options = newOptions;
        }
    }
}

