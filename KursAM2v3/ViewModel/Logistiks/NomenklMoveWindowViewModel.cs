using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.View.Logistiks;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklMoveWindowViewModel : RSWindowViewModelBase
    {
        private NomenklCalcCostOperation myCurrentOperation;
        private Core.EntityViewModel.NomenklManagement.Warehouse myCurrentWarehouse;
        private DateTime myDateForSklad;
        private Nomenkl mySelectedNomenkl;

        public NomenklMoveWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            LoadReference();
            DateForSklad = DateTime.Today;
            Operations = new List<NomenklCalcCostOperation>();
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<Core.EntityViewModel.NomenklManagement.Warehouse> StoreCollection { set; get; } =
            new ObservableCollection<Core.EntityViewModel.NomenklManagement.Warehouse>();

        public List<Nomenkl> Nomenkls =>
            MainReferences.ALLNomenkls.Values.Where(_ => _.IsUsluga == false).ToList();

        public List<NomenklCalcCostOperation> Operations { set; get; }

        public Core.EntityViewModel.NomenklManagement.Warehouse CurrentWarehouse
        {
            get => myCurrentWarehouse;
            set
            {
                if (myCurrentWarehouse != null && myCurrentWarehouse.Equals(value)) return;
                myCurrentWarehouse = value;
                LoadOstatki();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NomenklRemains));
            }
        }

        public DateTime DateForSklad
        {
            get => myDateForSklad;
            set
            {
                if (myDateForSklad == value) return;
                myDateForSklad = value;
                RaisePropertyChanged();
            }
        }

        public NomenklCalcCostOperation CurrentOperation
        {
            get => myCurrentOperation;
            set
            {
                if (myCurrentOperation != null && myCurrentOperation.Equals(value)) return;
                myCurrentOperation = value;
                if (myCurrentOperation != null)
                    CalcOstatki(myCurrentOperation);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FinanceDocumentName));
                RaisePropertyChanged(nameof(TovarDocumentName));
            }
        }

        public Nomenkl SelectedNomenkl
        {
            get => mySelectedNomenkl;
            set
            {
                if (mySelectedNomenkl != null && mySelectedNomenkl.Equals(value)) return;
                mySelectedNomenkl = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NomenklNumber));
                if (mySelectedNomenkl == null)
                {
                    Operations = new List<NomenklCalcCostOperation>();
                    SkladOstatki.Clear();
                    RaisePropertiesChanged(nameof(SkladOstatki));
                    RaisePropertyChanged(nameof(Operations));
                    return;
                }

                LoadOperation();
                if (Operations.Count > 0)
                    CurrentOperation = Operations.First();
                RaisePropertyChanged(nameof(Operations));
                CalcOstatki(myCurrentOperation);
            }
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<NomenklOstatkiWithPrice> SkladOstatki { set; get; } =
            new ObservableCollection<NomenklOstatkiWithPrice>();

        public ObservableCollection<NomenklOstatkiWithPrice> NomenklRemainForAllStores { set; get; }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<NomenklRemainWithPrice> NomenklRemains { set; get; } =
            new ObservableCollection<NomenklRemainWithPrice>();

        public string NomenklNumber => SelectedNomenkl?.NomenklNumber;

        public string FinanceDocumentName
        {
            get
            {
                if (CurrentOperation?.FinDocumentDC == null) return null;
                switch (CurrentOperation.OperCode)
                {
                    case 1:
                        return "Открыть счет/фактура поставщика";
                    case 2:
                        return "Открыть счет/фактура клиенту";
                    case 5:
                        return "Открыть инвентаризационную ведомость";
                    case 12:
                        return "Открыть счет/фактура клиенту";
                    case 18:
                        return "Открыть Продажа за наличный расчет";
                }

                return "Финансовый документ отсутствует";
            }
        }

        public string TovarDocumentName
        {
            get
            {
                if (CurrentOperation.TovarDocument == null) return null;
                switch (CurrentOperation.OperCode)
                {
                    case 1:
                        return "Открыть приходный складской ордер";
                    case 2:
                        return "Открыть расходный складской ордер";
                    case 5:
                        return "Открыть инвентаризационную ведомость";
                    case 12:
                        return "Открыть расходную накладную";
                    case 18:
                        return "Открыть Продажа за наличный расчет";
                }

                return "Товарный документ отсутствует";
            }
        }

        private void LoadReference()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var c in ctx.SD_27)
                        StoreCollection.Add(new Core.EntityViewModel.NomenklManagement.Warehouse(c));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void RefreshData(object obj)
        {
            LoadOperation();
            if (CurrentWarehouse != null)
                LoadOstatki();
        }

        private void LoadOstatki()
        {
            NomenklRemains.Clear();
            if (CurrentWarehouse == null) return;
            foreach (var nom in NomenklCalculationManager.GetNomenklStoreRemains(DateForSklad, CurrentWarehouse.DocCode)
                )
                //LoadRemains(DateForSklad,CurrentWarehouse))
                NomenklRemains.Add(
                    new NomenklRemainWithPrice
                    {
                        Nomenkl = MainReferences.GetNomenkl(nom.NomenklDC),
                        IsSelected = false,
                        Quantity = nom.Remain,
                        Price = nom.Price
                    });
        }

        private void CalcOstatki(NomenklCalcCostOperation op)
        {
            SkladOstatki.Clear();
            if (op == null || Operations == null || Operations.Count == 0) return;
            var skladsIn =
                Operations.Where(_ => _.DocDate <= op.DocDate)
                    .Select(_ => _.SkladInName)
                    .Where(_ => _ != null)
                    .ToList();
            var skladsOut =
                Operations.Where(_ => _.DocDate <= op.DocDate)
                    .Select(_ => _.SkladOutName)
                    .Where(_ => _ != null)
                    .ToList();
            var sklads = skladsIn.Concat(skladsOut).Distinct().ToList();
            var prcRow = Operations.Where(_ => _.DocDate <= op.DocDate).Max(_ => _.RowNumber);
            var prc = Operations.Single(_ => _.RowNumber == prcRow);
            foreach (var s in sklads)
            {
                var q =
                    Operations.Where(_ => _.SkladInName == s && _.DocDate <= op.DocDate)
                        .Sum(_ => _.QuantityIn) -
                    Operations.Where(_ => _.SkladOutName == s && _.DocDate <= op.DocDate)
                        .Sum(_ => _.QuantityOut);
                var newSklad = new NomenklOstatkiWithPrice
                {
                    Quantity = q,
                    PriceWONaklad = prc.CalcPrice,
                    Price = prc.CalcPriceNaklad,
                    SummaWONaklad = prc.CalcPrice * q,
                    Summa = prc.CalcPriceNaklad * q
                };
                if (newSklad.Quantity != 0)
                    SkladOstatki.Add(newSklad);
            }

            RaisePropertyChanged(nameof(SkladOstatki));
        }

        private void LoadOperation()
        {
            if (SelectedNomenkl == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var calc = new NomenklCostMediumSliding(ctx);
                Operations = new List<NomenklCalcCostOperation>(calc.GetAllOperations(SelectedNomenkl.DocCode));
                if (Operations.Count > 0)
                    CurrentOperation = Operations.First();
            }
            RaisePropertyChanged(nameof(Operations));
        }

        #region Command

        public ICommand FinanceDocumentOpenCommand
        {
            get { return new Command(FinanceDocumentOpen, param => CurrentOperation.FinDocumentDC != null); }
        }

        private void FinanceDocumentOpen(object obj)
        {
            switch (CurrentOperation.OperCode)
            {
                case 1:
                    WindowManager.ShowFunctionNotReleased();
                    //return $"Открыть счет/фактура поставщика";
                    break;
                case 2:
                    WindowManager.ShowFunctionNotReleased();
                    //return $"Открыть счет/фактура клиенту";
                    break;
                case 5:
                    WindowManager.ShowFunctionNotReleased();
                    //return $"Открыть инвентаризационную ведомость";
                    break;
                case 12:
                    WindowManager.ShowFunctionNotReleased();
                    //return $"Открыть счет/фактура клиенту";
                    break;
                case 18:
                    WindowManager.ShowFunctionNotReleased();
                    //return $"Открыть Продажа за наличный расчет";
                    break;
            }
        }

        public ICommand ClearNomenklCommand
        {
            get { return new Command(ClearNomenkl, _ => true); }
        }

        private void ClearNomenkl(object obj)
        {
            SelectedNomenkl = null;
        }

        #endregion

        #region Warehouses

        private decimal mySklad1;

        public decimal Sklad1
        {
            get => mySklad1;
            set
            {
                if (mySklad1 == value) return;
                mySklad1 = value;
                RaisePropertyChanged();
            }
        }

        private decimal mySklad2;

        public decimal Sklad2
        {
            get => mySklad2;
            set
            {
                if (mySklad2 == value) return;
                mySklad2 = value;
                RaisePropertyChanged();
            }
        }

        private decimal mySklad3;

        public decimal Sklad3
        {
            get => mySklad3;
            set
            {
                if (mySklad3 == value) return;
                mySklad3 = value;
                RaisePropertyChanged();
            }
        }

        private decimal mySklad4;

        public decimal Sklad4
        {
            get => mySklad4;
            set
            {
                if (mySklad4 == value) return;
                mySklad4 = value;
                RaisePropertyChanged();
            }
        }

        #endregion
    }
}