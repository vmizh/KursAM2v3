using KursDomain.Result;
using System;
using System.Collections.Generic;

namespace KursAM2.Repositories.Projects;

public interface IProjectRepository
{
    IBoolResult SaveReference(IEnumerable<Data.Projects> data, IEnumerable<Guid> deleteIds = null);
    IEnumerable<Data.Projects> LoadReference();

    IBoolResult SaveGroups(IEnumerable<Data.ProjectGroups> data, IEnumerable<Guid> deleteGrpIds = null, IEnumerable<Guid> deleteLinkIds = null);
    IEnumerable<Data.ProjectGroups> LoadGroups();
    
    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();

    void SaveChanges();

}
