using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace KursDomain.Documents.Bank;

public class SD_101ViewModel : RSViewModelBase, IEntity<SD_101>
{
    private SD_101 myEntity;

    public SD_101ViewModel()
    {
        Entity = new SD_101 { DOC_CODE = -1 };
    }

    public SD_101ViewModel(SD_101 entity)
    {
        Entity = entity ?? new SD_101 { DOC_CODE = -1 };
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

    public DateTime VV_START_DATE
    {
        get => Entity.VV_START_DATE;
        set
        {
            if (Entity.VV_START_DATE == value) return;
            Entity.VV_START_DATE = value;
            RaisePropertyChanged();
        }
    }

    public DateTime VV_STOP_DATE
    {
        get => Entity.VV_STOP_DATE;
        set
        {
            if (Entity.VV_STOP_DATE == value) return;
            Entity.VV_STOP_DATE = value;
            RaisePropertyChanged();
        }
    }

    public decimal VV_ACC_DC
    {
        get => Entity.VV_ACC_DC;
        set
        {
            if (Entity.VV_ACC_DC == value) return;
            Entity.VV_ACC_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? VV_RUB_MONEY_START
    {
        get => Entity.VV_RUB_MONEY_START;
        set
        {
            if (Entity.VV_RUB_MONEY_START == value) return;
            Entity.VV_RUB_MONEY_START = value;
            RaisePropertyChanged();
        }
    }

    public decimal? VV_RUB_MONEY_STOP
    {
        get => Entity.VV_RUB_MONEY_STOP;
        set
        {
            if (Entity.VV_RUB_MONEY_STOP == value) return;
            Entity.VV_RUB_MONEY_STOP = value;
            RaisePropertyChanged();
        }
    }

    public SD_114 SD_114
    {
        get => Entity.SD_114;
        set
        {
            if (Entity.SD_114 == value) return;
            Entity.SD_114 = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public SD_101 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_101 DefaultValue()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        if (Entity.SD_114 == null) return null;
        return
            $"Банк {GlobalOptions.ReferencesCache.GetBankAccount(Entity.VV_ACC_DC)} от {VV_START_DATE.ToShortDateString()}";
    }

    public List<SD_101> LoadList()
    {
        throw new NotImplementedException();
    }

    public virtual SD_101 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual SD_101 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public virtual void Save(SD_101 doc)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public void Delete()
    {
        throw new NotImplementedException();
    }

    public void Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Delete(decimal dc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(SD_101 ent)
    {
        VV_START_DATE = ent.VV_START_DATE;
        VV_STOP_DATE = ent.VV_STOP_DATE;
        VV_ACC_DC = ent.VV_ACC_DC;
        VV_RUB_MONEY_START = ent.VV_RUB_MONEY_START;
        VV_RUB_MONEY_STOP = ent.VV_RUB_MONEY_STOP;
        SD_114 = ent.SD_114;
    }

    public void UpdateTo(SD_101 ent)
    {
        ent.VV_START_DATE = VV_START_DATE;
        ent.VV_STOP_DATE = VV_STOP_DATE;
        ent.VV_ACC_DC = VV_ACC_DC;
        ent.VV_RUB_MONEY_START = VV_RUB_MONEY_START;
        ent.VV_RUB_MONEY_STOP = VV_RUB_MONEY_STOP;
        ent.SD_114 = SD_114;
    }
}
