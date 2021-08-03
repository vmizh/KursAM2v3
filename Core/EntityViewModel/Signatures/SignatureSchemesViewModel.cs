using System;
using Core.EntityViewModel.Systems;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Signatures
{
    public class SignatureSchemesViewModel : RSViewModelBase, IEntity<SignatureSchemes>
    {
        #region Fields

        private DataSourceViewModel myDataSource;

        #endregion

        #region Properties

        public SignatureSchemes Entity { get; set; }

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

        public SignatureSchemesViewModel()
        {
            Entity = DefaultValue();
        }

        public SignatureSchemesViewModel(SignatureSchemes entity)
        {
            Entity = entity ?? DefaultValue();
        }

        #endregion

        #region Methods

        public SignatureSchemes DefaultValue()
        {
            return new()
            {
                Id = Guid.NewGuid()
            };
        }

        public DataSourceViewModel DataSource
        {
            get
            {
                if (myDataSource != null) return myDataSource;
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

        #endregion
    }
}
