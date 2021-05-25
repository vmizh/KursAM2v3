using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Controls;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Data;
using Data.Repository;
using DevExpress.Mvvm.DataAnnotations;

// ReSharper disable All

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel
{
    /// <summary>
    ///     Счет-фактура от поставщика
    /// </summary>
    [MetadataType(typeof(SD_26LayoutData_FluentAPI))]
    public class InvoiceProvider : RSViewModelBase, IEntity<SD_26>, IDataErrorInfo
    {
        private readonly UnitOfWork<ALFAMEDIAEntities> context;

        protected string SFact(decimal dc)
        {
            var doc = context.Context.SD_84.FirstOrDefault(_ => _.DOC_CODE == dc);
            return doc == null ? null : $"№ {doc.SF_IN_NUM}/{doc.SF_OUT_NUM} от {doc.SF_DATE}  {doc.SF_NOTE}";
        }

        public virtual void Save(SD_26 doc)
        {
            throw new NotImplementedException();
        }

        #region Fields

        private CentrOfResponsibility myCO;
        private Employee myEmployee;
        private SD_26 myEntity;
        private FormPay myFormRaschet;
        private Kontragent myKontragent;
        private Kontragent myKontrReceiver;
        private UsagePay myPayCondition;
        private VzaimoraschetType myVzaimoraschetType;
        private ContractProvider myContract;
        private Employee myPersonaResponsible;
        private Money myNakladAll;
        private decimal mySummaFact;

        private bool isLoadAll;

        #endregion

        #region Constructors

        //public InvoiceProvider(SD_26 entity)
        //{
        //    context = new UnitOfWork<ALFAMEDIAEntities>();
        //    Entity = entity ?? DefaultValue();
        //    LoadReferences();
        //    Rows.CollectionChanged += (o, args) => State = RowStatus.NotEdited;
        //}


        public InvoiceProvider()
        {
            Entity = DefaultValue();
        }
        public InvoiceProvider(UnitOfWork<ALFAMEDIAEntities> ctx)
        {
            context = ctx;
            Entity = DefaultValue();
            LoadReferences();
        }

        public InvoiceProvider(SD_26 entity, UnitOfWork<ALFAMEDIAEntities> ctx, bool isLoadAll = false)
        {
            this.isLoadAll = isLoadAll;
            context = ctx;
            Entity = entity ?? DefaultValue();
            LoadReferences();
            Rows.CollectionChanged += (o, args) => State = RowStatus.NotEdited;
        }

        #endregion

        #region Properties

        public ObservableCollection<InvoiceProviderRow> Rows { set; get; } =
            new ObservableCollection<InvoiceProviderRow>();

        public ObservableCollection<WarehouseOrderInRow> Facts { set; get; } =
            new ObservableCollection<WarehouseOrderInRow>();

        public ObservableCollection<ProviderInvoicePayViewModel> PaymentDocs { set; get; } =
            new ObservableCollection<ProviderInvoicePayViewModel>();

        public override string Name =>
            $"С/ф поставщика №{SF_IN_NUM}/{SF_POSTAV_NUM} от {SF_POSTAV_DATE.ToShortDateString()} " +
            $"{SF_NOTES}";

        public override string Description => $"С/ф поставщика №{SF_IN_NUM}/{SF_POSTAV_NUM} от {SF_POSTAV_DATE.ToShortDateString()} " +
                                              $"{Kontragent} на {SF_CRS_SUMMA} {Currency} "  +
                                              $"{SF_NOTES}";

        public decimal DOC_CODE
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

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

        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
        }

        public decimal? NakladDistributedSumma
        {
            get => Entity.NakladDistributedSumma;
            set
            {
                if (Entity.NakladDistributedSumma == value) return;
                Entity.NakladDistributedSumma = value;
                RaisePropertyChanged();
            }
        }

        public bool IsInvoiceNakald
        {
            get => Entity.IsInvoiceNakald ?? false;
            set
            {
                if (Entity.IsInvoiceNakald == value) return;
                Entity.IsInvoiceNakald = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? SF_OPRIHOD_DATE
        {
            get => Entity.SF_OPRIHOD_DATE;
            set
            {
                if (Entity.SF_OPRIHOD_DATE == value) return;
                Entity.SF_OPRIHOD_DATE = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Ответственный
        /// </summary>
        public Employee PersonaResponsible
        {
            get
            {
                if (myPersonaResponsible == null && Employee != null)
                {
                    myPersonaResponsible = Employee;
                    RaisePropertyChanged();
                }
                return myPersonaResponsible;
            }
            set
            {
                if (myPersonaResponsible != null && myPersonaResponsible.Equals(value)) return;
                myPersonaResponsible = value;
                Employee = myPersonaResponsible;
                Entity.PersonalResponsibleDC = myPersonaResponsible?.DocCode;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Employee));
                RaisePropertyChanged(nameof(EmployeeTabelNumber));
            }
        }

        public string SF_POSTAV_NUM
        {
            get => Entity.SF_POSTAV_NUM;
            set
            {
                if (Entity.SF_POSTAV_NUM == value) return;
                Entity.SF_POSTAV_NUM = value;
                RaisePropertyChanged();
            }
        }

        public DateTime SF_POSTAV_DATE
        {
            get => Entity.SF_POSTAV_DATE;
            set
            {
                if (Entity.SF_POSTAV_DATE == value) return;
                Entity.SF_POSTAV_DATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal SF_POST_DC
        {
            get => Entity.SF_POST_DC;
            set
            {
                if (Entity.SF_POST_DC == value) return;
                Entity.SF_POST_DC = value;
                myKontragent = MainReferences.GetKontragent(Entity.SF_POST_DC);
                RaisePropertyChanged(nameof(Kontragent));
                RaisePropertyChanged();
            }
        }
        [Required(ErrorMessage = "Контрагент должен быть выбран обязательно.")] 
        public Kontragent Kontragent
        {
            get => myKontragent;
            set
            {
                if (myKontragent != null && myKontragent.Equals(value)) return;
                myKontragent = value;
                if (myKontragent != null)
                    Entity.SF_POST_DC = myKontragent.DOC_CODE;
                RaisePropertyChanged();
            }
        }

        public decimal SF_RUB_SUMMA
        {
            get => Entity.SF_RUB_SUMMA;
            set
            {
                if (Entity.SF_RUB_SUMMA == value) return;
                Entity.SF_RUB_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        public decimal SF_CRS_SUMMA
        {
            get => Entity.SF_CRS_SUMMA;
            set
            {
                if (Entity.SF_CRS_SUMMA == value) return;
                Entity.SF_CRS_SUMMA = value;
                Entity.SF_RUB_SUMMA = value;
                Entity.SF_FACT_SUMMA = value;
                Entity.SF_KONTR_CRS_SUMMA = value;
                Entity.SF_RUB_SUMMA = value;
                Entity.SF_CRS_RATE = 1;
                Entity.SF_KONTR_CRS_RATE = 1;
                Entity.SF_KONTR_CRS_DC = Kontragent?.BalansCurrency.DOC_CODE;
                Entity.SF_UCHET_VALUTA_RATE = 1;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Отфактурированная сумма
        /// </summary>
        public decimal SummaFact
        {
            get => mySummaFact;
            set
            {
                if (mySummaFact == value) return;
                mySummaFact = value;
                RaisePropertyChanged();
            }
        }

        public Money NakladAll
        {
            get => myNakladAll;
            set
            {
                if (Equals(myNakladAll, value)) return;
                myNakladAll = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_CRS_DC
        {
            get => Entity.SF_CRS_DC;
            set
            {
                if (Entity.SF_CRS_DC == value) return;
                Entity.SF_CRS_DC = value;
                RaisePropertyChanged();
            }
        }

        private Currency myCurrency;

        public Currency Currency
        {
            get => myCurrency;
            set
            {
                if (Equals(myCurrency, value)) return;
                myCurrency = value;
                if (myCurrency != null)
                    Entity.SF_CRS_DC = myCurrency.DOC_CODE;
                RaisePropertyChanged();
            }
        }

        public double? SF_CRS_RATE
        {
            get => Entity.SF_CRS_RATE;
            set
            {
                if (Entity.SF_CRS_RATE == value) return;
                Entity.SF_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }

        public double? V_CRS_ADD_PERCENT
        {
            get => Entity.V_CRS_ADD_PERCENT;
            set
            {
                if (Entity.V_CRS_ADD_PERCENT == value) return;
                Entity.V_CRS_ADD_PERCENT = value;
                RaisePropertyChanged();
            }
        }

        public decimal AddPercent
        {
            get => Convert.ToDecimal(V_CRS_ADD_PERCENT);
            set
            {
                if (Convert.ToDecimal(V_CRS_ADD_PERCENT) == value) return;
                Entity.V_CRS_ADD_PERCENT = (double?) value;
                RaisePropertyChanged();
            }
        }

        public short SF_PAY_FLAG
        {
            get => Entity.SF_PAY_FLAG;
            set
            {
                if (Entity.SF_PAY_FLAG == value) return;
                Entity.SF_PAY_FLAG = value;
                RaisePropertyChanged();
            }
        }

        public bool IsPay
        {
            get => SF_PAY_FLAG == 1;
            set
            {
                if (SF_PAY_FLAG == (value ? 1 : 0)) return;
                SF_PAY_FLAG = (short) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        public decimal PaySumma => (decimal) PaymentDocs?.Sum(_ => _.Summa);

        public decimal SF_FACT_SUMMA
        {
            get => Entity.SF_FACT_SUMMA;
            set
            {
                if (Entity.SF_FACT_SUMMA == value) return;
                Entity.SF_FACT_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        public byte[] SF_OLE
        {
            get => Entity.SF_OLE;
            set
            {
                if (Entity.SF_OLE == value) return;
                Entity.SF_OLE = value;
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
                myPayCondition = MainReferences.PayConditions.ContainsKey(Entity.SF_PAY_COND_DC)
                    ? MainReferences.PayConditions[Entity.SF_PAY_COND_DC]
                    : null;
                RaisePropertyChanged(nameof(PayCondition));
                RaisePropertyChanged();
            }
        }

        public UsagePay PayCondition
        {
            get => myPayCondition;
            set
            {
                if (myPayCondition != null && myPayCondition.Equals(value)) return;
                myPayCondition = value;
                if (myPayCondition != null)
                    Entity.SF_PAY_COND_DC = value.DOC_CODE;
                else
                    Entity.SF_PAY_COND_DC = -1;
                RaisePropertyChanged();
            }
        }

        public int TABELNUMBER
        {
            get => Entity.TABELNUMBER;
            set
            {
                if (Entity.TABELNUMBER == value) return;
                Entity.TABELNUMBER = value;
                myEmployee = MainReferences.Employees.Values.FirstOrDefault(_ => _.TabelNumber == Entity.TABELNUMBER);
                RaisePropertyChanged(nameof(Employee));
                RaisePropertyChanged();
            }
        }

        public int? EmployeeTabelNumber => Employee?.TabelNumber;

        [Required(ErrorMessage = "Ответственный должен быть выбран обязательно.")] 
        public Employee Employee
        {
            get => myEmployee;
            set
            {
                if (myEmployee != null && myEmployee.Equals(value)) return;
                myEmployee = value;
                if (myEmployee != null)
                    Entity.TABELNUMBER = myEmployee.TabelNumber;
                RaisePropertyChanged(nameof(TABELNUMBER));
                RaisePropertyChanged();
            }
        }

        public short SF_EXECUTED
        {
            get => Entity.SF_EXECUTED;
            set
            {
                if (Entity.SF_EXECUTED == value) return;
                Entity.SF_EXECUTED = value;
                RaisePropertyChanged();
            }
        }

        public bool IsExecuted
        {
            get => SF_EXECUTED == 1;
            set
            {
                if (SF_EXECUTED == (value ? 1 : 0)) return;
                SF_EXECUTED = (short) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        public string SF_GRUZOOTPRAVITEL
        {
            get => Entity.SF_GRUZOOTPRAVITEL;
            set
            {
                if (Entity.SF_GRUZOOTPRAVITEL == value) return;
                Entity.SF_GRUZOOTPRAVITEL = value;
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

        public short? SF_NAKLAD_METHOD
        {
            get => Entity.SF_NAKLAD_METHOD;
            set
            {
                if (Entity.SF_NAKLAD_METHOD == value) return;
                Entity.SF_NAKLAD_METHOD = value;
                RaisePropertyChanged();
            }
        }

        public short? SF_ACCEPTED
        {
            get => Entity.SF_ACCEPTED;
            set
            {
                if (Entity.SF_ACCEPTED == value) return;
                Entity.SF_ACCEPTED = value;
                RaisePropertyChanged(nameof(IsAccepted));
                RaisePropertyChanged();
            }
        }

        public bool IsAccepted
        {
            get => SF_ACCEPTED == 1;
            set
            {
                if (SF_ACCEPTED == (value ? 1 : 0)) return;
                SF_ACCEPTED = (short) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        public short? SF_SUMMA_S_NDS
        {
            get => Entity.SF_SUMMA_S_NDS;
            set
            {
                if (Entity.SF_SUMMA_S_NDS == value) return;
                Entity.SF_SUMMA_S_NDS = value;
                RaisePropertyChanged(nameof(IsSummaWithNDS));
                RaisePropertyChanged();
            }
        }

        public bool IsSummaWithNDS
        {
            get => SF_SUMMA_S_NDS == 1;
            set
            {
                if (Entity.SF_SUMMA_S_NDS == (value ? 1 : 0)) return;
                Entity.SF_SUMMA_S_NDS = (short) (value ? 1 : 0);
                RaisePropertyChanged(nameof(SF_SUMMA_S_NDS));
                RaisePropertyChanged();
            }
        }

        public DateTime? SF_REGISTR_DATE
        {
            get => Entity.SF_REGISTR_DATE;
            set
            {
                if (Entity.SF_REGISTR_DATE == value) return;
                Entity.SF_REGISTR_DATE = value;
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

        public string SF_NOTES
        {
            get => Entity.SF_NOTES;
            set
            {
                if (Entity.SF_NOTES == value) return;
                Entity.SF_NOTES = value;
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

        public decimal? SF_VZAIMOR_TYPE_DC
        {
            get => Entity.SF_VZAIMOR_TYPE_DC;
            set
            {
                if (Entity.SF_VZAIMOR_TYPE_DC == value) return;
                Entity.SF_VZAIMOR_TYPE_DC = value;
                if (Entity.SF_VZAIMOR_TYPE_DC != null &&
                    MainReferences.VzaimoraschetTypes.ContainsKey((decimal) Entity.SF_VZAIMOR_TYPE_DC))
                    myVzaimoraschetType = MainReferences.VzaimoraschetTypes[(decimal) Entity.SF_VZAIMOR_TYPE_DC];
                else
                    myVzaimoraschetType = null;
                RaisePropertyChanged(nameof(VzaimoraschetType));
                RaisePropertyChanged();
            }
        }

        public VzaimoraschetType VzaimoraschetType
        {
            get => myVzaimoraschetType;
            set
            {
                if (myVzaimoraschetType == value) return;
                myVzaimoraschetType = value;
                Entity.SF_VZAIMOR_TYPE_DC = value?.DOC_CODE;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_DOGOVOR_POKUPKI_DC
        {
            get => Entity.SF_DOGOVOR_POKUPKI_DC;
            set
            {
                if (Entity.SF_DOGOVOR_POKUPKI_DC == value) return;
                Entity.SF_DOGOVOR_POKUPKI_DC = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Договор закупки
        /// </summary>
        public ContractProvider Contract
        {
            get => myContract;
            set
            {
                if (myContract != null && myContract.Equals(value)) return;
                myContract = value;
                SF_DOGOVOR_POKUPKI_DC = myContract?.DocCode;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_PREDOPL_DOC_DC
        {
            get => Entity.SF_PREDOPL_DOC_DC;
            set
            {
                if (Entity.SF_PREDOPL_DOC_DC == value) return;
                Entity.SF_PREDOPL_DOC_DC = value;
                RaisePropertyChanged();
            }
        }

        public int? SF_IN_NUM
        {
            get => Entity.SF_IN_NUM;
            set
            {
                if (Entity.SF_IN_NUM == value) return;
                Entity.SF_IN_NUM = value;
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
                if (Entity.SF_FORM_RASCH_DC != null &&
                    MainReferences.FormRaschets.ContainsKey(Entity.SF_FORM_RASCH_DC.Value))
                    myFormRaschet = MainReferences.FormRaschets[Entity.SF_FORM_RASCH_DC.Value];
                else
                    myFormRaschet = null;
                RaisePropertyChanged(nameof(FormRaschet));
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

        public DateTime? SF_VIPOL_RABOT_DATE
        {
            get => Entity.SF_VIPOL_RABOT_DATE;
            set
            {
                if (Entity.SF_VIPOL_RABOT_DATE == value) return;
                Entity.SF_VIPOL_RABOT_DATE = value;
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

        public short? SF_NDS_VKL_V_CENU
        {
            get => Entity.SF_NDS_VKL_V_CENU;
            set
            {
                if (Entity.SF_NDS_VKL_V_CENU == value) return;
                Entity.SF_NDS_VKL_V_CENU = value;
                RaisePropertyChanged();
            }
        }

        public bool IsNDSInPrice
        {
            get => Entity.SF_NDS_VKL_V_CENU == 1;
            set
            {
                if (Entity.SF_NDS_VKL_V_CENU == (value ? 1 : 0)) return;
                Entity.SF_NDS_VKL_V_CENU = (short?) (value ? 1 : 0);
                RaisePropertyChanged();
                foreach (var r in Rows)
                {
                    r.IsIncludeInPrice = (Entity.SF_NDS_VKL_V_CENU ?? 0) == 0 ? false : true;
                }
            }
        }

        public decimal? SF_SFACT_DC
        {
            get => Entity.SF_SFACT_DC;
            set
            {
                if (Entity.SF_SFACT_DC == value) return;
                Entity.SF_SFACT_DC = value;
                RaisePropertyChanged();
            }
        }

        public string SFactName => SF_SFACT_DC != null ? SFact(SF_SFACT_DC.Value) : null;

        public decimal? SF_CENTR_OTV_DC
        {
            get => Entity.SF_CENTR_OTV_DC;
            set
            {
                if (Entity.SF_CENTR_OTV_DC == value) return;
                Entity.SF_CENTR_OTV_DC = value;
                myCO = MainReferences.COList.ContainsKey(Entity.SF_CENTR_OTV_DC.Value)
                    ? MainReferences.COList[Entity.SF_CENTR_OTV_DC.Value]
                    : null;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CO));
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

        public decimal? SF_POLUCH_KONTR_DC
        {
            get => Entity.SF_POLUCH_KONTR_DC;
            set
            {
                if (Entity.SF_POLUCH_KONTR_DC == value) return;
                Entity.SF_POLUCH_KONTR_DC = value;
                KontrReceiver = MainReferences.GetKontragent(Entity.SF_POLUCH_KONTR_DC);
                RaisePropertyChanged(nameof(KontrReceiver));
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Контрагент - получатель
        /// </summary>
        public Kontragent KontrReceiver
        {
            get => myKontrReceiver;
            set
            {
                if (myKontrReceiver != null && myKontrReceiver.Equals(value)) return;
                myKontrReceiver = value;
                Entity.SF_POLUCH_KONTR_DC = myKontrReceiver?.DocCode;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_PEREVOZCHIK_DC
        {
            get => Entity.SF_PEREVOZCHIK_DC;
            set
            {
                if (Entity.SF_PEREVOZCHIK_DC == value) return;
                Entity.SF_PEREVOZCHIK_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SF_PEREVOZCHIK_SUM
        {
            get => Entity.SF_PEREVOZCHIK_SUM;
            set
            {
                if (Entity.SF_PEREVOZCHIK_SUM == value) return;
                Entity.SF_PEREVOZCHIK_SUM = value;
                RaisePropertyChanged();
            }
        }

        public short? SF_AUTO_CREATE
        {
            get => Entity.SF_AUTO_CREATE;
            set
            {
                if (Entity.SF_AUTO_CREATE == value) return;
                Entity.SF_AUTO_CREATE = value;
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

        public SD_112 SD_112
        {
            get => Entity.SD_112;
            set
            {
                if (Entity.SD_112 == value) return;
                Entity.SD_112 = value;
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

        public SD_301 SD_3012
        {
            get => Entity.SD_3012;
            set
            {
                if (Entity.SD_3012 == value) return;
                Entity.SD_3012 = value;
                RaisePropertyChanged();
            }
        }

        public SD_37 SD_37
        {
            get => Entity.SD_37;
            set
            {
                if (Entity.SD_37 == value) return;
                Entity.SD_37 = value;
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

        public SD_26 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public string InvoiceInfo => $"{Kontragent} {Currency} {Note}";
        public EntityLoadCodition LoadCondition { get; set; }
        public bool IsAccessRight { get; set; }
        public ObservableCollection<Overhead> Overheads { set; get; }

        public ObservableCollection<InvoiceProviderRow> DeletedRows { get; set; } =
            new ObservableCollection<InvoiceProviderRow>();

        public ObservableCollection<InvoiceProviderRowCurrencyConvertViewModel> DeletedCurrencyRows { get; set; } =
            new ObservableCollection<InvoiceProviderRowCurrencyConvertViewModel>();


        #endregion Properties

        #region Methods

        private void LoadReferences()
        {
            if (SF_POST_DC > 0)
                Kontragent = MainReferences.GetKontragent(SF_POST_DC);
            if (SF_POLUCH_KONTR_DC != null)
                KontrReceiver = MainReferences.GetKontragent(SF_POLUCH_KONTR_DC);
            if (SF_CRS_DC != null)
                Currency = MainReferences.Currencies.ContainsKey(SF_CRS_DC.Value)
                    ? MainReferences.Currencies[SF_CRS_DC.Value]
                    : null;
            if (SF_CENTR_OTV_DC != null)
                CO = MainReferences.COList.ContainsKey(SF_CENTR_OTV_DC.Value)
                    ? MainReferences.COList[SF_CENTR_OTV_DC.Value]
                    : null;
            if (MainReferences.PayConditions.ContainsKey(SF_PAY_COND_DC))
                PayCondition = MainReferences.PayConditions[SF_PAY_COND_DC];
            Employee = MainReferences.Employees.Values.FirstOrDefault(_ => _.TabelNumber == TABELNUMBER);
            if (SF_VZAIMOR_TYPE_DC != null)
                VzaimoraschetType = MainReferences.VzaimoraschetTypes.ContainsKey(SF_VZAIMOR_TYPE_DC.Value)
                    ? MainReferences.VzaimoraschetTypes[SF_VZAIMOR_TYPE_DC.Value]
                    : null;
            if (SF_FORM_RASCH_DC != null)
                FormRaschet = MainReferences.FormRaschets.ContainsKey(SF_FORM_RASCH_DC.Value)
                    ? MainReferences.FormRaschets[SF_FORM_RASCH_DC.Value]
                    : null;
            if (SD_112 != null)
                Contract = new ContractProvider(SD_112);
            if (Entity.PersonalResponsibleDC != null)
                PersonaResponsible = MainReferences.Employees[Entity.PersonalResponsibleDC.Value];
            Rows = new ObservableCollection<InvoiceProviderRow>();
            if (Entity.TD_26 != null && Entity.TD_26.Count > 0)
            {
                foreach (var t in Entity.TD_26)
                {
                    var newRow = new InvoiceProviderRow(t)
                    {
                        Parent = this
                    };
                    Rows.Add(newRow);
                    if (t.TD_24 != null)
                        SummaFact += (decimal) t.SFT_SUMMA_K_OPLATE / t.SFT_KOL *
                                     t.TD_24.Sum(_ => _.DDT_KOL_PRIHOD);
                }
            }

            PaymentDocs = new ObservableCollection<ProviderInvoicePayViewModel>();
            if (Entity.ProviderInvoicePay != null && Entity.ProviderInvoicePay.Count > 0)
            {
                foreach (var pay in Entity.ProviderInvoicePay)
                {
                    var newItem = new ProviderInvoicePayViewModel(pay);
                    if (pay.TD_101 != null)
                    {
                        newItem.DocSumma = (decimal) pay.TD_101.VVT_VAL_RASHOD;
                        newItem.DocDate = pay.TD_101.SD_101.VV_START_DATE;
                        newItem.DocName = "Банковский платеж";
                        newItem.DocExtName = $"{pay.TD_101.SD_101.SD_114.BA_BANK_NAME} " +
                                             $"р/с {pay.TD_101.SD_101.SD_114.BA_RASH_ACC}";
                    }

                    if (pay.SD_34 != null)
                    {
                        newItem.DocSumma = (decimal) pay.SD_34.SUMM_ORD;
                        newItem.DocName = "Расходный кассовый ордер";
                        newItem.DocNum = pay.SD_34.NUM_ORD.ToString();
                        newItem.DocDate = (DateTime) pay.SD_34.DATE_ORD;
                        if (pay.SD_34.SD_22 != null)
                        {
                            newItem.DocExtName = $"Касса {pay.SD_34.SD_22.CA_NAME}";
                        }
                        else
                        {
                            newItem.DocExtName = $"Касса {MainReferences.CashsAll[(decimal) pay.SD_34.CA_DC].Name}";
                        }
                    }

                    if (pay.TD_110 != null)
                    {
                        newItem.DocSumma = (decimal) pay.TD_110.VZT_CRS_SUMMA;
                        newItem.DocName = "Акт взаимозачета";
                        newItem.DocNum = pay.TD_110.SD_110.VZ_NUM.ToString();
                        newItem.DocDate = pay.TD_110.SD_110.VZ_DATE;
                        newItem.DocExtName = $"{pay.TD_110.VZT_DOC_NOTES}";
                    }

                    PaymentDocs.Add(newItem);
                }
            }

            SummaFact = 0;
            if (Entity.SD_24 != null)
                SummaFact += (from q in Entity.TD_26
                    from d in q.TD_24
                    select d.DDT_KOL_PRIHOD * q.SFT_ED_CENA ?? 0).Sum();

            Facts = new ObservableCollection<WarehouseOrderInRow>();
            if (Entity.TD_26 != null && Entity.TD_26.Count > 0)
            {
                foreach (var r in Entity.TD_26)
                {
                    if (r.TD_24 != null && r.TD_24.Count > 0)
                    {
                        foreach (var r2 in r.TD_24)
                        {
                            var newItem = new WarehouseOrderInRow(r2, isLoadAll);
                            if (r2.SD_24 != null)
                            {
                                newItem.Parent = new WarehouseOrderIn(r2.SD_24);
                            }

                            Facts.Add(newItem);
                        }
                    }
                }
            }
        }

        public List<SD_26> LoadList()
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(SD_26 ent)
        {
            SF_OPRIHOD_DATE = ent.SF_OPRIHOD_DATE;
            SF_POSTAV_NUM = ent.SF_POSTAV_NUM;
            SF_POSTAV_DATE = ent.SF_POSTAV_DATE;
            SF_POST_DC = ent.SF_POST_DC;
            SF_RUB_SUMMA = ent.SF_RUB_SUMMA;
            //SF_CRS_SUMMA = ent.SF_CRS_SUMMA;
            SF_CRS_DC = ent.SF_CRS_DC;
            SF_CRS_RATE = ent.SF_CRS_RATE;
            V_CRS_ADD_PERCENT = ent.V_CRS_ADD_PERCENT;
            SF_PAY_FLAG = ent.SF_PAY_FLAG;
            SF_FACT_SUMMA = ent.SF_FACT_SUMMA;
            SF_OLE = ent.SF_OLE;
            SF_PAY_COND_DC = ent.SF_PAY_COND_DC;
            TABELNUMBER = ent.TABELNUMBER;
            SF_EXECUTED = ent.SF_EXECUTED;
            SF_GRUZOOTPRAVITEL = ent.SF_GRUZOOTPRAVITEL;
            SF_GRUZOPOLUCHATEL = ent.SF_GRUZOPOLUCHATEL;
            SF_NAKLAD_METHOD = ent.SF_NAKLAD_METHOD;
            SF_ACCEPTED = ent.SF_ACCEPTED;
            SF_SUMMA_S_NDS = ent.SF_SUMMA_S_NDS;
            SF_REGISTR_DATE = ent.SF_REGISTR_DATE;
            SF_SCHET_FLAG = ent.SF_SCHET_FLAG;
            SF_SCHET_FACT_FLAG = ent.SF_SCHET_FACT_FLAG;
            SF_NOTES = ent.SF_NOTES;
            CREATOR = ent.CREATOR;
            SF_VZAIMOR_TYPE_DC = ent.SF_VZAIMOR_TYPE_DC;
            SF_DOGOVOR_POKUPKI_DC = ent.SF_DOGOVOR_POKUPKI_DC;
            SF_PREDOPL_DOC_DC = ent.SF_PREDOPL_DOC_DC;
            SF_IN_NUM = ent.SF_IN_NUM;
            SF_FORM_RASCH_DC = ent.SF_FORM_RASCH_DC;
            SF_VIPOL_RABOT_DATE = ent.SF_VIPOL_RABOT_DATE;
            SF_TRANZIT = ent.SF_TRANZIT;
            SF_KONTR_CRS_DC = ent.SF_KONTR_CRS_DC;
            SF_KONTR_CRS_RATE = ent.SF_KONTR_CRS_RATE;
            //SF_KONTR_CRS_SUMMA = ent.SF_KONTR_CRS_SUMMA;
            SF_UCHET_VALUTA_DC = ent.SF_UCHET_VALUTA_DC;
            SF_UCHET_VALUTA_RATE = ent.SF_UCHET_VALUTA_RATE;
            SF_SUMMA_V_UCHET_VALUTE = ent.SF_SUMMA_V_UCHET_VALUTE;
            SF_NDS_VKL_V_CENU = ent.SF_NDS_VKL_V_CENU;
            SF_SFACT_DC = ent.SF_SFACT_DC;
            SF_CENTR_OTV_DC = ent.SF_CENTR_OTV_DC;
            SF_POLUCH_KONTR_DC = ent.SF_POLUCH_KONTR_DC;
            SF_PEREVOZCHIK_DC = ent.SF_PEREVOZCHIK_DC;
            SF_PEREVOZCHIK_SUM = ent.SF_PEREVOZCHIK_SUM;
            SF_AUTO_CREATE = ent.SF_AUTO_CREATE;
            TSTAMP = ent.TSTAMP;
            Id = ent.Id;
            SD_112 = ent.SD_112;
            SD_179 = ent.SD_179;
            SD_189 = ent.SD_189;
            SD_301 = ent.SD_301;
            SD_3011 = ent.SD_3011;
            SD_3012 = ent.SD_3012;
            SD_37 = ent.SD_37;
            SD_43 = ent.SD_43;
            SD_77 = ent.SD_77;
            SD_40 = ent.SD_40;
            SD_431 = ent.SD_431;
            SD_432 = ent.SD_432;
        }

        public void UpdateTo(SD_26 ent)
        {
            ent.SF_OPRIHOD_DATE = SF_OPRIHOD_DATE;
            ent.SF_POSTAV_NUM = SF_POSTAV_NUM;
            ent.SF_POSTAV_DATE = SF_POSTAV_DATE;
            ent.SF_POST_DC = SF_POST_DC;
            ent.SF_RUB_SUMMA = SF_RUB_SUMMA;
            ent.SF_CRS_SUMMA = SF_CRS_SUMMA;
            ent.SF_CRS_DC = SF_CRS_DC;
            ent.SF_CRS_RATE = SF_CRS_RATE;
            ent.V_CRS_ADD_PERCENT = V_CRS_ADD_PERCENT;
            ent.SF_PAY_FLAG = SF_PAY_FLAG;
            ent.SF_FACT_SUMMA = SF_FACT_SUMMA;
            ent.SF_OLE = SF_OLE;
            ent.SF_PAY_COND_DC = SF_PAY_COND_DC;
            ent.TABELNUMBER = TABELNUMBER;
            ent.SF_EXECUTED = SF_EXECUTED;
            ent.SF_GRUZOOTPRAVITEL = SF_GRUZOOTPRAVITEL;
            ent.SF_GRUZOPOLUCHATEL = SF_GRUZOPOLUCHATEL;
            ent.SF_NAKLAD_METHOD = SF_NAKLAD_METHOD;
            ent.SF_ACCEPTED = SF_ACCEPTED;
            ent.SF_SUMMA_S_NDS = SF_SUMMA_S_NDS;
            ent.SF_REGISTR_DATE = SF_REGISTR_DATE;
            ent.SF_SCHET_FLAG = SF_SCHET_FLAG;
            ent.SF_SCHET_FACT_FLAG = SF_SCHET_FACT_FLAG;
            ent.SF_NOTES = SF_NOTES;
            ent.CREATOR = CREATOR;
            ent.SF_VZAIMOR_TYPE_DC = SF_VZAIMOR_TYPE_DC;
            ent.SF_DOGOVOR_POKUPKI_DC = SF_DOGOVOR_POKUPKI_DC;
            ent.SF_PREDOPL_DOC_DC = SF_PREDOPL_DOC_DC;
            ent.SF_IN_NUM = SF_IN_NUM;
            ent.SF_FORM_RASCH_DC = SF_FORM_RASCH_DC;
            ent.SF_VIPOL_RABOT_DATE = SF_VIPOL_RABOT_DATE;
            ent.SF_TRANZIT = SF_TRANZIT;
            ent.SF_KONTR_CRS_DC = SF_KONTR_CRS_DC;
            ent.SF_KONTR_CRS_RATE = SF_KONTR_CRS_RATE;
            ent.SF_KONTR_CRS_SUMMA = SF_KONTR_CRS_SUMMA;
            ent.SF_UCHET_VALUTA_DC = SF_UCHET_VALUTA_DC;
            ent.SF_UCHET_VALUTA_RATE = SF_UCHET_VALUTA_RATE;
            ent.SF_SUMMA_V_UCHET_VALUTE = SF_SUMMA_V_UCHET_VALUTE;
            ent.SF_NDS_VKL_V_CENU = SF_NDS_VKL_V_CENU;
            ent.SF_SFACT_DC = SF_SFACT_DC;
            ent.SF_CENTR_OTV_DC = SF_CENTR_OTV_DC;
            ent.SF_POLUCH_KONTR_DC = SF_POLUCH_KONTR_DC;
            ent.SF_PEREVOZCHIK_DC = SF_PEREVOZCHIK_DC;
            ent.SF_PEREVOZCHIK_SUM = SF_PEREVOZCHIK_SUM;
            ent.SF_AUTO_CREATE = SF_AUTO_CREATE;
            ent.TSTAMP = TSTAMP;
            ent.Id = Id;
            ent.SD_112 = SD_112;
            ent.SD_179 = SD_179;
            ent.SD_189 = SD_189;
            ent.SD_301 = SD_301;
            ent.SD_3011 = SD_3011;
            ent.SD_3012 = SD_3012;
            ent.SD_37 = SD_37;
            ent.SD_43 = SD_43;
            ent.SD_77 = SD_77;
            ent.SD_40 = SD_40;
            ent.SD_431 = SD_431;
            ent.SD_432 = SD_432;
        }

        public SD_26 DefaultValue()
        {
            return new SD_26
            {
                Id = Guid.NewGuid(),
                DOC_CODE = -1
            };
        }

        public SD_26 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_26 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_26 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_26 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return
                $"С/ф (поставщика) №{SF_POSTAV_NUM}/{SF_IN_NUM} от {SF_POSTAV_DATE.ToShortDateString()} " +
                $"от: {Kontragent} сумма: {SF_CRS_SUMMA} {Currency}";
        }

        #endregion Methods

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(PersonaResponsible):
                        return PersonaResponsible == null ? Helper.ValidationError.FieldNotNull : null;
                    case nameof(Kontragent):
                        return Kontragent == null ? Helper.ValidationError.FieldNotNull : null;
                    case nameof(Currency):
                        return Currency == null ? Helper.ValidationError.FieldNotNull : null;
                    case nameof(CO):
                        return CO == null ? Helper.ValidationError.FieldNotNull : null;
                    case nameof(PayCondition):
                        return PayCondition == null ? Helper.ValidationError.FieldNotNull : null;
                    case nameof(VzaimoraschetType):
                        return VzaimoraschetType == null ? Helper.ValidationError.FieldNotNull : null;
                    case nameof(FormRaschet):
                        return FormRaschet == null ? Helper.ValidationError.FieldNotNull : null;
                }
                return null;
            }
        }

        public string Error
        {
            get { return null; }
        }
    }

    /// <summary>
    ///     Клас описывающий накладные расходы
    /// </summary>
    public class Overhead
    {
        public Kontragent Kontragent { set; get; }
        public Money Summa { set; get; }
    }

    public class SD_26LayoutData_FluentAPI : DataAnnotationForFluentApiBase, IMetadataProvider<InvoiceProvider>
    {
        private readonly string notNullMessage = "Поле должно быть заполнено";
        void IMetadataProvider<InvoiceProvider>.BuildMetadata(
            MetadataBuilder<InvoiceProvider> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(x => x.KontrReceiver).AutoGenerated()
                .DisplayName("Получатель");
            builder.Property(x => x.Currency).AutoGenerated()
                .DisplayName("Валюта").Required(()=>notNullMessage);
            builder.Property(x => x.SF_CRS_SUMMA).AutoGenerated()
                .DisplayName("Сумма").ReadOnly().DisplayFormatString("n2");
            builder.Property(x => x.SF_IN_NUM).AutoGenerated()
                .DisplayName("№");
            builder.Property(x => x.SF_POSTAV_DATE).AutoGenerated()
                .Required(()=>notNullMessage)
                .DisplayName("Дата");
            builder.Property(x => x.SF_POSTAV_NUM).AutoGenerated()
                .DisplayName("Внешний №");
            builder.Property(x => x.IsAccepted).AutoGenerated()
                .DisplayName("Акцептован");
            builder.Property(x => x.IsNDSInPrice).AutoGenerated()
                .DisplayName("НДС в цене");
            builder.Property(x => x.Kontragent).AutoGenerated()
                .Required(()=>notNullMessage)
                .DisplayName("Контрагент");
            builder.Property(x => x.CO).AutoGenerated()
                .Required(()=>notNullMessage)
                .DisplayName("Центр ответственности");
            builder.Property(x => x.VzaimoraschetType).AutoGenerated()
                .Required(()=>notNullMessage)
                .DisplayName("Тип взаиморасчета");
            builder.Property(x => x.PayCondition).AutoGenerated()
                .Required(()=>notNullMessage)
                .DisplayName("Условие оплаты");
            builder.Property(x => x.FormRaschet).AutoGenerated()
                .Required(()=>notNullMessage)
                .DisplayName("Форма расчета");
            //builder.Property(x => x.SF_REGISTR_DATE).NotAutoGenerated()
            //    .DisplayName("Дата регистр.");
            builder.Property(x => x.SF_GRUZOOTPRAVITEL).AutoGenerated()
                .DisplayName("Грузоотправитель");
            builder.Property(x => x.SF_GRUZOPOLUCHATEL).AutoGenerated()
                .DisplayName("Грузополучатель");
            builder.Property(x => x.VzaimoraschetType).AutoGenerated()
                .Required(()=>notNullMessage)
                .DisplayName("Тип продукции");
            builder.Property(x => x.SummaFact).ReadOnly().AutoGenerated()
                .DisplayName("Фактурировано");
            builder.Property(x => x.SF_KONTR_CRS_SUMMA).AutoGenerated()
                .DisplayName("Сумма контрагента").ReadOnly().DisplayFormatString("n2");
            builder.Property(x => x.NakladAll).NotAutoGenerated()
                .DisplayName("Всего").ReadOnly().DisplayFormatString("n2");
            builder.Property(x => x.Overheads).NotAutoGenerated()
                .DisplayName("По контрагентам").NullDisplayText("Накладные отсутствуют ");
            builder.Property(x => x.Contract).AutoGenerated()
                .DisplayName("Договор поставки");
            builder.Property(x => x.TABELNUMBER).AutoGenerated()
                .DisplayName("Таб.№");
            builder.Property(x => x.PersonaResponsible).NotAutoGenerated()
                .DisplayName("Имя");
            builder.Property(x => x.State).AutoGenerated()
                .DisplayName("Статус");
            builder.Property(x => x.CREATOR).AutoGenerated()
                .DisplayName("Создатель");
            builder.Property(x => x.SF_NOTES).AutoGenerated().MultilineTextDataType()
                .DisplayName("Примечание");
            builder.Property(x => x.PaySumma).AutoGenerated()
                .DisplayName("Оплачено");
            builder.Property(x => x.PersonaResponsible).AutoGenerated()
                .Required(()=>notNullMessage)
                .DisplayName("Ответственный");
            builder.Property(x => x.IsInvoiceNakald).AutoGenerated()
                .DisplayName("Счет накладных");
            builder.Property(x => x.NakladDistributedSumma).AutoGenerated()
                .DisplayName("Распределено (накл)").ReadOnly().DisplayFormatString("n2");

            #region Form Layout

            // @formatter:off
            builder.DataFormLayout()
                .Group("first",Orientation.Vertical)
                    .Group("Счет", Orientation.Horizontal)
                        .ContainsProperty(_ => _.SF_IN_NUM)
                        .ContainsProperty(_ => _.SF_POSTAV_DATE)
                        .ContainsProperty(_ => _.SF_POSTAV_NUM)
                        .ContainsProperty(_ => _.SF_REGISTR_DATE)
                        .ContainsProperty(_ => _.CREATOR)
                        .ContainsProperty(_ => _.State)
                    .EndGroup()
                    .Group("Поставщик и характеристики счета", Orientation.Vertical)
                        .Group("a1",Orientation.Horizontal)
                            .Group("a11",Orientation.Vertical)
                                .ContainsProperty(_ => _.Kontragent)
                                .ContainsProperty(_ => _.KontrReceiver)
                                .EndGroup()
                            .Group("a12", Orientation.Vertical)
                                .Group("a121", Orientation.Horizontal)
                                    .ContainsProperty(_ => _.VzaimoraschetType)
                                    .ContainsProperty(_ => _.IsNDSInPrice)
                                    .ContainsProperty(_ => _.IsAccepted)
                                .EndGroup()
                                .Group("a122", Orientation.Horizontal) 
                                    .ContainsProperty(_ => _.CO)
                                    .ContainsProperty(_ => _.Contract)
                                .EndGroup()
                            .EndGroup()
                        .EndGroup()
                        .GroupBox("Деньги", Orientation.Horizontal)
                            .Group("444", Orientation.Vertical)
                                .Group("444-3", Orientation.Horizontal)
                                    .ContainsProperty(_ => _.SF_CRS_SUMMA)
                                    .ContainsProperty(_ => _.Currency)
                                    .ContainsProperty(_ => _.IsInvoiceNakald)
                                    .ContainsProperty(_ => _.NakladDistributedSumma)
                                .EndGroup()
                                .Group("444-2", Orientation.Horizontal)
                                    .ContainsProperty(_ => _.PaySumma)
                                    .ContainsProperty(_ => _.SummaFact)
                                    .ContainsProperty(_ => _.SF_KONTR_CRS_SUMMA)
                                .EndGroup()
                            .EndGroup()
                            .Group("Условия", Orientation.Vertical)
                                .ContainsProperty(_ => _.PayCondition)
                                .ContainsProperty(_ => _.FormRaschet)
                            .EndGroup()
                        .EndGroup()
                        .GroupBox("Характеристики", Orientation.Horizontal)
                            .Group("000", Orientation.Horizontal)
                                .GroupBox("Ответственный", Orientation.Vertical)
                                    .ContainsProperty(_ => _.TABELNUMBER)
                                    //.ContainsProperty(_ => _.Employee)
                                    .ContainsProperty(_ => _.PersonaResponsible)
                                .EndGroup()
                                .GroupBox("Грузовые реквизиты", Orientation.Vertical)
                                    .ContainsProperty(_ => _.SF_GRUZOOTPRAVITEL)
                                    .ContainsProperty(_ => _.SF_GRUZOPOLUCHATEL)
                                .EndGroup()
                            .EndGroup()
                            .Group("Накладные расходы", Orientation.Vertical)
                                //    .ContainsProperty(_ => _.NakladAll)
                                //    .ContainsProperty(_ => _.Overheads)
                                .ContainsProperty(_ => _.SF_NOTES)
                            .EndGroup()
                        .EndGroup()
                    .EndGroup()
                .EndGroup();
            // @formatter:on

            #endregion
        }
    }

    [MetadataType(typeof(DataAnnotationsInvoiceProviderShort))]
    public class InvoiceProviderShort : InvoiceProvider
    {
        public InvoiceProviderShort(UnitOfWork<ALFAMEDIAEntities> ctx) : base(ctx)
        {
        }

        public InvoiceProviderShort(SD_26 entity, UnitOfWork<ALFAMEDIAEntities> ctx) : base(entity, ctx)
        {
        }
    }

    public class DataAnnotationsInvoiceProviderShort : DataAnnotationForFluentApiBase,
        IMetadataProvider<InvoiceProviderShort>
    {
        void IMetadataProvider<InvoiceProviderShort>.BuildMetadata(
            MetadataBuilder<InvoiceProviderShort> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(x => x.Currency).AutoGenerated()
                .DisplayName("Валюта");
            builder.Property(x => x.SF_CRS_SUMMA).AutoGenerated()
                .DisplayName("Сумма").ReadOnly().DisplayFormatString("n2");
            builder.Property(x => x.SF_IN_NUM).AutoGenerated()
                .DisplayName("№");
            builder.Property(x => x.SF_POSTAV_DATE).AutoGenerated()
                .DisplayName("Дата");
            builder.Property(x => x.SF_POSTAV_NUM).AutoGenerated()
                .DisplayName("Внешний №");
            builder.Property(x => x.IsAccepted).AutoGenerated()
                .DisplayName("Акцептован");
            builder.Property(x => x.Kontragent).AutoGenerated()
                .DisplayName("Контрагент");
            builder.Property(x => x.CO).AutoGenerated()
                .DisplayName("Центр ответственности");
            builder.Property(x => x.CREATOR).AutoGenerated()
                .DisplayName("Создатель");
            builder.Property(x => x.SF_NOTES).AutoGenerated()
                .DisplayName("Примечание");
            builder.Property(x => x.PaySumma).AutoGenerated()
                .DisplayName("Оплачено");
            builder.Property(x => x.PersonaResponsible).AutoGenerated()
                .DisplayName("Ответственный");
        }
    }
}