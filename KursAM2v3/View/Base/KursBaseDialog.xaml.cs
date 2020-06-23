using System.Windows;
using DevExpress.XtraSpreadsheet.Model;

namespace KursRepozit.Views.Base
{
    /// <summary>
    ///     Interaction logic for KursBaseDialog.xaml
    /// </summary>
    public partial class KursBaseDialog
    {
        public KursBaseDialog()
        {
            InitializeComponent();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}