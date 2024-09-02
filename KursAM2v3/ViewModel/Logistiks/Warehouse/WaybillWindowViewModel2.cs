using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using DevExpress.Xpf.Editors.Settings;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.ReportManagers.SFClientAndWayBill;
using KursAM2.View.DialogUserControl.Invoices.ViewModels;
using KursAM2.View.DialogUserControl.Standart;
using KursAM2.View.Helper;
using KursAM2.View.Logistiks.Warehouse;
using KursAM2.ViewModel.Management.Calculations;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Invoices;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository;
using Reports.Base;
using static KursAM2.View.DialogUserControl.Invoices.ViewModels.InvoiceClientSearchDialogViewModel;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
    public class WaybillWindowViewModel2 : RSWindowViewModelBase
    {
        #region Constructors

        public WaybillWindowViewModel2(decimal? docDC)
        {
            GenericRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            nomenklManager = new NomenklManager2(UnitOfWork.Context);
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

            if (docDC != null)
            {
                var doc = GenericRepository.GetById(docDC);
                Document = doc != null
                    ? new Waybill(doc)
                    {
                        State = RowStatus.NotEdited
                    }
                    : new Waybill(null)
                    {
                        State = RowStatus.NewRow
                    };
                var sf = LoadSFInfo();
                if (Document.State == RowStatus.NotEdited)
                    foreach (var r in Document.Rows)
                    {
                        r.InvoiceClientViewModel = sf.FirstOrDefault(_ => r.DDT_SFACT_DC == _.DocCode);
                        r.myState = RowStatus.NotEdited;
                    }

                LastDocumentManager.SaveLastOpenInfo(DocumentType.Waybill, null, Document.DocCode,
                    Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
            }
            else
            {
                Document = new Waybill(null)
                {
                    DD_IN_NUM = -1,
                    Date = DateTime.Today,
                    CREATOR = GlobalOptions.UserInfo.NickName,
                    Id = Guid.NewGuid(),
                    State = RowStatus.NewRow
                };
                UnitOfWork.Context.SD_24.Add(Document.Entity);
            }

            DocCurrencyVisible = Document.Client != null ? Visibility.Visible : Visibility.Hidden;
        }

        public override void UpdateVisualObjects()
        {
            base.UpdateVisualObjects();
            if (Form is WayBillView2 frm)
                foreach (var col in frm.gridRows.Columns)
                    switch (col.FieldName)
                    {
                        case "DDT_KOL_RASHOD":


                            if (col.EditSettings == null || col.EditSettings.GetType() != typeof(CalcEditSettings))
                                col.EditSettings = new CalcEditSettings();
                            ((CalcEditSettings)col.EditSettings).DisplayFormat =
                                GlobalOptions.SystemProfile.GetQuantityValueNumberFormat();
                            break;
                    }
        }

        #endregion

        #region Fields

        private readonly WindowManager winManager = new WindowManager();
        public readonly GenericKursDBRepository<SD_24> GenericRepository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private WaybillRow myCurrentNomenklRow;
        private readonly NomenklManager2 nomenklManager;
        private Waybill _document;
        private ReportManager _reportManager;
        private Visibility _docCurrencyVisible;
        private ObservableCollection<string> _byWhomLicoList = new ObservableCollection<string>();
        private ObservableCollection<WaybillRow> _selectedRows = new ObservableCollection<WaybillRow>();

        #endregion

        #region Properties

        public Waybill Document
        {
            set
            {
                if (Equals(value, _document)) return;
                _document = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(WindowName));
                RaisePropertyChanged(nameof(IsDocDeleteAllow));
                RaisePropertyChanged(nameof(IsCanRefresh));
                RaisePropertyChanged(nameof(IsCanSaveData));
                RaisePropertyChanged(nameof(IsDocNewCopyAllow));
                RaisePropertyChanged(nameof(IsDocNewCopyRequisiteAllow));
                RaisePropertyChanged(nameof(KontragentSelectCommand));
                RaisePropertyChanged(nameof(AddFromDocumentCommand));
                RaisePropertyChanged(nameof(AddNomenklCommand));
                RaisePropertyChanged(nameof(OpenMainSchetCommand));
                RaisePropertyChanged(nameof(DeleteSchetCommand));
                RaisePropertyChanged(nameof(SelectSchetCommand));
            }
            get => _document;
        }

        public ReportManager ReportManager
        {
            get => _reportManager;
            set
            {
                if (Equals(value, _reportManager)) return;
                _reportManager = value;
                RaisePropertyChanged();
            }
        }

        public Visibility DocCurrencyVisible
        {
            get => _docCurrencyVisible;
            set
            {
                if (value == _docCurrencyVisible) return;
                _docCurrencyVisible = value;
                RaisePropertyChanged();
            }
        }

        public override string LayoutName => "WaybillWindowViewModel22";

        public override string WindowName =>
            Document == null
                ? "Расходная накладная(новая)"
                : $"Расходная накладня №{Document?.DD_IN_NUM} от {Document?.Date.ToShortDateString()} для {Document?.Client}";

        // ReSharper disable once MemberCanBePrivate.Global
        public ObservableCollection<string> ByWhomLicoList
        {
            set
            {
                if (Equals(value, _byWhomLicoList)) return;
                _byWhomLicoList = value;
                RaisePropertyChanged();
            }
            get => _byWhomLicoList;
        }

        public List<KursDomain.References.Warehouse> Sklads =>
            GlobalOptions.ReferencesCache.GetWarehousesAll().Where(_ => _.IsOutBalans != true && _.IsDeleted == false)
                .Cast<KursDomain.References.Warehouse>().OrderBy(_ => _.Name).ToList();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<WaybillRow> SelectedRows
        {
            set
            {
                if (Equals(value, _selectedRows)) return;
                _selectedRows = value;
                RaisePropertyChanged();
            }
            get => _selectedRows;
        }

        public WaybillRow CurrentNomenklRow
        {
            get => myCurrentNomenklRow;
            set
            {
                if (myCurrentNomenklRow == value) return;
                myCurrentNomenklRow = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NewRow;

        public override bool IsCanSaveData => Document != null && Document.WarehouseOut != null
                                                               && Document.Client != null
                                                               && (Document.State != RowStatus.NotEdited
                                                                   || Document.Rows.Any(_ =>
                                                                       _.State != RowStatus.NotEdited)
                                                                   || Document.DeletedRows.Count > 0);

        public override bool IsDocNewCopyAllow => false;

        public override bool IsDocNewCopyRequisiteAllow => Document.State != RowStatus.NewRow;
        public override bool IsDocNewEmptyAllow => true;

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            if (Document.State != RowStatus.NewRow)
                DocumentHistoryManager.LoadHistory(DocumentType.Waybill, null, Document.DocCode, null);
        }

        public ICommand KontragentSelectCommand
        {
            get { return new Command(KontragentSelect, _ => Document.InvoiceClientViewModel == null); }
        }

        private void KontragentSelect(object obj)
        {
            SelectSchet(null);
            DocCurrencyVisible = Document.Client != null ? Visibility.Visible : Visibility.Hidden;
            Document.RaisePropertyChanged("DocCurrency");
        }

        public override void SaveData(object data)
        {
            var WinManager = new WindowManager();
            var isOldExist = false;
            var isCreateNum = true;
            using (var ctx = GlobalOptions.GetEntities())
            {
                isOldExist = ctx.SD_24.Any(_ => _.DOC_CODE == Document.DocCode);
            }

            if (!isOldExist && Document.State != RowStatus.NewRow)
            {
                var res = WinManager.ShowWinUIMessageBox("Документ уже удален! Сохранить заново?", "Предупреждение",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.No:
                        Form?.Close();
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }

                Document.State = RowStatus.NewRow;
                UnitOfWork.Context.Entry(Document.Entity).State = EntityState.Added;
                foreach (var r in Document.Entity.TD_24) UnitOfWork.Context.Entry(r).State = EntityState.Added;
                isCreateNum = false;
            }

            try
            {
                if (Document.State == RowStatus.NewRow)
                    if (isCreateNum)
                    {
                        Document.DD_IN_NUM = UnitOfWork.Context.SD_24.Any(_ => _.DD_TYPE_DC == 2010000012)
                            ? UnitOfWork.Context.SD_24.Where(_ => _.DD_TYPE_DC == 2010000012).Max(_ => _.DD_IN_NUM) + 1
                            : 1;
                        Document.DocCode = UnitOfWork.Context.SD_24.Any()
                            ? UnitOfWork.Context.SD_24.Max(_ => _.DOC_CODE) + 1
                            : 1;
                        Document.Id = Guid.NewGuid();
                        var code = 1;
                        foreach (var r in Document.Rows)
                        {
                            r.DOC_CODE = Document.DocCode;
                            r.Code = code;
                            r.Id = Guid.NewGuid();
                            r.DocId = Document.Id;
                            code++;
                        }
                    }

                Document.Entity.DD_OTRPAV_NAME = Document.Sender;
                Document.Entity.DD_POLUCH_NAME = Document.Receiver;
                UnitOfWork.CreateTransaction();
                UnitOfWork.Save();
                LastDocumentManager.SaveLastOpenInfo(DocumentType.Waybill, null, Document.DocCode,
                    Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
                nomenklManager.RecalcPrice(UnitOfWork.Context);
                foreach (var n in Document.Rows.Select(_ => _.Nomenkl.DocCode))
                {
                    var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DocCode, n,
                        Document.Date, Document.Date);
                    var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                    if (m < 0)
                    {
                        var nom = GlobalOptions.ReferencesCache.GetNomenkl(n) as Nomenkl;
                        WindowManager.ShowMessage($"По товару {nom.NomenklNumber} {nom.Name} " +
                                                  // ReSharper disable once PossibleInvalidOperationException
                                                  $"склад {Document.WarehouseOut} в кол-ве {q.First().OstatokQuantity} ",
                            "Отрицательные остатки", MessageBoxImage.Error);
                        UnitOfWork.Rollback();
                        return;
                    }
                }

                RecalcKontragentBalans.CalcBalans(Document.Client.DocCode, Document.Date, UnitOfWork.Context);
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.AccruedAmountOfSupplier),
                    Document.Id,
                    0, null, (string)Document.ToJson());
                UnitOfWork.Commit();
                foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
                Document.DeletedRows.Clear();
                Document.myState = RowStatus.NotEdited;
                Document.RaisePropertyChanged("State");
                ParentFormViewModel?.RefreshData(null);
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                WindowManager.ShowError(ex);
            }
        }

        public override void RefreshData(object obj)
        {
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("В документ внесены изменения. Сохранить?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        Document.DeletedRows.Clear();
                        foreach (var entity in UnitOfWork.Context.ChangeTracker.Entries()) entity.Reload();
                        Document.myState = RowStatus.NotEdited;
                        var sf = LoadSFInfo();
                        if (Document.State == RowStatus.NotEdited)
                            foreach (var r in Document.Rows)
                            {
                                r.InvoiceClientViewModel = sf.FirstOrDefault(_ => r.DDT_SFACT_DC == _.DocCode);
                                r.myState = RowStatus.NotEdited;
                            }

                        Document.RaisePropertyAllChanged();
                        break;
                }
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
                    try
                    {
                        UnitOfWork.CreateTransaction();
                        UnitOfWork.Context.SD_24.Remove(Document.Entity);
                        UnitOfWork.Save();
                        UnitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        UnitOfWork.Rollback();
                        WindowManager.ShowError(ex);
                        return;
                    }

                    CloseWindow(Form);
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new WayBillView2 { Owner = Application.Current.MainWindow };
            var ctx = new WaybillWindowViewModel2(null) { Form = frm };
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopy(object obj)
        {
            if (Document == null) return;
            var frm = new WayBillView2 { Owner = Application.Current.MainWindow };
            var ctx = new WaybillWindowViewModel2(null)
            {
                Form = frm
            };
            ctx.UnitOfWork.Context.Entry(ctx.Document.Entity).State = EntityState.Detached;
            var doc = ctx.GenericRepository.GetById(Document.DocCode);
            ctx.Document = new Waybill(doc);
            ctx.UnitOfWork.Context.Entry(doc).State = EntityState.Added;
            foreach (var r in ctx.Document.Rows)
            {
                ctx.UnitOfWork.Context.Entry(r.Entity).State = EntityState.Added;
                r.myState = RowStatus.NewRow;
            }

            ctx.Document.DocCode = -1;
            ctx.Document.Date = DateTime.Today;
            ctx.Document.DD_EXT_NUM = null;
            ctx.Document.DD_IN_NUM = -1;
            ctx.Document.CREATOR = GlobalOptions.UserInfo.NickName;
            ctx.Document.myState = RowStatus.NewRow;
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (Document == null) return;
            var frm = new WayBillView2 { Owner = Application.Current.MainWindow };
            var ctx = new WaybillWindowViewModel2(null)
            {
                Form = frm
            };
            ctx.UnitOfWork.Context.Entry(ctx.Document.Entity).State = EntityState.Detached;
            var doc = ctx.GenericRepository.GetById(Document.DocCode);
            ctx.UnitOfWork.Context.Entry(doc).State = EntityState.Added;
            doc.TD_24.Clear();
            ctx.Document = new Waybill(doc)
            {
                DocCode = -1,
                Date = DateTime.Today,
                DD_EXT_NUM = null,
                DD_IN_NUM = -1,
                CREATOR = GlobalOptions.UserInfo.NickName,
                myState = RowStatus.NewRow,
                InvoiceClientViewModel = null
            };
            frm.DataContext = ctx;
            ctx.DocCurrencyVisible = Visibility.Visible;
            frm.Show();
        }

        public ICommand AddFromDocumentCommand
        {
            get { return new Command(AddFromDocument, _ => Document.InvoiceClientViewModel != null); }
        }

        private void AddFromDocument(object obj)
        {
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var r in Document.InvoiceClientViewModel.Rows.Cast<InvoiceClientRowViewModel>())
                {
                    var oldf = Document.Rows.FirstOrDefault(_ => _.DDT_NOMENKL_DC == r.Entity.SFT_NEMENKL_DC);
                    if (oldf != null)
                    {
                        if (oldf.DDT_SFACT_DC == r.DocCode && oldf.DDT_SFACT_ROW_CODE == r.Code)
                            oldf.DDT_KOL_RASHOD = r.Quantity;
                        continue;
                    }

                    var otgr = ctx.TD_24.Where(_ => _.DDT_SFACT_DC == r.DocCode
                                                    && _.DDT_SFACT_ROW_CODE == r.Code);
                    if (otgr.Any())
                    {
                        var kol = otgr.Sum(_ => _.DDT_KOL_RASHOD);
                        if (kol < r.Quantity)
                        {
                            var n = GlobalOptions.ReferencesCache.GetNomenkl(r.Entity.SFT_NEMENKL_DC) as Nomenkl;
                            var newItem = new WaybillRow
                            {
                                DocCode = Document.DocCode,
                                Code = newCode,
                                Id = Guid.NewGuid(),
                                DocId = Document.Id,
                                Nomenkl = n,
                                DDT_KOL_RASHOD = r.Quantity - kol,
                                Unit = n.Unit as Unit,
                                Currency = n.Currency as Currency,
                                SchetLinkedRowViewModel = r,
                                State = RowStatus.NewRow,
                                DDT_SFACT_DC = r.DocCode,
                                DDT_SFACT_ROW_CODE = r.Code,
                                InvoiceClientViewModel = Document.InvoiceClientViewModel
                            };
                            Document.Rows.Add(newItem);
                            Document.Entity.TD_24.Add(newItem.Entity);
                        }
                    }
                    else
                    {
                        var n = GlobalOptions.ReferencesCache.GetNomenkl(r.Entity.SFT_NEMENKL_DC) as Nomenkl;
                        var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DocCode, n.DocCode,
                            Document.Date, Document.Date);
                        var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                        if (m <= 0)
                        {
                            winManager.ShowWinUIMessageBox(
                                $"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
                                $"{GlobalOptions.ReferencesCache.GetWarehouse(Document.WarehouseOut.DocCode)}" +
                                $"кол-во {m}. Операция по номенклатуре не может быть проведена.",
                                "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        var newItem = new WaybillRow
                        {
                            DocCode = Document.DocCode,
                            Code = newCode,
                            Id = Guid.NewGuid(),
                            DocId = Document.Id,
                            Nomenkl = n,
                            DDT_KOL_RASHOD = r.Quantity,
                            Unit = n.Unit as Unit,
                            Currency = n.Currency as Currency,
                            SchetLinkedRowViewModel = r,
                            State = RowStatus.NewRow,
                            DDT_SFACT_DC = r.DocCode,
                            DDT_SFACT_ROW_CODE = r.Code,
                            InvoiceClientViewModel = Document.InvoiceClientViewModel
                        };
                        Document.Rows.Add(newItem);
                        Document.Entity.TD_24.Add(newItem.Entity);
                    }
                }
            }
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
                var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DocCode, n.DocCode,
                    Document.Date, Document.Date);
                var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                if (m <= 0)
                {
                    winManager.ShowWinUIMessageBox($"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
                                                   $"{GlobalOptions.ReferencesCache.GetWarehouse(Document.WarehouseOut.DocCode)}" +
                                                   $"кол-во {m}. Операция по номенклатуре не может быть проведена.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode))
                {
                    var newItem = new WaybillRow
                    {
                        DocCode = -1,
                        Code = newCode,
                        Id = Guid.NewGuid(),
                        DocId = Document.Id,
                        Nomenkl = n,
                        DDT_KOL_PRIHOD = 1,
                        Unit = n.Unit as Unit,
                        Currency = n.Currency as Currency,
                        State = RowStatus.NewRow
                    };
                    Document.Rows.Add(newItem);
                    Document.Entity.TD_24.Add(newItem.Entity);
                }
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
                    Document.Entity.TD_24.Remove(row.Entity);
                }
                else
                {
                    Document.DeletedRows.Add(row);
                    Document.Rows.Remove(row);
                    Document.Entity.TD_24.Remove(row.Entity);
                }
        }
        //OpenMainSchetCommand

        public ICommand OpenMainSchetCommand
        {
            get { return new Command(OpenMainSchet, _ => Document.InvoiceClientViewModel != null); }
        }

        private void OpenMainSchet(object obj)
        {
            // ReSharper disable once PossibleInvalidOperationException
            DocumentsOpenManager.Open(DocumentType.InvoiceClient, Document.InvoiceClientViewModel.DocCode);
        }

        public ICommand OpenSchetCommand
        {
            get { return new Command(OpenSchet, _ => CurrentNomenklRow?.SchetLinkedRowViewModel != null); }
        }

        private void OpenSchet(object obj)
        {
            // ReSharper disable once PossibleInvalidOperationException
            DocumentsOpenManager.Open(DocumentType.InvoiceClient, CurrentNomenklRow.SchetLinkedRowViewModel.DocCode);
        }

        public ICommand DeleteSchetCommand
        {
            get { return new Command(DeleteSchet, _ => Document.InvoiceClientViewModel != null); }
        }

        private void DeleteSchet(object obj)
        {
            if (winManager.ShowWinUIMessageBox("Вы хотите удалить счет и связанные с ним строки?",
                    "Запрос", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
            var delList =
                new List<WaybillRow>(Document.Rows.Where(_ => _.DDT_SFACT_DC == Document.Entity.DD_SFACT_DC));
            foreach (var r in delList)
            {
                if (r.State != RowStatus.NewRow) Document.DeletedRows.Add(r);
                Document.Rows.Remove(r);
                Document.Entity.TD_24.Remove(r.Entity);
            }

            Document.DD_SCHET = null;
            Document.InvoiceClientViewModel = null;
            Document.RaisePropertyChanged("State");
        }

        public ICommand SelectSchetCommand
        {
            get { return new Command(SelectSchet, _ => Document.WarehouseOut != null); }
        }

        private void addFromOneSchet(InvoiceClientViewModel inv)
        {
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            foreach (var r in inv.Rows.Cast<InvoiceClientRowViewModel>())
            {
                var oldf = Document.Rows.FirstOrDefault(_ => _.DDT_NOMENKL_DC == r.Entity.SFT_NEMENKL_DC);
                if (oldf != null)
                {
                    if (oldf.DDT_SFACT_DC == r.DocCode && oldf.DDT_SFACT_ROW_CODE == r.Code)
                        oldf.DDT_KOL_RASHOD = r.Quantity;
                    continue;
                }

                var otgr = UnitOfWork.Context.TD_24.Where(_ => _.DDT_SFACT_DC == r.DocCode
                                                               && _.DDT_SFACT_ROW_CODE == r.Code);
                if (otgr.Any())
                {
                    var kol = otgr.Sum(_ => _.DDT_KOL_RASHOD);
                    if (kol < r.Quantity)
                    {
                        var n = GlobalOptions.ReferencesCache.GetNomenkl(r.Entity.SFT_NEMENKL_DC) as Nomenkl;
                        var newItem = new WaybillRow
                        {
                            DocCode = Document.DocCode,
                            Code = newCode,
                            Nomenkl = n,
                            DDT_KOL_RASHOD = r.Quantity - kol,
                            Unit = n.Unit as Unit,
                            Currency = n.Currency as Currency,
                            SchetLinkedRowViewModel = r,
                            State = RowStatus.NewRow,
                            DDT_SFACT_DC = r.DocCode,
                            DDT_SFACT_ROW_CODE = r.Code,
                            InvoiceClientViewModel = Document.InvoiceClientViewModel
                        };
                        Document.Rows.Add(newItem);
                        Document.Entity.TD_24.Add(newItem.Entity);
                    }
                }
                else
                {
                    var n = GlobalOptions.ReferencesCache.GetNomenkl(r.Entity.SFT_NEMENKL_DC) as Nomenkl;
                    var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DocCode, n.DocCode,
                        Document.Date, Document.Date);
                    var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                    if (m <= 0)
                    {
                        winManager.ShowWinUIMessageBox(
                            $"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
                            $"{GlobalOptions.ReferencesCache.GetWarehouse(Document.WarehouseOut.DocCode)}" +
                            $"кол-во {m}. Операция по номенклатуре не может быть проведена.",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    var newItem = new WaybillRow
                    {
                        DocCode = Document.DocCode,
                        Code = newCode,
                        Nomenkl = n,
                        DDT_KOL_RASHOD = r.Quantity,
                        Unit = n.Unit as Unit,
                        Currency = n.Currency as Currency,
                        SchetLinkedRowViewModel = r,
                        State = RowStatus.NewRow,
                        DDT_SFACT_DC = r.DocCode,
                        DDT_SFACT_ROW_CODE = r.Code,
                        InvoiceClientViewModel = Document.InvoiceClientViewModel
                    };
                    Document.Rows.Add(newItem);
                    Document.Entity.TD_24.Add(newItem.Entity);
                }
            }
        }

        public void SelectSchet(object obj)
        {
            var loadType = InvoiceClientSearchType.NotShipped;
            loadType |= InvoiceClientSearchType.OnlyAccepted;
            if (Document.Client != null) loadType |= InvoiceClientSearchType.OneKontragent;
            var mode = Document.Client == null ? OperatingMode.SelectKontragent : OperatingMode.AllKontragent;
            var multiMode = Document.Client == null ? false : true;
            //ToDo отслеживание вставленных позиций в диалоге выбора строк ***
            var dtx = new InvoiceClientSearchDialogViewModel(true, multiMode, loadType, mode,
                Document.Rows.Select(_ => _.DDT_NOMENKL_DC).ToList(), UnitOfWork.Context)
            {
                WindowName = "Выбор счетов фактур",
                KontragentDC = Document.Client?.DocCode
            };
            dtx.RefreshData(null);
            var dialog = new RSDialogView
            {
                DataContext = dtx,
                Owner = Application.Current.MainWindow
            };
            dtx.Form = dialog;
            dialog.ShowDialog();
            if (dtx.DialogResult == MessageResult.OK)
            {
                if (Document.Client == null && dtx.SelectedItems.Count > 0)
                {
                    Document.InvoiceClientViewModel =
                        InvoicesManager.GetInvoiceClient(dtx.SelectedItems.First().DocCode);
                    Document.Client = Document.InvoiceClientViewModel.Client;
                    addFromOneSchet(Document.InvoiceClientViewModel);
                }

                if (Document.Client != null && dtx.SelectedItems.Count > 0)
                {
                    Document.InvoiceClientViewModel =
                        InvoicesManager.GetInvoiceClient(dtx.SelectedItems.First().DocCode);
                    Document.Client = Document.InvoiceClientViewModel.Client;
                    var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
                    foreach (var item in dtx.SelectedItems)
                    {
                        var n = GlobalOptions.ReferencesCache.GetNomenkl(item.NomenklDC) as Nomenkl;
                        var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DocCode, n.DocCode,
                            Document.Date, Document.Date);
                        var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                        if (m <= 0)
                        {
                            winManager.ShowWinUIMessageBox(
                                $"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
                                $"{GlobalOptions.ReferencesCache.GetWarehouse(Document.WarehouseOut.DocCode)}" +
                                $"кол-во {m}. Операция по номенклатуре не может быть проведена.",
                                "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        if (Document.Rows.All(_ => _.Nomenkl.DocCode != n.DocCode))
                        {
                            var newItem = new WaybillRow
                            {
                                DocCode = Document.DocCode,
                                Code = newCode,
                                Id = Guid.NewGuid(),
                                DocId = Document.Id,
                                DDT_SFACT_DC = item.DocCode,
                                DDT_SFACT_ROW_CODE = item.RowCode,
                                Nomenkl = n,
                                DDT_KOL_RASHOD = (item.Quantity - item.Shipped ?? 0) <= m
                                    ? item.Quantity - item.Shipped ?? 0
                                    : m,
                                Unit = n.Unit as Unit,
                                Currency = n.Currency as Currency,
                                InvoiceClientViewModel = Document.InvoiceClientViewModel,
                                State = RowStatus.NewRow,
                                SchetLinkedRowViewModel =
                                    Document.InvoiceClientViewModel.Rows.FirstOrDefault(_ => _.Code == item.RowCode) as
                                        InvoiceClientRowViewModel
                            };
                            Document.Rows.Add(newItem);
                            Document.Entity.TD_24.Add(newItem.Entity);
                        }

                        newCode++;
                    }
                }
            }
        }

        public Command ExportCommand
        {
            get { return new Command(ExportWayBill, _ => true); }
        }

        public void ExportWayBill(object obj)
        {
            ReportManager.Reports["Экспорт"].Show();
        }

        public Command PrintWaybillCommand
        {
            get { return new Command(PrintWaybill, _ => true); }
        }

        public void PrintWaybill(object obj)
        {
            ReportManager.Reports["Торг12"].Show();
        }

        #endregion

        #region Methods

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

        private List<InvoiceClientViewModel> LoadSFInfo()
        {
            var sfDCs = Document.Rows.Select(_ => _.DDT_SFACT_DC).Distinct();
            var res = new List<InvoiceClientViewModel>();
            foreach (var dc in sfDCs)
            {
                var d = UnitOfWork.Context.SD_84.SingleOrDefault(_ => _.DOC_CODE == dc);
                if (d != null)
                    res.Add(new InvoiceClientViewModel(d, UnitOfWork, true));
            }

            return res;
        }

        #endregion
    }
}
