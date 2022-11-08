using System;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{Id} {Name,nq} {ParentId,nq}")]
public class Project : IProject, IDocGuid, IName, IEquatable<Project>
{
    public Guid Id { get; set; }

    public bool Equals(Project other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Проект: {Name}";
    public bool IsDeleted { get; set; }
    public bool IsClosed { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public IEmployee Employee { get; set; }
    public Guid? ParentId { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(Projects entity, IReferencesCache refCache)
    {
        if (entity == null)
        {
            Id = Guid.Empty;
            return;
        }

        Id = entity.Id;
        IsDeleted = entity.IsDeleted;
        IsClosed = entity.IsClosed;
        DateStart = entity.DateStart;
        DateEnd = entity.DateEnd;
        Employee = refCache?.GetEmployee(entity.ManagerDC);
        ParentId = entity.ParentId;
        Name = entity.Name;
        Notes = entity.Note;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Project) obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
