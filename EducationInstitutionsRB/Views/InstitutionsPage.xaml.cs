using EducationInstitutionsRB.Models;
using EducationInstitutionsRB.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.Views;

public sealed partial class InstitutionsPage : Page
{
    private readonly IDataService _dataService;
    private readonly DialogService _dialogService;
    private List<Institution> _allInstitutions = new();
    private List<Region> _regions = new();
    private List<District> _districts = new();
    private bool _isDialogOpen = false;

    public InstitutionsPage()
    {
        this.InitializeComponent();
        _dataService = App.GetService<IDataService>();
        _dialogService = App.GetService<DialogService>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await LoadDataAsync();
    }

    private void InstitutionsList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Institution institution)
        {
            Debug.WriteLine($"Нажато учреждение: {institution.Name} (ID: {institution.Id})");

            // Переход на детальную страницу с передачей ID
            Frame.Navigate(typeof(InstitutionDetailPage), institution.Id,
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _allInstitutions = await _dataService.GetInstitutionsAsync();
            _regions = await _dataService.GetRegionsAsync();

            InstitutionsList.ItemsSource = _allInstitutions;
            RegionCombo.ItemsSource = _regions;

            System.Diagnostics.Debug.WriteLine($"Загружено учреждений: {_allInstitutions.Count}");
        }
        catch (Exception ex)
        {
            // Безопасный вызов с проверкой XamlRoot
            if (this.Content?.XamlRoot != null)
            {
                await _dialogService.ShowErrorAsync($"Ошибка загрузки данных: {ex.Message}", this.Content.XamlRoot);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            }
        }
    }

    private async void AddInstitutionButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isDialogOpen) return;
        _isDialogOpen = true;

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

            var dialog = new ExtendedInstitutionDialog(newInstitution, "Добавить учреждение");

            if (this.Content?.XamlRoot != null)
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    await _dataService.AddInstitutionAsync(newInstitution);
                    await LoadDataAsync();

                    if (this.Content?.XamlRoot != null)
                    {
                        await _dialogService.ShowSuccessAsync("Учреждение успешно добавлено!", this.Content.XamlRoot);
                    }
                }
                catch (Exception ex)
                {
                    if (this.Content?.XamlRoot != null)
                    {
                        await _dialogService.ShowErrorAsync($"Ошибка при добавлении: {ex.Message}", this.Content.XamlRoot);
                    }
                }
            }
        }
        finally
        {
            _isDialogOpen = false;
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isDialogOpen) return;

        if (sender is Button button && button.Tag is Institution selectedInstitution)
        {
            await EditInstitutionAsync(selectedInstitution);
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isDialogOpen) return;

        if (sender is Button button && button.Tag is Institution selectedInstitution)
        {
            await DeleteInstitutionAsync(selectedInstitution);
        }
    }

    private async Task EditInstitutionAsync(Institution selectedInstitution)
    {
        _isDialogOpen = true;

        try
        {
            // Создаем копию с ВСЕМИ полями
            var institutionToEdit = new Institution
            {
                // Старые поля
                Id = selectedInstitution.Id,
                Name = selectedInstitution.Name,
                Type = selectedInstitution.Type,
                Address = selectedInstitution.Address,
                Contacts = selectedInstitution.Contacts,
                DistrictId = selectedInstitution.DistrictId,
                Status = selectedInstitution.Status,
                RegistrationDate = selectedInstitution.RegistrationDate,
                StudentCount = selectedInstitution.StudentCount,
                AdmittedCount = selectedInstitution.AdmittedCount,
                ExpelledCount = selectedInstitution.ExpelledCount,
                StaffCount = selectedInstitution.StaffCount,

                // Новые поля
                LicenseNumber = selectedInstitution.LicenseNumber,
                LicenseExpiryDate = selectedInstitution.LicenseExpiryDate,
                AccreditationCategory = selectedInstitution.AccreditationCategory,
                OwnershipType = selectedInstitution.OwnershipType,
                LanguageOfEducation = selectedInstitution.LanguageOfEducation,
                DirectorName = selectedInstitution.DirectorName,
                Email = selectedInstitution.Email,
                Website = selectedInstitution.Website,
                FoundationYear = selectedInstitution.FoundationYear,
                InstitutionStatus = selectedInstitution.InstitutionStatus,
                ClassroomCount = selectedInstitution.ClassroomCount,
                TeacherCount = selectedInstitution.TeacherCount,
                AdministrativeStaffCount = selectedInstitution.AdministrativeStaffCount,
                ComputerCount = selectedInstitution.ComputerCount,
                HasSportsHall = selectedInstitution.HasSportsHall,
                HasDiningRoom = selectedInstitution.HasDiningRoom,
                HasLibrary = selectedInstitution.HasLibrary,
                TotalArea = selectedInstitution.TotalArea,
                Specialization = selectedInstitution.Specialization,
                EducationalPrograms = selectedInstitution.EducationalPrograms,
                Infrastructure = selectedInstitution.Infrastructure
            };

            var dialog = new ExtendedInstitutionDialog(institutionToEdit, "Редактировать учреждение");

            if (this.Content?.XamlRoot != null)
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    await _dataService.UpdateInstitutionAsync(institutionToEdit);
                    await LoadDataAsync();

                    if (this.Content?.XamlRoot != null)
                    {
                        await _dialogService.ShowSuccessAsync("Учреждение успешно обновлено!", this.Content.XamlRoot);
                    }
                }
                catch (Exception ex)
                {
                    if (this.Content?.XamlRoot != null)
                    {
                        await _dialogService.ShowErrorAsync($"Ошибка при обновлении: {ex.Message}", this.Content.XamlRoot);
                    }
                }
            }
        }
        finally
        {
            _isDialogOpen = false;
        }
    }

    private async Task DeleteInstitutionAsync(Institution selectedInstitution)
    {
        // Безопасный вызов с проверкой XamlRoot
        if (this.Content?.XamlRoot == null) return;

        var result = await _dialogService.ShowConfirmationAsync(
            "Подтверждение удаления",
            $"Вы уверены, что хотите удалить учреждение \"{selectedInstitution.Name}\"?",
            this.Content.XamlRoot
        );

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                await _dataService.DeleteInstitutionAsync(selectedInstitution.Id);
                await LoadDataAsync();

                await _dialogService.ShowSuccessAsync("Учреждение успешно удалено!", this.Content.XamlRoot);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync($"Ошибка при удалении: {ex.Message}", this.Content.XamlRoot);
            }
        }
    }

    // Остальные методы без изменений...
    private async void RegionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RegionCombo.SelectedItem is Region selectedRegion)
        {
            try
            {
                _districts = await _dataService.GetDistrictsByRegionAsync(selectedRegion.Id);
                DistrictCombo.ItemsSource = _districts;
                DistrictCombo.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // Безопасный вызов с проверкой XamlRoot
                if (this.Content?.XamlRoot != null)
                {
                    await _dialogService.ShowErrorAsync($"Ошибка загрузки районов: {ex.Message}", this.Content.XamlRoot);
                }
            }
        }
        else
        {
            DistrictCombo.ItemsSource = null;
            DistrictCombo.IsEnabled = false;
        }
        await FilterInstitutionsAsync();
    }

    private async void DistrictCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await FilterInstitutionsAsync();
    }

    private async void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await FilterInstitutionsAsync();
    }

    private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        await FilterInstitutionsAsync();
    }

    private async Task FilterInstitutionsAsync()
    {
        try
        {
            var searchText = SearchTextBox.Text;
            var regionId = (RegionCombo.SelectedItem as Region)?.Id;
            var districtId = (DistrictCombo.SelectedItem as District)?.Id;
            var type = TypeCombo.SelectedItem as string;
            var status = "Активно";

            var filtered = await _dataService.SearchInstitutionsAsync(
                searchText, regionId, districtId, type, status);

            InstitutionsList.ItemsSource = filtered;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка фильтрации: {ex.Message}");
        }
    }
}