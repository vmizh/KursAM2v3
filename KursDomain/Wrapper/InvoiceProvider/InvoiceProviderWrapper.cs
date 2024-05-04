using Data;
using KursDomain.Wrapper.Base;
using System;
using KursDomain.ICommon;
using Prism.Events;

namespace KursDomain.Wrapper.InvoiceProvider;

public class InvoiceProviderWrapper : BaseWrapper<SD_26>, IEquatable<InvoiceProviderWrapper>
{
    public InvoiceProviderWrapper(SD_26 model, IEventAggregator eventAggregator,
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

   #endregion

    public bool Equals(InvoiceProviderWrapper other)
    {
        if (other == null) return false;
        return DocCode == other.DocCode;
    }
}
