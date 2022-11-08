using System;
using System.Collections.ObjectModel;
using System.Linq;
using Calculates.Materials;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Xpf.CodeView;
using KursAM2.View.Logistiks;
using KursDomain;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomTransferAddForSklad : RSWindowViewModelBase
    {
        private Currency myCurrency;
        private NomTransferAddForSkladUC myDataUserControl;
        private DateTime myDate;
        private KursDomain.Documents.NomenklManagement.Warehouse myWarehouse;

        public NomTransferAddForSklad(KursDomain.Documents.NomenklManagement.Warehouse warehouse)
        {
            myDataUserControl = new NomTransferAddForSkladUC();
            Warehouse = warehouse;
            Date = DateTime.Today;
            LoadReference();
        }

        public NomTransferAddForSklad(KursDomain.Documents.NomenklManagement.Warehouse warehouse, DateTime date)
        {
            myDataUserControl = new NomTransferAddForSkladUC();
            Warehouse = warehouse;
            Date = date;
            LoadReference();
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<Currency> CurrencyCollection { set; get; } = new ObservableCollection<Currency>();

        public KursDomain.Documents.NomenklManagement.Warehouse Warehouse
        {
            get => myWarehouse;
            set
            {
                if (Equals(myWarehouse,value)) return;
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
                    {
                        var crs = new Currency();
                        crs.LoadFromEntity(c);
                        CurrencyCollection.Add(crs);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void LoadOstatki(KursDomain.Documents.NomenklManagement.Warehouse warehouse, Currency crs)
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
