using System;
using System.Collections.Generic;
using Data;
using KursDomain.References;

namespace KursDomain.IDocuments.TransferOut;

public interface ITransferOutBalansRefund
{
    public Guid Id { get; set; }
    public DateTime DocDate { get; set; }
    public int DocNum { get; set; }
    public Warehouse Warehouse { get; set; }
    public string Creator { get; set; }
    public string Note { get; set; }

    public ICollection<ITransferOutBalansRefundRows> Rows { get; set; }

    public StorageLocations StorageLocation { get; set; }
}
