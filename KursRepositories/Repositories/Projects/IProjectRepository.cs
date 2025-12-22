using System;
using System.Collections.Generic;
using Data;
using KursDomain.Base;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Projects;
using KursDomain.IDocuments.Finance;
using KursDomain.Result;

namespace KursRepositories.Repositories.Projects
{
    public record ProjectManualParameter
    {
        public DocumentType DocType { set; get; } 
        public Guid ProjectId { set; get; }
        public Guid? DocId { set; get; }
        public decimal? DocDC { set; get; }
        public decimal NomDC { set; get; }
        public decimal Quantity { set; get; }
    }

    public interface IProjectRepository
    {
        void UpdateManualQuantity(ProjectManualParameter param);

        IBoolResult SaveReference(
            IEnumerable<Data.Projects> data,
            IEnumerable<Guid> deleteIds = null
        );
        IEnumerable<Data.Projects> LoadReference();

        IEnumerable<ProjectDocumentInfo> LoadProjectDocuments(Guid projectId, bool isShowAll);

        IEnumerable<ProjectDocumentInfo> LoadProjectDocuments2(Guid projectId, bool isShowAll);

        void AddDocumentInfo(ProjectDocumentInfoBase doc);
        void UpdateDocumentInfo(Guid id, string note);
        void DeleteDocumentInfo(Guid projectId);

        IBoolResult SaveGroups(
            IEnumerable<ProjectGroups> data,
            IEnumerable<Guid> deleteGrpIds = null,
            IEnumerable<Guid> deleteLinkIds = null
        );

        IEnumerable<ProjectGroups> LoadGroups();

        IEnumerable<SD_33> GetCashInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        );
        int GetcCashInCount(DateTime dateStart, DateTime dateEnd);

        IEnumerable<SD_33> GetCashInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        int GetCashOutCount(DateTime dateStart, DateTime dateEnd);
        IEnumerable<SD_34> GetCashOutForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        );

        IEnumerable<SD_34> GetCashOutForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        int GetBankCount(DateTime dateStart, DateTime dateEnd);
        IEnumerable<TD_101> GetBankForProject(Guid projectId, DateTime dateStart, DateTime dateEnd);

        IEnumerable<TD_101> GetBankForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        int GetWarehouseInCount(DateTime dateStart, DateTime dateEnd);
        IEnumerable<SD_24> GetWarehouseInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        );

        IEnumerable<SD_24> GetWarehouseInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        int GetWaybillCount(DateTime dateStart, DateTime dateEnd);
        IEnumerable<SD_24> GetWaybillInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        );

        IEnumerable<SD_24> GetWaybillInForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        int GetUslugaClientCount(DateTime dateStart, DateTime dateEnd);

        IEnumerable<TD_84> GetUslugaClientForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        IEnumerable<TD_84> GetUslugaClientForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        );
        int GetUslugaProviderCount(DateTime dateStart, DateTime dateEnd);
        IEnumerable<TD_26> GetUslugaProviderForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        );

        IEnumerable<TD_26> GetUslugaProviderForProject(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        int GetAccruedAmountForClientsCount(DateTime dateStart, DateTime dateEnd);

        IEnumerable<AccuredAmountForClientRow> GetAccruedAmountForClients(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        );

        IEnumerable<AccuredAmountForClientRow> GetAccruedAmountForClients(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        int GetAccruedAmountOfSuppliersCount(DateTime dateStart, DateTime dateEnd);

        IEnumerable<AccuredAmountOfSupplierRow> GetAccruedAmountOfSuppliers(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        );

        IEnumerable<AccuredAmountOfSupplierRow> GetAccruedAmountOfSuppliers(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        int GetCurrencyConvertsCount(DateTime dateStart, DateTime dateEnd);
        IEnumerable<TD_26_CurrencyConvert> GetCurrencyConverts(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        );

        IEnumerable<TD_26_CurrencyConvert> GetCurrencyConverts(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        void UpdateCache();

        string GetDocDescription(DocumentType docType, ProjectDocumentInfoBase doc);

        decimal? GetInvoiceClientDC(Guid rowId, bool isRow);
        decimal? GetInvoiceProviderDC(Guid rowId, bool isRow, bool isCrsConvert);

        Guid? GetAccruedAmountForClientsId(Guid rowId);
        Guid? GetAccruedAmountProviderId(Guid rowId);

        List<IInvoiceProvider> GetInvoicesProvider(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        );

        List<IInvoiceProvider> GetInvoicesProvider(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        List<IInvoiceClient> GetInvoicesClient(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd
        );

        List<IInvoiceClient> GetInvoicesClient(
            Guid projectId,
            DateTime dateStart,
            DateTime dateEnd,
            int page,
            int limit,
            out int count
        );

        int GetInvoicesProviderCount(DateTime dateStart, DateTime dateEnd);
        int GetInvoicesClientCount(DateTime dateStart, DateTime dateEnd);

        List<TD_24> GetNomenklRows(decimal dc);

        List<TD_84> GetInvoiceClientRows(Guid id);
        List<TD_26> GetInvoiceProviderRows(Guid id);

        List<Guid> GetDocumentsProjects(DocumentType docType, decimal dc, bool isCrsConvert);

        Dictionary<decimal, string> GetInvoicesLinkWithProjects(
            DocumentType docType,
            bool isCrsConvert
        );

        Dictionary<IdItem, string> GetDocumentsLinkWithProjects(DocumentType docType);

        List<Guid> GetAllTreeProjectIds(Guid id);
        List<Guid> GetChilds(List<Data.Projects> list, Guid id);

        List<ManualQuantity> GetManualQuantity(Guid projectId);

        List<NomenklMoveForProject_Result> GetNomenklMoveForProject(
            Guid projectId,
            bool isRecursive,
            bool isExcludeShow
        );

        List<Guid> GetChilds(Guid projectId);

        List<ProjectNomenklMoveDocumentInfo> GetDocumentsForNomenkl(
            List<Guid> projectIds,
            decimal nomDC
        );

        List<ProjectNomenklMoveDocumentInfo> GetDocumentsForNomenkl(
            List<Guid> projectIds,
            decimal nomDC,
            bool isExcludeShow
        );

        List<ProjectRowExclude> GetDocumentsRowExclude(List<Guid> projectIdGuids);

        void ExcludeNomenklFromProjects(List<Guid> projectIdGuids, decimal nomDC);

        void ExcludeNomenklFromProjects(List<Guid> projectIdGuids, DocumentType docType, Guid rowId);

        void IncludeNomenklToProject(Guid projectIdGuid, decimal nomDC);

        void IncludeNomenklToProject(List<Guid> projectIdGuids, DocumentType docType, Guid rowId);

        List<Tuple<Guid, int>> GetCountDocumentsForProjects();

        void ExcludeFromProfitLoss(Guid projectId);
        void IncludeInProfitLoss(Guid projectId);

        void DeleteRowExcludeForDocGuid(Guid id);

    }
}
