using System;
using System.ComponentModel;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.StockHolder
{
    public class StockHolderAccrualRowViewModel : RSViewModelBase, IDataErrorInfo, IEntity<StockHolderAccrualRows>
    {
        #region Fields

        private StockHolderAccrualTypeViewModel myAccrualType;
        private StockHolderViewModel myStockHolder;

        #endregion

        #region Constructors

        public StockHolderAccrualRowViewModel(StockHolderAccrualRows entity)
        {
            Entity = entity ?? DefaultValue();
            LoadRefernces();
        }

        #endregion

        #region Properties

        public StockHolderAccrualRows Entity { get; set; }

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

        public new StockHolderAccrual Parent
        {
            get => (StockHolderAccrual)myParent;
            set
            {
                if (myParent == value) return;
                myParent = value;
                if (myParent is StockHolderAccrual p)
                {
                    Entity.DocId = p.Id;
                }
                RaisePropertyChanged();
            }
        }
        
        public StockHolderViewModel StockHolder
        {
            get => myStockHolder;
            set
            {
                if (myStockHolder == value) return;
                myStockHolder = value;
                if (myStockHolder != null)
                    Entity.StockHolderId = myStockHolder.Id;
                RaisePropertyChanged();
            }
        }
        
        public StockHolderAccrualTypeViewModel AccrualType
        {
            get => myAccrualType;
            set
            {
                if (myAccrualType == value) return;
                myAccrualType = value;
                if (myAccrualType != null)
                    Entity.AcrrualTypeId = myAccrualType.Id;
                RaisePropertyChanged();
            }
        }

        public Currency Currency
        {
            get => MainReferences.GetCurrency(Entity.CurrencyDC);
            set
            {
                if (MainReferences.GetCurrency(Entity.CurrencyDC) == value) return;
                Entity.CurrencyDC = value?.DocCode;
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
        public decimal Summa
        {
            get => Entity.Summa ?? 0;
            set
            {
                if (Entity.Summa == value) return;
                Entity.Summa = value;
                RaisePropertyChanged();
            }
        }

       
        #endregion

        #region Methods 

        public StockHolderAccrualRows DefaultValue()
        {
            return new StockHolderAccrualRows
            {
                Id = Guid.NewGuid(),
                Summa = 0
            };
        }

        private void LoadRefernces()
        {
            AccrualType = new StockHolderAccrualTypeViewModel(Entity.StockHolderAccrualType);
            StockHolder = new StockHolderViewModel(Entity.StockHolders);
        }

        #endregion

        #region IDztaErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    //case nameof(Note):
                    //    return Name == null ? "Примечание не может быть пустым" : null;
                    default:
                        return null;
                }
            }
        }

        public string Error => null;

        #endregion
    }
}