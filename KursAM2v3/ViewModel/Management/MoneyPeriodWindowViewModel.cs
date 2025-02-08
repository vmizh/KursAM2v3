using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Xpf.Editors;
using KursAM2.Managers;
using KursAM2.View.Management;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

// ReSharper disable CollectionNeverQueried.Global
namespace KursAM2.ViewModel.Management
{
    public sealed class MoneyPeriodWindowViewModel : RSWindowViewModelBase
    {
        private MoneyPeriodRowViewModel myCurrentItem;
        private DateTime myEndDate;
        private bool myIncludeVzaimozachet;
        private DateTime myStartDate;
        private ComboBoxEditItem myTimeOut;
        private int myTimeOutIndex;

        public MoneyPeriodWindowViewModel()
        {
            AllOperations = new ObservableCollection<MoneyPeriodRowViewModel>();
            CommonOperations = new ObservableCollection<MoneyPeriodRowViewModel>();
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            StartDate = DateTime.Today.AddDays(-30);
            EndDate = DateTime.Today;
            TimeOutIndex = 0;
        }

        public MoneyPeriodWindowViewModel(Window form) : base(form)
        {
            AllOperations = new ObservableCollection<MoneyPeriodRowViewModel>();
            CommonOperations = new ObservableCollection<MoneyPeriodRowViewModel>();
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            StartDate = new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
            EndDate = DateTime.Today;
            TimeOutIndex = 0;
        }

        public override string LayoutName => "MoneyPeriodWindowViewModel";
        public bool IsIncludeVzaimozachet
        {
            get => myIncludeVzaimozachet;
            set
            {
                if (myIncludeVzaimozachet == value) return;
                myIncludeVzaimozachet = value;
                RefreshData(null);
                RaisePropertyChanged();
            }
        }

        public override void AddSearchList(object obj)
        {
            var form = new MoneyPeriodMove
            {
                Owner = Application.Current.MainWindow
            };
            var dv = new MoneyPeriodWindowViewModel(form);
            dv.RefreshData(null);
            form.DataContext = dv;
            form.Show();

        }

