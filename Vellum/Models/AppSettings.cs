using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Vellum.Models;

/// <summary>
/// Модель настроек приложения, сохраняемая в JSON-файл
/// </summary>
public class AppSettings
{
    /// <summary>Текущая тема оформления</summary>
    public string Theme { get; set; } = "Dark";

    /// <summary>Семейство шрифта</summary>
    public string FontFamily { get; set; } = "Cascadia Code";

    /// <summary>Размер шрифта</summary>
    public double FontSize { get; set; } = 14;

    /// <summary>Кодировка по умолчанию</summary>
    public string Encoding { get; set; } = "utf-8";

    /// <summary>Тип переноса строк</summary>
    public string LineEnding { get; set; } = "LF";

    /// <summary>Показывать номера строк</summary>
    public bool ShowLineNumbers { get; set; } = true;

    /// <summary>Включать свёртывание блоков кода</summary>
    public bool EnableCodeFolding { get; set; } = true;

    /// <summary>Автосохранение включено</summary>
    public bool AutoSaveEnabled { get; set; } = false;

    /// <summary>Интервал автосохранения (секунды)</summary>
    public int AutoSaveIntervalSeconds { get; set; } = 60;

    /// <summary>Акцентный цвет</summary>
    public string AccentColor { get; set; } = "#007ACC";

    /// <summary>Путь к последней директории открытия</summary>
    public string LastDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Получить текущую кодировку
    /// </summary>
    public Encoding GetEncoding()
    {
        return Encoding.ToLowerInvariant() switch
        {
            "utf-8" => System.Text.Encoding.UTF8,
            "utf-16" => System.Text.Encoding.Unicode,
            "utf-16le" => System.Text.Encoding.Unicode,
            "utf-16be" => System.Text.Encoding.BigEndianUnicode,
            "windows-1251" => System.Text.Encoding.GetEncoding(1251),
            "windows-1252" => System.Text.Encoding.GetEncoding(1252),
            "iso-8859-1" => System.Text.Encoding.Latin1,
            _ => System.Text.Encoding.UTF8
        };
    }

    /// <summary>
    /// Создать настройки по умолчанию
    /// </summary>
    public static AppSettings CreateDefault() => new();

    /// <summary>
    /// Сериализовать настройки в JSON
    /// </summary>
    public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

    /// <summary>
    /// Десериализовать настройки из JSON
    /// </summary>
    public static AppSettings FromJson(string json) =>
        JsonConvert.DeserializeObject<AppSettings>(json) ?? CreateDefault();
}
