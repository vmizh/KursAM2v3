using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.ViewModel.MutualAccounting;
using Core.WindowsManager;
using DevExpress.Xpf.Editors;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Base;
using KursAM2.Managers.Invoices;
using KursAM2.View.Finance;
using KursAM2.ViewModel.Management.Calculations;
using static KursAM2.ViewModel.Finance.MutualAccountingDebitorCreditors;

namespace KursAM2.ViewModel.Finance
{
    public sealed class MutualAcountingWindowViewModel : RSWindowViewModelBase
    {
        public readonly MutualAccountingManager Manager = new MutualAccountingManager();
        private MutualAccountingCreditorViewModel myCurrentCreditor;
        private MutualAccountingDebitorViewModel myCurrentDebitor;
        private bool myIsCurrencyConvert;

        public MutualAcountingWindowViewModel()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            DebitorCollection.CollectionChanged += DebitorCollection_CollectionChanged;
            CreditorCollection.CollectionChanged += CreditorCollection_CollectionChanged;
            UpdateVisualData();
        }

        public MutualAcountingWindowViewModel(decimal dc) : this()
        {
            RefreshData(dc);
        }

        public new string WindowName => IsCurrencyConvert ? "Акт конвертации" : "Акт взаимозачета";
        public bool IsTypeVzaimEnabled => !IsCurrencyConvert;

        public decimal CurrencyConvertRate
        {
            get
            {
                if (!IsCurrencyConvert)
                    return 1;
                var sumCreditor = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1).Sum(_ => _.VZT_CRS_SUMMA) ??
                                  0;
                var sumDebitor = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0).Sum(_ => _.VZT_CRS_SUMMA) ?? 0;
                if (Equals(Document.DebitorCurrency, GlobalOptions.SystemProfile.MainCurrency))
                {
                    if (sumDebitor == 0) return 0;
                    return sumCreditor / sumDebitor;
                }

