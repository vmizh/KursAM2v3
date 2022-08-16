using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.EntityViewModel.NomenklManagement;
using Core.Helper;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Mvvm.DataAnnotations;
using Helper;
using KursAM2.Managers.Nomenkl;

namespace KursAM2.View.Logistiks.UC
{
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    public sealed class AddNomenklFromInvoiceProviderViewModel : RSWindowViewModelBase, IDataUserControl
    {
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());
        #region Constructors

        public AddNomenklFromInvoiceProviderViewModel(Core.EntityViewModel.NomenklManagement.Warehouse warehouse, Kontragent kontr = null)
        {
            Kontragent = kontr;
            Warehouse = warehouse;
            LayoutControl = myDataUserControl = new AddNomenklFromInvoiceProviderUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            RefreshData(null);
        }

        #endregion

        public override string WindowName => "Выбор счета";

        #region Commands

        public override void RefreshData(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var sql = Kontragent == null
                        ? "SELECT t26.DOC_CODE, t26.CODE, t26.SFT_KOL, ISNULL(SUM(t24.DDT_KOL_PRIHOD),0) as Shipped FROM td_26 t26 " +
                          "LEFT OUTER JOIN td_24 t24 ON t24.DDT_SPOST_DC = t26.DOC_CODE AND t24.DDT_SPOST_ROW_CODE = t26.CODE " +
                          "INNER JOIN SD_83 s83 on s83.doc_code = t26.SFT_NEMENKL_DC AND isnull(s83.NOM_0MATER_1USLUGA,0) = 0 " +
                          "GROUP BY t26.DOC_CODE, t26.CODE, t26.SFT_KOL " +
                          "HAVING t26.SFT_KOL -  ISNULL(SUM(t24.DDT_KOL_PRIHOD),0) > 0 "
                        : "SELECT t26.DOC_CODE, t26.CODE, t26.SFT_KOL, ISNULL(SUM(t24.DDT_KOL_PRIHOD), 0) as Shipped " +
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
                            decimal quan = q.Count == 0 ? 0 : q.First().OstatokQuantity;
                            var newRow = new InvoiceShortRow
                            {
                                Id = r.Id,
                                DocCode = r.DOC_CODE,
                                DocId = (Guid) r.DocId,
                                InvoiceQuantity = r.SFT_KOL,
                                Nomenkl = MainReferences.GetNomenkl(r.SFT_NEMENKL_DC),
                                AlreadyShippedQuantity = (decimal) s.Shipped,
                                Code = r.CODE,
                                Note = r.SFT_TEXT,
                                Price = (decimal) (r.SFT_SUMMA_K_OPLATE / r.SFT_KOL),
                                AlreadyShippedSumma =
                                    (decimal) (r.SFT_SUMMA_K_OPLATE / r.SFT_KOL) * (decimal) s.Shipped,
                                IsChecked = true,
                                QuantityOnSklad =  quan,
                                Quantity = r.SFT_KOL - (decimal) s.Shipped
                            };
                            doc.Rows.Add(newRow);
                        }

                        foreach (var r in doc.Rows) doc.AlreadyShipped += r.AlreadyShippedSumma;
                        doc.Remain = doc.Remain = doc.Summa - doc.AlreadyShipped;
                        Invoices.Add(doc);
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

        private readonly Kontragent Kontragent;
        private InvoiceShort myCurrentInvoice;
        private InvoiceShortRow myCurrentNomenkl;
        private readonly Core.EntityViewModel.NomenklManagement.Warehouse Warehouse;
        private AddNomenklFromInvoiceProviderUC myDataUserControl;

        #endregion

        #region Properties

        public ObservableCollection<InvoiceShort> Invoices { set; get; } = new ObservableCollection<InvoiceShort>();

        public ObservableCollection<InvoiceShortRow> Nomenkls { set; get; } =
            new ObservableCollection<InvoiceShortRow>();

