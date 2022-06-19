using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;
using LayoutManager;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for InventorySheetSearchView.xaml
    /// </summary>
    public partial class InventorySheetSearchView : ILayout
    {
        public InventorySheetSearchView()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Closing += InventorySheetSearchView_Closing;
            Loaded += InventorySheetSearchView_Loaded;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void InventorySheetSearchView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void InventorySheetSearchView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void Export_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
