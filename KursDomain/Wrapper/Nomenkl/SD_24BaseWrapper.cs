using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Data;
using KursDomain.ICommon;
using KursDomain.IDocuments;
using KursDomain.IReferences;
using KursDomain.References;
using KursDomain.Wrapper.Base;
using Prism.Events;

namespace KursDomain.Wrapper.Nomenkl;

public class SD_24BaseWrapper : BaseWrapper<SD_24>, IEquatable<TD_24BaseWrapper>
{
    #region Constructors

    protected SD_24BaseWrapper(SD_24 model, IReferencesCache cache, ALFAMEDIAEntities context,
        IEventAggregator eventAggregator, IMessageDialogService messageDialogService) : base(model, eventAggregator,
        messageDialogService)
    {
        myCache = cache;
        myContext = context;
        Rows = new ObservableCollection<TD_24BaseWrapper>();
    }

    #endregion

    #region Methods

    public override void Initialize()
    {
        if (Model.TD_24 is { Count: > 0 })
            foreach (var row in Model.TD_24)
                Rows.Add(new TD_24BaseWrapper(row, myCache, myContext, EventAggregator, MessageDialogService));
    }

    #endregion

    #region Fields

    protected readonly IReferencesCache myCache;
    protected readonly ALFAMEDIAEntities myContext;

    #endregion


    #region Properties

    public virtual ObservableCollection<TD_24BaseWrapper> Rows { get; }


