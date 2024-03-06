using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Data;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;

namespace Calculates.Materials
{
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public class NomenklCostMediumSliding : NomenklCostBase
    {
        // ReSharper disable once InconsistentNaming
        public ALFAMEDIAEntities context;

        public NomenklCostMediumSliding(ALFAMEDIAEntities ctx)
        {
            context = ctx;
        }

        public override ObservableCollection<NomenklCalcCostOperation> GetOperations(decimal nomDC,
            bool isCalOnly = true)
        {
            var currentRowNumber = 0;
            var ret = new NomenklCost
            {
                Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(nomDC) as Nomenkl
            };
            if (ret.Nomenkl.IsUsluga) return null;
            try
            {
                var dataTemp = context.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Include(_ => _.TD_26)
                    .Include(_ => _.TD_26.SD_26)
                    .Include(_ => _.TD_84)
                    .Include(_ => _.TD_84.SD_84)
                    .Where(_ => _.SD_24.DD_TYPE_DC != 2010000014 && _.DDT_NOMENKL_DC == nomDC).ToList();
                if (dataTemp.Count != 0)
                {
                    var data =
                        dataTemp.OrderBy(_ => _.DDT_NOMENKL_DC)
                            .ThenBy(_ => _.SD_24.DD_DATE)
                            .ThenBy(_ => _.DDT_KOL_RASHOD);
                    foreach (var d in data)
                    {
                        currentRowNumber++;
                        var oper = new NomenklCalcCostOperation
                        {
                            RowNumber = currentRowNumber,
                            NomenklDC = nomDC,
                            Note = d.SD_24.DD_NOTES + " " + d.TD_26?.SFT_TEXT + d.TD_84?.SFT_TEXT,
                            CalcPrice = 0,
                            CalcPriceNaklad = 0,
                            DocDate = d.SD_24.DD_DATE,
                            KontragentIn =  
                                (Kontragent) GlobalOptions.ReferencesCache.GetKontragent(d.SD_24.DD_KONTR_POL_DC),
                            KontragentOut =
                                (Kontragent) GlobalOptions.ReferencesCache.GetKontragent(d.SD_24.DD_KONTR_OTPR_DC),
                            OperationName = d.SD_24.SD_201.D_NAME,
                            OperCode = (d.SD_24.DD_VOZVRAT ?? 0) == 1 ? 25 : d.SD_24.SD_201.D_OP_CODE,
                            QuantityIn = d.DDT_KOL_PRIHOD,
                            QuantityOut = d.DDT_KOL_RASHOD,
                            // ReSharper disable once PossibleInvalidOperationException
                            DocPrice = (decimal) (d.SD_24.DD_TYPE_DC == 2010000005 ? d.DDT_TAX_CENA : 0),
                            Naklad = 0,
                            SkladIn =
                                GlobalOptions.ReferencesCache.GetWarehouse(d.SD_24.DD_SKLAD_POL_DC) as Warehouse,
                            SkladOut =
                                GlobalOptions.ReferencesCache.GetWarehouse(d.SD_24.DD_SKLAD_OTPR_DC) as Warehouse,
                            SummaIn =
                                (decimal) ((d.SD_24.DD_VOZVRAT ?? 0) == 1
                                    ? d.DDT_TAX_CRS_CENA ?? 0
                                    : d.TD_26 != null
                                        ? d.SD_24.DD_TYPE_DC == 2010000005
                                            ? d.DDT_TAX_CENA * d.DDT_KOL_PRIHOD
                                            : d.TD_26.SFT_SUMMA_K_OPLATE /
                                            (d.TD_26.SFT_KOL == 0 ? 1 : d.TD_26.SFT_KOL) * d.DDT_KOL_PRIHOD
                                        : 0),
                            SummaInWithNaklad =
                                (decimal) ((d.SD_24.DD_VOZVRAT ?? 0) == 1
                                    ? d.DDT_TAX_CRS_CENA ?? 0
                                    : d.TD_26 != null
                                        ? d.SD_24.DD_TYPE_DC == 2010000005
                                            ? d.DDT_TAX_CENA * d.DDT_KOL_PRIHOD
                                            : (d.TD_26.SFT_SUMMA_K_OPLATE + d.TD_26?.SFT_SUMMA_NAKLAD /
                                                (d.TD_26.SFT_KOL == 0 ? 1 : d.TD_26.SFT_KOL)) * d.DDT_KOL_PRIHOD
                                        : 0),
                            SummaOut = 0,
                            SummaOutWithNaklad = 0,
                            QuantityNakopit = 0,
                            TovarDocDC = d.DOC_CODE
                        };
                        if (d.TD_26 != null)
                        {
                            oper.FinDocumentDC = d.TD_26.SD_26.DOC_CODE;
                            oper.FinDocument =
                                $"С/ф поставщика №{d.TD_26.SD_26.SF_IN_NUM}/{d.TD_26.SD_26.SF_POSTAV_NUM} от {d.TD_26.SD_26.SF_POSTAV_DATE.ToShortDateString()}";
                            oper.Naklad = (d.TD_26.SFT_SUMMA_NAKLAD ?? 0) / d.TD_26.SFT_KOL;
                            oper.DocPrice = (decimal) d.TD_26.SFT_ED_CENA;
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
                            case 25:
                                oper.TovarDocument = "Возврат товара ";
                                break;
                        }

                        oper.TovarDocument +=
                            $"№{d.SD_24.DD_IN_NUM}/{d.SD_24.DD_EXT_NUM} от {d.SD_24.DD_DATE.ToShortDateString()}";
                        if (oper.OperCode == 3 && oper.QuantityIn <= 0) continue;
                        ret.Operations.Add(oper);
                    }
                }

                var dataTransfer = context.NomenklTransferRow.Include(_ => _.NomenklTransfer)
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
                            DocPrice = 0,
                            Naklad = 0,
                            SkladIn =
                                GlobalOptions.ReferencesCache.GetWarehouse(d.NomenklTransfer.SkladDC) as Warehouse,
                            SkladOut =
                                GlobalOptions.ReferencesCache.GetWarehouse(d.NomenklTransfer.SkladDC) as Warehouse,
                            SummaIn = d.PriceIn * d.Quantity,
                            SummaInWithNaklad = (d.PriceIn + (d.NakladNewEdSumma ?? 0)) * d.Quantity,
                            SummaOut = d.PriceOut * d.Quantity,
                            SummaOutWithNaklad = d.PriceOut * d.Quantity,
                            QuantityNakopit = 0,
                            TovarDocDC = -1,
                            NomenklDC = nomDC,
                            Id = d.DocId
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
                                SkladIn =
                                    GlobalOptions.ReferencesCache.GetWarehouse(d.NomenklTransfer.SkladDC) as Warehouse,
                                SkladOut =
                                    GlobalOptions.ReferencesCache.GetWarehouse(d.NomenklTransfer.SkladDC) as Warehouse,
                                SummaIn = d.PriceIn * d.Quantity,
                                SummaInWithNaklad = (d.PriceIn + (d.NakladNewEdSumma ?? 0)) * d.Quantity,
                                SummaOut = 0,
                                SummaOutWithNaklad = 0,
                                QuantityNakopit = 0,
                                TovarDocDC = -1,
                                NomenklDC = d.NomenklInDC,
                                Id = d.DocId
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
                                DocPrice = 0,
                                Naklad = 0,
                                SkladIn =
                                    GlobalOptions.ReferencesCache.GetWarehouse(d.NomenklTransfer.SkladDC) as Warehouse,
                                SkladOut =
                                    GlobalOptions.ReferencesCache.GetWarehouse(d.NomenklTransfer.SkladDC) as Warehouse,
                                SummaIn = 0,
                                SummaInWithNaklad = 0,
                                SummaOut = d.PriceOut * d.Quantity,
                                SummaOutWithNaklad = d.PriceOut * d.Quantity,
                                QuantityNakopit = 0,
                                TovarDocDC = -1,
                                NomenklDC = d.NomenklOutDC,
                                Id = d.DocId
                            };
                            ret.Operations.Add(newTransOper);
                        }
                    }
                }

                var nomId = ((IDocGuid) GlobalOptions.ReferencesCache.GetNomenkl(nomDC)).Id;
                var nomcrsconverts = context.TD_26_CurrencyConvert
                    .Include(_ => _.TD_26)
                    .Include(_ => _.TD_26.SD_26)
                    .Where(_ => _.NomenklId == nomId).ToList();
                var nomcrsconverts2 = context.TD_26_CurrencyConvert
                    .Include(_ => _.TD_26)
                    .Include(_ => _.TD_26.SD_26)
                    .Where(_ => _.TD_26.SFT_NEMENKL_DC == nomDC).ToList();
                foreach (var d in nomcrsconverts)
                {
                    currentRowNumber++;
                    var newTransOper = new NomenklCalcCostOperation
                    {
                        RowNumber = currentRowNumber,
                        Note = d.Note,
                        CalcPrice = 0,
                        CalcPriceNaklad = 0,
                        DocDate = d.Date,
                        KontragentIn = null,
                        KontragentOut = null,
                        OperationName = "Валютный перевод товара(из счета)",
                        OperCode = 20,
                        QuantityIn = d.Quantity,
                        QuantityOut = 0,
                        DocPrice = 0,
                        Naklad = 0,
                        SkladIn = GlobalOptions.ReferencesCache.GetWarehouse(d.StoreDC) as Warehouse,
                        SkladOut = null,
                        SummaIn = d.Price * d.Quantity,
                        SummaInWithNaklad = d.PriceWithNaklad * d.Quantity,
                        SummaOut = 0,
                        SummaOutWithNaklad = 0,
                        QuantityNakopit = 0,
                        TovarDocDC = -1,
                        NomenklDC = nomDC,
                        FinDocumentDC = d.DOC_CODE

                    };
                    ret.Operations.Add(newTransOper);
                }

                foreach (var d in nomcrsconverts2)
                {
                    currentRowNumber++;
                    var newTransOper = new NomenklCalcCostOperation
                    {
                        RowNumber = currentRowNumber,
                        Note = d.Note,
                        CalcPrice = 0,
                        CalcPriceNaklad = 0,
                        DocDate = d.Date,
                        KontragentIn = null,
                        KontragentOut = null,
                        OperationName = "Валютный перевод товара(из счета)",
                        OperCode = 20,
                        QuantityIn = 0,
                        QuantityOut = d.Quantity,
                        DocPrice = 0,
                        Naklad = 0,
                        SkladIn = null,
                        SkladOut = GlobalOptions.ReferencesCache.GetWarehouse(d.StoreDC) as Warehouse,
                        SummaIn = 0,
                        SummaInWithNaklad = 0,
                        SummaOut = 0,
                        SummaOutWithNaklad = 0,
                        QuantityNakopit = 0,
                        TovarDocDC = -1,
                        NomenklDC = nomDC,
                        FinDocumentDC = d.DOC_CODE
                    };
                    ret.Operations.Add(newTransOper);
                }

                var spisanieData = from spisRow in context.AktSpisaniya_row
                    join spisHead in context.AktSpisaniyaNomenkl_Title on spisRow.Doc_Id equals spisHead.Id
                    join sign in context.DocumentSignatures.Where(_ => _.IsSign == true) on spisHead.Id equals sign
                        .DocId
                    where spisRow.Nomenkl_DC == nomDC
                    select new
                    {
                        Note = spisRow.Note + " " + spisHead.Note + " " + spisHead.Reason_Creation,
                        CalcPrice = 0,
                        CalcPriceNaklad = 0,
                        DocDate = spisHead.Date_Doc,
                        OperationName = "Акт списания товара",
                        OperCode = 72,
                        QuantityIn = 0,
                        QuantityOut = spisRow.Quantity,
                        DocPrice = 0,
                        Naklad = 0,
                        SkladOutDC = spisHead.Warehouse_DC,
                        SummaIn = 0,
                        SummaInWithNaklad = 0,
                        SummaOut = 0,
                        SummaOutWithNaklad = 0,
                        QuantityNakopit = 0,
                        TovarDocDC = -1,
                        NomenklDC = nomDC
                    };
                foreach (var d in spisanieData.ToList())
                {
                    currentRowNumber++;
                    var newspis = new NomenklCalcCostOperation
                    {
                        RowNumber = currentRowNumber,
                        Note = d.Note,
                        CalcPrice = 0,
                        CalcPriceNaklad = 0,
                        DocDate = d.DocDate,
                        KontragentIn = null,
                        KontragentOut = null,
                        OperationName = "Акт списания товара",
                        OperCode = 72,
                        QuantityIn = 0,
                        QuantityOut = d.QuantityOut,
                        DocPrice = 0,
                        Naklad = 0,
                        SkladIn = null,
                        SkladOut = GlobalOptions.ReferencesCache.GetWarehouse(d.SkladOutDC) as Warehouse,
                        SummaIn = 0,
                        SummaInWithNaklad = 0,
                        SummaOut = 0,
                        SummaOutWithNaklad = 0,
                        QuantityNakopit = 0,
                        TovarDocDC = -1,
                        NomenklDC = nomDC
                    };
                    ret.Operations.Add(newspis);
                }

                if (isCalOnly)
                {
                    var calc = Calc(ret.Operations);
                    return new ObservableCollection<NomenklCalcCostOperation>(calc);
                }

                return ret.Operations;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);
            }

            return null;
        }

        public override List<NomenklCalcCostOperation> Calc(ObservableCollection<NomenklCalcCostOperation> operList)
        {
            if (operList.Count == 0) return new List<NomenklCalcCostOperation>();
            var currentRowNumber = 1;
            var calcs = (from op in operList.ToList().OrderBy(_ => _.DocDate)
                group op by op.DocDate
                into dateGrp
                orderby dateGrp.Key
                select dateGrp).Select(d => new NomenklCalcCostOperation
            {
                RowNumber = currentRowNumber,
                NomenklDC = d.Max(_ => _.NomenklDC),
                Note = string.Empty,
                CalcPrice = 0,
                CalcPriceNaklad = 0,
                DocDate = d.Key,
                KontragentIn = null,
                KontragentOut = null,
                OperationName = string.Empty,
                OperCode = 0,
                QuantityIn = d.Sum(_ => _.QuantityIn),
                QuantityOut = d.Sum(_ => _.QuantityOut),
                DocPrice = 0,
                Naklad = d.Sum(_ => _.Naklad),
                SkladIn = null,
                SkladOut = null,
                SummaIn = d.Sum(_ => _.SummaIn),
                SummaInWithNaklad = d.Sum(_ => _.SummaInWithNaklad),
                SummaOut = 0,
                SummaOutWithNaklad = 0,
                QuantityNakopit = 0,
                TovarDocDC = 0
            }).ToList();
            decimal startPrice = 0;
            decimal startPriceWithNaklad = 0;
            decimal quantityNakopit = 0;
            foreach (var d in calcs)
            {
                var isVozvrat = operList.Any(_ => _.OperCode == 25 && _.DocDate == d.DocDate);
                var isNomenklTransfer =
                    operList.Any(
                        _ =>
                            _.OperCode == 19 && _.DocDate == d.DocDate &&
                            (d.QuantityIn > 0 || d.QuantityOut > 0));
                if (d.QuantityIn > 0 && d.QuantityOut > 0 && !isVozvrat && !isNomenklTransfer)
                {
                    startPrice = (startPrice * quantityNakopit + d.SummaIn) / (quantityNakopit + d.QuantityIn);
                    startPriceWithNaklad = (startPriceWithNaklad * quantityNakopit + d.SummaIn + d.Naklad) /
                                           (quantityNakopit + d.QuantityIn);
                    d.SummaOut = startPrice * d.QuantityIn;
                    d.SummaOutWithNaklad = startPrice * d.QuantityIn + d.Naklad;
                    d.CalcPrice = startPrice;
                    d.CalcPriceNaklad = startPriceWithNaklad;
                }
                else if (!isVozvrat && !isNomenklTransfer)
                {
                    if (d.QuantityIn > 0)
                    {
                        if (quantityNakopit + d.QuantityIn > 0)
                        {
                            startPrice = (startPrice * quantityNakopit +
                                          d.SummaIn) / (quantityNakopit + d.QuantityIn);
                            d.CalcPrice = startPrice;
                            startPriceWithNaklad = (startPriceWithNaklad * quantityNakopit +
                                                    d.SummaIn + d.Naklad) / (quantityNakopit + d.QuantityIn);
                            d.CalcPriceNaklad = startPriceWithNaklad;
                        }
                    }
                    else
                    {
                        d.CalcPrice = startPrice;
                        d.CalcPriceNaklad = startPriceWithNaklad;
                    }
                }

                if (isVozvrat)
                {
                    //var quan = operList.Where(_ => (_.DocDate == d.DocDate) && (_.OperCode == 25))
                    //    .Sum(k => k.QuantityIn);
                    startPrice = (startPrice * quantityNakopit +
                                  d.SummaIn) / (quantityNakopit + d.QuantityIn);
                    d.CalcPrice = startPrice;
                    startPriceWithNaklad = (startPriceWithNaklad * quantityNakopit +
                                            d.SummaIn + d.Naklad) / (quantityNakopit + d.QuantityIn);
                    d.CalcPriceNaklad = startPriceWithNaklad;
                }

                if (isNomenklTransfer)
                {
                    var trans = (from op in operList.Where(_ => _.OperCode == 19 && _.DocDate == d.DocDate)
                        group op by op.DocDate
                        into dateGrp
                        orderby dateGrp.Key
                        select dateGrp).Select(d1 => new NomenklCalcCostOperation
                    {
                        RowNumber = currentRowNumber,
                        NomenklDC = d1.Max(_ => _.NomenklDC),
                        Note = string.Empty,
                        CalcPrice = 0,
                        CalcPriceNaklad = 0,
                        DocDate = d1.Key,
                        KontragentIn = null,
                        KontragentOut = null,
                        OperationName = string.Empty,
                        OperCode = 0,
                        QuantityIn = d1.Sum(_ => _.QuantityIn),
                        QuantityOut = d1.Sum(_ => _.QuantityOut),
                        DocPrice = d1.Max(_ => _.DocPrice),
                        Naklad = d1.Sum(_ => _.Naklad),
                        SkladIn = null,
                        SkladOut = null,
                        SummaIn = d1.Sum(_ => _.SummaIn),
                        SummaInWithNaklad = d1.Sum(_ => _.SummaInWithNaklad),
                        SummaOut = 0,
                        SummaOutWithNaklad = 0,
                        QuantityNakopit = 0,
                        TovarDocDC = 0
                    }).ToList();
                    foreach (var t in trans)
                    {
                        if (t.QuantityIn + quantityNakopit > 0)
                        {
                            startPrice = (startPrice * quantityNakopit + t.SummaIn) /
                                         (t.QuantityIn + quantityNakopit);
                            startPriceWithNaklad =
                                (startPriceWithNaklad * quantityNakopit + t.SummaInWithNaklad * t.QuantityIn) /
                                (t.QuantityIn + quantityNakopit);
                        }
                        else
                        {
                            startPrice = t.DocPrice;
                            startPriceWithNaklad = t.DocPrice;
                        }

                        d.CalcPrice = startPrice;
                        d.CalcPriceNaklad = startPriceWithNaklad;
                    }

                    quantityNakopit += d.QuantityIn - d.QuantityOut;
                }
                else
                {
                    quantityNakopit += d.QuantityIn - d.QuantityOut;
                }

                d.QuantityNakopit = quantityNakopit;
            }

            return calcs;
        }

        public override void Save(IEnumerable<NomenklCalcCostOperation> operList)
        {
            decimal nomDC = 0;
            //var pdata = ctx.NOM_PRICE.Where(operList.Select(_ => _.NomenklDC).Contains(d => d.NOM_DC).ToList();
            foreach (var pn in context.NOM_PRICE)
                // ReSharper disable once PossibleMultipleEnumeration
                if (operList != null && operList.Select(_ => _.NomenklDC).Contains(pn.NOM_DC))
                    context.NOM_PRICE.Remove(pn);
            SaveNomPrice.Clear();
            var dates = new List<DateTime>();
            // ReSharper disable once PossibleMultipleEnumeration
            if (operList != null)
            {
                dates.AddRange(operList.Select(_ => _.DocDate).Distinct());
                foreach (var dt in dates)
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    var rn = operList.Where(_ => _.DocDate == dt).Select(_ => _.RowNumber).Max();
                    // ReSharper disable once PossibleMultipleEnumeration
                    SaveNomPrice.Add(dt, operList.Single(_ => _.DocDate == dt && _.RowNumber == rn));
                }
            }

            foreach (var d in SaveNomPrice.Values)
            {
                nomDC = d.NomenklDC;
                context.NOM_PRICE.Add(new NOM_PRICE
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
            context.Database.ExecuteSqlCommand(s);
            //Console.WriteLine($"Сохранение для {nomDC}");
            context.SaveChanges();
            context.Database.ExecuteSqlCommand("delete FROM WD_27 " +
                                               " INSERT INTO dbo.WD_27 (DOC_CODE, SKLW_NOMENKL_DC, SKLW_DATE, SKLW_KOLICH) " +
                                               " SELECT n.StoreDC, n.nomdc, n.Date, SUM(n.prihod) - SUM(n.rashod) " +
                                               " FROM NomenklMoveWithPrice n " +
                                               " WHERE n.date = (SELECT MAX(n1.date) FROM NomenklMoveWithPrice n1 " +
                                               " WHERE n1.nomdc = n.nomdc AND n.storedc = n1.storedc)" +
                                               " GROUP BY n.nomdc,n.StoreDc, n.Date " +
                                               " HAVING SUM(n.prihod) - SUM(n.rashod) > 0");
        }
    }
}
