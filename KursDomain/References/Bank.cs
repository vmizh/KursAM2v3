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

/// <summary>
///     Банк основные реквизиты
/// </summary>
[DebuggerDisplay("{DocCode,nq}/{Id} {Name,nq}")]
public class Bank : IBank, IDocCode, IName, IEquatable<Bank>, IComparable
{
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    [Display(AutoGenerateField = true, Name = "Корр.счет")]
    [MaxLength(30)]
    public string CorrAccount { get; set; }

    [MaxLength(10)]
    [Display(AutoGenerateField = true, Name = "БИК")]
    public string BIK { get; set; }

    [Display(AutoGenerateField = true, Name = "Адрес")]
    public string Address { get; set; }

    [Display(AutoGenerateField = true, Name = "Короткое имя")]
    public string NickName { get; set; }

    [Display(AutoGenerateField = false, Name = "Ид")]
    public decimal DocCode { get; set; }

    public bool Equals(Bank other)
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
    public string Description => $"Банк: {Name}";

    public override string ToString()
    {
        return Name;
    }


    public void LoadFromEntity(SD_44 entity)
    {
        if (entity == null)
        {
            DocCode = 1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Name = entity.BANK_NAME;
        Address = entity.ADDRESS;
        CorrAccount = entity.CORRESP_ACC;
        BIK = entity.POST_CODE;
        NickName = string.IsNullOrWhiteSpace(entity.BANK_NICKNAME) ? entity.BANK_NAME : entity.BANK_NICKNAME;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Bank) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }
}

[MetadataType(typeof(DataAnnotationsBankViewModel))]
public class BankViewModel : RSViewModelBase, IEntity<SD_44>
{
    private SD_44 myEntity;

    public BankViewModel()
    {
        Entity = DefaultValue();
    }

    public BankViewModel(SD_44 entity)
    {
        Entity = entity ?? DefaultValue();
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

    public override string Name
    {
        get => Entity.BANK_NAME;
        set
        {
            if (Entity.BANK_NAME == value) return;
            Entity.BANK_NAME = value;
            RaisePropertyChanged();
        }
    }

    public string CORRESP_ACC
    {
        get => Entity.CORRESP_ACC;
        set
        {
            if (Entity.CORRESP_ACC == value) return;
            Entity.CORRESP_ACC = value;
            RaisePropertyChanged();
        }
    }

    public string MFO_NEW
    {
        get => Entity.MFO_NEW;
        set
        {
            if (Entity.MFO_NEW == value) return;
            Entity.MFO_NEW = value;
            RaisePropertyChanged();
        }
    }

    public string MFO_OLD
    {
        get => Entity.MFO_OLD;
        set
        {
            if (Entity.MFO_OLD == value) return;
            Entity.MFO_OLD = value;
            RaisePropertyChanged();
        }
    }

    public string POST_CODE
    {
        get => Entity.POST_CODE;
        set
        {
            if (Entity.POST_CODE == value) return;
            Entity.POST_CODE = value;
            RaisePropertyChanged();
        }
    }

    public string SUB_CORR_ACC
    {
        get => Entity.SUB_CORR_ACC;
        set
        {
            if (Entity.SUB_CORR_ACC == value) return;
            Entity.SUB_CORR_ACC = value;
            RaisePropertyChanged();
        }
    }

    public string ADDRESS
    {
        get => Entity.ADDRESS;
        set
        {
            if (Entity.ADDRESS == value) return;
            Entity.ADDRESS = value;
            RaisePropertyChanged();
        }
    }

    public string BANK_NICKNAME
    {
        get => Entity.BANK_NICKNAME;
        set
        {
            if (Entity.BANK_NICKNAME == value) return;
            Entity.BANK_NICKNAME = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }
    public string Info => $"{Name} БИК {POST_CODE} К/счет {CORRESP_ACC}";

    public bool IsAccessRight { get; set; }

    public SD_44 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_44 DefaultValue()
    {
        return new SD_44
        {
            DOC_CODE = -1
        };
    }

    public List<SD_44> LoadList()
    {
        throw new NotImplementedException();
    }

    public virtual void Save(SD_44 doc)
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

    public void UpdateFrom(SD_44 ent)
    {
        Name = ent.BANK_NAME;
        CORRESP_ACC = ent.CORRESP_ACC;
        MFO_NEW = ent.MFO_NEW;
        MFO_OLD = ent.MFO_OLD;
        POST_CODE = ent.POST_CODE;
        SUB_CORR_ACC = ent.SUB_CORR_ACC;
        ADDRESS = ent.ADDRESS;
        BANK_NICKNAME = ent.BANK_NICKNAME;
    }

    public void UpdateTo(SD_44 ent)
    {
        ent.BANK_NAME = Name;
        ent.CORRESP_ACC = CORRESP_ACC;
        ent.MFO_NEW = MFO_NEW;
        ent.MFO_OLD = MFO_OLD;
        ent.POST_CODE = POST_CODE;
        ent.SUB_CORR_ACC = SUB_CORR_ACC;
        ent.ADDRESS = ADDRESS;
        ent.BANK_NICKNAME = BANK_NICKNAME;
    }

    // ReSharper disable once UnusedParameter.Global
    public SD_44 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public SD_44 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual SD_44 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual SD_44 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return Name;
    }
}

public class DataAnnotationsBankViewModel : DataAnnotationForFluentApiBase, IMetadataProvider<BankViewModel>
{
    void IMetadataProvider<BankViewModel>.BuildMetadata(MetadataBuilder<BankViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
        builder.Property(_ => _.CORRESP_ACC).AutoGenerated().DisplayName("Корреспондентский счет");
        builder.Property(_ => _.ADDRESS).AutoGenerated().DisplayName("Адрес");
        builder.Property(_ => _.POST_CODE).AutoGenerated().DisplayName("КПП");
    }
}
