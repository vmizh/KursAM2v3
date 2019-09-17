using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using DevExpress.Xpf.Core;

namespace KursStart
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
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Start(() => base.OnStartup(e));
        }

        private static void Start(Action baseStart)
        {
            ThemeManager.ApplicationThemeName = "MetropolisLight";
            baseStart();
        }
    }
}