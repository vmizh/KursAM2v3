using System;

namespace KursDomain.IReferences;

public interface IProject
{
    bool IsDeleted { get; set; }
    bool IsClosed { get; set; }
    DateTime DateStart { get; set; }
    DateTime? DateEnd { get; set; }
    IEmployee Employee { get; set; }
    Guid? ParentId { get; set; }
}