                if (!Equals(Document.CreditorCurrency, GlobalOptions.SystemProfile.MainCurrency)) return 0;
                if (sumCreditor == 0) return 0;
                return sumDebitor / sumCreditor;
            }
        }

        public SD_110ViewModel Document { set; get; }
        public bool IsNotOld => !Document.IsOld;
        public bool IsCanDebitorCrsChanged => DebitorCollection.Count == 0;
        public bool IsCanCreditorCrsChanged => IsCurrencyConvert && CreditorCollection.Count == 0;

        public ObservableCollection<MutualAccountingDebitorViewModel> DebitorCollection { set; get; } =
            new ObservableCollection<MutualAccountingDebitorViewModel>();

        public ObservableCollection<MutualAccountingCreditorViewModel> CreditorCollection { set; get; } =
            new ObservableCollection<MutualAccountingCreditorViewModel>();

        public MutualAccountingDebitorViewModel CurrentDebitor
        {
            get => myCurrentDebitor;
            set
            {
                if (Equals(myCurrentDebitor, value)) return;
                myCurrentDebitor = value;
                RaisePropertyChanged();
            }
        }

        public MutualAccountingCreditorViewModel CurrentCreditor
        {
            get => myCurrentCreditor;
            set
            {
                if (Equals(myCurrentCreditor, value)) return;
                myCurrentCreditor = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCurrencyConvert
        {
            get => myIsCurrencyConvert;
            set
            {
                if (myIsCurrencyConvert == value) return;
                myIsCurrencyConvert = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Зпускает RaisePropertyChanged
        ///     для объектов на экране
        /// </summary>
        public void UpdateVisualData()
        {
            UpdateDebitorCreditorCollections(null);
            UpdateCalcSumma(null);
            RaisePropertyChanged(nameof(IsCurrencyConvert));
            RaisePropertyChanged(nameof(Document));
            RaisePropertyChanged(nameof(DebitorCollection));
            RaisePropertyChanged(nameof(CreditorCollection));
        }

        private void CreditorCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(IsCanCreditorCrsChanged));
        }

        private void DebitorCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(IsCanDebitorCrsChanged));
        }

        public void CreateMenu()
        {
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
        }

        #region Commands

        public ICommand SetDebitorSFCommand
        {
            get { return new Command(SetDebitorSF, _ => true); }
        }

        private void SetCreditorSF(object obj)
        {
            if (CurrentCreditor != null)
            {
                var item = CurrentCreditor.VZT_KONTR_DC > 0
                    ? StandartDialogs.SelectInvoiceProvider(CurrentCreditor.VZT_KONTR_DC, true, true)
                    : StandartDialogs.SelectInvoiceProvider(true, true);
                if (item == null) return;
                CurrentCreditor.VZT_DOC_NUM = (Document.Rows.Count + 1).ToString();
                CurrentCreditor.VZT_CRS_POGASHENO = item.SF_CRS_SUMMA - item.PaySumma;
                CurrentCreditor.VZT_UCH_CRS_POGASHENO = item.SF_CRS_SUMMA - item.PaySumma;
                CurrentCreditor.VZT_CRS_SUMMA = item.SF_CRS_SUMMA - item.PaySumma;
                CurrentCreditor.VZT_KONTR_CRS_SUMMA = item.SF_CRS_SUMMA - item.PaySumma;
                CurrentCreditor.VZT_UCH_CRS_RATE = 1;
                CurrentCreditor.VZT_SFACT_DC = item.DocCode;
                CurrentCreditor.Kontragent = MainReferences.GetKontragent(item.SF_POST_DC);
                CurrentCreditor.SFProvider = item;
                if (CurrentCreditor.State == RowStatus.NotEdited) CurrentCreditor.myState = RowStatus.Edited;
                KontragentManager.UpdateSelectCount(CurrentCreditor.Kontragent.DocCode);
            }
            else
            {
                var item = StandartDialogs.SelectInvoiceProvider(true, true);
                if (item == null) return;
                VzaimoraschetType vzdefault = null;
                var vzdefDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(_ =>
                    _.SECTION == "MUTUAL_ACCOUNTING" && _.ITEM == "DEFAULT_TYPE_PRODUCT");
                if (vzdefDC != null)
                    vzdefault = MainReferences.VzaimoraschetTypes[Convert.ToDecimal(vzdefDC.ITEM_VALUE)];

                var newcred = new MutualAccountingCreditorViewModel
                {
                    DocCode = Document.DocCode,
                    Code = Document.Rows.Count + 1,
                    VZT_DOC_DATE = Document.VZ_DATE,
                    VZT_DOC_NUM = (Document.Rows.Count + 1).ToString(),
                    VZT_1MYDOLZH_0NAMDOLZH = 1,
                    VZT_CRS_POGASHENO = item.SF_CRS_SUMMA - item.PaySumma,
                    VZT_UCH_CRS_POGASHENO = item.SF_CRS_SUMMA - item.PaySumma,
                    VZT_CRS_SUMMA = item.SF_CRS_SUMMA - item.PaySumma,
                    VZT_KONTR_CRS_SUMMA = item.SF_CRS_SUMMA - item.PaySumma,
                    VZT_UCH_CRS_RATE = 1,
                    State = RowStatus.NewRow,
                    VzaimoraschType = vzdefault,
                    Parent = Document,
                    Kontragent = MainReferences.GetKontragent(item.SF_POST_DC),
                    SFProvider = item
                };
                Document.Rows.Add(newcred);
                CreditorCollection.Add(newcred);
                CurrentCreditor = newcred;
                KontragentManager.UpdateSelectCount(newcred.Kontragent.DocCode);
            }

            UpdateVisualData();
        }

        public ICommand SetCreditorSFCommand
        {
            get { return new Command(SetCreditorSF, _ => true); }
        }

        private void SetDebitorSF(object obj)
        {
            if (CurrentDebitor != null)
            {
                var item = CurrentDebitor.VZT_KONTR_DC > 0
                    ? StandartDialogs.SelectInvoiceClient(CurrentCreditor.VZT_KONTR_DC, true, true)
                    : StandartDialogs.SelectInvoiceClient(true, true);
                if (item == null) return;
                CurrentDebitor.VZT_DOC_NUM = (Document.Rows.Count + 1).ToString();
                CurrentDebitor.VZT_CRS_POGASHENO = item.SF_CRS_SUMMA_K_OPLATE - item.PaySumma;
                CurrentDebitor.VZT_UCH_CRS_POGASHENO = item.SF_CRS_SUMMA_K_OPLATE - item.PaySumma;
                CurrentDebitor.VZT_CRS_SUMMA = item.SF_CRS_SUMMA_K_OPLATE - item.PaySumma;
                CurrentDebitor.VZT_KONTR_CRS_SUMMA = -(item.SF_CRS_SUMMA_K_OPLATE - item.PaySumma);
                CurrentDebitor.VZT_UCH_CRS_RATE = 1;
                CurrentDebitor.VZT_SFACT_DC = item.DocCode;
                CurrentDebitor.Kontragent = MainReferences.GetKontragent(item.SF_CLIENT_DC);
                CurrentDebitor.SFClient = item;
                if (CurrentDebitor.State == RowStatus.NotEdited) CurrentDebitor.myState = RowStatus.Edited;
                KontragentManager.UpdateSelectCount(CurrentDebitor.Kontragent.DocCode);
            }
            else
            {
                var item = StandartDialogs.SelectInvoiceClient(true, true);
                if (item == null) return;
                VzaimoraschetType vzdefault = null;
                var vzdefDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(_ =>
                    _.SECTION == "MUTUAL_ACCOUNTING" && _.ITEM == "DEFAULT_TYPE_PRODUCT");
                if (vzdefDC != null)
                    vzdefault = MainReferences.VzaimoraschetTypes[Convert.ToDecimal(vzdefDC.ITEM_VALUE)];

                var newdeb = new MutualAccountingDebitorViewModel
                {
                    DocCode = Document.DocCode,
                    Code = Document.Rows.Count + 1,
                    VZT_DOC_DATE = Document.VZ_DATE,
                    VZT_DOC_NUM = (Document.Rows.Count + 1).ToString(),
                    VZT_1MYDOLZH_0NAMDOLZH = 0,
                    VZT_CRS_POGASHENO = item.SF_CRS_SUMMA_K_OPLATE - item.PaySumma,
                    VZT_UCH_CRS_POGASHENO = item.SF_CRS_SUMMA_K_OPLATE - item.PaySumma,
                    VZT_CRS_SUMMA = item.SF_CRS_SUMMA_K_OPLATE - item.PaySumma,
                    VZT_KONTR_CRS_SUMMA = -(item.SF_CRS_SUMMA_K_OPLATE - item.PaySumma),
                    VZT_UCH_CRS_RATE = 1,
                    State = RowStatus.NewRow,
                    VzaimoraschType = vzdefault,
                    Parent = Document,
                    Kontragent = MainReferences.GetKontragent(item.SF_CLIENT_DC),
                    SFClient = item
                };
                Document.Rows.Add(newdeb);
                DebitorCollection.Add(newdeb);
                CurrentDebitor = newdeb;
                KontragentManager.UpdateSelectCount(newdeb.Kontragent.DocCode);
            }

            UpdateVisualData();
        }

        public ICommand UpdateCalcSummaCommand
        {
            get { return new Command(UpdateCalcSumma, _ => true); }
        }

        private decimal CalcItogoSumma()
        {
            decimal sumLeft = 0, sumRight = 0;
            foreach (var l in Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0))
                if (l.Kontragent.IsBalans)
                    // ReSharper disable once PossibleInvalidOperationException
                    sumLeft += Math.Abs((decimal) l.VZT_KONTR_CRS_SUMMA);
            foreach (var l in Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1))
                if (l.Kontragent.IsBalans)
                    // ReSharper disable once PossibleInvalidOperationException
                    sumRight += (decimal) l.VZT_KONTR_CRS_SUMMA;
            return sumRight - sumLeft;
        }

        private void UpdateCalcSumma(object obj)
        {
            if (Document == null) return;
            var state = Document.State;
            if (IsNotOld)
            {
                Document.VZ_LEFT_UCH_CRS_SUM = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0)
                    .Sum(_ => _.VZT_CRS_SUMMA);
                Document.VZ_RIGHT_UCH_CRS_SUM = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1)
                    .Sum(_ => _.VZT_CRS_SUMMA);
                Document.VZ_PRIBIL_UCH_CRS_SUM =
                    CalcItogoSumma(); //Document.VZ_RIGHT_UCH_CRS_SUM - Document.VZ_LEFT_UCH_CRS_SUM;
            }
            else
            {
                Document.VZ_LEFT_UCH_CRS_SUM = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0)
                    .Sum(_ => _.VZT_UCH_CRS_POGASHENO);
                Document.VZ_RIGHT_UCH_CRS_SUM = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1)
                    .Sum(_ => _.VZT_UCH_CRS_POGASHENO);
                Document.VZ_PRIBIL_UCH_CRS_SUM =
                    CalcItogoSumma(); //Document.VZ_RIGHT_UCH_CRS_SUM - Document.VZ_LEFT_UCH_CRS_SUM;
            }

            Document.myState = state;
            RaisePropertyChanged(nameof(Document));
            RaisePropertyChanged(nameof(CurrencyConvertRate));
        }

        public override void RefreshData(object obj)
        {
            try
            {
                base.RefreshData(obj);
                if (Document != null && IsCanSaveData)
                {
                    var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    switch (res)
                    {
                        case MessageBoxResult.Yes:
                            SaveData(null);
                            break;
                        case MessageBoxResult.No:
                            break;
                    }
                }

                if (Document?.DocCode > 0)
                {
                    Document = Manager.Load(Document.DocCode);
                    RaisePropertyChanged(nameof(Document));
                }
                else
                {
                    if (obj == null) return;
                    var dc = (decimal) obj;
                    Document = Manager.Load(dc);
                    if (Document == null)
                    {
                        WinManager.ShowWinUIMessageBox($"Не найден документ с кодом {dc}!",
                            "Ошибка обращения к базе данных", MessageBoxButton.OK, MessageBoxImage.Error,
                            MessageBoxResult.None, MessageBoxOptions.None);
                        return;
                    }

                    Document.IsOld = Manager.CheckDocumentForOld(Document.DocCode);
                    RaisePropertyChanged(nameof(Document));
                }

                foreach (var r in Document.Rows)
                {
                    if (r.VZT_SFACT_DC != null)
                        r.SFClient = InvoicesManager.GetInvoiceClient((decimal) r.VZT_SFACT_DC);
                    if (r.VZT_SPOST_DC != null)
                        r.SFProvider = InvoicesManager.GetInvoiceProvider((decimal) r.VZT_SPOST_DC);
                    r.myState = RowStatus.NotEdited;
                }

                UpdateDebitorCreditorCollections(null);
                UpdateCalcSumma(null);
                Document.myState = RowStatus.NotEdited;
                RaisePropertiesChanged(nameof(IsCanSaveData));
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Вы уверены, что хотите удалить документ?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    Manager.Delete(Document);
                    CloseWindow(null);
                    break;
                case MessageBoxResult.No:
                    return;
            }
        }

        public override void SaveData(object data)
        {
            if (IsCurrencyConvert && Document.CreditorCurrency == null)
                Document.CreditorCurrency = Document.DebitorCurrency;
            RaisePropertyChanged(nameof(Document));
            var res = Manager.Save(Document);
            if (res != null)
            {
                var dcs = new List<decimal>();
                foreach (var d in Document.DeletedRows)
                    if (dcs.All(_ => _ != d.VZT_KONTR_DC))
                        dcs.Add(d.VZT_KONTR_DC);
                foreach (var d in Document.Rows)
                    if (dcs.All(_ => _ != d.VZT_KONTR_DC))
                        dcs.Add(d.VZT_KONTR_DC);
                foreach (var d in dcs)
                    RecalcKontragentBalans.CalcBalans(d, Document.VZ_DATE);
                Document.DeletedRows.Clear();
            }
        }

        private void UpdateDebitorCreditorCollections(TD_110ViewModel vm, bool isDebitor = true)
        {
            DebitorCollection.Clear();
            CreditorCollection.Clear();
            if (!(Document?.Rows.Count > 0)) return;
            // ReSharper disable once PossibleNullReferenceException
            foreach (var r in Document.Rows)
                if (r.VZT_1MYDOLZH_0NAMDOLZH == 0)
                {
                    var n = new MutualAccountingDebitorViewModel(r)
                    {
                        Kontragent = r.Kontragent,
                        SFClient = r.SFClient,
                        Parent = r.Parent
                    };
                    DebitorCollection.Add(n);
                }
                else
                {
                    var n = new MutualAccountingCreditorViewModel(r)
                    {
                        Kontragent = r.Kontragent,
                        SFProvider = r.SFProvider,
                        Parent = r.Parent
                    };
                    CreditorCollection.Add(n);
                }

            if (vm == null) return;
            if (isDebitor)
                CurrentDebitor = DebitorCollection.FirstOrDefault(_ => _.Code == vm.Code);
            else
                CurrentCreditor = CreditorCollection.FirstOrDefault(_ => _.Code == vm.Code);
        }

        public override bool IsDocDeleteAllow => Document?.State != RowStatus.NewRow;

        public override bool IsCanSaveData => Document != null && (Document.State != RowStatus.NotEdited
                                                                   || Document.DeletedRows.Count > 0 ||
                                                                   Document.Rows.Any(
                                                                       _ => _.State != RowStatus.NotEdited))
                                                               && Document.DebitorCurrency != null &&
                                                               Document.MutualAccountingOldType != null;

        //private bool isCanSave()
        //{
        //    return Document.State != RowStatus.NotEdited && manager.IsChecked(Document);
        //}

        public override bool IsCanRefresh => Document.State != RowStatus.NewRow;

        public ICommand AddNewCreditorCommand
        {
            get
            {
                return new Command(AddNewCreditor,
                    appDomain => Document?.CreditorCurrency != null ||
                                  !IsCurrencyConvert && Document?.DebitorCurrency != null);
            }
        }

        public ICommand AddNewDebitorCommand
        {
            get { return new Command(AddNewDebitor, _ => Document?.DebitorCurrency != null); }
        }

        public ICommand RemoveCreditorCommand
        {
            get { return new Command(RemoveCreditor, _ => CurrentCreditor != null); }
        }

        public void RemoveCreditor(object obj)
        {
            if (CurrentCreditor != null)
            {
                if (CurrentCreditor.State == RowStatus.NewRow)
                {
                    Document.Rows.Remove(CurrentCreditor);
                    CreditorCollection.Remove(CurrentCreditor);
                    return;
                }

                Document.DeletedRows.Add(CurrentCreditor);
                Document.Rows.Remove(CurrentCreditor);
                var prnt = CurrentCreditor.Parent;
                if (prnt != null)
                    prnt.State = RowStatus.Edited;
                CreditorCollection.Remove(CurrentCreditor);
                UpdateCalcSumma(null);
            }
        }

        public ICommand RemoveDebitorCommand
        {
            get { return new Command(RemoveDebitor, _ => CurrentDebitor != null); }
        }

        private void RemoveDebitor(object obj)
        {
            //manager.DeleteRow(Document.Rows, Document.DeletedRows, CurrentDebitor);
            if (CurrentDebitor != null)
            {
                if (CurrentDebitor.State == RowStatus.NewRow)
                {
                    Document.Rows.Remove(CurrentDebitor);
                    DebitorCollection.Remove(CurrentDebitor);
                    return;
                }

                Document.DeletedRows.Add(CurrentDebitor);
                Document.Rows.Remove(CurrentDebitor);
                var prnt = CurrentDebitor.Parent;
                if (prnt != null)
                    prnt.State = RowStatus.Edited;
                DebitorCollection.Remove(CurrentDebitor);
                UpdateCalcSumma(null);
            }

            //DebitorCollection.Remove(CurrentDebitor);
        }

        public void AddNewDebitor(object obj)
        {
            try
            {
                VzaimoraschetType vzdefault = null;
                var vzdefDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(_ =>
                    _.SECTION == "MUTUAL_ACCOUNTING" && _.ITEM == "DEFAULT_TYPE_PRODUCT");
                if (vzdefDC != null)
                    vzdefault = MainReferences.VzaimoraschetTypes[Convert.ToDecimal(vzdefDC.ITEM_VALUE)];
                var k = StandartDialogs.SelectKontragent(Document.IsOld ? null : Document.DebitorCurrency);
                if (k == null) return;
                var newdeb = new MutualAccountingDebitorViewModel
                {
                    DocCode = Document.DocCode,
                    Code = Document.Rows.Count == 0 ? 1 : Document.Rows.Max(_ => _.Code) + 1,
                    VZT_DOC_DATE = Document.VZ_DATE,
                    VZT_DOC_NUM = (Document.Rows.Count + 1).ToString(),
                    VZT_1MYDOLZH_0NAMDOLZH = 0,
                    VZT_CRS_POGASHENO = 0,
                    VZT_UCH_CRS_POGASHENO = 0,
                    VZT_CRS_SUMMA = 0,
                    VZT_KONTR_CRS_RATE = 1,
                    VZT_KONTR_CRS_SUMMA = 0,
                    VZT_UCH_CRS_RATE = 1,
                    VZT_KONTR_DC = k.DocCode,
                    State = RowStatus.NewRow,
                    VzaimoraschType = vzdefault,
                    Parent = Document,
                    Kontragent = MainReferences.GetKontragent(k.DocCode)
                };
                Document.Rows.Add(newdeb);
                DebitorCollection.Add(newdeb);
                CurrentDebitor = newdeb;
                KontragentManager.UpdateSelectCount(k.DocCode);
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public void AddNewCreditor(object obj)
        {
            var k = StandartDialogs.SelectKontragent(Document.IsOld ? null : Document.CreditorCurrency);
            if (k == null) return;
            VzaimoraschetType vzdefault = null;
            var vzdefDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(_ =>
                _.SECTION == "MUTUAL_ACCOUNTING" && _.ITEM == "DEFAULT_TYPE_PRODUCT");
            if (vzdefDC != null)
                vzdefault = MainReferences.VzaimoraschetTypes[Convert.ToDecimal(vzdefDC.ITEM_VALUE)];
            var newcred = new MutualAccountingCreditorViewModel
            {
                DocCode = Document.DocCode,
                Code = Document.Rows.Count == 0 ? 1 : Document.Rows.Max(_ => _.Code) + 1,
                VZT_DOC_DATE = Document.VZ_DATE,
                VZT_DOC_NUM = (Document.Rows.Count + 1).ToString(),
                VZT_1MYDOLZH_0NAMDOLZH = 1,
                VZT_CRS_POGASHENO = 0,
                VZT_UCH_CRS_POGASHENO = 0,
                VZT_CRS_SUMMA = 0,
                VZT_KONTR_CRS_RATE = 1,
                VZT_KONTR_CRS_SUMMA = 0,
                VZT_UCH_CRS_RATE = 1,
                VZT_KONTR_DC = k.DocCode,
                VzaimoraschType = vzdefault,
                State = RowStatus.NewRow,
                Parent = Document,
                Kontragent = MainReferences.GetKontragent(k.DocCode)
            };
            Document.Rows.Add(newcred);
            CreditorCollection.Add(newcred);
            CurrentCreditor = newcred;
            KontragentManager.UpdateSelectCount(k.DocCode);
        }

        public override string SaveInfo => Manager.CheckedInfo;

        public override void DocNewEmpty(object form)
        {
            var frm = new MutualAccountingView {Owner = Application.Current.MainWindow};
            var ctx = new MutualAcountingWindowViewModel
            {
                IsCurrencyConvert = IsCurrencyConvert,
                Form = frm
            };
            ctx.CreateMenu();
            ctx.Document = ctx.Manager.New();
            if (IsCurrencyConvert)
                frm.typeVzaimozachetComboBoxEdit.SelectedIndex = 0;
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopy(object form)
        {
            var frm = new MutualAccountingView {Owner = Application.Current.MainWindow};
            var ctx = new MutualAcountingWindowViewModel
            {
                IsCurrencyConvert = IsCurrencyConvert,
                Form = frm
            };
            ctx.CreateMenu();
            ctx.Document = ctx.Manager.NewFullCopy(Document);
            ctx.UpdateVisualData();
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object form)
        {
            var frm = new MutualAccountingView {Owner = Application.Current.MainWindow};
            var ctx = new MutualAcountingWindowViewModel
            {
                IsCurrencyConvert = IsCurrencyConvert,
                Form = frm
            };
            ctx.CreateMenu();
            ctx.Document = ctx.Manager.NewRequisity(Document);
            if (IsCurrencyConvert)
                frm.typeVzaimozachetComboBoxEdit.SelectedIndex = 0;
            frm.Show();
            frm.DataContext = ctx;
        }

        public ICommand ChangeDebitorKontragentCommand
        {
            get { return new Command(ChangeDebitorKontragent, _ => CurrentDebitor != null); }
        }

        private void ChangeDebitorKontragent(object obj)
        {
            var k = obj as Kontragent ??
                    StandartDialogs.SelectKontragent(Document.IsOld ? null : Document.DebitorCurrency);
            if (k == null) return;
            CurrentDebitor.Kontragent = MainReferences.GetKontragent(k.DocCode);
            CurrentDebitor.VZT_KONTR_CRS_RATE = 1;
            CurrentDebitor.VZT_UCH_CRS_RATE = CurrencyRate.GetCBRate(CurrentDebitor.Currency,
                GlobalOptions.SystemProfile.MainCurrency, Document.VZ_DATE);
            var r = Document.Rows.FirstOrDefault(_ => _.Code == CurrentDebitor.Code);
            if (r != null)
            {
                r.VZT_CRS_DC = k.BalansCurrency.DocCode;
                r.VZT_KONTR_CRS_DC = k.BalansCurrency.DocCode;
                r.VZT_KONTR_DC = k.DocCode;
            }

            if (!(Form is MutualAccountingView f)) return;
            if (f.gridViewDebitor.ActiveEditor != null)
            {
                f.gridViewDebitor.ActiveEditor.EditValuePostMode = PostMode.Immediate;
                f.gridViewDebitor.ActiveEditor.EditValue = k;
            }
        }

        public ICommand ChangeCreditorKontragentCommand
        {
            get { return new Command(ChangeCreditorKontragent, _ => CurrentCreditor != null); }
        }

        public ICommand RemoveDebitorSFCommand
        {
            get { return new Command(RemoveDebitorSF, _ => CurrentDebitor != null && CurrentDebitor.SFClient != null); }
        }

        private void RemoveDebitorSF(object obj)
        {
            CurrentDebitor.SFClient = null;
            if (!(Form is MutualAccountingView f)) return;
            if (f.gridViewDebitor.ActiveEditor != null)
            {
                f.gridViewDebitor.ActiveEditor.EditValuePostMode = PostMode.Immediate;
                f.gridViewDebitor.ActiveEditor.EditValue = null;
            }
        }

        private void ChangeCreditorKontragent(object obj)
        {
            var k = obj as Kontragent ??
                    StandartDialogs.SelectKontragent(Document.IsOld ? null : Document.CreditorCurrency);
            if (k == null) return;
            CurrentCreditor.Kontragent = k;
            CurrentCreditor.VZT_KONTR_CRS_RATE = 1;
            CurrentCreditor.VZT_UCH_CRS_RATE = CurrencyRate.GetCBRate(CurrentCreditor.Currency,
                GlobalOptions.SystemProfile.MainCurrency, Document.VZ_DATE);
            var r = Document.Rows.FirstOrDefault(_ => _.Code == CurrentCreditor.Code);
            if (r != null)
            {
                r.VZT_KONTR_DC = k.DocCode;
                r.VZT_CRS_DC = k.BalansCurrency.DocCode;
                r.VZT_KONTR_CRS_DC = k.BalansCurrency.DocCode;
            }

            if (!(Form is MutualAccountingView f)) return;
            if (f.gridViewCreditor.ActiveEditor != null)
            {
                f.gridViewCreditor.ActiveEditor.EditValuePostMode = PostMode.Immediate;
                f.gridViewCreditor.ActiveEditor.EditValue = k;
            }
        }

        #endregion
    }
}