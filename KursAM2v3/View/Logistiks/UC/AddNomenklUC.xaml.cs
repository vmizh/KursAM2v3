using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.Logistiks.UC
{
    /// <summary>
    ///     Interaction logic for AddNomenklUC.xaml
    /// </summary>
    public partial class AddNomenklUC
    {
        public AddNomenklUC()
        {
            InitializeComponent();
        }

        private void TreeListPermissionStruct_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }

        private void NomenklItemGrid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }


        private void SelectedNomenklGrid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }

        private void BaseEdit_OnEditValueChanging(object sender, EditValueChangingEventArgs e)
        {
            if (string.IsNullOrWhiteSpace((string)e.NewValue))
                if (DataContext is AddNomenklViewModel ctx)
                    ctx.ClearSearch(null);
        }
    }
}