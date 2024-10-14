using System;
using System.ComponentModel.DataAnnotations;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.References;
using KursDomain.Wrapper.Base;
using KursDomain.Wrapper.InvoiceClient;
using KursDomain.Wrapper.InvoiceProvider;
using Prism.Events;

namespace KursDomain.Wrapper.Nomenkl;

public class TD_24BaseWrapper : BaseWrapper<TD_24>, IEquatable<TD_24BaseWrapper>, IRowDC
{
    #region Constructors

    public TD_24BaseWrapper(TD_24 model, IReferencesCache cache, ALFAMEDIAEntities context,
        IEventAggregator eventAggregator, IMessageDialogService messageDialogService) : base(model, eventAggregator,
        messageDialogService)
    {
        myCache = cache;
        myContext = context;
    }

    #endregion

    #region Methods

    public bool Equals(TD_24BaseWrapper other)
    {
        if (other == null) return false;
        return DocCode == other.DocCode && Code == other.Code;
    }

    #endregion

    #region Fields

    private readonly IReferencesCache myCache;
    private readonly ALFAMEDIAEntities myContext;
    private InvoiceProviderRowWrapper myInvoiceProviderRow;
    private InvoiceClientRowWrapper myInvoiceClientRow;
    private TD_24BaseWrapper myWarehouseOrderOutRow;

    #endregion

    #region Properties

    
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

    public override int Code
    {
        get => Model.CODE;
        set
        {
            if (Model.CODE == value) return;
            Model.CODE = value;
            RaisePropertyChanged();
        }
    }

    private References.Nomenkl myNomenkl;

