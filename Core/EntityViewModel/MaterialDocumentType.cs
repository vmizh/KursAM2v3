using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    /// <summary>
    ///     Тип документа по товарным операциям
    /// </summary>
    public class MaterialDocumentType : RSViewModelBase, IEntity<SD_201>
    {
        private SD_201 myEntity;

        public MaterialDocumentType()
        {
            Entity = new SD_201 {DOC_CODE = -1};
        }

        public MaterialDocumentType(SD_201 entity)
        {
            Entity = entity ?? new SD_201 {DOC_CODE = -1};
        }

        public SD_201 Entity
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
        public string D_NAME
        {
            get => Entity.D_NAME;
            set
            {
                if (Entity.D_NAME == value) return;
                Entity.D_NAME = value;
                RaisePropertyChanged();
            }
        }
        public int D_OP_CODE
        {
            get => Entity.D_OP_CODE;
            set
            {
                if (Entity.D_OP_CODE == value) return;
                Entity.D_OP_CODE = value;
                RaisePropertyChanged();
            }
        }
        public string D_SAL_FORM
        {
            get => Entity.D_SAL_FORM;
            set
            {
                if (Entity.D_SAL_FORM == value) return;
                Entity.D_SAL_FORM = value;
                RaisePropertyChanged();
            }
        }
        public short? D_PRIHOD
        {
            get => Entity.D_PRIHOD;
            set
            {
                if (Entity.D_PRIHOD == value) return;
                Entity.D_PRIHOD = value;
                RaisePropertyChanged();
            }
        }
        public short? D_RASHOD
        {
            get => Entity.D_RASHOD;
            set
            {
                if (Entity.D_RASHOD == value) return;
                Entity.D_RASHOD = value;
                RaisePropertyChanged();
            }
        }
        public int? D_HELP_CONTEXT
        {
            get => Entity.D_HELP_CONTEXT;
            set
            {
                if (Entity.D_HELP_CONTEXT == value) return;
                Entity.D_HELP_CONTEXT = value;
                RaisePropertyChanged();
            }
        }
        public int? ORDER_CALC
        {
            get => Entity.ORDER_CALC;
            set
            {
                if (Entity.ORDER_CALC == value) return;
                Entity.ORDER_CALC = value;
                RaisePropertyChanged();
            }
        }
        public bool IsAccessRight { get; set; }

        public List<SD_201> LoadList()
        {
            throw new NotImplementedException();
        }

        public virtual SD_201 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_201 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_201 doc)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return D_NAME;
        }

        public void UpdateFrom(SD_201 ent)
        {
            D_NAME = ent.D_NAME;
            D_OP_CODE = ent.D_OP_CODE;
            D_SAL_FORM = ent.D_SAL_FORM;
            D_PRIHOD = ent.D_PRIHOD;
            D_RASHOD = ent.D_RASHOD;
            D_HELP_CONTEXT = ent.D_HELP_CONTEXT;
            ORDER_CALC = ent.ORDER_CALC;
        }

        public void UpdateTo(SD_201 ent)
        {
            ent.D_NAME = D_NAME;
            ent.D_OP_CODE = D_OP_CODE;
            ent.D_SAL_FORM = D_SAL_FORM;
            ent.D_PRIHOD = D_PRIHOD;
            ent.D_RASHOD = D_RASHOD;
            ent.D_HELP_CONTEXT = D_HELP_CONTEXT;
            ent.ORDER_CALC = ORDER_CALC;
        }
    }
}