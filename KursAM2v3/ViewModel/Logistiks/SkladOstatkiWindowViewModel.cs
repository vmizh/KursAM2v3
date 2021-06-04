using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using Helper;
using KursAM2.Managers;
using KursAM2.View.Logistiks;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace KursAM2.ViewModel.Logistiks
{
    public class SkladOstatkiWindowViewModel : RSWindowViewModelBase
    {
        private NomenklOstatkiForSklad myCurrentNomenklForSklad;
        private NomenklOstatkiWithPrice myCurrentNomenklStore;
        private NomenklCalcCostOperation myCurrentOperation;
        private Core.EntityViewModel.NomenklManagement.Warehouse myCurrentWarehouse;
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
            PeriodDateStart = new DateTime(PeriodDateEnd.Value.Year, 1, 1);
            while (!MainReferences.IsReferenceLoadComplete) Thread.Sleep(5000);
            RefreshReferences();
        }

        public ObservableCollection<Core.EntityViewModel.NomenklManagement.Warehouse> Sklads { set; get; } =
            new ObservableCollection<Core.EntityViewModel.NomenklManagement.Warehouse>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public List<NomPrice> Prices { set; get; } = new List<NomPrice>();

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<OperationNomenklMove> NomenklOperationsForSklad { set; get; } =
            new ObservableCollection<OperationNomenklMove>();

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<NomenklCalcCostOperation> NomenklOperations { set; get; } =
            new ObservableCollection<NomenklCalcCostOperation>();
        public NomenklCalcCostOperation CurrentOperation
        {
            get => myCurrentOperation;
            set
            {
                if (myCurrentOperation == value) return;
                myCurrentOperation = value;
                RaisePropertyChanged();
            }
        }
        public DateTime OstatokDate
        {
            get => myOstatokDate;
            set
            {
                if (myOstatokDate == value) return;
                myOstatokDate = value;
                RefreshData(null);
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
                RefreshData(null);
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
                if (myPeriodDateStart > PeriodDateEnd)
                    PeriodDateEnd = myPeriodDateStart;
                if (IsPeriodSet) RefreshData(null);
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
                if (PeriodDateStart > myPeriodDateEnd)
                    PeriodDateStart = myPeriodDateEnd;
                if (IsPeriodSet) RefreshData(null);
                RaisePropertyChanged();
            }
        }
        public Core.EntityViewModel.NomenklManagement.Warehouse CurrentWarehouse
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
                NomenklOperations.Clear();
                if (myCurrentNomenklStore == null) return;
                loadDocumentsForNomenkl();
                RaisePropertyChanged(nameof(NomenklOperations));
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<SkladQuantity> NomenklOnSklads { set; get; } =
            new ObservableCollection<SkladQuantity>();

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<NomenklOstatkiWithPrice> NomenklsForSklad { set; get; } =
            new ObservableCollection<NomenklOstatkiWithPrice>();
        public NomenklOstatkiForSklad CurrentNomenklForSklad
        {
            get => myCurrentNomenklForSklad;
            set
            {
                if (myCurrentNomenklForSklad == value) return;
                myCurrentNomenklForSklad = value;
                if (myCurrentNomenklForSklad != null)
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
        public ICommand DocumentTovarOpenCommand
        {
            get { return new Command(DocumentTovarOpen,
                _ => CurrentOperation != null && CurrentOperation.TovarDocDC != null); }
        }
        public ICommand DocumentFinanceOpenCommand
        {
            get { return new Command(DocumentFinanceOpen, _ => CurrentOperation != null && CurrentOperation.FinDocumentDC != null); }
        }
        public ICommand NomenklCalcOpenCommand
        {
            get { return new Command(NomenklCalcOpen, _ => CurrentNomenklStore != null); }
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
                foreach (var s in skls) Sklads.Add(MainReferences.Warehouses[s]);
            }
        }

        public override void RefreshData(object obj)
        {
            if (CurrentWarehouse == null) return;
            NomenklsForSklad.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                DateTime dateStart, dateEnd;
                if (IsPeriodSet)
                {
                    dateStart = (DateTime) PeriodDateStart;
                    dateEnd = (DateTime) PeriodDateEnd;
                }
                else
                {
                    dateStart = dateEnd = OstatokDate;
                }
                var sql1 = "SELECT  NomDC ,Date ,StoreDC ,Start ,Prihod ,Rashod ,Nakopit ," +
                           "SummaIn ,SummaNakladIn ,SummaOut ,SummaNakladOut ,Price ," +
                           "PriceWithNaklad " +
                           "FROM dbo.NomenklMoveStore n1 " +
                           $"WHERE StoreDC = {CustomFormat.DecimalToSqlDecimal(CurrentWarehouse.DOC_CODE)} " +
                           "AND n1.Date = (SELECT MAX(n2.Date) FROM NomenklMoveForCalc n2  " +
                           "WHERE n1.NomDC = n2.NomDC  AND n1.StoreDC = n2.StoreDC  " +
                           $"AND n2.Date <= '{CustomFormat.DateToString(dateStart)}') " +
                           "AND n1.Nakopit != 0 " +
                           "UNION " +
                           "SELECT  NomDC ,Date ,StoreDC ,Start ,Prihod ,Rashod ," +
                           "Nakopit ,SummaIn ,SummaNakladIn ," +
                           "SummaOut ,SummaNakladOut ,Price ,PriceWithNaklad " +
                           "FROM NomenklMoveStore " +
                           $"WHERE StoreDC = {CustomFormat.DecimalToSqlDecimal(CurrentWarehouse.DOC_CODE)} " +
                           $"AND Date >= '{CustomFormat.DateToString(dateStart)}' " +
                           $"AND Date <= '{CustomFormat.DateToString(dateEnd)}'";
                var data = ctx.Database.SqlQuery<NomenklMoveStore>(sql1).ToList();
                var nomList = data.Select(_ => _.NomDC).Distinct().ToList();
                foreach (var dc in nomList)
                {
                    //if (dc == 10830000587)
                    //{
                    //    var i = 1;
                    //}
                    var dtemp = data.Where(_ => _.NomDC == dc).ToList();
                    var kolIn = (decimal) dtemp.Where(_ => _.Date >= dateStart && _.Date <= dateEnd).Sum(_ => _.Prihod);
                    var kolOut = (decimal) dtemp.Where(_ => _.Date >= dateStart && _.Date <= dateEnd)
                        .Sum(_ => _.Rashod);
                    var dStartrows = dtemp.Where(_ => _.Date <= dateStart);
                    var dEnd = dtemp.Where(_ => _.Date <= dateEnd).Max(_ => _.Date);
                    var datarow = dtemp.First(_ => _.Date == dEnd);
                    decimal start;
                    if (dStartrows.Any())
                    {
                        var dt = dStartrows.Max(d => d.Date);
                        if (dt < dateStart)
                            start = (decimal) dtemp.First(_ => _.Date == dt).Nakopit;
                        else
                            start = (decimal) dtemp.First(_ => _.Date == dt).Start;
                    }
                    else
                    {
                        start = 0;
                    }
                    var summaIn = (decimal) dtemp.Where(_ => _.Date >= dateStart && _.Date <= dateEnd)
                        .Sum(_ => _.SummaIn);
                    var summaOut = (decimal) dtemp.Where(_ => _.Date >= dateStart && _.Date <= dateEnd)
                        .Sum(_ => _.SummaOut);
                    var summaInNaklad = (decimal) dtemp.Where(_ => _.Date >= dateStart && _.Date <= dateEnd)
                        .Sum(_ => _.SummaNakladIn);
                    var summaOutNaklad = (decimal) dtemp.Where(_ => _.Date >= dateStart && _.Date <= dateEnd)
                        .Sum(_ => _.SummaNakladOut);
                    if (start == 0 && kolIn == 0 && kolOut == 0 && datarow.Nakopit == 0) continue;
                    NomenklsForSklad.Add(new NomenklOstatkiWithPrice
                    {
                        Nomenkl = MainReferences.GetNomenkl(dc),
                        Warehouse = CurrentWarehouse,
                        Prihod = kolIn,
                        Rashod = kolOut,
                        Quantity = (decimal) datarow.Nakopit,
                        StartQuantity = start,
                        PriceWONaklad = datarow.Price,
                        Price = datarow.PriceWithNaklad,
                        SummaWONaklad = Math.Round((decimal) (datarow.Price * datarow.Nakopit), 2),
                        Summa = Math.Round((decimal) (datarow.Price * datarow.Nakopit), 2),
                        CurrencyName = MainReferences.GetNomenkl(datarow.NomDC).Currency.Name,
                        SummaIn = summaInNaklad,
                        SummaOut = summaOutNaklad,
                        SummaInWONaklad = summaIn,
                        SummaOutWONaklad = summaOut
                    });
                }
                RaisePropertiesChanged(nameof(NomenklsForSklad));
            }
        }

        /*switch (oper.OperCode)
                        {
                            case 1:
                                oper.TovarDocument = "Приходный складской ордер ";
                                break;
                            case 2:
                                oper.TovarDocument = "Расходный складской ордер ";
                                break;
                            case 5:
                                oper.TovarDocument = "Инвентаризационная ведомость ";
                                break;
                            case 7:
                                oper.TovarDocument = "Акт приемки готовой продукции ";
                                break;
                            case 8:
                                oper.TovarDocument = "Акт разукомплектации готовой продукции ";
                                break;
                            case 9:
                                oper.TovarDocument = "Акт списания материалов ";
                                break;
                            case 12:
                                oper.TovarDocument = "Расходная накладная (без требования) ";
                                break;
                            case 13:
                                oper.TovarDocument = "Накладная на внутренее перемещение ";
                                break;
                            case 18:
                                oper.TovarDocument = "Продажа за наличный расчет ";
                                break;
                            case 25:
                                oper.TovarDocument = "Возврат товара ";
                                break;*/

        private void DocumentTovarOpen(object obj)
        {
            switch (CurrentOperation.OperCode)
            {
                case 1:
                case 25:
                    DocumentsOpenManager.Open(DocumentType.StoreOrderIn, (decimal) CurrentOperation.TovarDocDC);
                    break;
                case 5:
                    DocumentsOpenManager.Open(DocumentType.InventoryList, (decimal) CurrentOperation.TovarDocDC);
                    break;
                case 12:
                    DocumentsOpenManager.Open(DocumentType.Waybill, (decimal) CurrentOperation.TovarDocDC);
                    break;
            }
        }

        private void DocumentFinanceOpen(object obj)
        {
            switch (CurrentOperation.OperCode)
            {
                case 1:
                    DocumentsOpenManager.Open(DocumentType.InvoiceProvider, (decimal) CurrentOperation.FinDocumentDC);
                    break;
                case 12:
                    DocumentsOpenManager.Open(DocumentType.InvoiceClient, (decimal) CurrentOperation.FinDocumentDC);
                    break;
            }
        }

        private void NomenklCalcOpen(object obj)
        {
            var ctxost1 = new NomenklCostCalculatorWindowViewModel(CurrentNomenklStore.Nomenkl);
            var form = new NomenklCostCalculator
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctxost1
            };
            ctxost1.Form = form;
            ctxost1.RefreshData(null);
            form.Show();
        }

        private void loadDocumentsForNomenkl()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var clc = new NomenklCostMediumSliding(ctx);
                NomenklOperations.Clear();
                foreach (var op in clc.GetOperations(CurrentNomenklStore.Nomenkl.DocCode, false))
                    if (IsPeriodSet)
                    {
                        if (op.DocDate >= PeriodDateStart && op.DocDate <= PeriodDateEnd) NomenklOperations.Add(op);
                    }
                    else
                    {
                        if (op.DocDate == OstatokDate) NomenklOperations.Add(op);
                    }
            }
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
        public Core.EntityViewModel.NomenklManagement.Warehouse Warehouse { set; get; }
        public string Name => Warehouse?.Name;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public decimal Ostatok { set; get; }
    }

    public class NomenklOstatkiForSklad
    {
        public Nomenkl Nomenkl { set; get; }
        public string NomenklName { set; get; }
        public Core.EntityViewModel.NomenklManagement.Warehouse Warehouse { set; get; }
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