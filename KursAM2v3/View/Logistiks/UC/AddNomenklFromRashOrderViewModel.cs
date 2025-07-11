﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.View.DialogUserControl;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;

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



        [Display(AutoGenerateField = true, Name = "Склад")]
        public string WarehouseName => Entity?.SD_24?.DD_SKLAD_OTPR_DC != null
            ? ((IName)GlobalOptions.ReferencesCache.GetWarehouse(Entity.SD_24.DD_SKLAD_OTPR_DC)).Name
            : null;

        [Display(AutoGenerateField = true, Name = "Дата")]
        public DateTime? DocDate => Entity?.SD_24?.DD_DATE;

        [Display(AutoGenerateField = true, Name = "Дата")]
        public int? DocNum => Entity?.SD_24?.DD_IN_NUM;
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class AddNomenklFromRashOrderViewModel : RSWindowViewModelBase
    {
        private readonly List<Tuple<decimal, int>> myExclude;
        private readonly KursDomain.References.Warehouse myFromStore;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private WarehouseOrderOutRowSelect myCurrentNomenkl;
        private WarehouseOrderOutRowSelect myCurrentSelectNomenkl;
        private StandartDialogSelectUC myDataUserControl;
        private WarehouseOrderOutRowSelect mySelectedItem;
        private KursDomain.References.Warehouse myStore;
        private readonly DateTime myDate;


        public AddNomenklFromRashOrderViewModel(KursDomain.References.Warehouse store, DateTime date,
            List<Tuple<decimal, int>> exclude = null,
            KursDomain.References.Warehouse fromStore = null)
        {
            myStore = store;
            myDate = date;
            myFromStore = fromStore;
            myExclude = exclude;
            myDataUserControl = new StandartDialogSelectUC("AddNomenklFromRashOrder");
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        public ObservableCollection<WarehouseOrderOutRowSelect> ItemsCollection { set; get; } =
            new ObservableCollection<WarehouseOrderOutRowSelect>();

        public ObservableCollection<WarehouseOrderOutRowSelect> ItemsCollectionFull { set; get; } =
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
                if (Equals(myCurrentNomenkl, value)) return;
                myCurrentNomenkl = value;
                RaisePropertiesChanged();
            }
            get => myCurrentNomenkl;
        }

        public WarehouseOrderOutRowSelect SelectedItem
        {
            set
            {
                if (Equals(mySelectedItem, value)) return;
                mySelectedItem = value;
                RaisePropertiesChanged();
            }
            get => mySelectedItem;
        }

        public KursDomain.References.Warehouse Store
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

        public ICommand SelectedValueChangedCommand
        {
            get { return new Command(SelectedValueChanged, _ => true); }
        }


        #region command

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            ItemsCollection.Clear();
            ItemsCollectionFull.Clear();
            SelectedItems.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var sql = $@"SELECT
                                    t.DOC_CODE DocCode,
                                    t.CODE     Code
                                FROM TD_24 t
                                    INNER JOIN SD_24 s ON t.DOC_CODE = s.DOC_CODE AND s.DD_TYPE_DC = 2010000003
                                        AND s.DD_SKLAD_POL_DC = '{CustomFormat.DecimalToSqlDecimal(Store.DocCode)}'
                                WHERE NOT EXISTS(SELECT 1 FROM td_24 t24 WHERE t24.DDT_RASH_ORD_DC = t.DOC_CODE AND t24.DDT_RASH_ORD_CODE = t.CODE)";
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
                        if (d == null) continue;
                        if (d.SD_24.DD_DATE <= myDate)
                        {
                            if (myFromStore == null)
                            {
                                ItemsCollection.Add(new WarehouseOrderOutRowSelect(d));
                            }
                            else
                            {
                                if (myFromStore.DocCode == d.SD_24.DD_SKLAD_OTPR_DC)
                                    ItemsCollection.Add(new WarehouseOrderOutRowSelect(d));
                            }
                        }
                    }

                    if (myExclude is { Count: > 0 })
                        foreach (var item in myExclude)
                        {
                            var old = ItemsCollection.FirstOrDefault(_ =>
                                _.DOC_CODE == item.Item1 && _.Code == item.Item2);
                            if (old != null)
                                ItemsCollection.Remove(old);
                        }

                    foreach (var item in ItemsCollection) ItemsCollectionFull.Add(item);
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        #endregion

        private void SelectedValueChanged(object obj)
        {
            if (obj is not CellValueChangedEventArgs arg) return;
            if ((bool)arg.Value)
            {
                var delItems =
                    new List<WarehouseOrderOutRowSelect>(ItemsCollectionFull.Where(_ =>
                        !_.WarehouseName.Equals(CurrentItem.WarehouseName)));
                foreach (var item in delItems) ItemsCollection.Remove(item);
            }
            else
            {
                if (ItemsCollection.Any(_ => _.IsSelected)) return;
                ItemsCollection.Clear();
                foreach (var item in ItemsCollectionFull) ItemsCollection.Add(item);
            }
        }

        public class TempList
        {
            public decimal DocCode { set; get; }
            public int Code { set; get; }
        }
    }
}
