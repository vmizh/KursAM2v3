using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Invoices;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.ReportManagers;
using KursAM2.View.Base;
using KursAM2.View.Logistiks.UC;
using KursAM2.View.Logistiks.Warehouse;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class OrderInWindowViewModel : RSWindowViewModelBase
    {
        private readonly WarehouseManager orderManager;
        private WarehouseOrderIn myDocument;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public OrderInWindowViewModel(StandartErrorManager errManager)
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            orderManager = new WarehouseManager(errManager);
            Document = orderManager.NewOrderIn();
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            prn?.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Ордер",
                Command = PrintOrderCommand
            });
        }

        public OrderInWindowViewModel(StandartErrorManager errManager, decimal dc)
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            orderManager = new WarehouseManager(errManager);
            RefreshData(dc);
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            prn?.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Ордер",
                Command = PrintOrderCommand
            });
        }

        #region Properties

        public WarehouseOrderIn Document
        {
            set
            {
                if (myDocument != null && myDocument.Equals(value)) return;
                myDocument = value;
                RaisePropertyChanged();
            }
            get => myDocument;
        }

        //public ObservableCollection<WarehouseOrderInRow> Rows { set; get; } = new ObservableCollection<WarehouseOrderInRow>();
        private WarehouseOrderInRow myCurrentRow;

        public WarehouseOrderInRow CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow != null && myCurrentRow.Equals(value)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCanChangedWarehouseType => Document?.Sender == null;

        public override string WindowName =>
            $"Приходный складской ордер №{Document?.DD_IN_NUM}/{Document?.DD_EXT_NUM} от {Document?.Date}";

        #endregion

        #region Command

        public ICommand PrintOrderCommand
        {
            get { return new Command(PrintOrder, param => State != RowStatus.NewRow); }
        }

        private void PrintOrder(object obj)
        {
            ReportManager.WarehouseOrderInReport(Document.DocCode);
        }

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NotEdited;

        public override bool IsCanSaveData => Document != null && (Document.State != RowStatus.NotEdited
                                                                   || Document.Rows.Any(_ =>
                                                                       _.State != RowStatus.NotEdited)
                                                                   || Document.DeletedRows.Count > 0);

        public override RowStatus State => Document?.State ?? RowStatus.NewRow;

        public override void DocNewEmpty(object form)
        {
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                {Form = frm};
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopy(object form)
        {
            if (Document == null) return;
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                {Form = frm};
            ctx.Document = orderManager.NewOrderInCopy(Document);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (Document == null) return;
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                "WarehouseOrderIn", true)) {Form = frm, Document = orderManager.NewOrderInRecuisite(Document)};
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void RefreshData(object obj)
        {
            if (Document != null && Document.DocCode > 0)
            {
                Document = orderManager.GetOrderIn(Document.DocCode);
            }
            else
            {
                var dc = obj as decimal? ?? 0;
                if (dc != 0)
                    Document = orderManager.GetOrderIn(dc);
            }

            RaisePropertyChanged(nameof(Document));
            RaisePropertyChanged(nameof(Document.Sender));
            RaisePropertyChanged(nameof(Document.WarehouseIn));
        }

        public override void SaveData(object data)
        {
            Document.DD_POLUCH_NAME = Document.WarehouseIn.Name;
            Document.DD_OTRPAV_NAME = Document.Sender;
            Document.Entity.DD_TYPE_DC = 2010000001;
            var dc = orderManager.SaveOrderIn(Document);
            if (dc > 0)
            {
                RefreshData(dc);
                DocumentsOpenManager.SaveLastOpenInfo(DocumentType.StoreOrderIn, Document.Id, Document.DocCode, Document.CREATOR,
                    "", Document.Description);
            }
        }

        public ICommand DeleteLinkDocumentCommand
        {
            get { return new Command(DeleteLinkDocument, _ => CurrentRow != null && CurrentRow.LinkDocument != null); }
        }

        private void DeleteLinkDocument(object obj)
        {
            foreach (var r in Document.SelectedRows)
            {
                r.LinkInvoice = null;
                r.LinkOrder = null;
                r.DDT_SPOST_DC = null;
                r.DDT_SPOST_ROW_CODE = null;
                r.InvoiceProvider = null;
                r.InvoiceProviderRow = null;
                r.DDT_TAX_EXECUTED = 0;
                r.DDT_FACT_EXECUTED = 0;
            }
        }

        public ICommand OpenLinkDocumentCommand
        {
            get
            {
                return new Command(OpenLinkDocument,
                    _ => CurrentRow != null && CurrentRow.LinkDocument != null && CurrentRow.DDT_SPOST_DC != null);
            }
        }

        private void OpenLinkDocument(object obj)
        {
            // ReSharper disable once PossibleInvalidOperationException
            DocumentsOpenManager.Open(DocumentType.InvoiceProvider, (decimal) CurrentRow.DDT_SPOST_DC);
        }

        public ICommand AddFromDocumentCommand
        {
            get
            {
                return new Command(AddFromDocument, _ => Document?.WarehouseIn != null &&
                                                         Document?.KontragentSender != null);
            }
        }

        private void AddFromDocument(object obj)
        {
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            var ctx = new AddNomenklFromInvoiceProviderRowViewModel(Document.WarehouseIn,
                Document.KontragentSender);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            if (dlg.ShowDialog() == false) return;
            using (var dbctx = GlobalOptions.GetEntities())
            {
                foreach (var r in ctx.Nomenkls.Where(_ => _.IsChecked))
                {
                    var old = Document.Rows.FirstOrDefault(_ => _.Nomenkl.DocCode == r.Nomenkl.DocCode);
                    if (old != null && old.LinkInvoice == null)
                    {
                        var invRow = dbctx.TD_26
                            .Include(_ => _.SD_26)
                            .FirstOrDefault(_ => _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                        var schetRow = invRow != null ? new InvoiceProviderRow(invRow) : null;
                        old.LinkInvoice = schetRow;
                    }

                    if (old == null)
                    {
                        var invRow = dbctx.TD_26
                            .Include(_ => _.SD_26)
                            .FirstOrDefault(_ => _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                        var schetRow = invRow != null ? new InvoiceProviderRow(invRow) : null;
                        Document.Rows.Add(new WarehouseOrderInRow
                        {
                            DocCode = Document.DocCode,
                            Code = newCode,
                            Nomenkl = r.Nomenkl,
                            DDT_KOL_PRIHOD = r.Quantity,
                            Unit = r.Nomenkl.Unit,
                            DDT_SPOST_DC = r.DocCode,
                            LinkInvoice = schetRow,
                            DDT_SPOST_ROW_CODE = r.Code,
                            DDT_CRS_DC = r.Nomenkl.Currency.DocCode,
                            IsTaxExecuted = true,
                            IsFactExecuted = true,
                            State = RowStatus.NewRow
                        });
                    }
                }
            }
        }

        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => true); }
        }

        private void AddNomenkl(object obj)
        {
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            if (Document.WarehouseSenderType == WarehouseSenderType.Kontragent)
            {
                var nomenkls = StandartDialogs.SelectNomenkls(null, true);
                if (nomenkls == null || nomenkls.Count <= 0) return;
                foreach (var n in nomenkls)
                    if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode))
                        Document.Rows.Add(new WarehouseOrderInRow
                        {
                            DocCode = Document.DocCode,
                            Code = newCode,
                            Nomenkl = n,
                            DDT_KOL_PRIHOD = 1,
                            Unit = n.Unit,
                            Currency = n.Currency,
                            State = RowStatus.NewRow
                        });
            }
            else
            {
                var datarows = StandartDialogs.SelectNomenklsFromRashodOrder(Document.WarehouseIn);
                if (datarows == null || datarows.Count <= 0) return;
                foreach (var n in datarows)
                    if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.DDT_NOMENKL_DC))
                    {
                        var nom = MainReferences.GetNomenkl(n.DDT_NOMENKL_DC);
                        Document.Rows.Add(new WarehouseOrderInRow
                        {
                            DocCode = Document.DocCode,
                            Code = newCode,
                            Nomenkl = nom,
                            DDT_KOL_PRIHOD = n.DDT_KOL_RASHOD,
                            Unit = nom.Unit,
                            Currency = nom.Currency,
                            DDT_SKLAD_OTPR_DC = n.SD_24.DD_SKLAD_OTPR_DC,
                            DDT_RASH_ORD_DC = n.DOC_CODE,
                            DDT_RASH_ORD_CODE = n.Code,
                            State = RowStatus.NewRow
                        });
                    }
            }

            RaisePropertyChanged(nameof(Document));
        }

        public ICommand DeleteNomenklCommand
        {
            get { return new Command(DeleteNomenkl, _ => Document.SelectedRows.Count > 0); }
        }

        private void DeleteNomenkl(object obj)
        {
            var delList = new List<WarehouseOrderInRow>(Document.SelectedRows.ToList());
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

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Вы уверены, что хотите удалить данный документ?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    orderManager.DeleteOrderIn(Document);
                    CloseWindow(Form);
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        public override void CloseWindow(object form)
        {
            if (IsCanSaveData && Document?.State != RowStatus.Deleted)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            if (Form != null)
            {
                Form.Close();
                return;
            }

            var frm = form as Window;
            frm?.Close();
        }

        #endregion
    }
}