using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Calculates.Materials;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;
using DevExpress.Data;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Managers;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.KursReferences;
using KursAM2.View.Logistiks;
using KursAM2.ViewModel.Reference.Nomenkl;
using KursDomain;
using KursDomain.Documents.Base;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.IDocuments.Finance;
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.RepositoryHelper;
using NomenklMain = Data.NomenklMain;


namespace KursAM2.ViewModel.Logistiks
{
    public sealed class SkladOstatkiWindowViewModel : RSWindowViewModelBase
    {
        private readonly ImageSource KontrImage;
        private readonly InvoiceClientRepository myInvoiceClientRepository;
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());

        private readonly ImageSource StoreImage;
        private bool IsRightOnNomenklReference;
        private IInvoiceClient myCurrentInvoice;

        private NomenklOstatkiForSklad myCurrentNomenklForSklad;
        private NomenklOstatkiWithPrice myCurrentNomenklStore;
        private NomenklCalcCostOperation myCurrentOperation;
        private KursDomain.References.Warehouse myCurrentWarehouse;
        private bool myIsPeriodSet;
        private DateTime myOstatokDate;
        private IniFileManager iniFile;

        public SkladOstatkiWindowViewModel()
        {
            var iniFileName = Application.Current.Properties["DataPath"] + "\\User.ini";
            try
            {
                iniFile = new IniFileManager(iniFileName);
                InitIniFile(iniFile);
            }
            catch
            {
                if (File.Exists(iniFileName)) File.Delete(iniFileName);
                iniFile = new IniFileManager(iniFileName);
                InitIniFile(iniFile);
            }

            WindowName = "Остатки товаров на складах";

            myInvoiceClientRepository = new InvoiceClientRepository(GlobalOptions.GetEntities());

            var svgToImageSource = new SvgImageSourceExtension
            {
                Uri = new Uri(
                    "pack://application:,,,/DevExpress.Images.v23.2;component/SvgImages/Business Objects/BO_Vendor.svg")
            };
            KontrImage = (ImageSource)svgToImageSource.ProvideValue(null);

            svgToImageSource.Uri = new Uri(
                "pack://application:,,,/DevExpress.Images.v23.2;component/SvgImages/Business Objects/BO_Order.svg");
            StoreImage = (ImageSource)svgToImageSource.ProvideValue(null);

            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
            });
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            myIsPeriodSet = false;
            myOstatokDate = DateTime.Today;
            getRightOnNomenklReference();
            RefreshReferences();
        }

        public override void AddSearchList(object obj)
        {
            var ctxost = new SkladOstatkiWindowViewModel();
            var form = new SkladOstatkiView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctxost
            };
            ctxost.Form = form;
            form.Show(); 
        }

        private void InitIniFile(IniFileManager userIni)
        {
            if (!userIni.KeyExists("SkaldOstatkiReceiverName", "Layot"))
                userIni.Write("Layout", "SkaldOstatkiReceiverName", "5");
        }

        public override string LayoutName => "SkladOstatkiWindowViewModelLayout";

        public IInvoiceClient CurrentInvoice
        {
            set
            {
                if (Equals(value, myCurrentInvoice)) return;
                myCurrentInvoice = value;
                RaisePropertyChanged();
            }
            get => myCurrentInvoice;
        }

        public ObservableCollection<InvoiceClientRemains> InvoiceClientList { set; get; } =
            new ObservableCollection<InvoiceClientRemains>();

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<KursDomain.References.Warehouse> Sklads { set; get; } =
            new ObservableCollection<KursDomain.References.Warehouse>();

        public List<NomPrice> Prices { set; get; } = new List<NomPrice>();

        //public ObservableCollection<OperationNomenklMove> NomenklOperationsForSklad { set; get; } =
        //    new ObservableCollection<OperationNomenklMove>();

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
                //else
                //{
                //    NomenklOperationsForSklad.Clear();
                //    RaisePropertyChanged(nameof(NomenklOperationsForSklad));
                //}

                RaisePropertyChanged();
            }
        }

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
                loadInvoices();
                RaisePropertyChanged(nameof(NomenklOperations));
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<SkladQuantity> NomenklOnSklads { set; get; } =
            new ObservableCollection<SkladQuantity>();

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


        public ICommand NomenklMainCardOpenCommand
        {
            get { return new Command(NomenklMainCardOpen, _ => IsRightOnNomenklReference); }
        }

        public ICommand InvoiceOpenCommand
        {
            get { return new Command(InvoiceOpen, _ => CurrentInvoice != null); }
        }


        private void RefreshReferences()
        {
            Sklads.Clear();
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

        private void updateSklads()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql = "SELECT DISTINCT storeDC FROM NomenklMoveForCalc nmfc " +
                          "INNER JOIN HD_27 h ON h.DOC_CODE = nmfc.StoreDC " +
                          "INNER JOIN  EXT_USERS U ON U.USR_ID = H.USR_ID " +
                          $"AND UPPER(U.USR_NICKNAME) = UPPER('{GlobalOptions.UserInfo.NickName}')";
                var skls = ctx.Database.SqlQuery<decimal>(sql);
                foreach (var dc in skls)
                {
                    if (Sklads.Any(_ => _.DocCode == dc)) continue;
                    Sklads.Add(GlobalOptions.ReferencesCache.GetWarehouse(dc) as KursDomain.References.Warehouse);
                }
            }
        }


        public override void RefreshData(object obj)
        {
            NomenklsForSklad.Clear();
            LoadedRemains.Clear();
            //LoadedRemains = NomenklCalculationManager.GetNomenklStoreRemains(OstatokDate, true);
            if (CurrentWarehouse != null) LoadNomForSklad();
            updateSklads();
            RaisePropertyChanged(nameof(NomenklsForSklad));
        }

        private void getRightOnNomenklReference()
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                IsRightOnNomenklReference = ctx.UserMenuRight.Any(_ => _.LoginName == GlobalOptions.UserInfo.NickName
                                                                       && _.DBId == GlobalOptions.DataBaseId);
            }
        }

        private void NomenklMainCardOpen(object obj)
        {
            NomenklMain nm;
            using (var context = GlobalOptions.GetEntities())
            {
                nm = context.NomenklMain
                    .Include(_ => _.SD_119)
                    .Include(_ => _.SD_175)
                    .Include(_ => _.SD_82)
                    .Include(_ => _.SD_83)
                    .Include(_ => _.SD_50)
                    .Include(_ => _.Countries)
                    .FirstOrDefault(_ => _.Id == CurrentNomenklStore.Nomenkl.MainId);
            }

            var ctx = new MainCardWindowViewModel
            {
                ParentReference = null,
                NomenklMain = new NomenklMainViewModel(nm)
            };
            // ReSharper disable once UseObjectOrCollectionInitializer
            var form = new NomenklMainCardView { Owner = Application.Current.MainWindow };
            form.Show();
            form.DataContext = ctx;
        }


        private void loadInvoices()
        {
            InvoiceClientList.Clear();
            if (CurrentNomenklStore.Reserved == 0) return;
            using (var context = GlobalOptions.GetEntities())
            {
                var sql = $@"SELECT distinct DocCode FROM (
                            SELECT DISTINCT
                                    t84.DOC_CODE                                                         DocCode,
                                    t84.CODE                                                             Code,
                                    t84.SFT_NEMENKL_DC                                                   NomDC,
                                    CAST(SFT_KOL AS NUMERIC(18, 0)) - SUM(ISNULL(t24.DDT_KOL_RASHOD, 0)) Reserved
                            FROM
                                    TD_84 t84
                                LEFT OUTER JOIN
                                  TD_24 t24
                                    ON t84.DOC_CODE = t24.DDT_SFACT_DC
                                      AND t84.CODE = t24.DDT_SFACT_ROW_CODE
                                INNER JOIN
                                  SD_83 s83
                                    ON s83.DOC_CODE = t84.SFT_NEMENKL_DC
                                      AND s83.NOM_0MATER_1USLUGA = 0
                             AND t84.SFT_NEMENKL_DC = {CustomFormat.DecimalToSqlDecimal(CurrentNomenklStore.Nomenkl.DocCode)}
                            GROUP BY
                                    t84.DOC_CODE,
                                    t84.CODE,
                                    t84.SFT_NEMENKL_DC,
                                    SFT_KOL
                            HAVING
                                    CAST(SFT_KOL AS NUMERIC(18, 0)) - SUM(ISNULL(t24.DDT_KOL_RASHOD, 0)) > 0) tab";

                var res = context.Database.SqlQuery<decimal>(sql).ToList();
                foreach (var item in myInvoiceClientRepository.GetByDocCodes(res))
                {
                    var newItem = new InvoiceClientRemains(item);
                    var row = context.TD_84.FirstOrDefault(_ =>
                        _.DOC_CODE == item.DocCode && _.SFT_NEMENKL_DC == CurrentNomenklStore.Nomenkl.DocCode);
                    newItem.NomQuantity = (decimal)(row?.SFT_KOL ?? 0); 
                    InvoiceClientList.Add(newItem);
                }
            }
        }

        private void InvoiceOpen(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.InvoiceClient, CurrentInvoice.DocCode);
        }


        private void LoadNomForSklad()
        {
            NomenklOperations.Clear();
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
                        SummaWONaklad = d.OstatokSumma,
                        Reserved = d.Reserved
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
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.StoreOrderIn, (decimal)CurrentOperation.TovarDocDC);
                    break;
                case 3:
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
                    // ReSharper disable once PossibleInvalidOperationException
                    DocumentsOpenManager.Open(DocumentType.InvoiceProvider, (decimal)CurrentOperation.FinDocumentDC);
                    break;
                case 1007:
                    DocumentsOpenManager.Open(
                        DocumentType.NomenklReturnOfClient, CurrentOperation.Id);
                    break;
                case 1008:
                    DocumentsOpenManager.Open(
                        DocumentType.NomenklReturnToProvider, CurrentOperation.Id);
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
                    // ReSharper disable once PossibleInvalidOperationException
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
            if (CurrentWarehouse == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var clc = new NomenklCostMediumSliding(ctx);
                NomenklOperations.Clear();
                var data = clc.GetOperations(CurrentNomenklStore.Nomenkl.DocCode, false, CurrentWarehouse.DocCode);
                if (data == null || data.Count <= 0) return;
                decimal nakop = 0;
                foreach (var op in data.Where(_ => _.SkladIn?.DocCode == CurrentWarehouse.DocCode
                                                   || _.SkladOut?.DocCode == CurrentWarehouse.DocCode)
                             .OrderBy(_ => _.DocDate).ThenByDescending(_ => _.QuantityIn))
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

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);
              UpdateVisualObjects();
        }

        public override void UpdateVisualObjects()
        {
            if (Form is SkladOstatkiView frm)
            {
                frm.nomenklskladGridControl.TotalSummary.Clear();
                foreach (var col in frm.nomenklskladGridControl.Columns)
                {
                    var calcEd = col.EditSettings as CalcEditSettings;
                    switch (col.FieldName)
                    {
                        case "State":
                            col.Visible = false;
                            break;
                        case nameof(NomenklOstatkiWithPrice.Name):
                            frm.nomenklskladGridControl.TotalSummary.Add(new GridSummaryItem
                            {
                                FieldName = col.FieldName,
                                SummaryType = SummaryItemType.Count,
                                DisplayFormat = "{0:n0}"
                            });
                            break;

                        case nameof(NomenklOstatkiWithPrice.Prihod):
                        case nameof(NomenklOstatkiWithPrice.Rashod):
                            if (calcEd != null)
                                calcEd.DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat();
                            break;

                        case nameof(NomenklOstatkiWithPrice.Quantity):
                            if (calcEd != null)
                                calcEd.DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat();
                            else
                                col.EditSettings = new CalcEditSettings
                                {
                                    DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat(),
                                    AllowDefaultButton = false,
                                   };
                            
                            frm.nomenklskladGridControl.TotalSummary.Add(new GridSummaryItem
                            {
                                FieldName = col.FieldName,
                                SummaryType = SummaryItemType.Sum,
                                DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat()
                            });
                            break;
                         case nameof(NomenklOstatkiWithPrice.StockLevel):
                             if (calcEd != null)
                                 calcEd.DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat();
                             else
                                 col.EditSettings = new CalcEditSettings
                                 {
                                     DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat(),
                                     AllowDefaultButton = false
                                 };

                             var notShippedFormatCondition = new FormatCondition
                             {
                                 //Expression = "[SummaFact] < [Summa]",
                                 FieldName = "StockLevel",
                                 ApplyToRow = false,
                                 Format = new Format
                                 {
                                     Foreground = Brushes.Red
                                 },
                                 ValueRule = ConditionRule.Less,
                                 Value1 = 0m
                             };
                             frm.tableOstatkiView.FormatConditions.Add(notShippedFormatCondition);
                             frm.nomenklskladGridControl.TotalSummary.Add(new GridSummaryItem
                             {
                                 FieldName = col.FieldName,
                                 SummaryType = SummaryItemType.Sum,
                                 DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat()
                             });

                             break;
                       case nameof(NomenklOstatkiWithPrice.Reserved):
                            if (calcEd != null)
                                calcEd.DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat();
                            else
                                col.EditSettings = new CalcEditSettings
                                {
                                    DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat(),
                                    AllowDefaultButton = false
                                };
                            frm.nomenklskladGridControl.TotalSummary.Add(new GridSummaryItem
                            {
                                FieldName = col.FieldName,
                                SummaryType = SummaryItemType.Sum,
                                DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat()
                            });

                            break;
                        case nameof(NomenklOstatkiWithPrice.Summa):
                        case nameof(NomenklOstatkiWithPrice.SummaWONaklad):
                            if (calcEd != null)
                                calcEd.DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat();
                            else
                                col.EditSettings = new CalcEditSettings
                                {
                                    DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat(),
                                    AllowDefaultButton = false
                                };
                            frm.nomenklskladGridControl.TotalSummary.Add(new GridSummaryItem
                            {
                                FieldName = col.FieldName,
                                SummaryType = SummaryItemType.Sum,
                                DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat()
                            });
                            break;
                        case nameof(NomenklOstatkiWithPrice.Price):
                        case nameof(NomenklOstatkiWithPrice.PriceWONaklad):
                            if (calcEd != null)
                                calcEd.DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat();
                            else
                                col.EditSettings = new CalcEditSettings
                                {
                                    DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat(),
                                    AllowDefaultButton = false
                                };
                            break;
                    }
                }

                var sCol = KursGridControlHelper.GetSummaryForField(frm.invoiceClientGridControl,
                    "NomQuantity");
                if (sCol == null)
                {
                    frm.invoiceClientGridControl.TotalSummary.Add(new GridSummaryItem()
                    {
                        FieldName = "NomQuantity",
                        DisplayFormat = "{0:n2}",
                        SummaryType = SummaryItemType.Sum
                    });
                }
                var nomQCol = KursGridControlHelper.GetColumnForField(frm.invoiceClientGridControl,
                    "NomQuantity");
                nomQCol.EditSettings = new CalcEditSettings()
                {
                    AllowDefaultButton = false,
                    DisplayFormat = "{0:n2}",

                };

                var colIndex = Convert.ToInt32(iniFile.ReadINI("Layout", "SkaldOstatkiReceiverName"));

                var recNameCol = KursGridControlHelper.GetColumnForField(frm.nomenklskladOperGridControl,
                    "SenderReceiverName");
                recNameCol.VisibleIndex = colIndex;
                frm.nomenklskladOperGridControl.TotalSummary.Clear();
                foreach (var col in frm.nomenklskladOperGridControl.Columns)
                {
                    var calcEd = col.EditSettings;
                    switch (col.FieldName)
                    {
                        case nameof(NomenklCalcCostOperation.OperationName):
                            frm.nomenklskladOperGridControl.TotalSummary.Add(new GridSummaryItem
                            {
                                FieldName = col.FieldName,
                                SummaryType = SummaryItemType.Count,
                                DisplayFormat="n0"
                            });
                            break;
                        case nameof(NomenklCalcCostOperation.QuantityIn):
                        case nameof(NomenklCalcCostOperation.QuantityOut):
                            if (calcEd != null)
                                calcEd.DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat();
                            frm.nomenklskladOperGridControl.TotalSummary.Add(new GridSummaryItem
                            {
                                FieldName = col.FieldName,
                                SummaryType = SummaryItemType.Sum,
                                DisplayFormat = GlobalOptions.SystemProfile.GetQuantityValueNumberFormat()
                            });
                            break;
                        case nameof(NomenklCalcCostOperation.SummaIn):
                        case nameof(NomenklCalcCostOperation.SummaOut):
                        case nameof(NomenklCalcCostOperation.SummaInWithNaklad):
                        case nameof(NomenklCalcCostOperation.SummaOutWithNaklad):
                            if (calcEd != null)
                                calcEd.DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat();
                            frm.nomenklskladOperGridControl.TotalSummary.Add(new GridSummaryItem
                            {
                                FieldName = col.FieldName,
                                SummaryType = SummaryItemType.Sum,
                                DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat()
                            });
                            break;
                        case nameof(NomenklCalcCostOperation.CalcPrice):
                        case nameof(NomenklCalcCostOperation.CalcPriceNaklad):
                        case nameof(NomenklCalcCostOperation.DocPrice):
                            if (calcEd != null)
                                calcEd.DisplayFormat = GlobalOptions.SystemProfile.GetCurrencyFormat();
                            break;
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

    public class NomenklOstatkiForSklad : RSViewModelBase
    {
        [Display(AutoGenerateField = true, Name = "Ном.№", Order = 0)]
        public string NomenklNumber => Nomenkl?.NomenklNumber;

        [Display(AutoGenerateField = true, Name = "Наименование", Order = 1)]
        public override string Name => Nomenkl?.Name;

        [Display(AutoGenerateField = true, Name = "Валюта", Order = 2)]
        public string CurrencyName { set; get; }

        [Display(AutoGenerateField = true, Name = "Наличие", Order = 3)]
        public decimal Quantity { set; get; }

        [Display(AutoGenerateField = false)] public Nomenkl Nomenkl { set; get; }

        [Display(AutoGenerateField = false)] public string NomenklName => Nomenkl?.Name;

        [Display(AutoGenerateField = false)] public KursDomain.References.Warehouse Warehouse { set; get; }

        [Display(AutoGenerateField = false)] public string StoreName => Warehouse?.Name;

        [Display(AutoGenerateField = false, Name = "Приход")]
        public decimal Prihod { set; get; }

        [Display(AutoGenerateField = false, Name = "Расход")]
        public decimal Rashod { set; get; }

        [Display(AutoGenerateField = false, Name = "Остаток на начало")]
        public decimal StartQuantity { set; get; }

        [Display(AutoGenerateField = true, Name = "Вид продукции", Order = 7)]
        public ProductType NomenklProductType => Nomenkl?.ProductType as ProductType;

        [Display(AutoGenerateField = true, Name = "Тип номенклатуры", Order = 6)]
        public NomenklType NomenklType => Nomenkl?.NomenklType as NomenklType;
    }

    public class NomenklOstatkiWithPrice : NomenklOstatkiForSklad
    {
        [Display(AutoGenerateField = true, Name = "Цена (б/н)", Order = 8)]
        public decimal PriceWONaklad { set; get; }

        [Display(AutoGenerateField = true, Name = "Цена", Order = 10)]
        public decimal Price { set; get; }

        [Display(AutoGenerateField = true, Name = "Сумма  (б/н)", Order = 9)]
        public decimal SummaWONaklad { set; get; }

        [Display(AutoGenerateField = true, Name = "Сумма", Order = 11)]
        public decimal Summa { set; get; }

        [Display(AutoGenerateField = false)] public decimal SummaIn { set; get; }

        [Display(AutoGenerateField = false)] public decimal SummaInWONaklad { set; get; }

        [Display(AutoGenerateField = false)] public decimal SummaOut { set; get; }

        [Display(AutoGenerateField = false)] public decimal SummaOutWONaklad { set; get; }

        [Display(AutoGenerateField = false)] public decimal Result { set; get; }

        [Display(AutoGenerateField = false)] public decimal ResultWONaklad { set; get; }

        [Display(AutoGenerateField = true, Name = "Зарезервировано", Order = 4)]
        public decimal Reserved { set; get; }

        [Display(AutoGenerateField = true, Name = "Остаток", Order = 5)]
        public decimal StockLevel => Quantity - Reserved;
    }
}
