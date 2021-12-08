using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Core;
using Core.EntityViewModel.NomenklManagement;
using Data;
using Helper;
using JetBrains.Annotations;

namespace Calculates.Materials
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class NomenklCalcRemain
    {
        public decimal Remain { set; get; }
        public decimal NomDC { set; get; }
        public decimal StoreDC { set; get; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class NomenklCalcWithMoveRemain
    {
        public decimal NomDC { set; get; }
        public decimal StoreDC { set; get; }

        public decimal Start { set; get; }
        public decimal In { set; get; }
        public decimal Out { set; get; }

        public decimal End { set; get; }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class NomenklCalcFull : NomenklCalcWithMoveRemain
    {
        public decimal PriceStart { set; get; }
        public decimal PriceStartWithNaklad { set; get; }
        public decimal SumIn { set; get; }
        public decimal SumOut { set; get; }
        public decimal SumInWithNaklad { set; get; }
        public decimal SumOutWithNaklad { set; get; }
        public decimal PriceEnd { set; get; }
        public decimal PriceEndWithNaklad { set; get; }
    }

    public static class NomenklCalculationManager
    {
        public static List<NOM_PRICE> GetCalculateOperations(decimal nomDC)
        {
            return
                new List<NOM_PRICE>(
                    GlobalOptions.GetEntities().NOM_PRICE.Where(_ => _.NOM_DC == nomDC).OrderBy(_ => _.DATE));
        }

        public static ObservableCollection<NomenklMoveOnSkladViewModel> NomenklMoveList { set; get; } =
            new ObservableCollection<NomenklMoveOnSkladViewModel>();

        /// <summary>
        ///     Получение остатка товара на складе
        /// </summary>
        /// <returns></returns>
        public static decimal GetNomenklStoreRemain(DateTime date, decimal nomDC, decimal storeDC)
        {
            var sql = "DECLARE @tab TABLE (  NomDC NUMERIC(18, 0) " +
                      ",StoreDC NUMERIC(18, 0) " +
                      ",Date DATETIME ,Prihod NUMERIC(18, 4) ,Rashod NUMERIC(18, 4) " +
                      ",OrderBy INT ,SumPrihod NUMERIC(18, 4) ,SumRashod NUMERIC(18, 4) );  " +
                      "INSERT INTO @tab  SELECT    NomDC   ,StoreDC   ,Date   ,Prihod   ,Rashod   " +
                      ",CASE      WHEN Prihod > 0 THEN 0      ELSE 1    END OrderBy   " +
                      ",SUM(Prihod) OVER (PARTITION BY NomDC, StoreDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumPrihod   " +
                      ",SUM(Rashod) OVER (PARTITION BY NomDC, StoreDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumRashod  " +
                      "FROM NomenklMoveForCalc  " +
                      $"WHERE NomDC = {CustomFormat.DecimalToSqlDecimal(nomDC)} " +
                      $"AND StoreDC = {CustomFormat.DecimalToSqlDecimal(storeDC)}  " +
                      "ORDER BY NomDC, OrderBy;  " +
                      "SELECT  NomDC ,NOM_NOMENKL AS NomNumber ,NOM_NAME AS NomName ,StoreDC ,SD_27.SKL_NAME as StoreName ,Date " +
                      ",Prihod ,Rashod " +
                      ",SUM(Prihod - Rashod) OVER (PARTITION BY NomDC,StoreDC  ORDER BY Date, OrderBy  " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING  AND CURRENT ROW) AS Ostatok " +
                      "FROM @tab " +
                      "INNER JOIN SD_83  ON NomDC = SD_83.DOC_CODE " +
                      "INNER JOIN SD_27  ON SD_27.DOC_CODE = StoreDC " +
                      $"WHERE Date <= '{CustomFormat.DateToString(date)}' " +
                      "ORDER BY Date;";
            var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklCalcMove>(sql).ToList();
            if (data.Count == 0)
                return 0;
            return data.Last().Ostatok;
        }

        public static decimal GetNomenklStoreRemain(ALFAMEDIAEntities ctx, DateTime date, decimal nomDC,
            decimal storeDC)
        {
            var sql = "DECLARE @tab TABLE (  NomDC NUMERIC(18, 0) " +
                      ",StoreDC NUMERIC(18, 0) " +
                      ",Date DATETIME ,Prihod NUMERIC(18, 4) ,Rashod NUMERIC(18, 4) " +
                      ",OrderBy INT ,SumPrihod NUMERIC(18, 4) ,SumRashod NUMERIC(18, 4) );  " +
                      "INSERT INTO @tab  SELECT    NomDC   ,StoreDC   ,Date   ,Prihod   ,Rashod   " +
                      ",CASE      WHEN Prihod > 0 THEN 0      ELSE 1    END OrderBy   " +
                      ",SUM(Prihod) OVER (PARTITION BY NomDC, StoreDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumPrihod   " +
                      ",SUM(Rashod) OVER (PARTITION BY NomDC, StoreDC    ORDER BY Date    " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING    AND CURRENT ROW) AS SumRashod  " +
                      "FROM NomenklMoveForCalc  " +
                      $"WHERE NomDC = {CustomFormat.DecimalToSqlDecimal(nomDC)} " +
                      $"AND StoreDC = {CustomFormat.DecimalToSqlDecimal(storeDC)}  " +
                      "ORDER BY NomDC, OrderBy;  " +
                      "SELECT  NomDC ,NOM_NOMENKL AS NomNumber ,NOM_NAME AS NomName ,StoreDC ,SD_27.SKL_NAME as StoreName ,Date " +
                      ",Prihod ,Rashod " +
                      ",SUM(Prihod - Rashod) OVER (PARTITION BY NomDC,StoreDC  ORDER BY Date, OrderBy  " +
                      "ROWS BETWEEN UNBOUNDED PRECEDING  AND CURRENT ROW) AS Ostatok " +
                      "FROM @tab INNER JOIN sd_83  ON NomDC = DOC_CODE " +
                      "INNER JOIN SD_27  ON SD_27.DOC_CODE = StoreDC " +
                      $"WHERE Date <= '{CustomFormat.DateToString(date)}' " +
                      "ORDER BY NomDC, StoreDC;";
            var data = ctx.Database.SqlQuery<NomenklCalcMove>(sql).ToList();
            if (data.Count == 0)
            {
                return 0;
            }

            return data.Last().Ostatok;
        }

        public static decimal GetNomenklStoreRemain(DateTime date, decimal nomDC)
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
                      "INNER JOIN sd_83  ON NomDC = DOC_CODE " +
                      $"WHERE Date <= '{CustomFormat.DateToString(date)}' " +
                      "ORDER BY Date ;";
            var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklCalcMove>(sql).ToList();
            if (data.Count == 0)
                return 0;
            return data.Last().Ostatok;
        }

        public static decimal GetNomenklStoreRemain(decimal nomDC)
        {
            return GetNomenklStoreRemain(DateTime.Today, nomDC);
        }

        /// <summary>
        ///     Возвращает остатки по товару на складах в форме
        ///     кортежа (склад, остатки)
        /// </summary>
        /// <param name="date"></param>
        /// <param name="nomDC"></param>
        /// <returns></returns>
        public static List<Tuple<decimal, decimal>> NomenklRemain(DateTime date, decimal nomDC)
        {
            var ret = new List<Tuple<decimal, decimal>>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql = "SELECT cast(0 as numeric(18,0)) as NomDC, StoreDC, Sum(Prihod - Rashod) Remain " +
                          "FROM dbo.NomenklMoveForCalc " +
                          $"WHERE Date <= '{CustomFormat.DateToString(date)}' " +
                          $"AND NomDC = {CustomFormat.DecimalToSqlDecimal(nomDC)} " +
                          "GROUP BY NomDC,StoreDC " +
                          "HAVING Sum(Prihod - Rashod) != 0 " +
                          "ORDER BY 1,2,3";
                foreach (var d in ctx.Database.SqlQuery<NomenklCalcRemain>(sql).ToList())
                    ret.Add(Tuple.Create(d.StoreDC, d.Remain));

                return ret;
            }
        }

        /// <summary>
        ///     Возвращает остатки на начало и конец по товару на складах, а также
        ///     движение (приход/расход) в форме
        ///     кортежа (склад, начальные остатки, приход, расход, конечные остатки)
        /// </summary>
        /// <param name="end"></param>
        /// <param name="nomDC"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static List<Tuple<decimal, decimal, decimal, decimal, decimal>> NomenklRemain(DateTime start,
            DateTime end, decimal nomDC)
        {
            var ret = new List<Tuple<decimal, decimal, decimal, decimal, decimal>>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql =
                    "SELECT NomDC, NomCrsDC ,StoreDC, SUM(Start) AS Start, SUM([In]) AS [In], " +
                    "SUM([Out]) AS [Out], SUM([End]) AS [End] " +
                    "FROM (SELECT  NomDC ,NomCrsDC ,StoreDC, " +
                    "SUM(Prihod - Rashod) Start ," +
                    "CAST(0 AS NUMERIC(18, 8)) AS [In] ,CAST(0 AS NUMERIC(18, 8)) AS [Out] ," +
                    "CAST(0 AS NUMERIC(18, 8)) AS [End] " +
                    "FROM dbo.NomenklMoveForCalc " +
                    $"WHERE Date <= '{CustomFormat.DateToString(start)}' " +
                    "GROUP BY NomDC, NomCrsDC, StoreDC " +
                    "UNION ALL " +
                    "SELECT  NomDC ,NomCrsDC ,StoreDC ," +
                    "0 Start ,SUM(prihod) AS [In] ,SUM(rashod) AS [Out] ,0 AS [End] " +
                    "FROM dbo.NomenklMoveForCalc " +
                    $"WHERE Date >= '{CustomFormat.DateToString(start)}' " +
                    $"AND Date <= '{CustomFormat.DateToString(end)}' " +
                    "GROUP BY NomDC,NomCrsDC,StoreDC " +
                    "UNION ALL " +
                    "SELECT NomDC ,NomCrsDC ,StoreDC ,0 Start ,0 AS [In] ,0 AS [Out] ," +
                    "SUM(Prihod - Rashod) AS [End] " +
                    "FROM dbo.NomenklMoveForCalc " +
                    $"WHERE Date <= '{CustomFormat.DateToString(end)}' " +
                    "GROUP BY NomDC, NomCrsDC, StoreDC  ) T " +
                    $"WHERE NomDC = {CustomFormat.DecimalToSqlDecimal(nomDC)} " +
                    "GROUP BY NomDC,NomCrsDC,StoreDC " +
                    "HAVING  SUM(Start) != 0 OR SUM([In]) != 0 OR SUM([Out]) != 0 OR SUM([End]) != 0";
                foreach (var d in ctx.Database.SqlQuery<NomenklCalcWithMoveRemain>(sql).ToList())
                    ret.Add(Tuple.Create(d.StoreDC, d.Start, d.In, d.Out, d.End));

                return ret;
            }
        }

        public static decimal NomenklRemain(DateTime date, decimal nomDC, decimal storeDC)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.NomenklMoveForCalc.Where(_ => _.Date <= date && _.NomDC == nomDC
                                                                            && _.StoreDC == storeDC).ToList();
                return data.Count == 0 ? 0 : data.Sum(_ => _.Prihod - _.Rashod);
            }
        }

        public static decimal NomenklRemain(ALFAMEDIAEntities ctx, DateTime date, decimal nomDC, decimal storeDC)
        {

            var data = ctx.NomenklMoveForCalc.Where(_ => _.Date <= date && _.NomDC == nomDC
                                                                        && _.StoreDC == storeDC).ToList();
            return data.Count == 0 ? 0 : data.Sum(_ => _.Prihod - _.Rashod);
        }

        /// <summary>
        ///     Возвращает список остатков товаров на складах вформе
        ///     кортежа (номенклатура, склад, остаток)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static List<Tuple<decimal, decimal, decimal>> NomenklRemains(DateTime date)
        {
            var ret = new List<Tuple<decimal, decimal, decimal>>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql = "SELECT NomDC, StoreDC, Sum(Prihod - Rashod) Remain " +
                          "FROM dbo.NomenklMoveForCalc " +
                          $"WHERE Date <= '{CustomFormat.DateToString(date)}'" +
                          "GROUP BY NomDC, StoreDC " +
                          "HAVING Sum(Prihod - Rashod) != 0 " +
                          "ORDER BY 1,2,3";
                foreach (var d in ctx.Database.SqlQuery<NomenklCalcRemain>(sql).ToList())
                    ret.Add(Tuple.Create(d.NomDC, d.StoreDC, d.Remain));

                return ret;
            }
        }

        /// <summary>
        ///     Возвращает список остатков товаров на складах cдвижением (приход/расход) в форме
        ///     кортежа (номенклатура,  склад, начальные остатки, приход, расход, конечные остатки)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<Tuple<decimal, decimal, decimal, decimal, decimal, decimal>> NomenklRemains(DateTime start,
            DateTime end)
        {
            var ret = new List<Tuple<decimal, decimal, decimal, decimal, decimal, decimal>>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql =
                    "SELECT NomDC, NomCrsDC ,StoreDC, SUM(Start) AS Start, SUM([In]) AS [In], " +
                    "SUM([Out]) AS [Out], SUM([End]) AS [End] " +
                    "FROM (SELECT  NomDC ,NomCrsDC ,StoreDC, " +
                    "SUM(Prihod - Rashod) Start ," +
                    "CAST(0 AS NUMERIC(18, 8)) AS [In] ,CAST(0 AS NUMERIC(18, 8)) AS [Out] ," +
                    "CAST(0 AS NUMERIC(18, 8)) AS [End] " +
                    "FROM dbo.NomenklMoveForCalc " +
                    $"WHERE Date <= '{CustomFormat.DateToString(start)}' " +
                    "GROUP BY NomDC, NomCrsDC, StoreDC " +
                    "UNION ALL " +
                    "SELECT  NomDC ,NomCrsDC ,StoreDC ," +
                    "0 Start ,SUM(prihod) AS [In] ,SUM(rashod) AS [Out] ,0 AS [End] " +
                    "FROM dbo.NomenklMoveForCalc " +
                    $"WHERE Date >= '{CustomFormat.DateToString(start)}' " +
                    $"AND Date <= '{CustomFormat.DateToString(end)}' " +
                    "GROUP BY NomDC,NomCrsDC,StoreDC " +
                    "UNION ALL " +
                    "SELECT NomDC ,NomCrsDC ,StoreDC ,0 Start ,0 AS [In] ,0 AS [Out] ," +
                    "SUM(Prihod - Rashod) AS [End] " +
                    "FROM dbo.NomenklMoveForCalc " +
                    $"WHERE Date <= '{CustomFormat.DateToString(end)}' " +
                    "GROUP BY NomDC, NomCrsDC, StoreDC  ) T " +
                    "GROUP BY NomDC,NomCrsDC,StoreDC " +
                    "HAVING  SUM(Start) != 0 OR SUM([In]) != 0 OR SUM([Out]) != 0 OR SUM([End]) != 0";
                foreach (var d in ctx.Database.SqlQuery<NomenklCalcWithMoveRemain>(sql).ToList())
                    ret.Add(Tuple.Create(d.NomDC, d.StoreDC, d.Start, d.In, d.Out, d.End));

                return ret;
            }
        }

        /// <summary>
        ///     Возвращает список остатков товаров на складе в форме
        ///     кортежа (номенклатура,  остаток)
        /// </summary>
        /// <param name="date"></param>
        /// <param name="storeDC"></param>
        /// <returns></returns>
        public static List<Tuple<decimal, decimal>> NomenklRemains(DateTime date, decimal storeDC)
        {
            var ret = new List<Tuple<decimal, decimal>>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql = "SELECT NomDC, cast(0 as numeric(18,0)) StoreDC, Sum(Prihod - Rashod) Remain " +
                          "FROM dbo.NomenklMoveForCalc " +
                          $"WHERE Date <= '{CustomFormat.DateToString(date)}' " +
                          $"AND StoreDC = {CustomFormat.DecimalToSqlDecimal(storeDC)} " +
                          "GROUP BY NomDC, StoreDC " +
                          "HAVING Sum(Prihod - Rashod) != 0 " +
                          "ORDER BY 1,2,3";
                foreach (var d in ctx.Database.SqlQuery<NomenklCalcRemain>(sql).ToList())
                    ret.Add(Tuple.Create(d.NomDC, d.Remain));
            }

            return ret;
        }

        /// <summary>
        ///     Возвращает остатки на начало и конец по товару на складах, а также
        ///     движение (приход/расход) в форме
        ///     кортежа (номенклатура, начальные остатки, приход, расход, конечные остатки)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="storeDC"></param>
        /// <returns></returns>
        public static List<Tuple<decimal, decimal, decimal, decimal, decimal>> NomenklRemains(DateTime start,
            DateTime end, decimal storeDC)
        {
            var ret = new List<Tuple<decimal, decimal, decimal, decimal, decimal>>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql =
                    "SELECT NomDC, NomCrsDC ,StoreDC, SUM(Start) AS Start, SUM([In]) AS [In], " +
                    "SUM([Out]) AS [Out], SUM([End]) AS [End] " +
                    "FROM (SELECT  NomDC ,NomCrsDC ,StoreDC, " +
                    "SUM(Prihod - Rashod) Start ," +
                    "CAST(0 AS NUMERIC(18, 8)) AS [In] ,CAST(0 AS NUMERIC(18, 8)) AS [Out] ," +
                    "CAST(0 AS NUMERIC(18, 8)) AS [End] " +
                    "FROM dbo.NomenklMoveForCalc " +
                    $"WHERE Date <= '{CustomFormat.DateToString(start)}' " +
                    "GROUP BY NomDC, NomCrsDC, StoreDC " +
                    "UNION ALL " +
                    "SELECT  NomDC ,NomCrsDC ,StoreDC ," +
                    "0 Start ,SUM(prihod) AS [In] ,SUM(rashod) AS [Out] ,0 AS [End] " +
                    "FROM dbo.NomenklMoveForCalc " +
                    $"WHERE Date >= '{CustomFormat.DateToString(start)}' " +
                    $"AND Date <= '{CustomFormat.DateToString(end)}' " +
                    "GROUP BY NomDC,NomCrsDC,StoreDC " +
                    "UNION ALL " +
                    "SELECT NomDC ,NomCrsDC ,StoreDC ,0 Start ,0 AS [In] ,0 AS [Out] ," +
                    "SUM(Prihod - Rashod) AS [End] " +
                    "FROM dbo.NomenklMoveForCalc " +
                    $"WHERE Date <= '{CustomFormat.DateToString(end)}' " +
                    "GROUP BY NomDC, NomCrsDC, StoreDC  ) T " +
                    $"WHERE StoreDC = {CustomFormat.DecimalToSqlDecimal(storeDC)} " +
                    "GROUP BY NomDC,NomCrsDC,StoreDC " +
                    "HAVING  SUM(Start) != 0 OR SUM([In]) != 0 OR SUM([Out]) != 0 OR SUM([End]) != 0";
                foreach (var d in ctx.Database.SqlQuery<NomenklCalcWithMoveRemain>(sql).ToList())
                    ret.Add(Tuple.Create(d.NomDC, d.Start, d.In, d.Out, d.End));
            }

            return ret;
        }

        /// <summary>
        ///     Возвращает среднескользящие цены товара на дату по которой были операции
        ///     и в виде кортежа цена без накладных, цена с накладными>
        /// </summary>
        /// <param name="date"></param>
        /// <param name="nomDC"></param>
        /// <returns></returns>
        public static Tuple<decimal, decimal> NomenklPrice(DateTime date, decimal nomDC)
        {
            // Учет возвратов ведется по сумме последней поставки на момент возврата товара

            Tuple<decimal, decimal> ret;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.NomenklMoveForCalc.Where(_ => _.Date <= date && _.NomDC == nomDC)
                    .OrderBy(_ => _.Date).ToList();
                if (data.Count == 0) return null;
                decimal price = 0, priceWithNaklad = 0, nakopit = 0;
                var dates = data.Select(_ => _.Date).Distinct().OrderBy(_ => _)
                    .ToList();
                foreach (var d in dates)
                {
                    var quanOut = data.Where(_ => _.Date == d && _.OperType == "Rashod").Sum(_ => _.Rashod);
                    var quanIn = data.Where(_ => _.Date == d && _.OperType == "Prihod")
                        .Sum(_ => _.Prihod);
                    var sumIn = data.Where(_ => _.Date == d && _.OperType == "Prihod").Sum(_ => _.Prihod * _.Price) ??
                                0;
                    var sumInWithNaklad = data.Where(_ => _.Date == d && _.OperType == "Prihod")
                        .Sum(_ => _.Prihod * (_.PriceWithNaklad ?? 0));
                    var quanVozvrat = data.Where(_ => _.Date == d && _.OperType == "Vozvrat").Sum(_ => _.Prihod);

                    var oldDate = d;
                    var priceOld = price;
                    if (quanVozvrat > 0)
                    {
                        var oldDateItems = data.Where(_ => _.Date <= d && _.Prihod > 0).ToList();
                        if (oldDateItems.Any())
                            oldDate = oldDateItems.Max(_ => _.Date);
                        var priceOldItems = data.Where(_ => _.Date == oldDate).ToList();

                        if (priceOldItems.Sum(_ => _.Price ?? 0) > 0)
                            priceOld = priceOldItems.Sum(_ => _.Prihod) / priceOldItems.Sum(_ => _.Price ?? 0);
                        else
                            priceOld = price;
                    }

                    if (nakopit <= 0) price = priceWithNaklad = 0;
                    if (nakopit + quanIn + quanVozvrat != 0)
                    {
                        price = (nakopit * price + sumIn + quanVozvrat * priceOld) /
                                (nakopit + quanIn + quanVozvrat);
                        priceWithNaklad = (nakopit * priceWithNaklad + sumInWithNaklad + quanVozvrat * priceOld) /
                                          (nakopit + quanIn + quanVozvrat);
                    }

                    nakopit += quanIn - quanOut + quanVozvrat;
                }

                ret = Tuple.Create(price, priceWithNaklad);
            }

            return ret;
        }

        /// <summary>
        ///     Возвращает обороты по приъоду/расходу пономенклатуре за период
        ///     в форме кортежа (дата, сумма прихода без накл, сумма прихода с накл, сумма расхда без накл.,
        ///     сумма расхода с накладн)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="nomDC"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, decimal, decimal, decimal, decimal>> NomenklMoveSum(DateTime start,
            DateTime end, decimal nomDC)
        {
            var ret = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.NomenklMoveForCalc.Where(_ => _.Date <= end && _.NomDC == nomDC)
                    .OrderBy(_ => _.Date).ToList();
                if (data.Count == 0) return null;
                decimal price = 0, priceWithNaklad = 0, nakopit = 0;
                var dates = data.Select(_ => _.Date).Distinct().OrderBy(_ => _)
                    .ToList();
                foreach (var d in dates)
                {
                    var quanOut = data.Where(_ => _.Date == d && _.OperType == "Rashod").Sum(_ => _.Rashod);
                    var quanIn = data.Where(_ => _.Date == d && (_.OperType == "Prihod" || _.OperType == "Vozvrat"))
                        .Sum(_ => _.Prihod);
                    var sumIn = data.Where(_ => _.Date == d && _.OperType == "Prihod").Sum(_ => _.Prihod * _.Price) ??
                                0;
                    var sumInWithNaklad = data.Where(_ => _.Date == d && _.OperType == "Prihod")
                        .Sum(_ => _.Prihod * _.PriceWithNaklad) ?? 0;

                    var sumIn2 = data.Where(_ => _.Date == d && _.OperType == "Vozvrat").Sum(_ => _.Prihod * price);
                    var sumInWithNaklad2 = data.Where(_ => _.Date == d && _.OperType == "Vozvrat")
                        .Sum(_ => _.Prihod * priceWithNaklad);
                    if (nakopit + quanIn != 0)
                    {
                        price = (nakopit * price + sumIn + sumIn2) / (nakopit + quanIn);
                        priceWithNaklad = (nakopit * priceWithNaklad + sumInWithNaklad + sumInWithNaklad2) /
                                          (nakopit + quanIn);
                    }

                    nakopit += quanIn - quanOut;
                    if (d >= start && d <= end)
                        ret.Add(Tuple.Create(d, sumIn + sumIn2, sumInWithNaklad + sumInWithNaklad2,
                            quanOut * price, quanOut * priceWithNaklad));
                }
            }

            return ret;
        }

        /// <summary>
        ///     Возвращает обороты по приоду/расходу пономенклатуре за период по складу
        ///     в форме кортежа (дата, сумма прихода без накл, сумма прихода с накл, сумма расхода без накл.,
        ///     сумма расхода с накладн, цена начало, цена с накл начало, кол-во начала, цена конец,
        ///     цена конец с накл,  кол-во конец)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="nomDC"></param>
        /// <param name="storeDC"></param>
        /// <returns></returns>
        public static List<Tuple<DateTime, decimal, decimal, decimal, decimal>> NomenklMoveSum(DateTime start,
            DateTime end, decimal nomDC, decimal storeDC)
        {
            var ret = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.NomenklMoveForCalc.Where(_ => _.Date <= end && _.NomDC == nomDC)
                    .OrderBy(_ => _.Date).ToList();
                if (data.Count == 0) return null;
                decimal price = 0, priceWithNaklad = 0, nakopit = 0;
                var dates = data.Select(_ => _.Date).Distinct().OrderBy(_ => _)
                    .ToList();

                foreach (var d in dates)
                {
                    var quanOut = data.Where(_ => _.Date == d && _.OperType == "Rashod").Sum(_ => _.Rashod);
                    var sQuanOut = data.Where(_ => _.Date == d && _.StoreDC == storeDC
                                                               && (_.OperType == "Rashod" || _.OperType == "InnerMove"))
                        .Sum(_ => _.Rashod);
                    var quanIn = data.Where(_ => _.Date == d && _.OperType == "Prihod").Sum(_ => _.Prihod);
                    var quanVozvrat = data.Where(_ => _.Date == d && _.OperType == "Vozvrat").Sum(_ => _.Prihod);
                    var sQuanIn = data.Where(_ => _.Date == d && _.StoreDC == storeDC
                                                              && (_.OperType == "Prihod" || _.OperType == "Vozvrat"
                                                                  || _.OperType == "InnerMove"))
                        .Sum(_ => _.Prihod);

                    var sumIn = data.Where(_ => _.Date == d && _.OperType == "Prihod").Sum(_ => _.Prihod * _.Price) ??
                                0;
                    var sumInWithNaklad = data.Where(_ => _.Date == d && _.OperType == "Prihod")
                        .Sum(_ => _.Prihod * _.PriceWithNaklad) ?? 0;

                    var oldDate = d;
                    var priceOld = price;
                    if (quanVozvrat > 0)
                    {
                        var oldDateItems = data.Where(_ => _.Date <= d && _.Prihod > 0).ToList();
                        if (oldDateItems.Any())
                            oldDate = oldDateItems.Max(_ => _.Date);
                        var priceOldItems = data.Where(_ => _.Date == oldDate).ToList();

                        if (priceOldItems.Sum(_ => _.Price ?? 0) > 0)
                            priceOld = priceOldItems.Sum(_ => _.Prihod) / priceOldItems.Sum(_ => _.Price ?? 0);
                        else
                            priceOld = price;
                    }

                    if (nakopit <= 0) price = priceWithNaklad = 0;
                    if (nakopit + quanIn + quanVozvrat != 0)
                    {
                        price = (nakopit * price + sumIn + quanVozvrat * priceOld) /
                                (nakopit + quanIn + quanVozvrat);
                        priceWithNaklad = (nakopit * priceWithNaklad + sumInWithNaklad + quanVozvrat * priceOld) /
                                          (nakopit + quanIn + quanVozvrat);
                    }

                    nakopit += quanIn - quanOut + quanVozvrat;

                    if (d >= start && d <= end)
                        ret.Add(Tuple.Create(d, sQuanIn * price, sQuanIn * priceWithNaklad,
                            sQuanOut * price, sQuanOut * priceWithNaklad));
                }
            }

            return ret;
        }

        public static List<NomenklCalcFull> NomenklMoveSum2(DateTime start,
            DateTime end, decimal storeDC)
        {
            var ret = new List<NomenklCalcFull>();
            // ReSharper disable once CollectionNeverUpdated.Local
            var temp = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var alldata = ctx.NomenklMoveForCalc.Where(_ => _.StoreDC == storeDC && _.Date <= end).ToList();

                var remDCs = alldata.Select(_ => _.NomDC).Distinct().ToList();

                foreach (var ndc in remDCs)
                {
                    temp.Clear();
                    var data = alldata.Where(_ => _.NomDC == ndc)
                        .OrderBy(_ => _.Date).ToList();
                    if (data.Count == 0) continue;
                    decimal nPriceStart = 0,
                        nPriceStartWithNaklad = 0,
                        nPriceEnd = 0,
                        nPriceEndWithNaklad = 0,
                        nIn = 0,
                        nOut = 0,
                        nSumIn = 0,
                        nSumOut = 0,
                        nSumInWithNaklad = 0,
                        nSumOutWithNaklad = 0;

                    var nStart = data.Where(_ => _.Date < start).Sum(_ => _.Prihod - _.Rashod);
                    var sdprc = data.FirstOrDefault(_ => _.Date < start);
                    if (sdprc != null)
                    {
                        nPriceStart = sdprc.Price ?? 0;
                        nPriceStartWithNaklad = sdprc.PriceWithNaklad ?? 0;
                    }

                    var nEnd = data.Sum(_ => _.Prihod - _.Rashod);
                    var sdprc1 = data.FirstOrDefault(_ => _.Date <= end);
                    if (sdprc1 != null)
                    {
                        nPriceEnd = sdprc1.Price ?? 0;
                        nPriceEndWithNaklad = sdprc1.PriceWithNaklad ?? 0;
                    }

                    var moves = data.Where(_ => _.Date >= start && _.Date <= end).ToList();
                    if (nStart == 0 && nEnd == 0 && moves.Count == 0)
                        continue;
                    if (moves.Count > 0)
                    {
                        nIn = moves.Sum(_ => _.Prihod);
                        nOut = moves.Sum(_ => _.Rashod);
                        nSumIn = moves.Sum(_ => (_.Price ?? 0) * _.Prihod);
                        nSumOut = moves.Sum(_ => (_.Price ?? 0) * _.Rashod);
                        nSumInWithNaklad = moves.Sum(_ => (_.PriceWithNaklad ?? 0) * _.Prihod);
                        nSumOutWithNaklad = moves.Sum(_ => (_.PriceWithNaklad ?? 0) * _.Rashod);
                    }

                    var nitem = new NomenklCalcFull
                    {
                        StoreDC = storeDC,
                        NomDC = ndc,
                        Start = nStart,
                        PriceStart = nPriceStart,
                        PriceStartWithNaklad = nPriceStartWithNaklad,
                        In = nIn,
                        SumIn = nSumIn,
                        SumInWithNaklad = nSumInWithNaklad,
                        Out = nOut,
                        SumOut = nSumOut,
                        SumOutWithNaklad = nSumOutWithNaklad,
                        End = nEnd,
                        PriceEnd = nPriceEnd,
                        PriceEndWithNaklad = nPriceEndWithNaklad,
                    };

                    ret.Add(nitem);
                }

                return ret;
            }
        }

        /// <summary>
        ///     остатки на складе
        /// </summary>
        /// <param name="date"></param>
        /// <param name="storeDC"></param>
        /// <returns></returns>
        public static List<NomenklStoreRemainItem> GetNomenklStoreRemains(DateTime date, decimal storeDC)
        {
            //var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklStoreRemainItem>(sql).ToList();
            var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklStoreRemainItem>("GetNomenklRemains @date",  
                new SqlParameter("@date", date)).Where(_ => _.StoreDC == storeDC).ToList(); 
            return data;
        }

        /// <summary>
        ///     все остатки по всем складам
        /// </summary>
        /// <returns></returns>
        public static List<NomenklStoreRemainItem> GetNomenklStoreRemains(DateTime date)
        {
            var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklStoreRemainItem>("GetNomenklRemains @date",  
                new SqlParameter("@date", date)); 
            return new List<NomenklStoreRemainItem>(data.OrderBy(_ => _.NomenklDC).ThenBy(_ => _.StoreDC)
                .ThenBy(_ => _.Prihod));
        }

        /// <summary>
        ///     остатки товара по всем складам
        /// </summary>
        /// <returns></returns>
        public static List<NomenklStoreRemainItem> GetNomenklAllStoreRemains(DateTime date, decimal nomDC)
        {
            var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklStoreRemainItem>("GetNomenklRemains @date",  
                new SqlParameter("@date", date)).Where(_ => _.NomenklDC == nomDC).ToList(); 
            return data;
        }

        public static void CalcAllNomenklRemains([NotNull] ALFAMEDIAEntities context)
        {
            var sql = "DECLARE @NomDC NUMERIC(15, 0); " +
                      "DECLARE NomenklList CURSOR FOR SELECT DISTINCT NOM_DC FROM NOMENKL_RECALC; " +
                      "OPEN NomenklList FETCH NEXT FROM NomenklList INTO @NomDC " +
                      "WHILE @@fetch_status = 0 BEGIN " +
                      "     EXEC dbo.NomenklCalculateCostsForOne @NomDC " +
                      "     FETCH NEXT FROM NomenklList INTO @NomDC " +
                      "END " +
                      "CLOSE NomenklList; " +
                      "DEALLOCATE NomenklList; " +
                      "DELETE FROM WD_27 " +
                      "INSERT INTO dbo.WD_27 (DOC_CODE , SKLW_NOMENKL_DC , SKLW_DATE , SKLW_KOLICH) " +
                      "SELECT n.StoreDC, n.nomdc, n.DATE, SUM(n.prihod) - SUM(n.rashod) Quantity " +
                      "FROM NomenklMoveWithPrice n   " +
                      "WHERE n.Date = (SELECT MAX(n1.Date) FROM NomenklMoveWithPrice n1 " +
                      "                WHERE n1.NomDC = n.NomDC AND n.StoreDC = n1.StoreDC) " +
                      "GROUP BY n.NomDC,n.StoreDC,n.Date " +
                      "HAVING SUM(n.Prihod) - SUM(n.Rashod) > 0;" +
                      "DELETE FROM NOMENKL_RECALC;";
            try
            {
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,sql);
            }
            catch (Exception ex)
            {
                SaveError(ex);
            }
        }

        private static void SaveError(Exception ex)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                StringBuilder b = new StringBuilder(ex.Message);
                var ex1 = ex;
                while (ex1.InnerException != null)
                {
                    b.Append(ex1.Message);
                    ex1 = ex1.InnerException;
                }

                ctx.Errors.Add(new Data.Errors
                {
                    Id = Guid.NewGuid(),
                    DbId = GlobalOptions.DataBaseId,
                    Host = Environment.MachineName,
                    UserId = GlobalOptions.UserInfo.KursId,
                    ErrorText = b.ToString(),
                    Moment = DateTime.Now
                });
            }
        }

        public static void InsertNomenklForCalc(ALFAMEDIAEntities context, [NotNull] List<decimal> nomenkls)
        {
            try
            {
                foreach (var dc in nomenkls)
                {
                    var sql = "INSERT INTO NOMENKL_RECALC(NOM_DC,OPER_DATE) " +
                              $"VALUES({CustomFormat.DecimalToSqlDecimal(dc)},'{CustomFormat.DateToString(DateTime.Now)}');" +
                              "GO;";
                    context.Database.ExecuteSqlCommand(sql);
                }
            }
            catch (Exception ex)
            {
                SaveError(ex);
            }

        }
    }
}