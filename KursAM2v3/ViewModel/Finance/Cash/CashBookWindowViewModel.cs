using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Core;
using Core.EntityViewModel.Cash;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Grid;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.View.Finance.Cash;

namespace KursAM2.ViewModel.Finance.Cash
{
    public sealed class CashBookWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private Core.EntityViewModel.Cash.Cash myCurrentCash;
        private CashBookDocument myCurrentDocument;
        private DatePeriod myCurrentPeriod;
        private bool myIsPeriodEnabled;
        private ObservableCollection<DatePeriod> myPeriods;

        #endregion

        #region Constructors

        public CashBookWindowViewModel()
        {
            WindowName = "Кассовая книга";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = SetMenuBar();
            Periods = new ObservableCollection<DatePeriod>();
            IsPeriodEnabled = true;
            LayoutName = "CashBookWindow";
        }

        public CashBookWindowViewModel(decimal cashDC) : this()
        {
            CurrentCash = MainReferences.Cashs[cashDC];
            WindowName = WindowName + " " + CurrentCash.Name;
        }

        #endregion

        #region Properties

        public ObservableCollection<Core.EntityViewModel.Cash.Cash> CashList { set; get; }
            = new ObservableCollection<Core.EntityViewModel.Cash.Cash>(
                MainReferences.Cashs.Values.Where(_ => _.IsAccessRight));

        public ObservableCollection<CashBookDocument> Documents { set; get; }
            = new ObservableCollection<CashBookDocument>();

        public ObservableCollection<MoneyRemains> MoneyRemains { set; get; } = new ObservableCollection<MoneyRemains>();

        public ObservableCollection<DatePeriod> Periods
        {
            get => myPeriods;
            set
            {
                if (myPeriods == value) return;
                myPeriods = value;
                RaisePropertyChanged();
            }
        }

