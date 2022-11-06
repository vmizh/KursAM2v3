using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Mvvm.DataAnnotations;
using Helper;
using KursDomain.Documents.NomenklManagement;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.View.Logistiks.UC
{
    public class AddNomenklFromOrderInViewModel : RSWindowViewModelBase, IDataUserControl
    {
        #region Constructors

        public AddNomenklFromOrderInViewModel(Kontragent kontr = null)
        {
            myKontragentViewModel = kontr;
            LayoutControl = myDataUserControl = new AddNomenklFromInvoiceProviderRowUC(GetType().Name);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            WindowName = "Выбор не отфакутрованных приходов";
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
                    var sql = "SELECT  t.doc_code as DocCode," +
                              "t.code as Code ,t.DDT_NOMENKL_DC as NomDC," +
                              "s.DD_DATE as DocDate, " +
                              "STR(s.DD_IN_NUM) + ' / ' + ISNULL(s.DD_EXT_NUM,'') as DocNum, " +
                              "SUM(ISNULL(t26.SFT_KOL, 0))  as QuantityFact," +
                              "SUM(t.DDT_KOL_PRIHOD) as QuantityIn " +
                              "FROM TD_24 t " +
                              "INNER JOIN sd_24 s  ON t.DOC_CODE = s.DOC_CODE  " +
                              "AND s.DD_TYPE_DC = 2010000001 " +
                              $"AND s.DD_KONTR_OTPR_DC = {CustomFormat.DecimalToSqlDecimal(myKontragentViewModel.DocCode)} " +
                              "INNER JOIN td_26 t26  ON t.DDT_SPOST_DC = t26.DOC_CODE  " +
                              "AND t.DDT_SPOST_ROW_CODE = t26.CODE " +
                              "GROUP BY t.doc_code, t.CODE, t.DDT_NOMENKL_DC, s.DD_IN_NUM, s.DD_EXT_NUM, s.DD_DATE " +
                              "HAVING SUM(ISNULL(t26.SFT_KOL, 0)) < SUM(t.DDT_KOL_PRIHOD) " +
                              "UNION ALL " +
                              "SELECT t.doc_code ,t.code ,t.DDT_NOMENKL_DC ," +
                              "s.DD_DATE, " +
                              "STR(s.DD_IN_NUM) + ' / ' + ISNULL(s.DD_EXT_NUM,''), " +
                              "CAST(0 AS NUMERIC(18,4)) ,t.DDT_KOL_PRIHOD " +
                              "FROM TD_24 t " +
                              "INNER JOIN sd_24 s  ON t.DOC_CODE = s.DOC_CODE  " +
                              "AND s.DD_TYPE_DC = 2010000001 " +
                              "AND ISNULL(s.DD_VOZVRAT,0) != 1  " +
                              $"AND s.DD_KONTR_OTPR_DC = {CustomFormat.DecimalToSqlDecimal(myKontragentViewModel.DocCode)} " +
                              "WHERE t.DDT_SPOST_DC IS NULL";
                    var data = ctx.Database.SqlQuery<SelectOrderInRowBase>(sql).ToList();
                    foreach (var r in data)
                        Nomenkls.Add(new SelectedOrderInRow
                        {
                            DocCode = r.DocCode,
                            Code = r.Code,
                            DocNum = r.DocNum,
                            DocDate = r.DocDate,
                            Nomenkl = MainReferences.GetNomenkl(r.NomDC),
                            QuantityIn = r.QuantityIn,
                            QuantityFact = r.QuantityFact,
                            Quantity = r.QuantityIn - r.QuantityFact,
                            NomDC = r.NomDC,
                            IsChecked = false
                        });
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        #endregion

        #region Fields

        private AddNomenklFromInvoiceProviderRowUC myDataUserControl;
        private readonly Kontragent myKontragentViewModel;

        #endregion

        #region Properties

        public ObservableCollection<SelectedOrderInRow> Nomenkls { set; get; } =
            new ObservableCollection<SelectedOrderInRow>();

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

        public DependencyObject LayoutControl { get; }

        #endregion
    }

    public class SelectOrderInRowBase
    {
        public decimal DocCode { set; get; }
        public int Code { set; get; }
        public decimal NomDC { set; get; }
        public decimal QuantityFact { set; get; }
        public decimal QuantityIn { set; get; }
        public DateTime DocDate { set; get; }
        public string DocNum { set; get; }
    }

    [MetadataType(typeof(DataAnnotationsSelectedOrderInRow))]
    public class SelectedOrderInRow : SelectOrderInRowBase
    {
        public Nomenkl Nomenkl { set; get; }
        public decimal Quantity { set; get; }
        public bool IsChecked { set; get; }
    }

    public class DataAnnotationsSelectedOrderInRow : DataAnnotationForFluentApiBase,
        IMetadataProvider<SelectedOrderInRow>
    {
        void IMetadataProvider<SelectedOrderInRow>.BuildMetadata(MetadataBuilder<SelectedOrderInRow> builder)
        {
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Code).NotAutoGenerated();
            builder.Property(_ => _.NomDC).NotAutoGenerated();
            builder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата");
            builder.Property(_ => _.DocNum).AutoGenerated().DisplayName("№");
            builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Номенклатура");
            builder.Property(_ => _.QuantityFact).AutoGenerated().DisplayName("Отфактуровано");
            builder.Property(_ => _.QuantityIn).AutoGenerated().DisplayName("Приход");
            builder.Property(_ => _.Quantity).AutoGenerated().DisplayName("Остаток");
            builder.Property(_ => _.IsChecked).AutoGenerated().DisplayName("Выбрать");
        }
    }
}
