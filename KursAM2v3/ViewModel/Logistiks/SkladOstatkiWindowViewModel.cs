using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Helper;
using JetBrains.Annotations;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace KursAM2.ViewModel.Logistiks
{
    public class SkladOstatkiWindowViewModel : RSWindowViewModelBase
    {
        private readonly List<FilterNomenkl> myFilterNomenkls = new List<FilterNomenkl>();
        private NomenklOstatkiForSklad myCurrentNomenklForSklad;
        private NomenklQuantity myCurrentNomenklQuantity;
        private NomenklOstatkiWithPrice myCurrentNomenklStore;
        private Core.EntityViewModel.Warehouse myCurrentWarehouse;
        private bool myIsPeriodSet;
        private DateTime myOstatokDate;
        private DateTime? myPeriodDateEnd;
        private DateTime? myPeriodDateStart;

        public SkladOstatkiWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            myIsPeriodSet = false;
            myOstatokDate = DateTime.Today;
            PeriodDateEnd = DateTime.Today;
            PeriodDateStart = new DateTime(PeriodDateEnd.Value.Year,1,1);
            RefreshReferences();
        }

        private void RefreshReferences()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql = "SELECT DISTINCT storeDC FROM NomenklMoveForCalc nmfc " +
                          "INNER JOIN HD_27 h ON h.DOC_CODE = nmfc.StoreDC " +
                          "INNER JOIN  EXT_USERS U ON U.USR_ID = H.USR_ID " +
                          $"AND UPPER(U.USR_NICKNAME) = UPPER('{GlobalOptions.UserInfo.NickName}')";
                var skls = ctx.Database.SqlQuery<decimal>(sql);
                foreach (var s in skls)
                {
                    Sklads.Add(MainReferences.Warehouses[s]);
                }
            }
        }

        public ObservableCollection<Core.EntityViewModel.Warehouse> Sklads { set; get; } =
            new ObservableCollection<Core.EntityViewModel.Warehouse>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public List<NomPrice> Prices { set; get; } = new List<NomPrice>();

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<OperationNomenklMove> NomenklOperationsForSklad { set; get; } =
            new ObservableCollection<OperationNomenklMove>();

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<NomenklCalcCostOperation> NomenklOperations { set; get; } =
            new ObservableCollection<NomenklCalcCostOperation>();

        public DateTime OstatokDate
        {
            get => myOstatokDate;
            set
            {
                if (myOstatokDate == value) return;
                myOstatokDate = value;
                //RefreshData(null);
                RaisePropertyChanged();
            }
        }

        public bool IsPeriodSet
        {
            get => myIsPeriodSet;
            set
            {
                if (myIsPeriodSet == value) return;
                myIsPeriodSet = value;
                if (myIsPeriodSet)
                    SetPeriodFilter();
                else
                    UnSetPeriodFilter();
                RaisePropertyChanged();
            }
        }

        public DateTime? PeriodDateStart
        {
            get => myPeriodDateStart;
            set
            {
                if (myPeriodDateStart == value) return;
                myPeriodDateStart = value;
                IsPeriodSet = false;
                if (myPeriodDateStart > PeriodDateEnd)
                    PeriodDateEnd = myPeriodDateStart;
                UpdateSkladOstatki();
                RaisePropertyChanged();
            }
        }

        public DateTime? PeriodDateEnd
        {
            get => myPeriodDateEnd;
            set
            {
                if (myPeriodDateEnd == value) return;
                myPeriodDateEnd = value;
                IsPeriodSet = false;
                if (PeriodDateStart > myPeriodDateEnd)
                    PeriodDateStart = myPeriodDateEnd;
                OstatokDate = myPeriodDateEnd ?? DateTime.Today;
                UpdateSkladOstatki();
                RaisePropertyChanged();
            }
        }

        public Core.EntityViewModel.Warehouse CurrentWarehouse
        {
            get => myCurrentWarehouse;
            set
            {
                if (myCurrentWarehouse != null && myCurrentWarehouse.Equals(value)) return;
                myCurrentWarehouse = value;
                if (myCurrentWarehouse != null)
                {
                    RefreshData(null);
                }
                else
                {
                    NomenklOperationsForSklad.Clear();
                    RaisePropertyChanged(nameof(NomenklOperationsForSklad));
                }

                RaisePropertyChanged();
            }
        }

        // ReSharper disable once CollectionNeverUpdated.Local
        [NotNull] private List<NomenklOstatkiWithPrice> MainOstatki { get; } = new List<NomenklOstatkiWithPrice>();

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<NomenklQuantity> NomenklQuantity { set; get; } =
            new ObservableCollection<NomenklQuantity>();

        public NomenklOstatkiWithPrice CurrentNomenklStore
        {
            get => myCurrentNomenklStore;
            set
            {
                if (myCurrentNomenklStore == value) return;
                myCurrentNomenklStore = value;
                if (CurrentWarehouse == null) return;
                NomenklOperations.Clear();
                if (myCurrentNomenklStore == null) return;
                var op = NomenklCostCalc.GetAllOperations(myCurrentNomenklStore.Nomenkl.DocCode);

                foreach (
                    var opp in
                    op.Operations.Where(_ => _.DocDate >= PeriodDateStart && _.DocDate <= PeriodDateEnd &&
                                             (_.SkladInName == CurrentWarehouse.Name ||
                                              _.SkladOutName == CurrentWarehouse.Name)))
                {
                    if (NomenklOperations.Any(_ => _.TovarDocDC > 0 && _.TovarDocDC == opp.TovarDocDC
                                                                    && _.TovarRowCode == opp.TovarRowCode)
                    )
                        continue;
                    NomenklOperations.Add(opp);
                }

                RaisePropertyChanged(nameof(NomenklOperations));
                RaisePropertyChanged();
            }
        }

        public NomenklQuantity CurrentNomenklQuantity
        {
            get => myCurrentNomenklQuantity;
            set
            {
                if (myCurrentNomenklQuantity == value) return;
                myCurrentNomenklQuantity = value;
                if (myCurrentNomenklQuantity == null)
                {
                    NomenklOnSklads = null;
                    RaisePropertyChanged(nameof(NomenklOnSklads));
                }
                else
                {
                    UpdateSkladQuantity();
                }

                RaisePropertyChanged();
            }
        }

        public ObservableCollection<SkladQuantity> NomenklOnSklads { set; get; } =
            new ObservableCollection<SkladQuantity>();

        // ReSharper disable once CollectionNeverQueried.Global
        public List<NomenklOstatkiWithPrice> NomenklsForSklad { set; get; } =
            new List<NomenklOstatkiWithPrice>();

        public NomenklOstatkiForSklad CurrentNomenklForSklad
        {
            get => myCurrentNomenklForSklad;
            set
            {
                if (myCurrentNomenklForSklad == value) return;
                myCurrentNomenklForSklad = value;
                if (myCurrentNomenklForSklad != null)
                {
                    UpdateOperationOstatki();
                }
                else
                {
                    NomenklOperationsForSklad.Clear();
                    RaisePropertyChanged(nameof(NomenklOperationsForSklad));
                }

                RaisePropertyChanged();
            }
        }

        //<MenuItem Header = "Открыть товарный документ" Command="{Binding DocumentTovarOpenCommand}" />
        //<MenuItem Header = "Открыть финансовый документ" Command="{Binding DocumentFinanceOpenCommand}" />
        public ICommand DocumentTovarOpenCommand
        {
            get { return new Command(DocumentTovarOpen, _ => true); }
        }

        public ICommand DocumentFinanceOpenCommand
        {
            get { return new Command(DocumentFinanceOpen, _ => true); }
        }

        private void UnSetPeriodFilter()
        {
            myFilterNomenkls.Clear();
            UpdateSkladOstatki();
            //RefreshData(null);
        }

        private void SetPeriodFilter()
        {
            if (myPeriodDateStart == null)
            {
                myPeriodDateStart = DateTime.Today;
                RaisePropertyChanged(nameof(PeriodDateStart));
            }

            if (myPeriodDateEnd == null)
            {
                myPeriodDateEnd = DateTime.Today;
                RaisePropertyChanged(nameof(PeriodDateEnd));
            }

            using (var ctx = GlobalOptions.GetEntities())
            {
                var nomDCs = ctx.TD_24.Include(_ => _.SD_24)
                    .Where(_ => _.SD_24.DD_DATE >= myPeriodDateStart && _.SD_24.DD_DATE <= myPeriodDateEnd)
                    .Select(_ => _.DDT_NOMENKL_DC)
                    .Distinct()
                    .ToList();
                myFilterNomenkls.Clear();
                var clc = new NomenklCostMediumSliding(ctx);
                foreach (var n in nomDCs)
                {
                    var ops =
                        clc.GetAllOperations(n)
                            .Where(_ => _.DocDate >= PeriodDateStart && _.DocDate <= PeriodDateEnd)
                            .ToList();
                    var any = ops.Any();
                    if (!any) continue;
                    foreach (var newItem in Sklads.Select(skl => new FilterNomenkl
                        {
                            NomDC = n,
                            StoreName = skl.Name,
                            In = ops.Where(_ => _.SkladInName == skl.Name).Sum(_ => _.QuantityIn),
                            Out = ops.Where(_ => _.SkladOutName == skl.Name).Sum(_ => _.QuantityOut),
                            SumIn = ops.Where(_ => _.SkladInName == skl.Name).Sum(_ => _.SummaInWithNaklad),
                            SumOut = ops.Where(_ => _.SkladOutName == skl.Name).Sum(_ => _.SummaOutWithNaklad),
                            SumInWONaklad = ops.Where(_ => _.SkladInName == skl.Name).Sum(_ => _.SummaIn),
                            SumOutWONaklad = ops.Where(_ => _.SkladOutName == skl.Name).Sum(_ => _.SummaOut)
                        })
                        .Where(newItem => newItem.In > 0 || newItem.Out > 0))
                        myFilterNomenkls.Add(newItem);
                }
            }

            RefreshData(null);
            UpdateSkladOstatki();
        }

        private void UpdateSkladQuantity()
        {
            if (NomenklOnSklads == null) return;
            NomenklOnSklads.Clear();
            foreach (var d in MainOstatki.Where(_ => _.Nomenkl.DocCode == CurrentNomenklQuantity.Nomenkl.DocCode))
                NomenklOnSklads.Add(new SkladQuantity
                {
                    Warehouse = MainReferences.Warehouses[d.Warehouse.DocCode],
                    Ostatok = d.Quantity
                });
            RaisePropertyChanged(nameof(NomenklOnSklads));
        }

        private void UpdateSkladOstatki()
        {
            NomenklsForSklad.Clear();
            if (CurrentWarehouse == null) return;
            if (IsPeriodSet)
                foreach (
                    var d in
                    MainOstatki.Where(d => d.Warehouse.DocCode == CurrentWarehouse.DocCode)
                        .Where(
                            d =>
                                myFilterNomenkls.Where(_ => _.StoreName == CurrentWarehouse.Name)
                                    .Select(_ => _.NomDC)
                                    .Contains(d.Nomenkl.DocCode)))
                    NomenklsForSklad.Add(d);
            else
                NomenklsForSklad = new List<NomenklOstatkiWithPrice>(MainOstatki.Where(d => d.Warehouse.DocCode == CurrentWarehouse.DocCode));
            RaisePropertyChanged(nameof(NomenklsForSklad));
        }

        private void UpdateOperationOstatki()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    NomenklOperationsForSklad = new ObservableCollection<OperationNomenklMove>(
                        ctx.Database.SqlQuery<OperationNomenklMove>("SELECT * FROM(SELECT s24.DOC_CODE AS DocDC, " +
                                                                    "'Приход' AS Operation, " +
                                                                    "CASE WHEN s24.DD_TYPE_DC = 2010000001 THEN 'Приходный ордер №' + CAST(s24.DD_IN_NUM as VARCHAR(100)) " +
                                                                    " WHEN s24.DD_TYPE_DC = 2010000005 THEN 'Инвентаризационная ведомость №' + CAST(s24.DD_IN_NUM as VARCHAR(100)) " +
                                                                    "WHEN s24.DD_TYPE_DC = 2010000008 THEN 'Акт приемки готовой продукции №' + CAST(s24.DD_IN_NUM as VARCHAR(100)) " +
                                                                    " WHEN s24.DD_TYPE_DC = 2010000009 THEN 'Акт разукомплектации готовой продукции №' + CAST(s24.DD_IN_NUM as VARCHAR(100)) " +
                                                                    " WHEN s24.DD_TYPE_DC = 2010000014 THEN 'Накладная на внутренее перемещение №' + CAST(s24.DD_IN_NUM as VARCHAR(100)) " +
                                                                    "ELSE 'Документ не указан' END AS Document, " +
                                                                    " s24.DD_DATE AS DocDate, " +
                                                                    "s24.DD_SKLAD_POL_DC AS WarehouseIn, " +
                                                                    "DDT_NOMENKL_DC AS Nomenkl, " +
                                                                    "DDT_KOL_PRIHOD AS Prihod, " +
                                                                    "0 AS Rashod " +
                                                                    "FROM TD_24 t24 " +
                                                                    "INNER JOIN SD_24 s24 " +
                                                                    " ON s24.DOC_CODE = t24.DOC_CODE AND s24.DD_TYPE_DC IN(2010000001, 2010000005, 2010000008, 2010000009, 2010000014)  AND s24.DD_SKLAD_POL_DC IS NOT NULL " +
                                                                    " WHERE T24.DDT_KOL_PRIHOD > 0 " +
                                                                    "UNION ALL " +
                                                                    "SELECT " +
                                                                    " s24.DOC_CODE AS DocDC, " +
                                                                    "'Расход' AS Operation, " +
                                                                    "CASE WHEN s24.DD_TYPE_DC = 2010000003 THEN 'Расходный складской ордер №' + CAST(s24.DD_IN_NUM as VARCHAR(100)) " +
                                                                    " WHEN s24.DD_TYPE_DC = 2010000005 THEN 'Инвентаризационная ведомость №' + CAST(s24.DD_IN_NUM as VARCHAR(100)) " +
                                                                    "WHEN s24.DD_TYPE_DC = 2010000008 THEN 'Акт приемки готовой продукции №' + CAST(s24.DD_IN_NUM as VARCHAR(100)) " +
                                                                    "WHEN s24.DD_TYPE_DC = 2010000009 THEN 'Акт разукомплектации готовой продукции №' + CAST(s24.DD_IN_NUM as VARCHAR(100)) " +
                                                                    "WHEN s24.DD_TYPE_DC = 2010000012 THEN 'Расходная накладная №' + CAST(s24.DD_IN_NUM as VARCHAR(100)) " +
                                                                    "WHEN s24.DD_TYPE_DC = 2010000014 THEN 'Накладная на внутренее перемещение №' + CAST(s24.DD_IN_NUM as VARCHAR(100)) " +
                                                                    "ELSE 'Документ не указан'  " +
                                                                    "END AS Document, " +
                                                                    "s24.DD_DATE AS DocDate, " +
                                                                    "s24.DD_SKLAD_OTPR_DC AS WarehouseIn, " +
                                                                    "DDT_NOMENKL_DC AS Nomenkl, " +
                                                                    "0 AS Prihod, " +
                                                                    "t24.DDT_KOL_RASHOD AS Rashod " +
                                                                    "FROM TD_24 t24 " +
                                                                    "INNER JOIN SD_24 s24 " +
                                                                    " ON s24.DOC_CODE = t24.DOC_CODE AND s24.DD_TYPE_DC IN(2010000003, 2010000005, 2010000008, 2010000009, 2010000010, 2010000012, 2010000014) AND s24.DD_SKLAD_OTPR_DC IS NOT NULL " +
                                                                    " WHERE T24.DDT_KOL_RASHOD > 0) tab " +
                                                                    "where tab.Nomenkl = " +
                                                                    CustomFormat.DecimalToSqlDecimal(
                                                                        // ReSharper disable once PossibleInvalidOperationException
                                                                        (decimal)
                                                                        CurrentNomenklForSklad?.Nomenkl.DocCode) +
                                                                    " AND tab.WarehouseIn = " +
                                                                    CustomFormat.DecimalToSqlDecimal(
                                                                        // ReSharper disable once PossibleInvalidOperationException
                                                                        (decimal) CurrentWarehouse?.DocCode) +
                                                                    " AND tab.DocDate <= '" +
                                                                    CustomFormat.DateToString(OstatokDate) + "'")
                            .ToList());
                    RaisePropertyChanged(nameof(NomenklOperationsForSklad));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void RefreshData(object obj)
        {
            
                if (CurrentWarehouse == null || PeriodDateStart == null || PeriodDateEnd == null) return;
                var allrem = NomenklCalculationManager.NomenklMoveSum2(PeriodDateStart.Value, PeriodDateEnd.Value,
                    CurrentWarehouse.DOC_CODE);
                var data = new List<NomenklOstatkiWithPrice>();
                foreach (var d in allrem)
                {
                    data.Add(new NomenklOstatkiWithPrice
                    {
                        Nomenkl = MainReferences.GetNomenkl(d.NomDC),
                        Warehouse = CurrentWarehouse,
                        Prihod = d.In,
                        Rashod = d.Out,
                        Quantity = d.End,
                        StartQuantity = d.Start,
                        PriceWONaklad = d.PriceEnd,
                        Price = d.PriceEndWithNaklad,
                        SummaWONaklad = Math.Round(d.PriceStart*d.End, 2),
                        Summa = Math.Round(d.PriceEndWithNaklad*d.End, 2),
                        CurrencyName = MainReferences.GetNomenkl(d.NomDC).Currency.Name
                    });
                }

                NomenklsForSklad = data;
                RaisePropertyChanged(nameof(NomenklsForSklad));
            


            //try
            //{
            //    var data = NomenklCalculationManager.GetNomenklStoreRemains(OstatokDate).ToList();
            //    MainOstatki.Clear();
            //    Sklads.Clear();
            //    NomenklQuantity.Clear();
            //    foreach (var d in data)
            //        MainOstatki.Add(new NomenklOstatkiWithPrice
            //        {
            //            Nomenkl = MainReferences.GetNomenkl(d.NomenklDC),
            //            Warehouse = MainReferences.Warehouses[d.SkladDC],
            //            Prihod = d.Prihod,
            //            Rashod = d.Rashod,
            //            Quantity = d.Prihod - d.Rashod,
            //            StartQuantity = 0,
            //            PriceWONaklad = d.Price,
            //            Price = d.PriceWithNaklad,
            //            SummaWONaklad = Math.Round(d.Price * (d.Prihod - d.Rashod), 2),
            //            Summa = Math.Round(d.PriceWithNaklad * (d.Prihod - d.Rashod), 2),
            //            CurrencyName = MainReferences.GetNomenkl(d.NomenklDC).Currency.Name
            //        });
            //    foreach (var skl in data.Select(_ => _.SkladDC).Distinct())
            //        Sklads.Add(MainReferences.Warehouses[skl]);
            //    foreach (var n in data.Select(_ => _.NomenklDC).Distinct())
            //        NomenklQuantity.Add(new NomenklQuantity
            //        {
            //            Nomenkl = MainReferences.GetNomenkl(n),
            //            Ostatok = data.Where(_ => _.NomenklDC == n).Sum(_ => _.Remain)
            //        });
            //    if (IsPeriodSet && myFilterNomenkls.Count > 0)
            //        foreach (var m in MainOstatki)
            //        {
            //            var fn =
            //                myFilterNomenkls.FirstOrDefault(
            //                    _ => _.StoreName == m.Warehouse.Name && _.NomDC == m.Nomenkl.DocCode);
            //            if (fn == null) continue;
            //            m.SummaIn = fn.SumIn;
            //            m.SummaInWONaklad = fn.SumInWONaklad;
            //            m.SummaOut = fn.SumOut;
            //            m.SummaOutWONaklad = fn.SumOutWONaklad;
            //            m.Prihod = fn.In;
            //            m.Rashod = fn.Out;
            //            m.Result = fn.SumIn - fn.SumOut;
            //            m.ResultWONaklad = fn.SumInWONaklad - fn.SumOutWONaklad;
            //            m.StartQuantity = m.Quantity + m.Rashod - m.Prihod;
            //        }

            //    RaisePropertyChanged(nameof(Sklads));
            //}
            //catch (Exception ex)
            //{
            //    WindowManager.ShowError(ex);
            //}
        }

        private void DocumentTovarOpen(object obj)
        {
        }

        private void DocumentFinanceOpen(object obj)
        {
        }

        public class FilterNomenkl
        {
            public string StoreName { set; get; }
            public decimal NomDC { set; get; }
            public decimal In { set; get; }
            public decimal Out { set; get; }
            public decimal SumIn { set; get; }
            public decimal SumOut { set; get; }
            public decimal SumInWONaklad { set; get; }
            public decimal SumOutWONaklad { set; get; }
        }

        #region Inner section

        public class OperationNomenklMove
        {
            public decimal DocDC { set; get; }
            public string Document { set; get; }
            public string Operation { set; get; }
            public string Note { set; get; }
            public DateTime DocDate { set; get; }
            public decimal Prihod { set; get; }
            public decimal Rashod { set; get; }
            public decimal Ostatok { set; get; }
        }

        public class NomPrice
        {
            public decimal NomDC { set; get; }
            public decimal Price { set; get; }
            public decimal PriceWONaklad { set; get; }
        }

        #endregion
    }

    public class NomenklQuantity
    {
        public Nomenkl Nomenkl { set; get; }
        public string NomenklNumber => Nomenkl?.NomenklNumber;
        public string Name => Nomenkl?.Name;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public decimal Ostatok { set; get; }
    }

    public class SkladQuantity
    {
        public Core.EntityViewModel.Warehouse Warehouse { set; get; }
        public string Name => Warehouse?.Name;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public decimal Ostatok { set; get; }
    }

    public class NomenklOstatkiForSklad
    {
        public Nomenkl Nomenkl { set; get; }
        public string NomenklName { set; get; }
        public Core.EntityViewModel.Warehouse Warehouse { set; get; }
        public string StoreName { set; get; }
        public string NomenklNumber => Nomenkl?.NomenklNumber;
        public string Name => Nomenkl?.Name;
        public decimal Prihod { set; get; }
        public decimal Rashod { set; get; }
        public decimal Quantity { set; get; }
        public decimal StartQuantity { set; get; }
        public string CurrencyName { set; get; }
    }

    public class NomenklOstatkiWithPrice : NomenklOstatkiForSklad
    {
        public decimal PriceWONaklad { set; get; }
        public decimal Price { set; get; }
        public decimal SummaWONaklad { set; get; }
        public decimal Summa { set; get; }
        public decimal SummaIn { set; get; }
        public decimal SummaInWONaklad { set; get; }
        public decimal SummaOut { set; get; }
        public decimal SummaOutWONaklad { set; get; }
        public decimal Result { set; get; }
        public decimal ResultWONaklad { set; get; }
    }
}