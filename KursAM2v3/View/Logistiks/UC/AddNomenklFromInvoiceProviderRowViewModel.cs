using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Helper;
using KursDomain;
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.View.Logistiks.UC
{
    public sealed class AddNomenklFromInvoiceProviderRowViewModel : RSWindowViewModelBase, IDataUserControl
    {
        private readonly Kontragent myKontragent;
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());
        private readonly decimal? DocDC;
        private readonly List<decimal> RowCodes;

        #region Commands

        public override void RefreshData(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    string sql = null;

                    sql =
                        "SELECT t26.DOC_CODE, t26.CODE, t26.SFT_KOL, ISNULL(SUM(t24.DDT_KOL_PRIHOD), 0) as Shipped " +
                        "FROM td_26 t26 " +
                        "INNER JOIN SD_83 s83 on s83.doc_code = t26.SFT_NEMENKL_DC AND isnull(s83.NOM_0MATER_1USLUGA,0) = 0 " +
                        $"INNER JOIN sd_26 s26 ON s26.DOC_CODE = t26.DOC_CODE AND s26.DOC_CODE = {CustomFormat.DecimalToSqlDecimal(DocDC)}" +
                        "LEFT OUTER JOIN td_24 t24 ON t24.DDT_SPOST_DC = t26.DOC_CODE AND t24.DDT_SPOST_ROW_CODE = t26.CODE " +
                        "GROUP BY t26.DOC_CODE,t26.CODE,t26.SFT_KOL " +
                        "HAVING t26.SFT_KOL - ISNULL(SUM(t24.DDT_KOL_PRIHOD), 0) > 0 ";

                    var data = ctx.Database.SqlQuery<SelectedTemp>(sql).ToList();
                    if (data.Count == 0) return;
                    var d = ctx.SD_26.FirstOrDefault(_ => _.DOC_CODE == DocDC);
                    if (d == null) return;
                    var doc = new InvoiceShort
                    {
                        Id = d.Id,
                        Kontragent = GlobalOptions.ReferencesCache.GetKontragent(d.SF_POST_DC) as Kontragent,
                        Date = d.SF_POSTAV_DATE,
                        DocCode = d.DOC_CODE,
                        Num = d.SF_POSTAV_NUM,
                        // ReSharper disable once PossibleInvalidOperationException
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(d.SF_CRS_DC) as Currency,
                        Rows = new List<InvoiceShortRow>()
                    };
                    var rows = ctx.TD_26.Where(_ => _.DOC_CODE == DocDC).ToList();
                    foreach (var r in rows)
                    {
                        if (RowCodes != null && RowCodes.Any(_ => _ == r.SFT_NEMENKL_DC))
                            continue;
                        var s = data.FirstOrDefault(_ => _.DOC_CODE == DocDC && _.CODE == r.CODE);
                        if (s == null) continue;
                        var q = nomenklManager.GetNomenklQuantity(Warehouse.DocCode, r.SFT_NEMENKL_DC,
                            DateTime.Today, DateTime.Today);
                        var quan = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                        var newRow = new InvoiceShortRow
                        {
                            Id = r.Id,
                            DocCode = r.DOC_CODE,
                            // ReSharper disable once PossibleInvalidOperationException
                            DocId = (Guid)r.DocId,
                            InvoiceQuantity = r.SFT_KOL,
                            Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(r.SFT_NEMENKL_DC) as Nomenkl,
                            // ReSharper disable once PossibleInvalidOperationException
                            AlreadyShippedQuantity = (decimal)s.Shipped,
                            Code = r.CODE,
                            Note = r.SFT_TEXT,
                            // ReSharper disable once PossibleInvalidOperationException
                            Price = (decimal)(r.SFT_SUMMA_K_OPLATE / r.SFT_KOL),
                            AlreadyShippedSumma =
                                // ReSharper disable once PossibleInvalidOperationException
                                (decimal)(r.SFT_SUMMA_K_OPLATE / r.SFT_KOL) * (decimal)s.Shipped,
                            IsChecked = false,
                            QuantityOnSklad = quan,
                            Quantity = r.SFT_KOL - (decimal)s.Shipped,
                            Invoice = doc
                        };
                        Nomenkls.Add(newRow);
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        #endregion

        #region Constructors

        public AddNomenklFromInvoiceProviderRowViewModel(KursDomain.References.Warehouse warehouse,
            Kontragent kontr = null)
        {
            Warehouse = warehouse;
            LayoutControl = myDataUserControl = new AddNomenklFromInvoiceProviderRowUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор строк из счета";
            myKontragent = kontr;
            RefreshData(null);
        }

        public AddNomenklFromInvoiceProviderRowViewModel(KursDomain.References.Warehouse warehouse, decimal sfDC,
            List<decimal> excludeDC)
        {
            RowCodes = excludeDC;
            DocDC = sfDC;
            Warehouse = warehouse;
            LayoutControl = myDataUserControl = new AddNomenklFromInvoiceProviderRowUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sf = ctx.SD_26.Include(_ => _.SD_43).FirstOrDefault(_ => _.DOC_CODE == DocDC);
                var extNum = string.IsNullOrWhiteSpace(sf.SF_POSTAV_NUM) ? null : $"/{sf.SF_POSTAV_NUM}";
                WindowName =
                    $"Выбор строк из счета №{sf.SF_IN_NUM}{extNum} от {sf.SF_POSTAV_DATE.ToShortDateString()} - {sf.SD_43.NAME}";
            }

            //myKontragent = kontr;
            RefreshData(null);
        }

        #endregion

        #region Fields

        private InvoiceShort myCurrentInvoice;
        private InvoiceShortRow myCurrentNomenkl;
        private readonly KursDomain.References.Warehouse Warehouse;
        private AddNomenklFromInvoiceProviderRowUC myDataUserControl;

        #endregion

        #region Properties

        public ObservableCollection<InvoiceShortRow> Nomenkls { set; get; } =
            new ObservableCollection<InvoiceShortRow>();

        public AddNomenklFromInvoiceProviderRowUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceShort CurrentInvoice
        {
            set
            {
                if (myCurrentInvoice == value) return;
                myCurrentInvoice = value;
                Nomenkls = new ObservableCollection<InvoiceShortRow>(myCurrentInvoice.Rows);
                RaisePropertiesChanged();
                RaisePropertyChanged(nameof(Nomenkls));
            }
            get => myCurrentInvoice;
        }

        public InvoiceShortRow CurrentNomenkl
        {
            set
            {
                if (myCurrentNomenkl == null || myCurrentNomenkl == value) return;
                myCurrentNomenkl = value;
                RaisePropertiesChanged();
            }
            get => myCurrentNomenkl;
        }

        public DependencyObject LayoutControl { get; }

        #endregion
    }
}
