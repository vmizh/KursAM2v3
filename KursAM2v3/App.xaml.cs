using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Core;

namespace KursAM2
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            var ci = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}" +
                                  $"\\KursAM2v3"))
                Directory.CreateDirectory(
                    $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KursAM2v3");
            Current.Properties.Add("DataPath",
                $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KursAM2v3");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Start(() => base.OnStartup(e));
        }

        private static void Start(Action baseStart)
        {
            ApplicationThemeHelper.ApplicationThemeName = "MetropolisLight";
            ToolTipService.ShowOnDisabledProperty.OverrideMetadata(
                typeof(Control),
                new FrameworkPropertyMetadata(true));
            baseStart();
        }
    }

}