using System;
using System.Collections.Generic;
using System.Linq;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.EntityViewModel.Invoices;
using Core.EntityViewModel.Periods;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.NomenklManagement
{
    public class SD_24ViewModel : RSViewModelBase, IEntity<SD_24>
    {
        private MaterialDocumentType myDocumentType;
        private SD_24 myEntity;
        private Warehouse myInFromWarehouse;
        private InvoiceClient myInvoiceClient;
        private InvoiceProvider myInvoiceProvider;
        private Employee.Employee myKladovshik;
        private Kontragent myKontragentReceiver;
        private Kontragent myKontragentSender;
        private Warehouse myOutOnWarehouse;
        private Period myPeriod;
        private Warehouse myWarehouseIn;
        private Warehouse myWarehouseOut;

        public SD_24ViewModel()
        {
            Entity = DefaultValue();
        }

        public SD_24ViewModel(SD_24 entity)
        {
            Entity = entity ?? DefaultValue();
            LoadReferences();
            DD_OTRPAV_NAME = Sender;
            DD_POLUCH_NAME = Receiver;
        }

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

        public decimal DD_TYPE_DC
        {
            get => Entity.DD_TYPE_DC;
            set
            {
                if (Entity.DD_TYPE_DC == value) return;
                Entity.DD_TYPE_DC = value;
                RaisePropertyChanged();
            }
        }

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

        public DateTime DD_DATE
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

        public decimal? DD_SKLAD_OTPR_DC
        {
            get => Entity.DD_SKLAD_OTPR_DC;
            set
            {
                if (Entity.DD_SKLAD_OTPR_DC == value) return;
                Entity.DD_SKLAD_OTPR_DC = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Sender));
            }
        }

        /// <summary>
        ///     Склад отправитель
        /// </summary>
        public Warehouse WarehouseOut
        {
            get => myWarehouseOut;
            set
            {
                if (myWarehouseOut != null && myWarehouseOut.Equals(value)) return;
                myWarehouseOut = value;
                DD_SKLAD_OTPR_DC = myWarehouseOut?.DocCode;
                DD_OTRPAV_NAME = myWarehouseOut?.Name;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Sender));
            }
        }

        public decimal? DD_SKLAD_POL_DC
        {
            get => Entity.DD_SKLAD_POL_DC;
            set
            {
                if (Entity.DD_SKLAD_POL_DC == value) return;
                Entity.DD_SKLAD_POL_DC = value;
                if (Entity.DD_SKLAD_POL_DC != null)
                    myWarehouseIn = MainReferences.Warehouses[Entity.DD_SKLAD_POL_DC.Value];
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(WarehouseIn));
            }
        }

        /// <summary>
        ///     Склад получатель
        /// </summary>
        public Warehouse WarehouseIn
        {
            get => myWarehouseIn;
            set
            {
                if (myWarehouseIn != null && myWarehouseIn.Equals(value)) return;
                myWarehouseIn = value;
                DD_SKLAD_POL_DC = myWarehouseIn?.DocCode;
                DD_POLUCH_NAME = myWarehouseIn?.Name;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Receiver));
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

        /// <summary>
        ///     Контрагент отправитель
        /// </summary>
        public Kontragent KontragentSender
        {
            get => myKontragentSender;
            set
            {
                if (myKontragentSender != null && myKontragentSender.Equals(value)) return;
                myKontragentSender = value;
                if (myKontragentSender != null)
                {
                    DD_KONTR_OTPR_DC = myKontragentSender.DocCode;
                    DD_OTRPAV_NAME = myKontragentSender.Name;
                }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Sender));
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

        /// <summary>
        ///     Контрагент получатель
        /// </summary>
        public Kontragent KontragentReceiver
        {
            get => myKontragentReceiver;
            set
            {
                if (myKontragentReceiver != null && myKontragentReceiver.Equals(value)) return;
                myKontragentReceiver = value;
                if (myKontragentReceiver != null)
                {
                    DD_KONTR_POL_DC = myKontragentReceiver.DocCode;
                    DD_POLUCH_NAME = myKontragentReceiver.Name;
                }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Sender));
            }
        }

        public int? DD_KLADOV_TN
        {
            get => Entity.DD_KLADOV_TN;
            set
            {
                if (Entity.DD_KLADOV_TN == value) return;
                Entity.DD_KLADOV_TN = value;
                RaisePropertyChanged();
            }
        }

        public Employee.Employee Kladovshik
        {
            get => myKladovshik;
            set
            {
                if (myKladovshik != null && myKladovshik.Equals(value)) return;
                myKladovshik = value;
                Entity.DD_KLADOV_TN = myKladovshik?.TabelNumber;
                RaisePropertyChanged();
            }
        }

        public short DD_EXECUTED
        {
            get => Entity.DD_EXECUTED;
            set
            {
                if (Entity.DD_EXECUTED == value) return;
                Entity.DD_EXECUTED = value;
                RaisePropertyChanged();
            }
        }

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

        public string DD_OTRPAV_NAME
        {
            get => Entity.DD_OTRPAV_NAME;
            set
            {
                if (Entity.DD_OTRPAV_NAME == value) return;
                Entity.DD_OTRPAV_NAME = value;
                RaisePropertyChanged();
            }
        }

        public string DD_POLUCH_NAME
        {
            get => Entity.DD_POLUCH_NAME;
            set
            {
                if (Entity.DD_POLUCH_NAME == value) return;
                Entity.DD_POLUCH_NAME = value;
                RaisePropertyChanged();
            }
        }

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

        public decimal? DD_OTPUSK_NA_SKLAD_DC
        {
            get => Entity.DD_OTPUSK_NA_SKLAD_DC;
            set
            {
                if (Entity.DD_OTPUSK_NA_SKLAD_DC == value) return;
                Entity.DD_OTPUSK_NA_SKLAD_DC = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Отпуск на склад
        /// </summary>
        public Warehouse OutOnWarehouse
        {
            get => myOutOnWarehouse;
            set
            {
                if (myOutOnWarehouse != null && myOutOnWarehouse.Equals(value)) return;
                myOutOnWarehouse = value;
                if (myOutOnWarehouse != null)
                    DD_OTPUSK_NA_SKLAD_DC = myOutOnWarehouse.DocCode;
                RaisePropertyChanged();
            }
        }

        public decimal? DD_PRIHOD_SO_SKLADA_DC
        {
            get => Entity.DD_PRIHOD_SO_SKLADA_DC;
            set
            {
                if (Entity.DD_PRIHOD_SO_SKLADA_DC == value) return;
                Entity.DD_PRIHOD_SO_SKLADA_DC = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Приход со склада
        /// </summary>
        public Warehouse InFromWarehouse
        {
            get => myInFromWarehouse;
            set
            {
                if (myInFromWarehouse != null && myInFromWarehouse.Equals(value)) return;
                myInFromWarehouse = value;
                if (myInFromWarehouse != null)
                    DD_PRIHOD_SO_SKLADA_DC = myInFromWarehouse.DocCode;
                RaisePropertyChanged();
            }
        }

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

        public decimal? DD_PERIOD_DC
        {
            get => Entity.DD_PERIOD_DC;
            set
            {
                if (Entity.DD_PERIOD_DC == value) return;
                Entity.DD_PERIOD_DC = value;
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

        public string DD_NOTES
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

        public double? DD_KONTR_CRS_RATE
        {
            get => Entity.DD_KONTR_CRS_RATE;
            set
            {
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
                if (Entity.DD_UCHET_VALUTA_RATE == value) return;
                Entity.DD_UCHET_VALUTA_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? DD_SPOST_DC
        {
            get => Entity.DD_SPOST_DC;
            set
            {
                if (Entity.DD_SPOST_DC == value) return;
                Entity.DD_SPOST_DC = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceProvider InvoiceProvider
        {
            get => myInvoiceProvider;
            set
            {
                if (myInvoiceProvider != null && myInvoiceProvider.Equals(value)) return;
                myInvoiceProvider = value;
                if (myInvoiceProvider != null)
                    Entity.DD_SPOST_DC = myInvoiceProvider.DOC_CODE;
                RaisePropertyChanged();
            }
        }

        public decimal? DD_SFACT_DC
        {
            get => Entity.DD_SFACT_DC;
            set
            {
                if (Entity.DD_SFACT_DC == value) return;
                Entity.DD_SFACT_DC = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceClient InvoiceClient
        {
            get => myInvoiceClient;
            set
            {
                if (myInvoiceClient != null && myInvoiceClient.Equals(value)) return;
                myInvoiceClient = value;
                if (myInvoiceClient != null && myInvoiceClient.DocCode > 0)
                    DD_SFACT_DC = myInvoiceClient.DocCode;
                RaisePropertyChanged();
            }
        }

        public short? DD_VOZVRAT
        {
            get => Entity.DD_VOZVRAT;
            set
            {
                if (Entity.DD_VOZVRAT == value) return;
                Entity.DD_VOZVRAT = value;
                RaisePropertyChanged();
            }
        }

        public bool IsVozvrat
        {
            get => DD_VOZVRAT == 1;
            set
            {
                if (DD_VOZVRAT == (short?) (value ? 1 : 0)) return;
                DD_VOZVRAT = (short?) (value ? 1 : 0);
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

        public SD_131 SD_131
        {
            get => Entity.SD_131;
            set
            {
                if (Entity.SD_131 == value) return;
                Entity.SD_131 = value;
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

        public SD_2 SD_2
        {
            get => Entity.SD_2;
            set
            {
                if (Entity.SD_2 == value) return;
                Entity.SD_2 = value;
                RaisePropertyChanged();
            }
        }

        public SD_201 SD_201
        {
            get => Entity.SD_201;
            set
            {
                if (Entity.SD_201 == value) return;
                Entity.SD_201 = value;
                RaisePropertyChanged();
            }
        }

        public SD_9 SD_9
        {
            get => Entity.SD_9;
            set
            {
                if (Entity.SD_9 == value) return;
                Entity.SD_9 = value;
                RaisePropertyChanged();
            }
        }

        public SD_84 SD_84
        {
            get => Entity.SD_84;
            set
            {
                if (Entity.SD_84 == value) return;
                Entity.SD_84 = value;
                RaisePropertyChanged();
            }
        }

        public SD_26 SD_26
        {
            get => Entity.SD_26;
            set
            {
                if (Entity.SD_26 == value) return;
                Entity.SD_26 = value;
                RaisePropertyChanged();
            }
        }

        public SD_24 SD_242
        {
            get => Entity.SD_242;
            set
            {
                if (Entity.SD_242 == value) return;
                Entity.SD_242 = value;
                RaisePropertyChanged();
            }
        }

        public SD_257 SD_257
        {
            get => Entity.SD_257;
            set
            {
                if (Entity.SD_257 == value) return;
                Entity.SD_257 = value;
                RaisePropertyChanged();
            }
        }

        public SD_432 SD_432
        {
            get => Entity.SD_432;
            set
            {
                if (Entity.SD_432 == value) return;
                Entity.SD_432 = value;
                RaisePropertyChanged();
            }
        }

        public XD_43 XD_43
        {
            get => Entity.XD_43;
            set
            {
                if (Entity.XD_43 == value) return;
                Entity.XD_43 = value;
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

        public SD_27 SD_27
        {
            get => Entity.SD_27;
            set
            {
                if (Entity.SD_27 == value) return;
                Entity.SD_27 = value;
                RaisePropertyChanged();
            }
        }

        public SD_27 SD_271
        {
            get => Entity.SD_271;
            set
            {
                if (Entity.SD_271 == value) return;
                Entity.SD_271 = value;
                RaisePropertyChanged();
            }
        }

        public SD_27 SD_272
        {
            get => Entity.SD_272;
            set
            {
                if (Entity.SD_272 == value) return;
                Entity.SD_272 = value;
                RaisePropertyChanged();
            }
        }

        public SD_27 SD_273
        {
            get => Entity.SD_273;
            set
            {
                if (Entity.SD_273 == value) return;
                Entity.SD_273 = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

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

        public List<SD_24> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public void LoadReferences()
        {
            if (Entity.SD_201 != null)
                DocumentType = new MaterialDocumentType(Entity.SD_201);
            if (DD_SKLAD_POL_DC != null && MainReferences.Warehouses.ContainsKey(DD_SKLAD_POL_DC.Value))
                WarehouseIn = MainReferences.Warehouses[DD_SKLAD_POL_DC.Value];
            if (DD_SKLAD_OTPR_DC != null && MainReferences.Warehouses.ContainsKey(DD_SKLAD_OTPR_DC.Value))
                WarehouseOut = MainReferences.Warehouses[DD_SKLAD_OTPR_DC.Value];
            if (DD_KONTR_OTPR_DC != null)
                KontragentSender = MainReferences.GetKontragent(DD_KONTR_OTPR_DC);
            if (DD_KONTR_POL_DC != null)
                KontragentReceiver = MainReferences.GetKontragent(DD_KONTR_POL_DC);
            if (DD_OTPUSK_NA_SKLAD_DC != null && MainReferences.Warehouses.ContainsKey(DD_OTPUSK_NA_SKLAD_DC.Value))
                WarehouseIn = MainReferences.Warehouses[DD_OTPUSK_NA_SKLAD_DC.Value];
            if (Entity.DD_KLADOV_TN != null)
                Kladovshik =
                    MainReferences.Employees.Values.FirstOrDefault(_ => _.TabelNumber == Entity.DD_KLADOV_TN.Value);
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
            DD_TYPE_DC = ent.DD_TYPE_DC;
            DD_DATE = ent.DD_DATE;
            DD_IN_NUM = ent.DD_IN_NUM;
            DD_EXT_NUM = ent.DD_EXT_NUM;
            DD_SKLAD_OTPR_DC = ent.DD_SKLAD_OTPR_DC;
            DD_SKLAD_POL_DC = ent.DD_SKLAD_POL_DC;
            DD_KONTR_OTPR_DC = ent.DD_KONTR_OTPR_DC;
            DD_KONTR_POL_DC = ent.DD_KONTR_POL_DC;
            DD_KLADOV_TN = ent.DD_KLADOV_TN;
            DD_EXECUTED = ent.DD_EXECUTED;
            DD_POLUCHATEL_TN = ent.DD_POLUCHATEL_TN;
            DD_OTRPAV_NAME = ent.DD_OTRPAV_NAME;
            DD_POLUCH_NAME = ent.DD_POLUCH_NAME;
            DD_KTO_SDAL_TN = ent.DD_KTO_SDAL_TN;
            DD_KOMU_PEREDANO = ent.DD_KOMU_PEREDANO;
            DD_OT_KOGO_POLUCHENO = ent.DD_OT_KOGO_POLUCHENO;
            DD_GRUZOOTPRAVITEL = ent.DD_GRUZOOTPRAVITEL;
            DD_GRUZOPOLUCHATEL = ent.DD_GRUZOPOLUCHATEL;
            DD_OTPUSK_NA_SKLAD_DC = ent.DD_OTPUSK_NA_SKLAD_DC;
            DD_PRIHOD_SO_SKLADA_DC = ent.DD_PRIHOD_SO_SKLADA_DC;
            DD_SHABLON = ent.DD_SHABLON;
            DD_VED_VIDACH_DC = ent.DD_VED_VIDACH_DC;
            DD_PERIOD_DC = ent.DD_PERIOD_DC;
            DD_TREB_NUM = ent.DD_TREB_NUM;
            DD_TREB_DATE = ent.DD_TREB_DATE;
            DD_TREB_DC = ent.DD_TREB_DC;
            CREATOR = ent.CREATOR;
            DD_PODTVERZHDEN = ent.DD_PODTVERZHDEN;
            DD_OSN_OTGR_DC = ent.DD_OSN_OTGR_DC;
            DD_SCHET = ent.DD_SCHET;
            DD_DOVERENNOST = ent.DD_DOVERENNOST;
            DD_NOSZATR_ID = ent.DD_NOSZATR_ID;
            DD_NOSZATR_DC = ent.DD_NOSZATR_DC;
            DD_DOGOVOR_POKUPKI_DC = ent.DD_DOGOVOR_POKUPKI_DC;
            DD_NOTES = ent.DD_NOTES;
            DD_KONTR_CRS_DC = ent.DD_KONTR_CRS_DC;
            DD_KONTR_CRS_RATE = ent.DD_KONTR_CRS_RATE;
            DD_UCHET_VALUTA_DC = ent.DD_UCHET_VALUTA_DC;
            DD_UCHET_VALUTA_RATE = ent.DD_UCHET_VALUTA_RATE;
            DD_SPOST_DC = ent.DD_SPOST_DC;
            DD_SFACT_DC = ent.DD_SFACT_DC;
            DD_VOZVRAT = ent.DD_VOZVRAT;
            DD_OTPRAV_TYPE = ent.DD_OTPRAV_TYPE;
            DD_POLUCH_TYPE = ent.DD_POLUCH_TYPE;
            DD_LISTOV_SERVIFICATOV = ent.DD_LISTOV_SERVIFICATOV;
            DD_VIEZD_FLAG = ent.DD_VIEZD_FLAG;
            DD_VIEZD_MASHINE = ent.DD_VIEZD_MASHINE;
            DD_VIEZD_CREATOR = ent.DD_VIEZD_CREATOR;
            DD_VIEZD_DATE = ent.DD_VIEZD_DATE;
            DD_KONTR_POL_FILIAL_DC = ent.DD_KONTR_POL_FILIAL_DC;
            DD_KONTR_POL_FILIAL_CODE = ent.DD_KONTR_POL_FILIAL_CODE;
            DD_PROZV_PROCESS_DC = ent.DD_PROZV_PROCESS_DC;
            TSTAMP = ent.TSTAMP;
            OWNER_ID = ent.OWNER_ID;
            OWNER_TEXT = ent.OWNER_TEXT;
            CONSIGNEE_ID = ent.CONSIGNEE_ID;
            CONSIGNEE_TEXT = ent.CONSIGNEE_TEXT;
            BUYER_ID = ent.BUYER_ID;
            BUYER_TEXT = ent.BUYER_TEXT;
            SHIPMENT_ID = ent.SHIPMENT_ID;
            SHIPMENT_TEXT = ent.SHIPMENT_TEXT;
            SUPPLIER_ID = ent.SUPPLIER_ID;
            SUPPLIER_TEXT = ent.SUPPLIER_TEXT;
            GRUZO_INFO_ID = ent.GRUZO_INFO_ID;
            GROZO_REQUISITE = ent.GROZO_REQUISITE;
            SD_112 = ent.SD_112;
            SD_131 = ent.SD_131;
            SD_189 = ent.SD_189;
            SD_2 = ent.SD_2;
            SD_201 = ent.SD_201;
            SD_9 = ent.SD_9;
            SD_84 = ent.SD_84;
            SD_26 = ent.SD_26;
            SD_242 = ent.SD_242;
            SD_257 = ent.SD_257;
            SD_432 = ent.SD_432;
            XD_43 = ent.XD_43;
            SD_301 = ent.SD_301;
            SD_3011 = ent.SD_3011;
            SD_43 = ent.SD_43;
            SD_433 = ent.SD_433;
            SD_27 = ent.SD_27;
            SD_271 = ent.SD_271;
            SD_272 = ent.SD_272;
            SD_273 = ent.SD_273;
        }

        public
            void UpdateTo
            (SD_24
                ent)
        {
            ent.DD_TYPE_DC = DD_TYPE_DC;
            ent.DD_DATE = DD_DATE;
            ent.DD_IN_NUM = DD_IN_NUM;
            ent.DD_EXT_NUM = DD_EXT_NUM;
            ent.DD_SKLAD_OTPR_DC = DD_SKLAD_OTPR_DC;
            ent.DD_SKLAD_POL_DC = DD_SKLAD_POL_DC;
            ent.DD_KONTR_OTPR_DC = DD_KONTR_OTPR_DC;
            ent.DD_KONTR_POL_DC = DD_KONTR_POL_DC;
            ent.DD_KLADOV_TN = DD_KLADOV_TN;
            ent.DD_EXECUTED = DD_EXECUTED;
            ent.DD_POLUCHATEL_TN = DD_POLUCHATEL_TN;
            ent.DD_OTRPAV_NAME = DD_OTRPAV_NAME;
            ent.DD_POLUCH_NAME = DD_POLUCH_NAME;
            ent.DD_KTO_SDAL_TN = DD_KTO_SDAL_TN;
            ent.DD_KOMU_PEREDANO = DD_KOMU_PEREDANO;
            ent.DD_OT_KOGO_POLUCHENO = DD_OT_KOGO_POLUCHENO;
            ent.DD_GRUZOOTPRAVITEL = DD_GRUZOOTPRAVITEL;
            ent.DD_GRUZOPOLUCHATEL = DD_GRUZOPOLUCHATEL;
            ent.DD_OTPUSK_NA_SKLAD_DC = DD_OTPUSK_NA_SKLAD_DC;
            ent.DD_PRIHOD_SO_SKLADA_DC = DD_PRIHOD_SO_SKLADA_DC;
            ent.DD_SHABLON = DD_SHABLON;
            ent.DD_VED_VIDACH_DC = DD_VED_VIDACH_DC;
            ent.DD_PERIOD_DC = DD_PERIOD_DC;
            ent.DD_TREB_NUM = DD_TREB_NUM;
            ent.DD_TREB_DATE = DD_TREB_DATE;
            ent.DD_TREB_DC = DD_TREB_DC;
            ent.CREATOR = CREATOR;
            ent.DD_PODTVERZHDEN = DD_PODTVERZHDEN;
            ent.DD_OSN_OTGR_DC = DD_OSN_OTGR_DC;
            ent.DD_SCHET = DD_SCHET;
            ent.DD_DOVERENNOST = DD_DOVERENNOST;
            ent.DD_NOSZATR_ID = DD_NOSZATR_ID;
            ent.DD_NOSZATR_DC = DD_NOSZATR_DC;
            ent.DD_DOGOVOR_POKUPKI_DC = DD_DOGOVOR_POKUPKI_DC;
            ent.DD_NOTES = DD_NOTES;
            ent.DD_KONTR_CRS_DC = DD_KONTR_CRS_DC;
            ent.DD_KONTR_CRS_RATE = DD_KONTR_CRS_RATE;
            ent.DD_UCHET_VALUTA_DC = DD_UCHET_VALUTA_DC;
            ent.DD_UCHET_VALUTA_RATE = DD_UCHET_VALUTA_RATE;
            ent.DD_SPOST_DC = DD_SPOST_DC;
            ent.DD_SFACT_DC = DD_SFACT_DC;
            ent.DD_VOZVRAT = DD_VOZVRAT;
            ent.DD_OTPRAV_TYPE = DD_OTPRAV_TYPE;
            ent.DD_POLUCH_TYPE = DD_POLUCH_TYPE;
            ent.DD_LISTOV_SERVIFICATOV = DD_LISTOV_SERVIFICATOV;
            ent.DD_VIEZD_FLAG = DD_VIEZD_FLAG;
            ent.DD_VIEZD_MASHINE = DD_VIEZD_MASHINE;
            ent.DD_VIEZD_CREATOR = DD_VIEZD_CREATOR;
            ent.DD_VIEZD_DATE = DD_VIEZD_DATE;
            ent.DD_KONTR_POL_FILIAL_DC = DD_KONTR_POL_FILIAL_DC;
            ent.DD_KONTR_POL_FILIAL_CODE = DD_KONTR_POL_FILIAL_CODE;
            ent.DD_PROZV_PROCESS_DC = DD_PROZV_PROCESS_DC;
            ent.TSTAMP = TSTAMP;
            ent.OWNER_ID = OWNER_ID;
            ent.OWNER_TEXT = OWNER_TEXT;
            ent.CONSIGNEE_ID = CONSIGNEE_ID;
            ent.CONSIGNEE_TEXT = CONSIGNEE_TEXT;
            ent.BUYER_ID = BUYER_ID;
            ent.BUYER_TEXT = BUYER_TEXT;
            ent.SHIPMENT_ID = SHIPMENT_ID;
            ent.SHIPMENT_TEXT = SHIPMENT_TEXT;
            ent.SUPPLIER_ID = SUPPLIER_ID;
            ent.SUPPLIER_TEXT = SUPPLIER_TEXT;
            ent.GRUZO_INFO_ID = GRUZO_INFO_ID;
            ent.GROZO_REQUISITE = GROZO_REQUISITE;
            ent.SD_112 = SD_112;
            ent.SD_131 = SD_131;
            ent.SD_189 = SD_189;
            ent.SD_2 = SD_2;
            ent.SD_201 = SD_201;
            ent.SD_9 = SD_9;
            ent.SD_84 = SD_84;
            ent.SD_26 = SD_26;
            ent.SD_242 = SD_242;
            ent.SD_257 = SD_257;
            ent.SD_432 = SD_432;
            ent.XD_43 = XD_43;
            ent.SD_301 = SD_301;
            ent.SD_3011 = SD_3011;
            ent.SD_43 = SD_43;
            ent.SD_433 = SD_433;
            ent.SD_27 = SD_27;
            ent.SD_271 = SD_271;
            ent.SD_272 = SD_272;
            ent.SD_273 = SD_273;
        }

        public SD_24 DefaultValue()
        {
            return new()
            {
                DOC_CODE = -1,
                Id = Guid.NewGuid()
            };
        }

        public SD_24 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

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
    }
}