﻿using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.Documents.Invoices;
using KursDomain.References;

namespace KursDomain.Documents.NomenklManagement;

/// <summary>
///     строки товарных накладных
/// </summary>
[MetadataType(typeof(WaybillRow_FluentAPI))]
public class WaybillRow : TD_24ViewModel
{
    private InvoiceClientRowViewModel _mySchetLinkedRow;

    public WaybillRow()
    {
        Entity = new TD_24 { DOC_CODE = -1, CODE = 0 };
       
    }

    public WaybillRow(TD_24 entity) : base(entity)
    {
        if (entity == null)
        {
            Entity = new TD_24 { DOC_CODE = -1, CODE = 0 };
            return;
        }

        var n = GlobalOptions.ReferencesCache.GetNomenkl(Entity.DDT_NOMENKL_DC);
        Currency = n?.Currency as References.Currency;
        SchetLinkedRowViewModel = new InvoiceClientRowViewModel(entity.TD_84,true,false);
        if(n != null)
            Unit = n.Unit as Unit;
    }

    public string IndoiceClientName => InvoiceClientViewModel?.Name;

    public InvoiceClientRowViewModel SchetLinkedRowViewModel
    {
        get => _mySchetLinkedRow;
        set
        {
            if (_mySchetLinkedRow == value) return;
            _mySchetLinkedRow = value;
            RaisePropertyChanged();
        }
    }

    //public decimal? Price => SchetLinkedRowViewModel?.Price;
    //public decimal? Summa => SchetLinkedRowViewModel?.Summa;
    public string NomenklNumber => Nomenkl?.NomenklNumber;
    public bool IsUsluga => Nomenkl?.IsUsluga ?? false;

    public override object ToJson()
    {
        return new
        {
            DocCode,
            Code,
            Номенлатурный_номер = NomenklNumber,
            Номенлатура = Nomenkl.Name,
            Единица_измерение = Unit.Name,
            Количество = DDT_KOL_RASHOD.ToString("n2")
        };
    }
}

public class WaybillRow_FluentAPI : DataAnnotationForFluentApiBase, IMetadataProvider<WaybillRow>
{
    void IMetadataProvider<WaybillRow>.BuildMetadata(
        MetadataBuilder<WaybillRow> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.SchetLinkedRowViewModel).NotAutoGenerated();
        builder.Property(_ => _.InvoiceProvider).NotAutoGenerated();
        builder.Property(_ => _.InvoiceClientViewModel).NotAutoGenerated();
        builder.Property(_ => _.InvoiceProviderRow).NotAutoGenerated();
        builder.Property(_ => _.SDRSchet).NotAutoGenerated();
        builder.Property(_ => _.Diler).NotAutoGenerated();
        builder.Property(x => x.IsUsluga).NotAutoGenerated();
        builder.Property(_ => _.Currency).NotAutoGenerated();
        builder.Property(x => x.Nomenkl).AutoGenerated()
            .DisplayName("Наименование").ReadOnly().LocatedAt(1);
        builder.Property(x => x.NomenklNumber).AutoGenerated()
            .DisplayName("Ном.№").ReadOnly().LocatedAt(0);
        builder.Property(x => x.DDT_KOL_RASHOD).AutoGenerated()
            .DisplayName("Кол-во").DisplayFormatString("n2").LocatedAt(2);
        builder.Property(x => x.Unit).AutoGenerated()
            .DisplayName("Ед.изм.").ReadOnly().LocatedAt(3);
        builder.Property(_ => _.IndoiceClientName).AutoGenerated().DisplayName("Счет-фактура").ReadOnly().LocatedAt(4);
    }
}
