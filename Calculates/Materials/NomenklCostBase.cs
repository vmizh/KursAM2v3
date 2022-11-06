using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core;
using JetBrains.Annotations;
using KursDomain;
using KursDomain.References;
using Warehouse = KursDomain.Documents.NomenklManagement.Warehouse;

namespace Calculates.Materials
{
    public abstract class NomenklCostBase
    {
        protected readonly Dictionary<DateTime, NomenklCalcCostOperation> SaveNomPrice =
            new Dictionary<DateTime, NomenklCalcCostOperation>();

        protected List<Nomenkl> NomenklsForCalc = new List<Nomenkl>();
        public bool IsSave { set; get; } = false;

        public abstract ObservableCollection<NomenklCalcCostOperation> GetOperations(decimal nomDC,
            bool isCalcOnly = true);

        public abstract List<NomenklCalcCostOperation> Calc(ObservableCollection<NomenklCalcCostOperation> operList);
        public abstract void Save(IEnumerable<NomenklCalcCostOperation> operList);

        /// <summary>
        ///     Возвращает все операци, без расчета, опираясь на уже существующий расчет
        /// </summary>
        /// <param name="nomDC"></param>
        /// <returns></returns>
        public ObservableCollection<NomenklCalcCostOperation> GetAllOperations(decimal nomDC)
        {
            var currentRowNumber = 0;
            var ret = new NomenklCost
            {
                Nomenkl = MainReferences.GetNomenkl(nomDC)
            };
            if (ret.Nomenkl.IsUsluga) return null;
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var dataTemp = (from td24 in ctx.TD_24.Include(_ => _.SD_24)
                            .Include(_ => _.SD_24.SD_201)
                            .Include(_ => _.TD_26)
                            .Include(_ => _.TD_26.SD_26)
                            .Include(_ => _.TD_84)
                            .Include(_ => _.TD_84.SD_84).Where(_ => _.DDT_NOMENKL_DC == nomDC)
                        join prc in ctx.NOM_PRICE on td24.DDT_NOMENKL_DC equals prc.NOM_DC
                        where prc.DATE == (from tprc in ctx.NOM_PRICE
                            where tprc.NOM_DC == nomDC && tprc.DATE <= td24.SD_24.DD_DATE
                            select tprc.DATE).Max()
                        select
                            new
                            {
                                Note = td24.SD_24.DD_NOTES + (td24.TD_26 != null
                                                               ? " " + td24.TD_26.SFT_TEXT
                                                               : null)
                                                           + (td24.TD_84 != null
                                                               ? " " + td24.TD_84.SFT_TEXT
                                                               : null),
                                PriceWONaklad = prc.PRICE_WO_NAKLAD,
                                Price = prc.PRICE,
                                Date = td24.SD_24.DD_DATE,
                                NomenklDC = nomDC,
                                QuantityOut = td24.DDT_KOL_RASHOD,
                                QuantityIn = td24.DDT_KOL_PRIHOD,
                                KontrPolDC = td24.SD_24.DD_KONTR_POL_DC,
                                KontrOtprDC = td24.SD_24.DD_KONTR_OTPR_DC,
                                OperName = td24.SD_24.SD_201.D_NAME,
                                OperCode = (td24.SD_24.DD_VOZVRAT ?? 0) == 1 ? 25 : td24.SD_24.SD_201.D_OP_CODE,
                                DocPrice = (decimal) (td24.SD_24.DD_TYPE_DC == 2010000005 ? td24.DDT_TAX_CENA : 0),
                                SkladIn = td24.SD_24.DD_SKLAD_POL_DC,
                                SkladOut = td24.SD_24.DD_SKLAD_OTPR_DC,
                                SummaIn = (decimal)
                                    (td24.SD_24.DD_TYPE_DC == 2010000005
                                        ? td24.DDT_TAX_CENA * td24.DDT_KOL_PRIHOD
                                        : (td24.TD_26 != null ? td24.TD_26.SFT_ED_CENA ?? 0 : 0) * td24.DDT_KOL_PRIHOD),
                                SummaInWithNaklad = (decimal) (td24.SD_24.DD_TYPE_DC == 2010000005
                                    ? td24.DDT_TAX_CENA * td24.DDT_KOL_PRIHOD
                                    : (td24.TD_26 != null
                                        ? td24.TD_26.SFT_ED_CENA + td24.TD_26.SFT_SUMMA_NAKLAD / td24.TD_26.SFT_KOL
                                        : 0) * td24.DDT_KOL_PRIHOD),
                                TovarDocDC = td24.DOC_CODE,
                                SFRashodRow = td24.TD_84,
                                //ReSharper disable once MergeConditionalExpression
                                SFPrihodRow = td24.TD_26,
                                TovarDocHead = td24.SD_24
                            }).ToList();
                    if (dataTemp.Count != 0)
                    {
                        var data =
                            dataTemp.OrderBy(_ => _.NomenklDC)
                                .ThenBy(_ => _.Date)
                                .ThenBy(_ => _.QuantityOut);
                        foreach (var d in data)
                        {
                            currentRowNumber++;
                            var oper = new NomenklCalcCostOperation
                            {
                                RowNumber = currentRowNumber,
                                NomenklDC = nomDC,
                                Note = d.Note,
                                CalcPrice = d.Price,
                                CalcPriceNaklad = d.PriceWONaklad,
                                DocDate = d.Date,
                                KontragentIn = (Kontragent) GlobalOptions.ReferencesCache.GetKontragent(d.KontrPolDC),
                                KontragentOut = (Kontragent) GlobalOptions.ReferencesCache.GetKontragent(d.KontrOtprDC),
                                OperationName = d.OperName,
                                OperCode = d.OperCode,
                                QuantityIn = d.QuantityIn,
                                QuantityOut = d.QuantityOut,
                                // ReSharper disable once PossibleInvalidOperationException
                                DocPrice = d.DocPrice,
                                Naklad = 0,
                                SkladIn = d.SkladIn != null ? MainReferences.Warehouses[d.SkladIn.Value] : null,
                                SkladOut = d.SkladOut != null ? MainReferences.Warehouses[d.SkladOut.Value] : null,
                                SummaIn = d.OperCode == 13 ? d.PriceWONaklad * d.QuantityIn : d.SummaIn,
                                SummaInWithNaklad = d.OperCode == 13 ? d.Price * d.QuantityIn : d.SummaInWithNaklad,
                                SummaOut = d.Price * d.QuantityOut,
                                SummaOutWithNaklad = d.Price * d.QuantityOut,
                                QuantityNakopit = 0,
                                TovarDocDC = d.TovarDocDC
                            };
                            if (d.SFPrihodRow != null)
                            {
                                oper.FinDocumentDC = d.SFPrihodRow.DOC_CODE;
                                oper.FinDocument =
                                    $"С/ф поставщика №{d.SFPrihodRow.SD_26.SF_IN_NUM}/{d.SFPrihodRow.SD_26.SF_POSTAV_NUM} от {d.SFPrihodRow.SD_26.SF_POSTAV_DATE.ToShortDateString()}";
                                oper.Naklad = (d.SFPrihodRow.SFT_SUMMA_NAKLAD ?? 0) / d.SFPrihodRow.SFT_KOL;
                                // ReSharper disable once PossibleInvalidOperationException
                                oper.DocPrice = (decimal) d.SFPrihodRow.SFT_ED_CENA;
                            }

                            if (d.SFRashodRow != null)
                            {
                                oper.FinDocumentDC = d.SFRashodRow.SD_84.DOC_CODE;
                                oper.FinDocument =
                                    $"С/ф клиенту №{d.SFRashodRow.SD_84.SF_IN_NUM}/{d.SFRashodRow.SD_84.SF_OUT_NUM} от {d.SFRashodRow.SD_84.SF_DATE.ToShortDateString()}";
                            }

                            switch (oper.OperCode)
                            {
                                case 1:
                                    oper.TovarDocument = "Приходный складской ордер ";
                                    break;
                                case 2:
                                    oper.TovarDocument = "Расходный складской ордер ";
                                    break;
                                case 5:
                                    oper.TovarDocument = "Инвентаризационная ведомость ";
                                    break;
                                case 7:
                                    oper.TovarDocument = "Акт приемки готовой продукции ";
                                    break;
                                case 8:
                                    oper.TovarDocument = "Акт разукомплектации готовой продукции ";
                                    break;
                                case 9:
                                    oper.TovarDocument = "Акт списания материалов ";
                                    break;
                                case 12:
                                    oper.TovarDocument = "Расходная накладная (без требования) ";
                                    break;
                                case 13:
                                    oper.TovarDocument = "Накладная на внутренее перемещение ";
                                    break;
                                case 18:
                                    oper.TovarDocument = "Продажа за наличный расчет ";
                                    break;
                                case 25:
                                    oper.TovarDocument = "Возврат товара ";
                                    break;
                            }

                            oper.TovarDocument +=
                                $"№{d.TovarDocHead.DD_IN_NUM}/{d.TovarDocHead.DD_EXT_NUM} от {d.TovarDocHead.DD_DATE.ToShortDateString()}";
                            ret.Operations.Add(oper);
                        }
                    }

                    var dataTransfer = ctx.NomenklTransferRow.Include(_ => _.NomenklTransfer)
                        .Where(_ => (_.NomenklInDC == nomDC || _.NomenklOutDC == nomDC) && _.IsAccepted).ToList();
                    foreach (var d in dataTransfer)
                    {
                        currentRowNumber++;
                        if (d.NomenklInDC == d.NomenklOutDC)
                        {
                            var newTransOper = new NomenklCalcCostOperation
                            {
                                RowNumber = currentRowNumber,
                                Note = d.Note,
                                CalcPrice = 0,
                                CalcPriceNaklad = 0,
                                DocDate = d.NomenklTransfer.Date,
                                KontragentIn = null,
                                KontragentOut = null,
                                OperationName = "Валютный перевод товара",
                                OperCode = 19,
                                QuantityIn = d.Quantity,
                                QuantityOut = d.Quantity,
                                DocPrice = d.PriceIn,
                                Naklad = 0,
                                // ReSharper disable once PossibleInvalidOperationException
                                SkladIn = MainReferences.Warehouses[d.NomenklTransfer.SkladDC.Value],
                                SkladOut = MainReferences.Warehouses[d.NomenklTransfer.SkladDC.Value],
                                SummaIn = d.PriceIn * d.Quantity,
                                SummaInWithNaklad = d.PriceIn * d.Quantity,
                                SummaOut = d.PriceOut * d.Quantity,
                                SummaOutWithNaklad = d.PriceOut * d.Quantity,
                                QuantityNakopit = 0,
                                TovarDocDC = -1,
                                NomenklDC = nomDC
                            };
                            ret.Operations.Add(newTransOper);
                        }
                        else
                        {
                            if (d.NomenklInDC == nomDC)
                            {
                                var newTransOper = new NomenklCalcCostOperation
                                {
                                    RowNumber = currentRowNumber,
                                    Note = d.Note,
                                    CalcPrice = 0,
                                    CalcPriceNaklad = 0,
                                    DocDate = d.NomenklTransfer.Date,
                                    KontragentIn = null,
                                    KontragentOut = null,
                                    OperationName = "Валютный перевод товара",
                                    OperCode = 19,
                                    QuantityIn = d.Quantity,
                                    QuantityOut = 0,
                                    DocPrice = d.PriceIn,
                                    Naklad = 0,
                                    // ReSharper disable once PossibleInvalidOperationException
                                    SkladIn = MainReferences.Warehouses[d.NomenklTransfer.SkladDC.Value],
                                    SkladOut = MainReferences.Warehouses[d.NomenklTransfer.SkladDC.Value],
                                    SummaIn = d.PriceIn * d.Quantity,
                                    SummaInWithNaklad = d.PriceIn * d.Quantity,
                                    SummaOut = 0,
                                    SummaOutWithNaklad = 0,
                                    QuantityNakopit = 0,
                                    TovarDocDC = -1,
                                    NomenklDC = d.NomenklInDC
                                };
                                ret.Operations.Add(newTransOper);
                            }

                            if (d.NomenklOutDC == nomDC)
                            {
                                var newTransOper = new NomenklCalcCostOperation
                                {
                                    RowNumber = currentRowNumber,
                                    Note = d.Note,
                                    CalcPrice = 0,
                                    CalcPriceNaklad = 0,
                                    DocDate = d.NomenklTransfer.Date,
                                    KontragentIn = null,
                                    KontragentOut = null,
                                    OperationName = "Валютный перевод товара",
                                    OperCode = 19,
                                    QuantityIn = 0,
                                    QuantityOut = d.Quantity,
                                    DocPrice = d.PriceOut,
                                    Naklad = 0,
                                    SkladIn = MainReferences.Warehouses[d.NomenklTransfer.SkladDC.Value],
                                    SkladOut = MainReferences.Warehouses[d.NomenklTransfer.SkladDC.Value],
                                    SummaIn = 0,
                                    SummaInWithNaklad = 0,
                                    SummaOut = d.PriceOut * d.Quantity,
                                    SummaOutWithNaklad = d.PriceOut * d.Quantity,
                                    QuantityNakopit = 0,
                                    TovarDocDC = -1,
                                    NomenklDC = d.NomenklOutDC
                                };
                                ret.Operations.Add(newTransOper);
                            }
                        }
                    }
                }

