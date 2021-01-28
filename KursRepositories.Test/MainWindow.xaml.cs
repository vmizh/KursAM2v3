using System.Windows;
using KursRepositories.View;
using KursRepositories.ViewModels;

namespace KursRepositories.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            
        }

        private void SimpleButton_Click(object sender, RoutedEventArgs e)
        {

            var ctx = new MainWindowPermissionsViewModel();
            var form = new MainWindowPermissions();
            form.DataContext = ctx;
            form.Show();

        }
    }
}
