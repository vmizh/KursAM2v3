using System;
using System.Collections.Generic;
using Data;
using Data.Repository;
using DevExpress.Utils.Filtering;
using KursDomain.Documents.Invoices;
using KursDomain.IDocuments.Finance;

namespace KursAM2.ViewModel.Finance.NomenklCurrencyConvert
{
    public interface INomenklCurrencyConvertRepository
    {
        IEnumerable<IInvoiceProvider> GetInvoicesForNomCurrencyConvert(DateTime dateStart, DateTime dateEnd);

    }

    public class NomenklCurrencyConvertRepository : GenericKursDBRepository<TD_26_CurrencyConvert>,
        INomenklCurrencyConvertRepository
    {
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
    }
}
