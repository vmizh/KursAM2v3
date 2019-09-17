using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.Management;
using LayoutManager;

namespace KursAM2.View.Management
{
    /// <summary>
    ///     Interaction logic for KontragentBalans.xaml
    /// </summary>
    public partial class KontragentBalansForm : ILayout
    {
        private readonly string myKontragentLookUpLayoutName;

        public KontragentBalansForm()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            myKontragentLookUpLayoutName =
                $"{Environment.CurrentDirectory}\\Layout\\KontragentLookUpEdit.{Guid.NewGuid()}.xml";
            Loaded += KontragentBalansForm_Loaded;
            Closing += KontragentBalansForm_Closing;
        }

        public LayoutManagerBase LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void KontragentBalansForm_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
            var ctx = DataContext as KontragentBalansWindowViewModel;
            if (ctx?.StartKontragent != null)
                ctx.Kontragent = ctx.Kontragents.Single(_ => _.DOC_CODE == ctx.StartKontragent.DOC_CODE);
        }

        private void KontragentBalansForm_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
            if (File.Exists(myKontragentLookUpLayoutName))
                File.Delete(myKontragentLookUpLayoutName);
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

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            //InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, new[] {VirtualKeyCode.VK_A});
        }
    }
}