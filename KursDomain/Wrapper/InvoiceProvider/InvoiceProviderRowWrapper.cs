using System;
using Data;
using KursDomain.ICommon;
using KursDomain.Wrapper.Base;
using Prism.Events;

namespace KursDomain.Wrapper.InvoiceProvider;

public class InvoiceProviderRowWrapper : BaseWrapper<TD_26>, IEquatable<InvoiceProviderRowWrapper>
{
    public InvoiceProviderRowWrapper(TD_26 model, IEventAggregator eventAggregator,
        IMessageDialogService messageDialogService) : base(model, eventAggregator, messageDialogService)
    {
    }

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

    public override int Code
    {
        get => Model.CODE;
        set
        {
            if (Model.CODE == value) return;
            Model.CODE = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    public bool Equals(InvoiceProviderRowWrapper other)
    {
        if (other == null) return false;
        return DocCode == other.DocCode && Code == other.Code;
    }
}
