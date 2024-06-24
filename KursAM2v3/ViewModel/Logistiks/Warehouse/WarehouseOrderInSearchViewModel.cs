using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.Base;
using KursAM2.View.Logistiks.Warehouse;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class WarehouseOrderInSearchViewModel : RSWindowSearchViewModelBase
    {
        private readonly WarehouseManager orderManager;
        private WarehouseOrderIn myCurrentDocument;

        public WarehouseOrderInSearchViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            orderManager =
                new WarehouseManager(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderInSearchViewModel"));
            StartDate = new  DateTime( DateTime.Today.AddMonths(-1).Year, DateTime.Today.AddMonths(-1).Month,1);
            EndDate = DateTime.Today;
        }

        public ObservableCollection<WarehouseOrderIn> Documents { set; get; } =
            new ObservableCollection<WarehouseOrderIn>();

        public WarehouseOrderIn CurrentDocument
        {
            set
            {
                if (Equals(myCurrentDocument, value)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
            get => myCurrentDocument;
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;
        public override bool IsDocNewCopyAllow => false;
        public override bool IsDocNewCopyRequisiteAllow => CurrentDocument != null;

        public override string WindowName => "Поиск приходных складских ордеров";
        public override string LayoutName => "WarehouseOrderInSearchViewModel";

        #region Commands

        public ICommand DoubleClickCommand
        {
            get { return new Command(DoubleClick, _ => true); }
        }

        private void DoubleClick(object obj)
        {
            DocumentOpen(null);
        }

        public override void Search(object obj)
        {
            GetSearchDocument(obj);
        }

        private void Delete(object obj)
        {
        }

        public void GetSearchDocument(object obj)
        {
            try
            {
                Documents.Clear();
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public ICommand DeleteCommand
        {
            get { return new Command(Delete, _ => true); }
        }

        public override void SearchClear(object obj)
        {
            base.SearchClear(obj);
            RefreshData(null);
        }

        public override void RefreshData(object data)
        {
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            try
            {
                var rows = new List<WarehouseOrderIn>();
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var d = ctx.SD_24.Where(_ =>
                            _.DD_DATE >= StartDate && _.DD_DATE <= EndDate &&
                            _.DD_TYPE_DC == 2010000001).OrderByDescending(_ => _.DD_DATE)
                        .ToList(); /*приходный складской ордер*/
                    foreach (var item in d)
                        rows.Add(new WarehouseOrderIn(item) { State = RowStatus.NotEdited });
                    Documents.Clear();
                    foreach (var item in rows)
                    {
                        item.WarehouseSenderType = item.KontragentSender != null ? WarehouseSenderType.Kontragent : WarehouseSenderType.Store;
                        Documents.Add(item);

                    } 
                    //Documents = new ObservableCollection<WarehouseOrderIn>(rows);
                    RaisePropertyChanged(nameof(Documents));
                }
                
                
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }

            RaisePropertyChanged(nameof(Documents));
            SearchText = "";
            //GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
        }

        public override void DocumentOpen(object form)
        {
            if (CurrentDocument == null) return;

            var ctx = new OrderInWindowViewModel(
                new StandartErrorManager(GlobalOptions.GetEntities(), "WarehouseOrderIn", true)
                , CurrentDocument.DocCode);
            var frm = new OrderInView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                "WarehouseOrderIn", true))
            {
                Form = frm
            };
            ctx.Document.myState = RowStatus.NewRow;
            frm.DataContext = ctx;
            frm.Show();
            
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                {Form = frm};
            ctx.Document = orderManager.NewOrderInCopy(CurrentDocument);
            ctx.Document.myState = RowStatus.NewRow;
            frm.DataContext = ctx;
            frm.Show();
            
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var dbContext = GlobalOptions.GetEntities();
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(dbContext,
                    "WarehouseOrderIn", true), 0)
                {Form = frm,
                    Document = orderManager.NewOrderInRecuisite(CurrentDocument)
                };

            ctx.Document.myState = RowStatus.NewRow;
            ctx.Document.WarehouseSenderType = ctx.Document.KontragentSender != null
                ? WarehouseSenderType.Kontragent
                : WarehouseSenderType.Store;
            frm.DataContext = ctx;
            frm.Show();
           
        }

        #endregion
    }
}
