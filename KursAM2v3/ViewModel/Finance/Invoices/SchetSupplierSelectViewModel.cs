using System;
using System.Collections.ObjectModel;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using KursAM2.View.Finance.UC;
using KursDomain;
using KursDomain.Documents.Invoices;

namespace KursAM2.ViewModel.Finance.Invoices
{
    public sealed class SchetSupplierSelectViewModel : RSWindowViewModelBase
    {
        private InvoiceProvider myCurrentInvoice;
        private SchetFacturaProviderSelectUC myDataUserControl;

        public SchetSupplierSelectViewModel()
        {
            myDataUserControl = new SchetFacturaProviderSelectUC();
            LoadReference();
        }

        public SchetSupplierSelectViewModel(string windowName) : this()
        {
            WindowName = windowName;
        }

        public ObservableCollection<InvoiceProvider> InvoiceCollection { set; get; } =
            new ObservableCollection<InvoiceProvider>();

        public SchetFacturaProviderSelectUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceProvider CurrentInvoice
        {
            get => myCurrentInvoice;
            set
            {
                if (Equals(myCurrentInvoice,value)) return;
                myCurrentInvoice = value;
                RaisePropertyChanged();
            }
        }

        private void LoadReference()
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var c in ctx.SD_26)
                        InvoiceCollection.Add(new InvoiceProvider(c, new UnitOfWork<ALFAMEDIAEntities>()));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }
    }
}
