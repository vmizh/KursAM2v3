using Data;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Projects;
using KursDomain.Result;
using System;
using System.Collections.Generic;

namespace KursRepositories.Repositories.Projects
{
    public interface IProjectRepository
    {
        IBoolResult SaveReference(IEnumerable<Data.Projects> data, IEnumerable<Guid> deleteIds = null);
        IEnumerable<Data.Projects> LoadReference();

        IEnumerable<ProjectDocumentInfo> LoadProjectDocuments(Data.Projects project);
        IEnumerable<ProjectDocumentInfo> LoadProjectDocuments(Guid projectId);

        void AddDocumentInfo(ProjectDocumentInfoBase doc);
        void UpdateDocumentInfo(Guid id, string note);
        void DeleteDocumentInfo(Guid projectId);

        IBoolResult SaveGroups(IEnumerable<ProjectGroups> data, IEnumerable<Guid> deleteGrpIds = null, IEnumerable<Guid> deleteLinkIds = null);
        IEnumerable<Data.ProjectGroups> LoadGroups();

        IEnumerable<SD_33> GetCashInForProject(Guid projectId, DateTime dateStart, DateTime dateEnd);
        IEnumerable<SD_34> GetCashOutForProject(Guid projectId, DateTime dateStart, DateTime dateEnd);

        IEnumerable<TD_101> GetBankForProject(Guid projectId, DateTime dateStart, DateTime dateEnd);
        IEnumerable<SD_24> GetWarehouseInForProject(Guid projectId, DateTime dateStart, DateTime dateEnd);
        IEnumerable<SD_24> GetWaybillInForProject(Guid projectId, DateTime dateStart, DateTime dateEnd);

        IEnumerable<TD_84> GetUslugaClientForProject(Guid projectId, DateTime dateStart, DateTime dateEnd);
        IEnumerable<TD_26> GetUslugaProviderForProject(Guid projectId, DateTime dateStart, DateTime dateEnd);

        IEnumerable<AccuredAmountForClientRow> GetAccruedAmountForClients(Guid projectId, DateTime dateStart, DateTime dateEnd);
        IEnumerable<AccuredAmountOfSupplierRow> GetAccruedAmountOfSuppliers(Guid projectId, DateTime dateStart, DateTime dateEnd);
        IEnumerable<TD_26_CurrencyConvert> GetCurrencyConverts(Guid projectId, DateTime dateStart, DateTime dateEnd);

        void UpdateCache();

        string GetDocDescription(DocumentType docType, ProjectDocumentInfoBase doc);

        decimal? GetInvoiceClientDC(Guid rowId);
        decimal? GetInvoiceProviderDC(Guid rowId, bool isCrsConvert);

        Guid? GetAccruedAmountForClientsId(Guid rowId);
        Guid? GetAccruedAmountProviderId(Guid rowId);

        List<TD_24> GetNomenklRows(decimal dc);

        List<Guid> GetInvoiceProjects(DocumentType docType, decimal dc, bool isCrsConvert);

        List<Guid> GetAllTreeProjectIds(Guid id);
        List<Guid> GetChilds(List<Data.Projects> list, Guid id);
    }
}
