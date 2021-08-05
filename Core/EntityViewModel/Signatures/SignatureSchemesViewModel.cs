using System;
using Core.EntityViewModel.Systems;
using Core.ViewModel.Base;
using Data;
using KursRepositories.ViewModels;

namespace Core.EntityViewModel.Signatures
{
    public class SignatureSchemesViewModel : RSViewModelBase, IEntity<SignatureSchemes>
    {
        #region Fields

        private DataSourceViewModel myDataSource;
        private KursMenuItemViewModel myDocumentType;

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

        public DataSourceViewModel DataSource
        {
            get
            {
                if (myDataSource != null) return myDataSource;
                if (Entity.DataSources != null)
                {
                    myDataSource = new DataSourceViewModel(Entity.DataSources);
                    RaisePropertyChanged();
                    return myDataSource;
                } 
                return null;
            }
            set
            {
                if (myDataSource == value) return;
                myDataSource = value;
                Entity.DataSources = myDataSource.Entity;
                RaisePropertyChanged();
            }
        }
        
        public KursMenuItemViewModel DocumentType
        {
            get
            {
                if (myDocumentType != null) return myDocumentType;
                if (Entity.KursMenuItem != null)
                {
                    myDocumentType = new KursMenuItemViewModel(Entity.KursMenuItem);
                    RaisePropertyChanged();
                    return myDocumentType;
                } 
                return null;
            }
            set
            {
                if (myDocumentType == value) return;
                myDocumentType = value;
                Entity.KursMenuItem = myDocumentType.Entity;
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

        #endregion
    }
}
