using System.IO;
using System.Windows;
using DevExpress.Xpf.Grid;
using LayoutManager;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for NomenklCostCalculator.xaml
    /// </summary>
    public partial class NomenklCostCalculator : ILayout
    {
        public NomenklCostCalculator()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Closing += (o, e) => { LayoutManager.Save(); };
            Loaded += (operGridControl, e) => { LayoutManager.Load(); };
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void ResetLayot()
        {
            throw new System.NotImplementedException();
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