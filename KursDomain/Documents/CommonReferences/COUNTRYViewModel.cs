using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.ViewModel.Base;
using Data;

namespace KursDomain.Documents.CommonReferences;

[SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter")]
public class COUNTRYViewModel : RSViewModelBase, IEntity<COUNTRY>
{
    private COUNTRY myEntity;

    public COUNTRYViewModel()
    {
        Entity = DefaultValue();
    }

    public COUNTRYViewModel(COUNTRY entity)
    {
        Entity = entity ?? DefaultValue();
    }

    public string ID
    {
        get => Entity.ID;
        set
        {
            if (Entity.ID == value) return;
            Entity.ID = value;
            RaisePropertyChanged();
        }
    }

    public string ALPHA2
    {
        get => Entity.ALPHA2;
        set
        {
            if (Entity.ALPHA2 == value) return;
            Entity.ALPHA2 = value;
            RaisePropertyChanged();
        }
    }

    public string ALPHA3
    {
        get => Entity.ALPHA3;
        set
        {
            if (Entity.ALPHA3 == value) return;
            Entity.ALPHA3 = value;
            RaisePropertyChanged();
        }
    }

    public string ISO
    {
        get => Entity.ISO;
        set
        {
            if (Entity.ISO == value) return;
            Entity.ISO = value;
            RaisePropertyChanged();
        }
    }

    public string FOREIGN_NAME
    {
        get => Entity.FOREIGN_NAME;
        set
        {
            if (Entity.FOREIGN_NAME == value) return;
            Entity.FOREIGN_NAME = value;
            RaisePropertyChanged();
        }
    }

    public string RUSSIAN_NAME
    {
        get => Entity.RUSSIAN_NAME;
        set
        {
            if (Entity.RUSSIAN_NAME == value) return;
            Entity.RUSSIAN_NAME = value;
            RaisePropertyChanged();
        }
    }

    public string NOTES
    {
        get => Entity.NOTES;
        set
        {
            if (Entity.NOTES == value) return;
            Entity.NOTES = value;
            RaisePropertyChanged();
        }
    }

    public string NAME
    {
        get => Entity.NAME;
        set
        {
            if (Entity.NAME == value) return;
            Entity.NAME = value;
            RaisePropertyChanged();
        }
    }

    public string LOCATION
    {
        get => Entity.LOCATION;
        set
        {
            if (Entity.LOCATION == value) return;
            Entity.LOCATION = value;
            RaisePropertyChanged();
        }
    }

    public string LOCATION_PRECISE
    {
        get => Entity.LOCATION_PRECISE;
        set
        {
            if (Entity.LOCATION_PRECISE == value) return;
            Entity.LOCATION_PRECISE = value;
            RaisePropertyChanged();
        }
    }

    public byte[] SMALL_FLAG
    {
        get => Entity.SMALL_FLAG;
        set
        {
            if (Entity.SMALL_FLAG == value) return;
            Entity.SMALL_FLAG = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public COUNTRY Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public COUNTRY DefaultValue()
    {
        return new COUNTRY
        {
            ID = Guid.NewGuid().ToString()
        };
    }

    public List<COUNTRY> LoadList()
    {
        throw new NotImplementedException();
    }

    public COUNTRY Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public virtual void Save(COUNTRY doc)
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

    public void UpdateFrom(COUNTRY ent)
    {
        ID = ent.ID;
        ALPHA2 = ent.ALPHA2;
        ALPHA3 = ent.ALPHA3;
        ISO = ent.ISO;
        FOREIGN_NAME = ent.FOREIGN_NAME;
        RUSSIAN_NAME = ent.RUSSIAN_NAME;
        NOTES = ent.NOTES;
        NAME = ent.NAME;
        LOCATION = ent.LOCATION;
        LOCATION_PRECISE = ent.LOCATION_PRECISE;
        SMALL_FLAG = ent.SMALL_FLAG;
    }

    public void UpdateTo(COUNTRY ent)
    {
        ent.ID = ID;
        ent.ALPHA2 = ALPHA2;
        ent.ALPHA3 = ALPHA3;
        ent.ISO = ISO;
        ent.FOREIGN_NAME = FOREIGN_NAME;
        ent.RUSSIAN_NAME = RUSSIAN_NAME;
        ent.NOTES = NOTES;
        ent.NAME = NAME;
        ent.LOCATION = LOCATION;
        ent.LOCATION_PRECISE = LOCATION_PRECISE;
        ent.SMALL_FLAG = SMALL_FLAG;
    }

    public virtual COUNTRY Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual COUNTRY Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public COUNTRY Load(decimal dc)
    {
        throw new NotImplementedException();
    }
}
