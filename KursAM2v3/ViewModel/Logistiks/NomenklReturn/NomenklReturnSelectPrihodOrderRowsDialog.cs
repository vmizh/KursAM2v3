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
using KursDomain.Managers;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks.NomenklReturn
{
    public class NomenklReturnSelectPrihodOrderRowsDialog : RSWindowViewModelBase
    {
        public NomenklReturnSelectPrihodOrderRowsDialog(Kontragent kontragent, KursDomain.References.Warehouse warehouse,
            List<Guid> existsPrihRows = null)
        {
            myKontragent = kontragent;
            myExistsPrihRows = existsPrihRows;
            myWarehouse = warehouse;
            IsDialog = true;
            myDataRepository = new NomenklReturnToProviderRepository(GlobalOptions.GetEntities());
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RefreshData();
        }

        #region Commands

        public sealed override void RefreshData()
        {
            MainRows.Clear();
            var ost = nomenklManager.GetNomenklStoreQuantity(myWarehouse.DocCode,DateTime.Today, DateTime.Today);
            foreach (var d in myDataRepository.GetPrihodOrderdRows(myKontragent.DocCode)
                         .Where(d => !myExistsPrihRows.Contains(d.Id) && d.Nomenkl.CurrencyDC == myKontragent.CurrencyDC))
            {
                if(ost.Exists(_ => _.NomDC == d.NomenklDC && _.OstatokQuantity > 0))
                    MainRows.Add(d);
            }
        }

        #endregion

        #region Fields

        private readonly INomenklReturnToProviderRepository myDataRepository;
        private PrhodOrderRow myCurrentMainRow;
        private readonly Kontragent myKontragent;
        private readonly KursDomain.References.Warehouse myWarehouse;
        private readonly List<Guid> myExistsPrihRows;
        private object myCurrentSelectedRow;
        private NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());

        #endregion

        #region Properties

        public UserControl CustomDataUserControl { set; get; } = new NomenklReturnSelectWaybillRows();
        public override string LayoutName => "NomenklReturnSelectPrihodOrderRows";

        public PrhodOrderRow CurrentMainRow
        {
            get => myCurrentMainRow;
            set
            {
                if (Equals(value, myCurrentMainRow)) return;
                myCurrentMainRow = value;
                RaisePropertyChanged();
            }
        }

        public PrhodOrderRow CurrentSelectedRow
        {
            get => myCurrentMainRow;
            set
            {
                if (Equals(value, myCurrentSelectedRow)) return;
                myCurrentSelectedRow = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<PrhodOrderRow> MainRows { set; get; } =
            new ObservableCollection<PrhodOrderRow>();

        public ObservableCollection<PrhodOrderRow> ActualMainRows { set; get; } =
            new ObservableCollection<PrhodOrderRow>();

        public ObservableCollection<PrhodOrderRow> SelectedRows { set; get; } =
            new ObservableCollection<PrhodOrderRow>();


        public ObservableCollection<PrhodOrderRow> ActualSelectedRows { set; get; } =
            new ObservableCollection<PrhodOrderRow>();

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
            get { return new Command(AddSelectedRow, _ => ActualMainRows.Count > 0); }
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
            get { return new Command(DeleteSelected, _ => ActualSelectedRows.Count > 0); }
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
