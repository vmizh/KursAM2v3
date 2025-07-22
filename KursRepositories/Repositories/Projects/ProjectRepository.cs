using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Data;
using Helper;
using KursAM2.Repositories.RedisRepository;
using KursDomain;
using KursDomain.Base;
using KursDomain.Documents.Base;
using KursDomain.Documents.CommonReferences;
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

        #endregion

        #region InvoiceProvider

        public int GetInvoicesProviderCount(DateTime dateStart, DateTime dateEnd)
        {
            return Context.InvoicePostQuery.Count(_ => _.Date >= dateStart && _.Date <= dateEnd);
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

        public IEnumerable<ProjectDocumentInfo> LoadProjectDocuments(Guid projectId)
        {
            var prj = Context
                .Projects.Include(_ => _.ProjectDocuments)
                .AsNoTracking()
                .FirstOrDefault(_ => _.Id == projectId);
            if (prj is null)
                return new List<ProjectDocumentInfo>();
            return LoadProjectDocuments(prj);
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
                _ => new List<Guid>(),
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

                _ => null,
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
                _ => null,
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

        public IEnumerable<ProjectDocumentInfo> LoadProjectDocuments(Data.Projects project)
        {
            var ret = new List<ProjectDocumentInfo>();
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
                        }
                    }

                    if (p.CurrencyConvertId is not null)
                    {
                        var ord = Context
                            .TD_26_CurrencyConvert.Include(_ => _.TD_26)
                            .Include(_ => _.TD_26.SD_26)
                            .FirstOrDefault(_ => _.Id == p.CurrencyConvertId);
                        if (ord != null)
                        {
                            var kontr =
                                GlobalOptions.ReferencesCache.GetKontragent(
                                    ord.TD_26.SD_26.SF_POST_DC
                                ) as Kontragent;
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
                        }
                    }

                    if (p.InvoiceProviderId is not null)
                    {
                        var d = Context
                            .InvoicePostQuery.Where(_ => _.Id == p.InvoiceProviderId)
                            .ToList();

                        if (d != null)
                        {
                            var doc = new InvoiceProviderBase(d);
                            newItem.Kontragent = doc.Kontragent;
                            newItem.Currency = newItem.Kontragent?.Currency as Currency;
                            newItem.SummaIn = doc.Summa;
                            newItem.SummaPay = doc.PaySumma;
                            newItem.SummaShipped = doc.SummaFact;
                            newItem.QuantityInDocument = d.Where(_ => _.DocCode == doc.DocCode)
                                .Sum(_ => _.Quantity);
                            foreach (var r in d)
                                if (r.IsUsluga ?? false)
                                    newItem.QuantityInShipped += r.Quantity;
                                else
                                    newItem.QuantityInShipped += d.Where(_ =>
                                            _.DocCode == doc.DocCode
                                        )
                                        .Sum(_ => _.Shipped);
                            newItem.QuantityInRemain =
                                newItem.QuantityInDocument - newItem.QuantityInShipped;
                            newItem.SummaInRemain = newItem.SummaIn - newItem.SummaShipped;
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
                            var doc = new InvoiceClientBase(d);
                            newItem.Kontragent = doc.Client;
                            newItem.Currency = doc.Currency;
                            newItem.SummaOut = doc.Summa;
                            newItem.SummaPay = doc.PaySumma;
                            newItem.SummaShipped = doc.SummaOtgruz;
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
                            ErrorText = $"Для проекта {old.Name} есть связанные в документах",
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
            return Context.Projects.AsNoTracking().ToList();
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
                        $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник проектов",
                };
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
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

        public List<GetNomenklMoveForProject_Result> GetNomenklMoveForProject(
            Guid projectId,
            bool isRecursive
        )
        {
            return Context
                .GetNomenklMoveForProject(projectId, (byte?)(isRecursive ? 1 : 0))
                .ToList();
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
            );
            var invoiceClient = invoicesClient.Where(_ =>
                _.TD_84.Any(x => x.SFT_NEMENKL_DC == nomDC)
            );

            foreach (var doc in invoiceProvider)
            {
                if (ret.All(_ => _.DocCode != doc.DOC_CODE))
                {
                    var shippedRow =
                        Context
                            .TD_24.Include(_ => _.SD_24).Include(_ => _.TD_26)
                            .FirstOrDefault(_ => _.DDT_SPOST_DC == doc.DOC_CODE);
                    var shipped =        shippedRow?.TD_26.SFT_SUMMA_K_OPLATE_KONTR_CRS
                                * shippedRow?.TD_26.SFT_KOL / shippedRow?.DDT_KOL_PRIHOD ?? 0;
                    ret.Add(
                        new ProjectNomenklMoveDocumentInfo
                        {
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
                            Warehouse = shippedRow != null ? GlobalOptions.ReferencesCache.GetWarehouse(shippedRow.SD_24.DD_SKLAD_POL_DC) as Warehouse : null,
                            ProviderQuantity = doc.ProviderInvoicePay.Sum(_ => _.Summa),
                            ProviderShipped = shipped,
                            ProviderSumma = doc.TD_26.Sum(_ => _.SFT_SUMMA_K_OPLATE_KONTR_CRS) ?? 0,
                            ProviderShippedQuantity = shippedRow?.DDT_KOL_PRIHOD ?? 0,
                        }
                    );
                }

                var s = doc.TD_26.FirstOrDefault(_ => _.SFT_NEMENKL_DC == nomDC);
                if (s is null)
                    continue;

                //var prih = Context
                //    .TD_24.Include(_ => _.SD_24)
                //    .Include(_ => _.TD_26)
                //    .FirstOrDefault(_ =>
                //        _.DDT_SPOST_DC == s.DOC_CODE && _.DDT_SPOST_ROW_CODE == s.CODE
                //    );
                //if (prih != null && ret.All(_ => _.DocCode != prih.DOC_CODE))
                //    ret.Add(
                //        new ProjectNomenklMoveDocumentInfo
                //        {
                //            DocCode = prih.DOC_CODE,
                //            Kontragent =
                //                GlobalOptions.ReferencesCache.GetKontragent(doc.SF_POST_DC)
                //                as Kontragent,
                //            DocDate = prih.SD_24.DD_DATE,
                //            DocumentType = DocumentType.StoreOrderIn,
                //            Note = prih.SD_24.DD_NOTES,
                //            ExtNumber = prih.SD_24.DD_EXT_NUM,
                //            InnerNumber = prih.SD_24.DD_IN_NUM,
                //            Creator = prih.SD_24.CREATOR,
                //        }
                //    );

                //var nakl = Context
                //    .DistributeNakladRow.Include(_ => _.DistributeNaklad)
                //    .Where(_ => _.TovarInvoiceRowId == s.Id)
                //    .ToList();
                //foreach (var nak in nakl)
                //{
                //    if (ret.Any(_ => _.Id == nak.DocId))
                //        continue;
                //    ret.Add(
                //        new ProjectNomenklMoveDocumentInfo
                //        {
                //            Id = nak.DocId,
                //            Kontragent =
                //                GlobalOptions.ReferencesCache.GetKontragent(doc.SF_POST_DC)
                //                as Kontragent,
                //            DocDate = nak.DistributeNaklad.DocDate,
                //            DocumentType = DocumentType.Naklad,
                //            Note = nak.DistributeNaklad.Note,
                //            ExtNumber = nak.DistributeNaklad.DocNum,
                //            InnerNumber = 0,
                //            Creator = nak.DistributeNaklad.Creator,
                //        }
                //    );
                //}
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
                        ClientShipped = shippedRow?.DDT_KOL_RASHOD *s.SFT_SUMMA_K_OPLATE_KONTR_CRS/(decimal?)s.SFT_KOL ?? 0,
                        ClientShippedQuantity = shippedRow?.DDT_KOL_RASHOD ?? 0,
                        Warehouse = shippedRow != null ? (Warehouse)GlobalOptions.ReferencesCache.GetWarehouse(shippedRow.SD_24.DD_SKLAD_OTPR_DC) : null
                    }
                );

                //var prih = Context
                //    .TD_24.Include(_ => _.SD_24)
                //    .FirstOrDefault(_ =>
                //        _.DDT_SFACT_DC == s.DOC_CODE && _.DDT_SFACT_ROW_CODE == s.CODE
                //    );
                //if (prih != null)
                //    ret.Add(
                //        new ProjectNomenklMoveDocumentInfo
                //        {
                //            DocCode = prih.DOC_CODE,
                //            Kontragent =
                //                GlobalOptions.ReferencesCache.GetKontragent(doc.SF_CLIENT_DC)
                //                as Kontragent,
                //            DocDate = prih.SD_24.DD_DATE,
                //            DocumentType = DocumentType.Waybill,
                //            Note = prih.SD_24.DD_NOTES,
                //            ExtNumber = prih.SD_24.DD_EXT_NUM,
                //            InnerNumber = prih.SD_24.DD_IN_NUM,
                //            Warehouse =
                //                GlobalOptions.ReferencesCache.GetWarehouse(
                //                    prih.SD_24.DD_SKLAD_OTPR_DC
                //                ) as Warehouse,
                //            Creator = prih.SD_24.CREATOR,
                //        }
                //    );
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
                    new ProjectRowExclude()
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = prjId,
                        NomenklDC = nomDC,
                    }
                );
                Context.SaveChanges();
            }
        }

        public void IncludeNomenklToProject(Guid projectIdGuid, decimal nomDC)
        {
            var old = Context.ProjectRowExclude.FirstOrDefault(_ =>
                _.ProjectId == projectIdGuid && _.NomenklDC == nomDC
            );
            if (old is null)
                return;
            Context.ProjectRowExclude.Remove(old);
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
            foreach (var r in data)
            {
                res.Add(new Tuple<Guid, int>(r.ProjectId, r.Count));
            }

            return res;
        }

        public List<TD_24> GetNomenklRows(decimal dc)
        {
            return Context
                .TD_24.Include(_ => _.TD_26)
                .Include(_ => _.TD_84)
                .Where(_ => _.DOC_CODE == dc)
                .ToList();
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
}