                foreach (var d in ret.Operations) d.QuantityNakopit += d.QuantityIn - d.QuantityOut;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);
            }

            return ret.Operations;
        }

        public static List<NomenklStoreRemainItem> LoadRemains(DateTime date, [NotNull] Warehouse sklad)
        {
            var ret = new List<NomenklStoreRemainItem>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var noms =
                    (from np in ctx.NOM_PRICE.Where(_ => _.DATE <= date).Select(_ => _.NOM_DC).Distinct().ToList()
                        let dmax = ctx.NOM_PRICE.Where(_ => _.NOM_DC == np && _.DATE <= date).Select(_ => _.DATE).Max()
                        select ctx.NOM_PRICE.FirstOrDefault(_ => _.NOM_DC == np && _.DATE == dmax)
                        into nomprc
                        where nomprc != null && nomprc.NAKOPIT != 0
                        select nomprc).ToList();
                //var data = (from op in ctx.TD_24
                //    .Include(_ => _.SD_24)
                //    .Include(_ => _.SD_83)
                //    .Where(_ => _.SD_24.DD_SKLAD_OTPR_DC == sklad.DocCode ||
                //                _.SD_24.DD_SKLAD_POL_DC == sklad.DocCode)
                //    select op).ToList();
                //var noms = data.Select(_ => _.DDT_NOMENKL_DC).Distinct().ToList();
                var trans =
                    ctx.NomenklTransferRow.Include(_ => _.NomenklTransfer)
                        .Include(_ => _.SD_83)
                        .Include(_ => _.SD_831)
                        .Where(
                            _ =>
                                _.NomenklTransfer.SkladDC == sklad.DocCode);

                //foreach (var t in trans)
                //{
                //    if (noms.Any(_ => _ != t.NomenklInDC))
                //    {
                //        noms.Add(t.NomenklInDC);
                //    }
                //    if (noms.Any(_ => _ != t.NomenklOutDC))
                //    {
                //        noms.Add(t.NomenklOutDC);
                //    }
                //}
                var data = (from op in ctx.TD_24
                        .Include(_ => _.SD_24)
                        .Include(_ => _.SD_83)
                        .Where(_ => _.SD_24.DD_SKLAD_OTPR_DC == sklad.DocCode ||
                                    _.SD_24.DD_SKLAD_POL_DC == sklad.DocCode)
                    select op).ToList();
                foreach (var n in noms)
                {
                    var q = data.Where(_ => _.DDT_NOMENKL_DC == n.NOM_DC && _.SD_24.DD_SKLAD_POL_DC == sklad.DocCode)
                                .Sum(_ => _.DDT_KOL_PRIHOD)
                            -
                            data.Where(_ => _.DDT_NOMENKL_DC == n.NOM_DC && _.SD_24.DD_SKLAD_OTPR_DC == sklad.DocCode)
                                .Sum(_ => _.DDT_KOL_RASHOD);
                    var q11 = trans.Where(_ => _.NomenklInDC == n.NOM_DC && _.NomenklTransfer.SkladDC == sklad.DocCode);
                    var q1 = !q11.Any() ? 0 : q11.Sum(_ => _.Quantity);
                    var q21 = trans.Where(_ =>
                        _.NomenklOutDC == n.NOM_DC && _.NomenklTransfer.SkladDC == sklad.DocCode);
                    var q2 = !q21.Any() ? 0 : q21.Sum(_ => _.Quantity);
                    q += q1 - q2;
                    if (q > 0) ret.Add(new NomenklStoreRemainItem());
                }
            }

            return ret;
        }

        //public static List<NomenklStoreRemainItem> LoadStoreRemains(DateTime date)
        //{
        //    var ret = new List<NomenklStoreRemainItem>();
        //    var sql = string.Format("SELECT tab.NomenklDC, tab.NomCurrencyDC, tab.NomenklName, tab.SkladDC, " +
        //                                "SUM(tab.Prihod) AS Prihod, SUM(tab.Rashod) AS Rashod, round((SUM(tab.Prihod) - SUM(tab.Rashod)),2) * round(isnull(p.PRICE_WO_NAKLAD,0),2) AS Summa " +
        //                                " FROM(SELECT " +
        //                                "                                 T24.DDT_NOMENKL_DC as NomenklDC, " +
        //                                "                                s83.NOM_SALE_CRS_DC as NomCurrencyDC, " +
        //                                "                              s83.NOM_NAME + '(' + s83.NOM_NOMENKL + ')' as NomenklName, " +
        //                                "                              S24.DD_SKLAD_POL_DC as SkladDC, " +
        //                                "                             SUM(T24.DDT_KOL_PRIHOD) as Prihod, " +
        //                                "                            cast(0 as numeric(18,4)) as Rashod " +
        //                                "                          FROM TD_24 T24 " +
        //                                "                         INNER JOIN SD_24 S24 " +
        //                                "                         ON S24.DOC_CODE = T24.DOC_CODE " +
        //                                "                                INNER JOIN SD_83 S83 " +
        //                                "                                ON S83.DOC_CODE = T24.DDT_NOMENKL_DC " +
        //                                "                              WHERE DD_DATE <= '{0}' AND S24.DD_SKLAD_POL_DC IS NOT null " +
        //                                "                             GROUP BY T24.DDT_NOMENKL_DC, s83.NOM_SALE_CRS_DC, s83.NOM_NAME + '(' + s83.NOM_NOMENKL + ')', " +
        //                                "                                     S24.DD_SKLAD_POL_DC " +
        //                                "UNION ALL " +
        //                                " SELECT T24.DDT_NOMENKL_DC as NomenklDC, " +
        //                                "                                   s83.NOM_SALE_CRS_DC as NomCurrencyDC, " +
        //                                "                                 s83.NOM_NAME + '(' + s83.NOM_NOMENKL + ')' as NomenklName, " +
        //                                "                                 S24.DD_SKLAD_OTPR_DC as SkladDC, " +
        //                                "                                cast(0 as numeric(18,4)) as Prihod, " +
        //                                "                               SUM(DDT_KOL_RASHOD) as Rashod " +
        //                                "                             FROM TD_24 T24 " +
        //                                "                            INNER JOIN SD_24 S24 " +
        //                                "                            ON S24.DOC_CODE = T24.DOC_CODE " +
        //                                "                          INNER JOIN SD_83 S83 " +
        //                                "                          ON S83.DOC_CODE = T24.DDT_NOMENKL_DC " +
        //                                "                        WHERE DD_DATE <= '{0}' AND S24.DD_SKLAD_OTPR_DC IS NOT null " +
        //                                "                       GROUP BY T24.DDT_NOMENKL_DC, s83.NOM_SALE_CRS_DC, s83.NOM_NAME + '(' + s83.NOM_NOMENKL + ')', " +
        //                                "                               S24.DD_SKLAD_OTPR_DC) tab " +
        //                                "                   LEFT OUTER JOIN NOM_PRICE p " +
        //                                "                     ON p.NOM_DC = tab.NomenklDC " +
        //                                "                    AND p.DATE = (SELECT " +
        //                                "                     MAX(pp.DATE) " +
        //                                "                  FROM NOM_PRICE pp " +
        //                                "                 WHERE pp.NOM_DC = tab.NomenklDC AND pp.DATE <= '{0}') " +
        //                                "               GROUP BY tab.NomenklDC, tab.NomCurrencyDC, tab.NomenklName, tab.SkladDC,p.PRICE_WO_NAKLAD HAVING SUM(tab.Prihod) - SUM(tab.Rashod) != 0",
        //                CustomFormat.DateToString(date));
        //    //var data = GlobalOptions.GetEntities().Database.SqlQuery<NomenklStoreRemainItem>(sql).ToList();
        //    return ret;
        //}

        public static List<NomenklStoreRemainItem> LoadStoreRemains(DateTime date, [NotNull] Warehouse sklad)
        {
            var ret = new List<NomenklStoreRemainItem>();
            return ret;
        }
    }
}
