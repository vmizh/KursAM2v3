using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Data;
using KursDomain.Annotations;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]
[DebuggerDisplay("{DocCode,nq}/{Id,nq} {Name,nq}")]
public class Currency : ICurrency, IDocCode, IName, IDocGuid, IEqualityComparer<Currency>
{
    private string _Code;
    private decimal _DocCode;
    private string _FullName;
    private bool _IsActive;
    private string _Name;
    private string _Notes;

    public string Code
    {
        get => _Code;
        set
        {
            if (_Code == value) return;
            _Code = value;
        }
    }

    public string FullName
    {
        get => _FullName;
        set
        {
            if (_FullName == value) return;
            _FullName = value;
        }
    }

    public bool IsActive
    {
        get => _IsActive;
        set
        {
            if (_IsActive == value) return;
            _IsActive = value;
        }
    }

    public decimal DocCode
    {
        get => _DocCode;
        set
        {
            if (_DocCode == value) return;
            _DocCode = value;
        }
    }

    public bool Equals(Currency crs1, Currency crs2)
    {
        if (crs2 == null && crs1 == null)
            return true;
        if (crs1 == null || crs2 == null)
            return false;
        return crs1.DocCode == crs2.DocCode;
    }

    public int GetHashCode(Currency obj)
    {
        return (DocCode + Name).GetHashCode();
    }

    public string Name
    {
        get => _Name;
        set
        {
            if (_Name == value) return;
            _Name = value;
        }
    }

    public string Notes
    {
        get => _Notes;
        set
        {
            if (_Notes == value) return;
            _Notes = value;
        }
    }

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

    }

    public Guid Id { get; set; }
    public string NalogName { get; set; }
    public string NalogCode { get; set; }
}

public class CurrencyViewModel : ICurrency, IDocCode, IName, INotifyPropertyChanged,
    IEqualityComparer<CurrencyViewModel>
{
    public readonly SD_301 Entity;

    public CurrencyViewModel(SD_301 entity)
    {
        Entity = entity;
    }

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

    public bool IsMain
    {
        get => Entity.CRS_MAIN_CURRENCY == 1;
        set
        {
            if ((Entity.CRS_MAIN_CURRENCY == 1) == value) return;
            Entity.CRS_MAIN_CURRENCY = (short)(value ? 1 : 0);
            OnPropertyChanged();
        }
    }

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

    public string Notes
    {
        get => string.Empty;
        set { }
    }

    public string Description => $"Валюта: {Name}({FullName})";
    public RowStatus State { get; set; } =  RowStatus.NotEdited;

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (State != RowStatus.NewRow)
            State = RowStatus.Edited;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
