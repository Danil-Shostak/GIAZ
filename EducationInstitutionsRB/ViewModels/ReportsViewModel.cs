using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducationInstitutionsRB.Models;
using EducationInstitutionsRB.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private List<Institution> _allInstitutions = new();

    [ObservableProperty]
    private ObservableCollection<ReportItem> _reportData = new();

    [ObservableProperty]
    private ReportType _selectedReportType = ReportType.ByRegion;

    [ObservableProperty]
    private ReportScale _selectedReportScale = ReportScale.Country;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _reportTitle = "Отчёт по регионам";

    [ObservableProperty]
    private string _generatedDate;

    [ObservableProperty]
    private int _totalInstitutions;

    [ObservableProperty]
    private int _totalStudents;

    [ObservableProperty]
    private int _totalTeachers;

    [ObservableProperty]
    private int _totalStaff;

    public List<ReportType> ReportTypes { get; } = Enum.GetValues(typeof(ReportType)).Cast<ReportType>().ToList();
    public List<ReportScale> ReportScales { get; } = Enum.GetValues(typeof(ReportScale)).Cast<ReportScale>().ToList();

    public ReportsViewModel(IDataService dataService)
    {
        _dataService = dataService;
        GeneratedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        _ = LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            AllInstitutions = await _dataService.GetInstitutionsAsync();
            await GenerateReportAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки данных для отчётов: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedReportTypeChanged(ReportType value)
    {
        _ = GenerateReportAsync();
    }

    partial void OnSelectedReportScaleChanged(ReportScale value)
    {
        _ = GenerateReportAsync();
    }

    [RelayCommand]
    public async Task GenerateReportAsync()
    {
        try
        {
            IsLoading = true;
            ReportData.Clear();
            GeneratedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            switch (SelectedReportType)
            {
                case ReportType.ByRegion:
                    await GenerateRegionReportAsync();
                    ReportTitle = "Отчёт по регионам";
                    break;
                case ReportType.ByDistrict:
                    await GenerateDistrictReportAsync();
                    ReportTitle = "Отчёт по районам";
                    break;
                case ReportType.ByType:
                    await GenerateTypeReportAsync();
                    ReportTitle = "Отчёт по типам учреждений";
                    break;
                case ReportType.Statistics:
                    await GenerateStatisticsReportAsync();
                    ReportTitle = "Статистический отчёт";
                    break;
                case ReportType.Infrastructure:
                    await GenerateInfrastructureReportAsync();
                    ReportTitle = "Отчёт по инфраструктуре";
                    break;
                case ReportType.Financial:
                    await GenerateFinancialReportAsync();
                    ReportTitle = "Финансовый отчёт";
                    break;
            }

            UpdateTotals();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка генерации отчёта: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task GenerateRegionReportAsync()
    {
        var regions = await _dataService.GetRegionsAsync();

        foreach (var region in regions)
        {
            var regionInstitutions = AllInstitutions
                .Where(i => i.District?.RegionId == region.Id)
                .ToList();

            if (regionInstitutions.Any())
            {
                ReportData.Add(new ReportItem
                {
                    Category = region.Name,
                    InstitutionCount = regionInstitutions.Count,
                    StudentCount = regionInstitutions.Sum(i => i.StudentCount),
                    TeacherCount = regionInstitutions.Sum(i => i.TeacherCount),
                    StaffCount = regionInstitutions.Sum(i => i.StaffCount),
                    AdmittedCount = regionInstitutions.Sum(i => i.AdmittedCount),
                    ExpelledCount = regionInstitutions.Sum(i => i.ExpelledCount),
                    ComputerCount = regionInstitutions.Sum(i => i.ComputerCount),
                    ClassroomCount = regionInstitutions.Sum(i => i.ClassroomCount),
                    AvgStudentTeacherRatio = regionInstitutions.Average(i => i.TeacherCount > 0 ? (double)i.StudentCount / i.TeacherCount : 0)
                });
            }
        }
    }

    private async Task GenerateDistrictReportAsync()
    {
        var districts = await _dataService.GetDistrictsAsync();

        foreach (var district in districts)
        {
            var districtInstitutions = AllInstitutions
                .Where(i => i.DistrictId == district.Id)
                .ToList();

            if (districtInstitutions.Any())
            {
                ReportData.Add(new ReportItem
                {
                    Category = $"{district.Name} ({district.Region?.Name})",
                    InstitutionCount = districtInstitutions.Count,
                    StudentCount = districtInstitutions.Sum(i => i.StudentCount),
                    TeacherCount = districtInstitutions.Sum(i => i.TeacherCount),
                    StaffCount = districtInstitutions.Sum(i => i.StaffCount),
                    AdmittedCount = districtInstitutions.Sum(i => i.AdmittedCount),
                    ExpelledCount = districtInstitutions.Sum(i => i.ExpelledCount),
                    ComputerCount = districtInstitutions.Sum(i => i.ComputerCount),
                    ClassroomCount = districtInstitutions.Sum(i => i.ClassroomCount),
                    AvgStudentTeacherRatio = districtInstitutions.Average(i => i.TeacherCount > 0 ? (double)i.StudentCount / i.TeacherCount : 0)
                });
            }
        }
    }

    private Task GenerateTypeReportAsync()
    {
        var types = AllInstitutions.Select(i => i.Type).Distinct();

        foreach (var type in types)
        {
            var typeInstitutions = AllInstitutions.Where(i => i.Type == type).ToList();

            ReportData.Add(new ReportItem
            {
                Category = type,
                InstitutionCount = typeInstitutions.Count,
                StudentCount = typeInstitutions.Sum(i => i.StudentCount),
                TeacherCount = typeInstitutions.Sum(i => i.TeacherCount),
                StaffCount = typeInstitutions.Sum(i => i.StaffCount),
                AdmittedCount = typeInstitutions.Sum(i => i.AdmittedCount),
                ExpelledCount = typeInstitutions.Sum(i => i.ExpelledCount),
                ComputerCount = typeInstitutions.Sum(i => i.ComputerCount),
                ClassroomCount = typeInstitutions.Sum(i => i.ClassroomCount),
                AvgStudentTeacherRatio = typeInstitutions.Average(i => i.TeacherCount > 0 ? (double)i.StudentCount / i.TeacherCount : 0)
            });
        }

        return Task.CompletedTask;
    }

    private Task GenerateStatisticsReportAsync()
    {
        // Общая статистика
        ReportData.Add(new ReportItem
        {
            Category = "Всего учреждений",
            InstitutionCount = AllInstitutions.Count,
            Value = AllInstitutions.Count
        });

        ReportData.Add(new ReportItem
        {
            Category = "Всего учащихся",
            StudentCount = AllInstitutions.Sum(i => i.StudentCount),
            Value = AllInstitutions.Sum(i => i.StudentCount)
        });

        ReportData.Add(new ReportItem
        {
            Category = "Всего преподавателей",
            TeacherCount = AllInstitutions.Sum(i => i.TeacherCount),
            Value = AllInstitutions.Sum(i => i.TeacherCount)
        });

        ReportData.Add(new ReportItem
        {
            Category = "Всего персонала",
            StaffCount = AllInstitutions.Sum(i => i.StaffCount),
            Value = AllInstitutions.Sum(i => i.StaffCount)
        });

        ReportData.Add(new ReportItem
        {
            Category = "Принято учащихся",
            AdmittedCount = AllInstitutions.Sum(i => i.AdmittedCount),
            Value = AllInstitutions.Sum(i => i.AdmittedCount)
        });

        ReportData.Add(new ReportItem
        {
            Category = "Отчислено учащихся",
            ExpelledCount = AllInstitutions.Sum(i => i.ExpelledCount),
            Value = AllInstitutions.Sum(i => i.ExpelledCount)
        });

        ReportData.Add(new ReportItem
        {
            Category = "Среднее соотношение ученик/учитель",
            AvgStudentTeacherRatio = AllInstitutions.Average(i => i.TeacherCount > 0 ? (double)i.StudentCount / i.TeacherCount : 0),
            Value = AllInstitutions.Average(i => i.TeacherCount > 0 ? (double)i.StudentCount / i.TeacherCount : 0)
        });

        return Task.CompletedTask;
    }

    private Task GenerateInfrastructureReportAsync()
    {
        ReportData.Add(new ReportItem
        {
            Category = "Учреждения со спортзалом",
            InstitutionCount = AllInstitutions.Count(i => i.HasSportsHall),
            Value = AllInstitutions.Count(i => i.HasSportsHall)
        });

        ReportData.Add(new ReportItem
        {
            Category = "Учреждения со столовой",
            InstitutionCount = AllInstitutions.Count(i => i.HasDiningRoom),
            Value = AllInstitutions.Count(i => i.HasDiningRoom)
        });

        ReportData.Add(new ReportItem
        {
            Category = "Учреждения с библиотекой",
            InstitutionCount = AllInstitutions.Count(i => i.HasLibrary),
            Value = AllInstitutions.Count(i => i.HasLibrary)
        });

        ReportData.Add(new ReportItem
        {
            Category = "Всего компьютеров",
            ComputerCount = AllInstitutions.Sum(i => i.ComputerCount),
            Value = AllInstitutions.Sum(i => i.ComputerCount)
        });

        ReportData.Add(new ReportItem
        {
            Category = "Всего учебных кабинетов",
            ClassroomCount = AllInstitutions.Sum(i => i.ClassroomCount),
            Value = AllInstitutions.Sum(i => i.ClassroomCount)
        });

        ReportData.Add(new ReportItem
        {
            Category = "Средняя площадь учреждений (м²)",
            Value = (double)AllInstitutions.Average(i => i.TotalArea) // Явное преобразование decimal to double
        });

        return Task.CompletedTask;
    }

    private Task GenerateFinancialReportAsync()
    {
        // Расчетные финансовые показатели (на основе имеющихся данных)
        var avgStudentsPerInstitution = AllInstitutions.Average(i => i.StudentCount);
        var avgTeachersPerInstitution = AllInstitutions.Average(i => i.TeacherCount);

        ReportData.Add(new ReportItem
        {
            Category = "Среднее количество учащихся на учреждение",
            Value = avgStudentsPerInstitution
        });

        ReportData.Add(new ReportItem
        {
            Category = "Среднее количество преподавателей на учреждение",
            Value = avgTeachersPerInstitution
        });

        ReportData.Add(new ReportItem
        {
            Category = "Общее соотношение ученик/учитель",
            Value = AllInstitutions.Sum(i => i.StudentCount) / (double)AllInstitutions.Sum(i => i.TeacherCount)
        });

        ReportData.Add(new ReportItem
        {
            Category = "Процент учреждений с библиотекой",
            Value = (AllInstitutions.Count(i => i.HasLibrary) / (double)AllInstitutions.Count) * 100
        });

        ReportData.Add(new ReportItem
        {
            Category = "Процент учреждений со спортзалом",
            Value = (AllInstitutions.Count(i => i.HasSportsHall) / (double)AllInstitutions.Count) * 100
        });

        return Task.CompletedTask;
    }

    private void UpdateTotals()
    {
        TotalInstitutions = ReportData.Sum(r => r.InstitutionCount);
        TotalStudents = ReportData.Sum(r => r.StudentCount);
        TotalTeachers = ReportData.Sum(r => r.TeacherCount);
        TotalStaff = ReportData.Sum(r => r.StaffCount);
    }

    [RelayCommand]
    public async Task ExportToCsvAsync()
    {
        try
        {
            // Здесь будет логика экспорта в CSV
            await App.GetService<DialogService>().ShowSuccessAsync(
                "Экспорт в CSV будет реализован в следующей версии",
                App.MainWindow?.Content?.XamlRoot);
        }
        catch (Exception ex)
        {
            await App.GetService<DialogService>().ShowErrorAsync(
                $"Ошибка экспорта: {ex.Message}",
                App.MainWindow?.Content?.XamlRoot);
        }
    }

    [RelayCommand]
    public async Task PrintReportAsync()
    {
        try
        {
            // Здесь будет логика печати
            await App.GetService<DialogService>().ShowSuccessAsync(
                "Печать отчёта будет реализована в следующей версии",
                App.MainWindow?.Content?.XamlRoot);
        }
        catch (Exception ex)
        {
            await App.GetService<DialogService>().ShowErrorAsync(
                $"Ошибка печати: {ex.Message}",
                App.MainWindow?.Content?.XamlRoot);
        }
    }
}

public enum ReportType
{
    ByRegion,
    ByDistrict,
    ByType,
    Statistics,
    Infrastructure,
    Financial
}

public enum ReportScale
{
    Country,
    Region,
    District
}

public class ReportItem
{
    public string Category { get; set; } = string.Empty;
    public int InstitutionCount { get; set; }
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int StaffCount { get; set; }
    public int AdmittedCount { get; set; }
    public int ExpelledCount { get; set; }
    public int ComputerCount { get; set; }
    public int ClassroomCount { get; set; }
    public double AvgStudentTeacherRatio { get; set; }
    public double Value { get; set; }

    // Display properties
    public string StudentCountDisplay => StudentCount.ToString("N0");
    public string TeacherCountDisplay => TeacherCount.ToString("N0");
    public string StaffCountDisplay => StaffCount.ToString("N0");
    public string AdmittedCountDisplay => AdmittedCount.ToString("N0");
    public string ExpelledCountDisplay => ExpelledCount.ToString("N0");
    public string ComputerCountDisplay => ComputerCount.ToString("N0");
    public string ClassroomCountDisplay => ClassroomCount.ToString("N0");
    public string RatioDisplay => AvgStudentTeacherRatio.ToString("F1");
    public string ValueDisplay => Value % 1 == 0 ? Value.ToString("N0") : Value.ToString("F2");
    public string InstitutionCountDisplay => InstitutionCount.ToString("N0");
}