using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.IReferences.Nomenkl;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class Unit : IUnit, IDocCode, IName, IEquatable<Unit>,IComparable, ICache
{
    public decimal DocCode { get; set; }

    public bool Equals(Unit other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    [JsonIgnore]
    public string Description => $"Ед. изм.: {Name}";
    public string OKEI { get; set; }
    public string OKEI_Code { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_175 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Name = entity.ED_IZM_NAME;
        OKEI = entity.ED_IZM_OKEI;
        OKEI_Code = entity.ED_IZM_OKEI_CODE;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Unit) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }

    public void LoadFromCache()
    {
        
    }

    public DateTime LastUpdateServe { get; set; }

    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
}

[MetadataType(typeof(Unit_FluentAPI))]
public class UnitViewModel : RSViewModelBase, IEntity<SD_175>
{
    private SD_175 myEntity;

    public UnitViewModel()
    {
        Entity = DefaultValue();
    }

    public UnitViewModel(SD_175 entity)
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

    public string ED_IZM_NAME
    {
        get => Entity.ED_IZM_NAME;
        set
        {
            if (Entity.ED_IZM_NAME == value) return;
            Entity.ED_IZM_NAME = value;
            RaisePropertyChanged();
        }
    }

    public override string Name
    {
        get => ED_IZM_NAME;
        set
        {
            if (ED_IZM_NAME == value) return;
            ED_IZM_NAME = value;
            RaisePropertyChanged();
        }
    }

    public string ED_IZM_OKEI
    {
        get => Entity.ED_IZM_OKEI;
        set
        {
            if (Entity.ED_IZM_OKEI == value) return;
            Entity.ED_IZM_OKEI = value;
            RaisePropertyChanged();
        }
    }

    public short? ED_IZM_MONEY
    {
        get => Entity.ED_IZM_MONEY;
        set
        {
            if (Entity.ED_IZM_MONEY == value) return;
            Entity.ED_IZM_MONEY = value;
            RaisePropertyChanged();
        }
    }

    public short? ED_IZM_INT
    {
        get => Entity.ED_IZM_INT;
        set
        {
            if (Entity.ED_IZM_INT == value) return;
            Entity.ED_IZM_INT = value;
            RaisePropertyChanged();
        }
    }

    public string ED_IZM_OKEI_CODE
    {
        get => Entity.ED_IZM_OKEI_CODE;
        set
        {
            if (Entity.ED_IZM_OKEI_CODE == value) return;
            Entity.ED_IZM_OKEI_CODE = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public SD_175 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_175 DefaultValue()
    {
        return new SD_175
        {
            DOC_CODE = -1
        };
    }

    public List<SD_175> LoadList()
    {
        throw new NotImplementedException();
    }

    public SD_175 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public virtual void Save(SD_175 doc)
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

    public void UpdateFrom(SD_175 ent)
    {
        ED_IZM_NAME = ent.ED_IZM_NAME;
        ED_IZM_OKEI = ent.ED_IZM_OKEI;
        ED_IZM_MONEY = ent.ED_IZM_MONEY;
        ED_IZM_INT = ent.ED_IZM_INT;
        ED_IZM_OKEI_CODE = ent.ED_IZM_OKEI_CODE;
    }

    public void UpdateTo(SD_175 ent)
    {
        ent.ED_IZM_NAME = ED_IZM_NAME;
        ent.ED_IZM_OKEI = ED_IZM_OKEI;
        ent.ED_IZM_MONEY = ED_IZM_MONEY;
        ent.ED_IZM_INT = ED_IZM_INT;
        ent.ED_IZM_OKEI_CODE = ED_IZM_OKEI_CODE;
    }

    public virtual SD_175 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual SD_175 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public SD_175 Load(decimal dc)
    {
        throw new NotImplementedException();
    }
}

public class Unit_FluentAPI : DataAnnotationForFluentApiBase, IMetadataProvider<UnitViewModel>
{
    void IMetadataProvider<UnitViewModel>.BuildMetadata(
        MetadataBuilder<UnitViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(x => x.Name).AutoGenerated()
            .DisplayName("Наименование");
        builder.Property(x => x.ED_IZM_OKEI).AutoGenerated()
            .DisplayName("OKEI");
        builder.Property(x => x.ED_IZM_OKEI_CODE).AutoGenerated()
            .DisplayName("OKEI Code");
    }
}
