using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using KursAM2.Managers;
using KursAM2.View.Base;
using KursAM2.View.StockHolder;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.StockHolder;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.StockHolder
{
    public sealed class StockHolderAccrualSearchViewModel : RSWindowSearchViewModelBase
    {
        #region Constructors

        public StockHolderAccrualSearchViewModel() 
        {
            repository = new StockHolderAccrualsRepository(unitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            EndDate = DateTime.Today;
            StartDate = EndDate.AddDays(-30);
        }

        #endregion

        #region Commands

        public override bool IsDocDeleteAllow => CurrentDocument != null;
        public override bool IsDocNewCopyAllow => CurrentDocument != null;
        public override bool IsDocNewCopyRequisiteAllow => CurrentDocument != null;
        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        public override void DocumentOpen(object obj)
        {
            if (CurrentDocument == null) return;
            DocumentsOpenManager.Open(
                DocumentType.StockHolderAccrual, 0, CurrentDocument.Id, this);
        }

        public override void DocNewCopy(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new StockHolderAccrualWindowViewModel
            {
                State = RowStatus.NotEdited,
                
            };
            ctx.RefreshData(CurrentDocument.Id);
            ctx.SetAsNewCopy(true);
            var frm = new StockHolderAccrualsView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
           //ctx.RefreshData(null);
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        { 
            var ctx = new StockHolderAccrualWindowViewModel
            {
                ParentFormViewModel = this
            };
            var view = new StockHolderAccrualsView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.RefreshData(null);
            view.Show();
            
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new StockHolderAccrualWindowViewModel();
            ctx.RefreshData(CurrentDocument.Id);
            ctx.SetAsNewCopy(false);
            var frm = new StockHolderAccrualsView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            
            frm.Show();
        }

        public override void RefreshData(object obj)
        {
            Documents.Clear();
            foreach (var d in repository.GetAllByDate(StartDate, EndDate))
                Documents.Add(new StockHolderAccrualViewModel(d)
                {
                    myState = RowStatus.NotEdited
                });
            SetCurrencyVisible();
        }

        #endregion

        #region Fields

        private readonly IStockHolderAccrualsRepository repository;

        private readonly UnitOfWork<ALFAMEDIAEntities> unitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private StockHolderAccrualViewModel myDocument;

        #endregion

        #region Properties

        public override string WindowName => "Ведомости наяислений акционерам";
        public override string LayoutName => "StockHolderAccrualSearchViewModel";

        public ObservableCollection<StockHolderAccrualViewModel> Documents { set; get; }
            = new ObservableCollection<StockHolderAccrualViewModel>();

        public ObservableCollection<StockHolderAccrualViewModel> SelectedDocuments { set; get; }
            = new ObservableCollection<StockHolderAccrualViewModel>();

        public StockHolderAccrualViewModel CurrentDocument
        {
            get => myDocument;
            set
            {
                if (myDocument == value) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        private void SetCurrencyVisible()
        {
            if(Form is StandartSearchView frm)
            {
                
                foreach (var c in frm.gridDocuments.Columns)
                {
                    switch (c.FieldName)
                    {
                        case "SummaRUB":
                            c.Visible = Documents.Sum(_ => _.SummaRUB) != 0;
                            break;
                        case "SummaCHF":
                            c.Visible =Documents.Sum(_ => _.SummaCHF) != 0;
                            break;
                        case "SummaEUR":
                            c.Visible = Documents.Sum(_ => _.SummaEUR) != 0;
                            break;
                        case "SummaGBP":
                            c.Visible = Documents.Sum(_ => _.SummaGBP) != 0;
                            break;
                        case "SummaSEK":
                            c.Visible = Documents.Sum(_ => _.SummaSEK) != 0;
                            break;
                        case "SummaUSD":
                            c.Visible = Documents.Sum(_ => _.SummaUSD) != 0;
                            break;
                        case "SummaCNY":
                            c.Visible = Documents.Sum(_ => _.SummaCNY) != 0;
                            break;
                    }
                }
            }
        }

        #endregion

        #region IDataErrorInfo

        #endregion
    }
}
