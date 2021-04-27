using System.ComponentModel;
using System.IO;
using System.Windows;
using DevExpress.Xpf.Grid;
using LayoutManager;

namespace KursAM2.View.Logistiks
{
    public partial class NomenklMove : ILayout
    {
        public NomenklMove()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += NomenklMove_Loaded;
            Closing += NomenklMove_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void NomenklMove_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void NomenklMove_Loaded(object sender, RoutedEventArgs e)
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
            grid?.SaveLayoutToXml($"{spath}\\{GetType().Name}.NomenklLookUpEdit.xml");
        }
    }
}