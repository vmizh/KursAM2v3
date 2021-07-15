using System;

namespace Core.ViewModel.Base
{
    public interface ISimpleDialogItem
    {
        decimal DocCode { set; get; }
        Guid Id { set; get; }
        int Code { set; get; }
        string Name { set; get; }
    }
}