using System.IO;
using Newtonsoft.Json;
using Vellum.Models;

namespace Vellum.Services;

/// <summary>
/// Сервис для сохранения и загрузки настроек приложения
/// </summary>
public class SettingsService
{
    private static readonly string AppDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Vellum");

    private static readonly string SettingsFilePath = Path.Combine(AppDataPath, "settings.json");

    /// <summary>
    /// Загрузить настройки из файла. Если файл не существует — вернуть настройки по умолчанию
    /// </summary>
    public AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                return AppSettings.FromJson(json);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
        }

        return AppSettings.CreateDefault();
    }

    /// <summary>
    /// Сохранить настройки в JSON-файл
    /// </summary>
    public void Save(AppSettings settings)
    {
        try
        {
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);

            var json = settings.ToJson();
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
        }
    }
}
