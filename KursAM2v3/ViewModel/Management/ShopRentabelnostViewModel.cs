using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Helper;
using KursAM2.View.Base;
using KursAM2.ViewModel.Logistiks;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace KursAM2.ViewModel.Management
{
    public class ShopRentabelnostViewModel : RSWindowViewModelBase
    {
        private readonly Guid myShopId = Guid.Parse("{99F9BA57-2105-40BE-B0DB-4AEF73EF2DCB}");
        private ShopNomenklViewModel myCurrentNomenklItem;
        private DatePeriod myCurrentPeriod;

        public ShopRentabelnostViewModel()
        {
            COList = new ObservableCollection<ShopHeaderViewModel>();
            NomenklList = new ObservableCollection<ShopNomenklViewModel>();
            DocumentsList = new ObservableCollection<ShopDocumentViewModel>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            Period = new List<DatePeriod>();
            var d = new List<DateTime>();
            var dd = DateTime.Today;
            while (dd >= new DateTime(2014, 11, 1))
            {
                d.Add(dd);
                dd = dd.AddDays(-1);
            }

            foreach (var mm in DatePeriod.GenerateIerarhy(d, PeriodIerarhy.YearMonth)
                .Where(_ => _.PeriodType == PeriodType.Month))
                Period.Add(new DatePeriod
                {
                    Id = mm.Id,
                    ParentId = mm.ParentId,
                    Name = mm.DateStart.Year + " " + mm.Name,
                    DateStart = mm.DateStart,
                    DateEnd = mm.DateEnd,
                    PeriodType = PeriodType.Month
                });
            CurrentPeriod = null;
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<ShopHeaderViewModel> COList { set; get; }
        public ObservableCollection<ShopNomenklViewModel> NomenklList { set; get; }
        public ObservableCollection<ShopDocumentViewModel> DocumentsList { set; get; }
        public List<DatePeriod> Period { get; set; }

        public ShopNomenklViewModel CurrentNomenklItem
        {
            get => myCurrentNomenklItem;
            set
            {
                if (myCurrentNomenklItem == value) return;
                myCurrentNomenklItem = value;
                RaisePropertyChanged();
            }
        }

        public bool IsRefreshEnabled => CurrentPeriod != null;

        public DatePeriod CurrentPeriod
        {
            get => myCurrentPeriod;
            set
            {
                if (Equals(value, myCurrentPeriod)) return;
                myCurrentPeriod = value;
                RaisePropertyChanged(nameof(CurrentPeriod));
                RaisePropertyChanged(nameof(IsRefreshEnabled));
            }
        }

        public override ICommand RefreshDataCommand
        {
            get { return new Command(RefreshData, param => IsRefreshEnabled); }
        }

        public ICommand OpenCalcRentabelnostCommand
        {
            get { return new Command(OpenCalcRentabelnost, _ => CurrentNomenklItem != null); }
        }

        private void OpenCalcRentabelnost(object obj)
        {
            if (CurrentNomenklItem?.Nomenkl?.DocCode == null) return;
            var ctx = new NomPriceWindowViewModel((decimal) CurrentNomenklItem?.Nomenkl?.DocCode);
            var dlg = new SelectDialogView {DataContext = ctx};
            dlg.ShowDialog();
        }

        public override void RefreshData(object o)
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var rates =
                    ent.CURRENCY_RATES_CB.Where(
                            _ => _.RATE_DATE >= CurrentPeriod.DateStart && _.RATE_DATE <= DateTime.Today)
                        .ToList();
                var dt = rates.Select(_ => _.RATE_DATE).Distinct().ToList();
                rates.AddRange(dt.Select(r => new CURRENCY_RATES_CB
                {
                    CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                    NOMINAL = 1,
                    RATE = 1,
                    RATE_DATE = r
                }));
                var s1 =
                    "SELECT DISTINCT t.DOC_CODE DocCode, s83.NOM_NOMENKL NomNomenkl, s83.NOM_SALE_CRS_DC AS NomDC, s83.NOM_NAME NomName, t.Kol Kol, " +
                    " t.Summa Summa, SUM(t.Kol) KolIn, SUM(t.Rate * price.PRICE * t.Kol) SummaIn, max(Rate) FROM (SELECT " +
                    " t24.DDT_NOMENKL_DC doc_code, SUM(t24.DDT_KOL_RASHOD) KOL, CAST(SUM(ISNULL(SFT_SUMMA_K_OPLATE, 0) / SFT_KOL * ISNULL(t24.DDT_KOL_RASHOD, 0)) AS NUMERIC(18, 2)) summa, " +
                    " max(ISNULL(crs_nom.RATE, 1) / ISNULL(crs_doc.RATE, 1)) Rate FROM TD_24 t24 " +
                    $" INNER JOIN SD_24 s24 ON s24.doc_code = t24.doc_code AND isnull(s24.DD_VOZVRAT,0) = 0 AND s24.DD_DATE BETWEEN '{CustomFormat.DateToString(CurrentPeriod.DateStart)}' AND '{CustomFormat.DateToString(CurrentPeriod.DateEnd)}' " +
                    " INNER JOIN TD_84 t84 ON t24.DDT_SFACT_DC = t84.doc_code AND t24.DDT_SFACT_ROW_CODE = t84.Code " +
                    " INNER JOIN SD_84 s84 ON s84.doc_code = t84.doc_code AND s84.SF_CENTR_OTV_DC = 10400000012 " +
                    " INNER JOIN SD_83 ss83 ON ss83.doc_code = t24.DDT_NOMENKL_DC " +
                    " LEFT OUTER JOIN CURRENCY_RATES_CB crs_nom ON crs_nom.RATE_DATE = s24.DD_DATE AND crs_nom.crs_dc = ss83.NOM_SALE_CRS_DC " +
                    " LEFT OUTER JOIN CURRENCY_RATES_CB crs_doc ON crs_doc.RATE_DATE = s24.DD_DATE AND crs_doc.crs_dc = s84.SF_CRS_DC " +
                    " GROUP BY t24.DDT_NOMENKL_DC) t " +
                    " INNER JOIN SD_83 s83 ON s83.DOC_CODE = t.DOC_CODE " +
                    " INNER JOIN TD_24 tT24 ON tT24.DDT_NOMENKL_DC = t.DOC_CODE " +
                    $" INNER JOIN SD_24 s24 ON s24.DOC_CODE = tT24.DOC_CODE AND s24.DD_DATE BETWEEN '{CustomFormat.DateToString(CurrentPeriod.DateStart)}' AND '{CustomFormat.DateToString(CurrentPeriod.DateEnd)}'  " +
                    " inner JOIN[dbo].[NOM_PRICE] price ON price.NOM_DC = s83.DOC_CODE AND price.DATE = (SELECT MAX(price2.DATE) " +
                    " FROM NOM_PRICE price2 WHERE price2.NOM_DC = s83.DOC_CODE AND price2.DATE <= s24.DD_DATE) " +
                    " GROUP BY t.DOC_CODE, s83.NOM_NOMENKL, s83.NOM_NAME, t.Kol, t.Summa, s83.NOM_SALE_CRS_DC";
                var data = GlobalOptions.GetEntities().Database.SqlQuery<DataRent>(s1).ToList();
                try
                {
                    NomenklList.Clear();
                    foreach (var d in data)
                        NomenklList.Add(new ShopNomenklViewModel
                        {
                            Nomenkl = MainReferences.GetNomenkl(d.DocCode),
                            Name = d.NomName,
                            NomenklNumber = d.NomNomenkl,
                            Quantity = d.Kol,
                            Price = d.Summa / d.Kol,
                            Summa = d.Summa,
                            CostOne = d.SummaIn / d.KolIn,
                            Cost = d.Kol * (d.SummaIn / d.KolIn),
                            Result = d.Summa - d.Kol * (d.SummaIn / d.KolIn)
                        });
                    COList.Clear();
                    COList.Add(new ShopHeaderViewModel
                    {
                        Name = "Shop",
                        Id = myShopId,
                        Summa = NomenklList.Sum(_ => _.Summa),
                        Sebestoimost = NomenklList.Sum(_ => _.Cost),
                        Result = NomenklList.Sum(_ => _.Result)
                    });
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }

                RaisePropertyChanged(nameof(COList));
                RaisePropertyChanged(nameof(NomenklList));
            }
        }

        public class DataRent
        {
            public decimal DocCode { set; get; }
            public decimal NomDC { set; get; }
            public string NomNomenkl { set; get; }
            public string NomName { set; get; }
            public decimal Kol { set; get; }
            public decimal Summa { set; get; }
            public decimal KolIn { set; get; }
            public decimal SummaIn { set; get; }
            public int IsSFProctNull { set; get; }
        }

        #region Command

        public ICommand PeriodChangedCommand
        {
            get { return new Command(PeriodChanged, param => true); }
        }

        private void PeriodChanged(object obj)
        {
            COList.Clear();
            COList.Add(new ShopHeaderViewModel
            {
                Name = "Shop",
                Id = myShopId,
                Summa = 0,
                Sebestoimost = 0,
                Result = 0
            });
            RefreshData(null);
            RaisePropertyChanged(nameof(COList));
        }

        #endregion
    }
}