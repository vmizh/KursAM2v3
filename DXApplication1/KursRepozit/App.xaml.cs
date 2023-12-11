using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Core;
using NLog;

namespace KursRepozit
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var ci = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            Start(() => base.OnStartup(e));
            Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            LogManager.Shutdown();
        }

        private static void Start(Action baseStart)
        {
            ApplicationThemeHelper.ApplicationThemeName = Theme.Win11LightName;;
            ToolTipService.ShowOnDisabledProperty.OverrideMetadata(
                typeof(Control),
                new FrameworkPropertyMetadata(true));

            baseStart();
        }
    }
}
