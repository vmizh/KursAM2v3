using System.Windows;
using DevExpress.Xpf.Grid;
using LayoutManager;

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
    }
}