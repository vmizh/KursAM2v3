using System.Windows;
using DevExpress.Xpf.Core;

namespace KursAM2.View
{
    /// <summary>
    ///     Interaction logic for WindowStart.xaml
    /// </summary>
    public partial class WindowStart
    {
        public WindowStart()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            Loaded += WindowStart_Loaded;
        }

        private void WindowStart_Loaded(object sender, RoutedEventArgs e)
        {
            var main = new MainWindow {Owner = Application.Current.MainWindow};
            main.Show();
        }
    }
}
