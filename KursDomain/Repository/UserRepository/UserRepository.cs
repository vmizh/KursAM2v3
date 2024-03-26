using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Data;
using Helper;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.UserRepository;

public class UserRepository : KursGenericRepository<Users, KursSystemEntities, Guid>, IUserRepository
{
    public UserRepository(KursSystemEntities context) : base(context)
    {
    }

    public List<Users> GetUsersForStores(decimal storeDC)
    {
        List<Users>  ret = new List<Users>();
        using (var ctx = GlobalOptions.GetEntities())
        {
            var uids = ctx.Database.SqlQuery<int>(
                $"SELECT USR_ID FROM HD_27 WHERE DOC_CODE = {CustomFormat.DecimalToSqlDecimal(storeDC)}").ToList();
            foreach (var uid in uids)
            {
                var u = ctx.EXT_USERS.FirstOrDefault(_ => _.USR_ID == uid);
                if (u != null)
                {
                    var us = Context.Users.FirstOrDefault(_ => _.Name == u.USR_NICKNAME);
                    if(us != null)
                        ret.Add(us);
                }
            }
        }

        return ret;
    }

    public List<Users> GetAllUsers()
    {
        var usrs = Context.Users.Where(_ => _.DataSources.Any(d => d.Id == GlobalOptions.DataBaseId)).ToList();
        List<EXT_USERS> localUsers;

        using (var ctx = GlobalOptions.GetEntities())
        {
            localUsers = ctx.EXT_USERS.ToList();
        }

        return (from u in localUsers
            where usrs.Any(_ => _.Name == u.USR_NICKNAME)
            select usrs.Single(_ => _.Name == u.USR_NICKNAME)).ToList();
    }

    public Users GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     По id user получить список доступных складов
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public List<SD_27> GetStores(Guid id)
    {
        var ret = new List<SD_27>();
        var u = Context.Users.Single(_ => _.Id == id);
        using (var ctx = GlobalOptions.GetEntities())
        {
            var localUser = ctx.EXT_USERS.AsNoTracking().FirstOrDefault(_ => _.USR_NICKNAME == u.Name);
            if (localUser != null)
            {
                var dc = ctx.Database.SqlQuery<decimal>($"SELECT DOC_CODE from HD_27 where USR_ID = {localUser.USR_ID}")
                    .ToList();
                foreach (var store in ctx.SD_27.AsNoTracking().ToList())
                    if (dc.Any(_ => _ == store.DOC_CODE))
                        ret.Add(store);
            }
        }

        return ret;
    }

    public EXT_USERS GetUsers(string name)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            return ctx.EXT_USERS.AsNoTracking().FirstOrDefault(_ => _.USR_NICKNAME == name);
        }
    }
}
