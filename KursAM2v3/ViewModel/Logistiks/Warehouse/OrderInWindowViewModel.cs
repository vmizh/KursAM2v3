using System;
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
using Core.Helper;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.ReportManagers;
using KursAM2.Repositories;
using KursAM2.View.Base;
using KursAM2.View.Helper;
using KursAM2.View.Logistiks.UC;
using KursAM2.View.Logistiks.Warehouse;
using KursAM2.ViewModel.Management.Calculations;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class OrderInWindowViewModel : RSWindowViewModelBase
    {
        public readonly GenericKursDBRepository<SD_24> GenericOrderInRepository;
        private readonly WarehouseManager orderManager;

        // ReSharper disable once NotAccessedField.Local
        public readonly ISD_24Repository SD_24Repository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private WarehouseOrderIn myDocument;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public OrderInWindowViewModel(StandartErrorManager errManager)
        {
            GenericOrderInRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            SD_24Repository = new SD_24Repository(UnitOfWork);
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
            GenericOrderInRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            SD_24Repository = new SD_24Repository(UnitOfWork);
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            orderManager = new WarehouseManager(errManager);
            //RefreshData(dc);
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            prn?.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Ордер",
                Command = PrintOrderCommand
            });
            if (dc == 0)
            {
                Document = new WarehouseOrderIn {State = RowStatus.NewRow};
                UnitOfWork.Context.SD_24.Add(Document.Entity);
            }
            else
            {
                Document = new WarehouseOrderIn(GenericOrderInRepository
                            .GetById(dc));
                {
                    State = RowStatus.NotEdited;
                }
                if (Document != null)
                    WindowName = Document.ToString();
                Document.Rows.ForEach(_ => _.State = RowStatus.NotEdited);
                Document.myState = RowStatus.NotEdited;
            }
        }

        private void RaiseAll()
        {
            Document.RaisePropertyAllChanged();
            foreach (var r in Document.Rows) r.RaisePropertyAllChanged();
        }
        #region Properties

        public List<Core.EntityViewModel.NomenklManagement.Warehouse>
            WarehouseList { set; get; } = MainReferences.Warehouses.Values.OrderBy(_ => _.Name).ToList();

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

  
        public ICommand LinkToSchetCommand
        {
            get { return new Command(LinkToSchet, _ => true); }
        }

        private void LinkToSchet(object obj)
        {
            switch (Document.WarehouseSenderType)
            {
                case WarehouseSenderType.Kontragent:
                    SelectSchet();
                    break;
                case WarehouseSenderType.Store:
                    SelectRashOrder();
                    break;
            }
        }

        private void SelectRashOrder()
        {
            WindowManager.ShowFunctionNotReleased();
        }

        private void SelectSchet()
        {
            if (Document.WarehouseIn == null)
            {
                WinManager.ShowWinUIMessageBox("Не выбран склад.", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var ctx = new AddNomenklFromInvoiceProviderViewModel(Document.WarehouseIn,
                Document.KontragentSender);
            var dlg = new SelectDialogView {DataContext = ctx};
            ctx.Form = dlg;
            if (dlg.ShowDialog() == false) return;
            if (Document.KontragentSender == null) Document.KontragentSender = ctx.CurrentInvoice.Kontragent;

            using (var dbctx = GlobalOptions.GetEntities())
            {
                foreach (var r in ctx.Nomenkls.Where(_ => _.IsChecked && _.Quantity > 0).ToList())
                {
                    var old = Document.Rows.FirstOrDefault(_ => _.DDT_NOMENKL_DC == r.Nomenkl.DocCode);
                    if (old != null) continue;
                    var invRow = dbctx.TD_26
                        .Include(_ => _.SD_26).FirstOrDefault(_ => _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                    var schetRow = invRow != null ? new InvoiceProviderRow(invRow) : null;
                    Document.Rows.Add(new WarehouseOrderInRow
                    {
                        DocCode = -1,
                        Nomenkl = r.Nomenkl,
                        DDT_KOL_PRIHOD = r.Quantity,
                        Unit = r.Nomenkl.Unit,
                        DDT_SPOST_DC = r.DocCode,
                        LinkInvoice = schetRow,
                        DDT_SPOST_ROW_CODE = r.Code,
                        DDT_CRS_DC = r.Nomenkl.Currency.DocCode,
                        State = RowStatus.NewRow
                    });
                }
            }

            if (Document.Entity.DD_SPOST_DC == null)
                using (var context = GlobalOptions.GetEntities())
                {
                    var s26 = context.SD_26.FirstOrDefault(_ => _.DOC_CODE == ctx.CurrentInvoice.DocCode);
                    if (s26 != null)
                    {
                        Document.DD_SCHET =
                            $"№{s26.SF_POSTAV_NUM}/{s26.SF_IN_NUM} " +
                            $"от {s26.SF_POSTAV_DATE.ToShortDateString()} ";
                        Document.Entity.DD_SPOST_DC = ctx.CurrentInvoice.DocCode;
                    }
                }
        }

        public ICommand DeleteLinkSchetCommand
        {
            get { return new Command(DeleteLinkSchet, _ => Document.Entity.DD_SPOST_DC != null); }
        }

        private void DeleteLinkSchet(object obj)
        {
            if (WinManager.ShowWinUIMessageBox("Вы хотите удалить счет и связанные с ним строки?",
                "Запрос", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
            var delList =
                new List<WarehouseOrderInRow>(Document.Rows.Where(_ =>
                    _.DDT_SPOST_DC == Document.Entity.DD_SPOST_DC));
            foreach (var r in delList)
            {
                if (r.State != RowStatus.NewRow) Document.DeletedRows.Add(r);
                Document.Rows.Remove(r);
            }

            Document.Entity.DD_SPOST_DC = null;
            Document.DD_SCHET = null;
        }

        public ICommand OpenLinkSchetCommand
        {
            get { return new Command(OpenLinkSchet, _ => Document.Entity.DD_SPOST_DC != null); }
        }

        private void OpenLinkSchet(object obj)
        {
            // ReSharper disable once PossibleInvalidOperationException
            DocumentsOpenManager.Open(DocumentType.InvoiceProvider, (decimal) Document.Entity.DD_SPOST_DC);
        }

        public ICommand PrintOrderCommand
        {
            get { return new Command(PrintOrder, _ => State != RowStatus.NewRow); }
        }

        private void PrintOrder(object obj)
        {
            ReportManager.WarehouseOrderInReport(Document.DocCode);
        }

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NewRow;

        public override bool IsCanSaveData => Document != null && (Document.State != RowStatus.NotEdited
                                                                   || Document.Rows.Any(_ =>
                                                                       _.State != RowStatus.NotEdited)
                                                                   || Document.DeletedRows.Count > 0);

        public override RowStatus State => Document?.State ?? RowStatus.NewRow;
        public override string LayoutName => "OrderWarehouseInLayout";

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
            ctx.Document = orderManager.NewOrderInCopy(Document.DocCode);
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
            base.RefreshData(obj);
            if (IsCanSaveData)
            {
                var service = GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "В документ внесены изменения, сохранить?";
                if (service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this) == MessageResult.Yes)
                {
                    SaveData(null);
                    return;
                }
            }
            EntityManager.EntityReload(UnitOfWork.Context);
            foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
            RaiseAll();
            foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
            Document.myState = RowStatus.NotEdited;
            Document.RaisePropertyChanged("State");
        }

        public override void SaveData(object data)
        {
            Document.DD_POLUCH_NAME = Document.WarehouseIn.Name;
            Document.DD_OTRPAV_NAME = Document.Sender;
            Document.Entity.DD_TYPE_DC = 2010000001;
            var dc = orderManager.SaveOrderIn(Document);
            DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.StoreOrderIn), null,
                Document.DocCode, null, (string)Document.ToJson());
            if (dc > 0)
            {
                RefreshData(dc);
                DocumentsOpenManager.SaveLastOpenInfo(DocumentType.StoreOrderIn, Document.Id, Document.DocCode,
                    Document.CREATOR,
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

        public ICommand SenderSelectCommand
        {
            get { return new Command(SenderSelect, _ => true); }
        }

        public bool IsSenderTypeEnabled => Document.Sender == null;

        public void SenderSelect(object obj)
        {
            switch (Document?.WarehouseSenderType)
            {
                case WarehouseSenderType.Kontragent:
                    var kontr = StandartDialogs.SelectKontragent();
                    if (kontr == null) return;
                    Document.KontragentSender = kontr;
                    break;
                case WarehouseSenderType.Store:
                    var warehouse = StandartDialogs.SelectWarehouseDialog();
                    if (warehouse == null) return;
                    Document.WarehouseOut = warehouse;
                    //var win = new WindowManager();
                    // ReSharper disable once PossibleUnintendedReferenceComparison
                    break;
            }

            Document?.RaisePropertyChanged("Sender");
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
        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.StoreOrderIn, null, Document.DocCode, null);
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
            if (Document.WarehouseIn == null)
            {
                WinManager.ShowWinUIMessageBox("Не выбран склад получатель","Предупреждение",MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
            get { return new Command(DeleteNomenkl, _ => CurrentRow != null && (CurrentRow.State == RowStatus.NewRow 
                                                                    || CurrentRow.LinkInvoice == null)); }
        }

        private void DeleteNomenkl(object obj)
        {
            if (CurrentRow.State == RowStatus.NewRow)
            {
                Document.Rows.Remove(CurrentRow);
            }
            else
            {
                Document.Entity.TD_24.Remove(CurrentRow.Entity);
                Document.DeletedRows.Add(CurrentRow);
                Document.Rows.Remove(CurrentRow);
            }
        }

        public override void DocDelete(object form)
        {
            if (Document.State != RowStatus.NewRow)
            {
                foreach (var r in Document.Rows)
                {
                    if (r.LinkInvoice == null) continue;
                    MessageBox.Show("В ордере есть строки привязанные к счету, удаление не возможно.", "Запрос",
                        MessageBoxButton.OK,
                        MessageBoxImage.Stop);
                    return;
                }
            }

            var res = MessageBox.Show("Вы уверены, что хотите удалить данный документ?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    var dc = Document.DocCode;
                    var docdate = Document.Date;
                    if (Document.State == RowStatus.NewRow)
                    {
                        Form.Close();
                        return;
                    }

                    try
                    {
                        UnitOfWork.CreateTransaction();
                        var doc = UnitOfWork.Context.SD_24.FirstOrDefault(_ => _.DOC_CODE == Document.DocCode);
                        if(doc != null)
                            UnitOfWork.Context.SD_24.Remove(doc);
                        UnitOfWork.Save();
                        UnitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        UnitOfWork.Rollback();
                        WindowManager.ShowError(ex);
                    }

                    RecalcKontragentBalans.CalcBalans(dc, docdate);
                    //orderManager.DeleteOrderIn(Document);
                    if (Form != null) Form.Close();
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