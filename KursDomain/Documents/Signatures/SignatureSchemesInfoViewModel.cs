using System;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;

namespace KursDomain.Documents.Signatures;

public class SignatureSchemesInfoViewModel : RSViewModelBase, IEntity<SignatureSchemesInfo>
{
    #region Methods

    public SignatureSchemesInfo DefaultValue()
    {
        return new SignatureSchemesInfo
        {
            Id = Guid.NewGuid()
        };
    }

    #endregion

    #region Fields

    private SignatureSchemesInfoViewModel myParentSignature;
    private SignatureTypeViewModel mySignatureType;

    #endregion

    #region Properties

    [Display(AutoGenerateField = false)] public SignatureSchemesInfo Entity { get; set; }

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = false)]
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

    [Display(AutoGenerateField = true, Name = "Тип подписи")]
    public SignatureTypeViewModel SignatureType
    {
        get => mySignatureType;
        set
        {
            if (mySignatureType == value) return;
            mySignatureType = value;
            Entity.SignatureType = mySignatureType.Entity;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)] public override string Name => mySignatureType?.Name ?? "Не указан";

    [Display(AutoGenerateField = true, Name = "Обязателен")]
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

    [Display(AutoGenerateField = true, Name = "Примечание")]
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
        if (Entity.SignatureType != null)
            SignatureType = new SignatureTypeViewModel(Entity.SignatureType);
    }

    #endregion
}
