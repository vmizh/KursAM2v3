using System.Windows;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using LayoutManager;

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
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, LayoutControlMain);
            Loaded += UsersManagerView_Loaded;
            Closing += UsersManagerView_Closing;
        }

        private void UsersManagerView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void UsersManagerView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void RolesGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            foreach (GridColumn column in RolesGridControl.Columns)
            {
                e.Column.Name = e.Column.FieldName;
                if (column.FieldName == "IsSelectedItem")
                    column.Visible = false;
            }
        }

        private void UsersGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }

        private void PermissionsGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            if(e.Column.FieldName != "IsSelectedItem")
                e.Column.ReadOnly = true;
        }

        private void GridControlPermissionItems_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            if (e.Column.Name != "IsSelectedItem")
                e.Column.ReadOnly = true;
        }
    }
}