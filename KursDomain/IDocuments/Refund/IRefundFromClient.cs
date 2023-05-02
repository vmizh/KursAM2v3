using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.IReferences.Nomenkl;
using KursDomain.References;

namespace KursDomain.IDocuments.Refund;

public interface IRefundFromClientRow : IRowId
{
    INomenkl Nomenkl { set; get; }
    decimal Quantity { set; get; }
    WaybillRow WaybillRow { set; get; }
    decimal FactPrice { set; get; }
    string Note { set; get; }
  
    IRefundFromClient Parent { set; get; }
}

public interface IRefundFromClient : IDocGuid
{
    int Num { get; set; }
    DateTime Date { get; set; }
    IWarehouse Warehouse { set; get; }
    IKontragent Client { set; get; }
    string Note { set; get; }
    string Creator { set; get; }

    IEnumerable<IRefundFromClientRow> Rows { get; }

}
