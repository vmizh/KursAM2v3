using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using KursAM2.ViewModel.Logistiks;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Logistiks.UC
{
    /// <summary>
    ///     Interaction logic for SelectNomenkls.xaml
    /// </summary>
    public partial class SelectWarehouse : ILayout
    {
        public SelectWarehouse()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, treeListWarehouse);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void ButtonEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is WarehouseSelectDialogViewModel ctx)) return;
            if (e.Key == Key.Enter)
                ctx.SearchExecute(null);
        }
    }
}
