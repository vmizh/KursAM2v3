using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Helper;
using KursDomain.ICommon;
using KursDomain.References;
using KursDomain.Repository.Base;
using KursDomain.RepositoryHelper;

namespace KursDomain.Repository.TransferOut;

public class TransferOutBalansRepository : KursGenericRepository<TransferOutBalans, ALFAMEDIAEntities, Guid>,
    ITransferOutBalansRepository
{
    private readonly string _searchQuery = "TRANSFER_OUT_SELECT_SEARCH";
    private readonly string _searchQueryWithDate = "TRANSFER_OUT_SELECT_SEARCH_WITH_DATE";

    public TransferOutBalansRepository(ALFAMEDIAEntities context) : base(context)
    {

    }

    public int GetNewDocumentNumber()
    { 
        return Context.Database.SqlQuery<int>("SELECT isnull(max(DocNum),0) FROM TransferOutBalans (nolock)").First() + 1;
    }

    public TransferOutBalans GetById(Guid id)
    {
        return Context.Set<TransferOutBalans>().Find(id);
    }

    public async Task<List<TransferOutBalans>> GetForDatesAsync(DateTime start, DateTime end)
    {
        return await Context.Set<TransferOutBalans>()
            .Include(_ => _.StorageLocations)
            .Include(_ => _.TransferOutBalansRows)
            .Where(_ => _.DocDate >= start && _.DocDate <= end).ToListAsync();
    }

    public async Task<List<TransferOutBalans>> GetSearchTextAsync(string searchText, DateTime? start = null,
        DateTime? end = null)
    {
        List<Guid> ids;
        if (start == null && end == null)
        {
            ids = await getSearchedListAsync(searchText, null, null);
        }
        else
        {
            if (start == null) start = DateTime.MinValue;
            if (end == null) end = DateTime.MaxValue;
            ids = await getSearchedListAsync(searchText, start, end);
        }

        return await Context.Set<TransferOutBalans>().Include(_ => _.StorageLocations)
            .Include(_ => _.SD_27)
            .Include(_ => _.TransferOutBalansRows)
            .Include(_ => _.TransferOutBalansRows.Select(t => t.SD_83))
            .Where(_ => ids.Contains(_.Id)).AsNoTracking().ToListAsync();
    }

    public async Task<List<TransferOutBalans>> GetForWarehouseAsync(Warehouse warehouse, DateTime? start = null,
        DateTime? end = null)
    {
        if (start == null && end == null)
            return await Context.Set<TransferOutBalans>().Include(_ => _.StorageLocations)
                .Include(_ => _.SD_27).Include(_ => _.TransferOutBalansRows)
                .Where(_ => _.SD_27.DOC_CODE == warehouse.DocCode)
                .ToListAsync();
        if (start == null) start = DateTime.MinValue;
        if (end == null) end = DateTime.MaxValue;
        return await Context.Set<TransferOutBalans>().Include(_ => _.StorageLocations)
            .Include(_ => _.SD_27).Include(_ => _.TransferOutBalansRows)
            .Where(_ => _.SD_27.DOC_CODE == warehouse.DocCode && _.DocDate >= start && _.DocDate <= end)
            .ToListAsync();
    }

    public async Task<List<TransferOutBalans>> GetForLocationsAsync(StorageLocations location, DateTime? start = null,
        DateTime? end = null)
    {
        if (start == null && end == null)
            return await Context.Set<TransferOutBalans>().Include(_ => _.StorageLocations)
                .Include(_ => _.SD_27).Include(_ => _.TransferOutBalansRows)
                .Where(_ => _.StorageLocations.Id == location.Id)
                .ToListAsync();
        if (start == null) start = DateTime.MinValue;
        if (end == null) end = DateTime.MaxValue;
        return await Context.Set<TransferOutBalans>().Include(_ => _.StorageLocations)
            .Include(_ => _.SD_27).Include(_ => _.TransferOutBalansRows)
            .Where(_ => _.StorageLocations.Id == location.Id && _.DocDate >= start && _.DocDate <= end)
            .ToListAsync();
    }

    public async Task<List<TransferOutBalans>> GetForNomenklAsync(Nomenkl nom, DateTime? start = null,
        DateTime? end = null)
    {
        if (start == null && end == null)
            return await (from doc in Context.Set<TransferOutBalans>().Include(_ => _.StorageLocations)
                    .Include(_ => _.SD_27).Include(_ => _.TransferOutBalansRows)
                from row in Context.Set<TransferOutBalansRows>().Include(_ => _.SD_83)
                where row.DocId == doc.Id
                where row.NomenklDC == nom.DocCode
                select doc).ToListAsync();

        if (start == null) start = DateTime.MinValue;
        if (end == null) end = DateTime.MaxValue;
        return await (from doc in Context.Set<TransferOutBalans>().Include(_ => _.StorageLocations)
                .Include(_ => _.SD_27).Include(_ => _.TransferOutBalansRows)
                .Where(d => d.DocDate >= start && d.DocDate <= end)
            from row in Context.Set<TransferOutBalansRows>().Include(_ => _.SD_83)
            where row.DocId == doc.Id
            where row.NomenklDC == nom.DocCode
            select doc).ToListAsync();
    }

    public void RemoveRow(TransferOutBalansRows row)
    {
        Context.Set<TransferOutBalansRows>().Remove(row);
    }

    public void AddRow(TransferOutBalansRows row)
    {
        Context.Set<TransferOutBalansRows>().Add(row);
    }

    public TransferOutBalans New()
    {
        return Context.Set<TransferOutBalans>().Add(new TransferOutBalans
        {
            Id = Guid.NewGuid(),
            DocDate = DateTime.Today,
            Creator = GlobalOptions.UserInfo.NickName,
            DocNum = -1,
        });
    }

    public async Task<TransferOutBalans> NewCopyAsync(Guid id)
    {
        var d = await Context.Set<TransferOutBalans>()
            .Include(_ => _.TransferOutBalansRows).SingleAsync(_ => _.Id == id);
        Context.Entry(d).State = EntityState.Added;
        SetNewValues(d);
        var l = new List<Guid>(d.TransferOutBalansRows.Select(_ => _.Id));
        foreach (var i in l)
        {
            var old = d.TransferOutBalansRows.Single(_ => _.Id == i);
            Context.Entry(old).State = EntityState.Added;
            old.Id = Guid.NewGuid();
            old.DocId = d.Id;
        }
        return d;
    }

    public async Task<TransferOutBalans> NewCopyRequisiteAsync(Guid id)
    {
        var d = await GetByIdAsync(id); 
        Context.Entry(d).State = EntityState.Added;
        SetNewValues(d);
        d.TransferOutBalansRows.Clear();
        return d;
    }

    public async Task<List<NomenklStoreLocationItem>> GetLocationStoreRemainAsync(Guid? slId = null, DateTime? date = null)
    {
        if(date == null) date = DateTime.Today;
        if (slId == null)
        {
            return await (from d in Context.Set<TransferOutBalansRows>().Include(_ => _.TransferOutBalans)
                where d.TransferOutBalans.DocDate <= date
                select new NomenklStoreLocationItem
                {
                    NomenklDC = d.NomenklDC,
                    Quantity = d.Quatntity,
                    Summa = d.Quatntity * d.Price,
                    StorageLocationId = d.TransferOutBalans.StorageLocationId
                }).ToListAsync();
        }
        return await (from d in Context.Set<TransferOutBalansRows>().Include(_ => _.TransferOutBalans)
            where d.TransferOutBalans.DocDate <= date && d.TransferOutBalans.StorageLocationId == slId
            select new NomenklStoreLocationItem
            {
                NomenklDC = d.NomenklDC,
                Quantity = d.Quatntity,
                Summa = d.Quatntity * d.Price,
                StorageLocationId = d.TransferOutBalans.StorageLocationId
            }).ToListAsync();
    }

    public async Task<List<TransferOutBalansRows>> GetLocationStorageRemainDocuments(decimal nomDC, Guid? slId = null,
        DateTime? dateEnd = null)
    {
        var date = dateEnd ?? DateTime.Today;
        if (slId == null)
        {
            return await Context.Set<TransferOutBalansRows>()
                .Include(_ => _.TransferOutBalans)
                .Include(_ => _.TransferOutBalans.StorageLocations).AsNoTracking()
                .Where(_ => _.NomenklDC == nomDC && _.TransferOutBalans.DocDate <= date).ToListAsync();
        }

        return await Context.Set<TransferOutBalansRows>()
            .Include(_ => _.TransferOutBalans)
            .Include(_ => _.TransferOutBalans.StorageLocations).AsNoTracking()
            .Where(_ => _.NomenklDC == nomDC && _.TransferOutBalans.DocDate <= date &&
                        _.TransferOutBalans.StorageLocationId == slId).ToListAsync();
    }

    private void SetNewValues(TransferOutBalans entity)
    {
        entity.Id = Guid.NewGuid();
        entity.Creator = GlobalOptions.UserInfo.NickName;
        entity.DocDate = DateTime.Today;
        entity.DocNum = -1;
    }

    
    public override RowStatus GetRowStatus(TransferOutBalans model)
    {
        var s = Context.Entry(model).State;
        if (s == EntityState.Added) return RowStatus.NewRow;
        //if (model.TransferOutBalansRows == null || model.TransferOutBalansRows.Count == 0)
        //    return s != EntityState.Unchanged ? RowStatus.Edited : RowStatus.NotEdited;
        if (model.TransferOutBalansRows.Any(r => Context.Entry(r).State != EntityState.Unchanged))
            return RowStatus.Edited;
        return s != EntityState.Unchanged ? RowStatus.Edited : RowStatus.NotEdited;
    }


    private async Task<List<Guid>> getSearchedListAsync(string search, DateTime? start, DateTime? end)
    {
        var sql = start == null && end == null
            ? Context.Set<EXT_SqlQueries>().FirstOrDefault(_ => _.Name == _searchQuery)?.Query
            : Context.Set<EXT_SqlQueries>().FirstOrDefault(_ => _.Name == _searchQueryWithDate)?.Query;
        if (sql == null) return new List<Guid>();

        sql = sql.Replace("$SEARCH$", search).Replace("$DATE_START$", CustomFormat.DateToString(start))
            .Replace("$DATE_END$", CustomFormat.DateToString(end));
        return await Context.Database.SqlQuery<Guid>(sql).ToListAsync();
    }
}
