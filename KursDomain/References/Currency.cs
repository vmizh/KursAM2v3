using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Data;
using KursDomain.Annotations;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

[DataContract]
[DebuggerDisplay("{DocCode,nq}/{Id,nq} {Name,nq}")]
public class Currency : ICurrency, IDocCode, IName, IDocGuid, IEquatable<Currency>, IComparable, ICache
{

    public Currency()
    {
        LoadFromCache();
    }
    private string _Code;
    private decimal _DocCode;
    private string _FullName;
    private bool _IsActive;
    private string _Name;
    private string _Notes;

    [DataMember]
    [Display(AutoGenerateField = false, Name = "Код")]
    public string NalogName { get; set; }
    [DataMember]
    [Display(AutoGenerateField = false, Name = "Код")]
    public string NalogCode { get; set; }

    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : string.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    [DataMember]
    [Display(AutoGenerateField = true, Name = "Код")]
    public string Code
    {
        get => _Code;
        set
        {
            if (string.Equals(_Code, value, StringComparison.InvariantCulture)) return;
            _Code = value;
        }
    }
    [DataMember]
    [Display(AutoGenerateField = true, Name = "Полное наименование")]
    public string FullName
    {
        get => _FullName;
        set
        {
            if (string.Equals(_FullName, value, StringComparison.InvariantCulture)) return;
            _FullName = value;
        }
    }
    [DataMember]
    [Display(AutoGenerateField = true, Name = "Активная")]
    public bool IsActive
    {
        get => _IsActive;
        set
        {
            if (_IsActive.Equals(value)) return;
            _IsActive = value;
        }
    }
    [DataMember]
    [Display(AutoGenerateField = false, Name = "DocCode")]
    public decimal DocCode
    {
        get => _DocCode;
        set
        {
            if (_DocCode.Equals(value)) return;
            _DocCode = value;
        }
    }
    [DataMember]
    [Display(AutoGenerateField = false, Name = "Id")]
    public Guid Id { get; set; }

    public bool Equals(Currency other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _DocCode == other._DocCode;
    }
    [DataMember]
    [Display(AutoGenerateField = true, Name = "Наименование")]
    public string Name
    {
        get => _Name;
        set
        {
            if (string.Equals(_Name, value, StringComparison.InvariantCulture)) return;
            _Name = value;
        }
    }
    [DataMember]
    [Display(AutoGenerateField = true, Name = "Примечание")]
    public string Notes
    {
        get => _Notes;
        set
        {
            if (string.Equals(_Notes, value, StringComparison.InvariantCulture)) return;
            _Notes = value;
        }
    }
    [DataMember]
    [Display(AutoGenerateField = false, Name = "Описание")]
    [JsonIgnore]
    public string Description => $"Валюта: {Name} {FullName}";

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_301 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            Id = Guid.Empty;
            return;
        }

        DocCode = entity.DOC_CODE;
        Name = entity.CRS_SHORTNAME;
        Code = entity.CRS_CODE;
        FullName = entity.CRS_NAME;
        IsActive = entity.CRS_ACTIVE == 1;
        Id = entity.Id;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Currency)obj);
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return _DocCode.GetHashCode();
    }

    public void LoadFromCache()
    {
        
    }

    public static bool operator ==(Currency left, Currency right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Currency left, Currency right)
    {
        return !Equals(left, right);
    }
}

public class CurrencyViewModel : ICurrency, IDocCode, IName, INotifyPropertyChanged,
    IEqualityComparer<CurrencyViewModel>
{
    public readonly SD_301 Entity;


    public CurrencyViewModel(SD_301 entity)
    {
        Entity = entity;
    }

    [Display(AutoGenerateField = false, Name = "Id")]
    public Guid Id
    {
        get => Entity.Id;
        set
        {
            if (Entity.Id == value) return;
            Entity.Id = value;
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Основная")]
    public bool IsMain
    {
        get => Entity.CRS_MAIN_CURRENCY == 1;
        set
        {
            if (Entity.CRS_MAIN_CURRENCY == 1 == value) return;
            Entity.CRS_MAIN_CURRENCY = (short)(value ? 1 : 0);
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = false, Name = "Код")]
    public RowStatus State { get; set; } = RowStatus.NotEdited;

    [Display(AutoGenerateField = true, Name = "Код")]
    public string Code
    {
        get => Entity.CRS_CODE;
        set
        {
            if (Entity.CRS_CODE == value) return;
            Entity.CRS_CODE = value;
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Полное имя")]
    public string FullName
    {
        get => Entity.CRS_NAME;
        set
        {
            if (Entity.CRS_NAME == value) return;
            Entity.CRS_NAME = value;
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Активная")]
    public bool IsActive
    {
        get => Entity.CRS_ACTIVE == 1;
        set
        {
            if (Entity.CRS_ACTIVE == 1 == value) return;
            Entity.CRS_ACTIVE = value ? 1 : 0;
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = false, Name = "DocCode")]
    public decimal DocCode
    {
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            OnPropertyChanged();
        }
    }

    public bool Equals(CurrencyViewModel crs1, CurrencyViewModel crs2)
    {
        if (crs2 == null && crs1 == null)
            return true;
        if (crs1 == null || crs2 == null)
            return false;
        return crs1.DocCode == crs2.DocCode;
    }

    public int GetHashCode(CurrencyViewModel obj)
    {
        return obj.Entity != null ? obj.Entity.GetHashCode() : 0;
    }

    [Display(AutoGenerateField = true, Name = "Наименование")]
    public string Name
    {
        get => Entity.CRS_SHORTNAME;
        set
        {
            if (Entity.CRS_SHORTNAME == value) return;
            Entity.CRS_SHORTNAME = value;
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Примечания")]
    public string Notes
    {
        get => string.Empty;
        set { }
    }

    [Display(AutoGenerateField = false, Name = "Описание")]
    public string Description => $"Валюта: {Name}({FullName})";

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (State != RowStatus.NewRow)
            State = RowStatus.Edited;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
