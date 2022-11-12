using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using Calculates.Materials;
using Core.WindowsManager;
using Data;
using KursAM2.Managers.Base;
using KursAM2.ViewModel.Logistiks;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.Managers
{
    public class InventorySheetManager : DocumentManager<InventorySheetViewModel>
    {
        //private readonly WindowManager myWindowManager = new WindowManager();
        public List<Warehouse> StoreCollection =>
            GlobalOptions.ReferencesCache.GetWarehousesAll().Cast<Warehouse>().ToList();

        public override List<InventorySheetViewModel> GetDocuments()
        {
            List<SD_24> docs;
            var ret = new List<InventorySheetViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                docs =
                    ctx.SD_24
                        .Where(_ => _.DD_TYPE_DC == 2010000005)
                        .ToList();
            }

            ret.Clear();
            ret.AddRange(docs.Select(d => new InventorySheetViewModel(d)));
            return ret;
        }

        public override List<InventorySheetViewModel> GetDocuments(DateTime dateStart, DateTime dateEnd)
        {
            List<SD_24> docs;
            var ret = new List<InventorySheetViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                docs =
                    ctx.SD_24
                        .Include(_ => _.TD_24)
                        .Where(_ => _.DD_TYPE_DC == 2010000005 && _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd)
                        .ToList();
            }

            foreach (var d in docs) ret.Add(new InventorySheetViewModel(d));
            return ret;
        }

        public override List<InventorySheetViewModel> GetDocuments(DateTime dateStart, DateTime dateEnd,
            string searchText)
        {
            var docs = new List<SD_24>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var heads =
                    ctx.SD_24
                        .Include(_ => _.TD_24)
                        .Include(_ => _.SD_271)
                        .Where(_ => _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd && _.DD_TYPE_DC == 2010000005)
                        .ToList();
                docs.AddRange(heads.Where(_ => _.SD_271.SKL_NAME.ToUpper().Contains(searchText.ToUpper())));
                var noms = new List<SD_24>();
                foreach (
                    var h in heads.Where(h => h.TD_24
                        .Select(item => GlobalOptions.ReferencesCache.GetNomenkl(item.DDT_NOMENKL_DC))
                        .Any(nom => nom.NomenklNumber.ToUpper().Contains(searchText.ToUpper())
                                    || ((IName)nom).Name.ToUpper().Contains(searchText.ToUpper()))))
                {
                    noms.Add(h);
                    break;
                }

                foreach (
                    var n in
                    from n in noms let dcs = docs.Where(_ => _.DOC_CODE == n.DOC_CODE) where !dcs.Any() select n)
                    docs.Add(n);
            }

            return docs.Select(d => new InventorySheetViewModel(d)).ToList();
        }

        public override InventorySheetViewModel Save(InventorySheetViewModel doc)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        switch (doc.State)
                        {
                            case RowStatus.Edited:
                                var dh = ctx.SD_24.SingleOrDefault(_ => _.DOC_CODE == doc.DocCode);
                                if (dh == null) return null;
                                dh.DD_SKLAD_OTPR_DC = doc.Warehouse.DocCode;
                                dh.DD_SKLAD_POL_DC = doc.Warehouse.DocCode;
                                dh.DD_DATE = doc.Date;
                                dh.DD_EXECUTED = (short)(doc.IsClosed ? 1 : 0);
                                dh.DD_POLUCH_NAME = doc.Warehouse.Name;
                                dh.DD_OTRPAV_NAME = doc.Warehouse.Name;
                                dh.DD_NOTES = doc.Note;
                                //TODO Обязательно исправить!!!
                                //foreach (
                                //    var delrow in
                                //    doc.DeletedRows.Select(
                                //            d =>
                                //                    ctx.TD_24.FirstOrDefault(
                                //                        _ =>
                                //                            _.DOC_CODE == d.DocCode &&
                                //                            _.DDT_NOMENKL_DC == d.Nomenkl.DocCode))
                                //        .Where(delrow => delrow != null))
                                //    ctx.TD_24.Remove(delrow);
                                var code = 0;
                                var rs =
                                    ctx.TD_24.Where(_ => _.DOC_CODE == doc.DocCode).ToList();
                                if (rs.Count > 0)
                                    code = rs.Max(_ => _.CODE) + 1;
                                foreach (var dr in doc.Rows)
                                    switch (dr.State)
                                    {
                                        case RowStatus.NewRow:
                                            ctx.TD_24.Add(ctx.TD_24.Add(new TD_24
                                                {
                                                    DOC_CODE = doc.DocCode,
                                                    CODE = code,
                                                    DDT_NOMENKL_DC = dr.Nomenkl.DocCode,
                                                    DDT_OSTAT_NOV = (double?)dr.QuantityFact,
                                                    DDT_OSTAT_STAR = (double?)dr.QuantityCalc,
                                                    DDT_KOL_PRIHOD = dr.Difference >= 0 ? dr.Difference : 0,
                                                    DDT_KOL_RASHOD = dr.Difference < 0 ? -dr.Difference : 0,
                                                    DDT_ED_IZM_DC = ((IDocCode)dr.Nomenkl.Unit).DocCode,
                                                    DDT_CRS_DC = ((IDocCode)dr.Nomenkl.Currency).DocCode,
                                                    DDT_TAX_CENA = dr.Price,
                                                    DDT_TAX_CRS_CENA = dr.Price,
                                                    DDT_FACT_CENA = dr.Price,
                                                    DDT_FACT_CRS_CENA = dr.Price,
                                                    DDT_TAX_EXECUTED = (short)(dr.IsTaxExecuted ? 1 : 0),
                                                    DDT_SHPZ_DC = 13030000006,
                                                    DocId = doc.Id,
                                                    Id = Guid.NewGuid(),
                                                    DDT_CENA_V_UCHET_VALUTE = 0,
                                                    DDT_SUMMA_V_UCHET_VALUTE = 0
                                                }
                                            ));
                                            code++;
                                            break;
                                        case RowStatus.Edited:
                                            var row = ctx.TD_24.SingleOrDefault(_ => _.Id == dr.Id);
                                            if (row == null) break;
                                            row.DDT_NOMENKL_DC = dr.Nomenkl.DocCode;
                                            row.DDT_KOL_PRIHOD = dr.Difference >= 0 ? dr.Difference : 0;
                                            row.DDT_KOL_RASHOD = dr.Difference < 0 ? -dr.Difference : 0;
                                            row.DDT_OSTAT_NOV = (double?)dr.QuantityFact;
                                            row.DDT_OSTAT_STAR = (double?)dr.QuantityCalc;
                                            row.DDT_ED_IZM_DC = ((IDocCode)dr.Nomenkl.Unit).DocCode;
                                            row.DDT_CRS_DC = ((IDocCode)dr.Nomenkl.Currency).DocCode;
                                            row.DDT_TAX_CENA = dr.Price;
                                            row.DDT_TAX_CRS_CENA = dr.Price;
                                            row.DDT_FACT_CENA = dr.Price;
                                            row.DDT_FACT_CRS_CENA = dr.Price;
                                            row.DDT_TAX_EXECUTED = (short)(dr.IsTaxExecuted ? 1 : 0);
                                            row.DDT_SHPZ_DC = 13030000006;
                                            row.DDT_CENA_V_UCHET_VALUTE = 0;
                                            row.DDT_SUMMA_V_UCHET_VALUTE = 0;
                                            break;
                                    }

                                break;
                            case RowStatus.NewRow:
                                var dc = ctx.SD_24.Any() ? ctx.SD_24.Max(_ => _.DOC_CODE) + 1 : 10240000001;
                                var id = Guid.NewGuid();
                                doc.DocCode = dc;
                                ctx.SD_24.Add(new SD_24
                                {
                                    DOC_CODE = dc,
                                    DD_DATE = doc.Date,
                                    DD_EXECUTED = (short)(doc.IsClosed ? 1 : 0),
                                    DD_TYPE_DC = 2010000005,
                                    DD_NOTES = doc.Note,
                                    Id = id,
                                    DD_EXT_NUM = string.Empty,
                                    DD_SKLAD_OTPR_DC = doc.Warehouse.DocCode,
                                    DD_SKLAD_POL_DC = doc.Warehouse.DocCode,
                                    DD_POLUCH_NAME = doc.Warehouse.Name,
                                    DD_OTRPAV_NAME = doc.Warehouse.Name,
                                    CREATOR = GlobalOptions.UserInfo.NickName
                                });
                                var rowNum = 1;
                                foreach (var dr in doc.Rows)
                                {
                                    ctx.TD_24.Add(new TD_24
                                    {
                                        DOC_CODE = dc,
                                        CODE = rowNum,
                                        DDT_NOMENKL_DC = dr.Nomenkl.DocCode,
                                        DDT_OSTAT_NOV = (double?)dr.QuantityFact,
                                        DDT_OSTAT_STAR = (double?)dr.QuantityCalc,
                                        DDT_KOL_PRIHOD = dr.Difference >= 0 ? dr.Difference : 0,
                                        DDT_KOL_RASHOD = dr.Difference < 0 ? -dr.Difference : 0,
                                        DDT_ED_IZM_DC = ((IDocCode)dr.Nomenkl.Unit).DocCode,
                                        DDT_CRS_DC = ((IDocCode)dr.Nomenkl.Currency).DocCode,
                                        DDT_TAX_CENA = dr.Price,
                                        DDT_TAX_CRS_CENA = dr.Price,
                                        DDT_FACT_CENA = dr.Price,
                                        DDT_FACT_CRS_CENA = dr.Price,
                                        DDT_TAX_EXECUTED = (short)(dr.IsTaxExecuted ? 1 : 0),
                                        DDT_SHPZ_DC = 13030000006,
                                        DocId = id,
                                        Id = dr.Id,
                                        DDT_CENA_V_UCHET_VALUTE = 0,
                                        DDT_SUMMA_V_UCHET_VALUTE = 0
                                    });
                                    rowNum++;
                                }

                                break;
                        }

                        if (GlobalOptions.SystemProfile.NomenklCalcType == NomenklCalcType.NakladSeparately)
                        {
                            var calc = new NomenklCostMediumSliding(ctx);
                            foreach (
                                var op in
                                doc.Rows.Where(_ => _.State != RowStatus.NotEdited)
                                    .Select(_ => _.Nomenkl.DocCode)
                                    .Distinct()
                                    .ToList())
                            {
                                var ops = calc.GetOperations(op);
                                calc.Save(ops);
                            }
                        }

                        ctx.SaveChanges();
                        tnx.Commit();
                        return null;
                    }
                    catch (Exception ex)
                    {
                        tnx.Rollback();
                        WindowManager.ShowError(ex);
                        return null;
                    }
                }
            }
        }

        public override bool IsChecked(InventorySheetViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override InventorySheetViewModel New()
        {
            throw new NotImplementedException();
        }

        public override InventorySheetViewModel NewFullCopy(InventorySheetViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override InventorySheetViewModel NewRequisity(InventorySheetViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override void Delete(InventorySheetViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override InventorySheetViewModel Load()
        {
            throw new NotImplementedException();
        }

        public override InventorySheetViewModel Load(decimal docCode)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var doc =
                    ctx.SD_24
                        .Include(_ => _.TD_24)
                        .SingleOrDefault(_ => _.DOC_CODE == docCode);
                if (doc == null) return null;
                var ret = new InventorySheetViewModel(doc);
                foreach (var row in doc.TD_24)
                    ret.Rows.Add(new InventorySheetRowViewModel(row));
                foreach (var r in ret.Rows)
                    r.State = RowStatus.NotEdited;
                ret.State = RowStatus.NotEdited;
                return ret;
            }
        }

        public override InventorySheetViewModel Load(Guid id)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var doc =
                    ctx.SD_24
                        .Include(_ => _.TD_24)
                        .SingleOrDefault(_ => _.Id == id);
                return doc == null ? null : Load(doc.DOC_CODE);
            }
        }

        public override void Delete(decimal docCode)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = new TransactionScope())
                {
                    try
                    {
                        ctx.TD_24.RemoveRange(ctx.TD_24.Where(_ => _.DOC_CODE == docCode));
                        ctx.SD_24.RemoveRange(ctx.SD_24.Where(_ => _.DOC_CODE == docCode));
                        ctx.SaveChanges();
                        tnx.Complete();
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public override void Delete(Guid id)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = new TransactionScope())
                {
                    try
                    {
                        ctx.TD_24.RemoveRange(ctx.TD_24.Where(_ => _.DocId == id));
                        ctx.SD_24.RemoveRange(ctx.SD_24.Where(_ => _.Id == id));
                        ctx.SaveChanges();
                        tnx.Complete();
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public InventorySheetViewModel CreateNew()
        {
            int num;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var n = ctx.SD_24.Where(_ => _.DD_TYPE_DC == 2010000005);
                if (!n.Any())
                    num = 1;
                else
                    num = n.Max(_ => _.DD_IN_NUM) + 1;
            }

            var ret = new InventorySheetViewModel(null)
            {
                Date = DateTime.Today,
                Creator = GlobalOptions.UserInfo.NickName,
                Id = Guid.NewGuid(),
                State = RowStatus.NewRow,
                Num = num
            };
            return ret;
        }

        public InventorySheetViewModel CreateNew(InventorySheetViewModel doc,
            DocumentCopyType copyType = DocumentCopyType.Empty)
        {
            InventorySheetViewModel ret = null;
            switch (copyType)
            {
                case DocumentCopyType.Requisite:
                    ret = new InventorySheetViewModel(null)
                    {
                        Date = doc.Date,
                        Creator = GlobalOptions.UserInfo.NickName,
                        Id = Guid.NewGuid()
                    };
                    break;
                case DocumentCopyType.Full:
                    ret = new InventorySheetViewModel(null)
                    {
                        Date = doc.Date,
                        Creator = GlobalOptions.UserInfo.NickName,
                        Id = Guid.NewGuid()
                    };
                    break;
            }

            return ret;
        }

        public InventorySheetViewModel CreateNew(decimal docCode,
            DocumentCopyType copyType = DocumentCopyType.Empty)
        {
            var ret = new InventorySheetViewModel(null)
            {
                Date = DateTime.Today,
                Creator = GlobalOptions.UserInfo.NickName,
                Id = Guid.NewGuid(),
                State = RowStatus.NewRow
            };
            return ret;
        }

        public InventorySheetViewModel CreateNew(Guid id, DocumentCopyType copyType = DocumentCopyType.Empty)
        {
            var ret = new InventorySheetViewModel(null)
            {
                Date = DateTime.Today,
                Creator = GlobalOptions.UserInfo.NickName,
                Id = Guid.NewGuid(),
                State = RowStatus.NewRow
            };
            return ret;
        }
    }
}
