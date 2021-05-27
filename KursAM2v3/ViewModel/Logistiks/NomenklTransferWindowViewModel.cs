using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Managers;
using KursAM2.Managers.Nomenkl;
using KursAM2.View.Base;
using KursAM2.ViewModel.Finance.Invoices;
using KursAM2.ViewModel.Reference;

namespace KursAM2.ViewModel.Logistiks
{
    public sealed class NomenklTransferWindowViewModel : RSWindowViewModelBase
    {
        public readonly List<NomenklTransferRowViewModelExt> DeletedRows = new List<NomenklTransferRowViewModelExt>();
        private readonly DbContext myDBContext = GlobalOptions.GetEntities();
        private NomenklTransferRowViewModelExt myCurrentRow;
        private Core.EntityViewModel.Warehouse myCurrentWarehouse;

        // public bool IsCannotChangeStore => Document.State == RowStatus.NewRow || Document.SchetFacturaBase == null;
        private bool myIsCanChangeStore;
        private bool myIsCannotChangeStore;
        private string mySchetFactura;

        private NomenklTransferWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            LoadReference();
        }

        public NomenklTransferWindowViewModel(Guid? id) : this()
        {
            Document = id == null
                ? NomenklTransferViewModelExt.New()
                : NomenklTransferViewModelExt.Load((Guid) id, myDBContext);
            if (Document.Warehouse != null)
                Document.Warehouse = StoreCollection.SingleOrDefault(_ => _.DocCode == Document.Warehouse.DocCode);
            Document.State = RowStatus.NotEdited;
            foreach (var r in Document.Rows)
            {
                r.Parent = Document;
                r.State = RowStatus.NotEdited;
            }

            RaisePropertyChanged(nameof(Document));
            RaisePropertiesChanged(nameof(IsCannotChangeStore));
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<ProviderRowWithNakladViewModel> ProviderRows { set; get; } =
            new ObservableCollection<ProviderRowWithNakladViewModel>();

        public string SchetFactura
        {
            get => mySchetFactura;
            set
            {
                if (mySchetFactura == value) return;
                mySchetFactura = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsDocDeleteAllow => true;
        public NomenklTransferViewModelExt Document { set; get; }

        public ObservableCollection<Core.EntityViewModel.Warehouse> StoreCollection { set; get; } =
            new ObservableCollection<Core.EntityViewModel.Warehouse>();

        //private IDialogService DialogService => GetService<IDialogService>();
        //private IMessageBoxService MessageBoxService => GetService<IMessageBoxService>();
        public ICommand AddForNotShippedCommand
        {
            get { return new Command(AddForNotShipped, _ => true); }
        }

        public bool IsCannotChangeStore
        {
            get => myIsCannotChangeStore;
            set
            {
                if (myIsCannotChangeStore == value) return;
                myIsCannotChangeStore = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCanChangeStore
        {
            get => myIsCanChangeStore;
            set
            {
                if (myIsCanChangeStore == value) return;
                myIsCanChangeStore = value;
                RaisePropertyChanged();
            }
        }

        public ICommand SchetFacteraRowSelectCommad
        {
            get { return new Command(SchetFacteraRowSelect, _ => Document.SchetFacturaBase != null); }
        }

        public ICommand SchetFacturaSelectCommad
        {
            get { return new Command(SchetFacturaSelect, _ => Document.Warehouse == null); }
        }

        public ICommand AddForStoreCommand
        {
            get { return new Command(AddForStore, param => Document.Warehouse != null); }
        }

        public ICommand UpdateStartPriceCommand
        {
            get { return new Command(UpdateStartPrice, _ => true); }
        }

        public ICommand SetNomenklInCommand
        {
            get { return new Command(SetNomenklIn, _ => true); }
        }

        public NomenklTransferRowViewModelExt CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow != null && myCurrentRow.Equals(value)) return;
                myCurrentRow = value;
                if (myCurrentRow != null)
                    LoadInvoiceInfo(myCurrentRow.NomenklOutDC, Document.Date);
                RaisePropertyChanged();
            }
        }

        public bool IsRowReadOnly => CurrentRow != null && CurrentRow.IsAccepted;

        //public bool IsCannotChangeStore => Document.;
        public Core.EntityViewModel.Warehouse CurrentWarehouse
        {
            get => myCurrentWarehouse;
            set
            {
                if (myCurrentWarehouse != null && myCurrentWarehouse.Equals(value)) return;
                myCurrentWarehouse = value;
                Document.Warehouse = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsCanSaveData
            =>
                Document.State != RowStatus.NotEdited || Document.Rows.Any(_ => _.State != RowStatus.NotEdited) ||
                DeletedRows.Count > 0 && Document.Rows.All(_ => _.NomenklNumberIn != null);

        public ICommand DeleteRowsCommand
        {
            get { return new Command(DeleteRow, _ => CurrentRow != null); }
        }

        public ICommand SelectStoreCommand
        {
            get { return new Command(SelectStore, _ => Document?.SchetFacturaBase == null); }
        }

        public ICommand SelectStoreRowCommand
        {
            get { return new Command(SelectStoreRow, _ => Document?.Warehouse == null); }
        }

        private void UpdateStartPrice(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var d in Document.Rows.Where(_ => _.IsAccepted == false))
                    {
                        var date = ctx.NOM_PRICE.Where(_ => _.NOM_DC == d.NomenklOutDC && _.DATE <= Document.Date)
                            .Max(_ => _.DATE);
                        var prc = ctx.NOM_PRICE.FirstOrDefault(_ => _.NOM_DC == d.NomenklOutDC && _.DATE == date);
                        if (prc == null) continue;
                        d.PriceOut = prc.PRICE_WO_NAKLAD;
                        d.NakladEdSumma = prc.PRICE - prc.PRICE_WO_NAKLAD;
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void LoadInvoiceInfo(decimal nomDC, DateTime date)
        {
            ProviderRows.Clear();
            var d = Nomenkl.GetLastDateNomenklZero(nomDC, date);
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data =
                    ctx.TD_24.Include(_ => _.SD_24)
                        .Include(_ => _.TD_26)
                        .Include(_ => _.TD_26.SD_26)
                        .Where(
                            _ =>
                                _.DDT_NOMENKL_DC == nomDC && _.SD_24.DD_DATE >= d && _.SD_24.DD_DATE <= date &&
                                (_.SD_24.DD_TYPE_DC == 2010000001 || _.SD_24.DD_TYPE_DC == 2010000005))
                        .ToList();
                foreach (var dd in data)
                    if (dd.TD_26 != null)
                        ProviderRows.Add(new ProviderRowWithNakladViewModel
                        {
                            TypePrihodDocument = "Приходный складской ордер",
                            OrderInfo =
                                $"№{dd.SD_24.DD_IN_NUM}  \"/\"  {dd.SD_24.DD_EXT_NUM} от {dd.SD_24.DD_DATE:d} поставщик {dd.SD_24.DD_OTRPAV_NAME}",
                            InvoiceInfo =
                                $"№{dd.TD_26.SD_26.SF_IN_NUM}  \"/\" {dd.TD_26.SD_26.SF_POSTAV_NUM} от {dd.TD_26.SD_26.SF_POSTAV_DATE:d}",
                            Quantity = dd.DDT_KOL_PRIHOD,
                            // ReSharper disable once PossibleInvalidOperationException
                            Price = Math.Round((decimal) dd.TD_26.SFT_ED_CENA),
                            PriceWithNaklad =
                                Math.Round(
                                    (decimal) dd.TD_26.SFT_ED_CENA +
                                    (dd.TD_26.SFT_SUMMA_NAKLAD != null && dd.TD_26.SFT_KOL != 0
                                        ? dd.TD_26.SFT_SUMMA_NAKLAD.Value / dd.TD_26.SFT_KOL
                                        : 0), 2),
                            // ReSharper disable once PossibleInvalidOperationException
                            Currency = MainReferences.Currencies[(decimal) dd.TD_26.SD_26.SF_CRS_DC].Name,
                            UnitNaklad =
                                Math.Round(dd.TD_26.SFT_SUMMA_NAKLAD != null && dd.TD_26.SFT_KOL != 0
                                    ? dd.TD_26.SFT_SUMMA_NAKLAD.Value / dd.TD_26.SFT_KOL
                                    : 0, 2),
                            SummaIn = Math.Round((decimal) dd.TD_26.SFT_ED_CENA * dd.DDT_KOL_PRIHOD, 2),
                            SummaNaklad =
                                Math.Round((dd.TD_26.SFT_SUMMA_NAKLAD != null && dd.TD_26.SFT_KOL != 0
                                               ? dd.TD_26.SFT_SUMMA_NAKLAD.Value / dd.TD_26.SFT_KOL
                                               : 0) * dd.DDT_KOL_PRIHOD, 2),
                            SummaWithNaklad =
                                Math.Round((decimal) dd.TD_26.SFT_ED_CENA * dd.DDT_KOL_PRIHOD +
                                           (dd.TD_26.SFT_SUMMA_NAKLAD != null && dd.TD_26.SFT_KOL != 0
                                               ? dd.TD_26.SFT_SUMMA_NAKLAD.Value / dd.TD_26.SFT_KOL
                                               : 0) * dd.DDT_KOL_PRIHOD, 2),
                            Date = dd.SD_24.DD_DATE,
                            Note = dd.TD_26.SFT_TEXT + " " + dd.TD_26.SD_26.SF_NOTES
                        });
                    else
                        ProviderRows.Add(new ProviderRowWithNakladViewModel
                        {
                            TypePrihodDocument = "Инвентаризационная ведомость",
                            OrderInfo =
                                $"№{dd.SD_24.DD_IN_NUM}",
                            InvoiceInfo = null,
                            Quantity = Math.Round(dd.DDT_KOL_PRIHOD, 2),
                            // ReSharper disable once PossibleInvalidOperationException
                            Price = (decimal) dd.DDT_TAX_CRS_CENA,
                            PriceWithNaklad = Math.Round((decimal) dd.DDT_TAX_CRS_CENA, 2),
                            Currency =
                                MainReferences.GetNomenkl(nomDC).Currency.Name,
                            UnitNaklad = 0,
                            SummaIn = Math.Round((decimal) dd.DDT_TAX_CRS_CENA * dd.DDT_KOL_PRIHOD, 2),
                            SummaNaklad = 0,
                            SummaWithNaklad = Math.Round((decimal) dd.DDT_TAX_CRS_CENA * dd.DDT_KOL_PRIHOD, 2),
                            Date = dd.SD_24.DD_DATE
                        });
                var transf =
                    ctx.NomenklTransferRow.Include(_ => _.NomenklTransfer)
                        .Where(
                            _ =>
                                _.NomenklTransfer.Date >= d && _.NomenklTransfer.Date <= date &&
                                _.NomenklInDC == nomDC)
                        .ToList();
                foreach (var dd in transf)
                    ProviderRows.Add(new ProviderRowWithNakladViewModel
                    {
                        TypePrihodDocument = "Валютная таксировка товара",
                        OrderInfo =
                            $"№{dd.NomenklTransfer.DucNum}  от {dd.NomenklTransfer.Date:d}",
                        InvoiceInfo = null,
                        Quantity = Math.Round(dd.Quantity, 3),
                        Price = Math.Round(dd.PriceIn, 2),
                        PriceWithNaklad = Math.Round(dd.PriceIn + (dd.NakladEdSumma ?? 0), 2),
                        Currency =
                            MainReferences.Currencies[MainReferences.GetNomenkl(dd.NomenklOutDC).Currency.DocCode].Name,
                        UnitNaklad = Math.Round(dd.NakladNewEdSumma ?? 0, 2),
                        SummaIn = Math.Round(dd.PriceIn * dd.Quantity, 2),
                        SummaNaklad = Math.Round((dd.NakladNewEdSumma ?? 0) * dd.Quantity, 2),
                        SummaWithNaklad = Math.Round((dd.PriceIn + (dd.NakladNewEdSumma ?? 0)) * dd.Quantity, 2),
                        Date = dd.NomenklTransfer.Date,
                        Note = dd.Note
                    });
            }
        }

        private void AddForNotShipped(object obj)
        {
            WindowManager.ShowFunctionNotReleased();
            //var ctxTransf = new NomenklAddFromNotShippedWindowViewModel();
            //var dlg = new SelectDialogView { DataContext = ctxTransf };
            //dlg.ShowDialog();
            //if (!ctxTransf.DialogResult) return;
            //var ret =
            //    ctxTransf.Rows.Where(_ => _.IsAccepted).Select(row => new NomenklTransferRowViewModelExt
            //    {
            //        Id = Guid.NewGuid(),
            //        //DocId = Document.Id,
            //        NomenklOut = row.Nomenkl,
            //        MaxQuantity = row.Quantity,
            //        Quantity = row.Quantity,
            //        State = RowStatus.NewRow,
            //        PriceOut = row.Price,
            //        SummaOut = row.Quantity * row.Price
            //        //Parent = Document
            //    }).ToList();
            //return ret;
        }

        private void SchetFacteraRowSelect(object obj)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        private void SchetFacturaSelect(object obj)
        {
            var ctxTransf = new SchetSupplierSelectViewModel("Выбор счета поставщика");
            var dlg = new SelectDialogView {DataContext = ctxTransf};
            dlg.ShowDialog();
            if (!ctxTransf.DialogResult) return;
            var data = ctxTransf.CurrentInvoice;
            if (data != null)
            {
                Document.SchetFacturaBase = ctxTransf.CurrentInvoice.Entity;
                SchetFactura = data.InvoiceInfo;
            }

            if (Document.SchetFacturaBase != null)
                IsCannotChangeStore = true;
            RaisePropertiesChanged(nameof(IsCannotChangeStore));
        }

        private void SelectStore(object obj)
        {
            var ctxTransf = new SimpleObjectsSelectWindowViewModel("Выбор склада");
            var dlg = new SelectDialogView {DataContext = ctxTransf};
            dlg.ShowDialog();
            if (!ctxTransf.DialogResult) return;
            if (ctxTransf.CurrentObject is Core.EntityViewModel.Warehouse skl)
                Document.Warehouse = skl;
        }

        private void SelectStoreRow(object obj)
        {
            var ctxTransf = new SimpleObjectsSelectWindowViewModel("Выбор склада");
            var dlg = new SelectDialogView {DataContext = ctxTransf};
            dlg.ShowDialog();
            if (!ctxTransf.DialogResult) return;
            if (ctxTransf.CurrentObject is Core.EntityViewModel.Warehouse skl)
                CurrentRow.Warehouse = skl;
        }

        public override void RefreshData(object obj)
        {
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        return;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            Refresh();
        }

        public override void CloseWindow(object form)
        {
            var vin = form as Window;
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("Были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (res == MessageBoxResult.Cancel) return;
                if (res == MessageBoxResult.No)
                    vin?.Close();
                if (res == MessageBoxResult.Yes)
                    try
                    {
                        SaveData(null);
                        vin?.Close();
                    }
                    catch (Exception ex)
                    {
                        WinManager.ShowWinUIMessageBox(ex.Message, "Ошибка");
                    }
            }
            else
            {
                vin?.Close();
            }
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Документ будет удален, Вы уверены?", "Запрос",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;
            if (Document.State == RowStatus.NewRow || res == MessageBoxResult.Cancel)
            {
                var vin = form as Window;
                vin?.Close();
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    using (var tn = ctx.Database.BeginTransaction())
                    {
                        try
                        {
                            var d = ctx.NomenklTransfer.FirstOrDefault(_ => _.Id == Document.Id);
                            if (d == null) return;
                            {
                                foreach (var r in ctx.NomenklTransferRow.Where(_ => _.DocId == Document.Id))
                                    ctx.NomenklTransferRow.Remove(r);
                                ctx.NomenklTransfer.Remove(d);
                            }
                            ctx.SaveChanges();
                            tn.Commit();
                            var vin = form as Window;
                            vin?.Close();
                        }
                        catch (Exception ex)
                        {
                            tn.Rollback();
                            WindowManager.ShowError(null, ex);
                        }
                    }
                }
            }
        }

        private void Refresh()
        {
            Document = NomenklTransferViewModelExt.Load(Document.Id, myDBContext);
            if (Document.Warehouse != null)
                Document.Warehouse = StoreCollection.SingleOrDefault(_ => _.DocCode == Document.Warehouse.DocCode);
            Document.State = RowStatus.NotEdited;
            RaisePropertyChanged(nameof(Document));
        }

        private void SetNomenklIn(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ctxNom = new NomenklAddNomenklForInViewModel(CurrentRow.NomenklOut.MainId);
                    if (ctxNom.IsNomenklMainNull)
                    {
                        WinManager.ShowMessageBox("Не найдена основная номеклатура", "Сообщение");
                        return;
                    }

                    var dlg = new SelectDialogView {DataContext = ctxNom};
                    dlg.ShowDialog();
                    if (!ctxNom.DialogResult) return;
                    CurrentRow.NomenklIn = ctxNom.CurrentNomenkl;
                    var firstDate = Document.Date.AddDays(-10);
                    var rates =
                        ctx.CURRENCY_RATES_CB.Where(
                                _ => _.RATE_DATE >= firstDate && _.RATE_DATE <= Document.Date)
                            .ToList();
                    var dt = rates.Select(_ => _.RATE_DATE).Distinct().ToList();
                    rates.AddRange(dt.Select(r => new CURRENCY_RATES_CB
                    {
                        CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                        NOMINAL = 1,
                        RATE = 1,
                        RATE_DATE = r
                    }));
                    var crsRateOut =
                        rates.SingleOrDefault(
                                _ => _.CRS_DC == CurrentRow.CurrencyOut?.DocCode && _.RATE_DATE == Document.Date)
                            ?.RATE ??
                        -1;
                    var crsRateIn =
                        rates.SingleOrDefault(
                                _ => _.CRS_DC == CurrentRow.CurrencyIn?.DocCode && _.RATE_DATE == Document.Date)
                            ?.RATE ??
                        -1;
                    if (crsRateIn != 0)
                        CurrentRow.Rate = crsRateOut / crsRateIn;
                    else
                        CurrentRow.Rate = -1;
                    RaisePropertyChanged(nameof(Document));
                    RaisePropertyChanged(nameof(CurrentRow));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        private void LoadReference()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var c in ctx.SD_27)
                        StoreCollection.Add(new Core.EntityViewModel.Warehouse(c));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void SaveData(object data)
        {
            try
            {
                foreach (var r in Document.Rows)
                {
                    r.LastUpdate = DateTime.Now;
                    r.LastUpdater = GlobalOptions.UserInfo.NickName;
                }

                Document.LastUpdate = DateTime.Now;
                Document.LastUpdater = GlobalOptions.UserInfo.NickName;
                Document.Save();
                var calc = new NomenklCostMediumSlidingOnServer();
                calc.Calc(null);
                Refresh();
                RaisePropertyChanged(nameof(Document));
                DocumentsOpenManager.SaveLastOpenInfo(DocumentType.NomenklTransfer, Document.Id, Document.DocCode, Document.Creator,
                    "", Document.Description);
            }
            catch (Exception ex)
            {
                WindowManager.ShowDBError(ex);
            }
        }

        private void DeleteRow(object obj)
        {
            if (CurrentRow == null) return;
            if (CurrentRow.State == RowStatus.NewRow)
            {
                Document.Rows.Remove(CurrentRow);
            }
            else
            {
                CurrentRow.State = RowStatus.Deleted;
                DeletedRows.Add(CurrentRow);
                Document.Rows.Remove(CurrentRow);
            }
        }

        private void AddForStore(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var res = NomenklManager.SelectNomenklsWithRemainsDialog(Document.Warehouse, Document.Date);
                    if (res == null) return;
                    foreach (var row in res)
                    {
                        row.DocId = Document.Id;
                        row.Parent = Document;
                        Document.Rows.Add(new NomenklTransferRowViewModelExt
                        {
                            Id = Guid.NewGuid(),
                            NomenklOut = row.NomenklOut,
                            MaxQuantity = row.Quantity,
                            Quantity = row.Quantity,
                            State = RowStatus.NewRow,
                            PriceOut = row.PriceOut,
                            SummaOut = row.Quantity * row.PriceOut,
                            NakladEdSumma = row.NakladEdSumma,
                            NakladRate = row.NakladRate
                        });
                    }

                    var firstDate = Document.Date.AddDays(-10);
                    var rates =
                        ctx.CURRENCY_RATES_CB.Where(
                                _ => _.RATE_DATE >= firstDate && _.RATE_DATE <= Document.Date)
                            .ToList();
                    var dt = rates.Select(_ => _.RATE_DATE).Distinct().ToList();
                    rates.AddRange(dt.Select(r => new CURRENCY_RATES_CB
                    {
                        CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                        NOMINAL = 1,
                        RATE = 1,
                        RATE_DATE = r
                    }));
                    foreach (var nr in Document.Rows.Where(_ => _.NomenklIn == null))
                    {
                        var d =
                            ctx.SD_83.Where(
                                _ => _.MainId == nr.NomenklOut.MainId && _.DOC_CODE != nr.NomenklOut.DocCode);
                        if (!d.Any())
                        {
                            nr.NomenklIn = nr.NomenklOut;
                            nr.Rate = 1;
                            continue;
                        }

                        if (d.Count() != 1) continue;
                        nr.NomenklIn = MainReferences.GetNomenkl(d.First().DOC_CODE);
                        var crsRateOut =
                            rates.SingleOrDefault(
                                    _ => _.CRS_DC == nr.CurrencyOut?.DocCode && _.RATE_DATE == Document.Date)
                                ?.RATE ??
                            -1;
                        var crsRateIn =
                            rates.SingleOrDefault(
                                    _ => _.CRS_DC == nr.CurrencyIn?.DocCode && _.RATE_DATE == Document.Date)
                                ?.RATE ??
                            -1;
                        if (crsRateIn != 0)
                        {
                            nr.Rate = crsRateOut / crsRateIn;
                            nr.NakladRate = crsRateOut / crsRateIn;
                        }
                        else
                        {
                            nr.Rate = -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }
    }
}