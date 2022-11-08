using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;

namespace KursDomain.Documents.CommonReferences.Kontragent;

public class KontragentBank : RSViewModelBase, IEntity<TD_43>
{
    private References.Bank myBank;
    private TD_43 myEntity;

    public KontragentBank()
    {
        Entity = DefaultValue();
    }

    public KontragentBank(TD_43 entity)
    {
        Entity = entity ?? DefaultValue();
        if (Entity.SD_44 != null)
        {
            Bank = new References.Bank();
            Bank.LoadFromEntity(Entity.SD_44);
        }
    }

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = false)]
    public override int Code
    {
        get => Entity.CODE;
        set
        {
            if (Entity.CODE == value) return;
            Entity.CODE = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
    public References.Bank Bank
    {
        get => myBank;
        set
        {
            if (myBank == value) return;
            myBank = value;
            Entity.BANK_DC = value?.DocCode;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "№ расчетного счета")]
    public string AccountNumber
    {
        get => Entity.RASCH_ACC;
        set
        {
            if (Entity.RASCH_ACC == value) return;
            Entity.RASCH_ACC = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Наименование банка")]
    public string BankName => Bank?.Name;

    [Display(AutoGenerateField = true, Name = "Корр.счет")]
    public string BankKorrAccount => Bank?.CorrAccount;

    [Display(AutoGenerateField = true, Name = "КПП")]
    public string BankKPP => Bank?.BIK;


    [Display(AutoGenerateField = true, Name = "Для печати")]
    public bool IsForPrint
    {
        get => (Entity.USE_FOR_TLAT_TREB ?? 0) == 1;
        set
        {
            if ((Entity.USE_FOR_TLAT_TREB ?? 0) == (value ? 1 : 0)) return;
            Entity.USE_FOR_TLAT_TREB = value ? 1 : 0;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false, Name = "Удален")]
    public bool IsDeleted
    {
        get => (Entity.DELETED ?? 0) == 1;
        set
        {
            if ((Entity.DELETED ?? 0) == (value ? 1 : 0)) return;
            Entity.DELETED = (short?)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Заблокирован")]
    public bool IsDisabled
    {
        get => (Entity.DISABLED ?? 0) == 1;
        set
        {
            if ((Entity.DISABLED ?? 0) == (value ? 1 : 0)) return;
            Entity.DISABLED = (short?)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)] public bool IsAccessRight { get; set; }

    public TD_43 DefaultValue()
    {
        return new TD_43 { DOC_CODE = -1, CODE = -1 };
    }

    [Display(AutoGenerateField = false)]
    public TD_43 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public List<TD_43> LoadList()
    {
        throw new NotImplementedException();
    }
}
