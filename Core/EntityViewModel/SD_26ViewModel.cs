using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Data;
using DevExpress.Mvvm.DataAnnotations;

// ReSharper disable InconsistentNaming

namespace Core.EntityViewModel
{
    /// <summary>
    ///     Счет-фактура от поставщика
    /// </summary>
    [MetadataType(typeof(SD_26LayoutData_FluentAPI))]
    public class SD_26ViewModel : RSViewModelBase, IEntity<SD_26>
    {
#pragma warning disable 169
        private CentrOfResponsibility myCO;
#pragma warning restore 169

        private EmployeeViewModel myEmployee;
        private SD_26 myEntity;

#pragma warning disable 169
        private SD_189ViewModel myFormRaschet;
#pragma warning restore 169


        // ReSharper disable once NotAccessedField.Local
        private Kontragent myKontragent;


        private Kontragent myKontrReceiver;


#pragma warning disable 169
        private PayCondition myPayCondition;
#pragma warning restore 169

#pragma warning disable 169
        private VzaimoraschetType myVzaimoraschetType;
#pragma warning restore 169

        public SD_26ViewModel()
        {
            Entity = DefaultValue();
        }

        public SD_26ViewModel(SD_26 entity)
        {
            Entity = entity ?? DefaultValue();
        }

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

        public Kontragent Kontragent
        {
            get => MainReferences.AllKontragents.ContainsKey(Entity.SF_POST_DC)
                ? MainReferences.AllKontragents[Entity.SF_POST_DC]
                : null;
            set
            {
                if (Entity.SF_POST_DC == value.DOC_CODE) return;
                Entity.SF_POST_DC = value.DOC_CODE;
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
                if (Entity.SF_CRS_DC != null && MainReferences.Currencies.ContainsKey(Entity.SF_CRS_DC.Value))
                    Currency = MainReferences.Currencies[Entity.SF_CRS_DC.Value];
                RaisePropertyChanged(nameof(Currency));
                RaisePropertyChanged();
            }
        }


        public Currency Currency
        {
            // ReSharper disable once PossibleInvalidOperationException
            get => !MainReferences.Currencies.ContainsKey(Entity.SF_CRS_DC.Value)
                ? null
                : MainReferences.Currencies[Entity.SF_CRS_DC.Value];
            set
            {
                if (Entity.SF_CRS_DC == value.DOC_CODE) return;
                Entity.SF_CRS_DC = value.DOC_CODE;
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
                RaisePropertyChanged();
            }
        }

