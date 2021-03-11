using System.Windows;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.Repozit
{
    /// <summary>
    ///     Interaction logic for UsersManagerView.xaml
    /// </summary>
    public partial class UsersManagerView
    {
        public UsersManagerView()
        {
            InitializeComponent();
        }

        private void RolesGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            foreach (GridColumn column in RolesGridControl.Columns)
            {
                if (column.FieldName == "IsSelectedItem")
                    column.Visible = false;

            }
        }


        private void UsersGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.ReadOnly = true;
        }

        private void PermissionsGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            if(e.Column.FieldName != "IsSelectedItem")
                e.Column.ReadOnly = true;
        }
    }
}