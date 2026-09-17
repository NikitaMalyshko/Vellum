using System.Windows;

namespace Vellum;

/// <summary>
/// Логика приложения Vellum — текстовый редактор
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Обработка неhandled исключений
        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show(
                $"Произошла непредвиденная ошибка:\n\n{args.Exception.Message}",
                "Ошибка приложения",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}
