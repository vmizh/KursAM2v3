using System;
using Data;
using KursDomain.ICommon;
using KursDomain.Wrapper.Base;
using Prism.Events;

namespace KursDomain.Wrapper.InvoiceClient;

public class InvoiceClientRowWrapper : BaseWrapper<TD_84>, IEquatable<InvoiceClientRowWrapper>
{
    public InvoiceClientRowWrapper(TD_84 model, IEventAggregator eventAggregator, IMessageDialogService messageDialogService) 
        : base(model,eventAggregator, messageDialogService)
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

    public bool Equals(InvoiceClientRowWrapper other)
    {
        if (other == null) return false;
        return DocCode == other.DocCode && Code == other.Code;
    }
}
