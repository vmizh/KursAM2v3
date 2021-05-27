using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.EntityViewModel;
using Core.Finance;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using DevExpress.Data;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Managers;
using KursAM2.View.Base;
using KursAM2.View.Management;
using KursAM2.ViewModel.Logistiks;
using KursAM2.ViewModel.Management.Calculations;
using KursAM2.ViewModel.Personal;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace KursAM2.ViewModel.Management.ManagementBalans
{
    public class ManagementBalansWindowViewModel : RSWindowViewModelBase
    {
        private readonly List<SD_114> myBanks;
        private readonly List<SD_22> myCashs;
        private ManagementBalansBuilder myBalansBuilder;
        private object myCompareDate;
        private ManagementBalanceGroupViewModel myCurrentBalansRow;
        private CrossCurrencyRate myCurrentCurrencyRate;
        private DateTime myCurrentDate = DateTime.Today;
        private ManagementBalanceExtendRowViewModel myCurrentExtendItem;
        private Currency myRecalcCurrency;

        public ManagementBalansWindowViewModel()
        {
            BalansStructure = new ObservableCollection<ManagementBalanceGroupViewModel>();
            ExtendRows = new ObservableCollection<ManagementBalanceExtendRowViewModel>();
            ExtendRowsActual = new ObservableCollection<ManagementBalanceExtendRowViewModel>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = GetRightMenu();
            using (var ent = GlobalOptions.GetEntities())
            {
                myBanks = ent.SD_114.ToList();
                myCashs = ent.SD_22.ToList();
            }

            CurrenciesForRecalc.Add(MainReferences.Currencies[CurrencyCode.RUB]);
            CurrenciesForRecalc.Add(MainReferences.Currencies[CurrencyCode.USD]);
            CurrenciesForRecalc.Add(MainReferences.Currencies[CurrencyCode.EUR]);
            CurrenciesForRecalc.Add(MainReferences.Currencies[CurrencyCode.GBP]);
            CurrenciesForRecalc.Add(MainReferences.Currencies[CurrencyCode.CNY]);
            var crsrate = new CrossCurrencyRate();
            crsrate.SetRates(DateTime.Today);
            foreach (var c in crsrate.CurrencyList)
                CurrencyRates.Add(c);
            CurrentCurrencyRate = CurrencyRates[0];
        }

        public ObservableCollection<ColumnSummary> SummaryList { get; } =
            new ObservableCollection<ColumnSummary>();

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public List<Currency> CurrenciesForRecalc { set; get; } = new List<Currency>();

        public ObservableCollection<CrossCurrencyRate> CurrencyRates { set; get; } =
            new ObservableCollection<CrossCurrencyRate>();

        public CrossCurrencyRate CurrentCurrencyRate
        {
            get => myCurrentCurrencyRate;
            set
            {
                if (myCurrentCurrencyRate != null && myCurrentCurrencyRate.Equals(value)) return;
                myCurrentCurrencyRate = value;
                RaisePropertyChanged();
            }
        }

        public Currency RecalcCurrency
        {
            get => myRecalcCurrency;
            set
            {
                if (Equals(myRecalcCurrency, value)) return;
                myRecalcCurrency = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(RecalcCrsName));
            }
        }

        public string RecalcCrsName => $"Пересчет в {RecalcCurrency?.Name}";
        public ICommand SummaryCommand => new Command(CalcTotalSummaries, param => true);

        public ICommand KontragentAccountOpenCommand
        {
            get { return new Command(KontragentAccountOpen, _ => CurrentExtendItem?.Kontragent != null); }
        }

        public ICommand NomenklCalcOpenCommand
        {
            get { return new Command(NomenklCalcOpen, _ => CurrentExtendItem?.Nom != null); }
        }

        public ManagementBalanceExtendRowViewModel CurrentExtendItem
        {
            get => myCurrentExtendItem;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentExtendItem == value) return;
                myCurrentExtendItem = value;
                RaisePropertyChanged();
            }
        }

        public ManagementBalanceGroupViewModel CurrentBalansRow
        {
            get => myCurrentBalansRow;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentBalansRow == value) return;
                myCurrentBalansRow = value;
                ExtendRowsActual.Clear();
                if (myCurrentBalansRow != null)
                    foreach (var d in ExtendRows.Where(_ => _.GroupId == CurrentBalansRow.Id))
                        ExtendRowsActual.Add(d);
                if (ExtendRowsActual.Count > 0 && RecalcCurrency != null && CurrentCurrencyRate != null)
                {
                    foreach (var b in ExtendRowsActual)
                    {
                        b.PriceCalc = decimal.Round(b.PriceEUR *
                                                    CurrentCurrencyRate.GetRate(
                                                        MainReferences.Currencies[CurrencyCode.EUR],
                                                        RecalcCurrency) +
                                                    b.PriceUSD *
                                                    CurrentCurrencyRate.GetRate(
                                                        MainReferences.Currencies[CurrencyCode.USD],
                                                        RecalcCurrency) +
                                                    b.PriceGBP *
                                                    CurrentCurrencyRate.GetRate(
                                                        MainReferences.Currencies[CurrencyCode.GBP],
                                                        RecalcCurrency) +
                                                    b.PriceRUB *
                                                    CurrentCurrencyRate.GetRate(
                                                        MainReferences.Currencies[CurrencyCode.RUB],
                                                        RecalcCurrency) +
                                                    b.PriceCNY *
                                                    CurrentCurrencyRate.GetRate(
                                                        MainReferences.Currencies[CurrencyCode.CNY],
                                                        RecalcCurrency), 2);
                        b.SummaCalc = decimal.Round(b.SummaEUR *
                                                    CurrentCurrencyRate.GetRate(
                                                        MainReferences.Currencies[CurrencyCode.EUR],
                                                        RecalcCurrency) +
                                                    b.SummaUSD *
                                                    CurrentCurrencyRate.GetRate(
                                                        MainReferences.Currencies[CurrencyCode.USD],
                                                        RecalcCurrency) +
                                                    b.SummaGBP *
                                                    CurrentCurrencyRate.GetRate(
                                                        MainReferences.Currencies[CurrencyCode.GBP],
                                                        RecalcCurrency) +
                                                    b.SummaCNY *
                                                    CurrentCurrencyRate.GetRate(
                                                        MainReferences.Currencies[CurrencyCode.CNY],
                                                        RecalcCurrency) +
                                                    b.SummaRUB *
                                                    CurrentCurrencyRate.GetRate(
                                                        MainReferences.Currencies[CurrencyCode.RUB],
                                                        RecalcCurrency), 2);
                    }

                    RaisePropertyChanged(nameof(RecalcCrsName));
                }

                var frm = (ManagementBalansView) Form;
                foreach (var col in frm.gridExtend.Columns)
                {
                    GridControlBand b;
                    switch (col.FieldName)
                    {
                        case "SummaEUR":
                            b =
                                frm.gridExtend.Bands.FirstOrDefault(_ => _.Columns.Any(c => c.FieldName == "SummaEUR"));
                            if (b != null)
                                b.Visible = ExtendRowsActual.Sum(_ => _.SummaEUR) != 0;
                            break;
                        case "SummaUSD":
                            b =
                                frm.gridExtend.Bands.FirstOrDefault(_ => _.Columns.Any(c => c.FieldName == "SummaUSD"));
                            if (b != null)
                                b.Visible = ExtendRowsActual.Sum(_ => _.SummaUSD) != 0;
                            break;
                        case "SummaRUB":
                            b =
                                frm.gridExtend.Bands.FirstOrDefault(_ => _.Columns.Any(c => c.FieldName == "SummaRUB"));
                            if (b != null)
                                b.Visible = ExtendRowsActual.Sum(_ => _.SummaRUB) != 0;
                            break;
                        case "SummaGBP":
                            b =
                                frm.gridExtend.Bands.FirstOrDefault(_ => _.Columns.Any(c => c.FieldName == "SummaGBP"));
                            if (b != null)
                                b.Visible = ExtendRowsActual.Sum(_ => _.SummaGBP) != 0;
                            break;
                        case "SummaCHF":
                            b =
                                frm.gridExtend.Bands.FirstOrDefault(_ => _.Columns.Any(c => c.FieldName == "SummaCHF"));
                            if (b != null)
                                b.Visible = ExtendRowsActual.Sum(_ => _.SummaCHF) != 0;
                            break;
                        case "SummaSEK":
                            b =
                                frm.gridExtend.Bands.FirstOrDefault(_ => _.Columns.Any(c => c.FieldName == "SummaSEK"));
                            if (b != null)
                                b.Visible = ExtendRowsActual.Sum(_ => _.SummaSEK) != 0;
                            break;
                        case "SummaCNY":
                            b =
                                frm.gridExtend.Bands.FirstOrDefault(_ => _.Columns.Any(c => c.FieldName == "SummaCNY"));
                            if (b != null)
                                b.Visible = ExtendRowsActual.Sum(_ => _.SummaCNY) != 0;
                            break;
                    }
                }

                GenerateSummaries();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ExtendRowsActual));
            }
        }

        public DateTime CurrentDate
        {
            set
            {
                if (Equals(value, myCurrentDate)) return;
                myCurrentDate = value;
                RaisePropertyChanged();
            }
            get => myCurrentDate;
        }

        public DateTime? CompareDate
        {
            set
            {
                if (Equals(value, myCompareDate)) return;
                myCompareDate = value;
                RaisePropertyChanged();
            }
            get => myCurrentDate;
        }

        public ObservableCollection<ManagementBalanceGroupViewModel> BalansStructure { set; get; }
        public ObservableCollection<ManagementBalanceExtendRowViewModel> ExtendRows { get; }
        public ObservableCollection<ManagementBalanceExtendRowViewModel> ExtendRowsActual { get; }

        public ICommand BalansCrossRateRecalcCommand
        {
            get
            {
                return new Command(BalansCrossRateRecalc, _ => RecalcCurrency != null && CurrentCurrencyRate != null);
            }
        }

        public override bool IsDocumentOpenAllow => CurrentExtendItem != null;

        private void KontragentAccountOpen(object obj)
        {
            var ctxk = new KontragentBalansWindowViewModel(CurrentExtendItem.Kontragent.DOC_CODE);
            var frm = new KontragentBalansForm {Owner = Application.Current.MainWindow, DataContext = ctxk};
            frm.Show();
        }

        private void NomenklCalcOpen(object obj)
        {
            if (CurrentExtendItem?.Nom.DocCode == null) return;
            var ctx = new NomPriceWindowViewModel((decimal) CurrentExtendItem?.Nom.DocCode);
            var dlg = new SelectDialogView {DataContext = ctx};
            dlg.ShowDialog();
        }

        private void GenerateSummaries()
        {
            SummaryList.Clear();
            SummaryList.Add(
                new ColumnSummary {Type = SummaryItemType.Count, FieldName = "Name", DisplayFormat = "{n0}"});
            if (ExtendRowsActual == null || ExtendRowsActual.Count == 0) return;
            foreach (var crs in ExtendRowsActual.Select(_ => _.CurrencyName).Distinct())
                SummaryList.Add(new ColumnSummary
                {
                    Type = SummaryItemType.Custom,
                    Key = crs,
                    FieldName = "Summa",
                    DisplayFormat = crs + " {0,20:n2}",
                    Summa = 0
                });
            SummaryList.Add(new ColumnSummary
            {
                Type = SummaryItemType.Custom,
                Key = "SummaCalc",
                FieldName = "Summa",
                DisplayFormat = "{0,20:n2}",
                Summa = 0
            });
            var frm = (ManagementBalansView) Form;
            foreach (var b in frm.gridExtend.Bands)
                switch (b.Name)
                {
                    case "LossEUR":
                        b.Visible = ExtendRowsActual.Sum(_ => _.SummaEUR) != 0;
                        break;
                    case "LossUSD":
                        b.Visible = ExtendRowsActual.Sum(_ => _.SummaUSD) != 0;
                        break;
                    case "LossRUB":
                        b.Visible = ExtendRowsActual.Sum(_ => _.SummaRUB) != 0;
                        break;
                    case "LossGBP":
                        b.Visible = ExtendRowsActual.Sum(_ => _.SummaGBP) != 0;
                        break;
                    case "LossCHF":
                        b.Visible = ExtendRowsActual.Sum(_ => _.SummaCHF) != 0;
                        break;
                    case "LossSEK":
                        b.Visible = ExtendRowsActual.Sum(_ => _.SummaSEK) != 0;
                        break;
                    case "LossCNY":
                        b.Visible = ExtendRowsActual.Sum(_ => _.SummaCNY) != 0;
                        break;
                }
        }

        private void CalcTotalSummaries(object obj)
        {
            try
            {
                if (!(obj is CustomSummaryEventArgs e)) return;
                if (ExtendRowsActual == null || ExtendRowsActual.Count == 0)
                {
                    SummaryList.Clear();
                    return;
                }

                if (((GridSummaryItem) e.Item).FieldName == "Summa")
                    if (e.IsTotalSummary)
                    {
                        if (e.SummaryProcess == CustomSummaryProcess.Start)
                            SummaryList.Single(_ => _.Key == (string) ((GridSummaryItem) e.Item).Tag).Summa = 0;
                        if (e.SummaryProcess == CustomSummaryProcess.Calculate)
                        {
                            var val = (decimal) e.FieldValue;
                            if (!(e.Row is ManagementBalanceExtendRowViewModel row)) return;
                            if (row.CurrencyName == (string) ((GridSummaryItem) e.Item).Tag)
                            {
                                SummaryList.Single(_ => _.Key == (string) ((GridSummaryItem) e.Item).Tag).Summa += val;
                                e.TotalValue =
                                    SummaryList.Single(_ => _.Key == (string) ((GridSummaryItem) e.Item).Tag).Summa;
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            RaisePropertyChanged(nameof(SummaryList));
        }

        private ObservableCollection<MenuButtonInfo> GetRightMenu()
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuSave"] as ControlTemplate,
                    ToolTip = "Сохранить изменения",
                    Command = SaveDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = CloseWindowCommand
                }
            };
        }

        public override void RefreshData(object obj)
        {
            myBalansBuilder = new ManagementBalansBuilder();
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

            ExtendRows.Clear();
            ExtendRowsActual.Clear();
            BalansStructure.Clear();
            foreach (var r in myBalansBuilder.Structure)
                BalansStructure.Add(r);
            try
            {
                GetCash();
                GetBank();
                GetKontragent();
                GetNomenkl();
                GetZarplata();
                GetMoneyInPath();
                var ch = BalansStructure.Single(_ => _.Tag == BalansSection.Head);
                //ch.Summa =
                //    BalansStructure.Where(_ => _.ParentId == Guid.Parse("{9DC33178-1DAA-4A65-88BD-E1AD617B12D9}"))
                //        .Sum(_ => _.Summa);
                ch.SummaEUR =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaEUR);
                ch.SummaUSD =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaUSD);
                ch.SummaRUB =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaRUB);
                ch.SummaGBP =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaGBP);
                ch.SummaCHF =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaCHF);
                ch.SummaSEK =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaSEK);
                ch.SummaCNY =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaCNY);
                var frm = (ManagementBalansView) Form;
                if (frm != null)
                {
                    var frm1 = frm.ManagementBalansMainUI;
                    //if(frm1)
                    foreach (var col in frm1.treeListBalans.Columns)
                        switch (col.FieldName)
                        {
                            case "SummaEUR":
                                col.Visible = ch.SummaEUR != 0;
                                break;
                            case "SummaUSD":
                                col.Visible = ch.SummaUSD != 0;
                                break;
                            case "SummaRUB":
                                col.Visible = ch.SummaRUB != 0;
                                break;
                            case "SummaGBP":
                                col.Visible = ch.SummaGBP != 0;
                                break;
                            case "SummaCHF":
                                col.Visible = ch.SummaCHF != 0;
                                break;
                            case "SummaSEK":
                                col.Visible = ch.SummaSEK != 0;
                                break;
                            case "SummaCNY":
                                col.Visible = ch.SummaCNY != 0;
                                break;
                        }

                    if (CurrentCurrencyRate != null && RecalcCurrency != null)
                        BalansCrossRateRecalc(null);
                }

                RaisePropertyChanged();
                RaisePropertiesChanged("BalansStructure");
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        /// <summary>
        ///     деньги в пути
        /// </summary>
        public void GetMoneyInPath()
        {
            var ch = BalansStructure.Single(_ => _.Id == myBalansBuilder.Structure
                .Single(t => t.Tag == BalansSection.MoneyInPath)
                .Id);
            ch.Summa = 0;
            ch.SummaRUB = 0;
            ch.SummaEUR = 0;
            ch.SummaUSD = 0;
            ch.SummaCNY = 0;
            using (var ent = GlobalOptions.GetEntities())
            {
                var CashOut = ent.SD_34.Where(_ =>
                    (_.CASH_TO_DC != null || _.BANK_RASCH_SCHET_DC != null) && _.DATE_ORD <= CurrentDate).ToList();
                var CashIn = ent.SD_33.Where(_ =>
                        (_.RASH_ORDER_FROM_DC != null || _.BANK_RASCH_SCHET_DC != null) && _.DATE_ORD <= CurrentDate)
                    .ToList();
                var BankOut = ent.TD_101.Include(_ => _.SD_101).Where(_ =>
                        _.SD_101.VV_STOP_DATE <= CurrentDate && _.BankAccountDC != null &&
                        _.VVT_KASS_PRIH_ORDER_DC == null)
                    .ToList();
                //var BankIn = ent.TD_101.Include(_ => _.SD_101).Where(_ => _.SD_101.VV_STOP_DATE <= CurrentDate &&
                //                                                          (_.BankFromTransactionCode == null ||
                //                                                           _.VVT_RASH_KASS_ORDER_DC == null)).ToList();
                foreach (var d in CashOut)
                {
                    var pay = ent.SD_33.FirstOrDefault(_ => _.RASH_ORDER_FROM_DC == d.DOC_CODE);
                    var bankpay = ent.TD_101.Include(_ => _.SD_101)
                        .FirstOrDefault(_ => _.VVT_RASH_KASS_ORDER_DC == d.DOC_CODE);
                    var name = d.CASH_TO_DC != null
                        ? MainReferences.Cashs[d.CASH_TO_DC.Value].Name
                        // ReSharper disable once PossibleInvalidOperationException
                        : MainReferences.BankAccounts[d.BANK_RASCH_SCHET_DC.Value].Name;
                    if (pay != null || bankpay != null) continue;
                    ExtendRows.Add(new ManagementBalanceExtendRowViewModel
                    {
                        DocCode = d.DOC_CODE,
                        GroupId = ManagemenentBalansStructrue.MoneyInPah,
                        // ReSharper disable once PossibleInvalidOperationException
                        Date = d.DATE_ORD.Value,
                        DocumentType = DocumentTypes.CashOut,
                        DocNum = d.NUM_ORD.ToString(),
                        KontragentName = name,
                        Name =
                            $"Расходный кассовый ордер №{d.NUM_ORD} от {d.DATE_ORD.Value.ToShortDateString()} в {name}",
                        Quantity = 1,
                        // ReSharper disable once PossibleInvalidOperationException
                        Price = (decimal) d.SUMM_ORD,
                        Summa = (decimal) d.SUMM_ORD,
                        Nom = null,
                        Nomenkl = null,
                        CurrencyName = MainReferences.GetCurrency(d.CRS_DC)?.Name,
                        Currency = MainReferences.GetCurrency(d.CRS_DC),
                        // ReSharper disable PossibleInvalidOperationException
                        SummaEUR = d.CRS_DC.Value == CurrencyCode.EUR ? (decimal) d.SUMM_ORD : 0,
                        // ReSharper restore PossibleInvalidOperationException
                        SummaUSD = d.CRS_DC.Value == CurrencyCode.USD ? (decimal) d.SUMM_ORD : 0,
                        SummaRUB = d.CRS_DC.Value == CurrencyCode.RUB ? (decimal) d.SUMM_ORD : 0,
                        SummaCNY = d.CRS_DC.Value == CurrencyCode.CNY ? (decimal) d.SUMM_ORD : 0
                    });
                }

                foreach (var d in CashIn)
                {
                    if (d.RASH_ORDER_FROM_DC != null) continue;
                    if (ent.TD_101.Any(_ =>
                        _.VVT_KASS_PRIH_ORDER_DC == d.DOC_CODE && _.SD_101.VV_START_DATE <= CurrentDate)) continue;
                    // ReSharper disable once PossibleInvalidOperationException
                    var kontrName = MainReferences.BankAccounts[d.BANK_RASCH_SCHET_DC.Value].Name;
                    ExtendRows.Add(new ManagementBalanceExtendRowViewModel
                    {
                        DocCode = d.DOC_CODE,
                        GroupId = ManagemenentBalansStructrue.MoneyInPah,
                        // ReSharper disable once PossibleInvalidOperationException
                        Date = d.DATE_ORD.Value,
                        DocumentType = DocumentTypes.CashIn,
                        DocNum = d.NUM_ORD.ToString(),
                        KontragentName = kontrName,
                        Name =
                            $"Приходный кассовый ордер №{d.NUM_ORD} от {d.DATE_ORD.Value.ToShortDateString()} из {kontrName}",
                        Quantity = 1,
                        // ReSharper disable once PossibleInvalidOperationException
                        Price = (decimal) d.SUMM_ORD,
                        Summa = (decimal) d.SUMM_ORD,
                        Nom = null,
                        Nomenkl = null,
                        // ReSharper disable once PossibleInvalidOperationException
                        CurrencyName = MainReferences.Currencies[d.CRS_DC.Value].Name,
                        Currency = MainReferences.Currencies[d.CRS_DC.Value],
                        SummaEUR = d.CRS_DC.Value == CurrencyCode.EUR ? (decimal) d.SUMM_ORD : 0,
                        SummaUSD = d.CRS_DC.Value == CurrencyCode.USD ? (decimal) d.SUMM_ORD : 0,
                        SummaRUB = d.CRS_DC.Value == CurrencyCode.RUB ? (decimal) d.SUMM_ORD : 0,
                        SummaCNY = d.CRS_DC.Value == CurrencyCode.CNY ? (decimal) d.SUMM_ORD : 0
                    });
                }

                foreach (var d in BankOut)
                {
                    var tr = ent.TD_101.FirstOrDefault(_ => _.BankFromTransactionCode == d.CODE);
                    if (tr != null) continue;
                    // ReSharper disable once PossibleInvalidOperationException
                    var name = MainReferences.BankAccounts[d.BankAccountDC.Value].Name;
                    ExtendRows.Add(new ManagementBalanceExtendRowViewModel
                    {
                        DocCode = d.DOC_CODE,
                        Code = d.CODE,
                        GroupId = ManagemenentBalansStructrue.MoneyInPah,
                        Date = d.SD_101.VV_START_DATE,
                        DocumentType = DocumentTypes.Bank,
                        DocNum = "",
                        KontragentName = name,
                        Name = $"Банковская транзакция от {d.SD_101.VV_START_DATE.ToShortDateString()} в {name}",
                        Quantity = 1,
                        // ReSharper disable once PossibleInvalidOperationException
                        Price = (decimal) d.VVT_VAL_RASHOD,
                        Summa = (decimal) d.VVT_VAL_RASHOD,
                        Nom = null,
                        Nomenkl = null,
                        CurrencyName = MainReferences.Currencies[d.VVT_CRS_DC].Name,
                        Currency = MainReferences.Currencies[d.VVT_CRS_DC],
                        SummaEUR = d.VVT_CRS_DC == CurrencyCode.EUR ? (decimal) d.VVT_VAL_RASHOD : 0,
                        SummaUSD = d.VVT_CRS_DC == CurrencyCode.USD ? (decimal) d.VVT_VAL_RASHOD : 0,
                        SummaRUB = d.VVT_CRS_DC == CurrencyCode.RUB ? (decimal) d.VVT_VAL_RASHOD : 0,
                        SummaCNY = d.VVT_CRS_DC == CurrencyCode.CNY ? (decimal) d.VVT_VAL_RASHOD : 0
                    });
                }
            }

            foreach (var e in ExtendRows.Where(_ => _.GroupId == ManagemenentBalansStructrue.MoneyInPah))
                switch (e.CurrencyName)
                {
                    case CurrencyCode.RUBName:
                    case CurrencyCode.RURName:
                        ch.SummaRUB += e.SummaRUB;
                        break;
                    case CurrencyCode.EURName:
                        ch.SummaEUR += e.SummaEUR;
                        break;
                    case CurrencyCode.USDName:
                        ch.SummaUSD += e.SummaUSD;
                        break;
                    case CurrencyCode.CNYName:
                        ch.SummaCNY += e.SummaCNY;
                        break;
                }
        }

        private void BalansCrossRateRecalc(object obj)
        {
            if (RecalcCurrency == null || CurrentCurrencyRate == null) return;
            foreach (var b in BalansStructure)
                b.RecalcCurrency = b.SummaEUR *
                                   CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.EUR],
                                       RecalcCurrency) +
                                   b.SummaUSD * CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.USD],
                                       RecalcCurrency) +
                                   b.SummaGBP * CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.GBP],
                                       RecalcCurrency) +
                                   b.SummaRUB * CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.RUB],
                                       RecalcCurrency) +
                                   b.SummaCNY * CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.CNY],
                                       RecalcCurrency);
            if (ExtendRowsActual.Count > 0 && RecalcCurrency != null && CurrentCurrencyRate != null)
                foreach (var b in ExtendRowsActual)
                {
                    b.PriceCalc = b.PriceEUR *
                                  CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.EUR],
                                      RecalcCurrency) +
                                  b.PriceUSD *
                                  CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.USD],
                                      RecalcCurrency) +
                                  b.PriceGBP *
                                  CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.GBP],
                                      RecalcCurrency) +
                                  b.PriceRUB *
                                  CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.RUB],
                                      RecalcCurrency) +
                                  b.PriceCNY *
                                  CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.CNY],
                                      RecalcCurrency);
                    b.SummaCalc = b.SummaEUR *
                                  CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.EUR],
                                      RecalcCurrency) +
                                  b.SummaUSD *
                                  CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.USD],
                                      RecalcCurrency) +
                                  b.SummaGBP *
                                  CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.GBP],
                                      RecalcCurrency) +
                                  b.SummaRUB *
                                  CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.RUB],
                                      RecalcCurrency) +
                                  b.SummaCNY *
                                  CurrentCurrencyRate.GetRate(MainReferences.Currencies[CurrencyCode.CNY],
                                      RecalcCurrency);
                }

            RaisePropertyChanged(nameof(BalansStructure));
        }

        public override void DocumentOpen(object obj)
        {
            if (CurrentExtendItem == null) return;
            switch (CurrentExtendItem.DocumentType)
            {
                case DocumentTypes.CashIn:
                    DocumentsOpenManager.Open(DocumentType.CashIn, CurrentExtendItem.DocCode);
                    break;
                case DocumentTypes.CashOut:
                    DocumentsOpenManager.Open(DocumentType.CashOut, CurrentExtendItem.DocCode);
                    break;
            }
        }

        public override void SaveData(object obj)
        {
        }

        private void GetCash()
        {
            var ch = BalansStructure.Single(_ => _.Id == myBalansBuilder.Structure
                .Single(t => t.Tag == BalansSection.Cash)
                .Id);
            ch.Summa = 0;
            var nn =
                BalansStructure.Where(_ => _.ParentId == ch.Id).ToList();
            foreach (
                var r in nn)
                BalansStructure.Remove(r);
            var data = new List<ManagementCash>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var cashList = ctx.TD_22.Include(_ => _.SD_22).ToList();
                foreach (var c in cashList)
                {
                    var newItem = new ManagementCash
                    {
                        CashDC = c.DOC_CODE,
                        CurrencyDC = c.CRS_DC,
                        Summa = CashManager.GetCashCurrencyRemains(c.DOC_CODE, c.CRS_DC, CurrentDate)
                    };
                    data.Add(newItem);
                }

                foreach (var d in data.Select(_ => _.CashDC).Distinct())
                {
                    var d1 = d;
                    var s =
                        data.Where(_ => _.CashDC == d1);
                    BalansStructure.Add(new ManagementBalanceGroupViewModel
                    {
                        Id = Guid.NewGuid(),
                        ParentId = ch.Id,
                        Name = myCashs.Single(_ => _.DOC_CODE == d).CA_NAME,
                        Order = 1,
                        // ReSharper disable PossibleMultipleEnumeration
                        SummaEUR = s.Where(_ => _.CurrencyDC == CurrencyCode.EUR).Sum(_ => _.Summa),
                        SummaUSD = s.Where(_ => _.CurrencyDC == CurrencyCode.USD).Sum(_ => _.Summa),
                        SummaRUB = s.Where(_ => _.CurrencyDC == CurrencyCode.RUB).Sum(_ => _.Summa),
                        SummaGBP = s.Where(_ => _.CurrencyDC == CurrencyCode.GBP).Sum(_ => _.Summa),
                        SummaCHF = s.Where(_ => _.CurrencyDC == CurrencyCode.CHF).Sum(_ => _.Summa),
                        SummaSEK = s.Where(_ => _.CurrencyDC == CurrencyCode.SEK).Sum(_ => _.Summa),
                        SummaCNY = s.Where(_ => _.CurrencyDC == CurrencyCode.CNY).Sum(_ => _.Summa),
                        //Tag = BalansSection.ManagementCash,
                        ObjectDC = d

                        // ReSharper restore PossibleMultipleEnumeration
                    });
                }

                ch.SummaEUR =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaEUR);
                ch.SummaRUB =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaRUB);
                ch.SummaUSD =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaUSD);
                ch.SummaGBP =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaGBP);
                ch.SummaCHF =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaCHF);
                ch.SummaSEK =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaSEK);
                ch.SummaCNY =
                    BalansStructure.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaCNY);
            }
        }

        private void GetBank()
        {
            var ch = BalansStructure.Single(_ => _.Id == myBalansBuilder.Structure
                .Single(t => t.Tag == BalansSection.Bank)
                .Id);
            ch.Summa = 0;
            var nn =
                BalansStructure.Where(_ => _.ParentId == ch.Id).ToList();
            foreach (
                var r in nn)
                BalansStructure.Remove(r);
            var bankManager = new BankOperationsManager();
            foreach (var d in MainReferences.BankAccounts.Values.Select(_ => _.DocCode).Distinct())
            {
                var bank = MainReferences.BankAccounts[d];
                var rem = bankManager.GetRemains2(d, CurrentDate, CurrentDate);
                if (rem.SummaEnd != 0)
                    BalansStructure.Add(new ManagementBalanceGroupViewModel
                    {
                        Id = Guid.NewGuid(),
                        ParentId = ch.Id,
                        Name =
                            myBanks.Single(_ => _.DOC_CODE == d).BA_BANK_NAME + " / " +
                            myBanks.Single(_ => _.DOC_CODE == d).BA_RASH_ACC,
                        Order = 1,
                        // ReSharper disable PossibleMultipleEnumeration
                        // ReSharper disable PossibleInvalidOperationException
                        SummaEUR = bank.Currency.DocCode == CurrencyCode.EUR ? (decimal) rem.SummaEnd : 0,
                        SummaUSD = bank.Currency.DocCode == CurrencyCode.USD ? (decimal) rem.SummaEnd : 0,
                        SummaRUB = bank.Currency.DocCode == CurrencyCode.RUB ? (decimal) rem.SummaEnd : 0,
                        SummaGBP = bank.Currency.DocCode == CurrencyCode.GBP ? (decimal) rem.SummaEnd : 0,
                        SummaCHF = bank.Currency.DocCode == CurrencyCode.CHF ? (decimal) rem.SummaEnd : 0,
                        SummaSEK = bank.Currency.DocCode == CurrencyCode.SEK ? (decimal) rem.SummaEnd : 0,
                        SummaCNY = bank.Currency.DocCode == CurrencyCode.CNY ? (decimal) rem.SummaEnd : 0,
                        ObjectDC = d
                        // ReSharper restore PossibleMultipleEnumeration
                        // ReSharper restore PossibleInvalidOperationException
                    });
            }

            ch.SummaEUR =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaEUR);
            ch.SummaRUB =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaRUB);
            ch.SummaUSD =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaUSD);
            ch.SummaGBP =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaGBP);
            ch.SummaCHF =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaCHF);
            ch.SummaSEK =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaSEK);
            ch.SummaCNY =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaCNY);
        }

        private void GetKontragent()
        {
            var cc = BalansStructure.Single(_ => _.Id == myBalansBuilder.Structure
                .Single(t => t.Tag == BalansSection.Creditors)
                .Id);
            cc.Summa = 0;
            cc.SummaRUB = 0;
            cc.SummaUSD = 0;
            cc.SummaEUR = 0;
            cc.SummaGBP = 0;
            cc.SummaCHF = 0;
            cc.SummaSEK = 0;
            cc.SummaCNY = 0;
            var dd = BalansStructure.Single(_ => _.Id == myBalansBuilder.Structure
                .Single(t => t.Tag == BalansSection.Debitors)
                .Id);
            dd.Summa = 0;
            dd.SummaRUB = 0;
            dd.SummaUSD = 0;
            dd.SummaEUR = 0;
            dd.SummaGBP = 0;
            dd.SummaCHF = 0;
            dd.SummaSEK = 0;
            dd.SummaCNY = 0;
            var sql = "SELECT kboab.KONTR_DC as KontrDC, " +
                      "       cast(SUM(isnull(kboab.CRS_KONTR_OUT,0) - isnull(kboab.CRS_KONTR_IN,0)) AS numeric(18,2)) AS Summa, " +
                      " s43.valuta_dc AS KontrCrsDC" +
                      "  FROM KONTR_BALANS_OPER_ARC kboab " +
                      " INNER JOIN SD_43 S43 ON S43.DOC_CODE = kboab.KONTR_DC " +
                      $" WHERE kboab.DOC_DATE <= '{CustomFormat.DateToString(CurrentDate)}' AND ISNULL(DELETED,0) = 0 AND FLAG_BALANS = 1 AND kboab.DOC_DATE >= s43.START_BALANS " +
                      "  group by kboab.KONTR_DC, s43.valuta_dc " +
                      "  HAVING SUM(kboab.CRS_KONTR_OUT - kboab.CRS_KONTR_IN) != 0";
            var kontrChanged = GlobalOptions.GetEntities().KONTR_BLS_RECALC.ToList();
            foreach (var k in kontrChanged.Select(_ => _.KONTR_DC).Distinct())
            {
                var dMin = kontrChanged.Where(_ => _.KONTR_DC == k).Min(_ => _.DATE_CHANGED);
                RecalcKontragentBalans.CalcBalans(k, dMin);
            }

            var data = GlobalOptions.GetEntities().Database.SqlQuery<Kontr>(sql).ToList();
            foreach (var d in data.Where(_ => _.Summa < 0))
            {
                var k = MainReferences.GetKontragent(d.KontrDC);
                cc.SummaRUB += d.KontrCrsDC == CurrencyCode.RUB ? d.Summa : 0;
                cc.SummaUSD += d.KontrCrsDC == CurrencyCode.USD ? d.Summa : 0;
                cc.SummaEUR += d.KontrCrsDC == CurrencyCode.EUR ? d.Summa : 0;
                cc.SummaGBP += d.KontrCrsDC == CurrencyCode.GBP ? d.Summa : 0;
                cc.SummaCHF += d.KontrCrsDC == CurrencyCode.CHF ? d.Summa : 0;
                cc.SummaSEK += d.KontrCrsDC == CurrencyCode.SEK ? d.Summa : 0;
                cc.SummaCNY += d.KontrCrsDC == CurrencyCode.CNY ? d.Summa : 0;
                ExtendRows.Add(new ManagementBalanceExtendRowViewModel
                {
                    GroupId = cc.Id,
                    Name = k.Name,
                    Summa =
                        d.Summa
                    //* GetRate(myRates, k.SD_301.DOC_CODE,GlobalOptions.SystemProfile.MainCurrency.DocCode,CurrentDate)
                    ,
                    CurrencyName = k.BalansCurrency.Name,
                    SummaEUR = d.KontrCrsDC == CurrencyCode.EUR ? d.Summa : 0,
                    SummaUSD = d.KontrCrsDC == CurrencyCode.USD ? d.Summa : 0,
                    SummaRUB = d.KontrCrsDC == CurrencyCode.RUB ? d.Summa : 0,
                    SummaGBP = d.KontrCrsDC == CurrencyCode.GBP ? d.Summa : 0,
                    SummaCHF = d.KontrCrsDC == CurrencyCode.CHF ? d.Summa : 0,
                    SummaSEK = d.KontrCrsDC == CurrencyCode.SEK ? d.Summa : 0,
                    SummaCNY = d.KontrCrsDC == CurrencyCode.CNY ? d.Summa : 0,
                    Kontragent = k
                });
            }

            foreach (var d in data.Where(_ => _.Summa > 0))
            {
                var k = MainReferences.GetKontragent(d.KontrDC);
                dd.SummaRUB += d.KontrCrsDC == CurrencyCode.RUB ? d.Summa : 0;
                dd.SummaUSD += d.KontrCrsDC == CurrencyCode.USD ? d.Summa : 0;
                dd.SummaEUR += d.KontrCrsDC == CurrencyCode.EUR ? d.Summa : 0;
                dd.SummaGBP += d.KontrCrsDC == CurrencyCode.GBP ? d.Summa : 0;
                dd.SummaCHF += d.KontrCrsDC == CurrencyCode.CHF ? d.Summa : 0;
                dd.SummaSEK += d.KontrCrsDC == CurrencyCode.SEK ? d.Summa : 0;
                dd.SummaCNY += d.KontrCrsDC == CurrencyCode.CNY ? d.Summa : 0;
                ExtendRows.Add(new ManagementBalanceExtendRowViewModel
                {
                    GroupId = dd.Id,
                    Name = k.Name,
                    Summa =
                        d.Summa
                    //*GetRate(myRates, k.SD_301.DOC_CODE,GlobalOptions.SystemProfile.MainCurrency.DocCode,CurrentDate)
                    ,
                    CurrencyName = k.BalansCurrency.Name,
                    SummaEUR = d.KontrCrsDC == CurrencyCode.EUR ? d.Summa : 0,
                    SummaUSD = d.KontrCrsDC == CurrencyCode.USD ? d.Summa : 0,
                    SummaRUB = d.KontrCrsDC == CurrencyCode.RUB ? d.Summa : 0,
                    SummaGBP = d.KontrCrsDC == CurrencyCode.GBP ? d.Summa : 0,
                    SummaCHF = d.KontrCrsDC == CurrencyCode.CHF ? d.Summa : 0,
                    SummaSEK = d.KontrCrsDC == CurrencyCode.SEK ? d.Summa : 0,
                    SummaCNY = d.KontrCrsDC == CurrencyCode.SEK ? d.Summa : 0,
                    Kontragent = k
                });
            }
        }

        private decimal GetRound(decimal d)
        {
            return d - (d - Math.Round(d, 4));
        }

        private void GetNomenkl()
        {
            var ch = BalansStructure.Single(_ => _.Id == myBalansBuilder.Structure
                .Single(t => t.Tag == BalansSection.Store)
                .Id);
            ch.Summa = 0;
            var chNach = BalansStructure.Single(_ => _.Id == ch.Id);
            chNach.Summa = 0;
            var data = NomenklCalculationManager.GetNomenklStoreRemains(CurrentDate);
            if (data.Count == 0) return;
            if (data.Select(_ => _.StoreDC).ToList().Count == 0) return;
            var skl = data.Select(_ => _.StoreDC).Distinct().ToList();
            foreach (var s in skl)
            {
                var n = MainReferences.Warehouses[s];
                var newId = Guid.NewGuid();
                BalansStructure.Add(new ManagementBalanceGroupViewModel
                {
                    Id = newId,
                    ParentId = ch.Id,
                    Name = n.SKL_NAME,
                    Order = 1,
                    SummaRUB =
                        data.Where(_ => _.NomCurrencyDC == CurrencyCode.RUB && _.StoreDC == s)
                            .Sum(_ => GetRound(_.Summa)),
                    SummaUSD =
                        data.Where(_ => _.NomCurrencyDC == CurrencyCode.USD && _.StoreDC == s)
                            .Sum(_ => GetRound(_.Summa)),
                    SummaEUR =
                        data.Where(_ => _.NomCurrencyDC == CurrencyCode.EUR && _.StoreDC == s)
                            .Sum(_ => GetRound(_.Summa)),
                    SummaGBP =
                        data.Where(_ => _.NomCurrencyDC == CurrencyCode.GBP && _.StoreDC == s)
                            .Sum(_ => GetRound(_.Summa)),
                    SummaCHF =
                        data.Where(_ => _.NomCurrencyDC == CurrencyCode.CHF && _.StoreDC == s)
                            .Sum(_ => GetRound(_.Summa)),
                    SummaSEK =
                        data.Where(_ => _.NomCurrencyDC == CurrencyCode.SEK && _.StoreDC == s)
                            .Sum(_ => GetRound(_.Summa)),
                    SummaCNY =
                        data.Where(_ => _.NomCurrencyDC == CurrencyCode.CNY && _.StoreDC == s)
                            .Sum(_ => GetRound(_.Summa)),
                    ObjectDC = n.DocCode
                    //Tag = BalansSection.WarehouseIn
                });
                var s1 = s;
                var nomsDC = data.Where(_ => _.StoreDC == s1).Select(_ => _.NomenklDC).Distinct();
                foreach (var dd in (from nDC in nomsDC
                    let s2 = s
                    let dc = nDC
                    select data.FirstOrDefault(_ => _.StoreDC == s2 && _.NomenklDC == dc)
                    into dd
                    where dd != null
                    select dd).Where(dd => dd.Prihod != dd.Rashod))
                {
                    ExtendRows.Add(new ManagementBalanceExtendRowViewModel
                    {
                        GroupId = newId,
                        Name = dd.NomenklName,
                        Quantity = dd.Prihod - dd.Rashod,
                        Price = dd.Summa / (dd.Prihod - dd.Rashod),
                        Summa = dd.Summa,
                        Nom = MainReferences.GetNomenkl(dd.NomenklDC),
                        Nomenkl = MainReferences.GetNomenkl(dd.NomenklDC).NomenklNumber,
                        CurrencyName = MainReferences.Currencies[dd.NomCurrencyDC].Name,
                        SummaEUR = dd.NomCurrencyDC == CurrencyCode.EUR ? dd.Summa : 0,
                        SummaUSD = dd.NomCurrencyDC == CurrencyCode.USD ? dd.Summa : 0,
                        SummaRUB = dd.NomCurrencyDC == CurrencyCode.RUB ? dd.Summa : 0,
                        SummaCNY = dd.NomCurrencyDC == CurrencyCode.CNY ? dd.Summa : 0
                    });
                }
            }

            ch.Summa =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.Summa);
            ch.SummaEUR =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaEUR);
            ch.SummaUSD =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaUSD);
            ch.SummaRUB =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaRUB);
            ch.SummaGBP =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaGBP);
            ch.SummaCHF =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaCHF);
            ch.SummaSEK =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaSEK);
            ch.SummaCNY =
                BalansStructure.Where(_ => _.ParentId == ch.Id)
                    .Sum(_ => _.SummaCNY);
        }

        private void GetZarplata()
        {
            var ch = BalansStructure.Single(_ => _.Id == myBalansBuilder.Structure
                .Single(t => t.Tag == BalansSection.Salary)
                .Id);
            ch.Summa = 0;
            ch.SummaRUB = 0;
            ch.SummaEUR = 0;
            ch.SummaUSD = 0;
            ch.SummaCNY = 0;
            var zp = new EmployeePayWindowViewModel(false);
            zp.LoadForAll(CurrentDate);
            foreach (var per in zp.EmployeeMain)
            {
                switch (per.CrsName)
                {
                    case CurrencyCode.RUBName:
                    case CurrencyCode.RURName:
                        ch.SummaRUB += -per.DolgSumma;
                        break;
                    case CurrencyCode.EURName:
                        ch.SummaEUR += -per.DolgSumma;
                        break;
                    case CurrencyCode.USDName:
                        ch.SummaUSD += -per.DolgSumma;
                        break;
                    case CurrencyCode.CNYName:
                        ch.SummaCNY += -per.DolgSumma;
                        break;
                }

                ExtendRows.Add(new ManagementBalanceExtendRowViewModel
                {
                    Name = per.EmployeeName,
                    GroupId = ch.Id,
                    Summa = -per.DolgSumma,
                    CurrencyName = per.CrsName,
                    // ReSharper disable PossibleInvalidOperationException
                    SummaEUR = per.Employee.crs_dc.Value == CurrencyCode.EUR ? -per.DolgSumma : 0,
                    SummaUSD = per.Employee.crs_dc.Value == CurrencyCode.USD ? -per.DolgSumma : 0,
                    SummaRUB = per.Employee.crs_dc.Value == CurrencyCode.RUB ? -per.DolgSumma : 0,
                    SummaGBP = per.Employee.crs_dc.Value == CurrencyCode.GBP ? -per.DolgSumma : 0,
                    SummaCHF = per.Employee.crs_dc.Value == CurrencyCode.CHF ? -per.DolgSumma : 0,
                    SummaSEK = per.Employee.crs_dc.Value == CurrencyCode.SEK ? -per.DolgSumma : 0,
                    SummaCNY = per.Employee.crs_dc.Value == CurrencyCode.CNY ? -per.DolgSumma : 0,
                    Persona = per.Employee
                    // ReSharper restore PossibleInvalidOperationException
                });
            }
        }

        public class ManagementCash
        {
            public decimal CashDC { set; get; }
            public decimal CurrencyDC { set; get; }
            public decimal Summa { set; get; }
        }

        public class Bank
        {
            public decimal BankDC { set; get; }
            public decimal CurrencyDC { set; get; }
            public decimal Summa { set; get; }
        }

        private class Kontr
        {
            public decimal KontrDC { set; get; }
            public decimal Summa { set; get; }
            public decimal KontrCrsDC { set; get; }
        }
    }
}