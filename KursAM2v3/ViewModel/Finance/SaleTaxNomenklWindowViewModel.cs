using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Windows;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Mvvm.Native;
using KursAM2.View.Finance;
using KursDomain.Documents.Currency;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Finance
{
    public sealed class SaleTaxNomenklWindowViewModel : RSWindowViewModelBase
    {
        public readonly WindowManager WindowManager = new WindowManager();
        private SaleTaxNomenkl myCurrentNomenklRow;
        private KontragentOperationInfo myCurrentPayment;
        private PurchaseRow myCurrentPurchase;
        private DateTime myDateEnd;
        private DateTime myDateStart;
        private CurrencyRates myRates;

        public SaleTaxNomenklWindowViewModel()
        {
            IsDocNewCopyAllow = false;
            IsDocNewCopyRequisiteAllow = false;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.TableEditRightBar(this);
            DateStart = DateTime.Today;
            DateEnd = DateTime.Today;
        }

        public SaleTaxNomenklWindowViewModel(Window form) : base(form)
        {
            IsDocNewCopyAllow = false;
            IsDocNewCopyRequisiteAllow = false;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.TableEditRightBar(this);
            DateStart = DateTime.Today;
            DateEnd = DateTime.Today;
        }

        public ObservableCollection<SaleTaxNomenkl> SaleTaxRows { set; get; } =
            new ObservableCollection<SaleTaxNomenkl>();

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<KontragentOperationInfo> Payments { set; get; } =
            new ObservableCollection<KontragentOperationInfo>();

        public ObservableCollection<PurchaseRow> PurchaseDocuments
            =>
                new ObservableCollection<PurchaseRow>(
                    AllPurchaseDocuments.Where(_ => _.DDT_NOMENKL_DC == CurrentNomenklRow?.DDT_NOMENKL_DC).ToList());

        public ObservableCollection<PurchaseRow> AllPurchaseDocuments { set; get; } =
            new ObservableCollection<PurchaseRow>();

        public SaleTaxNomenkl CurrentNomenklRow
        {
            get => myCurrentNomenklRow;
            set
            {
                if (myCurrentNomenklRow != null && myCurrentNomenklRow.Equals(value)) return;
                myCurrentNomenklRow = value;
                RaisePropertyChanged(nameof(PurchaseDocuments));
                RaisePropertyChanged();
            }
        }

        public KontragentOperationInfo CurrentPayment
        {
            get => myCurrentPayment;
            set
            {
                if (myCurrentPayment != null && myCurrentPayment.Equals(value)) return;
                myCurrentPayment = value;
                RaisePropertyChanged();
            }
        }

        public PurchaseRow CurrentPurchase
        {
            get => myCurrentPurchase;
            set
            {
                if (myCurrentPurchase != null && myCurrentPurchase.Equals(value)) return;
                myCurrentPurchase = value;
                LoadPayments();
                RaisePropertyChanged();
            }
        }

        public DateTime DateStart
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DateEnd
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsCanSaveData
        {
            get { return SaleTaxRows.Any(_ => _.State != RowStatus.NotEdited); }
            set => base.IsCanSaveData = value;
        }

        private void LoadPayments()
        {
            try
            {
                Payments.Clear();
                if (CurrentPurchase == null) return;
                GlobalOptions.GetEntities()
                    .KONTR_BALANS_OPER_ARC
                    .Where(_ => _.KONTR_DC == CurrentPurchase.SD_24.DD_KONTR_OTPR_DC
                                &&
                                (_.DOC_TYPE_CODE == 110 || _.DOC_TYPE_CODE == 101 || _.DOC_TYPE_CODE == 34 ||
                                 _.DOC_TYPE_CODE == 33))
                    .OrderByDescending(_ => _.DOC_DATE)
                    .ForEach(_ => Payments.Add(new KontragentOperationInfo(_)));
                RaisePropertyChanged(nameof(Payments));
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        private void UpdateSystemRate()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    using (var tnx = new TransactionScope())
                    {
                        foreach (var sr in SaleTaxRows)
                        {
                            if (sr.IsAutoTax != null) continue;
                            // ReSharper disable PossibleMultipleEnumeration
                            var nomPurchases =
                                AllPurchaseDocuments.Where(
                                        _ => _.DDT_NOMENKL_DC == sr.DDT_NOMENKL_DC && _.SD_24.DD_DATE <= sr.Date)
                                    .OrderBy(_ => _.TD_26.SD_26.SF_POSTAV_DATE)
                                    .ThenBy(_ => _.TD_26.SD_26.DOC_CODE);
                            if (!nomPurchases.Any()) continue;
                            
                            var nomPurchaseLast = nomPurchases.Last();
                            // ReSharper disable once ConstantConditionalAccessQualifier
                            if (nomPurchaseLast?.TD_26.SD_26.SF_CRS_DC == sr.TD_84.SD_84.SF_CRS_DC)
                            {
                                sr.IsAutoTax = true;
                                sr.IsSaleTax = true;
                                if (nomPurchaseLast.TD_26.SD_26.SF_CRS_RATE != null)
                                    //sr.SystemUpdateRate(
                                    //    myRates.GetRate(sr.SD_83.NOM_SALE_CRS_DC, nomPurchaseLast.TD_26.SD_26.SF_CRS_DC, nomPurchaseLast.SD_24.DD_DATE));
                                    sr.SystemUpdatePrice((nomPurchaseLast.TD_26.SFT_ED_CENA ?? 0) +
                                                         (nomPurchaseLast.TD_26.SFT_SUMMA_NAKLAD ?? 0) /
                                                         nomPurchaseLast.TD_26.SFT_KOL);
                                else continue;
                            }
                            else
                            {
                                continue;
                            }
                            // ReSharper restore PossibleMultipleEnumeration
                            var r = ctx.TD_24.SingleOrDefault(_ => _.DOC_CODE == sr.DOC_CODE && _.CODE == sr.Code);
                            if (r == null) continue;
                            r.SaleTaxRate = sr.Rate;
                            r.SaleTaxNote = sr.Note;
                            r.IsSaleTax = sr.IsSaleTax;
                            r.SaleTaxPrice = sr.SaleTaxPrice;
                            r.IsAutoTax = true;
                            r.TaxUpdater = "Система";
                            sr.UpdatePropertyChanged();
                        }

                        ctx.SaveChanges();
                        tnx.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        private void Refresh(DateTime start, DateTime end)
        {
            myRates = new CurrencyRates(new DateTime(2010, 1, 1), end);
            var ctxTemp = GlobalOptions.GetEntities();
            try
            {
                var data = GlobalOptions.GetEntities()
                    .TD_24.Include(_ => _.SD_24)
                    .Include(_ => _.TD_84)
                    .Include(_ => _.TD_84.SD_84)
                    .Include(_ => _.SD_83)
                    .Where(_ => _.SD_24.DD_DATE >= start && _.SD_24.DD_DATE <= end && _.TD_84 != null
                                && _.TD_84.SD_84.SF_CRS_DC != _.SD_83.NOM_SALE_CRS_DC)
                    .AsNoTracking()
                    .ToList();
                SaleTaxRows = new ObservableCollection<SaleTaxNomenkl>();
                foreach (var d in data)
                {
                    var dd =
                        ctxTemp.NOM_PRICE.Where(_ => _.NOM_DC == d.DDT_NOMENKL_DC && _.DATE <= d.SD_24.DD_DATE)
                            .Max(_ => _.DATE);
                    var np =
                        ctxTemp.NOM_PRICE.Where(_ => _.NOM_DC == d.DDT_NOMENKL_DC && _.DATE == dd)
                            .Select(_ => _.PRICE)
                            .Single();
                    SaleTaxRows.Add(new SaleTaxNomenkl(d, np, np * d.DDT_KOL_RASHOD, d.SaleTaxRate ?? 0, d.SaleTaxNote)
                    {
                        CBRate = myRates.GetRate(d.SD_83.NOM_SALE_CRS_DC, d.TD_84.SD_84.SF_CRS_DC, d.SD_24.DD_DATE),
                        CBCostSumma =
                            np * d.DDT_KOL_RASHOD *
                            myRates.GetRate(d.SD_83.NOM_SALE_CRS_DC, d.TD_84.SD_84.SF_CRS_DC, d.SD_24.DD_DATE)
                    });
                }

                AllPurchaseDocuments = new ObservableCollection<PurchaseRow>();
                var ctx = GlobalOptions.GetEntities();
                foreach (
                    var dlist in
                    SaleTaxRows.Select(_ => _.DDT_NOMENKL_DC)
                        .Distinct()
                        .Select(dc => ctx.TD_24.Include(_ => _.SD_24)
                            .Include(_ => _.TD_26)
                            .Include(_ => _.SD_83)
                            .Include("TD_26.SD_26")
                            .Where(_ => _.DDT_NOMENKL_DC == dc
                                        && _.SD_24.DD_DATE <= DateEnd
                                        && _.TD_26 != null)
                            .ToList()))
                {
                    if (dlist.Count == 0) continue;
                    foreach (var dd in dlist)
                    {
                        var averPrices =
                            GlobalOptions.GetEntities().NOM_PRICE.Where(_ => _.NOM_DC == dd.DDT_NOMENKL_DC).ToList();
                        AllPurchaseDocuments.Add(new PurchaseRow(dd)
                        {
                            AveragePrice =
                                averPrices.Single(
                                        _ =>
                                            _.DATE ==
                                            averPrices.Where(x => x.DATE <= dd.SD_24.DD_DATE).Max(y => y.DATE))
                                    .PRICE,
                            CBRate =
                                myRates.GetRate(dd.SD_83.NOM_SALE_CRS_DC, dd.TD_26.SD_26.SF_CRS_DC, dd.SD_24.DD_DATE)
                        });
                    }
                }

                UpdateSystemRate();
                SaleTaxRows.ForEach(_ => _.UpdatePropertyChanged());
                RaisePropertyChanged(nameof(SaleTaxRows));
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void SaveData(object data)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    using (var tnx = new TransactionScope())
                    {
                        foreach (var row in SaleTaxRows.Where(_ => _.State == RowStatus.Edited))
                        {
                            var r = ctx.TD_24.SingleOrDefault(_ => _.DOC_CODE == row.DOC_CODE && _.CODE == row.Code);
                            if (r == null) continue;
                            r.SaleTaxRate = row.Rate;
                            r.SaleTaxNote = row.Note;
                            r.IsSaleTax = row.IsSaleTax;
                            r.SaleTaxPrice = row.RateCost;
                            r.IsAutoTax = row.IsAutoTax;
                            r.TaxUpdater = row.TaxUpdater;
                        }

                        ctx.SaveChanges();
                        tnx.Complete();
                    }

                    RefreshData(null);
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            Refresh(DateStart, DateEnd);
        }

        public override void ResetLayout(object form)
        {
            var frm = form as SaleTaxNomenklView;
            frm?.LayoutManager.ResetLayout();
        }
    }
}
