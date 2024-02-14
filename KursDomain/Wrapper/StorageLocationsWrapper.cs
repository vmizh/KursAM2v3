using System;
using System.ComponentModel.DataAnnotations;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.References;
using KursDomain.Wrapper.Base;

namespace KursDomain.Wrapper;

public class StorageLocationsWrapper : BaseWrapper<StorageLocations>, IEquatable<StorageLocationsWrapper>
{
    public StorageLocationsWrapper(StorageLocations model) : base(model)
    {
    }

    [Display(AutoGenerateField = false)] public override Guid Id => Model.Id;

    [Display(AutoGenerateField = true, Name = "Наименование")]
    public override string Name
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Регион")]
    [GridEditor(TemplateKey = "RegionLookupEditTemplate")]  
    public Region Region
    {
        get => GlobalOptions.ReferencesCache.GetRegion(Model.RegionDC) as Region;
        set
        {
            if (Model.RegionDC == (value?.DocCode ?? 0)) return;
            SetValue(value?.DocCode ?? 0,nameof(StorageLocations.RegionDC));
        }
    }

    [Display(AutoGenerateField = true, Name = "Склад")]
    public Warehouse Warehouse
    {
        get => GlobalOptions.ReferencesCache.GetWarehouse(Model.WarehouseDC) as Warehouse;
        set
        {
            if (Model.WarehouseDC == (value?.DocCode ?? 0)) return;
            SetValue(value?.DocCode ?? 0,nameof(StorageLocations.WarehouseDC));
        }
    }

    [Display(AutoGenerateField = true, Name = "Ответственный")]
    public Employee Employee
    {
        get => GlobalOptions.ReferencesCache.GetEmployee(Model.OwnerDC) as Employee;
        set
        {
            if (Model.OwnerDC == (value?.DocCode ?? 0)) return;
            SetValue(value?.DocCode ?? 0,nameof(StorageLocations.OwnerDC));
            //RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Примечания")]
    public override string Note
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Удален")]
    public bool? IsDeleted
    {
        get => GetValue<bool?>();
        set => SetValue(value);
    }

    public bool Equals(StorageLocationsWrapper other)
    {
        if (other == null) return false;
        return Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((StorageLocationsWrapper)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(StorageLocationsWrapper left, StorageLocationsWrapper right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(StorageLocationsWrapper left, StorageLocationsWrapper right)
    {
        return !Equals(left, right);
    }
}
