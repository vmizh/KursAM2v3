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
        public NomenklReturnSelectWaybillRowsDialog(Kontragent kontragent)
        {
            myKontragent = kontragent;
            IsDialog = true;
            ActualSelectedWaybillRows = new ObservableCollection<RashodNakladRow>();
            ActualSelectedWaybillRows2 = new ObservableCollection<RashodNakladRow>();
            myDataRepository = new NomenklReturnOfClientRepository(GlobalOptions.GetEntities());
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RefreshData();
        }

        #region Commands

        public sealed override void RefreshData()
        {
            WaybillRows.Clear();
            SelectedWaybillRows.Clear();
            foreach (var d in myDataRepository.GetRashodNakladRows(myKontragent.DocCode)) WaybillRows.Add(d);
        }

        #endregion

        #region Fields

        private readonly INomenklReturnOfClientRepository myDataRepository;
        private RashodNakladRow myCurrentWaybillRow;
        private readonly Kontragent myKontragent;

        #endregion

        #region Properties

        public UserControl CustomDataUserControl { set; get; } = new NomenklReturnSelectWaybillRows();


        public override string LayoutName => "NomenklReturnSelectWaybillRows";

        public RashodNakladRow CurrentWaybillRow
        {
            get => myCurrentWaybillRow;
            set
            {
                if (Equals(value, myCurrentWaybillRow)) return;
                myCurrentWaybillRow = value;
                RaisePropertyChanged();
            }
        }

        public RashodNakladRow CurrentSelectedWaybillRow
        {
            get => myCurrentWaybillRow;
            set
            {
                if (Equals(value, myCurrentWaybillRow)) return;
                myCurrentWaybillRow = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<RashodNakladRow> WaybillRows { set; get; } =
            new ObservableCollection<RashodNakladRow>();

        public ObservableCollection<RashodNakladRow> ActualSelectedWaybillRows { set; get; } 

        public ObservableCollection<RashodNakladRow> SelectedWaybillRows { set; get; } =
            new ObservableCollection<RashodNakladRow>();

        public ObservableCollection<RashodNakladRow> ActualSelectedWaybillRows2 { set; get; }

        #endregion

        #region Commands

        public override void UpdateVisualObjects()
        {
            if (myDataRepository is NomenklReturnSelectWaybillRows frm)
            {
                frm.gridWaybillRows.SelectionMode = MultiSelectMode.Row;
                frm.tableViewWaybillRow.NavigationStyle = GridViewNavigationStyle.Cell;
            }
        }

        public ICommand AddSelectedRowCommand
        {
            get { return new Command(AddSelectedRow, _ => CurrentWaybillRow is not null); }
        }

        private void AddSelectedRow(object obj)
        {
            foreach (var item in ActualSelectedWaybillRows)
            {
                if (SelectedWaybillRows.Any(_ => _.DocCode == item.DocCode && _.Code == item.Code)) continue;
                SelectedWaybillRows.Add(item);
            }
        }

        public ICommand DeleteSelectedRowCommand
        {
            get { return new Command(DeleteSelected, _ => CurrentSelectedWaybillRow is not null); }
        }

        private void DeleteSelected(object obj)
        {
            var selectedId = ActualSelectedWaybillRows2.Select(_m => _m.Id).ToList();
            foreach (var item in selectedId.Select(id => SelectedWaybillRows.FirstOrDefault(_ => _.Id == id))
                         .Where(item => item != null))
                SelectedWaybillRows.Remove(item);

            #endregion
        }
    }
}
