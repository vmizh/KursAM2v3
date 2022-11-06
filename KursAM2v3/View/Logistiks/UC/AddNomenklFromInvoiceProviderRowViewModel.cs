using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Helper;
using KursAM2.Managers.Nomenkl;
using KursDomain;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.View.Logistiks.UC
{
    public sealed class AddNomenklFromInvoiceProviderRowViewModel : RSWindowViewModelBase, IDataUserControl
    {
        private readonly Kontragent myKontragent;
        private readonly NomenklManager2 nomenklManager = new(GlobalOptions.GetEntities());

        #region Constructors

        public AddNomenklFromInvoiceProviderRowViewModel(KursDomain.Documents.NomenklManagement.Warehouse warehouse,
            Kontragent kontr = null)
        {
            Warehouse = warehouse;
            LayoutControl = myDataUserControl = new AddNomenklFromInvoiceProviderRowUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор строк из счетов";
            myKontragent = kontr;
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
                        $"INNER JOIN sd_26 s26 ON s26.DOC_CODE = t26.DOC_CODE AND s26.SF_POST_DC = {CustomFormat.DecimalToSqlDecimal(myKontragent.DocCode)}" +
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
                            KontragentViewModel = MainReferences.GetKontragent(d.SF_POST_DC),
                            Date = d.SF_POSTAV_DATE,
                            DocCode = d.DOC_CODE,
                            Num = d.SF_POSTAV_NUM,
                            // ReSharper disable once PossibleInvalidOperationException
                            Currency = MainReferences.Currencies[(decimal) d.SF_CRS_DC],
                            Rows = new List<InvoiceShortRow>()
                        };
                        var rows = ctx.TD_26.Where(_ => _.DOC_CODE == dc).ToList();
                        foreach (var r in rows)
                        {
                            var s = data.FirstOrDefault(_ => _.DOC_CODE == dc && _.CODE == r.CODE);
                            if (s == null) continue;
                            var q = nomenklManager.GetNomenklQuantity(Warehouse.DocCode, r.SFT_NEMENKL_DC,
                                DateTime.Today, DateTime.Today);
                            var quan = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                            var newRow = new InvoiceShortRow
                            {
                                Id = r.Id,
                                DocCode = r.DOC_CODE,
                                // ReSharper disable once PossibleInvalidOperationException
                                DocId = (Guid) r.DocId,
                                InvoiceQuantity = r.SFT_KOL,
                                Nomenkl = MainReferences.GetNomenkl(r.SFT_NEMENKL_DC),
                                // ReSharper disable once PossibleInvalidOperationException
                                AlreadyShippedQuantity = (decimal) s.Shipped,
                                Code = r.CODE,
                                Note = r.SFT_TEXT,
                                // ReSharper disable once PossibleInvalidOperationException
                                Price = (decimal) (r.SFT_SUMMA_K_OPLATE / r.SFT_KOL),
                                AlreadyShippedSumma =
                                    // ReSharper disable once PossibleInvalidOperationException
                                    (decimal) (r.SFT_SUMMA_K_OPLATE / r.SFT_KOL) * (decimal) s.Shipped,
                                IsChecked = false,
                                QuantityOnSklad = quan,
                                Quantity = r.SFT_KOL - (decimal) s.Shipped,
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
        private readonly KursDomain.Documents.NomenklManagement.Warehouse Warehouse;
        private AddNomenklFromInvoiceProviderRowUC myDataUserControl;

        #endregion

        #region Properties

        public ObservableCollection<InvoiceShortRow> Nomenkls { set; get; } = new();

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
