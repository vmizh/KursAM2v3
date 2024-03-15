using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using KursAM2.Managers;
using KursAM2.Repositories;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.View.Base;
using KursAM2.View.Logistiks.Warehouse;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.Repository;
using Reports.Base;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public sealed class WaybillSearchViewModel : RSWindowSearchViewModelBase
    {
        private readonly WarehouseManager DocManager = new WarehouseManager(new StandartErrorManager(
            GlobalOptions.GetEntities(),
            "WaybillViewModel"));

        public readonly GenericKursDBRepository<SD_24> GenericProviderRepository;

        // ReSharper disable once NotAccessedField.Local
        public readonly ISD_24Repository SD_24Repository;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        private WayBillShort myCurrentDocument;

        public WaybillSearchViewModel()
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            SD_24Repository = new SD_24Repository(UnitOfWork);
            Documents = new ObservableCollection<WayBillShort>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            if (prn != null)
            {
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Товарная накладная",
                    Command = PrintSFCommand
                });
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportSFCommand
                });
            }

            //StartDate = DateTime.Today.AddDays(-30);
            //начальная дата поиска - 1-е число предыдущего месяца или 1-го января
            StartDate = new DateTime(DateTime.Today.Year, ((DateTime.Today.Month != 1) ? (DateTime.Today.Month - 1) : 1), 1);
            EndDate = DateTime.Today;
        }

        /*
        public WaybillSearchViewModel(Window form) : base(form)
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            SD_24Repository = new SD_24Repository(UnitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            if (prn == null) return;
            prn.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Товарная накладная",
                Command = PrintSFCommand
            });
            prn.SubMenu.Add(new MenuButtonInfo
            {
                Caption = "Экспорт",
                Command = ExportSFCommand
            });
            StartDate = DateTime.Today.AddDays(-30);
            EndDate = DateTime.Today;
        }
        */

        public WayBillShort CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument?.DocCode == value?.DocCode) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;
        public override bool IsDocNewCopyAllow => CurrentDocument != null;
        public override bool IsDocNewCopyRequisiteAllow => CurrentDocument != null;
        public override bool IsPrintAllow => CurrentDocument != null;

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<WayBillShort> Documents { set; get; } = new ObservableCollection<WayBillShort>();

        public override string WindowName => "Поиск расходных накладных для клиентов";
        public override string LayoutName => "WaybillSearchViewModel";

        public Command ExportSFCommand
        {
            get { return new Command(ExportSF, _ => IsDocumentOpenAllow); }
        }

        public Command PrintSFCommand
        {
            get { return new Command(PrintSF, _ => IsDocumentOpenAllow); }
        }

        public override void Print(object form)
        {
            var rep = new ExportView();
            rep.Show();
        }

        private void ExportSF(object obj)
        {
            var ctx = new WaybillWindowViewModel2(CurrentDocument.DocCode);
            ctx.ExportWayBill(null);
        }

        private void PrintSF(object obj)
        {
            var ctx = new WaybillWindowViewModel2(CurrentDocument.DocCode);
            ctx.PrintWaybill(null);
        }

        #region Commands

        //private Task Load()
        //{
        //    GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
        //    var res = Task.Factory.StartNew(() =>
        //    {
        //        var data = SD_24Repository.GetWayBillAllByDates(StartDate, EndDate);
        //        List<WayBillShort> w = new List<WayBillShort>();
        //        foreach (var d in data)
        //        {
        //            w.Add(new WayBillShort(d)
        //            {
        //                DocCode = d.DOC_CODE,
        //                InvoiceClient = d.SD_84 != null
        //                    ? $"С/ф №{d.SD_84.SF_IN_NUM}/{d.SD_84.SF_OUT_NUM} от {d.SD_84.SF_DATE.ToShortDateString()}"
        //                    : null,
        //                State = RowStatus.NotEdited
        //            });
        //        }

        //        return w;
        //    });
        //    Documents.Clear();
        //    foreach (var d in res.Result) Documents.Add(d);
        //    RaisePropertyChanged(nameof(Documents));
        //    DispatcherService.BeginInvoke(SplashScreenService.HideSplashScreen);
        //    GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
        //    return res;
        //}

        public override void RefreshData(object data)
        {
            var frm = Form as StandartSearchView;
            Documents.Clear();
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            Task.Run(() =>
            {
                frm?.Dispatcher.Invoke(() => { frm.loadingIndicator.Visibility = Visibility.Visible; });
                var result = SD_24Repository.GetWayBillAllByDates(StartDate, EndDate)
                    .Select(d => new WayBillShort(d)
                    {
                        DocCode = d.DOC_CODE,
                        InvoiceClient = d.SD_84 != null
                            ? $"С/ф №{d.SD_84.SF_IN_NUM}/{d.SD_84.SF_OUT_NUM} от {d.SD_84.SF_DATE.ToShortDateString()}"
                            : null,
                        State = RowStatus.NotEdited
                    })
                    .ToList();
                frm?.Dispatcher.Invoke(() =>
                {
                    foreach (var d in result) Documents.Add(d);
                    frm.loadingIndicator.Visibility = Visibility.Hidden;
                    RaisePropertyChanged(nameof(Documents));
                });
                GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
            });
        }

        public override void DocumentOpen(object form)
        {
            DocumentsOpenManager.Open(DocumentType.Waybill, CurrentDocument.DocCode);
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new WayBillView2 {Owner = Application.Current.MainWindow};
            var ctx = new WaybillWindowViewModel2(null) {Form = frm};
            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new WayBillView2 {Owner = Application.Current.MainWindow};
            var ctx = new WaybillWindowViewModel2(null)
            {
                Form = frm, Document = DocManager.NewWaybillCopy(CurrentDocument.DocCode)
            };

            frm.DataContext = ctx;
            frm.Show();
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new WayBillView2 {Owner = Application.Current.MainWindow};
            var ctx = new WaybillWindowViewModel2(null)
                {Form = frm};
            ctx.Document = DocManager.NewWaybillRecuisite(CurrentDocument.DocCode);
            frm.DataContext = ctx;
            frm.Show();
        }

        #endregion
    }
}
