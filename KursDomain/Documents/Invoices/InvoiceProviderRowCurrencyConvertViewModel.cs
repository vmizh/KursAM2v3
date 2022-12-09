using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.IDocuments.DistributeOverhead;
using KursDomain.IDocuments.Finance;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;
using KursDomain.References;

namespace KursDomain.Documents.Invoices;

public enum DirectCalc
{
    Rate = 0,
    Price = 1,
    PriceWithNaklad = 2,
    Summa = 3,
    SummaWithNaklad = 4,
    Quantity = 5
}

// ReSharper disable once InconsistentNaming
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[MetadataType(typeof(InvPrvdRowCrsConvertLayoutData_FluentAPI))]
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public sealed class InvoiceProviderRowCurrencyConvertViewModel : RSViewModelBase, IEntity<TD_26_CurrencyConvert>,
    ISFProviderNomenklCurrencyConvert

{
    private TD_26_CurrencyConvert myEntity;

    public InvoiceProviderRowCurrencyConvertViewModel()
    {
        Entity = DefaultValue();
    }

    public InvoiceProviderRowCurrencyConvertViewModel(TD_26_CurrencyConvert entity)
    {
        Entity = entity ?? new TD_26_CurrencyConvert { Id = Guid.NewGuid() };
    }

    public override Guid Id
    {
        get => Entity.Id;
        set
        {
            if (Entity.Id == value) return;
            Entity.Id = value;
            RaisePropertyChanged();
        }
    }

    public INomenkl NomenklFrom { get; set; }
    public INomenkl NomenklTo { get => GlobalOptions.ReferencesCache.GetNomenkl(NomenklId) as Nomenkl;
        set
        {
            if (Entity.NomenklId == ((IDocGuid)value).Id) return;
            Entity.NomenklId = ((IDocGuid)value).Id;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(NomenkNumber));
        } }

    public override decimal DocCode
    {
        get => Entity.DOC_CODE ?? 0;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public override int Code
    {
        get => Entity.CODE ?? 0;
        set
        {
            if (Entity.CODE == value) return;
            Entity.CODE = value;
            RaisePropertyChanged();
        }
    }

    public Guid NomenklId
    {
        get => Entity.NomenklId;
        set
        {
            if (Entity.NomenklId == value) return;
            Entity.NomenklId = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Nomenkl));
            RaisePropertyChanged(nameof(NomenkNumber));
            RaisePropertyChanged(nameof(Currency));
        }
    }

    public Nomenkl Nomenkl
    {
        get => GlobalOptions.ReferencesCache.GetNomenkl(NomenklId) as Nomenkl;
        set
        {
            if (Entity.NomenklId == value.Id) return;
            Entity.NomenklId = value.Id;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(NomenkNumber));
        }
    }

    public References.Warehouse Store =>
        GlobalOptions.ReferencesCache.GetWarehouse(Entity.StoreDC) as References.Warehouse;

    public decimal StoreDC
    {
        get => Entity.StoreDC;
        set
        {
            if (Entity.StoreDC == value) return;
            Entity.StoreDC = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Store));
        }
    }

    public string NomenkNumber => Nomenkl?.NomenklNumber;

    public References.Currency Currency => (References.Currency)Nomenkl?.Currency;

    public DateTime Date
    {
        get => Entity.Date;
        set
        {
            if (Entity.Date == value) return;
            Entity.Date = value;
            RaisePropertyChanged();
        }
    }

    public decimal Quantity
    {
        get => Entity.Quantity;
        set
        {
            if (Entity.Quantity == value) return;
            Entity.Quantity = value;
            RaisePropertyChanged();
        }
    }

    public decimal Rate
    {
        get => Entity.Rate;
        set
        {
            if (Entity.Rate == value) return;
            Entity.Rate = value;

            RaisePropertyChanged();
        }
    }

    public decimal FromPrice { get; set; }
    public decimal FromPriceWithNaklad { get; set; }

    public decimal OLdPrice { set; get; }
    public decimal OLdNakladPrice { set; get; }

    public decimal Price
    {
        get => Entity.Price;
        set
        {
            if (Entity.Price == value) return;
            Entity.Price = value;
            RaisePropertyChanged();
        }
    }

    public decimal PriceWithNaklad
    {
        get => Entity.PriceWithNaklad;
        set
        {
            if (Entity.PriceWithNaklad == value) return;
            Entity.PriceWithNaklad = value;
            RaisePropertyChanged();
        }
    }

    public decimal FromSumma { get; }
    public decimal FromSummaWithNaklad { get; }

    public decimal Summa
    {
        get => Entity.Summa;
        set
        {
            if (Entity.Summa == value) return;
            Entity.Summa = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaWithNaklad
    {
        get => Entity.SummaWithNaklad;
        set
        {
            if (Entity.SummaWithNaklad == value) return;
            Entity.SummaWithNaklad = value;
            RaisePropertyChanged();
        }
    }

    public decimal SFDocCode { get; set; }
    public int SFCode { get; set; }
    public IWarehouse Warehouse { get; set; }
    public IEnumerable<IDistributeNakladRow> NakladRows { get; set; }
    public IInvoiceProviderRow InvoiceProviderRow { get; set; }

    public override string Note
    {
        get => Entity.Note;
        set
        {
            if (Entity.Note == value) return;
            Entity.Note = value;
            RaisePropertyChanged();
        }
    }

    public bool IsAccessRight { get; set; }

    public TD_26_CurrencyConvert DefaultValue()
    {
        return new TD_26_CurrencyConvert { Id = Guid.NewGuid() };
    }

    public TD_26_CurrencyConvert Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public List<TD_26_CurrencyConvert> LoadList()
    {
        throw new NotImplementedException();
    }

    public void CalcRow()
    {
        Price = OLdPrice * Rate;
        PriceWithNaklad = OLdNakladPrice * Rate;
        Summa = Price * Quantity;
        SummaWithNaklad = PriceWithNaklad * Quantity;
    }

    public void CalcRow(DirectCalc direct)
    {
        switch (direct)
        {
            case DirectCalc.Price:
                if (Currency.Id == GlobalOptions.SystemProfile.NationalCurrency.Id)
                    Rate = Math.Round(Price / OLdPrice, 2);
                else
                    Rate = Math.Round(OLdPrice / Price, 2);
                PriceWithNaklad = Math.Round(OLdNakladPrice * Rate, 2);
                break;
            case DirectCalc.PriceWithNaklad:
                if (Currency.Id == GlobalOptions.SystemProfile.NationalCurrency.Id)
                    Rate = Math.Round(PriceWithNaklad / OLdNakladPrice, 2);
                else
                    Rate = Math.Round(OLdNakladPrice / PriceWithNaklad, 2);
                Price = Math.Round(OLdPrice * Rate, 2);
                break;
            case DirectCalc.Rate:
                if (Currency.Id == GlobalOptions.SystemProfile.NationalCurrency.Id)
                {
                    Price = Math.Round(OLdPrice * Rate, 2);
                    PriceWithNaklad = Math.Round(OLdNakladPrice * Rate, 2);
                }
                else
                {
                    Price = Math.Round(OLdPrice / Rate, 2);
                    PriceWithNaklad = Math.Round(OLdNakladPrice / Rate, 2);
                }

                break;
        }

        Summa = Math.Round(Price * Quantity, 2);
        SummaWithNaklad = Math.Round(PriceWithNaklad * Quantity, 2);
    }

    public TD_26_CurrencyConvert Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public TD_26_CurrencyConvert Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Save(TD_26_CurrencyConvert doc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(TD_26_CurrencyConvert ent)
    {
        Id = ent.Id;
        NomenklId = ent.NomenklId;
        Date = ent.Date;
        Quantity = ent.Quantity;
        Rate = ent.Rate;
        Price = ent.Price;
        PriceWithNaklad = ent.PriceWithNaklad;
        Summa = ent.Summa;
        SummaWithNaklad = ent.SummaWithNaklad;
        Note = ent.Note;
    }

    public void UpdateTo(TD_26_CurrencyConvert ent)
    {
        ent.Id = Id;
        ent.NomenklId = NomenklId;
        ent.Date = Date;
        ent.Quantity = Quantity;
        ent.Rate = Rate;
        ent.Price = Price;
        ent.PriceWithNaklad = PriceWithNaklad;
        ent.Summa = Summa;
        ent.SummaWithNaklad = SummaWithNaklad;
        ent.Note = Note;
    }
}

// ReSharper disable once InconsistentNaming
public class InvPrvdRowCrsConvertLayoutData_FluentAPI : DataAnnotationForFluentApiBase,
    IMetadataProvider<InvoiceProviderRowCurrencyConvertViewModel>
{
    void IMetadataProvider<InvoiceProviderRowCurrencyConvertViewModel>.BuildMetadata(
        MetadataBuilder<InvoiceProviderRowCurrencyConvertViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Entity).NotAutoGenerated();
        builder.Property(_ => _.Name).NotAutoGenerated();
        builder.Property(_ => _.StoreDC).NotAutoGenerated();
        builder.Property(_ => _.Store).AutoGenerated().DisplayName("Склад");
        builder.Property(_ => _.State).NotAutoGenerated();
        builder.Property(_ => _.NomenklId).NotAutoGenerated();
        builder.Property(_ => _.NomenkNumber).AutoGenerated().DisplayName("Ном.№");
        builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Номенклатура").ReadOnly();
        builder.Property(_ => _.Currency).AutoGenerated().DisplayName("Валюта").ReadOnly();
        builder.Property(_ => _.Quantity).AutoGenerated().DisplayName("Кол-во").DisplayFormatString("n4");
        builder.Property(_ => _.Rate).AutoGenerated().DisplayName("Курс").DisplayFormatString("n4");
        builder.Property(_ => _.Date).AutoGenerated().DisplayName("Дата");
        builder.Property(_ => _.Price).AutoGenerated().DisplayName("Цена").DisplayFormatString("n2");
        builder.Property(_ => _.PriceWithNaklad).AutoGenerated().DisplayName("Цена(накл)")
            .DisplayFormatString("n2");
        builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.SummaWithNaklad).AutoGenerated().DisplayName("Сумма(накл").DisplayFormatString("n2")
            .ReadOnly();
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
    }
}
