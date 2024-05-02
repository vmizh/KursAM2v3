using System;
using Data;
using KursDomain.ICommon;
using KursDomain.Wrapper.Base;
using Prism.Events;

namespace KursDomain.Wrapper.InvoiceClient;

public class InvoiceClientWrapper : BaseWrapper<SD_84>, IEquatable<InvoiceClientWrapper>
{
    #region Constructors
    public InvoiceClientWrapper(SD_84 model, IEventAggregator eventAggregator, IMessageDialogService messageDialogService) 
        : base(model, eventAggregator, messageDialogService)
    {
    }
    #endregion


    #region Properties

    public override decimal DocCode
    {
        get => Model.DOC_CODE;
        set
        {
            if (Model.DOC_CODE == value) return;
            Model.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Methods

    public bool Equals(InvoiceClientWrapper other)
    {
        if (other == null) return false;
        return DocCode == other.DocCode;
    }

    #endregion
}
