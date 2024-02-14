using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using KursDomain.Base;
using KursDomain.References;
using KursDomain.Wrapper;
using KursDomain.Wrapper.TransferOut;

namespace KursDomain.IDocuments.TransferOut;

public interface ITransferOutBalans
{
    public Guid Id { get; set; }
    public DateTime DocDate { get; set; }
    public int DocNum { get; set; }
    public string Note { get; set; }
    public string Creator { get; set; }

    ObservableCollection<CurrencyValue> CurrenciesSummaries { get; set; }

    public Warehouse Warehouse { get; set; }

    public ICollection<TransferOutBalansRowsWrapper> Rows { get; set; }

    public StorageLocationsWrapper StorageLocation { get; set; }
}
