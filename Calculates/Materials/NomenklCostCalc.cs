using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Transactions;
using Core;
using Data;
using KursDomain;
using KursDomain.References;

namespace Calculates.Materials
{
    /// <summary>
    ///     Класс для получения операций движения
    ///     товаров и расчета себестоимости
    /// </summary>
    public class NomenklCostCalc
    {
        protected static readonly Dictionary<DateTime, NomenklCalcCostOperation> SaveNomPrice =
            new Dictionary<DateTime, NomenklCalcCostOperation>();

        public static void CalcAll()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var nomDCs = ctx.NOMENKL_RECALC.Select(_ => _.NOM_DC).Distinct()
                    .ToList();
                var noms = nomDCs.Select(MainReferences.GetNomenkl).Where(newNom => !newNom.IsUsluga).ToList();
                if (!noms.Any()) return;
                foreach (var op in noms.Select(_ => _.DocCode).Select(GetOperations)) Save(op);
            }
        }

        /// <summary>
        ///     Возвращает все операци, без расчета, опираясь на уже существующий расчет
        /// </summary>
        /// <param name="nomDC"></param>
        /// <returns></returns>
        public static NomenklCost GetAllOperations(decimal nomDC)
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
                    var dataTemp = (from d in ctx.TD_24
                            .Include(_ => _.SD_24)
                            .Include(_ => _.SD_24.SD_201)
                            .Include(_ => _.TD_26)
                            .Include(_ => _.TD_26.SD_26)
                            .Include(_ => _.TD_84)
                            .Include(_ => _.TD_84.SD_84).Where(_ => _.DDT_NOMENKL_DC == nomDC)
                        join prc in ctx.NOM_PRICE on d.DDT_NOMENKL_DC equals prc.NOM_DC
                        select
                            new
                            {
                                Note = d.SD_24.DD_NOTES + (d.TD_26 != null
                                    ? " " + d.TD_26.SFT_TEXT
                                    : null
                                      + (d.TD_84 != null
                                          ? " " + d.TD_84.SFT_TEXT
                                          : null)),
                                PriceWONaklad = prc.PRICE_WO_NAKLAD,
                                Price = prc.PRICE,
                                Date = d.SD_24.DD_DATE,
                                NomenklDC = nomDC,
                                QuantityOut = d.DDT_KOL_RASHOD,
                                QuantityIn = d.DDT_KOL_PRIHOD,
                                KontrPolDC = d.SD_24.DD_KONTR_POL_DC,
                                KontrOtprDC = d.SD_24.DD_KONTR_OTPR_DC,
                                OperName = d.SD_24.SD_201.D_NAME,
                                OperCode = (d.SD_24.DD_VOZVRAT ?? 0) == 1 ? 25 : d.SD_24.SD_201.D_OP_CODE,
                                DocPrice = d.SD_24.DD_TYPE_DC == 2010000005 ? d.DDT_TAX_CENA : 0,
                                SkladInDC = d.SD_24.DD_SKLAD_POL_DC,
                                SkladOutDC = d.SD_24.DD_SKLAD_OTPR_DC,
                                //SkladIn = d.SD_24.DD_SKLAD_POL_DC != null
                                //    ? MainReferences.Warehouses[d.SD_24.DD_SKLAD_POL_DC.Value]
                                //    : null,
                                //SkladOut = d.SD_24.DD_SKLAD_OTPR_DC != null
                                //    ? MainReferences.Warehouses[d.SD_24.DD_SKLAD_OTPR_DC.Value]
                                //    : null,
                                SummaIn = d.SD_24.DD_TYPE_DC == 2010000005
                                    ? d.DDT_TAX_CENA * d.DDT_KOL_PRIHOD
                                    : d.TD_26 != null
                                        ? d.TD_26.SFT_ED_CENA ?? 0
                                        : 0 * d.DDT_KOL_PRIHOD,
                                SummaInWithNaklad = d.SD_24.DD_TYPE_DC == 2010000005
                                    ? d.DDT_TAX_CENA * d.DDT_KOL_PRIHOD
                                    : (d.TD_26 != null
                                        ? d.TD_26.SFT_ED_CENA + d.TD_26.SFT_SUMMA_NAKLAD / d.TD_26.SFT_KOL
                                        : 0) * d.DDT_KOL_PRIHOD,
                                TovarDocDC = d.DOC_CODE,
                                TovarRowCode = d.CODE,
                                SFPrihodRow = d.TD_26,
                                SFRashodRow = d.TD_84,
                                TovarDocHead = d.SD_24
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
                                DocPrice = (decimal) d.DocPrice,
                                Naklad = 0,
                                SkladIn = d.SkladInDC != null ? MainReferences.Warehouses[d.SkladInDC.Value] : null,
                                SkladOut = d.SkladOutDC != null ? MainReferences.Warehouses[d.SkladOutDC.Value] : null,
                                SummaIn = (decimal) (d.OperCode == 13 ? d.Price * d.QuantityIn : d.SummaIn),
                                SummaInWithNaklad = (decimal) d.SummaInWithNaklad,
                                SummaOut = d.Price * d.QuantityOut,
                                SummaOutWithNaklad = d.PriceWONaklad * d.QuantityOut,
                                QuantityNakopit = 0,
                                TovarDocDC = d.TovarDocDC,
                                TovarRowCode = d.TovarRowCode
                            };

                            if (d.SFPrihodRow != null)
                            {
                                oper.FinDocumentDC = d.SFPrihodRow.SD_26.DOC_CODE;
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

            return ret;
        }

        public static NomenklCost GetOperations(decimal nomDC)
        {
            var currentRowNumber = 0;
            var ret = new NomenklCost
            {
                Nomenkl = MainReferences.GetNomenkl(nomDC)
            };

            if (ret.Nomenkl.IsUsluga) return null;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var dataTemp = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Include(_ => _.TD_26)
                    .Include(_ => _.TD_26.SD_26)
                    .Include(_ => _.TD_84)
                    .Include(_ => _.TD_84.SD_84)
                    .Where(_ => _.DDT_NOMENKL_DC == nomDC).ToList();
                if (dataTemp.Count == 0) return ret;
                var data =
                    dataTemp.OrderBy(_ => _.DDT_NOMENKL_DC).ThenBy(_ => _.SD_24.DD_DATE).ThenBy(_ => _.DDT_KOL_RASHOD);
                foreach (var d in data)
                {
                    currentRowNumber++;
                    var oper = new NomenklCalcCostOperation
                    {
                        RowNumber = currentRowNumber,
                        Note = d.SD_24.DD_NOTES + " " + d.TD_26?.SFT_TEXT + d.TD_84?.SFT_TEXT,
                        CalcPrice = 0,
                        CalcPriceNaklad = 0,
                        DocDate = d.SD_24.DD_DATE,
                        KontragentIn =
                            (Kontragent) GlobalOptions.ReferencesCache.GetKontragent(d.SD_24.DD_KONTR_POL_DC),
                        KontragentOut =
                            (Kontragent) GlobalOptions.ReferencesCache.GetKontragent(d.SD_24.DD_KONTR_OTPR_DC),
                        OperationName = d.SD_24.SD_201.D_NAME,
                        OperCode = d.SD_24.SD_201.D_OP_CODE,
                        QuantityIn = d.DDT_KOL_PRIHOD,
                        QuantityOut = d.DDT_KOL_RASHOD,
                        DocPrice = 0,
                        Naklad = 0,
                        SkladIn =
                            d.SD_24.DD_SKLAD_POL_DC != null
                                ? MainReferences.Warehouses[d.SD_24.DD_SKLAD_POL_DC.Value]
                                : null,
                        SkladOut =
                            d.SD_24.DD_SKLAD_OTPR_DC != null
                                ? MainReferences.Warehouses[d.SD_24.DD_SKLAD_OTPR_DC.Value]
                                : null,
                        SummaIn = 0,
                        SummaInWithNaklad = 0,
                        SummaOut = 0,
                        SummaOutWithNaklad = 0,
                        QuantityNakopit = 0,
                        TovarDocDC = d.DOC_CODE,
                        TovarRowCode = d.CODE
                    };
                    if (d.TD_26 != null)
                    {
                        oper.FinDocumentDC = d.TD_26.SD_26.DOC_CODE;
                        oper.FinDocument =
                            $"С/ф поставщика №{d.TD_26.SD_26.SF_IN_NUM}/{d.TD_26.SD_26.SF_POSTAV_NUM} от {d.TD_26.SD_26.SF_POSTAV_DATE.ToShortDateString()}";
                        oper.Naklad = Math.Round((d.TD_26.SFT_SUMMA_NAKLAD ?? 0) / d.TD_26.SFT_KOL, 2);
                        // ReSharper disable once PossibleInvalidOperationException
                        oper.DocPrice = Math.Round((decimal) d.TD_26.SFT_ED_CENA, 2);
                    }

                    if (d.TD_84 != null)
                    {
                        oper.FinDocumentDC = d.TD_84.SD_84.DOC_CODE;
                        oper.FinDocument =
                            $"С/ф клиенту №{d.TD_84.SD_84.SF_IN_NUM}/{d.TD_84.SD_84.SF_OUT_NUM} от {d.TD_84.SD_84.SF_DATE.ToShortDateString()}";
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
                    }

                    oper.TovarDocument +=
                        $"№{d.SD_24.DD_IN_NUM}/{d.SD_24.DD_EXT_NUM} от {d.SD_24.DD_DATE.ToShortDateString()}";
                    ret.Operations.Add(oper);
                }

                var dataTransferIn =
                    ctx.NomenklTransferRow.Include(_ => _.NomenklTransfer).Where(_ => _.NomenklInDC == nomDC).ToList();
                var dataTransferOut =
                    ctx.NomenklTransferRow.Include(_ => _.NomenklTransfer).Where(_ => _.NomenklOutDC == nomDC).ToList();
                foreach (var d in dataTransferIn)
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
                        OperCode = -1,
                        QuantityIn = d.Quantity,
                        QuantityOut = 0,
                        DocPrice = d.PriceIn,
                        Naklad = 0,
                        SkladIn = MainReferences.Warehouses[d.NomenklTransfer.SkladDC.Value],
                        SkladOut = null,
                        SummaIn = 0,
                        SummaInWithNaklad = 0,
                        SummaOut = 0,
                        SummaOutWithNaklad = 0,
                        QuantityNakopit = 0,
                        TovarDocDC = -1
                    };
                    ret.Operations.Add(newTransOper);
                }

                foreach (var d in dataTransferOut)
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
                        OperCode = -1,
                        QuantityIn = 0,
                        QuantityOut = d.Quantity,
                        DocPrice = 0,
                        Naklad = 0,
                        SkladIn = null,
                        SkladOut = MainReferences.Warehouses[d.NomenklTransfer.SkladDC.Value],
                        SummaIn = 0,
                        SummaInWithNaklad = 0,
                        SummaOut = 0,
                        SummaOutWithNaklad = 0,
                        QuantityNakopit = 0,
                        TovarDocDC = -1
                    };
                    ret.Operations.Add(newTransOper);
                }

                Calc(ret);
                //Save(ret);
            }

            return ret;
        }

        public static void Calc(NomenklCost operList)
        {
            decimal startPrice = 0;
            decimal startPriceWithNaklad = 0;
            decimal quantityNakopit = 0;
            foreach (var d in operList.Operations)
            {
                if (d.QuantityIn > 0 && d.QuantityOut > 0)
                {
                    d.CalcPrice = Math.Round(startPrice, 2);
                    d.CalcPriceNaklad = Math.Round(startPriceWithNaklad, 2);
                    d.SummaIn = Math.Round(startPrice * d.QuantityIn, 2);
                    d.SummaInWithNaklad = Math.Round(startPriceWithNaklad * d.QuantityIn, 2);
                    d.SummaOut = Math.Round(d.QuantityOut * startPrice, 2);
                    d.SummaOutWithNaklad = Math.Round(d.QuantityOut * startPriceWithNaklad, 2);
                    d.QuantityNakopit = quantityNakopit;
                    continue;
                }

                if (d.QuantityIn > 0)
                {
                    var qOut = operList.Operations.Where(_ => _.DocDate == d.DocDate).Sum(_ => _.QuantityOut);
                    if (d.QuantityNakopit + d.QuantityIn > 0)
                    {
                        startPrice = Math.Round((startPrice * d.QuantityNakopit +
                                                 (d.DocPrice * d.QuantityIn - startPrice * qOut)) /
                                                (d.QuantityNakopit + d.QuantityIn), 2);
                        d.CalcPrice = startPrice;
                        startPriceWithNaklad = Math.Round((startPriceWithNaklad * d.QuantityNakopit +
                                                           ((d.DocPrice + d.Naklad) * d.QuantityIn -
                                                            d.CalcPriceNaklad * qOut)) /
                                                          (d.QuantityNakopit + d.QuantityIn), 2);
                        d.CalcPriceNaklad = startPriceWithNaklad;
                        d.SummaIn = Math.Round(d.DocPrice * d.QuantityIn, 2);
                        d.SummaInWithNaklad = Math.Round((d.DocPrice + d.Naklad) * d.QuantityIn, 2);
                    }

                    startPrice = d.CalcPrice;
                    startPriceWithNaklad = d.CalcPriceNaklad;
                    quantityNakopit += d.QuantityIn;
                    d.QuantityNakopit = quantityNakopit;
                }

                if (d.QuantityOut > 0)
                {
                    quantityNakopit -= d.QuantityOut;
                    d.QuantityNakopit = quantityNakopit;
                    d.SummaOut = Math.Round(d.QuantityOut * startPrice, 2);
                    d.SummaOutWithNaklad = Math.Round(d.QuantityOut * startPriceWithNaklad, 2);
                    d.DocPrice = Math.Round(startPrice, 2);
                    d.CalcPrice = Math.Round(startPrice, 2);
                    d.CalcPriceNaklad = Math.Round(startPriceWithNaklad, 2);
                }
            }
        }

        public static void Save(NomenklCost operList)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                try
                {
                    using (var tnx = new TransactionScope())
                    {
                        decimal nomDC = 0;
                        //var pdata = ctx.NOM_PRICE.Where(operList.Select(_ => _.NomenklDC).Contains(d => d.NOM_DC).ToList();
                        foreach (
                            var pn in
                            ctx.NOM_PRICE.Where(
                                pn => operList.Operations.Select(_ => _.NomenklDC).Contains(pn.NOM_DC)))
                            ctx.NOM_PRICE.Remove(pn);
                        SaveNomPrice.Clear();
                        var dates = operList.Operations.Select(_ => _.DocDate).Distinct();
                        foreach (var dt in dates)
                        {
                            var rn = operList.Operations.Where(_ => _.DocDate == dt).Select(_ => _.RowNumber).Max();
                            SaveNomPrice.Add(dt, operList.Operations.Single(_ => _.DocDate == dt && _.RowNumber == rn));
                        }

                        foreach (var d in SaveNomPrice.Values)
                        {
                            nomDC = d.NomenklDC;
                            ctx.NOM_PRICE.Add(new NOM_PRICE
                            {
                                ID = Guid.NewGuid().ToString().Replace("-", string.Empty).ToUpper(),
                                NOM_DC = d.NomenklDC,
                                DATE = d.DocDate,
                                CALC_INFO = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                                AVG = 0,
                                FIFO = 0,
                                LIFO = 0,
                                PRICE = d.CalcPriceNaklad,
                                KOL_IN = d.QuantityIn,
                                KOL_OUT = d.QuantityOut,
                                NAKOPIT = d.QuantityNakopit,
                                PRICE_WO_NAKLAD = d.CalcPrice,
                                SUM_IN = d.SummaInWithNaklad,
                                SUM_OUT = d.SummaInWithNaklad,
                                SUM_IN_WO_NAKLAD = d.SummaIn,
                                SUM_OUT_WO_NAKLAD = d.SummaOut
                            });
                        }

                        var s = $"DELETE FROM NOMENKL_RECALC WHERE NOM_DC={nomDC}";
                        ctx.Database.ExecuteSqlCommand(s);
                        Console.WriteLine($"Сохранение для {nomDC}");
                        ctx.SaveChanges();
                        tnx.Complete();
                    }
                }
                catch (Exception ex)
                {
                    var errText = new StringBuilder(ex.Message);
                    if (ex.InnerException != null)
                    {
                        errText.Append("\n Внутрення ошибка:\n");
                        errText.Append(ex.InnerException.Message);
                    }

                    Console.WriteLine(errText);
                }
            }
        }
    }
}
