using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KursDomain.IDocuments.Finance;

public interface IInvoiceProvider
{
    IEnumerable<IInvoiceProviderRow> Rows { set; get; }
}

public interface IInvoiceProviderRow
{
}

public class InvoiceProvider : IInvoiceProvider
{
    public IEnumerable<IInvoiceProviderRow> Rows { get; set; } = new ObservableCollection<IInvoiceProviderRow>();
}
