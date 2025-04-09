using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Core.ViewModel.Base;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.References.RedisCache;

namespace KursDomain.References;

[DebuggerDisplay("{Id} {Name,nq}")]
public class ProjectGroup : IProjectGroup, IDocGuid, IName, IEquatable<ProjectGroup>, IComparable, ICache
{
    public DateTime UpdateDate { get; set; }

    public void LoadFromCache()
    {
    }

    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : string.Compare(Name, c.Name, StringComparison.Ordinal);
    }

    public Guid Id { get; set; }

    public bool Equals(ProjectGroup other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || Id.Equals(other.Id);
    }

    public string Description => $"Группа проектов {Name}";
    public string Notes { get; set; }
    public string Name { get; set; }
    public ObservableCollection<IProject> Projects { get; set; }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public void LoadFromEntity(Projects entity, IReferencesCache refCache)
    {
        if (entity == null)
        {
            Id = Guid.Empty;
            return;
        }

        Id = entity.Id;
        Name = entity.Name;
        Notes = entity.Note;
        UpdateDate = entity.UpdateDate ?? DateTime.Now;
    }

    public override string ToString()
    {
        return Name;
    }
}

[DebuggerDisplay("{Id} {Name,nq}")]
public class ProjectGroupViewModel : RSViewModelBase, IProjectGroup, IEntity<ProjectGroups>
{
    private ProjectGroups myEntity;

    public override Guid Id
    {
        set
        {
            if (Entity.Id == value) return;
            Entity.Id = value;
            RaisePropertyChanged();
        }
        get => Entity.Id;
    }

    public override string Name
    {
        set
        {
            if (Entity.Name == value) return;
            Entity.Name = value;
            RaisePropertyChanged();
        }
        get => Entity.Name;
    }

    public override string Note
    {
        set
        {
            if (Entity.Note == value) return;
            Entity.Note = value;
            RaisePropertyChanged();
        }
        get => Entity.Note;
    }

    public ProjectGroups Entity
    {
        get => myEntity;
        set
        {
            if (Equals(value, myEntity)) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public ProjectGroups DefaultValue()
    {
        return new ProjectGroups
        {
            Id = Guid.NewGuid(),
            ProjectGroupLink = new List<ProjectGroupLink>()
        };
    }

    public ObservableCollection<IProject> Projects { get; set; } = new ObservableCollection<IProject>();
}
