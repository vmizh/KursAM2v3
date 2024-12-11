using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows.Controls;
using Core.Helper;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using Helper;
using KursDomain.Documents.CommonReferences;
using KursDomain.Event;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.References;
using Newtonsoft.Json;
using Prism.Events;

namespace KursDomain.Wrapper.Nomenkl.WarehouseOut;

[MetadataType(typeof(DataAnnotationsWarehouseOutWrap))]
public class WarehouseOutWrapper : SD_24BaseWrapper
{
    #region Constructors

    public WarehouseOutWrapper(SD_24 model, IReferencesCache cache, ALFAMEDIAEntities ctx,
        IEventAggregator eventAggregator, IMessageDialogService messageDialogService) : base(model, cache, ctx,
        eventAggregator, messageDialogService)
    {
        Rows.CollectionChanged += Rows_CollectionChanged;
    }

    private void Rows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        EventAggregator.GetEvent<AfterUpdateBaseWrapperEvent<WarehouseOutWrapper>>()
            .Publish(new AfterUpdateBaseWrapperEventArgs<WarehouseOutWrapper>()
            {
                DocCode = DocCode,
                DocumentType = DocumentType.StoreOrderOut,
                Id = Id,
                wrapper = this
            });
    }

    #endregion


    #region Error

    protected override IEnumerable<string> ValidateProperty(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(WarehouseIn):
                if (WarehouseIn == null) yield return "Не выбран склад-получатель";
                break;
            case nameof(WarehouseOut):
                if (WarehouseOut == null) yield return "Не выбран склад-отправитель";
                break;
        }
    }

    #endregion

    #region Properties

    public override DateTime DocDate
    {
        get => Model.DD_DATE;
        set
        {
            if (Rows.Any(_ => _.WarehouseOrderIn != null))
            {
                MessageDialogService.ShowInfoDialog("По ордеру есть приходный ордер, изменение даты запрещено!");
                EventAggregator.GetEvent<AfterUpdateBaseWrapperEvent<WarehouseOutWrapper>>()
                    .Publish(new AfterUpdateBaseWrapperEventArgs<WarehouseOutWrapper>()
                    {
                        DocCode = DocCode,
                        DocumentType = DocumentType.StoreOrderOut,
                        Id = Id,
                        wrapper = this,
                        FieldName = "DocDate"
                    });
                return;
            }
            if (Model.DD_DATE == value) return;
            Model.DD_DATE = value;
            RaisePropertyChanged();
        }
    }

    public new ObservableCollection<WarehouseOutRowWrapper> Rows { get; } =
        new ObservableCollection<WarehouseOutRowWrapper>();

    public override object ToJson()
    {
        var res = new
        {
            DocCode,
            Статус = CustomFormat.GetEnumName(State),
            Номер = DocNum,
            Дата = DocDate.ToString(GlobalOptions.SystemProfile.GetShortDateFormat()),
            Кладовщик = StoreKeeper?.Name ?? "Не указан",
            Через_кого = string.IsNullOrWhiteSpace(SenderPersonaName) ? "Не указан" : SenderPersonaName,
            Со_склада = WarehouseOut?.Name ?? "Не указан",
            На_склад = WarehouseIn?.Name ?? "Не указан",
            Создатель = Creator,
            Позиции = Rows.Select(_ => _.ToJson())
        };
        return JsonConvert.SerializeObject(res);
    }

    #endregion

    #region Method

    public override void Initialize()
    {
        var orders = myContext.TD_24.Include(_ => _.SD_24)
            .Where(_ => _.DDT_RASH_ORD_DC == Model.DOC_CODE).AsNoTracking().ToList();
        if (Model.DD_SKLAD_OTPR_DC != null)
            WarehouseOut = GlobalOptions.ReferencesCache.GetWarehouse(Model.DD_SKLAD_OTPR_DC) as Warehouse;
        if (Model.TD_24 is { Count: > 0 })
            foreach (var row in Model.TD_24)
            {
                var newItem =
                    new WarehouseOutRowWrapper(row, myCache, myContext, EventAggregator, MessageDialogService);
                if (orders.Count > 0 &&
                    orders.Exists(_ => row.DOC_CODE == _.DDT_RASH_ORD_DC && row.CODE == _.DDT_RASH_ORD_CODE))
                {
                    var ord = orders.First(_ => row.DOC_CODE == _.DDT_RASH_ORD_DC && row.CODE == _.DDT_RASH_ORD_CODE);
                    newItem.WarehouseOrderIn = new TD_24BaseWrapper(ord, myCache, myContext, EventAggregator,
                        MessageDialogService);
                }
                Rows.Add(newItem);
            }
    }

    public override void RaisePropertyChanged(string propertyName = null)
    {
        base.RaisePropertyChanged(propertyName);
        if (propertyName == "State") return;
        EventAggregator.GetEvent<AfterUpdateBaseWrapperEvent<WarehouseOutWrapper>>()
            .Publish(new AfterUpdateBaseWrapperEventArgs<WarehouseOutWrapper>()
            {
                DocCode = DocCode,
                DocumentType = DocumentType.StoreOrderOut,
                Id = Id,
                wrapper = this,
                FieldName = propertyName
            });
    }

    public bool Equals(WarehouseOutWrapper other)
    {
        if (other == null) return false;
        return Id == other.Id;
    }

    public override string ToString()
    {
        var num = string.IsNullOrWhiteSpace(DocExtNum) ? "" : $"/{DocExtNum}";
        return
            $@"Расходный складской ордер №{DocNum}{num} от {DocDate.ToString(GlobalOptions.SystemProfile.GetShortDateFormat())} " +
            $"со склада {WarehouseOut} на склад {WarehouseIn}";
    }

    #endregion

    #region Document operation

    #endregion

    #region Commands

    #endregion
}

