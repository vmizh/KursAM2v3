﻿using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace KursDomain.Documents.Periods;

public class PERIOD_CLOSED_EXCLUDEViewModel : RSViewModelBase, IEntity<PERIOD_CLOSED_EXCLUDE>
{
    private PERIOD_CLOSED_EXCLUDE myEntity;

    public PERIOD_CLOSED_EXCLUDEViewModel()
    {
        Entity = new PERIOD_CLOSED_EXCLUDE();
    }

    public PERIOD_CLOSED_EXCLUDEViewModel(PERIOD_CLOSED_EXCLUDE entity)
    {
        Entity = entity ?? new PERIOD_CLOSED_EXCLUDE();
    }

    public override Guid Id
    {
        get => Entity.ID;
        set
        {
            if (Entity.ID == value) return;
            Entity.ID = value;
            RaisePropertyChanged();
        }
    }

    public Guid CLOSED_ID
    {
        get => Entity.CLOSED_ID;
        set
        {
            if (Entity.CLOSED_ID == value) return;
            Entity.CLOSED_ID = value;
            RaisePropertyChanged();
        }
    }

    public Guid USER_GROUP_ID
    {
        get => Entity.USER_GROUP_ID;
        set
        {
            if (Entity.USER_GROUP_ID == value) return;
            Entity.USER_GROUP_ID = value;
            RaisePropertyChanged();
        }
    }

    public DateTime DateExclude
    {
        get => Entity.DateExclude;
        set
        {
            if (Entity.DateExclude == value) return;
            Entity.DateExclude = value;
            RaisePropertyChanged();
        }
    }

    public override string Note
    {
        get => Entity.Note;
        set
        {
            if (Entity.Note == value) return;
            Entity.Note = value;
            RaisePropertyChanged();
        }
    }

    public DateTime DateFrom
    {
        get => Entity.DateFrom;
        set
        {
            if (Entity.DateFrom == value) return;
            Entity.DateFrom = value;
            RaisePropertyChanged();
        }
    }

    public PERIOD_CLOSED PERIOD_CLOSED
    {
        get => Entity.PERIOD_CLOSED;
        set
        {
            if (Entity.PERIOD_CLOSED == value) return;
            Entity.PERIOD_CLOSED = value;
            RaisePropertyChanged();
        }
    }

    public PERIOD_GROUPS PERIOD_GROUPS
    {
        get => Entity.PERIOD_GROUPS;
        set
        {
            if (Entity.PERIOD_GROUPS == value) return;
            Entity.PERIOD_GROUPS = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public PERIOD_CLOSED_EXCLUDE Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public PERIOD_CLOSED_EXCLUDE DefaultValue()
    {
        return new PERIOD_CLOSED_EXCLUDE();
    }

    public List<PERIOD_CLOSED_EXCLUDE> LoadList()
    {
        throw new NotImplementedException();
    }

    public virtual PERIOD_CLOSED_EXCLUDE Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual PERIOD_CLOSED_EXCLUDE Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public virtual void Save(PERIOD_CLOSED_EXCLUDE doc)
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

    public void UpdateFrom(PERIOD_CLOSED_EXCLUDE ent)
    {
        //ID = ent.ID;
        CLOSED_ID = ent.CLOSED_ID;
        USER_GROUP_ID = ent.USER_GROUP_ID;
        DateExclude = ent.DateExclude;
        Note = ent.Note;
        DateFrom = ent.DateFrom;
        PERIOD_CLOSED = ent.PERIOD_CLOSED;
        PERIOD_GROUPS = ent.PERIOD_GROUPS;
    }

    public void UpdateTo(PERIOD_CLOSED_EXCLUDE ent)
    {
        //ent.ID = ID;
        ent.CLOSED_ID = CLOSED_ID;
        ent.USER_GROUP_ID = USER_GROUP_ID;
        ent.DateExclude = DateExclude;
        ent.Note = Note;
        ent.DateFrom = DateFrom;
        ent.PERIOD_CLOSED = PERIOD_CLOSED;
        ent.PERIOD_GROUPS = PERIOD_GROUPS;
    }
}
