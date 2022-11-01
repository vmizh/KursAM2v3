using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace KursDomain.Documents.Dogovora.Old;

/// <summary>
///     Строка договора покупки у поставщика
/// </summary>
public class ContractProviderRowViewModel : RSViewModelBase, IEntity<TD_112>
{
    private TD_112 myEntity;

    public ContractProviderRowViewModel()
    {
        Entity = DefaultValue();
    }

    public ContractProviderRowViewModel(TD_112 entity)
    {
        Entity = entity ?? new TD_112 { DOC_CODE = -1 };
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

    public int CODE
    {
        get => Entity.CODE;
        set
        {
            if (Entity.CODE == value) return;
            Entity.CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal DOT_NOMENKL_DC
    {
        get => Entity.DOT_NOMENKL_DC;
        set
        {
            if (Entity.DOT_NOMENKL_DC == value) return;
            Entity.DOT_NOMENKL_DC = value;
            RaisePropertyChanged();
        }
    }

    public double DOT_KOL
    {
        get => Entity.DOT_KOL;
        set
        {
            if (Entity.DOT_KOL - value < 0.01) return;
            Entity.DOT_KOL = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DOT_CRS_CENA
    {
        get => Entity.DOT_CRS_CENA;
        set
        {
            if (Entity.DOT_CRS_CENA == value) return;
            Entity.DOT_CRS_CENA = value;
            RaisePropertyChanged();
        }
    }

    public double DOT_NDS_PERCENT
    {
        get => Entity.DOT_NDS_PERCENT;
        set
        {
            if (Entity.DOT_NDS_PERCENT - value < 0.01) return;
            Entity.DOT_NDS_PERCENT = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DOT_NDS
    {
        get => Entity.DOT_NDS;
        set
        {
            if (Entity.DOT_NDS == value) return;
            Entity.DOT_NDS = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DOT_ACCIZ_SUMMA
    {
        get => Entity.DOT_ACCIZ_SUMMA;
        set
        {
            if (Entity.DOT_ACCIZ_SUMMA == value) return;
            Entity.DOT_ACCIZ_SUMMA = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DOT_K_OPLATE_SUMMA
    {
        get => Entity.DOT_K_OPLATE_SUMMA;
        set
        {
            if (Entity.DOT_K_OPLATE_SUMMA == value) return;
            Entity.DOT_K_OPLATE_SUMMA = value;
            RaisePropertyChanged();
        }
    }

    public SD_112 SD_112
    {
        get => Entity.SD_112;
        set
        {
            if (Entity.SD_112 == value) return;
            Entity.SD_112 = value;
            RaisePropertyChanged();
        }
    }

    public SD_83 SD_83
    {
        get => Entity.SD_83;
        set
        {
            if (Entity.SD_83 == value) return;
            Entity.SD_83 = value;
            RaisePropertyChanged();
        }
    }

    public bool IsAccessRight { get; set; }

    public TD_112 DefaultValue()
    {
        return new TD_112 { DOC_CODE = -1 };
    }

    public TD_112 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public List<TD_112> LoadList()
    {
        throw new NotImplementedException();
    }

    public virtual TD_112 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual TD_112 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public virtual void Save(TD_112 doc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(TD_112 ent)
    {
        CODE = ent.CODE;
        DOT_NOMENKL_DC = ent.DOT_NOMENKL_DC;
        DOT_KOL = ent.DOT_KOL;
        DOT_CRS_CENA = ent.DOT_CRS_CENA;
        DOT_NDS_PERCENT = ent.DOT_NDS_PERCENT;
        DOT_NDS = ent.DOT_NDS;
        DOT_ACCIZ_SUMMA = ent.DOT_ACCIZ_SUMMA;
        DOT_K_OPLATE_SUMMA = ent.DOT_K_OPLATE_SUMMA;
        SD_112 = ent.SD_112;
        SD_83 = ent.SD_83;
    }

    public void UpdateTo(TD_112 ent)
    {
        ent.CODE = CODE;
        ent.DOT_NOMENKL_DC = DOT_NOMENKL_DC;
        ent.DOT_KOL = DOT_KOL;
        ent.DOT_CRS_CENA = DOT_CRS_CENA;
        ent.DOT_NDS_PERCENT = DOT_NDS_PERCENT;
        ent.DOT_NDS = DOT_NDS;
        ent.DOT_ACCIZ_SUMMA = DOT_ACCIZ_SUMMA;
        ent.DOT_K_OPLATE_SUMMA = DOT_K_OPLATE_SUMMA;
        ent.SD_112 = SD_112;
        ent.SD_83 = SD_83;
    }
}
