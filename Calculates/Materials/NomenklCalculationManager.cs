using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core;
using Data;
using Helper;

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

        /// <summary>
        ///     Получение остатка товара на складе
        /// </summary>
        /// <returns></returns>
        public static NomenklStoreRemainItem GetNomenklStoreRemain(DateTime date, decimal nomDC, decimal storeDC)
        {
            //return new NomenklStoreRemainItem();
            var sql =
                "SELECT tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,SUM(tab.Prihod) AS Prihod, SUM(tab.Rashod)AS Rashod " +
                ", SUM(tab.Prihod) - SUM(tab.Rashod) AS Remain, ROUND(ISNULL(p.Price, 0), 2) AS PriceWithNaklad, ROUND(ISNULL(p.PRICE_WO_NAKLAD, 0), 2) AS Price " +
                ", ROUND((SUM(tab.Prihod) - SUM(tab.Rashod)), 2) * ROUND(ISNULL(p.PRICE_WO_NAKLAD, 0), 2) AS Summa,  " +
                "ROUND((SUM(tab.Prihod) - SUM(tab.Rashod)), 2) * ROUND(ISNULL(p.Price, 0), 2) AS SummaWithNaklad  " +
                "FROM(SELECT " +
                "T24.DDT_NOMENKL_DC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", S24.DD_SKLAD_POL_DC AS SkladDC " +
                ", SUM(T24.DDT_KOL_PRIHOD) AS Prihod " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Rashod " +
                "FROM TD_24 T24 " +
                " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                $" INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_POL_DC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' " +
                "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_POL_DC " +
                "UNION ALL " +
                "SELECT T24.DDT_NOMENKL_DC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", S24.DD_SKLAD_OTPR_DC AS SkladDC " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                ", SUM(DDT_KOL_RASHOD) AS Rashod " +
                "FROM TD_24 T24 " +
                " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                $" INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC  AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_OTPR_DC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' " +
                "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_OTPR_DC " +
                "UNION ALL " +
                "SELECT ntr.NomenklInDC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", nt.SkladDC AS SkladDC " +
                ", ntr.Quantity AS Prihod " +
                ", CAST(0 AS NUMERIC(18, 4))AS Rashod " +
                "FROM NomenklTransfer nt " +
                "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id " +
                $"   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklInDC  AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE NT.Date <= '{CustomFormat.DateToString(date)}' AND ISNULL(NTR.IsAccepted,0) = 1  AND nt.SkladDC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' " +
                "UNION all  " +
                "SELECT ntr.NomenklOutDC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", nt.SkladDC AS SkladDC " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                ", ntr.Quantity AS Rashod " +
                "FROM NomenklTransfer nt " +
                "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id  " +
                $"   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklOutDC  AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE nt.DATE <= '{CustomFormat.DateToString(date)}' AND ISNULL(NTR.IsAccepted,0) = 1  AND nt.SkladDC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' ) tab " +
                "LEFT OUTER JOIN NOM_PRICE p ON p.NOM_DC = tab.NomenklDC " +
                $"AND p.DATE = (SELECT MAX(pp.DATE) FROM NOM_PRICE pp WHERE pp.NOM_DC = tab.NomenklDC AND pp.DATE <= '{CustomFormat.DateToString(date)}') " +
                "GROUP BY tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,p.PRICE_WO_NAKLAD,p.Price " +
                "HAVING SUM(tab.Prihod) - SUM(tab.Rashod) != 0 ";
            var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklStoreRemainItem>(sql).ToList();
            if (data.Count == 0)
                return null;
            return data.First();
        }

        public static NomenklStoreRemainItem GetNomenklStoreRemain(ALFAMEDIAEntities ctx, DateTime date, decimal nomDC, decimal storeDC)
        {
            //return new NomenklStoreRemainItem();
            var sql =
                "SELECT tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,SUM(tab.Prihod) AS Prihod, SUM(tab.Rashod)AS Rashod " +
                ", SUM(tab.Prihod) - SUM(tab.Rashod) AS Remain, ROUND(ISNULL(p.Price, 0), 2) AS PriceWithNaklad, ROUND(ISNULL(p.PRICE_WO_NAKLAD, 0), 2) AS Price " +
                ", ROUND((SUM(tab.Prihod) - SUM(tab.Rashod)), 2) * ROUND(ISNULL(p.PRICE_WO_NAKLAD, 0), 2) AS Summa,  " +
                "ROUND((SUM(tab.Prihod) - SUM(tab.Rashod)), 2) * ROUND(ISNULL(p.Price, 0), 2) AS SummaWithNaklad  " +
                "FROM(SELECT " +
                "T24.DDT_NOMENKL_DC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", S24.DD_SKLAD_POL_DC AS SkladDC " +
                ", SUM(T24.DDT_KOL_PRIHOD) AS Prihod " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Rashod " +
                "FROM TD_24 T24 " +
                " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                $" INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_POL_DC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' " +
                "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_POL_DC " +
                "UNION ALL " +
                "SELECT T24.DDT_NOMENKL_DC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", S24.DD_SKLAD_OTPR_DC AS SkladDC " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                ", SUM(DDT_KOL_RASHOD) AS Rashod " +
                "FROM TD_24 T24 " +
                " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                $" INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC  AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_OTPR_DC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' " +
                "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_OTPR_DC " +
                "UNION ALL " +
                "SELECT ntr.NomenklInDC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", nt.SkladDC AS SkladDC " +
                ", ntr.Quantity AS Prihod " +
                ", CAST(0 AS NUMERIC(18, 4))AS Rashod " +
                "FROM NomenklTransfer nt " +
                "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id " +
                $"   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklInDC  AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE NT.Date <= '{CustomFormat.DateToString(date)}' AND ISNULL(NTR.IsAccepted,0) = 1  AND nt.SkladDC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' " +
                "UNION all  " +
                "SELECT ntr.NomenklOutDC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", nt.SkladDC AS SkladDC " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                ", ntr.Quantity AS Rashod " +
                "FROM NomenklTransfer nt " +
                "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id  " +
                $"   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklOutDC  AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE nt.DATE <= '{CustomFormat.DateToString(date)}' AND ISNULL(NTR.IsAccepted,0) = 1  AND nt.SkladDC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' ) tab " +
                "LEFT OUTER JOIN NOM_PRICE p ON p.NOM_DC = tab.NomenklDC " +
                $"AND p.DATE = (SELECT MAX(pp.DATE) FROM NOM_PRICE pp WHERE pp.NOM_DC = tab.NomenklDC AND pp.DATE <= '{CustomFormat.DateToString(date)}') " +
                "GROUP BY tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,p.PRICE_WO_NAKLAD,p.Price " +
                "HAVING SUM(tab.Prihod) - SUM(tab.Rashod) != 0 ";
            var data = ctx.Database.SqlQuery<NomenklStoreRemainItem>(sql).ToList();
            if (data.Count == 0)
            {
                var nom = MainReferences.GetNomenkl(nomDC);
                return new NomenklStoreRemainItem()
                {
                    NomenklDC = nomDC,
                    Summa = 0,
                    NomCurrencyDC = nom.Currency.DocCode,
                    NomenklName = nom.Name,
                    Price = 0,
                    PriceWithNaklad = 0,
                    Prihod = 0,
                    Rashod = 0,
                    Remain = 0,
                    SkladDC = storeDC,
                    SummaWithNaklad = 0
                };
            }
            return data.First();
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
            var temp = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
            var rems = NomenklRemains(start, end, storeDC);
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var n in rems)
                {
                    temp.Clear();
                    var nitem = new NomenklCalcFull
                    {
                        NomDC = n.Item1,
                        StoreDC = storeDC
                    };
                    var data = ctx.NomenklMoveForCalc.Where(_ => _.Date <= end && _.NomDC == n.Item1)
                        .OrderBy(_ => _.Date).ToList();
                    if (data.Count == 0) return null;
                    decimal price = 0, priceWithNaklad = 0, nakopit = 0;
                    var dates = data.Select(_ => _.Date).Distinct().OrderBy(_ => _)
                        .ToList();

                    foreach (var d in dates)
                    {
                        var quanOut = data.Where(_ => _.Date == d && _.OperType == "Rashod").Sum(_ => _.Rashod);
                        var sQuanOut = data.Where(_ => _.Date == d && _.StoreDC == storeDC
                                                                   && (_.OperType == "Rashod" ||
                                                                       _.OperType == "InnerMove"))
                            .Sum(_ => _.Rashod);
                        var quanIn = data.Where(_ => _.Date == d && _.OperType == "Prihod").Sum(_ => _.Prihod);
                        var quanVozvrat = data.Where(_ => _.Date == d && _.OperType == "Vozvrat").Sum(_ => _.Prihod);
                        var sQuanIn = data.Where(_ => _.Date == d && _.StoreDC == storeDC
                                                                  && (_.OperType == "Prihod" || _.OperType == "Vozvrat"
                                                                                             || _.OperType ==
                                                                                             "InnerMove"))
                            .Sum(_ => _.Prihod);

                        var sumIn =
                            data.Where(_ => _.Date == d && _.OperType == "Prihod").Sum(_ => _.Prihod * _.Price) ??
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
                        if (temp.Count == 0 && d >= start && d <= end)
                        {
                            nitem.Start = nakopit;
                            nitem.PriceStart = price;
                            nitem.PriceStartWithNaklad = priceWithNaklad;
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
                        {
                            if (temp.Count == dates.Count - 1)
                            {
                                nitem.End = nakopit;
                                nitem.PriceEnd = price;
                                nitem.PriceEndWithNaklad = priceWithNaklad;
                            }
                            nitem.In += sQuanIn;
                            nitem.Out += sQuanOut;
                            nitem.SumIn += sQuanIn * price;
                            nitem.SumInWithNaklad += sQuanIn * priceWithNaklad;
                            nitem.SumOut += sQuanOut * price;
                            nitem.SumOutWithNaklad += sQuanOut * priceWithNaklad;
                            temp.Add(Tuple.Create(d, sQuanIn * price, sQuanIn * priceWithNaklad,
                                sQuanOut * price, sQuanOut * priceWithNaklad));
                        }
                    }
                    ret.Add(nitem);
                }
            }

            return ret;
        }

        /// <summary>
        ///     остатки на складе
        /// </summary>
        /// <param name="date"></param>
        /// <param name="storeDC"></param>
        /// <returns></returns>
        public static List<NomenklStoreRemainItem> GetNomenklStoreRemains(DateTime date, decimal storeDC)
        {
            var sql =
                "SELECT tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,SUM(tab.Prihod) AS Prihod, SUM(tab.Rashod)AS Rashod " +
                ", SUM(tab.Prihod) - SUM(tab.Rashod) AS Remain, ROUND(ISNULL(p.Price, 0), 2) AS PriceWithNaklad, ROUND(ISNULL(p.PRICE_WO_NAKLAD, 0), 2) AS Price " +
                ", ROUND((SUM(tab.Prihod) - SUM(tab.Rashod)), 2) * ROUND(ISNULL(p.PRICE_WO_NAKLAD, 0), 2) AS Summa,  " +
                "ROUND((SUM(tab.Prihod) - SUM(tab.Rashod)), 2) * ROUND(ISNULL(p.Price, 0), 2) AS SummaWithNaklad  " +
                "FROM(SELECT " +
                "T24.DDT_NOMENKL_DC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", S24.DD_SKLAD_POL_DC AS SkladDC " +
                ", SUM(T24.DDT_KOL_PRIHOD) AS Prihod " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Rashod " +
                "FROM TD_24 T24 " +
                " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                " INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC " +
                $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_POL_DC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' " +
                "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_POL_DC " +
                "UNION ALL " +
                "SELECT T24.DDT_NOMENKL_DC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", S24.DD_SKLAD_OTPR_DC AS SkladDC " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                ", SUM(DDT_KOL_RASHOD) AS Rashod " +
                "FROM TD_24 T24 " +
                " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                " INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC " +
                $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_OTPR_DC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' " +
                "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_OTPR_DC " +
                "UNION ALL " +
                "SELECT ntr.NomenklInDC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", nt.SkladDC AS SkladDC " +
                ", ntr.Quantity AS Prihod " +
                ", CAST(0 AS NUMERIC(18, 4))AS Rashod " +
                "FROM NomenklTransfer nt " +
                "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id " +
                "   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklInDC " +
                $"WHERE NT.Date <= '{CustomFormat.DateToString(date)}' AND ISNULL(NTR.IsAccepted,0) = 1  AND nt.SkladDC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' " +
                "UNION all  " +
                "SELECT ntr.NomenklOutDC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", nt.SkladDC AS SkladDC " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                ", ntr.Quantity AS Rashod " +
                "FROM NomenklTransfer nt " +
                "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id  " +
                "   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklOutDC " +
                $"WHERE nt.DATE <= '{CustomFormat.DateToString(date)}' AND ISNULL(NTR.IsAccepted,0) = 1  AND nt.SkladDC = '{CustomFormat.DecimalToSqlDecimal(storeDC)}' ) tab " +
                "LEFT OUTER JOIN NOM_PRICE p ON p.NOM_DC = tab.NomenklDC " +
                $"AND p.DATE = (SELECT MAX(pp.DATE) FROM NOM_PRICE pp WHERE pp.NOM_DC = tab.NomenklDC AND pp.DATE <= '{CustomFormat.DateToString(date)}') " +
                "GROUP BY tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,p.PRICE_WO_NAKLAD,p.Price " +
                "HAVING SUM(tab.Prihod) - SUM(tab.Rashod) != 0 ";
            var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklStoreRemainItem>(sql).ToList();
            return data;
        }

        /// <summary>
        ///     все остатки по всем складам
        /// </summary>
        /// <returns></returns>
        public static List<NomenklStoreRemainItem> GetNomenklStoreRemains(DateTime date)
        {
            string sql = null;
            switch (GlobalOptions.SystemProfile.NomenklCalcType)
            {
                case NomenklCalcType.NakladSeparately:
                    sql =
                        "SELECT tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,SUM(tab.Prihod) AS Prihod, SUM(tab.Rashod)AS Rashod " +
                        ", SUM(tab.Prihod) - SUM(tab.Rashod) AS Remain, ISNULL(p.Price, 0) AS PriceWithNaklad, ISNULL(p.PRICE_WO_NAKLAD, 0) AS Price " +
                        ", (SUM(tab.Prihod) - SUM(tab.Rashod)) * ISNULL(p.PRICE_WO_NAKLAD, 0) AS Summa,  " +
                        "(SUM(tab.Prihod) - SUM(tab.Rashod)) * ISNULL(p.Price, 0) AS SummaWithNaklad  " +
                        "FROM(SELECT " +
                        "T24.DDT_NOMENKL_DC AS NomenklDC " +
                        ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                        ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                        ", S24.DD_SKLAD_POL_DC AS SkladDC " +
                        ", SUM(T24.DDT_KOL_PRIHOD) AS Prihod " +
                        ", CAST(0 AS NUMERIC(18, 4)) AS Rashod " +
                        "FROM TD_24 T24 " +
                        " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                        " INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC " +
                        $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_POL_DC IS NOT NULL " +
                        "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_POL_DC " +
                        "UNION ALL " +
                        "SELECT T24.DDT_NOMENKL_DC AS NomenklDC " +
                        ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                        ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                        ", S24.DD_SKLAD_OTPR_DC AS SkladDC " +
                        ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                        ", SUM(DDT_KOL_RASHOD) AS Rashod " +
                        "FROM TD_24 T24 " +
                        " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                        " INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC " +
                        $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_OTPR_DC IS NOT NULL " +
                        "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_OTPR_DC " +
                        "UNION ALL " +
                        "SELECT ntr.NomenklInDC AS NomenklDC " +
                        ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                        ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                        ", nt.SkladDC AS SkladDC " +
                        ", ntr.Quantity AS Prihod " +
                        ", CAST(0 AS NUMERIC(18, 4))AS Rashod " +
                        "FROM NomenklTransfer nt " +
                        "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id " +
                        "   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklInDC " +
                        $"WHERE NT.Date <= '{CustomFormat.DateToString(date)}' AND ISNULL(NTR.IsAccepted,0) = 1 " +
                        "UNION all  " +
                        "SELECT ntr.NomenklOutDC AS NomenklDC " +
                        ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                        ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                        ", nt.SkladDC AS SkladDC " +
                        ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                        ", ntr.Quantity AS Rashod " +
                        "FROM NomenklTransfer nt " +
                        "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id  " +
                        "   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklOutDC " +
                        $"WHERE nt.DATE <= '{CustomFormat.DateToString(date)}' AND ISNULL(NTR.IsAccepted,0) = 1) tab " +
                        "LEFT OUTER JOIN NOM_PRICE p ON p.NOM_DC = tab.NomenklDC " +
                        $"AND p.DATE = (SELECT MAX(pp.DATE) FROM NOM_PRICE pp WHERE pp.NOM_DC = tab.NomenklDC AND pp.DATE <= '{CustomFormat.DateToString(date)}') " +
                        "GROUP BY tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,p.PRICE_WO_NAKLAD,p.Price " +
                        "HAVING SUM(tab.Prihod) - SUM(tab.Rashod) != 0 ";
                    break;
                case NomenklCalcType.Standart:
                    sql =
                        "SELECT tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,SUM(tab.Prihod) AS Prihod, SUM(tab.Rashod)AS Rashod " +
                        ", SUM(tab.Prihod) - SUM(tab.Rashod) AS Remain, ISNULL(p.Price, 0) AS PriceWithNaklad, ISNULL(p.PRICE_WO_NAKLAD, 0) AS Price " +
                        ", (SUM(tab.Prihod) - SUM(tab.Rashod)) * ISNULL(p.PRICE_WO_NAKLAD, 0) AS Summa,  " +
                        "(SUM(tab.Prihod) - SUM(tab.Rashod)) * ISNULL(p.Price, 0) AS SummaWithNaklad  " +
                        "FROM(SELECT " +
                        "T24.DDT_NOMENKL_DC AS NomenklDC " +
                        ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                        ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                        ", S24.DD_SKLAD_POL_DC AS SkladDC " +
                        ", SUM(T24.DDT_KOL_PRIHOD) AS Prihod " +
                        ", CAST(0 AS NUMERIC(18, 4)) AS Rashod " +
                        "FROM TD_24 T24 " +
                        " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                        " INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC " +
                        $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_POL_DC IS NOT NULL " +
                        "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_POL_DC " +
                        "UNION ALL " +
                        "SELECT T24.DDT_NOMENKL_DC AS NomenklDC " +
                        ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                        ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                        ", S24.DD_SKLAD_OTPR_DC AS SkladDC " +
                        ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                        ", SUM(DDT_KOL_RASHOD) AS Rashod " +
                        "FROM TD_24 T24 " +
                        " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                        " INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC " +
                        $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_OTPR_DC IS NOT NULL " +
                        "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_OTPR_DC " +
                        "UNION ALL " +
                        "SELECT ntr.NomenklInDC AS NomenklDC " +
                        ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                        ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                        ", nt.SkladDC AS SkladDC " +
                        ", ntr.Quantity AS Prihod " +
                        ", CAST(0 AS NUMERIC(18, 4))AS Rashod " +
                        "FROM NomenklTransfer nt " +
                        "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id " +
                        "   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklInDC " +
                        $"WHERE NT.Date <= '{CustomFormat.DateToString(date)}' AND ISNULL(NTR.IsAccepted,0) = 1 " +
                        "UNION all  " +
                        "SELECT ntr.NomenklOutDC AS NomenklDC " +
                        ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                        ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                        ", nt.SkladDC AS SkladDC " +
                        ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                        ", ntr.Quantity AS Rashod " +
                        "FROM NomenklTransfer nt " +
                        "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id  " +
                        "   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklOutDC " +
                        $"WHERE nt.DATE <= '{CustomFormat.DateToString(date)}' AND ISNULL(NTR.IsAccepted,0) = 1) tab " +
                        "LEFT OUTER JOIN NOM_PRICE p ON p.NOM_DC = tab.NomenklDC " +
                        $"AND p.DATE = (SELECT MAX(pp.DATE) FROM NOM_PRICE pp WHERE pp.NOM_DC = tab.NomenklDC AND pp.DATE <= '{CustomFormat.DateToString(date)}') " +
                        "GROUP BY tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,p.PRICE_WO_NAKLAD,p.Price " +
                        "HAVING SUM(tab.Prihod) - SUM(tab.Rashod) != 0 ";
                    break;
            }

            var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklStoreRemainItem>(sql).ToList();
            return data;
        }

        /// <summary>
        ///     остатки товара по всем складам
        /// </summary>
        /// <returns></returns>
        public static List<NomenklStoreRemainItem> GetNomenklAllStoreRemains(DateTime date, decimal nomDC)
        {
            var sql =
                "SELECT tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,SUM(tab.Prihod) AS Prihod, SUM(tab.Rashod)AS Rashod " +
                ", SUM(tab.Prihod) - SUM(tab.Rashod) AS Remain, ROUND(ISNULL(p.Price, 0), 2) AS PriceWithNaklad, ROUND(ISNULL(p.PRICE_WO_NAKLAD, 0), 2) AS Price " +
                ", ROUND((SUM(tab.Prihod) - SUM(tab.Rashod)), 2) * ROUND(ISNULL(p.PRICE_WO_NAKLAD, 0), 2) AS Summa,  " +
                "ROUND((SUM(tab.Prihod) - SUM(tab.Rashod)), 2) * ROUND(ISNULL(p.Price, 0), 2) AS SummaWithNaklad  " +
                "FROM(SELECT " +
                "T24.DDT_NOMENKL_DC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", S24.DD_SKLAD_POL_DC AS SkladDC " +
                ", SUM(T24.DDT_KOL_PRIHOD) AS Prihod " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Rashod " +
                "FROM TD_24 T24 " +
                " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                $" INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC  AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_POL_DC IS NOT NULL " +
                "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_POL_DC " +
                "UNION ALL " +
                "SELECT T24.DDT_NOMENKL_DC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", S24.DD_SKLAD_OTPR_DC AS SkladDC " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                ", SUM(DDT_KOL_RASHOD) AS Rashod " +
                "FROM TD_24 T24 " +
                " INNER JOIN SD_24 S24 ON S24.DOC_CODE = T24.DOC_CODE " +
                $" INNER JOIN SD_83 S83 ON S83.DOC_CODE = T24.DDT_NOMENKL_DC  AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE DD_DATE <= '{CustomFormat.DateToString(date)}' AND S24.DD_SKLAD_OTPR_DC IS NOT NULL " +
                "GROUP BY T24.DDT_NOMENKL_DC, S83.NOM_SALE_CRS_DC, S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')', S24.DD_SKLAD_OTPR_DC " +
                "UNION ALL " +
                "SELECT ntr.NomenklInDC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", nt.SkladDC AS SkladDC " +
                ", ntr.Quantity AS Prihod " +
                ", CAST(0 AS NUMERIC(18, 4))AS Rashod " +
                "FROM NomenklTransfer nt " +
                "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id " +
                $"   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklInDC  AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE NT.Date <= '{CustomFormat.DateToString(date)}' AND ISNULL(NTR.IsAccepted,0) = 1 " +
                "UNION all  " +
                "SELECT ntr.NomenklOutDC AS NomenklDC " +
                ", S83.NOM_SALE_CRS_DC AS NomCurrencyDC " +
                ", S83.NOM_NAME + '(' + S83.NOM_NOMENKL + ')' AS NomenklName " +
                ", nt.SkladDC AS SkladDC " +
                ", CAST(0 AS NUMERIC(18, 4)) AS Prihod " +
                ", ntr.Quantity AS Rashod " +
                "FROM NomenklTransfer nt " +
                "   INNER JOIN NomenklTransferRow ntr ON ntr.DocId = nt.Id  " +
                $"   INNER JOIN SD_83 S83 ON S83.DOC_CODE = ntr.NomenklOutDC  AND S83.DOC_CODE = '{CustomFormat.DecimalToSqlDecimal(nomDC)}' " +
                $"WHERE nt.DATE <= '{CustomFormat.DateToString(date)} AND ISNULL(NTR.IsAccepted,0) = 1 ') tab " +
                "LEFT OUTER JOIN NOM_PRICE p ON p.NOM_DC = tab.NomenklDC " +
                $"AND p.DATE = (SELECT MAX(pp.DATE) FROM NOM_PRICE pp WHERE pp.NOM_DC = tab.NomenklDC AND pp.DATE <= '{CustomFormat.DateToString(date)}') " +
                "GROUP BY tab.NomenklDC,tab.NomCurrencyDC,tab.NomenklName,tab.SkladDC,p.PRICE_WO_NAKLAD,p.Price " +
                "HAVING SUM(tab.Prihod) - SUM(tab.Rashod) != 0 ";
            var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklStoreRemainItem>(sql).ToList();
            return data;
        }
    }
}