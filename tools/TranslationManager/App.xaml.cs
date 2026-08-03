using System;
using System.Windows;
using System.Windows.Threading;

namespace TranslationManager;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Chyba: {e.Exception.Message}\n\n{e.Exception.StackTrace}",
            "DDV Translation Manager - Chyba",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show(
                $"Kritická chyba: {ex.Message}\n\n{ex.StackTrace}",
                "DDV Translation Manager - Kritická chyba",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
