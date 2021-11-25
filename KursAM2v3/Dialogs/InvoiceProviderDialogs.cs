using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using JetBrains.Annotations;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl;

namespace KursAM2.Dialogs
{
    public sealed class InvoiceProviderDialogs : KursBaseControlViewModel
    {
        #region Fields

        private readonly IInvoiceProviderRepository invoiceProviderRepository;

        #endregion

        #region Constructors

        public InvoiceProviderDialogs()
        {
            ModelView = new StandartDialogSelectWithDateUC(GetType().Name);
            RightMenuBar = MenuGenerator.RefreshOnlyRightBar(this);
            invoiceProviderRepository = new InvoiceProviderRepository(GlobalOptions.GetEntities());

        }

        public InvoiceProviderDialogs(IInvoiceProviderRepository repos) : this()
        {
            invoiceProviderRepository = repos;
        }

        public ObservableCollection<InvoiceProvider> ItemsCollection { set; get; }
            = new ObservableCollection<InvoiceProvider>();

        public InvoiceProviderDialogs(IInvoiceProviderRepository repos, [NotNull] Currency crs,
            DateTime? dateStart = null, DateTime? dateEnd = null) : this(repos)
        {
            Currency = crs;
            StartDate = dateStart;
            EndDate = dateEnd;
            RightMenuBar = MenuGenerator.StandartDialogRightBar(this);
            WindowName = "Выбор счета";
            ItemsCollection.Clear();
        }

        #endregion

        #region Properties

        public bool IsNakladInvoices { set; get; }

        public InvoiceProvider CurrentItem
        {
            set => SetValue(value);
            get => GetValue<InvoiceProvider>();
        }

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<InvoiceProvider> SelectedItems { set; get; }
            = new ObservableCollection<InvoiceProvider>();


        public Currency Currency
        {
            get => GetValue<Currency>();
            set => SetValue(value);
        }

        public DateTime? StartDate
        {
            get => GetValue<DateTime?>();
            set => SetValue(value, () =>
            {
                if(EndDate < StartDate)
                    EndDate = StartDate;
            });
        }

        public DateTime? EndDate
        {
            get => GetValue<DateTime?>();
            set => SetValue(value);
        }

        #endregion

        #region Methods

        #endregion

        #region Commands

        public override void Load(object o)
        {
            ItemsCollection.Clear();
            if (!IsNakladInvoices)
            {
                foreach (var d in invoiceProviderRepository.GetAllForNakladDistribute(Currency,
                    StartDate, EndDate).OrderByDescending(_ => _.DocDate))
                    ItemsCollection.Add(d);
            }
            else
            {
                foreach (var d in invoiceProviderRepository.GetNakladInvoices(StartDate, EndDate)
                    .OrderByDescending(_ => _.DocDate))
                    ItemsCollection.Add(d);
            }
        }

        public override bool IsCanRefresh { set; get; } = true;

        public bool? ShowDialog()
        {
            ItemsCollection.Clear();
            var dsForm = new KursBaseDialog
            {
                Owner = Application.Current.MainWindow
            };
            Form = dsForm;
            dsForm.DataContext = this;
            return dsForm.ShowDialog();
        }

        public bool? ShowDialog(Currency crs, DateTime? dateStart = null, DateTime? dateEnd = null)
        {
            ItemsCollection.Clear();
            Currency = crs;
            StartDate = dateStart;
            EndDate = dateEnd;
            var dsForm = new KursBaseDialog
            {
                Owner = Application.Current.MainWindow
            };
            Form = dsForm;
            dsForm.DataContext = this;
            return dsForm.ShowDialog();
        }

        #endregion
    }
}