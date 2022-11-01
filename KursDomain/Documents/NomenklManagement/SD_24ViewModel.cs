using System;
using System.Collections.Generic;
using Core;
using Core.ViewModel.Base;
using Data;
using KursDomain.Documents.CommonReferences.Kontragent;
using KursDomain.Documents.Invoices;
using KursDomain.Documents.Periods;

namespace KursDomain.Documents.NomenklManagement;

public interface ISD_24
{
    string Sender { set; get; }
    string Receiver { set; get; }

    decimal DocCode { set; get; }

    string DocTypeName { set; get; }
    DateTime Date { set; get; }
    int DD_IN_NUM { set; get; }
    string DD_EXT_NUM { set; get; }
    Warehouse WarehouseOut { set; get; }
    Warehouse WarehouseIn { set; get; }
    Kontragent KontragentSender { set; get; }
    Kontragent KontragentReceiver { set; get; }
    Employee.Employee Kladovshik { set; get; }
    bool IsExecuted { set; get; }
    string DD_KOMU_PEREDANO { set; get; }
    string DD_OT_KOGO_POLUCHENO { set; get; }

    int? DD_POLUCHATEL_TN { set; get; }

    string CREATOR { set; get; }
    string DD_SCHET { set; get; }

    string Note { set; get; }
    IInvoiceProvider InvoiceProvider { set; get; }
    IInvoiceClient InvoiceClient { set; get; }
    bool IsVozvrat { set; get; }
}

public class SD_24ViewModel : RSViewModelBase, IEntity<SD_24>
{
    public string Sender => KontragentSender?.Name ?? WarehouseOut?.Name;
    public string Receiver => KontragentReceiver?.Name ?? WarehouseIn?.Name;

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

    public string DocTypeName => Entity.SD_201?.D_NAME;

    public MaterialDocumentType DocumentType
    {
        get => myDocumentType;
        set
        {
            if (myDocumentType != null && myDocumentType.Equals(value)) return;
            myDocumentType = value;
            if (myDocumentType != null)
                Entity.DD_TYPE_DC = myDocumentType.DOC_CODE;
            RaisePropertyChanged();
        }
    }

    public DateTime Date
    {
        get => Entity.DD_DATE;
        set
        {
            if (Entity.DD_DATE == value) return;
            Entity.DD_DATE = value;
            RaisePropertyChanged();
        }
    }

    public int DD_IN_NUM
    {
        get => Entity.DD_IN_NUM;
        set
        {
            if (Entity.DD_IN_NUM == value) return;
            Entity.DD_IN_NUM = value;
            RaisePropertyChanged();
        }
    }

    public string DD_EXT_NUM
    {
        get => Entity.DD_EXT_NUM;
        set
        {
            if (Entity.DD_EXT_NUM == value) return;
            Entity.DD_EXT_NUM = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    ///     Склад отправитель
    /// </summary>
    public Warehouse WarehouseOut
    {
        get => MainReferences.GetWarehouse(Entity.DD_SKLAD_OTPR_DC);
        set
        {
            if (Entity.DD_SKLAD_OTPR_DC == value?.DocCode) return;
            Entity.DD_SKLAD_OTPR_DC = value?.DocCode;
            Entity.DD_OTRPAV_NAME = MainReferences.GetWarehouse(Entity.DD_SKLAD_OTPR_DC)?.Name;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Sender));
        }
    }

