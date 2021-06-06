using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Invoices;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.View.Finance.Invoices;
using Reports.Base;

namespace KursAM2.ViewModel.Finance.Invoices
{
    public sealed class InvoiceClientSearchViewModel : RSWindowSearchViewModelBase
    {
        private InvoiceClient myCurrentDocument;

        public InvoiceClientSearchViewModel()
        {
            WindowName = "Счета фактуры для клиентов";
            Documents = new ObservableCollection<InvoiceClient>();
        }

        public InvoiceClientSearchViewModel(Window form) : base(form)
        {
            WindowName = "Счета фактуры для клиентов";
            Documents = new ObservableCollection<InvoiceClient>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            EndDate = DateTime.Today;
            StartDate = EndDate.AddDays(-30);
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            if (prn != null)
            {
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Заказ",
                    Command = PrintZakazCommand
                });
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Счет",
                    Command = PrintSFSchetCommand
                });
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Счет фактура",
                    Command = PrintSFCommand
                });
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportSFCommand
                });
            }
        }

        public InvoiceClient CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument != null && myCurrentDocument.Equals(value)) return;
                myCurrentDocument = value;
                if (myCurrentDocument != null)
                {
                    IsDocumentOpenAllow = true;
                    IsDocNewCopyAllow = true;
                    IsDocNewCopyRequisiteAllow = true;
                    IsPrintAllow = true;
                }

                RaisePropertyChanged();
            }
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<InvoiceClient> Documents { set; get; }

        public Command ExportSFCommand
        {
            get { return new Command(ExportSF, param => IsDocumentOpenAllow); }
        }

        public Command PrintSFCommand
        {
            get { return new Command(PrintSF, param => IsDocumentOpenAllow); }
        }

        public Command PrintZakazCommand
        {
            get { return new Command(PrintZakaz, param => true); }
        }

        public Command PrintSFSchetCommand
        {
            get { return new Command(PrintSChet, param => true); }
        }

        private void PrintSChet(object obj)
        {
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.PrintSChet(null);
        }

        private void PrintZakaz(object obj)
        {
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.PrintZakaz(null);
        }

        public override void Print(object form)
        {
            var rep = new ExportView();
            rep.Show();
        }

        private void ExportSF(object obj)
        {
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.ExportSF(null);
        }

        private void PrintSF(object obj)
        {
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.PrintSF(null);
        }

        #region Commands

        public override void Search(object obj)
        {
            GetSearchDocument(obj);
        }

        private void Delete(object obj)
        {
            if (CurrentDocument != null)
                InvoicesManager.DeleteProvider(CurrentDocument.DocCode);
            RefreshData(null);
        }

        public void GetSearchDocument(object obj)
        {
            try
            {
                Documents.Clear();
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var query = ctx
                        .SD_84
                        .Include(_ => _.SD_43)
                        .Include(_ => _.SD_431)
                        .Include(_ => _.SD_432)
                        .Include(_ => _.SD_301)
                        .Include(_ => _.TD_84)
                        .Include("TD_84.SD_83")
                        .Include("TD_84.TD_24")
                        .Where(_ => _.SF_DATE >= StartDate && _.SF_DATE <= EndDate);
                    foreach (var item in query.ToList())
                    {
                        var newItem = new InvoiceClient(item);
                        string d;
                        d = newItem.Diler != null ? newItem.Diler.Name : "";
                        if (newItem.InnerNumber.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                            newItem.OuterNumber.ToUpper().Contains(SearchText.ToUpper()) ||
                            newItem.SF_CLIENT_NAME.ToUpper().Contains(SearchText.ToUpper()) ||
                            newItem.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                            newItem.SF_DILER_SUMMA.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                            newItem.CO.Name.ToUpper().Contains(SearchText.ToUpper()) ||
                            newItem.Summa.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                            d.ToUpper().Contains(SearchText.ToUpper()))
                            Documents.Add(newItem);
                    }
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public ICommand DeleteCommand
        {
            get { return new Command(Delete, param => true); }
        }

        public override void SearchClear(object obj)
        {
            base.SearchClear(obj);
            RefreshData(null);
        }

        public override void RefreshData(object data)
        {
            base.RefreshData(data);
            IsDocumentOpenAllow = false;
            IsDocNewCopyAllow = false;
            IsDocNewCopyRequisiteAllow = false;
            IsPrintAllow = false;
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

            try
            {
                Documents.Clear();
                foreach (var d in InvoicesManager.GetInvoicesClient(StartDate, EndDate, false, SearchText))
                    Documents.Add(d);
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            RaisePropertyChanged(nameof(Documents));
            SearchText = "";
        }

        public override void DocumentOpen(object form)
        {
            if (CurrentDocument == null) return;
            DocumentsOpenManager.Open(DocumentType.InvoiceClient, CurrentDocument.DocCode);
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new InvoiceClientView {Owner = Application.Current.MainWindow};
            var ctx = new ClientWindowViewModel {Form = frm};
            ctx.Document = InvoicesManager.NewClient();
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new InvoiceClientView {Owner = Application.Current.MainWindow};
            var ctx = new ClientWindowViewModel {Form = frm};
            ctx.Document = InvoicesManager.NewClientCopy(CurrentDocument);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new InvoiceClientView {Owner = Application.Current.MainWindow};
            var ctx = new ClientWindowViewModel {Form = frm};
            ctx.Document = InvoicesManager.NewClientRequisite(CurrentDocument);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void ResetLayout(object form)
        {
            var frm = form as SearchInvoiceClientView;
            frm?.LayoutManager.ResetLayout();
        }

        #endregion
    }
}