using System.Windows;

namespace KursAM2.View.Base
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