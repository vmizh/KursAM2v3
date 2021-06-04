using System;
using System.Collections.ObjectModel;
using System.Linq;
using Calculates.Materials;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.View.Logistiks;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomTransferAddForSklad : RSWindowViewModelBase
    {
        private Currency myCurrency;
        private NomTransferAddForSkladUC myDataUserControl;
        private DateTime myDate;
        private Core.EntityViewModel.NomenklManagement.Warehouse myWarehouse;

        public NomTransferAddForSklad(Core.EntityViewModel.NomenklManagement.Warehouse warehouse)
        {
            myDataUserControl = new NomTransferAddForSkladUC();
            Warehouse = warehouse;
            Date = DateTime.Today;
            LoadReference();
        }

        public NomTransferAddForSklad(Core.EntityViewModel.NomenklManagement.Warehouse warehouse, DateTime date)
        {
            myDataUserControl = new NomTransferAddForSkladUC();
            Warehouse = warehouse;
            Date = date;
            LoadReference();
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<Currency> CurrencyCollection { set; get; } = new ObservableCollection<Currency>();

        public Core.EntityViewModel.NomenklManagement.Warehouse Warehouse
        {
            get => myWarehouse;
            set
            {
                if (myWarehouse != null && myWarehouse.Equals(value)) return;
                myWarehouse = value;
                if (myWarehouse != null && Currency != null)
                    LoadOstatki(myWarehouse, Currency);
                RaisePropertyChanged();
            }
        }

        public Currency Currency
        {
            get => myCurrency;
            set
            {
                if (Equals(myCurrency, value)) return;
                myCurrency = value;
                if (myWarehouse != null && Currency != null)
                    LoadOstatki(myWarehouse, Currency);
                RaisePropertyChanged();
            }
        }

        public DateTime Date
        {
            get => myDate;
            set
            {
                if (myDate == value) return;
                myDate = value;
                if (myWarehouse != null && Currency != null)
                    LoadOstatki(myWarehouse, Currency);
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<NomenklRemainWithPrice> Rows { set; get; } =
            new ObservableCollection<NomenklRemainWithPrice>();

        public NomTransferAddForSkladUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        private void LoadReference()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var c in ctx.SD_301)
                        CurrencyCollection.Add(new Currency(c));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void LoadOstatki(Core.EntityViewModel.NomenklManagement.Warehouse warehouse, Currency crs)
        {
            Rows.Clear();
            foreach (
                var n in
                NomenklCalculationManager.GetNomenklStoreRemains(Date, warehouse.DocCode)
                    .Where(_ => _.NomCurrencyDC == crs.DocCode
                                && _.Remain != 0))
                Rows.Add(new NomenklRemainWithPrice
                {
                    Nomenkl = MainReferences.GetNomenkl(n.NomenklDC),
                    IsSelected = false,
                    Price = n.Price,
                    Quantity = n.Remain,
                    PriceWithNaklad = n.PriceWithNaklad
                });
        }
    }
}