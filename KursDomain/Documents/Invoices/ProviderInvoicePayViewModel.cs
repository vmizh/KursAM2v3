using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace KursDomain.Documents.Invoices;

[MetadataType(typeof(ProviderInvoicePayData_FluentAPI))]
// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public sealed class ProviderInvoicePayViewModel : RSViewModelBase, IEntity<ProviderInvoicePay>
{
    private DateTime myDocDate;
    private string myDocExtName;
    private string myDocName;
    private string myDocNum;
    private decimal myDocSumma;
    private ProviderInvoicePay myEntity;

    public ProviderInvoicePayViewModel()
    {
        Entity = DefaultValue();
    }

    public ProviderInvoicePayViewModel(ProviderInvoicePay entity)
    {
        Entity = entity ?? DefaultValue();
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

    public decimal DocSumma
    {
        get => myDocSumma;
        set
        {
            if (myDocSumma == value) return;
            myDocSumma = value;
            RaisePropertyChanged();
        }
    }

    public string DocExtName
    {
        get => myDocExtName;
        set
        {
            if (myDocExtName == value) return;
            myDocExtName = value;
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

    public string DocName
    {
        get => myDocName;
        set
        {
            if (myDocName == value) return;
            myDocName = value;
            RaisePropertyChanged();
        }
    }

    public DateTime DocDate
    {
        get => myDocDate;
        set
        {
            if (myDocDate == value) return;
            myDocDate = value;
            RaisePropertyChanged();
        }
    }

    public string DocNum
    {
        get => myDocNum;
        set
        {
            if (myDocNum == value) return;
            myDocNum = value;
            RaisePropertyChanged();
        }
    }

    public int? BankCode
    {
        get => Entity.BankCode;
        set
        {
            if (Entity.BankCode == value) return;
            Entity.BankCode = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CashDC
    {
        get => Entity.CashDC;
        set
        {
            if (Entity.CashDC == value) return;
            Entity.CashDC = value;
            RaisePropertyChanged();
        }
    }

    // ReSharper disable once InconsistentNaming
    public decimal? VZDC
    {
        get => Entity.VZDC;
        set
        {
            if (Entity.VZDC == value) return;
            Entity.VZDC = value;
            RaisePropertyChanged();
        }
    }

    public int? VZCode
    {
        get => Entity.VZCode;
        set
        {
            if (Entity.VZCode == value) return;
            Entity.VZCode = value;
            RaisePropertyChanged();
        }
    }

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

    public TD_110 TD_110
    {
        get => Entity.TD_110;
        set
        {
            if (Entity.TD_110 == value) return;
            Entity.TD_110 = value;
            RaisePropertyChanged();
        }
    }

    public TD_101 TD_101
    {
        get => Entity.TD_101;
        set
        {
            if (Entity.TD_101 == value) return;
            Entity.TD_101 = value;
            RaisePropertyChanged();
        }
    }

    public SD_34 SD_34
    {
        get => Entity.SD_34;
        set
        {
            if (Entity.SD_34 == value) return;
            Entity.SD_34 = value;
            RaisePropertyChanged();
        }
    }

    public bool IsAccessRight { get; set; }

    public ProviderInvoicePay DefaultValue()
    {
        return new ProviderInvoicePay { Id = Guid.NewGuid() };
    }

    public ProviderInvoicePay Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public List<ProviderInvoicePay> LoadList()
    {
        throw new NotImplementedException();
    }

    public ProviderInvoicePay Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public ProviderInvoicePay Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Save(ProviderInvoicePay doc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(ProviderInvoicePay ent)
    {
        Id = ent.Id;
        Summa = ent.Summa;
        BankCode = ent.BankCode;
        CashDC = ent.CashDC;
        VZDC = ent.VZDC;
        VZCode = ent.VZCode;
        Note = ent.Note;
        TD_110 = ent.TD_110;
        TD_101 = ent.TD_101;
        SD_34 = ent.SD_34;
    }

    public void UpdateTo(ProviderInvoicePay ent)
    {
        ent.Id = Id;
        ent.Summa = Summa;
        ent.BankCode = BankCode;
        ent.CashDC = CashDC;
        ent.VZDC = VZDC;
        ent.VZCode = VZCode;
        ent.Note = Note;
        ent.TD_110 = TD_110;
        ent.TD_101 = TD_101;
        ent.SD_34 = SD_34;
    }
}

// ReSharper disable once InconsistentNaming
public class ProviderInvoicePayData_FluentAPI : DataAnnotationForFluentApiBase,
    IMetadataProvider<ProviderInvoicePayViewModel>
{
    void IMetadataProvider<ProviderInvoicePayViewModel>.BuildMetadata(
        MetadataBuilder<ProviderInvoicePayViewModel> metadataBuilder)
    {
        SetNotAutoGenerated(metadataBuilder);
        metadataBuilder.Property(_ => _.Entity).NotAutoGenerated();
        metadataBuilder.Property(_ => _.DocName).AutoGenerated().DisplayName("Документ");
        metadataBuilder.Property(_ => _.DocExtName).AutoGenerated().DisplayName("Расшифровка");
        metadataBuilder.Property(_ => _.DocNum).AutoGenerated().DisplayName("№");
        metadataBuilder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата");
        metadataBuilder.Property(_ => _.Rate).AutoGenerated().DisplayName("Курс(справочно)");
        metadataBuilder.Property(_ => _.DocSumma).AutoGenerated().DisplayName("Сумма док-та")
            .DisplayFormatString("n2");
        metadataBuilder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма")
            .DisplayFormatString("n2");
        // .MatchesInstanceRule(_ => _.Summa > 0 && _.Summa <= _.DocSumma,
        //     ()=>"Сумма должна быть больше нуля и не больше суммы документа");
        metadataBuilder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
    }
}
