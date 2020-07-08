using System.Windows;

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
            Loaded += WindowStart_Loaded;
        }

        private void WindowStart_Loaded(object sender, RoutedEventArgs e)
        {
            var main = new MainWindow {Owner = Application.Current.MainWindow};
            main.Show();
        }
    }
}