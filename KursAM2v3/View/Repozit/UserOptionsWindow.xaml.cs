using System.Windows;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.Repozit
{
    /// <summary>
    /// Interaction logic for UserOptionsWindow.xaml
    /// </summary>
    public partial class UserOptionsWindow
    {
        public UserOptionsWindow()
        {
            InitializeComponent();
        }

        private void RoleGrid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            if (e.Column.FieldName != "IsSelectedItem")
                e.Column.ReadOnly = true;
        }

        private void DatasourceGrid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            if (e.Column.FieldName != "IsSelectedItem")
                e.Column.ReadOnly = true;
        }
    }
}
