using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.ReportManagers.SFClientAndWayBill;
using Reports.Base;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public class WaybillWindowViewModel : RSWindowViewModelBase
    {
        private readonly WarehouseManager DocManager = new WarehouseManager(new StandartErrorManager(
            GlobalOptions.GetEntities(),
            "WaybillViewModel"));

        private WaybillRow myCurrentNomenklRow;
        private Waybill myDocument;

        public WaybillWindowViewModel()
        {
            ReportManager = new ReportManager();
            CreateReports();
            LoadByWhom();
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocRightBar(this);
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            if (prn != null)
            {
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Товарная накладная",
                    Command = PrintWaybillCommand
                });
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportCommand
                });
            }
            Document = DocManager.NewWaybill();
        }

        public WaybillWindowViewModel(decimal dc)
        {
            ReportManager = new ReportManager();
            CreateReports();
            LoadByWhom();
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocRightBar(this);
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            if (prn != null)
            {
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Товарная накладная",
                    Command = PrintWaybillCommand
                });
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportCommand
                });
            }
            RefreshData(dc);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ReportManager ReportManager { get; set; }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<string> ByWhomLicoList { set; get; } = new ObservableCollection<string>();
        public ObservableCollection<WaybillRow> Rows { set; get; } = new ObservableCollection<WaybillRow>();
        public List<Core.EntityViewModel.Warehouse> Sklads => MainReferences.Warehouses.Values.ToList();
        public Waybill Document
        {
            set
            {
                if (myDocument != null && myDocument.Equals(value)) return;
                myDocument = value;
                Rows.Clear();
                foreach (var r in Document.Rows) Rows.Add(r);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Rows));
            }
            get => myDocument;
        }
        public override string WindowName => Document?.Name;
        public List<Nomenkl> Nomenkls => MainReferences.ALLNomenkls.Values.ToList();
        public List<Kontragent> Kontragents => MainReferences.AllKontragents.Values.ToList();
        public WaybillRow CurrentNomenklRow
        {
            get => myCurrentNomenklRow;
            set
            {
                if (myCurrentNomenklRow != null && myCurrentNomenklRow.Equals(value)) return;
                myCurrentNomenklRow = value;
                RaisePropertyChanged();
            }
        }

        private void CreateReports()
        {
            ReportManager.Reports.Add("Экспорт", new WaybillTorg12(this)
            {
                PrintOptions = new KursReportA4PrintOptions(),
                ShowType = ReportShowType.Spreadsheet,
                XlsFileName = "torg12"
            });
            ReportManager.Reports.Add("Торг12", new WaybillTorg12(this)
            {
                PrintOptions = new KursReportLandscapeA4PrintOptions
                {
                    FitToWidth = 1
                },
                ShowType = ReportShowType.Report,
                XlsFileName = "torg12"
            });
        }

        private void LoadByWhom()
        {
            ByWhomLicoList.Clear();
            foreach (var item in GlobalOptions.GetEntities()
                .Database.SqlQuery<string>("SELECT DISTINCT DD_KOMU_PEREDANO from sd_24 (nolock)")
                .ToList())
                ByWhomLicoList.Add(item);
        }

        private void Refresh(decimal dc)
        {
            LoadByWhom();
            var d = DocManager.GetWaybill(dc);
            Document = d ?? DocManager.NewWaybill();
        }

        #region Command

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NotEdited;
        public override bool IsCanSaveData => Document != null && (Document.State != RowStatus.NotEdited
                                                                   || Document.Rows.Any(_ =>
                                                                       _.State != RowStatus.NotEdited)
                                                                   || Document.DeletedRows.Count > 0);
        public override RowStatus State => Document?.State ?? RowStatus.NewRow;

        public override void RefreshData(object obj)
        {
            if (Document != null && Document.DocCode > 0)
            {
                var d = DocManager.GetWaybill(Document.DocCode);
                Document = d ?? DocManager.NewWaybill();
            }
            else
            {
                var dc = obj as decimal? ?? 0;
                if (dc != 0)
                    Document = DocManager.GetWaybill(dc);
            }
            if (Document != null && Document.State == RowStatus.NewRow)
                WindowName = Document.ToString();
            else
                WindowName = "Расходная накладная (новая)";
            RaisePropertyChanged(nameof(Document));
            RaisePropertyChanged(nameof(State));
            RaisePropertyChanged(nameof(Document.Sender));
            RaisePropertyChanged(nameof(Document.WarehouseIn));
        }

        public override bool IsRedoAllow
        {
            get => Document.DeletedRows != null && Document.DeletedRows.Count > 0;
            set => base.IsRedoAllow = value;
        }
        public Command ExportCommand
        {
            get { return new Command(ExportWayBill, param => true); }
        }

        public void ExportWayBill(object obj)
        {
            ReportManager.Reports["Экспорт"].Show();
        }

        public Command PrintWaybillCommand
        {
            get { return new Command(PrintWaybill, param => true); }
        }

        public void PrintWaybill(object obj)
        {
            ReportManager.Reports["Торг12"].Show();
        }

        public Command SFClientSelectCommand
        {
            get { return new Command(SFClientSelect, _ => true); }
        }

        private void SFClientSelect(object obj)
        {
            StandartDialogs.SelectInvoiceClient(Document);
        }

        public Command SelectKontragentCommand
        {
            get { return new Command(SelectKontragent, param => Document.InvoiceClient == null); }
        }

        private void SelectKontragent(object obj)
        {
            var k = StandartDialogs.SelectKontragent();
            if (k != null)
                Document.Client = k;
        }

        #endregion
    }
}