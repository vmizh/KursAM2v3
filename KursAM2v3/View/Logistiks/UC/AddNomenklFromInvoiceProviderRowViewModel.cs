using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Helper;
using KursAM2.Managers.Nomenkl;

namespace KursAM2.View.Logistiks.UC
{
    public sealed class AddNomenklFromInvoiceProviderRowViewModel : RSWindowViewModelBase, IDataUserControl
    {
        private readonly Kontragent Kontragent;

        #region Constructors

        public AddNomenklFromInvoiceProviderRowViewModel(Core.EntityViewModel.NomenklManagement.Warehouse warehouse,
            Kontragent kontr = null)
        {
            Warehouse = warehouse;
            LayoutControl = myDataUserControl = new AddNomenklFromInvoiceProviderRowUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор строк из счетов";
            Kontragent = kontr;
            RefreshData(null);
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var sql =
                        "SELECT t26.DOC_CODE, t26.CODE, t26.SFT_KOL, ISNULL(SUM(t24.DDT_KOL_PRIHOD), 0) as Shipped " +
                        "FROM td_26 t26 " +
                        "INNER JOIN SD_83 s83 on s83.doc_code = t26.SFT_NEMENKL_DC AND isnull(s83.NOM_0MATER_1USLUGA,0) = 0 " +
                        $"INNER JOIN sd_26 s26 ON s26.DOC_CODE = t26.DOC_CODE AND s26.SF_POST_DC = {CustomFormat.DecimalToSqlDecimal(Kontragent.DocCode)}" +
                        "LEFT OUTER JOIN td_24 t24 ON t24.DDT_SPOST_DC = t26.DOC_CODE AND t24.DDT_SPOST_ROW_CODE = t26.CODE " +
                        "GROUP BY t26.DOC_CODE,t26.CODE,t26.SFT_KOL " +
                        "HAVING t26.SFT_KOL - ISNULL(SUM(t24.DDT_KOL_PRIHOD), 0) > 0 ";

                    var data = ctx.Database.SqlQuery<SelectedTemp>(sql).ToList();
                    foreach (var dc in data.Select(_ => _.DOC_CODE).Distinct())
                    {
                        var d = ctx.SD_26.FirstOrDefault(_ => _.DOC_CODE == dc);
                        if (d == null) continue;
                        var doc = new InvoiceShort
                        {
                            Id = d.Id,
                            Kontragent = MainReferences.GetKontragent(d.SF_POST_DC),
                            Date = d.SF_POSTAV_DATE,
                            DocCode = d.DOC_CODE,
                            Num = d.SF_POSTAV_NUM,
                            Currency = MainReferences.Currencies[(decimal)d.SF_CRS_DC],
                            Rows = new List<InvoiceShortRow>()
                        };
                        var rows = ctx.TD_26.Where(_ => _.DOC_CODE == dc).ToList();
                        foreach (var r in rows)
                        {
                            var s = data.FirstOrDefault(_ => _.DOC_CODE == dc && _.CODE == r.CODE);
                            if (s == null) continue;
                            var newRow = new InvoiceShortRow
                            {
                                Id = r.Id,
                                DocCode = r.DOC_CODE,
                                DocId = (Guid)r.DocId,
                                InvoiceQuantity = r.SFT_KOL,
                                Nomenkl = MainReferences.GetNomenkl(r.SFT_NEMENKL_DC),
                                AlreadyShippedQuantity = (decimal)s.Shipped,
                                Code = r.CODE,
                                Note = r.SFT_TEXT,
                                Price = (decimal)(r.SFT_SUMMA_K_OPLATE / r.SFT_KOL),
                                AlreadyShippedSumma =
                                    (decimal)(r.SFT_SUMMA_K_OPLATE / r.SFT_KOL) * (decimal)s.Shipped,
                                IsChecked = false,
                                QuantityOnSklad = NomenklManager.GetNomenklCount(r.SFT_NEMENKL_DC, Warehouse.DocCode),
                                Quantity = r.SFT_KOL - (decimal)s.Shipped,
                                Invoice = doc
                            };
                            Nomenkls.Add(newRow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        #endregion

        #region Fields

        private InvoiceShort myCurrentInvoice;
        private InvoiceShortRow myCurrentNomenkl;
        private readonly NomenklManager nomManager = new NomenklManager();
        private readonly Core.EntityViewModel.NomenklManagement.Warehouse Warehouse;
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