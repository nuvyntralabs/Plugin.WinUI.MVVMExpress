using Microsoft.UI.Xaml;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Hosting;

namespace Plugin.WinUI.MVVMExpress.Lifecycle;

/// <summary>Forwards <see cref="FrameworkElement.Loaded"/> / <see cref="FrameworkElement.Unloaded"/> to <see cref="IViewModel"/>.</summary>
public static class ViewModelLifecycle
{
    public static readonly DependencyProperty AutoProperty = DependencyProperty.RegisterAttached(
        "Auto",
        typeof(bool),
        typeof(ViewModelLifecycle),
        new PropertyMetadata(false, OnAutoChanged));

    private static readonly DependencyProperty OptionsProperty = DependencyProperty.RegisterAttached(
        "Options",
        typeof(MvvmExpressOptions),
        typeof(ViewModelLifecycle),
        new PropertyMetadata(null));

    private static readonly DependencyProperty InitializedProperty = DependencyProperty.RegisterAttached(
        "Initialized",
        typeof(bool),
        typeof(ViewModelLifecycle),
        new PropertyMetadata(false));

    public static bool GetAuto(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (bool)element.GetValue(AutoProperty);
    }

    public static void SetAuto(DependencyObject element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(AutoProperty, value);
    }

    public static void Attach(FrameworkElement element, MvvmExpressOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(OptionsProperty, options);
        SetAuto(element, true);
    }

    private static void OnAutoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        element.Loaded -= OnLoaded;
        element.Unloaded -= OnUnloaded;
        if (e.NewValue is true)
        {
            element.Loaded += OnLoaded;
            element.Unloaded += OnUnloaded;
        }
    }

    private static async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not IViewModel viewModel)
        {
            return;
        }

        try
        {
            if (element.GetValue(InitializedProperty) is not true)
            {
                element.SetValue(InitializedProperty, true);
                await viewModel.InitializeAsync().ConfigureAwait(true);
            }

            await viewModel.OnAppearingAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private static async void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not IViewModel viewModel)
        {
            return;
        }

        try
        {
            await viewModel.OnDisappearingAsync().ConfigureAwait(true);
            if (element.GetValue(OptionsProperty) is MvvmExpressOptions { CancelOperationsOnDisappear: true })
            {
                viewModel.CancelPendingOperations();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
