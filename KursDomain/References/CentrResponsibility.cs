using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq} {ParentDC,nq}")]
public class CentrResponsibility : ICentrResponsibility, IDocCode, IName, IEqualityComparer<IDocCode>
{
    public string FullName { get; set; }
    public decimal? ParentDC { get; set; }
    public bool IsDeleted { get; set; }
    public decimal DocCode { get; set; }

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

    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"ЦО: {Name}";

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_40 entity)
    {
        DocCode = entity.DOC_CODE;
        Name = entity.CENT_NAME;
        FullName = entity.CENT_FULLNAME;
        ParentDC = entity.CENT_PARENT_DC;
        IsDeleted = entity.IS_DELETED == 1;
    }
}

[DataContract]
[MetadataType(typeof(DataAnnotationCentrOfResponsibility))]
public class CentrResponsibilityViewModel : RSViewModelBase, IEntity<SD_40>
{
    private SD_40 myEntity;

    public bool IsAccessRight { get; set; }

    [Display(AutoGenerateField = false)]
    public SD_40 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public List<SD_40> LoadList()
    {
        var data = GetAllFromDataBase();
        return data.Select(_ => _.Entity).ToList();
    }

    public override string ToString()
    {
        return Name;
    }

    public IList<CentrResponsibilityViewModel> GetAllFromDataBase()
    {
        return GlobalOptions.GetEntities().SD_40.Select(item => new CentrResponsibilityViewModel(item)).ToList();
    }

    #region Constructor

    public CentrResponsibilityViewModel()
    {
        Entity = DefaultValue();
    }

    public CentrResponsibilityViewModel(SD_40 entity)
    {
        Entity = entity ?? DefaultValue();
    }

    #endregion

    #region Properties

    [Display(AutoGenerateField = false)]
    public override decimal DocCode
    {
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value)
                return;
            Entity.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
    public decimal? CentParentDC
    {
        get => Entity.CENT_PARENT_DC;
        set
        {
            if (Entity.CENT_PARENT_DC == value)
                return;
            Entity.CENT_PARENT_DC = value;
            RaisePropertyChanged();
        }
    }

    //[Display(Name = "Наименование")]
    public override string Name
    {
        get => Entity.CENT_NAME;
        set
        {
            if (Entity.CENT_NAME == value)
                return;
            Entity.CENT_NAME = value;
            RaisePropertyChanged();
        }
    }

    [Display(Name = "Полное наименование")]
    public string FullName
    {
        get => Entity.CENT_FULLNAME;
        set
        {
            if (Entity.CENT_FULLNAME == value)
                return;
            Entity.CENT_FULLNAME = value;
            RaisePropertyChanged();
        }
    }

    [Display(Name = "Удален")]
    public bool IsDeleted
    {
        get => Entity.IS_DELETED == 1;
        set
        {
            if (Entity.IS_DELETED == (value ? 1 : 0))
                return;
            Entity.IS_DELETED = value ? 1 : 0;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)]
    public SD_40 SD_402
    {
        get => Entity.SD_402;
        set
        {
            if (Entity.SD_402 == value) return;
            Entity.SD_402 = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Methods

    public void UpdateFrom(SD_40 ent)
    {
        FullName = ent.CENT_FULLNAME;
        Name = ent.CENT_NAME;
        CentParentDC = ent.CENT_PARENT_DC;
        IsDeleted = ent.IS_DELETED == 1;
        SD_402 = ent.SD_402;
    }

    public SD_40 DefaultValue()
    {
        return new SD_40
        {
            DOC_CODE = -1
        };
    }

    #endregion
}

public class DataAnnotationCentrOfResponsibility : DataAnnotationForFluentApiBase,
    IMetadataProvider<CentrResponsibilityViewModel>
{
    void IMetadataProvider<CentrResponsibilityViewModel>.BuildMetadata(
        MetadataBuilder<CentrResponsibilityViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Краткое наименование");
        builder.Property(_ => _.FullName).AutoGenerated().DisplayName("Полное наименование");
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
    }
}
