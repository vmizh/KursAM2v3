using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Core;
using Core.EntityViewModel.NomenklManagement;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using KursAM2.Managers;
using KursAM2.Repositories;
using KursAM2.View.Logistiks.Warehouse;
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

        private Waybill myCurrentDocument;

        public WaybillSearchViewModel()
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            SD_24Repository = new SD_24Repository(UnitOfWork);
            WindowName = "Расходные накладные для клиентов";
            Documents = new ObservableCollection<Waybill>();
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
            StartDate = DateTime.Today.AddDays(-30);
            EndDate = DateTime.Today;
        }

        public WaybillSearchViewModel(Window form) : base(form)
        {
            GenericProviderRepository = new GenericKursDBRepository<SD_24>(UnitOfWork);
            SD_24Repository = new SD_24Repository(UnitOfWork);
            WindowName = "Расходные накладные для клиентов";
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

        public Waybill CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument != null && myCurrentDocument.Equals(value)) return;
                myCurrentDocument = value;
                if (myCurrentDocument != null)
                {
                    IsDocumentOpenAllow = true;
                    IsDocNewCopyAllow = true;
                    IsDocNewCopyRequisiteAllow = true;
                    IsPrintAllow = true;
                }

                RaisePropertyChanged();
            }
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<Waybill> Documents { set; get; } = new ObservableCollection<Waybill>();

        public override string WindowName => "Поиск расходных ракладных для клипентов";
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
            var ctx = new WaybillWindowViewModel(CurrentDocument.DocCode);
            ctx.ExportWayBill(null);
        }

        private void PrintSF(object obj)
        {
            var ctx = new WaybillWindowViewModel(CurrentDocument.DocCode);
            ctx.PrintWaybill(null);
        }

        #region Commands

        private Task Load()
        {

            var res = Task.Factory.StartNew(() =>
            {
                var data = SD_24Repository.GetWayBillAllByDates(StartDate, EndDate);
                List<Waybill> w = new List<Waybill>();
                foreach (var d in data)
                {
                    w.Add(new Waybill(d)
                    {
                        State=RowStatus.NotEdited
                    });
                }
                return w;
            });
            Documents.Clear();
            foreach (var d in res.Result) Documents.Add(d);
            RaisePropertyChanged(nameof(Documents));
            DispatcherService.BeginInvoke(SplashScreenService.HideSplashScreen);
            return res;
        }

        public override async void RefreshData(object data)
        {
            SplashScreenService.ShowSplashScreen();
            IsDocumentOpenAllow = false;
            IsDocNewCopyAllow = false;
            IsDocNewCopyRequisiteAllow = false;
            IsPrintAllow = false;
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }
            base.RefreshData(null);
            await Load();
        }

        public override void DocumentOpen(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new WaybillWindowViewModel(CurrentDocument.DocCode);
            var frm = new WaybillView {Owner = Application.Current.MainWindow, DataContext = ctx};
            ctx.Form = frm;
            frm.Show();
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new WaybillView {Owner = Application.Current.MainWindow};
            var ctx = new WaybillWindowViewModel {Form = frm};
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopy(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new WaybillView {Owner = Application.Current.MainWindow};
            var ctx = new WaybillWindowViewModel
            {
                Form = frm, Document = DocManager.NewWaybillCopy(CurrentDocument.DocCode)
            };
            frm.Show();
            frm.DataContext = ctx;
        }

        public override void DocNewCopyRequisite(object obj)
        {
            if (CurrentDocument == null) return;
            var frm = new WaybillView {Owner = Application.Current.MainWindow};
            var ctx = new WaybillWindowViewModel
                {Form = frm};
            ctx.Document = DocManager.NewWaybillRecuisite(CurrentDocument.DocCode);
            frm.Show();
            frm.DataContext = ctx;
        }

        #endregion
    }
}