        public PayCondition PayCondition
        {
            get
            {
                if (MainReferences.PayConditions.ContainsKey(Entity.SF_PAY_COND_DC))
                    return MainReferences.PayConditions[Entity.SF_PAY_COND_DC];
                return null;
            }
            set
            {
                if (Entity.SF_PAY_COND_DC == value?.DOC_CODE) return;
                if (value != null) Entity.SF_PAY_COND_DC = value.DOC_CODE;
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

        public EmployeeViewModel Employee
        {
            get => myEmployee;
            set
            {
                if (myEmployee == value) return;
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
                if (Entity.SF_ACCEPTED == (value ? 1 : 0)) return;
                Entity.SF_ACCEPTED = (short) (value ? 1 : 0);
                RaisePropertyChanged(nameof(SF_ACCEPTED));
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
                RaisePropertyChanged();
            }
        }

        public VzaimoraschetType VzaimoraschetType
        {
            get
            {
                if (Entity.SF_VZAIMOR_TYPE_DC == null) return null;
                if (MainReferences.VzaimoraschetTypes.ContainsKey(Entity.SF_VZAIMOR_TYPE_DC.Value))
                    return MainReferences.VzaimoraschetTypes[Entity.SF_VZAIMOR_TYPE_DC.Value];
                return null;
            }
            set
            {
                if (value == null)
                {
                    Entity.SF_VZAIMOR_TYPE_DC = null;
                }
                else
                {
                    if (Entity.SF_VZAIMOR_TYPE_DC == value.DOC_CODE) return;
                    Entity.SF_VZAIMOR_TYPE_DC = value.DOC_CODE;
                }
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
                RaisePropertyChanged();
            }
        }

        public SD_189ViewModel FormRaschet
        {
            get
            {
                if (Entity.SF_FORM_RASCH_DC == null) return null;
                if (MainReferences.FormRaschets.ContainsKey(Entity.SF_FORM_RASCH_DC.Value))
                    return MainReferences.FormRaschets[Entity.SF_FORM_RASCH_DC.Value];
                return null;
            }
            set
            {
                if (value == null)
                {
                    Entity.SF_FORM_RASCH_DC = null;
                }
                else
                {
                    if (Entity.SF_FORM_RASCH_DC == value.DOC_CODE) return;
                    Entity.SF_FORM_RASCH_DC = value.DOC_CODE;
                }
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
            }
        }

        public CentrOfResponsibility CO
        {
            get
            {
                if (Entity.SF_CENTR_OTV_DC == null) return null;
                if (MainReferences.COList.ContainsKey(Entity.SF_CENTR_OTV_DC.Value))
                    return MainReferences.COList[Entity.SF_CENTR_OTV_DC.Value];
                return null;
            }
            set
            {
                if (value == null)
                {
                    Entity.SF_CENTR_OTV_DC = null;
                }
                else
                {
                    if (Entity.SF_CENTR_OTV_DC == value.DOC_CODE) return;
                    Entity.SF_CENTR_OTV_DC = value.DOC_CODE;
                }
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
                if (myKontrReceiver == value) return;
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

        public ObservableCollection<TD_26ViewModel> Rows { set; get; }
        public string InvoiceInfo => $"{Kontragent} {Currency} {Note}";

        public EntityLoadCodition LoadCondition { get; set; }

        public List<SD_26> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        private string SFact(decimal dc)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var doc = ctx.SD_84.FirstOrDefault(_ => _.DOC_CODE == dc);
                return doc == null ? null : $"№ {doc.SF_IN_NUM}/{doc.SF_OUT_NUM} от {doc.SF_DATE}  {doc.SF_NOTE}";
            }
        }

        public virtual void Save(SD_26 doc)
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

        public void UpdateFrom(SD_26 ent)
        {
            SF_OPRIHOD_DATE = ent.SF_OPRIHOD_DATE;
            SF_POSTAV_NUM = ent.SF_POSTAV_NUM;
            SF_POSTAV_DATE = ent.SF_POSTAV_DATE;
            SF_POST_DC = ent.SF_POST_DC;
            SF_RUB_SUMMA = ent.SF_RUB_SUMMA;
            SF_CRS_SUMMA = ent.SF_CRS_SUMMA;
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
            SF_KONTR_CRS_SUMMA = ent.SF_KONTR_CRS_SUMMA;
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
    }

    public class SD_26LayoutData_FluentAPI : DataAnnotationForFluentApiBase, IMetadataProvider<SD_26ViewModel>
    {
        void IMetadataProvider<SD_26ViewModel>.BuildMetadata(MetadataBuilder<SD_26ViewModel> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(x => x.Employee).NotAutoGenerated();
            builder.Property(x => x.KontrReceiver).NotAutoGenerated();
            builder.Property(x => x.Currency).AutoGenerated()
                .DisplayName("Валюта");
            builder.Property(x => x.SF_CRS_SUMMA).AutoGenerated()
                .DisplayName("Сумма");
            builder.Property(x => x.SF_POSTAV_DATE).AutoGenerated()
                .DisplayName("Дата");
            builder.Property(x => x.SF_POSTAV_NUM).AutoGenerated()
                .DisplayName("Внешний №");
            builder.Property(x => x.SF_IN_NUM).AutoGenerated()
                .DisplayName("№");
            builder.Property(x => x.SF_POSTAV_DATE).AutoGenerated()
                .DisplayName("Дата");
            builder.Property(x => x.Kontragent).AutoGenerated()
                .DisplayName("Контрагент");
            builder.Property(x => x.CO).AutoGenerated()
                .DisplayName("Центр ответственности");
            builder.Property(x => x.VzaimoraschetType).AutoGenerated()
                .DisplayName("Тип");
            builder.Property(x => x.PayCondition).AutoGenerated()
                .DisplayName("Условие оплаы");
            builder.Property(x => x.FormRaschet).AutoGenerated()
                .DisplayName("Форма расчета");
        }
    }
}