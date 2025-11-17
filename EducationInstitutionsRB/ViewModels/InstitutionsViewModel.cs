using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducationInstitutionsRB.Models;
using EducationInstitutionsRB.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace EducationInstitutionsRB.ViewModels;

public partial class InstitutionsViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private List<Institution> _institutions = new();

    [ObservableProperty]
    private List<Region> _regions = new();

    [ObservableProperty]
    private List<District> _districts = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int? _selectedRegionId;

    [ObservableProperty]
    private int? _selectedDistrictId;

    [ObservableProperty]
    private string? _selectedType;

    [ObservableProperty]
    private string? _selectedStatus;

    public List<string> InstitutionTypes { get; } = new()
    {
        "Школа", "Гимназия", "Лицей", "Колледж", "Университет"
    };

    public List<string> StatusTypes { get; } = new()
    {
        "Активно", "Закрыто", "На реконструкции"
    };

    public InstitutionsViewModel()
    {
        _dataService = App.GetService<IDataService>();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        Institutions = await _dataService.GetInstitutionsAsync();
        Regions = await _dataService.GetRegionsAsync();
    }

    partial void OnSelectedRegionIdChanged(int? value)
    {
        _ = LoadDistrictsAsync();
        _ = FilterInstitutionsAsync();
    }

    partial void OnSelectedDistrictIdChanged(int? value)
    {
        _ = FilterInstitutionsAsync();
    }

    partial void OnSelectedTypeChanged(string? value)
    {
        _ = FilterInstitutionsAsync();
    }

    partial void OnSelectedStatusChanged(string? value)
    {
        _ = FilterInstitutionsAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = FilterInstitutionsAsync();
    }

    private async Task LoadDistrictsAsync()
    {
        if (SelectedRegionId.HasValue)
        {
            Districts = await _dataService.GetDistrictsByRegionAsync(SelectedRegionId.Value);
        }
        else
        {
            Districts = new List<District>();
        }
        SelectedDistrictId = null;
    }

    private async Task FilterInstitutionsAsync()
    {
        Institutions = await _dataService.SearchInstitutionsAsync(
            SearchText,
            SelectedRegionId,
            SelectedDistrictId,
            SelectedType,
            SelectedStatus
        );
    }
}