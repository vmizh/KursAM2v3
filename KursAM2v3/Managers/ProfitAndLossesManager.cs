using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core.ViewModel.Base;
using Data;
using FinanceAnalitic;
using Helper;
using KursAM2.View.Base;
using KursAM2.View.Management;
using KursAM2.ViewModel.Logistiks;
using KursAM2.ViewModel.Management;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Management;
using KursDomain.ICommon;
using KursDomain.Managers;
using KursDomain.References;
using KursDomain.References.RedisCache;
using NomenklMain = KursDomain.References.NomenklMain;

namespace KursAM2.Managers
{
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public class ProfitAndLossesManager : RSWindowViewModelBase
    {
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());
        public DateTime DateEnd;
        public DateTime DateStart;
        private Project myProject;
        public List<CURRENCY_RATES_CB> MyRates = new List<CURRENCY_RATES_CB>();
        private Guid spisanieTovara;

        public ProfitAndLossesManager(ProfitAndLossesWindowViewModel vm)
        {
            DataViewModel = vm;
        }

        public ProfitAndLossesManager(ProfitAndLossesWindowViewModel2 vm)
        {
            DataViewModel = vm;
        }

        public ProfitAndLossesManager(ProjectProfitAndLossesWindowViewModel vm)
        {
            DataViewModel = vm;
        }

        public List<decimal> ProjectDocDC { set; get; } = new List<decimal>();

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public List<Guid> ProjectIds { set; get; }

        public Project Project
        {
            get => myProject;
            set
            {
                if (Equals(myProject, value)) return;
                myProject = value;
                if (myProject != null)
                    ProjectIds = getProjectIdsRecursively(myProject.Id);
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public RSWindowViewModelBase DataViewModel { set; get; }
        public ObservableCollection<ProfitAndLossesMainRowViewModel> Main { set; get; }
        public ObservableCollection<ProfitAndLossesMainRowViewModel> MainNach { set; get; }
        public ObservableCollection<ProfitAndLossesExtendRowViewModel> Extend { set; get; }
        public ObservableCollection<ProfitAndLossesExtendRowViewModel> ExtendNach { set; get; }

        private List<Guid> getProjectIdsRecursively(Guid mainId)
        {
            var ret = new List<Guid> { mainId };
            var childs = GlobalOptions.ReferencesCache.GetProjectsAll().Where(_ => _.ParentId == mainId).Cast<Project>()
                .Select(_ => _.Id).ToList();
            if (childs.Count == 0)
                return ret;
            //ret.AddRange(childs);
            foreach (var id in childs) ret.AddRange(getProjectIdsRecursively(id));
            return ret;
        }

        public List<CURRENCY_RATES_CB> GetCbrates(DateTime dateStart, DateTime dateEnd)
        {
            var ret = new List<CURRENCY_RATES_CB>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var rets = ctx.CURRENCY_RATES_CB.Where(_ => _.RATE_DATE >= dateStart && _.RATE_DATE <= dateEnd)
                    .ToList();
                var dt = rets.Select(_ => _.RATE_DATE).Distinct().ToList();
                ret.AddRange(dt.Select(r => new CURRENCY_RATES_CB
                {
                    CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                    NOMINAL = 1,
                    RATE = 1,
                    RATE_DATE = r
                }));
            }

            return ret;
        }

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

        public void NomenklCalc(object obj)
        {
            if (!(obj is ProfitAndLossesExtendRowViewModel currentExtend)) return;
            if (currentExtend.Nomenkl == null) return;
            var ctx = new NomPriceWindowViewModel(currentExtend.Nomenkl.DocCode);
            var dlg = new SelectDialogView { DataContext = ctx };
            dlg.ShowDialog();
        }

        public void CalcOutCach()
        {
            ProfitAndLossesExtendRowViewModel newOp;
            using (var ent = GlobalOptions.GetEntities())
            {
                var cashOut =
                    ent.SD_34.Where(
                        _ =>
                            _.NCODE != 100 && _.TABELNUMBER != null && _.DATE_ORD >= DateStart &&
                            _.DATE_ORD <= DateEnd);
                foreach (var d in cashOut)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    // ReSharper disable once AssignNullToNotNullAttribute
                    //var crsName = MainReferences.Currencies[d.CRS_DC.Value].Name;
                    newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{52EA160E-27DC-47E1-9006-70DF349943F6}"),
                        // ReSharper disable once PossibleNullReferenceException
                        Name = ((IName)GlobalOptions.ReferencesCache.GetEmployee(d.TABELNUMBER)).Name,
                        DocCode = d.DOC_CODE,
                        Quantity = 0,
                        Kontragent =
                            // ReSharper disable once PossibleNullReferenceException
                            ((IName)GlobalOptions.ReferencesCache.GetEmployee(d.TABELNUMBER)).Name,
                        // ReSharper disable once PossibleInvalidOperationException
                        Date = (DateTime)d.DATE_ORD,
                        Note = null,
                        Nomenkl = null,
                        DocTypeCode = (DocumentType)34
                    };
                    SetCurrenciesValue(newOp, d.CRS_DC, 0m, d.SUMM_ORD);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        private void updateNomenklCache(List<decimal> nomDCs)
        {
            if (GlobalOptions.ReferencesCache is not RedisCacheReferences) return;
            var cache = (RedisCacheReferences)GlobalOptions.ReferencesCache;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var mains = new Dictionary<Guid, NomenklMain>();
                var noms = new Dictionary<decimal, KursDomain.References.Nomenkl>();

                var mainIDs = new List<Guid>();
                var nomsDCs = new List<decimal>();
                var allNoms = ctx.SD_83.Select(_ => new { DocCode = _.DOC_CODE, MainId = _.MainId });
                foreach (var item in allNoms)
                {
                    if (!cache.NomenklMains.ContainsKey(item.MainId.Value))
                    {
                        mainIDs.Add(item.MainId.Value);
                    }

                    if (!cache.Nomenkls.ContainsKey(item.DocCode))
                    {
                        nomsDCs.Add(item.DocCode);
                    }
                }

                foreach (var entity in ctx.NomenklMain.AsNoTracking().ToList())
                {
                    if (mainIDs.Contains(entity.Id))
                    {
                        var item = new NomenklMain
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            Notes = entity.Note,
                            NomenklNumber = entity.NomenklNumber,
                            FullName = entity.FullName,
                            IsUsluga = entity.IsUsluga,
                            IsProguct = entity.IsComplex,
                            IsNakladExpense = entity.IsNakladExpense,
                            IsOnlyState = entity.IsOnlyState ?? false,
                            IsCurrencyTransfer = entity.IsCurrencyTransfer ?? false,
                            IsRentabelnost = entity.IsRentabelnost ?? false,
                            UnitDC = entity.UnitDC,
                            CategoryDC = entity.CategoryDC,
                            NomenklTypeDC = entity.TypeDC,
                            ProductTypeDC = entity.ProductDC
                        };
                        //item.LoadFromEntity(entity,this);
                        mains.Add(item.Id, item);
                    }
                }

                cache.UpdateListGuid(mains.Values);

                foreach (var entity in ctx.SD_83.Include(_ => _.NomenklMain).AsNoTracking())
                {
                    if (nomsDCs.Contains(entity.DOC_CODE))
                    {
                        var item = new KursDomain.References.Nomenkl
                        {
                            DocCode = entity.DOC_CODE,
                            Id = entity.Id,
                            Name = entity.NOM_NAME,
                            FullName =
                                entity.NOM_FULL_NAME,
                            Notes = entity.NOM_NOTES,
                            IsUsluga =
                                entity.NOM_0MATER_1USLUGA == 1,
                            IsProguct = entity.NOM_1PROD_0MATER == 1,
                            IsNakladExpense =
                                entity.NOM_1NAKLRASH_0NO == 1,
                            DefaultNDSPercent = (decimal?)entity.NOM_NDS_PERCENT,
                            IsDeleted =
                                entity.NOM_DELETED == 1,
                            IsUslugaInRentabelnost =
                                entity.IsUslugaInRent ?? false,
                            UpdateDate =
                                entity.UpdateDate ?? DateTime.MinValue,
                            MainId =
                                entity.MainId ?? Guid.Empty,
                            IsCurrencyTransfer = entity.NomenklMain.IsCurrencyTransfer ?? false,
                            NomenklNumber =
                                entity.NOM_NOMENKL,
                            NomenklTypeDC =
                                entity.NomenklMain.TypeDC,
                            ProductTypeDC = entity.NomenklMain.ProductDC,
                            UnitDC = entity.NOM_ED_IZM_DC,
                            CurrencyDC = entity.NOM_SALE_CRS_DC,
                            GroupDC = entity.NOM_CATEG_DC
                        };
                        noms.Add(item.DocCode, item);
                    }

                    cache.UpdateList(noms.Values);
                }

                
            }
        }

        public void CalcTovar()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                //var cos = ent.SD_40.ToList();
                string sql = null;
                switch (GlobalOptions.SystemProfile.NomenklCalcType)
                {
                    case NomenklCalcType.Standart:
                        if (Project == null)
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
                                "  INNER JOIN TD_24 T24 ON T24.DDT_SFACT_DC = S84.DOC_CODE " +
                                "   AND T24.DDT_SFACT_ROW_CODE = T84.Code AND DDT_KOL_RASHOD > 0 " +
                                $"  INNER JOIN SD_24 S24  ON S24.DOC_CODE = T24.DOC_CODE AND S24.DD_DATE BETWEEN '{CustomFormat.DateToString(DateStart)}' AND '{CustomFormat.DateToString(DateEnd)}' " +
                                "  INNER JOIN SD_83 S83 ON T84.SFT_NEMENKL_DC = S83.DOC_CODE) TAB " +
                                " INNER JOIN SD_43 s43 ON s43.DOC_CODE = TAB.SF_CLIENT_DC  AND s43.FLAG_BALANS=1 " +
                                " INNER JOIN SD_83 s83 ON s83.DOC_CODE = TAB.SFT_NEMENKL_DC " +
                                " INNER JOIN SD_50 s50 ON s83.NOM_PRODUCT_DC = s50.DOC_CODE " +
                                " LEFT OUTER JOIN EXT_USERS EU ON TAB.CREATOR = EU.USR_NICKNAME " +
                                " LEFT OUTER JOIN SD_2 S2 ON S2.TABELNUMBER = EU.TABELNUMBER ";
                        else
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
                                $"  INNER JOIN Projects p ON p.id = pd.ProjectId AND P.Id = '{CustomFormat.GuidToSqlString(Project.Id)}'" +
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
                        if (Project == null)
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
                                "   AND T24.DDT_SFACT_ROW_CODE = T84.Code AND DDT_KOL_RASHOD > 0 " +
                                $"  INNER JOIN SD_24 S24  ON S24.DOC_CODE = T24.DOC_CODE AND S24.DD_DATE BETWEEN '{CustomFormat.DateToString(DateStart)}' AND '{CustomFormat.DateToString(DateEnd)}' " +
                                "  INNER JOIN SD_83 S83 ON T84.SFT_NEMENKL_DC = S83.DOC_CODE) TAB " +
                                " INNER JOIN SD_43 s43 ON s43.DOC_CODE = TAB.SF_CLIENT_DC  AND s43.FLAG_BALANS=1 " +
                                " INNER JOIN SD_83 s83 ON s83.DOC_CODE = TAB.SFT_NEMENKL_DC " +
                                " INNER JOIN SD_50 s50 ON s83.NOM_PRODUCT_DC = s50.DOC_CODE " +
                                " LEFT OUTER JOIN EXT_USERS EU ON TAB.CREATOR = EU.USR_NICKNAME " +
                                " LEFT OUTER JOIN SD_2 S2 ON S2.TABELNUMBER = EU.TABELNUMBER ";
                        else
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
                                $"  INNER JOIN Projects p ON p.id = pd.ProjectId AND P.Id = '{CustomFormat.GuidToSqlString(Project.Id)}'" +
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

                var data = ent.Database.SqlQuery<BreakEvenTemp>(sql).ToList();
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

                    newOp.DocumentDC = ent.TD_24.Include(_ => _.SD_24)
                        .FirstOrDefault(_ => _.DDT_NOMENKL_DC == e.DocCode && _.SD_24.DD_DATE == e.Date)?.DOC_CODE;
                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
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

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }

                var delp = Main.Where(_ => _.ParentId == Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}")).ToList();
                foreach (var d in delp)
                {
                    Main.Remove(d);
                    MainNach.Remove(d);
                }

