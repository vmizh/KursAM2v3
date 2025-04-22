using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Core.ViewModel.Base;
using DevExpress.Xpf.Grid;
using KursAM2.Repositories.NomenklReturn;
using KursAM2.View.Logistiks.NomenklReturn;
using KursAM2.ViewModel.Logistiks.NomenklReturn.Helper;
using KursDomain;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks.NomenklReturn
{
    public class NomenklReturnSelectWaybillRowsDialog : RSWindowViewModelBase
    {
        public NomenklReturnSelectWaybillRowsDialog(Kontragent kontragent, List<Guid> existsPrihRows = null)
        {
            myKontragent = kontragent;
            myExistsPrihRows = existsPrihRows;
            IsDialog = true;
            myDataRepository = new NomenklReturnOfClientRepository(GlobalOptions.GetEntities());
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RefreshData();
        }

        #region Fields

        private readonly INomenklReturnOfClientRepository myDataRepository;
        private RashodNakladRow myCurrentMainRow;
        private readonly Kontragent myKontragent;
        private readonly List<Guid> myExistsPrihRows;

        #endregion

        #region Properties

        public UserControl CustomDataUserControl { set; get; } = new NomenklReturnSelectWaybillRows();
        public override string LayoutName => "NomenklReturnSelectWaybillRows";

        public RashodNakladRow CurrentMainRow
        {
            get => myCurrentMainRow;
            set
            {
                if (Equals(value, myCurrentMainRow)) return;
                myCurrentMainRow = value;
                RaisePropertyChanged();
            }
        }

        public RashodNakladRow CurrentSelectedRow
        {
            get => myCurrentMainRow;
            set
            {
                if (Equals(value, myCurrentMainRow)) return;
                myCurrentMainRow = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<RashodNakladRow> MainRows { set; get; } =
            new ObservableCollection<RashodNakladRow>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<RashodNakladRow> ActualMainRows { set; get; } =
            new ObservableCollection<RashodNakladRow>();

        public ObservableCollection<RashodNakladRow> SelectedRows { set; get; } =
            new ObservableCollection<RashodNakladRow>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<RashodNakladRow> ActualSelectedRows { set; get; } =
            new ObservableCollection<RashodNakladRow>();

        #endregion

        #region Commands

        public sealed override void RefreshData()
        {
            MainRows.Clear();
            SelectedRows.Clear();
            foreach (var d in myDataRepository.GetRashodNakladRows(myKontragent.DocCode)
                         .Where(d => !myExistsPrihRows.Contains(d.Id)))
                MainRows.Add(d);
        }

        public override void UpdateVisualObjects()
        {
            if (CustomDataUserControl is NomenklReturnSelectWaybillRows frm)
            {
                frm.gridWaybillRows.SelectionMode = MultiSelectMode.Row;
                frm.tableViewWaybillRow.NavigationStyle = GridViewNavigationStyle.Cell;
            }
        }

        public ICommand AddSelectedRowCommand
        {
            get { return new Command(AddSelectedRow, _ => CurrentMainRow is not null); }
        }

        private void AddSelectedRow(object obj)
        {
            foreach (var item in ActualMainRows)
            {
                if (SelectedRows.Any(_ => _.DocCode == item.DocCode && _.Code == item.Code)) continue;
                SelectedRows.Add(item);
            }
        }

        public ICommand DeleteSelectedRowCommand
        {
            get { return new Command(DeleteSelected, _ => CurrentSelectedRow is not null); }
        }

        private void DeleteSelected(object obj)
        {
            var selectedId = ActualSelectedRows.Select(_m => _m.Id).ToList();
            foreach (var item in selectedId.Select(id => SelectedRows.FirstOrDefault(_ => _.Id == id))
                         .Where(item => item != null))
                SelectedRows.Remove(item);

            #endregion
        }
    }
}
