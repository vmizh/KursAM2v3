using System;
using System.Collections.Generic;
using System.Linq;
using Core.WindowsManager;
using Data;
using KursAM2.Managers.Base;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.Managers.Nomenkl
{
    public class NomenklCategoryManager : BaseItemManager<NomenklGroupViewModel>
    {
        public List<NomenklGroupViewModel> Load()
        {
            var ret = new List<NomenklGroupViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var u in ctx.SD_82)
                    {
                        var n = new NomenklGroupViewModel(u);
                        ret.Add(n);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            return ret;
        }

        public override NomenklGroupViewModel New(NomenklGroupViewModel parent = null)
        {
            return new NomenklGroupViewModel
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                ParentDC = parent?.DocCode
            };
        }

        public override NomenklGroupViewModel NewCopy(NomenklGroupViewModel u)
        {
            return new NomenklGroupViewModel
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                Name = u.Name + "новый"
            };
        }

        public override bool Save(IEnumerable<NomenklGroupViewModel> listProds)
        {
            if (listProds == null || !listProds.Any()) return true;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tn = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var newDC = ctx.SD_82.Any() ? ctx.SD_82.Max(_ => _.DOC_CODE) : 10820000001;
                        foreach (var u in listProds)
                            switch (u.State)
                            {
                                case RowStatus.NewRow:
                                    newDC++;
                                    if (listProds.Any(_ => _.ParentDC == u.DocCode))
                                        foreach (var prd in listProds.Where(_ => _.ParentDC == u.DocCode))
                                            prd.ParentDC = newDC;
                                    var sd82 = new SD_82
                                    {
                                        DOC_CODE = newDC,
                                        CAT_NAME = u.Name,
                                        CAT_PARENT_DC = u.ParentDC
                                    };
                                    ctx.SD_82.Add(sd82);
                                    break;
                                case RowStatus.Edited:
                                    var old1 = ctx.SD_82.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old1 != null)
                                    {
                                        old1.CAT_NAME = u.Name;
                                        old1.CAT_PARENT_DC = u.ParentDC;
                                    }

                                    break;
                                case RowStatus.Deleted:
                                    var old = ctx.SD_82.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old != null)
                                        ctx.SD_82.Remove(old);
                                    break;
                            }

                        ctx.SaveChanges();
                        tn.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tn.Rollback();
                        WindowManager.ShowDBError(ex);
                        return false;
                    }
                }
            }
        }

        public override bool Delete(IEnumerable<NomenklGroupViewModel> listUnits)
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
                            var old = ctx.SD_82.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                            if (old != null)
                                ctx.SD_82.Remove(old);
                            break;
                        }

                        ctx.SaveChanges();
                        tn.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tn.Rollback();
                        WindowManager.ShowError(ex);
                        return false;
                    }
                }
            }
        }
    }
}
