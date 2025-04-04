﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Data.Entity;
using System.Windows.Controls;
using Core.Helper;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.IDocuments;
using KursDomain.Managers;

namespace KursDomain.Documents.NomenklManagement;

/// <summary>
///     Расходный складской ордер
/// </summary>
[MetadataType(typeof(DataAnnotationsWarehouseStorageOrderOut))]
public sealed class WarehouseOrderOut : SD_24ViewModel, ILastChanged
{
    //public string Receiver => KontragentReceiver != null ? KontragentReceiver.Name : WarehouseIn?.Name;
    private WarehouseSenderType myWarehouseSenderType;
    private NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());
    private ObservableCollection<WarehouseOrderOutRow> myRows = new ObservableCollection<WarehouseOrderOutRow>();
    private ObservableCollection<WarehouseOrderOutRow> myDeletedRows = new ObservableCollection<WarehouseOrderOutRow>();
    private ObservableCollection<WarehouseOrderOutRow> mySelectedRows = new ObservableCollection<WarehouseOrderOutRow>();
    private string myLastChanger;
    private DateTime myLastChangerDate;

    public WarehouseOrderOut()
    {
        Entity.DD_DATE = DateTime.Today;
        Entity.DD_TYPE_DC = 2010000003;
        Id = Guid.NewGuid();
        State = RowStatus.NewRow;
        CREATOR = GlobalOptions.UserInfo.NickName;
    }

    public override void Initialize()
    {
        if (Entity.DD_SKLAD_OTPR_DC != null)
            WarehouseOut = GlobalOptions.ReferencesCache.GetWarehouse(Entity.DD_SKLAD_OTPR_DC) as References.Warehouse;
        if (Entity.DD_SKLAD_POL_DC != null)
            WarehouseIn = GlobalOptions.ReferencesCache.GetWarehouse(Entity.DD_SKLAD_POL_DC) as References.Warehouse;
    }

    public WarehouseOrderOut(SD_24 entity) : base(entity)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            Entity = entity ?? DefaultValue();
            var data = Entity.TD_24.ToList();
            var dd = new List<TD_24>();
            var dist = new List<decimal>();
            
            if (data.Count != 0)
            {
                foreach (var row in data.Where(_ => _.DDT_RASH_ORD_DC is not null))
                {
                    if (dist.Contains(row.DDT_RASH_ORD_DC.Value)) continue;
                    dist.Add(row.DDT_RASH_ORD_DC.Value);
                    var s = ctx.TD_24.Include(_ => _.SD_24).Where(_ => _.DOC_CODE == row.DDT_RASH_ORD_DC);
                    var ss = s.FirstOrDefault(_ => _.CODE == row.DDT_RASH_ORD_CODE);
                    if(ss != null) { dd.Add(ss); }
                }

                foreach (var item in data)
                {
                    var r = new WarehouseOrderOutRow(item) { Parent = this };
                    var ordIn = dd.FirstOrDefault(_ =>
                        _.DDT_RASH_ORD_DC == item.DOC_CODE && _.DDT_RASH_ORD_CODE == item.CODE);
                    if (ordIn != null)
                    {
                        var h = new WarehouseOrderIn
                        {
                            Entity = ordIn.SD_24
                        };
                        r.OrderInRow = new WarehouseOrderInRow(ordIn)
                        {
                            Parent = h
                        };
                    }

                    Rows.Add(r);
                }
            }
        }

        myState = RowStatus.NotEdited;
        foreach (var r in Rows) r.myState = RowStatus.NotEdited;
        Rows.CollectionChanged += (o, args) => State = RowStatus.Edited;
    }


    public override DateTime Date
    {
        get => Entity.DD_DATE;
        set
        {
            if (Entity.DD_DATE == value) return;
            Entity.DD_DATE = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Флаг для внутреннего перемещения. full - все отгружено>  not - нет отгрузки, part - частичная отгрузка
    /// </summary>
    public string FlagShipped { set; get; } = string.Empty;

    public void UpdateMaxQuantity(DateTime newDate)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            DateTime dateOld = ctx.SD_24.FirstOrDefault(_ => _.DOC_CODE == DocCode)?.DD_DATE ?? Date;
            foreach (var r in Rows)
            {
                decimal oldQuan = 0;
                var nq = nomenklManager.GetNomenklQuantity(WarehouseOut.DocCode, r.Nomenkl.DocCode,
                    newDate, newDate > DateTime.Today ? newDate : DateTime.Today);

                if (State != RowStatus.NewRow && Entity.DD_DATE >= dateOld)
                {


                    var d = ctx.TD_24.FirstOrDefault(_ => _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                    oldQuan = d?.DDT_KOL_RASHOD ?? 0;
                }


                r.MaxQuantity = nq.Count > 0 ? nq.Min(_ => _.OstatokQuantity) + oldQuan : 0;
                if (r.Quantity > r.MaxQuantity)
                    r.Quantity = r.MaxQuantity;
            }
        }
    }

    public ObservableCollection<WarehouseOrderOutRow> Rows
    {
        set
        {
            if (Equals(value, myRows)) return;
            myRows = value;
            RaisePropertyChanged();
        }
        get => myRows;
    }

    public ObservableCollection<WarehouseOrderOutRow> DeletedRows
    {
        get => myDeletedRows;
        set
        {
            if (Equals(value, myDeletedRows)) return;
            myDeletedRows = value;
            RaisePropertyChanged();
        }
    }

    // ReSharper disable once CollectionNeverUpdated.Global
    public ObservableCollection<WarehouseOrderOutRow> SelectedRows
    {
        get => mySelectedRows;
        set
        {
            if (Equals(value, mySelectedRows)) return;
            mySelectedRows = value;
            RaisePropertyChanged();
        }
    }


    public WarehouseSenderType WarehouseSenderType
    {
        get => myWarehouseSenderType;
        set
        {
            if (myWarehouseSenderType == value) return;
            myWarehouseSenderType = value;
            RaisePropertyChanged();
        }
    }

    public override string ToString()
    {  
        var strN = "";
        if (DD_EXT_NUM != null) strN =  "/ + DD_EXT_NUM";
        return
            $"Расходный складской ордер №{DD_IN_NUM}{strN} от {Date.ToShortDateString()} склад: {WarehouseOut}";
    }

    public string LastChanger
    {
        get => myLastChanger;
        set
        {
            if (value == myLastChanger) return;
            myLastChanger = value;
            RaisePropertyChanged();
        }
    }

    public DateTime LastChangerDate
    {
        get => myLastChangerDate;
        set
        {
            if (value.Equals(myLastChangerDate)) return;
            myLastChangerDate = value;
            RaisePropertyChanged();
        }
    }

    private class LinkDoc
    {
        public decimal DC { set; get; }
        public int Code { set; get; }
    }
}

