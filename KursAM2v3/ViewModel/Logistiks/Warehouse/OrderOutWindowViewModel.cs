using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using KursAM2.Managers.Invoices;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public class OrderOutWindowViewModel : RSWindowViewModelBase
    {
        private readonly WarehouseManager orderManager;
        private WarehouseOrderOut myDocument;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public OrderOutWindowViewModel(StandartErrorManager errManager)
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            orderManager = new WarehouseManager(errManager);
        }

        public OrderOutWindowViewModel(StandartErrorManager errManager, decimal dc)
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = true;
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            orderManager = new WarehouseManager(errManager);
            RefreshData(dc);
        }

        public WarehouseOrderOut Document
        {
            set
            {
                if (myDocument != null && myDocument.Equals(value)) return;
                myDocument = value;
                RaisePropertyChanged();
            }
            get => myDocument;
        }

        public override void RefreshData(object obj)
        {
            var dc = obj as decimal? ?? 0;
            if (dc != 0)
                Document = orderManager.GetOrderOut(dc);
            RaisePropertyChanged(nameof(Document));
        }

        #region Справочники

        public List<Kontragent> Kontragents => MainReferences.ActiveKontragents.Values.ToList();
        public List<Core.EntityViewModel.Warehouse> StoreDictionary => MainReferences.Warehouses.Values.ToList();

        #endregion
    }
}