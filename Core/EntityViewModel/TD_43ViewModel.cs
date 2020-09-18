using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class TD_43ViewModel : RSViewModelBase, IEntity<TD_43>
    {
        private TD_43 myEntity;

        public TD_43ViewModel()
        {
            Entity = DefaultValue();
        }

        public TD_43ViewModel(TD_43 entity)
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

        public override int Code
        {
            get => Entity.CODE;
            set
            {
                if (Entity.CODE == value) return;
                Entity.CODE = value;
                RaisePropertyChanged();
            }
        }

        public string RASCH_ACC
        {
            get => Entity.RASCH_ACC;
            set
            {
                if (Entity.RASCH_ACC == value) return;
                Entity.RASCH_ACC = value;
                RaisePropertyChanged();
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

        public decimal? BANK_DC
        {
            get => Entity.BANK_DC;
            set
            {
                if (Entity.BANK_DC == value) return;
                Entity.BANK_DC = value;
                RaisePropertyChanged();
            }
        }

        public short? VALUTNIY
        {
            get => Entity.VALUTNIY;
            set
            {
                if (Entity.VALUTNIY == value) return;
                Entity.VALUTNIY = value;
                RaisePropertyChanged();
            }
        }

        public short? TRANSITNIY
        {
            get => Entity.TRANSITNIY;
            set
            {
                if (Entity.TRANSITNIY == value) return;
                Entity.TRANSITNIY = value;
                RaisePropertyChanged();
            }
        }

        public string FILIAL_BANKA
        {
            get => Entity.FILIAL_BANKA;
            set
            {
                if (Entity.FILIAL_BANKA == value) return;
                Entity.FILIAL_BANKA = value;
                RaisePropertyChanged();
            }
        }

        public short? DISABLED
        {
            get => Entity.DISABLED;
            set
            {
                if (Entity.DISABLED == value) return;
                Entity.DISABLED = value;
                RaisePropertyChanged();
            }
        }

        public int? USE_FOR_TLAT_TREB
        {
            get => Entity.USE_FOR_TLAT_TREB;
            set
            {
                if (Entity.USE_FOR_TLAT_TREB == value) return;
                Entity.USE_FOR_TLAT_TREB = value;
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

        public SD_44 SD_44
        {
            get => Entity.SD_44;
            set
            {
                if (Entity.SD_44 == value) return;
                Entity.SD_44 = value;
                RaisePropertyChanged();
            }
        }

        public TD_43 Entity
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

        public List<TD_43> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual void Save(TD_43 doc)
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

        public void UpdateFrom(TD_43 ent)
        {
            Code = ent.CODE;
            RASCH_ACC = ent.RASCH_ACC;
            DELETED = ent.DELETED;
            BANK_DC = ent.BANK_DC;
            VALUTNIY = ent.VALUTNIY;
            TRANSITNIY = ent.TRANSITNIY;
            FILIAL_BANKA = ent.FILIAL_BANKA;
            DISABLED = ent.DISABLED;
            USE_FOR_TLAT_TREB = ent.USE_FOR_TLAT_TREB;
            SD_43 = ent.SD_43;
            SD_44 = ent.SD_44;
        }

        public void UpdateTo(TD_43 ent)
        {
            ent.CODE = Code;
            ent.RASCH_ACC = RASCH_ACC;
            ent.DELETED = DELETED;
            ent.BANK_DC = BANK_DC;
            ent.VALUTNIY = VALUTNIY;
            ent.TRANSITNIY = TRANSITNIY;
            ent.FILIAL_BANKA = FILIAL_BANKA;
            ent.DISABLED = DISABLED;
            ent.USE_FOR_TLAT_TREB = USE_FOR_TLAT_TREB;
            ent.SD_43 = SD_43;
            ent.SD_44 = SD_44;
        }

        public TD_43 DefaultValue()
        {
            return new TD_43
            {
                DOC_CODE = -1
            };
        }

        public TD_43 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public TD_43 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual TD_43 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual TD_43 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}