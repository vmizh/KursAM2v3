using System.Windows.Controls;
using DevExpress.Xpf.Grid;
using KursDomain.ViewModel.Base2;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for TableUserControl.xaml
    /// </summary>
    public partial class TableUserControl : UserControl
    {
        private readonly GridControlColumnsAutogenerating OnColumnsAutogenerating;

        public TableUserControl(GridControlColumnsAutogenerating onColumnsAutogenerating)
        {
            if (onColumnsAutogenerating != null)
            {
                OnColumnsAutogenerating = onColumnsAutogenerating;
            } 
            InitializeComponent();
        }


        private void OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            OnColumnsAutogenerating?.Invoke(sender, e);
        }
    }
}
