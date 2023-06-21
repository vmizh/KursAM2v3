using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Repozit
{
    /// <summary>
    /// Interaction logic for UserOptionsWindow.xaml
    /// </summary>
    public partial class UserOptionsWindow : ILayout
    {
        public UserOptionsWindow()
        {
            InitializeComponent(); 
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, validationContainer);
            Loaded += UserOptionsWindow_Loaded;
            Closing += UserOptionsWindow_Closing;
        }

        private void UserOptionsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void UserOptionsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
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

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }
    }
}
