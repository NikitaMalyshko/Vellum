using System.IO;
using System.Windows;
using Microsoft.Win32;
using Vellum.Models;

namespace Vellum.Services;

/// <summary>
/// Сервис для показа диалоговых окон (открытие/сохранение файлов)
/// </summary>
public class DialogService
{
    /// <summary>
    /// Показать диалог открытия файла
    /// </summary>
    public (string? path, string encoding)? ShowOpenFileDialog(string lastDirectory = "")
    {
        var dialog = new OpenFileDialog
        {
            Title = "Открыть файл",
            Filter = "Все файлы (*.*)|*.*|" +
                     "Текстовые файлы (*.txt)|*.txt|" +
                     "C# файлы (*.cs)|*.cs|" +
                     "JSON файлы (*.json)|*.json|" +
                     "XML файлы (*.xml)|*.xml|" +
                     "XAML файлы (*.xaml)|*.xaml|" +
                     "Markdown (*.md)|*.md|" +
                     "HTML (*.html;*.htm)|*.html;*.htm|" +
                     "CSS (*.css)|*.css|" +
                     "JavaScript (*.js)|*.js|" +
                     "Python (*.py)|*.py|" +
                     "SQL (*.sql)|*.sql",
            FilterIndex = 1,
            RestoreDirectory = true
        };

        if (!string.IsNullOrEmpty(lastDirectory) && Directory.Exists(lastDirectory))
            dialog.InitialDirectory = lastDirectory;

        if (dialog.ShowDialog() == true)
        {
            return (dialog.FileName, "utf-8");
        }

        return null;
    }

    /// <summary>
    /// Показать диалог сохранения файла
    /// </summary>
    public string? ShowSaveFileDialog(string currentFileName = "", string lastDirectory = "")
    {
        var dialog = new SaveFileDialog
        {
            Title = "Сохранить файл как",
            Filter = "Текстовые файлы (*.txt)|*.txt|" +
                     "C# файлы (*.cs)|*.cs|" +
                     "JSON файлы (*.json)|*.json|" +
                     "XML файлы (*.xml)|*.xml|" +
                     "Все файлы (*.*)|*.*",
            FilterIndex = 1,
            RestoreDirectory = true,
            FileName = currentFileName
        };

        if (!string.IsNullOrEmpty(lastDirectory) && Directory.Exists(lastDirectory))
            dialog.InitialDirectory = lastDirectory;

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    /// <summary>
    /// Диалог подтверждения сохранения изменений
    /// </summary>
    public MessageBoxResult ShowSaveChangesDialog(string fileName)
    {
        var result = MessageBox.Show(
            $"Хотите сохранить изменения в файле \"{fileName}\"?",
            "Несохранённые изменения",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        return result;
    }

    /// <summary>
    /// Диалог ошибки
    /// </summary>
    public void ShowError(string message, string title = "Ошибка")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Диалог информации
    /// </summary>
    public void ShowInfo(string message, string title = "Информация")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
