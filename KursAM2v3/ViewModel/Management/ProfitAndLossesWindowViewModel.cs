﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Grid;
using KursAM2.Managers;
using KursAM2.View.Base;
using KursAM2.View.Finance;
using KursAM2.View.Management;
using KursAM2.ViewModel.Finance;
using KursAM2.ViewModel.Logistiks;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Management;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using static System.Math;

// ReSharper disable All
namespace KursAM2.ViewModel.Management
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ProfitAndLossesWindowViewModel : RSWindowViewModelBase
    {
        private readonly List<Guid?> myTempIdList = new List<Guid?>();
        private readonly List<Guid?> myTempIdList2 = new List<Guid?>();
        private ProfitAndLossesMainRowViewModel myBalansCalc;
        private ProfitAndLossesMainRowViewModel myBalansFact;
        private CrossCurrencyRate myCurrentCurrencyRate;
        private ProfitAndLossesExtendRowViewModel myCurrentExtend;
        private DateTime myEndDate;
        private Currency myRecalcCurrency;
        private DateTime myStartDate;
        private int myTabSelected;

        public ProfitAndLossesWindowViewModel()
        {
            manager = new ProfitAndLossesManager(this)
            {
                Main = Main,
                MainNach = MainNach,
                Extend = Extend,
                ExtendNach = ExtendNach
            };
            ExtendActual = new ObservableCollection<ProfitAndLossesExtendRowViewModel>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = GetRightMenu();
            RecalcCurrency = GlobalOptions.SystemProfile.NationalCurrency;
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as Currency);
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as Currency);
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as Currency);
            CurrenciesForRecalc.Add(GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as Currency);
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
            var crsrate = new CrossCurrencyRate();
            crsrate.SetRates(DateTime.Today);
            foreach (var c in crsrate.CurrencyList)
            {
                CurrencyRates.Add(c);
            }

            CurrentCurrencyRate = CurrencyRates[0];
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

        public ProfitAndLossesManager manager { set; get; }

        public ObservableCollection<GridControlSummaryItem> TotalSummaries { set; get; } =
            new ObservableCollection<GridControlSummaryItem>();

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
                manager.DateEnd = value;
                if (myEndDate < StartDate)
                    StartDate = myEndDate;
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
                manager.DateStart = myStartDate;
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
                if (myCurrentCurrencyRate == value) return;
                myCurrentCurrencyRate = value;
                RaisePropertyChanged();
            }
        }

        //public CurrenciesForRecalc => MainReferences.
        public ICommand NomenklCalcCommand
        {
            get { return new Command(NomenklCalc, _ => CurrentExtend?.Nomenkl != null); }
        }

        public ProfitAndLossesExtendRowViewModel CurrentExtend
        {
            get => myCurrentExtend;
            set
            {
                if (myCurrentExtend == value) return;
                myCurrentExtend = value;
                if (myCurrentExtend == null) return;
                myCurrentExtend.VzaimozachetInfo.Clear();
                if (myCurrentExtend.Name == "Взаимозачет")
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var zdata = ctx.TD_110.Where(_ => _.DOC_CODE == myCurrentExtend.DocCode).ToList();
                        if (TabSelected == 0)
                        {
                            if (BalansFact?.CalcType == TypeProfitAndLossCalc.IsProfit)
                                foreach (var r in zdata.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0))
                                {
                                    myCurrentExtend.VzaimozachetInfo.Add(new VzaimozachetRow
                                    {
                                        Kontragent =
                                            ((IName)GlobalOptions.ReferencesCache.GetKontragent(r.VZT_KONTR_DC)).Name,
                                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(r.VZT_CRS_DC))
                                            .Name,
                                        Summa = (decimal)r.VZT_CRS_SUMMA,
                                        Note = r.VZT_DOC_NOTES,
                                        SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(r.VZT_SHPZ_DC) as SDRSchet
                                    });
                                }

                            if (BalansFact?.CalcType == TypeProfitAndLossCalc.IsLoss)
                                foreach (var r in zdata.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1))
                                {
                                    myCurrentExtend.VzaimozachetInfo.Add(new VzaimozachetRow
                                    {
                                        Kontragent =
                                            ((IName)GlobalOptions.ReferencesCache.GetKontragent(r.VZT_KONTR_DC)).Name,
                                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(r.VZT_CRS_DC))
                                            .Name,
                                        Summa = (decimal)r.VZT_CRS_SUMMA,
                                        Note = r.VZT_DOC_NOTES,
                                        SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(r.VZT_SHPZ_DC) as SDRSchet
                                    });
                                }
                        }

                        if (TabSelected == 1)
                        {
                            if (BalansCalc?.CalcType == TypeProfitAndLossCalc.IsProfit)
                                foreach (var r in zdata.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0))
                                {
                                    myCurrentExtend.VzaimozachetInfo.Add(new VzaimozachetRow
                                    {
                                        Kontragent =
                                            ((IName)GlobalOptions.ReferencesCache.GetKontragent(r.VZT_KONTR_DC)).Name,
                                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(r.VZT_CRS_DC))
                                            .Name,
                                        Summa = (decimal)r.VZT_CRS_SUMMA,
                                        Note = r.VZT_DOC_NOTES,
                                        SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(r.VZT_SHPZ_DC) as SDRSchet
                                    });
                                }

                            if (BalansCalc?.CalcType == TypeProfitAndLossCalc.IsLoss)
                                foreach (var r in zdata.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1))
                                {
                                    myCurrentExtend.VzaimozachetInfo.Add(new VzaimozachetRow
                                    {
                                        Kontragent =
                                            ((IName)GlobalOptions.ReferencesCache.GetKontragent(r.VZT_KONTR_DC)).Name,
                                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(r.VZT_CRS_DC))
                                            .Name,
                                        Summa = (decimal)r.VZT_CRS_SUMMA,
                                        Note = r.VZT_DOC_NOTES,
                                        SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(r.VZT_SHPZ_DC) as SDRSchet
                                    });
                                }
                        }
                    }
                }

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
                if (myBalansFact == value) return;
                myBalansFact = value;
                RaisePropertyChanged();
            }
        }

        public ProfitAndLossesMainRowViewModel BalansCalc
        {
            get => myBalansCalc;
            set
            {
                if (myBalansCalc == value) return;
                myBalansCalc = value;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<ProfitAndLossesExtendRowViewModel> ExtendActual { set; get; }

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public List<Currency> CurrenciesForRecalc { set; get; } = new List<Currency>();
        public string RecalcCrsName => $"Пересчет в {RecalcCurrency?.Name}";

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

        bool IsActConvert()
        {
            return BalansFact != null && (BalansFact.Id == Guid.Parse("{B6F2540A-9593-42E3-B34F-8C0983BC39A2}")
                                          || BalansFact.Id == Guid.Parse("{35EBABEC-EAC3-4C3C-8383-6326C5D64C8C}"));
        }

        private void ExtendInfo(object obj)
        {
            var ctx = new MutualAcountingInfoWindowViewModel()
            {
                DateStart = StartDate,
                DateEnd = EndDate
            };
            ctx.RefreshData(null);
            var form = new MutualAccountingInfo()
            {
                DataContext = ctx
            };
            form.Show();
        }

        bool IsActConvert2()
        {
            return BalansCalc != null && (BalansCalc.Id == Guid.Parse("{B6F2540A-9593-42E3-B34F-8C0983BC39A2}")
                                          || BalansCalc.Id == Guid.Parse("{35EBABEC-EAC3-4C3C-8383-6326C5D64C8C}"));
        }

        private void ExtendInfo2(object obj)
        {
            var ctx = new MutualAcountingInfoWindowViewModel()
            {
                DateStart = StartDate,
                DateEnd = EndDate
            };
            ctx.RefreshData(null);
            var form = new MutualAccountingInfo()
            {
                DataContext = ctx
            };
            form.Show();
        }

        private void NomenklCalc(object obj)
        {
            //NomenklCalc(CurrentExtend);
            if (CurrentExtend?.Nomenkl?.DocCode == null) return;
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

        private void ResetCurrencyDetailColumns()
        {
            if (Form is ProfitAndLosses frm)
                foreach (var column in frm.GridControlExtend.Columns)
                {
                    GridControlBand b;
                    switch (column.FieldName)
                    {
                        case "ProfitEUR":
                            b =
                                frm.GridControlExtend.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "ProfitEUR"));
                            if (b != null)
                                b.Visible = ExtendActual.Sum(_ => _.ProfitEUR) != 0 ||
                                            ExtendActual.Sum(_ => _.LossEUR) != 0;
                            break;
                        case "LossUSD":
                            b =
                                frm.GridControlExtend.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "LossUSD"));
                            if (b != null)
                                b.Visible = ExtendActual.Sum(_ => _.ProfitUSD) != 0 ||
                                            ExtendActual.Sum(_ => _.LossUSD) != 0;
                            break;
                        case "ProfitRUB":
                            b =
                                frm.GridControlExtend.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "ProfitRUB"));
                            if (b != null)
                                b.Visible = ExtendActual.Sum(_ => _.ProfitRUB) != 0 ||
                                            ExtendActual.Sum(_ => _.LossRUB) != 0;
                            break;
                        case "ProfitGBP":
                            b =
                                frm.GridControlExtend.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "ProfitGBP"));
                            if (b != null)
                                b.Visible = ExtendActual.Sum(_ => _.ProfitGBP) != 0 ||
                                            ExtendActual.Sum(_ => _.LossGBP) != 0;
                            break;
                        case "ProfitCHF":
                            b =
                                frm.GridControlExtend.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "ProfitCHF"));
                            if (b != null)
                                b.Visible = ExtendActual.Sum(_ => _.ProfitCHF) != 0 ||
                                            ExtendActual.Sum(_ => _.LossCHF) != 0;
                            break;
                        case "ProfitSEK":
                            b =
                                frm.GridControlExtend.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "ProfitSEK"));
                            if (b != null)
                                b.Visible = ExtendActual.Sum(_ => _.ProfitSEK) != 0 ||
                                            ExtendActual.Sum(_ => _.LossSEK) != 0;
                            break;
                    }
                }
        }

        public void UpdateExtend2(Guid id)
        {
            ExtendActual.Clear();
            foreach (var d in manager.ExtendNach.Where(d => d.GroupId == id))
                ExtendActual.Add(d);
            if (id == Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}") ||
                id == Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}") ||
                id == Guid.Parse("{2FA1DD9F-6842-4209-B0CC-DDEF3B920496}") ||
                id == Guid.Parse("{E47EF726-3BEA-4B18-9773-E564D624FDF6}"))
            {
                foreach (var id2 in manager.MainNach.Where(_ => _.ParentId == id).Select(_ => _.Id))
                {
                    foreach (var d in manager.ExtendNach.Where(d => d.GroupId == id2))
                        ExtendActual.Add(d);
                }
            }

            ResetCurrencyDetailColumns();
        }

        public void UpdateExtend2()
        {
            ExtendActual.Clear();
            if (BalansFact == null) return;
            foreach (var d in manager.Extend.Where(d => d.GroupId == BalansFact.Id))
                ExtendActual.Add(d);
            if (BalansFact.Id == Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}") ||
                BalansFact.Id == Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}"))
            {
                foreach (var id in Main.Where(_ => _.ParentId == BalansFact.Id).Select(_ => _.Id))
                {
                    foreach (var d in manager.Extend.Where(d => d.GroupId == id))
                        ExtendActual.Add(d);
                }
            }

            ResetCurrencyDetailColumns();
            RaisePropertyChanged(nameof(ExtendActual));
        }

        public void UpdateExtend(Guid id)
        {
            ExtendActual.Clear();
            foreach (var d in manager.Extend.Where(d => d.GroupId == id))
                ExtendActual.Add(d);
            if (id == Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}") ||
                id == Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}") ||
                id == Guid.Parse("{2FA1DD9F-6842-4209-B0CC-DDEF3B920496}") ||
                id == Guid.Parse("{E47EF726-3BEA-4B18-9773-E564D624FDF6}")
               )
            {
                foreach (var id2 in Main.Where(_ => _.ParentId == id).Select(_ => _.Id))
                {
                    foreach (var d in manager.Extend.Where(d => d.GroupId == id2))
                        ExtendActual.Add(d);
                }
            }

            ResetCurrencyDetailColumns();
        }

        public void UpdateExtend()
        {
            ExtendActual.Clear();
            if (BalansCalc == null) return;
            foreach (var d in manager.ExtendNach.Where(d => d.GroupId == BalansCalc.Id))
                ExtendActual.Add(d);
            if (BalansCalc.Id == Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}") ||
                BalansCalc.Id == Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}"))
            {
                foreach (var id in MainNach.Where(_ => _.ParentId == BalansCalc.Id).Select(_ => _.Id))
                {
                    foreach (var d in manager.ExtendNach.Where(d => d.GroupId == id))
                        ExtendActual.Add(d);
                }
            }

            ResetCurrencyDetailColumns();
            RaisePropertyChanged(nameof(ExtendActual));
        }

        public override void DocumentOpen(object obj)
        {
            DocumentsOpenManager.Open(CurrentExtend.DocTypeCode, CurrentExtend.DocCode);
        }

        public class NakladTemp
        {
            public decimal DocCode { set; get; }
            public int Code { set; get; }
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

            public decimal? CODC { set; get; }
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
                m.ResultUSD = Round(manager.MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ResultUSD), 2);
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
                m.ResultCHF = Round(manager.MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ResultCHF), 2);
                m.ProfitSEK = Round(m.CalcType == TypeProfitAndLossCalc.IsProfit ||
                                    m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ProfitSEK)
                    : 0, 2);
                m.LossSEK = Round(m.CalcType == TypeProfitAndLossCalc.IsLoss ||
                                  m.CalcType == TypeProfitAndLossCalc.IsAll
                    ? MainNach.Where(_ => _.ParentId == id).Sum(_ => _.LossSEK)
                    : 0, 2);
                m.ResultSEK = Round(MainNach.Where(_ => _.ParentId == id).Sum(_ => _.ResultSEK), 2);
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
                myTempIdList2.Add(d.ParentId);
            }

            foreach (var id in myTempIdList2.Where(id => id != null).Select(_ => _.Value).Distinct())
                SummaNode(id);
        }

        public override void RefreshData(object obj)
        {
            var currentBlsFact = BalansFact;
            var currentBlsCalc = BalansCalc;
            //innerStartDate = StartDate != EndDate ? StartDate.AddDays(1) : StartDate;
            try
            {
                manager.MyRates =
                    GlobalOptions.GetEntities()
                        .CURRENCY_RATES_CB.Where(_ => _.RATE_DATE >= StartDate && _.RATE_DATE <= EndDate)
                        .ToList();
                var dt = manager.MyRates.Select(_ => _.RATE_DATE).Distinct().ToList();
                manager.MyRates.AddRange(dt.Select(r => new CURRENCY_RATES_CB
                {
                    CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                    NOMINAL = 1,
                    RATE = 1,
                    RATE_DATE = r
                }));
                manager.Extend.Clear();
                manager.ExtendNach.Clear();
                ExtendActual.Clear();
                manager.CalcTovar();
                manager.CalcStartKontragentBalans();
                manager.CalcOutCach();
                manager.CalcStartCash();
                manager.CalcStartBank();
                manager.CalcNomInRounding();
                manager.CalcDilers();
                manager.CalcUslugiDilers();
                manager.CalcVozvrat();
                manager.CalcSpisanie();
                manager.CalcTovarTransfer();
                manager.CalcUslugi();
                manager.CalcFinance();
                manager.CalcOutBalans();
                manager.CalcCurrencyChange();
                manager.CalcZarplata();
                manager.CalcZarplataNach();
                manager.CalcCashPercent();
                manager.CalcMoneyInWay();
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
                                  || Abs(Abs(m.ResultSEK) - Abs(n.ResultSEK)) > 1)
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
        }

        private void ResetCurrencyColumns()
        {
            var frm = (ProfitAndLosses)Form;
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
                }
            }
        }

        #endregion
    }


    public class DataAnnotationVzaimozachetRow : DataAnnotationForFluentApiBase,
        IMetadataProvider<VzaimozachetRow>
    {
        void IMetadataProvider<VzaimozachetRow>.BuildMetadata(
            MetadataBuilder<VzaimozachetRow> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Kontragent).AutoGenerated().DisplayName("Контрагент").ReadOnly();
            builder.Property(_ => _.SDRSchet).AutoGenerated().DisplayName("Счет доходов/расходов").ReadOnly();
            builder.Property(_ => _.CurrencyName).AutoGenerated().DisplayName("Валюта").ReadOnly();
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").ReadOnly();
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание").ReadOnly();

            builder.Property(_ => _.ProfitRUB).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossRUB).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultRUB).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitUSD).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossUSD).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultUSD).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitEUR).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossEUR).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultEUR).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitCNY).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossCNY).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultCNY).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitGBP).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossGBP).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultGBP).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitCHF).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossCHF).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultCHF).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

            builder.Property(_ => _.ProfitSEK).AutoGenerated().DisplayName("Доход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.LossSEK).AutoGenerated().DisplayName("Расход").DisplayFormatString("n2").ReadOnly();
            builder.Property(_ => _.ResultSEK).AutoGenerated().DisplayName("Результат").DisplayFormatString("n2")
                .ReadOnly();

                        // @formatter:off
            builder.TableLayout()
                .Group("Основные данные")
                    .ContainsProperty(_ => _.Kontragent)
                    .ContainsProperty(_ => _.SDRSchet)
                    .ContainsProperty(_ => _.CurrencyName)
                    .ContainsProperty(_ => _.Summa)
                    .ContainsProperty(_ => _.Note)
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
    [MetadataType(typeof(DataAnnotationVzaimozachetRow))]
    public class VzaimozachetRow
    {
        public string Kontragent { set; get; }
        public SDRSchet SDRSchet { set; get; }
        public string CurrencyName { set; get; }
        public decimal Summa { set; get; }
        public string Note { set; get; }
        public decimal ProfitGBP { get; set; }
        public decimal LossGBP { get; set; }
        public decimal ResultGBP { get; set; }
        public decimal ProfitCHF { get; set; }
        public decimal LossCHF { get; set; }
        public decimal ResultCHF { get; set; }
        public decimal ProfitSEK { get; set; }
        public decimal LossSEK { get; set; }
        public decimal ResultSEK { get; set; }
        public decimal ProfitRUB { set; get; }
        public decimal LossRUB { set; get; }
        public decimal ResultRUB { set; get; }
        public decimal ProfitUSD { set; get; }
        public decimal LossUSD { set; get; }
        public decimal ResultUSD { set; get; }
        public decimal ProfitCNY { set; get; }
        public decimal LossCNY { set; get; }
        public decimal ResultCNY { set; get; }
        public decimal ProfitEUR { set; get; }
        public decimal LossEUR { set; get; }
        public decimal ResultEUR { set; get; }
    }
}
