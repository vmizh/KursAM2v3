using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Core.ViewModel.Base;
using Data;
using KursDomain;
using KursDomain.IDocuments.IQueryResults;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class NomenklInnerMoveQueryViewModel : RSWindowViewModelBase
    {
        public ObservableCollection<NomenklInnerMoveQueryResult> ItemsCollection { get; set; }
            = new ObservableCollection<NomenklInnerMoveQueryResult>();
        public NomenklInnerMoveQueryViewModel(KursDomain.References.Warehouse warehouseTo, DateTime maxDate)
        {
            LayoutName = "NomenklInnerMoveQueryViewModel";
            WarehouseTo = warehouseTo;
            MaxDate = maxDate;
        }

        public KursDomain.References.Warehouse WarehouseTo { get; }
        public DateTime MaxDate { get; }

        public override string WindowName => $"Поиск перемещений на склад {WarehouseTo}";

        public override void RefreshData(object obj)
        {
            ItemsCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = from td24 in ctx.TD_24
                    from sd24 in ctx.SD_24
                    where td24.DOC_CODE == sd24.DOC_CODE
                          && sd24.DD_DATE <= new DateTime(2024, 1, 1)
                          && sd24.DD_TYPE_DC == 2010000003
                          && sd24.DD_SKLAD_POL_DC != null
                          && td24.DDT_RASH_ORD_DC == null
                    orderby sd24.DD_DATE
                    select new
                    {
                        DocDC = td24.DOC_CODE,
                        Code = td24.CODE,
                        NomDC = td24.DDT_NOMENKL_DC,
                        Quantity = td24.DDT_KOL_RASHOD,
                        StoreFromDC = sd24.DD_SKLAD_OTPR_DC,
                        DocDate = sd24.DD_DATE,
                        DocInNum = sd24.DD_IN_NUM,
                        DocExtNum = sd24.DD_EXT_NUM,
                        Note = sd24.DD_NOTES
                    };
                foreach (var d in data.ToList())
                {
                    var nom = GlobalOptions.ReferencesCache.GetNomenkl(d.NomDC) as Nomenkl;
                    var store = GlobalOptions.ReferencesCache.GetWarehouse(d.StoreFromDC) as KursDomain.References.Warehouse;
                    string num = string.IsNullOrWhiteSpace(d.DocExtNum) ? d.DocInNum.ToString() : $"{d.DocInNum}/{d.DocExtNum}";
                    ItemsCollection.Add(new NomenklInnerMoveQueryResult(d.DocDC, d.Code, nom, 
                        d.Quantity, store, d.DocDate, num, d.Note));
                }

            }

        }
    }
}
