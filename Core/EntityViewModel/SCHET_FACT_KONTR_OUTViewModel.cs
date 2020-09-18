using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel
{
    public class SCHET_FACT_KONTR_OUTViewModel : RSViewModelBase, IEntity<SCHET_FACT_KONTR_OUT>
    {
        private SCHET_FACT_KONTR_OUT myEntity;

        public SCHET_FACT_KONTR_OUTViewModel()
        {
            Entity = DefaultValue();
        }

        public SCHET_FACT_KONTR_OUTViewModel(SCHET_FACT_KONTR_OUT entity)
        {
            Entity = entity ?? DefaultValue();
        }

        public Guid ID
        {
            get => Entity.ID;
            set
            {
                if (Entity.ID == value) return;
                Entity.ID = value;
                RaisePropertyChanged();
            }
        }

        public string NUM
        {
            get => Entity.NUM;
            set
            {
                if (Entity.NUM == value) return;
                Entity.NUM = value;
                RaisePropertyChanged();
            }
        }

        public string CORRECT_NUM
        {
            get => Entity.CORRECT_NUM;
            set
            {
                if (Entity.CORRECT_NUM == value) return;
                Entity.CORRECT_NUM = value;
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

        public DateTime? DOC_CORRECT_DATE
        {
            get => Entity.DOC_CORRECT_DATE;
            set
            {
                if (Entity.DOC_CORRECT_DATE == value) return;
                Entity.DOC_CORRECT_DATE = value;
                RaisePropertyChanged();
            }
        }

        public Guid KONTR_ID
        {
            get => Entity.KONTR_ID;
            set
            {
                if (Entity.KONTR_ID == value) return;
                Entity.KONTR_ID = value;
                RaisePropertyChanged();
            }
        }

        public decimal CURRENCY_DC
        {
            get => Entity.CURRENCY_DC;
            set
            {
                if (Entity.CURRENCY_DC == value) return;
                Entity.CURRENCY_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SUMMA
        {
            get => Entity.SUMMA;
            set
            {
                if (Entity.SUMMA == value) return;
                Entity.SUMMA = value;
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

        public string GRUZOOTPRAVITEL
        {
            get => Entity.GRUZOOTPRAVITEL;
            set
            {
                if (Entity.GRUZOOTPRAVITEL == value) return;
                Entity.GRUZOOTPRAVITEL = value;
                RaisePropertyChanged();
            }
        }

        public string GRUZOPOLUCHATEL
        {
            get => Entity.GRUZOPOLUCHATEL;
            set
            {
                if (Entity.GRUZOPOLUCHATEL == value) return;
                Entity.GRUZOPOLUCHATEL = value;
                RaisePropertyChanged();
            }
        }

        public string PAYDOC_TEXT
        {
            get => Entity.PAYDOC_TEXT;
            set
            {
                if (Entity.PAYDOC_TEXT == value) return;
                Entity.PAYDOC_TEXT = value;
                RaisePropertyChanged();
            }
        }

        public string DIRECTOR
        {
            get => Entity.DIRECTOR;
            set
            {
                if (Entity.DIRECTOR == value) return;
                Entity.DIRECTOR = value;
                RaisePropertyChanged();
            }
        }

        public string GL_BUH
        {
            get => Entity.GL_BUH;
            set
            {
                if (Entity.GL_BUH == value) return;
                Entity.GL_BUH = value;
                RaisePropertyChanged();
            }
        }

        public string NOTE
        {
            get => Entity.NOTE;
            set
            {
                if (Entity.NOTE == value) return;
                Entity.NOTE = value;
                RaisePropertyChanged();
            }
        }

        public Guid? RECEIVER_KONTR_ID
        {
            get => Entity.RECEIVER_KONTR_ID;
            set
            {
                if (Entity.RECEIVER_KONTR_ID == value) return;
                Entity.RECEIVER_KONTR_ID = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SD_84_DC
        {
            get => Entity.SD_84_DC;
            set
            {
                if (Entity.SD_84_DC == value) return;
                Entity.SD_84_DC = value;
                RaisePropertyChanged();
            }
        }

        public KONTRAGENT_REF_OUT KONTRAGENT_REF_OUT
        {
            get => Entity.KONTRAGENT_REF_OUT;
            set
            {
                if (Entity.KONTRAGENT_REF_OUT == value) return;
                Entity.KONTRAGENT_REF_OUT = value;
                RaisePropertyChanged();
            }
        }

        public KONTRAGENT_REF_OUT KONTRAGENT_REF_OUT1
        {
            get => Entity.KONTRAGENT_REF_OUT1;
            set
            {
                if (Entity.KONTRAGENT_REF_OUT1 == value) return;
                Entity.KONTRAGENT_REF_OUT1 = value;
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

        public SCHET_FACT_KONTR_OUT Entity
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

        public List<SCHET_FACT_KONTR_OUT> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public SCHET_FACT_KONTR_OUT Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SCHET_FACT_KONTR_OUT doc)
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

        public void UpdateFrom(SCHET_FACT_KONTR_OUT ent)
        {
            ID = ent.ID;
            NUM = ent.NUM;
            CORRECT_NUM = ent.CORRECT_NUM;
            DOC_DATE = ent.DOC_DATE;
            DOC_CORRECT_DATE = ent.DOC_CORRECT_DATE;
            KONTR_ID = ent.KONTR_ID;
            CURRENCY_DC = ent.CURRENCY_DC;
            SUMMA = ent.SUMMA;
            CREATOR = ent.CREATOR;
            GRUZOOTPRAVITEL = ent.GRUZOOTPRAVITEL;
            GRUZOPOLUCHATEL = ent.GRUZOPOLUCHATEL;
            PAYDOC_TEXT = ent.PAYDOC_TEXT;
            DIRECTOR = ent.DIRECTOR;
            GL_BUH = ent.GL_BUH;
            NOTE = ent.NOTE;
            RECEIVER_KONTR_ID = ent.RECEIVER_KONTR_ID;
            SD_84_DC = ent.SD_84_DC;
            KONTRAGENT_REF_OUT = ent.KONTRAGENT_REF_OUT;
            KONTRAGENT_REF_OUT1 = ent.KONTRAGENT_REF_OUT1;
            SD_301 = ent.SD_301;
            SD_84 = ent.SD_84;
        }

        public void UpdateTo(SCHET_FACT_KONTR_OUT ent)
        {
            ent.ID = ID;
            ent.NUM = NUM;
            ent.CORRECT_NUM = CORRECT_NUM;
            ent.DOC_DATE = DOC_DATE;
            ent.DOC_CORRECT_DATE = DOC_CORRECT_DATE;
            ent.KONTR_ID = KONTR_ID;
            ent.CURRENCY_DC = CURRENCY_DC;
            ent.SUMMA = SUMMA;
            ent.CREATOR = CREATOR;
            ent.GRUZOOTPRAVITEL = GRUZOOTPRAVITEL;
            ent.GRUZOPOLUCHATEL = GRUZOPOLUCHATEL;
            ent.PAYDOC_TEXT = PAYDOC_TEXT;
            ent.DIRECTOR = DIRECTOR;
            ent.GL_BUH = GL_BUH;
            ent.NOTE = NOTE;
            ent.RECEIVER_KONTR_ID = RECEIVER_KONTR_ID;
            ent.SD_84_DC = SD_84_DC;
            ent.KONTRAGENT_REF_OUT = KONTRAGENT_REF_OUT;
            ent.KONTRAGENT_REF_OUT1 = KONTRAGENT_REF_OUT1;
            ent.SD_301 = SD_301;
            ent.SD_84 = SD_84;
        }

        public SCHET_FACT_KONTR_OUT DefaultValue()
        {
            return new SCHET_FACT_KONTR_OUT
            {
                ID = Guid.NewGuid()
            };
        }

        public virtual SCHET_FACT_KONTR_OUT Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SCHET_FACT_KONTR_OUT Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SCHET_FACT_KONTR_OUT Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }
}