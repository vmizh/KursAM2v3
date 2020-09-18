using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel
{
    [MetadataType(typeof(DataAnnotationsCashStartRemains))]
    public class CashStartRemains : RSViewModelBase, IEntity<TD_22>
    {
        private CashReference myCash;
        private Currency myCurrency;
        private TD_22 myEntity;

        public CashStartRemains()
        {
            Entity = new TD_22 {DOC_CODE = -1};
        }

        public CashStartRemains(TD_22 entity)
        {
            Entity = entity ?? new TD_22 {DOC_CODE = -1};
            Currency = MainReferences.Currencies.ContainsKey(Entity.CRS_DC) ? MainReferences.Currencies[CRS_DC] : null;
        }

        public CashReference Cash
        {
            get => myCash;
            set
            {
                if (myCash != null && myCash.Equals(value)) return;
                myCash = value;
                RaisePropertyChanged();
            }
        }

        public TD_22 Entity
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
            get => DOC_CODE;
            set
            {
                if (DOC_CODE == value) return;
                DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

        public override int Code
        {
            get => (int) Entity.CODE;
            set
            {
                if (Entity.CODE == value) return;
                Entity.CODE = value;
                RaisePropertyChanged();
            }
        }

        public decimal CRS_DC
        {
            get => Entity.CRS_DC;
            set
            {
                if (Entity.CRS_DC == value) return;
                Entity.CRS_DC = value;
                RaisePropertyChanged();
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
                    CRS_DC = myCurrency.DocCode;
                RaisePropertyChanged();
            }
        }

        public DateTime DATE_START
        {
            get => Entity.DATE_START;
            set
            {
                if (Entity.DATE_START == value) return;
                Entity.DATE_START = value;
                RaisePropertyChanged();
            }
        }

        public decimal SUMMA_START
        {
            get => Entity.SUMMA_START;
            set
            {
                if (Entity.SUMMA_START == value) return;
                Entity.SUMMA_START = value;
                RaisePropertyChanged();
            }
        }

        public decimal? CASH_DATE_DC
        {
            get => Entity.CASH_DATE_DC;
            set
            {
                if (Entity.CASH_DATE_DC == value) return;
                Entity.CASH_DATE_DC = value;
                RaisePropertyChanged();
            }
        }

        public SD_22 SD_22
        {
            get => Entity.SD_22;
            set
            {
                if (Entity.SD_22 == value) return;
                Entity.SD_22 = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAccessRight { get; set; }

        public List<TD_22> LoadList()
        {
            throw new NotImplementedException();
        }

        public virtual TD_22 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual TD_22 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(TD_22 doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(TD_22 ent)
        {
            Code = (int) ent.CODE;
            CRS_DC = ent.CRS_DC;
            DATE_START = ent.DATE_START;
            SUMMA_START = ent.SUMMA_START;
            CASH_DATE_DC = ent.CASH_DATE_DC;
            SD_22 = ent.SD_22;
        }

        public void UpdateTo(TD_22 ent)
        {
            ent.CODE = Code;
            ent.CRS_DC = CRS_DC;
            ent.DATE_START = DATE_START;
            ent.SUMMA_START = SUMMA_START;
            ent.CASH_DATE_DC = CASH_DATE_DC;
            ent.SD_22 = SD_22;
        }
    }

    public class DataAnnotationsCashStartRemains : DataAnnotationForFluentApiBase, IMetadataProvider<CashStartRemains>
    {
        void IMetadataProvider<CashStartRemains>.BuildMetadata(MetadataBuilder<CashStartRemains> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.DATE_START).AutoGenerated().DisplayName("Дата");
            builder.Property(_ => _.Currency).AutoGenerated().DisplayName("Валюта");
            builder.Property(_ => _.SUMMA_START).AutoGenerated().DisplayName("Сумма");
        }
    }
}