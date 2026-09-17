using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vellum.Models;
using Vellum.Services;

namespace Vellum.ViewModels;

/// <summary>
/// Главный ViewModel приложения — управляет вкладками, командами, настройками
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly FileService _fileService = new();
    private readonly SettingsService _settingsService = new();
    private readonly DialogService _dialogService = new();
    private readonly DispatcherTimer _autoSaveTimer;

    /// <summary>Коллекция открытых документов (вкладок)</summary>
    public ObservableCollection<DocumentViewModel> Documents { get; } = new();

    /// <summary>Текущий активный документ</summary>
    [ObservableProperty]
    private DocumentViewModel? _activeDocument;

    /// <summary>Текущая тема оформления</summary>
    [ObservableProperty]
    private string _currentTheme = "Dark";

    /// <summary>Настройки приложения</summary>
    public AppSettings Settings { get; private set; }

    /// <summary>ViewModel настроек</summary>
    public SettingsViewModel SettingsViewModel { get; }

    /// <summary>Панель поиска видима</summary>
    [ObservableProperty]
    private bool _isSearchPanelVisible;

    /// <summary>Панель поиска в режиме замены</summary>
    [ObservableProperty]
    private bool _isReplaceMode;

    /// <summary>Текст поиска</summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>Текст замены</summary>
    [ObservableProperty]
    private string _replaceText = string.Empty;

    /// <summary>Искать с учётом регистра</summary>
    [ObservableProperty]
    private bool _matchCase;

    /// <summary>Использовать регулярные выражения</summary>
    [ObservableProperty]
    private bool _useRegex;

    /// <summary>Искать целые слова</summary>
    [ObservableProperty]
    private bool _wholeWord;

    /// <summary>Статус-бар сообщение</summary>
    [ObservableProperty]
    private string _statusMessage = "Готово";

    /// <summary>Видимость панели командной палитры</summary>
    [ObservableProperty]
    private bool _isCommandPaletteVisible;

    /// <summary>Текст фильтра командной палитры</summary>
    [ObservableProperty]
    private string _commandPaletteFilter = string.Empty;

    /// <summary>Список команд для палитры</summary>
    public ObservableCollection<CommandItem> FilteredCommands { get; } = new();

    /// <summary>Все доступные команды</summary>
    private readonly List<CommandItem> _allCommands;

    public MainViewModel()
    {
        Settings = _settingsService.Load();
        SettingsViewModel = new SettingsViewModel(Settings);
        CurrentTheme = Settings.Theme;

        // Инициализация таймера автосохранения
        _autoSaveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(Settings.AutoSaveIntervalSeconds)
        };
        _autoSaveTimer.Tick += AutoSaveTimer_Tick;
        if (Settings.AutoSaveEnabled)
            _autoSaveTimer.Start();

        // Инициализация списка команд для палитры
        _allCommands = new List<CommandItem>
        {
            new("Новый документ", "Ctrl+N", () => _ = NewFileAsync()),
            new("Открыть файл", "Ctrl+O", () => _ = OpenFileAsync()),
            new("Сохранить", "Ctrl+S", () => _ = SaveFileAsync()),
            new("Сохранить как", "Ctrl+Shift+S", () => _ = SaveFileAsAsync()),
            new("Отменить", "Ctrl+Z", () => ActiveDocument?.Undo?.Invoke()),
            new("Повторить", "Ctrl+Y", () => ActiveDocument?.Redo?.Invoke()),
            new("Поиск", "Ctrl+F", () => ToggleSearch(false)),
            new("Замена", "Ctrl+H", () => ToggleSearch(true)),
            new("Перейти к строке", "Ctrl+G", () => GoToLine()),
            new("Новая тема: Тёмная", "", () => SwitchTheme("Dark")),
            new("Новая тема: Светлая", "", () => SwitchTheme("Light")),
            new("Настройки", "Ctrl+Shift+P", () => ShowSettings()),
            new("Закрыть вкладку", "Ctrl+W", () => CloseActiveDocument()),
            new("Следующая вкладка", "Ctrl+Tab", () => SwitchToNextDocument()),
            new("Предыдущая вкладка", "Ctrl+Shift+Tab", () => SwitchToPreviousDocument()),
            new("Все файлы", "", () => { }),
        };
    }

    /// <summary>
    /// Новый документ (Ctrl+N)
    /// </summary>
    [RelayCommand]
    private async Task NewFileAsync()
    {
        var doc = new DocumentViewModel(new DocumentModel
        {
            DisplayName = $"Новый {Documents.Count + 1}"
        });
        doc.Undo = () => { };
        doc.Redo = () => { };
        Documents.Add(doc);
        ActiveDocument = doc;
        StatusMessage = "Создан новый документ";
        await Task.CompletedTask;
    }

    /// <summary>
    /// Открыть файл (Ctrl+O)
    /// </summary>
    [RelayCommand]
    private async Task OpenFileAsync()
    {
        try
        {
            var result = _dialogService.ShowOpenFileDialog(Settings.LastDirectory);
            if (result == null) return;

            var (filePath, _) = result.Value;

            // Проверяем, не открыт ли уже этот файл
            var existing = Documents.FirstOrDefault(d => d.FilePath == filePath);
            if (existing != null)
            {
                ActiveDocument = existing;
                StatusMessage = $"Файл уже открыт: {Path.GetFileName(filePath)}";
                return;
            }

            var (content, encoding) = await _fileService.ReadFileAsync(filePath!);

            var model = DocumentModel.FromFile(filePath!, content, encoding);
            var doc = new DocumentViewModel(model);
            doc.LoadFromModel();

            Documents.Add(doc);
            ActiveDocument = doc;

            Settings.LastDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
            StatusMessage = $"Открыт: {Path.GetFileName(filePath)} ({encoding.WebName})";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Ошибка открытия файла: {ex.Message}");
            StatusMessage = "Ошибка открытия файла";
        }
    }

    /// <summary>
    /// Сохранить файл (Ctrl+S)
    /// </summary>
    [RelayCommand]
    private async Task SaveFileAsync()
    {
        if (ActiveDocument == null) return;

        try
        {
            if (ActiveDocument.IsNew || string.IsNullOrEmpty(ActiveDocument.FilePath))
            {
                await SaveFileAsAsync();
                return;
            }

            var encoding = Settings.GetEncoding();
            await _fileService.WriteFileAsync(ActiveDocument.FilePath, ActiveDocument.Content, encoding);
            ActiveDocument.IsModified = false;
            StatusMessage = $"Сохранён: {Path.GetFileName(ActiveDocument.FilePath)}";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Ошибка сохранения: {ex.Message}");
            StatusMessage = "Ошибка сохранения";
        }
    }

    /// <summary>
    /// Сохранить как (Ctrl+Shift+S)
    /// </summary>
    [RelayCommand]
    private async Task SaveFileAsAsync()
    {
        if (ActiveDocument == null) return;

        try
        {
            var fileName = ActiveDocument.IsNew ? "" : Path.GetFileName(ActiveDocument.FilePath);
            var path = _dialogService.ShowSaveFileDialog(fileName, Settings.LastDirectory);

            if (path == null) return;

            var encoding = Settings.GetEncoding();
            await _fileService.WriteFileAsync(path, ActiveDocument.Content, encoding);

            ActiveDocument.FilePath = path;
            ActiveDocument.DisplayName = Path.GetFileName(path);
            ActiveDocument.IsModified = false;
            ActiveDocument.SelectedEncoding = encoding.WebName;

            Settings.LastDirectory = Path.GetDirectoryName(path) ?? string.Empty;
            StatusMessage = $"Сохранён как: {Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Ошибка сохранения: {ex.Message}");
            StatusMessage = "Ошибка сохранения";
        }
    }

    /// <summary>
    /// Закрыть активный документ
    /// </summary>
    [RelayCommand]
    private void CloseActiveDocument()
    {
        if (ActiveDocument == null) return;

        if (ActiveDocument.IsModified)
        {
            var result = _dialogService.ShowSaveChangesDialog(ActiveDocument.DisplayName);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    _ = SaveFileAsync();
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }
        }

        var index = Documents.IndexOf(ActiveDocument);
        Documents.Remove(ActiveDocument);

        if (Documents.Count > 0)
        {
            ActiveDocument = Documents[Math.Min(index, Documents.Count - 1)];
        }
        else
        {
            ActiveDocument = null;
        }

        StatusMessage = "Вкладка закрыта";
    }

    /// <summary>
    /// Открыть панель поиска (Ctrl+F)
    /// </summary>
    [RelayCommand]
    private void ToggleSearch(bool replaceMode = false)
    {
        IsReplaceMode = replaceMode;
        IsSearchPanelVisible = !IsSearchPanelVisible || IsReplaceMode != replaceMode;

        if (IsSearchPanelVisible && ActiveDocument != null)
        {
            StatusMessage = replaceMode ? "Режим поиска и замены" : "Поиск";
        }
    }

    /// <summary>
    /// Найти следующее вхождение
    /// </summary>
    [RelayCommand]
    private void FindNext()
    {
        StatusMessage = $"Поиск: «{SearchText}»";
    }

    /// <summary>
    /// Найти предыдущее вхождение
    /// </summary>
    [RelayCommand]
    private void FindPrevious()
    {
        StatusMessage = $"Поиск назад: «{SearchText}»";
    }

    /// <summary>
    /// Заменить текущее вхождение
    /// </summary>
    [RelayCommand]
    private void ReplaceCurrent()
    {
        StatusMessage = $"Замена: «{SearchText}» → «{ReplaceText}»";
    }

    /// <summary>
    /// Заменить все вхождения
    /// </summary>
    [RelayCommand]
    private void ReplaceAll()
    {
        StatusMessage = $"Замена всех: «{SearchText}» → «{ReplaceText}»";
    }

    /// <summary>
    /// Перейти к строке (Ctrl+G)
    /// </summary>
    [RelayCommand]
    private void GoToLine()
    {
        StatusMessage = "Переход к строке";
    }

    /// <summary>
    /// Показать настройки
    /// </summary>
    [RelayCommand]
    private void ShowSettings()
    {
        StatusMessage = "Настройки";
    }

    /// <summary>
    /// Переключить тему
    /// </summary>
    [RelayCommand]
    private void SwitchTheme(string themeName)
    {
        CurrentTheme = themeName;
        Settings.Theme = themeName;
        _settingsService.Save(Settings);
        StatusMessage = $"Тема: {themeName}";
    }

    /// <summary>
    /// Переключить на следующую вкладку
    /// </summary>
    [RelayCommand]
    private void SwitchToNextDocument()
    {
        if (Documents.Count < 2) return;
        var idx = Documents.IndexOf(ActiveDocument!);
        ActiveDocument = Documents[(idx + 1) % Documents.Count];
    }

    /// <summary>
    /// Переключить на предыдущую вкладку
    /// </summary>
    [RelayCommand]
    private void SwitchToPreviousDocument()
    {
        if (Documents.Count < 2) return;
        var idx = Documents.IndexOf(ActiveDocument!);
        ActiveDocument = Documents[(idx - 1 + Documents.Count) % Documents.Count];
    }

    /// <summary>
    /// Открыть/закрыть командную палитру (Ctrl+Shift+P)
    /// </summary>
    [RelayCommand]
    private void ToggleCommandPalette()
    {
        IsCommandPaletteVisible = !IsCommandPaletteVisible;
        if (IsCommandPaletteVisible)
        {
            CommandPaletteFilter = string.Empty;
            UpdateFilteredCommands();
            StatusMessage = "Командная палитра";
        }
    }

    /// <summary>
    /// Обновить список отфильтрованных команд
    /// </summary>
    partial void OnCommandPaletteFilterChanged(string value)
    {
        UpdateFilteredCommands();
    }

    private void UpdateFilteredCommands()
    {
        FilteredCommands.Clear();
        var filtered = string.IsNullOrEmpty(CommandPaletteFilter)
            ? _allCommands
            : _allCommands.Where(c => c.Name.Contains(CommandPaletteFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var cmd in filtered)
            FilteredCommands.Add(cmd);
    }

    /// <summary>
    /// Выполнить команду из палитры
    /// </summary>
    [RelayCommand]
    private void ExecuteCommand(CommandItem command)
    {
        IsCommandPaletteVisible = false;
        command.Action?.Invoke();
    }

    /// <summary>
    /// Получить текст статус-бара для текущего документа
    /// </summary>
    public string StatusBarText
    {
        get
        {
            if (ActiveDocument == null) return "Нет открытых документов";
            return $"Строка {ActiveDocument.CursorLine}, столбец {ActiveDocument.CursorColumn} | " +
                   $"Строк: {ActiveDocument.TotalLines} | " +
                   $"{ActiveDocument.SelectedEncoding} | " +
                   $"Синтаксис: {ActiveDocument.SyntaxHighlighting}";
        }
    }

    partial void OnActiveDocumentChanged(DocumentViewModel? value)
    {
        OnPropertyChanged(nameof(StatusBarText));
        StatusMessage = value != null
            ? $"Активно: {value.DisplayName}"
            : "Нет открытых документов";
    }

    private void AutoSaveTimer_Tick(object? sender, EventArgs e)
    {
        if (Settings.AutoSaveEnabled && ActiveDocument?.IsModified == true)
        {
            _ = SaveFileAsync();
        }
    }

    /// <summary>
    /// Сохранить настройки и закрыть
    /// </summary>
    public void Shutdown()
    {
        _autoSaveTimer.Stop();
        Settings = SettingsViewModel.GetSettings();
        _settingsService.Save(Settings);
    }
}

/// <summary>
/// Элемент командной палитры
/// </summary>
public class CommandItem
{
    public string Name { get; }
    public string Shortcut { get; }
    public Action? Action { get; }

    public CommandItem(string name, string shortcut, Action? action)
    {
        Name = name;
        Shortcut = shortcut;
        Action = action;
    }
}
