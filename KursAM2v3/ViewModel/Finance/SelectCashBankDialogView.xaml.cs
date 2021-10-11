using DevExpress.Xpf.Grid;

namespace KursAM2.ViewModel.Finance
{
    /// <summary>
    ///     Interaction logic for SelectCashBankDialogView.xaml
    /// </summary>
    public partial class SelectCashBankDialogView
    {
        public SelectCashBankDialogView()
        {
            InitializeComponent();
        }

        private void DataControlBase_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.ReadOnly = true;
            e.Column.Name = e.Column.FieldName;
            switch (e.Column.Name)
            {
                case "Name":
                    e.Column.Width = new GridColumnWidth(100, GridColumnUnitType.Auto);
                    break;
            }
        }
    }
}