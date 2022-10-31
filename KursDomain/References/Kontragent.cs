using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Data;
using KursDomain.Annotations;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq} {Currency,nq}")]
public class Kontragent : IKontragent, IDocCode, IDocGuid, IName, IEqualityComparer<IDocCode>
{
    public Kontragent()
    {
        DocCode = -1;
    }

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

    public string ShortName { get; set; }
    public string FullName { get; set; }
    public IKontragentCategory Category { get; set; }
    public string INN { get; set; }
    public string KPP { get; set; }
    public string Director { get; set; }
    public string GlavBuh { get; set; }
    public bool IsDeleted { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string OKPO { get; set; }
    public string OKONH { get; set; }
    public bool IsPersona { get; set; }
    public string Passport { get; set; }
    public IEmployee Employee { get; set; }
    public IClientCategory ClientCategory { get; set; }
    public bool IsBalans { get; set; }
    public ICurrency Currency { get; set; }

    public decimal StartSumma { get; set; }
    public DateTime StartBalans { get; set; }
    public string EMail { get; set; }
    public IEmployee ResponsibleTabelNumber { get; set; }
    public IRegion Region { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Контрагент: {Name}({Currency})";

    public void LoadFromEntity(SD_43 entity, IReferencesCache refCache)
    {
        if(entity == null) return;
        DocCode = entity.DOC_CODE;
        Name = entity.NAME;
        Category = refCache?.GetKontragentCategory(entity.EG_ID);
        Notes = entity.NOTES;
        Currency = refCache?.GetCurrency(entity.VALUTA_DC);
        IsBalans = entity.FLAG_BALANS == 1;
        IsDeleted = entity.DELETED == 1;
        Id = entity.Id;
    }

    public Guid Id { get; set; }
}

public class KontragentViewModel : IKontragent, IDocCode, IDocGuid, IName, IEqualityComparer<IDocCode>, INotifyPropertyChanged,
    IDocumentState
{
    private readonly SD_43 Entity;
    private readonly IReferencesCache myReferenceCache;

    public KontragentViewModel(SD_43 entity, IReferencesCache refCache)
    {
        Entity = entity;
        myReferenceCache = refCache;
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

    public RowStatus RowStatus { get; set; }

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

    public string ShortName
    {
        get => Entity.NAME;
        set
        {
            if (Entity.NAME == value) return;
            Entity.NAME = value;
            OnPropertyChanged();
        }
    }

    public string FullName
    {
        get => Entity.NAME_FULL;
        set
        {
            if (Entity.NAME_FULL == value) return;
            Entity.NAME_FULL = value;
            OnPropertyChanged();
        }
    }

    public IKontragentCategory Category
    {
        get => myReferenceCache.GetKontragentCategory(Entity.EG_ID ?? 0);
        set
        {
            if (myReferenceCache.GetKontragentCategory(Entity.EG_ID ?? 0).Equals(value)) return;
            Entity.EG_ID = value?.Id;
            OnPropertyChanged();
        }
    }

    public string INN
    {
        get => Entity.INN;
        set
        {
            if (Entity.INN == value) return;
            Entity.INN = value;
            OnPropertyChanged();
        }
    }

    public string KPP
    {
        get => Entity.KPP;
        set
        {
            if (Entity.KPP == value) return;
            Entity.KPP = value;
            OnPropertyChanged();
        }
    }

    public string Director
    {
        get => Entity.HEADER;
        set
        {
            if (Entity.HEADER == value) return;
            Entity.HEADER = value;
            OnPropertyChanged();
        }
    }

    public string GlavBuh
    {
        get => Entity.GLAVBUH;
        set
        {
            if (Entity.GLAVBUH == value) return;
            Entity.GLAVBUH = value;
            OnPropertyChanged();
        }
    }

    public bool IsDeleted
    {
        get => Entity.DELETED == 1;
        set
        {
            if (Entity.DELETED == 1 == value) return;
            Entity.DELETED = (short?)(value ? 1 : 0);
            OnPropertyChanged();
        }
    }

    public string Address
    {
        get => Entity.ADDRESS;
        set
        {
            if (Entity.ADDRESS == value) return;
            Entity.ADDRESS = value;
            OnPropertyChanged();
        }
    }

    public string Phone
    {
        get => Entity.TEL;
        set
        {
            if (Entity.TEL == value) return;
            Entity.TEL = value;
            OnPropertyChanged();
        }
    }

    public string OKPO
    {
        get => Entity.OKPO;
        set
        {
            if (Entity.OKPO == value) return;
            Entity.OKPO = value;
            OnPropertyChanged();
        }
    }

    public string OKONH
    {
        get => Entity.OKONH;
        set
        {
            if (Entity.OKONH == value) return;
            Entity.OKONH = value;
            OnPropertyChanged();
        }
    }

    public bool IsPersona
    {
        get => Entity.FLAG_0UR_1PHYS == 1;
        set
        {
            if (Entity.FLAG_0UR_1PHYS == 1 == value) return;
            Entity.FLAG_0UR_1PHYS = (short?)(value ? 1 : 0);
            OnPropertyChanged();
        }
    }

    public string Passport
    {
        get => Entity.PASSPORT;
        set
        {
            if (Entity.PASSPORT == value) return;
            Entity.PASSPORT = value;
            OnPropertyChanged();
        }
    }

    public IEmployee Employee
    {
        get => myReferenceCache.GetEmployee(Entity.TABELNUMBER);
        set
        {
            if (myReferenceCache.GetEmployee(Entity.TABELNUMBER).Equals(value)) return;
            Entity.TABELNUMBER = value?.TabelNumber;
            OnPropertyChanged();
        }
    }

    public IClientCategory ClientCategory
    {
        get => myReferenceCache.GetClientCategory(Entity.CLIENT_CATEG_DC);
        set
        {
            if (myReferenceCache.GetEmployee(Entity.CLIENT_CATEG_DC).Equals(value)) return;
            Entity.CLIENT_CATEG_DC = (value as IDocCode)?.DocCode;
            OnPropertyChanged();
        }
    }

    public bool IsBalans
    {
        get => Entity.FLAG_BALANS == 1;
        set
        {
            if (Entity.FLAG_BALANS == 1 == value) return;
            Entity.FLAG_BALANS = (short?)(value ? 1 : 0);
            OnPropertyChanged();
        }
    }

    public ICurrency Currency
    {
        get => myReferenceCache.GetCurrency(Entity.VALUTA_DC);
        set
        {
            if (myReferenceCache.GetEmployee(Entity.VALUTA_DC).Equals(value)) return;
            Entity.VALUTA_DC = (value as IDocCode)?.DocCode;
            OnPropertyChanged();
        }
    }

    public decimal StartSumma
    {
        get => Entity.START_SUMMA ?? 0;
        set
        {
            if (Entity.START_SUMMA == value) return;
            Entity.START_SUMMA = value;
            OnPropertyChanged();
        }
    }

    public DateTime StartBalans
    {
        get => Entity.START_BALANS ?? DateTime.Now;
        set
        {
            if (Entity.START_BALANS == value) return;
            Entity.START_BALANS = value;
            OnPropertyChanged();
        }
    }

    public string EMail
    {
        get => Entity.E_MAIL;
        set
        {
            if (Entity.E_MAIL == value) return;
            Entity.E_MAIL = value;
            OnPropertyChanged();
        }
    }

    public IEmployee ResponsibleTabelNumber
    {
        get => myReferenceCache.GetEmployee(Entity.OTVETSTV_LICO);
        set
        {
            if (myReferenceCache.GetEmployee(Entity.OTVETSTV_LICO).Equals(value)) return;
            Entity.OTVETSTV_LICO = value?.TabelNumber;
            OnPropertyChanged();
        }
    }

    public IRegion Region
    {
        get => myReferenceCache.GetRegion(Entity.REGION_DC);
        set
        {
            if (myReferenceCache.GetEmployee(Entity.REGION_DC).Equals(value)) return;
            Entity.REGION_DC = (value as IDocCode)?.DocCode;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => Entity.NAME;
        set
        {
            if (Entity.NAME == value) return;
            Entity.NAME = value;
            OnPropertyChanged();
        }
    }

    public string Notes
    {
        get => Entity.NOTES;
        set
        {
            if (Entity.NOTES == value) return;
            Entity.NOTES = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get => Name;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Guid Id { get => Entity.Id;
        set
        {
            if (Entity.Id == value) return;
            Entity.Id = value;
            OnPropertyChanged();
        } }
}
