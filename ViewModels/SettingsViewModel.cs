using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vellum.Models;

namespace Vellum.ViewModels;

/// <summary>
/// ViewModel настроек приложения
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _settings;

    public SettingsViewModel(AppSettings settings)
    {
        _settings = settings;
    }

    /// <summary>Тема оформления</summary>
    public string Theme
    {
        get => _settings.Theme;
        set { _settings.Theme = value; OnPropertyChanged(); }
    }

    /// <summary>Семейство шрифта</summary>
    public string FontFamily
    {
        get => _settings.FontFamily;
        set { _settings.FontFamily = value; OnPropertyChanged(); }
    }

    /// <summary>Размер шрифта</summary>
    public double FontSize
    {
        get => _settings.FontSize;
        set { _settings.FontSize = value; OnPropertyChanged(); }
    }

    /// <summary>Кодировка</summary>
    public string Encoding
    {
        get => _settings.Encoding;
        set { _settings.Encoding = value; OnPropertyChanged(); }
    }

    /// <showLineNumbers>Показывать номера строк</showLineNumbers>
    public bool ShowLineNumbers
    {
        get => _settings.ShowLineNumbers;
        set { _settings.ShowLineNumbers = value; OnPropertyChanged(); }
    }

    /// <summary>Включить свёртывание блоков кода</summary>
    public bool EnableCodeFolding
    {
        get => _settings.EnableCodeFolding;
        set { _settings.EnableCodeFolding = value; OnPropertyChanged(); }
    }

    /// <summary>Автосохранение включено</summary>
    public bool AutoSaveEnabled
    {
        get => _settings.AutoSaveEnabled;
        set { _settings.AutoSaveEnabled = value; OnPropertyChanged(); }
    }

    /// <summary>Интервал автосохранения (секунды)</summary>
    public int AutoSaveIntervalSeconds
    {
        get => _settings.AutoSaveIntervalSeconds;
        set { _settings.AutoSaveIntervalSeconds = value; OnPropertyChanged(); }
    }

    /// <summary>Акцентный цвет</summary>
    public string AccentColor
    {
        get => _settings.AccentColor;
        set { _settings.AccentColor = value; OnPropertyChanged(); }
    }

    /// <summary>Получить текущую модель настроек</summary>
    public AppSettings GetSettings() => _settings;

    /// <summary>Доступные размеры шрифта</summary>
    public double[] AvailableFontSizes => new double[]
        { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36, 48, 72 };

    /// <summary>Доступные кодировки</summary>
    public string[] AvailableEncodings => new[]
        { "utf-8", "utf-16", "windows-1251", "windows-1252", "iso-8859-1" };

    /// <summary>Доступные темы</summary>
    public string[] AvailableThemes => new[] { "Dark", "Light" };

    /// <summary>Доступные шрифты</summary>
    public string[] AvailableFonts => new[]
    {
        "Cascadia Code", "Consolas", "Courier New", "Fira Code",
        "JetBrains Mono", "Source Code Pro", "Ubuntu Mono",
        "Segoe UI", "Calibri", "Arial"
    };
}