    public override decimal DocCode
    {
        get => Model.DOC_CODE;
        set
        {
            if (Model.DOC_CODE == value) return;
            Model.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public override Guid Id
    {
        get => Model.Id;
        set
        {
            if (Model.Id == value) return;
            Model.Id = value;
            RaisePropertyChanged();
        }
    }


    [Display(AutoGenerateField = true, Name = "Тип документа")]
    public string DocumentTypeName
    {
        get
        {
            if (Model.DD_TYPE_DC <= 0) return "Не указан";
            var en = (MaterialDocumentTypeEnum)Model.DD_TYPE_DC;
            return en.GetDisplayAttributesFrom(typeof(MaterialDocumentTypeEnum)).Name;
        }
    }

    [Display(AutoGenerateField = true, Name = "Дата")]
    public virtual DateTime DocDate
    {
        get => Model.DD_DATE;
        set
        {
            if (Model.DD_DATE == value) return;
            Model.DD_DATE = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "№")]
    public virtual int DocNum
    {
        get => Model.DD_IN_NUM;
        set
        {
            if (Model.DD_IN_NUM == value) return;
            Model.DD_IN_NUM = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Внеш.№")]
    public virtual string DocExtNum
    {
        set
        {
            if (Model.DD_EXT_NUM == value) return;
            Model.DD_EXT_NUM = value;
            RaisePropertyChanged();
        }

        get => Model.DD_EXT_NUM;
    }

    private Warehouse myWarehouseOut;

    [Display(AutoGenerateField = true, Name = "Склад-отправитель")]
    public virtual Warehouse WarehouseOut
    {
        get => myWarehouseOut; //myCache.GetWarehouse(Model.DD_SKLAD_OTPR_DC) as Warehouse;
        set
        {
            if (myWarehouseOut is not null && Model.DD_SKLAD_OTPR_DC == value?.DocCode) return;
            myWarehouseOut = value;
            Model.DD_SKLAD_OTPR_DC = value?.DocCode;
            Model.DD_POLUCH_NAME = value?.Name ?? string.Empty;
            RaisePropertyChanged();
        }
    }

    private Warehouse myWarehouseIn;

    [Display(AutoGenerateField = true, Name = "Склад-получатель")]
    public virtual Warehouse WarehouseIn
    {
        get => myWarehouseIn; //myCache.GetWarehouse(Model.DD_SKLAD_POL_DC) as Warehouse;
        set
        {
            if (Model.DD_SKLAD_POL_DC == value?.DocCode) return;
            myWarehouseIn = value;
            Model.DD_SKLAD_POL_DC = value?.DocCode;
            Model.DD_OTRPAV_NAME = value?.Name;
            RaisePropertyChanged();
        }
    }

    private Kontragent myKontargentSender;

    [Display(AutoGenerateField = true, Name = "Контрагент-отправитель")]
    public virtual Kontragent KontargentSender
    {
        get => myKontargentSender; // myCache.GetKontragent(Model.DD_KONTR_OTPR_DC) as Kontragent;
        set
        {
            if (Model.DD_KONTR_OTPR_DC == value?.DocCode) return;
            myKontargentSender = value;
            Model.DD_OTRPAV_NAME = value?.Name;
            Model.DD_KONTR_OTPR_DC = value?.DocCode;
            RaisePropertyChanged();
        }
    }

    private Kontragent myKontragent;

    [Display(AutoGenerateField = true, Name = "Контрагент-получатель")]
    public virtual Kontragent KontargentRecepient
    {
        get => myKontragent; // myCache.GetKontragent(Model.DD_KONTR_POL_DC) as Kontragent;
        set
        {
            if (Model.DD_KONTR_POL_DC == value?.DocCode) return;
            myKontragent = value;
            Model.DD_KONTR_POL_DC = value?.DocCode;
            Model.DD_POLUCH_NAME = value?.Name ?? string.Empty;
            RaisePropertyChanged();
        }
    }

    private Employee myStoreKeeper;

    [Display(AutoGenerateField = true, Name = "Кладовщик")]
    public virtual Employee StoreKeeper
    {
        get => myStoreKeeper; //myCache.GetEmployee(Model.DD_KLADOV_TN) as Employee;
        set
        {
            if (Model.DD_KLADOV_TN == value?.TabelNumber) return;
            myStoreKeeper = value;
            Model.DD_KLADOV_TN = value?.TabelNumber;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Проведен")]
    public virtual bool IsExecuted
    {
        get => Model.DD_EXECUTED != 1;
        set
        {
            if (Model.DD_EXECUTED == (value ? 1 : 0)) return;
            Model.DD_EXECUTED = (short)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    private Employee myPersonaRecepient;


    [Display(AutoGenerateField = true, Name = "Лицо-получатель")]
    public virtual Employee PersonaRecepient
    {
        get => myPersonaRecepient; // myCache.GetEmployee(Model.DD_POLUCHATEL_TN) as Employee;
        set
        {
            if (Model.DD_POLUCHATEL_TN == value?.TabelNumber) return;
            myPersonaRecepient = value;
            Model.DD_POLUCHATEL_TN = value?.TabelNumber;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Отправитель")]
    public virtual string SenderName
    {
        get => Model.DD_OTRPAV_NAME;
        set
        {
            if (Model.DD_OTRPAV_NAME == value) return;
            Model.DD_OTRPAV_NAME = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Получатель")]
    public virtual string RecepientName
    {
        get => Model.DD_POLUCH_NAME ?? string.Empty;
        set
        {
            if (Model.DD_POLUCH_NAME == value) return;
            Model.DD_POLUCH_NAME = value ?? string.Empty;
            RaisePropertyChanged();
        }
    }

    private Employee myPersonaSendNomenkl;

    [Display(AutoGenerateField = true, Name = "Сдал товар")]
    public virtual Employee PersonaSendNomenkl
    {
        get => myPersonaSendNomenkl; //myCache.GetEmployee(Model.DD_KTO_SDAL_TN) as Employee;
        set
        {
            if (Model.DD_KTO_SDAL_TN == value?.TabelNumber) return;
            myPersonaSendNomenkl = value;
            Model.DD_KTO_SDAL_TN = value?.TabelNumber;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Получил")]
    public virtual string RecepientPersonaName
    {
        get => Model.DD_KOMU_PEREDANO;
        set
        {
            if (Model.DD_KOMU_PEREDANO == value) return;
            Model.DD_KOMU_PEREDANO = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Через кого")]
    public virtual string SenderPersonaName
    {
        get => Model.DD_OT_KOGO_POLUCHENO;
        set
        {
            if (Model.DD_OT_KOGO_POLUCHENO == value) return;
            Model.DD_OT_KOGO_POLUCHENO = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Грузоотправитель")]
    public virtual string ShipperName
    {
        get => Model.DD_GRUZOOTPRAVITEL;
        set
        {
            if (Model.DD_GRUZOOTPRAVITEL == value) return;
            Model.DD_GRUZOOTPRAVITEL = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Грузополучатель")]
    public virtual string ConsigneeName
    {
        get => Model.DD_GRUZOPOLUCHATEL;
        set
        {
            if (Model.DD_GRUZOPOLUCHATEL == value) return;
            Model.DD_GRUZOPOLUCHATEL = value;
            RaisePropertyChanged();
        }
    }

    private Warehouse myWarehouseTo;

    [Display(AutoGenerateField = true, Name = "Отпуск на склад")]
    public virtual Warehouse WarehouseTo
    {
        get => myWarehouseTo; //myCache.GetWarehouse(Model.DD_OTPUSK_NA_SKLAD_DC) as Warehouse;
        set
        {
            if (Model.DD_OTPUSK_NA_SKLAD_DC == value?.DocCode) return;
            myWarehouseTo = value;
            Model.DD_OTPUSK_NA_SKLAD_DC = value?.DocCode;
            RaisePropertyChanged();
        }
    }

    private Warehouse myWarehouseFrom;

    [Display(AutoGenerateField = true, Name = "Приход со склада")]
    public virtual Warehouse WarehouseFrom
    {
        get => myWarehouseFrom; //myCache.GetWarehouse(Model.DD_PRIHOD_SO_SKLADA_DC) as Warehouse;
        set
        {
            if (Model.DD_PRIHOD_SO_SKLADA_DC == value?.DocCode) return;
            myWarehouseFrom = value;
            Model.DD_PRIHOD_SO_SKLADA_DC = value?.DocCode;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Шаблон")]
    public virtual bool IsTemplate
    {
        get => Model.DD_SHABLON != 1;
        set
        {
            if (Model.DD_SHABLON == (value ? 1 : 0)) return;
            Model.DD_SHABLON = (short)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Создатель")]
    public string Creator
    {
        get => Model.CREATOR;
        set
        {
            if (Model.CREATOR == value) return;
            Model.CREATOR = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Счет")]
    public string Invoice
    {
        get => Model.DD_SCHET;
        set
        {
            if (Model.DD_SCHET == value) return;
            Model.DD_SCHET = value;
            RaisePropertyChanged();
        }
    }

    public override string Note
    {
        get => Model.DD_NOTES;
        set
        {
            if (Model.DD_NOTES == value) return;
            Model.DD_NOTES = value;
            RaisePropertyChanged();
        }
    }

    public Currency KontragentCurrency => (KontargentSender?.Currency ?? KontargentRecepient?.Currency) as Currency;

    public bool Equals(TD_24BaseWrapper other)
    {
        if (other == null) return false;
        return DocCode == other.DocCode;
    }

    #endregion
}
