using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.NomenklManagement
{
    /// <summary>
    ///     Условия поставки - франко-вагон и т.п.
    /// </summary>
    public class DeliveryCondition : RSViewModelBase, IEntity<SD_103>
    {
        private SD_103 myEntity;

        public DeliveryCondition()
        {
            Entity = DefaultValue();
        }

        public SD_103 DefaultValue()
        {
            return new SD_103 {DOC_CODE = -1};
        }

        public DeliveryCondition(SD_103 entity)
        {
            Entity = entity ?? new SD_103 {DOC_CODE = -1};
        }

        public SD_103 Entity
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

        public string BUP_NAME
        {
            get => Entity.BUP_NAME;
            set
            {
                if (Entity.BUP_NAME == value) return;
                Entity.BUP_NAME = value;
                RaisePropertyChanged();
            }
        }

        public List<SD_103> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual SD_103 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_103 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_103 doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(SD_103 ent)
        {
            BUP_NAME = ent.BUP_NAME;
        }

        public void UpdateTo(SD_103 ent)
        {
            ent.BUP_NAME = BUP_NAME;
        }
    }
}