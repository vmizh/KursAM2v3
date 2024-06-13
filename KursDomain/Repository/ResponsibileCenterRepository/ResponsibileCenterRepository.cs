using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Data;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.ResponsibileCenterRepository;

public class ResponsibileCenterRepository : KursGenericRepository<SD_40, ALFAMEDIAEntities, decimal>,
    IResponsibileCenterRepository
{
    private KursSystemEntities _systemEntities;

    public ResponsibileCenterRepository(ALFAMEDIAEntities context) : base(context)
    {
        _systemEntities = GlobalOptions.KursSystem();
    }

    public IEnumerable<SD_40> GetAll()
    {
        return Context.Set<SD_40>().ToList();
    }

    public IEnumerable<UserRightsResponsibilityCenter> GetUserRightAll()
    {
        return _systemEntities.UserRightsResponsibilityCenter.Include(_ => _.Users)
            .Where(_ => _.DbId == GlobalOptions.DataBaseId).AsNoTracking().ToList();
    }

    public IEnumerable<UserRightsResponsibilityCenter> GetUserRightForCentResp(decimal centrRespDC)
    {
        return _systemEntities.UserRightsResponsibilityCenter.Include(_ => _.Users)
            .Where(_ => _.DbId == GlobalOptions.DataBaseId && _.RespCentDC == centrRespDC).AsNoTracking().ToList();
    }

    public void UserRightSet(Guid userId, decimal centrRespDC)
    {
        var dcs = getChildDocCodes(centrRespDC);
        foreach (var d in dcs)
        {
            var row = _systemEntities.UserRightsResponsibilityCenter
                .FirstOrDefault(_ => _.DbId == GlobalOptions.DataBaseId && _.UserId == userId && _.RespCentDC == centrRespDC);
            if (row == null)
            {
                _systemEntities.UserRightsResponsibilityCenter.Add(new UserRightsResponsibilityCenter()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RespCentDC = centrRespDC,
                    DbId = GlobalOptions.DataBaseId
                });
            }
        }
        _systemEntities.SaveChanges();
    }

    public void UserRightUnSet(Guid userId, decimal centrRespDC)
    {
        var dcs = getChildDocCodes(centrRespDC);
        foreach (var d in dcs)
        {
            var row = _systemEntities.UserRightsResponsibilityCenter
                .FirstOrDefault(_ =>
                    _.DbId == GlobalOptions.DataBaseId && _.UserId == userId && _.RespCentDC == centrRespDC);
            if (row != null)
            {
                _systemEntities.UserRightsResponsibilityCenter.Remove(row);
            }

        }
        _systemEntities.SaveChanges();
    }

    private List<decimal> getChildDocCodes(decimal dc)
    {
        List<decimal> ret = new List<decimal> { dc };
        var childs = Context.SD_40.Where(_ => _.CENT_PARENT_DC == dc);
        if (!childs.Any()) return ret;
        foreach (var ch in childs)
        {
            ret.AddRange(getChildDocCodes(ch.CENT_PARENT_DC.Value));
        }
        return ret;
    }
}
