using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Helper;
using KursAM2.View.DialogUserControl;
using KursDomain.Documents.NomenklManagement;

namespace KursAM2.View.Logistiks.UC
{
    public class WarehouseOrderOutRowSelect : WarehouseOrderOutRow
    {
        private bool myIsSelected;

        public WarehouseOrderOutRowSelect(TD_24 entity) : base(entity)
        {
        }

        [Display(AutoGenerateField = true, Name = "Выбор")]
        public bool IsSelected
        {
            set
            {
                if (myIsSelected == value) return;
                myIsSelected = value;
                RaisePropertiesChanged();
            }
            get => myIsSelected;
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class AddNomenklFromRashOrderViewModel : RSWindowViewModelBase
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private WarehouseOrderOutRowSelect myCurrentNomenkl;
        private WarehouseOrderOutRowSelect myCurrentSelectNomenkl;
        private StandartDialogSelectUC myDataUserControl;
        private KursDomain.Documents.NomenklManagement.Warehouse myStore;

        public AddNomenklFromRashOrderViewModel(KursDomain.Documents.NomenklManagement.Warehouse store)
        {
            myStore = store;
            myDataUserControl = new StandartDialogSelectUC("AddNomenklFromRashOrder");
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        public ObservableCollection<WarehouseOrderOutRowSelect> ItemsCollection { set; get; } =
            new ObservableCollection<WarehouseOrderOutRowSelect>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<WarehouseOrderOutRowSelect> SelectedItems { set; get; } =
            new ObservableCollection<WarehouseOrderOutRowSelect>();


        public StandartDialogSelectUC DataUserControl
        {
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertiesChanged();
            }
            get => myDataUserControl;
        }

        public WarehouseOrderOutRowSelect CurrentItem
        {
            set
            {
                if (myCurrentNomenkl != null && myCurrentNomenkl.Equals(value)) return;
                myCurrentNomenkl = value;
                RaisePropertiesChanged();
            }
            get => myCurrentNomenkl;
        }

        public KursDomain.Documents.NomenklManagement.Warehouse Store
        {
            set
            {
                if (myStore == value) return;
                myStore = value;
                RaisePropertiesChanged();
            }
            get => myStore;
        }

        public WarehouseOrderOutRowSelect CurrentSelectNomenkl
        {
            set
            {
                if (myCurrentSelectNomenkl == value) return;
                myCurrentSelectNomenkl = value;
                RaisePropertiesChanged();
            }
            get => myCurrentSelectNomenkl;
        }


        #region command

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            ItemsCollection.Clear();
            SelectedItems.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var sql = $@"SELECT 
                                  t.DOC_CODE DocCode 
                                 ,t.CODE Code 
                                FROM TD_24 t 
                                INNER JOIN SD_24 s 
                                  ON t.DOC_CODE = s.DOC_CODE 
                                    AND s.DD_TYPE_DC = 2010000003 
                                    AND s.DD_SKLAD_POL_DC = '{CustomFormat.DecimalToSqlDecimal(Store.DOC_CODE)}' 
                                LEFT OUTER JOIN td_24 t1 ON t1.DDT_RASH_ORD_CODE = t.DOC_CODE AND t1.CODE = t.DDT_RASH_ORD_CODE 
                                LEFT OUTER JOIN sd_24 s1 
                                    ON t1.DOC_CODE = s1.DOC_CODE 
                                    AND s1.DD_TYPE_DC = 2010000001 
                                    AND s1.DD_SKLAD_POL_DC = '{CustomFormat.DecimalToSqlDecimal(Store.DOC_CODE)}' 
                                  WHERE t1.DOC_CODE IS null";
                    var rowslist = ctx.Database.SqlQuery<TempList>(sql).ToList();
                    foreach (var r in rowslist)
                    {
                        var d = ctx.TD_24
                            .Include(_ => _.SD_24)
                            .Include(_ => _.SD_175)
                            .Include(_ => _.SD_1751)
                            .Include(_ => _.SD_301)
                            .Include(_ => _.SD_3011)
                            .Include(_ => _.SD_3012)
                            .Include(_ => _.SD_303)
                            .Include(_ => _.SD_384)
                            .Include(_ => _.SD_43)
                            .Include(_ => _.SD_83)
                            .Include(_ => _.SD_831)
                            .Include(_ => _.SD_832)
                            .Include(_ => _.SD_84)
                            .Include(_ => _.TD_73)
                            .Include(_ => _.TD_9)
                            .AsNoTracking()
                            .FirstOrDefault(_ => _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                        if (d != null)
                            ItemsCollection.Add(new WarehouseOrderOutRowSelect(d));
                    }
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        #endregion

        public class TempList
        {
            public decimal DocCode { set; get; }
            public int Code { set; get; }
        }
    }
}
