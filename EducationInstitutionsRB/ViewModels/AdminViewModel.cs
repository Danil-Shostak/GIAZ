using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EducationInstitutionsRB.Models;
using EducationInstitutionsRB.Services;
using EducationInstitutionsRB.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly DialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Region> _regions = new();

    [ObservableProperty]
    private ObservableCollection<District> _districts = new();

    [ObservableProperty]
    private int? _selectedRegionId;

    public AdminViewModel()
    {
        _dataService = App.GetService<IDataService>();
        _dialogService = App.GetService<DialogService>();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        try
        {
            Debug.WriteLine("Загрузка данных для администрирования...");

            // Загружаем регионы
            var regions = await _dataService.GetRegionsAsync();
            Regions = new ObservableCollection<Region>(regions);

            // Загружаем все районы
            var districts = await _dataService.GetDistrictsAsync();
            Districts = new ObservableCollection<District>(districts);

            Debug.WriteLine($"Загружено: {Regions.Count} областей, {Districts.Count} районов");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
        }
    }

    // Этот метод будет вызываться автоматически при изменении выбранного региона
    partial void OnSelectedRegionIdChanged(int? value)
    {
        _ = LoadDistrictsForRegionAsync();
    }

    private async Task LoadDistrictsForRegionAsync()
    {
        try
        {
            if (SelectedRegionId.HasValue)
            {
                // Загружаем районы только для выбранного региона
                var districts = await _dataService.GetDistrictsByRegionAsync(SelectedRegionId.Value);
                Districts = new ObservableCollection<District>(districts);
                Debug.WriteLine($"Загружено {Districts.Count} районов для региона {SelectedRegionId}");
            }
            else
            {
                // Если регион не выбран, показываем все районы
                var allDistricts = await _dataService.GetDistrictsAsync();
                Districts = new ObservableCollection<District>(allDistricts);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки районов: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task AddRegionAsync()
    {
        try
        {
            var dialog = new SimpleInputDialog("Добавить область", "Введите название области:");
            dialog.XamlRoot = App.MainWindow?.Content?.XamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                var newRegion = new Region { Name = dialog.InputText.Trim() };
                await _dataService.AddRegionAsync(newRegion);
                await LoadDataAsync();

                await _dialogService.ShowSuccessAsync("Область успешно добавлена!", dialog.XamlRoot);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Ошибка при добавлении области: {ex.Message}",
                App.MainWindow?.Content?.XamlRoot);
        }
    }

    public async Task EditRegionAsync(Region region)
    {
        try
        {
            var dialog = new SimpleInputDialog("Редактировать область", "Введите новое название:", region.Name);
            dialog.XamlRoot = App.MainWindow?.Content?.XamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                region.Name = dialog.InputText.Trim();
                await _dataService.UpdateRegionAsync(region);
                await LoadDataAsync();

                await _dialogService.ShowSuccessAsync("Область успешно обновлена!", dialog.XamlRoot);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Ошибка при редактировании области: {ex.Message}",
                App.MainWindow?.Content?.XamlRoot);
        }
    }

    public async Task DeleteRegionAsync(Region region)
    {
        try
        {
            // Проверяем, есть ли связанные районы
            var relatedDistricts = Districts.Where(d => d.RegionId == region.Id).ToList();
            if (relatedDistricts.Any())
            {
                await _dialogService.ShowErrorAsync(
                    $"Невозможно удалить область. Существуют связанные районы ({relatedDistricts.Count}). " +
                    "Сначала удалите все районы этой области.",
                    App.MainWindow?.Content?.XamlRoot);
                return;
            }

            var result = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить область \"{region.Name}\"?",
                App.MainWindow?.Content?.XamlRoot
            );

            if (result == ContentDialogResult.Primary)
            {
                await _dataService.DeleteRegionAsync(region.Id);
                await LoadDataAsync();
                await _dialogService.ShowSuccessAsync("Область успешно удалена!",
                    App.MainWindow?.Content?.XamlRoot);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Ошибка при удалении области: {ex.Message}",
                App.MainWindow?.Content?.XamlRoot);
        }
    }

    [RelayCommand]
    public async Task AddDistrictAsync()
    {
        try
        {
            if (!Regions.Any())
            {
                await _dialogService.ShowErrorAsync("Сначала добавьте хотя бы одну область.",
                    App.MainWindow?.Content?.XamlRoot);
                return;
            }

            var dialog = new DistrictDialog(new District(), "Добавить район", Regions.ToList());
            dialog.XamlRoot = App.MainWindow?.Content?.XamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await _dataService.AddDistrictAsync(dialog.District);
                await LoadDataAsync();
                await _dialogService.ShowSuccessAsync("Район успешно добавлен!", dialog.XamlRoot);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Ошибка при добавлении района: {ex.Message}",
                App.MainWindow?.Content?.XamlRoot);
        }
    }

    public async Task EditDistrictAsync(District district)
    {
        try
        {
            var dialog = new DistrictDialog(district, "Редактировать район", Regions.ToList());
            dialog.XamlRoot = App.MainWindow?.Content?.XamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await _dataService.UpdateDistrictAsync(dialog.District);
                await LoadDataAsync();
                await _dialogService.ShowSuccessAsync("Район успешно обновлен!", dialog.XamlRoot);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Ошибка при редактировании района: {ex.Message}",
                App.MainWindow?.Content?.XamlRoot);
        }
    }

    public async Task DeleteDistrictAsync(District district)
    {
        try
        {
            // Проверяем, есть ли связанные учреждения
            var institutions = await _dataService.GetInstitutionsAsync();
            var relatedInstitutions = institutions.Where(i => i.DistrictId == district.Id).ToList();

            if (relatedInstitutions.Any())
            {
                await _dialogService.ShowErrorAsync(
                    $"Невозможно удалить район. Существуют связанные учреждения ({relatedInstitutions.Count}). " +
                    "Сначала удалите или переместите все учреждения этого района.",
                    App.MainWindow?.Content?.XamlRoot);
                return;
            }

            var result = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить район \"{district.Name}\"?",
                App.MainWindow?.Content?.XamlRoot
            );

            if (result == ContentDialogResult.Primary)
            {
                await _dataService.DeleteDistrictAsync(district.Id);
                await LoadDataAsync();
                await _dialogService.ShowSuccessAsync("Район успешно удален!",
                    App.MainWindow?.Content?.XamlRoot);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Ошибка при удалении района: {ex.Message}",
                App.MainWindow?.Content?.XamlRoot);
        }
    }
}