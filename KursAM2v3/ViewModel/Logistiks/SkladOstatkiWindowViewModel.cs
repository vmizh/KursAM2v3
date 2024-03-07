using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Calculates.Materials;
using Core.ViewModel.Base;
using DevExpress.Xpf.Core;
using KursAM2.Managers;
using KursAM2.Managers.Nomenkl;
using KursAM2.View.Logistiks;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.RepositoryHelper;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace KursAM2.ViewModel.Logistiks
{
    public sealed class SkladOstatkiWindowViewModel : RSWindowViewModelBase
    {

        private readonly ImageSource KontrImage;

        private readonly ImageSource StoreImage; 

        private NomenklOstatkiForSklad myCurrentNomenklForSklad;
        private NomenklOstatkiWithPrice myCurrentNomenklStore;
        private NomenklCalcCostOperation myCurrentOperation;
        private KursDomain.References.Warehouse myCurrentWarehouse;
        private bool myIsPeriodSet;
        private DateTime myOstatokDate;
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());

        public SkladOstatkiWindowViewModel()
        {
            LayoutName = "SkladOstatkiWindowViewModel";
            WindowName = "Остатки товаров на складах";

            SvgImageSourceExtension svgToImageSource = new SvgImageSourceExtension
            {
                Uri = new Uri(
                    "pack://application:,,,/DevExpress.Images.v23.2;component/SvgImages/Business Objects/BO_Vendor.svg")
            };
            KontrImage = (ImageSource)svgToImageSource.ProvideValue(null);

            svgToImageSource.Uri = new Uri(
                "pack://application:,,,/DevExpress.Images.v23.2;component/SvgImages/Business Objects/BO_Order.svg");
            StoreImage = (ImageSource)svgToImageSource.ProvideValue(null);

            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            myIsPeriodSet = false;
            myOstatokDate = DateTime.Today;
            RefreshReferences();
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<KursDomain.References.Warehouse> Sklads { set; get; } =
            new ObservableCollection<KursDomain.References.Warehouse>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public List<NomPrice> Prices { set; get; } = new List<NomPrice>();

        // ReSharper disable once CollectionNeverQueried.Global
        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<OperationNomenklMove> NomenklOperationsForSklad { set; get; } =
            new ObservableCollection<OperationNomenklMove>();

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<NomenklCalcCostOperation> NomenklOperations { set; get; } =
            new ObservableCollection<NomenklCalcCostOperation>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public List<NomenklStoreRemainItem> LoadedRemains { set; get; } = new List<NomenklStoreRemainItem>();

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

        public KursDomain.References.Warehouse CurrentWarehouse
        {
            get => myCurrentWarehouse;
            set
            {
                if (Equals(myCurrentWarehouse, value)) return;
                myCurrentWarehouse = value;
                if (myCurrentWarehouse != null)
                {
                    NomenklsForSklad.Clear();
                    LoadNomForSklad();
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
        // ReSharper disable once CollectionNeverUpdated.Global
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
            get
            {
                return new Command(DocumentTovarOpen,
                    _ => CurrentOperation != null && CurrentOperation.TovarDocDC != null);
            }
        }

        public ICommand DocumentFinanceOpenCommand
        {
            get
            {
                return new Command(DocumentFinanceOpen,
                    _ => CurrentOperation != null && CurrentOperation.FinDocumentDC != null);
            }
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
                foreach (var s in skls)
                    Sklads.Add(GlobalOptions.ReferencesCache.GetWarehouse(s) as KursDomain.References.Warehouse);
            }
        }

        public override void RefreshData(object obj)
        {
            NomenklsForSklad.Clear();
            LoadedRemains.Clear();
            //LoadedRemains = NomenklCalculationManager.GetNomenklStoreRemains(OstatokDate, true);
            if (CurrentWarehouse != null) LoadNomForSklad();

            RaisePropertyChanged(nameof(NomenklsForSklad));
        }

        private void LoadNomForSklad()
        {
            var data = nomenklManager.GetNomenklStoreQuantity(CurrentWarehouse.DocCode, new DateTime(2000, 1, 1),
                OstatokDate);
            if (data != null)
                foreach (var d in data.Where(_ => _.OstatokQuantity > 0))
                    NomenklsForSklad.Add(new NomenklOstatkiWithPrice
                    {
                        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(d.NomDC) as Nomenkl,
                        Warehouse =
                            GlobalOptions.ReferencesCache.GetWarehouse(CurrentWarehouse.DocCode) as
                                KursDomain.References.Warehouse,
                        Quantity = d.OstatokQuantity,
                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetNomenkl(d.NomDC).Currency).Name,
                        Price = d.OstatokQuantity != 0 ? Math.Round(d.OstatokNaklSumma / d.OstatokQuantity, 2) : 0,
                        PriceWONaklad = d.OstatokQuantity != 0 ? Math.Round(d.OstatokSumma / d.OstatokQuantity, 2) : 0,
                        Summa = d.OstatokNaklSumma,
                        SummaWONaklad = d.OstatokSumma
                    });
        }

        private void DocumentTovarOpen(object obj)
        {
            switch (CurrentOperation.OperCode)
            {
                case 1:
                case 25:
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.StoreOrderIn, (decimal)CurrentOperation.TovarDocDC);
                    break;
                case 2:
                    DocumentsOpenManager.Open(DocumentType.StoreOrderIn, (decimal)CurrentOperation.TovarDocDC);
                    break;
                case 5:
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.InventoryList, (decimal)CurrentOperation.TovarDocDC);
                    break;
                case 12:
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.Waybill, (decimal)CurrentOperation.TovarDocDC);
                    break;
                case 19:
                    DocumentsOpenManager.Open(DocumentType.NomenklTransfer, CurrentOperation.Id);
                    break;
                case 20:
                    DocumentsOpenManager.Open(DocumentType.InvoiceProvider, (decimal)CurrentOperation.FinDocumentDC);
                    break;
            }
        }

        private void DocumentFinanceOpen(object obj)
        {
            switch (CurrentOperation.OperCode)
            {
                case 1:
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.InvoiceProvider, (decimal)CurrentOperation.FinDocumentDC);
                    break;
                case 12:
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.InvoiceClient, (decimal)CurrentOperation.FinDocumentDC);
                    break;
                case 20:
                    DocumentsOpenManager.Open(DocumentType.InvoiceProvider, (decimal)CurrentOperation.FinDocumentDC);
                    break;
                case 19:
                    DocumentsOpenManager.Open(DocumentType.NomenklTransfer, CurrentOperation.Id);
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
                var data = clc.GetOperations(CurrentNomenklStore.Nomenkl.DocCode, false);
                if (data == null || data.Count <= 0) return;
                decimal nakop = 0;
                foreach (var op in data.OrderBy(_ => _.DocDate).ThenByDescending(_ => _.QuantityIn))
                {
                    op.SenderReceiverIcon = (op.KontrInName ?? op.KontrOutName) == null ? StoreImage : KontrImage;
                    op.SenderReceiverName = (op.KontrInName ?? op.KontrOutName) ??
                                            (op.SkladInName == CurrentWarehouse.Name
                                                ? op.SkladOutName
                                                : op.SkladInName);
                    if (string.IsNullOrEmpty(op.SenderReceiverName))
                        op.SenderReceiverName = CurrentWarehouse.Name;
                    if (op.OperCode == 2 || op.OperCode == 1)
                    {
                        if ((CurrentWarehouse.DocCode == op.SkladIn?.DocCode && op.OperCode != 2) ||
                            (CurrentWarehouse.DocCode == op.SkladOut?.DocCode && op.OperCode != 1))
                        {
                            nakop += op.QuantityIn - op.QuantityOut;
                            op.QuantityNakopit = nakop;
                            NomenklOperations.Add(op);
                        }
                    }
                    else
                    {
                        nakop += op.QuantityIn - op.QuantityOut;
                        op.QuantityNakopit = nakop;
                        NomenklOperations.Add(op);
                    }

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

    public class NomenklPrice
    {
        public decimal Price { set; get; }
        public decimal PricWithNaklad { set; get; }
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
        public KursDomain.References.Warehouse Warehouse { set; get; }
        public string Name => Warehouse?.Name;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public decimal Ostatok { set; get; }
    }

    public class NomenklOstatkiForSklad
    {
        public Nomenkl Nomenkl { set; get; }
        public string NomenklName => Nomenkl?.Name;
        public KursDomain.References.Warehouse Warehouse { set; get; }
        public string StoreName => Warehouse?.Name;
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
