using DevExpress.Xpf.Grid;

namespace KursAM2.View.Logistiks.NomenklReturn
{
    /// <summary>
    ///     Interaction logic for NomenklReturnSelectWaybillRows.xaml
    /// </summary>
    public partial class NomenklReturnSelectWaybillRows
    {
        public NomenklReturnSelectWaybillRows()
        {
            InitializeComponent();
        }

        private void Grid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }
    }
}
