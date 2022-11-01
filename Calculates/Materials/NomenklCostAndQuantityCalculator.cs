using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Transactions;
using Core;
using Data;
using KursDomain.Documents.Currency;
using KursDomain.Documents.NomenklManagement;
using KursDomain.References;

//using Core.ViewModel.Common;

namespace Calculates.Materials
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public class NomenklCostAndQuantityCalculator
    {
        private readonly CurrencyRates myRates = new CurrencyRates();
        public List<NomenklCosts> CalculateCosts { set; get; } = new List<NomenklCosts>();
        private List<NomenklRashod> RashodData { set; get; }
        private List<NomenklPirhod> PrihodData { set; get; }
        public List<LoaderOperation> LoadfOperations { set; get; }
        public List<NomenklOperation> Operations { set; get; } = new List<NomenklOperation>();

        public void LoadOperation2()
        {
            try
            {
                using (var ctx = GlobalOptions.GetTestEntities())
                {
                    LoadfOperations =
                        new List<LoaderOperation>(ctx.Database.SqlQuery<LoaderOperation>("SELECT " +
                                                                                         "NEWID() AS Id, " +
                                                                                         "T24.DOC_CODE AS DOC_CODE, " +
                                                                                         "T24.Code AS Code, " +
                                                                                         "S24.DD_DATE as Date, " +
                                                                                         "S24.DD_TYPE_DC as OperType, " +
                                                                                         "T24.DDT_NOMENKL_DC AS NomenklDC, " +
                                                                                         "T24.DDT_KOL_PRIHOD AS Receipt, " +
                                                                                         "T24.DDT_KOL_RASHOD AS Expense, " +
                                                                                         "S24.DD_SKLAD_POL_DC AS SkladPolDC, " +
                                                                                         "S24.DD_SKLAD_OTPR_DC AS SkladOtprDC, " +
                                                                                         "S24.DD_KONTR_POL_DC AS KontrPolDC, " +
                                                                                         "S24.DD_KONTR_OTPR_DC AS KontrOtpravDC, " +
                                                                                         "T26.SFT_SUMMA_K_OPLATE_KONTR_CRS AS SummaIn " +
                                                                                         "FROM TD_24 T24 " +
                                                                                         "INNER JOIN SD_24 S24 " +
                                                                                         "  ON S24.DOC_CODE = T24.DOC_CODE AND S24.DD_TYPE_DC IN (2010000001,2010000003, 2010000005, 2010000008, 2010000009, 2010000010, 2010000012, 2010000014) " +
                                                                                         "LEFT OUTER JOIN TD_26 t26 " +
                                                                                         "  ON t26.DOC_CODE = T24.DDT_SPOST_DC " +
                                                                                         "  AND t26.Code = T24.DDT_SPOST_ROW_CODE " +
                                                                                         "LEFT OUTER JOIN SD_26 s26 " +
                                                                                         "  ON s26.DOC_CODE = t26.DOC_CODE " +
                                                                                         "LEFT OUTER JOIN TD_84 t84 " +
                                                                                         "  ON t84.DOC_CODE = T24.DDT_SFACT_DC " +
                                                                                         " AND t84.Code = T24.DDT_SFACT_ROW_CODE " +
                                                                                         "LEFT OUTER JOIN SD_84 s84 " +
                                                                                         " ON s84.DOC_CODE = t84.DOC_CODE " +
                                                                                         "where T24.DDT_KOL_PRIHOD > 0 OR T24.DDT_KOL_RASHOD > 0")
                            .ToList()
                            .OrderBy(_ => _.Date)
                            .ThenBy(_ => _.OperType)
                            .ThenBy(_ => _.NomenklDC));
                    Operations.Clear();
                    try
                    {
                        foreach (
                            var item in RemoveCircleInnerMove(LoadfOperations).SelectMany(NomenklOperation.Generate))
                            Operations.Add(item);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void CalcOneNomenkl(Nomenkl nom)
        {
            var data = Operations.Where(_ => _.Nomenkl.DocCode == nom.DocCode);
            var dates = data.Select(_ => _.Date).Distinct();
            foreach (var d in dates) CalcOneDay(nom, d);
        }

        private void CalcOneDay(Nomenkl nom, DateTime date)
        {
            // ReSharper disable once CollectionNeverQueried.Local
            var start = new List<NomenklCosts>();
            foreach (var store in MainReferences.Warehouses.Values)
            foreach (var crs in MainReferences.Currencies.Values)
                if (!CalculateCosts.Any(_ => _.Nomenkl.DocCode == nom.DocCode
                                             && _.Warehouse.DocCode == store.DocCode &&
                                             _.Currency.DocCode == crs.DocCode && _.Date < date))
                {
                    start.Add(new NomenklCosts
                    {
                        Date = date,
                        Quantity = 0,
                        Price = 0,
                        Currency = crs,
                        Nomenkl = nom,
                        Warehouse = store
                    });
                }
                else
                {
                    var d = CalculateCosts.Where(_ => _.Nomenkl.DocCode == nom.DocCode
                                                      && _.Warehouse.DocCode == store.DocCode &&
                                                      _.Currency.DocCode == crs.DocCode && _.Date < date)
                        .Select(dt => dt.Date).Max();
                    var s = CalculateCosts.Single(_ => _.Nomenkl.DocCode == nom.DocCode
                                                       && _.Warehouse.DocCode == store.DocCode &&
                                                       _.Currency.DocCode == crs.DocCode && _.Date == d);
                    start.Add(new NomenklCosts
                    {
                        Date = date,
                        Quantity = s.Quantity,
                        Price = s.Price,
                        Currency = crs,
                        Nomenkl = nom,
                        Warehouse = store
                    });
                }
            //foreach(var oper in data.Where(_ => _.OperationType != NomenklMoveOperationType.ReceiptInnerMove ))
        }

        private List<LoaderOperation> RemoveCircleInnerMove(List<LoaderOperation> loadfOperations)
        {
            return loadfOperations;
        }

        public void LoadOperation()
        {
            try
            {
                using (var ctx = GlobalOptions.GetTestEntities())
                {
                    RashodData = ctx.Database.SqlQuery<NomenklRashod>("SELECT " +
                                                                      "CASE " +
                                                                      " WHEN s24.DD_TYPE_DC = 2010000003 THEN 12 " +
                                                                      " WHEN s24.DD_TYPE_DC = 2010000005 THEN 11 " +
                                                                      " WHEN s24.DD_TYPE_DC = 2010000008 THEN 15 " +
                                                                      " WHEN s24.DD_TYPE_DC = 2010000009 THEN 14 " +
                                                                      " WHEN s24.DD_TYPE_DC = 2010000012 THEN 13 " +
                                                                      "    WHEN s24.DD_TYPE_DC = 2010000014 THEN 2 " +
                                                                      "    ELSE - 1 END AS OperType, " +
                                                                      "  s24.DD_DATE AS DocDate, " +
                                                                      "  s24.DD_SKLAD_OTPR_DC AS SkladDC, " +
                                                                      "  CASE WHEN s24.DD_TYPE_DC = 2010000014 THEN s24.DD_SKLAD_POL_DC ELSE 0 END as SklaPolDC, " +
                                                                      "  CASE " +
                                                                      "   WHEN s43.DOC_CODE IS NOT NULL THEN s43.VALUTA_DC " +
                                                                      "   ELSE s83.NOM_SALE_CRS_DC " +
                                                                      " END AS CurrencyDC, " +
                                                                      " DDT_NOMENKL_DC AS NomenklDC, " +
                                                                      " SUM(t24.DDT_KOL_RASHOD) AS Rashod " +
                                                                      "FROM TD_24 t24 " +
                                                                      "INNER JOIN SD_24 s24 " +
                                                                      "  ON s24.DOC_CODE = t24.DOC_CODE " +
                                                                      "  AND s24.DD_TYPE_DC IN(2010000003, 2010000005, 2010000008, 2010000009, 2010000010, 2010000012, 2010000014) " +
                                                                      "  AND s24.DD_SKLAD_OTPR_DC IS NOT NULL " +
                                                                      "INNER JOIN SD_83 s83 " +
                                                                      "  ON s83.DOC_CODE = t24.DDT_NOMENKL_DC " +
                                                                      "LEFT OUTER JOIN TD_84 t84 " +
                                                                      "  ON t84.DOC_CODE = t24.DDT_SFACT_DC " +
                                                                      "  AND t84.Code = t24.DDT_SFACT_ROW_CODE " +
                                                                      "LEFT OUTER JOIN SD_84 s84 " +
                                                                      "  ON s84.DOC_CODE = t84.DOC_CODE " +
                                                                      "LEFT OUTER JOIN SD_43 s43 " +
                                                                      "  ON s43.DOC_CODE = s84.SF_CLIENT_DC " +
                                                                      "GROUP BY s24.DD_TYPE_DC, " +
                                                                      "         s24.DD_DATE, " +
                                                                      "         s24.DD_SKLAD_OTPR_DC, " +
                                                                      "  s24.DD_SKLAD_POL_DC, " +
                                                                      "         CASE " +
                                                                      "           WHEN s43.DOC_CODE IS NOT NULL THEN s43.VALUTA_DC " +
                                                                      "           ELSE s83.NOM_SALE_CRS_DC " +
                                                                      "         END, " +
                                                                      "         DDT_NOMENKL_DC").ToList();
                    PrihodData = ctx.Database.SqlQuery<NomenklPirhod>("SELECT " +
                                                                      "CASE " +
                                                                      " WHEN s43.DOC_CODE IS NOT NULL THEN s43.VALUTA_DC " +
                                                                      " ELSE s83.NOM_SALE_CRS_DC " +
                                                                      "END AS CurrencyDC, " +
                                                                      "CAST(CASE " +
                                                                      "WHEN s24.DD_TYPE_DC != 2010000001 THEN t24.DDT_FACT_CENA " +
                                                                      "ELSE t26.SFT_SUMMA_K_OPLATE_KONTR_CRS / t26.SFT_KOL " +
                                                                      "END AS NUMERIC(18, 2)) AS Price, " +
                                                                      " CAST(CASE " +
                                                                      "  WHEN t26.DOC_CODE IS NOT NULL THEN ISNULL(t26.SFT_SUMMA_NAKLAD, 0) / t26.SFT_KOL " +
                                                                      " ELSE 0 " +
                                                                      " END AS NUMERIC(18, 2)) AS PriceNaklad, " +
                                                                      " s24.DD_DATE AS DocDate, " +
                                                                      " s24.DD_SKLAD_POL_DC AS SkladDC, " +
                                                                      " CASE WHEN s24.DD_TYPE_DC = 2010000014 THEN s24.DD_SKLAD_OTPR_DC ELSE 0 END as SklaOtprDC, " +
                                                                      " DDT_NOMENKL_DC AS NomenklDC, " +
                                                                      " DDT_KOL_PRIHOD AS Prihod, " +
                                                                      "CASE " +
                                                                      "  WHEN s24.DD_TYPE_DC = 2010000001 THEN 1 " +
                                                                      "  WHEN s24.DD_TYPE_DC = 2010000005 THEN 0 " +
                                                                      "  WHEN s24.DD_TYPE_DC = 2010000008 THEN 4 " +
                                                                      "  WHEN s24.DD_TYPE_DC = 2010000009 THEN 3 " +
                                                                      "  WHEN s24.DD_TYPE_DC = 2010000014 THEN 2 " +
                                                                      "  ELSE - 1 " +
                                                                      " END AS OperType " +
                                                                      " FROM TD_24 t24 " +
                                                                      " INNER JOIN SD_24 s24 " +
                                                                      "  ON s24.DOC_CODE = t24.DOC_CODE " +
                                                                      "  AND s24.DD_TYPE_DC IN(2010000001, 2010000005, 2010000008, 2010000009, 2010000014) " +
                                                                      "  AND s24.DD_SKLAD_POL_DC IS NOT NULL " +
                                                                      " INNER JOIN SD_83 s83 " +
                                                                      "  ON s83.DOC_CODE = t24.DDT_NOMENKL_DC " +
                                                                      " LEFT OUTER JOIN TD_26 t26 " +
                                                                      "  ON t26.DOC_CODE = t24.DDT_SPOST_DC " +
                                                                      "  AND t26.Code = t24.DDT_SPOST_ROW_CODE " +
                                                                      " LEFT OUTER JOIN SD_26 s26 " +
                                                                      " ON s26.DOC_CODE = t26.DOC_CODE " +
                                                                      "LEFT OUTER JOIN SD_43 s43 " +
                                                                      "  ON s43.DOC_CODE = s26.SF_POST_DC " +
                                                                      "  ORDER BY s24.DD_DATE ").ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private NomenklCosts CalcOne(NomenklPirhod p)
        {
            return CalcOne(p.NomenklDC, p.SkladDC, p.CurrencyDC, p.DocDate);
        }

        private NomenklCosts CalcOne(decimal nomDC, decimal sklDC, decimal crsDC, DateTime date,
            NomenklPirhod operInner = null)
        {
            var ret =
                CalculateCosts.SingleOrDefault(
                    _ =>
                        _.Warehouse.DocCode == sklDC && _.Nomenkl.DocCode == nomDC && _.Currency.DocCode == crsDC &&
                        _.Date == date);
            if (ret != null)
            {
                if (operInner == null) return ret;
                if (ret.Quantity >= operInner.Prihod)
                {
                    ret.Quantity = ret.Quantity - operInner.Prihod;
                    return ret;
                }
                var costs2 =
                    CalculateCosts.Where(
                        _ =>
                            _.Warehouse.DocCode == sklDC && _.Nomenkl.DocCode == nomDC && _.Date == date &&
                            _.Currency.DocCode != operInner.CurrencyDC).ToList();
                var cnt = operInner.Prihod;
                foreach (var cst in costs2)
                {
                    if (cnt <= 0) break;
                    cnt = cnt - cst.Quantity;
                    cst.Quantity = cnt > 0 ? 0 : cst.Quantity - cnt;
                    ret.Price = (ret.Price * ret.Quantity +
                                 myRates.GetRate(cst.Currency.DocCode, operInner.CurrencyDC, date) * cst.Price *
                                 (cnt > 0 ? cst.Quantity : cst.Quantity - cnt)) /
                                (ret.Quantity + (cnt > 0 ? cst.Quantity : cst.Quantity - cnt));
                }
                return ret;
            }
            if (operInner != null)
            {
                var costs =
                    CalculateCosts.Where(_ =>
                            _.Warehouse.DocCode == sklDC && _.Nomenkl.DocCode == nomDC && _.Date == date)
                        .ToList();
                if (costs.Count == 0)
                    foreach (var cst in costs)
                        CalcOne(nomDC, sklDC, cst.Currency.DocCode, date);
                var costs2 =
                    CalculateCosts.Where(_ =>
                            _.Warehouse.DocCode == sklDC && _.Nomenkl.DocCode == nomDC && _.Date == date)
                        .ToList();
                var nomCost = new NomenklCosts
                {
                    Nomenkl = MainReferences.GetNomenkl(nomDC),
                    Warehouse = MainReferences.Warehouses[sklDC],
                    Date = date,
                    Currency = MainReferences.Currencies[operInner.CurrencyDC],
                    Price = 0,
                    Quantity = operInner.Prihod
                };
                var cnt = operInner.Prihod;
                foreach (var cst in costs2)
                {
                    if (cnt <= 0) break;
                    cnt = cnt - cst.Quantity;
                    cst.Quantity = cnt > 0 ? 0 : cst.Quantity - cnt;
                    nomCost.Price = (nomCost.Price * nomCost.Quantity +
                                     myRates.GetRate(cst.Currency.DocCode, operInner.CurrencyDC, date) * cst.Price *
                                     (cnt > 0 ? cst.Quantity : cst.Quantity - cnt)) /
                                    (nomCost.Quantity + (cnt > 0 ? cst.Quantity : cst.Quantity - cnt));
                }
                CalculateCosts.Add(nomCost);
            }
            var oper =
                PrihodData.Where(
                    _ => _.NomenklDC == nomDC && _.SkladDC == sklDC && _.CurrencyDC == crsDC
                         && _.DocDate == date).ToList();
            if (oper.Any(_ => _.OperType == (int) NomenklOperationType.InnerNaklad))
                foreach (var item in oper.Where(_ => _.OperType == (int) NomenklOperationType.InnerNaklad))
                {
                    var d = CalcOne(nomDC, item.SklaOtprDC, crsDC, date, item);
                    item.Price = d.Price;
                }
            var last = CalculateCosts.SingleOrDefault(
                _ =>
                    _.Warehouse.DocCode == sklDC && _.Nomenkl.DocCode == nomDC && _.Currency.DocCode == crsDC &&
                    _.Date ==
                    CalculateCosts.Where(
                            s =>
                                s.Warehouse.DocCode == sklDC && s.Nomenkl.DocCode == nomDC &&
                                s.Currency.DocCode == crsDC &&
                                s.Date < date)
                        .Select(s => s.Date).Max());
            var qOut = RashodData.Where(_ =>
                _.SkladDC == sklDC && _.NomenklDC == nomDC && _.CurrencyDC == crsDC && _.DocDate > last.Date &&
                _.DocDate < date).Sum(_ => _.Rashod);
            ret = new NomenklCosts
            {
                Nomenkl = MainReferences.GetNomenkl(nomDC),
                Warehouse = MainReferences.Warehouses[sklDC],
                Currency = MainReferences.Currencies[crsDC],
                Date = date,
                Price =
                    // ReSharper disable once PossibleNullReferenceException
                    ((last.Quantity - qOut) * last.Price + oper.Sum(_ => _.Prihod * (_.Price + _.PriceNaklad))) /
                    (last.Quantity - qOut + oper.Sum(_ => _.Prihod)),
                Quantity =
                    last.Quantity + oper.Sum(_ => _.Prihod) -
                    RashodData.Where(_ => _.NomenklDC == nomDC && _.SkladDC == sklDC && _.CurrencyDC == crsDC
                                          && _.DocDate == date).Sum(_ => _.Rashod)
            };
            var cc = CalculateCosts.FirstOrDefault(
                _ =>
                    _.Warehouse.DocCode == ret.Warehouse.DocCode && _.Nomenkl.DocCode == ret.Nomenkl.DocCode
                                                                 && _.Currency.DocCode == ret.Currency.DocCode &&
                                                                 _.Date == ret.Date);
            if (cc != null)
            {
                cc.Price = ret.Price;
                cc.Quantity = ret.Quantity;
            }
            else
            {
                CalculateCosts.Add(ret);
            }
            return ret;
        }

        public void CalcAllPrice(DateTime? date)
        {
            if (date == null || date == DateTime.MinValue)
                foreach (var nom in MainReferences.ALLNomenkls.Values)
                foreach (var skl in MainReferences.Warehouses.Values)
                foreach (var crs in MainReferences.Currencies.Values)
                    CalculateCosts.Add(new NomenklCosts
                    {
                        Currency = crs,
                        Date = DateTime.MinValue,
                        Nomenkl = nom,
                        Warehouse = skl,
                        Price = 0,
                        Quantity = 0
                    });
            LoadOperation2();
            //foreach (var item in PrihodData)
            //    CalcOne(item);

            //SaveResult();
        }

        private void SaveResult()
        {
            using (var tnx = new TransactionScope())
            {
                try
                {
                    using (var ctx = GlobalOptions.GetTestEntities())
                    {
                        var d = ctx.NOM_SKLAD_CURRENCY_PRICE.ToList();
                        foreach (var item in d)
                            ctx.NOM_SKLAD_CURRENCY_PRICE.Remove(item);
                        ctx.SaveChanges();
                        foreach (var item in CalculateCosts.Where(_ => _.Date != DateTime.MinValue))
                            ctx.NOM_SKLAD_CURRENCY_PRICE.Add(new NOM_SKLAD_CURRENCY_PRICE
                            {
                                Quantity = item.Quantity,
                                Id = Guid.NewGuid(),
                                Cost = item.Price,
                                CostDate = item.Date,
                                CurrencyId = item.Currency.Id,
                                NomenklId = item.Nomenkl.Id,
                                SkladId = item.Warehouse.Id,
                                Note = $"Расчет произведен {DateTime.Now}"
                            });
                        ctx.SaveChanges();
                        tnx.Complete();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private class NomenklPirhod
        {
            public decimal CurrencyDC { set; get; }
            public decimal Price { set; get; }
            public decimal PriceNaklad { set; get; }
            public DateTime DocDate { set; get; }
            public decimal SkladDC { set; get; }
            public decimal SklaOtprDC { set; get; }
            public decimal NomenklDC { set; get; }
            public decimal Prihod { set; get; }
            public int OperType { set; get; }
        }

        private class NomenklRashod
        {
            public int OperType { set; get; }
            public DateTime DocDate { set; get; }
            public decimal SkladDC { set; get; }
            public decimal SklaPolDC { set; get; }
            public decimal CurrencyDC { set; get; }
            public decimal NomenklDC { set; get; }
            public decimal Rashod { set; get; }
        }
    }
}
