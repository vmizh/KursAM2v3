using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

// ReSharper disable InconsistentNaming
namespace KursDomain.Documents.Invoices;

public class SCHET_FACT_KONTR_OUT_ROWSViewModel : RSViewModelBase, IEntity<SCHET_FACT_KONTR_OUT_ROWS>
{
    private SCHET_FACT_KONTR_OUT_ROWS myEntity;

    public SCHET_FACT_KONTR_OUT_ROWSViewModel()
    {
        Entity = DefaultValue();
    }

    public SCHET_FACT_KONTR_OUT_ROWSViewModel(SCHET_FACT_KONTR_OUT_ROWS entity)
    {
        Entity = entity ?? DefaultValue();
    }

    public Guid ID
    {
        get => Entity.ID;
        set
        {
            if (Entity.ID == value) return;
            Entity.ID = value;
            RaisePropertyChanged();
        }
    }

    public Guid HEAD_ID
    {
        get => Entity.HEAD_ID;
        set
        {
            if (Entity.HEAD_ID == value) return;
            Entity.HEAD_ID = value;
            RaisePropertyChanged();
        }
    }

    public string ROW_TEXT
    {
        get => Entity.ROW_TEXT;
        set
        {
            if (Entity.ROW_TEXT == value) return;
            Entity.ROW_TEXT = value;
            RaisePropertyChanged();
        }
    }

    public decimal NOMENKL_DC
    {
        get => Entity.NOMENKL_DC;
        set
        {
            if (Entity.NOMENKL_DC == value) return;
            Entity.NOMENKL_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal KOL
    {
        get => Entity.KOL;
        set
        {
            if (Entity.KOL == value) return;
            Entity.KOL = value;
            RaisePropertyChanged();
        }
    }

    public decimal PRICE
    {
        get => Entity.PRICE;
        set
        {
            if (Entity.PRICE == value) return;
            Entity.PRICE = value;
            RaisePropertyChanged();
        }
    }

    public decimal NDS_PERCENT
    {
        get => Entity.NDS_PERCENT;
        set
        {
            if (Entity.NDS_PERCENT == value) return;
            Entity.NDS_PERCENT = value;
            RaisePropertyChanged();
        }
    }

    public decimal SUMMA_NDS
    {
        get => Entity.SUMMA_NDS;
        set
        {
            if (Entity.SUMMA_NDS == value) return;
            Entity.SUMMA_NDS = value;
            RaisePropertyChanged();
        }
    }

    public decimal SUMMA_WITHOUT_NDS
    {
        get => Entity.SUMMA_WITHOUT_NDS;
        set
        {
            if (Entity.SUMMA_WITHOUT_NDS == value) return;
            Entity.SUMMA_WITHOUT_NDS = value;
            RaisePropertyChanged();
        }
    }

    public decimal SUMMA_WITH_NDS
    {
        get => Entity.SUMMA_WITH_NDS;
        set
        {
            if (Entity.SUMMA_WITH_NDS == value) return;
            Entity.SUMMA_WITH_NDS = value;
            RaisePropertyChanged();
        }
    }

    public string COUNTRY
    {
        get => Entity.COUNTRY;
        set
        {
            if (Entity.COUNTRY == value) return;
            Entity.COUNTRY = value;
            RaisePropertyChanged();
        }
    }

    public string COUNTRY_CODE
    {
        get => Entity.COUNTRY_CODE;
        set
        {
            if (Entity.COUNTRY_CODE == value) return;
            Entity.COUNTRY_CODE = value;
            RaisePropertyChanged();
        }
    }

    public string NUM_GRUZ_DECLAR
    {
        get => Entity.NUM_GRUZ_DECLAR;
        set
        {
            if (Entity.NUM_GRUZ_DECLAR == value) return;
            Entity.NUM_GRUZ_DECLAR = value;
            RaisePropertyChanged();
        }
    }

    public SCHET_FACT_KONTR_OUT SCHET_FACT_KONTR_OUT
    {
        get => Entity.SCHET_FACT_KONTR_OUT;
        set
        {
            if (Entity.SCHET_FACT_KONTR_OUT == value) return;
            Entity.SCHET_FACT_KONTR_OUT = value;
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

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public SCHET_FACT_KONTR_OUT_ROWS Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SCHET_FACT_KONTR_OUT_ROWS DefaultValue()
    {
        return new SCHET_FACT_KONTR_OUT_ROWS
        {
            ID = Guid.NewGuid()
        };
    }

    public List<SCHET_FACT_KONTR_OUT_ROWS> LoadList()
    {
        throw new NotImplementedException();
    }

    public SCHET_FACT_KONTR_OUT_ROWS Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public virtual void Save(SCHET_FACT_KONTR_OUT_ROWS doc)
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

    public void UpdateFrom(SCHET_FACT_KONTR_OUT_ROWS ent)
    {
        ID = ent.ID;
        HEAD_ID = ent.HEAD_ID;
        ROW_TEXT = ent.ROW_TEXT;
        NOMENKL_DC = ent.NOMENKL_DC;
        KOL = ent.KOL;
        PRICE = ent.PRICE;
        NDS_PERCENT = ent.NDS_PERCENT;
        SUMMA_NDS = ent.SUMMA_NDS;
        SUMMA_WITHOUT_NDS = ent.SUMMA_WITHOUT_NDS;
        SUMMA_WITH_NDS = ent.SUMMA_WITH_NDS;
        COUNTRY = ent.COUNTRY;
        COUNTRY_CODE = ent.COUNTRY_CODE;
        NUM_GRUZ_DECLAR = ent.NUM_GRUZ_DECLAR;
        SCHET_FACT_KONTR_OUT = ent.SCHET_FACT_KONTR_OUT;
        SD_83 = ent.SD_83;
    }

    public void UpdateTo(SCHET_FACT_KONTR_OUT_ROWS ent)
    {
        ent.ID = ID;
        ent.HEAD_ID = HEAD_ID;
        ent.ROW_TEXT = ROW_TEXT;
        ent.NOMENKL_DC = NOMENKL_DC;
        ent.KOL = KOL;
        ent.PRICE = PRICE;
        ent.NDS_PERCENT = NDS_PERCENT;
        ent.SUMMA_NDS = SUMMA_NDS;
        ent.SUMMA_WITHOUT_NDS = SUMMA_WITHOUT_NDS;
        ent.SUMMA_WITH_NDS = SUMMA_WITH_NDS;
        ent.COUNTRY = COUNTRY;
        ent.COUNTRY_CODE = COUNTRY_CODE;
        ent.NUM_GRUZ_DECLAR = NUM_GRUZ_DECLAR;
        ent.SCHET_FACT_KONTR_OUT = SCHET_FACT_KONTR_OUT;
        ent.SD_83 = SD_83;
    }

    public virtual SCHET_FACT_KONTR_OUT_ROWS Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual SCHET_FACT_KONTR_OUT_ROWS Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public SCHET_FACT_KONTR_OUT_ROWS Load(decimal dc)
    {
        throw new NotImplementedException();
    }
}
