using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Controls;
using Core.Finance;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel
{
    [MetadataType(typeof(DataAnnotationsSFClientViewModel))]
    public class InvoiceClient : RSViewModelBase, IEntity<SD_84>
    {
        private Kontragent myClient;
        private CentrOfResponsibility myCO;
        private Currency myCurrency;
        private SD_84 myEntity;
        private FormPay myFormRaschet;
        private Employee myPersonaResponsible;
        private UsagePay myPyCondition;
        private Kontragent myReceiver;
        private decimal mySummaOtgruz;
        private VzaimoraschetType myVzaimoraschetType;

        public InvoiceClient()
        {
            Entity = DefaultValue();
            DeletedRows = new List<InvoiceClientRow>();
            Rows = new ObservableCollection<InvoiceClientRow>();
            ShipmentRows = new ObservableCollection<ShipmentRowViewModel>();
            Rows.CollectionChanged += (o, args) => State = RowStatus.Edited;
        }

        public InvoiceClient(SD_84 entity)
        {
            Entity = entity ?? DefaultValue();
            Rows = new ObservableCollection<InvoiceClientRow>();
            ShipmentRows = new ObservableCollection<ShipmentRowViewModel>();
            LoadReferences();

            // ReSharper disable once PossibleNullReferenceException
            if (Entity.TD_84 == null || Entity.TD_84.Count <= 0) return;
            foreach (var row in Entity.TD_84)
            {
                var newRow = new InvoiceClientRow(row) {Parent = this};
                Rows.Add(newRow);
            }
        }

        //TODO Убрать лишние property
        //public decimal Summa => SF_CRS_SUMMA_K_OPLATE ?? 0;
        //public string COName => CO?.Name;
        public bool IsAccepted
        {
            get => SF_ACCEPTED == 1;
            set
            {
                if (SF_ACCEPTED == 1 == value) return;
                SF_ACCEPTED = (short) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }
        public override Guid Id
        {
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
            get => Entity.Id;
        }
        public Kontragent Diler
        {
            get
            {
                if (Entity.SF_DILER_DC == null) return null;
                return MainReferences.AllKontragents.ContainsKey(Entity.SF_DILER_DC.Value)
                    ? MainReferences.AllKontragents[Entity.SF_DILER_DC.Value]
                    : null;
            }
            set
            {
                if (value == null)
                {
                    Entity.SF_DILER_DC = null;
                }
                else
                {
                    if (Entity.SF_DILER_DC == value.DOC_CODE) return;
                    Entity.SF_DILER_DC = value.DOC_CODE;
                }
                RaisePropertyChanged();
            }
        }
        public List<InvoiceClientRow> DeletedRows { set; get; } = new List<InvoiceClientRow>();
        public ObservableCollection<InvoiceClientRow> Rows { set; get; } =
            new ObservableCollection<InvoiceClientRow>();
        public ObservableCollection<ShipmentRowViewModel> ShipmentRows { set; get; } =
            new ObservableCollection<ShipmentRowViewModel>();
        public ObservableCollection<InvoicePaymentDocument> PaymentDocs { set; get; } =
            new ObservableCollection<InvoicePaymentDocument>();
        public Kontragent Receiver
        {
            //SF_RECEIVER_KONTR_DC
            get => myReceiver;
            set
            {
                if (myReceiver != null && myReceiver.Equals(value)) return;
                myReceiver = value;
                Entity.SF_RECEIVER_KONTR_DC = myReceiver?.DOC_CODE;
                RaisePropertyChanged();
            }
        }
        public Kontragent Client
        {
            get => myClient;
            set
            {
                if (value.Equals(myClient)) return;
                myClient = value;
                SF_CLIENT_DC = myClient.DOC_CODE;
                Entity.SF_CLIENT_NAME = myClient.Name;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        ///     Ответственный
        /// </summary>
        public Employee PersonaResponsible
        {
            get => myPersonaResponsible;
            set
            {
                if (myPersonaResponsible != null && myPersonaResponsible.Equals(value)) return;
                myPersonaResponsible = value;
                Entity.PersonalResponsibleDC = myPersonaResponsible?.DocCode;
                RaisePropertyChanged();
            }
        }
        public CentrOfResponsibility CO
        {
            get => myCO;
            set
            {
                if (myCO != null && myCO.Equals(value)) return;
                myCO = value;
                Entity.SF_CENTR_OTV_DC = myCO?.DocCode;
                RaisePropertyChanged();
            }
        }
        public VzaimoraschetType VzaimoraschetType
        {
            get => myVzaimoraschetType;
            set
            {
                if (myVzaimoraschetType != null && myVzaimoraschetType.Equals(value)) return;
                myVzaimoraschetType = value;
                Entity.SF_VZAIMOR_TYPE_DC = myVzaimoraschetType?.DocCode;
                RaisePropertyChanged();
            }
        }
        public FormPay FormRaschet
        {
            get => myFormRaschet;
            set
            {
                if (myFormRaschet != null && myFormRaschet.Equals(value)) return;
                myFormRaschet = value;
                Entity.SF_FORM_RASCH_DC = myFormRaschet?.DocCode;
                RaisePropertyChanged();
            }
        }
        public UsagePay PayCondition
        {
            get => myPyCondition;
            set
            {
                if (myPyCondition != null && myPyCondition.Equals(value)) return;
                myPyCondition = value;
                if (myPyCondition != null)
                    Entity.SF_PAY_COND_DC = myPyCondition.DocCode;
                RaisePropertyChanged();
            }
        }
        public decimal SummaOtgruz
        {
            get => mySummaOtgruz;
            set
            {
                if (mySummaOtgruz == value) return;
                mySummaOtgruz = value;
                RaisePropertyChanged();
            }
        }
        public Currency Currency
        {
            get => myCurrency;
            set
            {
                if (Equals(myCurrency, value)) return;
                //if (Client != null)
                //{
                //    //WindowManager.ShowMessage("Нельзя менять валюту при выбранном контрагенте.",
                //    //    "Предупреждение", MessageBoxImage.Information);
                //    //RaisePropertyChanged();
                //    return;
                //}
                myCurrency = value;
                if (myCurrency != null)
                {
                    Entity.SF_CRS_DC = myCurrency.DocCode;
                    Entity.SF_KONTR_CRS_DC = myCurrency.DocCode;
                }
                RaisePropertyChanged();
            }
        }
        public decimal PaySumma => (decimal) PaymentDocs?.Sum(_ => _.Summa);
        public override decimal DocCode
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }
        public override string Name
            =>
                $"С/ф №{Entity.SF_IN_NUM}/{Entity.SF_OUT_NUM} от {Entity.SF_DATE.ToShortDateString()} {SF_CRS_SUMMA_K_OPLATE} {Currency} {Note}";
        public int SF_IN_NUM
        {
            get => Entity.SF_IN_NUM;
            set
            {
                if (Entity.SF_IN_NUM == value) return;
                Entity.SF_IN_NUM = value;
                RaisePropertyChanged();
            }
        }
        public string SF_OUT_NUM
        {
            get => Entity.SF_OUT_NUM;
            set
            {
                if (Entity.SF_OUT_NUM == value) return;
                Entity.SF_OUT_NUM = value;
                RaisePropertyChanged();
            }
        }
        public DateTime SF_DATE
        {
            get => Entity.SF_DATE;
            set
            {
                if (Entity.SF_DATE == value) return;
                Entity.SF_DATE = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_CLIENT_DC
        {
            get => Entity.SF_CLIENT_DC;
            set
            {
                if (Entity.SF_CLIENT_DC == value) return;
                Entity.SF_CLIENT_DC = value;
                RaisePropertyChanged();
            }
        }
        public string SF_CLIENT_NAME
        {
            get => Entity.SF_CLIENT_NAME;
            set
            {
                if (Entity.SF_CLIENT_NAME == value) return;
                Entity.SF_CLIENT_NAME = value;
                RaisePropertyChanged();
            }
        }
        public int? SF_CLIENT_RS_CODE
        {
            get => Entity.SF_CLIENT_RS_CODE;
            set
            {
                if (Entity.SF_CLIENT_RS_CODE == value) return;
                Entity.SF_CLIENT_RS_CODE = value;
                RaisePropertyChanged();
            }
        }
        public decimal SF_CRS_DC
        {
            get => Entity.SF_CRS_DC;
            set
            {
                if (Entity.SF_CRS_DC == value) return;
                Entity.SF_CRS_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_CRS_SUMMA_K_OPLATE =>
            Rows == null || Rows.Count == 0 ? 0 : Rows.Sum(_ => _.SFT_SUMMA_K_OPLATE);
        public double SF_CRS_RATE
        {
            get => Entity.SF_CRS_RATE;
            set
            {
                if (Math.Abs(Entity.SF_CRS_RATE - value) < 0) return;
                Entity.SF_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_RUB_SUMMA_K_OPLATE
        {
            get => Entity.SF_RUB_SUMMA_K_OPLATE;
            set
            {
                if (Entity.SF_RUB_SUMMA_K_OPLATE == value) return;
                Entity.SF_RUB_SUMMA_K_OPLATE = value;
                RaisePropertyChanged();
            }
        }
        public decimal SF_PAY_COND_DC
        {
            get => Entity.SF_PAY_COND_DC;
            set
            {
                if (Entity.SF_PAY_COND_DC == value) return;
                Entity.SF_PAY_COND_DC = value;
                RaisePropertyChanged();
            }
        }
        public string CREATOR
        {
            get => Entity.CREATOR;
            set
            {
                if (Entity.CREATOR == value) return;
                Entity.CREATOR = value;
                RaisePropertyChanged();
            }
        }
        public short SF_ACCEPTED
        {
            get => Entity.SF_ACCEPTED;
            set
            {
                if (Entity.SF_ACCEPTED == value) return;
                Entity.SF_ACCEPTED = value;
                RaisePropertyChanged();
            }
        }
        public string SF_GROZOOTPRAVITEL
        {
            get => Entity.SF_GROZOOTPRAVITEL;
            set
            {
                if (Entity.SF_GROZOOTPRAVITEL == value) return;
                Entity.SF_GROZOOTPRAVITEL = value;
                RaisePropertyChanged();
            }
        }
        public string SF_GRUZOPOLUCHATEL
        {
            get => Entity.SF_GRUZOPOLUCHATEL;
            set
            {
                if (Entity.SF_GRUZOPOLUCHATEL == value) return;
                Entity.SF_GRUZOPOLUCHATEL = value;
                RaisePropertyChanged();
            }
        }
        public string SF_PAYDOC_TEXT
        {
            get => Entity.SF_PAYDOC_TEXT;
            set
            {
                if (Entity.SF_PAYDOC_TEXT == value) return;
                Entity.SF_PAYDOC_TEXT = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_BASE_DC
        {
            get => Entity.SF_BASE_DC;
            set
            {
                if (Entity.SF_BASE_DC == value) return;
                Entity.SF_BASE_DC = value;
                RaisePropertyChanged();
            }
        }
        public string SF_DOPOLN
        {
            get => Entity.SF_DOPOLN;
            set
            {
                if (Entity.SF_DOPOLN == value) return;
                Entity.SF_DOPOLN = value;
                RaisePropertyChanged();
            }
        }
        public short? SF_SCHET_FLAG
        {
            get => Entity.SF_SCHET_FLAG;
            set
            {
                if (Entity.SF_SCHET_FLAG == value) return;
                Entity.SF_SCHET_FLAG = value;
                RaisePropertyChanged();
            }
        }
        public short? SF_SCHET_FACT_FLAG
        {
            get => Entity.SF_SCHET_FACT_FLAG;
            set
            {
                if (Entity.SF_SCHET_FACT_FLAG == value) return;
                Entity.SF_SCHET_FACT_FLAG = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_VZAIMOR_TYPE_DC
        {
            get => Entity.SF_VZAIMOR_TYPE_DC;
            set
            {
                if (Entity.SF_VZAIMOR_TYPE_DC == value) return;
                Entity.SF_VZAIMOR_TYPE_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_FACT_SUMMA
        {
            get => Entity.SF_FACT_SUMMA;
            set
            {
                if (Entity.SF_FACT_SUMMA == value) return;
                Entity.SF_FACT_SUMMA = value;
                RaisePropertyChanged();
            }
        }
        public short? SF_PAY_FLAG
        {
            get => Entity.SF_PAY_FLAG;
            set
            {
                if (Entity.SF_PAY_FLAG == value) return;
                Entity.SF_PAY_FLAG = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_CENTR_OTV_DC
        {
            get => Entity.SF_CENTR_OTV_DC;
            set
            {
                if (Entity.SF_CENTR_OTV_DC == value) return;
                Entity.SF_CENTR_OTV_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_RECEIVER_KONTR_DC
        {
            get => Entity.SF_RECEIVER_KONTR_DC;
            set
            {
                if (Entity.SF_RECEIVER_KONTR_DC == value) return;
                Entity.SF_RECEIVER_KONTR_DC = value;
                RaisePropertyChanged();
            }
        }
        public int? SF_RECEIVER_RS_CODE
        {
            get => Entity.SF_RECEIVER_RS_CODE;
            set
            {
                if (Entity.SF_RECEIVER_RS_CODE == value) return;
                Entity.SF_RECEIVER_RS_CODE = value;
                RaisePropertyChanged();
            }
        }
        public short? SF_TRANZIT
        {
            get => Entity.SF_TRANZIT;
            set
            {
                if (Entity.SF_TRANZIT == value) return;
                Entity.SF_TRANZIT = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_TRANZIT_SPOST_DC
        {
            get => Entity.SF_TRANZIT_SPOST_DC;
            set
            {
                if (Entity.SF_TRANZIT_SPOST_DC == value) return;
                Entity.SF_TRANZIT_SPOST_DC = value;
                RaisePropertyChanged();
            }
        }
        public double? SF_TRANZIT_NACEN_PERC
        {
            get => Entity.SF_TRANZIT_NACEN_PERC;
            set
            {
                if (Entity.SF_TRANZIT_NACEN_PERC == value) return;
                Entity.SF_TRANZIT_NACEN_PERC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_KONTR_CRS_DC
        {
            get => Entity.SF_KONTR_CRS_DC;
            set
            {
                if (Entity.SF_KONTR_CRS_DC == value) return;
                Entity.SF_KONTR_CRS_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_KONTR_CRS_SUMMA
        {
            get => Entity.SF_KONTR_CRS_SUMMA;
            set
            {
                if (Entity.SF_KONTR_CRS_SUMMA == value) return;
                Entity.SF_KONTR_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }
        public double? SF_KONTR_CRS_RATE
        {
            get => Entity.SF_KONTR_CRS_RATE;
            set
            {
                if (Entity.SF_KONTR_CRS_RATE == value) return;
                Entity.SF_KONTR_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_UCHET_VALUTA_DC
        {
            get => Entity.SF_UCHET_VALUTA_DC;
            set
            {
                if (Entity.SF_UCHET_VALUTA_DC == value) return;
                Entity.SF_UCHET_VALUTA_DC = value;
                RaisePropertyChanged();
            }
        }
        public double? SF_UCHET_VALUTA_RATE
        {
            get => Entity.SF_UCHET_VALUTA_RATE;
            set
            {
                if (Entity.SF_UCHET_VALUTA_RATE == value) return;
                Entity.SF_UCHET_VALUTA_RATE = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_SUMMA_V_UCHET_VALUTE
        {
            get => Entity.SF_SUMMA_V_UCHET_VALUTE;
            set
            {
                if (Entity.SF_SUMMA_V_UCHET_VALUTE == value) return;
                Entity.SF_SUMMA_V_UCHET_VALUTE = value;
                RaisePropertyChanged();
            }
        }
        public short? SF_NDS_1INCLUD_0NO
        {
            get => Entity.SF_NDS_1INCLUD_0NO;
            set
            {
                if (Entity.SF_NDS_1INCLUD_0NO == value) return;
                Entity.SF_NDS_1INCLUD_0NO = value;
                RaisePropertyChanged();
            }
        }
        public bool IsNDSIncludeInPrice
        {
            get => (SF_NDS_1INCLUD_0NO ?? 0) == 1;
            set
            {
                if ((SF_NDS_1INCLUD_0NO ?? 0) == 1 == value) return;
                SF_NDS_1INCLUD_0NO = (short?) ((SF_NDS_1INCLUD_0NO ?? 0) == 1 ? 0 : 1);
                foreach (var r in Rows) r.IsNDSInPrice = (SF_NDS_1INCLUD_0NO ?? 0) == 1;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_FORM_RASCH_DC
        {
            get => Entity.SF_FORM_RASCH_DC;
            set
            {
                if (Entity.SF_FORM_RASCH_DC == value) return;
                Entity.SF_FORM_RASCH_DC = value;
                RaisePropertyChanged();
            }
        }
        public double? SF_NALOG_NA_PROD_PROC
        {
            get => Entity.SF_NALOG_NA_PROD_PROC;
            set
            {
                if (Entity.SF_NALOG_NA_PROD_PROC == value) return;
                Entity.SF_NALOG_NA_PROD_PROC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_DILER_DC
        {
            get => Entity.SF_DILER_DC;
            set
            {
                if (Entity.SF_DILER_DC == value) return;
                Entity.SF_DILER_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_DILER_SUMMA
        {
            get => Entity.SF_DILER_SUMMA;
            set
            {
                if (Entity.SF_DILER_SUMMA == value) return;
                Entity.SF_DILER_SUMMA = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_DILER_CRS_DC
        {
            get => Entity.SF_DILER_CRS_DC;
            set
            {
                if (Entity.SF_DILER_CRS_DC == value) return;
                Entity.SF_DILER_CRS_DC = value;
                RaisePropertyChanged();
            }
        }
        public double? SF_DILER_RATE
        {
            get => Entity.SF_DILER_RATE;
            set
            {
                if (Entity.SF_DILER_RATE == value) return;
                Entity.SF_DILER_RATE = value;
                RaisePropertyChanged();
            }
        }
        public short? SF_1INCLUD_NAL_S_PROD_0NO
        {
            get => Entity.SF_1INCLUD_NAL_S_PROD_0NO;
            set
            {
                if (Entity.SF_1INCLUD_NAL_S_PROD_0NO == value) return;
                Entity.SF_1INCLUD_NAL_S_PROD_0NO = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SF_DOP_USL_DC
        {
            get => Entity.SF_DOP_USL_DC;
            set
            {
                if (Entity.SF_DOP_USL_DC == value) return;
                Entity.SF_DOP_USL_DC = value;
                RaisePropertyChanged();
            }
        }
        public short? SF_AUTOMAT_CREATE
        {
            get => Entity.SF_AUTOMAT_CREATE;
            set
            {
                if (Entity.SF_AUTOMAT_CREATE == value) return;
                Entity.SF_AUTOMAT_CREATE = value;
                RaisePropertyChanged();
            }
        }
        public string SF_NOTE
        {
            get => Entity.SF_NOTE;
            set
            {
                if (Entity.SF_NOTE == value) return;
                Entity.SF_NOTE = value;
                RaisePropertyChanged();
            }
        }
        public int? SF_SOTRUDNIK_TN
        {
            get => Entity.SF_SOTRUDNIK_TN;
            set
            {
                if (Entity.SF_SOTRUDNIK_TN == value) return;
                Entity.SF_SOTRUDNIK_TN = value;
                RaisePropertyChanged();
            }
        }
        public byte[] TSTAMP
        {
            get => Entity.TSTAMP;
            set
            {
                if (Entity.TSTAMP == value) return;
                Entity.TSTAMP = value;
                RaisePropertyChanged();
            }
        }
        public string SF_CORRECT_NUM
        {
            get => Entity.SF_CORRECT_NUM;
            set
            {
                if (Entity.SF_CORRECT_NUM == value) return;
                Entity.SF_CORRECT_NUM = value;
                RaisePropertyChanged();
            }
        }
        public DateTime? SF_CORRECT_DATE
        {
            get => Entity.SF_CORRECT_DATE;
            set
            {
                if (Entity.SF_CORRECT_DATE == value) return;
                Entity.SF_CORRECT_DATE = value;
                RaisePropertyChanged();
            }
        }
        public string SF_CRS_CODE
        {
            get => Entity.SF_CRS_CODE;
            set
            {
                if (Entity.SF_CRS_CODE == value) return;
                Entity.SF_CRS_CODE = value;
                RaisePropertyChanged();
            }
        }
        public string CORRECTION_NUM
        {
            get => Entity.CORRECTION_NUM;
            set
            {
                if (Entity.CORRECTION_NUM == value) return;
                Entity.CORRECTION_NUM = value;
                RaisePropertyChanged();
            }
        }
        public string CORRECTION_DATE
        {
            get => Entity.CORRECTION_DATE;
            set
            {
                if (Entity.CORRECTION_DATE == value) return;
                Entity.CORRECTION_DATE = value;
                RaisePropertyChanged();
            }
        }
        public string COUNTRY_NAME
        {
            get => Entity.COUNTRY_NAME;
            set
            {
                if (Entity.COUNTRY_NAME == value) return;
                Entity.COUNTRY_NAME = value;
                RaisePropertyChanged();
            }
        }
        public string COUNTRY_CODE
        {
            get => Entity.COUNTRY_CODE;
            set
            {
                if (Entity.COUNTRY_CODE == value) return;
                Entity.COUNTRY_CODE = value;
                RaisePropertyChanged();
            }
        }
        public string KPP
        {
            get => Entity.KPP;
            set
            {
                if (Entity.KPP == value) return;
                Entity.KPP = value;
                RaisePropertyChanged();
            }
        }
        public Guid? GUID_ID
        {
            get => Entity.GUID_ID;
            set
            {
                if (Entity.GUID_ID == value) return;
                Entity.GUID_ID = value;
                RaisePropertyChanged();
            }
        }
        public Guid? GRUZO_INFO_ID
        {
            get => Entity.GRUZO_INFO_ID;
            set
            {
                if (Entity.GRUZO_INFO_ID == value) return;
                Entity.GRUZO_INFO_ID = value;
                RaisePropertyChanged();
            }
        }
        public DateTime? REGISTER_DATE
        {
            get => Entity.REGISTER_DATE;
            set
            {
                if (Entity.REGISTER_DATE == value) return;
                Entity.REGISTER_DATE = value;
                RaisePropertyChanged();
            }
        }
        public decimal? BUH_CLIENT_DC
        {
            get => Entity.BUH_CLIENT_DC;
            set
            {
                if (Entity.BUH_CLIENT_DC == value) return;
                Entity.BUH_CLIENT_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? BUH_RECEIVER_DC
        {
            get => Entity.BUH_RECEIVER_DC;
            set
            {
                if (Entity.BUH_RECEIVER_DC == value) return;
                Entity.BUH_RECEIVER_DC = value;
                RaisePropertyChanged();
            }
        }
        public string NAKL_GRUZOOTPRAV
        {
            get => Entity.NAKL_GRUZOOTPRAV;
            set
            {
                if (Entity.NAKL_GRUZOOTPRAV == value) return;
                Entity.NAKL_GRUZOOTPRAV = value;
                RaisePropertyChanged();
            }
        }
        public string NAKL_GRUZOPOLUCH
        {
            get => Entity.NAKL_GRUZOPOLUCH;
            set
            {
                if (Entity.NAKL_GRUZOPOLUCH == value) return;
                Entity.NAKL_GRUZOPOLUCH = value;
                RaisePropertyChanged();
            }
        }
        public GROZO_REQUISITE GROZO_REQUISITE
        {
            get => Entity.GROZO_REQUISITE;
            set
            {
                if (Entity.GROZO_REQUISITE == value) return;
                Entity.GROZO_REQUISITE = value;
                RaisePropertyChanged();
            }
        }
        public SD_179 SD_179
        {
            get => Entity.SD_179;
            set
            {
                if (Entity.SD_179 == value) return;
                Entity.SD_179 = value;
                RaisePropertyChanged();
            }
        }
        public SD_189 SD_189
        {
            get => Entity.SD_189;
            set
            {
                if (Entity.SD_189 == value) return;
                Entity.SD_189 = value;
                RaisePropertyChanged();
            }
        }
        public SD_301 SD_301
        {
            get => Entity.SD_301;
            set
            {
                if (Entity.SD_301 == value) return;
                Entity.SD_301 = value;
                RaisePropertyChanged();
            }
        }
        public SD_301 SD_3011
        {
            get => Entity.SD_3011;
            set
            {
                if (Entity.SD_3011 == value) return;
                Entity.SD_3011 = value;
                RaisePropertyChanged();
            }
        }
        public SD_40 SD_40
        {
            get => Entity.SD_40;
            set
            {
                if (Entity.SD_40 == value) return;
                Entity.SD_40 = value;
                RaisePropertyChanged();
            }
        }
        public SD_43 SD_43
        {
            get => Entity.SD_43;
            set
            {
                if (Entity.SD_43 == value) return;
                Entity.SD_43 = value;
                RaisePropertyChanged();
            }
        }
        public SD_43 SD_431
        {
            get => Entity.SD_431;
            set
            {
                if (Entity.SD_431 == value) return;
                Entity.SD_431 = value;
                RaisePropertyChanged();
            }
        }
        public SD_43 SD_432
        {
            get => Entity.SD_432;
            set
            {
                if (Entity.SD_432 == value) return;
                Entity.SD_432 = value;
                RaisePropertyChanged();
            }
        }
        public SD_43 SD_433
        {
            get => Entity.SD_433;
            set
            {
                if (Entity.SD_433 == value) return;
                Entity.SD_433 = value;
                RaisePropertyChanged();
            }
        }
        public SD_43 SD_434
        {
            get => Entity.SD_434;
            set
            {
                if (Entity.SD_434 == value) return;
                Entity.SD_434 = value;
                RaisePropertyChanged();
            }
        }
        public SD_77 SD_77
        {
            get => Entity.SD_77;
            set
            {
                if (Entity.SD_77 == value) return;
                Entity.SD_77 = value;
                RaisePropertyChanged();
            }
        }
        public SD_84 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }
        public EntityLoadCodition LoadCondition { get; set; }

        public List<SD_84> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public void UpdateActualValues()
        {
            RaisePropertiesChanged(nameof(Rows));
            RaisePropertiesChanged(nameof(SF_CRS_SUMMA_K_OPLATE));
        }

        private void LoadReferences()
        {
            if (MainReferences.Currencies.ContainsKey(Entity.SF_CRS_DC))
                Currency = MainReferences.Currencies[Entity.SF_CRS_DC];
            if (Entity.SF_CLIENT_DC != null) Client = MainReferences.GetKontragent(Entity.SF_CLIENT_DC);
            if (Entity.SF_CENTR_OTV_DC != null) CO = MainReferences.COList[Entity.SF_CENTR_OTV_DC.Value];
            if (Entity.SF_RECEIVER_KONTR_DC != null)
                Receiver = MainReferences.GetKontragent(Entity.SF_RECEIVER_KONTR_DC);
            if (Entity.SF_FORM_RASCH_DC != null)
                FormRaschet = MainReferences.FormRaschets[Entity.SF_FORM_RASCH_DC.Value];
            if (Entity.SD_179 != null)
                PayCondition = MainReferences.PayConditions[Entity.SD_179.DOC_CODE];
            if (Entity.SD_77 != null)
                VzaimoraschetType = MainReferences.VzaimoraschetTypes[Entity.SD_77.DOC_CODE];
            if (Entity.SF_CENTR_OTV_DC != null)
                CO = MainReferences.COList[Entity.SF_CENTR_OTV_DC.Value];
            if (Entity.PersonalResponsibleDC != null)
                PersonaResponsible = MainReferences.Employees[Entity.PersonalResponsibleDC.Value];
            SummaOtgruz = 0;
            if (Entity.TD_84 != null && Entity.TD_84.Count > 0)
                foreach (var d in Entity.TD_84)
                foreach (var o in d.TD_24)
                    SummaOtgruz += o.DDT_KOL_RASHOD * ((d.SFT_SUMMA_K_OPLATE ?? 0) / (decimal) d.SFT_KOL);
        }

        public virtual void Save(SD_84 doc)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Delete(decimal dc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(SD_84 ent)
        {
            SF_IN_NUM = ent.SF_IN_NUM;
            SF_OUT_NUM = ent.SF_OUT_NUM;
            SF_DATE = ent.SF_DATE;
            SF_CLIENT_DC = ent.SF_CLIENT_DC;
            SF_CLIENT_NAME = ent.SF_CLIENT_NAME;
            SF_CLIENT_RS_CODE = ent.SF_CLIENT_RS_CODE;
            SF_CRS_DC = ent.SF_CRS_DC;
            SF_CRS_RATE = ent.SF_CRS_RATE;
            SF_RUB_SUMMA_K_OPLATE = ent.SF_RUB_SUMMA_K_OPLATE;
            SF_PAY_COND_DC = ent.SF_PAY_COND_DC;
            CREATOR = ent.CREATOR;
            SF_ACCEPTED = ent.SF_ACCEPTED;
            SF_GROZOOTPRAVITEL = ent.SF_GROZOOTPRAVITEL;
            SF_GRUZOPOLUCHATEL = ent.SF_GRUZOPOLUCHATEL;
            SF_PAYDOC_TEXT = ent.SF_PAYDOC_TEXT;
            SF_BASE_DC = ent.SF_BASE_DC;
            SF_DOPOLN = ent.SF_DOPOLN;
            SF_SCHET_FLAG = ent.SF_SCHET_FLAG;
            SF_SCHET_FACT_FLAG = ent.SF_SCHET_FACT_FLAG;
            SF_VZAIMOR_TYPE_DC = ent.SF_VZAIMOR_TYPE_DC;
            SF_FACT_SUMMA = ent.SF_FACT_SUMMA;
            SF_PAY_FLAG = ent.SF_PAY_FLAG;
            SF_CENTR_OTV_DC = ent.SF_CENTR_OTV_DC;
            SF_RECEIVER_KONTR_DC = ent.SF_RECEIVER_KONTR_DC;
            SF_RECEIVER_RS_CODE = ent.SF_RECEIVER_RS_CODE;
            SF_TRANZIT = ent.SF_TRANZIT;
            SF_TRANZIT_SPOST_DC = ent.SF_TRANZIT_SPOST_DC;
            SF_TRANZIT_NACEN_PERC = ent.SF_TRANZIT_NACEN_PERC;
            SF_KONTR_CRS_DC = ent.SF_KONTR_CRS_DC;
            SF_KONTR_CRS_SUMMA = ent.SF_KONTR_CRS_SUMMA;
            SF_KONTR_CRS_RATE = ent.SF_KONTR_CRS_RATE;
            SF_UCHET_VALUTA_DC = ent.SF_UCHET_VALUTA_DC;
            SF_UCHET_VALUTA_RATE = ent.SF_UCHET_VALUTA_RATE;
            SF_SUMMA_V_UCHET_VALUTE = ent.SF_SUMMA_V_UCHET_VALUTE;
            SF_NDS_1INCLUD_0NO = ent.SF_NDS_1INCLUD_0NO;
            SF_FORM_RASCH_DC = ent.SF_FORM_RASCH_DC;
            SF_NALOG_NA_PROD_PROC = ent.SF_NALOG_NA_PROD_PROC;
            SF_DILER_DC = ent.SF_DILER_DC;
            SF_DILER_SUMMA = ent.SF_DILER_SUMMA;
            SF_DILER_CRS_DC = ent.SF_DILER_CRS_DC;
            SF_DILER_RATE = ent.SF_DILER_RATE;
            SF_1INCLUD_NAL_S_PROD_0NO = ent.SF_1INCLUD_NAL_S_PROD_0NO;
            SF_DOP_USL_DC = ent.SF_DOP_USL_DC;
            SF_AUTOMAT_CREATE = ent.SF_AUTOMAT_CREATE;
            SF_NOTE = ent.SF_NOTE;
            SF_SOTRUDNIK_TN = ent.SF_SOTRUDNIK_TN;
            TSTAMP = ent.TSTAMP;
            SF_CORRECT_NUM = ent.SF_CORRECT_NUM;
            SF_CORRECT_DATE = ent.SF_CORRECT_DATE;
            SF_CRS_CODE = ent.SF_CRS_CODE;
            CORRECTION_NUM = ent.CORRECTION_NUM;
            CORRECTION_DATE = ent.CORRECTION_DATE;
            COUNTRY_NAME = ent.COUNTRY_NAME;
            COUNTRY_CODE = ent.COUNTRY_CODE;
            KPP = ent.KPP;
            GUID_ID = ent.GUID_ID;
            GRUZO_INFO_ID = ent.GRUZO_INFO_ID;
            REGISTER_DATE = ent.REGISTER_DATE;
            BUH_CLIENT_DC = ent.BUH_CLIENT_DC;
            BUH_RECEIVER_DC = ent.BUH_RECEIVER_DC;
            NAKL_GRUZOOTPRAV = ent.NAKL_GRUZOOTPRAV;
            NAKL_GRUZOPOLUCH = ent.NAKL_GRUZOPOLUCH;
            GROZO_REQUISITE = ent.GROZO_REQUISITE;
            SD_179 = ent.SD_179;
            SD_189 = ent.SD_189;
            SD_301 = ent.SD_301;
            SD_3011 = ent.SD_3011;
            SD_40 = ent.SD_40;
            SD_43 = ent.SD_43;
            SD_431 = ent.SD_431;
            SD_432 = ent.SD_432;
            SD_433 = ent.SD_433;
            SD_434 = ent.SD_434;
            SD_77 = ent.SD_77;
        }

        public void UpdateTo(SD_84 ent)
        {
            ent.SF_IN_NUM = SF_IN_NUM;
            ent.SF_OUT_NUM = SF_OUT_NUM;
            ent.SF_DATE = SF_DATE;
            ent.SF_CLIENT_DC = SF_CLIENT_DC;
            ent.SF_CLIENT_NAME = SF_CLIENT_NAME;
            ent.SF_CLIENT_RS_CODE = SF_CLIENT_RS_CODE;
            ent.SF_CRS_DC = SF_CRS_DC;
            ent.SF_CRS_SUMMA_K_OPLATE = SF_CRS_SUMMA_K_OPLATE;
            ent.SF_CRS_RATE = SF_CRS_RATE;
            ent.SF_RUB_SUMMA_K_OPLATE = SF_RUB_SUMMA_K_OPLATE;
            ent.SF_PAY_COND_DC = SF_PAY_COND_DC;
            ent.CREATOR = CREATOR;
            ent.SF_ACCEPTED = SF_ACCEPTED;
            ent.SF_GROZOOTPRAVITEL = SF_GROZOOTPRAVITEL;
            ent.SF_GRUZOPOLUCHATEL = SF_GRUZOPOLUCHATEL;
            ent.SF_PAYDOC_TEXT = SF_PAYDOC_TEXT;
            ent.SF_BASE_DC = SF_BASE_DC;
            ent.SF_DOPOLN = SF_DOPOLN;
            ent.SF_SCHET_FLAG = SF_SCHET_FLAG;
            ent.SF_SCHET_FACT_FLAG = SF_SCHET_FACT_FLAG;
            ent.SF_VZAIMOR_TYPE_DC = SF_VZAIMOR_TYPE_DC;
            ent.SF_FACT_SUMMA = SF_FACT_SUMMA;
            ent.SF_PAY_FLAG = SF_PAY_FLAG;
            ent.SF_CENTR_OTV_DC = SF_CENTR_OTV_DC;
            ent.SF_RECEIVER_KONTR_DC = SF_RECEIVER_KONTR_DC;
            ent.SF_RECEIVER_RS_CODE = SF_RECEIVER_RS_CODE;
            ent.SF_TRANZIT = SF_TRANZIT;
            ent.SF_TRANZIT_SPOST_DC = SF_TRANZIT_SPOST_DC;
            ent.SF_TRANZIT_NACEN_PERC = SF_TRANZIT_NACEN_PERC;
            ent.SF_KONTR_CRS_DC = SF_KONTR_CRS_DC;
            ent.SF_KONTR_CRS_SUMMA = SF_KONTR_CRS_SUMMA;
            ent.SF_KONTR_CRS_RATE = SF_KONTR_CRS_RATE;
            ent.SF_UCHET_VALUTA_DC = SF_UCHET_VALUTA_DC;
            ent.SF_UCHET_VALUTA_RATE = SF_UCHET_VALUTA_RATE;
            ent.SF_SUMMA_V_UCHET_VALUTE = SF_SUMMA_V_UCHET_VALUTE;
            ent.SF_NDS_1INCLUD_0NO = SF_NDS_1INCLUD_0NO;
            ent.SF_FORM_RASCH_DC = SF_FORM_RASCH_DC;
            ent.SF_NALOG_NA_PROD_PROC = SF_NALOG_NA_PROD_PROC;
            ent.SF_DILER_DC = SF_DILER_DC;
            ent.SF_DILER_SUMMA = SF_DILER_SUMMA;
            ent.SF_DILER_CRS_DC = SF_DILER_CRS_DC;
            ent.SF_DILER_RATE = SF_DILER_RATE;
            ent.SF_1INCLUD_NAL_S_PROD_0NO = SF_1INCLUD_NAL_S_PROD_0NO;
            ent.SF_DOP_USL_DC = SF_DOP_USL_DC;
            ent.SF_AUTOMAT_CREATE = SF_AUTOMAT_CREATE;
            ent.SF_NOTE = SF_NOTE;
            ent.SF_SOTRUDNIK_TN = SF_SOTRUDNIK_TN;
            ent.TSTAMP = TSTAMP;
            ent.SF_CORRECT_NUM = SF_CORRECT_NUM;
            ent.SF_CORRECT_DATE = SF_CORRECT_DATE;
            ent.SF_CRS_CODE = SF_CRS_CODE;
            ent.CORRECTION_NUM = CORRECTION_NUM;
            ent.CORRECTION_DATE = CORRECTION_DATE;
            ent.COUNTRY_NAME = COUNTRY_NAME;
            ent.COUNTRY_CODE = COUNTRY_CODE;
            ent.KPP = KPP;
            ent.GUID_ID = GUID_ID;
            ent.GRUZO_INFO_ID = GRUZO_INFO_ID;
            ent.REGISTER_DATE = REGISTER_DATE;
            ent.BUH_CLIENT_DC = BUH_CLIENT_DC;
            ent.BUH_RECEIVER_DC = BUH_RECEIVER_DC;
            ent.NAKL_GRUZOOTPRAV = NAKL_GRUZOOTPRAV;
            ent.NAKL_GRUZOPOLUCH = NAKL_GRUZOPOLUCH;
            ent.GROZO_REQUISITE = GROZO_REQUISITE;
            ent.SD_179 = SD_179;
            ent.SD_189 = SD_189;
            ent.SD_301 = SD_301;
            ent.SD_3011 = SD_3011;
            ent.SD_40 = SD_40;
            ent.SD_43 = SD_43;
            ent.SD_431 = SD_431;
            ent.SD_432 = SD_432;
            ent.SD_433 = SD_433;
            ent.SD_434 = SD_434;
            ent.SD_77 = SD_77;
        }

        public SD_84 DefaultValue()
        {
            return new SD_84
            {
                DOC_CODE = -1,
                Id = Guid.NewGuid()
            };
        }

        public SD_84 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_84 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_84 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_84 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }

    public class DataAnnotationsSFClientViewModel : DataAnnotationForFluentApiBase, IMetadataProvider<InvoiceClient>
    {
        void IMetadataProvider<InvoiceClient>.BuildMetadata(MetadataBuilder<InvoiceClient> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Receiver).AutoGenerated().DisplayName("Поставщик");
            builder.Property(_ => _.CO).AutoGenerated().DisplayName("ЦО");
            builder.Property(_ => _.VzaimoraschetType).AutoGenerated().DisplayName("Тип продукции");
            builder.Property(_ => _.FormRaschet).AutoGenerated().DisplayName("Форма расчетов");
            builder.Property(_ => _.PayCondition).AutoGenerated().DisplayName("Условия оплаты");
            builder.Property(_ => _.SF_DATE).AutoGenerated().DisplayName("Дата");
            builder.Property(_ => _.SF_IN_NUM).AutoGenerated().DisplayName("№");
            builder.Property(_ => _.SF_OUT_NUM).AutoGenerated().DisplayName("Внешний №");
            builder.Property(_ => _.Client).AutoGenerated().DisplayName("Клиент");
            builder.Property(_ => _.Currency).AutoGenerated().DisplayName("Валюта");
            builder.Property(_ => _.SummaOtgruz).AutoGenerated().DisplayName("Отгружено").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.SF_DILER_SUMMA).AutoGenerated().DisplayName("Сумма дилера").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.SF_NOTE).AutoGenerated().DisplayName("Примечание");
            builder.Property(_ => _.Diler).AutoGenerated().DisplayName("Дилер");
            builder.Property(_ => _.IsAccepted).AutoGenerated().DisplayName("Акцептован");
            builder.Property(_ => _.SF_CRS_SUMMA_K_OPLATE).AutoGenerated().DisplayName("Сумма").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.CREATOR).AutoGenerated().DisplayName("Создатель");
            builder.Property(_ => _.State).AutoGenerated().DisplayName("Статус");
            builder.Property(_ => _.IsNDSIncludeInPrice).AutoGenerated().DisplayName("НДС вкл. в цену");
            builder.Property(_ => _.PaySumma).AutoGenerated().DisplayName("Оплачено");
            builder.Property(_ => _.PersonaResponsible).AutoGenerated().DisplayName("Ответственный");

            #region Form Layout

            // @formatter:off
            builder.DataFormLayout()
                .Group("Счет", Orientation.Horizontal)
                    .ContainsProperty(_ => _.SF_IN_NUM)
                    .ContainsProperty(_ => _.SF_OUT_NUM)
                    .ContainsProperty(_ => _.SF_DATE)
                    .ContainsProperty(_ => _.CREATOR)
                    .ContainsProperty(_ => _.State)
                .EndGroup()
                .GroupBox("Клиент и характеристики счета")
                    .Group("g1",Orientation.Horizontal)
                        .ContainsProperty(_ => _.Client)
                        .ContainsProperty(_ => _.Diler)
                        .ContainsProperty(_ => _.IsNDSIncludeInPrice)
                        .ContainsProperty(_ => _.IsAccepted)
                    .EndGroup()
                    .ContainsProperty(_ => _.Receiver)
                    .GroupBox("Деньги",Orientation.Horizontal)
                        .ContainsProperty(_ => _.SF_CRS_SUMMA_K_OPLATE)
                        .ContainsProperty(_ => _.Currency)
                        .ContainsProperty(_ => _.PaySumma)
                        .ContainsProperty(_ => _.SummaOtgruz)
                        .ContainsProperty(_ => _.SF_DILER_SUMMA)
                    .EndGroup()
                    .Group("g2",Orientation.Vertical)
                        .Group("g3",Orientation.Horizontal)
                            .ContainsProperty(_ => _.PayCondition)
                            .ContainsProperty(_ => _.VzaimoraschetType)
                        .EndGroup()
                        .Group("g4",Orientation.Horizontal)
                            .ContainsProperty(_ => _.FormRaschet)
                            .ContainsProperty(_ => _.CO)
                        .EndGroup()
                        .ContainsProperty(_ => _.SF_NOTE)
                        .ContainsProperty(_ => _.PersonaResponsible)
                    .EndGroup()
                .EndGroup();
            // @formatter:on

            #endregion
        }
    }
}