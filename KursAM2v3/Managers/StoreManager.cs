using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Managers.Base;

namespace KursAM2.Managers
{
    public class StoreManager : BaseItemManager<Warehouse>
    {
        private StandartErrorManager errorManager;

        public StoreManager(StandartErrorManager errManager)
        {
            errorManager = errManager;
        }
        public List<Warehouse> Load()
        {
            var ret = new List<Warehouse>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var u in ctx.SD_27)
                    {
                        var n = new Warehouse(u) {State = RowStatus.NotEdited};
                        ret.Add(n);
                    }
                }
            }
            catch (Exception ex)
            {
                errorManager.WriteErrorMessage(ex, "Ошибка","StoreManager.Load");
            }
            return ret;
        }

        public override Warehouse New(Warehouse parent = null)
        {
            return new Warehouse
            {
                DocCode = -1,
                Id = Guid.NewGuid(),
                State = RowStatus.NewRow,
                ParentId = parent?.Id
            };
        }

        public override Warehouse New()
        {
            return new Warehouse
            {
                DocCode = -1,
                Id = Guid.NewGuid(),
                State = RowStatus.NewRow,

            };
        }

        public override Warehouse NewCopy(Warehouse u)
        {
            return new Warehouse
            {
                DOC_CODE = -1,
                Id = Guid.NewGuid(),
                ParentId = u.ParentId,
                State = RowStatus.NewRow,
                Name = u.Name + "новый"
            };
        }

        public override bool Save(IEnumerable<Warehouse> listProds)
        {
            if (listProds == null || !listProds.Any()) return true;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tn = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var newDC = ctx.SD_27.Any() ? ctx.SD_27.Max(_ => _.DOC_CODE)+1 : 10270000001;
                        foreach (var u in listProds)
                            switch (u.State)
                            {
                                case RowStatus.NewRow:
                                    if (listProds.Any(_ => _.ParentDC == u.DocCode))
                                        foreach (var prd in listProds.Where(_ => _.ParentDC == u.DocCode))
                                            prd.ParentDC = newDC;
                                    var sd27 = new SD_27
                                    {
                                        DOC_CODE = newDC,
                                        SKL_NAME = u.Name,
                                        ParentId = u.ParentId,
                                        Id = u.Id,
                                        TABELNUMBER = u.TABELNUMBER,
                                        SKL_REGION_DC = u.SKL_REGION_DC

                                    };
                                    ctx.SD_27.Add(sd27);
                                    newDC++;
                                    break;
                                case RowStatus.Edited:
                                    var old1 = ctx.SD_27.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old1 != null)
                                    {
                                        old1.SKL_NAME = u.Name;
                                        old1.ParentId = u.ParentId;
                                        old1.TABELNUMBER = u.TABELNUMBER;
                                        old1.SKL_REGION_DC = u.SKL_REGION_DC;
                                    }
                                    break;
                                case RowStatus.Deleted:
                                    var old = ctx.SD_27.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old != null)
                                        ctx.SD_27.Remove(old);
                                    break;
                            }
                        ctx.SaveChanges();
                        tn.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tn.Rollback();
                        errorManager.WriteErrorMessage(ex, "Ошибка",null);
                        return false;
                    }
                }
            }
        }

        public override bool Delete(IEnumerable<Warehouse> listUnits)
        {
            if (listUnits == null || !listUnits.Any()) return true;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tn = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var u in listUnits)
                        {
                            var old = ctx.SD_27.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                            if (old != null)
                                ctx.SD_27.Remove(old);
                            break;
                        }
                        ctx.SaveChanges();
                        tn.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tn.Rollback();
                        errorManager.WriteErrorMessage(ex);
                        return false;
                    }
                }
            }
        }
    }
}