        public CashBookDocument CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument != null && myCurrentDocument.Equals(value)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        public Core.EntityViewModel.Cash.Cash CurrentCash
        {
            get => myCurrentCash;
            set
            {
                if (myCurrentCash != null && myCurrentCash.Equals(value)) return;
                myCurrentCash = value;
                WindowName = "Кассовая книга - " + CurrentCash?.Name;
                RefreshData(null);
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(WindowName));
                RaisePropertyChanged(nameof(IsSearchEnabled));
            }
        }

        public DatePeriod CurrentPeriod
        {
            get => myCurrentPeriod;
            set
            {
                if (myCurrentPeriod != null && myCurrentPeriod.Equals(value)) return;
                myCurrentPeriod = value;
                if (CurrentPeriod != null)
                {
                    LoadDocuments();
                    GetRemains();
                }

                RaisePropertyChanged();
            }
        }

        private void GetRemains()
        {
            MoneyRemains.Clear();
            if (CurrentPeriod == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var crs in ctx.TD_22.Where(_ => _.DOC_CODE == CurrentCash.DocCode).Select(_ => _.CRS_DC)
                    .ToList())
                {
                    var d1 = ctx.TD_22.Where(_ => _.DOC_CODE == CurrentCash.DocCode && _.CRS_DC == crs)
                        .Select(_ => _.SUMMA_START).ToList();
                    var dIn = ctx.SD_33.Where(_ => _.CA_DC == CurrentCash.DOC_CODE && _.CRS_DC == crs
                            && _.DATE_ORD >=
                            CurrentPeriod.DateStart &&
                            _.DATE_ORD <=
                            CurrentPeriod.DateEnd)
                        .Select(_ => _.SUMM_ORD).ToList();
                    var dIn1 = ctx.SD_33.Where(_ => _.CA_DC == CurrentCash.DOC_CODE && _.CRS_DC == crs
                            && _.DATE_ORD <
                            CurrentPeriod.DateStart)
                        .Select(_ => _.SUMM_ORD).ToList();
                    var dOut = ctx.SD_34.Where(_ =>
                            _.CA_DC == CurrentCash.DOC_CODE && _.CRS_DC == crs && _.DATE_ORD >=
                            CurrentPeriod.DateStart &&
                            _.DATE_ORD <=
                            CurrentPeriod.DateEnd)
                        .Select(_ => _.SUMM_ORD).ToList();
                    var dOut1 = ctx.SD_34.Where(_ =>
                            _.CA_DC == CurrentCash.DOC_CODE && _.CRS_DC == crs &&
                            _.DATE_ORD <
                            CurrentPeriod.DateStart)
                        .Select(_ => _.SUMM_ORD).ToList();
                    var dCrs1 = ctx.SD_251
                        .Where(_ => _.CH_CASH_DC == CurrentCash.DOC_CODE && _.CH_CRS_IN_DC == crs
                                                                         && _.CH_DATE >=
                                                                         CurrentPeriod.DateStart &&
                                                                         _.CH_DATE <=
                                                                         CurrentPeriod.DateEnd)
                        .Select(_ => _.CH_CRS_IN_SUM).ToList();
                    var dCrs2 = ctx.SD_251
                        .Where(_ => _.CH_CASH_DC == CurrentCash.DOC_CODE && _.CH_CRS_OUT_DC == crs
                                                                         && _.CH_DATE >=
                                                                         CurrentPeriod.DateStart &&
                                                                         _.CH_DATE <=
                                                                         CurrentPeriod.DateEnd)
                        .Select(_ => _.CH_CRS_OUT_SUM).ToList();
                    var dCrs11 = ctx.SD_251
                        .Where(_ => _.CH_CASH_DC == CurrentCash.DOC_CODE && _.CH_CRS_IN_DC == crs
                                                                         && _.CH_DATE <
                                                                         CurrentPeriod.DateStart)
                        .Select(_ => _.CH_CRS_IN_SUM).ToList();
                    var dCrs21 = ctx.SD_251
                        .Where(_ => _.CH_CASH_DC == CurrentCash.DOC_CODE && _.CH_CRS_OUT_DC == crs
                                                                         && _.CH_DATE <
                                                                         CurrentPeriod.DateStart)
                        .Select(_ => _.CH_CRS_OUT_SUM).ToList();
                    var p1 = d1.Sum() + dIn1.Sum()
                             - dOut1.Sum()
                             + dCrs11.Sum()
                             - dCrs21.Sum();
                    // ReSharper disable once PossibleInvalidOperationException
                    var p2 = (decimal) (dIn == null ? 0 : dIn.Sum()) +
                             dCrs1.Sum();
                    var p3 = dOut.Sum()
                             + dCrs2.Sum();
                    MoneyRemains.Add(new MoneyRemains
                        {
                            CurrencyName = MainReferences.Currencies[crs].Name,
                            // ReSharper disable PossibleInvalidOperationException
                            Start = (decimal) p1,
                            In = (decimal) p2,
                            Out = (decimal) p3,
                            End = (decimal) (p1 + p2 - p3)
                            // ReSharper restore PossibleInvalidOperationException
                        }
                    );
                }
            }
        }

        private void LoadDocuments()
        {
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

            Documents.Clear();
            foreach (var d in CashManager.LoadDocuments(CurrentCash, CurrentPeriod.DateStart, CurrentPeriod.DateEnd))
                Documents.Add(d);
        }

        private void LoadDocuments(string searchtext)
        {
            Documents.Clear();
            foreach (var d in CashManager.LoadDocuments(CurrentCash, CurrentPeriod.DateStart, CurrentPeriod.DateEnd,
                searchtext))
                Documents.Add(d);
        }

        public override string SearchText
        {
            get => mySearchText;
            set
            {
                if (mySearchText == value) return;
                mySearchText = value;
                IsPeriodEnabled = string.IsNullOrEmpty(mySearchText);
                RaisePropertyChanged();
            }
        }

        public bool IsPeriodEnabled
        {
            get => myIsPeriodEnabled;
            set
            {
                if (myIsPeriodEnabled == value) return;
                myIsPeriodEnabled = value;
                if (myIsPeriodEnabled)
                {
                    if (CurrentPeriod != null)
                    {
                        LoadDocuments(SearchText);
                        GetRemains();
                    }
                    else
                    {
                        Documents.Clear();
                    }
                }
                else
                {
                    Documents.Clear();
                    MoneyRemains.Clear();
                }

                RaisePropertyChanged();
            }
        }

        public bool IsSearchEnabled => CurrentCash != null;

        #endregion

        #region Commands

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        public override void DocumentOpen(object obj)
        {
            try
            {
                switch (CurrentDocument.DocumnetTypeName)
                {
                    case "Приходный ордер":
                        DocumentsOpenManager.Open(DocumentType.CashIn, CurrentDocument.DocCode, null, Form);
                        break;
                    case "Расходный ордер":
                        DocumentsOpenManager.Open(DocumentType.CashOut, CurrentDocument.DocCode, null, Form);
                        break;
                    case "Обмен валюты":
                        DocumentsOpenManager.Open(DocumentType.CurrencyChange, CurrentDocument.DocCode, null, Form);
                        break;
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override bool IsDocNewCopyAllow => CurrentDocument != null;
        public override bool IsDocNewEmptyAllow => true;

        public override void DocNewCopyRequisite(object form)
        {
            if (CurrentDocument.DocumnetTypeName == "Приходный ордер")
            {
                var vm = new CashInWindowViewModel
                {
                    Document = CashManager.NewRequisiteCashIn(CurrentDocument.DocCode)
                };
                vm.Document.Cash = CurrentCash;
                DocumentsOpenManager.Open(DocumentType.CashIn, vm, Form);
            }

            if (CurrentDocument.DocumnetTypeName == "Расходный ордер")
            {
                var vm = new CashOutWindowViewModel
                {
                    Document = CashManager.NewRequisisteCashOut(CurrentDocument.DocCode)
                };
                vm.Document.Cash = CurrentCash;
                DocumentsOpenManager.Open(DocumentType.CashOut, vm, Form);
            }
        }

        public override void DocNewCopy(object form)
        {
            if (CurrentDocument.DocumnetTypeName == "Приходный ордер")
            {
                var vm = new CashInWindowViewModel
                {
                    Document = CashManager.NewCopyCashIn(CurrentDocument.DocCode)
                };
                vm.Document.Cash = CurrentCash;
                DocumentsOpenManager.Open(DocumentType.CashIn, vm, Form);
            }

            if (CurrentDocument.DocumnetTypeName == "Расходный ордер")
            {
                var vm = new CashOutWindowViewModel
                {
                    Document = CashManager.NewCopyCashOut(CurrentDocument.DocCode)
                };
                vm.Document.Cash = CurrentCash;
                DocumentsOpenManager.Open(DocumentType.CashOut, vm, Form);
            }
        }

        public override void DocNewEmpty(object form)
        {
            if (form != null && form is string)
            {
                var code = (string) form;
                switch (code)
                {
                    case "0":
                        var vm = new CashInWindowViewModel
                        {
                            Document = CashManager.NewCashIn()
                        };
                        vm.Document.Cash = CurrentCash;
                        DocumentsOpenManager.Open(DocumentType.CashIn, vm, Form);
                        break;
                    case "1":
                        var vm1 = new CashOutWindowViewModel
                        {
                            Document = CashManager.NewCashOut()
                        };
                        vm1.Document.Cash = CurrentCash;
                        DocumentsOpenManager.Open(DocumentType.CashOut, vm1, Form);
                        break;
                    case "2":
                        var vm2 = new CashCurrencyExchangeWindowViewModel
                        {
                            Document = CashManager.NewCashCurrencyEchange()
                        };
                        vm2.Document.Cash = CurrentCash;
                        DocumentsOpenManager.Open(DocumentType.CurrencyChange, vm2, Form);
                        break;
                }
            }
        }

        public override bool IsCanSearch => !string.IsNullOrWhiteSpace(SearchText);

        public override void Search(object obj)
        {
            LoadDocuments(SearchText);
        }

        public override void SearchClear(object obj)
        {
            SearchText = null;
            RefreshActual();
        }

        public ICommand CashSetRemainsCommand
        {
            get { return new Command(CashSetRemains, _ => CurrentCash != null); }
        }

        private void CashSetRemains(object obj)
        {
            var updData = StandartDialogs.SetCashRemains(CurrentCash);
            if (updData == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var d in updData)
                        {
                            if (d.Currency == null) continue;
                            var old = ctx.TD_22.FirstOrDefault(_ =>
                                _.DOC_CODE == d.DOC_CODE && _.CRS_DC == d.Currency.DocCode);
                            if (old == null)
                            {
                                var newCode = ctx.TD_22.Max(_ => _.CODE) + 1;
                                var newItem = new TD_22
                                {
                                    DOC_CODE = CurrentCash.DocCode,
                                    CODE = newCode,
                                    CRS_DC = d.CRS_DC,
                                    DATE_START = d.DATE_START,
                                    SUMMA_START = d.SUMMA_START
                                };
                                ctx.TD_22.Add(newItem);
                            }
                            else
                            {
                                old.DATE_START = d.DATE_START;
                                old.SUMMA_START = d.SUMMA_START;
                            }

                            ctx.SaveChanges();
                            transaction.Commit();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public ICommand CashInNewCommand
        {
            get { return new Command(CashInNew, _ => true); }
        }

        public ICommand CashOutNewCommand
        {
            get { return new Command(CashOutNew, _ => true); }
        }

        public ICommand CashExchangeCurrencyCommand
        {
            get { return new Command(CashEchangeCurrencyNew, _ => true); }
        }

        public ICommand PeriodTreeClickCommand
        {
            get { return new Command(PeriodTreeClick, _ => true); }
        }

        private void PeriodTreeClick(object obj)
        {
            if (IsPeriodEnabled) return;
            SearchText = null;
            IsPeriodEnabled = true;
        }

        #endregion

        #region Methods

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            if (CurrentCash == null) return;
            MoneyRemains.Clear();
            Documents.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var dates = new List<DateTime>();
                    var d1 = ctx.TD_22.Where(_ => _.DOC_CODE == CurrentCash.DocCode).Select(_ => _.DATE_START);
                    var dIn = ctx.SD_33.Where(_ => _.CA_DC == CurrentCash.DOC_CODE).Select(_ => _.DATE_ORD).Distinct();
                    var dOut = ctx.SD_34.Where(_ => _.CA_DC == CurrentCash.DOC_CODE).Select(_ => _.DATE_ORD).Distinct();
                    var dCrs = ctx.SD_251.Where(_ => _.CH_CASH_DC == CurrentCash.DOC_CODE).Select(_ => _.CH_DATE)
                        .Distinct();
                    foreach (var i in d1) dates.Add(i);
                    foreach (var i in dIn)
                        if (dates.All(_ => _ != i))
                            // ReSharper disable once PossibleInvalidOperationException
                            dates.Add((DateTime) i);
                    foreach (var i in dOut)
                        if (dates.All(_ => _ != i))
                            // ReSharper disable once PossibleInvalidOperationException
                            dates.Add((DateTime) i);
                    foreach (var i in dCrs)
                        if (dates.All(_ => _ != i))
                            dates.Add(i);
                    Periods = new ObservableCollection<DatePeriod>(
                        DatePeriod.GenerateIerarhy(dates, PeriodIerarhy.YearMonthDay)
                            .OrderByDescending(_ => _.DateStart));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        /// <summary>
        ///     Обновляет данные при изменении документов,
        ///     открытых из данной кассы
        /// </summary>
        public void RefreshActual(RSViewModelBase vm)
        {
            if (vm == null) return;
            var cashOut = vm as CashOut;
            var cashExch = vm as CashCurrencyExchange;
            // ReSharper disable once PossibleInvalidOperationException
            var date = (DateTime) (vm is CashIn cashIn ? cashIn.DATE_ORD :
                cashOut != null ? cashOut.DATE_ORD :
                cashExch?.CH_DATE ?? DateTime.Today);
            RefreshData(null);
            if (!(Form is CashBookView frm)) return;
            var iteratorYear = new TreeListNodeIterator(frm.tableViewPeriods.Nodes, false);
            while (iteratorYear.MoveNext())
            {
                if (iteratorYear.Current == null) continue;
                if (!(iteratorYear.Current.Content is DatePeriod year)) return;
                if (year.DateStart.Year != date.Year) continue;
                iteratorYear.Current.IsExpanded = true;
                var iteratorMonth = new TreeListNodeIterator(iteratorYear.Current, false);
                while (iteratorMonth.MoveNext())
                {
                    var month = (DatePeriod) iteratorYear.Current.Content;
                    if (month.DateStart.Month == date.Month)
                    {
                        if (iteratorMonth.Current != null) iteratorMonth.Current.IsExpanded = true;
                        else return;
                        var iteratorDay = new TreeListNodeIterator(iteratorMonth.Current, false);
                        while (iteratorDay.MoveNext())
                        {
                            if (!(iteratorDay.Current?.Content is DatePeriod day) || date != day.DateStart)
                                continue;
                            frm.gridPeriods.CurrentItem = day;
                            frm.gridPeriods.SelectedItem = day;
                        }
                    }
                }
            }
        }

        public void RefreshActual()
        {
            if (CurrentPeriod == null) return;
            var date = CurrentPeriod.DateStart;
            var dateType = CurrentPeriod.PeriodType;
            RefreshData(null);
            if (!(Form is CashBookView frm)) return;
            var iteratorYear = new TreeListNodeIterator(frm.tableViewPeriods.Nodes, false);
            while (iteratorYear.MoveNext())
            {
                if (iteratorYear.Current == null) continue;
                var year = iteratorYear.Current.Content as DatePeriod;
                if (year == null) return;
                if (year.DateStart.Year != date.Year) continue;
                if (year.PeriodType == dateType)
                {
                    frm.gridPeriods.CurrentItem = year;
                    frm.gridPeriods.SelectedItem = year;
                    return;
                }

                iteratorYear.Current.IsExpanded = true;
                var iteratorMonth = new TreeListNodeIterator(iteratorYear.Current, false);
                while (iteratorMonth.MoveNext())
                {
                    var month = (DatePeriod) iteratorYear.Current.Content;
                    if (month.DateStart.Month != date.Month) continue;
                    if (iteratorMonth.Current != null) iteratorMonth.Current.IsExpanded = true;
                    else return;
                    if (month.PeriodType == dateType)
                    {
                        frm.gridPeriods.CurrentItem = month;
                        frm.gridPeriods.SelectedItem = month;
                        return;
                    }

                    var iteratorDay = new TreeListNodeIterator(iteratorMonth.Current, false);
                    while (iteratorDay.MoveNext())
                    {
                        if (!(iteratorDay.Current?.Content is DatePeriod day) || date != day.DateStart)
                            continue;
                        frm.gridPeriods.CurrentItem = day;
                        frm.gridPeriods.SelectedItem = day;
                        return;
                    }
                }
            }
        }

        public ObservableCollection<MenuButtonInfo> SetMenuBar()
        {
            var prn = new MenuButtonInfo
            {
                Name = "Print",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuPrinter"] as ControlTemplate,
                ToolTip = "Печать"
            };
            var docNew = new MenuButtonInfo
            {
                Name = "New",
                Alignment = Dock.Right,
                HAlignment = HorizontalAlignment.Right,
                Content = Application.Current.Resources["menuDocumentAdd"] as ControlTemplate,
                ToolTip = "Новый документ"
            };
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Приходный кассовый ордер",
                Image = Application.Current.Resources["imageDocumentNewEmpty"] as DrawingImage,
                Command = CashInNewCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Расходный касовый ордер",
                Image = Application.Current.Resources["imageDocumentNewRequisite"] as DrawingImage,
                Command = CashOutNewCommand
            });
            docNew.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Обмен валюты",
                Image = Application.Current.Resources["imageDocumentNewCopy"] as DrawingImage,
                Command = CashExchangeCurrencyCommand
            });
            var ret = new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить список документов",
                    Command = RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuDocumentOpen"] as ControlTemplate,
                    ToolTip = "Открыть выбранный документ",
                    Command = DocumentOpenCommand
                },
                docNew,
                prn,
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть поиск",
                    Command = CloseWindowCommand
                }
            };
            return ret;
        }

        private void CashEchangeCurrencyNew(object obj)
        {
            DocNewEmpty("2");
        }

        private void CashOutNew(object obj)
        {
            DocNewEmpty("1");
        }

        private void CashInNew(object obj)
        {
            DocNewEmpty("0");
        }

        #endregion
    }

    #region CashBookDocument

    [MetadataType(typeof(DataAnnotationsCashBookDocument))]
    public class CashBookDocument : RSViewModelBase
    {
        private string myCurrencyName;
        private DateTime myDocDate;
        private string myDocNum;
        private string myDocumnetTypeName;
        private bool myIsSalary;
        private string myKontragnetName;
        private string myKontragnetTypeName;
        private string myNameOrd;
        private string myOsnOrd;
        private decimal mySummaIn;
        private decimal mySummaOut;

        public string DocumnetTypeName
        {
            get => myDocumnetTypeName;
            set
            {
                if (myDocumnetTypeName == value) return;
                myDocumnetTypeName = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DocDate
        {
            get => myDocDate;
            set
            {
                if (myDocDate == value) return;
                myDocDate = value;
                RaisePropertyChanged();
            }
        }

        public string DocNum
        {
            get => myDocNum;
            set
            {
                if (myDocNum == value) return;
                myDocNum = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaIn
        {
            get => mySummaIn;
            set
            {
                if (mySummaIn == value) return;
                mySummaIn = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaOut
        {
            get => mySummaOut;
            set
            {
                if (mySummaOut == value) return;
                mySummaOut = value;
                RaisePropertyChanged();
            }
        }

        public string CurrencyName
        {
            get => myCurrencyName;
            set
            {
                if (myCurrencyName == value) return;
                myCurrencyName = value;
                RaisePropertyChanged();
            }
        }

        public string KontragnetTypeName
        {
            get => myKontragnetTypeName;
            set
            {
                if (myKontragnetTypeName == value) return;
                myKontragnetTypeName = value;
                RaisePropertyChanged();
            }
        }

        public string KontragnetName
        {
            get => myKontragnetName;
            set
            {
                if (myKontragnetName == value) return;
                myKontragnetName = value;
                RaisePropertyChanged();
            }
        }

        public string NameOrd
        {
            get => myNameOrd;
            set
            {
                if (myNameOrd == value) return;
                myNameOrd = value;
                RaisePropertyChanged();
            }
        }

        public string OsnOrd
        {
            get => myOsnOrd;
            set
            {
                if (myOsnOrd == value) return;
                myOsnOrd = value;
                RaisePropertyChanged();
            }
        }

        public bool IsSalary
        {
            get => myIsSalary;
            set
            {
                if (myIsSalary == value) return;
                myIsSalary = value;
                RaisePropertyChanged();
            }
        }
    }

    public class DataAnnotationsCashBookDocument : DataAnnotationForFluentApiBase, IMetadataProvider<CashBookDocument>
    {
        void IMetadataProvider<CashBookDocument>.BuildMetadata(MetadataBuilder<CashBookDocument> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.DocumnetTypeName).AutoGenerated().DisplayName("Документ");
            builder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата");
            builder.Property(_ => _.DocNum).AutoGenerated().DisplayName("№");
            builder.Property(_ => _.SummaIn).AutoGenerated().DisplayName("Приход");
            builder.Property(_ => _.SummaOut).AutoGenerated().DisplayName("Расход");
            builder.Property(_ => _.CurrencyName).AutoGenerated().DisplayName("Валюта");
            builder.Property(_ => _.KontragnetTypeName).AutoGenerated().DisplayName("Тип плательщика");
            builder.Property(_ => _.KontragnetName).AutoGenerated().DisplayName("Плательщик");
            builder.Property(_ => _.IsSalary).AutoGenerated().DisplayName("Зар./плата");
            builder.Property(_ => _.NameOrd).AutoGenerated().DisplayName("Доп.информация");
            builder.Property(_ => _.OsnOrd).AutoGenerated().DisplayName("Основание");
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
        }
    }

    #endregion

    #region MoneyRemains

    [MetadataType(typeof(DataAnnotationsMoneyRemains))]
    public class MoneyRemains
    {
        public string CurrencyName { set; get; }
        public decimal Start { set; get; }
        public decimal In { set; get; }
        public decimal Out { set; get; }
        public decimal End { set; get; }
    }

    public class DataAnnotationsMoneyRemains : DataAnnotationForFluentApiBase, IMetadataProvider<MoneyRemains>
    {
        void IMetadataProvider<MoneyRemains>.BuildMetadata(MetadataBuilder<MoneyRemains> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.CurrencyName).AutoGenerated().DisplayName("Валюты");
            builder.Property(_ => _.Start).AutoGenerated().DisplayName("Начало");
            builder.Property(_ => _.In).AutoGenerated().DisplayName("Приход");
            builder.Property(_ => _.Out).AutoGenerated().DisplayName("Расход");
            builder.Property(_ => _.End).AutoGenerated().DisplayName("Конец");
        }
    }

    #endregion
}