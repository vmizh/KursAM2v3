using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Xpf.Core;
using KursAM2.Managers;
using KursAM2.Managers.Invoices;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.Base;
using KursAM2.View.Finance.Invoices;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Invoices;
using KursDomain.IDocuments.Finance;
using KursDomain.Menu;
using KursDomain.Repository;

namespace KursAM2.ViewModel.Finance.Invoices
{
    public sealed class InvoiceClientSearchViewModel : RSWindowSearchViewModelBase
    {
        public readonly GenericKursDBRepository<SD_84> GenericProviderRepository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // ReSharper disable once NotAccessedField.Local
        public IInvoiceClientRepository InvoiceClientRepository;

        private IInvoiceClient myCurrentDocument;


        public InvoiceClientSearchViewModel()
        {
            WindowName = "Счета фактуры для клиентов";
            Documents = new ObservableCollection<IInvoiceClient>();
        }

        public InvoiceClientSearchViewModel(Window form) : base(form)
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_84>(UnitOfWork);
            InvoiceClientRepository = new InvoiceClientRepository(UnitOfWork);
            WindowName = "Счета фактуры для клиентов";
            Documents = new ObservableCollection<IInvoiceClient>();
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
                var schet = new MenuButtonInfo
                {
                    Caption = "Счет"
                    //Command = PrintSFSchetCommand
                };
                schet.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Печать",
                    Command = PrintSFSchetCommand
                });
                schet.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportSFCommand
                });
                prn.SubMenu.Add(schet);
                var sf = new MenuButtonInfo
                {
                    Caption = "Счет фактура"
                    //Command = PrintSFCommand
                };
                sf.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Печать",
                    Command = PrintSFCommand
                });
                sf.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportSFCommand
                });
                prn.SubMenu.Add(sf);
            }
        }

        public InvoiceClientSearchViewModel(ThemedWindow form) : base(form)
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_84>(UnitOfWork);
            InvoiceClientRepository = new InvoiceClientRepository(UnitOfWork);
            WindowName = "Счета фактуры для клиентов";
            Documents = new ObservableCollection<IInvoiceClient>();
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
                var schet = new MenuButtonInfo
                {
                    Caption = "Счет"
                    //Command = PrintSFSchetCommand
                };
                schet.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Печать",
                    Command = PrintSFSchetCommand
                });
                schet.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportSFCommand
                });
                prn.SubMenu.Add(schet);
                var sf = new MenuButtonInfo
                {
                    Caption = "Счет фактура"
                    //Command = PrintSFCommand
                };
                sf.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Печать",
                    Command = PrintSFCommand
                });
                sf.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportSFCommand
                });
                prn.SubMenu.Add(sf);
            }
        }


        public override string LayoutName => "InvoiceClientSearchViewModel";

        public IInvoiceClient CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (Equals(myCurrentDocument, value)) return;
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
        public ObservableCollection<IInvoiceClient> Documents { set; get; }

        public Command ExportSFCommand
        {
            get { return new Command(ExportSF, _ => IsDocumentOpenAllow); }
        }

        public Command PrintSFCommand
        {
            get { return new Command(PrintSF, _ => IsDocumentOpenAllow); }
        }

        public Command PrintZakazCommand
        {
            get { return new Command(PrintZakaz, _ => true); }
        }

        public override bool IsPrintAllow => CurrentDocument != null;

        public Command PrintSFSchetCommand
        {
            get { return new Command(PrintSChet, _ => true); }
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

        //public override void Print(object form)
        //{
        //    var rep = new ExportView();
        //    rep.Show();
        //}

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
                        var newItem = new InvoiceClientViewModel(item);
                        string d;
                        d = newItem.Diler != null ? newItem.Diler.Name : "";
                        if (newItem.InnerNumber.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                            newItem.OuterNumber.ToUpper().Contains(SearchText.ToUpper()) ||
                            newItem.SF_CLIENT_NAME.ToUpper().Contains(SearchText.ToUpper()) ||
                            newItem.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                            newItem.SF_DILER_SUMMA.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                            newItem.CO.Name.ToUpper().Contains(SearchText.ToUpper()) ||
                            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
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
            get { return new Command(Delete, _ => true); }
        }

        public override void SearchClear(object obj)
        {
            base.SearchClear(obj);
            RefreshData(null);
        }

        public override void RefreshData(object data)
        {
            InvoiceClientRepository =
                new InvoiceClientRepository(
                    new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString)));
            var frm = Form as StandartSearchView;
            Documents.Clear();
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            Task.Run(() =>
            {
                frm?.Dispatcher.Invoke(() => { frm.loadingIndicator.Visibility = Visibility.Visible; });
                var result = InvoiceClientRepository.GetAllByDates(StartDate, EndDate);
                frm?.Dispatcher.Invoke(() =>
                {
                    frm.loadingIndicator.Visibility = Visibility.Hidden;
                    foreach (var d in result)
                        Documents.Add(d);
                });
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            });
        }

        public override void DocumentOpen(object form)
        {
            if (CurrentDocument == null) return;
            DocumentsOpenManager.Open(DocumentType.InvoiceClient, CurrentDocument.DocCode);
        }

        public override void DocNewEmpty(object form)
        {
            var ctx = new ClientWindowViewModel(null);
            var frm = new InvoiceClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.SetAsNewCopy(true);
            var frm = new InvoiceClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var ctx = new ClientWindowViewModel(CurrentDocument.DocCode);
            ctx.SetAsNewCopy(false);
            var frm = new InvoiceClientView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        #endregion
    }
}
