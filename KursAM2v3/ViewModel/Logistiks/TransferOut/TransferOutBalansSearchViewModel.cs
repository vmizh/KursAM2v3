using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Data;
using KursAM2.Event;
using KursDomain;
using KursDomain.Menu;
using KursDomain.Repository.DocHistoryRepository;
using KursDomain.Repository.NomenklRepository;
using KursDomain.Repository.StorageLocationsRepositury;
using KursDomain.Repository.TransferOut;
using KursDomain.ViewModel.Base2;
using KursDomain.Wrapper.TransferOut;

namespace KursAM2.ViewModel.Logistiks.TransferOut
{
    public sealed class TransferOutBalansSearchViewModel : FormSearchViewModelBase<TransferOutBalansWrapper>, IFormMenu
    {
        #region Fields

        private ITransferOutBalansRepository _TransferOutBalansRepository;

        #endregion

        #region Constructors

        public TransferOutBalansSearchViewModel()
        {
            LayoutName = "TransferOutBalansSearch";
            WindowName = "Перевод за баланс";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateStart = DateTime.Today.AddDays(-30);
            DateEnd = DateTime.Today;
            Documents = new ObservableCollection<TransferOutBalansWrapper>();

            MainWindowViewModel.EventAggregator.GetEvent<AfterTransferOutBalansWrapperEvent>()
                .Subscribe(OnAfterSaveTransferOutBalansExecute);
        }

        #endregion

        #region Commands

        public override async Task OnRefreshDataAsync()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                _TransferOutBalansRepository = new TransferOutBalansRepository(ctx);
                List<TransferOutBalans> items;
                Form.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Form.loadingIndicator.Visibility = Visibility.Visible;
                }));
                if (!string.IsNullOrWhiteSpace(SearchText))
                    items = await _TransferOutBalansRepository.GetSearchTextAsync(SearchText, DateStart, DateEnd);
                else
                    items = await _TransferOutBalansRepository.GetForDatesAsync(DateStart, DateEnd);

                Form.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Documents.Clear();
                    foreach (var newItem in items.Select(item => new TransferOutBalansWrapper(item)))
                    {
                        newItem.StartLoad();
                        Documents.Add(newItem);
                    }

                    Form.loadingIndicator.Visibility = Visibility.Hidden;
                }));
            }
        }

        public override async Task OnDocumentOpenAsync()
        {
            var ctx = GlobalOptions.GetEntities();
            var doc = new TransferOutBalansViewModel(new TransferOutBalansRepository(ctx),
                new StorageLocationsRepository(ctx), new NomenklRepository(ctx), new DocHistoryRepository(ctx));
            await doc.InitializeAsync(CurrentDocument.Id);
            doc.Show();
        }

        protected override async Task OnDocNewEmptyExecute()
        {
            var ctx = GlobalOptions.GetEntities();
            var doc = new TransferOutBalansViewModel(new TransferOutBalansRepository(ctx),
                new StorageLocationsRepository(ctx), new NomenklRepository(ctx), new DocHistoryRepository(ctx));
            await doc.InitializeAsync(Guid.Empty, DocumentNewState.Empty);
            doc.Show();
        }

        protected override async Task OnDocNewCopyRequisiteExecute()
        {
            var ctx = GlobalOptions.GetEntities();
            var doc = new TransferOutBalansViewModel(new TransferOutBalansRepository(ctx),
                new StorageLocationsRepository(ctx), new NomenklRepository(ctx), new DocHistoryRepository(ctx));
            await doc.InitializeAsync(CurrentDocument.Id, DocumentNewState.Requisite);
            doc.Show();
        }


        protected override async Task OnDocNewCopyExecute()
        {
            var ctx = GlobalOptions.GetEntities();
            var doc = new TransferOutBalansViewModel(new TransferOutBalansRepository(ctx),
                new StorageLocationsRepository(ctx), new NomenklRepository(ctx), new DocHistoryRepository(ctx));
            await doc.InitializeAsync(CurrentDocument.Id, DocumentNewState.Copy);
            doc.Show();
        }

        #endregion

        #region Properties

        public ObservableCollection<MenuButtonInfo> RightMenuBar { get; set; }
        public ObservableCollection<MenuButtonInfo> LeftMenuBar { get; set; }

        #endregion

        #region Methods

        private void OnAfterSaveTransferOutBalansExecute(AfterTransferOutBalansWrapperEventArgs obj)
        {
            switch (obj.Operation)
            {
                case EnumAfterSaveOperation.Update:
                    var doc = Documents.FirstOrDefault(_ => _.Id == obj.Id);
                    if (doc == null) return;
                    var t = Documents.IndexOf(doc);
                    Documents[t] = obj.Document;
                    Documents[t].UpdateSummaries();
                    break;
                case EnumAfterSaveOperation.Add:
                    Documents.Add(obj.Document);
                    break;
                case EnumAfterSaveOperation.Delete:
                    var docDel = Documents.FirstOrDefault(_ => _.Id == obj.Id);
                    if (docDel == null) return;
                    Documents.Remove(docDel);
                    break;
            }
        }

        protected override async Task OnWindowLoaded()
        {
            await base.OnWindowLoaded();
            if (Form is not null)
                foreach (var col in Form.gridDocuments.Columns)
                    col.ReadOnly = true;
        }

        #endregion
    }
}
