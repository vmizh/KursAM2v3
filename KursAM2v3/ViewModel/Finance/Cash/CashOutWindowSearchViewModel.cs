using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers;
using KursAM2.View.Finance.Cash;
using KursDomain;
using KursDomain.Documents.Cash;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Finance.Cash
{
    public class CashOutWindowSearchViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public CashOutWindowSearchViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateEnd = DateTime.Today;
        }

        #endregion

        #region Fielfds

        private CashOut myCurrentDocument;
        private DateTime myDateEnd;
        private DateTime myDateStart;
        public override string LayoutName => "CashOutWindowSearchViewModel";

        #endregion

        #region Properties

        public override void AddSearchList(object obj)
        {
            var form = new CashInSearchView
            {
                Owner = Application.Current.MainWindow
            };
            form.DataContext = new CashInWindowSearchViewModel
            {
                Form = form
            };
            form.Show();

        }

        public DateTime DateStart
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                if (DateStart > DateEnd)
                    DateEnd = DateStart;
                RaisePropertyChanged();
            }
        }

        public DateTime DateEnd
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
                if (DateEnd < DateStart)
                    DateStart = DateEnd;
                RaisePropertyChanged();
            }
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        public CashOut CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (Equals(myCurrentDocument,value)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        public override string SearchText
        {
            get => mySearchText;
            set
            {
                if (mySearchText == value) return;
                mySearchText = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override bool IsDocNewCopyRequisiteAllow => State != RowStatus.NewRow;

        public ObservableCollection<CashOut> DocumentCollection { set; get; } =
            new ObservableCollection<CashOut>();

        public override void SearchClear(object obj)
        {
            SearchText = null;
            RefreshData(null);
        }

        public override void Search(object obj)
        {
            DocumentCollection.Clear();
            try
            {
                // ReSharper disable once UnusedVariable
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_34
                        .Include(_ => _.SD_114)
                        .Include(_ => _.SD_2)
                        .Include(_ => _.SD_22)
                        .Include(_ => _.SD_301)
                        .Include(_ => _.SD_3011)
                        .Include(_ => _.SD_3012)
                        .Include(_ => _.SD_3013)
                        .Include(_ => _.SD_303)
                        .Include(_ => _.SD_43)
                        .AsNoTracking()
                        .Where(_ => _.DATE_ORD >= DateStart && _.DATE_ORD <= DateEnd)
                        .ToList();
                    foreach (var d in data.Where(_=> GlobalOptions.UserInfo.CashAccess.Contains(_.CA_DC ?? 0)))
                        if (d.SD_43.NAME.Contains(SearchText))
                            DocumentCollection.Add(new CashOut(d)
                                { myState = RowStatus.NotEdited });
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            SearchText = null;
            DocumentCollection.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_34
                        .Include(_ => _.SD_114)
                        .Include(_ => _.SD_2)
                        .Include(_ => _.SD_22)
                        .Include(_ => _.SD_301)
                        .Include(_ => _.SD_3011)
                        .Include(_ => _.SD_3012)
                        .Include(_ => _.SD_3013)
                        .Include(_ => _.SD_303)
                        .Include(_ => _.SD_43)
                        .AsNoTracking()
                        .Where(_ => _.DATE_ORD >= DateStart && _.DATE_ORD <= DateEnd)
                        .OrderByDescending(_ => _.DATE_ORD)
                        .ToList();
                    foreach (var d in data)
                        DocumentCollection.Add(new CashOut(d)
                            { myState = RowStatus.NotEdited });
                }

                RaisePropertyChanged(nameof(DocumentCollection));
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void DocumentOpen(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.CashOut, CurrentDocument.DocCode);
        }

        public override void DocNewEmpty(object obj)
        {
            var vm = new CashOutWindowViewModel
            {
                Document = CashManager.NewCashOut()
            };
            DocumentsOpenManager.Open(DocumentType.CashOut, vm);
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var vm = new CashOutWindowViewModel
            {
                Document = CashManager.NewRequisisteCashOut(CurrentDocument.DocCode)
            };
            vm.Document.Cash = CurrentDocument.Cash;
            DocumentsOpenManager.Open(DocumentType.CashOut, vm);
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var vm = new CashOutWindowViewModel
            {
                Document = CashManager.NewCopyCashOut(CurrentDocument.DocCode)
            };
            vm.Document.Cash = CurrentDocument.Cash;
            DocumentsOpenManager.Open(DocumentType.CashOut, vm);
        }

        #endregion
    }
}