public class DataAnnotationsWarehouseStorageOrderOut : DataAnnotationForFluentApiBase,
    IMetadataProvider<WarehouseOrderOut>
{
    void IMetadataProvider<WarehouseOrderOut>.BuildMetadata(
        MetadataBuilder<WarehouseOrderOut> builder)
    {
        SetNotAutoGenerated(builder);
        //builder.Property(_ => _.OutOnWarehouse).NotAutoGenerated();
        //builder.Property(_ => _.InFromWarehouse).NotAutoGenerated();
        builder.Property(_ => _.KontragentSender).NotAutoGenerated();
        builder.Property(_ => _.WarehouseSenderType).NotAutoGenerated();
        builder.Property(_ => _.DocumentType).NotAutoGenerated();
        builder.Property(_ => _.KontragentViewModelReceiver).NotAutoGenerated();
        builder.Property(_ => _.Period).NotAutoGenerated();
        builder.Property(_ => _.InvoiceProvider).NotAutoGenerated();
        builder.Property(_ => _.InvoiceClientViewModel).NotAutoGenerated();
        builder.Property(_ => _.DD_IN_NUM).AutoGenerated().DisplayName("Номер").LocatedAt(1);
        builder.Property(_ => _.Date).AutoGenerated().DisplayName("Дата").LocatedAt(0);
        builder.Property(_ => _.Kladovshik).AutoGenerated().DisplayName("Кладовщик").LocatedAt(4);
        builder.Property(_ => _.CREATOR).AutoGenerated().DisplayName("Создатель").LocatedAt(6);
        builder.Property(_ => _.WarehouseOut).AutoGenerated().DisplayName("Склад-отправитель").LocatedAt(2);
        builder.Property(_ => _.WarehouseIn).AutoGenerated().DisplayName("Склад-получатель").LocatedAt(3);
        builder.Property(_ => _.DD_OT_KOGO_POLUCHENO).AutoGenerated().DisplayName("Через кого").LocatedAt(5);
        builder.Property(_ => _.State).AutoGenerated().DisplayName("Статус").ReadOnly();
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");

        builder.Property(_ => _.LastChanger).AutoGenerated().DisplayName("Посл. изменил").LocatedAt(8).ReadOnly();
        builder.Property(_ => _.LastChangerDate).AutoGenerated().DisplayName("Посл. измение").LocatedAt(9).ReadOnly();

        builder.Property(_ => _.FlagShipped).AutoGenerated().DisplayName("флаг").LocatedAt(10).ReadOnly();
        #region Form Layout

            // @formatter:off
            builder.DataFormLayout()
                .Group("1", Orientation.Horizontal)
                    .ContainsProperty(_ => _.DD_IN_NUM)
                    .ContainsProperty(_ => _.Date)
                    .ContainsProperty(_ => _.CREATOR)
                    .ContainsProperty(_ => _.State)
                .EndGroup()
                .Group("g2")
                    .ContainsProperty(_ => _.WarehouseOut)
                    .ContainsProperty(_ => _.WarehouseIn)
                .EndGroup()
                .ContainsProperty(_ => _.DD_OT_KOGO_POLUCHENO)
                .ContainsProperty(_ => _.Note);
        // @formatter:on

        #endregion
    }
}
