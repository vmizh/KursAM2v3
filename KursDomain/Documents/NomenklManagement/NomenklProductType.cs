using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace KursDomain.Documents.NomenklManagement;

/// <summary>
///     Справочник SD_119 Тип продукции
/// </summary>
[MetadataType(typeof(NomenklProductType_FluentAPI))]
public class NomenklProductType : RSViewModelBase, IEntity<SD_119>
{
    private SD_119 myEntity;

    public NomenklProductType()
    {
        Entity = DefaultValue();
    }

    public NomenklProductType(SD_119 entity)
    {
        Entity = entity ?? DefaultValue();
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
        get => DOC_CODE;
        set
        {
            if (DOC_CODE == value) return;
            DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public string MC_NAME
    {
        get => Entity.MC_NAME;
        set
        {
            if (Entity.MC_NAME == value) return;
            Entity.MC_NAME = value;
            RaisePropertyChanged();
        }
    }

    public override string Name
    {
        get => MC_NAME;
        set
        {
            if (MC_NAME == value) return;
            MC_NAME = value;
            RaisePropertyChanged();
        }
    }

    public short MC_DELETED
    {
        get => Entity.MC_DELETED;
        set
        {
            if (Entity.MC_DELETED == value) return;
            Entity.MC_DELETED = value;
            RaisePropertyChanged();
        }
    }

    public double? MC_PROC_OTKL
    {
        get => Entity.MC_PROC_OTKL;
        set
        {
            if (Entity.MC_PROC_OTKL == value) return;
            Entity.MC_PROC_OTKL = value;
            RaisePropertyChanged();
        }
    }

    public short? MC_TARA
    {
        get => Entity.MC_TARA;
        set
        {
            if (Entity.MC_TARA == value) return;
            Entity.MC_TARA = value;
            RaisePropertyChanged();
        }
    }

    public bool IsDeleted
    {
        get => MC_DELETED == 1;
        set
        {
            if (MC_DELETED == 1 == value) return;
            MC_DELETED = (short)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    public short? MC_TRANSPORT
    {
        get => Entity.MC_TRANSPORT;
        set
        {
            if (Entity.MC_TRANSPORT == value) return;
            Entity.MC_TRANSPORT = value;
            RaisePropertyChanged();
        }
    }

    public short? MC_PREDOPLATA
    {
        get => Entity.MC_PREDOPLATA;
        set
        {
            if (Entity.MC_PREDOPLATA == value) return;
            Entity.MC_PREDOPLATA = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public SD_119 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_119 DefaultValue()
    {
        return new SD_119
        {
            DOC_CODE = -1,
            MC_DELETED = 0,
            MC_PREDOPLATA = 0,
            MC_TARA = 0,
            MC_TRANSPORT = 0,
            MC_PROC_OTKL = 0
        };
    }

    public List<SD_119> LoadList()
    {
        throw new NotImplementedException();
    }

    public SD_119 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Save(SD_119 doc)
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

    public void UpdateFrom(SD_119 ent)
    {
        MC_NAME = ent.MC_NAME;
        MC_DELETED = ent.MC_DELETED;
        MC_PROC_OTKL = ent.MC_PROC_OTKL;
        MC_TARA = ent.MC_TARA;
        MC_TRANSPORT = ent.MC_TRANSPORT;
        MC_PREDOPLATA = ent.MC_PREDOPLATA;
    }

    public void UpdateTo(SD_119 ent)
    {
        ent.MC_NAME = MC_NAME;
        ent.MC_DELETED = MC_DELETED;
        ent.MC_PROC_OTKL = MC_PROC_OTKL;
        ent.MC_TARA = MC_TARA;
        ent.MC_TRANSPORT = MC_TRANSPORT;
        ent.MC_PREDOPLATA = MC_PREDOPLATA;
    }

    public virtual SD_119 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual SD_119 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public SD_119 Load(decimal dc)
    {
        throw new NotImplementedException();
    }
}

public class NomenklProductType_FluentAPI : DataAnnotationForFluentApiBase, IMetadataProvider<NomenklProductType>
{
    void IMetadataProvider<NomenklProductType>.BuildMetadata(
        MetadataBuilder<NomenklProductType> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(x => x.Name).AutoGenerated()
            .DisplayName("Наименование");
    }
}
