﻿using System;
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

[DebuggerDisplay("{DocCode,nq}/{Id} {Name,nq}")]
public class SDRSchet : ISDRSchet, IDocCode, IName, IEquatable<SDRSchet>, IComparable, ICache
{
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    [Display(AutoGenerateField = false, Name = "DocCode")]
    public decimal DocCode { get; set; }

    public bool Equals(SDRSchet other)
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
    public string Description => $"Счет дох/расх: {Name}";

    [Display(AutoGenerateField = true, Name = "Удален")]
    public bool IsDeleted { get; set; }

    public decimal? SDRStateDC { get; set; }

    [Display(AutoGenerateField = true, Name = "Статья")]
    [JsonIgnore]
    public ISDRState SDRState { get; set; }

    [Display(AutoGenerateField = true, Name = "Под отчет")]
    public bool IsPodOtchet { get; set; }

    [Display(AutoGenerateField = true, Name = "Зарплата")]
    public bool IsEmployeePayment { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_303 entity, IReferencesCache refCache)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        IsDeleted = entity.SHPZ_DELETED == 1;
        SDRState = refCache?.GetSDRState(entity.SHPZ_STATIA_DC);
        IsPodOtchet = entity.SHPZ_PODOTCHET == 1;
        IsEmployeePayment = entity.SHPZ_1ZARPLATA_0NO == 1;
        Name = entity.SHPZ_NAME;
        UpdateDate = entity.UpdateDate ?? DateTime.Now;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((SDRSchet) obj);
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

/// <summary>
///     Счет доходов и расходов
/// </summary>
// ReSharper disable once InconsistentNaming
[MetadataType(typeof(DataAnnotationsSDRSchet))]
public class SDRSchetViewModel : RSViewModelBase, IEntity<SD_303>
{
    private SD_303 myEntity;


    private string mySHPZ_SHIRF;

    public SDRSchetViewModel()
    {
        Entity = DefaultValue();
    }

    public SDRSchetViewModel(SD_303 entity)
    {
        Entity = entity ?? new SD_303 {DOC_CODE = -1};
    }

    public new string Name
    {
        get => Entity.SHPZ_NAME;
        set
        {
            if (Entity.SHPZ_NAME == value) return;
            Entity.SHPZ_NAME = value;
            RaisePropertyChanged();
        }
    }

    public string SHPZ_SHIRF
    {
        get => mySHPZ_SHIRF;
        set
        {
            if (mySHPZ_SHIRF == value) return;
            mySHPZ_SHIRF = value;
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

    public decimal? SHPZ_STATIA_DC
    {
        get => Entity.SHPZ_STATIA_DC;
        set
        {
            if (Entity.SHPZ_STATIA_DC == value) return;
            Entity.SHPZ_STATIA_DC = value;
            RaisePropertyChanged();
        }
    }

    public bool IsAccessRight { get; set; }

    public SD_303 DefaultValue()
    {
        return new SD_303 {DOC_CODE = -1};
    }

    public SD_303 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public List<SD_303> LoadList()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return Name;
    }
}

public class DataAnnotationsSDRSchet : DataAnnotationForFluentApiBase, IMetadataProvider<SDRSchetViewModel>
{
    void IMetadataProvider<SDRSchetViewModel>.BuildMetadata(MetadataBuilder<SDRSchetViewModel> builder)
    {
        SetNotAutoGenerated(builder);

        //builder.Property(_ => _.Entity).NotAutoGenerated();
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование").Required();
        builder.Property(_ => _.SHPZ_SHIRF).AutoGenerated().DisplayName("Шифр").Required();
    }
}
