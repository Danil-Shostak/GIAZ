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

public sealed partial class InstitutionDialog : ContentDialog
{
    public Institution Institution { get; set; }
    public string Title { get; set; }

    // Свойство для поиска районов
    public string DistrictSearchText { get; set; } = string.Empty;

    public List<string> InstitutionTypes { get; } = new()
    {
        "Школа", "Гимназия", "Лицей", "Колледж", "Университет"
    };

    public List<string> StatusTypes { get; } = new()
    {
        "Активно", "Закрыто", "На реконструкции"
    };

    private readonly IDataService _dataService;
    private bool _isLoading = false;
    private ObservableCollection<District> _currentDistricts = new();

    public InstitutionDialog(Institution institution, string title)
    {
        this.InitializeComponent();
        Institution = institution;
        Title = title;
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
            System.Diagnostics.Debug.WriteLine("Начало загрузки данных для диалога...");

            // Загружаем регионы
            var regions = await _dataService.GetRegionsAsync();
            RegionCombo.ItemsSource = regions;
            System.Diagnostics.Debug.WriteLine($"Загружено регионов: {regions.Count}");

            // Если у учреждения уже есть район, устанавливаем его
            if (Institution.DistrictId > 0)
            {
                System.Diagnostics.Debug.WriteLine($"У учреждения есть DistrictId: {Institution.DistrictId}");

                var district = await _dataService.GetDistrictAsync(Institution.DistrictId);
                if (district != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Найден район: {district.Name}, RegionId: {district.RegionId}");

                    // Устанавливаем выбранный регион
                    RegionCombo.SelectedValue = district.RegionId;

                    // Загружаем районы для этого региона
                    await LoadDistrictsForRegion(district.RegionId);

                    // Устанавливаем текст в AutoSuggestBox
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
            System.Diagnostics.Debug.WriteLine($"Выбран регион с ID: {regionId}");
            await LoadDistrictsForRegion(regionId);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Регион не выбран");
            ClearDistricts();
        }
    }

    private async Task LoadDistrictsForRegion(int regionId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Загрузка районов для региона {regionId}...");

            var districts = await _dataService.GetDistrictsByRegionAsync(regionId);

            // Используем ObservableCollection вместо List
            _currentDistricts = new ObservableCollection<District>(districts);

            System.Diagnostics.Debug.WriteLine($"Загружено районов: {districts.Count}");

            // Включаем AutoSuggestBox
            DistrictSuggestBox.IsEnabled = true;

            if (districts.Count > 0)
            {
                DistrictSuggestBox.PlaceholderText = "Начните вводить название района";
                // Устанавливаем начальные подсказки
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
                // Проверяем, принадлежит ли текущий район новому региону
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

    // Обработчик изменения текста в AutoSuggestBox
    private void DistrictSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var searchText = sender.Text.Trim();
            FilterDistricts(searchText);
        }
    }

    // Метод для фильтрации районов по введенному тексту
    private void FilterDistricts(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            // Если текст пустой, показываем первые 5 районов
            DistrictSuggestBox.ItemsSource = _currentDistricts.Take(5).ToList();
            return;
        }

        try
        {
            // Фильтруем районы по введенному тексту
            var filteredDistricts = _currentDistricts
                .Where(d => d.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Take(10) // Ограничиваем количество подсказок
                .ToList();

            DistrictSuggestBox.ItemsSource = filteredDistricts;

            System.Diagnostics.Debug.WriteLine($"Найдено районов по запросу '{searchText}': {filteredDistricts.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка фильтрации районов: {ex.Message}");
            DistrictSuggestBox.ItemsSource = _currentDistricts.Take(5).ToList();
        }
    }

    // Обработчик выбора подсказки
    private void DistrictSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        try
        {
            if (args.SelectedItem is District selectedDistrict)
            {
                Institution.DistrictId = selectedDistrict.Id;
                // Не устанавливаем DistrictSearchText здесь, чтобы пользователь мог видеть полное название
                System.Diagnostics.Debug.WriteLine($"Выбран район: {selectedDistrict.Name} (ID: {selectedDistrict.Id})");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка выбора района: {ex.Message}");
        }
    }

    // Обработчик отправки текста (когда пользователь нажимает Enter)
    private void DistrictSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        try
        {
            if (args.ChosenSuggestion is District selectedDistrict)
            {
                Institution.DistrictId = selectedDistrict.Id;
                DistrictSearchText = selectedDistrict.Name;
            }
            else if (!string.IsNullOrWhiteSpace(args.QueryText))
            {
                // Пытаемся найти район по точному совпадению
                var exactMatch = _currentDistricts.FirstOrDefault(d =>
                    d.Name.Equals(args.QueryText, StringComparison.OrdinalIgnoreCase));

                if (exactMatch != null)
                {
                    Institution.DistrictId = exactMatch.Id;
                    DistrictSearchText = exactMatch.Name;
                }
                else
                {
                    // Если точного совпадения нет, сбрасываем выбор
                    Institution.DistrictId = 0;
                    System.Diagnostics.Debug.WriteLine($"Район '{args.QueryText}' не найден");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка отправки запроса: {ex.Message}");
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
        // Валидация - проверяем обязательные поля
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Institution.Name))
            errors.Add("Название учреждения");

        if (string.IsNullOrWhiteSpace(Institution.Type))
            errors.Add("Тип учреждения");

        if (string.IsNullOrWhiteSpace(Institution.Address))
            errors.Add("Адрес");

        if (Institution.DistrictId == 0)
            errors.Add("Район");

        if (errors.Any())
        {
            args.Cancel = true;

            var errorMessage = "Пожалуйста, заполните следующие обязательные поля:\n• " +
                             string.Join("\n• ", errors);

            // Показываем ошибку валидации
            _ = ShowValidationErrorAsync(errorMessage);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Все поля заполнены корректно, можно сохранять");
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

            // Безопасная установка XamlRoot
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