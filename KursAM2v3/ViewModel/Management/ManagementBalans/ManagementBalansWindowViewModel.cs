using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
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
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository.NomenklRepository;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace KursAM2.ViewModel.Management.ManagementBalans
{
    public class ManagementBalansWindowViewModel : RSWindowViewModelBase
    {
        private readonly List<SD_114> myBanks;
        private readonly List<SD_22> myCashs;
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());
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
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = GetRightMenu();
            using (var ent = GlobalOptions.GetEntities())
            {
                myBanks = ent.SD_114.ToList();
                myCashs = ent.SD_22.ToList();
            }

            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as Currency);
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as Currency);
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as Currency);
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as Currency);
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as Currency);
            var crsrate = new CrossCurrencyRate();
            crsrate.SetRates(DateTime.Today);
            foreach (var c in crsrate.CurrencyList)
                CurrencyRates.Add(c);
            CurrentCurrencyRate = CurrencyRates[0];
        }


        public override string WindowName => "Управленческий баланс";
        public override string LayoutName => "ManagementBalansWindowViewModel";

        public ObservableCollection<ColumnSummary> SummaryList { get; } =
            new ObservableCollection<ColumnSummary>();

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public List<Currency> CurrenciesForRecalc { set; get; } = new List<Currency>();

        public ObservableCollection<CrossCurrencyRate> CurrencyRates { set; get; } =
            new ObservableCollection<CrossCurrencyRate>();

        public override void AddSearchList(object obj)
        {
            var ctx1 = new ManagementBalansWindowViewModel { CurrentDate = DateTime.Today };
            var form = new ManagementBalansView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx1
            };
            form.DataContext = ctx1;
            ctx1.Form = form;
            form.Show();

        }
        
        
        public CrossCurrencyRate CurrentCurrencyRate
        {
            get => myCurrentCurrencyRate;
            set
            {
                if (Equals(myCurrentCurrencyRate, value)) return;
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
        public ICommand SummaryCommand => new Command(CalcTotalSummaries, _ => true);

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
                                                        GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as
                                                            Currency,
                                                        RecalcCurrency) +
                                                    b.PriceUSD *
                                                    CurrentCurrencyRate.GetRate(
                                                        GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as
                                                            Currency,
                                                        RecalcCurrency) +
                                                    b.PriceGBP *
                                                    CurrentCurrencyRate.GetRate(
                                                        GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as
                                                            Currency,
                                                        RecalcCurrency) +
                                                    b.PriceRUB *
                                                    CurrentCurrencyRate.GetRate(
                                                        GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as
                                                            Currency,
                                                        RecalcCurrency) +
                                                    b.PriceCNY *
                                                    CurrentCurrencyRate.GetRate(
                                                        GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as
                                                            Currency,
                                                        RecalcCurrency), 2);
                        b.SummaCalc = decimal.Round(b.SummaEUR *
                                                    CurrentCurrencyRate.GetRate(
                                                        GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as
                                                            Currency,
                                                        RecalcCurrency) +
                                                    b.SummaUSD *
                                                    CurrentCurrencyRate.GetRate(
                                                        GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as
                                                            Currency,
                                                        RecalcCurrency) +
                                                    b.SummaGBP *
                                                    CurrentCurrencyRate.GetRate(
                                                        GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as
                                                            Currency,
                                                        RecalcCurrency) +
                                                    b.SummaCNY *
                                                    CurrentCurrencyRate.GetRate(
                                                        GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as
                                                            Currency,
                                                        RecalcCurrency) +
                                                    b.SummaRUB *
                                                    CurrentCurrencyRate.GetRate(
                                                        GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as
                                                            Currency,
                                                        RecalcCurrency), 2);
                    }

                    RaisePropertyChanged(nameof(RecalcCrsName));
                }

                var frm = (ManagementBalansView)Form;
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
            var ctxk = new KontragentBalansWindowViewModel(CurrentExtendItem.Kontragent.DocCode);
            var frm = new KontragentBalansForm { Owner = Application.Current.MainWindow, DataContext = ctxk };
            ctxk.Form = frm;
            frm.Show();
        }

        private void NomenklCalcOpen(object obj)
        {
            if (CurrentExtendItem?.Nom.DocCode == null) return;
            var ctx = new NomPriceWindowViewModel((decimal)CurrentExtendItem?.Nom.DocCode);
            var dlg = new SelectDialogView { DataContext = ctx };
            dlg.ShowDialog();
        }

        private void GenerateSummaries()
        {
            SummaryList.Clear();
            SummaryList.Add(
                new ColumnSummary { Type = SummaryItemType.Count, FieldName = "Name", DisplayFormat = "{n0}" });
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
            var frm = (ManagementBalansView)Form;
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

                if (((GridSummaryItem)e.Item).FieldName == "Summa")
                    if (e.IsTotalSummary)
                    {
                        if (e.SummaryProcess == CustomSummaryProcess.Start)
                            SummaryList.Single(_ => _.Key == (string)((GridSummaryItem)e.Item).Tag).Summa = 0;
                        if (e.SummaryProcess == CustomSummaryProcess.Calculate)
                        {
                            var val = (decimal)e.FieldValue;
                            if (!(e.Row is ManagementBalanceExtendRowViewModel row)) return;
                            if (row.CurrencyName == (string)((GridSummaryItem)e.Item).Tag)
                            {
                                SummaryList.Single(_ => _.Key == (string)((GridSummaryItem)e.Item).Tag).Summa += val;
                                e.TotalValue =
                                    SummaryList.Single(_ => _.Key == (string)((GridSummaryItem)e.Item).Tag).Summa;
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
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            myBalansBuilder = new ManagementBalansBuilder();
            ExtendRows.Clear();
            ExtendRowsActual.Clear();
            BalansStructure.Clear();
            foreach (var r in myBalansBuilder.Structure)
                BalansStructure.Add(r);
            try
            {
                //GetCash();
                GetCashAsync();
                GetBank();
                GetKontragent();
                GetNomenkl();
                GetZarplata();
                GetMoneyInPath();
                GetTovarInPath();
                //CalcStockHolder();
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
                var frm = (ManagementBalansView)Form;
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
                RaisePropertyChanged("BalansStructure");
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
            finally
            {
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            }

            GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
        }

        public void CalcStockHolder()
        {
            var ch = BalansStructure.Single(_ => _.Id == myBalansBuilder.Structure
                .Single(t => t.Tag == BalansSection.StockHolder)
                .Id);
            ch.Summa = 0;
            ch.SummaRUB = 0;
            ch.SummaEUR = 0;
            ch.SummaUSD = 0;
            ch.SummaCNY = 0;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var dataNach = ctx.StockHolderAccrualRows.Include(_ => _.StockHolderAccrual)
                    .Where(_ => _.StockHolderAccrual.Date <= CurrentDate).ToList();
                var dataOut = ctx.SD_34.Where(_ => _.StockHolderId != null && _.DATE_ORD <= CurrentDate).ToList();
                var listSH = ctx.StockHolders.ToList();
                foreach (var sh in listSH)
                {
                    var newItem = new ManagementBalanceExtendRowViewModel
                    {
                        GroupId = ch.Id,
                        Name = sh.Name,
                        Quantity = 1,
                        Price = 0,
                        Summa = 0,
                        Nom = null,
                        Nomenkl = null,
                        // ReSharper disable once PossibleInvalidOperationException
                        CurrencyName = null,
                        SummaEUR = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.EUR)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.EUR)
                                       .Sum(x => x.Summa) ?? 0),
                        PriceEUR = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.EUR)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.EUR)
                                       .Sum(x => x.Summa) ?? 0),
                        SummaUSD = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.USD)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.USD)
                                       .Sum(x => x.Summa) ?? 0),
                        PriceUSD = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.USD)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.USD)
                                       .Sum(x => x.Summa) ?? 0),
                        SummaRUB = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.RUB)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.RUB)
                                       .Sum(x => x.Summa) ?? 0),
                        PriceRUB = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.RUB)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.RUB)
                                       .Sum(x => x.Summa) ?? 0),
                        SummaCNY = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.CNY)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.CNY)
                                       .Sum(x => x.Summa) ?? 0),
                        PriceCNY = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.CNY)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.CNY)
                                       .Sum(x => x.Summa) ?? 0),
                        SummaCHF = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.CHF)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.CHF)
                                       .Sum(x => x.Summa) ?? 0),
                        PriceCHF = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.CHF)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.CHF)
                                       .Sum(x => x.Summa) ?? 0),
                        SummaGBP = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.GBP)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.GBP)
                                       .Sum(x => x.Summa) ?? 0),
                        PriceGBP = (dataOut.Where(_ => _.StockHolderId == sh.Id && _.CRS_DC == CurrencyCode.GBP)
                                       .Sum(x => x.SUMM_ORD) ?? 0) -
                                   (dataNach.Where(_ => _.StockHolderId == sh.Id && _.CurrencyDC == CurrencyCode.GBP)
                                       .Sum(x => x.Summa) ?? 0)
                    };
                    ExtendRows.Add(newItem);
                }
            }

            ch.Summa = 0;
            ch.SummaEUR = ExtendRows.Where(_ => _.GroupId == ch.Id).Sum(x => x.SummaEUR);
            ch.SummaUSD = ExtendRows.Where(_ => _.GroupId == ch.Id).Sum(x => x.SummaUSD);
            ch.SummaRUB = ExtendRows.Where(_ => _.GroupId == ch.Id).Sum(x => x.SummaRUB);
            ch.SummaGBP = ExtendRows.Where(_ => _.GroupId == ch.Id).Sum(x => x.SummaGBP);
            ch.SummaCHF = ExtendRows.Where(_ => _.GroupId == ch.Id).Sum(x => x.SummaCHF);
            ch.SummaSEK = ExtendRows.Where(_ => _.GroupId == ch.Id).Sum(x => x.SummaSEK);
            ch.SummaCNY = ExtendRows.Where(_ => _.GroupId == ch.Id).Sum(x => x.SummaCNY);
        }

        /// <summary>
        ///     товар в пути
        /// </summary>
        public void GetTovarInPath()
        {
            var ch = BalansStructure.Single(_ => _.Id == myBalansBuilder.Structure
                .Single(t => t.Tag == BalansSection.Store)
                .Id);
            ch.Summa = 0;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.TD_24.Include(_ => _.SD_24)
                    .Include(_ => _.SD_83)
                    .Where(_ => _.SD_24.DD_TYPE_DC == 2010000003
                                && _.SD_24.DD_SKLAD_POL_DC != null
                                && _.SD_24.DD_DATE <= CurrentDate).ToList();
                var prihData = ctx.TD_24.Include(_ => _.SD_24).Where(_ => _.SD_24.DD_TYPE_DC == 2010000001
                                                                          && _.DDT_RASH_ORD_DC != null
                                                                          && _.SD_24.DD_DATE <= CurrentDate).ToList();
                var inPanth = new List<TD_24>();
                foreach (var d in data)
                {
                    if (prihData.Any(_ => _.DDT_RASH_ORD_DC == d.DOC_CODE && _.DDT_RASH_ORD_CODE == d.CODE)) continue;
                    inPanth.Add(d);
                }

                if (inPanth.Count > 0)
                {
                    var newId = Guid.NewGuid();
                    foreach (var p in inPanth)
                    {
                        var prc = NomenklViewModel.Price(p.DDT_NOMENKL_DC, p.SD_24.DD_DATE);
                        var nom = GlobalOptions.ReferencesCache.GetNomenkl(p.DDT_NOMENKL_DC) as Nomenkl;
                        var newItem = new ManagementBalanceExtendRowViewModel
                        {
                            GroupId = newId,
                            Name = p.SD_83.NOM_NAME,
                            Quantity = p.DDT_KOL_RASHOD,
                            Price = prc,
                            Summa = prc * p.DDT_KOL_RASHOD,
                            Nom = nom,
                            Nomenkl = nom.NomenklNumber,
                            // ReSharper disable once PossibleInvalidOperationException
                            CurrencyName = ((IName)nom.Currency).Name,
                            SummaEUR =
                                ((IDocCode)nom.Currency).DocCode == CurrencyCode.EUR ? prc * p.DDT_KOL_RASHOD : 0,
                            PriceEUR = ((IDocCode)nom.Currency).DocCode == CurrencyCode.EUR ? prc : 0,
                            SummaUSD =
                                ((IDocCode)nom.Currency).DocCode == CurrencyCode.USD ? prc * p.DDT_KOL_RASHOD : 0,
                            PriceUSD = ((IDocCode)nom.Currency).DocCode == CurrencyCode.USD ? prc : 0,
                            SummaRUB =
                                ((IDocCode)nom.Currency).DocCode == CurrencyCode.RUB ? prc * p.DDT_KOL_RASHOD : 0,
                            PriceRUB = ((IDocCode)nom.Currency).DocCode == CurrencyCode.RUB ? prc : 0,
                            SummaCNY =
                                ((IDocCode)nom.Currency).DocCode == CurrencyCode.CNY ? prc * p.DDT_KOL_RASHOD : 0,
                            PriceCNY = ((IDocCode)nom.Currency).DocCode == CurrencyCode.CNY ? prc : 0,
                            SummaCHF =
                                ((IDocCode)nom.Currency).DocCode == CurrencyCode.CHF ? prc * p.DDT_KOL_RASHOD : 0,
                            PriceCHF = ((IDocCode)nom.Currency).DocCode == CurrencyCode.CHF ? prc : 0,
                            SummaGBP =
                                ((IDocCode)nom.Currency).DocCode == CurrencyCode.GBP ? prc * p.DDT_KOL_RASHOD : 0,
                            PriceGBP = ((IDocCode)nom.Currency).DocCode == CurrencyCode.GBP ? prc : 0
                        };
                        ExtendRows.Add(newItem);
                    }

                    var newGrp = new ManagementBalanceGroupViewModel
                    {
                        Id = newId,
                        ParentId = ch.Id,
                        Name = "Товары в пути",
                        Order = 1,
                        SummaEUR = ExtendRows.Where(_ => _.GroupId == newId).Sum(_ => _.SummaEUR),
                        SummaUSD = ExtendRows.Where(_ => _.GroupId == newId).Sum(_ => _.SummaUSD),
                        SummaRUB = ExtendRows.Where(_ => _.GroupId == newId).Sum(_ => _.SummaRUB),
                        SummaCNY = ExtendRows.Where(_ => _.GroupId == newId).Sum(_ => _.SummaCNY),
                        SummaCHF = ExtendRows.Where(_ => _.GroupId == newId).Sum(_ => _.SummaCHF),
                        SummaGBP = ExtendRows.Where(_ => _.GroupId == newId).Sum(_ => _.SummaGBP),
                        ObjectDC = 10270000000
                    };
                    BalansStructure.Add(newGrp);
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
                    var pay = ent.SD_33.FirstOrDefault(_ =>
                        _.RASH_ORDER_FROM_DC == d.DOC_CODE && _.DATE_ORD <= CurrentDate);
                    var bankpay = ent.TD_101.Include(_ => _.SD_101)
                        .FirstOrDefault(_ =>
                            _.VVT_RASH_KASS_ORDER_DC == d.DOC_CODE && _.SD_101.VV_STOP_DATE <= CurrentDate);
                    var name = d.CASH_TO_DC != null
                        ? ((IName)GlobalOptions.ReferencesCache.GetCashBox(d.CASH_TO_DC)).Name
                        // ReSharper disable once PossibleInvalidOperationException
                        : ((IName)GlobalOptions.ReferencesCache.GetBankAccount(d.BANK_RASCH_SCHET_DC)).Name;
                    if (pay != null || bankpay != null) continue;
                    ExtendRows.Add(new ManagementBalanceExtendRowViewModel
                    {
                        DocCode = d.DOC_CODE,
                        GroupId = ManagemenentBalansStructrue.MoneyInPah,
                        // ReSharper disable once PossibleInvalidOperationException
                        Date = d.DATE_ORD.Value,
                        DocumentType = DocumentType.CashOut,
                        DocNum = d.NUM_ORD.ToString(),
                        KontragentName = name,
                        Name =
                            $"Расходный кассовый ордер №{d.NUM_ORD} от {d.DATE_ORD.Value.ToShortDateString()} в {name}",
                        Quantity = 1,
                        // ReSharper disable once PossibleInvalidOperationException
                        Price = (decimal)d.SUMM_ORD,
                        Summa = (decimal)d.SUMM_ORD,
                        Nom = null,
                        Nomenkl = null,
                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC))?.Name,
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC) as Currency,
                        // ReSharper disable PossibleInvalidOperationException
                        SummaEUR = d.CRS_DC.Value == CurrencyCode.EUR ? (decimal)d.SUMM_ORD : 0,
                        // ReSharper restore PossibleInvalidOperationException
                        SummaUSD = d.CRS_DC.Value == CurrencyCode.USD ? (decimal)d.SUMM_ORD : 0,
                        SummaRUB = d.CRS_DC.Value == CurrencyCode.RUB ? (decimal)d.SUMM_ORD : 0,
                        SummaCNY = d.CRS_DC.Value == CurrencyCode.CNY ? (decimal)d.SUMM_ORD : 0
                    });
                }

                foreach (var d in CashIn)
                {
                    if (d.RASH_ORDER_FROM_DC != null) continue;
                    if (ent.TD_101.Any(_ =>
                            _.VVT_KASS_PRIH_ORDER_DC == d.DOC_CODE && _.SD_101.VV_START_DATE <= CurrentDate)) continue;
                    // ReSharper disable once PossibleInvalidOperationException
                    var kontrName = ((IName)GlobalOptions.ReferencesCache.GetBankAccount(d.BANK_RASCH_SCHET_DC)).Name;
                    ExtendRows.Add(new ManagementBalanceExtendRowViewModel
                    {
                        DocCode = d.DOC_CODE,
                        GroupId = ManagemenentBalansStructrue.MoneyInPah,
                        // ReSharper disable once PossibleInvalidOperationException
                        Date = d.DATE_ORD.Value,
                        DocumentType = DocumentType.CashIn,
                        DocNum = d.NUM_ORD.ToString(),
                        KontragentName = kontrName,
                        Name =
                            $"Приходный кассовый ордер №{d.NUM_ORD} от {d.DATE_ORD.Value.ToShortDateString()} из {kontrName}",
                        Quantity = 1,
                        // ReSharper disable once PossibleInvalidOperationException
                        Price = (decimal)d.SUMM_ORD,
                        Summa = (decimal)d.SUMM_ORD,
                        Nom = null,
                        Nomenkl = null,
                        // ReSharper disable once PossibleInvalidOperationException
                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC)).Name,
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC) as Currency,
                        SummaEUR = d.CRS_DC.Value == CurrencyCode.EUR ? (decimal)d.SUMM_ORD : 0,
                        SummaUSD = d.CRS_DC.Value == CurrencyCode.USD ? (decimal)d.SUMM_ORD : 0,
                        SummaRUB = d.CRS_DC.Value == CurrencyCode.RUB ? (decimal)d.SUMM_ORD : 0,
                        SummaCNY = d.CRS_DC.Value == CurrencyCode.CNY ? (decimal)d.SUMM_ORD : 0
                    });
                }

                foreach (var d in BankOut)
                {
                    var tr = ent.TD_101.FirstOrDefault(_ => _.BankFromTransactionCode == d.CODE);
                    if (tr != null) continue;
                    // ReSharper disable once PossibleInvalidOperationException
                    var name = ((IName)GlobalOptions.ReferencesCache.GetBankAccount(d.BankAccountDC)).Name;
                    ExtendRows.Add(new ManagementBalanceExtendRowViewModel
                    {
                        DocCode = d.DOC_CODE,
                        Code = d.CODE,
                        GroupId = ManagemenentBalansStructrue.MoneyInPah,
                        Date = d.SD_101.VV_START_DATE,
                        DocumentType = DocumentType.Bank,
                        DocNum = "",
                        KontragentName = name,
                        Name = $"Банковская транзакция от {d.SD_101.VV_START_DATE.ToShortDateString()} в {name}",
                        Quantity = 1,
                        // ReSharper disable once PossibleInvalidOperationException
                        Price = (decimal)d.VVT_VAL_RASHOD,
                        Summa = (decimal)d.VVT_VAL_RASHOD,
                        Nom = null,
                        Nomenkl = null,
                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(d.VVT_CRS_DC)).Name,
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(d.VVT_CRS_DC) as Currency,
                        SummaEUR = d.VVT_CRS_DC == CurrencyCode.EUR ? (decimal)d.VVT_VAL_RASHOD : 0,
                        SummaUSD = d.VVT_CRS_DC == CurrencyCode.USD ? (decimal)d.VVT_VAL_RASHOD : 0,
                        SummaRUB = d.VVT_CRS_DC == CurrencyCode.RUB ? (decimal)d.VVT_VAL_RASHOD : 0,
                        SummaCNY = d.VVT_CRS_DC == CurrencyCode.CNY ? (decimal)d.VVT_VAL_RASHOD : 0
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
                                   CurrentCurrencyRate.GetRate(
                                       GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as Currency,
                                       RecalcCurrency) +
                                   b.SummaUSD * CurrentCurrencyRate.GetRate(
                                       GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as Currency,
                                       RecalcCurrency) +
                                   b.SummaGBP * CurrentCurrencyRate.GetRate(
                                       GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as Currency,
                                       RecalcCurrency) +
                                   b.SummaRUB * CurrentCurrencyRate.GetRate(
                                       GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as Currency,
                                       RecalcCurrency) +
                                   b.SummaCNY * CurrentCurrencyRate.GetRate(
                                       GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as Currency,
                                       RecalcCurrency);
            if (ExtendRowsActual.Count > 0 && RecalcCurrency != null && CurrentCurrencyRate != null)
                foreach (var b in ExtendRowsActual)
                {
                    b.PriceCalc = b.PriceEUR *
                                  CurrentCurrencyRate.GetRate(
                                      GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as Currency,
                                      RecalcCurrency) +
                                  b.PriceUSD *
                                  CurrentCurrencyRate.GetRate(
                                      GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as Currency,
                                      RecalcCurrency) +
                                  b.PriceGBP *
                                  CurrentCurrencyRate.GetRate(
                                      GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as Currency,
                                      RecalcCurrency) +
                                  b.PriceRUB *
                                  CurrentCurrencyRate.GetRate(
                                      GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as Currency,
                                      RecalcCurrency) +
                                  b.PriceCNY *
                                  CurrentCurrencyRate.GetRate(
                                      GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as Currency,
                                      RecalcCurrency);
                    b.SummaCalc = b.SummaEUR *
                                  CurrentCurrencyRate.GetRate(
                                      GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as Currency,
                                      RecalcCurrency) +
                                  b.SummaUSD *
                                  CurrentCurrencyRate.GetRate(
                                      GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as Currency,
                                      RecalcCurrency) +
                                  b.SummaGBP *
                                  CurrentCurrencyRate.GetRate(
                                      GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as Currency,
                                      RecalcCurrency) +
                                  b.SummaRUB *
                                  CurrentCurrencyRate.GetRate(
                                      GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as Currency,
                                      RecalcCurrency) +
                                  b.SummaCNY *
                                  CurrentCurrencyRate.GetRate(
                                      GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as Currency,
                                      RecalcCurrency);
                }

            RaisePropertyChanged(nameof(BalansStructure));
        }

        public override void DocumentOpen(object obj)
        {
            if (CurrentExtendItem == null) return;
            switch (CurrentExtendItem.DocumentType)
            {
                case DocumentType.CashIn:
                    DocumentsOpenManager.Open(DocumentType.CashIn, CurrentExtendItem.DocCode);
                    break;
                case DocumentType.CashOut:
                    DocumentsOpenManager.Open(DocumentType.CashOut, CurrentExtendItem.DocCode);
                    break;
            }
        }

        public override void SaveData(object obj)
        {
        }

        public async Task GetCashAsync()
        {
            var ret = new List<ManagementBalanceGroupViewModel>();
            var res = await Task.Run(_GetCash);
        }

        public void GetCash()
        {
            foreach (var row in _GetCash()) BalansStructure.Add(row);
        }

        private List<ManagementBalanceGroupViewModel> _GetCash()
        {
            var ret = new List<ManagementBalanceGroupViewModel>();
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
                    ret.Add(new ManagementBalanceGroupViewModel
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
                    ret.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaEUR);
                ch.SummaRUB =
                    ret.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaRUB);
                ch.SummaUSD =
                    ret.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaUSD);
                ch.SummaGBP =
                    ret.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaGBP);
                ch.SummaCHF =
                    ret.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaCHF);
                ch.SummaSEK =
                    ret.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaSEK);
                ch.SummaCNY =
                    ret.Where(_ => _.ParentId == ch.Id)
                        .Sum(_ => _.SummaCNY);
            }

            return ret;
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
            foreach (var d in GlobalOptions.ReferencesCache.GetBankAccountAll().Cast<BankAccount>()
                         .Select(_ => _.DocCode).Distinct())
            {
                var bank = GlobalOptions.ReferencesCache.GetBankAccount(d);
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
                        SummaEUR = ((IDocCode)bank.Currency).DocCode == CurrencyCode.EUR ? (decimal)rem.SummaEnd : 0,
                        SummaUSD = ((IDocCode)bank.Currency).DocCode == CurrencyCode.USD ? (decimal)rem.SummaEnd : 0,
                        SummaRUB = ((IDocCode)bank.Currency).DocCode == CurrencyCode.RUB ? (decimal)rem.SummaEnd : 0,
                        SummaGBP = ((IDocCode)bank.Currency).DocCode == CurrencyCode.GBP ? (decimal)rem.SummaEnd : 0,
                        SummaCHF = ((IDocCode)bank.Currency).DocCode == CurrencyCode.CHF ? (decimal)rem.SummaEnd : 0,
                        SummaSEK = ((IDocCode)bank.Currency).DocCode == CurrencyCode.SEK ? (decimal)rem.SummaEnd : 0,
                        SummaCNY = ((IDocCode)bank.Currency).DocCode == CurrencyCode.CNY ? (decimal)rem.SummaEnd : 0,
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
                var k = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC);
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
                    Name = ((IName)k).Name,
                    Summa =
                        d.Summa
                    //* GetRate(myRates, k.SD_301.DOC_CODE,GlobalOptions.SystemProfile.MainCurrency.DocCode,CurrentDate)
                    ,
                    CurrencyName = ((IName)k.Currency).Name,
                    SummaEUR = d.KontrCrsDC == CurrencyCode.EUR ? d.Summa : 0,
                    SummaUSD = d.KontrCrsDC == CurrencyCode.USD ? d.Summa : 0,
                    SummaRUB = d.KontrCrsDC == CurrencyCode.RUB ? d.Summa : 0,
                    SummaGBP = d.KontrCrsDC == CurrencyCode.GBP ? d.Summa : 0,
                    SummaCHF = d.KontrCrsDC == CurrencyCode.CHF ? d.Summa : 0,
                    SummaSEK = d.KontrCrsDC == CurrencyCode.SEK ? d.Summa : 0,
                    SummaCNY = d.KontrCrsDC == CurrencyCode.CNY ? d.Summa : 0,
                    Kontragent = k as Kontragent
                });
            }

            foreach (var d in data.Where(_ => _.Summa > 0))
            {
                var k = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC) as Kontragent;
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
                    CurrencyName = ((IName)k.Currency).Name,
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
            calcNomenklBefore();
            var ch = BalansStructure.Single(_ => _.Id == myBalansBuilder.Structure
                .Single(t => t.Tag == BalansSection.Store)
                .Id);
            ch.Summa = 0;
            var chNach = BalansStructure.Single(_ => _.Id == ch.Id);
            chNach.Summa = 0;
            foreach (var s in GlobalOptions.ReferencesCache.GetWarehousesAll())
            {
                //var n = GlobalOptions.ReferencesCache.GetWarehouse(s) as Warehouse;
                var newId = Guid.NewGuid();
                var data1 = nomenklManager.GetNomenklStoreQuantity(((IDocCode)s).DocCode, new DateTime(2000, 1, 1),
                    CurrentDate);
                var data = data1.Select(d => new NomenklQuantityInfoExt(d)).ToList();
                var newSklad = new ManagementBalanceGroupViewModel
                {
                    Id = newId,
                    ParentId = ch.Id,
                    Name = ((IName)s).Name,
                    Order = 1,
                    SummaRUB =
                        data.Where(_ => _.NomDC == CurrencyCode.RUB)
                            .Sum(_ => GetRound(_.OstatokSumma)),
                    SummaUSD =
                        data.Where(_ => _.NomDC == CurrencyCode.USD)
                            .Sum(_ => GetRound(_.OstatokSumma)),
                    SummaEUR =
                        data.Where(_ => _.NomDC == CurrencyCode.EUR)
                            .Sum(_ => GetRound(_.OstatokSumma)),
                    SummaGBP =
                        data.Where(_ => _.NomDC == CurrencyCode.GBP)
                            .Sum(_ => GetRound(_.OstatokSumma)),
                    SummaCHF =
                        data.Where(_ => _.NomDC == CurrencyCode.CHF)
                            .Sum(_ => GetRound(_.OstatokSumma)),
                    SummaSEK =
                        data.Where(_ => _.NomDC == CurrencyCode.SEK)
                            .Sum(_ => GetRound(_.OstatokSumma)),
                    SummaCNY =
                        data.Where(_ => _.NomDC == CurrencyCode.CNY)
                            .Sum(_ => GetRound(_.OstatokSumma)),
                    ObjectDC = ((IDocCode)s).DocCode
                };
                if (newSklad.CurrencyTotal == 0) continue;
                BalansStructure.Add(newSklad);
                foreach (var dd in data)
                {
                    if (dd.OstatokQuantity == 0) continue;
                    var nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(dd.NomDC) as Nomenkl;
                    ExtendRows.Add(new ManagementBalanceExtendRowViewModel
                    {
                        GroupId = newId,
                        Name = dd.NomName,
                        Quantity = dd.OstatokQuantity,
                        Price = dd.OstatokSumma / dd.OstatokQuantity,
                        Summa = dd.OstatokSumma,
                        Nom = nomenkl,
                        Nomenkl = nomenkl.NomenklNumber,
                        CurrencyName = ((IName)nomenkl.Currency).Name,
                        SummaEUR = ((IDocCode)nomenkl.Currency).DocCode == CurrencyCode.EUR ? dd.OstatokSumma : 0,
                        SummaUSD = ((IDocCode)nomenkl.Currency).DocCode == CurrencyCode.USD ? dd.OstatokSumma : 0,
                        SummaRUB = ((IDocCode)nomenkl.Currency).DocCode == CurrencyCode.RUB ? dd.OstatokSumma : 0,
                        SummaCNY = ((IDocCode)nomenkl.Currency).DocCode == CurrencyCode.CNY ? dd.OstatokSumma : 0
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
            calcNomenklAfter();
        }

        private void calcNomenklBefore()
        {
            using var ctx = GlobalOptions.GetEntities();
            var sql = $@"dbo.NomenklBeforeCalc '{CustomFormat.DateToString(CurrentDate)}'";
            ctx.Database.ExecuteSqlCommand($@"dbo.NomenklBeforeCalc '{CustomFormat.DateToString(CurrentDate)}'");
        }

        private void calcNomenklAfter()
        {
            using var ctx = GlobalOptions.GetEntities();
            ctx.Database.ExecuteSqlCommand($@"dbo.NomenklAfterCalc");
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
                    SummaEUR = ((IDocCode)per.Employee.Currency).DocCode == CurrencyCode.EUR ? -per.DolgSumma : 0,
                    SummaUSD = ((IDocCode)per.Employee.Currency).DocCode == CurrencyCode.USD ? -per.DolgSumma : 0,
                    SummaRUB = ((IDocCode)per.Employee.Currency).DocCode == CurrencyCode.RUB ? -per.DolgSumma : 0,
                    SummaGBP = ((IDocCode)per.Employee.Currency).DocCode == CurrencyCode.GBP ? -per.DolgSumma : 0,
                    SummaCHF = ((IDocCode)per.Employee.Currency).DocCode == CurrencyCode.CHF ? -per.DolgSumma : 0,
                    SummaSEK = ((IDocCode)per.Employee.Currency).DocCode == CurrencyCode.SEK ? -per.DolgSumma : 0,
                    SummaCNY = ((IDocCode)per.Employee.Currency).DocCode == CurrencyCode.CNY ? -per.DolgSumma : 0,
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
