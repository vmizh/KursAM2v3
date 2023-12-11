using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using DevExpress.Data;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.ViewModel.Management;
using LayoutManager;

//using WindowsInput;

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
            
            //LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            myKontragentLookUpLayoutName =
                $"{Environment.CurrentDirectory}\\Layout\\KontragentLookUpEdit.{Guid.NewGuid()}.xml";
            Loaded += KontragentBalansForm_Loaded;
        }

        static KontragentBalansForm()
        {
            GridControlLocalizer.Active = new CustomDXGridLocalizer();
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            //LayoutManager.Save();
        }

        private void KontragentBalansForm_Loaded(object sender, RoutedEventArgs e)
        {
            // LayoutManager.Load();
            treePeriods.SelectionMode = MultiSelectMode.None;
            KontrOperGrid.SelectionMode = MultiSelectMode.None;
            var ctx = DataContext as KontragentBalansWindowViewModel;
            if (ctx?.StartKontragent != null)
                ctx.Kontragent = ctx.Kontragents.Single(_ => _.DocCode == ctx.StartKontragent.DocCode);
            searchLookUpEditKontragent.EditValue = ctx.Kontragent;
            searchLookUpEditKontragent.IsPopupOpen = false;
            //KontrOperGrid
            //    KontrOperTableView
            KontrOperTableView.ShowTotalSummary = true;
            KontrOperGrid.TotalSummary.Clear();
            /*<dxg:GridSummaryItem ShowInColumn="CrsKontrIn" FieldName="CrsKontrIn"
                                                 SummaryType="Sum" DisplayFormat="n2" />
                            <dxg:GridSummaryItem ShowInColumn="CrsKontrOut" FieldName="CrsKontrOut"
                                                 SummaryType="Sum" DisplayFormat="n2" />
                            <dxg:GridSummaryItem ShowInColumn="CrsKontrStart" FieldName="CrsKontrStart"
                                                 SummaryType="Sum" DisplayFormat="n2" />
                            <dxg:GridSummaryItem ShowInColumn="CrsKontrEnd" FieldName="CrsKontrEnd"
                                                 SummaryType="Sum" DisplayFormat="n2" />*/
            KontrOperGrid.TotalSummary.Add(new GridSummaryItem
            {
                SummaryType = SummaryItemType.Sum,
                FieldName = "CrsKontrIn",
                DisplayFormat = "n2",
                ShowInColumn = "CrsKontrIn"
            });
            KontrOperGrid.TotalSummary.Add(new GridSummaryItem
            {
                SummaryType = SummaryItemType.Sum,
                FieldName = "CrsKontrOut",
                DisplayFormat = "n2",
                ShowInColumn = "CrsKontrOut"
            });
            KontrOperGrid.TotalSummary.Add(new GridSummaryItem
            {
                SummaryType = SummaryItemType.Sum,
                FieldName = "CrsKontrStart",
                DisplayFormat = "n2",
                ShowInColumn = "CrsKontrStart"
            });
            KontrOperGrid.TotalSummary.Add(new GridSummaryItem
            {
                SummaryType = SummaryItemType.Sum,
                FieldName = "CrsKontrEnd",
                DisplayFormat = "n2",
                ShowInColumn = "CrsKontrEnd"
            });
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
