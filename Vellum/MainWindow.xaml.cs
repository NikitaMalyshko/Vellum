using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using Vellum.ViewModels;

namespace Vellum;

/// <summary>
/// Логика главного окна приложения
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    private bool _isMaximized;
    private Point _lastMousePos;
    private bool _isDragging;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Применяем сохранённую тему
        ApplyTheme(ViewModel.CurrentTheme);
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Запускаем анимацию появления
        var fadeIn = (System.Windows.Media.Animation.Storyboard)FindResource("FadeInStoryboard");
        fadeIn.Begin(this);
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        // Проверяем несохранённые изменения
        foreach (var doc in ViewModel.Documents)
        {
            if (doc.IsModified)
            {
                var result = MessageBox.Show(
                    $"Есть несохранённые изменения в «{doc.DisplayName}». Выйти без сохранения?",
                    "Несохранённые изменения",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == MessageBoxResult.Yes)
                {
                    // Сохраняем перед выходом
                    doc.ToModel();
                }
            }
        }

        ViewModel.Shutdown();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentTheme))
        {
            ApplyTheme(ViewModel.CurrentTheme);
        }
    }

    /// <summary>
    /// Применить тему оформления
    /// </summary>
    private void ApplyTheme(string themeName)
    {
        var themeUri = themeName == "Light"
            ? new Uri("Themes/LightTheme.xaml", UriKind.Relative)
            : new Uri("Themes/DarkTheme.xaml", UriKind.Relative);

        var resourceDict = new ResourceDictionary { Source = themeUri };

        // Сохраняем конвертеры
        var converters = Application.Current.Resources.MergedDictionaries
            .OfType<ResourceDictionary>()
            .FirstOrDefault(d => d.Source?.ToString().Contains("Converters") == true);

        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(resourceDict);

        // Восстанавливаем конвертеры
        if (converters != null)
            Application.Current.Resources.MergedDictionaries.Add(converters);

        // Добавляем конвертеры если их нет
        if (!Application.Current.Resources.MergedDictionaries
            .Any(d => d.Source?.ToString().Contains("Converters") == true))
        {
            var convertersDict = new ResourceDictionary
            {
                Source = new Uri("Themes/Converters.xaml", UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Add(convertersDict);
        }
    }

    #region Заголовок окна — перетаскивание и кнопки

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }
        _isDragging = true;
        _lastMousePos = e.GetPosition(this);
        CaptureMouse();
    }

    private void TitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        ReleaseMouseCapture();
    }

    private void TitleBar_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;

        var pos = e.GetPosition(this);
        var diff = pos - _lastMousePos;

        if (_isMaximized)
        {
            // При восстановлении из развёрнутого состояния
            ToggleMaximize();
            Left += diff.X - Width / 2;
            Top += diff.Y - 18;
        }
        else
        {
            Left += diff.X;
            Top += diff.Y;
        }
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnMaximize_Click(object sender, RoutedEventArgs e)
    {
        ToggleMaximize();
    }

    private void ToggleMaximize()
    {
        if (_isMaximized)
        {
            WindowState = WindowState.Normal;
            _isMaximized = false;
        }
        else
        {
            WindowState = WindowState.Maximized;
            _isMaximized = true;
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            _isMaximized = true;
        else if (WindowState == WindowState.Normal)
            _isMaximized = false;
    }

    #endregion

    #region Горячие клавиши

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        if (ctrl && shift && e.Key == Key.P)
        {
            ViewModel.ToggleCommandPaletteCommand.Execute(null);
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.N)
        {
            ViewModel.NewFileCommand.Execute(null);
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.O)
        {
            ViewModel.OpenFileCommand.Execute(null);
            e.Handled = true;
        }
        else if (ctrl && shift && e.Key == Key.S)
        {
            ViewModel.SaveFileAsCommand.Execute(null);
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.S)
        {
            ViewModel.SaveFileCommand.Execute(null);
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.Z)
        {
            MenuUndo_Click(sender, e);
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.Y)
        {
            MenuRedo_Click(sender, e);
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.F)
        {
            ViewModel.ToggleSearchCommand.Execute(false);
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.H)
        {
            ViewModel.ToggleSearchCommand.Execute(true);
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.G)
        {
            MenuGoToLine_Click(sender, e);
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.W)
        {
            ViewModel.CloseActiveDocumentCommand.Execute(null);
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.Tab)
        {
            if (shift)
                ViewModel.SwitchToPreviousDocumentCommand.Execute(null);
            else
                ViewModel.SwitchToNextDocumentCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            if (ViewModel.IsSearchPanelVisible)
            {
                ViewModel.IsSearchPanelVisible = false;
                e.Handled = true;
            }
            else if (ViewModel.IsCommandPaletteVisible)
            {
                ViewModel.IsCommandPaletteVisible = false;
                e.Handled = true;
            }
        }
    }

    #endregion

    #region Меню — редактирование

    private void MenuUndo_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        if (editor != null && editor.Document.UndoStack.CanUndo)
            editor.Undo();
    }

    private void MenuRedo_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        if (editor != null && editor.Document.UndoStack.CanRedo)
            editor.Redo();
    }

    private void MenuCut_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        editor?.Cut();
    }

    private void MenuCopy_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        editor?.Copy();
    }

    private void MenuPaste_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        editor?.Paste();
    }

    private void MenuSelectAll_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        if (editor != null)
            editor.SelectAll();
    }

    private void MenuGoToLine_Click(object sender, RoutedEventArgs e)
    {
        var editor = GetActiveEditor();
        if (editor == null) return;

        var input = Microsoft.VisualBasic.Interaction.InputBox(
            "Введите номер строки:", "Перейти к строке",
            (editor.TextArea.Caret.Line).ToString());

        if (int.TryParse(input, out int line) && line >= 1 && line <= editor.Document.LineCount)
        {
            editor.TextArea.Caret.Line = line;
            editor.TextArea.Caret.Column = 0;
            editor.ScrollToLine(line);
        }
    }

    private void MenuReplace_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleSearchCommand.Execute(true);
    }

    private void SearchFromContext_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleSearchCommand.Execute(false);
    }

    #endregion

    #region Меню — вид и тема

    private void MenuLineNumbers_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.MenuItem item)
        {
            ViewModel.SettingsViewModel.ShowLineNumbers = item.IsChecked;
        }
    }

    private void MenuCodeFolding_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.MenuItem item)
        {
            ViewModel.SettingsViewModel.EnableCodeFolding = item.IsChecked;
        }
    }

    private void MenuDarkTheme_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SwitchThemeCommand.Execute("Dark");
    }

    private void MenuLightTheme_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SwitchThemeCommand.Execute("Light");
    }

    private void MenuExit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MenuAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Vellum — Текстовый редактор\nВерсия 1.0\n\n" +
            "Создан с использованием:\n" +
            "• .NET 8 + WPF\n" +
            "• AvalonEdit\n" +
            "• CommunityToolkit.Mvvm\n" +
            "• MaterialDesign Themes\n\n" +
            "© 2026 Vellum",
            "О программе",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    #endregion

    #region Вкладки

    private void TabClose_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBlock tb && tb.Tag is DocumentViewModel doc)
        {
            ViewModel.ActiveDocument = doc;
            ViewModel.CloseActiveDocumentCommand.Execute(null);
        }
    }

    #endregion

    #region Редактор

    private void Editor_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextEditor editor && DataContext is MainViewModel vm)
        {
            // Обновляем активный документ при фокусе
            var tabItem = FindParentTabItem(editor);
            if (tabItem?.DataContext is DocumentViewModel doc)
            {
                vm.ActiveDocument = doc;
            }
        }
    }

    private void Editor_TextChanged(object sender, EventArgs e)
    {
        if (sender is TextEditor editor && DataContext is MainViewModel vm && vm.ActiveDocument != null)
        {
            var line = editor.TextArea.Caret.Line;
            var col = editor.TextArea.Caret.Column;
            vm.ActiveDocument.CursorLine = line;
            vm.ActiveDocument.CursorColumn = col;
            vm.ActiveDocument.TotalLines = editor.Document.LineCount;
        }
    }

    private TextEditor? GetActiveEditor()
    {
        if (DocumentTabs?.SelectedContent is System.Windows.Controls.ContentPresenter cp)
        {
            return FindVisualChild<TextEditor>(cp);
        }
        return null;
    }

    private static System.Windows.Controls.TabItem? FindParentTabItem(DependencyObject child)
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is System.Windows.Controls.TabItem tabItem)
                return tabItem;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed)
                return typed;
            var result = FindVisualChild<T>(child);
            if (result != null)
                return result;
        }
        return null;
    }

    #endregion

    #region Поиск

    private void CloseSearchPanel_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.IsSearchPanelVisible = false;
    }

    #endregion

    #region Командная палитра

    private void Overlay_Click(object sender, MouseButtonEventArgs e)
    {
        ViewModel.IsCommandPaletteVisible = false;
    }

    private void CommandPalette_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Предотвращаем закрытие при клике на саму палитру
        e.Handled = true;
    }

    #endregion
}
