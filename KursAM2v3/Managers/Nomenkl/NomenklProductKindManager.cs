using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Managers.Base;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;

namespace KursAM2.Managers.Nomenkl
{
    public class NomenklProductKindManager : BaseItemManager<NomenklProductKind>
    {
        public List<NomenklProductKind> Load()
        {
            var ret = new List<NomenklProductKind>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var u in ctx.SD_50)
                    {
                        var n = new NomenklProductKind(u) { State = RowStatus.NotEdited };
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

        public override NomenklProductKind New(NomenklProductKind parent = null)
        {
            return new NomenklProductKind
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                ParentDC = parent?.DocCode
            };
        }

        public override NomenklProductKind NewCopy(NomenklProductKind u)
        {
            return new NomenklProductKind
            {
                DOC_CODE = -1,
                State = RowStatus.NewRow,
                Name = u.Name + "новый",
                ParentDC = u.ParentDC
            };
        }

        public override bool Save(IEnumerable<NomenklProductKind> listProds)
        {
            if (listProds == null || !listProds.Any()) return true;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tn = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var newDC = ctx.SD_50.Any() ? ctx.SD_50.Max(_ => _.DOC_CODE) : 10500000001;
                        foreach (var u in listProds)
                            switch (u.State)
                            {
                                case RowStatus.NewRow:
                                    newDC++;
                                    if (listProds.Any(_ => _.ParentDC == u.DocCode))
                                        foreach (var prd in listProds.Where(_ => _.ParentDC == u.DocCode))
                                            prd.ParentDC = newDC;
                                    var sd50 = new SD_50
                                    {
                                        DOC_CODE = newDC,
                                        PROD_NAME = u.Name,
                                        PROD_FULL_NAME = u.PROD_FULL_NAME,
                                        PROD_PARENT_DC = u.PROD_PARENT_DC
                                    };
                                    ctx.SD_50.Add(sd50);
                                    break;
                                case RowStatus.Edited:
                                    var old1 = ctx.SD_50.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old1 != null)
                                    {
                                        old1.PROD_NAME = u.Name;
                                        old1.PROD_FULL_NAME = u.PROD_FULL_NAME;
                                        old1.PROD_PARENT_DC = u.PROD_PARENT_DC;
                                    }

                                    break;
                                case RowStatus.Deleted:
                                    var old = ctx.SD_50.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old != null)
                                        ctx.SD_50.Remove(old);
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

        public override bool Delete(IEnumerable<NomenklProductKind> listUnits)
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
                            var old = ctx.SD_50.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                            if (old != null)
                                ctx.SD_50.Remove(old);
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
