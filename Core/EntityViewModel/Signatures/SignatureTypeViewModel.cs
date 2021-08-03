using System;
using System.Collections.ObjectModel;
using Core.EntityViewModel.Systems;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Signatures
{
    public class SignatureTypeViewModel : RSViewModelBase, IEntity<SignatureType>
    {
        #region Fields

        private DataSourceViewModel myDataSource;

        #endregion

        #region Properties

        public SignatureType Entity { get; set; }

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

        #region Constructors

        public SignatureTypeViewModel()
        {
            Entity = DefaultValue();
        }

        public SignatureTypeViewModel(SignatureType entity)
        {
            Entity = entity ?? DefaultValue();
        }

        #endregion

        #region Methods

        public SignatureType DefaultValue()
        {
            return new SignatureType
            {
                Id = Guid.NewGuid(),
            };
        }
        
        public DataSourceViewModel DataSource
        {
            get
            {
                if(myDataSource != null) return myDataSource;
                if (Entity.DataSources != null)
                {
                    myDataSource = new DataSourceViewModel(Entity.DataSources);
                    return myDataSource;
                }
                return null;

            }
            set
            {
                if (myDataSource == value) return;
                myDataSource = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<UserViewModel> Users { set; get; } = new ObservableCollection<UserViewModel>();

        #endregion
    }
}