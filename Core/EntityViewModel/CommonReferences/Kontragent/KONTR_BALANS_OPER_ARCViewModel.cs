using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel.CommonReferences.Kontragent
{
    // ReSharper disable once InconsistentNaming
    public class KONTR_BALANS_OPER_ARCViewModel : RSViewModelBase, IEntity<KONTR_BALANS_OPER_ARC>
    {
        private KONTR_BALANS_OPER_ARC myEntity;

        public KONTR_BALANS_OPER_ARCViewModel()
        {
            Entity = DefaultValue();
        }

        public KONTR_BALANS_OPER_ARCViewModel(KONTR_BALANS_OPER_ARC entity)
        {
            Entity = entity ?? DefaultValue();
        }

        public string DOC_NAME
        {
            get => Entity.DOC_NAME;
            set
            {
                if (Entity.DOC_NAME == value) return;
                Entity.DOC_NAME = value;
                RaisePropertyChanged();
            }
        }

        public string DOC_NUM
        {
            get => Entity.DOC_NUM;
            set
            {
                if (Entity.DOC_NUM == value) return;
                Entity.DOC_NUM = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DOC_DATE
        {
            get => Entity.DOC_DATE;
            set
            {
                if (Entity.DOC_DATE == value) return;
                Entity.DOC_DATE = value;
                RaisePropertyChanged();
            }
        }

        public double CRS_KONTR_IN
        {
            get => Entity.CRS_KONTR_IN;
            set
            {
                if (Math.Abs(Entity.CRS_KONTR_IN - value) < 0.01) return;
                Entity.CRS_KONTR_IN = value;
                RaisePropertyChanged();
            }
        }

        public double CRS_KONTR_OUT
        {
            get => Entity.CRS_KONTR_OUT;
            set
            {
                if (Math.Abs(Entity.CRS_KONTR_OUT - value) < 0.01) return;
                Entity.CRS_KONTR_OUT = value;
                RaisePropertyChanged();
            }
        }

        public decimal DOC_DC
        {
            get => Entity.DOC_DC;
            set
            {
                if (Entity.DOC_DC == value) return;
                Entity.DOC_DC = value;
                RaisePropertyChanged();
            }
        }

        public int DOC_ROW_CODE
        {
            get => Entity.DOC_ROW_CODE;
            set
            {
                if (Entity.DOC_ROW_CODE == value) return;
                Entity.DOC_ROW_CODE = value;
                RaisePropertyChanged();
            }
        }

        public int DOC_TYPE_CODE
        {
            get => Entity.DOC_TYPE_CODE;
            set
            {
                if (Entity.DOC_TYPE_CODE == value) return;
                Entity.DOC_TYPE_CODE = value;
                RaisePropertyChanged();
            }
        }

        public double CRS_OPER_IN
        {
            get => Entity.CRS_OPER_IN;
            set
            {
                if (Math.Abs(Entity.CRS_OPER_IN - value) < 0.01) return;
                Entity.CRS_OPER_IN = value;
                RaisePropertyChanged();
            }
        }

        public double CRS_OPER_OUT
        {
            get => Entity.CRS_OPER_OUT;
            set
            {
                if (Math.Abs(Entity.CRS_OPER_OUT - value) < 0.01) return;
                Entity.CRS_OPER_OUT = value;
                RaisePropertyChanged();
            }
        }

        public decimal OPER_CRS_DC
        {
            get => Entity.OPER_CRS_DC;
            set
            {
                if (Entity.OPER_CRS_DC == value) return;
                Entity.OPER_CRS_DC = value;
                RaisePropertyChanged();
            }
        }

        public double OPER_CRS_RATE
        {
            get => Entity.OPER_CRS_RATE;
            set
            {
                if (Math.Abs(Entity.OPER_CRS_RATE - value) < 0.0001) return;
                Entity.OPER_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }

        public double UCH_CRS_RATE
        {
            get => Entity.UCH_CRS_RATE;
            set
            {
                if (Math.Abs(Entity.UCH_CRS_RATE - value) < 0.0001) return;
                Entity.UCH_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal KONTR_DC
        {
            get => Entity.KONTR_DC;
            set
            {
                if (Entity.KONTR_DC == value) return;
                Entity.KONTR_DC = value;
                RaisePropertyChanged();
            }
        }

        public string ID
        {
            get => Entity.ID;
            set
            {
                if (Entity.ID == value) return;
                Entity.ID = value;
                RaisePropertyChanged();
            }
        }

        public int NEW_CALC
        {
            get => Entity.NEW_CALC;
            set
            {
                if (Entity.NEW_CALC == value) return;
                Entity.NEW_CALC = value;
                RaisePropertyChanged();
            }
        }

        public KONTR_BALANS_OPER_ARC Entity
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

        public List<KONTR_BALANS_OPER_ARC> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public KONTR_BALANS_OPER_ARC Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(KONTR_BALANS_OPER_ARC doc)
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

        public void UpdateFrom(KONTR_BALANS_OPER_ARC ent)
        {
            DOC_NAME = ent.DOC_NAME;
            DOC_NUM = ent.DOC_NUM;
            DOC_DATE = ent.DOC_DATE;
            CRS_KONTR_IN = ent.CRS_KONTR_IN;
            CRS_KONTR_OUT = ent.CRS_KONTR_OUT;
            DOC_DC = ent.DOC_DC;
            DOC_ROW_CODE = ent.DOC_ROW_CODE;
            DOC_TYPE_CODE = ent.DOC_TYPE_CODE;
            CRS_OPER_IN = ent.CRS_OPER_IN;
            CRS_OPER_OUT = ent.CRS_OPER_OUT;
            OPER_CRS_DC = ent.OPER_CRS_DC;
            OPER_CRS_RATE = ent.OPER_CRS_RATE;
            UCH_CRS_RATE = ent.UCH_CRS_RATE;
            KONTR_DC = ent.KONTR_DC;
            ID = ent.ID;
            NEW_CALC = ent.NEW_CALC;
        }

        public void UpdateTo(KONTR_BALANS_OPER_ARC ent)
        {
            ent.DOC_NAME = DOC_NAME;
            ent.DOC_NUM = DOC_NUM;
            ent.DOC_DATE = DOC_DATE;
            ent.CRS_KONTR_IN = CRS_KONTR_IN;
            ent.CRS_KONTR_OUT = CRS_KONTR_OUT;
            ent.DOC_DC = DOC_DC;
            ent.DOC_ROW_CODE = DOC_ROW_CODE;
            ent.DOC_TYPE_CODE = DOC_TYPE_CODE;
            ent.CRS_OPER_IN = CRS_OPER_IN;
            ent.CRS_OPER_OUT = CRS_OPER_OUT;
            ent.OPER_CRS_DC = OPER_CRS_DC;
            ent.OPER_CRS_RATE = OPER_CRS_RATE;
            ent.UCH_CRS_RATE = UCH_CRS_RATE;
            ent.KONTR_DC = KONTR_DC;
            ent.ID = ID;
            ent.NEW_CALC = NEW_CALC;
        }

        public KONTR_BALANS_OPER_ARC DefaultValue()
        {
            return new KONTR_BALANS_OPER_ARC();
        }

        public virtual KONTR_BALANS_OPER_ARC Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual KONTR_BALANS_OPER_ARC Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public KONTR_BALANS_OPER_ARC Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }
}