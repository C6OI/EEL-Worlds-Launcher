using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using CmlLib.Core;
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
    string? _newJavaPath;
    bool _saveClick;
    bool _withoutSaving;

    public SettingsWindow() => InitializeComponent();

    public SettingsWindow(string optionsFile) {
        _optionsFile = optionsFile;
        
        HardwareInfo.RefreshMemoryStatus();
        MemoryStatus memoryStatus = HardwareInfo.MemoryStatus;
        List<string> arguments = _options.JVMArguments.ToList();
        uint ramMiB = (uint)(memoryStatus.TotalPhysical / 1024 / 1024);
        
        if (ramMiB % 2 != 0) ramMiB += 1;
        arguments.RemoveAt(0);

        _newJavaPath = _options.JavaPath;
        
        InitializeComponent();

        MemoryAllocate.Value = _options.Memory;
        MemoryAllocate.Maximum = ramMiB;

        IsFullScreen.IsChecked = _options.FullScreen;

        ScreenWidth.Text = _options.Width == 0 ? "" : _options.Width.ToString();
        ScreenHeight.Text = _options.Height == 0 ? "" : _options.Height.ToString();
        
        JVMArguments.Text = string.Join(Environment.NewLine, arguments.ToArray());

        CurrentJava.Text = _options.JavaPath ?? "По умолчанию";
        
        SelectJava.Click += async (_, _) => {
            OpenFolderDialog newJava = new() {
                Title = "Выберите папку с Java",
                Directory = _options.JavaPath ?? Path.GetDirectoryName(_optionsFile)
            };

            string? javaPath = await newJava.ShowAsync(this);
            
            if (javaPath == null) return;

            string javaFile = MRule.OSName switch {
                "osx" => "java",
                "linux" => "java",
                "windows" => "javaw.exe",
                _ => throw new ArgumentOutOfRangeException()
            };
            
            string newJavaPath = Path.Combine(javaPath, "bin", javaFile);

            if (!File.Exists(newJavaPath)) {
                MessageBoxStandardParams wrongPath = new() {
                    ContentTitle = "Java not found",
                    ContentMessage = $"Файл {newJavaPath} не найден.",
                    WindowIcon = Icon,
                    Icon = MessageBox.Avalonia.Enums.Icon.Error,
                    ShowInCenter = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                await MessageBoxManager.GetMessageBoxStandardWindow(wrongPath).ShowDialog(this);
                return;
            }

            _newJavaPath = newJavaPath;
            CurrentJava.Text = newJavaPath;
        };

        ResetJava.Click += (_, _) => {
            _newJavaPath = null;
            CurrentJava.Text = "По умолчанию";
        };

        Header.PointerPressed += (_, e) => { if (e.Pointer.IsPrimary) BeginMoveDrag(e); };
        
        CloseButton.PointerEntered += (_, _) => CloseButton.ChangeSvgContent("Close_Pressed.svg");
        CloseButton.PointerExited += (_, _) => CloseButton.ChangeSvgContent("Close_Normal.svg");
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
        List<string> arguments = new() { _options.JVMArguments.First() };
        arguments.AddRange(JVMArguments.Text!.Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(argument => $"-{argument.Trim()}"));

        OptionsData newOptions = new() {
            Height = ScreenHeight.Text == "" ? 0 : int.Parse(ScreenHeight.Text!),
            Width = ScreenWidth.Text == "" ? 0 : int.Parse(ScreenWidth.Text!),
            Memory = int.Parse(MemoryAllocate.Text!),
            FullScreen = IsFullScreen.IsChecked ?? false,
            JVMArguments = arguments.ToArray(),
            JavaPath = _newJavaPath
        };

        if (!_saveClick) {
            if (!Equals(newOptions, _options)) {
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

