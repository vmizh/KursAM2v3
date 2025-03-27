using System;
using System.Collections.Generic;

namespace KursAM2.Repositories.Projects;

public interface IProjectRepository
{
    void SaveReference(IEnumerable<Data.Projects> data, IEnumerable<Guid> deleteIds = null);
    IEnumerable<Data.Projects> LoadReference();

    void SaveGroups(IEnumerable<Data.ProjectGroups> data, IEnumerable<Guid> deleteIds = null);
    IEnumerable<Data.ProjectGroups> LoadGroups();
}
