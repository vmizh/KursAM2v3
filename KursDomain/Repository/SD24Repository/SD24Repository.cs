using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using AutoMapper;
using Data;
using KursDomain.IDocuments;
using KursDomain.Repository.Base;
using KursDomain.Documents.NomenklManagement;

namespace KursDomain.Repository.SD24Repository;

public class SD24Repository : KursGenericRepository<SD_24, ALFAMEDIAEntities, decimal>,
    ISD24Repository
{
    public SD24Repository(ALFAMEDIAEntities context) : base(context)
    {
    }

    public Task<IEnumerable<SD_24>> SearchAsync(MaterialDocumentTypeEnum materialDocumentType)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SD_24>> SearchAsync(List<MaterialDocumentTypeEnum> materialDocumentTypes)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd)
    {
        return await Context.SD_24.Include(_ => _.TD_24)
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_26.SD_26")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_122")
            .Include("TD_24.SD_170")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_1751")
            .Include("TD_24.SD_2")
            .Include("TD_24.SD_254")
            .Include("TD_24.SD_27")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_3011")
            .Include("TD_24.SD_3012")
            .Include("TD_24.SD_303")
            .Include("TD_24.SD_384")
            .Include("TD_24.SD_43")
            .Include("TD_24.SD_83")
            .Include("TD_24.SD_831")
            .Include("TD_24.SD_832")
            .Include("TD_24.SD_84")
            .Include("TD_24.TD_73")
            .Include("TD_24.TD_9")
            .Include("TD_24.TD_84")
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_241")
            .Where(_ => _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd)
            .ToListAsync();
    }

    public async Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd,
        MaterialDocumentTypeEnum materialDocumentType)
    {
        if ((decimal)materialDocumentType == 0) return new List<SD_24>();
        return await Context.SD_24.Include(_ => _.TD_24)
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_26.SD_26")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_122")
            .Include("TD_24.SD_170")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_1751")
            .Include("TD_24.SD_2")
            .Include("TD_24.SD_254")
            .Include("TD_24.SD_27")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_3011")
            .Include("TD_24.SD_3012")
            .Include("TD_24.SD_303")
            .Include("TD_24.SD_384")
            .Include("TD_24.SD_43")
            .Include("TD_24.SD_83")
            .Include("TD_24.SD_831")
            .Include("TD_24.SD_832")
            .Include("TD_24.SD_84")
            .Include("TD_24.TD_73")
            .Include("TD_24.TD_9")
            .Include("TD_24.TD_84")
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_241")
            .Where(_ => _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd && _.DD_TYPE_DC == (decimal)materialDocumentType)
            .ToListAsync();
    }

    public async Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd,
        List<MaterialDocumentTypeEnum> materialDocumentTypes)
    {
        if (materialDocumentTypes == null || materialDocumentTypes.Count == 0) return new List<SD_24>();
        var dcs = materialDocumentTypes.Cast<decimal>().ToList();
        return await Context.SD_24.Include(_ => _.TD_24)
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_26.SD_26")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_122")
            .Include("TD_24.SD_170")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_1751")
            .Include("TD_24.SD_2")
            .Include("TD_24.SD_254")
            .Include("TD_24.SD_27")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_3011")
            .Include("TD_24.SD_3012")
            .Include("TD_24.SD_303")
            .Include("TD_24.SD_384")
            .Include("TD_24.SD_43")
            .Include("TD_24.SD_83")
            .Include("TD_24.SD_831")
            .Include("TD_24.SD_832")
            .Include("TD_24.SD_84")
            .Include("TD_24.TD_73")
            .Include("TD_24.TD_9")
            .Include("TD_24.TD_84")
            .Include("TD_24.TD_26")
        .Include("TD_24.TD_241")
            .Where(_ => _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd && dcs.Contains(_.DD_TYPE_DC))
            .ToListAsync();
    }

    public Task<IEnumerable<SD_24>> SearchAsync(MaterialDocumentTypeEnum materialDocumentType, string pattern)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SD_24>> SearchAsync(List<MaterialDocumentTypeEnum> materialDocumentTypes, string pattern)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd, string pattern)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd,
        MaterialDocumentTypeEnum materialDocumentType, string pattern)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SD_24>> SearchAsync(DateTime dateStart, DateTime dateEnd,
        List<MaterialDocumentTypeEnum> materialDocumentTypes, string pattern)
    {
        throw new NotImplementedException();
    }

    public SD_24 GetDocument(decimal dc)
    {
        return Context.SD_24.Include(_ => _.TD_24)
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_26.SD_26")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_122")
            .Include("TD_24.SD_170")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_1751")
            .Include("TD_24.SD_2")
            .Include("TD_24.SD_254")
            .Include("TD_24.SD_27")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_3011")
            .Include("TD_24.SD_3012")
            .Include("TD_24.SD_303")
            .Include("TD_24.SD_384")
            .Include("TD_24.SD_43")
            .Include("TD_24.SD_83")
            .Include("TD_24.SD_831")
            .Include("TD_24.SD_832")
            .Include("TD_24.SD_84")
            .Include("TD_24.TD_73")
            .Include("TD_24.TD_9")
            .Include("TD_24.TD_84")
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_241")
            .FirstOrDefault(_ => _.DOC_CODE == dc);
    }

    public SD_24 GetDocument(Guid id)
    {
        return  Context.SD_24.Include(_ => _.TD_24)
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_26.SD_26")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_122")
            .Include("TD_24.SD_170")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_1751")
            .Include("TD_24.SD_2")
            .Include("TD_24.SD_254")
            .Include("TD_24.SD_27")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_3011")
            .Include("TD_24.SD_3012")
            .Include("TD_24.SD_303")
            .Include("TD_24.SD_384")
            .Include("TD_24.SD_43")
            .Include("TD_24.SD_83")
            .Include("TD_24.SD_831")
            .Include("TD_24.SD_832")
            .Include("TD_24.SD_84")
            .Include("TD_24.TD_73")
            .Include("TD_24.TD_9")
            .Include("TD_24.TD_84")
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_241")
            .FirstOrDefault(_ => _.Id == id);
    }

    public async Task<SD_24> GetDocumentAsync(decimal dc)
    {
        return await Context.SD_24.Include(_ => _.TD_24)
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_26.SD_26")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_122")
            .Include("TD_24.SD_170")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_1751")
            .Include("TD_24.SD_2")
            .Include("TD_24.SD_254")
            .Include("TD_24.SD_27")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_3011")
            .Include("TD_24.SD_3012")
            .Include("TD_24.SD_303")
            .Include("TD_24.SD_384")
            .Include("TD_24.SD_43")
            .Include("TD_24.SD_83")
            .Include("TD_24.SD_831")
            .Include("TD_24.SD_832")
            .Include("TD_24.SD_84")
            .Include("TD_24.TD_73")
            .Include("TD_24.TD_9")
            .Include("TD_24.TD_84")
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_241")
            .FirstOrDefaultAsync(_ => _.DOC_CODE == dc);
    }

    public async Task<SD_24> GetDocumentAsync(Guid id)
    {
        return await Context.SD_24.Include(_ => _.TD_24)
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_26.SD_26")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_122")
            .Include("TD_24.SD_170")
            .Include("TD_24.SD_175")
            .Include("TD_24.SD_1751")
            .Include("TD_24.SD_2")
            .Include("TD_24.SD_254")
            .Include("TD_24.SD_27")
            .Include("TD_24.SD_301")
            .Include("TD_24.SD_3011")
            .Include("TD_24.SD_3012")
            .Include("TD_24.SD_303")
            .Include("TD_24.SD_384")
            .Include("TD_24.SD_43")
            .Include("TD_24.SD_83")
            .Include("TD_24.SD_831")
            .Include("TD_24.SD_832")
            .Include("TD_24.SD_84")
            .Include("TD_24.TD_73")
            .Include("TD_24.TD_9")
            .Include("TD_24.TD_84")
            .Include("TD_24.TD_26")
            .Include("TD_24.TD_241")
            .FirstOrDefaultAsync(_ => _.Id == id);
    }

    public SD_24 CreateNew()
    {
        var newItem = new SD_24
        {
            DOC_CODE = -1,
            Id = Guid.NewGuid(),
            DD_DATE = DateTime.Today,
            CREATOR = GlobalOptions.UserInfo.NickName,
            DD_IN_NUM = -1,
            DD_EXT_NUM = null
        };
        Context.SD_24.Add(newItem);
        return newItem;
    }

    public SD_24 CreateCopy(SD_24 ent)
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<SD_24, SD_24>());
        var mapper = new Mapper(config);
        var ret = mapper.Map<SD_24>(ent);
        ret.DOC_CODE = -1;
        ret.DD_DATE = DateTime.Today;
        ret.CREATOR = GlobalOptions.UserInfo.NickName;
        ret.DD_IN_NUM = -1;
        ret.DD_EXT_NUM = null;
        Context.SD_24.Add(ret);
        return ret;
    }

    public SD_24 CreateCopy(Guid id)
    {
        throw new NotImplementedException();
    }

    public SD_24 CreateCopy(decimal dc)
    {
        var ret = GetDocument(dc);
        ret.DOC_CODE = -1;
        ret.DD_DATE = DateTime.Today;
        ret.Id = Guid.NewGuid();
        ret.CREATOR = GlobalOptions.UserInfo.NickName;
        foreach (var t in ret.TD_24)
        {
            t.DOC_CODE = -1;
            Context.Entry(t).State = EntityState.Added;
        }

        Context.Entry(ret).State = EntityState.Added;
        return ret;
    }

    public SD_24 CreateRequisiteCopy(SD_24 ent)
    {
        throw new NotImplementedException();
    }

    public SD_24 CreateRequisiteCopy(Guid id)
    {
        throw new NotImplementedException();
    }

    public SD_24 CreateRequisiteCopy(decimal dc)
    {
        var ret = GetDocument(dc);
        Context.Entry(ret).State = EntityState.Added;
        ret.DOC_CODE = -1;
        ret.DD_DATE = DateTime.Today;
        ret.Id = Guid.NewGuid();
        ret.CREATOR = GlobalOptions.UserInfo.NickName;
        ret.TD_24.Clear();

        return ret;
    }
}
