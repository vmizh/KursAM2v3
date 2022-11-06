using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Xpf.Bars;
using Helper;
using KursAM2.Managers;
using KursAM2.ViewModel.Management.Calculations;
using KursAM2.ViewModel.Splash;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.CommonReferences.Kontragent;
using KursDomain.Menu;
using KursDomain.References;

// ReSharper disable CollectionNeverQueried.Global
namespace KursAM2.ViewModel.Management.DebitorCreditor
{
    public class DebitorCreditorWindowViewModel : RSWindowViewModelBase
    {
        private DebitorCreditorRow myCurrentCreditor;
        private DebitorCreditorRow myCurrentDebitor;
        private DebitorCreditorRow myCurrentDebitorCreditor;
        private KonragentBalansRowViewModel myCurrentOperation;
        private DateTime myEnd;
        private DateTime myStart;

        public DebitorCreditorWindowViewModel()
        {
            DebitorCreditorAll = new ObservableCollection<DebitorCreditorRow>();
            Debitors = new GalleryCollection<DebitorCreditorRow>();
            Creditors = new ObservableCollection<DebitorCreditorRow>();
            Operations = new ObservableCollection<KonragentBalansRowViewModel>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        }

        public ObservableCollection<DebitorCreditorRow> DebitorCreditorAll { set; get; }
        public ObservableCollection<DebitorCreditorRow> Debitors { set; get; }
        public ObservableCollection<DebitorCreditorRow> Creditors { set; get; }
        public ObservableCollection<KonragentBalansRowViewModel> Operations { set; get; }

