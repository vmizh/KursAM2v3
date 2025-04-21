using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Data;
using KursDomain.Result;

namespace KursAM2.Repositories.Projects;

public class ProjectRepository(ALFAMEDIAEntities context) : IProjectRepository
{
    private DbContextTransaction myTransaction = null;
    public IBoolResult SaveReference(IEnumerable<Data.Projects> data, IEnumerable<Guid> deleteIds = null)
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
                if (old is null) continue;
                if (old.SD_24.Count == 0 && old.TD_101.Count == 0 && old.SD_33.Count == 0 && old.SD_34.Count == 0 &&
                    old.ProjectGroupLink.Count == 0
                    && old.NomenklReturnToProvider.Count == 0 && old.NomenklReturnOfClient.Count == 0)
                { 
                   context.Projects.Remove(old);
                }
                else
                {
                    return new BoolResult
                    {
                        Result = false,
                        ErrorText = $"Для проекта {old.Name} есть связанные в документах"
                    };
                }
            }

        foreach (var p in data)
        {
            if (context.Projects.Any(_ => _.Id == p.Id))
            {
                context.Projects.Attach(p);
                context.Entry(p).State = EntityState.Modified;
            }
            else
            {
                context.Projects.Add(p);
            }
        }

        //context.SaveChanges();
        return new BoolResult { Result = true };
    }

    public IEnumerable<Data.Projects> LoadReference()
    {
        return context.Projects.ToList();
    }

    
    public IBoolResult SaveGroups(IEnumerable<ProjectGroups> data, IEnumerable<Guid> deleteGrpIds = null, IEnumerable<Guid> deleteLinkIds = null)
    {
        if (deleteGrpIds is not null && deleteGrpIds.Any())
            foreach (var id in deleteGrpIds)
            {
                var old = context.ProjectGroups.Include(_ => _.ProjectGroupLink).FirstOrDefault(_ => _.Id == id);
                if (old?.ProjectGroupLink != null)
                    context.ProjectGroupLink.RemoveRange(old.ProjectGroupLink);
                if (old != null) context.ProjectGroups.Remove(old);
            }

        if (deleteLinkIds is not null && deleteLinkIds.Any())
            foreach (var id in deleteLinkIds)
            {
                var old = context.ProjectGroupLink.FirstOrDefault(_ => _.Id == id);
                if (old == null) continue;
                context.ProjectGroupLink.Remove(old);
            }

        foreach (var p in data)
            if (!context.ProjectGroups.Any(_ => _.Id == p.Id))
            {
                context.ProjectGroups.Add(p);
            }
            //else
            //{

            //    context.ProjectGroups.Attach(p);
            //    context.Entry(p).State = EntityState.Modified;
            //}

        //context.SaveChanges();
        return new BoolResult { Result = true };
    }

    public IEnumerable<ProjectGroups> LoadGroups()
    {
        return context.ProjectGroups.Include(_ => _.ProjectGroupLink).ToList();
    }

    public void BeginTransaction()
    {
        myTransaction = context.Database.BeginTransaction();
    }

    public void CommitTransaction()
    {
        if(myTransaction != null) 
            myTransaction.Commit();
    }

    public void RollbackTransaction()
    {
        if(myTransaction != null) 
            myTransaction.Rollback();
    }

    public void SaveChanges()
    {
        if(context.ChangeTracker.HasChanges())
            context.SaveChanges();
    }
}
