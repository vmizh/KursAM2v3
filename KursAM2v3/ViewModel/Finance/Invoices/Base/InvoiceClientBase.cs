using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core;
using Data;
using KursDomain;
using KursDomain.Documents.Invoices;
using KursDomain.Documents.Vzaimozachet;
using KursDomain.References;

namespace KursAM2.ViewModel.Finance.Invoices.Base
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public class InvoiceClientBase : IInvoiceClient
    {
        public InvoiceClientBase(SD_84 entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
        }

        public InvoiceClientBase(IEnumerable<InvoiceClientQuery> invList, bool isLoadDetails = false)
        {
            if (invList == null || !invList.Any())
                throw new ArgumentNullException();
            var doc = invList.First();
            DocCode = doc.DocCode;
            Id = doc.Id;
            DocDate = doc.DocDate;
            Receiver = GlobalOptions.ReferencesCache.GetKontragent(doc.ReceiverDC) as Kontragent;
            CO = MainReferences.GetCO(doc.CentOtvetstDC);
            VzaimoraschetType = MainReferences.GetVzaimoraschetType(doc.VzaimoraschetTypeDC);
            PayCondition = MainReferences.GetPayCondition(doc.PayConditionDC);
            InnerNumber = doc.InnerNumber;
            OuterNumber = doc.OuterNumber;
            Client = GlobalOptions.ReferencesCache.GetKontragent(doc.ClientDC) as Kontragent;
            Currency = MainReferences.GetCurrency(doc.CurrencyDC);
            SummaOtgruz = Math.Round(doc.SummaOtgruz ?? 0, 2);
            DilerSumma = Math.Round(doc.DilerSumma, 2);
            Note = doc.Note;
            Diler = GlobalOptions.ReferencesCache.GetKontragent(doc.DilerDC) as Kontragent;
            IsAccepted = doc.IsAccepted ?? false;
            Summa = Math.Round(doc.Summa ?? 0, 2);
            CREATOR = doc.CREATOR;
            IsNDSIncludeInPrice = doc.IsNDSIncludeInPrice ?? false;
            PaySumma = Math.Round(doc.PaySumma ?? 0, 2);
            if (!isLoadDetails) return;
            Rows = new ObservableCollection<IInvoiceClientRow>();
            foreach (var r in invList) Rows.Add(new InvoiceClientRowBase(r));
        }

        public PayCondition PayCondition { get; set; }

        public Kontragent Receiver { get; set; }
        public Kontragent Client { get; set; }
        public Kontragent Diler { get; set; }

        public decimal DocCode { get; set; }
        public Guid Id { get; set; }
        public CentrResponsibility CO { get; set; }
        public VzaimoraschetType VzaimoraschetType { get; set; }
        public PayForm FormRaschet { get; set; }
        public DateTime DocDate { get; set; }
        public int InnerNumber { get; set; }
        public string OuterNumber { get; set; }
        public Currency Currency { get; set; }
        public decimal SummaOtgruz { get; set; }
        public decimal DilerSumma { get; set; }
        public string Note { get; set; }
        public bool IsAccepted { get; set; }
        public decimal Summa { get; set; }
        public string CREATOR { get; set; }
        public bool IsNDSIncludeInPrice { get; set; }
        public decimal PaySumma { get; set; }
        public Employee PersonaResponsible { get; set; }
        public ObservableCollection<IInvoiceClientRow> Rows { get; set; }
    }

    public class InvoiceClientRowBase : IInvoiceClientRow
    {
        public InvoiceClientRowBase(InvoiceClientQuery row)
        {
            DocCode = row.DocCode;
            Code = row.RowCode;
            Id = row.Row2d;
            DocId = DocId;
            Nomenkl = MainReferences.GetNomenkl(row.NomenklDC);
            NomNomenkl = Nomenkl.NomenklNumber;
            IsUsluga = Nomenkl.IsUsluga;
            Quantity = row.Quantity ?? 0;
            Price = row.Price ?? 0;
            Summa = Quantity * Price;
            SFT_NACENKA_DILERA = row.SFT_NACENKA_DILERA;
            Shipped = row.Shipped;
            Rest = Quantity - Shipped;
            CurrentRemains = 0;
            NDSPercent = row.NDSPercent ?? 0;
            SFT_SUMMA_NDS = row.SFT_SUMMA_NDS;
        }

        public InvoiceClientRowBase()
        {
        }

        public InvoiceClientRowBase(decimal docCode, int code, Guid id, Guid docId, Nomenkl nomenkl, string nomNomenkl,
            bool isUsluga,
            decimal quantity, decimal price, decimal summa, decimal? sFT_NACENKA_DILERA, decimal shipped,
            decimal rest, decimal currentRemains, string note, decimal nDSPercent, decimal? sFT_SUMMA_NDS,
            SDRSchet sDRSchet, string gruzoDeclaration)
        {
            DocCode = docCode;
            Code = code;
            Id = id;
            DocId = docId;
            Nomenkl = nomenkl;
            NomNomenkl = nomNomenkl;
            IsUsluga = isUsluga;
            Quantity = quantity;
            Price = price;
            Summa = summa;
            SFT_NACENKA_DILERA = sFT_NACENKA_DILERA;
            Shipped = shipped;
            Rest = rest;
            CurrentRemains = currentRemains;
            Note = note;
            NDSPercent = nDSPercent;
            SFT_SUMMA_NDS = sFT_SUMMA_NDS;
            SDRSchet = sDRSchet;
            GruzoDeclaration = gruzoDeclaration;
        }

        public Nomenkl Nomenkl { get; set; }

        public decimal DocCode { get; set; }
        public int Code { get; set; }
        public Guid Id { get; set; }
        public Guid DocId { get; set; }
        public string NomNomenkl { get; set; }
        public bool IsUsluga { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Summa { get; set; }
        public decimal? SFT_NACENKA_DILERA { get; set; }
        public decimal Shipped { get; set; }
        public decimal Rest { get; set; }
        public decimal CurrentRemains { get; set; }
        public string Note { get; set; }
        public decimal NDSPercent { get; set; }
        public decimal? SFT_SUMMA_NDS { get; set; }
        public SDRSchet SDRSchet { get; set; }
        public string GruzoDeclaration { get; set; }
    }
}