        public int TimeOutIndex
        {
            get => myTimeOutIndex;
            set
            {
                if (myTimeOutIndex == value) return;
                myTimeOutIndex = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<MoneyRemainsRow> Remains { set; get; } =
            new ObservableCollection<MoneyRemainsRow>();

        public ObservableCollection<MoneyPeriodRowViewModel> Prihod { set; get; } =
            new ObservableCollection<MoneyPeriodRowViewModel>();

        public ObservableCollection<MoneyPeriodRowViewModel> Rashod { set; get; } =
            new ObservableCollection<MoneyPeriodRowViewModel>();

        public ObservableCollection<MoneyPeriodRowViewModel> AllOperations { set; get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ObservableCollection<MoneyPeriodRowViewModel> CommonOperations { set; get; }

        public MoneyPeriodRowViewModel CurrentItem
        {
            get => myCurrentItem;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentItem == value) return;
                myCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get => myStartDate;
            set
            {
                if (myStartDate == value) return;
                myStartDate = value;
                RaisePropertyChanged();
            }
        }

        public ComboBoxEditItem TimeOut
        {
            get => myTimeOut;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myTimeOut == value) return;
                myTimeOut = value;
                RaisePropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => myEndDate;
            set
            {
                if (myEndDate == value) return;
                myEndDate = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsDocumentOpenAllow =>
            CurrentItem != null && DocumentsOpenManager.IsDocumentOpen(CurrentItem.DocumentType);

        private string GetEmployeeName(int tn)
        {
            return GlobalOptions.ReferencesCache.GetEmployee(tn) is Employee emp ? emp.Name : "Сотрудник не найден";
        }

        private void LoadRemains()
        {
            foreach (var ch in GlobalOptions.ReferencesCache.GetCashBoxAll().Cast<CashBox>().OrderBy(_ => _.Name))
            foreach (var c in GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Currency>())
            {
                var start = CashManager.GetCashCurrencyRemains(ch.DocCode, c.DocCode, StartDate);
                var end = CashManager.GetCashCurrencyRemains(ch.DocCode, c.DocCode, EndDate);
                if (start != 0 || end != 0)
                    Remains.Add(new MoneyRemainsRow
                    {
                        Name =
                            ch.Name,
                        RemainsType = "Касса",
                        StartSumma = start,
                        EndSumma = end,
                        Currency = c
                    });
            }

            var bankManager = new BankOperationsManager();
            foreach (var b in GlobalOptions.ReferencesCache.GetBankAccountAll().Cast<BankAccount>())
            {
                var rem = bankManager.GetRemains2(b.DocCode, StartDate, EndDate);
                if (rem.SummaStart != 0 || rem.SummaEnd != 0)
                {
                    if (rem.Currency.DocCode == CurrencyCode.CHF)
                        Remains.Add(new MoneyRemainsRow
                        {
                            Name = b.Name + " / " + b.RashAccount,
                            RemainsType = "Банк",
                            StartSumma = (decimal)rem.SummaStart,
                            EndSumma = (decimal)rem.SummaEnd,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CHF) as Currency
                        });
                    if (rem.Currency.DocCode == CurrencyCode.CNY)
                        Remains.Add(new MoneyRemainsRow
                        {
                            Name = b.Name + " / " + b.RashAccount,
                            RemainsType = "Банк",
                            StartSumma = (decimal)rem.SummaStart,
                            EndSumma = (decimal)rem.SummaEnd,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.CNY) as Currency
                        });
                    if (rem.Currency.DocCode == CurrencyCode.USD)
                        Remains.Add(new MoneyRemainsRow
                        {
                            Name = b.Name + " / " + b.RashAccount,
                            RemainsType = "Банк",
                            StartSumma = (decimal)rem.SummaStart,
                            EndSumma = (decimal)rem.SummaEnd,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.USD) as Currency
                        });
                    if (rem.Currency.DocCode == CurrencyCode.EUR)
                        Remains.Add(new MoneyRemainsRow
                        {
                            Name = b.Name + " / " + b.RashAccount,
                            RemainsType = "Банк",
                            StartSumma = (decimal)rem.SummaStart,
                            EndSumma = (decimal)rem.SummaEnd,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.EUR) as Currency
                        });
                    if (rem.Currency.DocCode == CurrencyCode.RUB)
                        Remains.Add(new MoneyRemainsRow
                        {
                            Name = b.Name + " / " + b.RashAccount,
                            RemainsType = "Банк",
                            StartSumma = (decimal)rem.SummaStart,
                            EndSumma = (decimal)rem.SummaEnd,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.RUB) as Currency
                        });
                    if (rem.Currency.DocCode == CurrencyCode.GBP)
                        Remains.Add(new MoneyRemainsRow
                        {
                            Name = b.Name + " / " + b.RashAccount,
                            RemainsType = "Банк",
                            StartSumma = (decimal)rem.SummaStart,
                            EndSumma = (decimal)rem.SummaEnd,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.GBP) as Currency
                        });
                    if (rem.Currency.DocCode == CurrencyCode.SEK)
                        Remains.Add(new MoneyRemainsRow
                        {
                            Name = b.Name + " / " + b.RashAccount,
                            RemainsType = "Банк",
                            StartSumma = (decimal)rem.SummaStart,
                            EndSumma = (decimal)rem.SummaEnd,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(CurrencyCode.SEK) as Currency
                        });
                }
            }
        }

        public override void RefreshData(object obj)
        {
            Remains.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var cashOstatki =
                        ctx.TD_22.Include(_ => _.SD_22)
                            .Where(_ => _.DATE_START >= StartDate && _.DATE_START <= EndDate)
                            .ToList();
                    var bankDates =
                        ctx.SD_101.Where(_ => _.VV_START_DATE >= StartDate && _.VV_START_DATE <= EndDate).ToList();
                    var dates = bankDates.Select(_ => _.VV_ACC_DC)
                        .Distinct()
                        .ToDictionary(d => d, d => bankDates.Where(_ => _.VV_ACC_DC == d).Min(_ => _.VV_START_DATE));
                    var cashIn = ctx.SD_33
                        .Include(_ => _.SD_303)
                        .Include(_ => _.SD_303.SD_99)
                        .Where(_ => _.DATE_ORD >= StartDate && _.DATE_ORD <= EndDate
                                                            && (_.TABELNUMBER != null || _.KONTRAGENT_DC != null))
                        .ToList();
                    var cashOut = ctx.SD_34
                        .Include(_ => _.SD_303)
                        .Include(_ => _.SD_303.SD_99)
                        .Where(_ => _.DATE_ORD >= StartDate && _.DATE_ORD <= EndDate
                                                            && (_.TABELNUMBER != null || _.KONTRAGENT_DC != null))
                        .ToList();
                    var bankOper =
                        ctx.TD_101
                            .Include(_ => _.SD_101)
                            .Include(_ => _.SD_303)
                            .Include(_ => _.SD_303.SD_99)
                            .Where(_ => _.SD_101.VV_STOP_DATE >= StartDate && _.SD_101.VV_STOP_DATE <= EndDate)
                            .ToList();
                    var vzaim = ctx.TD_110
                        .Include(_ => _.SD_110)
                        .Include(_ => _.SD_303)
                        .Include(_ => _.SD_303.SD_99)
                        .Where(_ => _.VZT_DOC_DATE >= StartDate && _.VZT_DOC_DATE <= EndDate)
                        .ToList();
                    Prihod = new ObservableCollection<MoneyPeriodRowViewModel>();
                    Rashod = new ObservableCollection<MoneyPeriodRowViewModel>();
                    AllOperations = new ObservableCollection<MoneyPeriodRowViewModel>();
                    foreach (var ost in cashOstatki)
                    {
                        if (ost.SUMMA_START == 0) continue;
                        var n = new MoneyPeriodRowViewModel
                        {
                            Date = ost.DATE_START,
                            Currency = GlobalOptions.ReferencesCache.GetCurrency(ost.CRS_DC) as Currency,
                            DocName = "Остатки по кассе",
                            SummaPrihod = ost.SUMMA_START,
                            // ReSharper disable once PossibleInvalidOperationException
                            SummaRashod = 0,
                            Kontragent = ost.SD_22.CA_NAME,
                            Note = null,
                            SDRSchet = null,
                            SDRState = null
                        };
                        AllOperations.Add(n);
                        if (n.SummaPrihod > 0)
                            Prihod.Add(n);
                        if (n.SummaRashod > 0)
                            Rashod.Add(n);
                    }

                    foreach (var d in dates)
                    {
                        var dc =
                            ctx.SD_101.FirstOrDefault(_ => _.VV_ACC_DC == d.Key && _.VV_START_DATE == d.Value)
                                ?.DOC_CODE;
                        if (dc == null) continue;
                        var bank = ctx.SD_114.FirstOrDefault(_ => _.DOC_CODE == d.Key);
                        // ReSharper disable once PossibleNullReferenceException
                        var bankName = bank.BA_BANK_NAME + " (" + bank.BA_ACC_SHORTNAME + ") " + "р/с " +
                                       bank.BA_RASH_ACC;
                        foreach (var c in ctx.UD_101.Where(_ => _.DOC_CODE == dc && _.VVU_REST_TYPE == 0))
                            if (c.VVU_VAL_SUMMA > 0)
                            {
                                var n = new MoneyPeriodRowViewModel
                                {
                                    DocCode = c.SD_101.DOC_CODE,
                                    Date = c.SD_101.VV_START_DATE,
                                    Currency = GlobalOptions.ReferencesCache.GetCurrency(c.VVU_CRS_DC) as Currency,
                                    DocName = "Остатки по банку",
                                    // ReSharper disable once PossibleInvalidOperationException
                                    SummaPrihod = (decimal)c.VVU_VAL_SUMMA,
                                    SummaRashod = 0,
                                    Kontragent = bankName,
                                    Note = "",
                                    SDRSchet = null,
                                    SDRState = null
                                };
                                AllOperations.Add(n);
                                if (n.SummaPrihod > 0)
                                    Prihod.Add(n);
                                if (n.SummaRashod > 0)
                                    Rashod.Add(n);
                            }
                    }

                    foreach (var newAv in vzaim.Select(av => new MoneyPeriodRowViewModel
                                 {
                                     DocCode = av.DOC_CODE,
                                     Date = av.VZT_DOC_DATE,
                                     Currency = GlobalOptions.ReferencesCache.GetCurrency(av.VZT_CRS_DC) as Currency,
                                     DocName = "Акт взаимозачета",
                                     // ReSharper disable once PossibleInvalidOperationException
                                     SummaPrihod = av.VZT_1MYDOLZH_0NAMDOLZH == 0 ? av.VZT_CRS_SUMMA.Value : 0,
                                     // ReSharper disable once PossibleInvalidOperationException
                                     SummaRashod = av.VZT_1MYDOLZH_0NAMDOLZH == 1 ? av.VZT_CRS_SUMMA.Value : 0,
                                     Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(av.VZT_KONTR_DC))
                                         .Name,
                                     Note = "",
                                     SDRSchet = av.SD_303 != null
                                         ? new SDRSchet
                                         {
                                             DocCode = av.SD_303.DOC_CODE,
                                             Name = av.SD_303.SHPZ_NAME
                                         }
                                         : new SDRSchet
                                         {
                                             DocCode = -1,
                                             Name = "Не указан"
                                         },
                                     SDRState = av.SD_303 != null && av.SD_303.SD_99 != null
                                         ? new SDRState
                                         {
                                             DocCode = av.SD_303.SD_99.DOC_CODE,
                                             Name = av.SD_303.SD_99.SZ_NAME
                                         }
                                         : new SDRState
                                         {
                                             DocCode = -1,
                                             Name = "Статья не указана"
                                         },
                                     DocumentType = DocumentType.MutualAccounting
                                 })
                                 .Where(newAv => IsIncludeVzaimozachet))
                    {
                        AllOperations.Add(newAv);
                        if (newAv.SummaPrihod > 0)
                            Prihod.Add(newAv);
                        else
                            Rashod.Add(newAv);
                    }

                    foreach (
                        var op in
                        bankOper.Where(_ => _.VVT_KONTRAGENT != null || _.EmployeeDC != null)
                            .Select(d => new MoneyPeriodRowViewModel
                            {
                                DocCode = d.DOC_CODE,
                                Code = d.CODE,
                                Date = d.SD_101.VV_STOP_DATE,
                                Currency = GlobalOptions.ReferencesCache.GetCurrency(d.VVT_CRS_DC) as Currency,
                                DocName = "Банковская выписка",
                                // ReSharper disable once PossibleInvalidOperationException
                                SummaPrihod = d.VVT_VAL_PRIHOD.Value,
                                // ReSharper disable once PossibleInvalidOperationException
                                SummaRashod = d.VVT_VAL_RASHOD.Value,
                                Kontragent = d.VVT_KONTRAGENT != null ?
                                    ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.VVT_KONTRAGENT)).Name :
                                    ((IName)GlobalOptions.ReferencesCache.GetEmployee(d.EmployeeDC)).Name,
                                Note = "",
                                SDRSchet = d.SD_303 != null
                                    ? new SDRSchet
                                    {
                                        DocCode = d.SD_303.DOC_CODE,
                                        Name = d.SD_303.SHPZ_NAME
                                    }
                                    : new SDRSchet
                                    {
                                        DocCode = -1,
                                        Name = "Не указан"
                                    },
                                SDRState = d.SD_303 != null && d.SD_303.SD_99 != null
                                    ? new SDRState
                                    {
                                        DocCode = d.SD_303.SD_99.DOC_CODE,
                                        Name = d.SD_303.SD_99.SZ_NAME
                                    }
                                    : new SDRState
                                    {
                                        DocCode = -1,
                                        Name = "Статья не указана"
                                    },
                                DocumentType = DocumentType.Bank
                            }))
                    {
                        if (op.SummaPrihod > 0)
                            Prihod.Add(op);
                        else
                            Rashod.Add(op);
                        AllOperations.Add(op);
                    }

                    foreach (
                        var prih in
                        cashIn.Where(_ => _.BANK_RASCH_SCHET_DC == null && _.RASH_ORDER_FROM_DC == null)
                            .Select(d => new MoneyPeriodRowViewModel
                            {
                                DocCode = d.DOC_CODE,
                                // ReSharper disable once PossibleInvalidOperationException
                                Date = d.DATE_ORD.Value,
                                // ReSharper disable once AssignNullToNotNullAttribute
                                // ReSharper disable once PossibleInvalidOperationException
                                Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC.Value) as Currency,
                                DocName = "Приходный кассовый ордер",
                                // ReSharper disable once PossibleInvalidOperationException
                                SummaPrihod = d.SUMM_ORD.Value,
                                SummaRashod = 0,
                                Kontragent =
                                    d.TABELNUMBER == null
                                        ? ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KONTRAGENT_DC)).Name
                                        : GetEmployeeName(d.TABELNUMBER.Value),
                                Note = d.NOTES_ORD,
                                SDRSchet = d.SD_303 != null
                                    ? new SDRSchet
                                    {
                                        DocCode = d.SD_303.DOC_CODE,
                                        Name = d.SD_303.SHPZ_NAME
                                    }
                                    : new SDRSchet
                                    {
                                        DocCode = -1,
                                        Name = "Не указан"
                                    },
                                SDRState = d.SD_303 != null && d.SD_303.SD_99 != null
                                    ? new SDRState
                                    {
                                        DocCode = d.SD_303.SD_99.DOC_CODE,
                                        Name = d.SD_303.SD_99.SZ_NAME
                                    }
                                    : new SDRState
                                    {
                                        DocCode = -1,
                                        Name = "Статья не указана"
                                    },
                                DocumentType = DocumentType.CashIn
                            }))
                    {
                        Prihod.Add(prih);
                        AllOperations.Add(prih);
                    }

                    foreach (
                        var rash in
                        cashOut.Where(_ => _.BANK_RASCH_SCHET_DC == null && _.CASH_TO_DC == null)
                            .Select(d => new MoneyPeriodRowViewModel
                            {
                                DocCode = d.DOC_CODE,
                                // ReSharper disable once PossibleInvalidOperationException
                                Date = d.DATE_ORD.Value,
                                // ReSharper disable once AssignNullToNotNullAttribute
                                // ReSharper disable once PossibleInvalidOperationException
                                Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC.Value) as Currency,
                                DocName = "Расходный кассовый ордер",
                                SummaPrihod = 0,
                                // ReSharper disable once PossibleInvalidOperationException
                                SummaRashod = d.SUMM_ORD.Value,
                                Kontragent =
                                    d.TABELNUMBER == null
                                        ? ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KONTRAGENT_DC)).Name
                                        : GetEmployeeName(d.TABELNUMBER.Value),
                                Note = d.NOTES_ORD,
                                SDRSchet = d.SD_303 != null
                                    ? new SDRSchet
                                    {
                                        DocCode = d.SD_303.DOC_CODE,
                                        Name = d.SD_303.SHPZ_NAME
                                    }
                                    : new SDRSchet
                                    {
                                        DocCode = -1,
                                        Name = "Не указан"
                                    },
                                SDRState = d.SD_303 != null && d.SD_303.SD_99 != null
                                    ? new SDRState
                                    {
                                        DocCode = d.SD_303.SD_99.DOC_CODE,
                                        Name = d.SD_303.SD_99.SZ_NAME
                                    }
                                    : new SDRState
                                    {
                                        DocCode = -1,
                                        Name = "Статья не указана"
                                    },
                                DocumentType = DocumentType.CashOut
                            }))
                    {
                        Rashod.Add(rash);
                        AllOperations.Add(rash);
                    }
                }

                LoadRemains();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            RaisePropertyChanged(nameof(AllOperations));
            RaisePropertyChanged(nameof(Prihod));
            RaisePropertyChanged(nameof(Rashod));
        }

        public override void DocumentOpen(object obj)
        {
            if (CurrentItem.DocumentType == DocumentType.Bank)
            {
                DocumentsOpenManager.Open(CurrentItem.DocumentType, CurrentItem.Code);
                return;
            }

            DocumentsOpenManager.Open(CurrentItem.DocumentType, CurrentItem.DocCode);
        }
    }
}
