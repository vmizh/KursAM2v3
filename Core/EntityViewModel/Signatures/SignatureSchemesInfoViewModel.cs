using System;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Signatures
{
    public class SignatureSchemesInfoViewModel  : RSViewModelBase, IEntity<SignatureSchemesInfo>
    {
        #region Fields
        
        private SignatureSchemesInfoViewModel myParentSignature;
        private SignatureTypeViewModel mySignatureType;

        #endregion

        #region Properties

        public SignatureSchemesInfo Entity { get; set; }

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

        public override Guid? ParentId
        {
            get => Entity.ParentId;
            set
            {
                if (Entity.ParentId == value) return;
                Entity.ParentId = value;
                RaisePropertyChanged();
            }
        }

        public SignatureSchemesInfoViewModel ParentSignature
        {
            get
            {
                if (myParentSignature != null) return myParentSignature;
                if (Entity.SignatureSchemesInfo2 != null)
                {
                    myParentSignature = new SignatureSchemesInfoViewModel(Entity.SignatureSchemesInfo2);
                    RaisePropertyChanged();
                    return myParentSignature;
                } 
                return null;
            }
            set
            {
                if (myParentSignature == value) return;
                myParentSignature = value;
                Entity.SignatureSchemesInfo2 = myParentSignature.Entity;
                RaisePropertyChanged();
            }
        }
        public SignatureTypeViewModel SignatureType
        {
            get
            {
                if (mySignatureType != null) return mySignatureType;
                if (Entity.SignatureType != null)
                {
                    mySignatureType = new SignatureTypeViewModel(Entity.SignatureType);
                    RaisePropertyChanged();
                    return mySignatureType;
                } 
                return null;
            }
            set
            {
                if (mySignatureType == value) return;
                mySignatureType = value;
                Entity.SignatureType = mySignatureType.Entity;
                RaisePropertyChanged();
            }
        }

        public bool IsRequred
        {
            get => Entity.IsRequired;
            set
            {
                if (Entity.IsRequired == value) return;
                Entity.IsRequired = value;
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

        public SignatureSchemesInfoViewModel()
        {
            Entity = DefaultValue();
        }

        public SignatureSchemesInfoViewModel(SignatureSchemesInfo entity)
        {
            Entity = entity ?? DefaultValue();
        }

        #endregion

        #region Methods

        public SignatureSchemesInfo DefaultValue()
        {
            return new()
            {
                Id = Guid.NewGuid()
            };
        }

        #endregion
        
    }
}