    [Display(AutoGenerateField = true, Name = "Номенклатура")]
    public References.Nomenkl Nomenkl
    {
        get => myNomenkl;//myCache.GetNomenkl(Model.DDT_NOMENKL_DC) as References.Nomenkl;
        set
        {
            if (Model.DDT_NOMENKL_DC == value?.DocCode) return;
            myNomenkl = value;
            Model.DDT_NOMENKL_DC = value?.DocCode ?? 0;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Приход")]
    public virtual decimal QuantityIn
    {
        get => Model.DDT_KOL_PRIHOD;
        set
        {
            if (Model.DDT_KOL_PRIHOD == value) return;
            Model.DDT_KOL_PRIHOD = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Требование")]
    public virtual double Requirement
    {
        get => Math.Round(Model.DDT_KOL_ZATREBOVANO ?? 0, 4);
        set
        {
            if (Math.Abs(Math.Round(Model.DDT_KOL_ZATREBOVANO ?? 0, 4) - Math.Round(value, 4)) < 0.0001) return;
            Model.DDT_KOL_ZATREBOVANO = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Расход")]
    public virtual decimal QuantityOut
    {
        get => Model.DDT_KOL_RASHOD;
        set
        {
            if (Model.DDT_KOL_RASHOD == value) return;
            Model.DDT_KOL_RASHOD = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Подтверждено")]
    public virtual double Сonfirmed
    {
        get => Math.Round(Model.DDT_KOL_PODTVERZHDENO ?? 0, 4);
        set
        {
            if (Math.Abs(Math.Round(Model.DDT_KOL_PODTVERZHDENO ?? 0, 4) - Math.Round(value, 4)) < 0.0001) return;
            Model.DDT_KOL_PODTVERZHDENO = value;
            RaisePropertyChanged();
        }
    }

    private Unit myUnit;

    [Display(AutoGenerateField = true, Name = "Ед.изм.")]
    public Unit Unit
    {
        get => myUnit; //myCache.GetUnit(Model.DDT_ED_IZM_DC) as Unit;
        set
        {
            if (Model.DDT_ED_IZM_DC == value?.DocCode) return;
            myUnit = value;
            Model.DDT_ED_IZM_DC = value?.DocCode ?? 0;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Счет поставщика")]
    public InvoiceProviderRowWrapper InvoiceProviderRow
    {
        set
        {
            if (myInvoiceProviderRow == value) return;
            Model.DDT_SPOST_DC = value?.DocCode;
            Model.DDT_SPOST_ROW_CODE = value?.Code;
            myInvoiceProviderRow = value;
            RaisePropertyChanged();
        }
        get => myInvoiceProviderRow;
    }

    private Currency myCurrency;


    [Display(AutoGenerateField = true, Name = "Валюта")]
    public Currency Currency
    {
        get => myCurrency;//myCache.GetCurrency(Model.DDT_CRS_DC) as Currency;
        set
        {
            if (Model.DDT_CRS_DC == value?.DocCode) return;
            myCurrency = value;
            Model.DDT_CRS_DC = value?.DocCode ?? 0;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Счет клиента")]
    public InvoiceClientRowWrapper InvoiceClientRow
    {
        set
        {
            if (myInvoiceClientRow == value) return;
            Model.DDT_SFACT_DC = value?.DocCode;
            Model.DDT_SFACT_ROW_CODE = value?.Code;
            myInvoiceClientRow = value;
            RaisePropertyChanged();
        }
        get => myInvoiceClientRow;
    }

    private Kontragent myDiler;

    [Display(AutoGenerateField = true, Name = "Дилер")]
    public Kontragent Diler
    {
        set
        {
            if (Model.DDT_DILER_DC == value?.DocCode) return;
            myDiler = value;
            Model.DDT_DILER_DC = value?.DocCode;
            RaisePropertyChanged();
        }
        get => myDiler; //myCache.GetKontragent(Model.DDT_DILER_DC) as Kontragent;
    }

    
    [Display(AutoGenerateField = true, Name = "Остаток (стар)")]
    public virtual double OldRemains
    {
        get => Math.Round(Model.DDT_OSTAT_STAR ?? 0, 4);
        set
        {
            if (Math.Abs(Math.Round(Model.DDT_OSTAT_STAR ?? 0, 4) - Math.Round(value, 4)) < 0.0001) return;
            Model.DDT_OSTAT_STAR = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Остаток (нов)")]
    public virtual double NewRemains
    {
        get => Math.Round(Model.DDT_OSTAT_NOV ?? 0, 4);
        set
        {
            if (Math.Abs(Math.Round(Model.DDT_OSTAT_NOV ?? 0, 4) - Math.Round(value, 4)) < 0.0001) return;
            Model.DDT_OSTAT_NOV = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Цена (валюта)")]
    public virtual decimal PriceCurrency
    {
        get => Math.Round(Model.DDT_TAX_CRS_CENA ?? 0, 4);
        set
        {
            if (Model.DDT_TAX_CRS_CENA == value ) return;
            Model.DDT_TAX_CRS_CENA = value;
            RaisePropertyChanged();
        }
    }
    [Display(AutoGenerateField = true, Name = "Цена")]
    public virtual decimal Price
    {
        get => Math.Round(Model.DDT_TAX_CENA ?? 0, 4);
        set
        {
            if (Model.DDT_TAX_CENA == value ) return;
            Model.DDT_TAX_CENA = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Таксировка")]
    public virtual bool IsTaxExecuted
    {
        get => Model.DDT_TAX_EXECUTED == 1;
        set
        {
            if (Model.DDT_TAX_EXECUTED == (value ? 1 : 0) ) return;
            Model.DDT_TAX_EXECUTED = (short)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Таксировка в счете")]
    public virtual bool IsTaxSFactExecuted
    {
        get => Model.DDT_TAX_IN_SFACT == 1;
        set
        {
            if (Model.DDT_TAX_IN_SFACT == (value ? 1 : 0) ) return;
            Model.DDT_TAX_IN_SFACT = (short?)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Цена факт (валюта)")]
    public virtual decimal PriceFactCurrency
    {
        get => Math.Round(Model.DDT_FACT_CRS_CENA ?? 0, 4);
        set
        {
            if (Model.DDT_FACT_CRS_CENA == value ) return;
            Model.DDT_FACT_CRS_CENA = value;
            RaisePropertyChanged();
        }
    }
    [Display(AutoGenerateField = true, Name = "Цена факт")]
    public virtual decimal PriceFact
    {
        get => Math.Round(Model.DDT_FACT_CENA ?? 0, 4);
        set
        {
            if (Model.DDT_FACT_CENA == value ) return;
            Model.DDT_FACT_CENA = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Фактурирование")]
    public virtual bool IsFactExecuted
    {
        get => Model.DDT_FACT_EXECUTED == 1;
        set
        {
            if (Model.DDT_FACT_EXECUTED == (value ? 1 : 0) ) return;
            Model.DDT_FACT_EXECUTED = (short)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    private SDRSchet mySHPZ;

    [Display(AutoGenerateField = true, Name = "Счет дох/расх")]
    public SDRSchet SHPZ
    {
        get => mySHPZ; //myCache.GetSDRSchet(Model.DDT_SHPZ_DC) as SDRSchet;
        set
        {
            if (Model.DDT_SHPZ_DC == value?.DocCode) return;
            mySHPZ = value;
            Model.DDT_SHPZ_DC = value?.DocCode ?? 0;
            RaisePropertyChanged();
        }
    }


    [Display(AutoGenerateField = true, Name = "Сумма (вал. кон-та)")]
    public virtual decimal SummaKontrCurrency
    {
        get => Math.Round(Model.DDT_KONTR_CRS_SUMMA ?? 0, 4);
        set
        {
            if (Model.DDT_KONTR_CRS_SUMMA == value ) return;
            Model.DDT_KONTR_CRS_SUMMA = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Сумма (вал. учетн.)")]
    public virtual decimal SummaUchetCurrency
    {
        get => Math.Round(Model.DDT_SUMMA_V_UCHET_VALUTE ?? 0, 4);
        set
        {
            if (Model.DDT_SUMMA_V_UCHET_VALUTE == value ) return;
            Model.DDT_SUMMA_V_UCHET_VALUTE = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Цена (вал. учетн.)")]
    public virtual decimal PriceUchetCurrency
    {
        get => Math.Round(Model.DDT_CENA_V_UCHET_VALUTE ?? 0, 4);
        set
        {
            if (Model.DDT_CENA_V_UCHET_VALUTE == value ) return;
            Model.DDT_CENA_V_UCHET_VALUTE = value;
            RaisePropertyChanged();
        }
    }

    private Warehouse myWarehouseOut;


    [Display(AutoGenerateField = true, Name = "Склад-отправитель")]
    public virtual Warehouse WarehouseOut
    {
        get => myWarehouseOut; //myCache.GetWarehouse(Model.DDT_SKLAD_OTPR_DC) as Warehouse;
        set
        {
            if (Model.DDT_SKLAD_OTPR_DC == value?.DocCode) return;
            myWarehouseOut = value;
            Model.DDT_SKLAD_OTPR_DC = value?.DocCode;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Валюта (номенклатура)")]
    public Currency NomenklCurrency => Nomenkl?.Currency as Currency;

    [Display(AutoGenerateField = true, Name = "Сумма (вал. кон-та)")]
    public virtual decimal NomenklRate
    {
        get => (decimal)Math.Round(Model.DDT_NOM_CRS_RATE ?? 0, 4);
        set
        {
            if (Model.DDT_NOM_CRS_RATE == (double?)value) return;
            Model.DDT_NOM_CRS_RATE = (double?)value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Расх. склад. ордер")]
    public TD_24BaseWrapper WarehouseOrderOutRow
    {
        set
        {
            if (myWarehouseOrderOutRow == value) return;
            myWarehouseOrderOutRow = value;
            RaisePropertyChanged();
        }
        get => myWarehouseOrderOutRow;
    }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public override string Note
    {
        get => Model.DDT_NOTE;
        set
        {
            if (Model.DDT_NOTE == value ) return;
            Model.DDT_NOTE = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false, Name = "Id")]
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

    [Display(AutoGenerateField = false, Name = "DocId")]
    public Guid DocId
    {
        get => Model.DocId;
        set
        {
            if (Model.DocId == value) return;
            Model.DocId = value;
            RaisePropertyChanged();
        }
    }

    
    #endregion
}
