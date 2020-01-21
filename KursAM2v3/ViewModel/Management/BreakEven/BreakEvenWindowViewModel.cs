using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using Helper;
using KursAM2.Managers;
using KursAM2.View.Management;
using KursAM2.ViewModel.Management.Calculations;

// ReSharper disable CollectionNeverQueried.Global
namespace KursAM2.ViewModel.Management.BreakEven
{
    public class BreakEvenWindowViewModel : RSWindowViewModelBase
    {
        public readonly List<BreakEvenRow> DataAll;

        private readonly ObservableCollection<BreakEvenCOrGroupViewModel> myTempCoGroups =
            new ObservableCollection<BreakEvenCOrGroupViewModel>();

        private readonly ObservableCollection<BreakEvenKontrGroupViewModel> myTempKontrGroups =
            new ObservableCollection<BreakEvenKontrGroupViewModel>();

        private readonly ObservableCollection<BreakEvenManagerGroupViewModel> myTempManagerGroups =
            new ObservableCollection<BreakEvenManagerGroupViewModel>();

        private readonly ObservableCollection<BreakEvenNomGroupViewModel> myTempNomenklGroups =
            new ObservableCollection<BreakEvenNomGroupViewModel>();

        private DocumentRow myCurrentDocument;
        private DateTime myEndDate;
        private DateTime myStartDate;

        public BreakEvenWindowViewModel()
        {
            StartDate = DateTime.Today.AddDays(-30);
            EndDate = DateTime.Today;
            DataAll = new List<BreakEvenRow>();
            NomenklGroups = new ObservableCollection<BreakEvenNomGroupViewModel>();
            CoGroups = new ObservableCollection<BreakEvenCOrGroupViewModel>();
            KontrGroups = new ObservableCollection<BreakEvenKontrGroupViewModel>();
            ManagerGroups = new ObservableCollection<BreakEvenManagerGroupViewModel>();
            DocumentGroup = new ObservableCollection<DocumentRow>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<DocumentRow> DocumentGroup { set; get; }
        public ObservableCollection<DocumentRow> DocumentCurrencyGroup { set; get; } =
            new ObservableCollection<DocumentRow>();
        public ObservableCollection<BreakEvenNomGroupViewModel> NomenklGroups { set; get; }
        public ObservableCollection<BreakEvenCOrGroupViewModel> CoGroups { set; get; }
        public ObservableCollection<BreakEvenKontrGroupViewModel> KontrGroups { set; get; }
        public ObservableCollection<BreakEvenManagerGroupViewModel> ManagerGroups { set; get; }
        public DocumentRow CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument == value) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }
        public DateTime StartDate
        {
            set
            {
                if (myStartDate == value) return;
                myStartDate = value;
                RaisePropertyChanged();
            }
            get => myStartDate;
        }
        public DateTime EndDate
        {
            set
            {
                if (myEndDate == value) return;
                myEndDate = value;
                RaisePropertyChanged();
            }
            get => myEndDate;
        }
        public override bool IsDocumentOpenAllow => CurrentDocument != null
                                                    && DocumentsOpenManager.IsDocumentOpen(CurrentDocument.DocType);

