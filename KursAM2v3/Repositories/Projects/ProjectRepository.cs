using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Data;

namespace KursAM2.Repositories.Projects;

public class ProjectRepository(ALFAMEDIAEntities context) : IProjectRepository
{
    public void SaveReference(IEnumerable<Data.Projects> data, IEnumerable<Guid> deleteIds = null)
    {
        if (deleteIds is not null && deleteIds.Any())
            foreach (var id in deleteIds)
            {
                var old = context.Projects
                    .Include(_ => _.NomenklReturnToProvider)
                    .Include(_ => _.NomenklReturnOfClient)
                    .Include(_ => _.ProjectGroupLink)
                    .Include(_ => _.SD_33)
                    .Include(_ => _.SD_34)
                    .Include(_ => _.SD_24)
                    .Include(_ => _.TD_101)
                    .FirstOrDefault(_ => _.Id == id);
                if (old is { SD_24: null, TD_101: null, SD_33: null, SD_34: null, ProjectGroupLink: null, NomenklReturnToProvider: null, NomenklReturnOfClient: null })
                    context.Projects.Remove(old);
            }

        foreach (var p in data)
            if (context.Projects.Any(_ => _.Id == p.Id))
            {
                context.Projects.Add(p);
            }
            else
            {
                context.Projects.Attach(p);
                context.Entry(p).State = EntityState.Modified;
            }

        context.SaveChanges();
    }

    public IEnumerable<Data.Projects> LoadReference()
    {
        return context.Projects.ToList();
    }

    public void SaveGroups(IEnumerable<ProjectGroups> data, IEnumerable<Guid> deleteIds = null)
    {
        if (deleteIds is not null && deleteIds.Any())
            foreach (var id in deleteIds)
            {
                var old = context.ProjectGroups.Include(_ => _.ProjectGroupLink).FirstOrDefault(_ => _.Id == id);
                if (old?.ProjectGroupLink != null) continue;
                if (old != null) context.ProjectGroups.Remove(old);
            }

        foreach (var p in data)
            if (context.ProjectGroups.Any(_ => _.Id == p.Id))
            {
                context.ProjectGroups.Add(p);
            }
            else
            {
                context.ProjectGroups.Attach(p);
                context.Entry(p).State = EntityState.Modified;
            }

        context.SaveChanges();
    }

    public IEnumerable<ProjectGroups> LoadGroups()
    {
        return context.ProjectGroups.ToList();
    }
}
