using System;
using KursDomain.ICommon;
using KursDomain.IReferences;
using System.Collections.Generic;
using System.Diagnostics;
using Data;

namespace KursDomain.References;

[DebuggerDisplay("{Id} {Name,nq} {ParentId,nq}")]
public class Project : IProject, IDocGuid, IName,  IEqualityComparer<IDocGuid>
{
    public bool IsDeleted { get; set; }
    public bool IsClosed { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public IEmployee Employee { get; set; }
    public Guid? ParentId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Проект: {Name}";
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

    public bool Equals(IDocGuid x, IDocGuid y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Id.Equals(y.Id);
    }

    public int GetHashCode(IDocGuid obj)
    {
        return obj.Id.GetHashCode();
    }
}
