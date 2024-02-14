using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using KursDomain.References;
using KursDomain.Repository.Base;
using KursDomain.RepositoryHelper;

namespace KursDomain.Repository.TransferOut;

public interface ITransferOutBalansRepository : IKursGenericRepository<TransferOutBalans, Guid>
{
    int GetNewDocumentNumber();

    TransferOutBalans GetById(Guid id);

    Task<List<TransferOutBalans>> GetForDatesAsync(DateTime start, DateTime end);
    Task<List<TransferOutBalans>> GetSearchTextAsync(string searchText, DateTime? start = null, DateTime? end = null);

    Task<List<TransferOutBalans>> GetForWarehouseAsync(Warehouse warehouse, DateTime? start = null,
        DateTime? end = null);

    Task<List<TransferOutBalans>> GetForLocationsAsync(StorageLocations location, DateTime? start = null,
        DateTime? end = null);

    Task<List<TransferOutBalans>> GetForNomenklAsync(Nomenkl nom, DateTime? start = null, DateTime? end = null);

    void RemoveRow(TransferOutBalansRows row);
    void AddRow(TransferOutBalansRows row);

    TransferOutBalans New();
    Task<TransferOutBalans> NewCopyAsync(Guid id);
    Task<TransferOutBalans> NewCopyRequisiteAsync(Guid id);

    Task<List<NomenklStoreLocationItem>> GetLocationStoreRemainAsync(Guid? slId = null, DateTime? date = null);

    Task<List<TransferOutBalansRows>> GetLocationStorageRemainDocuments(decimal nomDC, Guid? slId = null,
        DateTime? date = null);
}
