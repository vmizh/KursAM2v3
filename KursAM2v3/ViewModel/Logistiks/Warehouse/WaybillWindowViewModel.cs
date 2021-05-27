using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.Managers.Nomenkl;
using KursAM2.ReportManagers.SFClientAndWayBill;
using Reports.Base;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public class WaybillWindowViewModel : RSWindowViewModelBase
    {
        private readonly WarehouseManager docManager = new WarehouseManager(new StandartErrorManager(
            GlobalOptions.GetEntities(),
            "WaybillViewModel"));

        private WaybillRow myCurrentNomenklRow;
        private Waybill myDocument;
        private readonly WindowManager winManager = new WindowManager();

        public WaybillWindowViewModel()
        {
            ReportManager = new ReportManager();
            CreateReports();
            LoadByWhom();
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
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

            Document = docManager.NewWaybill();
        }

        public WaybillWindowViewModel(decimal dc)
        {
            ReportManager = new ReportManager();
            CreateReports();
            LoadByWhom();
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
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

            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(dc);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ReportManager ReportManager { get; set; }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<string> ByWhomLicoList { set; get; } = new ObservableCollection<string>();
        public List<Core.EntityViewModel.Warehouse> Sklads => MainReferences.Warehouses.Values.ToList();

        public Waybill Document
        {
            set
            {
                if (myDocument != null && myDocument.Equals(value)) return;
                myDocument = value;
                RaisePropertyChanged();
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

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<WaybillRow> SelectedRows { set; get; }
            = new ObservableCollection<WaybillRow>();

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
                .Database.SqlQuery<string>("SELECT DISTINCT DD_KOMU_PEREDANO FROM sd_24 (nolock) " +
                                           "WHERE DD_KOMU_PEREDANO IS NOT null")
                .ToList())
                ByWhomLicoList.Add(item);
        }

        #region Command

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NewRow;

        public override bool IsCanSaveData => Document != null && Document.WarehouseOut != null
                                                               && (Document.State != RowStatus.NotEdited
                                                                   || Document.Rows.Any(_ =>
                                                                       _.State != RowStatus.NotEdited)
                                                                   || Document.DeletedRows.Count > 0);

        public override RowStatus State => Document?.State ?? RowStatus.NewRow;

        public ICommand DeleteLinkDocumentCommand
        {
            get { return new Command(DeleteLinkDocument, _ => true); }
        }

        private void DeleteLinkDocument(object obj)
        {
        }

        public override void DocDelete(object form)
        {
            if (Document.State != RowStatus.Edited)
            {
                var res = WinManager.ShowWinUIMessageBox("Вы уверены, что хотите удалить данный документ?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (res != MessageBoxResult.Yes) return;
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        //InvoicesManager.DeleteProvider(Document.DocCode);
                        using (var ctx = GlobalOptions.GetEntities())
                        {
                            var doc = ctx.SD_24.FirstOrDefault(_ => _.DOC_CODE == Document.DocCode);
                            if (doc != null)
                            {
                                foreach (var d in ctx.TD_24.Where(_ => _.DOC_CODE == doc.DOC_CODE)) ctx.TD_24.Remove(d);
                                ctx.SD_24.Remove(doc);
                            }

                            ctx.SaveChanges();
                        }

                        Form.Close();
                        return;
                    // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
                    case MessageBoxResult.No:
                        Form.Close();
                        return;
                    // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
                    case MessageBoxResult.Cancel:
                        return;
                }
            }
            else
            {
                InvoicesManager.DeleteProvider(Document.DocCode);
                Form.Close();
            }

            InvoicesManager.DeleteProvider(Document.DocCode);
            Form.Close();
        }

        public ICommand AddFromDocumentCommand
        {
            get { return new Command(AddFromDocument, _ => true); }
        }

        private void AddFromDocument(object obj)
        {
        }

        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => Document.WarehouseOut != null); }
        }

        private void AddNomenkl(object obj)
        {
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            var nomenkls = StandartDialogs.SelectNomenkls(null, true);
            if (nomenkls == null || nomenkls.Count <= 0) return;
            foreach (var n in nomenkls)
            {
                var m = NomenklCalculationManager.NomenklRemain(Document.DD_DATE, n.DocCode,
                    Document.WarehouseOut.DocCode);
                if (m <= 0)
                {
                    winManager.ShowWinUIMessageBox($"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
                                                   $"{MainReferences.Warehouses[Document.WarehouseOut.DocCode]}" +
                                                   $"кол-во {m}. Операция по номенклатуре не может быть проведена.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode))
                    Document.Rows.Add(new WaybillRow
                    {
                        DocCode = -1,
                        Code = newCode,
                        Nomenkl = n,
                        DDT_KOL_PRIHOD = 1,
                        Unit = n.Unit,
                        Currency = n.Currency,
                        State = RowStatus.NewRow
                    });
            }

            RaisePropertyChanged(nameof(Document));
        }

        public ICommand DeleteNomenklCommand
        {
            get { return new Command(DeleteNomenkl, _ => CurrentNomenklRow != null); }
        }

        private void DeleteNomenkl(object obj)
        {
            var delList = new List<WaybillRow>(SelectedRows.ToList());
            foreach (var row in delList)
                if (row.State == RowStatus.NewRow)
                {
                    Document.Rows.Remove(row);
                }
                else
                {
                    Document.DeletedRows.Add(row);
                    Document.Rows.Remove(row);
                }
        }

        public ICommand OpenSchetCommand
        {
            get
            {
                return new Command(OpenSchet, _ => CurrentNomenklRow != null &&
                                                   CurrentNomenklRow.DDT_SFACT_DC != null);
            }
        }

        private void OpenSchet(object obj)
        {
            // ReSharper disable once PossibleInvalidOperationException
            DocumentsOpenManager.Open(DocumentType.InvoiceClient, (decimal) CurrentNomenklRow.DDT_SFACT_DC);
        }

        public override void RefreshData(object obj)
        {
            if (Document != null && Document.DocCode > 0)
            {
                var d = docManager.GetWaybill(Document.DocCode);
                Document = d ?? docManager.NewWaybill();
            }
            else
            {
                var dc = obj as decimal? ?? 0;
                if (dc != 0)
                    Document = docManager.GetWaybill(dc);
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
            get { return new Command(SFClientSelect, _ => Document.Client != null); }
        }

        public void SFClientSelect(object obj)
        {
            var inv = StandartDialogs.SelectInvoiceClient(Document);
            if (inv != null)
            {
                Document.InvoiceClient = inv;
                Document.Client = inv.Client;
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
                    foreach (var r in inv.Rows)
                    {
                        var oldf = Document.Rows.FirstOrDefault(_ => _.DDT_NOMENKL_DC == r.SFT_NEMENKL_DC);
                        if (oldf != null)
                        {
                            if (oldf.DDT_SFACT_DC == r.DOC_CODE && oldf.DDT_SFACT_ROW_CODE == r.Code)
                                oldf.DDT_KOL_RASHOD = (decimal) r.SFT_KOL;
                            continue;
                        }

                        var otgr = ctx.TD_24.Where(_ => _.DDT_SFACT_DC == r.DOC_CODE
                                                        && _.DDT_SFACT_ROW_CODE == r.Code);
                        if (otgr.Any())
                        {
                            var kol = otgr.Sum(_ => _.DDT_KOL_RASHOD);
                            if (kol < (decimal) r.SFT_KOL)
                            {
                                var n = MainReferences.GetNomenkl(r.SFT_NEMENKL_DC);
                                var newItem = new WaybillRow
                                {
                                    DocCode = Document.DocCode,
                                    Code = newCode,
                                    Nomenkl = n,
                                    DDT_KOL_RASHOD = (decimal) r.SFT_KOL - kol,
                                    Unit = n.Unit,
                                    Currency = n.Currency,
                                    SchetLinkedRow = r,
                                    State = RowStatus.NewRow
                                };
                                newItem.DDT_SFACT_DC = r.DOC_CODE;
                                newItem.DDT_SFACT_ROW_CODE = r.Code;
                                Document.Rows.Add(newItem);
                            }
                        }
                        else
                        {
                            var n = MainReferences.GetNomenkl(r.SFT_NEMENKL_DC);
                            var m = NomenklManager.GetNomenklCount(Document.DD_DATE, n.DocCode,
                                Document.WarehouseOut.DocCode);
                            if (m <= 0)
                            {
                                winManager.ShowWinUIMessageBox($"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
                                                               $"{MainReferences.Warehouses[Document.WarehouseOut.DocCode]}" +
                                                               $"кол-во {m}. Операция по номенклатуре не может быть проведена.",
                                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                                continue;
                            }

                            var newItem = new WaybillRow
                            {
                                DocCode = Document.DocCode,
                                Code = newCode,
                                Nomenkl = n,
                                DDT_KOL_RASHOD = (decimal) r.SFT_KOL,
                                Unit = n.Unit,
                                Currency = n.Currency,
                                SchetLinkedRow = r,
                                State = RowStatus.NewRow,
                                DDT_SFACT_DC = r.DOC_CODE,
                                DDT_SFACT_ROW_CODE = r.Code
                            };
                            Document.Rows.Add(newItem);
                        }
                    }
                }
            }
        }

        public Command SelectKontragentCommand
        {
            get { return new Command(SelectKontragent, param => Document.InvoiceClient == null); }
        }

        public void SelectKontragent(object obj)
        {
            var k = StandartDialogs.SelectKontragent();
            if (k != null)
                Document.Client = k;
        }

        public override void SaveData(object data)
        {
            Document.DD_POLUCH_NAME = Document.Client.Name.Length > 50
                ? Document.Client.Name.Substring(0, 50)
                : Document.Client.Name;
            Document.DD_OTRPAV_NAME = Document.WarehouseOut.Name.Length > 50
                ? Document.WarehouseOut.Name.Substring(0, 50)
                : Document.WarehouseOut.Name;
            Document.DD_TYPE_DC = 2010000012;
            var dc = docManager.SaveWaybill(Document);
            if (dc > 0)
            {
                RefreshData(dc);
                DocumentsOpenManager.SaveLastOpenInfo(DocumentType.Waybill, Document.Id, Document.DocCode, Document.CREATOR,
                    "", Document.Description);
            }
        }

        #endregion
    }

    public class ListString
    {
        public decimal DocCode { set; get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Name { set; get; }

        public override string ToString()
        {
            return Name;
        }
    }
}