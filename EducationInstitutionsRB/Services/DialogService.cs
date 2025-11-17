using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.Services;

public class DialogService
{
    private ContentDialog _currentDialog;

    public async Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog, XamlRoot xamlRoot = null)
    {
        // Закрываем предыдущий диалог, если он есть
        if (_currentDialog != null)
        {
            _currentDialog.Hide();
        }

        _currentDialog = dialog;

        // Устанавливаем XamlRoot если передан
        if (xamlRoot != null)
        {
            dialog.XamlRoot = xamlRoot;
        }

        try
        {
            var result = await dialog.ShowAsync();
            return result;
        }
        finally
        {
            if (_currentDialog == dialog)
            {
                _currentDialog = null;
            }
        }
    }

    public async Task ShowErrorAsync(string message, XamlRoot xamlRoot = null)
    {
        var dialog = new ContentDialog
        {
            Title = "Ошибка",
            Content = message,
            CloseButtonText = "OK"
        };

        // Устанавливаем XamlRoot если передан
        if (xamlRoot != null)
        {
            dialog.XamlRoot = xamlRoot;
        }

        await ShowDialogAsync(dialog, xamlRoot);
    }

    public async Task ShowSuccessAsync(string message, XamlRoot xamlRoot = null)
    {
        var dialog = new ContentDialog
        {
            Title = "Успех",
            Content = message,
            CloseButtonText = "OK"
        };

        // Устанавливаем XamlRoot если передан
        if (xamlRoot != null)
        {
            dialog.XamlRoot = xamlRoot;
        }

        await ShowDialogAsync(dialog, xamlRoot);
    }

    public async Task<ContentDialogResult> ShowConfirmationAsync(string title, string message, XamlRoot xamlRoot = null)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Да",
            CloseButtonText = "Отмена",
            DefaultButton = ContentDialogButton.Close
        };

        // Устанавливаем XamlRoot если передан
        if (xamlRoot != null)
        {
            dialog.XamlRoot = xamlRoot;
        }

        return await ShowDialogAsync(dialog, xamlRoot);
    }
}