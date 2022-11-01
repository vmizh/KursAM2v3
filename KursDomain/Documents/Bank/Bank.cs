using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace KursDomain.Documents.Bank;

[MetadataType(typeof(DataAnnotationsSD_44ViewModel))]
public class Bank : RSViewModelBase, IEntity<SD_44>
{
    private SD_44 myEntity;

    public Bank()
    {
        Entity = DefaultValue();
    }

    public Bank(SD_44 entity)
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

public class DataAnnotationsSD_44ViewModel : DataAnnotationForFluentApiBase, IMetadataProvider<Bank>
{
    void IMetadataProvider<Bank>.BuildMetadata(MetadataBuilder<Bank> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
        builder.Property(_ => _.CORRESP_ACC).AutoGenerated().DisplayName("Корреспондентский счет");
        builder.Property(_ => _.ADDRESS).AutoGenerated().DisplayName("Адрес");
        builder.Property(_ => _.POST_CODE).AutoGenerated().DisplayName("КПП");
    }
}
