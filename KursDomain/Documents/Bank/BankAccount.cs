using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;


namespace KursDomain.Documents.Bank;

[MetadataType(typeof(DataAnnotationBankAccount))]
public sealed class BankAccount : RSViewModelBase, IEntity<SD_114>
{
    private Bank myBank;
    private References.Currency myCurrency;


    private SD_114 myEntity;
    private int myKontrBankCode;


    private int myLastYearOperationsCount;

    public BankAccount()
    {
        Entity = DefaultValue();
    }

    public BankAccount(SD_114 entity)
    {
        Entity = entity ?? new SD_114 { DOC_CODE = -1 };
        if (Entity.SD_44 != null)
            Bank = new Bank(Entity.SD_44);
        Name = $"{Bank?.Name} Сч.№ {Entity.BA_RASH_ACC} " +
               // ReSharper disable once PossibleInvalidOperationException
               $"{MainReferences.Currencies[(decimal)Entity.CurrencyDC]}";
    }

    public References.Currency Currency
    {
        get => myCurrency;
        set
        {
            if (myCurrency == value) return;
            myCurrency = value;
            RaisePropertyChanged();
        }
    }

    public decimal DOC_CODE
    {
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public override decimal DocCode
    {
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public int LastYearOperationsCount
    {
        get => myLastYearOperationsCount;
        set
        {
            if (myLastYearOperationsCount == value) return;
            myLastYearOperationsCount = value;
            RaisePropertyChanged();
        }
    }

    public bool IsLastYearOperations => myLastYearOperationsCount > 0;

    public Bank Bank
    {
        get => myBank;
        set
        {
            if (myBank == value) return;
            myBank = value;
            RaisePropertyChanged();
        }
    }

    public string BankName => Bank?.Name;

    public string CorrAccount => Bank?.CORRESP_ACC;

    public int KontrBankCode
    {
        get => myKontrBankCode;
        set
        {
            if (myKontrBankCode == value) return;
            myKontrBankCode = value;
            RaisePropertyChanged();
        }
    }

    public int BA_RASH_ACC_CODE
    {
        get => Entity.BA_RASH_ACC_CODE;
        set
        {
            if (Entity.BA_RASH_ACC_CODE == value) return;
            Entity.BA_RASH_ACC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public string Account
    {
        get => Entity.BA_RASH_ACC;
        set
        {
            if (Entity.BA_RASH_ACC == value) return;
            Entity.BA_RASH_ACC = value;
            RaisePropertyChanged();
        }
    }

    public short BA_CURRENCY
    {
        get => Entity.BA_CURRENCY;
        set
        {
            if (Entity.BA_CURRENCY == value) return;
            Entity.BA_CURRENCY = value;
            RaisePropertyChanged();
        }
    }

    public decimal? BA_BANK_AS_KONTRAGENT_DC
    {
        get => Entity.BA_BANK_AS_KONTRAGENT_DC;
        set
        {
            if (Entity.BA_BANK_AS_KONTRAGENT_DC == value) return;
            Entity.BA_BANK_AS_KONTRAGENT_DC = value;
            RaisePropertyChanged();
        }
    }

    public short BA_TRANSIT
    {
        get => Entity.BA_TRANSIT;
        set
        {
            if (Entity.BA_TRANSIT == value) return;
            Entity.BA_TRANSIT = value;
            RaisePropertyChanged();
        }
    }

    public decimal BankDC
    {
        get => Entity.BA_BANKDC;
        set
        {
            if (Entity.BA_BANKDC == value) return;
            Entity.BA_BANKDC = value;
            RaisePropertyChanged();
        }
    }


    public string BA_BANK_NAME
    {
        get => Entity.BA_BANK_NAME;
        set
        {
            if (Entity.BA_BANK_NAME == value) return;
            Entity.BA_BANK_NAME = value;
            RaisePropertyChanged();
        }
    }

    public short? BA_NEGATIVE_RESTS
    {
        get => Entity.BA_NEGATIVE_RESTS;
        set
        {
            if (Entity.BA_NEGATIVE_RESTS == value) return;
            Entity.BA_NEGATIVE_RESTS = value;
            RaisePropertyChanged();
        }
    }

    public short? BA_BANK_ACCOUNT
    {
        get => Entity.BA_BANK_ACCOUNT;
        set
        {
            if (Entity.BA_BANK_ACCOUNT == value) return;
            Entity.BA_BANK_ACCOUNT = value;
            RaisePropertyChanged();
        }
    }

    public string BA_ACC_SHORTNAME
    {
        get => Entity.BA_ACC_SHORTNAME;
        set
        {
            if (Entity.BA_ACC_SHORTNAME == value) return;
            Entity.BA_ACC_SHORTNAME = value;
            RaisePropertyChanged();
        }
    }

    public decimal? BA_CENTR_OTV_DC
    {
        get => Entity.BA_CENTR_OTV_DC;
        set
        {
            if (Entity.BA_CENTR_OTV_DC == value) return;
            Entity.BA_CENTR_OTV_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CurrencyDC
    {
        get => Entity.CurrencyDC;
        set
        {
            if (Entity.CurrencyDC == value) return;
            Entity.CurrencyDC = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? StartDate
    {
        get => Entity.StartDate;
        set
        {
            if (Entity.StartDate == value) return;
            Entity.StartDate = value;
            RaisePropertyChanged();
        }
    }

    public decimal? StartSumma
    {
        get => Entity.StartSumma;
        set
        {
            if (Entity.StartSumma == value) return;
            Entity.StartSumma = value;
            RaisePropertyChanged();
        }
    }

    public bool? IsDeleted
    {
        get => Entity.IsDeleted;
        set
        {
            if (Entity.IsDeleted == value) return;
            Entity.IsDeleted = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? DateNonZero
    {
        get => Entity.DateNonZero;
        set
        {
            if (Entity.DateNonZero == value) return;
            Entity.DateNonZero = value;
            RaisePropertyChanged();
        }
    }

    public SD_44 SD_44
    {
        get => Entity.SD_44;
        set
        {
            if (Entity.SD_44 == value) return;
            Entity.SD_44 = value;
            RaisePropertyChanged();
        }
    }

    public TD_43 TD_43
    {
        get => Entity.TD_43;
        set
        {
            if (Entity.TD_43 == value) return;
            Entity.TD_43 = value;
            RaisePropertyChanged();
        }
    }

    public SD_40 SD_40
    {
        get => Entity.SD_40;
        set
        {
            if (Entity.SD_40 == value) return;
            Entity.SD_40 = value;
            RaisePropertyChanged();
        }
    }

    public SD_301 SD_301
    {
        get => Entity.SD_301;
        set
        {
            if (Entity.SD_301 == value) return;
            Entity.SD_301 = value;
            RaisePropertyChanged();
        }
    }

    public bool IsAccessRight { get; set; }

    public SD_114 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_114 DefaultValue()
    {
        return new SD_114 { DOC_CODE = -1 };
    }

    public List<SD_114> LoadList()
    {
        throw new NotImplementedException();
    }

    public SD_114 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public SD_114 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Save(SD_114 doc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(SD_114 ent)
    {
        BA_RASH_ACC_CODE = ent.BA_RASH_ACC_CODE;
        Account = ent.BA_RASH_ACC;
        BA_CURRENCY = ent.BA_CURRENCY;
        BA_BANK_AS_KONTRAGENT_DC = ent.BA_BANK_AS_KONTRAGENT_DC;
        BA_TRANSIT = ent.BA_TRANSIT;
        BankDC = ent.BA_BANKDC;
        BA_BANK_NAME = ent.BA_BANK_NAME;
        BA_NEGATIVE_RESTS = ent.BA_NEGATIVE_RESTS;
        BA_BANK_ACCOUNT = ent.BA_BANK_ACCOUNT;
        BA_ACC_SHORTNAME = ent.BA_ACC_SHORTNAME;
        BA_CENTR_OTV_DC = ent.BA_CENTR_OTV_DC;
        CurrencyDC = ent.CurrencyDC;
        StartDate = ent.StartDate;
        StartSumma = ent.StartSumma;
        IsDeleted = ent.IsDeleted;
        DateNonZero = ent.DateNonZero;
        SD_44 = ent.SD_44;
        TD_43 = ent.TD_43;
        SD_40 = ent.SD_40;
        SD_301 = ent.SD_301;
    }

    public void UpdateTo(SD_114 ent)
    {
        ent.BA_RASH_ACC_CODE = BA_RASH_ACC_CODE;
        ent.BA_RASH_ACC = Account;
        ent.BA_CURRENCY = BA_CURRENCY;
        ent.BA_BANK_AS_KONTRAGENT_DC = BA_BANK_AS_KONTRAGENT_DC;
        ent.BA_TRANSIT = BA_TRANSIT;
        ent.BA_BANKDC = BankDC;
        ent.BA_BANK_NAME = BA_BANK_NAME;
        ent.BA_NEGATIVE_RESTS = BA_NEGATIVE_RESTS;
        ent.BA_BANK_ACCOUNT = BA_BANK_ACCOUNT;
        ent.BA_ACC_SHORTNAME = BA_ACC_SHORTNAME;
        ent.BA_CENTR_OTV_DC = BA_CENTR_OTV_DC;
        ent.CurrencyDC = CurrencyDC;
        ent.StartDate = StartDate;
        ent.StartSumma = StartSumma;
        ent.IsDeleted = IsDeleted;
        ent.DateNonZero = DateNonZero;
        ent.SD_44 = SD_44;
        ent.TD_43 = TD_43;
        ent.SD_40 = SD_40;
        ent.SD_301 = SD_301;
    }
}

public class DataAnnotationBankAccount : DataAnnotationForFluentApiBase, IMetadataProvider<BankAccount>
{
    void IMetadataProvider<BankAccount>.BuildMetadata(MetadataBuilder<BankAccount> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.KontrBankCode).NotAutoGenerated();
        builder.Property(_ => _.IsAccessRight).NotAutoGenerated();
        builder.Property(_ => _.CorrAccount).NotAutoGenerated();
        builder.Property(_ => _.BankName).NotAutoGenerated();
        builder.Property(_ => _.BankDC).NotAutoGenerated();
        builder.Property(_ => _.Bank).AutoGenerated().DisplayName("Банк");
        builder.Property(_ => _.Account).AutoGenerated().DisplayName("Счет");
        builder.Property(_ => _.Currency).AutoGenerated().DisplayName("Валюта");
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Краткое наименование");
    }
}
