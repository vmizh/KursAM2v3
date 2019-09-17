using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class SD_148ViewModel : RSViewModelBase, IEntity<SD_148>
    {
        private SD_148 myEntity;

        public SD_148ViewModel()
        {
            Entity = DefaultValue();
        }

        public SD_148ViewModel(SD_148 entity)
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
        public decimal CK_MIN_OBOROT
        {
            get => Entity.CK_MIN_OBOROT;
            set
            {
                if (Entity.CK_MIN_OBOROT == value) return;
                Entity.CK_MIN_OBOROT = value;
                RaisePropertyChanged();
            }
        }
        public double CK_MAX_PROSROCH_DNEY
        {
            get => Entity.CK_MAX_PROSROCH_DNEY;
            set
            {
                if (Entity.CK_MAX_PROSROCH_DNEY == value) return;
                Entity.CK_MAX_PROSROCH_DNEY = value;
                RaisePropertyChanged();
            }
        }
        public decimal CK_CREDIT_SUM
        {
            get => Entity.CK_CREDIT_SUM;
            set
            {
                if (Entity.CK_CREDIT_SUM == value) return;
                Entity.CK_CREDIT_SUM = value;
                RaisePropertyChanged();
            }
        }
        public string CK_NAME
        {
            get => Entity.CK_NAME;
            set
            {
                if (Entity.CK_NAME == value) return;
                Entity.CK_NAME = value;
                RaisePropertyChanged();
            }
        }
        public double? CK_NACEN_DEFAULT_ROZN
        {
            get => Entity.CK_NACEN_DEFAULT_ROZN;
            set
            {
                if (Entity.CK_NACEN_DEFAULT_ROZN == value) return;
                Entity.CK_NACEN_DEFAULT_ROZN = value;
                RaisePropertyChanged();
            }
        }
        public double? CK_NACEN_DEFAULT_KOMPL
        {
            get => Entity.CK_NACEN_DEFAULT_KOMPL;
            set
            {
                if (Entity.CK_NACEN_DEFAULT_KOMPL == value) return;
                Entity.CK_NACEN_DEFAULT_KOMPL = value;
                RaisePropertyChanged();
            }
        }
        public short? CK_IMMEDIATE_PRICE_CHANGE
        {
            get => Entity.CK_IMMEDIATE_PRICE_CHANGE;
            set
            {
                if (Entity.CK_IMMEDIATE_PRICE_CHANGE == value) return;
                Entity.CK_IMMEDIATE_PRICE_CHANGE = value;
                RaisePropertyChanged();
            }
        }
        public string CK_GROUP
        {
            get => Entity.CK_GROUP;
            set
            {
                if (Entity.CK_GROUP == value) return;
                Entity.CK_GROUP = value;
                RaisePropertyChanged();
            }
        }
        public SD_148 Entity
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

        public List<SD_148> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public SD_148 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_148 doc)
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

        public void UpdateFrom(SD_148 ent)
        {
            CK_MIN_OBOROT = ent.CK_MIN_OBOROT;
            CK_MAX_PROSROCH_DNEY = ent.CK_MAX_PROSROCH_DNEY;
            CK_CREDIT_SUM = ent.CK_CREDIT_SUM;
            CK_NAME = ent.CK_NAME;
            CK_NACEN_DEFAULT_ROZN = ent.CK_NACEN_DEFAULT_ROZN;
            CK_NACEN_DEFAULT_KOMPL = ent.CK_NACEN_DEFAULT_KOMPL;
            CK_IMMEDIATE_PRICE_CHANGE = ent.CK_IMMEDIATE_PRICE_CHANGE;
            CK_GROUP = ent.CK_GROUP;
        }

        public void UpdateTo(SD_148 ent)
        {
            ent.CK_MIN_OBOROT = CK_MIN_OBOROT;
            ent.CK_MAX_PROSROCH_DNEY = CK_MAX_PROSROCH_DNEY;
            ent.CK_CREDIT_SUM = CK_CREDIT_SUM;
            ent.CK_NAME = CK_NAME;
            ent.CK_NACEN_DEFAULT_ROZN = CK_NACEN_DEFAULT_ROZN;
            ent.CK_NACEN_DEFAULT_KOMPL = CK_NACEN_DEFAULT_KOMPL;
            ent.CK_IMMEDIATE_PRICE_CHANGE = CK_IMMEDIATE_PRICE_CHANGE;
            ent.CK_GROUP = CK_GROUP;
        }

        public SD_148 DefaultValue()
        {
            return new SD_148
            {
                DOC_CODE = -1
            };
        }

        public virtual SD_148 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_148 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_148 Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }
}