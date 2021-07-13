using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for AccruedAmountTypeView.xaml
    /// </summary>
    public partial class AccruedAmountTypeView : ThemedWindow
    {
        public AccruedAmountTypeView()
        {
            InitializeComponent();
        }

        private void gridAccruedAmountType_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
        }
    }
}