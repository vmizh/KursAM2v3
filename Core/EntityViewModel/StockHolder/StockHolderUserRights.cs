using System;
using System.ComponentModel;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.StockHolder
{

    public class StockHolderUserRightsViewModel : RSViewModelBase, IDataErrorInfo, IEntity<StockHolderUserRights>
    {
        #region Methods
        
        #endregion

        #region Constructors

        public StockHolderUserRightsViewModel(StockHolderUserRights entity)
        {
            Entity = entity ?? DefaultValue();
        }

        #endregion

        #region Properties

         public StockHolderUserRights Entity { get; set; }

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

         #endregion

        #region Methods

        public StockHolderUserRights DefaultValue()
        {
            return new StockHolderUserRights { Id = Guid.NewGuid()};

        }

        #endregion



        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Name):
                        return Name == null ? "Наименование не может быть пустым" : null;
                    default:
                        return null;
                }
            }
        }

        public string Error => null;

        #endregion
    }
}