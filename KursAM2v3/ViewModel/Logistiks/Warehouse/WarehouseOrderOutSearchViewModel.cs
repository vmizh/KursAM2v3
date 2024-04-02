using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using DevExpress.Data.Linq.Helpers;
using DevExpress.Pdf.Native;
using KursAM2.Managers;
using KursAM2.Managers.Nomenkl;
using KursAM2.ReportManagers;
using KursAM2.View.Base;
using KursAM2.View.Logistiks.Warehouse;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Managers;
using KursDomain.Menu;
using Application = System.Windows.Application;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class WarehouseOrderOutSearchViewModel : RSWindowSearchViewModelBase
    {
        private readonly WarehouseManager orderManager;
        private WarehouseOrderOut myCurrentDocument;
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());

        public WarehouseOrderOutSearchViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            orderManager =
                new WarehouseManager(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderSearchViewModel"));
            StartDate = new DateTime(DateTime.Today.Year,1,1);
            EndDate = DateTime.Today;
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            prn?.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Ордер",
                Command = PrintOrderCommand
            });
        }

        public ICommand PrintOrderCommand
        {
            get { return new Command(PrintOrder, _ => CurrentDocument != null); }
        }

        public ObservableCollection<WarehouseOrderOut> Documents { set; get; } =
            new ObservableCollection<WarehouseOrderOut>();

        public override string WindowName => "Поиск расходных складских ордеров";
        public override string LayoutName => "WarehouseOrderOutSearchViewModel";

        public WarehouseOrderOut CurrentDocument
        {
            set
            {
                if (myCurrentDocument == value) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
            get => myCurrentDocument;
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;
        public override bool IsDocNewCopyRequisiteAllow => CurrentDocument != null;
        public override bool IsDocNewCopyAllow => CurrentDocument != null;

        private void PrintOrder(object obj)
        {
            ReportManager.WarehouseOrderOutReport(CurrentDocument.DocCode);
        }

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
            Documents.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var d = ctx.SD_24.Where(_ =>
                        _.DD_DATE >= StartDate && _.DD_DATE <= EndDate &&
                        _.DD_TYPE_DC == 2010000003).ToList();
                    foreach (var item in d)
                        Documents.Add(new WarehouseOrderOut(item) { State = RowStatus.NotEdited });
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }

            RaisePropertyChanged(nameof(Documents));
            SearchText = "";
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
        }

        public override void DocumentOpen(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new OrderOutWindowViewModel(
                new StandartErrorManager(GlobalOptions.GetEntities(), "WarehouseOrderOut", true)
                , CurrentDocument.DocCode);
            var frm = new OrderOutView
            {
                Owner = Application.Current.MainWindow,
                DataContext = ctx
            };
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new OrderOutView { Owner = Application.Current.MainWindow };
            var ctx = new OrderOutWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                "WarehouseOrderOut", true)) { Form = frm };
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new OrderOutView { Owner = Application.Current.MainWindow };
            var ctx = new OrderOutWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                { Form = frm };
            ctx.Document = orderManager.NewOrderOutCopy(CurrentDocument);
            foreach (var rOut in ctx.Document.Rows)
            {
             var nq = nomenklManager.GetNomenklQuantity(ctx.Document.WarehouseOut.DocCode, rOut.DDT_NOMENKL_DC, ctx.Document.Date, ctx.Document.Date);
             rOut.MaxQuantity =  nq.Count == 0 ? 0 : nq.First().OstatokQuantity;;
            }
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new OrderOutView { Owner = Application.Current.MainWindow };
            var ctx = new OrderOutWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                { Form = frm };
            ctx.Document = orderManager.NewOrderOutRecuisite(CurrentDocument);
            frm.Show();
            frm.DataContext = ctx;
        }

        protected override void OnWindowLoaded(object obj)
        {
            base.OnWindowLoaded(obj);
            if (Form is StandartSearchView frm)
            {
                foreach (var col in frm.gridDocuments.Columns)
                {
                    switch (col.FieldName)
                    {
                        case "State":
                            col.Visible = false;
                            break;
                    }
                }
            }
        }

        #endregion
    }
}
