using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers;
using KursAM2.View.Logistiks.Warehouse;
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
            StartDate = DateTime.Today.AddDays(-30);
            EndDate = DateTime.Today;
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
        public override bool IsDocNewCopyAllow => CurrentDocument != null;
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
            get { return new Command(Delete, _ => true); }
        }

        public override void SearchClear(object obj)
        {
            base.SearchClear(obj);
            RefreshData(null);
        }

        public override void RefreshData(object data)
        {
            //Documents = new ObservableCollection<WarehouseOrderIn>();
            try
            {
                var rows = new List<WarehouseOrderIn>();
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var d = ctx.SD_24.Where(_ =>
                        _.DD_DATE >= StartDate && _.DD_DATE <= EndDate &&
                        _.DD_TYPE_DC == 2010000001).OrderByDescending(_ => _.DD_DATE).ToList(); /*приходный складской ордер*/
                    foreach (var item in d)
                        rows.Add(new WarehouseOrderIn(item) { State = RowStatus.NotEdited });
                    Documents = new ObservableCollection<WarehouseOrderIn>(rows);
                    RaisePropertyChanged(nameof(Documents));
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
            var frm = new OrderInView { Owner = Application.Current.MainWindow };
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                "WarehouseOrderIn", true)) { Form = frm };
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new OrderInView { Owner = Application.Current.MainWindow };
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                { Form = frm };
            ctx.Document = orderManager.NewOrderInCopy(CurrentDocument);
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new OrderInView { Owner = Application.Current.MainWindow };
            var ctx = new OrderInWindowViewModel(new StandartErrorManager(GlobalOptions.GetEntities(),
                    "WarehouseOrderIn", true))
                { Form = frm };
            ctx.Document = orderManager.NewOrderInRecuisite(CurrentDocument);
            frm.Show();
            frm.DataContext = ctx;
        }

        #endregion
    }
}
