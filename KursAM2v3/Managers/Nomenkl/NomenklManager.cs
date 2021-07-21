using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Calculates.Materials;
using Core;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Helper;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl.ViewModel;
using KursAM2.ViewModel.Logistiks;

namespace KursAM2.Managers.Nomenkl
{
    public class NomenklManager
    {
        public List<NomenklGroup> SelectNomenklGroups()
        {
            var ret = new List<NomenklGroup>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                ret.AddRange(ctx.SD_82.Select(g => new NomenklGroup
                {
                    DocCode = g.DOC_CODE,
                    Name = g.CAT_NAME,
                    ParentDC = g.CAT_PARENT_DC
                }));
            }

            return ret;
        }

        public static List<NomenklTransferRowViewModelExt> SelectNomenklsWithRemainsDialog(Warehouse warehouse)
        {
            var ctxTransf = new NomTransferAddForSklad(warehouse);
            var dlg = new SelectDialogView {DataContext = ctxTransf};
            dlg.ShowDialog();
            if (!ctxTransf.DialogResult) return null;
            var ret =
                ctxTransf.Rows.Where(_ => _.IsAccepted).Select(row => new NomenklTransferRowViewModelExt
                {
                    Id = Guid.NewGuid(),
                    //DocId = Document.Id,
                    NomenklOut = row.Nomenkl,
                    MaxQuantity = row.Quantity,
                    Quantity = row.Quantity,
                    State = RowStatus.NewRow,
                    PriceOut = row.Price,
                    SummaOut = row.Quantity * row.Price,
                    NakladEdSumma = row.PriceWithNaklad - row.Price,
                    NakladRate = 0

                    //Parent = Document
                }).ToList();
            return ret;
        }

        public static List<NomenklTransferRowViewModelExt> SelectNomenklsWithRemainsDialog(Warehouse warehouse,
            DateTime date)
        {
            var ctxTransf = new NomTransferAddForSklad(warehouse, date);
            var dlg = new SelectDialogView {DataContext = ctxTransf};
            dlg.ShowDialog();
            if (!ctxTransf.DialogResult) return null;
            var ret =
                ctxTransf.Rows.Where(_ => _.IsAccepted).Select(row => new NomenklTransferRowViewModelExt
                {
                    Id = Guid.NewGuid(),
                    //DocId = Document.Id,
                    NomenklOut = row.Nomenkl,
                    MaxQuantity = row.Quantity,
                    Quantity = row.Quantity,
                    State = RowStatus.NewRow,
                    PriceOut = row.Price,
                    SummaOut = row.Quantity * row.Price,
                    NakladEdSumma = row.PriceWithNaklad - row.Price,
                    NakladRate = 0

                    //Parent = Document
                }).ToList();
            return ret;
        }

