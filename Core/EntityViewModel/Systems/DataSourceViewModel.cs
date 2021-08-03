using System;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Systems
{
    public class DataSourceViewModel : RSViewModelBase, IEntity<DataSources>

    {
        #region Methods

        public DataSources DefaultValue()
        {
            return new()
            {
                Id = Guid.NewGuid()
            };
        }

        #endregion

        #region Fields

        #endregion

        #region Properties

        public DataSources Entity { get; set; }

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

        public override string Name
        {
            get => Entity.Name;
            set
            {
                if (Entity.Name == value) return;
                Entity.Name = value;
                RaisePropertyChanged();
            }
        }

        public string ShowName
        {
            get => Entity.ShowName;
            set
            {
                if (Entity.ShowName == value) return;
                Entity.ShowName = value;
                RaisePropertyChanged();
            }
        }


        public int? Order
        {
            get => Entity.Order;
            set
            {
                if (Entity.Order == value) return;
                Entity.Order = value;
                RaisePropertyChanged();
            }
        }

        
        public string Server
        {
            get => Entity.Server;
            set
            {
                if (Entity.Server == value) return;
                Entity.Server = value;
                RaisePropertyChanged();
            }
        }

        public string DBName
        {
            get => Entity.DBName;
            set
            {
                if (Entity.DBName == value) return;
                Entity.DBName = value;
                RaisePropertyChanged();
            }
        }

        public string Color
        {
            get => Entity.Color;
            set
            {
                if (Entity.Color == value) return;
                Entity.Color = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Constructors

        public DataSourceViewModel()
        {
            Entity = DefaultValue();
        }

        public DataSourceViewModel(DataSources entity)
        {
            Entity = entity ?? DefaultValue();
        }

        #endregion
    }
}