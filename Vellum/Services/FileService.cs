using System.IO;

namespace Vellum.Services;

/// <summary>
/// Сервис для чтения и записи файлов на диске
/// </summary>
public class FileService
{
    /// <summary>
    /// Асинхронно прочитать файл с указанными кодировкой и переносами строк
    /// </summary>
    public async Task<(string content, System.Text.Encoding encoding)> ReadFileAsync(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException($"Файл не найден: {filePath}");

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

        // Определяем кодировку по BOM
        var encoding = DetectEncoding(bytes);
        var content = encoding.GetString(bytes);

        // Нормализуем переносы строк
        content = content.Replace("\r\n", "\n").Replace("\r", "\n");

        return (content, encoding);
    }

    /// <summary>
    /// Асинхронно записать содержимое в файл
    /// </summary>
    public async Task WriteFileAsync(string filePath, string content, System.Text.Encoding encoding)
    {
        var directory = System.IO.Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            System.IO.Directory.CreateDirectory(directory);

        var normalizedContent = content.Replace("\n", Environment.NewLine);
        var bytes = encoding.GetBytes(normalizedContent);
        await System.IO.File.WriteAllBytesAsync(filePath, bytes);
    }

    /// <summary>
    /// Определяет кодировку по BOM (Byte Order Mark)
    /// </summary>
    private static System.Text.Encoding DetectEncoding(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return System.Text.Encoding.UTF8;
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return System.Text.Encoding.Unicode;
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return System.Text.Encoding.BigEndianUnicode;

        return System.Text.Encoding.UTF8;
    }

    /// <summary>
    /// Проверяет, есть ли несохранённые изменения
    /// </summary>
    public static string GetWindowTitle(string fileName, bool isModified)
    {
        var prefix = isModified ? "*" : "";
        return $"{prefix}{fileName} — Vellum";
    }
}
