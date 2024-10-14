using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using Helper;
using KursDomain.Documents.Dogovora;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.IDocuments.Finance;
using KursDomain.References;
using KursDomain.Repository;
using Newtonsoft.Json;
using NomenklProductType = KursDomain.References.NomenklProductType;
using Orientation = System.Windows.Controls.Orientation;
using ValidationError = Core.Helper.ValidationError;

namespace KursDomain.Documents.Invoices;

/// <summary>
///     Счет-фактура от поставщика
/// </summary>
[MetadataType(typeof(SD_26LayoutData_FluentAPI))]
[DebuggerDisplay("{DocCode} {Kontragent} {Summa}")]
public class InvoiceProvider : RSViewModelBase, IEntity<SD_26>, IDataErrorInfo, IInvoiceProvider
{
    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case nameof(PersonaResponsible):
                    return PersonaResponsible == null ? ValidationError.FieldNotNull : null;
                case nameof(Kontragent):
                    return Kontragent == null ? ValidationError.FieldNotNull : null;
                case nameof(Currency):
                    return Currency == null ? ValidationError.FieldNotNull : null;
                case nameof(CO):
                    return CO == null || CO.DocCode == 0 ? ValidationError.FieldNotNull : null;
                case nameof(PayCondition):
                    return PayCondition == null ? ValidationError.FieldNotNull : null;
                case nameof(VzaimoraschetType):
                    return VzaimoraschetType == null ? ValidationError.FieldNotNull : null;
                case nameof(FormRaschet):
                    return FormRaschet == null ? ValidationError.FieldNotNull : null;
            }

            return null;
        }
    }

    public string Error => null;

    protected string SFact(decimal dc)
    {
        SD_84 doc = null;
        if (context != null)
            doc = context.Context.SD_84.FirstOrDefault(_ => _.DOC_CODE == dc);
        return doc == null ? null : $"№ {doc.SF_IN_NUM}/{doc.SF_OUT_NUM} от {doc.SF_DATE}  {doc.SF_NOTE}";
    }

    public virtual void Save(SD_26 doc)
    {
        throw new NotImplementedException();
    }

    public override object ToJson()
    {
        var res = new
        {
            Статус = CustomFormat.GetEnumName(State),
            DocCode,
            Номер = SF_IN_NUM + "/" + SF_POSTAV_NUM,
            Дата = DocDate,
            Контрагент = Kontragent.Name,
            Сумма = Summa.ToString("n2"),
            Валюта = Currency.Name,
            Отгружено = SummaFact.ToString("n2"),
            Договор = Contract?.ToString(),
            Ответственный = PersonaResponsible?.Name,
            Центр_ответственности = CO?.Name,
            Тип_взаиморасчетов = VzaimoraschetType?.Name,
            Условия_оплаты = PayCondition?.Name,
            Формы_расчетов = FormRaschet?.Name,
            Примечание = Note,
            Позиции = Rows.Cast<InvoiceProviderRow>().Select(_ => _.ToJson())
        };

        return JsonConvert.SerializeObject(res);
    }

    #region Fields

    //private CentrResponsibility myCO;
    //private References.Employee myEmployee;
    private SD_26 myEntity;

    //private PayForm myFormRaschet;
    //private Kontragent myKontragent;
    //private Kontragent myKontrReceiver;
    //private PayCondition myPayConditionCondition;
    //private NomenklProductType myVzaimoraschetType;
    private DogovorOfSupplierViewModel myContract;

    //private References.Employee myPersonaResponsible;

    //private Money myNakladAll;
    private decimal mySummaFact;

    private readonly bool isLoadAll;

    //private References.Currency myCurrency;
    private DogovorOfSupplierViewModel myDogovorOfSupplier;
    private readonly UnitOfWork<ALFAMEDIAEntities> context;

    #endregion

    #region Constructors

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
        // ReSharper disable once UnusedParameter.Local
        Rows.CollectionChanged += (o, args) => State = RowStatus.NotEdited;
    }

    public InvoiceProvider(SD_26 entity)
    {
        Entity = entity ?? DefaultValue();
        LoadReferences();
        // ReSharper disable once UnusedParameter.Local
        Rows.CollectionChanged += (o, args) => State = RowStatus.NotEdited;
    }

    #endregion

    #region Properties

    public ObservableCollection<ProviderInvoicePayViewModel> PaymentDocs { set; get; } =
        new ObservableCollection<ProviderInvoicePayViewModel>();

    public DateTime LastChangerDate { get; set; }

    public ObservableCollection<IInvoiceProviderRow> Rows { set; get; } =
        new ObservableCollection<IInvoiceProviderRow>();

    public ObservableCollection<WarehouseOrderInRow> Facts { set; get; } =
        new ObservableCollection<WarehouseOrderInRow>();

    public override string Name =>
        $"С/ф поставщика №{SF_IN_NUM}/{SF_POSTAV_NUM} от {DocDate.ToShortDateString()} на {Summa:n2} {Currency}";

    public override string Description =>
        $"С/ф поставщика №{SF_IN_NUM}/{SF_POSTAV_NUM} от {DocDate.ToShortDateString()} " +
        $"{Kontragent} на {Summa} {Currency} " +
        $"{Note}";

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

    public DogovorOfSupplierViewModel DogovorOfSupplier
    {
        get => myDogovorOfSupplier;
        set
        {
            if (myDogovorOfSupplier == value) return;
            myDogovorOfSupplier = value;
            RaisePropertiesChanged();
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
            RaisePropertyChanged(nameof(Summa));
            RaisePropertyChanged();
        }
    }

    /// <summary>
    ///     Ответственный
    /// </summary>
    public References.Employee PersonaResponsible
    {
        get => GlobalOptions.ReferencesCache.GetEmployee(Entity.PersonalResponsibleDC) as References.Employee;
        set
        {
            if (Equals(GlobalOptions.ReferencesCache.GetEmployee(Entity.PersonalResponsibleDC), value)) return;
            Entity.PersonalResponsibleDC = value?.DocCode;
            Entity.TABELNUMBER = value?.TabelNumber ?? 0;
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

    public DateTime DocDate
    {
        get => Entity.SF_POSTAV_DATE;
        set
        {
            if (Entity.SF_POSTAV_DATE == value) return;
            Entity.SF_POSTAV_DATE = value;
            RaisePropertyChanged();
        }
    }

    [Required(ErrorMessage = "Контрагент должен быть выбран обязательно.")]
    public Kontragent Kontragent
    {
        get => GlobalOptions.ReferencesCache.GetKontragent(Entity.SF_POST_DC) as Kontragent;
        set
        {
            if (Equals(GlobalOptions.ReferencesCache.GetKontragent(Entity.SF_POST_DC), value)) return;
            Entity.SF_POST_DC = value.DocCode;
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

    public decimal Summa
    {
        get
        {
            if (Rows.Count <= 0) return Entity.SF_CRS_SUMMA;
            var s = Rows == null || Rows.Count == 0 ? 0 : Rows.Sum(_ => _.Summa);
            Entity.SF_FACT_SUMMA = s;
            Entity.SF_KONTR_CRS_SUMMA = s;
            Entity.SF_CRS_SUMMA = s;
            Entity.SF_FACT_SUMMA = s;
            Entity.SF_CRS_RATE = 1;
            Entity.SF_KONTR_CRS_RATE = 1;
            Entity.SF_UCHET_VALUTA_RATE = 1;
            return s;
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


    public References.Currency Currency
    {
        get => GlobalOptions.ReferencesCache.GetCurrency(Entity.SF_CRS_DC) as References.Currency;
        set
        {
            if (Equals(GlobalOptions.ReferencesCache.GetCurrency(Entity.SF_CRS_DC), value)) return;
            Entity.SF_CRS_DC = value?.DocCode;
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
            Entity.V_CRS_ADD_PERCENT = (double?)value;
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
            SF_PAY_FLAG = (short)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    public decimal PaySumma => PaymentDocs?.Sum(_ => _.Summa) ?? 0;

    public decimal SFT_SUMMA_K_OPLATE { get; set; }

    //public decimal PaySumma => PaymentDocs?.Sum(_ => _.DocSumma) ?? 0;

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
            RaisePropertyChanged(nameof(PayCondition));
            RaisePropertyChanged();
        }
    }

    public PayCondition PayCondition
    {
        get => GlobalOptions.ReferencesCache.GetPayCondition(Entity.SF_PAY_COND_DC) as PayCondition;
        set
        {
            if (Equals(GlobalOptions.ReferencesCache.GetPayCondition(Entity.SF_PAY_COND_DC), value)) return;
            Entity.SF_PAY_COND_DC = value?.DocCode ?? 0;
            RaisePropertyChanged();
        }
    }

    public int? EmployeeTabelNumber => PersonaResponsible?.TabelNumber;

    [Required(ErrorMessage = "Ответственный должен быть выбран обязательно.")]
    public References.Employee Employee
    {
        get => GlobalOptions.ReferencesCache.GetEmployee(Entity.TABELNUMBER) as References.Employee;
        set
        {
            if (Equals(GlobalOptions.ReferencesCache.GetEmployee(Entity.TABELNUMBER), value)) return;
            Entity.TABELNUMBER = value?.TabelNumber ?? 0;
            RaisePropertyChanged(nameof(EmployeeTabelNumber));
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
            SF_EXECUTED = (short)(value ? 1 : 0);
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
            SF_ACCEPTED = (short)(value ? 1 : 0);
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
            Entity.SF_SUMMA_S_NDS = (short)(value ? 1 : 0);
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

    public override string Note
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
            RaisePropertyChanged(nameof(VzaimoraschetType));
            RaisePropertyChanged();
        }
    }

    public NomenklProductType VzaimoraschetType
    {
        get => GlobalOptions.ReferencesCache.GetNomenklProductType(Entity.SF_VZAIMOR_TYPE_DC) as NomenklProductType;
        set
        {
            if (Equals(GlobalOptions.ReferencesCache.GetNomenklProductType(Entity.SF_VZAIMOR_TYPE_DC), value)) return;
            Entity.SF_VZAIMOR_TYPE_DC = value?.DocCode;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    ///     Договор закупки
    /// </summary>
    public DogovorOfSupplierViewModel Contract
    {
        get => myContract;
        set
        {
            if (myContract == value) return;
            myContract = value;
            Entity.DogovorOfSupplierId = myContract?.Id;
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
            RaisePropertyChanged(nameof(FormRaschet));
            RaisePropertyChanged();
        }
    }

    public PayForm FormRaschet
    {
        get => GlobalOptions.ReferencesCache.GetPayForm(Entity.SF_FORM_RASCH_DC) as PayForm;
        set
        {
            if (Equals(GlobalOptions.ReferencesCache.GetPayForm(Entity.SF_FORM_RASCH_DC), value)) return;
            Entity.SF_FORM_RASCH_DC = value?.DocCode;
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
        // изменил НДС в цене на вкл ***
        set
        {
            if (Entity.SF_NDS_VKL_V_CENU == (value ? 1 : 0)) return;
            Entity.SF_NDS_VKL_V_CENU = (short?)(value ? 1 : 0);
            RaisePropertyChanged();
            foreach (var r in Rows.Cast<InvoiceProviderRow>())
            {
                r.IsIncludeInPrice = Entity.SF_NDS_VKL_V_CENU == 0 ? false : true;
                r.CalcRow();
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
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(CO));
        }
    }

    private CentrResponsibility myCO;

    public CentrResponsibility CO
    {
        get => myCO; // GlobalOptions.ReferencesCache.GetCentrResponsibility(Entity.SF_CENTR_OTV_DC) as CentrResponsibility;
        set
        {
            if (myCO == value) return;
            myCO = value;
            Entity.SF_CENTR_OTV_DC = value?.DocCode;
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
            myKontrReceiver = GlobalOptions.ReferencesCache.GetKontragent(Entity.SF_POLUCH_KONTR_DC) as Kontragent;
            RaisePropertyChanged(nameof(KontrReceiver));
            RaisePropertyChanged();
        }
    }

    private Kontragent myKontrReceiver;

    /// <summary>
    ///     Контрагент - получатель
    /// </summary>
    public Kontragent KontrReceiver
    {
        get => myKontrReceiver; // GlobalOptions.ReferencesCache.GetKontragent(Entity.SF_POLUCH_KONTR_DC) as Kontragent;
        set
        {
            if (myKontrReceiver == value) return;
            myKontrReceiver = value;
            Entity.SF_POLUCH_KONTR_DC = value?.DocCode;
            RaisePropertyChanged();
        }
    }

    public string LastChanger { get; set; }

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

    public void LoadReferences()
    {
        if (Entity.PersonalResponsibleDC == null && Entity.TABELNUMBER > 0)
        {
            PersonaResponsible = GlobalOptions.ReferencesCache.GetEmployee(Entity.TABELNUMBER) as References.Employee;
        }
        if (Entity.DogovorOfSupplier != null)
            Contract = new DogovorOfSupplierViewModel(Entity.DogovorOfSupplier);
        if(Rows ==  null)
            Rows = new ObservableCollection<IInvoiceProviderRow>();
        else
            Rows.Clear();
        if (Entity.TD_26 != null && Entity.TD_26.Count > 0)
            foreach (var t in Entity.TD_26)
            {
                var newRow = new InvoiceProviderRow(t)
                {
                    Parent = this
                };
                Rows.Add(newRow);
                if (t.TD_24 != null)
                    SummaFact += (t.SFT_SUMMA_K_OPLATE ?? 0) / (t.SFT_KOL == 0 ? 1 :  t.SFT_KOL) *
                                 t.TD_24.Sum(_ => _.DDT_KOL_PRIHOD);
            }

        LoadPayments();

        SummaFact = 0;
        if (Entity.SD_24 != null)
            SummaFact += (from q in Entity.TD_26
                from d in q.TD_24
                select d.DDT_KOL_PRIHOD * q.SFT_ED_CENA ?? 0).Sum();
        if (Facts == null)
            Facts = new ObservableCollection<WarehouseOrderInRow>();
        else
            Facts.Clear();
        if (Entity.TD_26 != null && Entity.TD_26.Count > 0)
            foreach (var r in Entity.TD_26)
                if (r.TD_24 != null && r.TD_24.Count > 0)
                    foreach (var r2 in r.TD_24)
                    {
                        var newItem = new WarehouseOrderInRow(r2, isLoadAll);
                        if (r2.SD_24 != null) newItem.Parent = new WarehouseOrderIn(r2.SD_24);

                        Facts.Add(newItem);
                    }

        RaisePropertyChanged(nameof(Summa));
    }

    public void LoadPayments()
    {
        if (PaymentDocs == null)
            PaymentDocs = new ObservableCollection<ProviderInvoicePayViewModel>();
        else
            PaymentDocs.Clear();

        if (Entity.ProviderInvoicePay != null && Entity.ProviderInvoicePay.Count > 0)
            foreach (var pay in Entity.ProviderInvoicePay)
            {
                var newItem = new ProviderInvoicePayViewModel(pay);
                PaymentDocs.Add(newItem);
            }
    }

    public List<SD_26> LoadList()
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(SD_26 ent)
    {
    }

    public void UpdateTo(SD_26 ent)
    {
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
            $"С/ф (поставщика) №{SF_POSTAV_NUM}/{SF_IN_NUM} от {DocDate.ToShortDateString()} " +
            $"от: {Kontragent} сумма: {Summa} {Currency}";
    }

    #endregion Methods
}

/// <summary>
///     Клас описывающий накладные расходы
/// </summary>
public class Overhead
{
    public KontragentViewModel KontragentViewModel { set; get; }
    //public Money Summa { set; get; }
}

public class SD_26LayoutData_FluentAPI : DataAnnotationForFluentApiBase, IMetadataProvider<InvoiceProvider>
{
    private new readonly string notNullMessage = "Поле должно быть заполнено";

    void IMetadataProvider<InvoiceProvider>.BuildMetadata(
        MetadataBuilder<InvoiceProvider> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(x => x.KontrReceiver).AutoGenerated()
            .DisplayName("Получатель");
        builder.Property(x => x.Currency).AutoGenerated()
            .DisplayName("Валюта").Required(() => notNullMessage);
        builder.Property(x => x.Summa).AutoGenerated()
            .DisplayName("Сумма").ReadOnly().DisplayFormatString("n2");
        builder.Property(x => x.SF_IN_NUM).AutoGenerated().LocatedAt(1)
            .DisplayName("№");
        builder.Property(x => x.DocDate).AutoGenerated().LocatedAt(2)
            .Required(() => notNullMessage)
            .DisplayName("Дата");
        builder.Property(x => x.SF_POSTAV_NUM).AutoGenerated()
            .DisplayName("Внешний №");
        builder.Property(x => x.IsAccepted).AutoGenerated()
            .DisplayName("Акцептован");
        builder.Property(x => x.IsNDSInPrice).AutoGenerated()
            .DisplayName("НДС в цене");
        builder.Property(x => x.Kontragent).AutoGenerated()
            .Required(() => notNullMessage)
            .DisplayName("Контрагент");
        builder.Property(x => x.CO).AutoGenerated()
            .Required(() => notNullMessage)
            .DisplayName("Центр ответственности");
        builder.Property(x => x.VzaimoraschetType).AutoGenerated()
            .Required(() => notNullMessage)
            .DisplayName("Тип взаиморасчета");
        builder.Property(x => x.PayCondition).AutoGenerated()
            .Required(() => notNullMessage)
            .DisplayName("Условие оплаты");
        builder.Property(x => x.FormRaschet).AutoGenerated()
            .Required(() => notNullMessage)
            .DisplayName("Форма расчета");
        builder.Property(x => x.SF_GRUZOOTPRAVITEL).AutoGenerated()
            .DisplayName("Грузоотправитель");
        builder.Property(x => x.SF_GRUZOPOLUCHATEL).AutoGenerated()
            .DisplayName("Грузополучатель");
        builder.Property(x => x.VzaimoraschetType).AutoGenerated()
            .Required(() => notNullMessage)
            .DisplayName("Тип продукции");
        builder.Property(x => x.SummaFact).ReadOnly().AutoGenerated()
            .DisplayName("Фактурировано").DisplayFormatString("n2");
        builder.Property(x => x.SF_KONTR_CRS_SUMMA).AutoGenerated()
            .DisplayName("Сумма контрагента").ReadOnly().DisplayFormatString("n2");
        //builder.Property(x => x.NakladAll).NotAutoGenerated()
        //    .DisplayName("Всего").ReadOnly().DisplayFormatString("n2");
        builder.Property(x => x.Overheads).NotAutoGenerated()
            .DisplayName("По контрагентам").NullDisplayText("Накладные отсутствуют ");
        builder.Property(x => x.Contract).AutoGenerated()
            .DisplayName("Договор поставки");
        builder.Property(x => x.EmployeeTabelNumber).AutoGenerated()
            .DisplayName("Таб.№");
        builder.Property(x => x.PersonaResponsible).NotAutoGenerated()
            .DisplayName("Имя");
        builder.Property(x => x.State).AutoGenerated()
            .DisplayName("Статус");
        builder.Property(x => x.CREATOR).AutoGenerated()
            .DisplayName("Создатель");
        builder.Property(x => x.Note).AutoGenerated().DisplayName("Примечания").MultilineTextDataType()
            .DisplayName("Примечание");
        builder.Property(x => x.PaySumma).AutoGenerated()
            .DisplayName("Оплачено");
        builder.Property(x => x.PersonaResponsible).AutoGenerated()
            .Required(() => notNullMessage)
            .DisplayName("Ответственный");
        builder.Property(x => x.IsInvoiceNakald).AutoGenerated()
            .DisplayName("Счет накладных");
        builder.Property(x => x.NakladDistributedSumma).AutoGenerated()
            .DisplayName("Распределено (накл)").ReadOnly().DisplayFormatString("n2");
        builder.Property(x => x.DogovorOfSupplier).AutoGenerated()
            .DisplayName("Договор поставщика");

        #region Form Layout

        // @formatter:off
        builder.DataFormLayout()
                .Group("first",Orientation.Vertical)
                    .Group("Счет", Orientation.Horizontal)
                        .ContainsProperty(_ => _.SF_IN_NUM)
                        .ContainsProperty(_ => _.DocDate)
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
                                    .ContainsProperty(_ => _.Summa)
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
                                    .ContainsProperty(_ => _.EmployeeTabelNumber)
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
                                .ContainsProperty(_ => _.Note)
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
        builder.Property(x => x.Summa).AutoGenerated()
            .DisplayName("Сумма").ReadOnly().DisplayFormatString("n2");
        builder.Property(x => x.SF_IN_NUM).AutoGenerated()
            .DisplayName("№");
        builder.Property(x => x.DocDate).AutoGenerated()
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
        builder.Property(x => x.Note).AutoGenerated()
            .DisplayName("Примечание");
        builder.Property(x => x.PaySumma).AutoGenerated()
            .DisplayName("Оплачено");
        builder.Property(x => x.PersonaResponsible).AutoGenerated()
            .DisplayName("Ответственный");
        builder.Property(x => x.IsNDSInPrice).AutoGenerated()
            .DisplayName("НДС в цене");
    }
}
