using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core;
using Data;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Invoices;
using KursDomain.References;
using Employee = KursDomain.Documents.Employee.Employee;
using PayCondition = KursDomain.Documents.CommonReferences.PayCondition;
using SDRSchet = KursDomain.Documents.CommonReferences.SDRSchet;

namespace KursAM2.ViewModel.Finance.Invoices.Base
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public class InvoiceProviderBase : IInvoiceProvider
    {
        public InvoiceProviderBase()
        {
        }

        public InvoiceProviderBase(SD_26 entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
        }

        public InvoiceProviderBase(IEnumerable<InvoicePostQuery> invList, bool isLoadDetails = false)
        {
            if (invList == null || !invList.Any()) throw new ArgumentNullException(nameof(invList));
            var doc = invList.First();
            DocCode = doc.DocCode;
            Id = doc.Id;
            NakladDistributedSumma = doc.NakladDistributedSumma;
            PersonaResponsible = MainReferences.GetEmployee(doc.PersonalResponsibleDC);
            SF_IN_NUM = doc.InNum;
            SF_POSTAV_NUM = doc.PostavNum;
            DocDate = doc.Date;
            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(doc.PostDC) as Kontragent;
            Summa = doc.Summa ?? 0;
            SummaFact = doc.ShippedSumma;
            Currency = MainReferences.GetCurrency(doc.CurrencyDC);
            PaySumma = doc.PaySumma;
            IsPay = Summa <= PaySumma;
            PayCondition = MainReferences.GetPayCondition(doc.SF_PAY_COND_DC);
            IsAccepted = doc.IsAccepted ?? false;
            Note = doc.Note;
            CREATOR = doc.Creator;
            FormRasche = MainReferences.GetFormPay(doc.FormRaschetDC);
            IsNDSInPrice = doc.IsNDSInPrice ?? false;
            CO = MainReferences.GetCO(doc.CO_DC);
            if (!isLoadDetails) return;
            Rows = new ObservableCollection<IInvoiceProviderRow>();
            foreach (var r in invList) Rows.Add(new InvoiceProviderRowBase(r));
        }

        public Currency Currency { get; set; }


        public decimal DocCode { get; set; }
        public Guid Id { get; set; }
        public decimal? NakladDistributedSumma { get; set; }
        public Employee PersonaResponsible { get; set; }
        public int? SF_IN_NUM { get; set; }
        public string SF_POSTAV_NUM { get; set; }
        public DateTime DocDate { get; set; }
        public Kontragent Kontragent { get; set; }
        public decimal Summa { get; set; }
        public decimal SummaFact { get; set; }
        public bool IsPay { get; set; }
        public decimal PaySumma { get; set; }
        public PayCondition PayCondition { get; set; }
        public bool IsAccepted { get; set; }
        public string Note { get; set; }
        public string CREATOR { get; set; }
        public FormPay FormRasche { get; set; }
        public bool IsNDSInPrice { get; set; }
        public CentrResponsibility CO { get; set; }
        public Kontragent KontrReceiver { get; set; }
        public ObservableCollection<IInvoiceProviderRow> Rows { get; set; }
    }

    public class InvoiceProviderRowBase : IInvoiceProviderRow
    {
        public InvoiceProviderRowBase(InvoicePostQuery row)
        {
            DocCode = row.DocCode;
            Code = row.CODE;
            Id = row.RowId;
            DocId = row.Id;
            var Nomenkl = MainReferences.GetNomenkl(row.NomenklDC);
            var Unit = Nomenkl?.Unit;
            Price = row.Price ?? 0;
            Quantity = row.Quantity;
            SFT_NDS_PERCENT = row.NDSPercent;
            SFT_SUMMA_NAKLAD = row.NakladSumma;
            SFT_SUMMA_NDS = row.NDSSumma;
            SFT_SUMMA_K_OPLATE = row.Summa;
            Summa = row.Summa ?? 0;
            IsUsluga = Nomenkl.IsUsluga;
            IsNaklad = Nomenkl.IsNakladExpense;
            IsIncludeInPrice = row.IsNDSInPrice ?? false;
            SFT_SUMMA_K_OPLATE_KONTR_CRS = row.Summa;
        }

        public InvoiceProviderRowBase()
        {
        }


        public decimal DocCode { get; set; }
        public int Code { get; set; }
        public Guid Id { get; set; }
        public Guid DocId { get; set; }
        public string Note { get; set; }
        public Unit PostUnit { get; set; }
        public Unit UchUnit { get; set; }
        public decimal SFT_POST_KOL { get; set; }
        public Nomenkl Nomenkl { get; set; }
        public string NomenklNumber { get; set; }
        public Unit Unit { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal SFT_NDS_PERCENT { get; set; }
        public decimal? SFT_SUMMA_NAKLAD { get; set; }
        public decimal? SFT_SUMMA_NDS { get; set; }
        public decimal? SFT_SUMMA_K_OPLATE { get; set; }
        public decimal Summa { get; set; }
        public bool IsUsluga { get; }
        public bool IsNaklad { get; }
        public bool IsIncludeInPrice { get; set; }
        public decimal? SFT_SUMMA_K_OPLATE_KONTR_CRS { get; set; }
        public SDRSchet SDRSchet { get; set; }
    }
}
