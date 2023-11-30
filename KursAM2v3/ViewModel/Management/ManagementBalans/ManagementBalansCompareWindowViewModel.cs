using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Grid;
using KursAM2.View.Base;
using KursAM2.View.Management;
using KursAM2.View.Management.Controls;
using KursAM2.ViewModel.Finance;
using KursAM2.ViewModel.Logistiks;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.CommonReferences.Kontragent;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

// ReSharper disable All
namespace KursAM2.ViewModel.Management.ManagementBalans
{
    public class ManagementBalansCompareWindowViewModel : RSWindowViewModelBase
    {
        private ManagementBalansCompareGroupViewModel myCurrentBalansRow;
        private ManagementBalansCompareGroupViewModel myCurrentDataItem;
        private EmployeeSalary myCurrentEmployeeSalary;
        private KontragentCompareBalansDeltaItem myCurrentKontragent;
        private NomenklCompareBalansDeltaItem myCurrentStoreNomenklOperation;

        //public FinanseOperationItem 
        private FinanseOperation myFinanseCurrentItem;
        private ManagementBalansWindowViewModel myFirstBalans;
        private DateTime myFirstDate;
        private NomenklCompareBalansDeltaItem myNomenklCurrent;
        private ManagementBalansWindowViewModel mySecondBalans;
        private DateTime mySecondDate;

        public ManagementBalansCompareWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = GetRightMenu();
            FirstDate = DateTime.Today;
            SecondDate = DateTime.Today;
        }