        public static List<Core.EntityViewModel.NomenklManagement.Nomenkl> GetNomenklsSearch(string searchText,
            bool isDeleted = false)
        {
            var ret = new List<Core.EntityViewModel.NomenklManagement.Nomenkl>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var n in ctx.SD_83.Where(_ => (_.NOM_NAME.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_NOMENKL.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_FULL_NAME.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_NOTES.ToUpper().Contains(searchText.ToUpper())) &&
                                                       _.NOM_DELETED == (isDeleted ? 1 : 0)))
                    ret.Add(new Core.EntityViewModel.NomenklManagement.Nomenkl
                    {
                        DocCode = n.DOC_CODE,
                        Name = n.NOM_NAME,
                        NameFull = n.NOM_FULL_NAME,
                        Note = n.NOM_NOTES,
                        NomenklNumber = n.NOM_NOMENKL,
                        Group = MainReferences.NomenklGroups[n.NOM_CATEG_DC],
                        // ReSharper disable once PossibleInvalidOperationException
                        Currency = MainReferences.Currencies[n.NOM_SALE_CRS_DC.Value]
                    });
            }

            return ret;
        }

        /// <summary>
        ///     Текущий остаток товара на конкретном складе
        /// </summary>
        /// <param name="nomDC"></param>
        /// <param name="storeDC"></param>
        /// <returns></returns>
        public static decimal GetNomenklCount(decimal nomDC, decimal storeDC)
        {
            return NomenklCalculationManager.GetNomenklStoreRemain(DateTime.Today, nomDC, storeDC);
        }

        /// <summary>
        ///     Остаток товара на конкретном складе на дату
        /// </summary>
        /// <param name="date"></param>
        /// <param name="nomDC"></param>
        /// <param name="storeDC"></param>
        /// <returns></returns>
        public static decimal GetNomenklCount(DateTime date, decimal nomDC, decimal storeDC)
        {
            return NomenklCalculationManager.GetNomenklStoreRemain(date, nomDC, storeDC);
        }

        /// <summary>
        ///     Текущий остаток товара на всех складах
        /// </summary>
        /// <param name="nomDC"></param>
        /// <returns></returns>
        public static decimal GetNomenklCount(decimal nomDC)
        {
            return NomenklCalculationManager.GetNomenklStoreRemain(nomDC);
        }

        public static decimal GetNomenklCount(DateTime date, decimal nomDC)
        {
            return NomenklCalculationManager.GetNomenklStoreRemain(date, nomDC);
        }

        public static List<NomenklCalcMove> GetAllStoreRemain(DateTime date)
        {
            var sql = "DECLARE @tab TABLE (  NomDC NUMERIC(18, 0) " +
                      ",StoreDC  NUMERIC(18, 0) " +
                      ",Date DATETIME ,Prihod NUMERIC(18, 4) ,Rashod NUMERIC(18, 4) " +
                      ",OrderBy INT ,SumPrihod NUMERIC(18, 4) ,SumRashod NUMERIC(18, 4) );  " +
                      "INSERT INTO @tab  SELECT    NomDC, StoreDC, Date   ,Prihod   ,Rashod   " +
                      ",CASE      WHEN Prihod > 0 THEN 0      ELSE 1    END OrderBy   " +
                      ",SUM(Prihod) OVER (PARTITION BY StoreDC, NomDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumPrihod   " +
                      ",SUM(Rashod) OVER (PARTITION BY StoreDC, NomDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumRashod  " +
                      "FROM NomenklMoveForCalc  " +
                      //$"WHERE NomDC = {CustomFormat.DecimalToSqlDecimal(nomDC)}  " +
                      "ORDER BY StoreDC, NomDC, OrderBy;  " +
                      "SELECT  NomDC ,NOM_NOMENKL AS NomNumber ,NOM_NAME AS NomName ,t.Date " +
                      ",Prihod ,Rashod " +
                      ",SUM(Prihod - Rashod) OVER (PARTITION BY NomDC ORDER BY t.Date, OrderBy  " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING  AND CURRENT ROW) AS Ostatok " +
                      ",np.PRICE_WO_NAKLAD AS Price " +
                      ",np.PRICE AS PriceWithNaklad " +
                      ",StoreDC " +
                      ",s27.SKL_NAME as StoreName " +
                      "FROM @tab t " +
                      "INNER JOIN sd_83  ON NomDC = sd_83.DOC_CODE " +
                      "INNER JOIN sd_27 s27 ON StoreDC = s27.DOC_CODE " +
                      "INNER JOIN NOM_PRICE np ON np.NOM_DC = NomDC " +
                      "AND np.DATE = (SELECT MAX(np1.DATE) FROM NOM_PRICE np1 WHERE np1.NOM_DC = SD_83.DOC_CODE AND np1.Date <= t.Date) " +
                      $"WHERE t.Date <= '{CustomFormat.DateToString(date)}' " +
                      "ORDER BY NomDC, t.Date ;";
            var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklCalcMove>(sql).ToList();
            if (data.Count == 0)
                return new List<NomenklCalcMove>();
            var res = new List<NomenklCalcMove>();
            foreach (var storedc in data.Select(_ => _.StoreDC).Distinct())
            {
                var storedate = data.Where(_ => _.StoreDC == storedc).ToList();
                foreach (var nomdc in storedate.Select(_ => _.NomDC).Distinct())
                {
                    var nomLast = storedate.Last(_ => _.NomDC == nomdc);
                    if (nomLast.Ostatok == 0) continue;
                    res.Add(new NomenklCalcMove
                    {
                        Date = nomLast.Date,
                        NomDC = nomdc,
                        NomName = nomLast.NomName,
                        NomNomenkl = nomLast.NomNomenkl,
                        Ostatok = nomLast.Ostatok,
                        Price = nomLast.Price,
                        PriceWithNaklad = nomLast.PriceWithNaklad,
                        StoreDC = storedc,
                        StoreName = nomLast.StoreName
                    });
                }
            }

            return res;
        }

        public static List<NomenklCalcMove> GetNomenklMove(decimal nomDC, DateTime dateStart, DateTime dateEnd,
            out decimal startQuantity)
        {
            var sql = "DECLARE @tab TABLE (  NomDC NUMERIC(18, 0) " +
                      ",Date DATETIME ,Prihod NUMERIC(18, 4) ,Rashod NUMERIC(18, 4) " +
                      ",OrderBy INT ,SumPrihod NUMERIC(18, 4) ,SumRashod NUMERIC(18, 4) );  " +
                      "INSERT INTO @tab  SELECT    NomDC   ,Date   ,Prihod   ,Rashod   " +
                      ",CASE      WHEN Prihod > 0 THEN 0      ELSE 1    END OrderBy   " +
                      ",SUM(Prihod) OVER (PARTITION BY NomDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumPrihod   " +
                      ",SUM(Rashod) OVER (PARTITION BY NomDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumRashod  " +
                      "FROM NomenklMoveForCalc  " +
                      $"WHERE NomDC = {CustomFormat.DecimalToSqlDecimal(nomDC)}  " +
                      "ORDER BY NomDC, OrderBy;  " +
                      "SELECT  NomDC ,NOM_NOMENKL AS NomNumber ,NOM_NAME AS NomName ,Date " +
                      ",Prihod ,Rashod " +
                      ",SUM(Prihod - Rashod) OVER (PARTITION BY NomDC ORDER BY Date, OrderBy  " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING  AND CURRENT ROW) AS Ostatok " +
                      "FROM @tab " +
                      "INNER JOIN SD_83  ON NomDC = SD_83.DOC_CODE " +
                      $"WHERE Date <= '{CustomFormat.DateToString(dateEnd)}' " +
                      "ORDER BY Date;";
            using (var ctx = GlobalOptions.GetEntities())
            {
                startQuantity = 0;
                var data = ctx.Database.SqlQuery<NomenklCalcMove>(sql).ToList();
                if (data.Count == 0) return null;

                if (data.Any(_ => _.Date < dateStart))
                {
                    var d = data.Where(_ => _.Date < dateStart).Max(_ => _.Date);
                    var item = data.Last(_ => _.Date == d);
                    startQuantity = item.Ostatok;
                }

                return data.Where(_ => _.Date >= dateStart).ToList();
            }
        }

        public static List<NomenklCalcMove> GetNomenklMove(decimal nomDC, decimal storeDC, DateTime dateStart,
            DateTime dateEnd, out decimal startQuantity)
        {
            var sql = "DECLARE @tab TABLE (  NomDC NUMERIC(18, 0) " +
                      ",Date DATETIME ,Prihod NUMERIC(18, 4) ,Rashod NUMERIC(18, 4) " +
                      ",OrderBy INT ,SumPrihod NUMERIC(18, 4) ,SumRashod NUMERIC(18, 4) );  " +
                      "INSERT INTO @tab  SELECT    NomDC   ,Date   ,Prihod   ,Rashod   " +
                      ",CASE      WHEN Prihod > 0 THEN 0      ELSE 1    END OrderBy   " +
                      ",SUM(Prihod) OVER (PARTITION BY NomDC, StoreDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumPrihod   " +
                      ",SUM(Rashod) OVER (PARTITION BY NomDC, StoreDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumRashod  " +
                      "FROM NomenklMoveForCalc  " +
                      $"WHERE NomDC = {CustomFormat.DecimalToSqlDecimal(nomDC)}  " +
                      $"AND StoreDC = {CustomFormat.DecimalToSqlDecimal(storeDC)}" +
                      "ORDER BY NomDC,StoreDC,OrderBy;  " +
                      "SELECT  NomDC ,NOM_NOMENKL AS NomNumber ,NOM_NAME AS NomName ,t.Date " +
                      ",Prihod ,Rashod " +
                      ",SUM(Prihod - Rashod) OVER (PARTITION BY NomDC ORDER BY t.Date, OrderBy  " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING  AND CURRENT ROW) AS Ostatok " +
                      "FROM @tab t " +
                      "INNER JOIN sd_83  ON NomDC = SD_83.DOC_CODE " +
                      $"WHERE t.Date <= '{CustomFormat.DateToString(dateEnd)}' " +
                      "ORDER BY t.Date";
            using (var ctx = GlobalOptions.GetEntities())
            {
                startQuantity = 0;
                var data = ctx.Database.SqlQuery<NomenklCalcMove>(sql).ToList();
                if (data.Count == 0) return null;

                if (data.Any(_ => _.Date < dateStart))
                {
                    var d = data.Where(_ => _.Date < dateStart).Max(_ => _.Date);
                    var item = data.Last(_ => _.Date == d);
                    startQuantity = item.Ostatok;
                }

                return data.Where(_ => _.Date >= dateStart).ToList();
            }
        }


        public static List<NomenklCalcMove> GetNomenklMoveWithPrice(decimal nomDC, DateTime dateStart,
            DateTime dateEnd, out decimal startQuantity, out decimal startPrice, out decimal startPriceWithNaklad)
        {
            var sql = "DECLARE @tab TABLE (  NomDC NUMERIC(18, 0) " +
                      ",Date DATETIME ,Prihod NUMERIC(18, 4) ,Rashod NUMERIC(18, 4) " +
                      ",OrderBy INT ,SumPrihod NUMERIC(18, 4) ,SumRashod NUMERIC(18, 4) );  " +
                      "INSERT INTO @tab  SELECT    NomDC   ,Date   ,Prihod   ,Rashod   " +
                      ",CASE      WHEN Prihod > 0 THEN 0      ELSE 1    END OrderBy   " +
                      ",SUM(Prihod) OVER (PARTITION BY NomDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumPrihod   " +
                      ",SUM(Rashod) OVER (PARTITION BY NomDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumRashod  " +
                      "FROM NomenklMoveForCalc  " +
                      $"WHERE NomDC = {CustomFormat.DecimalToSqlDecimal(nomDC)}  " +
                      "ORDER BY NomDC, OrderBy;  " +
                      "SELECT  NomDC ,NOM_NOMENKL AS NomNumber ,NOM_NAME AS NomName ,t.Date " +
                      ",Prihod ,Rashod " +
                      ",SUM(Prihod - Rashod) OVER (PARTITION BY NomDC ORDER BY t.Date, OrderBy  " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING  AND CURRENT ROW) AS Ostatok " +
                      ",np.PRICE_WO_NAKLAD AS Price " +
                      ",np.PRICE AS PriceWithNaklad " +
                      "FROM @tab t " +
                      "INNER JOIN sd_83  ON NomDC = SD_83.DOC_CODE " +
                      "INNER JOIN NOM_PRICE np ON SD_83.DOC_CODE = np.NOM_DC " +
                      "AND np.DATE = (SELECT MAX(np1.DATE) FROM NOM_PRICE np1 WHERE np1.NOM_DC = SD_83.DOC_CODE AND np1.Date <= t.Date) " +
                      $"WHERE t.Date <= '{CustomFormat.DateToString(dateEnd)}'" +
                      "ORDER BY t.Date ";
            using (var ctx = GlobalOptions.GetEntities())
            {
                startQuantity = 0;
                startPrice = 0;
                startPriceWithNaklad = 0;
                var data = ctx.Database.SqlQuery<NomenklCalcMove>(sql).ToList();
                if (data.Count == 0) return null;

                if (data.Any(_ => _.Date < dateStart))
                {
                    var d = data.Where(_ => _.Date < dateStart).Max(_ => _.Date);
                    var item = data.Last(_ => _.Date == d);
                    startQuantity = item.Ostatok;
                    startPrice = item.Price;
                    startPriceWithNaklad = item.PriceWithNaklad;
                }

                return data.Where(_ => _.Date >= dateStart).ToList();
            }
        }

        public static List<NomenklCalcMove> GetNomenklMoveWithPrice(decimal nomDC, decimal storeDC, DateTime dateStart,
            DateTime dateEnd, out decimal startQuantity, out decimal startPrice, out decimal startPriceWithNaklad)
        {
            var sql = "DECLARE @tab TABLE (  NomDC NUMERIC(18, 0) " +
                      ",Date DATETIME ,Prihod NUMERIC(18, 4) ,Rashod NUMERIC(18, 4) " +
                      ",OrderBy INT ,SumPrihod NUMERIC(18, 4) ,SumRashod NUMERIC(18, 4) );  " +
                      "INSERT INTO @tab  SELECT    NomDC   ,Date   ,Prihod   ,Rashod   " +
                      ",CASE      WHEN Prihod > 0 THEN 0      ELSE 1    END OrderBy   " +
                      ",SUM(Prihod) OVER (PARTITION BY NomDC, StoreDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumPrihod   " +
                      ",SUM(Rashod) OVER (PARTITION BY NomDC, StoreDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumRashod  " +
                      "FROM NomenklMoveForCalc  " +
                      $"WHERE NomDC = {CustomFormat.DecimalToSqlDecimal(nomDC)}  " +
                      $"AND StoreDC = {CustomFormat.DecimalToSqlDecimal(storeDC)}" +
                      "ORDER BY NomDC, OrderBy;  " +
                      "SELECT  NomDC ,NOM_NOMENKL AS NomNumber ,NOM_NAME AS NomName ,t.Date " +
                      ",Prihod ,Rashod " +
                      ",SUM(Prihod - Rashod) OVER (PARTITION BY NomDC ORDER BY t.Date, OrderBy  " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING  AND CURRENT ROW) AS Ostatok " +
                      ",np.PRICE_WO_NAKLAD AS Price " +
                      ",np.PRICE AS PriceWithNaklad " +
                      "FROM @tab t " +
                      "INNER JOIN sd_83  ON NomDC = DOC_CODE " +
                      "INNER JOIN NOM_PRICE np ON SD_83.DOC_CODE = np.NOM_DC " +
                      "AND np.DATE = (SELECT MAX(np1.DATE) FROM NOM_PRICE np1 WHERE  np1.NOM_DC = SD_83.DOC_CODE AND np1.Date <= t.Date)" +
                      $"WHERE t.Date <= '{CustomFormat.DateToString(dateEnd)}' " +
                      "ORDER BY t.Date";
            using (var ctx = GlobalOptions.GetEntities())
            {
                startQuantity = 0;
                startPrice = 0;
                startPriceWithNaklad = 0;
                var data = ctx.Database.SqlQuery<NomenklCalcMove>(sql).ToList();
                if (data.Count == 0) return null;

                if (data.Any(_ => _.Date < dateStart))
                {
                    var d = data.Where(_ => _.Date < dateStart).Max(_ => _.Date);
                    var item = data.Last(_ => _.Date == d);
                    startQuantity = item.Ostatok;
                    startPrice = item.Price;
                    startPriceWithNaklad = item.PriceWithNaklad;
                }

                return data.Where(_ => _.Date >= dateStart).ToList();
            }
        }

        public static List<NomenklCalcMove> GetStoreMove(DateTime dateStart, DateTime dateEnd, decimal storeDC)
        {
            var sql = "DECLARE @tab TABLE (  NomDC NUMERIC(18, 0) " +
                      ",Date DATETIME ,Prihod NUMERIC(18, 4) ,Rashod NUMERIC(18, 4) " +
                      ",OrderBy INT ,SumPrihod NUMERIC(18, 4) ,SumRashod NUMERIC(18, 4) );  " +
                      "INSERT INTO @tab  SELECT    NomDC, Date   ,Prihod   ,Rashod   " +
                      ",CASE      WHEN Prihod > 0 THEN 0      ELSE 1    END OrderBy   " +
                      ",SUM(Prihod) OVER (PARTITION BY NomDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumPrihod   " +
                      ",SUM(Rashod) OVER (PARTITION BY NomDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumRashod  " +
                      "FROM NomenklMoveForCalc  " +
                      $"WHERE Date <= '{CustomFormat.DateToString(dateEnd)}' " +
                      $"AND StoreDC = {CustomFormat.DecimalToSqlDecimal(storeDC)} " +
                      "ORDER BY NomDC, OrderBy;  " +
                      "SELECT  NomDC, NOM_NOMENKL AS NomNumber ,NOM_NAME AS NomName , t.Date " +
                      ",Prihod ,Rashod " +
                      ",SUM(Prihod - Rashod) OVER (PARTITION BY NomDC ORDER BY t.Date, OrderBy  " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING  AND CURRENT ROW) AS Ostatok " +
                      "FROM @tab t " +
                      "INNER JOIN sd_83  ON NomDC = SD_83.DOC_CODE " +
                      $"WHERE t.Date <= '{CustomFormat.DateToString(dateEnd)}' " +
                      "ORDER BY NomDC, t.Date";
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.Database.SqlQuery<NomenklCalcMove>(sql).ToList();
                if (data.Count == 0) return null;

                var ndc = data.Select(_ => _.NomDC).Distinct().ToList();
                foreach (var dc in ndc)
                {
                    decimal startost = 0;
                    if (data.Any(_ => _.NomDC == dc && _.Date < dateStart))
                    {
                        var d = data.Where(_ => _.NomDC == dc && _.Date < dateStart).Max(_ => _.Date);
                        startost = data.Last(_ => _.NomDC == dc && _.Date == d).Ostatok;
                    }

                    foreach (var r in data.Where(_ => _.NomDC == dc && _.Date >= dateStart))
                    {
                        r.Start = startost;
                        startost = r.Ostatok;
                    }
                }

                return data.Where(_ => _.Date >= dateStart).ToList();
            }
        }

        public static List<NomenklCalcMove> GetStoreMoveWithPrice(DateTime dateStart, DateTime dateEnd, decimal storeDC)
        {
            var sql = "DECLARE @tab TABLE (  NomDC NUMERIC(18, 0) " +
                      ",Date DATETIME ,Prihod NUMERIC(18, 4) ,Rashod NUMERIC(18, 4) " +
                      ",OrderBy INT ,SumPrihod NUMERIC(18, 4) ,SumRashod NUMERIC(18, 4) );  " +
                      "INSERT INTO @tab  SELECT    NomDC, Date   ,Prihod   ,Rashod   " +
                      ",CASE      WHEN Prihod > 0 THEN 0      ELSE 1    END OrderBy   " +
                      ",SUM(Prihod) OVER (PARTITION BY NomDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumPrihod   " +
                      ",SUM(Rashod) OVER (PARTITION BY NomDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumRashod  " +
                      "FROM NomenklMoveForCalc  " +
                      $"WHERE Date <= '{CustomFormat.DateToString(dateEnd)}' " +
                      $"AND StoreDC = {CustomFormat.DecimalToSqlDecimal(storeDC)} " +
                      "ORDER BY NomDC, OrderBy;  " +
                      "SELECT  NomDC, NOM_NOMENKL AS NomNumber ,NOM_NAME AS NomName , t.Date " +
                      ",Prihod ,Rashod " +
                      ",SUM(Prihod - Rashod) OVER (PARTITION BY NomDC ORDER BY t.Date, OrderBy  " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING  AND CURRENT ROW) AS Ostatok " +
                      ",np.PRICE_WO_NAKLAD AS Price " +
                      ",np.PRICE AS PriceWithNaklad " +
                      "FROM @tab t " +
                      "INNER JOIN sd_83  ON NomDC = SD_83.DOC_CODE " +
                      "INNER JOIN NOM_PRICE np ON SD_83.DOC_CODE = np.NOM_DC " +
                      "AND np.DATE = (SELECT MAX(np1.DATE) FROM NOM_PRICE np1 WHERE np1.NOM_DC = SD_83.DOC_CODE AND np1.Date <= t.Date)" +
                      $"WHERE t.Date <= '{CustomFormat.DateToString(dateEnd)}' " +
                      "ORDER BY NomDC, t.Date";
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.Database.SqlQuery<NomenklCalcMove>(sql).ToList();
                if (data.Count == 0) return null;

                var ndc = data.Select(_ => _.NomDC).Distinct().ToList();
                foreach (var dc in ndc)
                {
                    decimal startost = 0;
                    if (data.Any(_ => _.NomDC == dc && _.Date < dateStart))
                    {
                        var d = data.Where(_ => _.NomDC == dc && _.Date < dateStart).Max(_ => _.Date);
                        startost = data.Last(_ => _.NomDC == dc && _.Date == d).Ostatok;
                    }

                    foreach (var r in data.Where(_ => _.NomDC == dc && _.Date >= dateStart))
                    {
                        r.Start = startost;
                        startost = r.Ostatok;
                    }
                }

                return data.Where(_ => _.Date >= dateStart).ToList();
            }
        }

        public static NomenklGroup CategoryAdd(NameNoteViewModel cat, decimal? parentDC)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = new TransactionScope())
                {
                    try
                    {
                        var newDC = ctx.SD_82.Any() ? ctx.SD_82.Max(_ => _.DOC_CODE) + 1 : 10820000001;
                        var newCat = new SD_82
                        {
                            DOC_CODE = newDC,
                            CAT_NAME = cat.Name,
                            CAT_PATH_NAME = cat.Name,
                            CAT_PARENT_DC = parentDC
                        };
                        ctx.SD_82.Add(newCat);
                        ctx.SaveChanges();
                        tnx.Complete();
                        if (!MainReferences.NomenklGroups.ContainsKey(newDC))
                            MainReferences.NomenklGroups.Add(newDC, new NomenklGroup
                            {
                                DocCode = newDC,
                                Name = newCat.CAT_NAME,
                                ParentDC = newCat.CAT_PARENT_DC
                            });

                        // ReSharper disable once InconsistentNaming
                        var newCatVM = new NomenklGroup(newCat)
                        {
                            State = RowStatus.NotEdited
                        };
                        return newCatVM;
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                        return null;
                    }
                }
            }
        }

        public static void RecalcPrice()
        {
            var sql = "DECLARE @NomDC NUMERIC(15, 0); " +
                      " DECLARE NomenklList CURSOR FOR " +
                      " SELECT DISTINCT NOM_DC " +
                      " FROM NOMENKL_RECALC; " +
                      " OPEN NomenklList " +
                      " FETCH NEXT FROM NomenklList INTO @NomDC " +
                      " WHILE @@fetch_status = 0 " +
                      " BEGIN " +
                      " EXEC dbo.NomenklCalculateCostsForOne @NomDC " +
                      " FETCH NEXT FROM NomenklList INTO @NomDC " +
                      " END" +
                      " CLOSE NomenklList; " +
                      " DEALLOCATE NomenklList; " +
                      " DELETE FROM WD_27 " +
                      " INSERT INTO dbo.WD_27 (DOC_CODE, SKLW_NOMENKL_DC, SKLW_DATE, SKLW_KOLICH) " +
                      " SELECT n.StoreDC, n.nomdc, n.DATE, SUM(n.prihod) - SUM(n.rashod) Quantity " +
                      " FROM NomenklMoveWithPrice n " +
                      " WHERE n.DATE = (SELECT MAX(n1.DATE) " +
                      " FROM NomenklMoveWithPrice n1 " +
                      " WHERE n1.nomdc = n.nomdc AND n.storedc = n1.storedc) " +
                      " GROUP BY n.nomdc, n.StoreDc, n.DATE " +
                      " HAVING SUM(n.prihod) - SUM(n.rashod) > 0; " +
                      " DELETE FROM NOMENKL_RECALC;";
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ctx.Database.ExecuteSqlCommand(sql);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        if (transaction.UnderlyingTransaction.Connection != null)
                            transaction.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public static void RecalcPrice(List<decimal> nomdcs, ALFAMEDIAEntities ent = null)
        {
            if (ent != null)
                try
                {
                    foreach (var n in nomdcs)
                        ent.Database.ExecuteSqlCommand("INSERT INTO NOMENKL_RECALC (NOM_DC, OPER_DATE) " +
                                                       $"VALUES ({CustomFormat.DecimalToSqlDecimal(n)}, '20000101');");
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }
            else
                using (var ctx = GlobalOptions.GetEntities())
                {
                    using (var transaction = ctx.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var n in nomdcs)
                                ctx.Database.ExecuteSqlCommand("INSERT INTO NOMENKL_RECALC (NOM_DC, OPER_DATE) " +
                                                               $"VALUES ({CustomFormat.DecimalToSqlDecimal(n)}, '20000101');");

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            if (transaction.UnderlyingTransaction.Connection != null)
                                transaction.Rollback();
                            WindowManager.ShowError(ex);
                        }
                    }

                    RecalcPrice();
                }
        }

        public static Prices NomenklPrice(decimal nomdc, DateTime date, ALFAMEDIAEntities ent = null)
        {
            var sql = "SELECT np.PRICE_WO_NAKLAD as Price, np.PRICE as PriceWithNaklad FROM nom_price np " +
                      $"WHERE np.nom_dc = {CustomFormat.DecimalToSqlDecimal(nomdc)} " +
                      "AND Date = (select MAX(np1.Date) " +
                      $"FROM NOM_PRICE np1 WHERE np1.NOM_DC = np.NOM_DC AND np1.DATE <= '{CustomFormat.DateToString(date)}')";
            if (ent == null)
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.Database.SqlQuery<Prices>(sql).ToList();
                    if (data.Count == 0) return null;
                    return data.First();
                }

            var data1 = ent.Database.SqlQuery<Prices>(sql).ToList();
            if (data1.Count == 0) return null;
            return data1.First();
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    
}