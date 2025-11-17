using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Threading.Tasks;

namespace EducationInstitutionsRB.Views;

public sealed partial class SplashPage : Page
{
    private Window _mainWindow;

    public SplashPage()
    {
        this.InitializeComponent();
        this.Loaded += SplashPage_Loaded;
    }

    public SplashPage(Window mainWindow) : this()
    {
        _mainWindow = mainWindow;
    }

    private async void SplashPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Ждем 2 секунды для показа заставки
        await Task.Delay(2000);

        // Переходим на главную страницу
        NavigateToMainPage();
    }

    private void NavigateToMainPage()
    {
        if (_mainWindow != null && _mainWindow.Content is Frame mainFrame)
        {
            mainFrame.Navigate(typeof(MainWindow), null, new DrillInNavigationTransitionInfo());
        }
        else
        {
            // Альтернативный способ навигации
            Frame newFrame = new Frame();
            newFrame.Navigate(typeof(MainWindow));

            if (_mainWindow != null)
            {
                _mainWindow.Content = newFrame;
            }
        }
    }
}