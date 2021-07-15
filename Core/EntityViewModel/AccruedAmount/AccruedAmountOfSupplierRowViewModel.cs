using System;
using System.ComponentModel;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.AccruedAmount
{
    public class AccruedAmountOfSupplierRowViewModel : RSViewModelBase, IDataErrorInfo
    {

        #region Fields

        private AccuredAmountOfSupplierRow myEntity;
        private AccruedAmountOfSupplierViewModel myParent;
        private SDRSchet mySDRSchet;

        #endregion

        #region Properties
        
        public new AccruedAmountOfSupplierViewModel Parent
        {
            get => myParent;
            set
            {
                if (myParent == value) return;
                myParent = value;
                RaisePropertyChanged();
            }
        }

        public AccuredAmountOfSupplierRow Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
        }

        public Guid DocId
        {
            get => Entity.DocId;
            set
            {
                if (Entity.DocId == value) return;
                Entity.DocId = value;
                RaisePropertyChanged();
            }
        }

        public AccruedAmountTypeViewModel AccruedAmountType
        {
            get => MainReferences.GetAccruedAmountType(Entity.AccuredAmountTypeId);
            set
            {
                if (MainReferences.GetAccruedAmountType(Entity.AccuredAmountTypeId) == value) return;
                if (value != null)
                {
                    Entity.AccuredAmountTypeId = value.Id;
                    RaisePropertyChanged();
                }
            }
        }

        public decimal Summa
        {
            get => Entity.Summa;
            set
            {
                if (Entity.Summa == value) return;
                Entity.Summa = value;
                RaisePropertyChanged();
            }
        }

        public SDRSchet SDRSchet
        {
            get => mySDRSchet;
            set
            {
                if (mySDRSchet == value) return;
                mySDRSchet = value;
                Entity.SHPZ_DC = value?.DocCode;
                RaisePropertyChanged();
            }
        }

        public override string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Constructors

        public AccruedAmountOfSupplierRowViewModel(AccuredAmountOfSupplierRow entity, 
            AccruedAmountOfSupplierViewModel parent = null)
        {
            Entity = entity ?? DefaultValue();
            if (parent != null)
            {
                Parent = parent;
                Entity.DocId = parent.Id;
            }
        }

        private AccuredAmountOfSupplierRow DefaultValue()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(AccruedAmountType):
                        return AccruedAmountType == null ? "Тип начисления должен быть обязательно выбран" : null;
                    case nameof(Summa):
                        return Summa <= 0 ? "Сумма должна быть больше 0" : null;
                    default:
                        return null;
                }
            }
        }

        public string Error => null;

        #endregion
    }
}