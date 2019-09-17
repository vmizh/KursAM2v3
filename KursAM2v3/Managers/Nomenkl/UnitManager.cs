using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Managers.Base;

namespace KursAM2.Managers.Nomenkl
{
    public class UnitManager : BaseItemManager<Unit>
    {
        public override List<Unit> LoadList()
        {
            var ret = new List<Unit>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var u in ctx.SD_175)
                    {
                        var n = new Unit(u) { State = RowStatus.NotEdited };
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

        public override Unit New()
        {
            return new Unit
            {
                DOC_CODE = -1,
                State = RowStatus.NewRow,
                ED_IZM_MONEY = 0,
                ED_IZM_INT = 1
            };
        }

        public override Unit New(Unit u = default(Unit))
        {
            return new Unit
            {
                DOC_CODE = -1,
                State = RowStatus.NewRow,
                ED_IZM_MONEY = 0,
                ED_IZM_INT = 1
            };
        }

        public override Unit NewCopy(Unit u)
        {
            return new Unit
            {
                DOC_CODE = -1,
                State = RowStatus.NewRow,
                ED_IZM_OKEI = null,
                ED_IZM_OKEI_CODE = null,
                ED_IZM_NAME = u.Name,
                ED_IZM_INT = u.ED_IZM_INT,
                ED_IZM_MONEY = 0,
                Name = u.Name
            };
        }

        public override bool Save(IEnumerable<Unit> listUnits)
        {
            if (listUnits == null || !listUnits.Any()) return true;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tn = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var newDC = ctx.SD_175.Any() ? ctx.SD_175.Max(_ => _.DOC_CODE) : 11750000001;
                        foreach (var u in listUnits)
                            switch (u.State)
                            {
                                case RowStatus.NewRow:
                                    newDC++;
                                    var sd175 = new SD_175
                                    {
                                        DOC_CODE = newDC,
                                        ED_IZM_OKEI = u.ED_IZM_OKEI,
                                        ED_IZM_OKEI_CODE = u.ED_IZM_OKEI_CODE,
                                        ED_IZM_NAME = u.Name,
                                        ED_IZM_INT = u.ED_IZM_INT,
                                        ED_IZM_MONEY = u.ED_IZM_MONEY
                                    };
                                    ctx.SD_175.Add(sd175);
                                    break;
                                case RowStatus.Edited:
                                    var old1 = ctx.SD_175.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old1 != null)
                                    {
                                        old1.ED_IZM_OKEI = u.ED_IZM_OKEI;
                                        old1.ED_IZM_OKEI_CODE = u.ED_IZM_OKEI_CODE;
                                        old1.ED_IZM_NAME = u.Name;
                                        old1.ED_IZM_INT = u.ED_IZM_INT;
                                        old1.ED_IZM_MONEY = u.ED_IZM_MONEY;
                                    }
                                    break;
                                case RowStatus.Deleted:
                                    var old = ctx.SD_175.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old != null)
                                        ctx.SD_175.Remove(old);
                                    break;
                            }
                        ctx.SaveChanges();
                        tn.Commit();
                        MainReferences.Units.Clear();
                        foreach (var item in ctx.SD_175.AsNoTracking().ToList())
                            MainReferences.Units.Add(item.DOC_CODE, new Unit(item));
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

        public override bool Delete(IEnumerable<Unit> listUnits)
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