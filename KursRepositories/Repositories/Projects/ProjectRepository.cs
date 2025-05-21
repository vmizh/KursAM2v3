using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Data;
using Helper;
using KursAM2.Repositories.RedisRepository;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Projects;
using KursDomain.ICommon;
using KursDomain.References;
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

        public IEnumerable<ProjectDocumentInfo> LoadProjectDocuments(Guid projectId)
        {
            var prj = Context.Projects.Include(_ => _.ProjectDocuments).AsNoTracking()
                .FirstOrDefault(_ => _.Id == projectId);
            if (prj is null) return new List<ProjectDocumentInfo>();
            return LoadProjectDocuments(prj);
        }

        public void AddDocumentInfo(ProjectDocumentInfoBase doc)
        {
            var sql = $@"INSERT INTO dbo.ProjectDocuments (
                  Id
                 ,ProjectId
                 ,DocType
                 ,DocInfo
                 ,Note
                 ,BankCode
                 ,CashInDC
                 ,CashOutDC
                 ,WarehouseOrderInDC
                 ,WaybillDC
                 ,AccruedClientRowId
                 ,AccruedSupplierRowId
                 ,UslugaClientRowId
                 ,UslugaProviderRowId
                )
                VALUES
                (
                  '{CustomFormat.GuidToSqlString(doc.Id)}'
                 ,'{CustomFormat.GuidToSqlString(doc.ProjectId)}'
                 ,{(int)doc.DocumentType}
                 ,'{doc.DocInfo}' 
                 ,'{doc.ProjectNote}'
                 ,{(doc.BankCode.HasValue ? doc.BankCode.Value : "NULL")} 
                 ,{(doc.CashInDC.HasValue ? doc.CashInDC.Value : "NULL")} 
                 ,{(doc.CashOutDC.HasValue ? doc.CashOutDC.Value : "NULL")} 
                 ,{(doc.WarehouseOrderInDC.HasValue ? doc.WarehouseOrderInDC.Value : "NULL")}
                 ,{(doc.WaybillDC.HasValue ? doc.WaybillDC.Value : "NULL")}
                 ,{(doc.AccruedClientRowId.HasValue ? "'" + doc.AccruedClientRowId.Value + "'" : "NULL")}
                 ,{(doc.AccruedSupplierRowId.HasValue ? "'" + doc.AccruedSupplierRowId.Value + "'" : "NULL")}
                 ,{(doc.UslugaClientRowId.HasValue ? "'" + doc.UslugaClientRowId.Value + "'" : "NULL")}
                 ,{(doc.UslugaProviderRowId.HasValue ? "'" + doc.UslugaProviderRowId.Value + "'" : "NULL")}
                );";
            Context.Database.ExecuteSqlCommand(sql);
        }

        public void UpdateDocumentInfo(Guid id, string note)
        {
            Context.Database.ExecuteSqlCommand(
                $"UPDATE ProjectDocuments SET Note = '{note}' WHERE Id = '{CustomFormat.GuidToSqlString(id)}'",
                null);
        }

        public void DeleteDocumentInfo(Guid id)
        {
            Context.Database.ExecuteSqlCommand(
                $"DELETE FROM ProjectDocuments WHERE Id = '{CustomFormat.GuidToSqlString(id)}'", null);
        }

        public string GetDocDescription(DocumentType docType, ProjectDocumentInfoBase doc)
        {
            switch (docType)
            {
                case DocumentType.Bank:
                    return $"Банковская транзакция от {doc.DocDate} {doc.Kontragent}" +
                           (doc.SummaIn > 0 ? "получено " : "выплачено ")
                           + (doc.SummaIn > 0 ? $"{doc.SummaIn:n2}" : $"{doc.SummaOut:n2}")
                           + $" {doc.Currency}";
                case DocumentType.AccruedAmountForClient:
                    break;
                case DocumentType.AccruedAmountOfSupplier:
                    break;
                case DocumentType.CashIn:
                    return $"Приходный кассовый ордер ({doc.CashBox}) №{doc.InnerNumber} " +
                           $"от {doc.DocDate} {doc.Kontragent} на {doc.SummaIn:n2} {doc.Currency}";
                case DocumentType.CashOut:
                    return $"Расходный кассовый ордер ({doc.CashBox}) №{doc.InnerNumber} " +
                           $"от {doc.DocDate} {doc.Kontragent} на {doc.SummaOut:n2} {doc.Currency}";
                case DocumentType.StoreOrderIn:
                    return
                        $"Приходный складской ордер ({doc.Warehouse}) №{doc.InnerNumber} от {doc.DocDate}" +
                        $" {doc.Kontragent} на {doc.SummaOut} {doc.Currency}";
                case DocumentType.Waybill:
                    return $"Расходная накладная ({doc.Warehouse}) №{doc.InnerNumber} от {doc.DocDate}" +
                           $" {doc.Kontragent} на {doc.SummaIn} {doc.Currency}";
                case DocumentType.InvoiceClient:
                    return
                        $"Сф клиенту №{doc.InnerNumber}/{doc.ExtNumber} от {doc.DocDate} " +
                        $"для {doc.Kontragent} услуга {doc.NomenklName} на {doc.SummaIn} {doc.Currency}";
                case DocumentType.InvoiceProvider:
                    return
                        $"Сф поставщика №{doc.InnerNumber}/{doc.ExtNumber} от {doc.DocDate} " +
                        $"для {doc.Kontragent} услуга {doc.NomenklName} на {doc.SummaOut} {doc.Currency}";
            }

            return null;
        }

        public decimal? GetInvoiceClientDC(Guid rowId)
        {
            return Context.TD_84.FirstOrDefault(_ => _.Id == rowId)?.DOC_CODE;
        }

        public decimal? GetInvoiceProviderDC(Guid rowId)
        {
            return Context.TD_26.FirstOrDefault(_ => _.Id == rowId)?.DOC_CODE;
        }

        public Guid? GetAccruedAmountForClientsId(Guid rowId)
        {
            return Context.AccuredAmountForClientRow.FirstOrDefault(_ => _.Id == rowId)?.DocId;
        }

        public Guid? GetAccruedAmountProviderId(Guid rowId)
        {
            return Context.AccuredAmountOfSupplierRow.FirstOrDefault(_ => _.Id == rowId)?.DocId;
        }

        public IEnumerable<ProjectDocumentInfo> LoadProjectDocuments(Data.Projects project)
        {
            var ret = new List<ProjectDocumentInfo>();
            if (project.ProjectDocuments is not null && project.ProjectDocuments.Count > 0)
                foreach (var p in project.ProjectDocuments)
                {
                    var newItem = new ProjectDocumentInfo(p);
                    if (p.BankCode is not null)
                    {
                        var bb = Context.TD_101.Include(_ => _.SD_101).FirstOrDefault(_ => _.CODE == p.BankCode);
                        if (bb != null)
                        {
                            newItem.BankAccount =
                                ((IName)GlobalOptions.ReferencesCache.GetBankAccount(bb.SD_101.VV_ACC_DC))?.Name;
                            newItem.Currency =
                                GlobalOptions.ReferencesCache.GetBankAccount(bb.SD_101.VV_ACC_DC)?.Currency as Currency;
                            if (bb.VVT_VAL_PRIHOD > 0)
                                newItem.SummaIn = bb.VVT_VAL_PRIHOD.Value;
                            else
                                newItem.SummaOut = bb.VVT_VAL_RASHOD.Value;
                            newItem.DocDate = bb.SD_101.VV_START_DATE;
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(bb.VVT_KONTRAGENT) as Kontragent;
                            newItem.DocInfo = GetDocDescription(DocumentType.Bank, newItem);
                        }
                    }

                    if (p.CashInDC is not null)
                    {
                        var chIn = Context.SD_33.FirstOrDefault(_ => _.DOC_CODE == p.CashInDC);
                        if (chIn != null)
                        {
                            newItem.CashBox = GlobalOptions.ReferencesCache.GetCashBox(chIn.CA_DC) as CashBox;
                            newItem.Currency =
                                GlobalOptions.ReferencesCache.GetCurrency(chIn.CRS_DC) as Currency;
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(chIn.KONTRAGENT_DC) as Kontragent;
                            newItem.SummaIn = (decimal)chIn.CRS_SUMMA;
                            newItem.DocDate = (DateTime)chIn.DATE_ORD;
                            newItem.InnerNumber = chIn.NUM_ORD;
                            newItem.Note = chIn.NOTES_ORD;
                            newItem.DocInfo = GetDocDescription(DocumentType.CashIn, newItem);
                        }
                    }

                    if (p.CashOutDC is not null)
                    {
                        var chOut = Context.SD_34.FirstOrDefault(_ => _.DOC_CODE == p.CashOutDC);
                        if (chOut != null)
                        {
                            newItem.CashBox = GlobalOptions.ReferencesCache.GetCashBox(chOut.CA_DC) as CashBox;
                            newItem.Currency =
                                GlobalOptions.ReferencesCache.GetCurrency(chOut.CRS_DC) as Currency;
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(chOut.KONTRAGENT_DC) as Kontragent;
                            newItem.SummaIn = (decimal)chOut.CRS_SUMMA;
                            newItem.DocDate = (DateTime)chOut.DATE_ORD;
                            newItem.InnerNumber = chOut.NUM_ORD;
                            newItem.Note = chOut.NOTES_ORD;
                            newItem.DocInfo = GetDocDescription(DocumentType.CashOut, newItem);
                        }
                    }

                    if (p.WarehouseOrderInDC is not null)
                    {
                        var ord = Context.SD_24.Include(sd24 => sd24.TD_24.Select(td24 => td24.TD_26))
                            .FirstOrDefault(_ => _.DOC_CODE == p.WarehouseOrderInDC);
                        if (ord != null)
                        {
                            newItem.Warehouse =
                                GlobalOptions.ReferencesCache.GetWarehouse(ord.DD_SKLAD_POL_DC) as Warehouse;
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(ord.DD_KONTR_OTPR_DC) as Kontragent;
                            newItem.Currency =
                                GlobalOptions.ReferencesCache.GetKontragent(ord.DD_KONTR_OTPR_DC)?.Currency as Currency;
                            newItem.SummaOut = ord.TD_24.Sum(_ =>
                                _.DDT_KOL_PRIHOD * (_.TD_26.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0) / _.TD_26.SFT_KOL);
                            newItem.DocDate = ord.DD_DATE;
                            newItem.InnerNumber = ord.DD_IN_NUM;
                            newItem.ExtNumber = ord.DD_EXT_NUM;
                            newItem.DocInfo = GetDocDescription(DocumentType.StoreOrderIn, newItem);
                            newItem.Note = ord.DD_NOTES;
                        }
                    }

                    if (p.WaybillDC is not null)
                    {
                        var ord = Context.SD_24.Include(sd24 => sd24.TD_24.Select(td24 => td24.TD_84))
                            .FirstOrDefault(_ => _.DOC_CODE == p.WaybillDC);
                        if (ord != null)
                        {
                            newItem.Warehouse =
                                GlobalOptions.ReferencesCache.GetWarehouse(ord.DD_SKLAD_OTPR_DC) as Warehouse;
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(ord.DD_KONTR_POL_DC) as Kontragent;
                            newItem.Currency =
                                newItem.Kontragent?.Currency as Currency;
                            newItem.SummaIn = ord.TD_24.Sum(_ =>
                                _.DDT_KOL_RASHOD * (_.TD_84.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0) /
                                (decimal)_.TD_84.SFT_KOL);
                            newItem.SummaDiler = ord.TD_24.Sum(_ =>
                                _.DDT_KOL_RASHOD * (_.TD_84.SFT_NACENKA_DILERA ?? 0) / (decimal)_.TD_84.SFT_KOL);
                            newItem.DocDate = ord.DD_DATE;
                            newItem.InnerNumber = ord.DD_IN_NUM;
                            newItem.ExtNumber = ord.DD_EXT_NUM;
                            newItem.Note = ord.DD_NOTES;
                        }
                    }

                    if (p.UslugaProviderRowId is not null)
                    {
                        var ord = Context.TD_26.Include(_ => _.SD_26).FirstOrDefault(_ => _.Id == p.UslugaProviderRowId);
                        if (ord != null)
                        {
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(ord.SD_26.SF_POST_DC) as Kontragent;
                            newItem.Currency =
                                newItem.Kontragent?.Currency as Currency;
                            newItem.SummaIn = (decimal)ord.SFT_SUMMA_K_OPLATE_KONTR_CRS;
                            newItem.DocDate = ord.SD_26.SF_POSTAV_DATE;
                            newItem.InnerNumber = ord.SD_26.SF_IN_NUM;
                            newItem.ExtNumber = ord.SD_26.SF_POSTAV_NUM;
                            newItem.Note = ord.SD_26.SF_NOTES;
                            newItem.Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(ord.SFT_NEMENKL_DC) as Nomenkl;
                            newItem.DocInfo = GetDocDescription(DocumentType.InvoiceProvider, newItem);

                        }
                    }
                    if (p.UslugaClientRowId is not null)
                    {
                        var ord = Context.TD_84.Include(_ => _.SD_84).FirstOrDefault(_ => _.Id == p.UslugaClientRowId);
                        if (ord != null)
                        {
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(ord.SD_84.SF_CLIENT_DC) as Kontragent;
                            newItem.Currency =
                                newItem.Kontragent?.Currency as Currency;
                            newItem.SummaIn = (decimal)ord.SFT_SUMMA_K_OPLATE_KONTR_CRS;
                            newItem.DocDate = ord.SD_84.SF_DATE;
                            newItem.InnerNumber = ord.SD_84.SF_IN_NUM;
                            newItem.ExtNumber = ord.SD_84.SF_OUT_NUM;
                            newItem.Note = ord.SD_84.SF_NOTE;
                            newItem.Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(ord.SFT_NEMENKL_DC) as Nomenkl;
                            newItem.DocInfo = GetDocDescription(DocumentType.InvoiceClient, newItem);
                        }
                    }

                    ret.Add(newItem);
                }

            return ret;
        }


        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
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

        public IEnumerable<SD_33> GetCashInForProject(Guid projectId, DateTime dateStart, DateTime dateEnd)
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context.SD_33.Include(_ => _.ProjectDocuments)
                .Where(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd).ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id)))
                .ToList();
        }

        public IEnumerable<SD_34> GetCashOutForProject(Guid projectId, DateTime dateStart, DateTime dateEnd)
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context.SD_34.Include(_ => _.ProjectDocuments)
                .Where(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd).ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id)))
                .ToList();
        }

        public IEnumerable<TD_101> GetBankForProject(Guid projectId, DateTime dateStart, DateTime dateEnd)
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context.TD_101.Include(_ => _.SD_101).Include(_ => _.SD_101.SD_114)
                .Include(_ => _.ProjectDocuments)
                .Where(_ => _.SD_101.VV_START_DATE >= dateStart && _.SD_101.VV_STOP_DATE <= dateEnd &&
                            _.VVT_KONTRAGENT != null).ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id)))
                .ToList();
        }

        public IEnumerable<SD_24> GetWarehouseInForProject(Guid projectId, DateTime dateStart, DateTime dateEnd)
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context.SD_24.Include(_ => _.TD_24)
                .Include(_ => _.ProjectDocuments)
                .Include(_ => _.SD_26)
                .Include("TD_24.TD_26")
                .Where(_ => _.DD_TYPE_DC == 2010000001 && _.DD_SKLAD_OTPR_DC == null && _.DD_DATE >= dateStart &&
                            _.DD_DATE <= dateEnd).ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id)))
                .ToList();
        }

        public IEnumerable<SD_24> GetWaybillInForProject(Guid projectId, DateTime dateStart, DateTime dateEnd)
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context.SD_24.Include(_ => _.TD_24).Include(_ => _.ProjectDocuments)
                .Where(_ => _.DD_TYPE_DC == 2010000012 && _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd).ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id)))
                .ToList();
        }

        public IEnumerable<TD_26> GetUslagaProviderForProject(Guid projectId, DateTime dateStart, DateTime dateEnd)
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context.TD_26.Include(_ => _.SD_26).Include(_ => _.SD_83)
                .Where(_ => _.SD_26.SF_POSTAV_DATE >= dateStart && _.SD_26.SF_POSTAV_DATE <= dateEnd &&
                            _.SD_83.NOM_0MATER_1USLUGA == 1
                            && _.SD_26.SF_ACCEPTED == 1).ToList();

            var docIds = new List<Guid>();
            foreach (var r in Context.Projects.Include(_ => _.ProjectDocuments)
                         .Where(_ => ids.Contains(_.Id)))
                docIds.AddRange(from rr in r.ProjectDocuments
                    where rr.UslugaProviderRowId is not null
                    select rr.UslugaProviderRowId.Value);

            return data.Where(row => !docIds.Contains(row.Id)).ToList();
        }

        public IEnumerable<TD_84> GetUslagaClientForProject(Guid projectId, DateTime dateStart, DateTime dateEnd)
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context.TD_84.Include(_ => _.SD_84).Include(_ => _.SD_83)
                .Where(_ => _.SD_84.SF_DATE >= dateStart && _.SD_84.SF_DATE <= dateEnd &&
                            _.SD_83.NOM_0MATER_1USLUGA == 1
                            && _.SD_84.SF_ACCEPTED == 1).ToList();

            var docIds = new List<Guid>();
            foreach (var r in Context.Projects.Include(_ => _.ProjectDocuments)
                         .Where(_ => ids.Contains(_.Id)))
                docIds.AddRange(from rr in r.ProjectDocuments
                    where rr.UslugaProviderRowId is not null
                    select rr.UslugaProviderRowId.Value);

            return data.Where(row => !docIds.Contains(row.Id)).ToList();
        }

        public IEnumerable<AccuredAmountForClientRow> GetAccruedAmountForClients(Guid projectId, DateTime dateStart,
            DateTime dateEnd)
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context.AccuredAmountForClientRow.Include(_ => _.AccruedAmountForClient)
                .Include(_ => _.ProjectDocuments)
                .Where(_ => _.AccruedAmountForClient.DocDate >= dateStart &&
                            _.AccruedAmountForClient.DocDate <= dateEnd).ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id)))
                .ToList();
        }

        public IEnumerable<AccuredAmountOfSupplierRow> GetAccruedAmountOfSuppliers(Guid projectId, DateTime dateStart,
            DateTime dateEnd)
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context.AccuredAmountOfSupplierRow.Include(_ => _.AccruedAmountOfSupplier)
                .Include(_ => _.ProjectDocuments)
                .Where(_ => _.AccruedAmountOfSupplier.DocDate >= dateStart &&
                            _.AccruedAmountOfSupplier.DocDate <= dateEnd).ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id)))
                .ToList();
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


        /// <summary>
        ///     Получить все Id проектов, находящиеся в одной ветке с id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private List<Guid> GetAllTreeProjectIds(Guid id)
        {
            var projects = Context.Projects.ToList();
            var headId = Guid.Empty;
            Guid? iid = id;
            while (iid != null)
            {
                headId = iid.Value;
                iid = projects.First(_ => _.Id == iid).ParentId;
            }

            return GetChilds(projects, headId);
        }

        private List<Guid> GetChilds(List<Data.Projects> list, Guid id)
        {
            var ret = new List<Guid>(new[] { id });
            var ch = list.Where(_ => _.ParentId == id).ToList();
            foreach (var item in ch) ret.AddRange(GetChilds(list, item.Id));

            return ret;
        }
    }
}
