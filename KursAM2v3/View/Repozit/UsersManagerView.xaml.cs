using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Repozit
{
    /// <summary>
    ///     Interaction logic for UsersManagerView.xaml
    /// </summary>
    public partial class UsersManagerView : ILayout
    {
        public UsersManagerView()
        {
            InitializeComponent(); 
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, LayoutControlMain);
            Loaded += UsersManagerView_Loaded;
            Closing += UsersManagerView_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void UsersManagerView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void UsersManagerView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void RolesGridControl_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            foreach (var column in RolesGridControl.Columns)
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
            if (e.Column.FieldName != "IsSelectedItem")
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
