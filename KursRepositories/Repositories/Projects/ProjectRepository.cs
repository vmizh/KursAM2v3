using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Helper;
using KursAM2.Repositories.RedisRepository;
using KursDomain;
using KursDomain.Base;
using KursDomain.Documents.Base;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Invoices;
using KursDomain.Documents.Projects;
using KursDomain.ICommon;
using KursDomain.IDocuments.Finance;
using KursDomain.References;
using KursDomain.Result;
using KursRepositories.Repositories.Base;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace KursRepositories.Repositories.Projects
{
    public class ProjectRepository : BaseRepository, IProjectRepository
    {
        #region Constructors

        public ProjectRepository(ALFAMEDIAEntities context)
        {
            Context = context;
            redis = ConnectionMultiplexer.Connect(
                ConfigurationManager.AppSettings["redis.connection"]
            );
            mySubscriber = redis.GetSubscriber();
        }

        #endregion

        #region DocumentInfo

        public void AddDocumentInfo(ProjectDocumentInfoBase doc)
        {
            var sql =
                $@"INSERT INTO dbo.ProjectDocuments (
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
                 ,CurrencyConvertId
                 ,InvoiceClientId
                 ,InvoiceProviderId
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
                 ,{(doc.CurrencyConvertId.HasValue ? "'" + doc.CurrencyConvertId.Value + "'" : "NULL")}
                 ,{(doc.InvoiceClientId.HasValue ? "'" + doc.InvoiceClientId.Value + "'" : "NULL")}
                 ,{(doc.InvoiceProviderId.HasValue ? "'" + doc.InvoiceProviderId.Value + "'" : "NULL")}
                );";
            Context.Database.ExecuteSqlCommand(sql);
        }

        public void UpdateDocumentInfo(Guid id, string note)
        {
            Context.Database.ExecuteSqlCommand(
                $"UPDATE ProjectDocuments SET Note = '{note}' WHERE Id = '{CustomFormat.GuidToSqlString(id)}'"
            );
        }

        public void DeleteDocumentInfo(Guid id)
        {
            Context.Database.ExecuteSqlCommand(
                $"DELETE FROM ProjectDocuments WHERE Id = '{CustomFormat.GuidToSqlString(id)}'"
            );
        }

        public string GetDocDescription(DocumentType docType, ProjectDocumentInfoBase doc)
        {
            switch (docType)
            {
                case DocumentType.Bank:
                    return $"Банковская транзакция от {doc.DocDate} {doc.Kontragent}"
                           + (doc.SummaIn > 0 ? "получено " : "выплачено ")
                           + (doc.SummaIn > 0 ? $"{doc.SummaIn:n2}" : $"{doc.SummaOut:n2}")
                           + $" {doc.Currency}";
                case DocumentType.AccruedAmountForClient:
                    return $"Прямые затраты клиенту  №{doc.InnerNumber}/{doc.ExtNumber} от {doc.DocDate} "
                           + $"для {doc.Kontragent} {doc.NomenklName} на {doc.SummaIn} {doc.Currency}";
                case DocumentType.AccruedAmountOfSupplier:
                    return $"Прямые затраты поставщика  №{doc.InnerNumber}/{doc.ExtNumber} от {doc.DocDate} "
                           + $"для {doc.Kontragent} {doc.NomenklName} на {doc.SummaOut} {doc.Currency}";
                case DocumentType.CashIn:
                    return $"Приходный кассовый ордер ({doc.CashBox}) №{doc.InnerNumber} "
                           + $"от {doc.DocDate} {doc.Kontragent} на {doc.SummaIn:n2} {doc.Currency}";
                case DocumentType.CashOut:
                    return $"Расходный кассовый ордер ({doc.CashBox}) №{doc.InnerNumber} "
                           + $"от {doc.DocDate} {doc.Kontragent} на {doc.SummaOut:n2} {doc.Currency}";
                case DocumentType.StoreOrderIn:
                    return $"Приходный складской ордер ({doc.Warehouse}) №{doc.InnerNumber} от {doc.DocDate}"
                           + $" {doc.Kontragent} на {doc.SummaOut} {doc.Currency}";
                case DocumentType.Waybill:
                    return $"Расходная накладная ({doc.Warehouse}) №{doc.InnerNumber} от {doc.DocDate}"
                           + $" {doc.Kontragent} на {doc.SummaIn} {doc.Currency}";
                case DocumentType.InvoiceServiceClient:
                case DocumentType.InvoiceClient:
                    return $"Сф клиенту №{doc.InnerNumber}/{doc.ExtNumber} от {doc.DocDate} "
                           + $"для {doc.Kontragent} услуга {doc.NomenklName} на {doc.SummaIn} {doc.Currency}";
                case DocumentType.InvoiceServiceProvider:
                case DocumentType.InvoiceProvider:
                    return $"Сф поставщика №{doc.InnerNumber}/{doc.ExtNumber} от {doc.DocDate} "
                           + $"для {doc.Kontragent} услуга {doc.NomenklName} на {doc.SummaOut} {doc.Currency}";
                case DocumentType.NomenklCurrencyConverterProvider:
                    return $"Валютная конвертация по Сф поставщика №{doc.InnerNumber}/{doc.ExtNumber} от {doc.DocDate} "
                           + $"для {doc.Kontragent} номенклатура {doc.NomenklName} на {doc.SummaOut} {doc.Currency}";
            }

            return null;
        }

        #endregion

        #region InvoiceClient

        public decimal? GetInvoiceClientDC(Guid rowId, bool isRow)
        {
            if (isRow)
                return Context.TD_84.FirstOrDefault(_ => _.Id == rowId)?.DOC_CODE;
            return Context.SD_84.FirstOrDefault(_ => _.Id == rowId)?.DOC_CODE;
        }

        public List<IInvoiceClient> GetInvoicesClient(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .InvoiceClientQuery.Where(_ => _.DocDate >= dateStart && _.DocDate <= dateEnd)
                .AsNoTracking()
                .OrderByDescending(_ => _.DocDate)
                .ToList();
            var ret = data.Select(_ => _.DocCode)
                .Distinct()
                .Select(dc => new InvoiceClientBase(data.Where(_ => _.DocCode == dc)))
                .Cast<IInvoiceClient>()
                .ToList();
            var docIds = new List<Guid>();
            foreach (
                var r in Context
                    .Projects.Include(_ => _.ProjectDocuments)
                    .Where(_ => ids.Contains(_.Id))
            )
                docIds.AddRange(
                    from rr in r.ProjectDocuments
                    where rr.InvoiceClientId is not null
                    select rr.InvoiceClientId.Value
                );
            return ret.Where(row => !docIds.Contains(row.Id)).ToList();
        }

        public List<IInvoiceClient> GetInvoicesClient(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            count = Context.InvoiceClientQuery.Count(_ =>
                _.DocDate >= dateStart && _.DocDate <= dateEnd
            );
            var data = Context
                .InvoiceClientQuery.Where(_ => _.DocDate >= dateStart && _.DocDate <= dateEnd)
                .AsNoTracking()
                .OrderByDescending(_ => _.DocDate)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
            var ret = data.Select(_ => _.DocCode)
                .Distinct()
                .Select(dc => new InvoiceClientBase(data.Where(_ => _.DocCode == dc)))
                .Cast<IInvoiceClient>()
                .ToList();
            var docIds = new List<Guid>();
            foreach (
                var r in Context
                    .Projects.Include(_ => _.ProjectDocuments)
                    .Where(_ => ids.Contains(_.Id))
            )
                docIds.AddRange(
                    from rr in r.ProjectDocuments
                    where rr.InvoiceClientId is not null
                    select rr.InvoiceClientId.Value
                );
            return ret.Where(row => !docIds.Contains(row.Id)).ToList();
        }

        public int GetInvoicesClientCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context.InvoiceClientQuery.Count(_ =>
                _.DocDate >= dateStart && _.DocDate <= dateEnd
            );
        }

        public List<TD_84> GetInvoiceClientRows(Guid id, Guid? projectId = null)
        {
            var data = Context.TD_84.AsNoTracking().Include(_ => _.SD_83).Include(_ => _.TD_24)
                .Include(_ => _.SD_83.SD_175).Where(_ => _.DocId == id).ToList();
            if (projectId == null) return data;

            var prjs = Context.ProjectInvoiceQuantityChanged.Where(_ =>
                _.ProjectId == projectId && _.ClientRowId != null);
            foreach (var d in data)
            {
                var p = prjs.FirstOrDefault(_ => _.ClientRowId == d.Id);
                if (p == null) continue;
                d.SFT_SUMMA_K_OPLATE_KONTR_CRS = p.Quantity * d.SFT_SUMMA_K_OPLATE_KONTR_CRS / (decimal?)d.SFT_KOL;
                d.SFT_KOL = (double)p.Quantity;
            }

            return data;
        }

        #endregion

        #region InvoiceProvider

        public int GetInvoicesProviderCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context.InvoicePostQuery.Count(_ => _.Date >= dateStart && _.Date <= dateEnd);
        }

        public List<TD_26> GetInvoiceProviderRows(Guid id)
        {
            return Context.TD_26.Include(_ => _.SD_83)
                .Include(_ => _.SD_83.SD_175)
                .Include(_ => _.TD_26_CurrencyConvert)
                .Include(_ => _.TD_24).Where(_ => _.DocId == id).ToList();
        }

        public decimal? GetInvoiceProviderDC(Guid rowId, bool isRow, bool isCrsConvert)
        {
            if (isCrsConvert)
                return Context.TD_26_CurrencyConvert.FirstOrDefault(_ => _.Id == rowId)?.DOC_CODE;
            return isRow
                ? Context.TD_26.FirstOrDefault(_ => _.Id == rowId)?.DOC_CODE
                : Context.SD_26.FirstOrDefault(_ => _.Id == rowId)?.DOC_CODE;
        }

        public List<IInvoiceProvider> GetInvoicesProvider(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .InvoicePostQuery.Where(_ => _.Date >= dateStart && _.Date <= dateEnd)
                .OrderByDescending(_ => _.Date)
                .ToList();
            var ret = data.Select(_ => _.DocCode)
                .Distinct()
                .Select(dc => new InvoiceProviderBase(data.Where(_ => _.DocCode == dc)))
                .Cast<IInvoiceProvider>()
                .ToList();

            var docIds = new List<Guid>();
            foreach (
                var r in Context
                    .Projects.Include(_ => _.ProjectDocuments)
                    .Where(_ => ids.Contains(_.Id))
            )
                docIds.AddRange(
                    from rr in r.ProjectDocuments
                    where rr.InvoiceProviderId is not null
                    select rr.InvoiceProviderId.Value
                );

            return ret.Where(row => !docIds.Contains(row.Id)).ToList();
        }

        public List<IInvoiceProvider> GetInvoicesProvider(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            count = Context.InvoicePostQuery.Count(_ => _.Date >= dateStart && _.Date <= dateEnd);
            var data = Context
                .InvoicePostQuery.Where(_ => _.Date >= dateStart && _.Date <= dateEnd)
                .AsNoTracking()
                .OrderByDescending(_ => _.Date)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
            var ret = data.Select(_ => _.DocCode)
                .Distinct()
                .Select(dc => new InvoiceProviderBase(data.Where(_ => _.DocCode == dc)))
                .Cast<IInvoiceProvider>()
                .ToList();

            var docIds = new List<Guid>();
            foreach (
                var r in Context
                    .Projects.Include(_ => _.ProjectDocuments)
                    .Where(_ => ids.Contains(_.Id))
            )
                docIds.AddRange(
                    from rr in r.ProjectDocuments
                    where rr.InvoiceProviderId is not null
                    select rr.InvoiceProviderId.Value
                );

            return ret.Where(row => !docIds.Contains(row.Id)).ToList();
        }

        #endregion

        #region Прямые затраты

        public Guid? GetAccruedAmountForClientsId(Guid rowId)
        {
            return Context.AccuredAmountForClientRow.FirstOrDefault(_ => _.Id == rowId)?.DocId;
        }

        public Guid? GetAccruedAmountProviderId(Guid rowId)
        {
            return Context.AccuredAmountOfSupplierRow.FirstOrDefault(_ => _.Id == rowId)?.DocId;
        }

        public int GetAccruedAmountForClientsCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context
                .AccuredAmountForClientRow.Include(_ => _.AccruedAmountForClient)
                .Count(_ =>
                    _.AccruedAmountForClient.DocDate >= dateStart
                    && _.AccruedAmountForClient.DocDate <= dateEnd
                );
        }

        public IEnumerable<AccuredAmountForClientRow> GetAccruedAmountForClients(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .AccuredAmountForClientRow.Include(_ => _.AccruedAmountForClient)
                .Include(_ => _.ProjectDocuments)
                .Where(_ =>
                    _.AccruedAmountForClient.DocDate >= dateStart
                    && _.AccruedAmountForClient.DocDate <= dateEnd
                )
                .ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        public IEnumerable<AccuredAmountForClientRow> GetAccruedAmountForClients(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            count = Context
                .AccuredAmountForClientRow.Include(_ => _.AccruedAmountForClient)
                .Include(_ => _.ProjectDocuments)
                .Count(_ =>
                    _.AccruedAmountForClient.DocDate >= dateStart
                    && _.AccruedAmountForClient.DocDate <= dateEnd
                );
            var data = Context
                .AccuredAmountForClientRow.Include(_ => _.AccruedAmountForClient)
                .Include(_ => _.ProjectDocuments)
                .Where(_ =>
                    _.AccruedAmountForClient.DocDate >= dateStart
                    && _.AccruedAmountForClient.DocDate <= dateEnd
                )
                .OrderByDescending(_ => _.AccruedAmountForClient.DocDate)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        public int GetAccruedAmountOfSuppliersCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context
                .AccuredAmountOfSupplierRow.Include(_ => _.AccruedAmountOfSupplier)
                .Count(_ =>
                    _.AccruedAmountOfSupplier.DocDate >= dateStart
                    && _.AccruedAmountOfSupplier.DocDate <= dateEnd
                );
        }

        public IEnumerable<AccuredAmountOfSupplierRow> GetAccruedAmountOfSuppliers(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .AccuredAmountOfSupplierRow.Include(_ => _.AccruedAmountOfSupplier)
                .Include(_ => _.ProjectDocuments)
                .Where(_ =>
                    _.AccruedAmountOfSupplier.DocDate >= dateStart
                    && _.AccruedAmountOfSupplier.DocDate <= dateEnd
                )
                .ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        public IEnumerable<AccuredAmountOfSupplierRow> GetAccruedAmountOfSuppliers(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .AccuredAmountOfSupplierRow.Include(_ => _.AccruedAmountOfSupplier)
                .Include(_ => _.ProjectDocuments)
                .Where(_ =>
                    _.AccruedAmountOfSupplier.DocDate >= dateStart
                    && _.AccruedAmountOfSupplier.DocDate <= dateEnd
                )
                .OrderByDescending(_ => _.AccruedAmountOfSupplier.DocDate)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            count = Context
                .AccuredAmountOfSupplierRow.Include(_ => _.AccruedAmountOfSupplier)
                .Count(_ =>
                    _.AccruedAmountOfSupplier.DocDate >= dateStart
                    && _.AccruedAmountOfSupplier.DocDate <= dateEnd
                );
            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        #endregion

        #region Связь проектов и документов

        public IEnumerable<ProjectDocumentInfo> LoadProjectDocuments(Guid projectId, bool isShowAll)
        {
            var prj = Context
                .Projects.Include(_ => _.ProjectDocuments)
                .AsNoTracking()
                .FirstOrDefault(_ => _.Id == projectId);
            if (prj is null)
                return new List<ProjectDocumentInfo>();
            return LoadProjectDocuments(prj, isShowAll);
        }

        private record ManualExt
        {
            public ProjectInvoiceQuantityChanged BaseRow { set; get; }
            public Guid? ClientId { set; get; }
            public Guid? ProviderId { set; get; }
        }

        public IEnumerable<ProjectDocumentInfo> LoadProjectDocuments2(Guid projectId, bool isShowAll)
        {
            var ret = new List<ProjectDocumentInfo>();
            var project = Context.Projects.Include(_ => _.ProjectDocuments).AsNoTracking()
                .FirstOrDefault(_ => _.Id == projectId);
            var manual = Context.ProjectInvoiceQuantityChanged.Where(_ => _.ProjectId == projectId).ToList();
            var manul = new List<ManualExt>();
            foreach (var r in manual)
            {
                var newItem = new ManualExt
                {
                    BaseRow = r
                };
                if (r.ProviderRowId != null)
                    newItem.ProviderId = Context.TD_26.FirstOrDefault(_ => _.Id == r.ProviderRowId)?.DocId;
                if (r.ClientRowId != null)
                    newItem.ClientId = Context.TD_84.FirstOrDefault(_ => _.Id == r.ClientRowId)?.DocId;
                if (r.ProviderCurrencyConvertRowId != null)
                    newItem.ProviderId = Context.TD_26_CurrencyConvert.Include(_ => _.TD_26)
                        .FirstOrDefault(_ => _.Id == r.ProviderCurrencyConvertRowId)?.TD_26?.DocId;
                manul.Add(newItem);
            }

            if (project is null) return new List<ProjectDocumentInfo>();
            var exclInvoices = Context.ProjectRowExclude.Include(_ => _.TD_26_CurrencyConvert)
                .AsNoTracking().Where(_ => _.ProjectId == projectId).ToList();
            if (project.ProjectDocuments.Any(_ => _.InvoiceProviderId != null))
            {
                var providerInvoices = GetInvoiceProviders(
                    project.ProjectDocuments.Where(_ => _.InvoiceProviderId != null).ToList(), isShowAll,
                    exclInvoices).ToList();
                if (manul.Any(_ => _.ProviderId != null))
                    foreach (var p in providerInvoices)
                        if (manul.Any(_ => _.ProviderId == p.InvoiceProviderId))
                            p.IsManualChanged = true;

                ret.AddRange(providerInvoices);
            }

            if (project.ProjectDocuments.Any(_ => _.InvoiceClientId != null))
            {
                var clientInvoices = GetInvoiceClients(
                    project.ProjectDocuments.Where(_ => _.InvoiceClientId != null).ToList(),
                    isShowAll, exclInvoices).ToList();
                if (manul.Any(_ => _.ClientId != null))
                    foreach (var c in clientInvoices)
                        if (manul.Any(_ => _.ClientId == c.InvoiceClientId))
                            c.IsManualChanged = true;

                ret.AddRange(clientInvoices);
            }

            if (project.ProjectDocuments.Any(_ => _.BankCode != null))
                ret.AddRange(GetBanksForProject(project.ProjectDocuments.Where(_ => _.BankCode != null).ToList(),
                    isShowAll, exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.CashInDC != null))
                ret.AddRange(GetCashInsForProject(project.ProjectDocuments.Where(_ => _.CashInDC != null).ToList(),
                    isShowAll, exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.CashOutDC != null))
                ret.AddRange(GetCashInsForProject(project.ProjectDocuments.Where(_ => _.CashOutDC != null).ToList(),
                    isShowAll, exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.WarehouseOrderInDC != null))
                ret.AddRange(GetWarehouseForProject(
                    project.ProjectDocuments.Where(_ => _.WarehouseOrderInDC != null).ToList(), isShowAll,
                    exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.WaybillDC != null))
                ret.AddRange(GetWaybillsForProject(project.ProjectDocuments.Where(_ => _.WaybillDC != null).ToList(),
                    isShowAll, exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.UslugaProviderRowId != null))
                ret.AddRange(GetUslugaProvidersForProject(
                    project.ProjectDocuments.Where(_ => _.UslugaProviderRowId != null).ToList(), isShowAll,
                    exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.UslugaClientRowId != null))
                ret.AddRange(GetUslugaClientsForProject(
                    project.ProjectDocuments.Where(_ => _.UslugaClientRowId != null).ToList(), isShowAll,
                    exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.AccruedClientRowId != null))
                ret.AddRange(GetAccruedClientsForProject(
                    project.ProjectDocuments.Where(_ => _.AccruedClientRowId != null).ToList(), isShowAll,
                    exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.AccruedSupplierRowId != null))
                ret.AddRange(GetAccruedSuppliersForProject(
                    project.ProjectDocuments.Where(_ => _.AccruedSupplierRowId != null).ToList(), isShowAll,
                    exclInvoices));


            return ret;
        }

        public async Task<IEnumerable<ProjectDocumentInfo>> LoadProjectDocuments2Async(Guid projectId, bool isShowAll)
        {
            var ret = new List<ProjectDocumentInfo>();
            var project = await Context.Projects.Include(_ => _.ProjectDocuments).AsNoTracking()
                .FirstOrDefaultAsync(_ => _.Id == projectId);
            var manual = await Context.ProjectInvoiceQuantityChanged.Where(_ => _.ProjectId == projectId).ToListAsync();
            var manul = new List<ManualExt>();
            foreach (var r in manual)
            {
                var newItem = new ManualExt
                {
                    BaseRow = r
                };
                if (r.ProviderRowId != null)
                {
                    var item = await Context.TD_26.FirstOrDefaultAsync(_ => _.Id == r.ProviderRowId);
                    newItem.ProviderId = item?.DocId;
                }

                if (r.ClientRowId != null)
                {
                    var item = await Context.TD_84.FirstOrDefaultAsync(_ => _.Id == r.ClientRowId);
                    newItem.ClientId = item?.DocId;
                }

                if (r.ProviderCurrencyConvertRowId != null)
                {
                    var item = await Context.TD_26_CurrencyConvert.Include(_ => _.TD_26)
                        .FirstOrDefaultAsync(_ => _.Id == r.ProviderCurrencyConvertRowId);
                    newItem.ProviderId = item?.TD_26?.DocId;
                }

                manul.Add(newItem);
            }

            if (project is null) return new List<ProjectDocumentInfo>();
            var exclInvoices = await Context.ProjectRowExclude.Include(_ => _.TD_26_CurrencyConvert)
                .AsNoTracking().Where(_ => _.ProjectId == projectId).ToListAsync();
            if (project.ProjectDocuments.Any(_ => _.InvoiceProviderId != null))
            {
                var providerInvoices = GetInvoiceProviders(
                    project.ProjectDocuments.Where(_ => _.InvoiceProviderId != null).ToList(), isShowAll,
                    exclInvoices).ToList();
                if (manul.Any(_ => _.ProviderId != null))
                    foreach (var p in providerInvoices)
                        if (manul.Any(_ => _.ProviderId == p.InvoiceProviderId))
                            p.IsManualChanged = true;

                ret.AddRange(providerInvoices);
            }

            if (project.ProjectDocuments.Any(_ => _.InvoiceClientId != null))
            {
                var clientInvoices = GetInvoiceClients(
                    project.ProjectDocuments.Where(_ => _.InvoiceClientId != null).ToList(),
                    isShowAll, exclInvoices).ToList();
                if (manul.Any(_ => _.ClientId != null))
                    foreach (var c in clientInvoices)
                        if (manul.Any(_ => _.ClientId == c.InvoiceClientId))
                            c.IsManualChanged = true;

                ret.AddRange(clientInvoices);
            }

            if (project.ProjectDocuments.Any(_ => _.BankCode != null))
                ret.AddRange(GetBanksForProject(project.ProjectDocuments.Where(_ => _.BankCode != null).ToList(),
                    isShowAll, exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.CashInDC != null))
                ret.AddRange(GetCashInsForProject(project.ProjectDocuments.Where(_ => _.CashInDC != null).ToList(),
                    isShowAll, exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.CashOutDC != null))
                ret.AddRange(GetCashInsForProject(project.ProjectDocuments.Where(_ => _.CashOutDC != null).ToList(),
                    isShowAll, exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.WarehouseOrderInDC != null))
                ret.AddRange(GetWarehouseForProject(
                    project.ProjectDocuments.Where(_ => _.WarehouseOrderInDC != null).ToList(), isShowAll,
                    exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.WaybillDC != null))
                ret.AddRange(GetWaybillsForProject(project.ProjectDocuments.Where(_ => _.WaybillDC != null).ToList(),
                    isShowAll, exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.UslugaProviderRowId != null))
                ret.AddRange(GetUslugaProvidersForProject(
                    project.ProjectDocuments.Where(_ => _.UslugaProviderRowId != null).ToList(), isShowAll,
                    exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.UslugaClientRowId != null))
                ret.AddRange(GetUslugaClientsForProject(
                    project.ProjectDocuments.Where(_ => _.UslugaClientRowId != null).ToList(), isShowAll,
                    exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.AccruedClientRowId != null))
                ret.AddRange(GetAccruedClientsForProject(
                    project.ProjectDocuments.Where(_ => _.AccruedClientRowId != null).ToList(), isShowAll,
                    exclInvoices));
            if (project.ProjectDocuments.Any(_ => _.AccruedSupplierRowId != null))
                ret.AddRange(GetAccruedSuppliersForProject(
                    project.ProjectDocuments.Where(_ => _.AccruedSupplierRowId != null).ToList(), isShowAll,
                    exclInvoices));


            return ret;
        }

        private IEnumerable<ProjectDocumentInfo> GetAccruedSuppliersForProject(List<ProjectDocuments> docs,
            bool isShowAll, List<ProjectRowExclude> exclInvoices)
        {
            var ret = new List<ProjectDocumentInfo>();
            foreach (var p in docs)
            {
                var newItem = new ProjectDocumentInfo(p);
                var row = Context
                    .AccuredAmountOfSupplierRow.Include(_ => _.AccruedAmountOfSupplier)
                    .FirstOrDefault(_ => _.Id == p.AccruedSupplierRowId);
                if (row != null)
                {
                    newItem.Kontragent =
                        GlobalOptions.ReferencesCache.GetKontragent(
                            row.AccruedAmountOfSupplier.KontrDC
                        ) as Kontragent;
                    newItem.Currency = newItem.Kontragent?.Currency as Currency;
                    newItem.SummaIn = row.Summa;
                    newItem.DocDate = row.AccruedAmountOfSupplier.DocDate;
                    newItem.InnerNumber = row.AccruedAmountOfSupplier.DocInNum;
                    newItem.ExtNumber = row.AccruedAmountOfSupplier.DocExtNum;
                    newItem.Note = row.AccruedAmountOfSupplier.Note;
                    newItem.Nomenkl =
                        GlobalOptions.ReferencesCache.GetNomenkl(row.NomenklDC) as Nomenkl;
                    newItem.DocumentType = DocumentType.AccruedAmountOfSupplier;
                    newItem.DocInfo = GetDocDescription(
                        DocumentType.AccruedAmountForClient,
                        newItem
                    );
                    newItem.Creator = row.AccruedAmountOfSupplier.Creator;
                }

                ret.Add(newItem);
            }

            return ret;
        }

        private IEnumerable<ProjectDocumentInfo> GetAccruedClientsForProject(List<ProjectDocuments> docs,
            bool isShowAll, List<ProjectRowExclude> exclInvoices)
        {
            var ret = new List<ProjectDocumentInfo>();
            foreach (var p in docs)
            {
                var newItem = new ProjectDocumentInfo(p);
                var row = Context
                    .AccuredAmountForClientRow.Include(_ => _.AccruedAmountForClient)
                    .FirstOrDefault(_ => _.Id == p.AccruedClientRowId);
                if (row != null)
                {
                    newItem.Kontragent =
                        GlobalOptions.ReferencesCache.GetKontragent(
                            row.AccruedAmountForClient.KontrDC
                        ) as Kontragent;
                    newItem.Currency = newItem.Kontragent?.Currency as Currency;
                    newItem.SummaIn = row.Summa;
                    newItem.DocDate = row.AccruedAmountForClient.DocDate;
                    newItem.InnerNumber = row.AccruedAmountForClient.DocInNum;
                    newItem.ExtNumber = row.AccruedAmountForClient.DocExtNum;
                    newItem.Note = row.AccruedAmountForClient.Note;
                    newItem.Nomenkl =
                        GlobalOptions.ReferencesCache.GetNomenkl(row.NomenklDC) as Nomenkl;
                    newItem.DocumentType = DocumentType.AccruedAmountForClient;
                    newItem.DocInfo = GetDocDescription(
                        DocumentType.AccruedAmountForClient,
                        newItem
                    );
                    newItem.Creator = row.AccruedAmountForClient.Creator;
                }

                ret.Add(newItem);
            }

            return ret;
        }

        private IEnumerable<ProjectDocumentInfo> GetUslugaClientsForProject(List<ProjectDocuments> docs, bool isShowAll,
            List<ProjectRowExclude> exclInvoices)
        {
            var ret = new List<ProjectDocumentInfo>();
            foreach (var p in docs)
            {
                var newItem = new ProjectDocumentInfo(p);
                var row = Context
                    .TD_84.Include(_ => _.SD_84)
                    .FirstOrDefault(_ => _.Id == p.UslugaClientRowId);
                if (row != null)
                {
                    newItem.Kontragent =
                        GlobalOptions.ReferencesCache.GetKontragent(row.SD_84.SF_CLIENT_DC)
                            as Kontragent;
                    newItem.Currency = newItem.Kontragent?.Currency as Currency;
                    newItem.SummaIn = (decimal)row.SFT_SUMMA_K_OPLATE_KONTR_CRS;
                    newItem.DocDate = row.SD_84.SF_DATE;
                    newItem.InnerNumber = row.SD_84.SF_IN_NUM;
                    newItem.ExtNumber = row.SD_84.SF_OUT_NUM;
                    newItem.Note = row.SD_84.SF_NOTE;
                    newItem.Nomenkl =
                        GlobalOptions.ReferencesCache.GetNomenkl(row.SFT_NEMENKL_DC)
                            as Nomenkl;
                    newItem.DocInfo = GetDocDescription(
                        DocumentType.InvoiceServiceClient,
                        newItem
                    );
                    newItem.Creator = row.SD_84.CREATOR;
                    newItem.DocumentType = DocumentType.InvoiceServiceClient;
                    newItem.ProductTypeName = (
                        (IName)
                        GlobalOptions.ReferencesCache.GetNomenklProductType(
                            row.SD_84.SF_VZAIMOR_TYPE_DC
                        )
                    )?.Name;
                }

                ret.Add(newItem);
            }

            return ret;
        }

        private IEnumerable<ProjectDocumentInfo> GetUslugaProvidersForProject(List<ProjectDocuments> docs,
            bool isShowAll, List<ProjectRowExclude> exclInvoices)
        {
            var ret = new List<ProjectDocumentInfo>();
            foreach (var p in docs)
            {
                var newItem = new ProjectDocumentInfo(p);
                var row = Context
                    .TD_26.Include(_ => _.SD_26)
                    .FirstOrDefault(_ => _.Id == p.UslugaProviderRowId);
                if (row != null)
                {
                    newItem.Kontragent =
                        GlobalOptions.ReferencesCache.GetKontragent(row.SD_26.SF_POST_DC)
                            as Kontragent;
                    newItem.Currency = newItem.Kontragent?.Currency as Currency;
                    newItem.SummaOut = (decimal)row.SFT_SUMMA_K_OPLATE_KONTR_CRS;
                    newItem.DocDate = row.SD_26.SF_POSTAV_DATE;
                    newItem.InnerNumber = row.SD_26.SF_IN_NUM;
                    newItem.ExtNumber = row.SD_26.SF_POSTAV_NUM;
                    newItem.Note = row.SD_26.SF_NOTES;
                    newItem.Nomenkl =
                        GlobalOptions.ReferencesCache.GetNomenkl(row.SFT_NEMENKL_DC)
                            as Nomenkl;
                    newItem.DocInfo = GetDocDescription(
                        DocumentType.InvoiceServiceProvider,
                        newItem
                    );
                    newItem.DocumentType = DocumentType.InvoiceServiceProvider;
                    newItem.Creator = row.SD_26.CREATOR;
                    newItem.ProductTypeName = (
                        (IName)
                        GlobalOptions.ReferencesCache.GetNomenklProductType(
                            row.SD_26.SF_VZAIMOR_TYPE_DC
                        )
                    )?.Name;
                    ret.Add(newItem);
                }
            }

            return ret;
        }

        private IEnumerable<ProjectDocumentInfo> GetWaybillsForProject(List<ProjectDocuments> docs, bool isShowAll,
            List<ProjectRowExclude> exclInvoices)
        {
            var ret = new List<ProjectDocumentInfo>();
            foreach (var p in docs)
            {
                var newItem = new ProjectDocumentInfo(p);
                var row = Context
                    .SD_24.Include(sd24 => sd24.TD_24.Select(td24 => td24.TD_84))
                    .Include(sd24 => sd24.SD_84)
                    .FirstOrDefault(_ => _.DOC_CODE == p.WaybillDC);
                if (row != null)
                {
                    newItem.HasExcludeRow = false;
                    newItem.Warehouse =
                        GlobalOptions.ReferencesCache.GetWarehouse(row.DD_SKLAD_OTPR_DC)
                            as Warehouse;
                    newItem.Kontragent =
                        GlobalOptions.ReferencesCache.GetKontragent(row.DD_KONTR_POL_DC)
                            as Kontragent;
                    newItem.Currency = newItem.Kontragent?.Currency as Currency;
                    newItem.SummaOut = row.TD_24.Sum(_ =>
                        _.DDT_KOL_RASHOD
                        * (_.TD_84.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0)
                        / (decimal)_.TD_84.SFT_KOL
                    );
                    newItem.SummaDiler = row.TD_24.Sum(_ =>
                        _.DDT_KOL_RASHOD
                        * (_.TD_84.SFT_NACENKA_DILERA ?? 0)
                        / (decimal)_.TD_84.SFT_KOL
                    );
                    newItem.DocDate = row.DD_DATE;
                    newItem.InnerNumber = row.DD_IN_NUM;
                    newItem.ExtNumber = row.DD_EXT_NUM;
                    newItem.Note = row.DD_NOTES;
                    newItem.Creator = row.CREATOR;
                    newItem.ProductTypeName = (
                        (IName)
                        GlobalOptions.ReferencesCache.GetNomenklProductType(
                            row.SD_84?.SF_VZAIMOR_TYPE_DC
                        )
                    )?.Name;
                    ret.Add(newItem);
                }
            }

            return ret;
        }

        private IEnumerable<ProjectDocumentInfo> GetWarehouseForProject(List<ProjectDocuments> docs, bool isShowAll,
            List<ProjectRowExclude> exclInvoices)
        {
            var ret = new List<ProjectDocumentInfo>();
            foreach (var p in docs)
            {
                var newItem = new ProjectDocumentInfo(p);
                var ord = Context
                    .SD_24.Include(sd24 => sd24.TD_24.Select(td24 => td24.TD_26))
                    .Include(sd24 => sd24.SD_26)
                    .FirstOrDefault(_ => _.DOC_CODE == p.WarehouseOrderInDC);
                if (ord != null)
                {
                    newItem.HasExcludeRow = false;
                    newItem.Warehouse =
                        GlobalOptions.ReferencesCache.GetWarehouse(ord.DD_SKLAD_POL_DC)
                            as Warehouse;
                    newItem.Kontragent =
                        GlobalOptions.ReferencesCache.GetKontragent(ord.DD_KONTR_OTPR_DC)
                            as Kontragent;
                    newItem.Currency =
                        GlobalOptions
                            .ReferencesCache.GetKontragent(ord.DD_KONTR_OTPR_DC)
                            ?.Currency as Currency;
                    newItem.SummaIn = ord.TD_24.Sum(_ =>
                        _.DDT_KOL_PRIHOD
                        * (_.TD_26.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0)
                        / _.TD_26.SFT_KOL
                    );
                    newItem.DocDate = ord.DD_DATE;
                    newItem.InnerNumber = ord.DD_IN_NUM;
                    newItem.ExtNumber = ord.DD_EXT_NUM;
                    newItem.DocInfo = GetDocDescription(DocumentType.StoreOrderIn, newItem);
                    newItem.Note = ord.DD_NOTES;
                    newItem.Creator = ord.CREATOR;
                    newItem.ProductTypeName = (
                        (IName)
                        GlobalOptions.ReferencesCache.GetNomenklProductType(
                            ord.SD_26?.SF_VZAIMOR_TYPE_DC
                        )
                    )?.Name;
                    ret.Add(newItem);
                }
            }

            return ret;
        }

        private IEnumerable<ProjectDocumentInfo> GetCashOutsForProject(List<ProjectDocuments> docs, bool isShowAll,
            List<ProjectRowExclude> exclInvoices)
        {
            var ret = new List<ProjectDocumentInfo>();
            foreach (var p in docs)
            {
                var newItem = new ProjectDocumentInfo(p);
                var chOut = Context.SD_34.FirstOrDefault(_ => _.DOC_CODE == p.CashOutDC);
                if (chOut != null)
                {
                    newItem.CashBox =
                        GlobalOptions.ReferencesCache.GetCashBox(chOut.CA_DC) as CashBox;
                    newItem.Currency =
                        GlobalOptions.ReferencesCache.GetCurrency(chOut.CRS_DC) as Currency;
                    newItem.Kontragent = chOut.KONTRAGENT_DC is not null
                        ? GlobalOptions.ReferencesCache.GetKontragent(chOut.KONTRAGENT_DC)
                            as Kontragent
                        : null;
                    newItem.Employee = chOut.TABELNUMBER is not null
                        ? GlobalOptions.ReferencesCache.GetEmployee(chOut.TABELNUMBER)
                            as Employee
                        : null;
                    newItem.SummaIn = (decimal)chOut.CRS_SUMMA;
                    newItem.DocDate = (DateTime)chOut.DATE_ORD;
                    newItem.InnerNumber = chOut.NUM_ORD;
                    newItem.Note = chOut.NOTES_ORD;
                    newItem.DocInfo = GetDocDescription(DocumentType.CashOut, newItem);
                    newItem.Creator = chOut.CREATOR;
                    newItem.DocumentType = DocumentType.CashOut;
                }

                ret.Add(newItem);
            }

            return ret;
        }

        private IEnumerable<ProjectDocumentInfo> GetCashInsForProject(List<ProjectDocuments> docs, bool isShowAll,
            List<ProjectRowExclude> exclInvoices)
        {
            var ret = new List<ProjectDocumentInfo>();
            foreach (var p in docs)
            {
                var newItem = new ProjectDocumentInfo(p);
                var chIn = Context.SD_33.FirstOrDefault(_ => _.DOC_CODE == p.CashInDC);
                if (chIn != null)
                {
                    newItem.CashBox =
                        GlobalOptions.ReferencesCache.GetCashBox(chIn.CA_DC) as CashBox;
                    newItem.Currency =
                        GlobalOptions.ReferencesCache.GetCurrency(chIn.CRS_DC) as Currency;
                    newItem.Kontragent =
                        GlobalOptions.ReferencesCache.GetKontragent(chIn.KONTRAGENT_DC)
                            as Kontragent;
                    newItem.SummaIn = (decimal)chIn.CRS_SUMMA;
                    newItem.DocDate = (DateTime)chIn.DATE_ORD;
                    newItem.InnerNumber = chIn.NUM_ORD;
                    newItem.Note = chIn.NOTES_ORD;
                    newItem.DocInfo = GetDocDescription(DocumentType.CashIn, newItem);
                    newItem.Creator = chIn.CREATOR;
                    newItem.DocumentType = DocumentType.CashIn;
                }

                ret.Add(newItem);
            }

            return ret;
        }

        private IEnumerable<ProjectDocumentInfo> GetBanksForProject(List<ProjectDocuments> docs, bool isShowAll,
            List<ProjectRowExclude> exclInvoices)
        {
            var ret = new List<ProjectDocumentInfo>();
            foreach (var p in docs)
            {
                var newItem = new ProjectDocumentInfo(p);
                var bb = Context
                    .TD_101.Include(_ => _.SD_101)
                    .FirstOrDefault(_ => _.CODE == p.BankCode);
                if (bb != null)
                {
                    newItem.BankAccount = (
                        (IName)
                        GlobalOptions.ReferencesCache.GetBankAccount(
                            bb.SD_101.VV_ACC_DC
                        )
                    )?.Name;
                    newItem.Currency =
                        GlobalOptions
                            .ReferencesCache.GetBankAccount(bb.SD_101.VV_ACC_DC)
                            ?.Currency as Currency;
                    if (bb.VVT_VAL_PRIHOD > 0)
                        newItem.SummaIn = bb.VVT_VAL_PRIHOD.Value;
                    else
                        newItem.SummaOut = bb.VVT_VAL_RASHOD.Value;
                    newItem.DocDate = bb.SD_101.VV_START_DATE;
                    newItem.Kontragent =
                        GlobalOptions.ReferencesCache.GetKontragent(bb.VVT_KONTRAGENT)
                            as Kontragent;
                    newItem.DocInfo = GetDocDescription(DocumentType.Bank, newItem);
                    newItem.DocumentType = DocumentType.Bank;
                }

                ret.Add(newItem);
            }

            return ret;
        }


        private IEnumerable<ProjectDocumentInfo> GetInvoiceProviders(ICollection<ProjectDocuments> docs, bool isShowAll,
            List<ProjectRowExclude> exclInvoices)
        {
            var ret = new List<ProjectDocumentInfo>();
            var invoices = new List<InvoiceProvider>();
            var ids = docs.Where(_ => _.InvoiceProviderId != null).Select(_ => _.InvoiceProviderId).ToList();
            var data = Context.SD_26.Include(_ => _.TD_26).Include("TD_26.TD_24").Where(_ => ids.Contains(_.Id));
            foreach (var d in docs.Where(_ => _.InvoiceProviderId != null))
            {
                var item = data.FirstOrDefault(_ => _.Id == d.InvoiceProviderId);
                if (item == null) continue;
                var newItem = InvoiceProvider.GetInvoiceProviderForProject(item, d.ProjectId);
                invoices.Add(newItem);
            }

            foreach (var p in docs)
            {
                var newItem = new ProjectDocumentInfo(p);
                var inv = invoices.FirstOrDefault(_ => _.Id == p.InvoiceProviderId);
                if (inv is null) continue;
                var exIds = inv.Rows.Where(_ => exclInvoices.Select(x => x.SFProviderRowId).Contains(_.Id))
                    .Select(y => y.Id).ToList();
                var exCrsRows = Context.TD_26_CurrencyConvert.AsNoTracking().Include(_ => _.TD_26)
                    .Where(_ => _.DOC_CODE == inv.DocCode).ToList();
                var exCrsIds = exCrsRows.Where(_ => exclInvoices.Select(x => x.NomCurrencyConvertRowId).Contains(_.Id))
                    .Select(y => y.Id).ToList();
                newItem.HasExcludeRow = exIds.Any() || exCrsIds.Any();
                newItem.Kontragent = inv.Kontragent;
                newItem.SummaPay = inv.PaySumma;
                newItem.Currency = inv.Currency;
                newItem.InnerNumber = inv.SF_IN_NUM;
                newItem.ExtNumber = inv.SF_POSTAV_NUM;
                newItem.DocumentType = DocumentType.InvoiceProvider;
                newItem.UslugaSummaIn = inv.Rows.Where(_ => _.IsUsluga).Sum(_ => _.Summa);

                if (newItem.HasExcludeRow && !isShowAll)
                {
                    var exRows = inv.Rows.Where(_ => exIds.Contains(_.Id)).ToList();
                    newItem.QuantityInDocument = inv.Rows.Sum(_ => _.Quantity) - exRows.Sum(_ => _.Quantity) -
                                                 exCrsRows.Sum(_ => _.Quantity);
                    newItem.QuantityInShipped = inv.Rows.Sum(_ => _.Shipped) -
                                                inv.Rows.Where(_ => exIds.Contains(_.Id)).Sum(_ => _.Shipped)
                                                - exCrsRows.Sum(_ => _.Quantity);
                    newItem.SummaShipped = (decimal)(inv.SummaFact - exRows.Sum(_ => _.Shipped * _.Summa) -
                                                     -exCrsRows.Sum(_ =>
                                                         _.TD_26.SFT_SUMMA_K_OPLATE_KONTR_CRS * _.Quantity /
                                                         _.TD_26.SFT_KOL));
                    newItem.SummaIn = (decimal)(inv.Summa - newItem.UslugaSummaIn - exRows
                        .Where(x => exCrsIds.Contains(x.Id))
                        .Sum(_ => _.Shipped * _.Summa) - exCrsRows.Where(x => exCrsIds.Contains(x.Id))
                        .Sum(_ => _.TD_26.SFT_SUMMA_K_OPLATE_KONTR_CRS * _.Quantity / _.TD_26.SFT_KOL));
                }
                else
                {
                    newItem.QuantityInDocument = inv.Rows.Sum(_ => _.Quantity);
                    newItem.QuantityInShipped = inv.Rows.Sum(_ => _.Shipped);
                    newItem.SummaShipped = inv.SummaFact - newItem.UslugaSummaIn;
                    newItem.SummaIn = inv.Summa - newItem.UslugaSummaIn;
                }

                newItem.QuantityInRemain =
                    newItem.QuantityInDocument - newItem.QuantityInShipped;
                newItem.SummaInRemain = newItem.SummaIn - newItem.SummaShipped;
                newItem.DocDate = inv.DocDate;
                newItem.InnerNumber = inv.SF_IN_NUM;
                newItem.ExtNumber = inv.SF_POSTAV_NUM;
                newItem.Note = inv.Note;
                newItem.DocInfo = GetDocDescription(
                    DocumentType.InvoiceClient,
                    newItem
                );
                newItem.Creator = inv.CREATOR;
                newItem.ProductTypeName = inv.VzaimoraschetType?.Name;

                if (isShowAll || inv.Rows.Any(_ => !exIds.Contains(_.Id)))
                    ret.Add(newItem);
            }

            return ret;
        }

        private IEnumerable<ProjectDocumentInfo> GetInvoiceClients(ICollection<ProjectDocuments> docs, bool isShowAll,
            List<ProjectRowExclude> exclInvoices)
        {
            var ret = new List<ProjectDocumentInfo>();
            var invoices = new List<InvoiceClientBase>();
            var ids = docs.Where(_ => _.InvoiceClientId != null).Select(_ => _.InvoiceClientId).ToList();
            var data = Context.SD_84.Include(_ => _.TD_84).Include("TD_84.TD_24").Where(_ => ids.Contains(_.Id))
                .ToList();
            foreach (var d in docs.Where(_ => _.InvoiceClientId != null))
            {
                var item = data.FirstOrDefault(_ => _.Id == d.InvoiceClientId);
                if (item == null) continue;
                var newItem = InvoiceClientBase.GetInvoiceForProject(item, d.ProjectId);
                invoices.Add(newItem);
            }
            //var invoices = docs.Select(_ => _.InvoiceClientId)
            //    .Select(id => 
            //    .Where(item => item != null)
            //    .Select(inv => new InvoiceClientBase(inv))
            //    .ToList();

            foreach (var p in docs)
            {
                var newItem = new ProjectDocumentInfo(p);
                var inv = invoices.FirstOrDefault(_ => _.Id == p.InvoiceClientId);
                if (inv is null) continue;
                var exIds = inv.Rows.Where(_ => exclInvoices.Select(x => x.SFClientRowId).Contains(_.Id))
                    .Select(y => y.Id).ToList();
                newItem.HasExcludeRow = exIds.Count > 0;
                newItem.Kontragent = inv.Client;
                newItem.SummaPay = inv.PaySumma;
                newItem.Currency = inv.Currency;
                newItem.InnerNumber = inv.InnerNumber;
                newItem.ExtNumber = inv.OuterNumber;
                newItem.DocumentType = DocumentType.InvoiceClient;

                if (newItem.HasExcludeRow && !isShowAll)
                {
                    var exRows = inv.Rows.Where(_ => exIds.Contains(_.Id)).ToList();
                    newItem.QuantityOutDocument = inv.Rows.Sum(_ => _.Quantity) - exRows.Sum(_ => _.Quantity);
                    newItem.QuantityOutShipped = inv.Rows.Sum(_ => _.Shipped) -
                                                 inv.Rows.Where(_ => exIds.Contains(_.Id)).Sum(_ => _.Shipped);
                    newItem.SummaShipped = inv.SummaOtgruz - exRows.Sum(_ => _.Shipped * _.Summa);
                    newItem.SummaOut = inv.Summa - newItem.UslugaSummaIn - exRows.Sum(_ => _.Shipped * _.Summa);
                }
                else
                {
                    newItem.QuantityOutDocument = inv.Rows.Sum(_ => _.Quantity);
                    newItem.QuantityOutShipped = inv.Rows.Sum(_ => _.Shipped);
                    newItem.UslugaSummaOut = inv.Rows.Where(_ => _.IsUsluga).Sum(_ => _.Summa);
                    newItem.SummaShipped = inv.SummaOtgruz - newItem.UslugaSummaIn;
                    newItem.SummaOut = inv.Summa - newItem.UslugaSummaIn;
                }

                newItem.QuantityOutRemain =
                    newItem.QuantityOutDocument - newItem.QuantityOutShipped;
                newItem.SummaOutRemain = newItem.SummaOut - newItem.SummaShipped;
                newItem.DocDate = inv.DocDate;
                newItem.InnerNumber = inv.InnerNumber;
                newItem.ExtNumber = inv.OuterNumber;
                newItem.Note = inv.Note;
                newItem.DocInfo = GetDocDescription(
                    DocumentType.InvoiceClient,
                    newItem
                );
                newItem.Creator = inv.CREATOR;
                newItem.ProductTypeName = inv.VzaimoraschetType?.Name;


                if (isShowAll || inv.Rows.Any(_ => !exIds.Contains(_.Id)))
                    ret.Add(newItem);
            }

            return ret;
        }

        public List<Guid> GetDocumentsProjects(DocumentType docType, decimal dc, bool isCrsConvert)
        {
            var sqlClient =
                $@"SELECT pd.ProjectId FROM ProjectDocuments pd
                        INNER JOIN SD_84 ON pd.InvoiceClientId = SD_84.Id
                        AND SD_84.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(dc)}";
            var sqlProvider = isCrsConvert
                ? $@"SELECT pd.ProjectId FROM ProjectDocuments pd
                        INNER JOIN TD_26_CurrencyConvert t ON pd.CurrencyConvertId = t.Id AND t.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(dc)}"
                : $@"SELECT pd.ProjectId FROM ProjectDocuments pd
                        INNER JOIN SD_26 ON pd.InvoiceProviderId = SD_26.Id 
                            AND SD_26.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(dc)}";
            var sqlBank =
                $@"select pd.ProjectId from ProjectDocuments pd
	                            inner join TD_101 t101 on t101.CODE = pd.BankCode and t101.CODE = cast({CustomFormat.DecimalToSqlDecimal(dc)} as int)";
            var sqlCashIn =
                $@"select pd.ProjectId from ProjectDocuments pd
	                            inner join SD_33 s33 on s33.DOC_CODE = pd.CashInDC and s33.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(dc)}";
            var sqlCashOut =
                $@"select pd.ProjectId from ProjectDocuments pd
	                            inner join SD_34 s34 on s34.DOC_CODE = pd.CashOutDC and s34.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(dc)}";
            var sqlWaybill =
                $@"select pd.ProjectId from ProjectDocuments pd
	                            inner join SD_24 s24 on s24.DOC_CODE = pd.WaybillDC and s24.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(dc)}";
            var sqlStoreIn =
                $@"select pd.ProjectId from ProjectDocuments pd
	                            inner join SD_24 s24 on s24.DOC_CODE = pd.[WarehouseOrderInDC] and s24.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(dc)}";
            return docType switch
            {
                DocumentType.InvoiceClient => Context
                    .Database.SqlQuery<Guid>(sqlClient)
                    .Distinct()
                    .ToList(),
                DocumentType.InvoiceProvider => Context
                    .Database.SqlQuery<Guid>(sqlProvider)
                    .Distinct()
                    .ToList(),
                DocumentType.Bank => Context.Database.SqlQuery<Guid>(sqlBank).Distinct().ToList(),
                DocumentType.CashIn => Context
                    .Database.SqlQuery<Guid>(sqlCashIn)
                    .Distinct()
                    .ToList(),
                DocumentType.CashOut => Context
                    .Database.SqlQuery<Guid>(sqlCashOut)
                    .Distinct()
                    .ToList(),
                DocumentType.Waybill => Context
                    .Database.SqlQuery<Guid>(sqlWaybill)
                    .Distinct()
                    .ToList(),
                DocumentType.StoreOrderIn => Context
                    .Database.SqlQuery<Guid>(sqlStoreIn)
                    .Distinct()
                    .ToList(),
                _ => new List<Guid>()
            };
        }

        public Dictionary<IdItem, string> GetDocumentsLinkWithProjects(DocumentType docType)
        {
            var sql = docType switch
            {
                DocumentType.InvoiceProvider =>
                    @"select SD_26.DOC_CODE as DocCode, p.Name as ProjectName  from ProjectDocuments pd
	                            inner join Projects p ON p.Id = pd.ProjectId
	                        inner join SD_26 on sd_26.Id = pd.InvoiceProviderId",
                DocumentType.InvoiceClient =>
                    @"select SD_84.DOC_CODE as DocCode, p.Name as ProjectName  from ProjectDocuments pd
	                            inner join Projects p ON p.Id = pd.ProjectId
	                        inner join SD_84 on sd_84.Id = pd.InvoiceClientId",
                DocumentType.Bank =>
                    @"select td_101.CODE as Code, p.Name as ProjectName  from ProjectDocuments pd
	                            inner join Projects p ON p.Id = pd.ProjectId
	                        inner join TD_101 on TD_101.CODE = pd.BankCode",
                DocumentType.CashIn =>
                    @"select SD_33.DOC_CODE as DocCode, p.Name as ProjectName  from ProjectDocuments pd
	                            inner join Projects p ON p.Id = pd.ProjectId
	                            inner join SD_33 on SD_33.DOC_CODE = pd.CashInDC",
                DocumentType.CashOut =>
                    @"select SD_34.DOC_CODE as DocCode, p.Name as ProjectName  from ProjectDocuments pd
	                            inner join Projects p ON p.Id = pd.ProjectId
	                            inner join SD_34 on SD_34.DOC_CODE = pd.CashOutDC",
                DocumentType.Waybill =>
                    @"select SD_24.DOC_CODE as DocCode, p.Name as ProjectName  from ProjectDocuments pd
	                            inner join Projects p ON p.Id = pd.ProjectId
	                            inner join SD_24 on SD_24.DOC_CODE = pd.WaybillDC",
                DocumentType.StoreOrderIn =>
                    @"select SD_24.DOC_CODE as DocCode, p.Name as ProjectName  from ProjectDocuments pd
	                            inner join Projects p ON p.Id = pd.ProjectId
	                            inner join SD_24 on SD_24.DOC_CODE = pd.WarehouseOrderInDC",

                _ => null
            };
            var res = new Dictionary<IdItem, string>();
            switch (docType)
            {
                case DocumentType.Bank:
                    var dataBank = Context.Database.SqlQuery<DocIntProjLink>(sql).ToList();
                    foreach (var code in dataBank.Select(_ => _.Code).Distinct())
                    {
                        var names = new StringBuilder();
                        foreach (var item in dataBank.Where(_ => _.Code == code))
                            names.Append($"{item.ProjectName}; ");

                        res.Add(new IdItem { Code = code }, names.ToString());
                    }

                    break;
                case DocumentType.InvoiceProvider:
                case DocumentType.InvoiceClient:
                case DocumentType.CashIn:
                case DocumentType.CashOut:
                case DocumentType.Waybill:
                    var data = Context.Database.SqlQuery<DocDecimalProjLink>(sql).ToList();
                    foreach (var dc in data.Select(_ => _.DocCode).Distinct())
                    {
                        var names = new StringBuilder();
                        foreach (var item in data.Where(_ => _.DocCode == dc))
                            names.Append($"{item.ProjectName}; ");

                        res.Add(new IdItem { DocCode = dc }, names.ToString());
                    }

                    break;
                case DocumentType.AccruedAmountForClient:
                case DocumentType.AccruedAmountOfSupplier:
                    var dataAcc = Context.Database.SqlQuery<DocGuidProjLink>(sql).ToList();
                    foreach (var id in dataAcc.Select(_ => _.Id).Distinct())
                    {
                        var names = new StringBuilder();
                        foreach (var item in dataAcc.Where(_ => _.Id == id))
                            names.Append($"{item.ProjectName}; ");

                        res.Add(new IdItem { Id = id }, names.ToString());
                    }

                    break;
            }

            return res;
        }

        public Dictionary<decimal, string> GetInvoicesLinkWithProjects(
            DocumentType docType,
            bool isCrsConvert
        )
        {
            var sql = docType switch
            {
                DocumentType.InvoiceProvider =>
                    @"select SD_26.DOC_CODE as DocCode, p.Name as ProjectName  from ProjectDocuments pd
	                            inner join Projects p ON p.Id = pd.ProjectId
	                        inner join SD_26 on sd_26.Id = pd.InvoiceProviderId",
                DocumentType.InvoiceClient =>
                    @"select SD_84.DOC_CODE as DocCode, p.Name as ProjectName  from ProjectDocuments pd
	                            inner join Projects p ON p.Id = pd.ProjectId
	                        inner join SD_84 on sd_84.Id = pd.InvoiceClientId",
                _ => null
            };

            var data = Context.Database.SqlQuery<DocDecimalProjLink>(sql).ToList();
            var res = new Dictionary<decimal, string>();
            foreach (var dc in data.Select(_ => _.DocCode).Distinct())
            {
                var names = new StringBuilder();
                foreach (var item in data.Where(_ => _.DocCode == dc))
                    names.Append($"{item.ProjectName}; ");
                res.Add(dc, names.ToString());
            }

            return res;
        }

        public IEnumerable<ProjectDocumentInfo> LoadProjectDocuments(Data.Projects project, bool isShowAll)
        {
            var ret = new List<ProjectDocumentInfo>();
            var excl = GetDocumentsRowExclude(new List<Guid> { project.Id }).Select(_ => _.NomenklDC).ToList();
            var exclInvoices = Context.ProjectRowExclude.Include(_ => _.TD_26_CurrencyConvert)
                .Where(_ => _.ProjectId == project.Id).ToList();
            if (project.ProjectDocuments is not null && project.ProjectDocuments.Count > 0)
                foreach (var p in project.ProjectDocuments)
                {
                    var newItem = new ProjectDocumentInfo(p);
                    if (p.BankCode is not null)
                    {
                        var bb = Context
                            .TD_101.Include(_ => _.SD_101)
                            .FirstOrDefault(_ => _.CODE == p.BankCode);
                        if (bb != null)
                        {
                            newItem.BankAccount = (
                                (IName)
                                GlobalOptions.ReferencesCache.GetBankAccount(
                                    bb.SD_101.VV_ACC_DC
                                )
                            )?.Name;
                            newItem.Currency =
                                GlobalOptions
                                    .ReferencesCache.GetBankAccount(bb.SD_101.VV_ACC_DC)
                                    ?.Currency as Currency;
                            if (bb.VVT_VAL_PRIHOD > 0)
                                newItem.SummaIn = bb.VVT_VAL_PRIHOD.Value;
                            else
                                newItem.SummaOut = bb.VVT_VAL_RASHOD.Value;
                            newItem.DocDate = bb.SD_101.VV_START_DATE;
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(bb.VVT_KONTRAGENT)
                                    as Kontragent;
                            newItem.DocInfo = GetDocDescription(DocumentType.Bank, newItem);
                        }
                    }

                    if (p.CashInDC is not null)
                    {
                        var chIn = Context.SD_33.FirstOrDefault(_ => _.DOC_CODE == p.CashInDC);
                        if (chIn != null)
                        {
                            newItem.CashBox =
                                GlobalOptions.ReferencesCache.GetCashBox(chIn.CA_DC) as CashBox;
                            newItem.Currency =
                                GlobalOptions.ReferencesCache.GetCurrency(chIn.CRS_DC) as Currency;
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(chIn.KONTRAGENT_DC)
                                    as Kontragent;
                            newItem.SummaIn = (decimal)chIn.CRS_SUMMA;
                            newItem.DocDate = (DateTime)chIn.DATE_ORD;
                            newItem.InnerNumber = chIn.NUM_ORD;
                            newItem.Note = chIn.NOTES_ORD;
                            newItem.DocInfo = GetDocDescription(DocumentType.CashIn, newItem);
                            newItem.Creator = chIn.CREATOR;
                        }
                    }

                    if (p.CashOutDC is not null)
                    {
                        var chOut = Context.SD_34.FirstOrDefault(_ => _.DOC_CODE == p.CashOutDC);
                        if (chOut != null)
                        {
                            newItem.CashBox =
                                GlobalOptions.ReferencesCache.GetCashBox(chOut.CA_DC) as CashBox;
                            newItem.Currency =
                                GlobalOptions.ReferencesCache.GetCurrency(chOut.CRS_DC) as Currency;
                            newItem.Kontragent = chOut.KONTRAGENT_DC is not null
                                ? GlobalOptions.ReferencesCache.GetKontragent(chOut.KONTRAGENT_DC)
                                    as Kontragent
                                : null;
                            newItem.Employee = chOut.TABELNUMBER is not null
                                ? GlobalOptions.ReferencesCache.GetEmployee(chOut.TABELNUMBER)
                                    as Employee
                                : null;
                            newItem.SummaIn = (decimal)chOut.CRS_SUMMA;
                            newItem.DocDate = (DateTime)chOut.DATE_ORD;
                            newItem.InnerNumber = chOut.NUM_ORD;
                            newItem.Note = chOut.NOTES_ORD;
                            newItem.DocInfo = GetDocDescription(DocumentType.CashOut, newItem);
                            newItem.Creator = chOut.CREATOR;
                        }
                    }

                    if (p.WarehouseOrderInDC is not null)
                    {
                        var ord = Context
                            .SD_24.Include(sd24 => sd24.TD_24.Select(td24 => td24.TD_26))
                            .Include(sd24 => sd24.SD_26)
                            .FirstOrDefault(_ => _.DOC_CODE == p.WarehouseOrderInDC);
                        if (ord != null)
                        {
                            newItem.HasExcludeRow = ord.TD_24.Any(_ => excl.All(x => x != _.DDT_NOMENKL_DC));
                            newItem.Warehouse =
                                GlobalOptions.ReferencesCache.GetWarehouse(ord.DD_SKLAD_POL_DC)
                                    as Warehouse;
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(ord.DD_KONTR_OTPR_DC)
                                    as Kontragent;
                            newItem.Currency =
                                GlobalOptions
                                    .ReferencesCache.GetKontragent(ord.DD_KONTR_OTPR_DC)
                                    ?.Currency as Currency;
                            newItem.SummaIn = ord.TD_24.Where(_ => excl.All(x => x != _.DDT_NOMENKL_DC)).Sum(_ =>
                                _.DDT_KOL_PRIHOD
                                * (_.TD_26.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0)
                                / _.TD_26.SFT_KOL
                            );
                            newItem.DocDate = ord.DD_DATE;
                            newItem.InnerNumber = ord.DD_IN_NUM;
                            newItem.ExtNumber = ord.DD_EXT_NUM;
                            newItem.DocInfo = GetDocDescription(DocumentType.StoreOrderIn, newItem);
                            newItem.Note = ord.DD_NOTES;
                            newItem.Creator = ord.CREATOR;
                            newItem.ProductTypeName = (
                                (IName)
                                GlobalOptions.ReferencesCache.GetNomenklProductType(
                                    ord.SD_26?.SF_VZAIMOR_TYPE_DC
                                )
                            )?.Name;
                        }
                    }

                    if (p.CurrencyConvertId is not null)
                    {
                        var ord = Context
                            .TD_26_CurrencyConvert.Include(_ => _.TD_26)
                            .Include(_ => _.TD_26.SD_26)
                            .FirstOrDefault(_ => _.Id == p.CurrencyConvertId);
                        var n = GlobalOptions.ReferencesCache.GetNomenkl(ord.NomenklId);
                        if (ord != null && excl.All(_ => _ != ((IDocCode)n).DocCode))
                        {
                            var kontr =
                                GlobalOptions.ReferencesCache.GetKontragent(
                                    ord.TD_26.SD_26.SF_POST_DC
                                ) as Kontragent;
                            newItem.HasExcludeRow = exclInvoices.Any(_ => _.NomCurrencyConvertRowId == ord.Id);
                            newItem.Nomenkl =
                                GlobalOptions.ReferencesCache.GetNomenkl(ord.NomenklId) as Nomenkl;
                            newItem.Warehouse =
                                GlobalOptions.ReferencesCache.GetWarehouse(ord.StoreDC)
                                    as Warehouse;
                            newItem.Kontragent = kontr;
                            newItem.Currency = newItem.Nomenkl.Currency as Currency;
                            newItem.SummaOut = ord.Summa;
                            newItem.DocDate = ord.TD_26.SD_26.SF_POSTAV_DATE;
                            newItem.InnerNumber = ord.TD_26.SD_26.SF_IN_NUM;
                            newItem.ExtNumber = ord.TD_26.SD_26.SF_POSTAV_NUM;
                            newItem.DocInfo = GetDocDescription(
                                DocumentType.InvoiceProvider,
                                newItem
                            );
                            newItem.Note = ord.TD_26.SD_26.SF_NOTES;
                            newItem.Creator = ord.TD_26.SD_26.CREATOR;
                            newItem.ProductTypeName = (
                                (IName)
                                GlobalOptions.ReferencesCache.GetNomenklProductType(
                                    ord.TD_26.SD_26?.SF_VZAIMOR_TYPE_DC
                                )
                            )?.Name;
                        }
                    }

                    if (p.WaybillDC is not null)
                    {
                        var row = Context
                            .SD_24.Include(sd24 => sd24.TD_24.Select(td24 => td24.TD_84))
                            .Include(sd24 => sd24.SD_84)
                            .FirstOrDefault(_ => _.DOC_CODE == p.WaybillDC);
                        if (row != null)
                        {
                            newItem.HasExcludeRow = row.TD_24.Any(_ => excl.All(x => x != _.DDT_NOMENKL_DC));
                            newItem.Warehouse =
                                GlobalOptions.ReferencesCache.GetWarehouse(row.DD_SKLAD_OTPR_DC)
                                    as Warehouse;
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(row.DD_KONTR_POL_DC)
                                    as Kontragent;
                            newItem.Currency = newItem.Kontragent?.Currency as Currency;
                            newItem.SummaOut = row.TD_24.Where(_ => excl.All(x => x != _.DDT_NOMENKL_DC)).Sum(_ =>
                                _.DDT_KOL_RASHOD
                                * (_.TD_84.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0)
                                / (decimal)_.TD_84.SFT_KOL
                            );
                            newItem.SummaDiler = row.TD_24.Where(_ => excl.All(x => x != _.DDT_NOMENKL_DC)).Sum(_ =>
                                _.DDT_KOL_RASHOD
                                * (_.TD_84.SFT_NACENKA_DILERA ?? 0)
                                / (decimal)_.TD_84.SFT_KOL
                            );
                            newItem.DocDate = row.DD_DATE;
                            newItem.InnerNumber = row.DD_IN_NUM;
                            newItem.ExtNumber = row.DD_EXT_NUM;
                            newItem.Note = row.DD_NOTES;
                            newItem.Creator = row.CREATOR;
                            newItem.ProductTypeName = (
                                (IName)
                                GlobalOptions.ReferencesCache.GetNomenklProductType(
                                    row.SD_84?.SF_VZAIMOR_TYPE_DC
                                )
                            )?.Name;
                        }
                    }

                    if (p.InvoiceProviderId is not null)
                    {
                        var d = Context
                            .InvoicePostQuery.Where(_ => _.Id == p.InvoiceProviderId)
                            .ToList();
                        decimal exQuan = 0, exSumma = 0, exSummaShipped = 0, exQuanShipped = 0;
                        if (d != null)
                        {
                            var doc = new InvoiceProviderBase(d);
                            var hasExclude = false;
                            foreach (var ex in from ex in exclInvoices
                                     from dd in d
                                     where dd.RowId == ex.SFProviderRowId
                                     select ex)
                            {
                                hasExclude = true;
                                var r = Context.TD_26.Include(_ => _.TD_24)
                                    .FirstOrDefault(_ => _.Id == ex.SFProviderRowId);
                                if (r is not null)
                                {
                                    exQuan += r.SFT_KOL;
                                    exSumma += r.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0;
                                    exQuanShipped += r.TD_24.Sum(_ => _.DDT_KOL_PRIHOD);
                                    if (r.TD_24.Sum(_ => _.DDT_KOL_PRIHOD) > 0)
                                        exSummaShipped += (r.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0) * r.SFT_KOL /
                                                          r.TD_24.Sum(_ => _.DDT_KOL_PRIHOD);
                                }
                            }

                            if (!hasExclude)
                            {
                                var dc = d.First().DocCode;
                                foreach (var c in Context.TD_26_CurrencyConvert.Where(_ => _.DOC_CODE == dc))
                                {
                                    if (exclInvoices.All(_ => _.NomCurrencyConvertRowId != c.Id)) continue;
                                    hasExclude = true;
                                    break;
                                }

                                if (hasExclude)
                                {
                                }
                            }

                            newItem.HasExcludeRow = hasExclude;
                            newItem.Kontragent = doc.Kontragent;
                            newItem.Currency = newItem.Kontragent?.Currency as Currency;
                            newItem.SummaIn = hasExclude ? doc.Summa - exSumma : doc.Summa;
                            newItem.SummaPay = doc.PaySumma;
                            newItem.SummaShipped = hasExclude ? doc.SummaFact - exSummaShipped : doc.SummaFact;
                            newItem.QuantityInDocument = d.Where(_ => _.DocCode == doc.DocCode)
                                .Sum(_ => _.Quantity);
                            foreach (var r in d)
                                if (r.IsUsluga ?? false)
                                {
                                    newItem.QuantityInShipped += r.Quantity;
                                    newItem.UslugaSummaOut += r.Summa ?? 0;
                                }
                                else
                                {
                                    newItem.QuantityInShipped += d.Where(_ =>
                                            _.DocCode == doc.DocCode
                                        )
                                        .Sum(_ => _.Shipped);
                                }

                            newItem.QuantityInRemain =
                                newItem.QuantityInDocument - newItem.QuantityInShipped;
                            newItem.SummaInRemain = hasExclude
                                ? newItem.SummaShipped - newItem.SummaIn - exSumma - exSummaShipped
                                : newItem.SummaShipped - newItem.SummaIn;
                            newItem.DocDate = doc.DocDate;
                            newItem.InnerNumber = doc.SF_IN_NUM;
                            newItem.ExtNumber = doc.SF_POSTAV_NUM;
                            newItem.Note = doc.Note;
                            newItem.DocInfo = GetDocDescription(
                                DocumentType.InvoiceProvider,
                                newItem
                            );
                            newItem.Creator = doc.CREATOR;
                            newItem.ProductTypeName = (
                                (IName)
                                GlobalOptions.ReferencesCache.GetNomenklProductType(
                                    ((IInvoiceProvider)doc)?.VzaimoraschetTypeDC
                                )
                            )?.Name;
                        }
                    }

                    if (p.InvoiceClientId is not null)
                    {
                        var d = Context
                            .InvoiceClientQuery.Where(_ => _.Id == p.InvoiceClientId)
                            .ToList();
                        if (d is not null)
                        {
                            //var hasExclude = Context.InvoiceClientQuery.Any(_ =>
                            //    _.Id == p.InvoiceClientId && excl.Any(x => x == _.NomenklDC));
                            var hasExclude = false;
                            foreach (var ex in from ex in exclInvoices
                                     from dd in d
                                     where dd.Row2d == ex.SFClientRowId
                                     select ex) hasExclude = true;

                            var doc = new InvoiceClientBase(d);
                            newItem.HasExcludeRow = hasExclude;
                            newItem.Kontragent = doc.Client;
                            newItem.Currency = doc.Currency;
                            if (!hasExclude)
                            {
                                newItem.SummaOut = doc.Summa;
                                newItem.SummaPay = doc.PaySumma;
                                newItem.SummaShipped = doc.SummaOtgruz;
                                newItem.QuantityOutDocument =
                                    d.Where(_ => _.DocCode == doc.DocCode).Sum(_ => _.Quantity) ?? 0;
                                foreach (var r in d)
                                    if (r.IsUsluga ?? false)
                                    {
                                        newItem.QuantityOutShipped += r.Quantity ?? 0;
                                        newItem.UslugaSummaOut += r.Summa ?? 0;
                                    }
                                    else
                                    {
                                        newItem.QuantityOutShipped += d.Where(_ =>
                                                _.DocCode == doc.DocCode
                                            )
                                            .Sum(_ => _.Shipped);
                                    }
                            }
                            else
                            {
                                var excludeRows = Context.TD_84.Include(_ => _.SD_83)
                                    .Where(_ => _.DOC_CODE == doc.DocCode && excl.Contains(_.SFT_NEMENKL_DC)).ToList();
                                newItem.SummaOut =
                                    doc.Summa - excludeRows.Sum(_ => _.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0);
                                newItem.SummaPay = doc.PaySumma;
                                var sOtgr = 0m;
                                var qOtgr = 0m;
                                foreach (var r in excludeRows)
                                {
                                    if (r.SD_83.NOM_0MATER_1USLUGA == 1)
                                    {
                                        sOtgr += r.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0;
                                        qOtgr += 1;
                                        continue;
                                    }

                                    var otgr = Context.TD_24.Where(_ => _.DOC_CODE == doc.DocCode).ToList();
                                    foreach (var rs in otgr)
                                    {
                                        sOtgr += rs.DDT_KOL_RASHOD * (r.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0) /
                                                 (decimal)r.SFT_KOL;
                                        qOtgr += rs.DDT_KOL_RASHOD;
                                    }
                                }

                                newItem.UslugaSummaIn = d.Where(_ => _.DocCode == doc.DocCode && (_.IsUsluga ?? false))
                                    .Sum(_ => _.Price * _.Quantity) ?? 0;
                                newItem.SummaShipped = doc.SummaOtgruz - sOtgr;
                                newItem.QuantityOutDocument =
                                    d.Where(_ => _.DocCode == doc.DocCode).Sum(_ => _.Quantity) ?? 0;
                                foreach (var r in d)
                                    if (r.IsUsluga ?? false)
                                        newItem.QuantityOutShipped += r.Quantity ?? 0;
                                    else
                                        newItem.QuantityOutShipped += d.Where(_ =>
                                                _.DocCode == doc.DocCode
                                            )
                                            .Sum(_ => _.Shipped);
                                newItem.QuantityOutShipped -= qOtgr;
                            }

                            newItem.QuantityOutRemain =
                                newItem.QuantityOutDocument - newItem.QuantityOutShipped;
                            newItem.SummaOutRemain = newItem.SummaOut - newItem.SummaShipped;
                            newItem.DocDate = doc.DocDate;
                            newItem.InnerNumber = doc.InnerNumber;
                            newItem.ExtNumber = doc.OuterNumber;
                            newItem.Note = doc.Note;
                            newItem.DocInfo = GetDocDescription(
                                DocumentType.InvoiceClient,
                                newItem
                            );
                            newItem.Creator = doc.CREATOR;
                            newItem.ProductTypeName = doc.VzaimoraschetType?.Name;
                        }
                    }

                    if (p.UslugaProviderRowId is not null)
                    {
                        var row = Context
                            .TD_26.Include(_ => _.SD_26)
                            .FirstOrDefault(_ => _.Id == p.UslugaProviderRowId && excl.All(x => x != _.SFT_NEMENKL_DC));
                        if (row != null)
                        {
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(row.SD_26.SF_POST_DC)
                                    as Kontragent;
                            newItem.Currency = newItem.Kontragent?.Currency as Currency;
                            newItem.SummaOut = (decimal)row.SFT_SUMMA_K_OPLATE_KONTR_CRS;
                            newItem.DocDate = row.SD_26.SF_POSTAV_DATE;
                            newItem.InnerNumber = row.SD_26.SF_IN_NUM;
                            newItem.ExtNumber = row.SD_26.SF_POSTAV_NUM;
                            newItem.Note = row.SD_26.SF_NOTES;
                            newItem.Nomenkl =
                                GlobalOptions.ReferencesCache.GetNomenkl(row.SFT_NEMENKL_DC)
                                    as Nomenkl;
                            newItem.DocInfo = GetDocDescription(
                                DocumentType.InvoiceServiceProvider,
                                newItem
                            );
                            newItem.Creator = row.SD_26.CREATOR;
                            newItem.ProductTypeName = (
                                (IName)
                                GlobalOptions.ReferencesCache.GetNomenklProductType(
                                    row.SD_26.SF_VZAIMOR_TYPE_DC
                                )
                            )?.Name;
                        }
                    }

                    if (p.UslugaClientRowId is not null)
                    {
                        var row = Context
                            .TD_84.Include(_ => _.SD_84)
                            .FirstOrDefault(_ => _.Id == p.UslugaClientRowId && excl.All(x => x != _.SFT_NEMENKL_DC));
                        if (row != null)
                        {
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(row.SD_84.SF_CLIENT_DC)
                                    as Kontragent;
                            newItem.Currency = newItem.Kontragent?.Currency as Currency;
                            newItem.SummaIn = (decimal)row.SFT_SUMMA_K_OPLATE_KONTR_CRS;
                            newItem.DocDate = row.SD_84.SF_DATE;
                            newItem.InnerNumber = row.SD_84.SF_IN_NUM;
                            newItem.ExtNumber = row.SD_84.SF_OUT_NUM;
                            newItem.Note = row.SD_84.SF_NOTE;
                            newItem.Nomenkl =
                                GlobalOptions.ReferencesCache.GetNomenkl(row.SFT_NEMENKL_DC)
                                    as Nomenkl;
                            newItem.DocInfo = GetDocDescription(
                                DocumentType.InvoiceServiceClient,
                                newItem
                            );
                            newItem.Creator = row.SD_84.CREATOR;
                            newItem.ProductTypeName = (
                                (IName)
                                GlobalOptions.ReferencesCache.GetNomenklProductType(
                                    row.SD_84.SF_VZAIMOR_TYPE_DC
                                )
                            )?.Name;
                        }
                    }

                    if (p.AccruedClientRowId is not null)
                    {
                        var row = Context
                            .AccuredAmountForClientRow.Include(_ => _.AccruedAmountForClient)
                            .FirstOrDefault(_ => _.Id == p.AccruedClientRowId);
                        if (row != null)
                        {
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(
                                    row.AccruedAmountForClient.KontrDC
                                ) as Kontragent;
                            newItem.Currency = newItem.Kontragent?.Currency as Currency;
                            newItem.SummaIn = row.Summa;
                            newItem.DocDate = row.AccruedAmountForClient.DocDate;
                            newItem.InnerNumber = row.AccruedAmountForClient.DocInNum;
                            newItem.ExtNumber = row.AccruedAmountForClient.DocExtNum;
                            newItem.Note = row.AccruedAmountForClient.Note;
                            newItem.Nomenkl =
                                GlobalOptions.ReferencesCache.GetNomenkl(row.NomenklDC) as Nomenkl;
                            newItem.DocInfo = GetDocDescription(
                                DocumentType.AccruedAmountForClient,
                                newItem
                            );
                            newItem.Creator = row.AccruedAmountForClient.Creator;
                        }
                    }

                    if (p.AccruedSupplierRowId is not null)
                    {
                        var row = Context
                            .AccuredAmountOfSupplierRow.Include(_ => _.AccruedAmountOfSupplier)
                            .FirstOrDefault(_ => _.Id == p.AccruedSupplierRowId);
                        if (row != null)
                        {
                            newItem.Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(
                                    row.AccruedAmountOfSupplier.KontrDC
                                ) as Kontragent;
                            newItem.Currency = newItem.Kontragent?.Currency as Currency;
                            newItem.SummaIn = row.Summa;
                            newItem.DocDate = row.AccruedAmountOfSupplier.DocDate;
                            newItem.InnerNumber = row.AccruedAmountOfSupplier.DocInNum;
                            newItem.ExtNumber = row.AccruedAmountOfSupplier.DocExtNum;
                            newItem.Note = row.AccruedAmountOfSupplier.Note;
                            newItem.Nomenkl =
                                GlobalOptions.ReferencesCache.GetNomenkl(row.NomenklDC) as Nomenkl;
                            newItem.DocInfo = GetDocDescription(
                                DocumentType.AccruedAmountForClient,
                                newItem
                            );
                            newItem.Creator = row.AccruedAmountOfSupplier.Creator;
                        }
                    }

                    ret.Add(newItem);
                }

            return ret;
        }

        #endregion

        #region Касса

        public int GetcCashInCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context.SD_33.Count(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd);
        }

        public IEnumerable<SD_33> GetCashInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .SD_33.Include(_ => _.ProjectDocuments)
                .Where(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd)
                .ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        public IEnumerable<SD_33> GetCashInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .SD_33.Include(_ => _.ProjectDocuments)
                .Where(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd)
                .OrderByDescending(_ => _.DATE_ORD)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
            count = Context.SD_33.Count(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd);
            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        public int GetCashOutCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context.SD_34.Count(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd);
        }

        public IEnumerable<SD_34> GetCashOutForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .SD_34.Include(_ => _.ProjectDocuments)
                .Where(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd)
                .ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        public IEnumerable<SD_34> GetCashOutForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            count = Context
                .SD_34.Include(_ => _.ProjectDocuments)
                .Count(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd);
            var data = Context
                .SD_34.Include(_ => _.ProjectDocuments)
                .Where(_ => _.DATE_ORD >= dateStart && _.DATE_ORD <= dateEnd)
                .OrderByDescending(_ => _.DATE_ORD)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        #endregion

        #region Банки

        public int GetBankCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context
                .TD_101.Include(_ => _.SD_101)
                .Count(_ =>
                    _.SD_101.VV_START_DATE >= dateStart && _.SD_101.VV_START_DATE <= dateEnd
                );
        }

        public IEnumerable<TD_101> GetBankForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .TD_101.Include(_ => _.SD_101)
                .Include(_ => _.SD_101.SD_114)
                .Include(_ => _.ProjectDocuments)
                .Where(_ =>
                    _.SD_101.VV_START_DATE >= dateStart
                    && _.SD_101.VV_STOP_DATE <= dateEnd
                    && _.VVT_KONTRAGENT != null
                )
                .ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        public IEnumerable<TD_101> GetBankForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .TD_101.Include(_ => _.SD_101)
                .Include(_ => _.SD_101.SD_114)
                .Include(_ => _.ProjectDocuments)
                .Where(_ =>
                    _.SD_101.VV_START_DATE >= dateStart
                    && _.SD_101.VV_STOP_DATE <= dateEnd
                    && _.VVT_KONTRAGENT != null
                )
                .OrderByDescending(_ => _.SD_101.VV_START_DATE)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            count = Context
                .TD_101.Include(_ => _.SD_101)
                .Count(_ =>
                    _.SD_101.VV_START_DATE >= dateStart && _.SD_101.VV_START_DATE <= dateEnd
                );
            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        #endregion

        #region Приходный складской ордер

        public int GetWarehouseInCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context
                .SD_24.Include(_ => _.TD_24)
                .Include(_ => _.SD_26)
                .Include("TD_24.TD_26")
                .Count(_ =>
                    _.DD_TYPE_DC == 2010000001
                    && _.DD_SKLAD_OTPR_DC == null
                    && _.DD_DATE >= dateStart
                    && _.DD_DATE <= dateEnd
                );
        }

        public IEnumerable<SD_24> GetWarehouseInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .SD_24.Include(_ => _.TD_24)
                .Include(_ => _.ProjectDocuments)
                .Include(_ => _.SD_26)
                .Include("TD_24.TD_26")
                .Where(_ =>
                    _.DD_TYPE_DC == 2010000001
                    && _.DD_SKLAD_OTPR_DC == null
                    && _.DD_DATE >= dateStart
                    && _.DD_DATE <= dateEnd
                )
                .ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        public IEnumerable<SD_24> GetWarehouseInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .SD_24.Include(_ => _.TD_24)
                .Include(_ => _.ProjectDocuments)
                .Include(_ => _.SD_26)
                .Include("TD_24.TD_26")
                .Where(_ =>
                    _.DD_TYPE_DC == 2010000001
                    && _.DD_SKLAD_OTPR_DC == null
                    && _.DD_DATE >= dateStart
                    && _.DD_DATE <= dateEnd
                )
                .OrderByDescending(_ => _.DD_DATE)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            count = Context
                .SD_24.Include(_ => _.TD_24)
                .Include(_ => _.ProjectDocuments)
                .Include(_ => _.SD_26)
                .Include("TD_24.TD_26")
                .Count(_ =>
                    _.DD_TYPE_DC == 2010000001
                    && _.DD_SKLAD_OTPR_DC == null
                    && _.DD_DATE >= dateStart
                    && _.DD_DATE <= dateEnd
                );

            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        #endregion

        #region Расходная накладная

        public int GetWaybillCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context
                .SD_24.Include(_ => _.TD_24)
                .Include(_ => _.ProjectDocuments)
                .Count(_ =>
                    _.DD_TYPE_DC == 2010000012 && _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd
                );
        }

        public IEnumerable<SD_24> GetWaybillInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .SD_24.Include(_ => _.TD_24)
                .Include(_ => _.ProjectDocuments)
                .Where(_ =>
                    _.DD_TYPE_DC == 2010000012 && _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd
                )
                .ToList();

            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        public IEnumerable<SD_24> GetWaybillInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .SD_24.Include(_ => _.TD_24)
                .Include(_ => _.ProjectDocuments)
                .Where(_ =>
                    _.DD_TYPE_DC == 2010000012 && _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd
                )
                .OrderByDescending(_ => _.DD_DATE)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            count = Context
                .SD_24.Include(_ => _.TD_24)
                .Include(_ => _.ProjectDocuments)
                .Count(_ =>
                    _.DD_TYPE_DC == 2010000012 && _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd
                );

            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        #endregion

        #region Услуги

        public IEnumerable<TD_84> GetUslugaClientForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .TD_84.Include(_ => _.SD_84)
                .Include(_ => _.SD_83)
                .Where(_ =>
                    _.SD_84.SF_DATE >= dateStart
                    && _.SD_84.SF_DATE <= dateEnd
                    && _.SD_83.NOM_0MATER_1USLUGA == 1
                    && _.SD_84.SF_ACCEPTED == 1
                )
                .ToList();

            var docIds = new List<Guid>();
            foreach (
                var r in Context
                    .Projects.Include(_ => _.ProjectDocuments)
                    .Where(_ => ids.Contains(_.Id))
            )
                docIds.AddRange(
                    from rr in r.ProjectDocuments
                    where rr.UslugaProviderRowId is not null
                    select rr.UslugaProviderRowId.Value
                );

            return data.Where(row => !docIds.Contains(row.Id)).ToList();
        }

        public int GetUslugaClientCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context
                .TD_84.Include(_ => _.SD_84)
                .Include(_ => _.SD_83)
                .Count(_ =>
                    _.SD_84.SF_DATE >= dateStart
                    && _.SD_84.SF_DATE <= dateEnd
                    && _.SD_83.NOM_0MATER_1USLUGA == 1
                    && _.SD_84.SF_ACCEPTED == 1
                );
        }

        public IEnumerable<TD_84> GetUslugaClientForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .TD_84.Include(_ => _.SD_84)
                .Include(_ => _.SD_83)
                .Where(_ =>
                    _.SD_84.SF_DATE >= dateStart
                    && _.SD_84.SF_DATE <= dateEnd
                    && _.SD_83.NOM_0MATER_1USLUGA == 1
                    && _.SD_84.SF_ACCEPTED == 1
                )
                .OrderByDescending(_ => _.SD_84.SF_DATE)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            var docIds = new List<Guid>();
            foreach (
                var r in Context
                    .Projects.Include(_ => _.ProjectDocuments)
                    .Where(_ => ids.Contains(_.Id))
            )
                docIds.AddRange(
                    from rr in r.ProjectDocuments
                    where rr.UslugaProviderRowId is not null
                    select rr.UslugaProviderRowId.Value
                );
            count = Context
                .TD_84.Include(_ => _.SD_84)
                .Include(_ => _.SD_83)
                .Count(_ =>
                    _.SD_84.SF_DATE >= dateStart
                    && _.SD_84.SF_DATE <= dateEnd
                    && _.SD_83.NOM_0MATER_1USLUGA == 1
                    && _.SD_84.SF_ACCEPTED == 1
                );
            return data.Where(row => !docIds.Contains(row.Id)).ToList();
        }

        public int GetUslugaProviderCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context
                .TD_26.Include(_ => _.SD_26)
                .Include(_ => _.SD_83)
                .Count(_ =>
                    _.SD_26.SF_POSTAV_DATE >= dateStart
                    && _.SD_26.SF_POSTAV_DATE <= dateEnd
                    && _.SD_83.NOM_0MATER_1USLUGA == 1
                    && _.SD_26.SF_ACCEPTED == 1
                );
        }

        public IEnumerable<TD_26> GetUslugaProviderForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .TD_26.Include(_ => _.SD_26)
                .Include(_ => _.SD_83)
                .Where(_ =>
                    _.SD_26.SF_POSTAV_DATE >= dateStart
                    && _.SD_26.SF_POSTAV_DATE <= dateEnd
                    && _.SD_83.NOM_0MATER_1USLUGA == 1
                    && _.SD_26.SF_ACCEPTED == 1
                )
                .ToList();

            var docIds = new List<Guid>();
            foreach (
                var r in Context
                    .Projects.Include(_ => _.ProjectDocuments)
                    .Where(_ => ids.Contains(_.Id))
            )
                docIds.AddRange(
                    from rr in r.ProjectDocuments
                    where rr.UslugaProviderRowId is not null
                    select rr.UslugaProviderRowId.Value
                );

            return data.Where(row => !docIds.Contains(row.Id)).ToList();
        }

        public IEnumerable<TD_26> GetUslugaProviderForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .TD_26.Include(_ => _.SD_26)
                .Include(_ => _.SD_83)
                .Where(_ =>
                    _.SD_26.SF_POSTAV_DATE >= dateStart
                    && _.SD_26.SF_POSTAV_DATE <= dateEnd
                    && _.SD_83.NOM_0MATER_1USLUGA == 1
                    && _.SD_26.SF_ACCEPTED == 1
                )
                .OrderByDescending(_ => _.SD_26)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            var docIds = new List<Guid>();
            foreach (
                var r in Context
                    .Projects.Include(_ => _.ProjectDocuments)
                    .Where(_ => ids.Contains(_.Id))
            )
                docIds.AddRange(
                    from rr in r.ProjectDocuments
                    where rr.UslugaProviderRowId is not null
                    select rr.UslugaProviderRowId.Value
                );

            count = Context
                .TD_26.Include(_ => _.SD_26)
                .Include(_ => _.SD_83)
                .Count(_ =>
                    _.SD_26.SF_POSTAV_DATE >= dateStart
                    && _.SD_26.SF_POSTAV_DATE <= dateEnd
                    && _.SD_83.NOM_0MATER_1USLUGA == 1
                    && _.SD_26.SF_ACCEPTED == 1
                );
            return data.Where(row => !docIds.Contains(row.Id)).ToList();
        }

        #endregion

        #region Группы проектов

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public IBoolResult SaveGroups(
            IEnumerable<ProjectGroups> data,
            IEnumerable<Guid> deleteGrpIds = null,
            IEnumerable<Guid> deleteLinkIds = null
        )
        {
            if (deleteGrpIds is not null && deleteGrpIds.Any())
                foreach (var id in deleteGrpIds)
                {
                    var old = Context
                        .ProjectGroups.Include(_ => _.ProjectGroupLink)
                        .FirstOrDefault(_ => _.Id == id);
                    if (old?.ProjectGroupLink != null)
                        Context.ProjectGroupLink.RemoveRange(old.ProjectGroupLink);
                    if (old != null)
                        Context.ProjectGroups.Remove(old);
                }

            if (deleteLinkIds is not null && deleteLinkIds.Any())
                foreach (var id in deleteLinkIds)
                {
                    var old = Context.ProjectGroupLink.FirstOrDefault(_ => _.Id == id);
                    if (old == null)
                        continue;
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

        #endregion

        #region Справочник проектов

        public IBoolResult SaveReference(
            IEnumerable<Data.Projects> data,
            IEnumerable<Guid> deleteIds = null
        )
        {
            var delList = deleteIds is null ? new List<Guid>() : deleteIds.ToList();
            if (deleteIds is not null && delList.Any())
                foreach (var id in delList)
                {
                    var old = Context
                        .Projects.Include(_ => _.NomenklReturnToProvider)
                        .Include(_ => _.NomenklReturnOfClient)
                        .Include(_ => _.ProjectGroupLink)
                        .Include(_ => _.SD_33)
                        .Include(_ => _.SD_34)
                        .Include(_ => _.SD_24)
                        .Include(_ => _.TD_101)
                        .FirstOrDefault(_ => _.Id == id);
                    if (old is null)
                        continue;
                    if (
                        old.SD_24.Count == 0
                        && old.TD_101.Count == 0
                        && old.SD_33.Count == 0
                        && old.SD_34.Count == 0
                        && old.ProjectGroupLink.Count == 0
                        && old.NomenklReturnToProvider.Count == 0
                        && old.NomenklReturnOfClient.Count == 0
                    )
                        Context.Projects.Remove(old);
                    else
                        return new BoolResult
                        {
                            Result = false,
                            ErrorText = $"Для проекта {old.Name} есть связанные в документах"
                        };
                }

            foreach (var p in data)
            {
                var old = Context.Projects.FirstOrDefault(_ => _.Id == p.Id);
                if (old != null)
                {
                    old.DateEnd = p.DateEnd;
                    old.DateStart = p.DateStart;
                    old.IsClosed = p.IsClosed;
                    old.IsDeleted = p.IsDeleted;
                    old.IsExcludeFromProfitAndLoss = p.IsExcludeFromProfitAndLoss;
                    old.ManagerDC = p.ManagerDC;
                    old.Note = p.Note;
                    old.ParentId = p.ParentId;
                    old.UpdateDate = DateTime.Now;
                    old.Name = p.Name;
                }
                else
                {
                    Context.Projects.Add(p);
                }
            }

            return new BoolResult { Result = true };
        }

        public IEnumerable<Data.Projects> LoadReference()
        {
            return Context.Projects.Include(_ => _.ProjectDocuments).AsNoTracking().ToList();
        }

        #endregion

        #region Валютная конвертация

        public int GetCurrencyConvertsCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context
                .TD_26_CurrencyConvert.Include(_ => _.TD_26)
                .Include(_ => _.TD_26.SD_26)
                .Include(td26CurrencyConvert => td26CurrencyConvert.ProjectDocuments)
                .Count(_ =>
                    _.TD_26.SD_26.SF_POSTAV_DATE >= dateStart
                    && _.TD_26.SD_26.SF_POSTAV_DATE <= dateEnd
                );
        }

        public IEnumerable<TD_26_CurrencyConvert> GetCurrencyConverts(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .TD_26_CurrencyConvert.Include(_ => _.TD_26)
                .Include(_ => _.TD_26.SD_26)
                .Include(td26CurrencyConvert => td26CurrencyConvert.ProjectDocuments)
                .Where(_ =>
                    _.TD_26.SD_26.SF_POSTAV_DATE >= dateStart
                    && _.TD_26.SD_26.SF_POSTAV_DATE <= dateEnd
                )
                .ToList();
            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        public IEnumerable<TD_26_CurrencyConvert> GetCurrencyConverts(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        )
        {
            var ids = GetAllTreeProjectIds(projectId);
            var data = Context
                .TD_26_CurrencyConvert.Include(_ => _.TD_26)
                .Include(_ => _.TD_26.SD_26)
                .Include(td26CurrencyConvert => td26CurrencyConvert.ProjectDocuments)
                .Where(_ =>
                    _.TD_26.SD_26.SF_POSTAV_DATE >= dateStart
                    && _.TD_26.SD_26.SF_POSTAV_DATE <= dateEnd
                )
                .OrderByDescending(_ => _.TD_26.SD_26.SF_POSTAV_DATE)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
            count = Context
                .TD_26_CurrencyConvert.Include(_ => _.TD_26)
                .Include(_ => _.TD_26.SD_26)
                .Include(td26CurrencyConvert => td26CurrencyConvert.ProjectDocuments)
                .Count(_ =>
                    _.TD_26.SD_26.SF_POSTAV_DATE >= dateStart
                    && _.TD_26.SD_26.SF_POSTAV_DATE <= dateEnd
                );
            return data.Where(item =>
                    item.ProjectDocuments == null
                    || ids.All(id => item.ProjectDocuments.All(_ => _.ProjectId != id))
                )
                .ToList();
        }

        #endregion

        #region Вспомогательное

        public void UpdateCache()
        {
            if (mySubscriber != null && mySubscriber.IsConnected())
            {
                var message = new RedisMessage
                {
                    DocumentType = DocumentType.InvoiceProvider,
                    IsDocument = false,
                    Message =
                        $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник проектов"
                };
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                mySubscriber.Publish(
                    new RedisChannel(
                        RedisMessageChannels.ProjectReference,
                        RedisChannel.PatternMode.Auto
                    ),
                    json
                );
            }
        }

        /// <summary>
        ///     Получить все Id проектов, находящиеся в одной ветке с id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Guid> GetAllTreeProjectIds(Guid id)
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

        public List<Guid> GetChilds(List<Data.Projects> list, Guid id)
        {
            var ret = new List<Guid>(new[] { id });
            var ch = list.Where(_ => _.ParentId == id).ToList();
            foreach (var item in ch)
                ret.AddRange(GetChilds(list, item.Id));

            return ret;
        }

        public List<NomenklMoveForProject_Result> GetNomenklMoveForProject(
            Guid projectId,
            bool isRecursive,
            bool isExcludeShow
        )
        {
            var prjList = GetChilds(projectId);
            var ret = new List<NomenklMoveForProject_Result>();
            var data = Context
                .GetNomenklMoveForProject(projectId, (byte?)(isRecursive ? 1 : 0), (byte?)(isExcludeShow ? 1 : 0))
                .ToList();

            var manual = GetManualQuantity(prjList);

            foreach (var r in data)
            {
                var newItem = new NomenklMoveForProject_Result
                {
                    DocQuantityIn = r.DocQuantityIn,
                    NomDC = r.NomDC,
                    DocSummaIn = r.DocSummaIn,
                    FactQuantityIn = r.FactQuantityIn,
                    DocQuantityOut = r.DocQuantityOut,
                    DocSummaOut = r.DocSummaOut,
                    FactQuantityOut = r.FactQuantityOut,
                    FactSummaIn = r.FactSummaIn,
                    DilerSumma = r.DilerSumma,
                    FactSummaOut = r.FactSummaOut,
                    HasExcluded = r.HasExcluded,
                    IsService = r.IsService,
                    NakladSumma = r.NakladSumma,
                    NomId = r.NomId,
                    NomName = r.NomName,
                    NomNomenkl = r.NomNomenkl
                };
                if (manual.Any(_ => _.NomDC == newItem.NomDC))
                {
                    var dInvoiceIn = manual.Where(_ => _.NomDC == newItem.NomDC && _.DocType is 1 or 4).ToList();
                    if (dInvoiceIn.Count > 0)
                    {
                        var prc = newItem.DocSummaIn / newItem.DocQuantityIn;
                        newItem.DocQuantityIn -= dInvoiceIn.Sum(_ => _.DeltaQuantity);
                        newItem.DocSummaIn = prc * newItem.DocQuantityIn;
                        if (newItem.FactQuantityIn > newItem.DocQuantityIn)
                        {
                            newItem.FactQuantityIn = newItem.DocQuantityIn;
                            newItem.FactSummaIn = newItem.DocSummaIn;
                        }

                        newItem.IsManualChanged = true;
                    }

                    var dInvoiceOut = manual.Where(_ => _.NomDC == newItem.NomDC && _.DocType == 0).ToList();
                    if (dInvoiceOut.Count > 0)
                    {
                        var s = dInvoiceOut.Sum(_ => _.Price * _.DeltaQuantity);
                        newItem.DocQuantityOut -= dInvoiceOut.Sum(_ => _.DeltaQuantity);
                        newItem.DocSummaOut = newItem.DocSummaOut - s;

                        if (newItem.FactQuantityOut > newItem.DocQuantityOut)
                        {
                            newItem.FactQuantityOut = newItem.DocQuantityOut;
                            newItem.FactSummaOut = newItem.DocSummaOut;
                        }

                        newItem.IsManualChanged = true;
                    }
                }

                ret.Add(newItem);
            }

            return ret;
        }

        public List<Guid> GetChilds(Guid projectId)
        {
            var sql =
                $@"WITH AllProjects (Id, ParentId, Name, ProjectLevel)
                    AS (   SELECT
                                   Id,
                                   ParentId,
                                   Name,
                                   0 AS ProjectLevel
                           FROM
                                   Projects
                           WHERE
                                   Id = '{CustomFormat.GuidToSqlString(projectId)}'
                           UNION ALL
                           SELECT
                                   p.Id,
                                   p.ParentId,
                                   p.Name,
                                   ProjectLevel + 1 AS ProjectLevel
                           FROM
                                   Projects p
                               INNER JOIN
                                 AllProjects AS ap
                                     ON p.ParentId = ap.Id)
                    SELECT
                            Id
                    FROM
                            AllProjects";
            return Context.Database.SqlQuery<Guid>(sql).ToList();
        }


        public List<ProjectNomenklMoveDocumentInfo> GetDocumentsForNomenkl(
            List<Guid> projectIds,
            decimal nomDC,
            bool isExcludeShow
        )
        {
            var projDocs = Context.ProjectDocuments.AsNoTracking().ToList();

            var ret = new List<ProjectNomenklMoveDocumentInfo>();

            var invProviderIds = new HashSet<Guid>();
            var invClientIds = new HashSet<Guid>();

            foreach (var r in projDocs.Where(_ =>
                         projectIds.Contains(_.ProjectId)))
            {
                if (r.InvoiceProviderId != null)
                    invProviderIds.Add(r.InvoiceProviderId.Value);
                if (r.InvoiceClientId != null)
                    invClientIds.Add(r.InvoiceClientId.Value);
            }

            var invoicesProvider = Context
                .SD_26.Include(_ => _.TD_26)
                .Include(_ => _.ProviderInvoicePay)
                .Include("TD_26.TD_24")
                .AsNoTracking()
                .Where(_ => invProviderIds.Contains(_.Id))
                .ToList();
            var invoicesClient = Context
                .SD_84.Include(_ => _.TD_84)
                .Include("TD_84.TD_24")
                .AsNoTracking()
                .Where(_ => invClientIds.Contains(_.Id))
                .ToList();

            var excl = Context.ProjectRowExclude.AsNoTracking().Where(_ => projectIds.Contains(_.ProjectId)).ToList();

            var exludedId = new List<Guid>();
            foreach (var ex in excl)
            {
                if (ex.NomCurrencyConvertRowId is not null)
                {
                    exludedId.Add(ex.NomCurrencyConvertRowId.Value);
                    continue;
                }

                if (ex.SFClientRowId is not null)
                {
                    exludedId.Add(ex.SFClientRowId.Value);
                    continue;
                }

                if (ex.SFProviderRowId is not null) exludedId.Add(ex.SFProviderRowId.Value);
            }

            if (!isExcludeShow)
                foreach (var doc in invoicesClient)
                {
                    var rems = doc.TD_84.Where(_ => exludedId.Contains(_.Id)).ToList();
                    foreach (var r in rems) doc.TD_84.Remove(r);
                }

            foreach (var doc in invoicesProvider)
            foreach (var ex in excl.Where(_ => _.SFProviderRowId is not null))
            {
                var rem = doc.TD_26.Where(_ => _.Id == ex.SFProviderRowId);
                if (isExcludeShow) continue;
                {
                    foreach (var rr in exludedId.Select(id => doc.TD_26.FirstOrDefault(_ => _.Id == id))
                                 .Where(rr => rr is not null))
                        doc.TD_26.Remove(rr);
                }
            }

            var manualRows = Context.ProjectInvoiceQuantityChanged.AsNoTracking().ToList();

            foreach (var doc in invoicesProvider)
            foreach (var row in doc.TD_26.Where(_ => _.SFT_NEMENKL_DC == nomDC))
            {
                var shippedRow = row.TD_24.FirstOrDefault(_ =>
                    _.DDT_SPOST_DC == row.DOC_CODE && _.DDT_SPOST_ROW_CODE == row.CODE);
                var shipped = shippedRow?.TD_26.SFT_SUMMA_K_OPLATE_KONTR_CRS
                    * shippedRow?.TD_26.SFT_KOL / shippedRow?.DDT_KOL_PRIHOD ?? 0;
                var manualQuantity = manualRows.FirstOrDefault(_ => _.ProviderRowId == row.Id)?.Quantity;
                var newItem = new ProjectNomenklMoveDocumentInfo
                {
                    Id = row.Id,
                    IsInclude = !exludedId.Contains(row.Id),
                    DocCode = doc.DOC_CODE,
                    Kontragent =
                        GlobalOptions.ReferencesCache.GetKontragent(doc.SF_POST_DC)
                            as Kontragent,
                    DocDate = doc.SF_POSTAV_DATE,
                    DocumentType = DocumentType.InvoiceProvider,
                    Note = doc.SF_NOTES,
                    ExtNumber = doc.SF_POSTAV_NUM,
                    InnerNumber = doc.SF_IN_NUM,
                    Creator = doc.CREATOR,
                    Warehouse = shippedRow != null
                        ? GlobalOptions.ReferencesCache.GetWarehouse(shippedRow.SD_24.DD_SKLAD_POL_DC) as
                            Warehouse
                        : null,
                    ProviderQuantity = doc.TD_26.Sum(_ => _.SFT_KOL),
                    ProviderShipped = shipped,
                    ProviderSumma = doc.TD_26.Sum(_ => _.SFT_SUMMA_K_OPLATE_KONTR_CRS) ?? 0,
                    ProviderShippedQuantity = shippedRow?.DDT_KOL_PRIHOD ?? 0,
                    ManualProviderQuantity = manualQuantity ?? doc.TD_26.Sum(_ => _.SFT_KOL),
                    ManualProviderSumma = (decimal)((manualQuantity ?? doc.TD_26.Sum(_ => _.SFT_KOL))
                                                    * (doc.TD_26.Sum(_ => _.SFT_SUMMA_K_OPLATE_KONTR_CRS) /
                                                       doc.TD_26.Sum(_ => _.SFT_KOL)))
                };
                if (newItem.ProviderShippedQuantity > newItem.ManualProviderQuantity)
                {
                    newItem.ProviderShippedQuantity = newItem.ManualProviderQuantity;
                    newItem.ProviderShipped = newItem.ProviderShippedQuantity * newItem.ProviderSumma /
                                              newItem.ProviderQuantity;
                }

                var pId = projDocs
                    .FirstOrDefault(_ => _.InvoiceProviderId == row.DocId && projectIds.Contains(_.ProjectId))
                    ?.ProjectId;
                if (pId != null)
                    newItem.ProjectId = pId.Value;
                ret.Add(newItem);
            }

            foreach (var doc in invoicesClient)
            foreach (var row in doc.TD_84.Where(_ => _.SFT_NEMENKL_DC == nomDC))
            {
                var manualQuantity = manualRows.FirstOrDefault(_ => _.ClientRowId == row.Id)?.Quantity;
                //var shippedRow = Context.TD_24.Include(_ => _.SD_24).AsNoTracking()
                //    .Where(_ => row.DOC_CODE == _.DDT_SFACT_DC && row.CODE == _.DDT_SFACT_ROW_CODE).ToList();
                var shippedRow = row.TD_24
                    .Where(_ => _.DDT_SPOST_DC == row.DOC_CODE && _.DDT_SPOST_ROW_CODE == row.CODE).ToList();
                var newItem = new ProjectNomenklMoveDocumentInfo
                {
                    Id = row.Id,
                    IsInclude = !exludedId.Contains(row.Id),
                    DocCode = row.DOC_CODE,
                    Kontragent =
                        GlobalOptions.ReferencesCache.GetKontragent(doc.SF_CLIENT_DC)
                            as Kontragent,
                    DocDate = doc.SF_DATE,
                    DocumentType = DocumentType.InvoiceClient,
                    Note = doc.SF_NOTE,
                    ExtNumber = doc.SF_OUT_NUM,
                    InnerNumber = doc.SF_IN_NUM,
                    Creator = doc.CREATOR,
                    ClientSumma = row.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0,
                    ClientQuantity = (decimal)row.SFT_KOL,
                    ClientShipped = shippedRow.Sum(_ =>
                        _.DDT_KOL_RASHOD * row.SFT_SUMMA_K_OPLATE_KONTR_CRS / (decimal?)row.SFT_KOL ?? 0),
                    ClientShippedQuantity = shippedRow.Sum(_ => _.DDT_KOL_RASHOD),
                    Warehouse = shippedRow.Any()
                        ? (Warehouse)GlobalOptions.ReferencesCache.GetWarehouse(shippedRow.First().SD_24
                            .DD_SKLAD_OTPR_DC)
                        : null,
                    ManualClientQuantity = manualQuantity ?? (decimal)row.SFT_KOL,
                    ManualClientSumma = (manualQuantity ?? (decimal)row.SFT_KOL) *
                        (row.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0) / (decimal)row.SFT_KOL
                };
                if (newItem.ClientShippedQuantity > newItem.ManualClientQuantity)
                {
                    newItem.ClientShippedQuantity = newItem.ManualClientQuantity;
                    newItem.ClientShipped = newItem.ClientShippedQuantity * newItem.ClientSumma /
                                            newItem.ClientQuantity;
                }

                var pId = projDocs
                    .FirstOrDefault(_ => _.InvoiceClientId == row.DocId && projectIds.Contains(_.ProjectId))?.ProjectId;
                if (pId != null)
                    newItem.ProjectId = pId.Value;

                ret.Add(newItem);
            }

            var nomId = ((Nomenkl)GlobalOptions.ReferencesCache.GetNomenkl(nomDC)).Id;

            var invCrs = Context.TD_26_CurrencyConvert.Include(_ => _.TD_26)
                .AsNoTracking().Where(_ => _.NomenklId == nomId).ToList();
            foreach (var doc in invoicesProvider)
            {
                var crsConv = invCrs.FirstOrDefault(_ => (_.DOC_CODE ?? 0) == doc.DOC_CODE);
                decimal? manualQuantity = null;
                if (crsConv != null)
                    manualQuantity = manualRows.FirstOrDefault(_ => _.ProviderCurrencyConvertRowId == crsConv.Id)
                        ?.Quantity;
                if (crsConv is not null)
                {
                    var newItem = new ProjectNomenklMoveDocumentInfo
                    {
                        Id = crsConv.Id,
                        IsInclude = !exludedId.Contains(crsConv.Id),
                        DocCode = doc.DOC_CODE,
                        Kontragent =
                            GlobalOptions.ReferencesCache.GetKontragent(doc.SF_POST_DC)
                                as Kontragent,
                        DocDate = doc.SF_POSTAV_DATE,
                        DocumentType = DocumentType.NomenklCurrencyConverterProvider,
                        Note = doc.SF_NOTES,
                        ExtNumber = doc.SF_POSTAV_NUM,
                        InnerNumber = doc.SF_IN_NUM,
                        Warehouse =
                            GlobalOptions.ReferencesCache.GetWarehouse(crsConv.StoreDC)
                                as Warehouse,
                        Creator = doc.CREATOR,
                        ProviderQuantity = crsConv.Quantity,
                        ProviderShippedQuantity = crsConv.Quantity,
                        ProviderShipped = crsConv.Summa,
                        ProviderSumma = crsConv.Summa,
                        ManualProviderSumma = manualQuantity == null
                            ? crsConv.Summa
                            : crsConv.Summa * manualQuantity.Value / crsConv.Quantity,
                        ManualProviderQuantity = manualQuantity ?? crsConv.Quantity
                    };
                    if (newItem.ProviderShippedQuantity > newItem.ManualProviderQuantity)
                    {
                        newItem.ProviderShippedQuantity = newItem.ManualProviderQuantity;
                        newItem.ProviderShipped = newItem.ProviderShippedQuantity * newItem.ProviderSumma /
                                                  newItem.ProviderQuantity;
                    }

                    var pId = projDocs.FirstOrDefault(_ => _.InvoiceProviderId == doc.Id
                                                           && projectIds.Contains(_.ProjectId))?.ProjectId;
                    if (pId != null)
                        newItem.ProjectId = pId.Value;

                    ret.Add(newItem);
                }
            }

            return ret;
        }


        public List<ProjectNomenklMoveDocumentInfo> GetDocumentsForNomenkl(
            List<Guid> projectIds,
            decimal nomDC
        )
        {
            var ret = new List<ProjectNomenklMoveDocumentInfo>();

            var invProviderIds = Context
                .ProjectDocuments.Where(_ =>
                    projectIds.Contains(_.ProjectId) && _.InvoiceProviderId != null
                )
                .Select(_ => _.InvoiceProviderId)
                .ToList();

            var invClientIds = Context
                .ProjectDocuments.Where(_ =>
                    projectIds.Contains(_.ProjectId) && _.InvoiceClientId != null
                )
                .Select(_ => _.InvoiceClientId)
                .ToList();

            var invoicesProvider = Context
                .SD_26.Include(_ => _.TD_26)
                .Include(_ => _.ProviderInvoicePay)
                .Include("TD_26.TD_24")
                .Where(_ => invProviderIds.Contains(_.Id))
                .ToList();
            var invoicesClient = Context
                .SD_84.Include(_ => _.TD_84)
                .Where(_ => invClientIds.Contains(_.Id))
                .ToList();

            var invoiceProvider = invoicesProvider.Where(_ =>
                _.TD_26.Any(x => x.SFT_NEMENKL_DC == nomDC)
            ).ToList();

            var invoiceClient = invoicesClient.Where(_ =>
                _.TD_84.Any(x => x.SFT_NEMENKL_DC == nomDC)
            ).ToList();

            foreach (var doc in invoiceProvider)
            {
                if (ret.All(_ => _.DocCode != doc.DOC_CODE))
                {
                    var shippedRow =
                        Context
                            .TD_24.Include(_ => _.SD_24).Include(_ => _.TD_26)
                            .FirstOrDefault(_ => _.DDT_SPOST_DC == doc.DOC_CODE);
                    var shipped = shippedRow?.TD_26.SFT_SUMMA_K_OPLATE_KONTR_CRS
                        * shippedRow?.TD_26.SFT_KOL / shippedRow?.DDT_KOL_PRIHOD ?? 0;

                    ret.Add(
                        new ProjectNomenklMoveDocumentInfo
                        {
                            Id = doc.TD_26.First().Id,
                            IsInclude = true,
                            DocCode = doc.DOC_CODE,
                            Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(doc.SF_POST_DC)
                                    as Kontragent,
                            DocDate = doc.SF_POSTAV_DATE,
                            DocumentType = DocumentType.InvoiceProvider,
                            Note = doc.SF_NOTES,
                            ExtNumber = doc.SF_POSTAV_NUM,
                            InnerNumber = doc.SF_IN_NUM,
                            Creator = doc.CREATOR,
                            Warehouse = shippedRow != null
                                ? GlobalOptions.ReferencesCache.GetWarehouse(shippedRow.SD_24.DD_SKLAD_POL_DC) as
                                    Warehouse
                                : null,
                            ProviderQuantity = doc.ProviderInvoicePay.Sum(_ => _.Summa),
                            ProviderShipped = shipped,
                            ProviderSumma = doc.TD_26.Sum(_ => _.SFT_SUMMA_K_OPLATE_KONTR_CRS) ?? 0,
                            ProviderShippedQuantity = shippedRow?.DDT_KOL_PRIHOD ?? 0
                        }
                    );
                }

                var s = doc.TD_26.FirstOrDefault(_ => _.SFT_NEMENKL_DC == nomDC);
                if (s is null)
                    continue;
            }

            foreach (var doc in invoiceClient)
            {
                var s = doc.TD_84.FirstOrDefault(_ => _.SFT_NEMENKL_DC == nomDC);
                if (s is null)
                    continue;
                var shippedRow = Context.TD_24.Include(_ => _.SD_24).Include(_ => _.TD_26)
                    .FirstOrDefault(_ => s.DOC_CODE == _.DDT_SFACT_DC && s.CODE == _.DDT_SFACT_ROW_CODE);

                ret.Add(
                    new ProjectNomenklMoveDocumentInfo
                    {
                        Id = doc.TD_84.First().Id,
                        IsInclude = true,
                        DocCode = s.DOC_CODE,
                        Kontragent =
                            GlobalOptions.ReferencesCache.GetKontragent(doc.SF_CLIENT_DC)
                                as Kontragent,
                        DocDate = doc.SF_DATE,
                        DocumentType = DocumentType.InvoiceClient,
                        Note = doc.SF_NOTE,
                        ExtNumber = doc.SF_OUT_NUM,
                        InnerNumber = doc.SF_IN_NUM,
                        Creator = doc.CREATOR,
                        ClientSumma = s.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0,
                        ClientQuantity = (decimal)s.SFT_KOL,
                        ClientShipped = shippedRow?.DDT_KOL_RASHOD * s.SFT_SUMMA_K_OPLATE_KONTR_CRS /
                            (decimal?)s.SFT_KOL ?? 0,
                        ClientShippedQuantity = shippedRow?.DDT_KOL_RASHOD ?? 0,
                        Warehouse = shippedRow != null
                            ? (Warehouse)GlobalOptions.ReferencesCache.GetWarehouse(shippedRow.SD_24.DD_SKLAD_OTPR_DC)
                            : null
                    }
                );
            }

            var nomId = Context.SD_83.Single(_ => _.DOC_CODE == nomDC).Id;
            var invCrs = Context.TD_26_CurrencyConvert.Where(_ => _.NomenklId == nomId).ToList();

            foreach (var doc in invoicesProvider)
            {
                var crsConv = invCrs.FirstOrDefault(_ => (_.DOC_CODE ?? 0) == doc.DOC_CODE);
                if (crsConv is not null)
                    ret.Add(
                        new ProjectNomenklMoveDocumentInfo
                        {
                            Id = crsConv.Id,
                            IsInclude = true,
                            DocCode = doc.DOC_CODE,
                            Kontragent =
                                GlobalOptions.ReferencesCache.GetKontragent(doc.SF_POST_DC)
                                    as Kontragent,
                            DocDate = doc.SF_POSTAV_DATE,
                            DocumentType = DocumentType.NomenklCurrencyConverterProvider,
                            Note = doc.SF_NOTES,
                            ExtNumber = doc.SF_POSTAV_NUM,
                            InnerNumber = doc.SF_IN_NUM,
                            Warehouse =
                                GlobalOptions.ReferencesCache.GetWarehouse(crsConv.StoreDC)
                                    as Warehouse,
                            Creator = doc.CREATOR,
                            ProviderQuantity = crsConv.Quantity,
                            ProviderShippedQuantity = crsConv.Quantity,
                            ProviderShipped = crsConv.Summa,
                            ProviderSumma = crsConv.Summa
                        }
                    );
            }

            return ret;
        }

        public List<ProjectRowExclude> GetDocumentsRowExclude(List<Guid> projectIdGuids)
        {
            return Context
                .ProjectRowExclude.Where(_ => projectIdGuids.Contains(_.ProjectId))
                .ToList();
        }

        public void ExcludeNomenklFromProjects(List<Guid> projectIdGuids, decimal nomDC)
        {
            foreach (
                var prjId in from prjId in projectIdGuids
                let old = Context.ProjectRowExclude.FirstOrDefault(_ =>
                    _.ProjectId == prjId && _.NomenklDC == nomDC
                )
                where old is null
                select prjId
            )
            {
                Context.ProjectRowExclude.Add(
                    new ProjectRowExclude
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = prjId,
                        NomenklDC = nomDC
                    }
                );
                Context.SaveChanges();
            }
        }

        public void ExcludeNomenklFromProjects(List<Guid> projectIdGuids, DocumentType docType, Guid rowId)
        {
            decimal? nomDC;
            switch (docType)
            {
                case DocumentType.InvoiceProvider:
                    var row = Context.TD_26.Single(_ => _.Id == rowId);
                    var pEx = Context.ProjectRowExclude.Where(_ =>
                        projectIdGuids.Contains(_.Id) && _.SFProviderRowId == rowId).ToList();
                    if (pEx.Count > 0)
                        foreach (var p in pEx)
                            Context.ProjectRowExclude.Remove(p);

                    Context.SaveChanges();

                    nomDC = row.SFT_NEMENKL_DC;
                    foreach (var pp in projectIdGuids)
                        Context.ProjectRowExclude.Add(new ProjectRowExclude
                        {
                            Id = Guid.NewGuid(),
                            ProjectId = pp,
                            SFProviderRowId = rowId,
                            NomenklDC = nomDC.Value
                        });

                    updateExcludeForClient(projectIdGuids, nomDC);

                    Context.SaveChanges();

                    break;
                case DocumentType.InvoiceClient:
                    var pcEx = Context.ProjectRowExclude.Where(_ =>
                        projectIdGuids.Contains(_.Id) && _.SFClientRowId == rowId).ToList();
                    if (pcEx.Count > 0)
                        foreach (var p in pcEx)
                            Context.ProjectRowExclude.Remove(p);

                    Context.SaveChanges();
                    nomDC = Context.TD_84.FirstOrDefault(_ => _.Id == rowId)?.SFT_NEMENKL_DC;
                    if (nomDC is null) return;
                    foreach (var pp in projectIdGuids)
                        Context.ProjectRowExclude.Add(new ProjectRowExclude
                        {
                            Id = Guid.NewGuid(),
                            ProjectId = pp,
                            SFClientRowId = rowId,
                            NomenklDC = nomDC.Value
                        });

                    Context.SaveChanges();
                    break;
                case DocumentType.NomenklCurrencyConverterProvider:
                    var pnEx = Context.ProjectRowExclude.Where(_ =>
                        projectIdGuids.Contains(_.Id) && _.NomCurrencyConvertRowId == rowId).ToList();
                    if (pnEx.Count > 0)
                        foreach (var p in pnEx)
                            Context.ProjectRowExclude.Remove(p);

                    Context.SaveChanges();
                    var ccRow = Context.TD_26_CurrencyConvert.FirstOrDefault(_ => _.Id == rowId);
                    if (ccRow is null) return;
                    nomDC = Context.SD_83.FirstOrDefault(_ => _.Id == ccRow.NomenklId)?.DOC_CODE;
                    if (nomDC is null) return;
                    foreach (var pp in projectIdGuids)
                        Context.ProjectRowExclude.Add(new ProjectRowExclude
                        {
                            Id = Guid.NewGuid(),
                            ProjectId = pp,
                            NomCurrencyConvertRowId = rowId,
                            NomenklDC = nomDC.Value
                        });
                    updateExcludeForClient(projectIdGuids, nomDC);

                    Context.SaveChanges();
                    break;
            }
        }

        private void updateExcludeForClient(List<Guid> projectIdGuids, decimal? nomDC)
        {
            var clientRowIds = new HashSet<Guid>();
            foreach (var pId in projectIdGuids)
            {
                var d = Context.ProjectDocuments.Where(_ => _.ProjectId == pId).Select(_ => _.InvoiceClientId).ToList();
                foreach (var r in d.SelectMany(docId =>
                             Context.TD_84.Where(_ => _.DocId == docId).ToList().Where(r => r.SFT_NEMENKL_DC == nomDC)))
                    clientRowIds.Add(r.Id);
            }

            foreach (var rId in clientRowIds)
            foreach (var pp in from pp in projectIdGuids
                     let t = Context.ProjectRowExclude.FirstOrDefault(_ =>
                         _.ProjectId == pp && _.SFClientRowId == rId)
                     where t == null
                     select pp)
                Context.ProjectRowExclude.Add(new ProjectRowExclude
                {
                    Id = Guid.NewGuid(),
                    ProjectId = pp,
                    SFClientRowId = rId,
                    NomenklDC = nomDC.Value
                });
        }

        public void IncludeNomenklToProject(Guid projectIdGuid, decimal nomDC)
        {
            var ids = GetAllTreeProjectIds(projectIdGuid);
            foreach (var old in ids.Select(id => Context.ProjectRowExclude.FirstOrDefault(_ =>
                         _.ProjectId == id && _.NomenklDC == nomDC
                     )))
            {
                if (old is null)
                    return;
                Context.ProjectRowExclude.Remove(old);
            }

            if (Context.ChangeTracker.HasChanges())
                Context.SaveChanges();
        }

        public void IncludeNomenklToProject(List<Guid> projectIdGuids, DocumentType docType, Guid rowId)
        {
            List<ProjectRowExclude> pEx = null;
            switch (docType)
            {
                case DocumentType.InvoiceProvider:
                    pEx = Context.ProjectRowExclude.Where(_ =>
                        projectIdGuids.Contains(_.ProjectId) && _.SFProviderRowId == rowId).ToList();
                    if (pEx.Count == 0) return;
                    foreach (var p in pEx) Context.ProjectRowExclude.Remove(p);
                    break;
                case DocumentType.InvoiceClient:
                    pEx = Context.ProjectRowExclude.Where(_ =>
                        projectIdGuids.Contains(_.ProjectId) && _.SFClientRowId == rowId).ToList();
                    if (pEx.Count == 0) return;
                    foreach (var p in pEx) Context.ProjectRowExclude.Remove(p);
                    break;
                case DocumentType.NomenklCurrencyConverterProvider:
                    pEx = Context.ProjectRowExclude.Where(_ =>
                        projectIdGuids.Contains(_.ProjectId) && _.NomCurrencyConvertRowId == rowId).ToList();
                    if (pEx.Count == 0) return;
                    foreach (var p in pEx) Context.ProjectRowExclude.Remove(p);
                    break;
            }

            Context.SaveChanges();
        }

        public List<Tuple<Guid, int>> GetCountDocumentsForProjects()
        {
            var res = new List<Tuple<Guid, int>>();
            var data = Context
                .ProjectDocuments.Where(_ =>
                    _.CurrencyConvertId != null
                    || _.InvoiceClientId != null
                    || _.InvoiceProviderId != null
                )
                .GroupBy(p => p.ProjectId)
                .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                .ToList();
            foreach (var r in data) res.Add(new Tuple<Guid, int>(r.ProjectId, r.Count));

            return res;
        }

        public void ExcludeFromProfitLoss(Guid projectId)
        {
            var pr = Context.Projects.FirstOrDefault(_ => _.Id == projectId);
            if (pr is null) return;
            pr.IsExcludeFromProfitAndLoss = true;
            Context.SaveChanges();
        }

        public void IncludeInProfitLoss(Guid projectId)
        {
            var pr = Context.Projects.FirstOrDefault(_ => _.Id == projectId);
            if (pr is null) return;
            pr.IsExcludeFromProfitAndLoss = false;
            Context.SaveChanges();
        }

        public void DeleteRowExcludeForDocGuid(Guid id)
        {
            var hasChanges = false;
            var row = Context.ProjectDocuments.FirstOrDefault(_ => _.Id == id);
            if (row is null) return;
            var projs = GetChilds(row.ProjectId);
            if (row.InvoiceClientId is not null)
            {
                var rowsId = Context.TD_84.Where(_ => _.DocId == row.InvoiceClientId).ToList();
                foreach (var ex in rowsId.Select(rowId => Context.ProjectRowExclude.FirstOrDefault(_ =>
                             _.SFClientRowId == rowId.Id)).Where(ex => ex is not null))
                {
                    if (!projs.Contains(ex.ProjectId)) continue;
                    hasChanges = true;
                    Context.ProjectRowExclude.Remove(ex);
                }
            }

            if (row.InvoiceProviderId is not null)
            {
                var rowsId = Context.TD_26.Where(_ => _.DocId == row.InvoiceProviderId).ToList();
                foreach (var ex in rowsId.Select(rowId => Context.ProjectRowExclude.FirstOrDefault(_ =>
                             _.SFProviderRowId == rowId.Id)).Where(ex => ex is not null))
                {
                    if (!projs.Contains(ex.ProjectId)) continue;
                    hasChanges = true;
                    Context.ProjectRowExclude.Remove(ex);
                }
            }

            if (hasChanges)
                Context.SaveChanges();
        }

        public List<TD_24> GetNomenklRows(decimal dc)
        {
            return Context
                .TD_24.Include(_ => _.TD_26)
                .Include(_ => _.TD_84)
                .Where(_ => _.DOC_CODE == dc)
                .ToList();
        }

        public void UpdateManualQuantity(ProjectManualParameter param)
        {
            switch (param.DocType)
            {
                case DocumentType.InvoiceClient:
                    TD_84 cRow = null;
                    if (param.DocId != null)
                        cRow = Context.TD_84.FirstOrDefault(_ =>
                            _.SFT_NEMENKL_DC == param.NomDC && _.DocId == param.DocId.Value);

                    if (param.DocDC != null)
                        cRow = Context.TD_84.FirstOrDefault(_ =>
                            _.SFT_NEMENKL_DC == param.NomDC && _.DOC_CODE == param.DocDC.Value);

                    if (cRow == null) return;
                    var old = Context.ProjectInvoiceQuantityChanged.FirstOrDefault(_ =>
                        _.ProjectId == param.ProjectId && _.ClientRowId == cRow.Id);
                    if (old != null)
                    {
                        if (param.Quantity == (decimal)cRow.SFT_KOL)
                            Context.ProjectInvoiceQuantityChanged.Remove(old);
                        else
                            old.Quantity = param.Quantity;

                        Context.SaveChanges();
                    }
                    else
                    {
                        if (param.Quantity != (decimal)cRow.SFT_KOL)
                        {
                            var newItem = new ProjectInvoiceQuantityChanged
                            {
                                Id = Guid.NewGuid(),
                                ProjectId = param.ProjectId,
                                ClientRowId = cRow.Id,
                                Quantity = param.Quantity > (decimal)cRow.SFT_KOL
                                    ? (decimal)cRow.SFT_KOL
                                    : param.Quantity
                            };
                            Context.ProjectInvoiceQuantityChanged.Add(newItem);
                            Context.SaveChanges();
                        }
                    }

                    break;
                case DocumentType.InvoiceProvider:
                    TD_26 pRow = null;
                    if (param.DocDC != null)
                        pRow = Context.TD_26.FirstOrDefault(_ =>
                            _.SFT_NEMENKL_DC == param.NomDC && _.DOC_CODE == param.DocDC.Value);

                    if (param.DocId != null)
                        pRow = Context.TD_26.FirstOrDefault(_ =>
                            _.SFT_NEMENKL_DC == param.NomDC && _.DocId == param.DocId.Value);

                    if (pRow == null) return;
                    var old2 = Context.ProjectInvoiceQuantityChanged.FirstOrDefault(_ =>
                        _.ProjectId == param.ProjectId && _.ProviderRowId == pRow.Id);
                    if (old2 != null)
                    {
                        if (param.Quantity == pRow.SFT_KOL)
                            Context.ProjectInvoiceQuantityChanged.Remove(old2);
                        else
                            old2.Quantity = param.Quantity;

                        Context.SaveChanges();
                    }
                    else
                    {
                        if (param.Quantity != pRow.SFT_KOL)
                        {
                            var newItem = new ProjectInvoiceQuantityChanged
                            {
                                Id = Guid.NewGuid(),
                                ProjectId = param.ProjectId,
                                ProviderRowId = pRow.Id,
                                Quantity = param.Quantity > pRow.SFT_KOL
                                    ? pRow.SFT_KOL
                                    : param.Quantity
                            };
                            Context.ProjectInvoiceQuantityChanged.Add(newItem);
                            Context.SaveChanges();
                        }
                    }

                    break;
                case DocumentType.NomenklCurrencyConverterProvider:
                    TD_26_CurrencyConvert cpRow = null;
                    if (param.DocDC != null)
                    {
                        var nomId = Context.SD_83.AsNoTracking().Single(_ => _.DOC_CODE == param.NomDC).Id;
                        cpRow = Context.TD_26_CurrencyConvert.FirstOrDefault(_ =>
                            _.NomenklId == nomId && _.DOC_CODE == param.DocDC.Value);
                    }

                    if (cpRow == null) return;
                    var old3 = Context.ProjectInvoiceQuantityChanged.FirstOrDefault(_ =>
                        _.ProjectId == param.ProjectId && _.ProviderCurrencyConvertRowId == cpRow.Id);
                    if (old3 != null)
                    {
                        if (param.Quantity == cpRow.Quantity)
                            Context.ProjectInvoiceQuantityChanged.Remove(old3);
                        else
                            old3.Quantity = param.Quantity;

                        Context.SaveChanges();
                    }
                    else
                    {
                        if (param.Quantity != cpRow.Quantity)
                        {
                            var newItem = new ProjectInvoiceQuantityChanged
                            {
                                Id = Guid.NewGuid(),
                                ProjectId = param.ProjectId,
                                ProviderCurrencyConvertRowId = cpRow.Id,
                                Quantity = param.Quantity > cpRow.Quantity
                                    ? cpRow.Quantity
                                    : param.Quantity
                            };
                            Context.ProjectInvoiceQuantityChanged.Add(newItem);
                            Context.SaveChanges();
                        }
                    }

                    break;
            }
        }

        public List<ManualQuantity> GetManualQuantity(List<Guid> projectId)
        {
            var inclPrIds = new StringBuilder("(");
            foreach (var id in projectId) inclPrIds.Append($"'{CustomFormat.GuidToSqlString(id)}',");

            inclPrIds.Remove(inclPrIds.Length - 1, 1);
            inclPrIds.Append(")");
            var sql = @$"SELECT
        CAST(tab.NomDC AS NUMERIC(18, 0))         AS NomDC,
        tab.ProjectId                             AS ProjectId,
        CAST(tab.DeltaQuantity AS NUMERIC(18, 4)) AS DeltaQuantity,
        CAST(tab.DocType AS INT)                  AS DocType,
        cast(tab.Price AS numeric(18,4))          AS Price
FROM
        (   SELECT
                    t.SFT_NEMENKL_DC AS NomDC,
                    piqc.ProjectId AS ProjectId,
                    t.SFT_KOL - piqc.Quantity AS DeltaQuantity,
                    1 AS DocType,
                    t.SFT_SUMMA_K_OPLATE_KONTR_CRS/t.SFT_KOL AS Price
            FROM
                    ProjectInvoiceQuantityChanged piqc
                INNER JOIN
                  TD_26 t
                      ON piqc.ProviderRowId = t.Id

            UNION
            SELECT
                    t.SFT_NEMENKL_DC,
                    piqc.ProjectId,
                    t.SFT_KOL - piqc.Quantity,
                    0,
                    t.SFT_SUMMA_K_OPLATE_KONTR_CRS/t.SFT_KOL
            FROM
                    ProjectInvoiceQuantityChanged piqc
                INNER JOIN
                  TD_84 t
                      ON piqc.ClientRowId = t.Id

            UNION
            SELECT
                    s83.DOC_CODE,
                    piqc.ProjectId,
                    tcc.Quantity - piqc.Quantity,
                    4,
                    tcc.Price
            FROM
                    ProjectInvoiceQuantityChanged piqc
                INNER JOIN
                  TD_26_CurrencyConvert tcc
                      ON piqc.ProviderCurrencyConvertRowId = tcc.Id
                INNER JOING
                  SD_83 s83
                      ON s83.id = tcc.NomenklId
                INNER JOIN
                  TD_26 t
                      ON tcc.DOC_CODE = t.DOC_CODE
                      AND tcc.CODE = t.CODE) tab
            WHERE  ProjectId in {inclPrIds};";
            var data = Context.Database.SqlQuery<ManualQuantity>(sql);
            return data.ToList();
        }

        List<ProjectManagerDocumentInfo> IProjectRepository.GetDocumentsIncludesInProjects(DateTime dateStart, DateTime dateEnd)
        {
            //ProjectManagerDocumentInfoRequest
            var sql = $@"SELECT * FROM (SELECT s26.Id AS Id,
       s26.DOC_CODE AS DocCode,
       convert(int,26) AS DocType,
       convert(int,s26.SF_IN_NUM) AS NumberInner,
       convert(varchar,s26.SF_POSTAV_NUM) AS NumberOuter,
       s26.SF_POSTAV_DATE AS DocDate,
       s26.SF_POST_DC AS KontrDC,
       NULL AS WarehouseDC,
       (SELECT sum(t26.SFT_SUMMA_K_OPLATE_KONTR_CRS) FROM td_26 t26 WHERE t26.DOC_CODE = s26.DOC_CODE) AS Summa,
       s26.SF_CRS_DC AS CurrencyDC,
       s26.SF_NOTES AS Note,
       s26.CREATOR AS Creator
       FROM sd_26 s26 
    INNER JOIN ProjectDocuments pd ON s26.Id = pd.InvoiceProviderId
union all

SELECT
        s84.Id             AS Id,
        s84.DOC_CODE       AS DocCode,
        84                 AS DocType,
        s84.SF_IN_NUM      AS NumberInner,
        s84.SF_OUT_NUM      AS NumberOuter,
        s84.SF_DATE AS DocDate,
        s84.SF_CLIENT_DC     AS KontrDC,
        NULL               AS WarehouseDC,
        (   SELECT
                    SUM(t84.SFT_SUMMA_K_OPLATE_KONTR_CRS)
            FROM
                    td_84 t84
            WHERE
                    t84.DOC_CODE = s84.DOC_CODE)
        AS Summa,
        s84.SF_CRS_DC      AS CurrencyDC,
        s84.SF_NOTE       AS Note,
        s84.CREATOR        AS Creator
FROM
        sd_84 s84
    INNER JOIN
      ProjectDocuments pd
        ON s84.Id = pd.InvoiceClientId) tab 
        WHERE tab.DocDate BETWEEN '{CustomFormat.DateToString(dateStart)}' AND '{CustomFormat.DateToString(dateEnd)}'";

            var ret = new List<ProjectManagerDocumentInfo>();
            var data = Context.Database.SqlQuery<ProjectManagerDocumentInfoRequest>(sql).ToList();
            foreach (var row in data)
            {
                ret.Add(new ProjectManagerDocumentInfo()
                {
                    Summa = row.Summa ?? 0,
                    Creator = row.Creator,
                    Currency = GlobalOptions.ReferencesCache.GetCurrency(row.CurrencyDC) as Currency,
                    DocCode = row.DocCode,
                    DocDate = row.DocDate,
                    DocType = row.DocType == 84 ? DocumentType.InvoiceClient : DocumentType.InvoiceProvider,
                    Id = row.Id,
                    Note = row.Note,
                    Kontragent = GlobalOptions.ReferencesCache.GetKontragent(row.KontrDC) as Kontragent,
                    NumberInner = row.NumberInner,
                    NumberOuter = row.NumberOuter
                });
            }
            return ret;
        }

        public List<Guid> GetProjetFromDocument(Guid id)
        {
            return Context.ProjectDocuments.Where(_ => _.InvoiceClientId == id || _.InvoiceProviderId == id)
                .Select(_ => _.ProjectId).ToList();
        }

        public void ExcludeInvoiceClientFromProject(Guid invoiceId, Guid projectId)
        {
            var row = Context.ProjectDocuments.FirstOrDefault(_ =>
                _.InvoiceClientId == invoiceId && _.ProjectId == projectId);
            if (row is not null)
            {
                Context.ProjectDocuments.Remove(row);
                Context.SaveChanges();
            }
        }

        public void ExcludeInvoiceProviderFromProject(Guid invoiceId, Guid projectId)
        {
            var row = Context.ProjectDocuments.FirstOrDefault(_ =>
                _.InvoiceProviderId == invoiceId && _.ProjectId == projectId);
            if (row is not null)
            {
                Context.ProjectDocuments.Remove(row);
                Context.SaveChanges();
            }
        }

        private class DocDecimalProjLink
        {
            public decimal DocCode { set; get; }
            public string ProjectName { set; get; }
        }

        private class DocGuidProjLink
        {
            public Guid Id { set; get; }
            public string ProjectName { set; get; }
        }

        private class DocIntProjLink
        {
            public int Code { set; get; }
            public string ProjectName { set; get; }
        }

        #endregion
    }

    public class ManualQuantity
    {
        public decimal NomDC { set; get; }
        public Guid ProjectId { set; get; }
        public decimal DeltaQuantity { set; get; }

        /// <summary>
        ///     0 - инвойс клиентам
        ///     1 - инвойс поставщика
        ///     2 - приход товара
        ///     3 - отгрузка
        ///     4 - валютная конвертация
        /// </summary>
        public int DocType { set; get; }

        public decimal Price { set; get; }
    }
}

public class NomenklMoveForProject_Result : GetNomenklMoveForProject_Result1
{
    //[Display(AutoGenerateField = true, GroupName = "Документы", Name = "Сумма(приход)")]
    public bool IsManualChanged { set; get; }
}
