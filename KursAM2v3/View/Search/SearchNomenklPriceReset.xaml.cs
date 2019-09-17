using System.ComponentModel;
using System.IO;
using System.Windows;
using DevExpress.Xpf.Grid;
using LayoutManager;

namespace KursAM2.View.Search
{
    /// <summary>
    ///     Interaction logic for SearchNomenklPriceReset.xaml
    /// </summary>
    public partial class SearchNomenklPriceReset : ILayout
    {
        public SearchNomenklPriceReset()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += SearchBaseView_Loaded;
            Closing += SearchBaseView_Closing;
        }

        public LayoutManagerBase LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void SearchBaseView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void SearchBaseView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void PART_GridControlNomenkl_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as GridControl;
            if (grid == null) return;
            var spath = (string) Application.Current.Properties["DataPath"];
            if (!File.Exists($"{spath}\\{GetType().Name}.{"NomenklLookUpEdit"}.xml")) return;
            grid.RestoreLayoutFromXml($"{spath}\\{GetType().Name}.{"NomenklLookUpEdit"}.xml");
        }

        private void PART_GridControlNomenkl_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as GridControl;
            var spath = (string) Application.Current.Properties["DataPath"];
            grid?.SaveLayoutToXml($"{spath}\\{GetType().Name}.{"NomenklLookUpEdit"}.xml");
        }
    }
}