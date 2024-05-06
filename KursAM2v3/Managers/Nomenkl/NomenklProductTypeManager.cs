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
    public class NomenklProductTypeManager : BaseItemManager<NomenklProductType>
    {
        public override List<NomenklProductType> LoadList()
        {
            var ret = new List<NomenklProductType>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var u in ctx.SD_119)
                    {
                        var n = new NomenklProductType(u) { State = RowStatus.NotEdited };
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

        public override NomenklProductType New(decimal newDC = -1)
        {
            return new NomenklProductType
            {
                DOC_CODE = newDC,
                State = RowStatus.NewRow,
                MC_DELETED = 0,
                MC_PROC_OTKL = 0,
                MC_TARA = 0,
                MC_TRANSPORT = 0,
                MC_PREDOPLATA = 0
            };
        }

        public override NomenklProductType NewCopy(NomenklProductType u, decimal newDC = -1)
        {
            return new NomenklProductType
            {
                DOC_CODE = newDC,
                State = RowStatus.NewRow,
                MC_NAME = u.Name,
                MC_DELETED = 0,
                MC_PROC_OTKL = 0,
                MC_TARA = 0,
                MC_TRANSPORT = 0,
                MC_PREDOPLATA = 0,
                Name = u.Name + "новый"
            };
        }

        public override bool Save(IEnumerable<NomenklProductType> listUnits)
        {
            if (listUnits == null || !listUnits.Any()) return true;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tn = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var newDC = ctx.SD_119.Any() ? ctx.SD_119.Max(_ => _.DOC_CODE) : 11190000001;
                        foreach (var u in listUnits)
                            switch (u.State)
                            {
                                case RowStatus.NewRow:
                                    newDC++;
                                    var sd119 = new SD_119
                                    {
                                        DOC_CODE = newDC,
                                        MC_NAME = u.Name,
                                        MC_DELETED = u.MC_DELETED,
                                        MC_PROC_OTKL = u.MC_PROC_OTKL,
                                        MC_TARA = u.MC_TARA,
                                        MC_TRANSPORT = u.MC_TRANSPORT,
                                        MC_PREDOPLATA = u.MC_PREDOPLATA
                                    };
                                    ctx.SD_119.Add(sd119);
                                    break;
                                case RowStatus.Edited:
                                    var old1 = ctx.SD_119.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old1 != null)
                                    {
                                        old1.MC_NAME = u.Name;
                                        old1.MC_DELETED = u.MC_DELETED;
                                        old1.MC_PROC_OTKL = u.MC_PROC_OTKL;
                                        old1.MC_TARA = u.MC_TARA;
                                        old1.MC_TRANSPORT = u.MC_TRANSPORT;
                                        old1.MC_PREDOPLATA = u.MC_PREDOPLATA;
                                    }

                                    break;
                                case RowStatus.Deleted:
                                    var old = ctx.SD_119.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old != null)
                                        ctx.SD_119.Remove(old);
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

        public override bool Delete(IEnumerable<NomenklProductType> listUnits)
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
                            var old = ctx.SD_175.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                            if (old != null)
                                ctx.SD_175.Remove(old);
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
