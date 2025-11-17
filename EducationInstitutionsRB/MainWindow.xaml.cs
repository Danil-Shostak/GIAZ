using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WinRT.Interop;

namespace EducationInstitutionsRB;

public sealed partial class MainWindow : Window
{
    private AppWindow _appWindow;

    public MainWindow()
    {
        try
        {
            this.InitializeComponent();

            // Быстрая настройка окна
            SetupWindow();

            // Запускаем плавный переход к основному контенту
            _ = AnimatedTransitionToMainContentAsync();

            Debug.WriteLine("MainWindow: Конструктор завершен");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindow: Ошибка в конструкторе: {ex.Message}");
        }
    }

    private void SetupWindow()
    {
        try
        {
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(DragRegion);

            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.SetBorderAndTitleBar(false, false);
                presenter.Maximize();
            }

            Debug.WriteLine("MainWindow: Окно настроено");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindow: Ошибка настройки окна: {ex.Message}");
        }
    }

    private async Task AnimatedTransitionToMainContentAsync()
    {
        try
        {
            Debug.WriteLine("MainWindow: Начало анимированного перехода");

            // Ждем немного чтобы показать сплеш-скрин
            await Task.Delay(1400);

            // Запускаем анимацию перехода
            await PlayTransitionAnimation();

            Debug.WriteLine("MainWindow: Анимированный переход завершен");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindow: Ошибка анимированного перехода: {ex.Message}");
            // В случае ошибки просто показываем основной контент
            ShowMainContentWithoutAnimation();
        }
    }

    private async Task PlayTransitionAnimation()
    {
        // Создаем основную storyboard для комплексной анимации
        var mainStoryboard = new Storyboard();

        // 1. Анимация исчезновения сплеш-скрина (ТОЛЬКО прозрачность, без масштабирования)
        var splashFadeOut = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(700),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        Storyboard.SetTarget(splashFadeOut, SplashGrid);
        Storyboard.SetTargetProperty(splashFadeOut, "Opacity");

        mainStoryboard.Children.Add(splashFadeOut);

        // Запускаем анимацию исчезновения сплеш-скрина
        mainStoryboard.Begin();
        await Task.Delay(500);

        // Показываем основной контент
        MainContentGrid.Visibility = Visibility.Visible;

        // 2. Анимация появления основного контента (увеличение + прозрачность)
        var contentStoryboard = new Storyboard();

        var contentFadeIn = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var contentScaleX = new DoubleAnimation
        {
            From = 0.9,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var contentScaleY = new DoubleAnimation
        {
            From = 0.9,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        Storyboard.SetTarget(contentFadeIn, MainContentGrid);
        Storyboard.SetTargetProperty(contentFadeIn, "Opacity");

        Storyboard.SetTarget(contentScaleX, MainContentScale);
        Storyboard.SetTargetProperty(contentScaleX, "ScaleX");

        Storyboard.SetTarget(contentScaleY, MainContentScale);
        Storyboard.SetTargetProperty(contentScaleY, "ScaleY");

        contentStoryboard.Children.Add(contentFadeIn);
        contentStoryboard.Children.Add(contentScaleX);
        contentStoryboard.Children.Add(contentScaleY);

        contentStoryboard.Begin();
        await Task.Delay(800);

        // Скрываем сплеш-скрин после анимации
        SplashGrid.Visibility = Visibility.Collapsed;

        // Устанавливаем фон после завершения анимации
        SetupBackdrop();
    }

    private void ShowMainContentWithoutAnimation()
    {
        SplashGrid.Visibility = Visibility.Collapsed;
        MainContentGrid.Visibility = Visibility.Visible;
        MainContentGrid.Opacity = 1.0;
        SetupBackdrop();
    }

    private void SetupBackdrop()
    {
        _ = Task.Run(() =>
        {
            try
            {
                this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
                Debug.WriteLine("MainWindow: Mica backdrop установлен");
            }
            catch
            {
                try
                {
                    this.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
                    Debug.WriteLine("MainWindow: Acrylic backdrop установлен");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MainWindow: Ошибка установки backdrop: {ex.Message}");
                }
            }
        });
    }

    // Метод для анимированного перехода между страницами
    public void NavigateWithAnimation(Type pageType)
    {
        try
        {
            var entranceAnimation = new EntranceNavigationTransitionInfo();
            ContentFrame.Navigate(pageType, null, entranceAnimation);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindow: Ошибка анимированной навигации: {ex.Message}");
            ContentFrame.Navigate(pageType);
        }
    }

    // Обработчики кнопок
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.Minimize();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindow: Ошибка сворачивания: {ex.Message}");
        }
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                if (presenter.State == OverlappedPresenterState.Maximized)
                {
                    presenter.Restore();
                    MaximizeButton.Content = "&#xE922;";
                }
                else
                {
                    presenter.Maximize();
                    MaximizeButton.Content = "&#xE923;";
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindow: Ошибка изменения размера: {ex.Message}");
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindow: Ошибка закрытия: {ex.Message}");
        }
    }

    private void RootNavigation_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Используем анимацию для начальной навигации
            NavigateWithAnimation(typeof(Views.OverviewPage));
            RootNavigation.SelectedItem = RootNavigation.MenuItems[0];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindow: Ошибка инициализации навигации: {ex.Message}");
        }
    }

    private void RootNavigation_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        try
        {
            if (args.InvokedItemContainer is NavigationViewItem item)
            {
                Type pageType = item.Tag?.ToString() switch
                {
                    "Overview" => typeof(Views.OverviewPage),
                    "Institutions" => typeof(Views.InstitutionsPage),
                    "Import" => typeof(Views.ImportPage),
                    "Reports" => typeof(Views.ReportsPage),
                    "Admin" => typeof(Views.AdminPage),
                    _ => typeof(Views.OverviewPage)
                };

                // Анимированная навигация между страницами
                NavigateWithAnimation(pageType);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindow: Ошибка навигации: {ex.Message}");
        }
    }

    private void RootNavigation_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        try
        {
            if (ContentFrame.CanGoBack)
            {
                // Назад с анимацией
                ContentFrame.GoBack(new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindow: Ошибка возврата: {ex.Message}");
        }
    }
}