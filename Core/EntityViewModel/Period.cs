using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    /// <summary>
    ///     Период для финансового учета
    /// </summary>
    public class Period : RSViewModelBase, IEntity<SD_138>
    {
        private SD_138 myEntity;

        public Period()
        {
            Entity = new SD_138 {DOC_CODE = -1};
        }

        public Period(SD_138 entity)
        {
            Entity = entity ?? new SD_138 {DOC_CODE = -1};
        }

        public SD_138 Entity
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
                {
                    if (Entity.DOC_CODE == value) return;
                    Entity.DOC_CODE = value;
                    RaisePropertyChanged();
                }
            }
        }

        public short? DELETED
        {
            get => Entity.DELETED;
            set
            {
                if (Entity.DELETED == value) return;
                Entity.DELETED = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? BS_START_BLS
        {
            get => Entity.BS_START_BLS;
            set
            {
                if (Entity.BS_START_BLS == value) return;
                Entity.BS_START_BLS = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? BS_STOP_BLS
        {
            get => Entity.BS_STOP_BLS;
            set
            {
                if (Entity.BS_STOP_BLS == value) return;
                Entity.BS_STOP_BLS = value;
                RaisePropertyChanged();
            }
        }

        public string BS_COMMENT
        {
            get => Entity.BS_COMMENT;
            set
            {
                if (Entity.BS_COMMENT == value) return;
                Entity.BS_COMMENT = value;
                RaisePropertyChanged();
            }
        }

        public short? BS_UCHET_TYPE
        {
            get => Entity.BS_UCHET_TYPE;
            set
            {
                if (Entity.BS_UCHET_TYPE == value) return;
                Entity.BS_UCHET_TYPE = value;
                RaisePropertyChanged();
            }
        }

        public short? BS_EXECUTED
        {
            get => Entity.BS_EXECUTED;
            set
            {
                if (Entity.BS_EXECUTED == value) return;
                Entity.BS_EXECUTED = value;
                RaisePropertyChanged();
            }
        }

        public short? OA_COMPLETE_FLAG
        {
            get => Entity.OA_COMPLETE_FLAG;
            set
            {
                if (Entity.OA_COMPLETE_FLAG == value) return;
                Entity.OA_COMPLETE_FLAG = value;
                RaisePropertyChanged();
            }
        }

        public short? BS_INC_NAKLAD_TO_CENA
        {
            get => Entity.BS_INC_NAKLAD_TO_CENA;
            set
            {
                if (Entity.BS_INC_NAKLAD_TO_CENA == value) return;
                Entity.BS_INC_NAKLAD_TO_CENA = value;
                RaisePropertyChanged();
            }
        }

        public decimal? BS_PREV_PERIOD_DC
        {
            get => Entity.BS_PREV_PERIOD_DC;
            set
            {
                if (Entity.BS_PREV_PERIOD_DC == value) return;
                Entity.BS_PREV_PERIOD_DC = value;
                RaisePropertyChanged();
            }
        }

        public short? BS_0AVER_1AVERSLIDE
        {
            get => Entity.BS_0AVER_1AVERSLIDE;
            set
            {
                if (Entity.BS_0AVER_1AVERSLIDE == value) return;
                Entity.BS_0AVER_1AVERSLIDE = value;
                RaisePropertyChanged();
            }
        }

        public short? BS_UCH_PROZV_V_BUH
        {
            get => Entity.BS_UCH_PROZV_V_BUH;
            set
            {
                if (Entity.BS_UCH_PROZV_V_BUH == value) return;
                Entity.BS_UCH_PROZV_V_BUH = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAccessRight { get; set; }

        public List<SD_138> LoadList()
        {
            throw new NotImplementedException();
        }

        public virtual SD_138 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_138 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_138 doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(SD_138 ent)
        {
            DELETED = ent.DELETED;
            BS_START_BLS = ent.BS_START_BLS;
            BS_STOP_BLS = ent.BS_STOP_BLS;
            BS_COMMENT = ent.BS_COMMENT;
            BS_UCHET_TYPE = ent.BS_UCHET_TYPE;
            BS_EXECUTED = ent.BS_EXECUTED;
            OA_COMPLETE_FLAG = ent.OA_COMPLETE_FLAG;
            BS_INC_NAKLAD_TO_CENA = ent.BS_INC_NAKLAD_TO_CENA;
            BS_PREV_PERIOD_DC = ent.BS_PREV_PERIOD_DC;
            BS_0AVER_1AVERSLIDE = ent.BS_0AVER_1AVERSLIDE;
            BS_UCH_PROZV_V_BUH = ent.BS_UCH_PROZV_V_BUH;
        }

        public void UpdateTo(SD_138 ent)
        {
            ent.DELETED = DELETED;
            ent.BS_START_BLS = BS_START_BLS;
            ent.BS_STOP_BLS = BS_STOP_BLS;
            ent.BS_COMMENT = BS_COMMENT;
            ent.BS_UCHET_TYPE = BS_UCHET_TYPE;
            ent.BS_EXECUTED = BS_EXECUTED;
            ent.OA_COMPLETE_FLAG = OA_COMPLETE_FLAG;
            ent.BS_INC_NAKLAD_TO_CENA = BS_INC_NAKLAD_TO_CENA;
            ent.BS_PREV_PERIOD_DC = BS_PREV_PERIOD_DC;
            ent.BS_0AVER_1AVERSLIDE = BS_0AVER_1AVERSLIDE;
            ent.BS_UCH_PROZV_V_BUH = BS_UCH_PROZV_V_BUH;
        }

        public override string ToString()
        {
            return BS_COMMENT;
        }
    }
}