        public AddNomenklFromInvoiceProviderUC DataUserControl
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
            }
            get => myCurrentNomenkl;
        }

        public DependencyObject LayoutControl { get; }

        #endregion
    }

    public class SelectedTemp
    {
        public decimal DOC_CODE { set; get; }
        public int CODE { set; get; }
        public decimal SFT_KOL { set; get; }
        public decimal? Shipped { set; get; }
    }

    [MetadataType(typeof(DataAnnotationsInvoiceShortRow))]
    public class InvoiceShortRow
    {
        public decimal DocCode { set; get; }
        public int Code { set; get; }
        public Nomenkl Nomenkl { set; get; }
        public string NomenklNumber => Nomenkl?.NomenklNumber;
        public decimal Price { set; get; }
        public decimal Summa => Price * InvoiceQuantity;
        public decimal InvoiceQuantity { set; get; }

        /// <summary>
        ///     уже отгружено
        /// </summary>
        public decimal AlreadyShippedQuantity { set; get; }

        public decimal AlreadyShippedSumma { set; get; }
        public decimal QuantityOnSklad { set; get; }
        public decimal Quantity { set; get; }
        public string Note { set; get; }
        public bool IsChecked { set; get; }
        public Guid Id { set; get; }
        public Guid DocId { set; get; }
        public InvoiceShort Invoice { set; get; }
    }

    public class DataAnnotationsInvoiceShortRow : DataAnnotationForFluentApiBase, IMetadataProvider<InvoiceShortRow>
    {
        void IMetadataProvider<InvoiceShortRow>.BuildMetadata(MetadataBuilder<InvoiceShortRow> builder)
        {
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Code).NotAutoGenerated();
            builder.Property(_ => _.Id).NotAutoGenerated();
            builder.Property(_ => _.DocId).NotAutoGenerated();
            builder.Property(_ => _.Invoice).AutoGenerated().DisplayName("Счет").ReadOnly();
            builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Номенклатура").ReadOnly();
            builder.Property(_ => _.IsChecked).AutoGenerated().DisplayName("Включено");
            builder.Property(_ => _.NomenklNumber).AutoGenerated().DisplayName("Ном №").ReadOnly();
            builder.Property(_ => _.Price).AutoGenerated().DisplayName("Цена").ReadOnly();
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").ReadOnly();
            builder.Property(_ => _.InvoiceQuantity).AutoGenerated().DisplayName("Кол-во").ReadOnly();
            builder.Property(_ => _.AlreadyShippedQuantity).AutoGenerated().DisplayName("Отгружено").ReadOnly();
            builder.Property(_ => _.AlreadyShippedSumma).AutoGenerated().DisplayName("Отгружено сумма").ReadOnly();
            builder.Property(_ => _.QuantityOnSklad).AutoGenerated().DisplayName("Кол-во на складе").ReadOnly();
            builder.Property(_ => _.Quantity).AutoGenerated().DisplayName("Отгрузить");
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание").ReadOnly();
        }
    }

    [MetadataType(typeof(DataAnnotationsInvoiceShort))]
    public class InvoiceShort
    {
        public decimal DocCode { set; get; }
        public Guid Id { set; get; }
        public DateTime Date { set; get; }
        public string Num { set; get; }
        public Kontragent Kontragent { set; get; }
        public string Note { set; get; }
        public decimal Summa { set; get; }

        /// <summary>
        ///     уже отгружено
        /// </summary>
        public decimal AlreadyShipped { set; get; }

        public decimal Remain { set; get; }
        public Currency Currency { set; get; }
        public List<InvoiceShortRow> Rows { set; get; }

        public override string ToString()
        {
            return $"Счет №{Num} от {Date.ToShortDateString()} Контр:{Kontragent}";
        }
    }

    public class DataAnnotationsInvoiceShort : DataAnnotationForFluentApiBase, IMetadataProvider<InvoiceShort>
    {
        void IMetadataProvider<InvoiceShort>.BuildMetadata(MetadataBuilder<InvoiceShort> builder)
        {
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Id).NotAutoGenerated();
            builder.Property(_ => _.Rows).NotAutoGenerated();
            builder.Property(_ => _.Date).AutoGenerated().DisplayName("Дата").ReadOnly();
            builder.Property(_ => _.Num).AutoGenerated().DisplayName("Номер").ReadOnly();
            builder.Property(_ => _.Kontragent).AutoGenerated().DisplayName("Контрагент").ReadOnly();
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").ReadOnly();
            builder.Property(_ => _.Currency).AutoGenerated().DisplayName("Валюта").ReadOnly();
            builder.Property(_ => _.AlreadyShipped).AutoGenerated().DisplayName("Отгружено").ReadOnly();
            builder.Property(_ => _.Remain).AutoGenerated().DisplayName("Остаток").ReadOnly();
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
        }
    }
}
