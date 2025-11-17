using EducationInstitutionsRB.Models;
using EducationInstitutionsRB.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.Views;

public sealed partial class ExtendedInstitutionDialog : ContentDialog
{
    public Institution Institution { get; set; }
    public string DistrictSearchText { get; set; } = string.Empty;

    // Списки для ComboBox
    public List<string> InstitutionTypes { get; } = new()
    {
        "Школа", "Гимназия", "Лицей", "ПТУ", "Колледж", "Университет", "Академия", "Институт"
    };

    public List<string> AccreditationCategories { get; } = new()
    {
        "I категория", "II категория", "III категория", "IV категория", "Без аккредитации"
    };

    public List<string> OwnershipTypes { get; } = new()
    {
        "Государственное", "Частное", "Ведомственное"
    };

    public List<string> LanguageTypes { get; } = new()
    {
        "Русский", "Белорусский", "Русский/Белорусский"
    };

    public List<string> StatusTypes { get; } = new()
    {
        "Действующее", "Реорганизовано", "Ликвидировано", "На реконструкции"
    };

    private readonly IDataService _dataService;
    private bool _isLoading = false;
    private ObservableCollection<District> _currentDistricts = new();

    public ExtendedInstitutionDialog(Institution institution, string title)
    {
        this.InitializeComponent();
        Institution = institution;
        DialogTitle.Text = title;
        _dataService = App.GetService<IDataService>();

        // Загружаем данные
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            // Загружаем регионы
            var regions = await _dataService.GetRegionsAsync();
            RegionCombo.ItemsSource = regions;

            // Если у учреждения уже есть район, устанавливаем его
            if (Institution.DistrictId > 0)
            {
                var district = await _dataService.GetDistrictAsync(Institution.DistrictId);
                if (district != null)
                {
                    RegionCombo.SelectedValue = district.RegionId;
                    await LoadDistrictsForRegion(district.RegionId);
                    DistrictSearchText = district.Name;
                    Institution.DistrictId = district.Id;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void RegionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading) return;

        if (RegionCombo.SelectedValue is int regionId)
        {
            await LoadDistrictsForRegion(regionId);
        }
        else
        {
            ClearDistricts();
        }
    }

    private async Task LoadDistrictsForRegion(int regionId)
    {
        try
        {
            var districts = await _dataService.GetDistrictsByRegionAsync(regionId);
            _currentDistricts = new ObservableCollection<District>(districts);

            DistrictSuggestBox.IsEnabled = true;

            if (districts.Count > 0)
            {
                DistrictSuggestBox.PlaceholderText = "Начните вводить название района";
                DistrictSuggestBox.ItemsSource = _currentDistricts.Take(5).ToList();
            }
            else
            {
                DistrictSuggestBox.PlaceholderText = "Нет доступных районов";
                DistrictSuggestBox.ItemsSource = null;
            }

            // Сбрасываем выбор района при смене региона
            if (Institution.DistrictId > 0)
            {
                var currentDistrict = districts.FirstOrDefault(d => d.Id == Institution.DistrictId);
                if (currentDistrict == null)
                {
                    Institution.DistrictId = 0;
                    DistrictSearchText = string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки районов: {ex.Message}");
            ClearDistricts();
        }
    }

    private void DistrictSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var searchText = sender.Text.Trim();
            FilterDistricts(searchText);
        }
    }

    private void FilterDistricts(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            DistrictSuggestBox.ItemsSource = _currentDistricts.Take(5).ToList();
            return;
        }

        try
        {
            var filteredDistricts = _currentDistricts
                .Where(d => d.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();

            DistrictSuggestBox.ItemsSource = filteredDistricts;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка фильтрации районов: {ex.Message}");
            DistrictSuggestBox.ItemsSource = _currentDistricts.Take(5).ToList();
        }
    }

    private void DistrictSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        try
        {
            if (args.SelectedItem is District selectedDistrict)
            {
                Institution.DistrictId = selectedDistrict.Id;
                DistrictSearchText = selectedDistrict.Name;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка выбора района: {ex.Message}");
        }
    }

    private void ClearDistricts()
    {
        DistrictSuggestBox.ItemsSource = null;
        DistrictSuggestBox.IsEnabled = false;
        DistrictSuggestBox.PlaceholderText = "Сначала выберите область";
        Institution.DistrictId = 0;
        DistrictSearchText = string.Empty;
        _currentDistricts.Clear();
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var errors = new List<string>();

        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(Institution.Name))
            errors.Add("Название учреждения");

        if (string.IsNullOrWhiteSpace(Institution.Type))
            errors.Add("Тип учреждения");

        if (string.IsNullOrWhiteSpace(Institution.Address))
            errors.Add("Адрес");

        if (string.IsNullOrWhiteSpace(Institution.DirectorName))
            errors.Add("ФИО директора");

        if (string.IsNullOrWhiteSpace(Institution.Contacts))
            errors.Add("Контактный телефон");

        if (Institution.DistrictId == 0)
            errors.Add("Район");

        if (errors.Any())
        {
            args.Cancel = true;
            var errorMessage = "Пожалуйста, заполните следующие обязательные поля:\n• " +
                             string.Join("\n• ", errors);
            _ = ShowValidationErrorAsync(errorMessage);
        }
    }

    private async Task ShowValidationErrorAsync(string message)
    {
        try
        {
            var errorDialog = new ContentDialog
            {
                Title = "Не все поля заполнены",
                Content = message,
                CloseButtonText = "OK"
            };

            if (this.XamlRoot != null)
            {
                errorDialog.XamlRoot = this.XamlRoot;
            }

            await errorDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка показа диалога: {ex.Message}");
        }
    }
}