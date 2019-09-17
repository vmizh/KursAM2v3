using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers.Invoices;
using KursAM2.View.Finance.Invoices;
using KursAM2.View.Logistiks.Warehouse;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public class WarehouseOrderSearchViewModel : RSWindowSearchViewModelBase
    {
        private readonly WarehouseManager orderManager;
        private WarehouseOrderIn myCurrentDocument;

        public WarehouseOrderSearchViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            orderManager =
                new WarehouseManager(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderSearchViewModel"));
            WindowName = "Приходные складские ордера";
        }

        public ObservableCollection<WarehouseOrderIn> Documents { set; get; } =
            new ObservableCollection<WarehouseOrderIn>();

        public WarehouseOrderIn CurrentDocument
        {
            set
            {
                if (myCurrentDocument != null && myCurrentDocument.Equals(value)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
            get => myCurrentDocument;
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        #region Commands

        public override void Search(object obj)
        {
            GetSearchDocument(obj);
        }

        private void Delete(object obj)
        {
            if (CurrentDocument != null)
                InvoicesManager.DeleteProvider(CurrentDocument.DocCode);
            RefreshData(null);
        }

        public void GetSearchDocument(object obj)
        {
            try
            {
                Documents.Clear();
                // ReSharper disable once UnusedVariable
                using (var ctx = GlobalOptions.GetEntities())
                {
                    //var query = ctx
                    //    .SD_84
                    //    .Include(_ => _.SD_43)
                    //    .Include(_ => _.SD_431)
                    //    .Include(_ => _.SD_432)
                    //    .Include(_ => _.SD_301)
                    //    .Include(_ => _.TD_84)
                    //    .Include("TD_84.SD_83")
                    //    .Include("TD_84.TD_24")
                    //    .Where(_ => _.SF_DATE >= StartDate && _.SF_DATE <= EndDate);
                    //foreach (var item in query.ToList())
                    //{
                    //    var newItem = new InvoiceClient(item);
                    //    if (item.TD_84 != null && item.TD_84.Count > 0)
                    //        newItem.SummaOtgruz = item.TD_84.Sum(i =>
                    //            i.TD_24.Sum(i2 => i.SFT_ED_CENA * i2.DDT_KOL_RASHOD ?? 0));
                    //    string d;
                    //    d = newItem.Diler != null ? newItem.Diler.Name : "";
                    //    if (newItem.SF_IN_NUM.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                    //        newItem.SF_OUT_NUM.ToUpper().Contains(SearchText.ToUpper()) ||
                    //        newItem.SF_CLIENT_NAME.ToUpper().Contains(SearchText.ToUpper()) ||
                    //        newItem.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                    //        newItem.SF_DILER_SUMMA.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                    //        newItem.CO.Name.ToUpper().Contains(SearchText.ToUpper()) ||
                    //        newItem.SF_CRS_SUMMA_K_OPLATE.ToString().ToUpper().Contains(SearchText.ToUpper()) ||
                    //        d.ToUpper().Contains(SearchText.ToUpper()))
                    //        Documents.Add(newItem);
                    //}
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public ICommand DeleteCommand
        {
            get { return new Command(Delete, param => true); }
        }

        public override void SearchClear(object obj)
        {
            base.SearchClear(obj);
            RefreshData(null);
        }

        public override void RefreshData(object data)
        {
            Documents = new ObservableCollection<WarehouseOrderIn>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var d = ctx.SD_24.Where(_ =>
                        _.DD_DATE >= StartDate && _.DD_DATE <= EndDate &&
                        _.DD_TYPE_DC == 2010000001).ToList(); /*приходный складской ордер*/
                    foreach (var item in d)
                        Documents.Add(new WarehouseOrderIn(item));
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }

            RaisePropertyChanged(nameof(Documents));
            SearchText = "";
        }

        public override void DocumentOpen(object form)
        {
            if (CurrentDocument == null) return;
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(
                new StandartErrorManager(GlobalOptions.GetEntities(), "WarehouseOrderIn", true)
                , CurrentDocument.DocCode)
            {
                Form = frm
            };
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                "WarehouseOrderIn", true)) {Form = frm};
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                {Form = frm};
            ctx.Document = orderManager.NewOrderInCopy(CurrentDocument);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new OrderInView {Owner = Application.Current.MainWindow};
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                {Form = frm};
            ctx.Document = orderManager.NewOrderInRecuisite(CurrentDocument);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void ResetLayout(object form)
        {
            var frm = form as SearchInvoiceClientView;
            frm?.LayoutManager.ResetLayout();
        }

        #endregion
    }
}