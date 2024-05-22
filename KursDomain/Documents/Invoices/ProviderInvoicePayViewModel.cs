using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursDomain.Documents.Invoices;

//[MetadataType(typeof(ProviderInvoicePayData_FluentAPI))]
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

    private void LoadReferences()
    {
       
    }

    [Display(AutoGenerateField = false)]
    public BankAccount Bank => GlobalOptions.ReferencesCache.GetBankAccount(Entity.TD_101?.SD_101?.SD_114?.DOC_CODE) as BankAccount;
    [Display(AutoGenerateField = false)]
    public CashBox CashBook => GlobalOptions.ReferencesCache.GetCashBox(Entity.SD_34?.CA_DC) as CashBox;

    [Display(AutoGenerateField = true, Name = "Касса/Банк")]
    public override string Name => Bank?.Name ?? CashBook?.Name;

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = true, Name = "Сумма", Order = 2)]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
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
    [Display(AutoGenerateField = false)]
    public decimal DocSumma =>
        (TD_101?.VVT_VAL_RASHOD ?? 0) + (TD_110?.VZT_CRS_SUMMA ?? 0) + (SD_34?.SUMM_ORD ?? 0);

    [Display(AutoGenerateField = true, Name = "Оплата", Order = 3)]
    [ReadOnly(true)]
    public string DocExtName
    {
        get
        {
            if (TD_101 != null)
                return $"{TD_101.SD_101.SD_114.BA_BANK_NAME} " +
                       $"р/с {TD_101.SD_101.SD_114.BA_RASH_ACC}";
            ;
            if (TD_110 != null) return $"{TD_110.VZT_DOC_NOTES}";
            if (SD_34 != null) return $"Касса {((IName)GlobalOptions.ReferencesCache.GetCashBox(SD_34.CA_DC)).Name}";
            ;
            return null;
        }
    }

    [Display(AutoGenerateField = true, Name = "Курс(справочно)", Order = 5)]
    [DisplayFormat(DataFormatString = "n4")]
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


    [Display(AutoGenerateField = true, Name = "Тип документа", Order = 1)]
    [ReadOnly(true)]
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

    [Display(AutoGenerateField = true, Name = "Дата", Order = 0)]
    [ReadOnly(true)]
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

    [Display(AutoGenerateField = true, Name = "Расшифровка", Order = 4)]
    [ReadOnly(true)]
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
    [Display(AutoGenerateField = false)]
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
    [Display(AutoGenerateField = false)]
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
    [Display(AutoGenerateField = false)]
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
    [Display(AutoGenerateField = false)]
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
    [Display(AutoGenerateField = true, Name = "Примечание", Order = 7)]
    [ReadOnly(true)]
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
    [Display(AutoGenerateField = false)]
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
    [Display(AutoGenerateField = false)]
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
    [Display(AutoGenerateField = false)]
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
    [Display(AutoGenerateField = false)]
    public bool IsAccessRight { get; set; }

    public ProviderInvoicePay DefaultValue()
    {
        return new ProviderInvoicePay { Id = Guid.NewGuid() };
    }
    [Display(AutoGenerateField = false)]
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

        metadataBuilder.Property(_ => _.DocDate).AutoGenerated().LocatedAt(0).DisplayName("Дата").ReadOnly();
        metadataBuilder.Property(_ => _.DocName).AutoGenerated().DisplayName("Тип документа").ReadOnly();
        metadataBuilder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма")
            .DisplayFormatString("n2").ReadOnly();
        metadataBuilder.Property(_ => _.DocExtName).AutoGenerated().DisplayName("Оплата").ReadOnly();
        metadataBuilder.Property(_ => _.DocNum).AutoGenerated().DisplayName("Расшифровка").ReadOnly();
        metadataBuilder.Property(_ => _.Rate).AutoGenerated().DisplayName("Курс(справочно)").ReadOnly();
        metadataBuilder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
    }
}