        public void UpdateDocForNomenkl(string nomNum)
        {
            DocumentGroup.Clear();
            DocumentCurrencyGroup.Clear();
            foreach (var d in DataAll.Where(t => t.Nomenkl.NomenklNumber == nomNum))
            {
                DocumentGroup.Add(new DocumentRow
                {
                    COName = d.CentrOfResponsibility.Name,
                    Date = d.Date,
                    DilerName = d.Diler,
                    DilerSumma = d.DilerSumma,
                    IsUsluga = d.IsUsluga,
                    KontragentName = d.Kontragent,
                    KontrSummaCrs = d.KontrSummaCrs,
                    Price = d.Price,
                    KontrSumma = d.KontrSumma,
                    ManagerName = d.Manager,
                    Naklad = d.Naklad,
                    Nomenkl = d.Nomenkl,
                    NomenklSumWOReva = Math.Round(d.KontrSummaCrs - d.SummaNomenkl - d.DilerSumma, 2),
                    OperCrsName = d.OperCrsName,
                    Quantity = d.Quantity,
                    Schet = d.Schet,
                    SummaNomenkl = d.SummaNomenkl,
                    SummaNomenklCrs = d.SummaNomenklCrs
                });
                DocumentCurrencyGroup.Add(new DocumentRow
                {
                    COName = d.CentrOfResponsibility.Name,
                    Date = d.Date,
                    DilerName = d.Diler,
                    DilerSumma = d.DilerSumma,
                    IsUsluga = d.IsUsluga,
                    KontragentName = d.Kontragent,
                    KontrSummaCrs = d.KontrSummaCrs,
                    Price = d.Price,
                    KontrSumma = d.KontrSumma,
                    ManagerName = d.Manager,
                    Naklad = d.Naklad,
                    Nomenkl = d.Nomenkl,
                    NomenklSumWOReva = Math.Round(d.KontrSummaCrs - d.SummaNomenkl - d.DilerSumma, 2),
                    OperCrsName = d.OperCrsName,
                    Quantity = d.Quantity,
                    Schet = d.Schet,
                    SummaNomenkl = d.SummaNomenkl,
                    SummaNomenklCrs = d.SummaNomenklCrs,
                    SummaOperNomenkl = d.SummaOperNomenkl,
                    KontrOperSummaCrs = d.KontrOperSummaCrs,
                    SummaOperNomenklCrs = d.SummaOperNomenklCrs,
                    ResultEUR =
                        d.OperCurrency.Name == "EUR"
                            ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs - d.DilerSumma, 2)
                            : 0,
                    ResultRUB =
                        d.OperCurrency.Name == "RUB" || d.OperCurrency.Name == "RUR"
                            ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs - d.DilerSumma, 2)
                            : 0,
                    ResultUSD =
                        d.OperCurrency.Name == "USD"
                            ? Math.Round(d.KontrSumma - d.SummaOperNomenklCrs - d.DilerSumma, 2)
                            : 0
                });
            }
        }

        public override void RefreshData(object obj)
        {
            RefreshData(StartDate, EndDate);
        }

        public void RefreshData(DateTime start, DateTime end)
        {
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }
            using (var ent = GlobalOptions.GetEntities())
            {
                var empls = ent.SD_2.ToList();
                var rates =
                    ent.CURRENCY_RATES_CB.Where(
                            _ => _.RATE_DATE >= start && _.RATE_DATE <= end)
                        .ToList();
                var dt = rates.Select(_ => _.RATE_DATE).Distinct().ToList();
                rates.AddRange(dt.Select(r => new CURRENCY_RATES_CB
                {
                    CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                    NOMINAL = 1,
                    RATE = 1,
                    RATE_DATE = r
                }));
                var sql = " SELECT s83.NOM_0MATER_1USLUGA IsUsluga, " + "DD_DATE DATE, " +
                          "SFT_NEMENKL_DC NomenklDC, " +
                          "isnull(SF_CENTR_OTV_DC,0) Sd40, " + "isnull(SF_CLIENT_DC,0) KontragentDC, " +
                          "isnull(SF_CRS_DC,0) Currency, " + "isnull(S2.DOC_CODE,0) Manager, " +
                          "cast(DDT_KOL_RASHOD as numeric(18,8)) Quantity, " +
                          "cast(isnull(SFT_ED_CENA,0) as numeric(18,4)) Price, " +
                          "cast(isnull(KONTR_CRS,0) as numeric(18,4)) SummaKontrCrs, " +
                          "cast(isnull(NomenklSum,0) as numeric(18,4)) NomSumm, " + "SF_DATE as SF_DATE, " +
                          "SF_NUM as SF_NUM, " + "isnull(SF_NOTES,'') as SF_NOTES, " + "NAKL_NUM as NAKL_NUM, " +
                          "isnull(NAKL_NOTES,'') as NAKL_NOTES, " + "isnull(SF_DILER_DC,0) Diler, " +
                          "isnull(DILER_SUMMA,0) DilerSumma, " +
                          "isnull(NomenklSumWOReval,0) as NomenklSumWOReval, " +
                          "S43.VALUTA_DC KontrCrsDC, " + "S83.NOM_SALE_CRS_DC NomenklCrsDC, " +
                          "SaleTaxPrice as SaleTaxPrice,  SaleTaxRate as SaleTaxRate,IsSaleTax as IsSaleTax,  DocDC as DocDC, " +
                          "TypeDC as TypeDC" +
                          " FROM " +
                          " (SELECT S84.DOC_CODE DocDC, S24.DD_TYPE_DC AS TypeDC, s83.NOM_0MATER_1USLUGA " +
                          "      , S84.SF_DATE SF_DATE " +
                          "     , cast(S84.SF_IN_NUM as varchar(50)) + '/' + isnull(S84.SF_OUT_NUM,'') SF_NUM " +
                          "    , S84.SF_NOTE + ' / ' + T84.SFT_TEXT AS SF_NOTES " +
                          "   , CAST(S24.DD_IN_NUM AS VARCHAR(50)) + '/' + ISNULL(S24.DD_EXT_NUM,'') NAKL_NUM " +
                          "  , S24.DD_NOTES AS NAKL_NOTES " + "       , DD_DATE " + "      , SFT_NEMENKL_DC " +
                          "     , SF_CENTR_OTV_DC " + "    , SF_CLIENT_DC " + "   , SF_CRS_DC " + "  , S84.CREATOR " +
                          "     , DDT_KOL_RASHOD " + "    , SFT_ED_CENA " +
                          "   , (SFT_SUMMA_K_OPLATE_KONTR_CRS * (DDT_KOL_RASHOD / SFT_KOL )) as KONTR_CRS " +
                          "  , cast((SELECT p1.PRICE " + "         FROM " + "          nom_price p1 (NOLOCK) " +
                          "       WHERE " + "        NOM_DC = t84.SFT_NEMENKL_DC " +
                          "       AND p1.date = (SELECT max(p2.date) " + "                      FROM " +
                          "                       NOM_PRICE p2 (NOLOCK) " + "                    WHERE " +
                          "                     p1.nom_dc = P2.nom_dc " +
                          "                    AND p2.DATE <= s24.DD_date)) * T24.DDT_KOL_RASHOD AS NUMERIC(18, 2)) NomenklSum " +
                          "         , s84.SF_DILER_DC " +
                          "        , isnull(sft_nacenka_dilera,0)*ddt_kol_rashod DILER_SUMMA " +
                          "       , cast((SELECT p1.PRICE_WO_NAKLAD " + "              FROM " +
                          "               nom_price p1 (NOLOCK) " + "            WHERE " +
                          "             NOM_DC = t84.SFT_NEMENKL_DC " +
                          "            AND p1.date = (SELECT max(p2.date) " + "                          FROM " +
                          "                           NOM_PRICE p2 (NOLOCK) " + "                        WHERE " +
                          "                         p1.nom_dc = P2.nom_dc " +
                          "                        AND p2.DATE <= s24.DD_date)) * T24.DDT_KOL_RASHOD AS NUMERIC(18, 2)) as NomenklSumWOReval, " +
                          " T24.SaleTaxPrice as SaleTaxPrice, T24.SaleTaxRate as SaleTaxRate, T24.IsSaleTax as IsSaleTax " +
                          "            FROM " + "             TD_84 T84 " + "           INNER JOIN SD_84 S84 " +
                          "              ON S84.DOC_CODE = T84.DOC_CODE " + "          INNER JOIN TD_24 T24 " +
                          "           ON T24.DDT_SFACT_DC = S84.DOC_CODE AND T24.DDT_SFACT_ROW_CODE = T84.Code AND DDT_KOL_RASHOD > 0 " +
                          "        INNER JOIN SD_24 S24 " +
                          $"         ON S24.DOC_CODE = T24.DOC_CODE AND S24.DD_DATE BETWEEN '{CustomFormat.DateToString(start)}' AND '{CustomFormat.DateToString(end)}' " +
                          "      INNER JOIN SD_83 S83 " + "       ON T84.SFT_NEMENKL_DC = S83.DOC_CODE) TAB " +
                          "      inner join sd_43 s43 on s43.doc_code = tab.SF_CLIENT_DC " +
                          "     inner join sd_83 s83 on s83.doc_code = tab.SFT_NEMENKL_DC " +
                          "     LEFT OUTER JOIN EXT_USERS EU ON TAB.CREATOR = EU.USR_NICKNAME " +
                          "    LEFT OUTER JOIN SD_2 S2 ON S2.TABELNUMBER = EU.TABELNUMBER";
                try
                {
                    DataAll.Clear();
                    var uslugaIn = ent.TD_26
                        .Include(_ => _.SD_26)
                        .Include(_ => _.SD_83)
                        .Where(_ => _.SD_83.IsUslugaInRent == true && _.SD_26.SF_POSTAV_DATE >= start &&
                                    _.SD_26.SF_POSTAV_DATE <= end).ToList();
                    var uslugaOut = ent.TD_84
                        .Include(_ => _.SD_84)
                        .Include(_ => _.SD_83)
                        .Where(_ => _.SD_83.IsUslugaInRent == true && _.SD_84.SF_DATE >= start &&
                                    _.SD_84.SF_DATE <= end).ToList();
                    var uslugaDCs = new List<decimal>();
                    foreach (var t in uslugaIn) uslugaDCs.Add(t.SFT_NEMENKL_DC);
                    foreach (var t in uslugaOut) uslugaDCs.Add(t.SFT_NEMENKL_DC);
                    if (uslugaDCs.Count > 0)
                        foreach (var dc in uslugaDCs.Distinct())
                        {
                            decimal sumQuanInStart = 0;
                            decimal sumQuanOutStart = 0;
                            decimal summaAllInStart = 0;
                            decimal sumInPeriod = 0;
                            decimal summaInStart = 0;
                            decimal sumQuanInPeriod = 0;
                            decimal sumQuanStart = 0;
                            var td26 = ent.TD_26
                                .Include(_ => _.SD_26).Where(_ =>
                                    _.SFT_NEMENKL_DC == dc && _.SD_26.SF_POSTAV_DATE < start);
                            if (td26.Any())
                                sumQuanInStart = td26.Sum(_ => _.SFT_KOL);
                            var td84 = ent.TD_84
                                .Include(_ => _.SD_84).Where(_ =>
                                    _.SFT_NEMENKL_DC == dc && _.SD_84.SF_DATE < start);
                            if (td84.Any())
                                sumQuanOutStart = (decimal) td84.Sum(_ => _.SFT_KOL);
                            sumQuanStart = sumQuanInStart - sumQuanOutStart;
                            td26 = ent.TD_26
                                .Include(_ => _.SD_26).Where(_ =>
                                    _.SFT_NEMENKL_DC == dc && _.SD_26.SF_POSTAV_DATE < start);
                            if (td26.Any())
                                summaAllInStart = (decimal) td26.Sum(_ => _.SFT_SUMMA_K_OPLATE);
                            if (sumQuanInStart > 0)
                            {
                                var priceEd = summaAllInStart / sumQuanInStart;
                                summaInStart = sumQuanStart * priceEd;
                            }
                            td26 = ent.TD_26
                                .Include(_ => _.SD_26).Where(_ =>
                                    _.SFT_NEMENKL_DC == dc && _.SD_26.SF_POSTAV_DATE >= start &&
                                    _.SD_26.SF_POSTAV_DATE <= end);
                            if (td26.Any())
                            {
                                sumInPeriod = (decimal) td26.Sum(_ => _.SFT_SUMMA_K_OPLATE);
                                sumQuanInPeriod = td26.Sum(_ => _.SFT_KOL);
                            }
                            decimal price = 0;
                            if (sumQuanStart + sumQuanInPeriod > 0)
                            {
                                price = (summaInStart + sumInPeriod) / (sumQuanStart + sumQuanInPeriod);
                            }
                            var operList = ent.TD_84
                                .Include(_ => _.SD_84).Where(_ =>
                                    _.SFT_NEMENKL_DC == dc && _.SD_84.SF_DATE >= start &&
                                    _.SD_84.SF_DATE <= end).ToList();
                            if (operList.Any())
                                foreach (var d in operList)
                                {
                                    var kontr = MainReferences.GetKontragent(d.SD_84.SF_CLIENT_DC);
                                    var otvlico = d.SD_84.PersonalResponsibleDC != null
                                        ? MainReferences.Employees[d.SD_84.PersonalResponsibleDC.Value].Name
                                        : null;
                                    if (otvlico == null && kontr.OTVETSTV_LICO != null)
                                    {
                                        otvlico = MainReferences.Employees.Values
                                            .FirstOrDefault(_ => _.TabelNumber == kontr.OTVETSTV_LICO)?.Name;
                                    }
                                    DataAll.Add(new BreakEvenRow
                                    {
                                        Kontragent = kontr.Name,
                                        Kontr = kontr,
                                        Nomenkl = MainReferences.GetNomenkl(d.SFT_NEMENKL_DC),
                                        CentrOfResponsibility = MainReferences.GetCO(d.SD_84.SF_CENTR_OTV_DC),
                                        Date = d.SD_84.SF_DATE,
                                        Diler = null, //diler != null ? diler.Name : "",
                                        //DilerSumma = 0, //Convert.ToDecimal(d.DilerSumma) * drate,
                                        IsUsluga = true,
                                        KontrSumma = (decimal) d.SFT_SUMMA_K_OPLATE, //Convert.ToDecimal(d.KontrSumma),
                                        KontrSummaCrs = (decimal) d.SFT_SUMMA_K_OPLATE,
                                        Manager = otvlico != null ? otvlico : "Менеджер не указан",
                                        Naklad = null,
                                        NomenklSumWOReval = 0,
                                        OperCrsName = MainReferences.Currencies[d.SD_84.SF_CRS_DC].Name,
                                        OperCurrency = MainReferences.Currencies[d.SD_84.SF_CRS_DC],
                                        Schet =
                                            $"Счет №'{d.SD_84.SF_IN_NUM} от {d.SD_84.SF_DATE.ToShortDateString()} {d.SD_84.SF_NOTE}",
                                        Quantity = Convert.ToDecimal(d.SFT_KOL),
                                        SummaNomenkl = Convert.ToDecimal(d.SFT_KOL) * price,
                                        SummaNomenklCrs = Convert.ToDecimal(d.SFT_KOL) * price,
                                        Price = price,
                                        KontrOperSummaCrs = (decimal) d.SFT_SUMMA_K_OPLATE,
                                        SummaOperNomenkl = Convert.ToDecimal(d.SFT_KOL) * price,
                                        SummaOperNomenklCrs = Convert.ToDecimal(d.SFT_KOL) * price,
                                        NomenklOperSumWOReval = Convert.ToDecimal(d.SFT_KOL) * price,
                                        DocType = DocumentType.InvoiceClient
                                    });
                                }
                        }
                    foreach (var d in ent.Database.SqlQuery<BreakEvenTemp>(sql))
                    {
                        var nom = MainReferences.GetNomenkl(d.NomenklDC);
                        var diler = MainReferences.AllKontragents.ContainsKey(d.Diler)
                            ? MainReferences.AllKontragents[d.Diler]
                            : null;
                        var co = MainReferences.COList.ContainsKey(d.CO)
                            ? MainReferences.COList[d.CO]
                            : new CentrOfResponsibility {Entity = new SD_40 {DOC_CODE = 0, CENT_NAME = "Не указан"}};
                        var emp = empls.FirstOrDefault(_ => _.DOC_CODE == d.Manager);
                        var crs = MainReferences.Currencies[d.Currency];
                        //var krate = GetRate(rates, d.KontrCrsDC, GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                        //    d.DATE);
                        //var nrate = GetRate(rates, d.NomenklCrsDC,
                        //    GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                        //    d.DATE);
                        //var drate = GetRate(rates, d.Currency, GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                        //    d.DATE);
                        //var crsKontrRate = GetRate(rates, d.KontrCrsDC,
                        //    GlobalOptions.SystemProfile.NationalCurrency.DocCode /*crs.DocCode*/, d.DATE);
                        // ReSharper disable once PossibleInvalidOperationException
                        //var crsNomenklRate = nom != null
                        //    ? GetRate(rates, (decimal) nom.NOM_SALE_CRS_DC,
                        //        GlobalOptions.SystemProfile.NationalCurrency.DocCode
                        //        /*crs.DocCode*/, d.DATE)
                        //    : 0;
                        DataAll.Add(new BreakEvenRow
                        {
                            Kontragent = MainReferences.AllKontragents[d.KontragentDC].Name,
                            Kontr = MainReferences.AllKontragents[d.KontragentDC],
                            Nomenkl = nom,
                            CentrOfResponsibility = co,
                            Date = d.DATE,
                            Diler = diler != null ? diler.Name : "",
                            DilerSumma = Convert.ToDecimal(d.DilerSumma),// * drate,
                            IsUsluga = d.IsUsluga == 1,
                            KontrSumma = d.SummaKontrCrs, //Convert.ToDecimal(d.KontrSumma),
                            KontrSummaCrs = d.SummaKontrCrs /* *krate*/ - Convert.ToDecimal(d.DilerSumma), //* drate,
                            //Convert.ToDecimal(d.KontrSummaCrs),
                            Manager = emp != null ? emp.NAME : "Менеджер не указан",
                            //personaInfo != null ? personaInfo.FullName : "",
                            Naklad =
                                $"Накладная №'{d.NAKL_NUM} от {d.DATE.ToShortDateString()} {d.NAKL_NOTES}rdr[15]",
                            //d.Naklad,
                            NomenklSumWOReval =
                                Convert.ToDecimal(d.NomenklSumWOReval)+ //* nrate +
                                Convert.ToDecimal(d.DilerSumma), //* drate,
                            OperCrsName = crs.CRS_SHORTNAME, // crsInfo.Name,
                            OperCurrency = crs,
                            Schet =
                                $"Счет №'{d.SF_NUM} от {d.SF_DATE.ToShortDateString()} {d.SF_NOTES}",
                            //d.Schet,
                            Quantity = Convert.ToDecimal(d.Quantity),
                            SummaNomenkl = d.NomSumm, //Convert.ToDecimal(d.SummaNomenkl),
                            SummaNomenklCrs = !d.IsSaleTax ? d.NomSumm /* * nrate */ : d.Quantity * (d.SaleTaxPrice ?? 0),
                            //Convert.ToDecimal(d.SummaNomenklCrs),
                            Price = Convert.ToDecimal(d.Price),
                            KontrOperSummaCrs = d.SummaKontrCrs, //* crsKontrRate,
                            SummaOperNomenkl =
                                !d.IsSaleTax ? d.NomSumm /* * crsNomenklRate */: d.Quantity * (d.SaleTaxPrice ?? 0),
                            SummaOperNomenklCrs =
                                !d.IsSaleTax ? d.NomSumm /* * crsNomenklRate */: d.Quantity * (d.SaleTaxPrice ?? 0),
                            NomenklOperSumWOReval =
                                !d.IsSaleTax
                                    ? Convert.ToDecimal(d.NomenklSumWOReval) //19655691* crsNomenklRate
                                    : (d.NomSumm - (d.SaleTaxPrice ?? 0)) * d.Quantity,
                            DocType = DocumentsOpenManager.GetMaterialDocTypeFromDC(d.TypeDC)
                        });
                    }
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }
                SetMain();
            }
        }

        //private static decimal GetRate(List<CURRENCY_RATES_CB> rates, decimal firstDC, decimal secondDC, DateTime date)
        //{
        //    if (firstDC == secondDC) return 1;
        //    var date1 = rates.Where(_ => _.RATE_DATE <= date).Max(_ => _.RATE_DATE);
        //    var f = rates.SingleOrDefault(_ => _.CRS_DC == firstDC && _.RATE_DATE == date1);
        //    var s = rates.SingleOrDefault(_ => _.CRS_DC == secondDC && _.RATE_DATE == date1);
        //    if (f != null && s != null && s.RATE != 0)
        //        return f.RATE / f.NOMINAL / (s.RATE / s.NOMINAL);
        //    return -1;
        //}

        public void SetMain()
        {
            myTempNomenklGroups.Clear();
            myTempCoGroups.Clear();
            myTempKontrGroups.Clear();
            myTempManagerGroups.Clear();
            foreach (var row in DataAll)
            {
                var n = myTempNomenklGroups.FirstOrDefault(t => t.NomenklNumber == row.Nomenkl.NomenklNumber);
                if (n != null)
                {
                    n.Quantity += Math.Round(row.Quantity, 2);
                    n.Summa += Math.Round(row.KontrSummaCrs, 2);
                    n.Cost += Math.Round(row.SummaNomenklCrs, 2);
                    n.CostWOReval += Math.Round(row.NomenklSumWOReval, 2);
                    n.DilerSumma += Math.Round(row.DilerSumma, 2);
                    n.Naklad += Math.Round(row.SummaNomenklCrs, 2) - Math.Round(row.NomenklSumWOReval);
                }
                else
                {
                    myTempNomenklGroups.Add(new BreakEvenNomGroupViewModel
                    {
                        Name = row.Nomenkl.Name,
                        NomenklNumber = row.Nomenkl.NomenklNumber,
                        Quantity = Math.Round(row.Quantity, 2),
                        Summa = Math.Round(row.KontrSummaCrs, 2),
                        DilerSumma = Math.Round(row.DilerSumma, 2),
                        Cost = Math.Round(row.SummaNomenklCrs, 2),
                        CostWOReval = Math.Round(row.NomenklSumWOReval, 2),
                        Naklad = Math.Round(row.SummaNomenklCrs, 2) - Math.Round(row.NomenklSumWOReval)
                    });
                }
                var k = myTempKontrGroups.FirstOrDefault(t => t.Name == row.Kontragent);
                if (k != null)
                {
                    k.Quantity += Math.Round(row.Quantity, 2);
                    k.Summa += Math.Round(row.KontrSummaCrs, 2);
                    k.Cost += Math.Round(row.SummaNomenklCrs, 2);
                    k.CostWOReval += Math.Round(row.NomenklSumWOReval, 2);
                    k.DilerSumma += Math.Round(row.DilerSumma, 2);
                    k.Naklad += Math.Round(row.SummaNomenklCrs, 2) - Math.Round(row.NomenklSumWOReval);
                }
                else
                {
                    myTempKontrGroups.Add(new BreakEvenKontrGroupViewModel
                    {
                        Kontragent = row.Kontr,
                        Name = row.Kontragent,
                        Quantity = Math.Round(row.Quantity, 2),
                        DilerSumma = Math.Round(row.DilerSumma, 2),
                        Summa = Math.Round(row.KontrSummaCrs, 2),
                        Cost = Math.Round(row.SummaNomenklCrs, 2),
                        CostWOReval = Math.Round(row.NomenklSumWOReval, 2),
                        Naklad = Math.Round(row.SummaNomenklCrs, 2) - Math.Round(row.NomenklSumWOReval)
                    });
                }
                var c = myTempCoGroups.FirstOrDefault(t => t.Name == row.CentrOfResponsibility.Name);
                if (c != null)
                {
                    c.Quantity += Math.Round(row.Quantity, 2);
                    c.Summa += Math.Round(row.KontrSummaCrs, 2);
                    c.Cost += Math.Round(row.SummaNomenklCrs, 2);
                    c.CostWOReval += Math.Round(row.NomenklSumWOReval, 2);
                    c.DilerSumma += Math.Round(row.DilerSumma, 2);
                    c.Naklad += Math.Round(row.SummaNomenklCrs, 2) - Math.Round(row.NomenklSumWOReval);
                }
                else
                {
                    myTempCoGroups.Add(new BreakEvenCOrGroupViewModel
                    {
                        Name = row.CentrOfResponsibility.Name,
                        Quantity = Math.Round(row.Quantity, 2),
                        Summa = Math.Round(row.KontrSummaCrs, 2),
                        DilerSumma = Math.Round(row.DilerSumma, 2),
                        Cost = Math.Round(row.SummaNomenklCrs, 2),
                        CostWOReval = Math.Round(row.NomenklSumWOReval, 2),
                        Naklad = Math.Round(row.SummaNomenklCrs, 2) - Math.Round(row.NomenklSumWOReval)
                    });
                }
                var m = myTempManagerGroups.FirstOrDefault(t => t.Name == row.Manager);
                if (m != null)
                {
                    m.Quantity += Math.Round(row.Quantity, 2);
                    m.Summa += Math.Round(row.KontrSummaCrs, 2);
                    m.Cost += Math.Round(row.SummaNomenklCrs, 2);
                    m.CostWOReval += Math.Round(row.NomenklSumWOReval, 2);
                    m.DilerSumma += Math.Round(row.DilerSumma, 2);
                    m.Naklad += Math.Round(row.SummaNomenklCrs, 2) - Math.Round(row.NomenklSumWOReval);
                }
                else
                {
                    myTempManagerGroups.Add(new BreakEvenManagerGroupViewModel
                    {
                        Name = row.Manager,
                        Quantity = Math.Round(row.Quantity, 2),
                        Summa = Math.Round(row.KontrSummaCrs, 2),
                        DilerSumma = Math.Round(row.DilerSumma, 2),
                        Cost = Math.Round(row.SummaNomenklCrs, 2),
                        CostWOReval = Math.Round(row.NomenklSumWOReval, 2),
                        Naklad = Math.Round(row.SummaNomenklCrs, 2) - Math.Round(row.NomenklSumWOReval)
                    });
                }
            }
            using (var ent = GlobalOptions.GetEntities())
            {
                var kontrChanged = ent.KONTR_BLS_RECALC.Where(_ => _.UserInsert != "dbo").ToList();
                foreach (var k in myTempKontrGroups)
                {
                    if (kontrChanged.Any(_ => _.KONTR_DC == k.Kontragent.DocCode))
                        RecalcKontragentBalans.CalcBalans(k.Kontragent.DocCode,
                            kontrChanged.Where(_ => _.KONTR_DC == k.Kontragent.DocCode).Min(_ => _.DATE_CHANGED));
                    var sqlBalans = "SELECT SUM(CRS_KONTR_OUT) - SUM(CRS_KONTR_IN) FROM KONTR_BALANS_OPER_ARC " +
                                    $"WHERE DOC_DATE BETWEEN '20000101' AND '{CustomFormat.DateToString(EndDate)}' AND KONTR_DC = {CustomFormat.DecimalToSqlDecimal(k.Kontragent.DocCode)}";
                    var sqlCurrentBalans = "SELECT SUM(CRS_KONTR_OUT) - SUM(CRS_KONTR_IN) FROM KONTR_BALANS_OPER_ARC " +
                                           $"WHERE DOC_DATE BETWEEN '20000101' AND '{CustomFormat.DateToString(DateTime.Today)}' AND KONTR_DC = {CustomFormat.DecimalToSqlDecimal(k.Kontragent.DocCode)}";
                    try
                    {
                        var d = ent.Database.SqlQuery<double?>(sqlBalans).FirstOrDefault();
                        var d2 = ent.Database.SqlQuery<double?>(sqlCurrentBalans).FirstOrDefault();
                        k.KontrBalans = Math.Round((decimal) (d ?? 0), 2);
                        k.KontrCrs = k.Kontragent.BalansCurrency.Name;
                        k.KontrCurrentBalans = Math.Round((decimal) (d2 ?? 0), 2);
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                    }
                }
            }

            // Расчет сумм
            foreach (var item in myTempNomenklGroups)
            {
                item.Result = Math.Round(item.Summa - item.Cost - item.DilerSumma, 2);
                item.ResultWOReval = Math.Round(item.Summa - item.CostWOReval - item.DilerSumma, 2);
                item.Price = Math.Round(item.Summa / item.Quantity, 2);
            }
            NomenklGroups.Clear();
            foreach (var d in myTempNomenklGroups)
                NomenklGroups.Add(d);
            foreach (var item in myTempKontrGroups)
            {
                item.Result = Math.Round(item.Summa - item.Cost - item.DilerSumma, 2);
                item.ResultWOReval = Math.Round(item.Summa - item.CostWOReval - item.DilerSumma, 2);
                item.Price = Math.Round(item.Summa / item.Quantity, 2);
            }
            KontrGroups.Clear();
            foreach (var d in myTempKontrGroups)
                KontrGroups.Add(d);
            foreach (var item in myTempCoGroups)
            {
                item.Result = Math.Round(item.Summa - item.Cost - item.DilerSumma, 2);
                item.ResultWOReval = Math.Round(item.Summa - item.CostWOReval - item.DilerSumma, 2);
                item.Price = Math.Round(item.Summa / item.Quantity, 2);
            }
            CoGroups.Clear();
            foreach (var d in myTempCoGroups)
                CoGroups.Add(d);
            foreach (var item in myTempManagerGroups)
            {
                item.Result = Math.Round(item.Summa - item.Cost - item.DilerSumma, 2);
                item.ResultWOReval = Math.Round(item.Summa - item.CostWOReval - item.DilerSumma, 2);
                item.Price = Math.Round(item.Summa / item.Quantity, 2);
            }
            ManagerGroups.Clear();
            foreach (var d in myTempManagerGroups)
                ManagerGroups.Add(d);
        }

        public override void DocumentOpen(object obj)
        {
            DocumentsOpenManager.Open(CurrentDocument.DocType, CurrentDocument.DocCode);
        }
    }
}