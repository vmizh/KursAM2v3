using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.XtraPrinting.Native;
using KursDomain;
using KursDomain.IDocuments.Finance;
using KursDomain.IReferences;

namespace KursAM2.ViewModel.Finance.NomenklCurrencyConvert
{
    public class NomenklCurrencyConvertWindowViewModel
    {
        public IInvoiceProvider Invoice { set; get; }
        public List<IInvoiceProviderRow> NomenklFrom { set; get; }

        public ObservableCollection<ISFProviderNomenklCurrencyConvert> NomenklTo { set; get; }
            = new ObservableCollection<ISFProviderNomenklCurrencyConvert>();

        private IReferencesCache refCache;
        public NomenklCurrencyConvertWindowViewModel()
        {
            refCache = GlobalOptions.ReferencesCache;
        }
        public NomenklCurrencyConvertWindowViewModel(IReferencesCache cache)
        {
            refCache = cache;
        }
        public NomenklCurrencyConvertWindowViewModel(IInvoiceProvider invoice, IReferencesCache cache) : this(cache)
        {
            Invoice = invoice;
        }

        
    }
}
