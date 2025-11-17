using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducationInstitutionsRB.Models;
using EducationInstitutionsRB.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.ViewModels;

public partial class OverviewViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private int _totalInstitutions;

    [ObservableProperty]
    private int _totalStudents;

    [ObservableProperty]
    private int _totalTeachers;

    [ObservableProperty]
    private int _totalStaff; // ВОССТАНАВЛИВАЕМ это свойство

    [ObservableProperty]
    private double _successRate = 98.2;

    [ObservableProperty]
    private List<Institution> _recentInstitutions = new();

    [ObservableProperty]
    private bool _isLoading;

    // Вычисляемые свойства для форматирования
    public string SuccessRateDisplay => $"{SuccessRate:F1}%";
    public string TotalInstitutionsDisplay => TotalInstitutions.ToString("N0");
    public string TotalStudentsDisplay => TotalStudents.ToString("N0");
    public string TotalTeachersDisplay => TotalTeachers.ToString("N0");
    public string TotalStaffDisplay => TotalStaff.ToString("N0"); // ВОССТАНАВЛИВАЕМ это свойство

    public OverviewViewModel(IDataService dataService)
    {
        Debug.WriteLine("OverviewViewModel создан");
        _dataService = dataService;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            Debug.WriteLine("Начало загрузки данных для Overview");

            var institutions = await _dataService.GetInstitutionsAsync();
            Debug.WriteLine($"Загружено учреждений: {institutions.Count}");

            // ОТЛАДОЧНАЯ ИНФОРМАЦИЯ: выводим данные по каждому учреждению
            foreach (var institution in institutions)
            {
                Debug.WriteLine($"Учреждение: {institution.Name}, Учащихся: {institution.StudentCount}, Преподавателей: {institution.TeacherCount}, Персонала: {institution.StaffCount}");
            }

            // Обновляем статистику
            TotalInstitutions = institutions.Count;
            TotalStudents = institutions.Sum(i => i.StudentCount);
            TotalTeachers = institutions.Sum(i => i.TeacherCount);
            TotalStaff = institutions.Sum(i => i.StaffCount); // ВОССТАНАВЛИВАЕМ подсчет персонала

            Debug.WriteLine($"ИТОГО: Учреждений={TotalInstitutions}, Учащихся={TotalStudents}, Преподавателей={TotalTeachers}, Персонала={TotalStaff}");

            // Последние добавленные учреждения (последние 3)
            RecentInstitutions = institutions
                .OrderByDescending(i => i.RegistrationDate)
                .Take(3)
                .ToList();

            Debug.WriteLine($"Недавних учреждений: {RecentInstitutions.Count}");

            // Уведомляем об изменении вычисляемых свойств
            OnPropertyChanged(nameof(TotalInstitutionsDisplay));
            OnPropertyChanged(nameof(TotalStudentsDisplay));
            OnPropertyChanged(nameof(TotalTeachersDisplay));
            OnPropertyChanged(nameof(TotalStaffDisplay));
            OnPropertyChanged(nameof(SuccessRateDisplay));
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки данных в OverviewViewModel: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}