    /// <summary>
    ///     Склад получатель
    /// </summary>
    public Warehouse WarehouseIn
    {
        get => MainReferences.GetWarehouse(Entity.DD_SKLAD_POL_DC);
        set
        {
            if (Entity.DD_SKLAD_POL_DC == value?.DocCode) return;
            Entity.DD_SKLAD_POL_DC = value?.DocCode;
            Entity.DD_POLUCH_NAME = MainReferences.GetWarehouse(Entity.DD_SKLAD_POL_DC).Name;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Receiver));
        }
    }

    /// <summary>
    ///     Контрагент отправитель
    /// </summary>
    public Kontragent KontragentSender
    {
        get => MainReferences.GetKontragent(DD_KONTR_OTPR_DC);
        set
        {
            if (DD_KONTR_OTPR_DC == value?.DocCode) return;
            DD_KONTR_OTPR_DC = value?.DocCode;
            Entity.DD_OTRPAV_NAME = MainReferences.GetKontragent(DD_KONTR_OTPR_DC)?.Name;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Sender));
        }
    }

    /// <summary>
    ///     Контрагент получатель
    /// </summary>
    public Kontragent KontragentReceiver
    {
        get => MainReferences.GetKontragent(DD_KONTR_POL_DC);
        set
        {
            if (DD_KONTR_POL_DC == value?.DocCode) return;
            DD_KONTR_POL_DC = value?.DocCode;
            Entity.DD_POLUCH_NAME = MainReferences.GetKontragent(DD_KONTR_POL_DC).Name;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Sender));
        }
    }

    public Employee.Employee Kladovshik
    {
        // ReSharper disable once RedundantCast
        get => MainReferences.GetEmployee(Entity.DD_KLADOV_TN as int?);
        set
        {
            if (Entity.DD_KLADOV_TN == value?.TabelNumber) return;
            Entity.DD_KLADOV_TN = value?.TabelNumber;
            RaisePropertyChanged();
        }
    }

    public bool IsExecuted
    {
        get => Entity.DD_EXECUTED == 1;
        set
        {
            if (Entity.DD_EXECUTED == 1 == value) return;
            Entity.DD_EXECUTED = (short)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }


    //public short DD_EXECUTED
    //{
    //    get => Entity.DD_EXECUTED;
    //    set
    //    {
    //        if (Entity.DD_EXECUTED == value) return;
    //        Entity.DD_EXECUTED = value;
    //        RaisePropertyChanged();
    //    }
    //}

    public int? DD_POLUCHATEL_TN
    {
        get => Entity.DD_POLUCHATEL_TN;
        set
        {
            if (Entity.DD_POLUCHATEL_TN == value) return;
            Entity.DD_POLUCHATEL_TN = value;
            RaisePropertyChanged();
        }
    }

    //public string DD_OTRPAV_NAME
    //{
    //    get => Entity.DD_OTRPAV_NAME;
    //    set
    //    {
    //        if (Entity.DD_OTRPAV_NAME == value) return;
    //        Entity.DD_OTRPAV_NAME = value;
    //        RaisePropertyChanged();
    //    }
    //}

    //public string DD_POLUCH_NAME
    //{
    //    get => Entity.DD_POLUCH_NAME;
    //    set
    //    {
    //        if (Entity.DD_POLUCH_NAME == value) return;
    //        Entity.DD_POLUCH_NAME = value;
    //        RaisePropertyChanged();
    //    }
    //}

    public int? DD_KTO_SDAL_TN
    {
        get => Entity.DD_KTO_SDAL_TN;
        set
        {
            if (Entity.DD_KTO_SDAL_TN == value) return;
            Entity.DD_KTO_SDAL_TN = value;
            RaisePropertyChanged();
        }
    }

    public string DD_KOMU_PEREDANO
    {
        get => Entity.DD_KOMU_PEREDANO;
        set
        {
            if (Entity.DD_KOMU_PEREDANO == value) return;
            Entity.DD_KOMU_PEREDANO = value;
            RaisePropertyChanged();
        }
    }

    public string DD_OT_KOGO_POLUCHENO
    {
        get => Entity.DD_OT_KOGO_POLUCHENO;
        set
        {
            if (Entity.DD_OT_KOGO_POLUCHENO == value) return;
            Entity.DD_OT_KOGO_POLUCHENO = value;
            RaisePropertyChanged();
        }
    }

    public string DD_GRUZOOTPRAVITEL
    {
        get => Entity.DD_GRUZOOTPRAVITEL;
        set
        {
            if (Entity.DD_GRUZOOTPRAVITEL == value) return;
            Entity.DD_GRUZOOTPRAVITEL = value;
            RaisePropertyChanged();
        }
    }

    public string DD_GRUZOPOLUCHATEL
    {
        get => Entity.DD_GRUZOPOLUCHATEL;
        set
        {
            if (Entity.DD_GRUZOPOLUCHATEL == value) return;
            Entity.DD_GRUZOPOLUCHATEL = value;
            RaisePropertyChanged();
        }
    }


    ///// <summary>
    /////     Отпуск на склад
    ///// </summary>
    //public Warehouse OutOnWarehouse
    //{
    //    get => myOutOnWarehouse;
    //    set
    //    {
    //        if (myOutOnWarehouse != null && myOutOnWarehouse.Equals(value)) return;
    //        myOutOnWarehouse = value;
    //        if (myOutOnWarehouse != null)
    //            Entity.DD_OTPUSK_NA_SKLAD_DC = myOutOnWarehouse.DocCode;
    //        RaisePropertyChanged();
    //    }
    //}

    ///// <summary>
    /////     Приход со склада
    ///// </summary>
    //public Warehouse InFromWarehouse
    //{
    //    get => myInFromWarehouse;
    //    set
    //    {
    //        if (myInFromWarehouse != null && myInFromWarehouse.Equals(value)) return;
    //        myInFromWarehouse = value;
    //        if (myInFromWarehouse != null)
    //            Entity.DD_PRIHOD_SO_SKLADA_DC = myInFromWarehouse.DocCode;
    //        RaisePropertyChanged();
    //    }
    //}

    public short DD_SHABLON
    {
        get => Entity.DD_SHABLON;
        set
        {
            if (Entity.DD_SHABLON == value) return;
            Entity.DD_SHABLON = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DD_VED_VIDACH_DC
    {
        get => Entity.DD_VED_VIDACH_DC;
        set
        {
            if (Entity.DD_VED_VIDACH_DC == value) return;
            Entity.DD_VED_VIDACH_DC = value;
            RaisePropertyChanged();
        }
    }

    public Period Period
    {
        get => myPeriod;
        set
        {
            if (myPeriod != null && myPeriod.Equals(value)) return;
            myPeriod = value;
            Entity.DD_PERIOD_DC = myPeriod?.DOC_CODE;
            RaisePropertyChanged();
        }
    }

    public int? DD_TREB_NUM
    {
        get => Entity.DD_TREB_NUM;
        set
        {
            if (Entity.DD_TREB_NUM == value) return;
            Entity.DD_TREB_NUM = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? DD_TREB_DATE
    {
        get => Entity.DD_TREB_DATE;
        set
        {
            if (Entity.DD_TREB_DATE == value) return;
            Entity.DD_TREB_DATE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DD_TREB_DC
    {
        get => Entity.DD_TREB_DC;
        set
        {
            if (Entity.DD_TREB_DC == value) return;
            Entity.DD_TREB_DC = value;
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

    public short? DD_PODTVERZHDEN
    {
        get => Entity.DD_PODTVERZHDEN;
        set
        {
            if (Entity.DD_PODTVERZHDEN == value) return;
            Entity.DD_PODTVERZHDEN = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DD_OSN_OTGR_DC
    {
        get => Entity.DD_OSN_OTGR_DC;
        set
        {
            if (Entity.DD_OSN_OTGR_DC == value) return;
            Entity.DD_OSN_OTGR_DC = value;
            RaisePropertyChanged();
        }
    }

    public string DD_SCHET
    {
        get => Entity.DD_SCHET;
        set
        {
            if (Entity.DD_SCHET == value) return;
            Entity.DD_SCHET = value;
            RaisePropertyChanged();
        }
    }

    public string DD_DOVERENNOST
    {
        get => Entity.DD_DOVERENNOST;
        set
        {
            if (Entity.DD_DOVERENNOST == value) return;
            Entity.DD_DOVERENNOST = value;
            RaisePropertyChanged();
        }
    }

    public int? DD_NOSZATR_ID
    {
        get => Entity.DD_NOSZATR_ID;
        set
        {
            if (Entity.DD_NOSZATR_ID == value) return;
            Entity.DD_NOSZATR_ID = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DD_NOSZATR_DC
    {
        get => Entity.DD_NOSZATR_DC;
        set
        {
            if (Entity.DD_NOSZATR_DC == value) return;
            Entity.DD_NOSZATR_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DD_DOGOVOR_POKUPKI_DC
    {
        get => Entity.DD_DOGOVOR_POKUPKI_DC;
        set
        {
            if (Entity.DD_DOGOVOR_POKUPKI_DC == value) return;
            Entity.DD_DOGOVOR_POKUPKI_DC = value;
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

    public override string Note
    {
        get => Entity.DD_NOTES;
        set
        {
            if (Entity.DD_NOTES == value) return;
            Entity.DD_NOTES = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DD_KONTR_CRS_DC
    {
        get => Entity.DD_KONTR_CRS_DC;
        set
        {
            if (Entity.DD_KONTR_CRS_DC == value) return;
            Entity.DD_KONTR_CRS_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DD_KONTR_OTPR_DC
    {
        get => Entity.DD_KONTR_OTPR_DC;
        set
        {
            if (Entity.DD_KONTR_OTPR_DC == value) return;
            Entity.DD_KONTR_OTPR_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DD_KONTR_POL_DC
    {
        get => Entity.DD_KONTR_POL_DC;
        set
        {
            if (Entity.DD_KONTR_POL_DC == value) return;
            Entity.DD_KONTR_POL_DC = value;
            RaisePropertyChanged();
        }
    }

    public double? DD_KONTR_CRS_RATE
    {
        get => Entity.DD_KONTR_CRS_RATE;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.DD_KONTR_CRS_RATE == value) return;
            Entity.DD_KONTR_CRS_RATE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DD_UCHET_VALUTA_DC
    {
        get => Entity.DD_UCHET_VALUTA_DC;
        set
        {
            if (Entity.DD_UCHET_VALUTA_DC == value) return;
            Entity.DD_UCHET_VALUTA_DC = value;
            RaisePropertyChanged();
        }
    }

    public double? DD_UCHET_VALUTA_RATE
    {
        get => Entity.DD_UCHET_VALUTA_RATE;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.DD_UCHET_VALUTA_RATE == value) return;
            Entity.DD_UCHET_VALUTA_RATE = value;
            RaisePropertyChanged();
        }
    }

    public InvoiceProvider InvoiceProvider
    {
        get => myInvoiceProvider;
        set
        {
            if (myInvoiceProvider?.DocCode == value?.DocCode) return;
            myInvoiceProvider = value;
            Entity.DD_SPOST_DC = value?.DocCode;
            RaisePropertyChanged();
        }
    }

    public InvoiceClient InvoiceClient
    {
        get => myInvoiceClient;
        set
        {
            if (myInvoiceClient?.DocCode == value?.DocCode) return;
            myInvoiceClient = value;
            Entity.DD_SFACT_DC = value?.DocCode;
            RaisePropertyChanged();
        }
    }

    public bool IsVozvrat
    {
        get => Entity.DD_VOZVRAT == 1;
        set
        {
            if (Entity.DD_VOZVRAT == (short?)(value ? 1 : 0)) return;
            Entity.DD_VOZVRAT = (short?)(value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    public short? DD_OTPRAV_TYPE
    {
        get => Entity.DD_OTPRAV_TYPE;
        set
        {
            if (Entity.DD_OTPRAV_TYPE == value) return;
            Entity.DD_OTPRAV_TYPE = value;
            RaisePropertyChanged();
        }
    }

    public short? DD_POLUCH_TYPE
    {
        get => Entity.DD_POLUCH_TYPE;
        set
        {
            if (Entity.DD_POLUCH_TYPE == value) return;
            Entity.DD_POLUCH_TYPE = value;
            RaisePropertyChanged();
        }
    }

    public short? DD_LISTOV_SERVIFICATOV
    {
        get => Entity.DD_LISTOV_SERVIFICATOV;
        set
        {
            if (Entity.DD_LISTOV_SERVIFICATOV == value) return;
            Entity.DD_LISTOV_SERVIFICATOV = value;
            RaisePropertyChanged();
        }
    }

    public short? DD_VIEZD_FLAG
    {
        get => Entity.DD_VIEZD_FLAG;
        set
        {
            if (Entity.DD_VIEZD_FLAG == value) return;
            Entity.DD_VIEZD_FLAG = value;
            RaisePropertyChanged();
        }
    }

    public string DD_VIEZD_MASHINE
    {
        get => Entity.DD_VIEZD_MASHINE;
        set
        {
            if (Entity.DD_VIEZD_MASHINE == value) return;
            Entity.DD_VIEZD_MASHINE = value;
            RaisePropertyChanged();
        }
    }

    public string DD_VIEZD_CREATOR
    {
        get => Entity.DD_VIEZD_CREATOR;
        set
        {
            if (Entity.DD_VIEZD_CREATOR == value) return;
            Entity.DD_VIEZD_CREATOR = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? DD_VIEZD_DATE
    {
        get => Entity.DD_VIEZD_DATE;
        set
        {
            if (Entity.DD_VIEZD_DATE == value) return;
            Entity.DD_VIEZD_DATE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DD_KONTR_POL_FILIAL_DC
    {
        get => Entity.DD_KONTR_POL_FILIAL_DC;
        set
        {
            if (Entity.DD_KONTR_POL_FILIAL_DC == value) return;
            Entity.DD_KONTR_POL_FILIAL_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? DD_KONTR_POL_FILIAL_CODE
    {
        get => Entity.DD_KONTR_POL_FILIAL_CODE;
        set
        {
            if (Entity.DD_KONTR_POL_FILIAL_CODE == value) return;
            Entity.DD_KONTR_POL_FILIAL_CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DD_PROZV_PROCESS_DC
    {
        get => Entity.DD_PROZV_PROCESS_DC;
        set
        {
            if (Entity.DD_PROZV_PROCESS_DC == value) return;
            Entity.DD_PROZV_PROCESS_DC = value;
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

    public string OWNER_ID
    {
        get => Entity.OWNER_ID;
        set
        {
            if (Entity.OWNER_ID == value) return;
            Entity.OWNER_ID = value;
            RaisePropertyChanged();
        }
    }

    public string OWNER_TEXT
    {
        get => Entity.OWNER_TEXT;
        set
        {
            if (Entity.OWNER_TEXT == value) return;
            Entity.OWNER_TEXT = value;
            RaisePropertyChanged();
        }
    }

    public string CONSIGNEE_ID
    {
        get => Entity.CONSIGNEE_ID;
        set
        {
            if (Entity.CONSIGNEE_ID == value) return;
            Entity.CONSIGNEE_ID = value;
            RaisePropertyChanged();
        }
    }

    public string CONSIGNEE_TEXT
    {
        get => Entity.CONSIGNEE_TEXT;
        set
        {
            if (Entity.CONSIGNEE_TEXT == value) return;
            Entity.CONSIGNEE_TEXT = value;
            RaisePropertyChanged();
        }
    }

    public string BUYER_ID
    {
        get => Entity.BUYER_ID;
        set
        {
            if (Entity.BUYER_ID == value) return;
            Entity.BUYER_ID = value;
            RaisePropertyChanged();
        }
    }

    public string BUYER_TEXT
    {
        get => Entity.BUYER_TEXT;
        set
        {
            if (Entity.BUYER_TEXT == value) return;
            Entity.BUYER_TEXT = value;
            RaisePropertyChanged();
        }
    }

    public string SHIPMENT_ID
    {
        get => Entity.SHIPMENT_ID;
        set
        {
            if (Entity.SHIPMENT_ID == value) return;
            Entity.SHIPMENT_ID = value;
            RaisePropertyChanged();
        }
    }

    public string SHIPMENT_TEXT
    {
        get => Entity.SHIPMENT_TEXT;
        set
        {
            if (Entity.SHIPMENT_TEXT == value) return;
            Entity.SHIPMENT_TEXT = value;
            RaisePropertyChanged();
        }
    }

    public string SUPPLIER_ID
    {
        get => Entity.SUPPLIER_ID;
        set
        {
            if (Entity.SUPPLIER_ID == value) return;
            Entity.SUPPLIER_ID = value;
            RaisePropertyChanged();
        }
    }

    public string SUPPLIER_TEXT
    {
        get => Entity.SUPPLIER_TEXT;
        set
        {
            if (Entity.SUPPLIER_TEXT == value) return;
            Entity.SUPPLIER_TEXT = value;
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


    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public SD_24 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_24 DefaultValue()
    {
        return new SD_24
        {
            DOC_CODE = -1,
            Id = Guid.NewGuid()
        };
    }

    public List<SD_24> LoadList()
    {
        throw new NotImplementedException();
    }

    public void LoadReferences()
    {
        if (Entity.SD_201 != null)
            DocumentType = new MaterialDocumentType(Entity.SD_201);
    }

    public virtual void Save(SD_24 doc)
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

    public void UpdateFrom(SD_24 ent)
    {
    }

    public
        void UpdateTo
        (SD_24
            ent)
    {
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public SD_24 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public SD_24 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual SD_24 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual SD_24 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    #region Fields

    private MaterialDocumentType myDocumentType;
    private SD_24 myEntity;
    private InvoiceClient myInvoiceClient;
    private InvoiceProvider myInvoiceProvider;
    private Period myPeriod;

    #endregion

    #region Constructors

    public SD_24ViewModel()
    {
        Entity = DefaultValue();
    }

    public SD_24ViewModel(SD_24 entity)
    {
        Entity = entity ?? DefaultValue();
        LoadReferences();
    }

    #endregion
}
