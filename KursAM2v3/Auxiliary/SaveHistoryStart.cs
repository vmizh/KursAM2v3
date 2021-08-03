using System.Linq;
using Core;
using Core.EntityViewModel.Bank;
using Core.EntityViewModel.Cash;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Dogovora;
using Core.EntityViewModel.Invoices;
using Core.EntityViewModel.NomenklManagement;
using Core.EntityViewModel.Vzaimozachet;
using Core.Helper;
using Core.Invoices.EntityViewModel;
using Data;
using Data.Repository;
using Helper;

namespace KursAM2.Auxiliary
{
    public class SaveHistoryStart
    {
        public static void SaveHistory()
        {
            var ctx = GlobalOptions.GetEntities();

            #region Касса

            var cashIn = ctx.SD_33.ToList();
            foreach (var doc in cashIn.Select(ent => new CashIn(ent)))
            {
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.CashIn), null,
                    doc.DocCode, null, (string)doc.ToJson());
            }

            var cashOut = ctx.SD_34.ToList();
            foreach (var doc in cashOut.Select(ent => new CashOut(ent)))
            {
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.CashOut), null,
                    doc.DocCode, null, (string)doc.ToJson());
            }

            var cashExch = ctx.SD_251.ToList();
            foreach (var doc in cashExch.Select(ent => new CashCurrencyExchange(ent)))
            {
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.CurrencyChange), null,
                    doc.DocCode, null, (string)doc.ToJson());
            }

            #endregion

            #region Счета

            var invoiceSupplier = ctx.SD_26.ToList();
            foreach (var doc in invoiceSupplier.Select(ent => new InvoiceProvider(ent,new UnitOfWork<ALFAMEDIAEntities>(ctx))))
            {
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.InvoiceProvider), null,
                    doc.DocCode, null, (string)doc.ToJson());
            }

            var invoiceClient = ctx.SD_84.ToList();
            foreach (var doc in invoiceClient.Select(ent => new InvoiceClient(ent,new UnitOfWork<ALFAMEDIAEntities>(ctx))))
            {
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.InvoiceClient), null,
                    doc.DocCode, null, (string)doc.ToJson());
            }

            #endregion

            #region Договора

            var DogovorClient = ctx.DogovorClient.ToList();
            foreach (var doc in DogovorClient.Select(ent => new DogovorClientViewModel(ent)))
            {
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.DogovorClient), null,
                    doc.DocCode, null, (string)doc.ToJson());
            }

            var DogovorOfSupplier = ctx.DogovorOfSupplier.ToList();
            foreach (var doc in DogovorOfSupplier.Select(ent => new DogovorOfSupplierViewModel(ent)))
            {
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.DogovorOfSupplier), null,
                    doc.DocCode, null, (string)doc.ToJson());
            }

            #endregion

            #region Склад

            var orderIn = ctx.SD_24.Where(_ => _.DD_TYPE_DC == 2010000001).ToList();
            foreach (var doc in orderIn.Select(ent => new WarehouseOrderIn(ent)))
            {
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.StoreOrderIn), null,
                    doc.DocCode, null, (string)doc.ToJson());
            }

            var wayBill = ctx.SD_24.Where(_ => _.DD_TYPE_DC == 2010000012).ToList();
            foreach (var doc in wayBill.Select(ent => new Waybill(ent)))
            {
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.Waybill), null,
                    doc.DocCode, null, (string)doc.ToJson());
            }

            #endregion

            #region Зачеты

            var akt = ctx.SD_110.ToList();
            foreach (var doc in akt.Select(ent => new SD_110ViewModel(ent)))
            {
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.MutualAccounting), null,
                    doc.DocCode, null, (string)doc.ToJson());
            }

            #endregion

            #region Банк

            var bank = ctx.TD_101.ToList();
            foreach (var doc in bank.Select(ent => new BankOperationsViewModel(ent)))
            {
                DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.Bank), null,
                    doc.DocCode, doc.Code, (string)doc.ToJson());
            }

            #endregion
        }
        
    }
}