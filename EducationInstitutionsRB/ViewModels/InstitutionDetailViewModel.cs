using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducationInstitutionsRB.Models;
using EducationInstitutionsRB.Services;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EducationInstitutionsRB.ViewModels;

public partial class InstitutionDetailViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private Institution? _institution;

    public InstitutionDetailViewModel()
    {
        _dataService = App.GetService<IDataService>();
        Debug.WriteLine("InstitutionDetailViewModel создан");
    }

    [RelayCommand]
    public async Task LoadInstitutionAsync(int institutionId)
    {
        try
        {
            Debug.WriteLine($"Загрузка учреждения с ID: {institutionId}");

            var institution = await _dataService.GetInstitutionAsync(institutionId);

            if (institution != null)
            {
                Institution = institution;
                Debug.WriteLine($"Учреждение загружено: {institution.Name}");
                Debug.WriteLine($"StudentCount: {institution.StudentCount}");
                Debug.WriteLine($"TeacherCount: {institution.TeacherCount}");
            }
            else
            {
                Debug.WriteLine("Учреждение не найдено");
                Institution = null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки учреждения: {ex.Message}");
            Institution = null;
        }
    }
}