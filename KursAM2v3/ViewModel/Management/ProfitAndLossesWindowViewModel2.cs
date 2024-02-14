using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Grid;
using FinanceAnalitic;
using Helper;
using KursAM2.Managers;
using KursAM2.View.Base;
using KursAM2.View.Finance;
using KursAM2.View.Management;
using KursAM2.View.Management.ProfitAndLossesControls;
using KursAM2.ViewModel.Finance;
using KursAM2.ViewModel.Logistiks;
using KursAM2.ViewModel.Logistiks.TransferOut;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Management;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository.DocHistoryRepository;
using KursDomain.Repository.NomenklRepository;
using KursDomain.Repository.StorageLocationsRepositury;
using KursDomain.Repository.TransferOut;
using static System.Math;

namespace KursAM2.ViewModel.Management
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class ProfitAndLossesWindowViewModel2 : RSWindowViewModelBase
    {
        private readonly List<Guid?> myTempIdList = new List<Guid?>();
        private readonly List<Guid?> myTempIdList2 = new List<Guid?>();


        private bool _IsDataLoaded;
        private ProfitAndLossesMainRowViewModel myBalansCalc;
        private ProfitAndLossesMainRowViewModel myBalansFact;
        private CurrencyConvertRow myCurrentCrsConvert;
        private CrossCurrencyRate myCurrentCurrencyRate;
        private ProfitAndLossesExtendRowViewModel myCurrentExtend;
        private DateTime myEndDate;
        private Currency myRecalcCurrency;
        private DateTime myStartDate;
        private int myTabSelected;

        public ProfitAndLossesWindowViewModel2()
        {
            WindowName = "Прибыли и убытки";
            LayoutName = "ProfitAndLossesWindowViewModel2";
            ExtendActual = new ObservableCollection<ProfitAndLossesExtendRowViewModel>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = GetRightMenu();
            RecalcCurrency = GlobalOptions.SystemProfile.NationalCurrency;
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as Currency);
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as Currency);
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as Currency);
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as Currency);
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as Currency);
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
            var crsrate = new CrossCurrencyRate();
            crsrate.SetRates(DateTime.Today);
            foreach (var c in crsrate.CurrencyList) CurrencyRates.Add(c);

            CurrentCurrencyRate = CurrencyRates[0];

            Manager = new ProfitAndLossesManager(this)
            {
                Main = Main,
                MainNach = MainNach,
                Extend = Extend,
                ExtendNach = ExtendNach,
                DateStart = StartDate,
                DateEnd = EndDate
            };
        }

        public ObservableCollection<ProfitAndLossesMainRowViewModel> Main { set; get; } =
            new ObservableCollection<ProfitAndLossesMainRowViewModel>(
                ProfitAndLossesMainRowViewModel.GetStructure());

        public ObservableCollection<ProfitAndLossesMainRowViewModel> MainNach { set; get; } =
            new ObservableCollection<ProfitAndLossesMainRowViewModel>(
                ProfitAndLossesMainRowViewModel.GetStructure());

        public ObservableCollection<ProfitAndLossesExtendRowViewModel> Extend { set; get; } =
            new ObservableCollection<ProfitAndLossesExtendRowViewModel>();

        public ObservableCollection<ProfitAndLossesExtendRowViewModel> ExtendNach { set; get; } =
            new ObservableCollection<ProfitAndLossesExtendRowViewModel>();

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<CurrencyConvertRow> CurrencyConvertRows { set; get; }
            = new ObservableCollection<CurrencyConvertRow>();

        public ProfitAndLossesManager Manager { set; get; }

        public ObservableCollection<GridControlSummaryItem> TotalSummaries { set; get; } =
            new ObservableCollection<GridControlSummaryItem>();

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<ProfitAndLossesExtendRowViewModel> ExtendActual { set; get; }

        public DateTime EndDate
        {
            get
            {
                if (myEndDate == DateTime.MinValue)
                    EndDate = DateTime.Today;
                return myEndDate;
            }
            set
            {
                if (myEndDate == value) return;
                myEndDate = value;
                if (myEndDate < StartDate)
                    StartDate = myEndDate;
                RaisePropertyChanged();
            }
        }

        public bool IsDataLoaded
        {
            get => _IsDataLoaded;
            set
            {
                if (_IsDataLoaded == value) return;
                _IsDataLoaded = value;
                RaisePropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get
            {
                if (myStartDate == DateTime.MinValue)
                    StartDate = DateTime.Today;
                return myStartDate;
            }
            set
            {
                if (myStartDate == value) return;
                myStartDate = value;
                if (myStartDate > EndDate)
                    EndDate = myStartDate;
                RaisePropertyChanged();
            }
        }

        public ICommand BalansCrossRateRecalcCommand
        {
            get
            {
                return new Command(BalansCrossRateRecalc, _ => RecalcCurrency != null && CurrentCurrencyRate != null);
            }
        }

        public ObservableCollection<CrossCurrencyRate> CurrencyRates { set; get; } =
            new ObservableCollection<CrossCurrencyRate>();

        public CrossCurrencyRate CurrentCurrencyRate
        {
            get => myCurrentCurrencyRate;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentCurrencyRate == value) return;
                myCurrentCurrencyRate = value;
                RaisePropertyChanged();
            }
        }

        public ICommand NomenklCalcCommand
        {
            get { return new Command(NomenklCalc, _ => CurrentExtend?.Nomenkl != null); }
        }

        public ProfitAndLossesExtendRowViewModel CurrentExtend
        {
            get => myCurrentExtend;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentExtend == value) return;
                myCurrentExtend = value;
                if (myCurrentExtend == null) return;
                RaisePropertyChanged();
            }
        }

        public CurrencyConvertRow CurrentCrsConvert
        {
            get => myCurrentCrsConvert;
            set
            {
                if (myCurrentCrsConvert == value) return;
                myCurrentCrsConvert = value;
                RaisePropertyChanged();
            }
        }

        public int TabSelected
        {
            get => myTabSelected;
            set
            {
                if (myTabSelected == value) return;
                myTabSelected = value;
                ExtendActual.Clear();
                RaisePropertyChanged(nameof(ExtendActual));
                RaisePropertyChanged();
                if (TabSelected == 0)
                    RaisePropertyChanged(nameof(Main));
                if (TabSelected == 1)
                    RaisePropertyChanged(nameof(MainNach));
            }
        }

        public ProfitAndLossesMainRowViewModel BalansFact
        {
            get => myBalansFact;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myBalansFact == value) return;
                myBalansFact = value;
                var frm = Form as ProfitAndLosses2;
                if (myBalansFact != null)
                {
                    switch (myBalansFact.Id.ToString())
                    {
                        case "459937df-085f-4825-9ae9-810b054d0276":
                        case "30e9bd73-9bda-4d75-b897-332f9210b9b1":
                            frm?.NavigateTo(typeof(ProfitAndLossExtendVzaimozchetUI));
                            break;
                        case "b6f2540a-9593-42e3-b34f-8c0983bc39a2":
                        case "35ebabec-eac3-4c3c-8383-6326c5d64c8c":
                            frm?.NavigateTo(typeof(CurrencyConvertView));
                            UpdateCurrencyConvert(StartDate, EndDate);
                            break;
                        case "35c9783e-e19f-452b-8479-d6f022444552":
                            frm?.NavigateTo(typeof(CurrencyConvertView));
                            UpdateBalansOper(StartDate, EndDate);
                            break;
                        default:
                            frm?.NavigateTo(typeof(ProfitAndLossExtendBaseUI));
                            break;
                    }

                    UpdateExtend(myBalansFact.Id);
                }

                RaisePropertyChanged();
            }
        }

        public ProfitAndLossesMainRowViewModel BalansCalc
        {
            get => myBalansCalc;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myBalansCalc == value) return;
                myBalansCalc = value;
                var frm = Form as ProfitAndLosses2;
                if (myBalansCalc != null)
                {
                    switch (myBalansCalc.Id.ToString())
                    {
                        case "459937df-085f-4825-9ae9-810b054d0276":
                        case "30e9bd73-9bda-4d75-b897-332f9210b9b1":
                            frm?.NavigateTo(typeof(ProfitAndLossExtendVzaimozchetUI));
                            break;
                        case "b6f2540a-9593-42e3-b34f-8c0983bc39a2":
                        case "35ebabec-eac3-4c3c-8383-6326c5d64c8c":
                            frm?.NavigateTo(typeof(CurrencyConvertView));
                            UpdateCurrencyConvert(StartDate, EndDate);
                            break;
                        case "35c9783e-e19f-452b-8479-d6f022444552":
                            frm?.NavigateTo(typeof(CurrencyConvertView));
                            UpdateBalansOper(StartDate, EndDate);
                            break;
                        default:
                            frm?.NavigateTo(typeof(ProfitAndLossExtendBaseUI));
                            break;
                    }
                    //if (myBalansCalc.Id == Guid.Parse("{459937df-085f-4825-9ae9-810b054d0276}")
                    //    || myBalansCalc.Id == Guid.Parse("{30e9bd73-9bda-4d75-b897-332f9210b9b1}"))

                    //else


                    UpdateExtend(myBalansCalc.Id);
                }

                // ReSharper disable once PossibleNullReferenceException1965569UpdateExtend2(myBalansCalc.Id);
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once CollectionNeverQueried.Global
        public List<Currency> CurrenciesForRecalc { set; get; } = new List<Currency>();

        public Currency RecalcCurrency
        {
            get => myRecalcCurrency;
            set
            {
                if (Equals(myRecalcCurrency, value)) return;
                myRecalcCurrency = value;
                if (Form is ProfitAndLosses2 frm)
                {
                    var b = frm.treeListMain.Bands.FirstOrDefault(
                        _ => _.Columns.Any(c => c.FieldName == "RecalcResult"));
                    // ReSharper disable once PossibleNullReferenceException
                    b.Header = $"Пересчет в {RecalcCurrency?.Name}";
                }

                RaisePropertyChanged();
            }
        }

        public ICommand ExtendInfoCommand
        {
            get { return new Command(ExtendInfo, _ => IsActConvert()); }
        }

        public ICommand ExtendInfo2Command
        {
            get { return new Command(ExtendInfo2, _ => IsActConvert2()); }
        }

        public override bool IsDocumentOpenAllow =>
            CurrentExtend != null; // && IsDocumentOpen(CurrentExtend.DocTypeCode);

        public ICommand ActConvertOpenCommand
        {
            get { return new Command(ActConvertOpen, _ => CurrentCrsConvert != null); }
        }

        private void UpdateBalansOper(DateTime dateStart, DateTime dateEnd)
        {
            CurrencyConvertRows.Clear();
            UpdateCurrencyConvert(dateStart, dateEnd, false);
        }

        private void UpdateCurrencyConvert(DateTime dateStart, DateTime dateEnd, bool isCurrencyOnly = true)
        {
            if (isCurrencyOnly)
                CurrencyConvertRows.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                //var sql =
                //    "SELECT s110.DOC_CODE as DocCode, cast(s110.VZ_NUM as varchar) as DocNum, VZ_DATE as DocDate, " +
                //    "CREATOR as Creator, " +
                //    "s110.CurrencyFromDC as CurrencyFromDC, s110.CurrencyToDC as CurrencyToDC," +
                //    "SUM(CASE WHEN t110.VZT_1MYDOLZH_0NAMDOLZH = 0 THEN t110.VZT_CRS_SUMMA ELSE 0 END) AS FromSumma, " +
                //    "SUM(CASE WHEN t110.VZT_1MYDOLZH_0NAMDOLZH = 1 THEN t110.VZT_CRS_SUMMA ELSE 0 END) AS ToSumma " +
                //    "FROM TD_110 t110 " +
                //    "INNER JOIN SD_110 s110  ON t110.DOC_CODE = s110.DOC_CODE " +
                //    "INNER JOIN SD_111 s111  ON s110.VZ_TYPE_DC = s111.DOC_CODE AND s111.IsCurrencyConvert = 1 " +
                //    $"WHERE s110.VZ_DATE >= '{Helper.CustomFormat.DateToString(dateStart)}' " +
                //    $"AND s110.VZ_DATE <= '{Helper.CustomFormat.DateToString(dateEnd)}' " +
                //    "GROUP BY s110.DOC_CODE, s110.VZ_NUM, VZ_DATE, s110.CREATOR, s110.CurrencyFromDC, s110.CurrencyToDC";

                var sql = $@"SELECT
                      s110.DOC_CODE AS DocCode
                      ,'Акт конвертации' AS DocName
                     ,CAST(s110.VZ_NUM AS VARCHAR) AS DocNum
                     ,VZ_DATE AS DocDate
                     ,CREATOR AS Creator
                     ,s110.CurrencyFromDC AS CurrencyFromDC
                     ,s110.CurrencyToDC AS CurrencyToDC
                     ,SUM(CASE
                        WHEN t110.VZT_1MYDOLZH_0NAMDOLZH = 0 THEN t110.VZT_CRS_SUMMA
                        ELSE 0
                      END) AS FromSumma
                     ,SUM(CASE
                        WHEN t110.VZT_1MYDOLZH_0NAMDOLZH = 1 THEN t110.VZT_CRS_SUMMA
                        ELSE 0
                      END) AS ToSumma,
                      NULL AS BankRowCode
                    FROM TD_110 t110
                    INNER JOIN SD_110 s110
                      ON t110.DOC_CODE = s110.DOC_CODE
                    INNER JOIN SD_111 s111
                      ON s110.VZ_TYPE_DC = s111.DOC_CODE
                        AND s111.IsCurrencyConvert = 1
                    WHERE s110.VZ_DATE >= '{CustomFormat.DateToString(dateStart)}'
                    AND s110.VZ_DATE <= '{CustomFormat.DateToString(dateEnd)}'
                    GROUP BY s110.DOC_CODE
                            ,s110.VZ_NUM
                            ,VZ_DATE
                            ,s110.CREATOR
                            ,s110.CurrencyFromDC
                            ,s110.CurrencyToDC
                    union all
                    SELECT cast(0 AS numeric(18,0)) AS DocCode
                    ,'Банковская конвертация' AS DocName,
                    '' AS DocNum,
                    bcc.DocDate AS DocDate
                    ,bcc.CREATOR AS Creator
                    ,bcc.CrsFromDC AS CurrencyFromDC
                    ,bcc.CrsToDC AS CurrencyToDC
                    ,bcc.SummaFrom AS FromSumma
                    ,bcc.SummaTo AS ToSumma
                    ,bcc.DocRowToCode 
                    FROM BankCurrencyChange bcc
                        WHERE bcc.DocDate >= '{CustomFormat.DateToString(dateStart)}' 
                            AND bcc.DocDate <= '{CustomFormat.DateToString(dateEnd)}' 
                    UNION all
                    SELECT DOC_CODE,
                    'Касса'
                    ,cast(CH_NUM_ORD AS varchar)
                    ,CH_DATE
                    ,CREATOR
                    ,CH_CRS_OUT_DC
                    ,CH_CRS_IN_DC
                    ,CH_CRS_OUT_SUM
                    ,CH_CRS_IN_SUM
                    ,null
                    from sd_251
                        WHERE CH_DATE >= '{CustomFormat.DateToString(dateStart)}' 
                            AND CH_date <= '{CustomFormat.DateToString(dateEnd)}' ";
                var data = ctx.Database.SqlQuery<CurrencyConvertRow>(sql);
                foreach (var d in data)
                {
                    d.Operation = $"{GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyFromDC)} -> " +
                                  $"{GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyToDC)}";
                    if (d.ToSumma > 0)
                    {
                        if (d.CurrencyFromDC == GlobalOptions.SystemProfile.NationalCurrency.DocCode)
                        {
                            d.Rate = d.FromSumma / d.ToSumma;
                        }
                        else
                        {
                            if (d.CurrencyToDC == GlobalOptions.SystemProfile.NationalCurrency.DocCode)
                                d.Rate = d.ToSumma / d.FromSumma;
                            else
                                d.Rate = d.FromSumma / d.ToSumma;
                        }
                    }
                    else
                    {
                        d.Rate = 1;
                    }

                    ProfitAndLossesManager.SetCurrenciesValue(d, d.CurrencyToDC, d.ToSumma, 0);
                    ProfitAndLossesManager.SetCurrenciesValue(d, d.CurrencyFromDC, 0, d.FromSumma);
                    CurrencyConvertRows.Add(d);
                }
            }
        }

        private bool IsActConvert()
        {
            return BalansFact != null && (BalansFact.Id == Guid.Parse("{B6F2540A-9593-42E3-B34F-8C0983BC39A2}")
                                          || BalansFact.Id == Guid.Parse("{35EBABEC-EAC3-4C3C-8383-6326C5D64C8C}"));
        }

        private void ExtendInfo(object obj)
        {
            var ctx = new MutualAcountingInfoWindowViewModel
            {
                DateStart = StartDate,
                DateEnd = EndDate
            };
            ctx.RefreshData(null);
            var form = new MutualAccountingInfo
            {
                DataContext = ctx
            };
            form.Show();
        }

        private bool IsActConvert2()
        {
            return BalansCalc != null && (BalansCalc.Id == Guid.Parse("{B6F2540A-9593-42E3-B34F-8C0983BC39A2}")
                                          || BalansCalc.Id == Guid.Parse("{35EBABEC-EAC3-4C3C-8383-6326C5D64C8C}"));
        }

        private void ExtendInfo2(object obj)
        {
            var ctx = new MutualAcountingInfoWindowViewModel
            {
                DateStart = StartDate,
                DateEnd = EndDate
            };
            ctx.RefreshData(null);
            var form = new MutualAccountingInfo
            {
                DataContext = ctx
            };
            form.Show();
        }

        private void NomenklCalc(object obj)
        {
            if (CurrentExtend?.Nomenkl?.DocCode == null) return;
            // ReSharper disable once PossibleInvalidOperationException
            // ReSharper disable once PossibleNullReferenceException
            var ctx = new NomPriceWindowViewModel((decimal)CurrentExtend?.Nomenkl?.DocCode);
            var dlg = new SelectDialogView { DataContext = ctx };
            dlg.ShowDialog();
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

        //private void ResetCurrencyDetailColumns()
        //{
        //    if (Form is ProfitAndLosses frm)
        //        foreach (var column in frm.GridControlExtend.Columns)
        //        {
        //            GridControlBand b;
        //            switch (column.FieldName)
        //            {
        //                case "ProfitEUR":
        //                    b =
        //                        frm.GridControlExtend.Bands.FirstOrDefault(
        //                            _ => _.Columns.Any(c => c.FieldName == "ProfitEUR"));
        //                    if (b != null)
        //                        b.Visible = ExtendActual.Sum(_ => _.ProfitEUR) != 0 ||
        //                                    ExtendActual.Sum(_ => _.LossEUR) != 0;
        //                    break;
        //                case "ProfitUSD":
        //                    b =
        //                        frm.GridControlExtend.Bands.FirstOrDefault(
        //                            _ => _.Columns.Any(c => c.FieldName == "ProfitUSD"));
        //                    if (b != null)
        //                        b.Visible = ExtendActual.Sum(_ => _.ProfitUSD) != 0 ||
        //                                    ExtendActual.Sum(_ => _.LossUSD) != 0;
        //                    break;
        //                case "ProfitCNY":
        //                    b =
        //                        frm.GridControlExtend.Bands.FirstOrDefault(
        //                            _ => _.Columns.Any(c => c.FieldName == "ProfitCNY"));
        //                    if (b != null)
        //                        b.Visible = ExtendActual.Sum(_ => _.ProfitCNY) != 0 ||
        //                                    ExtendActual.Sum(_ => _.LossCNY) != 0;
        //                    break;
        //                case "ProfitRUB":
        //                    b =
        //                        frm.GridControlExtend.Bands.FirstOrDefault(
        //                            _ => _.Columns.Any(c => c.FieldName == "ProfitRUB"));
        //                    if (b != null)
        //                        b.Visible = ExtendActual.Sum(_ => _.ProfitRUB) != 0 ||
        //                                    ExtendActual.Sum(_ => _.LossRUB) != 0;
        //                    break;
        //                case "ProfitGBP":
        //                    b =
        //                        frm.GridControlExtend.Bands.FirstOrDefault(
        //                            _ => _.Columns.Any(c => c.FieldName == "ProfitGBP"));
        //                    if (b != null)
        //                        b.Visible = ExtendActual.Sum(_ => _.ProfitGBP) != 0 ||
        //                                    ExtendActual.Sum(_ => _.LossGBP) != 0;
        //                    break;
        //                case "ProfitCHF":
        //                    b =
        //                        frm.GridControlExtend.Bands.FirstOrDefault(
        //                            _ => _.Columns.Any(c => c.FieldName == "ProfitCHF"));
        //                    if (b != null)
        //                        b.Visible = ExtendActual.Sum(_ => _.ProfitCHF) != 0 ||
        //                                    ExtendActual.Sum(_ => _.LossCHF) != 0;
        //                    break;
        //                case "ProfitSEK":
        //                    b =
        //                        frm.GridControlExtend.Bands.FirstOrDefault(
        //                            _ => _.Columns.Any(c => c.FieldName == "ProfitSEK"));
        //                    if (b != null)
        //                        b.Visible = ExtendActual.Sum(_ => _.ProfitSEK) != 0 ||
        //                                    ExtendActual.Sum(_ => _.LossSEK) != 0;
        //                    break;
        //            }
        //        }
        //}

        public void UpdateExtend2(Guid id)
        {
            ExtendActual.Clear();
            if (myBalansCalc.Id == Guid.Parse("{459937df-085f-4825-9ae9-810b054d0276}")
                || myBalansCalc.Id == Guid.Parse("{30e9bd73-9bda-4d75-b897-332f9210b9b1}"))
                foreach (var d in Manager.ExtendNach.Where(d => d.GroupId == id))
                {
                    var f = ExtendActual.FirstOrDefault(_ => _.DocCode == d.DocCode);
                    if (f == null)
                    {
                        d.VzaimozachetInfo = new ObservableCollection<VzaimozachetRow>
                        {
                            new VzaimozachetRow
                            {
                                Kontragent = d.KontragentBase?.Name,
                                CurrencyName = d.CurrencyName,
                                Note = d.Note,
                                SDRSchet = d.SDRSchet,
                                Summa = d.ResultCHF + d.ResultEUR + d.ResultGBP + d.ResultRUB + d.ResultSEK +
                                        d.ResultUSD + d.ResultCNY,
                                ProfitCHF = d.ProfitCHF,
                                ProfitEUR = d.ProfitEUR,
                                ProfitGBP = d.ProfitGBP,
                                ProfitRUB = d.ProfitRUB,
                                ProfitSEK = d.ProfitSEK,
                                ProfitUSD = d.ProfitUSD,
                                ProfitCNY = d.ProfitCNY,
                                LossCHF = d.LossCHF,
                                LossEUR = d.LossEUR,
                                LossGBP = d.LossGBP,
                                LossRUB = d.LossRUB,
                                LossSEK = d.LossSEK,
                                LossUSD = d.LossUSD,
                                LossCNY = d.LossCNY,
                                ResultCHF = d.ResultCHF,
                                ResultEUR = d.ResultEUR,
                                ResultGBP = d.ResultGBP,
                                ResultRUB = d.ResultRUB,
                                ResultSEK = d.ResultSEK,
                                ResultUSD = d.ResultUSD,
                                ResultCNY = d.ResultCNY
                            }
                        };
                        ExtendActual.Add(ProfitAndLossesExtendRowViewModel.GetCopy(d));
                    }
                    else
                    {
                        f.VzaimozachetInfo.Add(new VzaimozachetRow
                        {
                            Kontragent = d.KontragentBase?.Name,
                            CurrencyName = d.CurrencyName,
                            Note = d.Note,
                            SDRSchet = d.SDRSchet,
                            Summa = d.ResultCHF + d.ResultEUR + d.ResultGBP + d.ResultRUB + d.ResultSEK +
                                    d.ResultUSD + d.ResultCNY,
                            ProfitCHF = d.ProfitCHF,
                            ProfitEUR = d.ProfitEUR,
                            ProfitGBP = d.ProfitGBP,
                            ProfitRUB = d.ProfitRUB,
                            ProfitSEK = d.ProfitSEK,
                            ProfitUSD = d.ProfitUSD,
                            ProfitCNY = d.ProfitCNY,
                            LossCHF = d.LossCHF,
                            LossEUR = d.LossEUR,
                            LossGBP = d.LossGBP,
                            LossRUB = d.LossRUB,
                            LossSEK = d.LossSEK,
                            LossUSD = d.LossUSD,
                            LossCNY = d.LossCNY,
                            ResultCHF = d.ResultCHF,
                            ResultEUR = d.ResultEUR,
                            ResultGBP = d.ResultGBP,
                            ResultRUB = d.ResultRUB,
                            ResultSEK = d.ResultSEK,
                            ResultUSD = d.ResultUSD,
                            ResultCNY = d.ResultCNY
                        });
                        f.ProfitCHF += d.ProfitCHF;
                        f.ProfitEUR += d.ProfitEUR;
                        f.ProfitGBP += d.ProfitGBP;
                        f.ProfitRUB += d.ProfitRUB;
                        f.ProfitSEK += d.ProfitSEK;
                        f.ProfitUSD += d.ProfitUSD;
                        f.ProfitCNY += d.ProfitCNY;
                        f.LossCHF += d.LossCHF;
                        f.LossEUR += d.LossEUR;
                        f.LossGBP += d.LossGBP;
                        f.LossRUB += d.LossRUB;
                        f.LossSEK += d.LossSEK;
                        f.LossUSD += d.LossUSD;
                        f.LossCNY += d.LossCNY;
                        f.ResultCHF = f.VzaimozachetInfo.Sum(_ => _.ProfitCHF - _.LossCHF);
                        f.ResultEUR = f.VzaimozachetInfo.Sum(_ => _.ProfitEUR - _.LossEUR);
                        f.ResultGBP = f.VzaimozachetInfo.Sum(_ => _.ProfitGBP - _.LossGBP);
                        f.ResultRUB = f.VzaimozachetInfo.Sum(_ => _.ProfitRUB - _.LossRUB);
                        f.ResultSEK = f.VzaimozachetInfo.Sum(_ => _.ProfitSEK - _.LossSEK);
                        f.ResultUSD = f.VzaimozachetInfo.Sum(_ => _.ProfitUSD - _.LossUSD);
                        f.ResultCNY = f.VzaimozachetInfo.Sum(_ => _.ProfitCNY - _.LossCNY);
                    }
                }
            else
                foreach (var d in Manager.ExtendNach.Where(d => d.GroupId == id))
                    ExtendActual.Add(d);

            if (id == Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}") ||
                id == Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}") ||
                id == Guid.Parse("{2FA1DD9F-6842-4209-B0CC-DDEF3B920496}") ||
                id == Guid.Parse("{E47EF726-3BEA-4B18-9773-E564D624FDF6}"))
                foreach (var id2 in Manager.MainNach.Where(_ => _.ParentId == id).Select(_ => _.Id))
                foreach (var d in Manager.ExtendNach.Where(d => d.GroupId == id2))
                    ExtendActual.Add(d);

            //ResetCurrencyDetailColumns();
        }

        public void UpdateExtend2()
        {
            ExtendActual.Clear();
            if (BalansFact == null) return;
            foreach (var d in Manager.Extend.Where(d => d.GroupId == BalansFact.Id))
                ExtendActual.Add(d);
            if (BalansFact.Id == Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}") ||
                BalansFact.Id == Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}"))
                foreach (var id in Main.Where(_ => _.ParentId == BalansFact.Id).Select(_ => _.Id))
                foreach (var d in Manager.Extend.Where(d => d.GroupId == id))
                    ExtendActual.Add(d);

            //ResetCurrencyDetailColumns();
            RaisePropertyChanged(nameof(ExtendActual));
        }

        public void UpdateExtend(Guid id)
        {
            ExtendActual.Clear();
            if (myBalansFact?.Id == Guid.Parse("{459937df-085f-4825-9ae9-810b054d0276}")
                || myBalansFact?.Id == Guid.Parse("{30e9bd73-9bda-4d75-b897-332f9210b9b1}"))
            {
                foreach (var d in Manager.ExtendNach.Where(d => d.GroupId == id))
                {
                    var f = ExtendActual.FirstOrDefault(_ => _.DocCode == d.DocCode);
                    if (f == null)
                    {
                        d.VzaimozachetInfo = new ObservableCollection<VzaimozachetRow>
                        {
                            new VzaimozachetRow
                            {
                                Kontragent = d.KontragentBase?.Name,
                                CurrencyName = d.CurrencyName,
                                Note = d.Note,
                                SDRSchet = d.SDRSchet,
                                Summa = d.ResultCHF + d.ResultEUR + d.ResultGBP + d.ResultRUB + d.ResultSEK +
                                        d.ResultUSD + d.ResultCNY,
                                ProfitCHF = d.ProfitCHF,
                                ProfitEUR = d.ProfitEUR,
                                ProfitGBP = d.ProfitGBP,
                                ProfitRUB = d.ProfitRUB,
                                ProfitSEK = d.ProfitSEK,
                                ProfitUSD = d.ProfitUSD,
                                ProfitCNY = d.ProfitCNY,
                                LossCHF = d.LossCHF,
                                LossEUR = d.LossEUR,
                                LossGBP = d.LossGBP,
                                LossRUB = d.LossRUB,
                                LossSEK = d.LossSEK,
                                LossUSD = d.LossUSD,
                                LossCNY = d.LossCNY,
                                ResultCHF = d.ResultCHF,
                                ResultEUR = d.ResultEUR,
                                ResultGBP = d.ResultGBP,
                                ResultRUB = d.ResultRUB,
                                ResultSEK = d.ResultSEK,
                                ResultUSD = d.ResultUSD,
                                ResultCNY = d.ResultCNY
                            }
                        };
                        ExtendActual.Add(ProfitAndLossesExtendRowViewModel.GetCopy(d));
                    }
                    else
                    {
                        f.VzaimozachetInfo.Add(new VzaimozachetRow
                        {
                            Kontragent = d.KontragentBase?.Name,
                            CurrencyName = d.CurrencyName,
                            Note = d.Note,
                            SDRSchet = d.SDRSchet,
                            Summa = d.ResultCHF + d.ResultEUR + d.ResultGBP + d.ResultRUB + d.ResultSEK +
                                    d.ResultUSD + d.ResultCNY,
                            ProfitCHF = d.ProfitCHF,
                            ProfitEUR = d.ProfitEUR,
                            ProfitGBP = d.ProfitGBP,
                            ProfitRUB = d.ProfitRUB,
                            ProfitSEK = d.ProfitSEK,
                            ProfitUSD = d.ProfitUSD,
                            ProfitCNY = d.ProfitCNY,
                            LossCHF = d.LossCHF,
                            LossEUR = d.LossEUR,
                            LossGBP = d.LossGBP,
                            LossRUB = d.LossRUB,
                            LossSEK = d.LossSEK,
                            LossUSD = d.LossUSD,
                            LossCNY = d.LossCNY,
                            ResultCHF = d.ResultCHF,
                            ResultEUR = d.ResultEUR,
                            ResultGBP = d.ResultGBP,
                            ResultRUB = d.ResultRUB,
                            ResultSEK = d.ResultSEK,
                            ResultUSD = d.ResultUSD,
                            ResultCNY = d.ResultCNY
                        });
                        f.ProfitCHF += d.ProfitCHF;
                        f.ProfitEUR += d.ProfitEUR;
                        f.ProfitGBP += d.ProfitGBP;
                        f.ProfitRUB += d.ProfitRUB;
                        f.ProfitSEK += d.ProfitSEK;
                        f.ProfitUSD += d.ProfitUSD;
                        f.ProfitCNY += d.ProfitCNY;
                        f.LossCHF += d.LossCHF;
                        f.LossEUR += d.LossEUR;
                        f.LossGBP += d.LossGBP;
                        f.LossRUB += d.LossRUB;
                        f.LossSEK += d.LossSEK;
                        f.LossUSD += d.LossUSD;
                        f.LossCNY += d.LossCNY;
                        f.ResultCHF += d.ResultCHF;
                        f.ResultEUR += d.ResultEUR;
                        f.ResultGBP += d.ResultGBP;
                        f.ResultRUB += d.ResultRUB;
                        f.ResultSEK += d.ResultSEK;
                        f.ResultUSD += d.ResultUSD;
                        f.ResultCNY += d.ResultCNY;
                    }
                }
            }
            else
            {
                foreach (var d in Manager.Extend.Where(d => d.GroupId == id))
                    ExtendActual.Add(d);
                if (id == Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}") ||
                    id == Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}") ||
                    id == Guid.Parse("{2FA1DD9F-6842-4209-B0CC-DDEF3B920496}") ||
                    id == Guid.Parse("{E47EF726-3BEA-4B18-9773-E564D624FDF6}")
                   )
                    foreach (var id2 in Main.Where(_ => _.ParentId == id).Select(_ => _.Id))
                    foreach (var d in Manager.Extend.Where(d => d.GroupId == id2))
                        ExtendActual.Add(d);
            }

            //ResetCurrencyDetailColumns();
        }

        public void UpdateExtend()
        {
            ExtendActual.Clear();
            if (BalansCalc == null) return;
            foreach (var d in Manager.ExtendNach.Where(d => d.GroupId == BalansCalc.Id))
                ExtendActual.Add(d);
            if (BalansCalc.Id == Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}") ||
                BalansCalc.Id == Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}"))
                foreach (var id in MainNach.Where(_ => _.ParentId == BalansCalc.Id).Select(_ => _.Id))
                foreach (var d in Manager.ExtendNach.Where(d => d.GroupId == id))
                    ExtendActual.Add(d);

            //ResetCurrencyDetailColumns();
            RaisePropertyChanged(nameof(ExtendActual));
        }

        private void ActConvertOpen(object obj)
        {
            if (CurrentCrsConvert.BankRowCode != null)
                DocumentsOpenManager.Open(DocumentType.Bank, (decimal)CurrentCrsConvert.BankRowCode);
            else
                DocumentsOpenManager.Open(
                    CurrentCrsConvert.DocName == "Касса"
                        ? DocumentType.CurrencyChange
                        : DocumentType.CurrencyConvertAccounting, CurrentCrsConvert.DocCode);
        }

        public override void DocumentOpen(object obj)
        {
            switch (CurrentExtend.DocTypeCode)
            {
                case DocumentType.Bank:
                    DocumentsOpenManager.Open(CurrentExtend.DocTypeCode, CurrentExtend.Code);
                    return;
                case DocumentType.AccruedAmountForClient:
                case DocumentType.AccruedAmountOfSupplier:
                case DocumentType.AktSpisaniya:
                {
                    var id = Guid.Parse(CurrentExtend.StringId);
                    DocumentsOpenManager.Open(CurrentExtend.DocTypeCode, 0, id);
                    return;
                }
                case DocumentType.PayRollVedomost:
                    DocumentsOpenManager.Open(CurrentExtend.DocTypeCode, 0, Guid.Parse(CurrentExtend.StringId));
                    return;
                case DocumentType.Waybill:
                    if (CurrentExtend.DocumentDC != null)
                        DocumentsOpenManager.Open(CurrentExtend.DocTypeCode, CurrentExtend.DocumentDC.Value);
                    break;
                case DocumentType.StockHolderAccrual:
                    DocumentsOpenManager.Open(DocumentType.StockHolderAccrual, 0, CurrentExtend.Id);
                    break;
                case DocumentType.TransferOutBalans:
                    var ctx = GlobalOptions.GetEntities();
                    var doc = new TransferOutBalansViewModel(new TransferOutBalansRepository(ctx),
                        new StorageLocationsRepository(ctx), new NomenklRepository(ctx), new DocHistoryRepository(ctx)); 
                    doc.Show();
                    doc.Initialize(CurrentExtend.Id);
                    break;
                default:
                    DocumentsOpenManager.Open(CurrentExtend.DocTypeCode, CurrentExtend.DocCode);
                    break;
            }
        }

        public class NakladTemp
        {
            public decimal DocCode { set; get; }
            public Guid Id { set; get; }
            public DateTime Date { set; get; }
            public decimal NomenklDC { set; get; }
            public string Name { set; get; }

            // ReSharper disable once MergeConditionalExpression
            public string COName { set; get; }

            /// <summary>
            ///     Вспомогательная переменна для перевода вещемтвенного
            ///     числа в десятично в запросах к EF
            /// </summary>
            public double DoubleQuantity { set; get; }

            public decimal Quantity { set; get; }
            public decimal? Summa { set; get; }
            public decimal? KontrCrsDC { set; get; }
            public string Kontragent { set; get; }
            public decimal KontragentDC { set; get; }
            public decimal? SfCrsDC { get; set; }
            public double? SfUchCurrencyRate { get; set; }
            public decimal? SDRSchetDC { get; set; }
            public bool IsNaklad { set; get; }
        }

        #region Calc

        private void BalansCrossRateRecalc(object obj)
        {
            if (RecalcCurrency == null || CurrentCurrencyRate == null) return;
            if (Main.Count > 0 && RecalcCurrency != null && CurrentCurrencyRate != null)
            {
                foreach (var b in Main)
                {
                    b.RecalcProfit = Round(b.ProfitRUB *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitUSD *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitEUR *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitGBP *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitCHF *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CHF) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitSEK *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.SEK) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitCNY *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as Currency,
                                               RecalcCurrency), 2);
                    b.RecalcLoss = Round(b.LossRUB *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as Currency,
                                             RecalcCurrency) +
                                         b.LossUSD *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as Currency,
                                             RecalcCurrency) +
                                         b.LossEUR *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as Currency,
                                             RecalcCurrency) +
                                         b.LossGBP *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as Currency,
                                             RecalcCurrency) +
                                         b.LossCHF *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CHF) as Currency,
                                             RecalcCurrency) +
                                         b.LossCNY *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as Currency,
                                             RecalcCurrency) +
                                         b.LossSEK *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.SEK) as Currency,
                                             RecalcCurrency), 2);
                    b.RecalcResult = b.RecalcProfit - b.RecalcLoss;
                }

                foreach (var b in MainNach)
                {
                    b.RecalcProfit = Round(b.ProfitRUB *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitUSD *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitEUR *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitGBP *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitCHF *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CHF) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitCNY *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as Currency,
                                               RecalcCurrency) +
                                           b.ProfitSEK *
                                           CurrentCurrencyRate.GetRate(
                                               GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.SEK) as Currency,
                                               RecalcCurrency), 2);
                    b.RecalcLoss = Round(b.LossRUB *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as Currency,
                                             RecalcCurrency) +
                                         b.LossUSD *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as Currency,
                                             RecalcCurrency) +
                                         b.LossEUR *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as Currency,
                                             RecalcCurrency) +
                                         b.LossGBP *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as Currency,
                                             RecalcCurrency) +
                                         b.LossCHF *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CHF) as Currency,
                                             RecalcCurrency) +
                                         b.LossCNY *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as Currency,
                                             RecalcCurrency) +
                                         b.LossSEK *
                                         CurrentCurrencyRate.GetRate(
                                             GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.SEK) as Currency,
                                             RecalcCurrency), 2);
                    b.RecalcResult = Round(b.RecalcProfit - b.RecalcLoss, 2);
                }
            }
        }

        private void SummaNode(Guid? id)
        {
            if (id == null) return;
            var m = Main.Single(_ => _.Id == id);
            if (id == Guid.Parse("{E7DA6232-CAA0-4358-9BAE-5D96C2EE248A}"))
            {
                m.ProfitRUB = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitRUB), 2);
                m.LossRUB = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.LossRUB), 2);
                m.ResultRUB = Round(m.ProfitRUB - m.LossRUB, 2);
                m.ProfitUSD = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitUSD), 2);
                m.LossUSD = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.LossUSD), 2);
                m.ResultUSD = Round(m.ProfitUSD - m.LossUSD, 2);
                m.ProfitEUR = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitEUR), 2);
                m.LossEUR = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.LossEUR), 2);
                m.ResultEUR = Round(m.ProfitEUR - m.LossEUR, 2);
                m.ProfitGBP = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitGBP), 2);
                m.LossGBP = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.LossGBP), 2);
                m.ResultGBP = Round(m.ProfitGBP - m.LossGBP, 2);
                m.ProfitCHF = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitCHF), 2);
                m.LossCHF = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.LossCHF), 2);
                m.ResultCHF = Round(m.ProfitCHF - m.LossCHF, 2);
                m.ProfitSEK = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitSEK), 2);
                m.LossSEK = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.LossSEK), 2);
                m.ResultSEK = Round(m.ProfitSEK - m.LossSEK, 2);
                m.ProfitCNY = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitCNY), 2);
                m.LossCNY = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.LossCNY), 2);
                m.ResultCNY = Round(m.ProfitCNY - m.LossCNY, 2);
            }
            else
            {
                m.ProfitRUB = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitRUB)
                    : 0, 2);
                m.LossRUB = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.LossRUB)
                    : 0, 2);
                m.ResultRUB = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ResultRUB), 2);
                m.ProfitUSD = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitUSD)
                    : 0, 2);
                m.LossUSD = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.LossUSD)
                    : 0, 2);
                m.ResultUSD = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ResultUSD), 2);
                m.ProfitEUR = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitEUR)
                    : 0, 2);
                m.LossEUR = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.LossEUR)
                    : 0, 2);
                m.ResultEUR = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ResultEUR), 2);
                m.ProfitGBP = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitGBP)
                    : 0, 2);
                m.LossGBP = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.LossGBP)
                    : 0, 2);
                m.ResultGBP = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ResultGBP), 2);
                m.ProfitCHF = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitCHF)
                    : 0, 2);
                m.LossCHF = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.LossCHF)
                    : 0, 2);
                m.ResultCHF = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ResultCHF), 2);
                m.ProfitSEK = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitSEK)
                    : 0, 2);
                m.LossSEK = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.LossSEK)
                    : 0, 2);
                m.ResultSEK = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ResultSEK), 2);
                m.ProfitCNY = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.ProfitCNY)
                    : 0, 2);
                m.LossCNY = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Main.Where(_ => _.ParentId == id).Sum(_ => _.LossCNY)
                    : 0, 2);
                m.ResultCNY = Round(Main.Where(_ => _.ParentId == id).Sum(_ => _.ResultCNY), 2);
            }

            SummaNode(m.ParentId);
        }

        private void SummaNodeNach(Guid? id)
        {
            if (id == null) return;
            var m = MainNach.Single(_ => _.Id == id);
            if (id == Guid.Parse("{E7DA6232-CAA0-4358-9BAE-5D96C2EE248A}"))
            {
                m.ProfitRUB = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitRUB), 2);
                m.LossRUB = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossRUB), 2);
                m.ResultRUB = Round(m.ProfitRUB - m.LossRUB, 2);
                m.ProfitUSD = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitUSD), 2);
                m.LossUSD = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossUSD), 2);
                m.ResultUSD = Round(m.ProfitUSD - m.LossUSD, 2);
                m.ProfitEUR = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitEUR), 2);
                m.LossEUR = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossEUR), 2);
                m.ResultEUR = Round(m.ProfitEUR - m.LossEUR, 2);
                m.ProfitGBP = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitGBP), 2);
                m.LossGBP = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossGBP), 2);
                m.ResultGBP = Round(m.ProfitGBP - m.LossGBP, 2);
                m.ProfitCHF = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitCHF), 2);
                m.LossCHF = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossCHF), 2);
                m.ResultCHF = Round(m.ProfitCHF - m.LossCHF, 2);
                m.ProfitSEK = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitSEK), 2);
                m.LossSEK = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossSEK), 2);
                m.ResultSEK = Round(m.ProfitSEK - m.LossSEK, 2);
                m.ProfitCNY = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitCNY), 2);
                m.LossCNY = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossCNY), 2);
                m.ResultCNY = Round(m.ProfitCNY - m.LossCNY, 2);
            }
            else
            {
                m.ProfitRUB = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitRUB)
                    : 0, 2);
                m.LossRUB = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossRUB)
                    : 0, 2);
                m.ResultRUB = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ResultRUB), 2);
                m.ProfitUSD = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitUSD)
                    : 0, 2);
                m.LossUSD = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossUSD)
                    : 0, 2);
                m.ResultUSD = Round(Manager.MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ResultUSD), 2);
                m.ProfitEUR = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitEUR)
                    : 0, 2);
                m.LossEUR = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossEUR)
                    : 0, 2);
                m.ResultEUR = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ResultEUR), 2);
                m.ProfitGBP = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitGBP)
                    : 0, 2);
                m.LossGBP = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossGBP)
                    : 0, 2);
                m.ResultGBP = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ResultGBP), 2);
                m.ProfitCHF = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitCHF)
                    : 0, 2);
                m.LossCHF = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossCHF)
                    : 0, 2);
                m.ResultCHF = Round(Manager.MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ResultCHF), 2);
                m.ProfitSEK = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitSEK)
                    : 0, 2);
                m.LossSEK = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossSEK)
                    : 0, 2);
                m.ResultSEK = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ResultSEK), 2);
                m.ProfitCNY = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitCNY)
                    : 0, 2);
                m.LossCNY = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossCNY)
                    : 0, 2);
                m.ResultCNY = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ResultCNY), 2);
            }

            SummaNodeNach(m.ParentId);
        }

        private void CalcTreeSumm()
        {
            myTempIdList.Clear();
            foreach (
                var d in from d in MainNach let dd = MainNach.Where(_ => _.ParentId == d.Id) where !dd.Any() select d)
            {
                d.ProfitRUB = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitRUB)
                    : 0, 2);
                d.LossRUB = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossRUB)
                    : 0, 2);
                d.ResultRUB = Round(d.ProfitRUB - d.LossRUB, 2);
                d.ProfitUSD = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitUSD)
                    : 0, 2);
                d.LossUSD = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossUSD)
                    : 0, 2);
                d.ResultUSD = Round(d.ProfitUSD - d.LossUSD, 2);
                d.ProfitEUR = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitEUR)
                    : 0, 2);
                d.LossEUR = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossEUR)
                    : 0, 2);
                d.ResultEUR = Round(d.ProfitEUR - d.LossEUR, 2);
                d.ProfitGBP = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitGBP)
                    : 0, 2);
                d.LossGBP = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossGBP)
                    : 0, 2);
                d.ResultGBP = Round(d.ProfitGBP - d.LossGBP, 2);
                d.ProfitCHF = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitCHF)
                    : 0, 2);
                d.LossCHF = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossCHF)
                    : 0, 2);
                d.ResultCHF = Round(d.ProfitCHF - d.LossCHF, 2);
                d.ProfitSEK = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitSEK)
                    : 0, 2);
                d.LossSEK = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossSEK)
                    : 0, 2);
                d.ResultSEK = Round(d.ProfitSEK - d.LossSEK, 2);
                d.ProfitCNY = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitCNY)
                    : 0, 2);
                d.LossCNY = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? ExtendNach.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossCNY)
                    : 0, 2);
                d.ResultCNY = Round(d.ProfitCNY - d.LossCNY, 2);
                myTempIdList.Add(d.ParentId);
            }

            foreach (var id in myTempIdList.Where(id => id != null).Select(_ => _.Value).Distinct())
                SummaNodeNach(id);
            myTempIdList2.Clear();
            foreach (var d in from d in Main let dd = Main.Where(_ => _.ParentId == d.Id) where !dd.Any() select d)
            {
                d.ProfitRUB = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitRUB)
                    : 0, 2);
                d.LossRUB = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossRUB)
                    : 0, 2);
                d.ResultRUB = Round(d.ProfitRUB - d.LossRUB, 2);
                d.ProfitUSD = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitUSD)
                    : 0, 2);
                d.LossUSD = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossUSD)
                    : 0, 2);
                d.ResultUSD = Round(d.ProfitUSD - d.LossUSD, 2);
                d.ProfitEUR = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitEUR)
                    : 0, 2);
                d.LossEUR = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossEUR)
                    : 0, 2);
                d.ResultEUR = Round(d.ProfitEUR - d.LossEUR, 2);
                d.ProfitGBP = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitGBP)
                    : 0, 2);
                d.LossGBP = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossGBP)
                    : 0, 2);
                d.ResultGBP = Round(d.ProfitGBP - d.LossGBP, 2);
                d.ProfitCHF = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitCHF)
                    : 0, 2);
                d.LossCHF = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossCHF)
                    : 0, 2);
                d.ResultCHF = Round(d.ProfitCHF - d.LossCHF, 2);
                d.ProfitSEK = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitSEK)
                    : 0, 2);
                d.LossSEK = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossSEK)
                    : 0, 2);
                d.ResultSEK = Round(d.ProfitSEK - d.LossSEK, 2);
                d.ProfitCNY = Round(d.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.ProfitCNY)
                    : 0, 2);
                d.LossCNY = Round(d.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  d.CalcType == TypeProfitAndLossCalc.IsAll
                    ? Extend.Where(_ => _.GroupId == d.Id).Sum(_ => _.LossCNY)
                    : 0, 2);
                d.ResultCNY = Round(d.ProfitCNY - d.LossCNY, 2);
                myTempIdList2.Add(d.ParentId);
            }

            foreach (var id in myTempIdList2.Where(id => id != null).Select(_ => _.Value).Distinct())
                SummaNode(id);
        }

        public override void RefreshData(object obj)
        {
            if (Form is ProfitAndLosses2 frm)
            {
                var b = frm.treeListMain.Bands.FirstOrDefault(
                    _ => _.Columns.Any(c => c.FieldName == "RecalcResult"));
                if (b != null)
                    b.Header = $"Пересчет в {RecalcCurrency?.Name}";
            }

            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            if (Manager == null)
            {
                Manager = new ProfitAndLossesManager(this)
                {
                    Main = Main,
                    MainNach = MainNach,
                    Extend = Extend,
                    ExtendNach = ExtendNach,
                    DateStart = StartDate,
                    DateEnd = EndDate
                };
            }
            else
            {
                Manager.DateStart = StartDate;
                Manager.DateEnd = EndDate;
            }

            var currentBlsFact = BalansFact;
            var currentBlsCalc = BalansCalc;
            //innerStartDate = StartDate != EndDate ? StartDate.AddDays(1) : StartDate;
            try
            {
                var sDate = GlobalOptions.GetEntities()
                    .CURRENCY_RATES_CB.Where(_ => _.RATE_DATE <= EndDate).Max(x => x.RATE_DATE);
                if (sDate > StartDate)
                    sDate = StartDate;
                Manager.MyRates =
                    GlobalOptions.GetEntities()
                        .CURRENCY_RATES_CB.Where(_ => _.RATE_DATE >= sDate && _.RATE_DATE <= EndDate)
                        .ToList();
                var dt = Manager.MyRates.Select(_ => _.RATE_DATE).Distinct().ToList();
                Manager.MyRates.AddRange(dt.Select(r => new CURRENCY_RATES_CB
                {
                    CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                    NOMINAL = 1,
                    RATE = 1,
                    RATE_DATE = r
                }));
                Manager.Extend.Clear();
                Manager.ExtendNach.Clear();
                ExtendActual.Clear();
                Manager.CalcTovar();
                Manager.CalcStartKontragentBalans();
                Manager.CalcOutCach();
                Manager.CalcStartCash();
                Manager.CalcStartBank();
                Manager.CalcNomInRounding();
                Manager.CalcDilers();
                Manager.CalcUslugiDilers();
                Manager.CalcVozvrat();
                Manager.CalcSpisanie();
                Manager.CalcTovarTransfer();
                Manager.CalcUslugi();
                Manager.CalcFinance();
                Manager.CalcOutBalans();
                Manager.SpisanieTovar();
                Manager.CalcCurrencyChange();
                Manager.CalcZarplata();
                Manager.CalcZarplataNach();
                Manager.CalcCashPercent();
                Manager.CalcNomenklCurrencyChanged();
                Manager.CalcAccruedAmmount();
                Manager.CalcStockHolders();
                Manager.CalcTransferOutBalans();
                //manager.CalcMoneyInWay();
                //CalcCashAvans();
                CalcTreeSumm();
                foreach (var m in Main)
                {
                    var n = MainNach.SingleOrDefault(_ => _.Id == m.Id);
                    if (n == null || Abs(Abs(m.ResultRUB) - Abs(n.ResultRUB)) > 1
                                  || Abs(Abs(m.ResultUSD) - Abs(n.ResultUSD)) > 1
                                  || Abs(Abs(m.ResultEUR) - Abs(n.ResultEUR)) > 1
                                  || Abs(Abs(m.ResultGBP) - Abs(n.ResultGBP)) > 1
                                  || Abs(Abs(m.ResultCHF) - Abs(n.ResultCHF)) > 1
                                  || Abs(Abs(m.ResultSEK) - Abs(n.ResultSEK)) > 1
                                  || Abs(Abs(m.ResultCNY) - Abs(n.ResultCNY)) > 1)
                    {
                        m.IsDiv = true;
                        if (n != null)
                            n.IsDiv = true;
                    }
                    else
                    {
                        m.IsDiv = false;
                        n.IsDiv = false;
                    }
                }

                foreach (
                    var m in from m in MainNach let n = Main.SingleOrDefault(_ => _.Id == m.Id) where n == null select m
                )
                    m.IsDiv = true;
                BalansCrossRateRecalc(RecalcCurrency);
                RaisePropertyChanged(nameof(Main));
                RaisePropertyChanged(nameof(MainNach));
                if (TabSelected == 0)
                {
                    BalansFact = currentBlsFact;
                    UpdateExtend2();
                }

                if (TabSelected == 1)
                {
                    BalansCalc = currentBlsCalc;
                    UpdateExtend();
                }

                RaisePropertyChanged(nameof(ExtendActual));
                ResetCurrencyColumns();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
            finally
            {
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            }

            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            IsDataLoaded = true;
        }

        private void ResetCurrencyColumns()
        {
            var frm = (ProfitAndLosses2)Form;
            foreach (var col in frm.treeListMain.Columns)
            {
                TreeListControlBand b, b1;
                switch (col.FieldName)
                {
                    case "ProfitEUR":
                        b =
                            frm.treeListMain.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitEUR"));
                        b1 =
                            frm.treeListMainNach1.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitEUR"));
                        if (b != null)
                            b.Visible = Main.Sum(_ => _.ProfitEUR) != 0 ||
                                        Main.Sum(_ => _.LossEUR) != 0;
                        if (b1 != null)
                            b1.Visible = MainNach.Sum(_ => _.ProfitEUR) != 0 ||
                                         MainNach.Sum(_ => _.LossEUR) != 0;
                        break;
                    case "ProfitRUB":
                        b =
                            frm.treeListMain.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitRUB"));
                        b1 =
                            frm.treeListMainNach1.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitRUB"));
                        if (b != null)
                            b.Visible = Main.Sum(_ => _.ProfitRUB) != 0 ||
                                        Main.Sum(_ => _.LossRUB) != 0;
                        if (b1 != null)
                            b1.Visible = MainNach.Sum(_ => _.ProfitRUB) != 0 ||
                                         MainNach.Sum(_ => _.LossRUB) != 0;
                        break;
                    case "LossUSD":
                        b =
                            frm.treeListMain.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "LossUSD"));
                        b1 =
                            frm.treeListMainNach1.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "LossUSD"));
                        if (b != null)
                            b.Visible = Main.Sum(_ => _.ProfitUSD) != 0 ||
                                        Main.Sum(_ => _.LossUSD) != 0;
                        if (b1 != null)
                            b1.Visible = MainNach.Sum(_ => _.ProfitUSD) != 0 ||
                                         MainNach.Sum(_ => _.LossUSD) != 0;
                        break;
                    case "ProfitGBP":
                        b =
                            frm.treeListMain.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitGBP"));
                        b1 =
                            frm.treeListMainNach1.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitGBP"));
                        if (b != null)
                            b.Visible = Main.Sum(_ => _.ProfitGBP) != 0 ||
                                        Main.Sum(_ => _.LossGBP) != 0;
                        if (b1 != null)
                            b1.Visible = MainNach.Sum(_ => _.ProfitGBP) != 0 ||
                                         MainNach.Sum(_ => _.LossGBP) != 0;
                        break;
                    case "ProfitCHF":
                        b =
                            frm.treeListMain.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitCHF"));
                        b1 =
                            frm.treeListMainNach1.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitCHF"));
                        if (b != null)
                            b.Visible = Main.Sum(_ => _.ProfitCHF) != 0 ||
                                        Main.Sum(_ => _.LossCHF) != 0;
                        if (b1 != null)
                            b1.Visible = MainNach.Sum(_ => _.ProfitCHF) != 0 ||
                                         MainNach.Sum(_ => _.LossCHF) != 0;
                        break;
                    case "ProfitSEK":
                        b =
                            frm.treeListMain.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitSEK"));
                        b1 =
                            frm.treeListMainNach1.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitSEK"));
                        if (b != null)
                            b.Visible = Main.Sum(_ => _.ProfitSEK) != 0 ||
                                        Main.Sum(_ => _.LossSEK) != 0;
                        if (b1 != null)
                            b1.Visible = MainNach.Sum(_ => _.ProfitSEK) != 0 ||
                                         MainNach.Sum(_ => _.LossSEK) != 0;
                        break;
                    case "ProfitCNY":
                        b =
                            frm.treeListMain.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitCNY"));
                        b1 =
                            frm.treeListMainNach1.Bands.FirstOrDefault(
                                _ => _.Columns.Any(c => c.FieldName == "ProfitCNY"));
                        if (b != null)
                            b.Visible = Main.Sum(_ => _.ProfitCNY) != 0 ||
                                        Main.Sum(_ => _.LossCNY) != 0;
                        if (b1 != null)
                            b1.Visible = MainNach.Sum(_ => _.ProfitCNY) != 0 ||
                                         MainNach.Sum(_ => _.LossCNY) != 0;
                        break;
                }
            }
        }

        #endregion
    }

    public class DataAnnotationCurrencyConvertRow : DataAnnotationForFluentApiBase,
        IMetadataProvider<CurrencyConvertRow>
    {
        void IMetadataProvider<CurrencyConvertRow>.BuildMetadata(
            MetadataBuilder<CurrencyConvertRow> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.CurrencyFrom).NotAutoGenerated();
            builder.Property(_ => _.CurrencyTo).NotAutoGenerated();

            builder.Property(_ => _.DocName).LocatedAt(0).AutoGenerated().DisplayName("Документ").ReadOnly();
            builder.Property(_ => _.DocNum).LocatedAt(1).AutoGenerated().DisplayName("№").ReadOnly();
            builder.Property(_ => _.DocDate).LocatedAt(2).AutoGenerated().DisplayName("Дата").ReadOnly();
            builder.Property(_ => _.Operation).LocatedAt(3).AutoGenerated().DisplayName("Операция").ReadOnly();
            builder.Property(_ => _.Rate).LocatedAt(4).AutoGenerated().DisplayName("Курс").DisplayFormatString("n4")
                .ReadOnly();

            builder.Property(_ => _.ProfitRUB).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.LossRUB).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultRUB).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitUSD).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.LossUSD).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultUSD).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitEUR).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.LossEUR).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultEUR).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitCNY).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.LossCNY).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultCNY).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitGBP).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.LossGBP).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultGBP).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitCHF).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.LossCHF).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultCHF).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitSEK).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2")
                .ReadOnly();
            builder.Property(_ => _.LossSEK).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultSEK).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            // @formatter:off
            builder.TableLayout()
                .Group("Основные данные")
                    .ContainsProperty(_ => _.DocName)
                    .ContainsProperty(_ => _.DocNum)
                    .ContainsProperty(_ => _.DocDate)
                    .ContainsProperty(_ => _.Operation)
                    .ContainsProperty(_ => _.Rate)
                .EndGroup()
                .Group("RUB")
                    .ContainsProperty(_ => _.ProfitRUB)
                    .ContainsProperty(_ => _.LossRUB)
                    .ContainsProperty(_ => _.ResultRUB)
                .EndGroup()
                .Group("USD")
                    .ContainsProperty(_ => _.ProfitUSD)
                    .ContainsProperty(_ => _.LossUSD)
                    .ContainsProperty(_ => _.ResultUSD)
                .EndGroup()
                .Group("EUR")
                    .ContainsProperty(_ => _.ProfitEUR)
                    .ContainsProperty(_ => _.LossEUR)
                    .ContainsProperty(_ => _.ResultEUR)
                .EndGroup()
                .Group("CNY")
                    .ContainsProperty(_ => _.ProfitCNY)
                    .ContainsProperty(_ => _.LossCNY)
                    .ContainsProperty(_ => _.ResultCNY)
                .EndGroup()
                .Group("GBP")
                    .ContainsProperty(_ => _.ProfitGBP)
                    .ContainsProperty(_ => _.LossGBP)
                    .ContainsProperty(_ => _.ResultGBP)
                .EndGroup()
                .Group("CHF")
                    .ContainsProperty(_ => _.ProfitCHF)
                    .ContainsProperty(_ => _.LossCHF)
                    .ContainsProperty(_ => _.ResultCHF)
                .EndGroup()
                .Group("SEK")
                    .ContainsProperty(_ => _.ProfitSEK)
                    .ContainsProperty(_ => _.LossSEK)
                    .ContainsProperty(_ => _.ResultSEK)
                .EndGroup();
            // @formatter:on
        }
    }

    /// <summary>
    ///     Вспомогательный класс для формирования информации по акту валютной конвертации
    /// </summary>
    [MetadataType(typeof(DataAnnotationCurrencyConvertRow))]
    public class CurrencyConvertRow : IProfitCurrencyList
    {
        public decimal DocCode { set; get; }
        public Guid? DocId { set; get; }
        public string DocNum { set; get; }
        public DateTime DocDate { set; get; }
        public string Creator { set; get; }
        public string DocName { set; get; }
        public int? BankRowCode { set; get; }
        public string Operation { set; get; }
        public decimal CurrencyFromDC { set; get; }
        public Currency CurrencyFrom { set; get; }
        public decimal CurrencyToDC { set; get; }
        public Currency CurrencyTo { set; get; }
        public decimal RateCB { set; get; }
        public decimal FromSumma { set; get; }
        public decimal ToSumma { set; get; }
        public decimal Rate { set; get; }
        public decimal ProfitRUB { get; set; }
        public decimal LossRUB { get; set; }
        public decimal ResultRUB { get; set; }
        public decimal ProfitUSD { get; set; }
        public decimal LossUSD { get; set; }
        public decimal ResultUSD { get; set; }
        public decimal ProfitEUR { get; set; }
        public decimal LossEUR { get; set; }
        public decimal ResultEUR { get; set; }
        public decimal ProfitSEK { get; set; }
        public decimal LossSEK { get; set; }
        public decimal ResultSEK { get; set; }
        public decimal ProfitGBP { get; set; }
        public decimal LossGBP { get; set; }
        public decimal ResultGBP { get; set; }
        public decimal ProfitCHF { get; set; }
        public decimal LossCHF { get; set; }
        public decimal ResultCHF { get; set; }
        public decimal ProfitOther { get; set; }
        public decimal LossOther { get; set; }
        public decimal ResultOther { get; set; }
        public decimal ProfitCNY { get; set; }
        public decimal LossCNY { get; set; }
        public decimal ResultCNY { get; set; }
    }
}
