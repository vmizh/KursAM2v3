﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.Managers.Nomenkl;
using KursAM2.ReportManagers.SFClientAndWayBill;
using KursAM2.View.DialogUserControl.Invoices.ViewModels;
using KursAM2.View.DialogUserControl.Standart;
using KursAM2.View.Helper;
using KursAM2.View.Logistiks.Warehouse;
using KursAM2.ViewModel.Management.Calculations;
using Reports.Base;

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
                        r.InvoiceClient = sf.FirstOrDefault(_ => r.DDT_SFACT_DC == _.DocCode);
                        r.myState = RowStatus.NotEdited;
                    }
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
        }

        #endregion

        #region Fields

        private readonly WindowManager winManager = new WindowManager();
        public readonly GenericKursDBRepository<SD_24> GenericRepository;
        
        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private WaybillRow myCurrentNomenklRow;
        private readonly NomenklManager2 nomenklManager;

        #endregion

        #region Properties

        public Waybill Document { set; get; }
        public ReportManager ReportManager { get; set; }

        public override string LayoutName => "WaybillWindowViewModel22";

        public override string WindowName =>
            Document == null
                ? "Расходная накладная(новая)"
                : $"Расходная накладня №{Document?.DD_IN_NUM} от {Document?.Date.ToShortDateString()} для {Document?.Client}";

        // ReSharper disable once MemberCanBePrivate.Global
        public ObservableCollection<string> ByWhomLicoList { set; get; } = new ObservableCollection<string>();

        public List<Core.EntityViewModel.NomenklManagement.Warehouse> Sklads =>
            MainReferences.Warehouses.Values.Where(_ => _.IsOutBalans != true && _.IsDeleted == false)
                .OrderBy(_ => _.Name).ToList();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<WaybillRow> SelectedRows { set; get; } = new ObservableCollection<WaybillRow>();

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
                                                               && (Document.State != RowStatus.NotEdited
                                                                   || Document.Rows.Any(_ =>
                                                                       _.State != RowStatus.NotEdited)
                                                                   || Document.DeletedRows.Count > 0);

        public override bool IsDocNewCopyAllow => Document.State != RowStatus.NewRow;
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
            get { return new Command(KontragentSelect, _ => Document.InvoiceClient == null); }
        }

        private void KontragentSelect(object obj)
        {
            var k = StandartDialogs.SelectKontragent();
            if (k != null)
                Document.Client = k;
        }

        public override void SaveData(object data)
        {
            try
            {
                if (Document.State == RowStatus.NewRow)
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
                DocumentsOpenManager.DeleteFromLastDocument(Document.Id, null);
                nomenklManager.RecalcPrice(UnitOfWork.Context);
                foreach (var n in Document.Rows.Select(_ => _.Nomenkl.DocCode))
                {
                    var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DOC_CODE, n,
                        Document.Date, Document.Date);
                    var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                    if (m < 0)
                    {
                        var nom = MainReferences.GetNomenkl(n);
                        WindowManager.ShowMessage($"По товару {nom.NomenklNumber} {nom.Name} " +
                                                  // ReSharper disable once PossibleInvalidOperationException
                                                  $"склад {Document.WarehouseOut} в кол-ве {q.First().OstatokQuantity} ",
                            "Отрицательные остатки", MessageBoxImage.Error);
                        UnitOfWork.Rollback();
                        return;
                    }
                }

                RecalcKontragentBalans.CalcBalans(Document.Client.DOC_CODE, Document.Date, UnitOfWork.Context);
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
                                r.InvoiceClient = sf.FirstOrDefault(_ => r.DDT_SFACT_DC == _.DocCode);
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
            ctx.Document = new Waybill(doc);
            ctx.Document.myState = RowStatus.NewRow;
            frm.DataContext = ctx;
            frm.Show();
        }

        public ICommand AddFromDocumentCommand
        {
            get { return new Command(AddFromDocument, _ => Document.InvoiceClient != null); }
        }

        private void AddFromDocument(object obj)
        {
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var r in Document.InvoiceClient.Rows)
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
                            var n = MainReferences.GetNomenkl(r.Entity.SFT_NEMENKL_DC);
                            var newItem = new WaybillRow
                            {
                                DocCode = Document.DocCode,
                                Code = newCode,
                                Id = Guid.NewGuid(),
                                DocId = Document.Id,
                                Nomenkl = n,
                                DDT_KOL_RASHOD = r.Quantity - kol,
                                Unit = n.Unit,
                                Currency = n.Currency,
                                SchetLinkedRow = r,
                                State = RowStatus.NewRow,
                                DDT_SFACT_DC = r.DocCode,
                                DDT_SFACT_ROW_CODE = r.Code
                            };
                            Document.Rows.Add(newItem);
                            Document.Entity.TD_24.Add(newItem.Entity);
                        }
                    }
                    else
                    {
                        var n = MainReferences.GetNomenkl(r.Entity.SFT_NEMENKL_DC);
                        var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DOC_CODE, n.DOC_CODE,
                            Document.Date, Document.Date);
                        var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                        if (m <= 0)
                        {
                            winManager.ShowWinUIMessageBox(
                                $"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
                                $"{MainReferences.Warehouses[Document.WarehouseOut.DocCode]}" +
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
                            Unit = n.Unit,
                            Currency = n.Currency,
                            SchetLinkedRow = r,
                            State = RowStatus.NewRow,
                            DDT_SFACT_DC = r.DocCode,
                            DDT_SFACT_ROW_CODE = r.Code
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
                var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DOC_CODE, n.DOC_CODE,
                    Document.Date, Document.Date);
                var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                if (m <= 0)
                {
                    winManager.ShowWinUIMessageBox($"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
                                                   $"{MainReferences.Warehouses[Document.WarehouseOut.DocCode]}" +
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
                        Unit = n.Unit,
                        Currency = n.Currency,
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

        public ICommand OpenSchetCommand
        {
            get { return new Command(OpenSchet, _ => Document.InvoiceClient != null); }
        }

        private void OpenSchet(object obj)
        {
            // ReSharper disable once PossibleInvalidOperationException
            DocumentsOpenManager.Open(DocumentType.InvoiceClient, Document.InvoiceClient.DocCode);
        }

        public ICommand DeleteSchetCommand
        {
            get { return new Command(DeleteSchet, _ => Document.InvoiceClient != null); }
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
            Document.InvoiceClient = null;
            Document.RaisePropertyChanged("State");
        }

        public ICommand SelectSchetCommand
        {
            get { return new Command(SelectSchet, _ => Document.WarehouseOut != null); }
        }

        private void addFromOneSchet(InvoiceClient inv)
        {
            var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
            foreach (var r in inv.Rows)
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
                        var n = MainReferences.GetNomenkl(r.Entity.SFT_NEMENKL_DC);
                        var newItem = new WaybillRow
                        {
                            DocCode = Document.DocCode,
                            Code = newCode,
                            Nomenkl = n,
                            DDT_KOL_RASHOD = r.Quantity - kol,
                            Unit = n.Unit,
                            Currency = n.Currency,
                            SchetLinkedRow = r,
                            State = RowStatus.NewRow,
                            DDT_SFACT_DC = r.DocCode,
                            DDT_SFACT_ROW_CODE = r.Code
                        };
                        Document.Rows.Add(newItem);
                        Document.Entity.TD_24.Add(newItem.Entity);
                    }
                }
                else
                {
                    var n = MainReferences.GetNomenkl(r.Entity.SFT_NEMENKL_DC);
                    var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DOC_CODE, n.DOC_CODE,
                        Document.Date, Document.Date);
                    var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                    if (m <= 0)
                    {
                        winManager.ShowWinUIMessageBox(
                            $"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
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
                        DDT_KOL_RASHOD = r.Quantity,
                        Unit = n.Unit,
                        Currency = n.Currency,
                        SchetLinkedRow = r,
                        State = RowStatus.NewRow,
                        DDT_SFACT_DC = r.DocCode,
                        DDT_SFACT_ROW_CODE = r.Code
                    };
                    Document.Rows.Add(newItem);
                    Document.Entity.TD_24.Add(newItem.Entity);
                }
            }
        }

        public void SelectSchet(object obj)
        {
            InvoiceClientSearchType loadType = InvoiceClientSearchType.NotShipped;
            loadType |= InvoiceClientSearchType.OnlyAccepted;
            if (Document.Client != null) loadType |= InvoiceClientSearchType.OneKontragent;
            var dtx = new InvoiceClientSearchDialogViewModel(true, true, loadType, UnitOfWork.Context)
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
            dialog.ShowDialog();
            if (dtx.DialogResult == MessageResult.OK)
            {
                if (Document.Client == null && dtx.SelectedItems.Count > 0)
                {
                    Document.InvoiceClient = InvoicesManager.GetInvoiceClient(dtx.SelectedItems.First().DocCode);
                    Document.Client = Document.InvoiceClient.Client;
                    addFromOneSchet(Document.InvoiceClient);
                }

                if (Document.Client != null && dtx.SelectedItems.Count > 0)
                {
                    Document.InvoiceClient = InvoicesManager.GetInvoiceClient(dtx.SelectedItems.First().DocCode);
                    Document.Client = Document.InvoiceClient.Client;
                    var newCode = Document.Rows.Count > 0 ? Document.Rows.Max(_ => _.Code) + 1 : 1;
                    foreach (var item in dtx.SelectedItems)
                    {
                        var n = MainReferences.GetNomenkl(item.NomenklDC);
                        var q = nomenklManager.GetNomenklQuantity(Document.WarehouseOut.DOC_CODE, n.DocCode,
                            Document.Date, Document.Date);
                        var m = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                        if (m <= 0)
                        {
                            winManager.ShowWinUIMessageBox(
                                $"Остатки номенклатуры {n.NomenklNumber} {n.Name} на складе " +
                                $"{MainReferences.Warehouses[Document.WarehouseOut.DocCode]}" +
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
                                DDT_KOL_RASHOD = (item.Quantity ?? 0) <= m ? item.Quantity ?? 0 : m,
                                Unit = n.Unit,
                                Currency = n.Currency,
                                InvoiceClient = Document.InvoiceClient,
                                State = RowStatus.NewRow,
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

        private List<InvoiceClient> LoadSFInfo()
        {
            var sfDCs = Document.Rows.Select(_ => _.DDT_SFACT_DC).Distinct();
            var res = new List<InvoiceClient>();
            foreach (var dc in sfDCs)
            {
                var d = UnitOfWork.Context.SD_84.SingleOrDefault(_ => _.DOC_CODE == dc);
                if(d != null)
                    res.Add(new InvoiceClient(d, UnitOfWork));
            }

            return res;
        }

        #endregion
    }
}