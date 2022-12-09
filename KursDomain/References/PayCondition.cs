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

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class PayCondition : IPayCondition, IDocCode, IName, IEquatable<PayCondition>, IComparable
{
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    [Display(AutoGenerateField = false, Name = "Код")]
    public decimal DocCode { get; set; }

    public bool Equals(PayCondition other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    [Display(AutoGenerateField = true, Name = "Наименование")]
    public string Name { get; set; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public string Notes { get; set; }

    [Display(AutoGenerateField = false, Name = "Опичсание")]
    public string Description => $"Условие оплаты: {Name}";

    [Display(AutoGenerateField = true, Name = "По умолчанию")]
    public bool IsDefault { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_179 entity)
    {
        DocCode = entity.DOC_CODE;
        Name = entity.PT_NAME;
        IsDefault = entity.DEFAULT_VALUE == 1;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PayCondition) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }
}

/// <summary>
///     Условие оплаты SD_179
/// </summary>
[MetadataType(typeof(UsagePay_FluentAPI))]
public class PayConditionViewModel : RSViewModelBase, IEntity<SD_179>
{
    private SD_179 myEntity;

    public PayConditionViewModel()
    {
        Entity = DefaultValue();
    }

    public PayConditionViewModel(SD_179 entity)
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
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public string PT_NAME
    {
        get => Entity.PT_NAME;
        set
        {
            if (Entity.PT_NAME == value) return;
            Entity.PT_NAME = value;
            RaisePropertyChanged();
        }
    }

    public override string Name
    {
        get => Entity.PT_NAME;
        set
        {
            if (Entity.PT_NAME == value) return;
            Entity.PT_NAME = value;
            RaisePropertyChanged();
        }
    }

    public short? DEFAULT_VALUE
    {
        get => Entity.DEFAULT_VALUE;
        set
        {
            if (Entity.DEFAULT_VALUE == value) return;
            Entity.DEFAULT_VALUE = value;
            RaisePropertyChanged();
        }
    }

    public double? PT_DAYS_OPL
    {
        get => Entity.PT_DAYS_OPL;
        set
        {
            if (Entity.PT_DAYS_OPL == value) return;
            Entity.PT_DAYS_OPL = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public SD_179 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_179 DefaultValue()
    {
        return new SD_179
        {
            DOC_CODE = -1
        };
    }

    public List<SD_179> LoadList()
    {
        throw new NotImplementedException();
    }

    public SD_179 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public virtual void Save(SD_179 doc)
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

    public void UpdateFrom(SD_179 ent)
    {
        PT_NAME = ent.PT_NAME;
        DEFAULT_VALUE = ent.DEFAULT_VALUE;
        PT_DAYS_OPL = ent.PT_DAYS_OPL;
    }

    public void UpdateTo(SD_179 ent)
    {
        ent.PT_NAME = PT_NAME;
        ent.DEFAULT_VALUE = DEFAULT_VALUE;
        ent.PT_DAYS_OPL = PT_DAYS_OPL;
    }

    public virtual SD_179 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual SD_179 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public SD_179 Load(decimal dc)
    {
        throw new NotImplementedException();
    }
}

public class UsagePay_FluentAPI : DataAnnotationForFluentApiBase, IMetadataProvider<PayConditionViewModel>
{
    void IMetadataProvider<PayConditionViewModel>.BuildMetadata(
        MetadataBuilder<PayConditionViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(x => x.Name).AutoGenerated()
            .DisplayName("Наименование");
        builder.Property(x => x.PT_DAYS_OPL).AutoGenerated()
            .DisplayName("Дней на оплату").DisplayFormatString("n0");
    }
}
