using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core.Native;
using KursDomain.ICommon;

namespace KursDomain.Documents.Invoices;

[MetadataType(typeof(ProviderInvoicePayData_FluentAPI))]
// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public sealed class ProviderInvoicePayViewModel : RSViewModelBase, IEntity<ProviderInvoicePay>
{
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

    public decimal DocSumma =>
        (TD_101?.VVT_VAL_RASHOD ?? 0) + (TD_110?.VZT_CRS_SUMMA ?? 0) + (SD_34?.SUMM_ORD ?? 0);

    public string DocExtName
    {
        get
        { 
            if (TD_101 != null) return $"{TD_101.SD_101.SD_114.BA_BANK_NAME} " +
                                              $"р/с {TD_101.SD_101.SD_114.BA_RASH_ACC}";;
            if (TD_110 != null) return $"{TD_110.VZT_DOC_NOTES}";
            if (SD_34 != null) return $"Касса {((IName)GlobalOptions.ReferencesCache.GetCashBox(SD_34.CA_DC)).Name}";;
            return null;
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
        get
        { 
            if (TD_101 != null) return "Банковская транзакция";
            if (TD_110 != null) return "Акт взаимозачета";
            if (SD_34 != null) return "Расходный кассовый ордер";
            return null;
        }
    }

    public DateTime DocDate
    {
        get
        { 
            if (TD_101 != null) return TD_101.SD_101.VV_START_DATE;
            if (TD_110 != null) return TD_110.SD_110.VZ_DATE;
            if (SD_34 != null) return SD_34.DATE_ORD ?? DateTime.MinValue;
            return DateTime.MinValue;
        }
       
    }

    public string DocNum
    {
        get
        { 
            if (TD_101 != null) return TD_101.VVT_DOC_NUM;
            if (TD_110 != null) return TD_110.SD_110.VZ_NUM.ToString();
            if (SD_34 != null) return SD_34.NUM_ORD.ToString();
            return "";
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
        get =>  Entity.TD_110;
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
