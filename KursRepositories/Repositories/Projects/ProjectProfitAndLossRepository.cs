using Core.ViewModel.Base;
using Data;
using Helper;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Management;
using KursDomain.ICommon;
using KursDomain.References;
using KursDomain.References.RedisCache;
using KursRepositories.Repositories.Base;
using KursRepositories.Repositories.BreakEven;
using KursRepositories.Repositories.ProfitAndLoss;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace KursRepositories.Repositories.Projects
{
    public class ProjectProfitAndLossRepository : BaseRepository, IProjectProfitAndLossRepository
    {
        #region Fields

        private ALFAMEDIAEntities myContext;
        public Guid ProjectId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public readonly List<CURRENCY_RATES_CB> MyRates = new();

        #endregion

        #region Constructors

        public ProjectProfitAndLossRepository(ALFAMEDIAEntities context)
        {
            myContext = context;
        }

        public ProjectProfitAndLossRepository(ALFAMEDIAEntities context, Guid projectId, DateTime dateStart, DateTime dateEnd)
        {
            myContext = context;
            ProjectId = projectId;
            DateStart = dateStart;
            DateEnd = dateEnd;
            MyRates = myContext
                .CURRENCY_RATES_CB.Where(_ => _.RATE_DATE >= DateStart && _.RATE_DATE <= DateEnd)
                .ToList();
            var dt = MyRates.Select(_ => _.RATE_DATE).Distinct().ToList();
            MyRates.AddRange(dt.Select(r => new CURRENCY_RATES_CB
            {
                CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                NOMINAL = 1,
                RATE = 1,
                RATE_DATE = r
            }));
        }

        #endregion

        #region Properties


        #endregion

        #region Helper

        public static decimal GetRate(List<CURRENCY_RATES_CB> rates, decimal firstDC, decimal secondDC, DateTime date)
        {
            CURRENCY_RATES_CB f, s;
            try
            {
                if (firstDC == secondDC) return 1;
                var dd = rates.Where(_ => _.RATE_DATE <= date).ToList();
                var date1 = dd.Max(_ => _.RATE_DATE);
                f = rates.SingleOrDefault(_ => _.CRS_DC == firstDC && _.RATE_DATE == date1);
                s = rates.SingleOrDefault(_ => _.CRS_DC == secondDC && _.RATE_DATE == date1);
                if (f != null && s != null && s.RATE != 0)
                    return f.RATE / f.NOMINAL / (s.RATE / s.NOMINAL);
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static void SetCurrenciesValue(IProfitCurrencyList item, decimal crsDC, decimal profit, decimal loss)
        {
            switch (crsDC)
            {
                case CurrencyCode.NOT:
                    item.ProfitOther = profit;
                    item.LossOther = loss;
                    item.ResultOther = profit - loss;
                    break;

                case CurrencyCode.RUB:
                    item.ProfitRUB = profit;
                    item.LossRUB = loss;
                    item.ResultRUB = profit - loss;
                    break;

                case CurrencyCode.USD:
                    item.ProfitUSD = profit;
                    item.LossUSD = loss;
                    item.ResultUSD = profit - loss;
                    break;
                case CurrencyCode.EUR:

                    item.ProfitEUR = profit;
                    item.LossEUR = loss;
                    item.ResultEUR = profit - loss;
                    break;
                case CurrencyCode.GBP:
                    item.ProfitGBP = profit;
                    item.LossGBP = loss;
                    item.ResultGBP = profit - loss;
                    break;
                case CurrencyCode.CHF:
                    item.ProfitCHF = profit;
                    item.LossCHF = loss;
                    item.ResultCHF = profit - loss;
                    break;
                case CurrencyCode.SEK:
                    item.ProfitSEK = profit;
                    item.LossSEK = loss;
                    item.ResultSEK = profit - loss;
                    break;
                case CurrencyCode.CNY:
                    item.ProfitCNY = profit;
                    item.LossCNY = loss;
                    item.ResultCNY = profit - loss;
                    break;
            }
        }

        #endregion

        #region Methods

        public async Task<ProfitAndLossResult> CalcTovar()
        {
            var result = new ProfitAndLossResult();
            string sql = null;
            switch (GlobalOptions.SystemProfile.NomenklCalcType)
            {
                case NomenklCalcType.Standart:
                    sql =
                        "SELECT s83.NOM_0MATER_1USLUGA IsUsluga, DD_DATE DATE ,SFT_NEMENKL_DC NomenklDC, s50.PROD_NAME AS TypeProdName ,ISNULL(SF_CENTR_OTV_DC, 0) CentrOfResponsibility ,ISNULL(SF_CLIENT_DC, 0) KontragentDC " +
                        " ,ISNULL(SF_CRS_DC, 0) Currency ,ISNULL(S2.DOC_CODE, 0) Manager ,CAST(DDT_KOL_RASHOD AS NUMERIC(18, 8)) Quantity ,CAST(ISNULL(KONTR_CRS / CAST(DDT_KOL_RASHOD AS NUMERIC(18, 8)), 0) AS NUMERIC(18, 2)) Price , " +
                        " CAST(ISNULL(KONTR_CRS, 0) AS NUMERIC(18, 4)) SummaKontrCrs ,CAST(ISNULL(NomenklSum, 0) AS NUMERIC(18, 4)) NomSumm ,SF_DATE AS SF_DATE ,SF_NUM AS SF_NUM " +
                        " ,ISNULL(SF_NOTES, '') AS SF_NOTES, NAKL_NUM AS NAKL_NUM, ISNULL(NAKL_NOTES, '') AS NAKL_NOTES, ISNULL(SF_DILER_DC, 0) Diler ,ISNULL(DILER_SUMMA, 0) DilerSumma ,ISNULL(NomenklSumWOReval, 0) AS NomenklSumWOReval " +
                        " , s43.VALUTA_DC KontrCrsDC, s83.NOM_SALE_CRS_DC NomenklCrsDC " +
                        " FROM(SELECT " +
                        "   S83.NOM_0MATER_1USLUGA, S84.SF_DATE SF_DATE, CAST(S84.SF_IN_NUM AS VARCHAR(50)) + '/' + ISNULL(S84.SF_OUT_NUM, '') SF_NUM " +
                        "  , S84.SF_NOTE + ' / ' + T84.SFT_TEXT AS SF_NOTES, CAST(S24.DD_IN_NUM AS VARCHAR(50)) + '/' + ISNULL(S24.DD_EXT_NUM, '') NAKL_NUM " +
                        "  , S24.DD_NOTES AS NAKL_NOTES, DD_DATE, SFT_NEMENKL_DC, SF_CENTR_OTV_DC, SF_CLIENT_DC, SF_CRS_DC, S84.CREATOR, DDT_KOL_RASHOD " +
                        "  , SFT_ED_CENA, (SFT_SUMMA_K_OPLATE_KONTR_CRS * (DDT_KOL_RASHOD / SFT_KOL)) AS KONTR_CRS " +
                        "  , CAST((SELECT p1.PRICE FROM NOM_PRICE p1(NOLOCK) WHERE NOM_DC = T84.SFT_NEMENKL_DC AND p1.DATE = ISNULL((SELECT MAX(p2.DATE) " +
                        "        FROM NOM_PRICE p2(NOLOCK) WHERE p1.NOM_DC = p2.NOM_DC AND p2.DATE < S24.DD_DATE) " +
                        "      , (SELECT MAX(p2.DATE) FROM NOM_PRICE p2(NOLOCK) WHERE p1.NOM_DC = p2.NOM_DC  AND p2.DATE = S24.DD_DATE))) *T24.DDT_KOL_RASHOD AS NUMERIC(18, 4)) NomenklSum " +
                        "  ,S84.SF_DILER_DC,ISNULL(SFT_NACENKA_DILERA, 0) * DDT_KOL_RASHOD DILER_SUMMA " +
                        "  ,CAST((SELECT p1.PRICE_WO_REVAL FROM NOM_PRICE p1(NOLOCK) WHERE NOM_DC = T84.SFT_NEMENKL_DC AND p1.DATE = ISNULL((SELECT MAX(p2.DATE) FROM NOM_PRICE p2(NOLOCK)  WHERE p1.NOM_DC = p2.NOM_DC AND p2.DATE < S24.DD_DATE) " +
                        "      , (SELECT MAX(p2.DATE) FROM NOM_PRICE p2(NOLOCK) WHERE p1.NOM_DC = p2.NOM_DC AND p2.DATE = S24.DD_DATE))) *T24.DDT_KOL_RASHOD AS NUMERIC(18, 4)) AS NomenklSumWOReval " +
                        "   FROM TD_84 T84 " +
                        "  INNER JOIN SD_84 S84 ON S84.DOC_CODE = T84.DOC_CODE " +
                        "  INNER JOIN ProjectsDocs pd ON pd.DocDC = S84.DOC_CODE" +
                        $"  INNER JOIN Projects p ON p.id = pd.ProjectId AND P.Id = '{CustomFormat.GuidToSqlString(ProjectId)}'" +
                        "  INNER JOIN TD_24 T24 ON T24.DDT_SFACT_DC = S84.DOC_CODE " +
                        "   AND T24.DDT_SFACT_ROW_CODE = T84.Code AND DDT_KOL_RASHOD > 0 " +
                        $"  INNER JOIN SD_24 S24  ON S24.DOC_CODE = T24.DOC_CODE AND S24.DD_DATE BETWEEN '{CustomFormat.DateToString(DateStart)}' AND '{CustomFormat.DateToString(DateEnd)}' " +
                        "  INNER JOIN SD_83 S83 ON T84.SFT_NEMENKL_DC = S83.DOC_CODE) TAB " +
                        " INNER JOIN SD_43 s43 ON s43.DOC_CODE = TAB.SF_CLIENT_DC  AND s43.FLAG_BALANS=1 " +
                        " INNER JOIN SD_83 s83 ON s83.DOC_CODE = TAB.SFT_NEMENKL_DC " +
                        " INNER JOIN SD_50 s50 ON s83.NOM_PRODUCT_DC = s50.DOC_CODE " +
                        " LEFT OUTER JOIN EXT_USERS EU ON TAB.CREATOR = EU.USR_NICKNAME " +
                        " LEFT OUTER JOIN SD_2 S2 ON S2.TABELNUMBER = EU.TABELNUMBER ";
                    break;
                case NomenklCalcType.NakladSeparately:

                    sql =
                        "SELECT s83.NOM_0MATER_1USLUGA IsUsluga, DD_DATE DATE ,SFT_NEMENKL_DC NomenklDC, s50.PROD_NAME AS TypeProdName ,ISNULL(SF_CENTR_OTV_DC, 0) CentrOfResponsibility ,ISNULL(SF_CLIENT_DC, 0) KontragentDC " +
                        " ,ISNULL(SF_CRS_DC, 0) Currency ,ISNULL(S2.DOC_CODE, 0) Manager ,CAST(DDT_KOL_RASHOD AS NUMERIC(18, 8)) Quantity ,CAST(ISNULL(KONTR_CRS / CAST(DDT_KOL_RASHOD AS NUMERIC(18, 8)), 0) AS NUMERIC(18, 2)) Price , " +
                        " CAST(ISNULL(KONTR_CRS, 0) AS NUMERIC(18, 4)) SummaKontrCrs ,CAST(ISNULL(NomenklSum, 0) AS NUMERIC(18, 4)) NomSumm ,SF_DATE AS SF_DATE ,SF_NUM AS SF_NUM " +
                        " ,ISNULL(SF_NOTES, '') AS SF_NOTES, NAKL_NUM AS NAKL_NUM, ISNULL(NAKL_NOTES, '') AS NAKL_NOTES, ISNULL(SF_DILER_DC, 0) Diler ,ISNULL(DILER_SUMMA, 0) DilerSumma ,ISNULL(NomenklSumWOReval, 0) AS NomenklSumWOReval " +
                        " , s43.VALUTA_DC KontrCrsDC, s83.NOM_SALE_CRS_DC NomenklCrsDC " +
                        " FROM(SELECT " +
                        "   S83.NOM_0MATER_1USLUGA, S84.SF_DATE SF_DATE, CAST(S84.SF_IN_NUM AS VARCHAR(50)) + '/' + ISNULL(S84.SF_OUT_NUM, '') SF_NUM " +
                        "  , S84.SF_NOTE + ' / ' + T84.SFT_TEXT AS SF_NOTES, CAST(S24.DD_IN_NUM AS VARCHAR(50)) + '/' + ISNULL(S24.DD_EXT_NUM, '') NAKL_NUM " +
                        "  , S24.DD_NOTES AS NAKL_NOTES, DD_DATE, SFT_NEMENKL_DC, SF_CENTR_OTV_DC, SF_CLIENT_DC, SF_CRS_DC, S84.CREATOR, DDT_KOL_RASHOD " +
                        "  , SFT_ED_CENA, (SFT_SUMMA_K_OPLATE_KONTR_CRS * (DDT_KOL_RASHOD / SFT_KOL)) AS KONTR_CRS " +
                        "  , CAST((SELECT p1.PRICE_WO_NAKLAD FROM NOM_PRICE p1(NOLOCK) WHERE NOM_DC = T84.SFT_NEMENKL_DC AND p1.DATE = ISNULL((SELECT MAX(p2.DATE) " +
                        "        FROM NOM_PRICE p2(NOLOCK) WHERE p1.NOM_DC = p2.NOM_DC AND p2.DATE <= S24.DD_DATE) " +
                        "      , (SELECT MAX(p2.DATE) FROM NOM_PRICE p2(NOLOCK) WHERE p1.NOM_DC = p2.NOM_DC  AND p2.DATE = S24.DD_DATE))) * T24.DDT_KOL_RASHOD AS NUMERIC(18, 4)) NomenklSum " +
                        "  ,S84.SF_DILER_DC,ISNULL(SFT_NACENKA_DILERA, 0) * DDT_KOL_RASHOD DILER_SUMMA " +
                        "  ,CAST((SELECT p1.PRICE_WO_REVAL FROM NOM_PRICE p1(NOLOCK) WHERE NOM_DC = T84.SFT_NEMENKL_DC AND p1.DATE = ISNULL((SELECT MAX(p2.DATE) FROM NOM_PRICE p2(NOLOCK)  WHERE p1.NOM_DC = p2.NOM_DC AND p2.DATE <= S24.DD_DATE) " +
                        "      , (SELECT MAX(p2.DATE) FROM NOM_PRICE p2(NOLOCK) WHERE p1.NOM_DC = p2.NOM_DC AND p2.DATE = S24.DD_DATE))) *T24.DDT_KOL_RASHOD AS NUMERIC(18, 4)) AS NomenklSumWOReval " +
                        "   FROM TD_84 T84 " +
                        "  INNER JOIN SD_84 S84 ON S84.DOC_CODE = T84.DOC_CODE " +
                        "  INNER JOIN TD_24 T24 ON T24.DDT_SFACT_DC = S84.DOC_CODE " +
                        "  INNER JOIN ProjectsDocs pd ON pd.DocDC = S84.DOC_CODE" +
                        $"  INNER JOIN Projects p ON p.id = pd.ProjectId AND P.Id = '{CustomFormat.GuidToSqlString(ProjectId)}'" +
                        "   AND T24.DDT_SFACT_ROW_CODE = T84.Code AND DDT_KOL_RASHOD > 0 " +
                        $"  INNER JOIN SD_24 S24  ON S24.DOC_CODE = T24.DOC_CODE AND S24.DD_DATE BETWEEN '{CustomFormat.DateToString(DateStart)}' AND '{CustomFormat.DateToString(DateEnd)}' " +
                        "  INNER JOIN SD_83 S83 ON T84.SFT_NEMENKL_DC = S83.DOC_CODE) TAB " +
                        " INNER JOIN SD_43 s43 ON s43.DOC_CODE = TAB.SF_CLIENT_DC  AND s43.FLAG_BALANS=1 " +
                        " INNER JOIN SD_83 s83 ON s83.DOC_CODE = TAB.SFT_NEMENKL_DC " +
                        " INNER JOIN SD_50 s50 ON s83.NOM_PRODUCT_DC = s50.DOC_CODE " +
                        " LEFT OUTER JOIN EXT_USERS EU ON TAB.CREATOR = EU.USR_NICKNAME " +
                        " LEFT OUTER JOIN SD_2 S2 ON S2.TABELNUMBER = EU.TABELNUMBER ";
                    break;
            }

            var data = await myContext.Database.SqlQuery<BreakEvenTemp>(sql).ToListAsync();
            var dictProds = data.Select(_ => _.TypeProdName)
                .Distinct()
                .ToDictionary(d => d, _ => Guid.NewGuid());
            var dictLosses = data.Select(_ => _.TypeProdName)
                .Distinct()
                .ToDictionary(d => d, _ => Guid.NewGuid());
            //updateNomenklCache(data.Select(_ => _.NomenklDC).Distinct().ToList());
            if (GlobalOptions.ReferencesCache is RedisCacheReferences cache)
                cache.UpdateNomenkl(data.Select(_ => _.NomenklDC).Distinct().ToList());
            foreach (var e in from d in data
                     let nom =
                         GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as KursDomain.References.Nomenkl
                     let kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent
                     // ReSharper disable once PossibleInvalidOperationException
                     let nomRate = GetRate(MyRates, ((IDocCode)nom.Currency).DocCode,
                         GlobalOptions.SystemProfile.MainCurrency.DocCode, d.DATE)
                     // ReSharper disable once PossibleInvalidOperationException
                     let kontrRate = GetRate(MyRates, ((IDocCode)kontr.Currency).DocCode,
                         GlobalOptions.SystemProfile.MainCurrency.DocCode, d.DATE)
                     select new
                     {
                         // ReSharper disable once PossibleMultipleEnumeration
                         GroupId = dictProds[d.TypeProdName],
                         nom.Name,
                         Note = "Ном.№ - " + nom.NomenklNumber,
                         nom.DocCode,
                         Quantity = Convert.ToDecimal(d.Quantity),
                         d.Price,
                         Profit = d.Quantity * d.Price,
                         Loss = d.NomSumm,
                         Result = d.Quantity * d.Price - d.NomSumm,
                         Date = d.DATE,
                         Kontragent = kontr.Name,
                         Nomenkl = nom,
                         Kontr = kontr
                     })
            {
                var newOp = new ProfitAndLossesExtendRowViewModel
                {
                    GroupId = e.GroupId,
                    Name = e.Name,
                    Note = e.Note,
                    DocCode = e.DocCode,
                    Quantity = e.Quantity,
                    Price = e.Price,
                    Date = e.Date,
                    Kontragent = e.Kontragent,
                    CurrencyName = ((IName)e.Kontr.Currency).Name,
                    Nomenkl = e.Nomenkl,
                    DocTypeCode = DocumentType.Waybill
                };
                SetCurrenciesValue(newOp, ((IDocCode)e.Kontr.Currency).DocCode, e.Profit,
                    e.Loss * GetRate(MyRates, ((IDocCode)e.Nomenkl.Currency).DocCode,
                        ((IDocCode)e.Kontr.Currency).DocCode, newOp.Date));

                var dcquery = await myContext.TD_24.Include(_ => _.SD_24)
                    .FirstOrDefaultAsync(_ => _.DDT_NOMENKL_DC == e.DocCode && _.SD_24.DD_DATE == e.Date);
                newOp.DocumentDC = dcquery?.DOC_CODE;

                result.Extend.Add(newOp);
            }

            foreach (var e in from d in data
                     let nom =
                         GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as KursDomain.References.Nomenkl
                     let kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent
                     // ReSharper disable once PossibleInvalidOperationException
                     let nomRate = GetRate(MyRates, ((IDocCode)nom.Currency).DocCode,
                         GlobalOptions.SystemProfile.MainCurrency.DocCode, d.DATE)
                     // ReSharper disable once PossibleInvalidOperationException
                     let kontrRate = GetRate(MyRates, ((IDocCode)kontr.Currency).DocCode,
                         GlobalOptions.SystemProfile.MainCurrency.DocCode, d.DATE)
                     select new
                     {
                         GroupId = dictLosses[d.TypeProdName],
                         nom.Name,
                         Note = "Ном.№ - " + nom.NomenklNumber,
                         nom.DocCode,
                         d.Quantity,
                         d.Price,
                         Profit = d.Quantity * d.Price,
                         Loss = d.NomSumm,
                         Result = d.Quantity * d.Price - d.NomSumm,
                         Date = d.DATE,
                         Kontragent = kontr.Name,
                         Nomenkl = nom,
                         Kontr = kontr
                     })
            {
                var newOp = new ProfitAndLossesExtendRowViewModel
                {
                    GroupId = e.GroupId,
                    Name = e.Name,
                    Note = e.Note,
                    DocCode = e.DocCode,
                    Quantity = e.Quantity,
                    Price = e.Price,
                    Date = e.Date,
                    Kontragent = e.Kontragent,
                    CurrencyName = ((IName)e.Kontr.Currency).Name,
                    Nomenkl = e.Nomenkl,
                    DocTypeCode = DocumentType.Waybill
                };
                SetCurrenciesValue(newOp, ((IDocCode)e.Kontr.Currency).DocCode, e.Profit,
                    e.Loss * GetRate(MyRates, ((IDocCode)e.Nomenkl.Currency).DocCode,
                        ((IDocCode)e.Kontr.Currency).DocCode, newOp.Date));

                result.Extend.Add(newOp);
            }

            foreach (var n in dictProds.Select(d => new ProfitAndLossesMainRowViewModel
                     {
                         Id = d.Value,
                         ParentId = Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}"),
                         Name = d.Key,
                         CalcType = TypeProfitAndLossCalc.IsProfit,
                         LossRUB = 0,
                         ProfitRUB = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitRUB),
                         ResultRUB = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitRUB),
                         LossUSD = 0,
                         ProfitUSD = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitUSD),
                         ResultUSD = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitUSD),
                         LossEUR = 0,
                         ProfitEUR = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitEUR),
                         ResultEUR = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitEUR),
                         LossGBP = 0,
                         ProfitGBP = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitGBP),
                         ResultGBP = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitGBP),
                         LossCHF = 0,
                         ProfitCHF = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitCHF),
                         ResultCHF = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitCHF),
                         LossSEK = 0,
                         ProfitSEK = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitSEK),
                         ResultSEK = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitEUR)
                     }))
            {
                result.Main.Add(n);
            }

            foreach (var d in dictLosses)
            {
                var n = new ProfitAndLossesMainRowViewModel
                {
                    Id = d.Value,
                    ParentId = Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}"),
                    Name = d.Key,
                    CalcType = TypeProfitAndLossCalc.IsLoss,
                    LossRUB = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossRUB),
                    ProfitRUB = 0,
                    ResultRUB = -result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossRUB),
                    LossUSD = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossUSD),
                    ProfitUSD = 0,
                    ResultUSD = -result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossUSD),
                    LossEUR = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossEUR),
                    ProfitEUR = 0,
                    ResultEUR = -result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossEUR),
                    LossGBP = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossGBP),
                    ProfitGBP = 0,
                    ResultGBP = -result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossGBP),
                    LossCHF = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossCHF),
                    ProfitCHF = 0,
                    ResultCHF = -result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossCHF),
                    LossSEK = result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossSEK),
                    ProfitSEK = 0,
                    ResultSEK = -result.Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossSEK)
                };
                result.Main.Add(n);
            }

            var nomPrihod = await myContext.TD_24
                .Include(_ => _.SD_24)
                .Where(_ =>
                    _.SD_24.DD_DATE >= DateStart && _.SD_24.DD_DATE <= DateEnd &&
                    _.SD_24.DD_TYPE_DC == 2010000005).ToListAsync();
            var newPrihodId = Guid.NewGuid();
            var spisanieTovara = Guid.NewGuid();
            if (GlobalOptions.ReferencesCache is RedisCacheReferences cache2)
                cache2.UpdateNomenkl(nomPrihod.Select(_ => _.DDT_NOMENKL_DC).Distinct().ToList());
            foreach (var d in nomPrihod)
            {
                var nom =
                    GlobalOptions.ReferencesCache.GetNomenkl(d.DDT_NOMENKL_DC) as KursDomain.References.Nomenkl;

                var e = new
                {
                    GroupId = newPrihodId,
                    // ReSharper disable once PossibleNullReferenceException
                    nom.Name,
                    Note = "Ном.№ - " + nom.NomenklNumber,
                    nom.DocCode,
                    Quantity = Convert.ToDecimal(d.DDT_KOL_PRIHOD),
                    // ReSharper disable once PossibleInvalidOperationException
                    Price = d.DDT_TAX_CENA ?? 0,
                    // ReSharper disable once PossibleInvalidOperationException
                    Profit = d.DDT_KOL_PRIHOD * (d.DDT_TAX_CENA ?? 0),
                    Loss = d.DDT_KOL_RASHOD *
                           NomenklViewModel.PriceWithOutNaklad(d.DDT_NOMENKL_DC,
                               d.SD_24.DD_DATE),
                    Result =
                        // ReSharper disable once PossibleInvalidOperationException
                        d.DDT_KOL_PRIHOD * (d.DDT_TAX_CENA ?? 0) -
                        d.DDT_KOL_RASHOD *
                        NomenklViewModel.PriceWithOutNaklad(d.DDT_NOMENKL_DC,
                            d.SD_24.DD_DATE),
                    Date = d.SD_24.DD_DATE,
                    Kontragent = "Приход",
                    Nomenkl = nom
                };
                if (d.DDT_KOL_PRIHOD > 0)
                {
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = newPrihodId,
                        Name = e.Name,
                        Note = e.Note,
                        DocCode = e.DocCode,
                        Quantity = e.Quantity,
                        Price = e.Price,
                        Date = e.Date,
                        Kontragent = e.Kontragent,
                        CurrencyName = ((IName)e.Nomenkl.Currency).Name,
                        Nomenkl = e.Nomenkl,
                        DocTypeCode = DocumentType.StoreOrderIn
                    };
                    SetCurrenciesValue(newOp, ((IDocCode)e.Nomenkl.Currency).DocCode, e.Profit, 0m);

                    result.Extend.Add(newOp);
                }

                if (d.DDT_KOL_RASHOD > 0)
                {
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = spisanieTovara,
                        Name = e.Name,
                        Note = e.Note,
                        DocCode = e.DocCode,
                        Quantity = e.Quantity,
                        Price = e.Price,
                        Date = e.Date,
                        Kontragent = e.Kontragent,
                        CurrencyName = ((IName)e.Nomenkl.Currency).Name,
                        Nomenkl = e.Nomenkl,
                        DocTypeCode = DocumentType.Waybill
                    };
                    SetCurrenciesValue(newOp, ((IDocCode)e.Nomenkl.Currency).DocCode, 0m, e.Loss);

                    result.Extend.Add(newOp);
                }
            }

            return result;

        }

        public Task<ProfitAndLossResult> CalcStartKontragentBalans()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcOutCach()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcStartCash()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcStartBank()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcNomInRounding()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcDilers()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcUslugiDilers()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcVozvrat()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcNomenklReturn()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcSpisanie()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcTovarTransfer()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcUslugi()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcFinance()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcOutBalans()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> SpisanieTovar()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcCurrencyChange()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcZarplata()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcZarplataNach()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcNomenklCurrencyChanged()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcAccruedAmmount()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcStockHolders()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitAndLossResult> CalcTransferOutBalans()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Commands


        #endregion



    }
}
