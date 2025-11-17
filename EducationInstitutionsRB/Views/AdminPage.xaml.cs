using EducationInstitutionsRB.Models;
using EducationInstitutionsRB.Services;
using EducationInstitutionsRB.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.Views;

public sealed partial class AdminPage : Page
{
    public AdminViewModel ViewModel { get; }

    public AdminPage()
    {
        try
        {
            Debug.WriteLine("AdminPage конструктор начат");
            this.InitializeComponent();
            ViewModel = new AdminViewModel();
            Debug.WriteLine("AdminPage создана успешно");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка в конструкторе AdminPage: {ex.Message}");
            throw;
        }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        try
        {
            Debug.WriteLine("AdminPage OnNavigatedTo начат");
            base.OnNavigatedTo(e);
            await ViewModel.LoadDataAsync();
            Debug.WriteLine("AdminPage данные загружены");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка в OnNavigatedTo: {ex.Message}");
        }
    }

    private async void EditRegionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Region region)
        {
            await ViewModel.EditRegionAsync(region);
        }
    }

    private async void DeleteRegionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Region region)
        {
            await ViewModel.DeleteRegionAsync(region);
        }
    }

    private async void EditDistrictButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is District district)
        {
            await ViewModel.EditDistrictAsync(district);
        }
    }

    private async void DeleteDistrictButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is District district)
        {
            await ViewModel.DeleteDistrictAsync(district);
        }
    }
}