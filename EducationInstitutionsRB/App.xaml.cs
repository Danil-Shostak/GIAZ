using EducationInstitutionsRB.Services;
using EducationInstitutionsRB.Views;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EducationInstitutionsRB;

public partial class App : Application
{
    private static Window? _mainWindow;
    public static Window? MainWindow => _mainWindow;
    private static IDataService? _dataService;
    private static DialogService? _dialogService;

    public App()
    {
        this.InitializeComponent();
        this.UnhandledException += App_UnhandledException;

        Debug.WriteLine("=== ПРИЛОЖЕНИЕ ЗАПУЩЕНО ===");

        // Инициализируем сервисы в фоне
        _ = InitializeServicesAsync();
    }

    private async Task InitializeServicesAsync()
    {
        try
        {
            _dataService = new DataService();
            _dialogService = new DialogService();
            Debug.WriteLine("Сервисы инициализированы в фоне");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка инициализации сервисов: {ex.Message}");
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            Debug.WriteLine("OnLaunched: Создаем главное окно");

            // Создаем ОДНО главное окно
            _mainWindow = new MainWindow();
            _mainWindow.Activate();

            Debug.WriteLine("Главное окно создано и активировано");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка в OnLaunched: {ex.Message}");
        }
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine($"Необработанное исключение: {e.Message}");
        e.Handled = true;
    }

    public static T GetService<T>() where T : class
    {
        if (typeof(T) == typeof(IDataService) && _dataService is T dataService)
            return dataService;
        if (typeof(T) == typeof(DialogService) && _dialogService is T dialogService)
            return dialogService;
        throw new InvalidOperationException($"Service {typeof(T)} not registered");
    }
}