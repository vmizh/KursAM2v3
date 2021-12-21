using System;
using System.Collections.Generic;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Dogovora.Old
{
    /// <summary>
    ///     Договор закупки у поставщика. Шапка
    /// </summary>
    public class ContractProvider : RSViewModelBase, IEntity<SD_112>
    {
        private ContractType myContractType;
        private Currency myCurrency;
        private DeliveryCondition myDeliverCondidition;
        private SD_112 myEntity;
        private Kontragent myKontragent;

        public ContractProvider()
        {
            Entity = DefaultValue();
        }

        public SD_112 DefaultValue()
        {
            return new SD_112 {DOC_CODE = -1};
        }

        public ContractProvider(SD_112 entity)
        {
            Entity = entity ?? DefaultValue();
        }

        public SD_112 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
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

        public decimal DO_TYPE_DC
        {
            get => Entity.DO_TYPE_DC;
            set
            {
                if (Entity.DO_TYPE_DC == value) return;
                Entity.DO_TYPE_DC = value;
                ContractType = MainReferences.ContractTypes.ContainsKey(Entity.DO_TYPE_DC)
                    ? MainReferences.ContractTypes[Entity.DO_TYPE_DC]
                    : null;
                RaisePropertyChanged(nameof(ContractType));
                RaisePropertyChanged();
            }
        }

        public ContractType ContractType
        {
            get => myContractType;
            set
            {
                if (myContractType != null && myContractType.Equals(value)) return;
                myContractType = value;
                if (myContractType != null)
                    DO_TYPE_DC = myContractType.DocCode;
                RaisePropertyChanged();
            }
        }

        public string DO_NUM
        {
            get => Entity.DO_NUM;
            set
            {
                if (Entity.DO_NUM == value) return;
                Entity.DO_NUM = value;
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

        public DateTime? DO_PODPIS_DATE
        {
            get => Entity.DO_PODPIS_DATE;
            set
            {
                if (Entity.DO_PODPIS_DATE == value) return;
                Entity.DO_PODPIS_DATE = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? DO_EXEC_DATE
        {
            get => Entity.DO_EXEC_DATE;
            set
            {
                if (Entity.DO_EXEC_DATE == value) return;
                Entity.DO_EXEC_DATE = value;
                RaisePropertyChanged();
            }
        }

        public short DO_PODPISAN
        {
            get => Entity.DO_PODPISAN;
            set
            {
                if (Entity.DO_PODPISAN == value) return;
                Entity.DO_PODPISAN = value;
                RaisePropertyChanged();
            }
        }

        public short DO_ZAKONCHEN
        {
            get => Entity.DO_ZAKONCHEN;
            set
            {
                if (Entity.DO_ZAKONCHEN == value) return;
                Entity.DO_ZAKONCHEN = value;
                RaisePropertyChanged();
            }
        }

        public decimal DO_SALER_DC
        {
            get => Entity.DO_SALER_DC;
            set
            {
                if (Entity.DO_SALER_DC == value) return;
                Entity.DO_SALER_DC = value;
                myKontragent = MainReferences.GetKontragent(Entity.DO_SALER_DC);
                RaisePropertyChanged(nameof(Kontragent));
                RaisePropertyChanged();
            }
        }

        public Kontragent Kontragent
        {
            get => myKontragent;
            set
            {
                if (myKontragent != null && myKontragent.Equals(value)) return;
                myKontragent = value;
                if (myKontragent != null)
                    DO_SALER_DC = myKontragent.DocCode;
                RaisePropertyChanged();
            }
        }

        public decimal DO_USL_POST_DC
        {
            get => Entity.DO_USL_POST_DC;
            set
            {
                if (Entity.DO_USL_POST_DC == value) return;
                Entity.DO_USL_POST_DC = value;
                if (MainReferences.DeliveryConditions.ContainsKey(Entity.DO_USL_POST_DC))
                    myDeliverCondidition = MainReferences.DeliveryConditions[Entity.DO_USL_POST_DC];
                RaisePropertyChanged(nameof(DeliverCondidition));
                RaisePropertyChanged();
            }
        }

        public DeliveryCondition DeliverCondidition
        {
            get => myDeliverCondidition;
            set
            {
                if (myDeliverCondidition != null && myDeliverCondidition.Equals(value)) return;
                myDeliverCondidition = value;
                if (myDeliverCondidition != null)
                    DO_USL_POST_DC = myDeliverCondidition.DocCode;
                RaisePropertyChanged();
            }
        }

        public decimal DO_CRS_DC
        {
            get => Entity.DO_CRS_DC;
            set
            {
                if (Entity.DO_CRS_DC == value) return;
                Entity.DO_CRS_DC = value;
                if (MainReferences.Currencies.ContainsKey(Entity.DO_CRS_DC))
                    myCurrency = MainReferences.Currencies[Entity.DO_CRS_DC];
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Currency));
            }
        }

        public Currency Currency
        {
            get => myCurrency;
            set
            {
                if (Equals(myCurrency, value)) return;
                myCurrency = value;
                if (myCurrency != null)
                    Entity.DO_CRS_DC = myCurrency.DocCode;
                RaisePropertyChanged();
            }
        }

        public decimal? DO_CRS_SUMMA
        {
            get => Entity.DO_CRS_SUMMA;
            set
            {
                if (Entity.DO_CRS_SUMMA == value) return;
                Entity.DO_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        public decimal Summa
        {
            get => DO_CRS_SUMMA ?? 0;
            set
            {
                if (DO_CRS_SUMMA == value) return;
                DO_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        public decimal? DO_BASE_CONTRACT_DC
        {
            get => Entity.DO_BASE_CONTRACT_DC;
            set
            {
                if (Entity.DO_BASE_CONTRACT_DC == value) return;
                Entity.DO_BASE_CONTRACT_DC = value;
                RaisePropertyChanged();
            }
        }

        public string DO_NOTES
        {
            get => Entity.DO_NOTES;
            set
            {
                if (Entity.DO_NOTES == value) return;
                Entity.DO_NOTES = value;
                RaisePropertyChanged();
            }
        }

        public byte[] DO_OLE
        {
            get => Entity.DO_OLE;
            set
            {
                if (Entity.DO_OLE == value) return;
                Entity.DO_OLE = value;
                RaisePropertyChanged();
            }
        }

        public short DO_PRECISE_SUM
        {
            get => Entity.DO_PRECISE_SUM;
            set
            {
                if (Entity.DO_PRECISE_SUM == value) return;
                Entity.DO_PRECISE_SUM = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? DO_START_DATE
        {
            get => Entity.DO_START_DATE;
            set
            {
                if (Entity.DO_START_DATE == value) return;
                Entity.DO_START_DATE = value;
                RaisePropertyChanged();
            }
        }

        public short? DO_PRIORITET
        {
            get => Entity.DO_PRIORITET;
            set
            {
                if (Entity.DO_PRIORITET == value) return;
                Entity.DO_PRIORITET = value;
                RaisePropertyChanged();
            }
        }

        public int? DO_OTV_TABELNUMBER
        {
            get => Entity.DO_OTV_TABELNUMBER;
            set
            {
                if (Entity.DO_OTV_TABELNUMBER == value) return;
                Entity.DO_OTV_TABELNUMBER = value;
                RaisePropertyChanged();
            }
        }

        public short? DO_1INCLUDENDS_0NO
        {
            get => Entity.DO_1INCLUDENDS_0NO;
            set
            {
                if (Entity.DO_1INCLUDENDS_0NO == value) return;
                Entity.DO_1INCLUDENDS_0NO = value;
                RaisePropertyChanged();
            }
        }

        public SD_102 SD_102
        {
            get => Entity.SD_102;
            set
            {
                if (Entity.SD_102 == value) return;
                Entity.SD_102 = value;
                RaisePropertyChanged();
            }
        }

        public SD_103 SD_103
        {
            get => Entity.SD_103;
            set
            {
                if (Entity.SD_103 == value) return;
                Entity.SD_103 = value;
                RaisePropertyChanged();
            }
        }

        public SD_112 SD_1122
        {
            get => Entity.SD_1122;
            set
            {
                if (Entity.SD_1122 == value) return;
                Entity.SD_1122 = value;
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

        public List<SD_112> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual SD_112 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_112 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_112 doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(SD_112 ent)
        {
            DO_TYPE_DC = ent.DO_TYPE_DC;
            DO_NUM = ent.DO_NUM;
            CREATOR = ent.CREATOR;
            DO_PODPIS_DATE = ent.DO_PODPIS_DATE;
            DO_EXEC_DATE = ent.DO_EXEC_DATE;
            DO_PODPISAN = ent.DO_PODPISAN;
            DO_ZAKONCHEN = ent.DO_ZAKONCHEN;
            DO_SALER_DC = ent.DO_SALER_DC;
            DO_USL_POST_DC = ent.DO_USL_POST_DC;
            DO_CRS_DC = ent.DO_CRS_DC;
            DO_CRS_SUMMA = ent.DO_CRS_SUMMA;
            DO_BASE_CONTRACT_DC = ent.DO_BASE_CONTRACT_DC;
            DO_NOTES = ent.DO_NOTES;
            DO_OLE = ent.DO_OLE;
            DO_PRECISE_SUM = ent.DO_PRECISE_SUM;
            DO_START_DATE = ent.DO_START_DATE;
            DO_PRIORITET = ent.DO_PRIORITET;
            DO_OTV_TABELNUMBER = ent.DO_OTV_TABELNUMBER;
            DO_1INCLUDENDS_0NO = ent.DO_1INCLUDENDS_0NO;
            SD_102 = ent.SD_102;
            SD_103 = ent.SD_103;
            SD_1122 = ent.SD_1122;
            SD_43 = ent.SD_43;
            SD_2 = ent.SD_2;
        }

        public void UpdateTo(SD_112 ent)
        {
            ent.DO_TYPE_DC = DO_TYPE_DC;
            ent.DO_NUM = DO_NUM;
            ent.CREATOR = CREATOR;
            ent.DO_PODPIS_DATE = DO_PODPIS_DATE;
            ent.DO_EXEC_DATE = DO_EXEC_DATE;
            ent.DO_PODPISAN = DO_PODPISAN;
            ent.DO_ZAKONCHEN = DO_ZAKONCHEN;
            ent.DO_SALER_DC = DO_SALER_DC;
            ent.DO_USL_POST_DC = DO_USL_POST_DC;
            ent.DO_CRS_DC = DO_CRS_DC;
            ent.DO_CRS_SUMMA = DO_CRS_SUMMA;
            ent.DO_BASE_CONTRACT_DC = DO_BASE_CONTRACT_DC;
            ent.DO_NOTES = DO_NOTES;
            ent.DO_OLE = DO_OLE;
            ent.DO_PRECISE_SUM = DO_PRECISE_SUM;
            ent.DO_START_DATE = DO_START_DATE;
            ent.DO_PRIORITET = DO_PRIORITET;
            ent.DO_OTV_TABELNUMBER = DO_OTV_TABELNUMBER;
            ent.DO_1INCLUDENDS_0NO = DO_1INCLUDENDS_0NO;
            ent.SD_102 = SD_102;
            ent.SD_103 = SD_103;
            ent.SD_1122 = SD_1122;
            ent.SD_43 = SD_43;
            ent.SD_2 = SD_2;
        }

        public override string ToString()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return Kontragent == null
                ? null
                : $"Договор покупки №{DO_NUM} от {DO_PODPIS_DATE} Поставщик: {Kontragent} на сумму {Summa:n2}";
        }
    }
}