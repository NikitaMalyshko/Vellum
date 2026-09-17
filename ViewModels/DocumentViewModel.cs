using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text;
using Vellum.Models;

namespace Vellum.ViewModels;

/// <summary>
/// ViewModel для одного открытого документа/вкладки
/// </summary>
public partial class DocumentViewModel : ObservableObject
{
    private readonly DocumentModel _model;

    public DocumentViewModel(DocumentModel model)
    {
        _model = model;
    }

    public DocumentViewModel() : this(new DocumentModel()) { }

    public Guid Id => _model.Id;

    public string FilePath
    {
        get => _model.FilePath;
        set
        {
            _model.FilePath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayTitle));
            OnPropertyChanged(nameof(IsNew));
        }
    }

    public bool IsNew => _model.IsNew;

    public string SyntaxHighlighting
    {
        get => _model.SyntaxHighlighting;
        set
        {
            _model.SyntaxHighlighting = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private bool _isModified;

    [ObservableProperty]
    private string _displayName = "Новый документ";

    [ObservableProperty]
    private int _cursorLine = 1;

    [ObservableProperty]
    private int _cursorColumn = 1;

    [ObservableProperty]
    private int _totalLines = 0;

    [ObservableProperty]
    private string _selectedEncoding = "UTF-8";

    /// <summary>Делегат Undo (вызывается из View)</summary>
    public Action? Undo { get; set; }

    /// <summary>Делегат Redo (вызывается из View)</summary>
    public Action? Redo { get; set; }

    /// <summary>Отображаемое название вкладки с индикатором изменений</summary>
    public string DisplayTitle => IsModified ? $"*{DisplayName}" : DisplayName;

    partial void OnContentChanged(string value)
    {
        if (!IsModified && value != string.Empty)
        {
            IsModified = true;
            OnPropertyChanged(nameof(DisplayTitle));
        }
    }

    partial void OnIsModifiedChanged(bool value)
    {
        OnPropertyChanged(nameof(DisplayTitle));
    }

    partial void OnDisplayNameChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayTitle));
    }

    /// <summary>
    /// Обновить модель из ViewModel
    /// </summary>
    public DocumentModel ToModel()
    {
        _model.Content = Content;
        _model.IsModified = IsModified;
        _model.Encoding = SelectedEncoding;
        return _model;
    }

    /// <summary>
    /// Загрузить данные из модели
    /// </summary>
    public void LoadFromModel()
    {
        Content = _model.Content;
        DisplayName = _model.DisplayName;
        IsModified = _model.IsModified;
        SelectedEncoding = _model.Encoding;
        SyntaxHighlighting = _model.SyntaxHighlighting;
    }
}
