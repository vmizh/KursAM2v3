using System;
using System.Collections.ObjectModel;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.View.Finance.UC;

namespace KursAM2.ViewModel.Finance.Invoices
{
    public class SchetSupplierSelectViewModel : RSWindowViewModelBase
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
                if (myCurrentInvoice != null && myCurrentInvoice.Equals(value)) return;
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
                        InvoiceCollection.Add(new InvoiceProvider(c));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }
    }
}