                foreach (var n in dictProds.Select(d => new ProfitAndLossesMainRowViewModel
                         {
                             Id = d.Value,
                             ParentId = Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}"),
                             Name = d.Key,
                             CalcType = TypeProfitAndLossCalc.IsProfit,
                             LossRUB = 0,
                             ProfitRUB = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitRUB),
                             ResultRUB = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitRUB),
                             LossUSD = 0,
                             ProfitUSD = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitUSD),
                             ResultUSD = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitUSD),
                             LossEUR = 0,
                             ProfitEUR = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitEUR),
                             ResultEUR = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitEUR),
                             LossGBP = 0,
                             ProfitGBP = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitGBP),
                             ResultGBP = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitGBP),
                             LossCHF = 0,
                             ProfitCHF = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitCHF),
                             ResultCHF = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitCHF),
                             LossSEK = 0,
                             ProfitSEK = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitSEK),
                             ResultSEK = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitEUR)
                         }))
                {
                    Main.Add(n);
                    MainNach.Add(n);
                }

                var delp1 = Main.Where(_ => _.ParentId == Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}"))
                    .ToList();
                foreach (var d in delp1)
                {
                    Main.Remove(d);
                    MainNach.Remove(d);
                }

                foreach (var d in dictLosses)
                {
                    var n = new ProfitAndLossesMainRowViewModel
                    {
                        Id = d.Value,
                        ParentId = Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}"),
                        Name = d.Key,
                        CalcType = TypeProfitAndLossCalc.IsLoss,
                        LossRUB = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossRUB),
                        ProfitRUB = 0,
                        ResultRUB = -Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossRUB),
                        LossUSD = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossUSD),
                        ProfitUSD = 0,
                        ResultUSD = -Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossUSD),
                        LossEUR = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossEUR),
                        ProfitEUR = 0,
                        ResultEUR = -Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossEUR),
                        LossGBP = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossGBP),
                        ProfitGBP = 0,
                        ResultGBP = -Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossGBP),
                        LossCHF = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossCHF),
                        ProfitCHF = 0,
                        ResultCHF = -Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossCHF),
                        LossSEK = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossSEK),
                        ProfitSEK = 0,
                        ResultSEK = -Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossSEK)
                    };
                    Main.Add(n);
                    MainNach.Add(n);
                }

                var nomPrihod = ent.TD_24
                    .Include(_ => _.SD_24)
                    .Where(
                        _ =>
                            _.SD_24.DD_DATE >= DateStart && _.SD_24.DD_DATE <= DateEnd &&
                            _.SD_24.DD_TYPE_DC == 2010000005).ToList();
                var newPrihodId = Guid.NewGuid();
                spisanieTovara = Guid.NewGuid();
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

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
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

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }
                }

                if (Project == null)
                {
                    var newPrih = new ProfitAndLossesMainRowViewModel
                    {
                        Id = newPrihodId,
                        ParentId = Guid.Parse("{334973B4-1652-4473-9DED-FD4B31B31FC1}"),
                        Name = "Приход товара (инвентаризация)",
                        CalcType = TypeProfitAndLossCalc.IsProfit,
                        LossRUB = 0,
                        ProfitRUB = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitRUB),
                        ResultRUB = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitRUB),
                        LossUSD = 0,
                        ProfitUSD = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitUSD),
                        ResultUSD = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitUSD),
                        LossEUR = 0,
                        ProfitEUR = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitEUR),
                        ResultEUR = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitEUR),
                        LossGBP = 0,
                        ProfitGBP = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitGBP),
                        ResultGBP = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitGBP),
                        LossCHF = 0,
                        ProfitCHF = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitCHF),
                        ResultCHF = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitCHF),
                        LossSEK = 0,
                        ProfitSEK = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitSEK),
                        ResultSEK = Extend.Where(_ => _.GroupId == newPrihodId).Sum(_ => _.ProfitSEK)
                    };
                    Main.Add(newPrih);
                    MainNach.Add(newPrih);
                    newPrih = new ProfitAndLossesMainRowViewModel
                    {
                        Id = spisanieTovara,
                        ParentId = Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}"),
                        Name = "Списание товара (инвентаризация)",
                        CalcType = TypeProfitAndLossCalc.IsLoss,
                        LossRUB = Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossRUB),
                        ProfitRUB = 0,
                        ResultRUB = -Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossRUB),
                        LossUSD = Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossUSD),
                        ProfitUSD = 0,
                        ResultUSD = -Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossUSD),
                        LossEUR = Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossEUR),
                        ProfitEUR = 0,
                        ResultEUR = -Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossUSD),
                        LossGBP = Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossGBP),
                        ProfitGBP = 0,
                        ResultGBP = -Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossUSD),
                        LossCHF = Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossCHF),
                        ProfitCHF = 0,
                        ResultCHF = -Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossUSD),
                        LossSEK = Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossSEK),
                        ProfitSEK = 0,
                        ResultSEK = -Extend.Where(_ => _.GroupId == spisanieTovara).Sum(_ => _.LossUSD)
                    };
                    Main.Add(newPrih);
                    MainNach.Add(newPrih);
                }
            }
        }

        public void CalcStartKontragentBalans()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var ostatki =
                    ent.SD_43.Where(
                            _ =>
                                _.FLAG_BALANS == 1 && _.START_BALANS >= DateStart && _.START_BALANS <= DateEnd &&
                                _.START_SUMMA != 0)
                        .ToList();
                foreach (var d in ostatki)
                {
                    var newOp = new ProfitAndLossesExtendRowViewModel();
                    if (d.START_SUMMA < 0)
                    {
                        newOp.GroupId = Guid.Parse("{15DF4D79-D608-412A-87A8-1560714A706A}");
                        newOp.Name = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.DOC_CODE))?.Name;
                        newOp.DocCode = d.DOC_CODE;
                        newOp.Quantity = 0;
                        newOp.Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.DOC_CODE))?.Name;
                        // ReSharper disable once PossibleInvalidOperationException
                        newOp.Date = (DateTime)d.START_BALANS;
                        newOp.Note = d.NOTES;
                        newOp.Nomenkl = null;
                    }
                    else
                    {
                        newOp.GroupId = Guid.Parse("{2D07127B-72A8-4018-B9A8-62C7A78CB9C3}");
                        newOp.Name = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.DOC_CODE))?.Name;
                        newOp.DocCode = d.DOC_CODE;
                        newOp.Quantity = 0;
                        newOp.Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.DOC_CODE))?.Name;
                        // ReSharper disable once PossibleInvalidOperationException
                        newOp.Date = (DateTime)d.START_BALANS;
                        newOp.Note = d.NOTES;
                        newOp.Nomenkl = null;
                    }

                    SetCurrenciesValue(newOp, d.VALUTA_DC, (decimal)(d.START_SUMMA > 0 ? d.START_SUMMA : 0),
                        (decimal)(d.START_SUMMA < 0 ? -d.START_SUMMA : 0));

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        public void CalcNomenklCurrencyChanged()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var nomChanged = ent.TD_26_CurrencyConvert
                    .Include(_ => _.TD_26)
                    .Include(_ => _.TD_26.SD_26)
                    .Where(_ => _.Date >= DateStart && _.Date <= DateEnd && _.DOC_CODE != null).ToList();
                foreach (var n in nomChanged)
                {
                    var nom = GlobalOptions.ReferencesCache.GetNomenkl(n.NomenklId) as KursDomain.References.Nomenkl;
                    var kontr = GlobalOptions.ReferencesCache.GetKontragent(n.TD_26.SD_26.SF_POST_DC) as Kontragent;
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = ProfitAndLossesMainRowViewModel.NomenklCurrencyChanges,
                        DocNum = $"{n.TD_26.SD_26.SF_IN_NUM}/{n.TD_26.SD_26.SF_POSTAV_NUM}",
                        Name = nom?.NomenklNumber + " " + nom?.Name,
                        Note = n.Note,
                        DocCode = n.DOC_CODE ?? 0,
                        Quantity = n.Quantity,
                        Price = n.Price,
                        Date = n.Date,
                        Kontragent = kontr?.Name,
                        DocTypeCode = DocumentType.InvoiceProvider,
                        Nomenkl = nom,
                        KontragentName = kontr?.Name,
                        Currency =
                            // ReSharper disable once PossibleNullReferenceException
                            GlobalOptions.ReferencesCache.GetCurrency(((IDocCode)nom.Currency).DocCode) as Currency,
                        CurrencyName =
                            ((IName)GlobalOptions.ReferencesCache.GetCurrency(((IDocCode)nom.Currency).DocCode)).Name
                    };
                    SetCurrenciesValue(newOp, ((IDocCode)nom.Currency).DocCode, n.Summa, 0m);
                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);

                    var nom1 =
                        GlobalOptions.ReferencesCache.GetNomenkl(n.TD_26.SFT_NEMENKL_DC) as
                            KursDomain.References.Nomenkl;
                    var newOp1 = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = ProfitAndLossesMainRowViewModel.NomenklCurrencyChanges,
                        // ReSharper disable once PossibleNullReferenceException
                        Name = nom1.NomenklNumber + " " + nom1.Name,
                        DocNum = $"{n.TD_26.SD_26.SF_IN_NUM}/{n.TD_26.SD_26.SF_POSTAV_NUM}",
                        Note = n.Note,
                        DocCode = n.DOC_CODE ?? 0,
                        Quantity = n.Quantity,
                        Price = n.TD_26.SFT_ED_CENA ?? 0,
                        Date = n.Date,
                        // ReSharper disable once PossibleNullReferenceException
                        Kontragent = kontr.Name,
                        DocTypeCode = DocumentType.InvoiceProvider,
                        Nomenkl = nom1,
                        KontragentName = kontr.Name,
                        Currency =
                            GlobalOptions.ReferencesCache.GetCurrency(((IDocCode)nom1.Currency).DocCode) as Currency,
                        CurrencyName =
                            ((IName)GlobalOptions.ReferencesCache.GetCurrency(((IDocCode)nom1.Currency).DocCode)).Name
                    };
                    SetCurrenciesValue(newOp1, ((IDocCode)nom1.Currency).DocCode, 0m, n.TD_26.SFT_ED_CENA * n.Quantity);
                    Extend.Add(newOp1);
                    ExtendNach.Add(newOp1);
                }
            }
        }

        public void CalcCurrencyChange()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var cashChanged = ent.SD_251.Include(_ => _.SD_22)
                    .Include(_ => _.SD_301)
                    .Include(_ => _.SD_3011)
                    .Where(_ => _.CH_DATE >= DateStart && _.CH_DATE <= DateEnd)
                    .ToList();
                foreach (var d in cashChanged)
                {
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{B6F2540A-9593-42E3-B34F-8C0983BC39A2}"),
                        Name = d.SD_22.CA_NAME,
                        Note = d.CH_NOTE,
                        DocCode = d.DOC_CODE,
                        Quantity = 0,
                        Price = 0,
                        Date = d.CH_DATE,
                        Kontragent = d.CH_NAME_ORD,
                        DocTypeCode = (DocumentType)251
                    };
                    SetCurrenciesValue(newOp, d.SD_301.DOC_CODE, d.CH_CRS_IN_SUM, 0m);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                    var newOp1 = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{35EBABEC-EAC3-4C3C-8383-6326C5D64C8C}"),
                        Name = d.SD_22.CA_NAME,
                        Note = d.CH_NOTE,
                        DocCode = d.DOC_CODE,
                        Quantity = 0,
                        Price = 0,
                        Date = d.CH_DATE,
                        Kontragent = d.CH_NAME_ORD,
                        DocTypeCode = (DocumentType)251
                    };
                    SetCurrenciesValue(newOp1, d.SD_3011.DOC_CODE, 0m, d.CH_CRS_OUT_SUM);

                    Extend.Add(newOp1);
                    ExtendNach.Add(newOp1);
                }

                var bankChanged = ent.TD_101.Include(_ => _.SD_101)
                    .Include(_ => _.SD_101.SD_114)
                    .Include(_ => _.SD_301)
                    .Include(_ => _.SD_3011)
                    .Where(_ => _.IsCurrencyChange == true && _.SD_101.VV_START_DATE >= DateStart
                                                           && _.SD_101.VV_START_DATE <= DateEnd
                    )
                    .ToList();
                foreach (var d in bankChanged)
                {
                    if (d.VVT_VAL_PRIHOD > 0)
                    {
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{B6F2540A-9593-42E3-B34F-8C0983BC39A2}"),
                            Name = d.SD_101.SD_114.BA_ACC_SHORTNAME,
                            Note = d.VVT_DOC_NUM,
                            DocCode = d.DOC_CODE,
                            Quantity = 0,
                            Price = 0,
                            Date = d.SD_101.VV_START_DATE,
                            Kontragent = d.SD_101.SD_114.BA_ACC_SHORTNAME,
                            DocTypeCode = (DocumentType)251
                        };
                        SetCurrenciesValue(newOp, d.VVT_CRS_DC, d.VVT_VAL_PRIHOD, 0m);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }

                    if (d.VVT_VAL_RASHOD > 0)
                    {
                        var newOp1 = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{35EBABEC-EAC3-4C3C-8383-6326C5D64C8C}"),
                            Name = d.SD_101.SD_114.BA_ACC_SHORTNAME,
                            Note = d.VVT_DOC_NUM,
                            DocCode = d.DOC_CODE,
                            Quantity = 0,
                            Price = 0,
                            Date = d.SD_101.VV_START_DATE,
                            Kontragent = d.SD_101.SD_114.BA_ACC_SHORTNAME,
                            DocTypeCode = (DocumentType)251
                        };
                        SetCurrenciesValue(newOp1, d.VVT_CRS_DC, 0m, d.VVT_VAL_RASHOD);

                        Extend.Add(newOp1);
                        ExtendNach.Add(newOp1);
                    }
                }
            }
        }

        public void CalcStartCash()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var cashOstatki =
                    ent.TD_22.Include(_ => _.SD_22)
                        .Where(_ => _.DATE_START >= DateStart && _.DATE_START <= DateEnd)
                        .ToList();
                foreach (var d in cashOstatki)
                {
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{A084B37A-D942-4B7F-9AE9-3C3AAA0F4475}"),
                        Name = d.SD_22.CA_NAME,
                        DocCode = d.DOC_CODE,
                        Quantity = 0,
                        Kontragent = d.SD_22.CA_NAME,
                        Date = d.DATE_START,
                        Note = null,
                        Nomenkl = null,
                        DocTypeCode = 0
                    };
                    SetCurrenciesValue(newOp, d.CRS_DC, d.SUMMA_START, 0m);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        //TODO Преписать в связи с новым способом ведения банка
        public void CalcStartBank()
        {
            // ReSharper disable once TooWideLocalVariableScope
            ProfitAndLossesExtendRowViewModel newOp;
            using (var ent = GlobalOptions.GetEntities())
            {
                var bankDates =
                    ent.SD_101.Where(_ => _.VV_START_DATE >= DateStart && _.VV_START_DATE <= DateEnd).ToList();
                if (bankDates.Count == 0) return;
                var dates = bankDates.Select(_ => _.VV_ACC_DC)
                    .Distinct()
                    .ToDictionary(d => d, d => ent.SD_101.Where(_ => _.VV_ACC_DC == d).Min(_ => _.VV_START_DATE));
                foreach (var d in dates)
                {
                    if (d.Value < DateStart || d.Value > DateEnd) continue;
                    var dc =
                        ent.SD_101.FirstOrDefault(_ => _.VV_ACC_DC == d.Key && _.VV_START_DATE == d.Value)?.DOC_CODE;
                    if (dc == null) continue;
                    var bank = ent.SD_114.FirstOrDefault(_ => _.DOC_CODE == d.Key);
                    // ReSharper disable once PossibleNullReferenceException
                    var bankName = bank.BA_BANK_NAME + " (" + bank.BA_ACC_SHORTNAME + ") " + "р/с " + bank.BA_RASH_ACC;
                    newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{0AD95635-A46D-49F2-AE78-CBDF52BD6E27}"),
                        Name = bankName,
                        DocCode = d.Key,
                        Quantity = 0,
                        Kontragent = bankName,
                        Date = d.Value,
                        Note = null,
                        Nomenkl = null,
                        DocTypeCode = (DocumentType)101
                    };
                    foreach (var c in ent.UD_101.Where(_ => _.DOC_CODE == dc && _.VVU_REST_TYPE == 0))
                        if (c.VVU_VAL_SUMMA > 0)
                            SetCurrenciesValue(newOp, c.VVU_CRS_DC, c.VVU_VAL_SUMMA, 0);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        public void CalcSpisanie()
        {
            //{66E0F763-7362-488D-B367-50AC84A72AD4}
            using (var ent = GlobalOptions.GetEntities())
            {
                var spisano = ent.TD_24.Include(_ => _.SD_24)
                    .Where(_ => _.SD_24.DD_TYPE_DC == 2010000010
                                && _.SD_24.DD_DATE >= DateStart && _.SD_24.DD_DATE <= DateEnd
                    )
                    .ToList();
                foreach (var d in spisano)
                {
                    var nomPrice =
                        NomenklViewModel.PriceWithOutNaklad(d.DDT_NOMENKL_DC,
                            d.SD_24.DD_DATE);
                    var nom =
                        GlobalOptions.ReferencesCache.GetNomenkl(d.DDT_NOMENKL_DC) as KursDomain.References.Nomenkl;
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = spisanieTovara,
                        // ReSharper disable once PossibleNullReferenceException
                        Name = nom.Name,
                        DocCode = nom.DocCode,
                        Quantity = d.DDT_KOL_PRIHOD,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        Kontragent = ((IName)GlobalOptions.ReferencesCache.GetWarehouse(d.SD_24.DD_SKLAD_OTPR_DC)).Name,
                        Date = d.SD_24.DD_DATE,
                        Note = $"ном № - {nom.NomenklNumber}",
                        Nomenkl = nom,
                        DocTypeCode = (DocumentType)359
                    };
                    SetCurrenciesValue(newOp, ((IDocCode)nom.Currency).DocCode, 0m, nomPrice * d.DDT_KOL_RASHOD);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        public void CalcVozvrat()
        {
            //Id = Guid.Parse("{C5C36299-FDEF-4251-B525-3DF10C0E8CB9}"), вщзврат от клиента
            using (var ent = GlobalOptions.GetEntities())
            {
                var vozvratTovara = ent.TD_24.Include(_ => _.SD_24)
                    .Where(_ => _.DDT_SPOST_DC == null
                                && _.SD_24.DD_DATE >= DateStart && _.SD_24.DD_DATE <= DateEnd
                                && _.SD_24.DD_VOZVRAT == 1)
                    .ToList();
                //{C5C36299-FDEF-4251-B525-3DF10C0E8CB9}
                //{04A7B6BB-7B3C-49F1-8E10-F1AE5F5582E4}
                foreach (var d in vozvratTovara)
                {
                    var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.SD_24.DD_KONTR_OTPR_DC) as Kontragent;
                    var nom =
                        GlobalOptions.ReferencesCache.GetNomenkl(d.DDT_NOMENKL_DC) as KursDomain.References.Nomenkl;
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{C5C36299-FDEF-4251-B525-3DF10C0E8CB9}"),
                        // ReSharper disable PossibleNullReferenceException
                        Name = nom.Name,
                        DocCode = nom.DocCode,
                        Quantity = d.DDT_KOL_PRIHOD,
                        Kontragent = kontr.Name,
                        // ReSharper restore PossibleNullReferenceException
                        Date = d.SD_24.DD_DATE,
                        Note = $"ном № - {nom.NomenklNumber}",
                        Nomenkl = nom,
                        DocTypeCode = (DocumentType)357
                    };
                    SetCurrenciesValue(newOp, ((IDocCode)nom.Currency).DocCode, 0, d.DDT_KONTR_CRS_SUMMA);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                    var newOp1 = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{04A7B6BB-7B3C-49F1-8E10-F1AE5F5582E4}"),
                        Name = nom.Name,
                        DocCode = nom.DocCode,
                        Quantity = d.DDT_KOL_PRIHOD,
                        Kontragent = kontr.Name,
                        Date = d.SD_24.DD_DATE,
                        Note = $"ном № - {nom.NomenklNumber}",
                        Nomenkl = nom,
                        DocTypeCode = (DocumentType)357
                    };
                    var p = nomenklManager.GetNomenklPrice(nom.DocCode, d.SD_24.DD_DATE);
                    SetCurrenciesValue(newOp1, ((IDocCode)nom.Currency).DocCode, p.Price * d.DDT_KOL_PRIHOD, 0);

                    Extend.Add(newOp1);
                    ExtendNach.Add(newOp1);
                }
            }
        }

        public void CalcStockHolders()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.SD_34.Include(_ => _.StockHolders)
                    .AsNoTracking().Where(_ => _.DATE_ORD >= DateStart && _.DATE_ORD <= DateEnd
                                                                       && _.StockHolderId != null).ToList();
                foreach (var d in data)
                {
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        Date = d.DATE_ORD ?? DateTime.MinValue,
                        GroupId = ProfitAndLossesMainRowViewModel.StockHolderPay,
                        Name = d.StockHolders.Name,
                        DocCode = d.DOC_CODE,
                        Quantity = 1,
                        Kontragent = null,
                        DocTypeCode = DocumentType.CashOut,
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC) as Currency,
                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(d.CRS_DC)).Name,
                        DocNum = d.NUM_ORD.ToString(),
                        Price = d.CRS_SUMMA ?? 0
                    };
                    SetCurrenciesValue(newOp, d.CRS_DC, 0,
                        d.CRS_SUMMA);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        public void CalcTransferOutBalans()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.TransferOutBalansRows.Include(_ => _.TransferOutBalans)
                    .Where(_ => _.TransferOutBalans.DocDate >= DateStart && _.TransferOutBalans.DocDate <= DateEnd);
                foreach (var d in data)
                {
                    var nom = GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC);
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        Id = d.DocId,
                        Date = d.TransferOutBalans.DocDate,
                        GroupId = ProfitAndLossesMainRowViewModel.TransferOutBalans,
                        Name = "Вывод за баланс",
                        DocCode = 0,
                        Quantity = d.Quatntity,
                        Kontragent = null,
                        DocTypeCode = DocumentType.TransferOutBalans,
                        Currency = nom.Currency as Currency,
                        CurrencyName = (nom?.Currency as Currency)?.Name,
                        DocNum = d.TransferOutBalans.DocNum.ToString(),
                        Price = d.Price
                    };
                    SetCurrenciesValue(newOp, ((IDocCode)nom.Currency).DocCode, 0,
                        d.Quatntity * d.Price);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        public void CalcDilers()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                List<TD_24> data;
                if (Project == null)
                    data = ctx.TD_24.Include(_ => _.SD_24)
                        .Include(_ => _.TD_84)
                        .Include(_ => _.TD_84.SD_84)
                        .Where(_ => _.SD_24.DD_DATE >= DateStart && _.SD_24.DD_DATE <= DateEnd
                                                                 && _.TD_84.SD_84.SF_DILER_DC != null)
                        .ToList();
                else
                    data = ctx.TD_24.Include(_ => _.SD_24)
                        .Include(_ => _.TD_84)
                        .Include(_ => _.TD_84.SD_84)
                        .Where(_ => _.SD_24.DD_DATE >= DateStart && _.SD_24.DD_DATE <= DateEnd
                                                                 && _.TD_84.SD_84.SF_DILER_DC != null &&
                                                                 ProjectDocDC.Contains(_.TD_84.DOC_CODE))
                        .ToList();
                foreach (var d in data)
                {
                    var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.TD_84.SD_84.SF_DILER_DC) as Kontragent;
                    var nom =
                        GlobalOptions.ReferencesCache.GetNomenkl(d.DDT_NOMENKL_DC) as KursDomain.References.Nomenkl;
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        Date = d.SD_24.DD_DATE,
                        GroupId = Guid.Parse("{BA628F86-6AE4-4CF3-832B-C6F7388DD01B}"),
                        // ReSharper disable once PossibleNullReferenceException
                        Name = nom.Name,
                        DocCode = d.TD_84.DOC_CODE,
                        Quantity = d.DDT_KOL_RASHOD,
                        // ReSharper disable once PossibleNullReferenceException
                        Kontragent = kontr.Name,
                        DocTypeCode = (DocumentType)84
                    };
                    if (d.TD_84 != null)
                        SetCurrenciesValue(newOp, ((IDocCode)kontr.Currency).DocCode, 0m,
                            d.TD_84.SFT_NACENKA_DILERA * d.DDT_KOL_RASHOD);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void CalcUslugiDilers()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                List<TD_84> data;
                if (Project == null)
                    data = ctx.TD_84
                        .Include(_ => _.SD_84)
                        .Include(_ => _.SD_83)
                        .Where(_ => _.SD_83.NOM_0MATER_1USLUGA == 1 && _.SD_84.SF_DATE >= DateStart &&
                                    _.SD_84.SF_DATE <= DateEnd
                                    && _.SD_84.SF_DILER_DC != null)
                        .ToList();
                else
                    data = ctx.TD_84
                        .Include(_ => _.SD_84)
                        .Include(_ => _.SD_83)
                        .Where(_ => _.SD_83.NOM_0MATER_1USLUGA == 1 && _.SD_84.SF_DATE >= DateStart &&
                                    _.SD_84.SF_DATE <= DateEnd
                                    && _.SD_84.SF_DILER_DC != null &&
                                    ProjectDocDC.Contains(_.DOC_CODE))
                        .ToList();
                foreach (var d in data)
                {
                    var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.SD_84.SF_DILER_DC) as Kontragent;
                    var nom =
                        GlobalOptions.ReferencesCache.GetNomenkl(d.SD_83.DOC_CODE) as KursDomain.References.Nomenkl;
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{BA628F86-6AE4-4CF3-832B-C6F7388DD01B}"),
                        Name = nom.Name,
                        Date = d.SD_84.SF_DATE,
                        DocCode = d.DOC_CODE,
                        Quantity = (decimal)d.SFT_KOL,
                        Kontragent = kontr.Name,
                        DocTypeCode = (DocumentType)84
                    };
                    SetCurrenciesValue(newOp, ((IDocCode)kontr.Currency).DocCode, 0,
                        d.SFT_NACENKA_DILERA * (decimal)d.SFT_KOL);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        public void CalcZarplata()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var data = (from sd34 in ent.SD_34
                    from sd2 in ent.SD_2
                    where sd34.TABELNUMBER == sd2.TABELNUMBER
                          && sd34.DATE_ORD >= DateStart && sd34.DATE_ORD <= DateEnd
                          && sd34.NCODE == 100
                    select new
                    {
                        Date = sd34.DATE_ORD,
                        SotrCrsDC = sd2.crs_dc,
                        TabelNumber = sd2.TABELNUMBER,
                        Name = sd2.NAME,
                        Summa = sd34.SUMM_ORD,
                        OperCrsDC = sd34.CRS_DC,
                        SDRSchetDC = sd34.SHPZ_DC
                    }).ToList();
                var emps = ent.SD_2.ToList();
                foreach (var d in data)
                {
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{B96B2906-C5AA-4566-B77F-F3E4B912E72E}"),
                        Name = emps.Single(_ => _.TABELNUMBER == d.TabelNumber).NAME,
                        Date = d.Date ?? DateTime.MinValue,
                        DocTypeCode = (DocumentType)903,
                        SDRSchet = d.SDRSchetDC != null
                            ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                            : null,
                        SDRState = d.SDRSchetDC == null
                            ? null
                            : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                    };
                    SetCurrenciesValue(newOp, d.OperCrsDC, 0, d.Summa);

                    Extend.Add(newOp);
                }

                var data2 = (from td101 in ent.TD_101.Include(_ => _.SD_101)
                        from sd2 in ent.SD_2
                            where sd2.DOC_CODE == td101.EmployeeDC
                                && td101.SD_101.VV_START_DATE >= DateStart && td101.SD_101.VV_START_DATE <= DateEnd
                        select new
                        {
                            Date = td101.SD_101.VV_START_DATE,
                            SotrCrsDC = sd2.crs_dc,
                            TabelNumber = sd2.TABELNUMBER,
                            Name = sd2.NAME,
                            Summa = td101.VVT_VAL_RASHOD,
                            OperCrsDC = sd2.crs_dc,
                            SDRSchetDC = td101.VVT_SHPZ_DC
                        }).ToList();
                foreach (var d in data2)
                {
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{B96B2906-C5AA-4566-B77F-F3E4B912E72E}"),
                        Name = emps.Single(_ => _.TABELNUMBER == d.TabelNumber).NAME,
                        Date = d.Date,
                        DocTypeCode = (DocumentType)903,
                        SDRSchet = d.SDRSchetDC != null
                            ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                            : null,
                        SDRState = d.SDRSchetDC == null
                            ? null
                            : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                    };
                    SetCurrenciesValue(newOp, d.OperCrsDC, 0, d.Summa);

                    Extend.Add(newOp);
                }
            }
        }

        public static void SetCurrenciesValue(IProfitCurrencyList item, decimal? crsDC, decimal? profit, decimal? loss)
        {
            SetCurrenciesValue(item, crsDC ?? 0, profit ?? 0m, loss ?? 0m);
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

        public void CalcZarplataNach()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var dataNach = (from empRows in ent.EMP_PR_ROWS.Include(_ => _.EMP_PR_DOC)
                    from sd2 in ent.SD_2
                    from empNachHead in ent.EMP_PR_DOC
                    where empRows.EMP_DC == sd2.DOC_CODE
                          && empNachHead.ID == empRows.ID
                          && empRows.NachDate >= DateStart && empRows.NachDate <= DateEnd
                    select new
                    {
                        empRows.NachDate,
                        SotrCrsDC = sd2.crs_dc,
                        TabelNumber = sd2.TABELNUMBER,
                        Name = sd2.NAME,
                        Summa = empRows.SUMMA,
                        OperCrsDC = empRows.CRS_DC,
                        IsShablon = empNachHead.IS_TEMPLATE,
                        Id = empRows.EMP_PR_DOC.ID
                    }).ToList();
                var emps = ent.SD_2.ToList();
                foreach (var d in dataNach)
                {
                    if (d.IsShablon == 1) continue;
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{B96B2906-C5AA-4566-B77F-F3E4B912E72E}"),
                        Name = emps.Single(_ => _.TABELNUMBER == d.TabelNumber).NAME,
                        Date = d.NachDate ?? DateTime.Today,
                        DocTypeCode = (DocumentType)903,
                        StringId = d.Id
                    };
                    SetCurrenciesValue(newOp, d.OperCrsDC, 0, d.Summa);
                    ExtendNach.Add(newOp);
                }
            }
        }

        public void CalcTovarTransfer()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.NomenklTransferRow
                    .Include(_ => _.NomenklTransfer)
                    .Include(_ => _.SD_83)
                    .Include(_ => _.SD_831)
                    .Where(
                        _ =>
                            _.NomenklTransfer.Date >= DateStart && _.NomenklTransfer.Date <= DateEnd &&
                            _.IsAccepted);
                foreach (var row in data)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    // ReSharper disable once PossibleInvalidOperationException
                    var crs = GlobalOptions.ReferencesCache.GetCurrency(row.SD_831.NOM_SALE_CRS_DC) as Currency;
                    // ReSharper disable once AssignNullToNotNullAttribute
                    // ReSharper disable once PossibleInvalidOperationException
                    var crs1 = GlobalOptions.ReferencesCache.GetCurrency(row.SD_83.NOM_SALE_CRS_DC) as Currency;
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{564DB69C-6DAD-4B16-8BF5-118F5AF2D07F}"),
                        DocTypeCode = DocumentType.NomenklTransfer,
                        Name = row.SD_83.NOM_NAME,
                        Note = row.Note,
                        //DocCode = row.DocCode,
                        Quantity = row.Quantity,
                        Price = row.PriceIn,
                        Date = row.NomenklTransfer.Date,
                        Kontragent = null,
                        CurrencyName = crs.Name,
                        Nomenkl =
                            GlobalOptions.ReferencesCache.GetNomenkl(row.SD_83.DOC_CODE) as
                                KursDomain.References.Nomenkl
                    };
                    SetCurrenciesValue(newOp, crs.DocCode, row.PriceIn * row.Quantity, 0m);
                    var price = NomenklViewModel.PriceWithOutNaklad(row.SD_83.DOC_CODE,
                        row.NomenklTransfer.Date);
                    var newOp1 = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{564DB69C-6DAD-4B16-8BF5-118F5AF2D07F}"),
                        Name = row.SD_831.NOM_NAME,
                        Note = row.Note,
                        DocTypeCode = DocumentType.NomenklTransfer,
                        //DocCode = row.DocCode,
                        Quantity = row.Quantity,
                        Price = price,
                        Date = row.NomenklTransfer.Date,
                        Kontragent = null,
                        CurrencyName = crs1.Name,
                        Nomenkl =
                            GlobalOptions.ReferencesCache.GetNomenkl(row.SD_83.DOC_CODE) as
                                KursDomain.References.Nomenkl
                    };
                    SetCurrenciesValue(newOp1, crs1.DocCode, 0m, price * row.Quantity);
                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                    Extend.Add(newOp1);
                    ExtendNach.Add(newOp1);
                }

                var idNach = Guid.Parse("{564DB69C-6DAD-4B16-8BF5-118F5AF2D07F}");
                //var idNach1 = Guid.Parse("{04EBD711-D11C-415B-9ECD-BAC074EF588D}");
                var n = Main.FirstOrDefault(_ => _.Id == Guid.Parse("{564DB69C-6DAD-4B16-8BF5-118F5AF2D07F}"));
                var n1 = MainNach.FirstOrDefault(_ => _.Id == Guid.Parse("{564DB69C-6DAD-4B16-8BF5-118F5AF2D07F}"));

                // ReSharper disable once PossibleNullReferenceException
                n.LossRUB = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossRUB);
                n.ProfitRUB = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitRUB);
                n.ResultRUB = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitRUB - _.LossRUB);
                n.LossUSD = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossUSD);
                n.ProfitUSD = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitUSD);
                n.ResultUSD = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitUSD - _.LossUSD);
                n.LossEUR = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossEUR);
                n.ProfitEUR = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitEUR);
                n.ResultEUR = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitEUR - _.LossEUR);
                n.LossGBP = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossGBP);
                n.ProfitGBP = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitGBP);
                n.ResultGBP = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitGBP - _.LossGBP);
                n.LossCHF = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossCHF);
                n.ProfitCHF = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitCHF);
                n.ResultCHF = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitCHF - _.LossCHF);
                n.LossSEK = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossSEK);
                n.ProfitSEK = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitSEK);
                n.ResultSEK = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitSEK - _.LossSEK);

                // ReSharper disable once PossibleNullReferenceException
                n1.LossRUB = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossRUB);
                n1.ProfitRUB = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitRUB);
                n1.ResultRUB = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitRUB - _.LossRUB);
                n1.LossUSD = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossUSD);
                n1.ProfitUSD = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitUSD);
                n1.ResultUSD = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitUSD - _.LossUSD);
                n1.LossEUR = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossEUR);
                n1.ProfitEUR = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitEUR);
                n1.ResultEUR = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitEUR - _.LossEUR);
                n1.LossGBP = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossGBP);
                n1.ProfitGBP = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitGBP);
                n1.ResultGBP = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitGBP - _.LossGBP);
                n1.LossCHF = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossCHF);
                n1.ProfitCHF = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitCHF);
                n1.ResultCHF = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitCHF - _.LossCHF);
                n1.LossSEK = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.LossSEK);
                n1.ProfitSEK = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitSEK);
                n1.ResultSEK = Extend.Where(_ => _.GroupId == idNach).Sum(_ => _.ProfitSEK - _.LossSEK);
            }
        }

        // ReSharper disable once UnusedMember.Local
        public void CalcNomInRounding()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var dataTemp = ent.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.TD_26)
                    .Where(
                        _ =>
                            _.TD_26 != null && _.SD_24.DD_DATE >= DateStart && _.SD_24.DD_DATE <= DateEnd &&
                            // ReSharper disable once EqualExpressionComparison
                            _.TD_26.SFT_ED_CENA.Value != _.TD_26.SFT_ED_CENA.Value);
                foreach (var d in dataTemp)
                {
                    var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.SD_24.DD_KONTR_OTPR_DC) as Kontragent;
                    // ReSharper disable once EqualExpressionComparison
                    // ReSharper disable once PossibleInvalidOperationException
                    if (d.TD_26.SFT_ED_CENA.Value < d.TD_26.SFT_ED_CENA.Value)
                    {
                        var e = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{510BCC25-5EE0-467D-B459-8F51BDA0A4D9}"),
                            Name = "Приход товара",
                            Note = "",
                            DocCode = d.DOC_CODE,
                            Quantity = d.DDT_KOL_PRIHOD,
                            Price = d.TD_26.SFT_ED_CENA.Value - d.TD_26.SFT_ED_CENA.Value,
                            Date = d.SD_24.DD_DATE,
                            Kontragent = kontr.Name,
                            CurrencyName = ((IName)kontr.Currency).Name,
                            DocTypeCode = DocumentType.StoreOrderIn
                        };
                        SetCurrenciesValue(e, ((IDocCode)kontr.Currency).DocCode, 0m,
                            Convert.ToDecimal(d.TD_26.SFT_ED_CENA.Value - d.TD_26.SFT_ED_CENA.Value) *
                            d.DDT_KOL_PRIHOD);

                        Extend.Add(e);
                        ExtendNach.Add(e);
                    }
                    else
                    {
                        var e = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{3520934E-61CC-4964-9E7B-D3D19913E52B}"),
                            Name = "Приход товара",
                            Note = "",
                            DocCode = d.DOC_CODE,
                            Quantity = d.DDT_KOL_PRIHOD,
                            Price = d.TD_26.SFT_ED_CENA.Value - d.TD_26.SFT_ED_CENA.Value,
                            Date = d.SD_24.DD_DATE,
                            Kontragent = kontr.Name,
                            CurrencyName = ((IName)kontr.Currency).Name,
                            DocTypeCode = DocumentType.StoreOrderIn
                        };
                        SetCurrenciesValue(e, ((IDocCode)kontr.Currency).DocCode,
                            Convert.ToDecimal(d.TD_26.SFT_ED_CENA.Value - d.TD_26.SFT_ED_CENA.Value) * d.DDT_KOL_PRIHOD,
                            0m);

                        Extend.Add(e);
                        ExtendNach.Add(e);
                    }
                }
            }
        }

        // ReSharper disable once FunctionComplexityOverflow
        public void CalcUslugi()
        {
            // ReSharper disable once JoinDeclarationAndInitializer
            // ReSharper disable once CollectionNeverQueried.Local
            Dictionary<decimal, string> co;
            co
                =
                new Dictionary<decimal, string>();
            foreach (
                var c
                in
                GlobalOptions.ReferencesCache.GetCentrResponsibilitiesAll().Cast<CentrResponsibility>()
            )
                co.Add(c.DocCode, c.Name);
            using (
                var ent = GlobalOptions.GetEntities())
            {
                var dataIn = (from sd84 in ent.SD_84.Include(_ => _.SD_40)
                    // ReSharper disable once AccessToDisposedClosure
                    from td84 in ent.TD_84
                    // ReSharper disable once AccessToDisposedClosure
                    from sd83 in ent.SD_83
                    //from sd40 in ent.SD_40
                    // ReSharper disable once AccessToDisposedClosure
                    from sd43 in ent.SD_43
                    where sd84.DOC_CODE == td84.DOC_CODE && sd84.SF_ACCEPTED == 1
                                                         && sd83.DOC_CODE == td84.SFT_NEMENKL_DC
                                                         && sd43.DOC_CODE == sd84.SF_CLIENT_DC
                                                         && sd84.SF_DATE >= DateStart
                                                         && sd43.FLAG_BALANS == 1
                                                         && sd83.NOM_0MATER_1USLUGA == 1
                                                         && sd84.SF_DATE >= DateStart && sd84.SF_DATE <= DateEnd
                    //&& (sd84.SF_DATE >= sd43.START_BALANS)
                    select new ProfitAndLossesWindowViewModel.NakladTemp
                    {
                        DocCode = sd84.DOC_CODE,
                        Id = Guid.NewGuid(),
                        Date = sd84.SF_DATE,
                        NomenklDC = sd83.DOC_CODE,
                        Name = sd83.NOM_NAME,
                        // ReSharper disable once MergeConditionalExpression
                        COName = sd84.SD_40 != null ? sd84.SD_40.CENT_NAME : "ЦО не указан",
                        DoubleQuantity = td84.SFT_KOL,
                        Summa = td84.SFT_SUMMA_K_OPLATE_KONTR_CRS,
                        KontrCrsDC = sd43.VALUTA_DC,
                        Kontragent = sd43.NAME,
                        KontragentDC = sd43.DOC_CODE,
                        SDRSchetDC = td84.SFT_SHPZ_DC
                    }).ToList();
                foreach (var dd in dataIn) dd.Quantity = Convert.ToDecimal(dd.DoubleQuantity);
                var dataOut = (from sd26 in ent.SD_26.Include(_ => _.SD_40)
                    // ReSharper disable once AccessToDisposedClosure
                    from td26 in ent.TD_26
                    // ReSharper disable once AccessToDisposedClosure
                    from sd83 in ent.SD_83
                    //from sd40 in ent.SD_40
                    // ReSharper disable once AccessToDisposedClosure 
                    from sd43 in ent.SD_43
                    where sd26.DOC_CODE == td26.DOC_CODE && sd26.SF_ACCEPTED == 1
                                                         && sd83.DOC_CODE == td26.SFT_NEMENKL_DC
                                                         && sd43.DOC_CODE == sd26.SF_POST_DC
                                                         && sd26.SF_POSTAV_DATE >= DateStart &&
                                                         sd26.SF_POSTAV_DATE <= DateEnd
                                                         && (sd43.FLAG_BALANS ?? 0) == 1
                                                         && sd83.NOM_0MATER_1USLUGA == 1
                    //&& (td26.SFT_NAKLAD_KONTR_DC == null ||
                    //    td26.SFT_NAKLAD_KONTR_DC == sd26.SF_POST_DC)
                    //&& td26.SFT_IS_NAKLAD == 0
                    //&& sd26.IsInvoiceNakald  == false
                    select new ProfitAndLossesWindowViewModel.NakladTemp
                    {
                        DocCode = sd26.DOC_CODE,
                        Code = td26.CODE,
                        Id = Guid.NewGuid(),
                        Date = sd26.SF_POSTAV_DATE,
                        NomenklDC = sd83.DOC_CODE,
                        Name = sd83.NOM_NAME,
                        // ReSharper disable once MergeConditionalExpression
                        //COName =   MainReferences.COList[sd26.SF_CENTR_OTV_DC ?? 0].Name,
                        CODC = sd26.SF_CENTR_OTV_DC,
                        Quantity = td26.SFT_KOL,
                        Summa = (decimal)td26.SFT_SUMMA_K_OPLATE_KONTR_CRS,
                        KontrCrsDC = sd43.VALUTA_DC,
                        Kontragent = sd43.NAME,
                        KontragentDC = sd43.DOC_CODE,
                        SDRSchetDC = td26.SFT_SHPZ_DC,
                        IsNaklad = td26.SFT_IS_NAKLAD == 1 || (sd26.IsInvoiceNakald ?? false)
                    }).ToList();
                foreach (var c in dataOut)
                    c.COName = ((IName)GlobalOptions.ReferencesCache.GetCentrResponsibility(c.CODC)).Name;
                //var naklad = ent.KONTR_BALANS_OPER_ARC.Where(_ => _.DOC_DATE >= DateStart && _.DOC_DATE <= DateEnd
                //                                                                          && _.DOC_NAME ==
                //                                                                          "С/ф от поставщика (накладные расходы как услуги)")
                //    .ToList();
                List<string> dictCOIns1, dictCOOut1;
                Dictionary<string, Guid> dictCOIns, dictCOOuts;
                if (Project == null)
                {
                    dictCOIns1 = new List<string>(dataIn.Select(_ => _.COName).Distinct());
                    dictCOIns = dictCOIns1.ToDictionary(d => d, _ => Guid.NewGuid());
                    foreach (var d in dataIn)
                        if (!dictCOIns.ContainsKey(d.COName))
                            dictCOIns.Add(d.COName, Guid.NewGuid());
                    dictCOOut1 = new List<string>(dataOut.Select(_ => _.COName).Distinct());
                    dictCOOuts = dictCOOut1.ToDictionary(d => d, _ => Guid.NewGuid());
                    foreach (var d in dataOut)
                        if (!dictCOOuts.ContainsKey(d.COName))
                            dictCOOuts.Add(d.COName, Guid.NewGuid());
                }
                else
                {
                    dictCOIns1 = new List<string>(dataIn.Where(_ => ProjectDocDC.Contains(_.DocCode))
                        .Select(_ => _.COName).Distinct());
                    dictCOIns = dictCOIns1.ToDictionary(d => d, _ => Guid.NewGuid());
                    foreach (var d in dataIn.Where(_ => ProjectDocDC.Contains(_.DocCode)))
                        if (!dictCOIns.ContainsKey(d.COName))
                            dictCOIns.Add(d.COName, Guid.NewGuid());
                    dictCOOut1 = new List<string>(dataOut.Where(_ => ProjectDocDC.Contains(_.DocCode))
                        .Select(_ => _.COName).Distinct());
                    dictCOOuts = dictCOOut1.ToDictionary(d => d, _ => Guid.NewGuid());
                    foreach (var d in dataOut.Where(_ => ProjectDocDC.Contains(_.DocCode)))
                        if (!dictCOOuts.ContainsKey(d.COName))
                            dictCOOuts.Add(d.COName, Guid.NewGuid());
                }

                if (Project == null)
                {
                    foreach (var d in dataIn)
                    {
                        var nom =
                            GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as KursDomain.References.Nomenkl;
                        var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent;
                        //var kontrRate = GetRate(myRates, (decimal) d.KontrCrsDC,
                        //    GlobalOptions.SystemProfile.MainCurrency.DocCode, d.Date);
                        SDRSchet sdrSchet = null;
                        if (d.SDRSchetDC != null)
                            // ReSharper disable once PossibleInvalidOperationException
                            sdrSchet = GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC) as SDRSchet;
                        var e = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = dictCOIns[d.COName],
                            Name = nom.Name,
                            Note = "Ном.№ - " + nom.NomenklNumber,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = d.Summa ?? 0m / d.Quantity,
                            Date = d.Date,
                            Kontragent = d.Kontragent,
                            DocTypeCode = DocumentType.InvoiceClient,
                            // ReSharper disable once PossibleInvalidOperationException
                            SDRSchet = sdrSchet,
                            SDRState = GetSdrState(sdrSchet)
                        };
                        SetCurrenciesValue(e, ((IDocCode)kontr.Currency).DocCode, d.Summa, 0m);

                        Extend.Add(e);
                        ExtendNach.Add(e);
                    }

                    foreach (var d in dataOut)
                    {
                        var nom =
                            GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as KursDomain.References.Nomenkl;
                        var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent;
                        //var kontrRate = GetRate(myRates, (decimal) d.KontrCrsDC,
                        //    GlobalOptions.SystemProfile.MainCurrency.DocCode, d.Date);
                        SDRSchet sdrSchet = null;
                        if (d.SDRSchetDC != null)
                            // ReSharper disable once PossibleInvalidOperationException
                            sdrSchet = GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet;
                        var e = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = dictCOOuts[d.COName],
                            Name = nom.Name,
                            Note = "Ном.№ - " + nom.NomenklNumber,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (d.Summa ?? 0) / d.Quantity,
                            Date = d.Date,
                            Kontragent = d.Kontragent,
                            DocTypeCode = DocumentType.InvoiceProvider,
                            // ReSharper disable once PossibleInvalidOperationException
                            SDRSchet = sdrSchet,
                            SDRState = GetSdrState(sdrSchet)
                        };
                        SetCurrenciesValue(e, ((IDocCode)kontr.Currency).DocCode, 0m, d.Summa);

                        Extend.Add(e);
                        ExtendNach.Add(e);
                    }
                }
                else
                {
                    foreach (var d in dataIn.Where(_ => ProjectDocDC.Contains(_.DocCode)))
                    {
                        var nom =
                            GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as KursDomain.References.Nomenkl;
                        var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent;
                        SDRSchet sdrSchet = null;
                        if (d.SDRSchetDC != null)
                            // ReSharper disable once PossibleInvalidOperationException
                            sdrSchet = GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet;
                        var e = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = dictCOIns[d.COName],
                            Name = nom.Name,
                            Note = "Ном.№ - " + nom.NomenklNumber,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = d.Summa ?? 0m / d.Quantity,
                            Date = d.Date,
                            Kontragent = d.Kontragent,
                            DocTypeCode = DocumentType.InvoiceClient,
                            SDRSchet = sdrSchet,
                            SDRState = GetSdrState(sdrSchet)
                        };
                        SetCurrenciesValue(e, ((IDocCode)kontr.Currency).DocCode, d.Summa, 0m);

                        Extend.Add(e);
                        ExtendNach.Add(e);
                    }

                    foreach (var d in dataOut.Where(_ => ProjectDocDC.Contains(_.DocCode)))
                    {
                        var nom =
                            GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as KursDomain.References.Nomenkl;
                        var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontragentDC) as Kontragent;
                        SDRSchet sdrSchet = null;
                        if (d.SDRSchetDC != null)
                            // ReSharper disable once PossibleInvalidOperationException
                            sdrSchet = GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet;
                        var e = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = dictCOOuts[d.COName],
                            Name = nom.Name,
                            Note = "Ном.№ - " + nom.NomenklNumber,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (d.Summa ?? 0) / d.Quantity,
                            Date = d.Date,
                            Kontragent = d.Kontragent,
                            DocTypeCode = DocumentType.InvoiceProvider,
                            SDRSchet = sdrSchet,
                            SDRState = GetSdrState(sdrSchet)
                        };
                        SetCurrenciesValue(e, ((IDocCode)kontr.Currency).DocCode, 0m, d.Summa);

                        Extend.Add(e);
                        ExtendNach.Add(e);
                    }
                }

                var rm1 = Main.Where(_ => _.ParentId == Guid.Parse("{2FA1DD9F-6842-4209-B0CC-DDEF3B920496}")).ToList();
                var rmn1 = MainNach.Where(_ => _.ParentId == Guid.Parse("{2FA1DD9F-6842-4209-B0CC-DDEF3B920496}"))
                    .ToList();
                foreach (var m in rm1) Main.Remove(m);
                foreach (var mn in rmn1) MainNach.Remove(mn);
                foreach (
                    var n in dictCOIns.Select(d =>
                        new
                            ProfitAndLossesMainRowViewModel
                            {
                                Id = d.Value,
                                ParentId = Guid.Parse
                                    ("{2FA1DD9F-6842-4209-B0CC-DDEF3B920496}"),
                                Name = d.Key,
                                CalcType = TypeProfitAndLossCalc.IsProfit,
                                LossRUB = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossRUB),
                                ProfitRUB = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitRUB),
                                ResultRUB = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultRUB),
                                LossUSD = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossUSD),
                                ProfitUSD = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitUSD),
                                ResultUSD = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultUSD),
                                LossEUR = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossEUR),
                                ProfitEUR = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitEUR),
                                ResultEUR = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultEUR),
                                LossGBP = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossGBP),
                                ProfitGBP = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitGBP),
                                ResultGBP = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultGBP),
                                LossCHF = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossCHF),
                                ProfitCHF = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitCHF),
                                ResultCHF = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultCHF),
                                LossSEK = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossSEK),
                                ProfitSEK = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitSEK),
                                ResultSEK = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultSEK)
                            }
                    ))
                {
                    Main.Add(n);
                    MainNach.Add(n);
                }

                var rm = Main.Where(_ => _.ParentId == Guid.Parse("{E47EF726-3BEA-4B18-9773-E564D624FDF6}")).ToList();
                var rmn = MainNach.Where(_ => _.ParentId == Guid.Parse("{E47EF726-3BEA-4B18-9773-E564D624FDF6}"))
                    .ToList();
                foreach (var m in rm) Main.Remove(m);
                foreach (var mn in rmn) MainNach.Remove(mn);
                foreach (
                    var n in dictCOOuts.Select(d =>
                        new
                            ProfitAndLossesMainRowViewModel
                            {
                                Id = d.Value,
                                ParentId = Guid.Parse("{E47EF726-3BEA-4B18-9773-E564D624FDF6}"),
                                Name = d.Key,
                                CalcType = TypeProfitAndLossCalc.IsLoss,
                                LossRUB = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossRUB),
                                ProfitRUB = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitRUB),
                                ResultRUB = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultRUB),
                                LossUSD = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossUSD),
                                ProfitUSD = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitUSD),
                                ResultUSD = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultUSD),
                                LossEUR = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossEUR),
                                ProfitEUR = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitEUR),
                                ResultEUR = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultEUR),
                                LossGBP = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossGBP),
                                ProfitGBP = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitGBP),
                                ResultGBP = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultGBP),
                                LossCHF = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossCHF),
                                ProfitCHF = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitCHF),
                                ResultCHF = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultCHF),
                                LossSEK = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.LossSEK),
                                ProfitSEK = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ProfitSEK),
                                ResultSEK = Extend.Where(_ => _.GroupId == d.Value).Sum(_ => _.ResultSEK)
                            }
                    ))
                {
                    Main.Add(n);
                    MainNach.Add(n);
                }
            }
        }

        private SDRState GetSdrState(SDRSchet sdrSchet)
        {
            return sdrSchet?.SDRState as SDRState;
        }

        public void CalcProviderSupply()
        {
            var dclist = getProjectIdsRecursively(Project.Id);
            using (var ent = GlobalOptions.GetEntities())
            {
                var dataTovarPoluchen = (from prjDoc in ent.ProjectProviderPrihod
                    from td24 in ent.TD_24
                    from sd24 in ent.SD_24
                    from sd43 in ent.SD_43
                    from td26 in ent.TD_26
                    from sd83 in ent.SD_83
                    from sd26 in ent.SD_26
                    where td24.DOC_CODE == sd24.DOC_CODE
                          && dclist.Contains(prjDoc.ProjectId) && td24.Id == prjDoc.RowId
                          && sd24.DD_DATE >= DateStart && sd24.DD_DATE <= DateEnd
                          && sd43.DOC_CODE == sd24.DD_KONTR_OTPR_DC
                          && sd24.DD_DATE >= DateStart
                          && sd83.DOC_CODE == td24.DDT_NOMENKL_DC
                          && td26.DOC_CODE == td24.DDT_SPOST_DC && td26.CODE == td24.DDT_SPOST_ROW_CODE
                          && sd26.DOC_CODE == td26.DOC_CODE
                    select new
                    {
                        DocDC = sd26.DOC_CODE,
                        Date = sd24.DD_DATE,
                        NomenklCrsDC = sd83.NOM_SALE_CRS_DC,
                        Name = sd83.NOM_NAME + "(" + sd83.NOM_NOMENKL + ")",
                        Note = "С/ф №" + sd26.SF_IN_NUM + " от " + sd26.SF_POSTAV_DATE + " Поставщик - " + sd43.NAME,
                        DocCode = sd83.DOC_CODE,
                        Price = td26.SFT_ED_CENA,
                        Quantity = td24.DDT_KOL_PRIHOD,
                        Profit = 0,
                        Loss = td26.SFT_ED_CENA * (td26.SFT_KOL / td24.DDT_KOL_PRIHOD),
                        Result = td26.SFT_ED_CENA * (td26.SFT_KOL / td24.DDT_KOL_PRIHOD),
                        KontrName = sd43.NAME,
                        NomenklDC = sd83.DOC_CODE,
                        SDRSchetDC = td24.DDT_SHPZ_DC
                    }).ToList();
                foreach (var d in dataTovarPoluchen)
                {
                    var nom = GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as KursDomain.References.Nomenkl;
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{D89B1E18-074E-4A7D-A0EE-9537DC1585D8}"),
                        Name = d.Name,
                        Note = d.Note,
                        DocCode = d.DocCode,
                        Quantity = d.Quantity,
                        Price = d.Price ?? 0m,
                        Date = d.Date,
                        Kontragent = d.KontrName,
                        DocTypeCode = DocumentType.StoreOrderIn,
                        SDRSchet = d.SDRSchetDC != null
                            ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                            : null,
                        SDRState = d.SDRSchetDC == null
                            ? null
                            : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                    };
                    SetCurrenciesValue(newOp, ((IDocCode)nom.Currency).DocCode, 0m, d.Loss);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        public void CalcOutBalans()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var dataTovarPoluchen = (from td24 in ent.TD_24
                    from sd24 in ent.SD_24
                    from sd43 in ent.SD_43
                    from td26 in ent.TD_26
                    from sd83 in ent.SD_83
                    from sd26 in ent.SD_26
                    where td24.DOC_CODE == sd24.DOC_CODE
                          && sd24.DD_DATE >= DateStart && sd24.DD_DATE <= DateEnd
                          && sd43.DOC_CODE == sd24.DD_KONTR_OTPR_DC
                          && sd43.FLAG_BALANS == 0 && sd24.DD_DATE >= DateStart
                          && sd83.DOC_CODE == td24.DDT_NOMENKL_DC
                          && td26.DOC_CODE == td24.DDT_SPOST_DC && td26.CODE == td24.DDT_SPOST_ROW_CODE
                          && sd26.DOC_CODE == td26.DOC_CODE
                          && sd24.DD_KONTR_OTPR_DC != GlobalOptions.SystemProfile.OwnerKontragent.DocCode
                    select new
                    {
                        DocDC = sd26.DOC_CODE,
                        Date = sd24.DD_DATE,
                        NomenklCrsDC = sd83.NOM_SALE_CRS_DC,
                        Name = sd83.NOM_NAME + "(" + sd83.NOM_NOMENKL + ")",
                        Note = "С/ф №" + sd26.SF_IN_NUM + " от " + sd26.SF_POSTAV_DATE + " Поставщик - " + sd43.NAME,
                        DocCode = sd83.DOC_CODE,
                        Price = td26.SFT_ED_CENA,
                        Quantity = td24.DDT_KOL_PRIHOD,
                        Profit = td26.SFT_ED_CENA * (td26.SFT_KOL / td24.DDT_KOL_PRIHOD),
                        Loss = 0,
                        Result = td26.SFT_ED_CENA * (td26.SFT_KOL / td24.DDT_KOL_PRIHOD),
                        KontrName = sd43.NAME,
                        NomenklDC = sd83.DOC_CODE,
                        SDRSchetDC = td24.DDT_SHPZ_DC
                    }).ToList();

                // ReSharper disable AccessToDisposedClosure
                var dataTovarOtgr = (from td24 in ent.TD_24
                    from sd24 in ent.SD_24
                    from sd43 in ent.SD_43
                    from td84 in ent.TD_84
                    from sd83 in ent.SD_83
                    from sd84 in ent.SD_84
                    from nomPrice in ent.NOM_PRICE
                    where td24.DOC_CODE == sd24.DOC_CODE
                          && sd24.DD_DATE >= DateStart && sd24.DD_DATE <= DateEnd
                          && sd43.DOC_CODE == sd24.DD_KONTR_POL_DC
                          && sd43.FLAG_BALANS == 0
                          && sd83.DOC_CODE == td24.DDT_NOMENKL_DC
                          && td84.DOC_CODE == td24.DDT_SFACT_DC && td84.CODE == td24.DDT_SFACT_ROW_CODE
                          && sd84.DOC_CODE == td84.DOC_CODE
                          && sd24.DD_KONTR_OTPR_DC != GlobalOptions.SystemProfile.OwnerKontragent.DocCode
                          && nomPrice.NOM_DC == td24.DDT_NOMENKL_DC && nomPrice.DATE == ent.NOM_PRICE.Where(
                                  _ =>
                                      _.NOM_DC ==
                                      td24.DDT_NOMENKL_DC &&
                                      _.DATE <= sd24.DD_DATE)
                              .Max(_ => _.DATE)
                    select new
                    {
                        DocDC = sd84.DOC_CODE,
                        Date = sd24.DD_DATE,
                        NomenklCrsDC = sd83.NOM_SALE_CRS_DC,
                        Name = sd83.NOM_NAME + "(" + sd83.NOM_NOMENKL + ")",
                        Note = "С/ф №" + sd84.SF_IN_NUM + " от " + sd84.SF_DATE + " Получатель - " + sd43.NAME,
                        DocCode = sd83.DOC_CODE,
                        Price = td84.SFT_ED_CENA,
                        NomPrice =
                            GlobalOptions.SystemProfile.NomenklCalcType == NomenklCalcType.NakladSeparately
                                ? nomPrice.PRICE_WO_NAKLAD
                                : nomPrice.PRICE,
                        Quantity = td24.DDT_KOL_RASHOD,
                        Quantity2 = td84.SFT_KOL,
                        KonrtName = sd43.NAME,
                        NomenklDC = sd83.DOC_CODE,
                        SDRSchetDC = td24.DDT_SHPZ_DC
                    }).ToList();
                var dataBankIn = (from sd101 in ent.SD_101
                    from td101 in ent.TD_101
                    from sd43 in ent.SD_43
                    where sd101.DOC_CODE == td101.DOC_CODE &&
                          sd43.DOC_CODE == (td101.VVT_KONTRAGENT ?? 0) && (sd43.FLAG_BALANS ?? 0) == 0 &&
                          sd101.VV_STOP_DATE >= DateStart && sd101.VV_STOP_DATE <= DateEnd &&
                          td101.VVT_VAL_PRIHOD > 0
                          && td101.VVT_KONTRAGENT != GlobalOptions.SystemProfile.OwnerKontragent.DocCode
                          && td101.AccuredAmountForClientRow.Count == 0
                    select new
                    {
                        DocDC = sd101.DOC_CODE,
                        Date = sd101.VV_START_DATE,
                        CrsDC = td101.VVT_CRS_DC,
                        Code = td101.CODE,
                        Name = sd43.NAME ?? "Банковская выписка",
                        Note = "№" + td101.VVT_DOC_NUM + " Контрагент - " + sd43.NAME,
                        DocCode = sd101.DOC_CODE,
                        Price = td101.VVT_VAL_PRIHOD,
                        KontrDc = td101.VVT_KONTRAGENT,
                        Quantity = 1,
                        Profit = td101.VVT_VAL_PRIHOD,
                        Loss = 0,
                        Result = td101.VVT_VAL_PRIHOD,
                        OperCrsDC = td101.VVT_CRS_DC,
                        SDRSchetDC = td101.VVT_SHPZ_DC
                    }).ToList();
                var dataBankOut = (from sd101 in ent.SD_101
                    from td101 in ent.TD_101
                    from sd43 in ent.SD_43
                    where sd101.DOC_CODE == td101.DOC_CODE && sd43.DOC_CODE == (td101.VVT_KONTRAGENT ?? 0) &&
                          (sd43.FLAG_BALANS ?? 0) == 0 && sd101.VV_STOP_DATE >= DateStart &&
                          sd101.VV_STOP_DATE <= DateEnd && td101.VVT_VAL_RASHOD > 0
                          && td101.VVT_KONTRAGENT != GlobalOptions.SystemProfile.OwnerKontragent.DocCode
                          && td101.AccuredAmountOfSupplierRow == null
                    select new
                    {
                        DocDC = sd101.DOC_CODE,
                        Date = sd101.VV_START_DATE,
                        CrsDC = td101.VVT_CRS_DC,
                        Code = td101.CODE,
                        Name = sd43.NAME ?? "Банковская выписка",
                        Note = "№" + td101.VVT_DOC_NUM + " Контрагент - " + sd43.NAME,
                        DocCode = sd101.DOC_CODE,
                        KontrDc = td101.VVT_KONTRAGENT,
                        Price = td101.VVT_VAL_RASHOD,
                        Quantity = 1,
                        Profit = 0,
                        Loss = td101.VVT_VAL_RASHOD,
                        Result = -td101.VVT_VAL_RASHOD,
                        OperCrsDC = td101.VVT_CRS_DC,
                        SDRSchetDC = td101.VVT_SHPZ_DC
                    }).ToList();
                var dataCashIn = (from sd33 in ent.SD_33
                    // ReSharper disable once AccessToDisposedClosure
                    from sd43 in ent.SD_43
                    where sd33.DATE_ORD >= DateStart && sd33.DATE_ORD <= DateEnd
                                                     && sd43.DOC_CODE == sd33.KONTRAGENT_DC
                                                     && sd43.FLAG_BALANS == 0
                                                     && sd33.KONTRAGENT_DC != GlobalOptions.SystemProfile
                                                         .OwnerKontragent.DocCode
                                                     && sd33.AccuredAmountForClientRow.Count == 0
                    select new
                    {
                        DocDC = sd33.DOC_CODE,
                        Date = sd33.DATE_ORD,
                        CrsDC = sd33.CRS_DC,
                        Name = sd43.NAME, //"Приходный кассовый ордер",
                        Note = "№" + sd33.NUM_ORD + " от " + sd33.DATE_ORD + " Контрагент - " + sd43.NAME,
                        DocCode = sd33.DOC_CODE,
                        Price = sd33.SUMM_ORD,
                        Quantity = 1,
                        Profit = sd33.SUMM_ORD,
                        Loss = 0,
                        Result = sd33.SUMM_ORD,
                        KontrDC = sd33.KONTRAGENT_DC,
                        Percent = sd33.KONTR_CRS_SUM_CORRECT_PERCENT,
                        SDRSchetDC = sd33.SHPZ_DC
                    }).ToList();
                var dataCashOut = (from sd34 in ent.SD_34
                    // ReSharper disable once AccessToDisposedClosure
                    from sd43 in ent.SD_43
                    where sd34.DATE_ORD >= DateStart && sd34.DATE_ORD <= DateEnd
                                                     && sd43.DOC_CODE == sd34.KONTRAGENT_DC
                                                     && sd43.FLAG_BALANS == 0
                                                     && sd34.KONTRAGENT_DC != GlobalOptions.SystemProfile
                                                         .OwnerKontragent.DocCode
                                                     && sd34.AccuredAmountOfSupplierRow == null
                    select new
                    {
                        DocDC = sd34.DOC_CODE,
                        Date = sd34.DATE_ORD,
                        CrsDC = sd34.CRS_DC,
                        Name = sd43.NAME ?? "Расходный кассовый ордер",
                        Note = "№" + sd34.NUM_ORD + " от " + sd34.DATE_ORD + " Контрагент - " + sd43.NAME,
                        DocCode = sd34.DOC_CODE,
                        Price = sd34.SUMM_ORD,
                        Quantity = 1,
                        Profit = 0,
                        Loss = sd34.SUMM_ORD,
                        Result = -sd34.SUMM_ORD,
                        KontrDC = sd34.KONTRAGENT_DC,
                        Percent = sd34.KONTR_CRS_SUM_CORRECT_PERCENT,
                        SDRSchetDC = sd34.SHPZ_DC
                    }).ToList();

                // ReSharper restore AccessToDisposedClosure
                if (Project == null)
                {
                    foreach (var d in dataTovarPoluchen)
                    {
                        var nom =
                            GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as KursDomain.References.Nomenkl;
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{297D2727-5161-48ED-969E-651811906526}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Date = d.Date,
                            Kontragent = d.KontrName,
                            DocTypeCode = DocumentType.StoreOrderIn,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, ((IDocCode)nom.Currency).DocCode, d.Price * d.Quantity, 0m);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }

                    foreach (var d in dataTovarOtgr)
                    {
                        //var nomRate = GetRate(myRates, (decimal) d.NomenklCrsDC,
                        //    GlobalOptions.SystemProfile.MainCurrency.DocCode, d.Date);
                        var nom = GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC);
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{E47B2338-C42F-4B2A-8865-1024FC84F020}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Date = d.Date,
                            Kontragent = d.KonrtName,
                            DocTypeCode = DocumentType.Waybill,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, ((IDocCode)nom.Currency).DocCode, 0m, d.Quantity * d.NomPrice);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }

                    foreach (var d in dataBankIn)
                    {
                        var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDc) as Kontragent;
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{297D2727-5161-48ED-969E-651811906526}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Code = d.Code,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Date = d.Date,
                            Kontragent = kontr.Name,
                            DocTypeCode = DocumentType.Bank,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, d.OperCrsDC, d.Price, 0m);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }

                    foreach (var d in dataBankOut)
                    {
                        var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDc) as Kontragent;
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{E47B2338-C42F-4B2A-8865-1024FC84F020}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Code = d.Code,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Date = d.Date,
                            Kontragent = kontr.Name,
                            DocTypeCode = DocumentType.Bank,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, d.OperCrsDC, 0m, d.Price);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }

                    foreach (var d in dataCashIn)
                    {
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{297D2727-5161-48ED-969E-651811906526}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC)).Name,
                            Date = (DateTime)d.Date,
                            DocTypeCode = DocumentType.CashIn,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, d.CrsDC.Value, d.Profit, 0m);
                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }

                    foreach (var d in dataCashOut)
                    {
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{E47B2338-C42F-4B2A-8865-1024FC84F020}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Date = (DateTime)d.Date,
                            Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC)).Name,
                            DocTypeCode = DocumentType.CashOut,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, d.CrsDC.Value, 0m, d.Loss);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }
                }
                else
                {
                    foreach (var d in dataTovarPoluchen.Where(_ => ProjectDocDC.Contains(_.DocDC)))
                    {
                        var nom =
                            GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as KursDomain.References.Nomenkl;
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{297D2727-5161-48ED-969E-651811906526}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Date = d.Date,
                            Kontragent = d.KontrName,
                            DocTypeCode = DocumentType.StoreOrderIn,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, ((IDocCode)nom.Currency).DocCode, d.Profit, 0m);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }

                    foreach (var d in dataTovarOtgr.Where(_ => ProjectDocDC.Contains(_.DocDC)))
                    {
                        var nom =
                            GlobalOptions.ReferencesCache.GetNomenkl(d.NomenklDC) as KursDomain.References.Nomenkl;
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{E47B2338-C42F-4B2A-8865-1024FC84F020}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Date = d.Date,
                            Kontragent = d.KonrtName,
                            DocTypeCode = DocumentType.Waybill,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, ((IDocCode)nom.Currency).DocCode, 0m, d.Quantity * d.NomPrice);
                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }

                    foreach (var d in dataBankIn.Where(_ => ProjectDocDC.Contains(_.DocDC)))
                    {
                        var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDc) as Kontragent;
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{297D2727-5161-48ED-969E-651811906526}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Date = d.Date,
                            Kontragent = kontr.Name,
                            DocTypeCode = DocumentType.Bank,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, d.OperCrsDC, d.Price, 0m);
                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }

                    foreach (var d in dataBankOut.Where(_ => ProjectDocDC.Contains(_.DocDC)))
                    {
                        var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDc) as Kontragent;
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{E47B2338-C42F-4B2A-8865-1024FC84F020}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Date = d.Date,
                            Kontragent = kontr.Name,
                            DocTypeCode = DocumentType.Bank,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, d.OperCrsDC, 0m, d.Price);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }

                    foreach (var d in dataCashIn.Where(_ => ProjectDocDC.Contains(_.DocDC)))
                    {
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{297D2727-5161-48ED-969E-651811906526}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC)).Name,
                            Date = (DateTime)d.Date,
                            DocTypeCode = DocumentType.CashIn,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, d.CrsDC, d.Profit, 0m);
                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }

                    foreach (var d in dataCashOut.Where(_ => ProjectDocDC.Contains(_.DocDC)))
                    {
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{E47B2338-C42F-4B2A-8865-1024FC84F020}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price,
                            Date = (DateTime)d.Date,
                            Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC)).Name,
                            DocTypeCode = DocumentType.CashOut,
                            SDRSchet = d.SDRSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.SDRSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.SDRSchetDC.Value).SDRState as SDRState
                        };
                        SetCurrenciesValue(newOp, d.CrsDC, 0m, d.Loss);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }
                }
            }
        }

        public void SpisanieTovar()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.AktSpisaniya_row.Include(_ => _.AktSpisaniyaNomenkl_Title)
                    .Where(_ => _.AktSpisaniyaNomenkl_Title.Date_Doc >= DateStart &&
                                _.AktSpisaniyaNomenkl_Title.Date_Doc <= DateEnd);
                foreach (var d in data)
                {
                    var nom = GlobalOptions.ReferencesCache.GetNomenkl(d.Nomenkl_DC) as KursDomain.References.Nomenkl;
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = ProfitAndLossesMainRowViewModel.AktSpisaniaNomenkl,
                        Kontragent =
                            ((IName)GlobalOptions.ReferencesCache.GetWarehouse(d.AktSpisaniyaNomenkl_Title
                                .Warehouse_DC)).Name,
                        Name = $"({nom.NomenklNumber}) {nom.Name}",
                        Note = d.Note,
                        DocCode = 0,
                        Quantity = d.Quantity,
                        Price =
                            nomenklManager.GetNomenklPrice(d.Nomenkl_DC, d.AktSpisaniyaNomenkl_Title.Date_Doc).Price,
                        Date = d.AktSpisaniyaNomenkl_Title.Date_Doc,
                        DocTypeCode = DocumentType.AktSpisaniya,
                        StringId = d.Doc_Id.ToString()
                    };
                    SetCurrenciesValue(newOp, ((IDocCode)nom.Currency).DocCode, 0m, d.Quantity * newOp.Price);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        public void CalcAccruedAmmount()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var outBalansClient = ctx.AccruedAmountForClient
                    .Include(_ => _.AccuredAmountForClientRow)
                    .Where(_ => _.DocDate >= DateStart && _.DocDate <= DateEnd).ToList();
                var outBalansSupplier = ctx.AccuredAmountOfSupplierRow
                    .Include(_ => _.AccruedAmountOfSupplier)
                    .Where(_ => _.AccruedAmountOfSupplier.DocDate >= DateStart &&
                                _.AccruedAmountOfSupplier.DocDate <= DateEnd).ToList();

                foreach (var d in outBalansClient)
                {
                    var newOp1 = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = ProfitAndLossesMainRowViewModel.OutBalansAccrualAmmountClient,
                        Name = "Внебалансовые начисления для клиентов",
                        Note = string.Format(
                            $"Дата {d.DocDate.ToShortDateString()} №{d.DocInNum}/{d.DocExtNum} {d.Note}"),
                        DocCode = 0,
                        Quantity = 1,
                        Price = d.AccuredAmountForClientRow?.Sum(_ => _.Summa) ?? 0,
                        Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC)).Name,
                        KontragentBase = null,
                        Date = d.DocDate,
                        DocTypeCode = DocumentType.AccruedAmountForClient,
                        SDRSchet = null,
                        AktZachet = null,
                        AktZachetResult = 0,
                        CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC).Currency).Name,
                        DocNum = $"{d.DocInNum}/{d.DocExtNum}",
                        StringId = d.Id.ToString()
                    };

                    //CalcType = TypeProfitAndLossCalc.IsProfit

                    SetCurrenciesValue(newOp1,
                        ((IDocCode)GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC).Currency).DocCode,
                        newOp1.Price, 0m);
                    Extend.Add(newOp1);
                    ExtendNach.Add(newOp1);
                }

                foreach (var d in outBalansSupplier)
                {
                    var newOp1 = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = ProfitAndLossesMainRowViewModel.OutBalansAccrualAmmountSupplier,
                        Name = "Прямые затраты",
                        Note =
                            string.Format(
                                $"Дата {d.AccruedAmountOfSupplier.DocDate.ToShortDateString()} №{d.AccruedAmountOfSupplier.DocInNum}/{d.AccruedAmountOfSupplier.DocExtNum} {d.Note}"),
                        DocCode = 0,
                        Quantity = 1,
                        Price = d.Summa,
                        Kontragent =
                            ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.AccruedAmountOfSupplier.KontrDC))
                            .Name,
                        KontragentBase = null,
                        Date = d.AccruedAmountOfSupplier.DocDate,
                        DocTypeCode = DocumentType.AccruedAmountOfSupplier,
                        SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(d.SHPZ_DC) as SDRSchet,
                        SDRState =
                            d.SHPZ_DC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.SHPZ_DC.Value).SDRState as SDRState
                                : null,
                        AktZachet = null,
                        AktZachetResult = 0,
                        CurrencyName = ((IName)GlobalOptions.ReferencesCache
                                .GetKontragent(d.AccruedAmountOfSupplier.KontrDC).Currency)
                            .Name,
                        DocNum = $"{d.AccruedAmountOfSupplier.DocInNum}/{d.AccruedAmountOfSupplier.DocExtNum}",
                        StringId = d.DocId.ToString()
                        //CalcType = TypeProfitAndLossCalc.IsProfit
                    };
                    SetCurrenciesValue(newOp1,
                        ((IDocCode)GlobalOptions.ReferencesCache.GetKontragent(d.AccruedAmountOfSupplier.KontrDC)
                            .Currency).DocCode, 0m,
                        newOp1.Price);
                    Extend.Add(newOp1);
                    ExtendNach.Add(newOp1);
                }
            }
        }

        // ReSharper disable once UnusedMember.Local
        private decimal CalcItogoSumma(List<TD_110> rows)
        {
            decimal sumLeft = 0, sumRight = 0;
            foreach (var l in rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0))
                if (GlobalOptions.ReferencesCache.GetKontragent(l.VZT_KONTR_DC).IsBalans)
                    sumLeft += Math.Abs(l.VZT_KONTR_CRS_SUMMA ?? 0m);
            foreach (var l in rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1))
                if (GlobalOptions.ReferencesCache.GetKontragent(l.VZT_KONTR_DC).IsBalans)
                    sumRight += l.VZT_KONTR_CRS_SUMMA ?? 0m;
            return sumRight - sumLeft;
        }

        public void CalcFinance()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var data = (from sd110 in ent.SD_110
                    // ReSharper disable once AccessToDisposedClosure
                    from td110 in ent.TD_110
                    // ReSharper disable once AccessToDisposedClosure
                    from sd43 in ent.SD_43
                    from sd111 in ent.SD_111
                    where td110.DOC_CODE == sd110.DOC_CODE
                          && sd43.DOC_CODE == td110.VZT_KONTR_DC
                          //&& sd43.FLAG_BALANS == 1
                          && sd111.DOC_CODE == sd110.VZ_TYPE_DC
                          && td110.VZT_DOC_DATE >= DateStart && td110.VZT_DOC_DATE <= DateEnd
                    select new
                    {
                        DateRow = td110.VZT_DOC_DATE,
                        Num = sd110.VZ_NUM,
                        Date = sd110.VZ_DATE,
                        KontrCrsDC = sd43.VALUTA_DC,
                        IsProfit = td110.VZT_1MYDOLZH_0NAMDOLZH == 1,
                        Note = td110.VZT_DOC_NOTES,
                        DocCode = td110.DOC_CODE,
                        Summa = td110.VZT_KONTR_CRS_SUMMA,
                        Price = sd110.VZ_LEFT_UCH_CRS_SUM ?? 0,
                        Kontragent = sd43.NAME,
                        KontrDC = sd43.DOC_CODE,
                        sd111.IsCurrencyConvert,
                        sdrSchetDC = td110.VZT_SHPZ_DC,
                        Result = (sd110.VZ_LEFT_UCH_CRS_SUM ?? 0) - (sd110.VZ_RIGHT_UCH_CRS_SUM ?? 0),
                        CurrencyDC = td110.VZT_CRS_DC,
                        DocNum = sd110.VZ_NUM,
                        Rows = td110,
                        IsBalans = (sd43.FLAG_BALANS ?? 0) != 0
                    }).ToList();
                if (Project == null)
                    foreach (var d in data.Where(d => d.IsBalans))
                    {
                        decimal sumLeft = 0, sumRight = 0;
                        foreach (var l in data.Where(_ => _.DocCode == d.DocCode && !_.IsProfit))
                            if (GlobalOptions.ReferencesCache.GetKontragent(l.KontrDC).IsBalans)
                                sumLeft += Math.Abs((decimal)l.Summa);
                        foreach (var l in data.Where(_ => _.DocCode == d.DocCode && _.IsProfit))
                            if (GlobalOptions.ReferencesCache.GetKontragent(l.KontrDC).IsBalans)
                                sumRight += (decimal)l.Summa;
                        var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC);
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = GetGuidForActVzaimozachet(d.IsProfit, d.IsCurrencyConvert),
                            Name = d.IsCurrencyConvert ? "Акт конвертации" : "Взаимозачет",
                            Note =
                                string.Format("Дата {0} №{1}  {4}/ Дата строки {2} / {3}", d.Date.ToShortDateString(),
                                    d.Num,
                                    d.DateRow.ToShortDateString(), d.Note, d.Kontragent),
                            DocCode = d.DocCode,
                            Quantity = 1,
                            Price = d.Price,
                            Kontragent = ((IName)kontr).Name,
                            KontragentBase = kontr as Kontragent,
                            Date = d.DateRow,
                            DocTypeCode =
                                d.IsCurrencyConvert
                                    ? DocumentType.CurrencyConvertAccounting
                                    : DocumentType.MutualAccounting,
                            SDRSchet = d.sdrSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.sdrSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.sdrSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.sdrSchetDC.Value).SDRState as SDRState,
                            AktZachet = d.IsCurrencyConvert
                                ? null
                                : sumRight - sumLeft == 0
                                    ? null
                                    : sumRight - sumLeft > 0
                                        ? "green"
                                        : "red",
                            AktZachetResult = sumRight - sumLeft,
                            CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyDC)).Name,
                            DocNum = d.DocNum.ToString(),
                            CalcType = d.IsProfit ? TypeProfitAndLossCalc.IsProfit : TypeProfitAndLossCalc.IsLoss
                        };
                        SetCurrenciesValue(newOp, ((IDocCode)kontr.Currency).DocCode, d.IsProfit ? (decimal)d.Summa : 0,
                            d.IsProfit ? 0 : (decimal)-d.Summa);

                        if (d.IsBalans)
                        {
                            Extend.Add(newOp);
                            ExtendNach.Add(newOp);
                        }

                        if (!d.IsBalans && !d.IsCurrencyConvert)
                        {
                            var newOp2 = new ProfitAndLossesExtendRowViewModel
                            {
                                GroupId = GetGuidForActVzaimozachet(d.IsProfit, d.IsCurrencyConvert),
                                Name = d.IsCurrencyConvert ? "Акт конвертации" : "Взаимозачет",
                                Note =
                                    string.Format("Дата {0} №{1}  {4}/ Дата строки {2} / {3}",
                                        d.Date.ToShortDateString(),
                                        d.Num,
                                        d.DateRow.ToShortDateString(), d.Note, d.Kontragent),
                                DocCode = d.DocCode,
                                Quantity = 1,
                                Price = 0,
                                Kontragent = null,
                                KontragentBase = null,
                                Date = d.DateRow,
                                DocTypeCode = DocumentType.MutualAccounting,
                                SDRSchet = null,
                                AktZachet = d.IsCurrencyConvert
                                    ? null
                                    : sumRight - sumLeft == 0
                                        ? null
                                        : sumRight - sumLeft > 0
                                            ? "green"
                                            : "red",
                                AktZachetResult = sumRight - sumLeft,
                                CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyDC)).Name,
                                DocNum = d.DocNum.ToString(),
                                CalcType = d.IsProfit ? TypeProfitAndLossCalc.IsProfit : TypeProfitAndLossCalc.IsLoss
                            };

                            Extend.Add(newOp2);
                            ExtendNach.Add(newOp2);
                        }

                        // ReSharper disable once InvertIf
                        if (sumLeft == 0 || (sumRight == 0 && d.IsBalans && !d.IsCurrencyConvert))
                        {
                            var t = sumRight == 0;
                            var newOp1 = new ProfitAndLossesExtendRowViewModel
                            {
                                GroupId = GetGuidForActVzaimozachet(t, false),
                                Name = "Взаимозачет",
                                Note =
                                    string.Format("Дата {0} №{1}  {4}/ Дата строки {2} / {3}",
                                        d.Date.ToShortDateString(),
                                        d.Num,
                                        d.DateRow.ToShortDateString(), d.Note, d.Kontragent),
                                DocCode = d.DocCode,
                                Quantity = 1,
                                Price = 0, //(decimal) (d.IsProfit ? d.Summa * kontrRate : -d.Summa * kontrRate),
                                Kontragent = null,
                                KontragentBase = null,
                                Date = d.DateRow,
                                DocTypeCode = DocumentType.MutualAccounting,
                                SDRSchet = null,
                                AktZachet = d.IsCurrencyConvert
                                    ? null
                                    : sumRight - sumLeft == 0
                                        ? null
                                        : sumRight - sumLeft > 0
                                            ? "green"
                                            : "red",
                                AktZachetResult = sumRight - sumLeft,
                                CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyDC)).Name,
                                DocNum = d.DocNum.ToString(),
                                CalcType = !d.IsProfit ? TypeProfitAndLossCalc.IsProfit : TypeProfitAndLossCalc.IsLoss
                            };
                            Extend.Add(newOp1);
                            ExtendNach.Add(newOp1);
                        }
                    }
                else
                    foreach (var d in data.Where(_ => ProjectDocDC.Contains(_.DocCode)))
                    {
                        if (!d.IsBalans) continue;
                        var kontrRate = GetRate(MyRates, (decimal)d.KontrCrsDC,
                            GlobalOptions.SystemProfile.MainCurrency.DocCode, d.Date);
                        var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC);
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = GetGuidForActVzaimozachet(d.IsProfit, d.IsCurrencyConvert),
                            Name = d.IsCurrencyConvert ? "Акт конвертации" : "Взаимозачет",
                            Note =
                                string.Format("Дата {0} №{1}  {4}/ Дата строки {2} / {3}", d.Date.ToShortDateString(),
                                    d.Num,
                                    d.DateRow.ToShortDateString(), d.Note, d.Kontragent),
                            DocCode = d.DocCode,
                            Quantity = 1,
                            Price = (decimal)(d.IsProfit ? d.Summa * kontrRate : -d.Summa * kontrRate),
                            Kontragent = ((IName)kontr).Name,
                            KontragentBase = kontr as Kontragent,
                            Date = d.DateRow,
                            DocTypeCode =
                                d.IsCurrencyConvert
                                    ? DocumentType.CurrencyConvertAccounting
                                    : DocumentType.MutualAccounting,
                            SDRSchet = d.sdrSchetDC != null
                                ? GlobalOptions.ReferencesCache.GetSDRSchet(d.sdrSchetDC.Value) as SDRSchet
                                : null,
                            SDRState = d.sdrSchetDC == null
                                ? null
                                : GlobalOptions.ReferencesCache.GetSDRSchet(d.sdrSchetDC.Value).SDRState as SDRState,
                            AktZachet = d.IsCurrencyConvert
                                ? null
                                : d.Result == 0
                                    ? null
                                    : d.Result > 0
                                        ? "green"
                                        : "red",
                            AktZachetResult = d.Result,
                            CurrencyName = ((IName)GlobalOptions.ReferencesCache.GetCurrency(d.CurrencyDC)).Name,
                            DocNum = d.DocNum.ToString()
                        };
                        SetCurrenciesValue(newOp, d.CurrencyDC, d.IsProfit ? (decimal)d.Summa : 0,
                            d.IsProfit ? 0 : (decimal)-d.Summa);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }
            }
        }

        private Guid GetGuidForActVzaimozachet(bool isProfit, bool isConvert)
        {
            if (isConvert)
            {
                if (isProfit)
                    return Guid.Parse("{B6F2540A-9593-42E3-B34F-8C0983BC39A2}");
                return Guid.Parse("{35EBABEC-EAC3-4C3C-8383-6326C5D64C8C}");
            }

            if (isProfit)
                return Guid.Parse("{30E9BD73-9BDA-4D75-B897-332F9210B9B1}");
            return Guid.Parse("{459937DF-085F-4825-9AE9-810B054D0276}");
        }

        public void CalcCashPercent()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var dataCashIn = (from sd33 in ent.SD_33
                    // ReSharper disable once AccessToDisposedClosure
                    from sd43 in ent.SD_43
                    where sd33.DATE_ORD >= DateStart && sd33.DATE_ORD <= DateEnd
                                                     && sd43.DOC_CODE == sd33.KONTRAGENT_DC
                                                     && sd43.FLAG_BALANS == 1
                                                     // ReSharper disable once CompareOfFloatsByEqualityOperator
                                                     && sd33.KONTR_CRS_SUM_CORRECT_PERCENT != null &&
                                                     // ReSharper disable once CompareOfFloatsByEqualityOperator
                                                     sd33.KONTR_CRS_SUM_CORRECT_PERCENT != 0
                    select new
                    {
                        Date = sd33.DATE_ORD,
                        CrsDC = sd33.CRS_DC,
                        Name = "Приходный кассовый ордер",
                        Note = "№" + sd33.NUM_ORD + " от " + sd33.DATE_ORD + " Контрагент - " + sd43.NAME,
                        DocCode = sd33.DOC_CODE,
                        Price = sd33.SUMM_ORD,
                        Quantity = 1,
                        Profit = sd33.SUMM_ORD,
                        Loss = 0,
                        Result = sd33.SUMM_ORD,
                        KontrDC = sd33.KONTRAGENT_DC,
                        Percent = sd33.KONTR_CRS_SUM_CORRECT_PERCENT
                    }).ToList();
                var dataCashOut = (from sd34 in ent.SD_34
                    // ReSharper disable once AccessToDisposedClosure
                    from sd43 in ent.SD_43
                    where sd34.DATE_ORD >= DateStart && sd34.DATE_ORD <= DateEnd
                                                     && sd43.DOC_CODE == sd34.KONTRAGENT_DC
                                                     && sd43.FLAG_BALANS == 1
                                                     // ReSharper disable once CompareOfFloatsByEqualityOperator
                                                     && sd34.KONTR_CRS_SUM_CORRECT_PERCENT != null &&
                                                     // ReSharper disable once CompareOfFloatsByEqualityOperator
                                                     sd34.KONTR_CRS_SUM_CORRECT_PERCENT != 0
                    select new
                    {
                        Date = sd34.DATE_ORD,
                        CrsDC = sd34.CRS_DC,
                        Name = "Расходный кассовый ордер",
                        Note = "№" + sd34.NUM_ORD + " от " + sd34.DATE_ORD + " Контрагент - " + sd43.NAME,
                        DocCode = sd34.DOC_CODE,
                        Price = sd34.SUMM_ORD,
                        Quantity = 1,
                        Profit = 0,
                        Loss = sd34.SUMM_ORD,
                        Result = -sd34.SUMM_ORD,
                        KontrDC = sd34.KONTRAGENT_DC,
                        Percent = sd34.KONTR_CRS_SUM_CORRECT_PERCENT
                    }).ToList();
                foreach (var d in dataCashIn)
                {
                    //var crsRate = GetRate(MyRates, (decimal) d.CrsDC,
                    //    GlobalOptions.SystemProfile.MainCurrency.DocCode, (DateTime) d.Date);
                    var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC) as Kontragent;
                    if (d.Percent == null) continue;
                    if (d.Percent > 0)
                    {
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{E9D63300-829B-4CB1-AA05-68EC3A73C459}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            // ReSharper disable once PossibleInvalidOperationException
                            Price = (decimal)d.Price * (decimal)d.Percent / (100 - (decimal)d.Percent),
                            Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC)).Name,
                            // ReSharper disable once PossibleInvalidOperationException
                            Date = (DateTime)d.Date,
                            DocTypeCode = DocumentType.CashIn
                        };
                        SetCurrenciesValue(newOp, ((IDocCode)kontr.Currency).DocCode, 0m,
                            (d.Profit ?? 0) * (decimal)d.Percent / (100 - (decimal)d.Percent));

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }
                    else
                    {
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{836CC414-BEF4-4371-A253-47D2E8F4535F}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            // ReSharper disable once PossibleInvalidOperationException
                            Price = (decimal)d.Price * (decimal)d.Percent / (100 - (decimal)d.Percent),
                            Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC)).Name,
                            // ReSharper disable once PossibleInvalidOperationException
                            Date = (DateTime)d.Date,
                            DocTypeCode = DocumentType.CashIn
                        };
                        SetCurrenciesValue(newOp, ((IDocCode)kontr.Currency).DocCode,
                            (d.Profit ?? 0) * (decimal)d.Percent / (100 - (decimal)d.Percent), 0m);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }
                }

                foreach (var d in dataCashOut)
                {
                    //var crsRate = GetRate(MyRates, (decimal) d.CrsDC,
                    //    GlobalOptions.SystemProfile.MainCurrency.DocCode, (DateTime) d.Date);
                    var kontr = GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC) as Kontragent;
                    if (d.Percent == null) continue;
                    if (d.Percent > 0)
                    {
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{836CC414-BEF4-4371-A253-47D2E8F4535F}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price * (decimal)d.Percent / (100 - (decimal)d.Percent),
                            Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC)).Name,
                            Date = (DateTime)d.Date,
                            DocTypeCode = DocumentType.CashOut
                        };
                        SetCurrenciesValue(newOp, ((IDocCode)kontr.Currency).DocCode,
                            (d.Loss ?? 0) * (decimal)d.Percent / (100 - (decimal)d.Percent), 0m);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }
                    else
                    {
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{E9D63300-829B-4CB1-AA05-68EC3A73C459}"),
                            Name = d.Name,
                            Note = d.Note,
                            DocCode = d.DocCode,
                            Quantity = d.Quantity,
                            Price = (decimal)d.Price * (decimal)d.Percent / (100 - (decimal)d.Percent),
                            Kontragent = ((IName)GlobalOptions.ReferencesCache.GetKontragent(d.KontrDC)).Name,
                            Date = (DateTime)d.Date,
                            DocTypeCode = DocumentType.CashOut
                        };
                        SetCurrenciesValue(newOp, ((IDocCode)kontr.Currency).DocCode, 0m,
                            (d.Loss ?? 0) * (decimal)d.Percent / (100 - (decimal)d.Percent));

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }
                }
            }
        }

        /// <summary>
        ///     деньги в пути
        /// </summary>
        public void CalcMoneyInWay()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var cashOut = ent.SD_34.Where(_ =>
                    (_.CASH_TO_DC != null || _.BANK_RASCH_SCHET_DC != null) && _.DATE_ORD >= DateStart &&
                    _.DATE_ORD <= DateEnd).ToList();
                var cashIn = ent.SD_33.Where(_ =>
                    (_.RASH_ORDER_FROM_DC != null || _.BANK_RASCH_SCHET_DC != null) && _.DATE_ORD >= DateStart &&
                    _.DATE_ORD <= DateEnd
                ).ToList();
                var bankOut = ent.TD_101.Include(_ => _.SD_101).Where(_ => _.SD_101.VV_STOP_DATE >= DateStart &&
                                                                           _.SD_101.VV_STOP_DATE <= DateEnd &&
                                                                           (_.BankAccountDC == null ||
                                                                            _.VVT_KASS_PRIH_ORDER_DC == null)).ToList();
                var bankIn = ent.TD_101.Include(_ => _.SD_101).Where(_ => _.SD_101.VV_STOP_DATE >= DateStart &&
                                                                          _.SD_101.VV_STOP_DATE <= DateEnd &&
                                                                          (_.BankFromTransactionCode != null ||
                                                                           _.VVT_RASH_KASS_ORDER_DC == null)).ToList();

                foreach (var d in cashOut)
                {
                    var pay = ent.SD_33.FirstOrDefault(_ => _.RASH_ORDER_FROM_DC == d.DOC_CODE);
                    var bankpay = ent.TD_101.FirstOrDefault(_ => _.VVT_RASH_KASS_ORDER_DC == d.DOC_CODE);
                    if (pay != null || bankpay != null) continue;
                    if (d.CRS_DC != null)
                    {
                        var kontrName = d.CASH_TO_DC != null
                            ? ((IName)GlobalOptions.ReferencesCache.GetCashBox(d.CASH_TO_DC)).Name
                            : d.BANK_RASCH_SCHET_DC != null
                                ? ((IName)GlobalOptions.ReferencesCache.GetBankAccount(d.BANK_RASCH_SCHET_DC)).Name
                                : null;
                        var newOp = new ProfitAndLossesExtendRowViewModel
                        {
                            GroupId = Guid.Parse("{2D543CB3-7C79-4454-9EF3-E64F1A1FFA78}"),
                            Name = $"Расходный кассовый ордер {d.NAME_ORD}",
                            Note = d.NOTES_ORD,
                            DocCode = d.DOC_CODE,
                            Quantity = 1,
                            Price = d.SUMM_ORD ?? 0m,
                            Date = d.DATE_ORD ?? DateTime.MinValue,
                            Kontragent = kontrName,
                            DocTypeCode = DocumentType.CashOut
                        };
                        SetCurrenciesValue(newOp, d.CRS_DC, 0m, d.SUMM_ORD);

                        Extend.Add(newOp);
                        ExtendNach.Add(newOp);
                    }
                }

                foreach (var d in cashIn)
                {
                    if (d.RASH_ORDER_FROM_DC != null)
                        return;
                    //var ord = ent.SD_34.FirstOrDefault(_ => _.DOC_CODE == d.RASH_ORDER_FROM_DC.Value);
                    //if (ord != null)
                    //    KontrName = MainReferences.Cashs[ord.CA_DC.Value].Name;
                    var kontrName = ((IName)GlobalOptions.ReferencesCache.GetBankAccount(d.BANK_RASCH_SCHET_DC)).Name;
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{32F2AD21-7501-4C03-93A7-3C84E96CA671}"),
                        Name = $"Приходный кассовый ордер {d.NAME_ORD}",
                        Note = d.NOTES_ORD,
                        DocCode = d.DOC_CODE,
                        Quantity = 1,
                        Price = d.SUMM_ORD ?? 0m,
                        Date = d.DATE_ORD ?? DateTime.MinValue,
                        Kontragent = kontrName,
                        DocTypeCode = DocumentType.CashIn
                    };
                    SetCurrenciesValue(newOp, d.CRS_DC, d.SUMM_ORD, 0m);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }

                foreach (var d in bankOut)
                {
                    //var crsName = MainReferences.Currencies[d.VVT_CRS_DC].Name;
                    string kontrName = null;
                    if (d.VVT_KASS_PRIH_ORDER_DC != null)
                    {
                        var ord = ent.SD_33.FirstOrDefault(_ => _.DOC_CODE == d.VVT_KASS_PRIH_ORDER_DC.Value);
                        if (ord != null)
                            // ReSharper disable once PossibleInvalidOperationException
                            kontrName = ((IName)GlobalOptions.ReferencesCache.GetCashBox(ord.CA_DC.Value)).Name;
                    }
                    else
                    {
                        if (d.BankAccountDC != null)
                            kontrName = ((IName)GlobalOptions.ReferencesCache.GetBankAccount(d.BankAccountDC)).Name;
                    }

                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{2D543CB3-7C79-4454-9EF3-E64F1A1FFA78}"),
                        Name =
                            $"Банковская транзакция. Перевод из {((IName)GlobalOptions.ReferencesCache.GetBankAccount(d.SD_101.VV_ACC_DC)).Name} в {kontrName}",
                        Note = d.VVT_DOC_NUM,
                        DocCode = d.CODE,
                        Quantity = 1,
                        Price = d.VVT_VAL_RASHOD ?? 0m,
                        Date = d.SD_101.VV_STOP_DATE,
                        Kontragent = kontrName,
                        DocTypeCode = DocumentType.Bank
                    };
                    SetCurrenciesValue(newOp, d.VVT_CRS_DC, 0m, d.VVT_VAL_RASHOD);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }

                foreach (var d in bankIn)
                {
                    string kontrName = null;
                    var b = ent.TD_101.Include(_ => _.SD_101)
                        .FirstOrDefault(_ => _.CODE == d.BankFromTransactionCode);
                    if (b != null)
                        kontrName = ((IName)GlobalOptions.ReferencesCache.GetBankAccount(b.SD_101.VV_ACC_DC)).Name;
                    var newOp = new ProfitAndLossesExtendRowViewModel
                    {
                        GroupId = Guid.Parse("{32F2AD21-7501-4C03-93A7-3C84E96CA671}"),
                        Name =
                            $"Банковская транзакция. Перевод из {kontrName} в {((IName)GlobalOptions.ReferencesCache.GetBankAccount(d.SD_101.VV_ACC_DC)).Name}",
                        Note = d.VVT_DOC_NUM,
                        DocCode = d.CODE,
                        Quantity = 1,
                        Price = d.VVT_VAL_RASHOD ?? 0m,
                        Date = d.SD_101.VV_STOP_DATE,
                        Kontragent = kontrName,
                        DocTypeCode = DocumentType.Bank
                    };
                    SetCurrenciesValue(newOp, d.VVT_CRS_DC, d.VVT_VAL_PRIHOD, 0m);

                    Extend.Add(newOp);
                    ExtendNach.Add(newOp);
                }
            }
        }

        
        public override void RefreshData(object obj)
        {
        }
    }
}
