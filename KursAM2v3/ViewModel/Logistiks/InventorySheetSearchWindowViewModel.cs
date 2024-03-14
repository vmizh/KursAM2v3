using System;
using System.Collections.ObjectModel;
using System.Windows;
using Core;
using Core.ViewModel.Base;
using Data;
using KursAM2.Repositories;
using KursAM2.View.Logistiks;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.Repository;

namespace KursAM2.ViewModel.Logistiks
{
    public sealed class InventorySheetSearchWindowViewModel : RSWindowViewModelBase
    {
        public readonly GenericKursDBRepository<SD_24> GenericInventorySheetRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly ISD_24Repository SD_24Repository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        // private readonly InventorySheetManager myManager = new InventorySheetManager();
        private InventorySheetViewModel myCurrentRow;
        private DateTime myDateEnd;
        private DateTime myDateStart;

        public InventorySheetSearchWindowViewModel()
        {
            GenericInventorySheetRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            SD_24Repository = new SD_24Repository(UnitOfWork);
            WindowName = "Инвентаризационные ведомости";
            IsDocNewCopyRequisiteAllow = true;
            IsCanDocNew = true;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateEnd = DateTime.Today;
            DateStart = new DateTime(DateTime.Now.Year, 1, 1);
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
                if (Equals(myCurrentRow,value)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            Documents.Clear();
            foreach (var d in SD_24Repository.GetDocuments(MatearialDocumentType.InventorySheet, DateStart, DateEnd))
            {
                var newItem = new InventorySheetViewModel(d)
                {
                    State = RowStatus.NotEdited,
                    myState = RowStatus.NotEdited
                };
                Documents.Add(newItem);
            }

            RaisePropertyChanged(nameof(Documents));
        }

        public override void DocumentOpen(object obj)
        {
            var form = new InventorySheetView2
            {
                //DataContext = dtx,
                Owner = Application.Current.MainWindow
            };
            form.DataContext = new InventorySheetWindowViewModel(CurrentRow.DocCode)
            {
                Form = form,
                myState = RowStatus.NotEdited
            };
            form.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var doc = new InventorySheetViewModel(SD_24Repository.CreateNew(MatearialDocumentType.InventorySheet))
            {
                myState = RowStatus.NewRow
            };
            var dtx = new InventorySheetWindowViewModel
            {
                Document = doc,
                State = RowStatus.NewRow
            };
            dtx.UnitOfWork.Context.SD_24.Add(doc.Entity);
            var frm = new InventorySheetView2
            {
                DataContext = dtx,
                Owner = Application.Current.MainWindow
            };
            frm.Show();
        }

        public override void DocNewCopy(object form)
        {
            if (CurrentRow == null) return;
            var frm = new InventorySheetView2
            {
                DataContext = new InventorySheetWindowViewModel
                {
                    Document = new InventorySheetViewModel(SD_24Repository.CreateCopy(CurrentRow.DocCode))
                },
                Owner = Application.Current.MainWindow
            };
            frm.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (CurrentRow == null) return;
            var frm = new InventorySheetView2
            {
                DataContext = new InventorySheetWindowViewModel
                {
                    Document = new InventorySheetViewModel(SD_24Repository.CreateRequisiteCopy(CurrentRow.DocCode))
                },
                Owner = Application.Current.MainWindow
            };
            frm.Show();
        }

        #region Commands

        #endregion
    }
}
