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

[DebuggerDisplay("{DocCode,nq} {Name,nq} {ParentDC,nq} {NomenklCount,nq}")]
public class NomenklGroup : IDocCode, IDocGuid, IName, INomenklGroup, IEquatable<NomenklGroup>, IComparable, ICache
{
    public NomenklGroup()
    {
            LoadFromCache();
    }
    public int CompareTo(object obj) 
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    public decimal DocCode { get; set; }
    public Guid Id { get; set; }

    public bool Equals(NomenklGroup other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    [JsonIgnore]
    public string Description => $"Ном.категория: {Name}";
    public decimal? ParentDC { get; set; }
    public string PathName { get; set; }
    public int NomenklCount { get; set; }

    public bool Equals(IDocCode x, IDocCode y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.DocCode == y.DocCode;
    }

    public int GetHashCode(IDocCode obj)
    {
        return obj.DocCode.GetHashCode();
    }

    public void LoadFromEntity(SD_82 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Name = entity.CAT_NAME;
        PathName = entity.CAT_PATH_NAME;
        ParentDC = entity.CAT_PARENT_DC;
        UpdateDate = entity.UpdateDate ?? DateTime.Now;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((NomenklGroup) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }

    public void LoadFromCache()
    {
       
    }
    [Display(AutoGenerateField = false, Name = "Посл.обновление")]
    public DateTime UpdateDate { get; set; }
}

[MetadataType(typeof(DataAnnotationsSD_82ViewModel))]

// ReSharper disable once InconsistentNaming
public class NomenklGroupViewModel : RSViewModelBase, IEntity<SD_82>
{
    private SD_82 myEntity;

    public NomenklGroupViewModel()
    {
        Entity = DefaultValue();
    }

    public NomenklGroupViewModel(SD_82 entity)
    {
        Entity = entity ?? DefaultValue();
    }

    public int NomenklCount { set; get; }

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

    public override string Name
    {
        get => Entity.CAT_NAME;
        set
        {
            if (Entity.CAT_NAME == value) return;
            Entity.CAT_NAME = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CAT_PARENT_DC
    {
        get => Entity.CAT_PARENT_DC;
        set
        {
            if (Entity.CAT_PARENT_DC == value) return;
            Entity.CAT_PARENT_DC = value;
            RaisePropertyChanged();
        }
    }

    public string CAT_OKP
    {
        get => Entity.CAT_OKP;
        set
        {
            if (Entity.CAT_OKP == value) return;
            Entity.CAT_OKP = value;
            RaisePropertyChanged();
        }
    }

    public string CAT_PATH_NAME
    {
        get => Entity.CAT_PATH_NAME;
        set
        {
            if (Entity.CAT_PATH_NAME == value) return;
            Entity.CAT_PATH_NAME = value;
            RaisePropertyChanged();
        }
    }

    public SD_82 SD_822
    {
        get => Entity.SD_822;
        set
        {
            if (Entity.SD_822 == value) return;
            Entity.SD_822 = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public override decimal? ParentDC
    {
        get => Entity.CAT_PARENT_DC;
        set
        {
            if (Entity.CAT_PARENT_DC == value) return;
            Entity.CAT_PARENT_DC = value;
            RaisePropertyChanged();
        }
    }

    public bool IsAccessRight { get; set; }

    public SD_82 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_82 DefaultValue()
    {
        return new SD_82
        {
            DOC_CODE = -1
        };
    }

    public List<SD_82> LoadList()
    {
        throw new NotImplementedException();
    }

    public virtual void Save(SD_82 doc)
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

    public void UpdateFrom(SD_82 ent)
    {
        Name = ent.CAT_NAME;
        CAT_PARENT_DC = ent.CAT_PARENT_DC;
        CAT_OKP = ent.CAT_OKP;
        CAT_PATH_NAME = ent.CAT_PATH_NAME;
        SD_822 = ent.SD_822;
    }

    public void UpdateTo(SD_82 ent)
    {
        ent.CAT_NAME = Name;
        ent.CAT_PARENT_DC = CAT_PARENT_DC;
        ent.CAT_OKP = CAT_OKP;
        ent.CAT_PATH_NAME = CAT_PATH_NAME;
        ent.SD_822 = SD_822;
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public SD_82 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public SD_82 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual SD_82 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual SD_82 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return Name;
    }
}

public class DataAnnotationsSD_82ViewModel : DataAnnotationForFluentApiBase, IMetadataProvider<NomenklGroupViewModel>
{
    void IMetadataProvider<NomenklGroupViewModel>.BuildMetadata(MetadataBuilder<NomenklGroupViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование").ReadOnly();
        builder.Property(_ => _.NomenklCount).AutoGenerated().DisplayName("Кол-во ном.").ReadOnly();
    }
}
