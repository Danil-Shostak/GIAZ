using EducationInstitutionsRB.Models;
using EducationInstitutionsRB.Services;
using EducationInstitutionsRB.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.Views;

public sealed partial class OverviewPage : Page
{
    private readonly IDataService _dataService;
    private OverviewViewModel _viewModel;

    public OverviewPage()
    {
        try
        {
            Debug.WriteLine("OverviewPage конструктор начат");
            this.InitializeComponent();
            _dataService = App.GetService<IDataService>();
            _viewModel = new OverviewViewModel(_dataService);
            Debug.WriteLine("OverviewPage создана успешно");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка в конструкторе OverviewPage: {ex.Message}");
            throw;
        }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        try
        {
            Debug.WriteLine("OverviewPage OnNavigatedTo начат");
            base.OnNavigatedTo(e);
            await LoadDataAsync();
            Debug.WriteLine("OverviewPage данные загружены");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка в OnNavigatedTo: {ex.Message}");
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            LoadingProgress.IsActive = true;
            RecentInstitutionsList.Visibility = Visibility.Collapsed;
            NoDataText.Visibility = Visibility.Collapsed;

            // Загружаем данные через ViewModel
            await _viewModel.LoadDataAsync();

            // Обновляем UI
            TotalInstitutionsText.Text = _viewModel.TotalInstitutionsDisplay;
            TotalStudentsText.Text = _viewModel.TotalStudentsDisplay;
            TotalStaffText.Text = _viewModel.TotalTeachersDisplay;
            SuccessRateText.Text = _viewModel.SuccessRateDisplay;

            // Обновляем список недавних учреждений
            if (_viewModel.RecentInstitutions.Any())
            {
                RecentInstitutionsList.ItemsSource = _viewModel.RecentInstitutions;
                RecentInstitutionsList.Visibility = Visibility.Visible;
                NoDataText.Visibility = Visibility.Collapsed;
            }
            else
            {
                RecentInstitutionsList.Visibility = Visibility.Collapsed;
                NoDataText.Visibility = Visibility.Visible;
            }

            LoadingProgress.IsActive = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            LoadingProgress.IsActive = false;
        }
    }

    private async void AddInstitutionButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Кнопка 'Добавить учреждение' нажата на стартовой странице");

        if (App.MainWindow?.Content?.XamlRoot == null)
        {
            Debug.WriteLine("XamlRoot не доступен");
            return;
        }

        try
        {
            var newInstitution = new Institution
            {
                RegistrationDate = DateTime.Now,
                Status = "Активно",
                InstitutionStatus = "Действующее",
                OwnershipType = "Государственное",
                LanguageOfEducation = "Русский",
                FoundationYear = DateTime.Now.Year
            };

            var dialog = new ExtendedInstitutionDialog(newInstitution, "Добавить учреждение")
            {
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    await _dataService.AddInstitutionAsync(newInstitution);
                    Debug.WriteLine("Учреждение добавлено, перезагружаем данные");

                    await LoadDataAsync();

                    var dialogService = App.GetService<DialogService>();
                    await dialogService.ShowSuccessAsync("Учреждение успешно добавлено!", App.MainWindow.Content.XamlRoot);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при добавлении учреждения: {ex.Message}");
                    var dialogService = App.GetService<DialogService>();
                    await dialogService.ShowErrorAsync($"Ошибка при добавлении: {ex.Message}", App.MainWindow.Content.XamlRoot);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при открытии диалога добавления: {ex.Message}");
            var dialogService = App.GetService<DialogService>();
            await dialogService.ShowErrorAsync($"Ошибка: {ex.Message}", App.MainWindow.Content.XamlRoot);
        }
    }

    private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Кнопка 'Создать отчет' нажата");

        // Навигация на страницу отчетов
        if (Frame != null)
        {
            Frame.Navigate(typeof(ReportsPage));
        }
    }
}