        public ManagementBalansCompareWindowViewModel(Window win) : this()
        {
            Form = win;
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<ManagementBalansCompareGroupViewModel> Data { set; get; } =
            new ObservableCollection<ManagementBalansCompareGroupViewModel>();

        public ObservableCollection<ManagementBalansCompareRowViewModel> ExtendRowsActual { set; get; } =
            new ObservableCollection<ManagementBalansCompareRowViewModel>();

        public ObservableCollection<ManagementBalansCompareRowViewModel> ExtendRowsTemp { set; get; } =
            new ObservableCollection<ManagementBalansCompareRowViewModel>();

        public ObservableCollection<KontragentCompareBalansDeltaItem> KontragentDeltaList { set; get; } =
            new ObservableCollection<KontragentCompareBalansDeltaItem>();

        public ObservableCollection<KontragentCompareBalansDeltaItem> KontragentDeltaListTemp { set; get; } =
            new ObservableCollection<KontragentCompareBalansDeltaItem>();

        public ObservableCollection<NomenklCompareBalansDeltaItem> NomenklDeltaList { set; get; } =
            new ObservableCollection<NomenklCompareBalansDeltaItem>();

        public ObservableCollection<NomenklCompareBalansDeltaItem> NomenklDeltaListTemp { set; get; } =
            new ObservableCollection<NomenklCompareBalansDeltaItem>();

        //public ObservableCollection<NomenklCompareBalansOperation> NomenklOperations { set; get; } =
        //    new ObservableCollection<NomenklCompareBalansOperation>();
        public NomenklCompareBalansDeltaItem NomenklCurrent
        {
            get => myNomenklCurrent;
            set
            {
                if (myNomenklCurrent == value) return;
                myNomenklCurrent = value;
                LoadOperation();
                RaisePropertyChanged();
            }
        }

        public KontragentCompareBalansDeltaItem CurrentKontragent
        {
            get => myCurrentKontragent;
            set
            {
                if (myCurrentKontragent == value) return;
                myCurrentKontragent = value;
                if (myCurrentKontragent != null)
                    LoadKontragentOperations();
                RaisePropertyChanged();
            }
        }

        public DateTime FirstDate
        {
            get => myFirstDate;
            set
            {
                if (myFirstDate == value) return;
                myFirstDate = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FirstDateName));
            }
        }

        public ManagementBalansCompareGroupViewModel CurrentDataItem
        {
            get => myCurrentDataItem;
            set
            {
                if (myCurrentDataItem == value) return;
                myCurrentDataItem = value;
                RaisePropertyChanged();
            }
        }

        public string FirstDateName => FirstDate.ToShortDateString();
        public string SecondDateName => SecondDate.ToShortDateString();

        public DateTime SecondDate
        {
            get => mySecondDate;
            set
            {
                if (mySecondDate == value) return;
                mySecondDate = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SecondDateName));
            }
        }

        public FinanseOperation FinanseCurrentItem
        {
            get => myFinanseCurrentItem;
            set
            {
                if (myFinanseCurrentItem == value) return;
                myFinanseCurrentItem = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<FinanseOperation> FinanseOperations { set; get; } =
            new ObservableCollection<FinanseOperation>();

        public ObservableCollection<MoneyInPathItem> MoneyInPathOperations { set; get; } =
            new ObservableCollection<MoneyInPathItem>();

        public ManagementBalansCompareGroupViewModel CurrentBalansRow
        {
            get => myCurrentBalansRow;
            set
            {
                if (myCurrentBalansRow == value) return;
                myCurrentBalansRow = value;
                ExtendRowsActual.Clear();
                ExtendRowsTemp.Clear();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ExtendRowsActual));
                RaisePropertyChanged(nameof(ExtendRowsTemp));
                if (myCurrentBalansRow?.ParentId == null) return;
                var frm = Form as ManagementBalansCompareView;
                var parent = myFirstBalans.BalansStructure.FirstOrDefault(_ => _.Id == myCurrentBalansRow.ParentId);
                // ReSharper disable once PossibleNullReferenceException
                if (parent.Tag == BalansSection.Bank || parent.Tag == BalansSection.Cash)
                {
                    frm?.NavigateTo(typeof(BalansCompareFinanseUI));
                    if (parent.Tag == BalansSection.Cash)
                        LoadCashOperations();
                    if (parent.Tag == BalansSection.Bank)
                        LoadBankOperatrions();
                    //if (frm.CurrentDetailView is IResetCurrencyColumns view)
                    //{
                    //    view.ResetCurrencyColumns();
                    //}
                    return;
                }

                if (parent.Tag == BalansSection.Store)
                {
                    LoadNomenklList();
                    frm?.NavigateTo(typeof(BalansCompareStoreUI));
                    if (frm.CurrentDetailView is IResetCurrencyColumns view)
                    {
                        view.ResetCurrencyColumns();
                    }

                    return;
                }

                if (parent.Tag == BalansSection.Head && myCurrentBalansRow.Tag == BalansSection.Store)
                {
                    LoadAllNomenklList();
                    frm?.NavigateTo(typeof(BalansCompareStoreUI));
                    if (frm.CurrentDetailView is IResetCurrencyColumns view)
                    {
                        view.ResetCurrencyColumns();
                    }

                    return;
                }

                if (myCurrentBalansRow.Tag == BalansSection.Creditors ||
                    myCurrentBalansRow.Tag == BalansSection.Debitors)
                {
                    if (myCurrentBalansRow.Tag == BalansSection.Creditors)
                        LoadCreditorsList();
                    if (myCurrentBalansRow.Tag == BalansSection.Debitors)
                        LoadDebitorsList();
                    frm?.NavigateTo(typeof(BalansCompareKontragentUI));
                    if (frm.CurrentDetailView is IResetCurrencyColumns view)
                    {
                        view.ResetCurrencyColumns();
                    }

                    return;
                }

                if (myCurrentBalansRow.Tag == BalansSection.Salary)
                {
                    LoadSalary();
                    frm?.NavigateTo(typeof(BalansCompareSalaryUI));
                    return;
                }

                if (myCurrentBalansRow.Tag == BalansSection.MoneyInPath)
                {
                    LoadMoneyInPathOperations();
                    frm?.NavigateTo(typeof(BalansCompareMoneyInPathUI));
                    return;
                }

                frm?.NavigateTo(typeof(EmptyUI));
            }
        }

        public ObservableCollection<EmployeeSalary> EmployeeSalaryList { set; get; } =
            new ObservableCollection<EmployeeSalary>();

        public EmployeeSalary CurrentEmployeeSalary
        {
            get { return myCurrentEmployeeSalary; }
            set
            {
                if (myCurrentEmployeeSalary == value) return;
                myCurrentEmployeeSalary = value;
                RaisePropertyChanged();
            }
        }

        public NomenklCompareBalansDeltaItem CurrentStoreNomenklOperation
        {
            get { return myCurrentStoreNomenklOperation; }
            set
            {
                if (myCurrentStoreNomenklOperation == value) return;
                myCurrentStoreNomenklOperation = value;
                RaisePropertyChanged();
            }
        }

        public ICommand KontragentBalansOpenCommand
        {
            get { return new Command(KontragentBalansOpen, _ => CurrentKontragent != null); }
        }

        private void LoadOperation()
        {
            if (NomenklCurrent == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var calc = new NomenklCostMediumSliding(ctx);
                var Operations = new List<NomenklCalcCostOperation>(calc.GetAllOperations(NomenklCurrent.NomenklDC));
                NomenklCurrent.NomenklOperations.Clear();
                foreach (var op in Operations.Where(_ => _.DocDate >= FirstDate && _.DocDate <= SecondDate))
                {
                    NomenklCurrent.NomenklOperations.Add(new NomenklCompareBalansOperation
                    {
                        CalcPrice = op.CalcPrice,
                        CalcPriceNaklad = op.CalcPriceNaklad,
                        DocDate = op.DocDate,
                        DocPrice = op.DocPrice,
                        FinDocument = op.FinDocument,
                        FinDocumentDC = op.FinDocumentDC,
                        KontragentIn = op.KontragentIn,
                        KontragentOut = op.KontragentOut,
                        Naklad = op.Naklad,
                        NomenklDC = op.NomenklDC,
                        Note = op.Note,
                        OperCode = op.OperCode,
                        OperationName = op.OperationName,
                        QuantityIn = op.QuantityIn,
                        QuantityNakopit = op.QuantityNakopit,
                        QuantityOut = op.QuantityOut,
                        SkladIn = op.SkladIn,
                        SkladOut = op.SkladOut,
                        SummaIn = op.SummaIn,
                        SummaOut = op.SummaOut,
                        SummaInWithNaklad = op.SummaInWithNaklad,
                        SummaOutWithNaklad = op.SummaOutWithNaklad,
                        TovarDocDC = op.TovarDocDC,
                        TovarDocument = op.TovarDocument
                    });
                }
            }
        }

        private void LoadSalary()
        {
            EmployeeSalaryList = new ObservableCollection<EmployeeSalary>();
            var salaryStart = myFirstBalans.ExtendRows.Where(_ => _.Persona != null).ToList();
            var salaryEnd = mySecondBalans.ExtendRows.Where(_ => _.Persona != null).ToList();
            List<Employee> emps = new List<Employee>();
            emps.AddRange(salaryStart.Select(_ => _.Persona));
            foreach (var p in salaryEnd.Select(_ => _.Persona))
            {
                var e = emps.FirstOrDefault(_ => _.DocCode == p.DocCode);
                if (e != null) continue;
                emps.Add(p);
            }

            foreach (var emp in emps)
            {
                var e = new EmployeeSalary
                {
                    Employee = emp,
                    SummaEUR = salaryStart.FirstOrDefault(_ => _.Persona.DocCode == emp.DocCode)?.SummaEUR ?? 0,
                    SummaEUR2 = salaryEnd.FirstOrDefault(_ => _.Persona.DocCode == emp.DocCode)?.SummaEUR ?? 0,
                    SummaRUB = salaryStart.FirstOrDefault(_ => _.Persona.DocCode == emp.DocCode)?.SummaRUB ?? 0,
                    SummaRUB2 = salaryEnd.FirstOrDefault(_ => _.Persona.DocCode == emp.DocCode)?.SummaRUB ?? 0,
                    SummaUSD = salaryStart.FirstOrDefault(_ => _.Persona.DocCode == emp.DocCode)?.SummaUSD ?? 0,
                    SummaUSD2 = salaryEnd.FirstOrDefault(_ => _.Persona.DocCode == emp.DocCode)?.SummaUSD ?? 0
                };
                if (e.SummaEUR != 0 || e.SummaEUR2 != 0
                                    || e.SummaRUB != 0 || e.SummaRUB2 != 0
                                    || e.SummaUSD != 0 || e.SummaUSD2 != 0)
                    EmployeeSalaryList.Add(e);
            }
        }

        private void LoadKontragentOperations()
        {
            var kontrBalans = new KontragentBalansWindowViewModel();
            CurrentKontragent.KontragentOperations.Clear();
            foreach (var op in kontrBalans.Load(CurrentKontragent.KontragentDC)
                         .Where(_ => _.DocDate > FirstDate && _.DocDate <= SecondDate))
            {
                var newDoc = new KontragentBalansOperation
                {
                    DocName = op.DocName,
                    DocDate = op.DocDate,
                    DocNum = op.DocNum
                };
                newDoc.SetSumma(CurrentKontragent.Currency, op.CrsKontrIn, op.CrsKontrOut);
                if (newDoc.IsDifferent)
                    CurrentKontragent.KontragentOperations.Add(newDoc);
            }
        }

        private void LoadNomenklList()
        {
            NomenklDeltaList.Clear();
            NomenklDeltaListTemp.Clear();
            foreach (var d in myFirstBalans.ExtendRows.Where(_ => _.GroupId == CurrentBalansRow.Id))
            {
                var newNom = new NomenklCompareBalansDeltaItem
                {
                    Currency = d.Nom.Currency as Currency,
                    NomenklDC = d.Nom.DocCode,
                    NomenklName = d.Nom.Name,
                    NomenklNumber = d.Nom.NomenklNumber,
                    Note = d.Note,
                    QuantityStart = d.Quantity,
                    PriceStart = d.Price,
                    SummaStart = d.Summa
                };
                newNom.SetSumma(newNom.Currency, d.Summa, 0);
                NomenklDeltaListTemp.Add(newNom);
            }

            var secId = mySecondBalans.BalansStructure.FirstOrDefault(_ => _.ParentId == CurrentBalansRow.ParentId
                                                                           && _.ObjectDC == CurrentBalansRow.ObjectDC);
            foreach (var d in mySecondBalans.ExtendRows.Where(_ => _.GroupId == secId?.Id))
            {
                var old = NomenklDeltaListTemp.FirstOrDefault(_ => _.NomenklDC == d.Nom.DocCode);
                if (old == null)
                {
                    var newNom = new NomenklCompareBalansDeltaItem
                    {
                        Currency = d.Nom.Currency as Currency,
                        NomenklDC = d.Nom.DocCode,
                        NomenklName = d.Nom.Name,
                        NomenklNumber = d.Nom.NomenklNumber,
                        Note = d.Note,
                        QuantityEnd = d.Quantity,
                        PriceEnd = d.Price,
                        SummaEnd = d.Summa
                    };
                    newNom.SetSumma(newNom.Currency, 0, d.Summa);
                    NomenklDeltaListTemp.Add(newNom);
                }
                else
                {
                    old.QuantityEnd = d.Quantity;
                    old.PriceEnd = d.Price;
                    old.SummaEnd = d.Summa;
                    old.SetSumma2(old.Currency, d.Summa);
                }
            }

            foreach (var n in NomenklDeltaListTemp)
            {
                if (n.IsChanged())
                    NomenklDeltaList.Add(n);
            }
        }

        private void LoadAllNomenklList()
        {
            NomenklDeltaList.Clear();
            NomenklDeltaListTemp.Clear();
            foreach (var dgrp in myFirstBalans.BalansStructure.Where(_ => _.ParentId == CurrentBalansRow.Id))
            {
                foreach (var d in myFirstBalans.ExtendRows.Where(_ => _.GroupId == dgrp.Id))
                {
                    var old = NomenklDeltaListTemp.FirstOrDefault(_ => _.NomenklDC == d.Nom.DocCode);
                    if (old == null)
                    {
                        var newNom = new NomenklCompareBalansDeltaItem
                        {
                            Currency = d.Nom.Currency as Currency,
                            NomenklDC = d.Nom.DocCode,
                            NomenklName = d.Nom.Name,
                            NomenklNumber = d.Nom.NomenklNumber,
                            Note = d.Note,
                            QuantityStart = d.Quantity,
                            PriceStart = d.Price,
                            SummaStart = d.Summa
                        };
                        newNom.SetSumma(newNom.Currency, d.Summa, 0);
                        NomenklDeltaListTemp.Add(newNom);
                    }
                    else
                    {
                        old.SetAddSumma(d.Summa);
                    }
                }
            }

            foreach (var dgrp in mySecondBalans.BalansStructure.Where(_ => _.ParentId == CurrentBalansRow.Id))
            {
                foreach (var d in mySecondBalans.ExtendRows.Where(_ => _.GroupId == dgrp.Id))
                {
                    var old = NomenklDeltaListTemp.FirstOrDefault(_ => _.NomenklDC == d.Nom.DocCode);
                    if (old == null)
                    {
                        var newNom = new NomenklCompareBalansDeltaItem
                        {
                            Currency = d.Nom.Currency as Currency,
                            NomenklDC = d.Nom.DocCode,
                            NomenklName = d.Nom.Name,
                            NomenklNumber = d.Nom.NomenklNumber,
                            Note = d.Note,
                            QuantityEnd = d.Quantity,
                            PriceEnd = d.Price,
                            SummaEnd = d.Summa
                        };
                        newNom.SetSumma(newNom.Currency, 0, d.Summa);
                        NomenklDeltaListTemp.Add(newNom);
                    }
                    else
                    {
                        old.SetAddSumma(0, d.Summa);
                    }
                }
            }

            foreach (var n in NomenklDeltaListTemp)
                if (n.IsChanged())
                    NomenklDeltaList.Add(n);
        }

        private void LoadDebitorsList()
        {
            KontragentDeltaList.Clear();
            KontragentDeltaListTemp.Clear();
            foreach (var d in myFirstBalans.ExtendRows.Where(_ => _.GroupId == CurrentBalansRow.Id))
            {
                var newKontr = new KontragentCompareBalansDeltaItem
                {
                    Currency = d.Kontragent.Currency as Currency,
                    KontragentName = d.Kontragent.Name,
                    Note = d.Kontragent.Notes,
                    KontragentDC = d.Kontragent.DocCode
                };
                newKontr.SetSumma(newKontr.Currency, d.Summa, 0);
                KontragentDeltaListTemp.Add(newKontr);
            }

            foreach (var d in mySecondBalans.ExtendRows.Where(_ => _.GroupId == CurrentBalansRow.Id))
            {
                var old = KontragentDeltaListTemp.FirstOrDefault(_ => _.KontragentDC == d.Kontragent.DocCode);
                if (old == null)
                {
                    var newKontr = new KontragentCompareBalansDeltaItem
                    {
                        Currency = (Currency)d.Kontragent.Currency,
                        KontragentName = d.Kontragent.Name,
                        Note = d.Kontragent.Notes,
                        KontragentDC = d.Kontragent.DocCode
                    };
                    newKontr.SetSumma(newKontr.Currency, 0, d.Summa);
                    KontragentDeltaListTemp.Add(newKontr);
                }
                else
                {
                    old.SetSumma2(old.Currency, d.Summa);
                }
            }

            foreach (var k in KontragentDeltaListTemp)
                if (k.IsDifferent)
                    KontragentDeltaList.Add(k);
        }

        private void LoadCreditorsList()
        {
            KontragentDeltaList.Clear();
            KontragentDeltaListTemp.Clear();
            foreach (var d in myFirstBalans.ExtendRows.Where(_ => _.GroupId == CurrentBalansRow.Id))
            {
                var newKontr = new KontragentCompareBalansDeltaItem
                {
                    Currency = (Currency)d.Kontragent.Currency,
                    KontragentName = d.Kontragent.Name,
                    Note = d.Kontragent.Notes,
                    KontragentDC = d.Kontragent.DocCode
                };
                newKontr.SetSumma(newKontr.Currency, d.Summa, 0);
                KontragentDeltaListTemp.Add(newKontr);
            }

            foreach (var d in mySecondBalans.ExtendRows.Where(_ => _.GroupId == CurrentBalansRow.Id))
            {
                var old = KontragentDeltaListTemp.FirstOrDefault(_ => _.KontragentDC == d.Kontragent.DocCode);
                if (old == null)
                {
                    var newKontr = new KontragentCompareBalansDeltaItem
                    {
                        Currency = (Currency)d.Kontragent.Currency,
                        KontragentName = d.Kontragent.Name,
                        Note = d.Kontragent.Notes,
                        KontragentDC = d.Kontragent.DocCode
                    };
                    newKontr.SetSumma(newKontr.Currency, 0, d.Summa);
                    KontragentDeltaListTemp.Add(newKontr);
                }
                else
                {
                    old.SetSumma2(old.Currency, d.Summa);
                }
            }

            foreach (var k in KontragentDeltaListTemp)
                if (k.IsDifferent)
                    KontragentDeltaList.Add(k);
        }

        private void LoadBankOperatrions()
        {
            FinanseOperations.Clear();
            if (!CurrentBalansRow.IsDifferent) return;
            var bankDC = CurrentBalansRow.ObjectDC;
            if (bankDC == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var oper = ctx.TD_101.Include(_ => _.SD_101)
                    .Where(_ => _.SD_101.VV_ACC_DC == bankDC
                                && _.SD_101.VV_START_DATE > FirstDate && _.SD_101.VV_START_DATE <= SecondDate)
                    .ToList();
                foreach (var op in oper)
                {
                    var newDoc = new FinanseOperation
                    {
                        DocDC = op.DOC_CODE,
                        Date = op.SD_101.VV_START_DATE,
                        DocNum = op.VVT_DOC_NUM,
                        DocumentType = DocumentType.Bank,
                        KontragentType = op.VVT_KONTRAGENT != null
                            ? KontragentTypeEnum.Kontragent
                            : op.VVT_RASH_KASS_ORDER_DC != null
                                ? KontragentTypeEnum.Cash
                                : op.VVT_KASS_PRIH_ORDER_DC != null
                                    ? KontragentTypeEnum.Cash
                                    : KontragentTypeEnum.Unknown,
                        Note = null
                    };
                    switch (newDoc.KontragentType)
                    {
                        case KontragentTypeEnum.Kontragent:
                            newDoc.KontragentName =
                                ((IName)GlobalOptions.ReferencesCache.GetKontragent(op.VVT_KONTRAGENT)).Name;
                            newDoc.KontragentDC = (decimal)op.VVT_KONTRAGENT;
                            break;
                        case KontragentTypeEnum.Cash:
                            SD_22 ch;
                            if (op.VVT_RASH_KASS_ORDER_DC != null)
                            {
                                ch = ctx.SD_34.Include(_ => _.SD_22)
                                    .FirstOrDefault(_ => _.DOC_CODE == op.VVT_RASH_KASS_ORDER_DC)
                                    ?.SD_22;
                                if (ch != null)
                                {
                                    newDoc.KontragentName = ch.CA_NAME;
                                    newDoc.KontragentDC = ch.DOC_CODE;
                                }
                            }

                            if (op.VVT_KASS_PRIH_ORDER_DC != null)
                            {
                                ch = ctx.SD_33.Include(_ => _.SD_22)
                                    .FirstOrDefault(_ => _.DOC_CODE == op.VVT_KASS_PRIH_ORDER_DC)
                                    ?.SD_22;
                                if (ch != null)
                                {
                                    newDoc.KontragentName = ch.CA_NAME;
                                    newDoc.KontragentDC = ch.DOC_CODE;
                                }
                            }

                            break;
                    }

                    if (op.VVT_VAL_PRIHOD > 0)
                        newDoc.SetSumma(GlobalOptions.ReferencesCache.GetCurrency(op.VVT_CRS_DC) as Currency,
                            (decimal)op.VVT_VAL_PRIHOD, 0);
                    else
                        newDoc.SetSumma(GlobalOptions.ReferencesCache.GetCurrency(op.VVT_CRS_DC) as Currency, 0,
                            op.VVT_VAL_RASHOD ?? 0);
                    FinanseOperations.Add(newDoc);
                }
            }
        }

        private void LoadCashOperations()
        {
            FinanseOperations.Clear();
            var cashDC = CurrentBalansRow.ObjectDC;
            if (cashDC == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var operIn = ctx.SD_33
                    .Where(_ => _.DATE_ORD > FirstDate && _.DATE_ORD <= SecondDate && _.CA_DC == cashDC).ToList();
                var operOut = ctx.SD_34
                    .Where(_ => _.DATE_ORD > FirstDate && _.DATE_ORD <= SecondDate && _.CA_DC == cashDC).ToList();
                var crsChange = ctx.SD_251
                    .Where(_ => _.CH_DATE > FirstDate && _.CH_DATE <= SecondDate && _.CH_CASH_DC == cashDC).ToList();
                foreach (var op in operIn)
                {
                    var newDoc = new FinanseOperation
                    {
                        DocDC = op.DOC_CODE,
                        Date = (DateTime)op.DATE_ORD,
                        DocNum = op.NUM_ORD.ToString(),
                        DocumentType = DocumentType.CashIn,
                        KontragentType = op.KONTRAGENT_DC != null
                            ? KontragentTypeEnum.Kontragent
                            : op.TABELNUMBER != null
                                ? KontragentTypeEnum.Employee
                                : op.BANK_RASCH_SCHET_DC != null
                                    ? KontragentTypeEnum.Bank
                                    : op.RASH_ORDER_FROM_DC != null
                                        ? KontragentTypeEnum.Cash
                                        : KontragentTypeEnum.Unknown,
                        Note = op.NOTES_ORD
                    };
                    switch (newDoc.KontragentType)
                    {
                        case KontragentTypeEnum.Kontragent:
                            newDoc.KontragentName =
                                ((IName)GlobalOptions.ReferencesCache.GetKontragent(op.KONTRAGENT_DC)).Name;
                            newDoc.KontragentDC = (decimal)op.KONTRAGENT_DC;
                            break;
                        case KontragentTypeEnum.Employee:
                            var emp =
                                GlobalOptions.ReferencesCache.GetEmployee(op.TABELNUMBER) as Employee;
                            if (emp != null)
                            {
                                newDoc.KontragentName = emp.Name;
                                newDoc.KontragentDC = emp.DocCode;
                            }

                            break;
                        case KontragentTypeEnum.Bank:
                            var bank = ctx.SD_114.FirstOrDefault(_ => _.DOC_CODE == op.BANK_RASCH_SCHET_DC);
                            if (bank != null)
                            {
                                newDoc.KontragentName = bank.BA_RASH_ACC + " " + bank.BA_BANK_NAME;
                                newDoc.KontragentDC = bank.DOC_CODE;
                            }

                            break;
                        case KontragentTypeEnum.Cash:
                            var ch = ctx.SD_34.Include(_ => _.SD_22)
                                .FirstOrDefault(_ => _.DOC_CODE == op.RASH_ORDER_FROM_DC);
                            if (ch != null)
                            {
                                newDoc.KontragentName = ch.SD_22.CA_NAME;
                                newDoc.KontragentDC = (decimal)ch.CA_DC;
                            }

                            break;
                    }

                    newDoc.SetSumma(GlobalOptions.ReferencesCache.GetCurrency(op.CRS_DC) as Currency,
                        (decimal)op.SUMM_ORD, 0);
                    FinanseOperations.Add(newDoc);
                }

                foreach (var op in operOut)
                {
                    var newDoc = new FinanseOperation
                    {
                        DocDC = op.DOC_CODE,
                        Date = (DateTime)op.DATE_ORD,
                        DocNum = op.NUM_ORD.ToString(),
                        DocumentType = DocumentType.CashOut,
                        KontragentType = op.KONTRAGENT_DC != null
                            ? KontragentTypeEnum.Kontragent
                            : op.TABELNUMBER != null
                                ? KontragentTypeEnum.Employee
                                : op.BANK_RASCH_SCHET_DC != null
                                    ? KontragentTypeEnum.Bank
                                    : KontragentTypeEnum.Unknown,
                        Note = op.NOTES_ORD
                    };
                    switch (newDoc.KontragentType)
                    {
                        case KontragentTypeEnum.Kontragent:
                            newDoc.KontragentName =
                                ((IName)GlobalOptions.ReferencesCache.GetKontragent(op.KONTRAGENT_DC)).Name;
                            newDoc.KontragentDC = (decimal)op.KONTRAGENT_DC;
                            break;
                        case KontragentTypeEnum.Employee:
                            var emp =
                                GlobalOptions.ReferencesCache.GetEmployee(op.TABELNUMBER) as Employee;
                            if (emp != null)
                            {
                                newDoc.KontragentName = emp.Name;
                                newDoc.KontragentDC = emp.DocCode;
                            }

                            break;
                        case KontragentTypeEnum.Bank:
                            var bank = ctx.SD_114.FirstOrDefault(_ => _.DOC_CODE == op.BANK_RASCH_SCHET_DC);
                            if (bank != null)
                            {
                                newDoc.KontragentName = bank.BA_RASH_ACC + " " + bank.BA_BANK_NAME;
                                newDoc.KontragentDC = bank.DOC_CODE;
                            }

                            break;
                        case KontragentTypeEnum.Unknown:
                            var ch = ctx.SD_33.Include(_ => _.SD_22)
                                .FirstOrDefault(_ => _.RASH_ORDER_FROM_DC == op.DOC_CODE);
                            if (ch != null)
                            {
                                newDoc.KontragentType = KontragentTypeEnum.Cash;
                                newDoc.KontragentName = ch.SD_22.CA_NAME;
                                newDoc.KontragentDC = (decimal)ch.CA_DC;
                            }

                            break;
                    }

                    newDoc.SetSumma(GlobalOptions.ReferencesCache.GetCurrency(op.CRS_DC) as Currency, 0,
                        (decimal)op.SUMM_ORD);
                    FinanseOperations.Add(newDoc);
                }

                foreach (var op in crsChange)
                {
                    var newDocIn = new FinanseOperation
                    {
                        DocDC = op.DOC_CODE,
                        Date = op.CH_DATE,
                        DocNum = op.CH_NUM_ORD.ToString(),
                        DocumentType = DocumentType.CurrencyChange,
                        Note = op.CH_NOTE
                    };
                    var newDocOut = new FinanseOperation
                    {
                        DocDC = op.DOC_CODE,
                        Date = op.CH_DATE,
                        DocNum = op.CH_NUM_ORD.ToString(),
                        DocumentType = DocumentType.CurrencyChange,
                        Note = op.CH_NOTE
                    };
                    newDocIn.SetSumma(GlobalOptions.ReferencesCache.GetCurrency(op.CH_CRS_IN_DC) as Currency,
                        (decimal)op.CH_CRS_IN_SUM,
                        0);
                    FinanseOperations.Add(newDocIn);
                    newDocOut.SetSumma(GlobalOptions.ReferencesCache.GetCurrency(op.CH_CRS_OUT_DC) as Currency, 0,
                        op.CH_CRS_OUT_SUM);
                    FinanseOperations.Add(newDocOut);
                }
            }
        }

        private void LoadMoneyInPathOperations()
        {
            MoneyInPathOperations.Clear();
            var firstList = myFirstBalans.ExtendRows.Where(_ => _.GroupId == ManagemenentBalansStructrue.MoneyInPah);
            var secondList = mySecondBalans.ExtendRows.Where(_ => _.GroupId == ManagemenentBalansStructrue.MoneyInPah);
            foreach (var f in firstList)
            {
                var newItem = new MoneyInPathItem
                {
                    DocumentType = f.DocumentType,
                    Date = f.Date,
                    DocCode = f.DocCode,
                    Code = f.Code,
                    DocumentNum = f.DocNum,
                    Currency = f.Currency,
                    KontragentName = f.KontragentName,
                    Summa = f.Summa
                };
                newItem.SetSumma(f.Currency, f.Summa, 0);
                MoneyInPathOperations.Add(newItem);
            }

            foreach (var s in secondList)
            {
                var old = MoneyInPathOperations.FirstOrDefault(_ => _.DocCode == s.DocCode && _.Code == s.Code);
                if (old == null)
                {
                    var newItem = new MoneyInPathItem
                    {
                        DocumentType = s.DocumentType,
                        Date = s.Date,
                        DocCode = s.DocCode,
                        Code = s.Code,
                        DocumentNum = s.DocNum,
                        Currency = s.Currency,
                        KontragentName = s.KontragentName,
                        Summa = s.Summa
                    };
                    newItem.SetSumma(s.Currency, 0, s.Summa);
                    MoneyInPathOperations.Add(newItem);
                }
                else
                {
                    old.SetSumma(s.Currency, s.Summa, s.Summa);
                }
            }
        }

        private decimal GetDC(ManagementBalanceExtendRowViewModel d, BalansSection tag)
        {
            switch (tag)
            {
                case BalansSection.Creditors:
                case BalansSection.Debitors:
                    return d.Kontragent.DocCode;
                case BalansSection.Store:
                    return d.Nom.DocCode;
                case BalansSection.Salary:
                    return d.Persona.DocCode;
            }

            return 0;
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
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = CloseWindowCommand
                }
            };
        }

        public override void RefreshData(object obj)
        {
            var myBalansBuilder = new ManagementBalansBuilder();

            var frm = Form as ManagementBalansCompareView;
            frm?.NavigateTo(typeof(EmptyUI));
            myFirstBalans = new ManagementBalansWindowViewModel { CurrentDate = FirstDate };
            myFirstBalans.RefreshData(null);
            mySecondBalans = new ManagementBalansWindowViewModel { CurrentDate = SecondDate };
            mySecondBalans.RefreshData(null);
            var storeSection = myFirstBalans.BalansStructure.Single(_ => _.Id == myBalansBuilder.Structure
                .Single(t => t.Tag == BalansSection.Store)
                .Id);
            var newId = Guid.NewGuid();
            var newGrp = new ManagementBalanceGroupViewModel
            {
                Id = newId,
                ParentId = storeSection.Id,
                Name = "Товары в пути",
                Order = 1,
                Summa = 0,
                SummaEUR = 0,
                SummaUSD = 0,
                SummaRUB = 0,
                SummaCNY = 0,
                SummaCHF = 0,
                SummaGBP = 0,
                ObjectDC = 10270000000
            };
            if (myFirstBalans.BalansStructure.Any(_ => _.Name == "Товары в пути") &&
                !mySecondBalans.BalansStructure.Any(_ => _.Name == "Товары в пути"))
            {
                mySecondBalans.BalansStructure.Add(newGrp);
            }

            if (!myFirstBalans.BalansStructure.Any(_ => _.Name == "Товары в пути") &&
                mySecondBalans.BalansStructure.Any(_ => _.Name == "Товары в пути"))
            {
                myFirstBalans.BalansStructure.Add(newGrp);
            }

            Data.Clear();
            var head = myFirstBalans.BalansStructure.Single(t => t.Tag == BalansSection.Head);
            Data.Add(new ManagementBalansCompareGroupViewModel
            {
                Id = head.Id,
                ParentId = head.ParentId,
                Name = head.Name,
                Tag = head.Tag,
                ObjectDC = head.ObjectDC
            });
            foreach (var d in myFirstBalans.BalansStructure.Where(
                         _ => _.ParentId == myFirstBalans.BalansStructure.Single(t => t.Tag == BalansSection.Head).Id))
                Data.Add(new ManagementBalansCompareGroupViewModel
                {
                    Id = d.Id,
                    ParentId = d.ParentId,
                    Name = d.Name,
                    Tag = d.Tag,
                    ObjectDC = d.ObjectDC
                });
            foreach (var item in Data)
            {
                var i1 = myFirstBalans.BalansStructure.FirstOrDefault(_ => _.Id == item.Id);
                var i2 = mySecondBalans.BalansStructure.FirstOrDefault(_ => _.Id == item.Id);
                item.SummaRUB = i1?.SummaRUB ?? 0;
                item.SummaCHF = i1?.SummaCHF ?? 0;
                item.SummaEUR = i1?.SummaEUR ?? 0;
                item.SummaUSD = i1?.SummaUSD ?? 0;
                item.SummaGBP = i1?.SummaGBP ?? 0;
                item.SummaSEK = i1?.SummaSEK ?? 0;
                
                item.SummaRUB2 = i2?.SummaRUB ?? 0;
                item.SummaCHF2 = i2?.SummaCHF ?? 0;
                item.SummaEUR2 = i2?.SummaEUR ?? 0;
                item.SummaUSD2 = i2?.SummaUSD ?? 0;
                item.SummaGBP2 = i2?.SummaGBP ?? 0;
                item.SummaSEK2 = i2?.SummaSEK ?? 0;
                
                item.SummaCNY = i1?.SummaCNY ?? 0;
                item.SummaCNY2 = i2?.SummaCNY ?? 0;
            }

            // Касса
            var cashId = Data.Single(_ => _.Id == myFirstBalans.BalansStructure.Single(t => t.Tag == BalansSection.Cash)
                .Id);
            var cash2 = (from item1 in mySecondBalans.BalansStructure.Where(_ => _.ParentId == cashId.Id)
                where myFirstBalans.BalansStructure.All(item2 => item2.ObjectDC != item1.ObjectDC)
                select item1).ToList();
            foreach (var i1 in myFirstBalans.BalansStructure.Where(_ => _.ParentId == cashId.Id))
            {
                var i2 =
                    mySecondBalans.BalansStructure.FirstOrDefault(_ => _.ObjectDC == i1.ObjectDC);
                var newItem = new ManagementBalansCompareGroupViewModel
                {
                    Id = i1.Id,
                    ParentId = i1.ParentId,
                    Name = i1.Name,
                    ObjectDC = i1.ObjectDC,
                    // ReSharper disable ConstantConditionalAccessQualifier
                    SummaRUB = i1?.SummaRUB ?? 0,
                    SummaCHF = i1?.SummaCHF ?? 0,
                    SummaEUR = i1?.SummaEUR ?? 0,
                    SummaUSD = i1?.SummaUSD ?? 0,
                    SummaGBP = i1?.SummaGBP ?? 0,
                    SummaSEK = i1?.SummaSEK ?? 0,
                    SummaRUB2 = i2?.SummaRUB ?? 0,
                    SummaCHF2 = i2?.SummaCHF ?? 0,
                    SummaEUR2 = i2?.SummaEUR ?? 0,
                    SummaUSD2 = i2?.SummaUSD ?? 0,
                    SummaGBP2 = i2?.SummaGBP ?? 0,
                    SummaSEK2 = i2?.SummaSEK ?? 0,

                    SummaCNY = i1?.SummaCNY ?? 0,
                    SummaCNY2 = i2?.SummaCNY ?? 0
                    // ReSharper restore ConstantConditionalAccessQualifier
                };
                Data.Add(newItem);
            }

            if (cash2.Count > 0)
                foreach (var i2 in cash2)
                {
                    var newItem = new ManagementBalansCompareGroupViewModel
                    {
                        Id = i2.Id,
                        ParentId = i2.ParentId,
                        Name = i2.Name,
                        ObjectDC = i2.ObjectDC,
                        SummaRUB = 0,
                        SummaCHF = 0,
                        SummaEUR = 0,
                        SummaUSD = 0,
                        SummaGBP = 0,
                        SummaSEK = 0,
                        SummaCNY = 0,
                        SummaRUB2 = i2?.SummaRUB ?? 0,
                        SummaCHF2 = i2?.SummaCHF ?? 0,
                        SummaEUR2 = i2?.SummaEUR ?? 0,
                        SummaUSD2 = i2?.SummaUSD ?? 0,
                        SummaGBP2 = i2?.SummaGBP ?? 0,
                        SummaSEK2 = i2?.SummaSEK ?? 0,
                        SummaCNY2 = i2?.SummaCNY ?? 0,
                    };
                    Data.Add(newItem);
                }

            // банк
            var bankId = Data.Single(_ => _.Id == myFirstBalans.BalansStructure.Single(t => t.Tag == BalansSection.Bank)
                .Id);
            var bank2 = (from item1 in mySecondBalans.BalansStructure.Where(_ => _.ParentId == bankId.Id)
                where myFirstBalans.BalansStructure.All(item2 => item2.ObjectDC != item1.ObjectDC)
                select item1).ToList();
            foreach (var i1 in myFirstBalans.BalansStructure.Where(_ => _.ParentId == bankId.Id))
            {
                var i2 =
                    mySecondBalans.BalansStructure.FirstOrDefault(_ => _.ObjectDC == i1.ObjectDC);
                var newItem = new ManagementBalansCompareGroupViewModel
                {
                    Id = i1.Id,
                    ParentId = i1.ParentId,
                    Name = i1.Name,
                    ObjectDC = i1.ObjectDC,
                    SummaRUB = i1?.SummaRUB ?? 0,
                    SummaCHF = i1?.SummaCHF ?? 0,
                    SummaEUR = i1?.SummaEUR ?? 0,
                    SummaUSD = i1?.SummaUSD ?? 0,
                    SummaGBP = i1?.SummaGBP ?? 0,
                    SummaSEK = i1?.SummaSEK ?? 0,
                    SummaRUB2 = i2?.SummaRUB ?? 0,
                    SummaCHF2 = i2?.SummaCHF ?? 0,
                    SummaEUR2 = i2?.SummaEUR ?? 0,
                    SummaUSD2 = i2?.SummaUSD ?? 0,
                    SummaGBP2 = i2?.SummaGBP ?? 0,
                    SummaSEK2 = i2?.SummaSEK ?? 0,
                    
                    SummaCNY = i1?.SummaCNY ?? 0,
                    SummaCNY2 = i2?.SummaCNY ?? 0
                };
                Data.Add(newItem);
            }

            if (bank2.Count > 0)
                foreach (var i2 in bank2)
                {
                    var newItem = new ManagementBalansCompareGroupViewModel
                    {
                        Id = i2.Id,
                        ParentId = i2.ParentId,
                        Name = i2.Name,
                        ObjectDC = i2.ObjectDC,
                        SummaRUB = 0,
                        SummaCHF = 0,
                        SummaEUR = 0,
                        SummaUSD = 0,
                        SummaGBP = 0,
                        SummaSEK = 0,  
                        SummaCNY = 0,

                        SummaRUB2 = i2?.SummaRUB ?? 0,
                        SummaCHF2 = i2?.SummaCHF ?? 0,
                        SummaEUR2 = i2?.SummaEUR ?? 0,
                        SummaUSD2 = i2?.SummaUSD ?? 0,
                        SummaGBP2 = i2?.SummaGBP ?? 0,
                        SummaSEK2 = i2?.SummaSEK ?? 0,
                        SummaCNY2 = i2?.SummaCNY ?? 0
                    };
                    Data.Add(newItem);
                }

            // склады
            var storeId = Data.Single(_ => _.Id == myFirstBalans.BalansStructure
                .Single(t => t.Tag == BalansSection.Store)
                .Id);
            var store2 = (from item1 in mySecondBalans.BalansStructure.Where(_ => _.ParentId == storeId.Id)
                where myFirstBalans.BalansStructure.All(item2 => item2.ObjectDC != item1.ObjectDC)
                select item1).ToList();
            foreach (var i1 in myFirstBalans.BalansStructure.Where(_ => _.ParentId == storeId.Id))
            {
                var i2 =
                    mySecondBalans.BalansStructure.FirstOrDefault(_ => _.ObjectDC == i1.ObjectDC);
                var newItem = new ManagementBalansCompareGroupViewModel
                {
                    Id = i1.Id,
                    ParentId = i1.ParentId,
                    Name = i1.Name,
                    ObjectDC = i1.ObjectDC,
                    SummaRUB = i1?.SummaRUB ?? 0,
                    SummaCHF = i1?.SummaCHF ?? 0,
                    SummaEUR = i1?.SummaEUR ?? 0,
                    SummaUSD = i1?.SummaUSD ?? 0,
                    SummaGBP = i1?.SummaGBP ?? 0,
                    SummaSEK = i1?.SummaSEK ?? 0,
                    SummaRUB2 = i2?.SummaRUB ?? 0,
                    SummaCHF2 = i2?.SummaCHF ?? 0,
                    SummaEUR2 = i2?.SummaEUR ?? 0,
                    SummaUSD2 = i2?.SummaUSD ?? 0,
                    SummaGBP2 = i2?.SummaGBP ?? 0,
                    SummaSEK2 = i2?.SummaSEK ?? 0,

                     
                    SummaCNY = i1?.SummaCNY ?? 0,
                    SummaCNY2 = i2?.SummaCNY ?? 0
                };
                Data.Add(newItem);
            }

            if (store2.Count > 0)
                foreach (var i2 in store2)
                {
                    var newItem = new ManagementBalansCompareGroupViewModel
                    {
                        Id = i2.Id,
                        ParentId = i2.ParentId,
                        Name = i2.Name,
                        ObjectDC = i2.ObjectDC,
                        SummaRUB = 0,
                        SummaCHF = 0,
                        SummaEUR = 0,
                        SummaUSD = 0,
                        SummaGBP = 0,
                        SummaSEK = 0,
                        SummaRUB2 = i2?.SummaRUB ?? 0,
                        SummaCHF2 = i2?.SummaCHF ?? 0,
                        SummaEUR2 = i2?.SummaEUR ?? 0,
                        SummaUSD2 = i2?.SummaUSD ?? 0,
                        SummaGBP2 = i2?.SummaGBP ?? 0,
                        SummaSEK2 = i2?.SummaSEK ?? 0,
                        
                        SummaCNY = 0,
                        SummaCNY2 = i2?.SummaCNY ?? 0
                    };
                    Data.Add(newItem);
                }

            var ch = myFirstBalans.BalansStructure.Single(t => t.Tag == BalansSection.Head);
            var form = (ManagementBalansCompareView)Form;
            if (form != null)
            {
                TreeListControlBand b;
                var frm1 = frm.ManagementBalansCompareMainUI;
                foreach (var col in frm1.treeListBalans.Columns)
                {
                    switch (col.FieldName)
                    {
                        case "SummaEUR":
                            b =
                                frm1.treeListBalans.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "SummaEUR"));
                            if (b != null)
                                b.Visible = Data.Sum(_ => _.SummaEUR) != 0 || Data.Sum(_ => _.SummaEUR2) != 0;
                            break;
                        case "SummaUSD":
                            b =
                                frm1.treeListBalans.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "SummaUSD"));
                            if (b != null)
                                b.Visible = Data.Sum(_ => _.SummaUSD) != 0 || Data.Sum(_ => _.SummaUSD2) != 0;
                            break;
                        case "SummaRUB":
                            b =
                                frm1.treeListBalans.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "SummaRUB"));
                            if (b != null)
                                b.Visible = Data.Sum(_ => _.SummaRUB) != 0 || Data.Sum(_ => _.SummaRUB2) != 0;
                            break;
                        case "SummaGBP":
                            b =
                                frm1.treeListBalans.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "SummaGBP"));
                            if (b != null)
                                b.Visible = Data.Sum(_ => _.SummaGBP) != 0 || Data.Sum(_ => _.SummaGBP2) != 0;
                            break;
                        case "SummaCHF":
                            b =
                                frm1.treeListBalans.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "SummaCHF"));
                            if (b != null)
                                b.Visible = Data.Sum(_ => _.SummaCHF) != 0 || Data.Sum(_ => _.SummaCHF2) != 0;
                            break;
                        case "SummaSEK":
                            b =
                                frm1.treeListBalans.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "SummaSEK"));
                            if (b != null)
                                b.Visible = Data.Sum(_ => _.SummaSEK) != 0 || Data.Sum(_ => _.SummaSEK2) != 0;
                            break;
                        case "SummaCNY":
                            b =
                                frm1.treeListBalans.Bands.FirstOrDefault(
                                    _ => _.Columns.Any(c => c.FieldName == "SummaCNY"));
                            if (b != null)
                                b.Visible = Data.Sum(_ => _.SummaCNY) != 0 || Data.Sum(_ => _.SummaCNY2) != 0;
                            break;
                    }
                }
            }

            RaisePropertyChanged(nameof(Data));
        }

        private void KontragentBalansOpen(object obj)
        {
            if (CurrentKontragent == null) return;
            try
            {
                var ctxk = new KontragentBalansWindowViewModel(CurrentKontragent.KontragentDC);
                var frm = new KontragentBalansForm { Owner = Application.Current.MainWindow, DataContext = ctxk };
                frm.Show();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        [MetadataType(typeof(DataAnnotationsKontragentBalansOperations))]
        public class KontragentBalansOperation : KontragentBalansRowViewModel, IMultyCurrency
        {
            public KontragentViewModel KontragentViewModel { set; get; }
            public Currency Currency => KontragentViewModel?.BalansCurrency;

            public bool IsDifferent => ResultUSD != 0 || ResultRUB != 0 || ResultEUR != 0 || ResultGBP != 0 ||
                                       ResultCHF != 0 ||
                                       ResultSEK != 0;

            public decimal LossUSD { get; set; }
            public decimal LossEUR { get; set; }
            public decimal LossGBP { get; set; }
            public decimal LossCHF { get; set; }
            public decimal LossSEK { get; set; }
            public decimal LossRUB { get; set; }
            public decimal ProfitUSD { get; set; }
            public decimal ProfitEUR { get; set; }
            public decimal ProfitGBP { get; set; }
            public decimal ProfitCHF { get; set; }
            public decimal ProfitSEK { get; set; }
            public decimal ProfitRUB { get; set; }
            public decimal ResultUSD => ProfitUSD - LossUSD;
            public decimal ResultRUB => ProfitRUB - LossRUB;
            public decimal ResultEUR => ProfitEUR - LossEUR;
            public decimal ResultGBP => ProfitGBP - LossGBP;
            public decimal ResultCHF => ProfitCHF - LossCHF;
            public decimal ResultSEK => ProfitSEK - LossSEK;

            public decimal LossCNY { get; set; }
            public decimal ProfitCNY { get; set; }
            public decimal ResultCNY => ProfitCNY - LossCNY;

            public void SetSumma(Currency crs, decimal summa, decimal summa2)
            {
                switch (crs.Name)
                {
                    case CurrencyCode.RUBName:
                    case CurrencyCode.RURName:
                        LossRUB = summa;
                        ProfitRUB = summa2;
                        return;
                    case CurrencyCode.USDName:
                        LossUSD = summa;
                        ProfitUSD = summa2;
                        return;
                    case CurrencyCode.EURName:
                        LossEUR = summa;
                        ProfitEUR = summa2;
                        return;
                    case CurrencyCode.GBPName:
                        LossGBP = summa;
                        ProfitGBP = summa2;
                        return;
                    case CurrencyCode.SEKName:
                        LossSEK = summa;
                        ProfitSEK = summa2;
                        return;
                    case CurrencyCode.CHFName:
                        LossCHF = summa;
                        ProfitCHF = summa2;
                        return;
                    case CurrencyCode.CNYName:
                        LossCNY = summa;
                        ProfitCNY = summa2;
                        return;
                }
            }

            public void SetSumma2(Currency crs, decimal summa2)
            {
                switch (crs.Name)
                {
                    case CurrencyCode.RUBName:
                    case CurrencyCode.RURName:
                        ProfitRUB = summa2;
                        return;
                    case CurrencyCode.USDName:
                        ProfitUSD = summa2;
                        return;
                    case CurrencyCode.EURName:
                        ProfitEUR = summa2;
                        return;
                    case CurrencyCode.GBPName:
                        ProfitGBP = summa2;
                        return;
                    case CurrencyCode.SEKName:
                        ProfitSEK = summa2;
                        return;
                    case CurrencyCode.CHFName:
                        ProfitCHF = summa2;
                        return;
                    case CurrencyCode.CNYName:
                        ProfitCNY = summa2;
                        return;
                }
            }
        }

        public static class DataAnnotationsKontragentBalansOperations
        {
            public static void BuildMetadata(MetadataBuilder<KontragentBalansOperation> builder)
            {
                //builder.Property(_ => _.).DisplayName("").Description("").ReadOnly();
                builder.Property(_ => _.DocCode).NotAutoGenerated();
                builder.Property(_ => _.Id).NotAutoGenerated();
                builder.Property(_ => _.State).NotAutoGenerated();
                builder.Property(_ => _.DocDC).NotAutoGenerated();
                builder.Property(_ => _.IsDifferent).NotAutoGenerated();
                builder.Property(_ => _.Name).NotAutoGenerated();
                builder.Property(_ => _.KontragentViewModel).NotAutoGenerated();
                builder.Property(_ => _.Currency).NotAutoGenerated();
                builder.Property(_ => _.Note).NotAutoGenerated();
                builder.Property(_ => _.DocRowCode).NotAutoGenerated();
                builder.Property(_ => _.CrsOperDC).NotAutoGenerated();
                builder.Property(_ => _.DocTypeCode).NotAutoGenerated();
                builder.Property(_ => _.CrsKontrIn).NotAutoGenerated();
                builder.Property(_ => _.CrsKontrOut).NotAutoGenerated();
                builder.Property(_ => _.CrsOperIn).NotAutoGenerated();
                builder.Property(_ => _.CrsOperOut).NotAutoGenerated();
                builder.Property(_ => _.CrsUchRate).NotAutoGenerated();
                builder.Property(_ => _.CrsOperRate).NotAutoGenerated();
                builder.Property(_ => _.Nakopit).NotAutoGenerated();
                builder.Property(_ => _.DocName).DisplayName("Наименование").Description("").ReadOnly();
                builder.Property(_ => _.DocNum).DisplayName("№").Description("").ReadOnly();
                builder.Property(_ => _.DocDate).DisplayName("Дата").Description("").ReadOnly();
                builder.Property(_ => _.Note).DisplayName("Примечание").Description("").ReadOnly();
                builder.Group("Документ")
                    .ContainsProperty(_ => _.DocName)
                    .ContainsProperty(_ => _.DocNum)
                    .ContainsProperty(_ => _.DocDate)
                    .ContainsProperty(_ => _.Note);
                builder.Property(_ => _.LossRUB).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.ProfitRUB)
                    .DisplayName("Расход")
                    .Description("Расход RUB")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.ResultRUB)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("RUB")
                    .ContainsProperty(_ => _.LossRUB)
                    .ContainsProperty(_ => _.ProfitRUB)
                    .ContainsProperty(_ => _.ResultRUB);
                builder.Property(_ => _.LossUSD).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.ProfitUSD)
                    .DisplayName("Расход")
                    .Description("Расход USD")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.ResultUSD)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("USD")
                    .ContainsProperty(_ => _.LossUSD)
                    .ContainsProperty(_ => _.ProfitUSD)
                    .ContainsProperty(_ => _.ResultUSD);
                builder.Property(_ => _.LossEUR).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.ProfitEUR)
                    .DisplayName("Расход")
                    .Description("Расход EUR")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.ResultEUR)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("EUR")
                    .ContainsProperty(_ => _.LossEUR)
                    .ContainsProperty(_ => _.ProfitEUR)
                    .ContainsProperty(_ => _.ResultEUR);
                builder.Property(_ => _.LossGBP).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.ProfitGBP)
                    .DisplayName("Расход")
                    .Description("Расход GBP")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.ResultGBP)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("GBP")
                    .ContainsProperty(_ => _.LossGBP)
                    .ContainsProperty(_ => _.ProfitGBP)
                    .ContainsProperty(_ => _.ResultGBP);
                builder.Property(_ => _.LossCHF).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.ProfitCHF)
                    .DisplayName("Расход")
                    .Description("Расход CHF")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.ResultCHF)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("CHF")
                    .ContainsProperty(_ => _.LossCHF)
                    .ContainsProperty(_ => _.ProfitCHF)
                    .ContainsProperty(_ => _.ResultCHF);
                builder.Property(_ => _.LossSEK).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.ProfitSEK)
                    .DisplayName("Расход")
                    .Description("Расход SEK")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.ResultSEK)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("SEK")
                    .ContainsProperty(_ => _.LossSEK)
                    .ContainsProperty(_ => _.ProfitSEK)
                    .ContainsProperty(_ => _.ResultSEK);

                builder.Property(_ => _.LossCNY).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.ProfitCNY)
                    .DisplayName("Расход")
                    .Description("Расход CNY")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.ResultCNY)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("CNY")
                    .ContainsProperty(_ => _.LossCNY)
                    .ContainsProperty(_ => _.ProfitCNY)
                    .ContainsProperty(_ => _.ResultCNY);
            }
        }

        [MetadataType(typeof(DataAnnotationsCFinanseOperation))]
        public class FinanseOperation : FinanseOperationItem
        {
        }

        public static class DataAnnotationsCFinanseOperation
        {
            public static void BuildMetadata(MetadataBuilder<FinanseOperation> builder)
            {
                //builder.Property(_ => _.).DisplayName("").Description("").ReadOnly();
                builder.Property(_ => _.ParentId).NotAutoGenerated();
                builder.Property(_ => _.CurrencyName).NotAutoGenerated();
                builder.Property(_ => _.DocCode).NotAutoGenerated();
                builder.Property(_ => _.Id).NotAutoGenerated();
                builder.Property(_ => _.Parent).NotAutoGenerated();
                builder.Property(_ => _.ParentDC).NotAutoGenerated();
                builder.Property(_ => _.ParentDC).NotAutoGenerated();
                builder.Property(_ => _.State).NotAutoGenerated();
                builder.Property(_ => _.StringId).NotAutoGenerated();
                builder.Property(_ => _.KontragentDC).NotAutoGenerated();
                builder.Property(_ => _.DocDC).NotAutoGenerated();
                builder.Property(_ => _.IsDifferent).NotAutoGenerated();
                builder.Property(_ => _.Name).NotAutoGenerated();
                builder.Property(_ => _.Currency).DisplayName("Валюта").Description("Валюта операции").ReadOnly();
                builder.Property(_ => _.Date).DisplayName("Дата").ReadOnly();
                builder.Property(_ => _.DocNum).ReadOnly().DisplayName("Номер документа").Description("");
                builder.Property(_ => _.KontragentName).ReadOnly().DisplayName("Котнрагент").Description("");
                builder.Property(_ => _.Summa).ReadOnly().DisplayName("Сумма").Description("Сумма операции");
                builder.Property(_ => _.TabelNumber)
                    .ReadOnly()
                    .DisplayName("Таб.№")
                    .Description("Табельный номер для сорудника");
                builder.Property(_ => _.DocumentType).DisplayName("Документ").Description("").ReadOnly();
                builder.Property(_ => _.KontragentType)
                    .DisplayName("Тип контрагента")
                    .Description("Контрагент/сотрудник/касса/банк")
                    .ReadOnly();
                builder.Property(_ => _.Note).DisplayName("Примечание").Description("").ReadOnly();
                builder.Group("Документ")
                    .ContainsProperty(_ => _.DocumentType)
                    .ContainsProperty(_ => _.Date)
                    .ContainsProperty(_ => _.DocNum)
                    .ContainsProperty(_ => _.KontragentName)
                    .ContainsProperty(_ => _.KontragentType)
                    .ContainsProperty(_ => _.TabelNumber)
                    .ContainsProperty(_ => _.Note)
                    .ContainsProperty(_ => _.Currency)
                    .ContainsProperty(_ => _.Summa);
                builder.Property(_ => _.SummaRUB).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.SummaRUB2)
                    .DisplayName("Расход")
                    .Description("Расход RUB")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaRUB)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("RUB")
                    .ContainsProperty(_ => _.SummaRUB)
                    .ContainsProperty(_ => _.SummaRUB2)
                    .ContainsProperty(_ => _.DeltaRUB);
                builder.Property(_ => _.SummaUSD).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.SummaUSD2)
                    .DisplayName("Расход")
                    .Description("Расход USD")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaUSD)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("USD")
                    .ContainsProperty(_ => _.SummaUSD)
                    .ContainsProperty(_ => _.SummaUSD2)
                    .ContainsProperty(_ => _.DeltaUSD);
                builder.Property(_ => _.SummaEUR).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.SummaEUR2)
                    .DisplayName("Расход")
                    .Description("Расход EUR")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaEUR)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("EUR")
                    .ContainsProperty(_ => _.SummaEUR)
                    .ContainsProperty(_ => _.SummaEUR2)
                    .ContainsProperty(_ => _.DeltaEUR);
                builder.Property(_ => _.SummaGBP).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.SummaGBP2)
                    .DisplayName("Расход")
                    .Description("Расход GBP")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaGBP)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("GBP")
                    .ContainsProperty(_ => _.SummaGBP)
                    .ContainsProperty(_ => _.SummaGBP2)
                    .ContainsProperty(_ => _.DeltaGBP);
                builder.Property(_ => _.SummaCHF).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.SummaCHF2)
                    .DisplayName("Расход")
                    .Description("Расход CHF")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaCHF)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("CHF")
                    .ContainsProperty(_ => _.SummaCHF)
                    .ContainsProperty(_ => _.SummaCHF2)
                    .ContainsProperty(_ => _.DeltaCHF);
                builder.Property(_ => _.SummaSEK).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.SummaSEK2)
                    .DisplayName("Расход")
                    .Description("Расход SEK")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaSEK)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("SEK")
                    .ContainsProperty(_ => _.SummaSEK)
                    .ContainsProperty(_ => _.SummaSEK2)
                    .ContainsProperty(_ => _.DeltaSEK);

                builder.Property(_ => _.SummaCNY).ReadOnly().DisplayName("Приход").Description("").NumericMask("n2");
                builder.Property(_ => _.SummaCNY2)
                    .DisplayName("Расход")
                    .Description("Расход CNY")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaCNY)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .ReadOnly()
                    .NumericMask("n2");
                builder.Group("CNY")
                    .ContainsProperty(_ => _.SummaCNY)
                    .ContainsProperty(_ => _.SummaCNY2)
                    .ContainsProperty(_ => _.DeltaCNY);
            }
        }

        [MetadataType(typeof(DataAnnotationsEmployeeSalary))]
        public class EmployeeSalary : SummaCompareCurrencies
        {
            public Employee Employee { set; get; }
            public string CurrencyName => ((IName)Employee?.Currency)?.Name;
        }

        public class DataAnnotationsEmployeeSalary : DataAnnotationForFluentApiBase, IMetadataProvider<EmployeeSalary>
        {
            void IMetadataProvider<EmployeeSalary>.BuildMetadata(MetadataBuilder<EmployeeSalary> builder)
            {
                SetNotAutoGenerated(builder);
                builder.Property(_ => _.Employee).AutoGenerated().DisplayName("ФИО");
                builder.Property(_ => _.CurrencyName).AutoGenerated().DisplayName("Валюта");
                builder.Group("Сотрудник")
                    .ContainsProperty(_ => _.Employee)
                    .ContainsProperty(_ => _.CurrencyName);
                builder.Property(_ => _.SummaRUB).AutoGenerated().DisplayName("На начало").Description("")
                    .NumericMask("n2");
                builder.Property(_ => _.SummaRUB2)
                    .DisplayName("На конец")
                    .Description("На конец")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaRUB)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Group("RUB")
                    .ContainsProperty(_ => _.SummaRUB)
                    .ContainsProperty(_ => _.SummaRUB2)
                    .ContainsProperty(_ => _.DeltaRUB);
                builder.Property(_ => _.SummaUSD).AutoGenerated().DisplayName("На начало").Description("")
                    .NumericMask("n2");
                builder.Property(_ => _.SummaUSD2)
                    .DisplayName("На конец")
                    .Description("На конец")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaUSD)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Group("USD")
                    .ContainsProperty(_ => _.SummaUSD)
                    .ContainsProperty(_ => _.SummaUSD2)
                    .ContainsProperty(_ => _.DeltaUSD);
                builder.Property(_ => _.SummaEUR).AutoGenerated().DisplayName("На начало").Description("")
                    .NumericMask("n2");
                builder.Property(_ => _.SummaEUR2)
                    .DisplayName("На конец")
                    .Description("На конец")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaEUR)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Group("EUR")
                    .ContainsProperty(_ => _.SummaEUR)
                    .ContainsProperty(_ => _.SummaEUR2)
                    .ContainsProperty(_ => _.DeltaEUR);
                builder.Property(_ => _.SummaGBP).AutoGenerated().DisplayName("На начало").Description("")
                    .NumericMask("n2");
                builder.Property(_ => _.SummaGBP2)
                    .DisplayName("На конец")
                    .Description("На конец")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaGBP)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Group("GBP")
                    .ContainsProperty(_ => _.SummaGBP)
                    .ContainsProperty(_ => _.SummaGBP2)
                    .ContainsProperty(_ => _.DeltaGBP);
                builder.Property(_ => _.SummaCHF).AutoGenerated().DisplayName("На начало").Description("")
                    .NumericMask("n2");
                builder.Property(_ => _.SummaCHF2)
                    .DisplayName("На конец")
                    .Description("На конец")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaCHF)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Group("CHF")
                    .ContainsProperty(_ => _.SummaCHF)
                    .ContainsProperty(_ => _.SummaCHF2)
                    .ContainsProperty(_ => _.DeltaCHF);
                builder.Property(_ => _.SummaSEK).AutoGenerated().DisplayName("На начало").Description("")
                    .NumericMask("n2");
                builder.Property(_ => _.SummaSEK2)
                    .DisplayName("На конец")
                    .Description("На конец")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaSEK)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Group("SEK")
                    .ContainsProperty(_ => _.SummaSEK)
                    .ContainsProperty(_ => _.SummaSEK2)
                    .ContainsProperty(_ => _.DeltaSEK);

                builder.Property(_ => _.SummaCNY).AutoGenerated().DisplayName("На начало").Description("")
                    .NumericMask("n2");
                builder.Property(_ => _.SummaCNY2)
                    .DisplayName("На конец")
                    .Description("На конец")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Property(_ => _.DeltaCNY)
                    .DisplayName("Результат")
                    .Description("Результат")
                    .AutoGenerated()
                    .NumericMask("n2");
                builder.Group("CNY")
                    .ContainsProperty(_ => _.SummaCNY)
                    .ContainsProperty(_ => _.SummaCNY2)
                    .ContainsProperty(_ => _.DeltaCNY);
            }
        }

        #region Command

        public ICommand NomenklCalcOpenCommand
        {
            get { return new Command(NomenklCalcOpen, _ => NomenklCurrent != null); }
        }

        public ICommand KontragentAccountOpenCommand
        {
            get
            {
                return new Command(KontragentAccountOpen,
                    _ => CurrentExtendItem?.Tag == BalansSection.Debitors ||
                         CurrentExtendItem?.Tag == BalansSection.Creditors);
            }
        }

        private void KontragentAccountOpen(object obj)
        {
            var ctxk = new KontragentBalansWindowViewModel((decimal)CurrentExtendItem?.DocCode);
            var frm = new KontragentBalansForm { Owner = Application.Current.MainWindow, DataContext = ctxk };
            frm.Show();
        }

        private void NomenklCalcOpen(object obj)
        {
            if (NomenklCurrent?.DocCode == null) return;
            var ctx = new NomPriceWindowViewModel((decimal)NomenklCurrent?.NomenklDC);
            var dlg = new SelectDialogView { DataContext = ctx };
            dlg.ShowDialog();
        }

        private ManagementBalansCompareRowViewModel myCurrentExtendItem;

        public ManagementBalansCompareRowViewModel CurrentExtendItem
        {
            get => myCurrentExtendItem;
            set
            {
                if (myCurrentExtendItem == value) return;
                myCurrentExtendItem = value;
                RaisePropertyChanged();
            }
        }

        #endregion
    }

    public enum ObjectTypeEnum
    {
        Kontragent,
        Nomenkl
    }
}