        public DebitorCreditorRow CurrentDebitorCreditor
        {
            get => myCurrentDebitorCreditor;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentDebitorCreditor == value) return;
                myCurrentDebitorCreditor = value;
                if (myCurrentDebitorCreditor != null)
                    LoadKontragentOperation(myCurrentDebitorCreditor.KontrInfo.DocCode);
                else
                    Operations.Clear();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Operations));
            }
        }

        public DebitorCreditorRow CurrentDebitor
        {
            get => myCurrentDebitor;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentDebitor == value) return;
                myCurrentDebitor = value;
                if (myCurrentDebitor != null)
                    LoadKontragentOperation(myCurrentDebitor.KontrInfo.DocCode);
                else
                    Operations.Clear();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Operations));
            }
        }

        public decimal DebitorSumma
        {
            get { return DebitorCreditorAll.Where(_ => _.IsBalans && _.KontrEnd >= 0).Sum(_ => _.UchetEnd); }
        }

        public decimal CreditorSumma
        {
            get { return DebitorCreditorAll.Where(_ => _.IsBalans && _.KontrEnd < 0).Sum(_ => _.UchetEnd); }
        }

        public DateTime End
        {
            get
            {
                if (myEnd == DateTime.MinValue)
                    End = DateTime.Today;
                return myEnd;
            }
            set
            {
                if (myEnd == value) return;
                myEnd = value;
                if (myEnd < Start)
                    Start = myEnd;
                RaisePropertyChanged();
            }
        }

        public DateTime Start
        {
            get
            {
                if (myStart == DateTime.MinValue)
                    Start = DateTime.Today;
                return myStart;
            }
            set
            {
                if (myStart == value) return;
                myStart = value;
                if (myStart > End)
                    End = myStart;
                RaisePropertyChanged();
            }
        }

        public decimal BalansSumma => DebitorSumma + CreditorSumma;

        public DebitorCreditorRow CurrentCreditor
        {
            get => myCurrentCreditor;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentCreditor == value) return;
                myCurrentCreditor = value;
                if (myCurrentCreditor != null)
                    LoadKontragentOperation(myCurrentCreditor.KontrInfo.DocCode);
                else
                    Operations.Clear();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Operations));
            }
        }

        public KonragentBalansRowViewModel CurrentOperation
        {
            get => myCurrentOperation;
            set
            {
                if (myCurrentOperation != null && myCurrentOperation.Equals(value)) return;
                myCurrentOperation = value;
                RaisePropertyChanged();
            }
        }

        private void LoadKontragentOperation(decimal kontrDC)
        {
            var debitorCreditorRow = DebitorCreditorAll.FirstOrDefault(_ => _.KontrInfo.DocCode == kontrDC);
            if (debitorCreditorRow == null) return;
            var start = debitorCreditorRow.KontrStart;
            Operations = new ObservableCollection<KonragentBalansRowViewModel>();
            var sql = "SELECT DOC_NAME as DocName, " +
                      "ISNULL(CONVERT(VARCHAR, td_101.CODE), DOC_NUM) AS DocNum, " +
                      "DOC_DATE as DocDate, " +
                      "cast(CRS_KONTR_IN as numeric(18,4)) as CrsKontrIn, " +
                      "cast(CRS_KONTR_OUT as numeric(18,4)) as CrsKontrOut, " +
                      "DOC_DC as DocDC, " +
                      "DOC_ROW_CODE as DocRowCode, " +
                      "DOC_TYPE_CODE as DocTypeCode, " +
                      "cast(CRS_OPER_IN as numeric(18,4)) as CrsOperIn, " +
                      "cast(CRS_OPER_OUT as numeric(18,4)) as CrsOperOut, " +
                      "OPER_CRS_DC as CrsOperDC, " +
                      "cast(OPER_CRS_RATE as numeric(18,4)) as CrsOperRate, " +
                      "cast(UCH_CRS_RATE as numeric(18,4)) as CrsUchRate, " +
                      "KONTR_DC as DocCode, " +
                      "DOC_EXT_NUM as DocExtNum, " +
                      "ISNULL(sd_24.DD_NOTES, ISNULL(Sd_26.SF_NOTES, ISNULL(SD_33.NOTES_ORD, " +
                      "ISNULL(SD_34.NOTES_ORD, ISNULL(sd_84.SF_NOTE, ISNULL(td_101.VVT_DOC_NUM, " +
                      "ISNULL(td_110.VZT_DOC_NOTES, ISNULL(sd_430.ASV_NOTES,'')))))))) AS Notes " +
                      "FROM dbo.KONTR_BALANS_OPER_ARC kboa " +
                      "LEFT OUTER JOIN sd_24 ON DOC_DC = sd_24.DOC_CODE " +
                      "LEFT OUTER JOIN sd_26 ON DOC_DC = sd_26.DOC_CODE " +
                      "LEFT OUTER JOIN SD_33 ON DOC_DC = sd_33.DOC_CODE " +
                      "LEFT OUTER JOIN SD_34 ON DOC_DC = sd_34.DOC_CODE " +
                      "LEFT OUTER JOIN SD_84 ON DOC_DC = sd_84.DOC_CODE " +
                      "LEFT OUTER JOIN td_101 ON DOC_ROW_CODE = td_101.code " +
                      "LEFT OUTER JOIN td_110 ON DOC_DC = td_110.DOC_CODE AND kboa.DOC_ROW_CODE = td_110.code " +
                      "LEFT OUTER JOIN sd_430 ON doc_dc = sd_430.DOC_CODE " +
                      $"WHERE KONTR_DC = {CustomFormat.DecimalToSqlDecimal(kontrDC)} " +
                      $"AND DOC_DATE >= '{CustomFormat.DateToString(Start)}' AND DOC_DATE <= '{CustomFormat.DateToString(End)}' " +
                      "ORDER BY kboa.KONTR_DC,kboa.DOC_DATE;";
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.Database.SqlQuery<KonragentBalansRowViewModel>(sql);
                foreach (var op in data)
                {
                    start += Convert.ToDecimal(op.CrsKontrOut) - Convert.ToDecimal(op.CrsKontrIn);
                    op.Nakopit = start;
                    Operations.Add(op);
                }
            }
            RaisePropertyChanged(nameof(Operations));
        }

        public void ChangedKontr(int i)
        {
            Operations.Clear();
            RaisePropertyChanged(nameof(Operations));
            switch (i)
            {
                case 1:
                    if (myCurrentDebitor != null)
                    {
                        LoadKontragentOperation(myCurrentDebitor.KontrInfo.DocCode);
                        RaisePropertyChanged(nameof(CurrentDebitor));
                    }

                    break;
                case 2:
                    if (myCurrentCreditor != null)
                    {
                        LoadKontragentOperation(myCurrentCreditor.KontrInfo.DocCode);
                        RaisePropertyChanged(nameof(CurrentCreditor));
                    }

                    break;
                case 3:
                    if (myCurrentDebitorCreditor != null)
                    {
                        LoadKontragentOperation(myCurrentDebitorCreditor.KontrInfo.DocCode);
                        RaisePropertyChanged(nameof(CurrentDebitorCreditor));
                    }

                    break;
            }

            RaisePropertyChanged(nameof(Operations));
        }

        public static decimal GetRate(List<CURRENCY_RATES_CB> rates, decimal firstDC, decimal secondDC, DateTime date)
        {
            if (firstDC == secondDC) return 1;
            var d1 = rates.Where(_ => _.RATE_DATE <= date);
            var date1 = !d1.Any() ? DateTime.Today : rates.Where(_ => _.RATE_DATE <= date).Max(_ => _.RATE_DATE);
            var f = rates.SingleOrDefault(_ => _.CRS_DC == firstDC && _.RATE_DATE == date1);
            var s = rates.SingleOrDefault(_ => _.CRS_DC == secondDC && _.RATE_DATE == date1);
            if (f != null && s != null && s.RATE != 0)
                return f.RATE / f.NOMINAL / (s.RATE / s.NOMINAL);
            return -1;
        }

        public override void RefreshData(object obj)
        {
            CurrentCreditor = null;
            CurrentDebitor = null;
            CurrentDebitorCreditor = null;
            Operations.Clear();
            DebitorCreditorAll = new ObservableCollection<DebitorCreditorRow>(Load(Start, End));
            Debitors = new ObservableCollection<DebitorCreditorRow>(DebitorCreditorAll.Where(_ => _.KontrEnd > 0));
            Creditors = new ObservableCollection<DebitorCreditorRow>(DebitorCreditorAll.Where(_ => _.KontrEnd < 0));
            foreach (var kontr in DebitorCreditorAll.Where(_ => _.KontrEnd == 0))
            {
                if (kontr.KontrStart < 0)
                    Creditors.Add(kontr);
                if (kontr.KontrStart > 0)
                    Debitors.Add(kontr);
            }

            try
            {
                var v =
                    (from d in GlobalOptions.GetEntities().SD_43_USER_BLS_NOT_RIGHTS
                        select d).ToList();
                var listDC =
                    (from d in v
                        where d.USER_NAME.ToUpper() == GlobalOptions.UserInfo.Name.ToUpper()
                        select d.KONTR_DC)
                    .ToList();
                foreach (
                    var d in
                    listDC.Select(dd => DebitorCreditorAll.FirstOrDefault(t => t.KontrInfo.DocCode == dd))
                        .Where(d => d != null))
                    DebitorCreditorAll.Remove(d);
                RaisePropertyChanged(nameof(DebitorCreditorAll));
                RaisePropertyChanged(nameof(Debitors));
                RaisePropertyChanged(nameof(Creditors));
                RaisePropertyChanged(nameof(CreditorSumma));
                RaisePropertyChanged(nameof(DebitorSumma));
                RaisePropertyChanged(nameof(BalansSumma));
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        #region Command

        public List<DebitorCreditorRow> Load(DateTime start, DateTime end)
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                //var currencies = ent.SD_301.ToList();
                var kontrs = ent.SD_43.Include(_ => _.SD_301).AsNoTracking().ToList();
                var rates =
                    ent.CURRENCY_RATES_CB.Where(_ => _.RATE_DATE >= start && _.RATE_DATE <= end)
                        .ToList();
                var dt = rates.Select(_ => _.RATE_DATE).Distinct().ToList();
                var kontrChanged = ent.KONTR_BLS_RECALC.Where(_ => _.UserInsert != "dbo");
                var dcU = new List<decimal>(kontrChanged.Select(_ => _.KONTR_DC).Distinct());
                if (dcU.Any())
                {
                    var vm = new DebitorCreditorCalcKontrSplashViewModel
                    {
                        MaxProgress = dcU.Count,
                        Minimum = 0,
                        Progress = 0,
                        ExtendExtendedTextVisibility = Visibility.Visible
                    };
                    SplashScreenService.ShowSplashScreen();
                    SplashScreenService.SetSplashScreenState(vm);
                    foreach (var k in dcU)
                    {
                        var k1 = k;
                        var dd = kontrChanged.Where(_ => _.KONTR_DC == k1).ToList();
                        if (!dd.Any()) continue;
                        var minDate = dd.Min(_ => _.DATE_CHANGED);
                        var kk = kontrs.SingleOrDefault(_ => _.DOC_CODE == k);
                        if (kk != null)
                            vm.ExtendedText = $"Контрагент: '{kk.NAME}' начиная с {minDate.ToShortDateString()}";
                        RecalcKontragentBalans.CalcBalans(k, new DateTime(minDate.Year, minDate.Month, minDate.Day));
                        vm.Progress += 100 / vm.MaxProgress;
                        vm.LabelState = vm.LabelState = $"{vm.Progress}%";
                    }

                    SplashScreenService.HideSplashScreen();
                }

                rates.AddRange(dt.Select(r => new CURRENCY_RATES_CB
                {
                    CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                    NOMINAL = 1,
                    RATE = 1,
                    RATE_DATE = r
                }));
                try
                {
                    var data =
                        ent.Database.SqlQuery<DebCredTemp>(string.Format("SELECT KONTR_DC as rdr0 " +
                                                                         ", cast(sum(BLS_START) AS NUMERIC(18, 2)) as rdr1 " +
                                                                         ", cast(sum(BLS_out) AS NUMERIC(18, 2)) as rdr2 " +
                                                                         ", cast(sum(BLS_in) AS NUMERIC(18, 2)) as rdr3 " +
                                                                         ", cast(sum(BLS_START) - sum(BLS_OUT) + sum(BLS_IN)   AS NUMERIC(18, 2)) as rdr4 " +
                                                                         ", isnull(s.flag_balans,0) as rdr5" +
                                                                         ", s.VALUTA_DC as rdr6 " +
                                                                         "FROM " +
                                                                         " (SELECT KONTR_DC KONTR_DC " +
                                                                         ", 0 BLS_START " +
                                                                         ", SUM(CRS_KONTR_IN ) BLS_IN " +
                                                                         ", SUM( CRS_KONTR_OUT ) BLS_OUT " +
                                                                         ", 0 BLS_END " +
                                                                         "FROM " +
                                                                         " KONTR_BALANS_OPER_ARC KBOA " +
                                                                         " INNER JOIN SD_43 S43 " +
                                                                         "  ON S43.DOC_CODE = KBOA.KONTR_DC " +
                                                                         "WHERE " +
                                                                         " DOC_DATE BETWEEN '{0}' AND '{1}' " +
                                                                         "AND DOC_DATE >= S43.START_BALANS " +
                                                                         "GROUP BY kontr_dc " +
                                                                         " UNION ALL " +
                                                                         "SELECT KONTR_DC " +
                                                                         "    , sum(CRS_KONTR_IN) - sum(CRS_KONTR_OUT) " +
                                                                         "   , 0 " +
                                                                         "  , 0 " +
                                                                         " , 0 " +
                                                                         "FROM " +
                                                                         " KONTR_BALANS_OPER_ARC KBOA " +
                                                                         "INNER JOIN SD_43 S43 " +
                                                                         " ON S43.DOC_CODE = KBOA.KONTR_DC " +
                                                                         "WHERE " +
                                                                         " DOC_DATE < '{0}'  " +
                                                                         "AND DOC_DATE >= S43.START_BALANS " +
                                                                         "GROUP BY KONTR_DC " +
                                                                         "UNION ALL " +
                                                                         "SELECT KONTR_DC " +
                                                                         "    , 0 " +
                                                                         "   , 0 " +
                                                                         "  , 0 " +
                                                                         " , sum(CRS_KONTR_IN) - sum(CRS_KONTR_OUT) " +
                                                                         "FROM " +
                                                                         " KONTR_BALANS_OPER_ARC KBOA " +
                                                                         "INNER JOIN SD_43 S43 " +
                                                                         "ON S43.DOC_CODE = KBOA.KONTR_DC " +
                                                                         "	WHERE " +
                                                                         "	  DOC_DATE <= '{1}' " +
                                                                         "	  AND DOC_DATE >= S43.START_BALANS " +
                                                                         "	GROUP BY " +
                                                                         "	  KONTR_DC) TAB " +
                                                                         "	 inner join sd_43 s on s.doc_code = tab.kontr_dc and isnull(DELETED,0)=0 " +
                                                                         "	GROUP BY " +
                                                                         "	  KONTR_DC, s.flag_balans, s.valuta_dc",
                                CustomFormat.DateToString(start), CustomFormat.DateToString(end)))
                            .ToList();
                    return (from d in data.Where(_ => _.rdr4 != 0 || _.rdr2 != 0 || _.rdr3 != 0 || _.rdr1 != 0)
                        let rate1 =
                            GetRate(rates, d.rdr6, GlobalOptions.SystemProfile.MainCurrency.DocCode, start)
                        let rate2 = GetRate(rates, d.rdr6, GlobalOptions.SystemProfile.MainCurrency.DocCode, end)
                        let kontrInfo =
                            new KontragentViewModel(kontrs.SingleOrDefault(_ => _.DOC_CODE == d.rdr0))
                        select new DebitorCreditorRow
                        {
                            Delta = Math.Round(d.rdr4 * rate2 - (d.rdr1 * rate1 + (d.rdr2 * rate2 - d.rdr3 * rate2)),
                                2),
                            KontrEnd = -d.rdr4,
                            KontrIn = d.rdr2,
                            KontrInfo = kontrInfo,
                            KontrOut = d.rdr3,
                            KontrStart = -d.rdr1,
                            UchetEnd = -Math.Round(d.rdr4 * rate2, 2),
                            UchetIn = Math.Round(d.rdr2 * rate2, 2),
                            UchetOut = Math.Round(d.rdr3 * rate2, 2),
                            UchetStart = -Math.Round(d.rdr1 * rate1, 2),
                            Kontragent = kontrInfo.Name,
                            CurrencyName = kontrInfo.BalansCurrency.Name,
                            IsBalans = d.rdr5 == 1
                        }).ToList();
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }
            }

            return null;
        }

        public override bool IsDocumentOpenAllow =>
            CurrentOperation != null && DocumentsOpenManager.IsDocumentOpen(CurrentOperation.DocTypeCode);

        public override void DocumentOpen(object obj)
        {
            var WinManager = new WindowManager();
            using (var ctx = GlobalOptions.GetEntities())
            {
                switch (CurrentOperation.DocTypeCode)
                {
                    case DocumentType.Bank:
                        DocumentsOpenManager.Open(CurrentOperation.DocTypeCode, CurrentOperation.DocRowCode);
                        return;
                    case DocumentType.AccruedAmountForClient:
                    case DocumentType.AccruedAmountOfSupplier:
                    {
                        //var id = Guid.Parse(CurrentOperation.StringId);
                        var s = CurrentOperation.DocNum.Split('/');
                        var num = Convert.ToInt32(s[0]);
                        if (CurrentOperation.DocTypeCode == DocumentType.AccruedAmountForClient)
                        {
                            var doc = ctx.AccruedAmountForClient.FirstOrDefault(_ => _.DocInNum == num);
                            if(doc != null)
                                DocumentsOpenManager.Open(CurrentOperation.DocTypeCode, 0, doc.Id);
                            else
                            {
                                WinManager.ShowWinUIMessageBox("Документ не найден.","Сообщение");
                            }
                        }

                        if (CurrentOperation.DocTypeCode == DocumentType.AccruedAmountOfSupplier)
                        {
                            var doc = ctx.AccruedAmountOfSupplier.FirstOrDefault(_ => _.DocInNum == num);
                            if(doc != null)
                                DocumentsOpenManager.Open(CurrentOperation.DocTypeCode, 0, doc.Id);
                            else
                            {
                                WinManager.ShowWinUIMessageBox("Документ не найден.","Сообщение");
                            }
                        }

                        return;
                    }
                    case DocumentType.PayRollVedomost:
                        DocumentsOpenManager.Open(CurrentOperation.DocTypeCode, 0,
                            Guid.Parse(CurrentOperation.StringId));
                        return;
                    default:
                        DocumentsOpenManager.Open(CurrentOperation.DocTypeCode, CurrentOperation.DocCode);
                        break;
                }
            }

            //DocumentsOpenManager.Open(CurrentOperation.DocTypeCode, CurrentOperation.DocDC);
        }

        #endregion
    }
}
