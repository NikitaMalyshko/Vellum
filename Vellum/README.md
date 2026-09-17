# Vellum — Text Editor

A modern text editor built with C# + WPF (.NET 8).

## Features

- **Editing**: typing, deleting, copy, paste, undo/redo
- **Tabs**: open multiple documents simultaneously
- **Search and replace**: with regex support, case sensitivity, whole word matching
- **Dark/Light theme**: switch on the fly
- **Customizable interface**: custom title bar, toolbar, status bar
- **Command palette** (Ctrl+Shift+P): quick access to all commands
- **Line numbers** and code folding
- **Auto-save**: configurable interval
- **Settings persistence**: theme, font, encoding saved to JSON

## Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| Ctrl+N | New document |
| Ctrl+O | Open file |
| Ctrl+S | Save |
| Ctrl+Shift+S | Save as |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Ctrl+F | Find |
| Ctrl+H | Replace |
| Ctrl+G | Go to line |
| Ctrl+W | Close tab |
| Ctrl+Tab | Next tab |
| Ctrl+Shift+Tab | Previous tab |
| Ctrl+Shift+P | Command palette |
| Esc | Close search panel / palette |

## Technologies

- .NET 8 + WPF
- CommunityToolkit.Mvvm (MVVM)
- AvalonEdit (text editor)
- MaterialDesignThemes (styling)
- Newtonsoft.Json (settings serialization)

## Build & Run

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

Or open `Vellum.sln` in Visual Studio 2022+.

## Project Structure

```
Vellum/
├── App.xaml / App.xaml.cs          — application entry point
├── MainWindow.xaml / .cs           — main window
├── Models/
│   ├── AppSettings.cs              — settings model
│   └── DocumentModel.cs            — document model
├── ViewModels/
│   ├── MainViewModel.cs            — main ViewModel
│   ├── DocumentViewModel.cs        — document ViewModel
│   └── SettingsViewModel.cs        — settings ViewModel
├── Services/
│   ├── FileService.cs              — file I/O
│   ├── SettingsService.cs          — settings load/save
│   └── DialogService.cs            — dialog windows
├── Themes/
│   ├── DarkTheme.xaml              — dark theme
│   ├── LightTheme.xaml             — light theme
│   └── Converters.xaml             — converters
├── Converters/
│   └── Converters.cs               — value converters
└── Resources/
    └── Icons/                      — icons
```
