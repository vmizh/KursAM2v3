using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using DevExpress.Data.ODataLinq.Helpers;
using KursDomain.Documents.Invoices;
using KursDomain.IDocuments.Finance;
using KursDomain.IReferences;
using KursDomain.Repository;

namespace KursAM2.ViewModel.Finance.NomenklCurrencyConvert
{
    public interface INomenklCurrencyConvertRepository
    {
        IEnumerable<IInvoiceProvider> GetInvoicesForNomCurrencyConvert(DateTime dateStart, DateTime dateEnd);
        IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(DateTime dateStart, DateTime dateEnd);
        IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(IInvoiceProviderRow invoiceRow);
        IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(IInvoiceProvider invoice);
        IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(decimal invoiceDC);
        IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(Guid invoiceId);
        IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(decimal invoiceDC, int code);

        void Save(IEnumerable<ISFProviderNomenklCurrencyConvert> rows);
    }

    public class NomenklCurrencyConvertRepository : GenericKursDBRepository<TD_26_CurrencyConvert>,
        INomenklCurrencyConvertRepository
    {
        private bool isViewModel;
        private IReferencesCache cache;

        public NomenklCurrencyConvertRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
        }

        public NomenklCurrencyConvertRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public IEnumerable<IInvoiceProvider> GetInvoicesForNomCurrencyConvert(DateTime dateStart, DateTime dateEnd)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(DateTime dateStart,
            DateTime dateEnd)
        {
            List<ISFProviderNomenklCurrencyConvert> result = new List<ISFProviderNomenklCurrencyConvert>();
            var data = Context.TD_26_CurrencyConvert.Where(_ => _.Date >= dateStart
                                                                && _.Date <= dateEnd);
            foreach (var d in data)
            {
                if (isViewModel)
                {
                    var newItem = new SFProviderNomenklCurrencyConvert();
                    newItem.LoadFromEntity(d,cache);
                }
            }

            return result;
        }

        public IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(IInvoiceProviderRow invoiceRow)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(IInvoiceProvider invoice)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(decimal invoiceDC)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(Guid invoiceId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISFProviderNomenklCurrencyConvert> GetNomenklCurrencyConvert(decimal invoiceDC, int code)
        {
            throw new NotImplementedException();
        }

        public void Save(IEnumerable<ISFProviderNomenklCurrencyConvert> rows)
        {
            throw new NotImplementedException();
        }
    }
}
