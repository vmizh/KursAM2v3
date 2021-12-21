using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.StockHolder
{
    public class StockHolderAccrualViewModel : RSViewModelBase, IDataErrorInfo, IEntity<StockHolderAccrual>
    {
        #region Fields

        #endregion

        #region Constructors

        public StockHolderAccrualViewModel(StockHolderAccrual entity)
        {
            Entity = entity ?? DefaultValue();
            LoadReference();
        }
        
        #endregion

        #region Properties

         public StockHolderAccrual Entity { get; set; }

         public ObservableCollection<StockHolderAccrualRowViewModel> Rows { set; get; } =
             new ObservableCollection<StockHolderAccrualRowViewModel>();

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

        public int Num
        {
            get => Entity.Num;
            set
            {
                if (Entity.Num == value) return;
                Entity.Num = value;
                RaisePropertyChanged();
            }
        }


        public DateTime Date
        {
            get => Entity.Date;
            set
            {
                if (Entity.Date == value) return;
                Entity.Date = value;
                RaisePropertyChanged();
            }
        }

        public string Creator
        {
            get => Entity.Creator;
            set
            {
                if (Entity.Creator == value) return;
                Entity.Creator = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        private void LoadReference()
        {
            if (Entity.StockHolderAccrualRows != null && Entity.StockHolderAccrualRows.Count > 0)
            {
                foreach (var r in Entity.StockHolderAccrualRows)
                {
                  Rows.Add(new StockHolderAccrualRowViewModel(r));  
                }
            }
        }

        public StockHolderAccrual DefaultValue()
        {
            return new StockHolderAccrual
            {
                Id = Guid.NewGuid(),
                Creator = GlobalOptions.UserInfo.NickName,
                Date = DateTime.Today,
                Note = ""
            };
        }

        #endregion
        

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Note):
                        return Name == null ? "Примечание не может быть пустым" : null;
                    default:
                        return null;
                }
            }
        }

        public string Error => null;

        #endregion
    }
}