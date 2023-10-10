using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace KursDomain.WindowsManager
{
    /// <summary>
    /// Interaction logic for KursDialog.xaml
    /// </summary>
    public partial class KursDialog : ThemedWindow
    {
        public KursDialog()
        {
            InitializeComponent();
            RoundCorners = true;
            
            
        }

        private void ThemedWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
