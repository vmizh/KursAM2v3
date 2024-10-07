using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

/// <summary>
///     Форма платежа
/// </summary>
[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class PayForm : IPayForm, IDocCode, IName, IEquatable<PayForm>, IComparable, ICache
{
    public PayForm()
    {
        LoadFromCache();
    }

    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    [Display(AutoGenerateField = false)] public decimal DocCode { get; set; }

    public bool Equals(PayForm other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    [Display(AutoGenerateField = true, Name = "Наименование")]
    public string Name { get; set; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public string Notes { get; set; }

    [Display(AutoGenerateField = false, Name = "Описание")]
    [JsonIgnore]
    public string Description => $"Форма оплаты: {Name}";

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_189 entity)
    {
        DocCode = entity.DOC_CODE;
        Name = entity.OOT_NAME;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PayForm) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }

    public void LoadFromCache()
    {
        
    }
}

/// <summary>
///     Форма оплаты SD_189
/// </summary>
[MetadataType(typeof(FormPay_FluentApi))]
public class PayFormViewModel : RSViewModelBase, IEntity<SD_189>
{
    private SD_189 myEntity;

    public PayFormViewModel()
    {
        Entity = DefaultValue();
    }

    public PayFormViewModel(SD_189 entity)
    {
        Entity = entity ?? DefaultValue();
    }

    public override decimal DocCode
    {
        get => Entity.DOC_CODE;
        set
        {
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }
    }

    public string OOT_NAME
    {
        get => Entity.OOT_NAME;
        set
        {
            if (Entity.OOT_NAME == value) return;
            Entity.OOT_NAME = value;
            RaisePropertyChanged();
        }
    }

    public override string Name
    {
        get => OOT_NAME;
        set
        {
            if (OOT_NAME == value) return;
            OOT_NAME = value;
            RaisePropertyChanged();
        }
    }

    public short? OOT_NALOG_S_PRODAZH
    {
        get => Entity.OOT_NALOG_S_PRODAZH;
        set
        {
            if (Entity.OOT_NALOG_S_PRODAZH == value) return;
            Entity.OOT_NALOG_S_PRODAZH = value;
            RaisePropertyChanged();
        }
    }

    public double? OOT_NALOG_PERCENT
    {
        get => Entity.OOT_NALOG_PERCENT;
        set
        {
            if (Math.Abs(Math.Round(Entity.OOT_NALOG_PERCENT ?? 0, 2) - Math.Round(value ?? 0, 2)) < 0.01) return;
            Entity.OOT_NALOG_PERCENT = value;
            RaisePropertyChanged();
        }
    }

    public decimal? OOT_USL_OPL_DEF_DC
    {
        get => Entity.OOT_USL_OPL_DEF_DC;
        set
        {
            if (Entity.OOT_USL_OPL_DEF_DC == value) return;
            Entity.OOT_USL_OPL_DEF_DC = value;
            RaisePropertyChanged();
        }
    }

    public SD_179 SD_179
    {
        get => Entity.SD_179;
        set
        {
            if (Entity.SD_179 == value) return;
            Entity.SD_179 = value;
            RaisePropertyChanged();
        }
    }

    public bool IsAccessRight { get; set; }

    public SD_189 DefaultValue()
    {
        return new SD_189 {DOC_CODE = -1};
    }

    public SD_189 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public List<SD_189> LoadList()
    {
        throw new NotImplementedException();
    }

    public SD_189 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public SD_189 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Save(SD_189 doc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(SD_189 ent)
    {
        OOT_NAME = ent.OOT_NAME;
        OOT_NALOG_S_PRODAZH = ent.OOT_NALOG_S_PRODAZH;
        OOT_NALOG_PERCENT = ent.OOT_NALOG_PERCENT;
        OOT_USL_OPL_DEF_DC = ent.OOT_USL_OPL_DEF_DC;
        SD_179 = ent.SD_179;
    }

    public void UpdateTo(SD_189 ent)
    {
        ent.OOT_NAME = OOT_NAME;
        ent.OOT_NALOG_S_PRODAZH = OOT_NALOG_S_PRODAZH;
        ent.OOT_NALOG_PERCENT = OOT_NALOG_PERCENT;
        ent.OOT_USL_OPL_DEF_DC = OOT_USL_OPL_DEF_DC;
        ent.SD_179 = SD_179;
    }
}

public class FormPay_FluentApi : DataAnnotationForFluentApiBase, IMetadataProvider<PayFormViewModel>
{
    void IMetadataProvider<PayFormViewModel>.BuildMetadata(
        MetadataBuilder<PayFormViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(x => x.Name).AutoGenerated()
            .DisplayName("Наименование");
    }
}