public class DataAnnotationsWarehouseOutWrap : DataAnnotationForFluentApiBase,
    IMetadataProvider<WarehouseOutWrapper>
{
    void IMetadataProvider<WarehouseOutWrapper>.BuildMetadata(
        MetadataBuilder<WarehouseOutWrapper> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.KontargentSender).NotAutoGenerated();
        builder.Property(_ => _.KontargentRecepient).NotAutoGenerated();
        builder.Property(_ => _.KontragentCurrency).NotAutoGenerated();
        builder.Property(_ => _.PersonaRecepient).NotAutoGenerated();
        builder.Property(_ => _.PersonaSendNomenkl).NotAutoGenerated();
        builder.Property(_ => _.WarehouseFrom).NotAutoGenerated();
        builder.Property(_ => _.WarehouseTo).NotAutoGenerated();
        builder.Property(_ => _.DocNum).AutoGenerated();
        builder.Property(_ => _.DocDate).AutoGenerated();
        builder.Property(_ => _.StoreKeeper).AutoGenerated();
        builder.Property(_ => _.Creator).AutoGenerated();
        builder.Property(_ => _.SenderPersonaName).AutoGenerated();
        builder.Property(_ => _.WarehouseOut).AutoGenerated().DisplayName("Склад-отправитель").LocatedAt(2);
        builder.Property(_ => _.WarehouseIn).AutoGenerated().DisplayName("Склад-получатель").LocatedAt(3);
        builder.Property(_ => _.State).AutoGenerated().DisplayName("Статус").ReadOnly();
        //builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");

        #region Form Layout

            // @formatter:off
            builder.DataFormLayout()
                .Group("1", Orientation.Horizontal)
                    .ContainsProperty(_ => _.DocNum)
                    .ContainsProperty(_ => _.DocDate)
                    .ContainsProperty(_ => _.Creator)
                    .ContainsProperty(_ => _.State)
                .EndGroup()
                .Group("g2")
                    .ContainsProperty(_ => _.WarehouseOut)
                    .ContainsProperty(_ => _.WarehouseIn)
                .EndGroup()
                .ContainsProperty(_ => _.StoreKeeper)
                .ContainsProperty(_ => _.SenderPersonaName);
                //.ContainsProperty(_ => _.Note);
            // @formatter:on

            #endregion


    }
}
