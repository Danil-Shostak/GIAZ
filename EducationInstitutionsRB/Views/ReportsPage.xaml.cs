using EducationInstitutionsRB.Models;
using EducationInstitutionsRB.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.Views;

public sealed partial class ReportsPage : Page
{
    private readonly IDataService _dataService;
    private List<Institution> _allInstitutions = new();
    private List<Region> _regions = new();
    private List<District> _districts = new();

    public ReportsPage()
    {
        this.InitializeComponent();
        _dataService = App.GetService<IDataService>();
        Loaded += ReportsPage_Loaded;

        // Обработчики событий
        RegionCombo.SelectionChanged += RegionCombo_SelectionChanged;
        DistrictCombo.SelectionChanged += DistrictCombo_SelectionChanged;
        ReportTypeCombo.SelectedIndex = 0; // Основной отчет по умолчанию
    }

    private async void ReportsPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadRegionsAsync();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadRegionsAsync();
    }

    private async Task LoadRegionsAsync()
    {
        try
        {
            _regions = await _dataService.GetRegionsAsync();
            RegionCombo.ItemsSource = _regions;

            // Загружаем все учреждения для отчетов
            _allInstitutions = await _dataService.GetInstitutionsAsync();
        }
        catch (Exception ex)
        {
            ShowNoDataMessage("Ошибка загрузки данных");
        }
    }

    private async void RegionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RegionCombo.SelectedItem is Region selectedRegion)
        {
            try
            {
                _districts = await _dataService.GetDistrictsByRegionAsync(selectedRegion.Id);
                DistrictCombo.ItemsSource = _districts;
                DistrictCombo.IsEnabled = true;
                DistrictCombo.SelectedIndex = -1;

                // Показываем кнопку очистки области
                ClearRegionButton.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ShowNoDataMessage("Ошибка загрузки районов");
            }
        }
        else
        {
            DistrictCombo.ItemsSource = null;
            DistrictCombo.IsEnabled = false;
            ClearRegionButton.Visibility = Visibility.Collapsed;
        }

        UpdateReportInfo();
    }

    private void DistrictCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DistrictCombo.SelectedItem is District)
        {
            ClearDistrictButton.Visibility = Visibility.Visible;
        }
        else
        {
            ClearDistrictButton.Visibility = Visibility.Collapsed;
        }
        UpdateReportInfo();
    }

    private void AllRepublicCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        // Отключаем выбор области и района при выборе всей республики
        RegionCombo.IsEnabled = false;
        DistrictCombo.IsEnabled = false;
        RegionCombo.SelectedIndex = -1;
        DistrictCombo.SelectedIndex = -1;
        ClearRegionButton.Visibility = Visibility.Collapsed;
        ClearDistrictButton.Visibility = Visibility.Collapsed;

        UpdateReportInfo();
    }

    private void AllRepublicCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        // Включаем выбор области обратно
        RegionCombo.IsEnabled = true;
        if (RegionCombo.SelectedItem != null)
        {
            DistrictCombo.IsEnabled = true;
        }

        UpdateReportInfo();
    }

    private void ReportTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateReportInfo();
    }

    private void UpdateReportInfo()
    {
        var reportType = ReportTypeCombo.SelectedItem as string;
        var region = RegionCombo.SelectedItem as Region;
        var district = DistrictCombo.SelectedItem as District;

        if (AllRepublicCheckBox.IsChecked == true)
        {
            ReportInfoText.Text = $"{reportType} - Вся Республика Беларусь";
            return;
        }

        if (region == null)
        {
            ReportInfoText.Text = "Выберите параметры для генерации отчёта";
            return;
        }

        var info = $"{reportType}";

        if (district != null)
        {
            info += $", Район: {district.Name}";
        }
        else
        {
            info += $", Область: {region.Name}";
        }

        ReportInfoText.Text = info;
    }

    // Кнопки очистки отдельных полей
    private void ClearRegionButton_Click(object sender, RoutedEventArgs e)
    {
        RegionCombo.SelectedIndex = -1;
        ClearRegionButton.Visibility = Visibility.Collapsed;
    }

    private void ClearDistrictButton_Click(object sender, RoutedEventArgs e)
    {
        DistrictCombo.SelectedIndex = -1;
        ClearDistrictButton.Visibility = Visibility.Collapsed;
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        // Очистка всех полей ввода
        AllRepublicCheckBox.IsChecked = false;
        RegionCombo.SelectedIndex = -1;
        DistrictCombo.SelectedIndex = -1;
        ReportTypeCombo.SelectedIndex = 0;

        // Очистка кнопок очистки
        ClearRegionButton.Visibility = Visibility.Collapsed;
        ClearDistrictButton.Visibility = Visibility.Collapsed;

        // Очистка результатов
        HideAllReports();
        ClearTotals();

        // Сброс информации
        ReportInfoText.Text = "Выберите параметры для генерации отчёта";
        NoDataMessage.Visibility = Visibility.Collapsed;
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        await GenerateReportAsync();
    }

    private async Task GenerateReportAsync()
    {
        try
        {
            LoadingProgress.IsActive = true;
            NoDataMessage.Visibility = Visibility.Collapsed;
            HideAllReports();

            List<Institution> institutionsList;

            if (AllRepublicCheckBox.IsChecked == true)
            {
                // Вся республика
                institutionsList = _allInstitutions;
            }
            else
            {
                var region = RegionCombo.SelectedItem as Region;
                if (region == null)
                {
                    ShowNoDataMessage("Выберите область для генерации отчёта");
                    return;
                }

                // Фильтруем учреждения по выбранным параметрам
                var filteredInstitutions = _allInstitutions.AsEnumerable();

                if (DistrictCombo.SelectedItem is District district)
                {
                    filteredInstitutions = filteredInstitutions.Where(i => i.DistrictId == district.Id);
                }
                else
                {
                    // Все учреждения выбранной области
                    var regionDistricts = await _dataService.GetDistrictsByRegionAsync(region.Id);
                    var districtIds = regionDistricts.Select(d => d.Id).ToList();
                    filteredInstitutions = filteredInstitutions.Where(i => districtIds.Contains(i.DistrictId));
                }

                institutionsList = filteredInstitutions.ToList();
            }

            if (!institutionsList.Any())
            {
                ShowNoDataMessage("Нет учреждений для выбранных параметров");
                return;
            }

            // Показываем соответствующий отчет
            var reportType = ReportTypeCombo.SelectedIndex;
            switch (reportType)
            {
                case 0: // 📊 Основной отчёт
                    ShowBasicReport(institutionsList);
                    break;
                case 1: // 🏫 Детальная информация
                    ShowDetailedReport(institutionsList);
                    break;
                case 2: // 💻 Инфраструктура
                    ShowInfrastructureReport(institutionsList);
                    break;
                case 3: // 👥 Персонал
                    ShowStaffReport(institutionsList);
                    break;
            }

        }
        catch (Exception ex)
        {
            ShowNoDataMessage("Ошибка при генерации отчёта");
        }
        finally
        {
            LoadingProgress.IsActive = false;
        }
    }

    private void ShowBasicReport(List<Institution> institutions)
    {
        BasicReportPanel.Visibility = Visibility.Visible;
        BasicReportListView.ItemsSource = institutions;
        UpdateBasicTotals(institutions);
    }

    private void ShowDetailedReport(List<Institution> institutions)
    {
        DetailedReportPanel.Visibility = Visibility.Visible;
        DetailedReportListView.ItemsSource = institutions;
    }

    private void ShowInfrastructureReport(List<Institution> institutions)
    {
        InfrastructureReportPanel.Visibility = Visibility.Visible;
        InfrastructureReportListView.ItemsSource = institutions;
    }

    private void ShowStaffReport(List<Institution> institutions)
    {
        StaffReportPanel.Visibility = Visibility.Visible;
        StaffReportListView.ItemsSource = institutions;
        UpdateStaffTotals(institutions);
    }

    private void HideAllReports()
    {
        BasicReportPanel.Visibility = Visibility.Collapsed;
        DetailedReportPanel.Visibility = Visibility.Collapsed;
        InfrastructureReportPanel.Visibility = Visibility.Collapsed;
        StaffReportPanel.Visibility = Visibility.Collapsed;

        BasicReportListView.ItemsSource = null;
        DetailedReportListView.ItemsSource = null;
        InfrastructureReportListView.ItemsSource = null;
        StaffReportListView.ItemsSource = null;
    }

    private void UpdateBasicTotals(List<Institution> institutions)
    {
        var totalStudents = institutions.Sum(i => i.StudentCount);
        var totalTeachers = institutions.Sum(i => i.TeacherCount);
        var totalClassrooms = institutions.Sum(i => i.ClassroomCount);

        var avgRatio = totalTeachers > 0 ? (double)totalStudents / totalTeachers : 0;

        TotalStudentsText.Text = totalStudents.ToString("N0");
        TotalTeachersText.Text = totalTeachers.ToString("N0");
        TotalClassroomsText.Text = totalClassrooms.ToString("N0");
        TotalRatioText.Text = avgRatio.ToString("F1");
    }

    private void UpdateStaffTotals(List<Institution> institutions)
    {
        var totalTeachers = institutions.Sum(i => i.TeacherCount);
        var totalAdminStaff = institutions.Sum(i => i.AdministrativeStaffCount);
        var totalStaff = institutions.Sum(i => i.StaffCount);
        var totalAdmitted = institutions.Sum(i => i.AdmittedCount);
        var totalExpelled = institutions.Sum(i => i.ExpelledCount);

        // Можно добавить отображение этих итогов если нужно
    }

    private void ClearTotals()
    {
        TotalStudentsText.Text = "0";
        TotalTeachersText.Text = "0";
        TotalClassroomsText.Text = "0";
        TotalRatioText.Text = "0";
    }

    private void ShowNoDataMessage(string message)
    {
        HideAllReports();
        NoDataMessage.Visibility = Visibility.Visible;
        NoDataMessageText.Text = message;
        ClearTotals();
    }
}