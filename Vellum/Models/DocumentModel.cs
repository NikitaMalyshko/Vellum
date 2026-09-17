using System.IO;

namespace Vellum.Models;

/// <summary>
/// Модель документа, хранящая метаданные открытого файла
/// </summary>
public class DocumentModel
{
    /// <summary>Уникальный идентификатор документа</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Путь к файлу на диске (пусто для нового документа)</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Отображаемое имя (имя файла или "Новый документ")</summary>
    public string DisplayName { get; set; } = "Новый документ";

    /// <summary>Содержимое документа</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Был ли документ изменён</summary>
    public bool IsModified { get; set; }

    /// <summary>Документ новый (ещё не сохранён)</summary>
    public bool IsNew => string.IsNullOrEmpty(FilePath);

    /// <summary>Кодировка документа</summary>
    public string Encoding { get; set; } = "utf-8";

    /// <summary>Тип подсветки синтаксиса (SyntaxHighlighting name)</summary>
    public string SyntaxHighlighting { get; set; } = "Text";

    /// <summary>
    /// Определяет тип подсветки по расширению файла
    /// </summary>
    public static string DetectHighlighting(string filePath)
    {
        var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".cs" => "C#",
            ".json" => "Json",
            ".xml" => "Xml",
            ".xaml" => "Xml",
            ".csproj" => "Xml",
            ".md" => "Markdown",
            ".cshtml" => "Html",
            ".html" => "Html",
            ".css" => "Css",
            ".js" => "JavaScript",
            ".ts" => "JavaScript",
            ".py" => "Python",
            ".java" => "Java",
            ".cpp" or ".c" or ".h" => "C++",
            ".sql" => "SQL",
            ".bat" or ".cmd" or ".ps1" => "PowerShell",
            ".yaml" or ".yml" => "YAML",
            ".txt" => "Text",
            _ => "Text"
        };
    }

    /// <summary>
    /// Создать модель из файла
    /// </summary>
    public static DocumentModel FromFile(string filePath, string content, System.Text.Encoding encoding)
    {
        return new DocumentModel
        {
            FilePath = filePath,
            DisplayName = System.IO.Path.GetFileName(filePath),
            Content = content,
            IsModified = false,
            Encoding = encoding.WebName,
            SyntaxHighlighting = DetectHighlighting(filePath)
        };
    }
}
