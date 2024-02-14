using System;
using KursDomain.References;

namespace KursDomain.IDocuments.TransferOut;

public interface ITransferOutBalansRows
{
    public Guid Id { get; set; }
    public Guid DocId { get; set; }
    public Nomenkl Nomenkl { get; set; }
    public decimal Quatntity { get; set; }
    public decimal Price { get; set; }
    public decimal Summa { get; }
    public string Note { get; set; }

    public ITransferOutBalans TransferOutBalans { get; set; }
}
