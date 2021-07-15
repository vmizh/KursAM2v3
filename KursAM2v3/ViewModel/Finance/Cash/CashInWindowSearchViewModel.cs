using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel.Cash;
using Core.EntityViewModel.CommonReferences;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers;

namespace KursAM2.ViewModel.Finance.Cash
{
    public class CashInWindowSearchViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public CashInWindowSearchViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateStart = DateTime.Today.AddDays(-100);
            DateEnd = DateTime.Today;
        }

        #endregion

        #region Fielfds

        private CashIn myCurrentDocument;
        private DateTime myDateEnd;
        private DateTime myDateStart;

        #endregion

        #region Properties

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

        public CashIn CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument != null && myCurrentDocument.Equals(value)) return;
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

        public ObservableCollection<CashIn> DocumentCollection { set; get; } =
            new ObservableCollection<CashIn>();

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
                    var data = ctx.SD_33
                        .Include(_ => _.SD_114)
                        .Include(_ => _.SD_2)
                        .Include(_ => _.SD_22)
                        .Include(_ => _.SD_301)
                        .Include(_ => _.SD_3011)
                        .Include(_ => _.SD_3012)
                        .Include(_ => _.SD_3013)
                        .Include(_ => _.SD_303)
                        .Include(_ => _.SD_34)
                        .Include(_ => _.VD_46)
                        .Include(_ => _.SD_90)
                        .Include(_ => _.SD_84)
                        .Include(_ => _.SD_43)
                        .AsNoTracking()
                        .Where(_ => _.DATE_ORD >= DateStart && _.DATE_ORD <= DateEnd)
                        .ToList();
                    foreach (var d in data)
                        if (d.SD_43.NAME.Contains(SearchText))
                            DocumentCollection.Add(new CashIn(d));
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
            DocumentCollection = new ObservableCollection<CashIn>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_33
                        .Include(_ => _.SD_114)
                        .Include(_ => _.SD_2)
                        .Include(_ => _.SD_22)
                        .Include(_ => _.SD_301)
                        .Include(_ => _.SD_3011)
                        .Include(_ => _.SD_3012)
                        .Include(_ => _.SD_3013)
                        .Include(_ => _.SD_303)
                        .Include(_ => _.SD_34)
                        .Include(_ => _.VD_46)
                        .Include(_ => _.SD_90)
                        .Include(_ => _.SD_84)
                        .Include(_ => _.SD_43)
                        .AsNoTracking()
                        .Where(_ => _.DATE_ORD >= DateStart && _.DATE_ORD <= DateEnd)
                        .OrderByDescending(_ => _.DATE_ORD)
                        .ToList();
                    foreach (var d in data)
                        DocumentCollection.Add(new CashIn(d)
                        {
                            myState = RowStatus.NotEdited
                        });
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
            DocumentsOpenManager.Open(DocumentType.CashIn, CurrentDocument.DocCode);
        }

        public override void DocNewEmpty(object obj)
        {
            var vm = new CashInWindowViewModel
            {
                Document = CashManager.NewCashIn()
            };
            DocumentsOpenManager.Open(DocumentType.CashIn, vm, Form);
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var vm = new CashInWindowViewModel
            {
                Document = CashManager.NewRequisiteCashIn(CurrentDocument.DocCode)
            };
            vm.Document.Cash = CurrentDocument.Cash;
            DocumentsOpenManager.Open(DocumentType.CashIn, vm);
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var vm = new CashInWindowViewModel
            {
                Document = CashManager.NewCopyCashIn(CurrentDocument.DocCode)
            };
            vm.Document.Cash = CurrentDocument.Cash;
            DocumentsOpenManager.Open(DocumentType.CashIn, vm);
        }

        #endregion
    }
}