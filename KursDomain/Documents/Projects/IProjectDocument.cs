using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.References;
using System;

namespace KursDomain.Documents.Projects;

public interface IProjectDocument : IDocGuid
{
        public Project Project { set; get; }
        public DocumentType DocumentType { set; get; }
        public string DocInfo { set; get; }
        public string Note { set; get; }
        public int? BankCode { get; set; }
        public decimal? CashInDC { get; set; }
        public decimal? CashOutDC { get; set; }
        public decimal? WarehouseOrderInDC { get; set; }
        public decimal? WaybillDC { get; set; }
        public Guid? AccruedClientRowId { get; set; }
        public Guid? AccruedSupplierRowId { get; set; }
        public References.Currency Currency { get; set; }
        public decimal SummaPrihod { get; set; }
        public decimal SummaRashod { set; get; }
}
