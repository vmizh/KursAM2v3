﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core.ViewModel.Base;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences.Kontragent;

namespace KursDomain.References;

[DebuggerDisplay("{Id,nq} {Name,nq} {ParentId,nq}")]
public class KontragentGroup : IName, IKontragentGroup, IEquatable<KontragentGroup>, IComparable
{
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    public bool Equals(KontragentGroup other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description { get; set; }

    public bool Equals(KontragentGroup cat1, KontragentGroup cat2)
    {
        if (cat2 == null && cat1 == null)
            return true;
        if (cat1 == null || cat2 == null)
            return false;
        return cat1.Id == cat2.Id;
    }

    public int GetHashCode(KontragentGroup obj)
    {
        return (Id + Name).GetHashCode();
    }

    public void LoadFromEntity(UD_43 entity)
    {
        Id = entity.EG_ID;
        Name = entity.EG_NAME;
        IsDeleted = entity.EG_DELETED == 1;
        ParentId = entity.EG_PARENT_ID;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((KontragentGroup) obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}

public class KontragentGroupViewModel : RSViewModelBase, IEntity<UD_43>
{
    private UD_43 myEntity;

    public KontragentGroupViewModel()
    {
        Entity = DefaultValue();
    }

    public KontragentGroupViewModel(UD_43 entity)
    {
        Entity = entity ?? DefaultValue();
    }

    public int EG_ID
    {
        get => Entity.EG_ID;
        set
        {
            if (Entity.EG_ID == value) return;
            Entity.EG_ID = value;
            RaisePropertyChanged();
        }
    }

    public override decimal DocCode
    {
        get => EG_ID;
        set
        {
            if (EG_ID == value) return;
            EG_ID = (int) value;
            RaisePropertyChanged();
        }
    }

    public string EG_NAME
    {
        get => Entity.EG_NAME;
        set
        {
            if (Entity.EG_NAME == value) return;
            Entity.EG_NAME = value;
            RaisePropertyChanged();
        }
    }

    public override string Name
    {
        get => EG_NAME;
        set
        {
            if (EG_NAME == value) return;
            EG_NAME = value;
            RaisePropertyChanged();
        }
    }

    public int? EG_PARENT_ID
    {
        get => Entity.EG_PARENT_ID;
        set
        {
            if (Entity.EG_PARENT_ID == value) return;
            Entity.EG_PARENT_ID = value;
            RaisePropertyChanged();
        }
    }

    public override decimal? ParentDC
    {
        get => EG_PARENT_ID;
        set
        {
            if (EG_PARENT_ID == value) return;
            EG_PARENT_ID = (int?) value;
            RaisePropertyChanged();
        }
    }

    public short? EG_DELETED
    {
        get => Entity.EG_DELETED;
        set
        {
            if (Entity.EG_DELETED == value) return;
            Entity.EG_DELETED = value;
            RaisePropertyChanged();
        }
    }

    public bool IsDeleted
    {
        get => EG_DELETED == 1;
        set
        {
            if (EG_DELETED == 1 == value) return;
            EG_DELETED = (short?) (value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    public short? EG_BALANS_GROUP
    {
        get => Entity.EG_BALANS_GROUP;
        set
        {
            if (Entity.EG_BALANS_GROUP == value) return;
            Entity.EG_BALANS_GROUP = value;
            RaisePropertyChanged();
        }
    }

    public UD_43 UD_432
    {
        get => Entity.UD_432;
        set
        {
            if (Entity.UD_432 == value) return;
            Entity.UD_432 = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public UD_43 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public UD_43 DefaultValue()
    {
        return new UD_43
        {
            EG_ID = -1,
            EG_DELETED = 0
        };
    }

    public List<UD_43> LoadList()
    {
        throw new NotImplementedException();
    }

    public virtual void Save(UD_43 doc)
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

    public void UpdateFrom(UD_43 ent)
    {
        EG_ID = ent.EG_ID;
        EG_NAME = ent.EG_NAME;
        EG_PARENT_ID = ent.EG_PARENT_ID;
        EG_DELETED = ent.EG_DELETED;
        EG_BALANS_GROUP = ent.EG_BALANS_GROUP;
        UD_432 = ent.UD_432;
    }

    public void UpdateTo(UD_43 ent)
    {
        ent.EG_ID = EG_ID;
        ent.EG_NAME = EG_NAME;
        ent.EG_PARENT_ID = EG_PARENT_ID;
        ent.EG_DELETED = EG_DELETED;
        ent.EG_BALANS_GROUP = EG_BALANS_GROUP;
        ent.UD_432 = UD_432;
    }


    public UD_43 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public UD_43 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual UD_43 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual UD_43 Load(Guid id)
    {
        throw new NotImplementedException();
    }
}