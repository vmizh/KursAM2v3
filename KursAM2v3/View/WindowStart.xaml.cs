using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KursAM2.View
{
    /// <summary>
    /// Interaction logic for WindowStart.xaml
    /// </summary>
    public partial class WindowStart : Window
    {
        public WindowStart()
        {
            InitializeComponent();
            Loaded += WindowStart_Loaded;
        }
       
        private void WindowStart_Loaded(object sender, RoutedEventArgs e)
        {
            var main = new MainWindow { Owner = Application.Current.MainWindow };
            main.Show();
        }
    }
}
