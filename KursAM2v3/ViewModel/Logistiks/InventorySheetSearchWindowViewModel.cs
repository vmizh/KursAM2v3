using System;
using System.Collections.ObjectModel;
using System.Windows;
using Core.Menu;
using Core.ViewModel.Base;
using KursAM2.Managers;
using KursAM2.Managers.Base;
using KursAM2.View.Logistiks;

namespace KursAM2.ViewModel.Logistiks
{
    public sealed class InventorySheetSearchWindowViewModel : RSWindowViewModelBase
    {
        private readonly InventorySheetManager myManager = new InventorySheetManager();
        private InventorySheetViewModel myCurrentRow;
        private DateTime myDateEnd;
        private DateTime myDateStart;

        public InventorySheetSearchWindowViewModel()
        {
            WindowName = "Инвентаризационные ведомости";
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateEnd = DateTime.Today;
            DateStart = DateEnd.AddYears(-1);
        }

        public override bool IsDocNewCopyRequisiteAllow => CurrentRow != null;

        public ObservableCollection<InventorySheetViewModel> Documents { set; get; } =
            new ObservableCollection<InventorySheetViewModel>();

        public DateTime DateStart
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
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

        public override bool IsDocumentOpenAllow => myCurrentRow != null;

        public InventorySheetViewModel CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow != null && myCurrentRow.Equals(value)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            Documents.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
                foreach (var d in myManager.GetDocuments(DateStart, DateEnd))
                    Documents.Add(d);
            else
                foreach (var d in myManager.GetDocuments(DateStart, DateEnd, SearchText))
                    Documents.Add(d);
            RaisePropertiesChanged(nameof(Documents));
        }

        public override void DocumentOpen(object obj)
        {
            var form = new InventorySheetView
            {
                DataContext = new InventorySheetWindowViewModel(CurrentRow.DocCode),
                Owner = Application.Current.MainWindow
            };
            form.Show();
            ((InventorySheetWindowViewModel) form.DataContext).RefreshData(null);
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new InventorySheetView
            {
                DataContext = new InventorySheetWindowViewModel
                {
                    Document = myManager.CreateNew()
                },
                Owner = Application.Current.MainWindow
            };
            frm.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (CurrentRow == null) return;
            var doc = myManager.Load(CurrentRow.DocCode);
            if (doc == null) return;
            var frm = new InventorySheetView
            {
                DataContext = new InventorySheetWindowViewModel
                {
                    Document = myManager.CreateNew(doc, DocumentCopyType.Requisite)
                },
                Owner = Application.Current.MainWindow
            };
            frm.Show();
        }

        #region Commands

        #endregion
    }
}