using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AutoMapper;
using DevExpress.Entity.Model.Metadata;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using Helper;
using Mapper = AutoMapper.Mapper;

namespace KursAM2
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static Mapper Mapper;
        public App()
        {
            var ci = new CultureInfo("ru-RU");
            Mapper =  MapperConfig.InitializeAutomapper();
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}" +
                                  "\\KursAM2v3"))
                Directory.CreateDirectory(
                    $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KursAM2v3");
            Current.Properties.Add("DataPath",
                $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KursAM2v3");
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Start(() => base.OnStartup(e));
        }

        private static void Start(Action baseStart)
        {
            try
            {
                ApplicationThemeHelper.ApplicationThemeName = Theme.Win11LightName;
                ToolTipService.ShowOnDisabledProperty.OverrideMetadata(
                    typeof(Control),
                    new FrameworkPropertyMetadata(true));
                GridControlLocalizer.Active = new CustomDXGridLocalizer();
                EditorLocalizer.Active = new CustomEditorsLocalizer();
                DXMessageBoxLocalizer.Active = new CustomDXMessageBoxLocalizer();

                EventManager.RegisterClassHandler(typeof(GridColumn), DXSerializer.AllowPropertyEvent,
                    new AllowPropertyEventHandler((d, e) =>
                    {
                        if (!e.Property.Name.Contains("Header")) return;
                        e.Allow = false;
                        e.Handled = true;
                    }));
                baseStart();
            }
            catch (Exception ex)
            {
                File.WriteAllText(ex.Message, "Error.txt");
            }
        }

        private void OnDispatcherUnhandledException(object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            var sb = new StringBuilder(e.Exception.Message);
            var ex1 = e.Exception.InnerException;
            while (ex1 != null) sb.Append($"\n{ex1.Message}");
            MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message, "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Не предвиденная ошибка, обратитьсь к администратору."
                            + Environment.NewLine + e.Exception.Message, "Непредвиденная ошибка");

            e.Handled = true;
        }
    }
}
