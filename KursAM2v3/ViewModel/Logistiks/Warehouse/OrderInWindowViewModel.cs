using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.ReportManagers;
using KursAM2.Repositories;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl.Invoices.ViewModels;
using KursAM2.View.DialogUserControl.Standart;
using KursAM2.View.Helper;
using KursAM2.View.Logistiks.UC;
using KursAM2.View.Logistiks.Warehouse;
using KursAM2.ViewModel.Management.Calculations;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository;
using InvoiceProviderRow = KursDomain.Documents.Invoices.InvoiceProviderRow;

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
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this); 
            IsDocNewCopyAllow = false;
            IsDocNewCopyRequisiteAllow = true;
            orderManager = new WarehouseManager(errManager);
            var doc = new SD_24
            {
                DOC_CODE = -1,
                //State = RowStatus.NewRow,
                DD_IN_NUM = -1,
                DD_EXT_NUM = null,
                DD_DATE = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name,
                Id = Guid.NewGuid()
            };
            UnitOfWork.Context.SD_24.Add(doc);
            Document = new WarehouseOrderIn(doc);
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
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            IsDocNewCopyAllow = false;
            IsDocNewCopyRequisiteAllow = true;
            orderManager = new WarehouseManager(errManager);
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            prn?.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Ордер",
                Command = PrintOrderCommand
            });
            if (dc == 0)
            {
                Document = new WarehouseOrderIn { State = RowStatus.NewRow };
                UnitOfWork.Context.SD_24.Add(Document.Entity);
            }
            else
            {
                var doc = SD_24Repository.GetByDC(dc);
                Document = new WarehouseOrderIn(doc);
                {
                    State = RowStatus.NotEdited;
                }
                if (Document != null)
                    WindowName = Document.ToString();
                if (Document.Entity.DD_SPOST_DC != null)
                {
                    var sf = UnitOfWork.Context.SD_26.First(_ => _.DOC_CODE == Document.Entity.DD_SPOST_DC);
                    Schet = $"№{sf.SF_IN_NUM}/{sf.SF_POSTAV_NUM} от {sf.SF_POSTAV_DATE.ToShortDateString()}";
                }

                Document.Rows.ForEach(_ => _.State = RowStatus.NotEdited);
                Document.State = RowStatus.NotEdited;
            }
        }

       #region Properties

        public List<KursDomain.References.Warehouse>
            WarehouseList { set; get; } = GlobalOptions.ReferencesCache.GetWarehousesAll()
            .Cast<KursDomain.References.Warehouse>().OrderBy(_ => _.Name).ToList();

        public WarehouseOrderIn Document
        {
            set
            {
                if (Equals(myDocument ,value)) return;
                myDocument = value;
                RaisePropertyChanged();
            }
            get => myDocument;
        }

        private WarehouseOrderInRow myCurrentRow;
        private string mySchet;

        public WarehouseOrderInRow CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (Equals(myCurrentRow,value)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCanChangedWarehouseType => Document?.Sender == null;

        public override string WindowName =>
            $"Приходный складской ордер №{Document?.DD_IN_NUM}/{Document?.DD_EXT_NUM} от {Document?.Date}";

        #endregion

        #region Command

        public string Sender => Document.KontragentSender?.Name ?? Document.WarehouseOut?.Name;

        public string Schet
        {
            get => mySchet;
            set
            {
                if (mySchet == value) return;
                mySchet = value;
                RaisePropertyChanged();
            }
        }
        
        public ICommand KontragentTypeChangedCommand
        {
            get
            {
                return new Command(KontragentTypeChanged, _ => IsCanChageKontragent);
            }
        }

        private void KontragentTypeChanged(object obj)
        {
            Document.KontragentSender = null;
            Document.WarehouseIn = null;
        }

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
            RaisePropertyChanged(nameof(IsCanChageKontragent));
        }

        private void SelectRashOrder()
        {
            WindowManager.ShowFunctionNotReleased();
        }

        private void SelectSchet()
        {
            var WinManager = new WindowManager();
            if (Document.WarehouseIn == null)
            {
                WinManager.ShowWinUIMessageBox("Не выбран склад.", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var loadType = InvoiceProviderSearchType.NotShipped;
            if (Document.KontragentSender != null)
                loadType = loadType | InvoiceProviderSearchType.OneKontragent;
            var dtx = new InvoiceProviderSearchDialogViewModel(false, true, loadType, UnitOfWork.Context)
            {
                WindowName = "Выбор счетов фактур",
                LayoutName = "InvoiceProviderSearchMulti",
                KontragentDC = Document.KontragentSender?.DocCode
            };
            dtx.ExistingRows = Document.Rows.Select(r => new Tuple<decimal, decimal, int>(r.Nomenkl.DocCode, r.LinkInvoice.DocCode, r.LinkInvoice.Code)).ToList();
            dtx.RefreshData(null);
            var dialog = new SelectInvoiceMultipleDialogView
            {
                DataContext = dtx,
                Owner = Application.Current.MainWindow
            };
            dtx.Form = dialog;
            dialog.ShowDialog();

            if (dtx.SelectedItems.Count > 0)
            {

                if (Document.KontragentSender == null)
                    Document.KontragentSender =
                        GlobalOptions.ReferencesCache.GetKontragent(dtx.SelectedItems.First().PostDC) as Kontragent;
                var dc = dtx.SelectedItems.First().DocCode;
                var sf = UnitOfWork.Context.SD_26.First(_ => _.DOC_CODE == dc);
                Schet = $"№{sf.SF_IN_NUM}/{sf.SF_POSTAV_NUM} от {sf.SF_POSTAV_DATE.ToShortDateString()}";
                Document.Entity.DD_SPOST_DC = sf.DOC_CODE;
                int code = Document.Rows.Count == 0 ? 1 :Document.Rows.Max(_ => _.Code) + 1;
                foreach (var row in dtx.SelectedItems)
                {
                    var old = Document.Rows.FirstOrDefault(_ => _.DDT_NOMENKL_DC == row.NomenklDC);
                    if (old != null) continue;
                    var invRow = UnitOfWork.Context.TD_26
                        .Include(_ => _.SD_26).FirstOrDefault(_ => _.DOC_CODE == row.DocCode && _.CODE == row.CODE);
                    var schetRow = invRow != null ? new InvoiceProviderRow(invRow) : null;
                    var nom = GlobalOptions.ReferencesCache.GetNomenkl(row.NomenklDC) as Nomenkl;
                    var newEntity = new TD_24
                    { 
                        DOC_CODE = Document.DocCode,
                        CODE = code,
                        DDT_KOL_PRIHOD = row.Quantity-row.Shipped,
                        DDT_SPOST_DC = row.DocCode,
                        DDT_SPOST_ROW_CODE = row.CODE,
                        DDT_CRS_DC = ((IDocCode)nom.Currency).DocCode,
                        DDT_NOMENKL_DC = ((IDocCode)nom).DocCode,
                        Id = Guid.NewGuid(),
                        DocId = Document.Id

                    };
                    Document.Entity.TD_24.Add(newEntity);
                    Document.Rows.Add(new WarehouseOrderInRow(newEntity)
                    {
                        Nomenkl = nom,
                        LinkInvoice = schetRow,
                        // ReSharper disable once PossibleNullReferenceException
                        State = RowStatus.NewRow
                    });
                    code++;
                }
            }

            if (Document.Entity.DD_SPOST_DC == null && dtx.SelectedItems.Count > 0)
            {
                var dc = dtx.SelectedItems.First().DocCode;
                
                    var s26 = UnitOfWork.Context.SD_26.FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (s26 != null)
                    {
                        Document.DD_SCHET =
                            $"№{s26.SF_POSTAV_NUM}/{s26.SF_IN_NUM} " +
                            $"от {s26.SF_POSTAV_DATE.ToShortDateString()} ";
                        Document.Entity.DD_SPOST_DC = dtx.SelectedItems.First().DocCode;
                    }
                
            }
        }



        public ICommand DeleteLinkSchetCommand
        {
            get { return new Command(DeleteLinkSchet, _ => Document.Entity.DD_SPOST_DC != null); }
        }

        private void DeleteLinkSchet(object obj)
        {
            var WinManager = new WindowManager();
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
            DocumentsOpenManager.Open(DocumentType.InvoiceProvider, (decimal)Document.Entity.DD_SPOST_DC);
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
                                                                   || Document.DeletedRows.Count > 0)
                                                               && Document.KontragentSender != null &&
                                                               Document.WarehouseIn != null;

        public override RowStatus State => Document?.State ?? RowStatus.NewRow;
        public override string LayoutName => "OrderWarehouseInLayout";

        public override void DocNewEmpty(object form)
        {
            var frm = new OrderInView { Owner = Application.Current.MainWindow };
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(UnitOfWork.Context,
                    "WarehouseOrderIn", true))
                { Form = frm };
            ctx.Document.myState = RowStatus.NewRow;
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopy(object form)
        {
            if (Document == null) return;
            var frm = new OrderInView { Owner = Application.Current.MainWindow };
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(UnitOfWork.Context,
                    "WarehouseOrderIn", true))
                { Form = frm,
                    Document = orderManager.NewOrderInCopy(Document.DocCode)
                };
            ctx.Document.myState = RowStatus.NewRow;
            frm.DataContext = ctx;
            frm.Show();
            
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (Document == null) return;
            var frm = new OrderInView { Owner = Application.Current.MainWindow };
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(UnitOfWork.Context,
                    "WarehouseOrderIn", true))
                { Form = frm, Document = orderManager.NewOrderInRecuisite(Document) };
            ctx.Document.myState = RowStatus.NewRow;
            frm.DataContext = ctx;
            frm.Show();
            
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            if (IsCanSaveData)
            {
                var service = this.GetService<IDialogService>("WinUIDialogService");
                dialogServiceText = "В документ внесены изменения, сохранить?";
                if (service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this) == MessageResult.Yes)
                {
                    SaveData(null);
                    return;
                }
            }

            var rrows = UnitOfWork.Context.TD_24.Where(_ => _.DOC_CODE == Document.DocCode).ToList();
            foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) 
                entity.Reload();
            var codes = Document.Rows.Select(_ => _.Code).ToList();
            foreach (var code in codes)
            {
                if(rrows.Any(_ => _.CODE == code)) continue;
                var d = Document.Rows.FirstOrDefault(_ => _.Code == code);
                Document.Rows.Remove(d);
            }
            Document.RaisePropertyAllChanged();
            Document.Rows.ForEach(_ => _.RaisePropertyAllChanged());
            Document.State = RowStatus.NotEdited;
            if (Document != null)
                WindowName = Document.ToString();
            if (Document.Entity.DD_SPOST_DC != null)
            {
                var sf = UnitOfWork.Context.SD_26.First(_ => _.DOC_CODE == Document.Entity.DD_SPOST_DC);
                Schet = $"№{sf.SF_IN_NUM}/{sf.SF_POSTAV_NUM} от {sf.SF_POSTAV_DATE.ToShortDateString()}";
            }

            foreach (var r in Document.Rows)
            {
                if(r.TD_26 != null)
                    r.LinkInvoice = new InvoiceProviderRow(r.TD_26);
                r.State = RowStatus.NotEdited;

            }
            Document.State = RowStatus.NotEdited;
        }

        public override void SaveData(object data)
        {
            Document.Entity.DD_TYPE_DC = 2010000001;
            Document.Entity.DD_POLUCH_NAME = Document.Sender;
            var ent = UnitOfWork.Context.ChangeTracker.Entries().ToList();
            UnitOfWork.CreateTransaction();
            try
            {
                if (Document.State == RowStatus.NewRow || Document.DocCode < 0)
                {
                    int code = 1;
                    Document.DD_IN_NUM = UnitOfWork.Context.SD_24.Any() ? UnitOfWork.Context.SD_24.Max(_ => _.DD_IN_NUM) + 1 : 1;
                    Document.DocCode = UnitOfWork.Context.SD_24.Any() ? UnitOfWork.Context.SD_24.Max(_ => _.DOC_CODE) + 1 : 10240000001;
                    foreach (var row in Document.Rows)
                    {
                        row.DocCode = Document.DocCode;
                        row.Code = code;
                        code++;
                    }
                }
                UnitOfWork.Save();
                UnitOfWork.Commit();
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.InvoiceClient), null,
                    Document.DocCode, null, (string)Document.ToJson());
                RecalcKontragentBalans.CalcBalans(Document.DD_KONTR_OTPR_DC.Value, Document.Date);
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                WindowManager.ShowError(ex);
            }
            Document.DeletedRows.Clear();
            Document.myState = RowStatus.NotEdited;
            foreach (var r in Document.Rows)
            {
                r.myState = RowStatus.NotEdited;
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
                r.RaisePropertyChanged(nameof(r.Taksirovka));
                r.RaisePropertyChanged(nameof(r.Factur));
            }
        }

        public ICommand SenderSelectCommand
        {
            get { return new Command(SenderSelect, _ => Document != null && Document.Rows.Count == 0); }
        }

        public bool IsSenderTypeEnabled => Document.Sender == null;
        public bool IsCanChageKontragent => Document != null && Document.Rows.Count == 0;

        public void SenderSelect(object obj)
        {
            switch (Document?.WarehouseSenderType)
            {
                case WarehouseSenderType.Kontragent:
                    var kontr = StandartDialogs.SelectKontragent();
                    if (kontr == null) return;
                    Document.KontragentSender = kontr;
                    Document.Entity.DD_KONTR_OTPR_DC = kontr.DocCode;
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
            DocumentsOpenManager.Open(DocumentType.InvoiceProvider, (decimal)CurrentRow.DDT_SPOST_DC);
        }

        public ICommand AddFromDocumentCommand
        {
            get
            {
                return new Command(AddFromDocument, _ => CurrentRow != null && CurrentRow.LinkInvoice != null);
            }
        }

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.StoreOrderIn, null, Document.DocCode, null);
        }

        private void AddFromDocument(object obj)
        {
            var ctx = new AddNomenklFromInvoiceProviderRowViewModel(Document.WarehouseIn,
                CurrentRow.LinkInvoice.DocCode,
                new List<decimal>(Document.Rows.Select(_ => _.Nomenkl.DocCode)));
            var dlg = new SelectDialogView
            {
                DataContext = ctx,

            };
            ctx.Form = dlg;
            if (dlg.ShowDialog() == false) return;

            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            foreach (var r in ctx.Nomenkls.Where(_ => _.IsChecked))
            {

                var invRow = UnitOfWork.Context.TD_26
                    .Include(_ => _.SD_26)
                    .FirstOrDefault(_ => _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                var schetRow = invRow != null ? new InvoiceProviderRow(invRow) : null;
                var newEntity = new TD_24
                {
                    DOC_CODE = Document.DocCode,
                    CODE = newCode,
                    DDT_KOL_PRIHOD = r.Quantity,
                    DDT_SPOST_DC = r.DocCode,
                    DDT_SPOST_ROW_CODE = r.Code,
                    DDT_CRS_DC = ((IDocCode)r.Nomenkl.Currency).DocCode,
                    DocId = Document.Id,
                    Id = Guid.NewGuid()
                };
                Document.Entity.TD_24.Add(newEntity);
                var newItem = new WarehouseOrderInRow(newEntity)
                {
                    Nomenkl = r.Nomenkl,
                    Unit = (Unit)r.Nomenkl.Unit,
                    LinkInvoice = schetRow,
                    IsTaxExecuted = true,
                    IsFactExecuted = true,
                    State = RowStatus.NewRow
                };
                Document.Rows.Add(newItem);
            }
            RaisePropertyChanged(nameof(IsCanChageKontragent));
        }


        public ICommand AddNomenklCommand
        {
            get { return new Command(AddNomenkl, _ => true); }
        }

        private void AddNomenkl(object obj)
        {
            var WinManager = new WindowManager();
            if (Document.WarehouseIn == null)
            {
                WinManager.ShowWinUIMessageBox("Не выбран склад получатель", "Предупреждение", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            if (Document.KontragentSender == null)
            {
                WinManager.ShowWinUIMessageBox("Не выбран контрагент", "Предупреждение", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            if (Document.WarehouseSenderType == WarehouseSenderType.Kontragent)
            {
                var nomenkls = StandartDialogs.SelectNomenkls(null, true);
                if (nomenkls == null || nomenkls.Count <= 0) return;
                foreach (var n in nomenkls)
                    if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode))
                    {
                        var newEntity = new TD_24
                        {
                            DOC_CODE = Document.DocCode,
                            CODE = newCode,
                            DDT_KOL_PRIHOD = 1,
                            DDT_NOMENKL_DC = n.DocCode,
                            DDT_ED_IZM_DC = ((IDocCode)n.Unit).DocCode,
                            DDT_POST_ED_IZM_DC = ((IDocCode)n.Unit).DocCode,
                            DDT_CRS_DC = ((IDocCode)n.Currency).DocCode,
                            Id = Guid.NewGuid(),
                            DocId =  Document.Id
                        };
                        Document.Entity.TD_24.Add(newEntity);
                        var newItem = new WarehouseOrderInRow(newEntity)
                        {
                            State = RowStatus.NewRow
                        };
                        Document.Rows.Add(newItem);
                    }
            }
            else
            {
                var datarows = StandartDialogs.SelectNomenklsFromRashodOrder(Document.WarehouseIn);
                if (datarows == null || datarows.Count <= 0) return;
                foreach (var n in datarows)
                    if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.DDT_NOMENKL_DC))
                    {
                        var nom = GlobalOptions.ReferencesCache.GetNomenkl(n.DDT_NOMENKL_DC) as Nomenkl;
                        var newEnt = new TD_24
                        {
                            DDT_KOL_PRIHOD = n.DDT_KOL_RASHOD,
                            DDT_SKLAD_OTPR_DC = n.SD_24.DD_SKLAD_OTPR_DC,
                            DDT_RASH_ORD_DC = n.DOC_CODE,
                            DDT_RASH_ORD_CODE = n.Code,
                        };
                        Document.Entity.TD_24.Add(newEnt);
                        var newItem =  new WarehouseOrderInRow(newEnt)
                        {
                            DocCode = Document.DocCode,
                            Code = newCode,
                            Nomenkl = nom,
                            Unit = nom?.Unit as Unit,
                            // ReSharper disable once PossibleNullReferenceException
                            Currency = (Currency)nom.Currency,
                            State = RowStatus.NewRow
                        };
                        Document.Rows.Add(newItem);
                    }
            }

            RaisePropertyChanged(nameof(Document));
            RaisePropertyChanged(nameof(IsCanChageKontragent));
        }

        public ICommand DeleteNomenklCommand
        {
            get
            {
                return new Command(DeleteNomenkl, _ => CurrentRow != null);
            }
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
            RaisePropertyChanged(nameof(IsCanChageKontragent));
        }

        public override void DocDelete(object form)
        {
            //if (Document.State != RowStatus.NewRow)
            //    foreach (var r in Document.Rows)
            //    {
            //        if (r.LinkInvoice == null) continue;
            //        MessageBox.Show("В ордере есть строки привязанные к счету, удаление не возможно.", "Запрос",
            //            MessageBoxButton.OK,
            //            MessageBoxImage.Stop);
            //        return;
            //    }

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
                        if (doc != null)
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
                    Form?.Close();
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);
            if (Form is not OrderInView frm) return;
            foreach (var col in frm.gridRows.Columns)
            {
                if (col.FieldName is "DocDate" or "DocInNum" or "DocExtNum" or "Warehouse")
                    col.Visible = false;
                col.ReadOnly = col.FieldName is not (nameof(WarehouseOrderInRow.DDT_KOL_PRIHOD)
                    or nameof(WarehouseOrderInRow.SDRSchet));
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


        public ICommand UpdateCalcRowSummaCommand
        {
            get { return new Command(UpdateCalcRowSumma, _ => CurrentRow != null); }
        }

        private void UpdateCalcRowSumma(object obj)
        {
            if (Form is not OrderInView frm) return;
            if (obj is not CellValueChangedEventArgs p) return;
            if (p.Column.FieldName != "DDT_KOL_PRIHOD") return;
            if ((decimal)p.Value == 0)
            {
                if (frm.tableViewRows.ActiveEditor is PopupCalcEdit ed)
                {
                    ed.Value = 1;
                    frm.tableViewRows.PostEditor();
                    CurrentRow.DDT_KOL_PRIHOD = 1;
                }
            }
            var maxKol = Convert.ToDecimal(UnitOfWork.Context.TD_26.FirstOrDefault(_ => _.DOC_CODE == CurrentRow.LinkInvoice.DocCode
                                                                      && _.CODE == CurrentRow.LinkInvoice.Code)
                ?.SFT_KOL ?? 0);
            var d = UnitOfWork.Context.TD_24.Where(_ => _.DDT_SPOST_DC == CurrentRow.LinkInvoice.DocCode
                                                        && _.DDT_SPOST_ROW_CODE == CurrentRow.LinkInvoice.Code
                                                        && _.DOC_CODE != Document.DocCode).ToList();
            var prih = d.Count > 0 ? d.Sum(_ => _.DDT_KOL_PRIHOD) : 0;
            if (prih + (decimal)p.Value <= maxKol) return;

            if (frm.tableViewRows.ActiveEditor is PopupCalcEdit editor)
            {
                editor.Value = maxKol - prih;
                frm.tableViewRows.PostEditor();
                CurrentRow.DDT_KOL_PRIHOD = maxKol - prih;
            }
        }

        #endregion
    }
}
