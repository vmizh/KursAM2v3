using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Managers;
using KursAM2.View.Logistiks.Warehouse;
using Reports.Base;

namespace KursAM2.ViewModel.Logistiks.Warehouse
{
    public class WaybillSearchViewModel : RSWindowSearchViewModelBase
    {
        private readonly WarehouseManager DocManager = new WarehouseManager(new StandartErrorManager(
            GlobalOptions.GetEntities(),
            "WaybillViewModel"));

        private Waybill myCurrentDocument;

        public WaybillSearchViewModel()
        {
            WindowName = "Расходные накладные для клиентов";
            DocumentCollection = new ObservableCollection<Waybill>();
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
        }

        public WaybillSearchViewModel(Window form) : base(form)
        {
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
        public ObservableCollection<Waybill> DocumentCollection { set; get; } = new ObservableCollection<Waybill>();

        public Command ExportSFCommand
        {
            get { return new Command(ExportSF, param => IsDocumentOpenAllow); }
        }

        public Command PrintSFCommand
        {
            get { return new Command(PrintSF, param => IsDocumentOpenAllow); }
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

        public override void RefreshData(object data)
        {
            try
            {
                IsDocumentOpenAllow = false;
                IsDocNewCopyAllow = false;
                IsDocNewCopyRequisiteAllow = false;
                IsPrintAllow = false;
                while (!MainReferences.IsReferenceLoadComplete)
                {
                }

                DocumentCollection.Clear();

                var d = DocManager.GetWaybills(StartDate, EndDate);
                foreach (var item in d)
                    DocumentCollection.Add(item);
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }

            RaisePropertyChanged();
        }

        public override void DocumentOpen(object form)
        {
            if (CurrentDocument == null) return;
            var ctx = new WaybillWindowViewModel(CurrentDocument.DocCode);
            var frm = new WaybillView {Owner = Application.Current.MainWindow, DataContext = ctx};
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