using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using Data;
using KursAM2.Repositories.RedisRepository;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Result;
using KursRepositories.Repositories.Base;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace KursRepositories.Repositories.Projects
{
    public class ProjectRepository : BaseRepository, IProjectRepository
    {
        public ProjectRepository(ALFAMEDIAEntities context)
        {
            Context = context;
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();
        }

        public IBoolResult SaveReference(IEnumerable<Data.Projects> data, IEnumerable<Guid> deleteIds = null)
        {
            var delList = deleteIds is null ? new List<Guid>() : deleteIds.ToList();
            if (deleteIds is not null && delList.Any())
                foreach (var id in delList)
                {
                    var old = Context.Projects
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
                        Context.Projects.Remove(old);
                    else
                        return new BoolResult
                        {
                            Result = false,
                            ErrorText = $"Для проекта {old.Name} есть связанные в документах"
                        };
                }

            foreach (var p in data)
                if (Context.Projects.Any(_ => _.Id == p.Id))
                {
                    Context.Projects.Attach(p);
                    Context.Entry(p).State = EntityState.Modified;
                }
                else
                {
                    Context.Projects.Add(p);
                }

            return new BoolResult { Result = true };
        }

        public IEnumerable<Data.Projects> LoadReference()
        {
            return Context.Projects.ToList();
        }


        public IBoolResult SaveGroups(IEnumerable<ProjectGroups> data, IEnumerable<Guid> deleteGrpIds = null,
            IEnumerable<Guid> deleteLinkIds = null)
        {
            if (deleteGrpIds is not null && deleteGrpIds.Any())
                foreach (var id in deleteGrpIds)
                {
                    var old = Context.ProjectGroups.Include(_ => _.ProjectGroupLink).FirstOrDefault(_ => _.Id == id);
                    if (old?.ProjectGroupLink != null)
                        Context.ProjectGroupLink.RemoveRange(old.ProjectGroupLink);
                    if (old != null) Context.ProjectGroups.Remove(old);
                }

            if (deleteLinkIds is not null && deleteLinkIds.Any())
                foreach (var id in deleteLinkIds)
                {
                    var old = Context.ProjectGroupLink.FirstOrDefault(_ => _.Id == id);
                    if (old == null) continue;
                    Context.ProjectGroupLink.Remove(old);
                }

            foreach (var p in data)
                if (!Context.ProjectGroups.Any(_ => _.Id == p.Id))
                    Context.ProjectGroups.Add(p);
            return new BoolResult { Result = true };
        }

        public IEnumerable<ProjectGroups> LoadGroups()
        {
            return Context.ProjectGroups.Include(_ => _.ProjectGroupLink).ToList();
        }

        public void UpdateCache()
        {
            if (mySubscriber != null && mySubscriber.IsConnected())
            {
                var message = new RedisMessage
                {
                    DocumentType = DocumentType.InvoiceProvider,
                    IsDocument = false,
                    Message = $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник проектов"
                };
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                mySubscriber.Publish(
                    new RedisChannel(RedisMessageChannels.ProjectReference, RedisChannel.PatternMode.Auto), json);
            }
        }
    }
}
