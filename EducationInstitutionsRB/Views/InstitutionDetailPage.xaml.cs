using EducationInstitutionsRB.Models;
using EducationInstitutionsRB.Services;
using EducationInstitutionsRB.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;

namespace EducationInstitutionsRB.Views;

public sealed partial class InstitutionDetailPage : Page
{
    public InstitutionDetailViewModel ViewModel { get; }

    public InstitutionDetailPage()
    {
        this.InitializeComponent();
        ViewModel = new InstitutionDetailViewModel();
        this.DataContext = ViewModel;
        Debug.WriteLine("InstitutionDetailPage создана");
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Debug.WriteLine($"OnNavigatedTo вызван с параметром: {e.Parameter}");

        if (e.Parameter is int institutionId)
        {
            Debug.WriteLine($"Передан ID учреждения: {institutionId}");
            await ViewModel.LoadInstitutionAsync(institutionId);
        }
        else if (e.Parameter is Institution institution)
        {
            Debug.WriteLine($"Передан объект учреждения: {institution.Name}");
            ViewModel.Institution = institution;
        }
        else
        {
            Debug.WriteLine("Неизвестный тип параметра");
        }
    }

    // НОВЫЙ МЕТОД: Кнопка возврата
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Нажата кнопка возврата");

        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
        else
        {
            // Если нельзя вернуться назад, переходим на страницу учреждений
            Frame.Navigate(typeof(InstitutionsPage));
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Institution == null)
        {
            Debug.WriteLine("Institution is null в EditButton_Click");
            return;
        }

        try
        {
            var dialog = new ExtendedInstitutionDialog(ViewModel.Institution, "Редактировать учреждение");

            if (this.Content?.XamlRoot != null)
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.LoadInstitutionAsync(ViewModel.Institution.Id);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка редактирования: {ex.Message}");
            var dialogService = App.GetService<DialogService>();
            await dialogService.ShowErrorAsync($"Ошибка при редактировании: {ex.Message}", this.Content?.XamlRoot);
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Institution == null)
        {
            Debug.WriteLine("Institution is null в DeleteButton_Click");
            return;
        }

        var dialogService = App.GetService<DialogService>();
        var result = await dialogService.ShowConfirmationAsync(
            "Подтверждение удаления",
            $"Вы уверены, что хотите удалить учреждение \"{ViewModel.Institution.Name}\"?",
            this.Content?.XamlRoot
        );

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                var dataService = App.GetService<IDataService>();
                await dataService.DeleteInstitutionAsync(ViewModel.Institution.Id);

                // Возвращаемся к списку учреждений
                Frame.Navigate(typeof(InstitutionsPage));
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync($"Ошибка при удалении: {ex.Message}", this.Content?.XamlRoot);
            }
        }
    }
}