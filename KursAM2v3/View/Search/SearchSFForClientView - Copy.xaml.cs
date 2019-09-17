using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Grid;
using KursAM2.Helper;
using KursAM2.LayoutManager;
using KursAM2.ViewModel.Finance;

namespace KursAM2.View.Search
{
    /// <summary>
    ///     Interaction logic for SearchDogovorForClientView.xaml
    /// </summary>
    public partial class SearchSFForClientView : DXWindow
    {
        private readonly string LayoutFileName = string.Format("{0}\\Layout\\{1}.xml", Environment.CurrentDirectory,
            "SearchSFForClientView");

        public SearchSFForClientView()
        {
            InitializeComponent();
            Loaded += SearchBaseView_Loaded;
            Closing += SearchBaseView_Closing;
        }

        private void SearchBaseView_Closing(object sender, CancelEventArgs e)
        {
            var saveLayout = new FormInfo
            {
                FormHeight = Height,
                FormWidth = Width,
                FormLeft = Left,
                FormTop = Top,
                FormStartLocation = WindowStartupLocation,
                FormState = WindowState
            };

            var ms = new MemoryStream();
            DXSerializer.Serialize(MainBarManager, ms, "Kurs", null);
            saveLayout.Layout = ms.ToArray();

            var writer = new FileStream(LayoutFileName, FileMode.Create);
            var ser =
                new DataContractSerializer(typeof (FormInfo));
            ser.WriteObject(writer, saveLayout);
            writer.Close();
        }

        private void SearchBaseView_Loaded(object sender, RoutedEventArgs e)
        {
            var ctx = DataContext as SFForClientSearchViewModel;
            if (ctx == null) return;
            Title = ctx.WindowName;
            ctx.StartDate = DateTime.Today.AddDays(-1);
            ctx.EndDate = DateTime.Today;
            try
            {
                if (!File.Exists(LayoutFileName)) return;
                using (var fs = File.OpenRead(LayoutFileName))
                {
                    var r = XmlReader.Create(fs);
                    var p =
                        new DataContractSerializer(typeof (FormInfo)).ReadObject(r) as
                            FormInfo;
                    if (p == null) return;
                    Height = p.FormHeight;
                    Width = p.FormWidth;
                    Left = p.FormLeft;
                    Top = p.FormTop;
                    WindowStartupLocation = p.FormStartLocation;
                    WindowState = p.FormState;
                    MemoryStream ms;
                    if (p.Layout == null) return;
                    ms = new MemoryStream(p.Layout);
                    DXSerializer.Deserialize(MainBarManager, ms, "Kurs", null);
                    ms.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.ShowError(ex);
            }
        }

        private void PART_GridControlKontragent_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as GridControl;
            if (grid == null) return;
            if (!File.Exists(string.Format("{0}\\Layout\\{1}.xml", Environment.CurrentDirectory,
                "KontragentLookUpEdit"))) return;

            grid.RestoreLayoutFromXml(string.Format("{0}\\Layout\\{1}.xml", Environment.CurrentDirectory,
                "KontragentLookUpEdit"));
        }

        private void PART_GridControlKontragent_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as GridControl;
            if (grid == null) return;

            grid.SaveLayoutToXml(string.Format("{0}\\Layout\\{1}.xml", Environment.CurrentDirectory,
                "KontragentLookUpEdit"));
        }

        private void PART_GridControlNomenkl_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as GridControl;
            if (grid == null) return;
            if (!File.Exists(string.Format("{0}\\Layout\\{1}.xml", Environment.CurrentDirectory,
                "NomenklLookUpEdit"))) return;
            grid.RestoreLayoutFromXml(string.Format("{0}\\Layout\\{1}.xml", Environment.CurrentDirectory,
                "NomenklLookUpEdit"));
        }

        private void PART_GridControlNomenkl_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as GridControl;
            if (grid == null) return;

            grid.SaveLayoutToXml(string.Format("{0}\\Layout\\{1}.xml", Environment.CurrentDirectory,
                "NomenklLookUpEdit"));
        }
    }
}