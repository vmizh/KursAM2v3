using System.IO;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using KursAM2.View.Base;

namespace KursAM2.View.Management
{
    /// <summary>
    ///     Interaction logic for KontragentBalansView.xaml
    /// </summary>
    public partial class KontragentBalansView : ThemedWindow
    {
        public KontragentBalansView()
        {
            InitializeComponent();
        }

        private void PART_GridControlKontragent_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as GridControl;
            var spath = (string) Application.Current.Properties["DataPath"];
            grid?.SaveLayoutToXml($"{spath}\\{GetType().Name}.KontragentLookUpEdit.xml");
        }

        private void PART_GridControlKontragent_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as GridControl;
            if (grid == null) return;
            var spath = (string) Application.Current.Properties["DataPath"];
            if (!File.Exists($"{spath}\\{GetType().Name}.KontragentLookUpEdit.xml")) return;
            grid.RestoreLayoutFromXml($"{spath}\\{GetType().Name}.KontragentLookUpEdit.xml");
        }

        private void Grid_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }

       
    }
}
