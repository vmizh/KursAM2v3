using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using KursAM2.ReportManagers.SFClientAndWayBill;
using Reports.Base;

namespace KursAM2.ViewModel.Logistiks
{
    public class WaybillWindowViewModel : RSWindowViewModelBase
    {
        private WaybillRow myCurrentNomenklRow;
        private Waybill myDocument;

        public WaybillWindowViewModel()
        {
            ReportManager = new ReportManager();
            CreateReports();
            LoadByWhoom();
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocRightBar(this);
            var prn = RightMenuBar.FirstOrDefault(_ => _.Name == "Print");
            if (prn != null)
            {
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Товарная накладная",
                    Command = PrintWaybillCommand
                });
                prn.SubMenu.Add(new MenuButtonInfo
                {
                    Caption = "Экспорт",
                    Command = ExportCommand
                });
            }
        }

        public WaybillWindowViewModel(decimal dc) : this()
        {
            Refresh(dc);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ReportManager ReportManager { get; set; }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<string> ByWhoomLicoList { set; get; } = new ObservableCollection<string>();
        public List<Core.EntityViewModel.Warehouse> Sklads => MainReferences.Warehouses.Values.ToList();

        public Waybill Document
        {
            get => myDocument;
            set
            {
                if (myDocument != null && myDocument.Equals(value)) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        public override string WindowName => Document?.Name;
        public List<Nomenkl> Nomenkls => MainReferences.ALLNomenkls.Values.ToList();
        public List<Kontragent> Kontragents => MainReferences.ActiveKontragents.Values.ToList();

        public override bool IsCanSaveData
        {
            get
            {
                return Document.State != RowStatus.NotEdited ||
                       Document.Rows.Any(_ => _.State != RowStatus.NotEdited
                                              || Document.DeletedRows != null && Document.DeletedRows.Count > 0)
                       && Document.Rows.All(x => x.DDT_KOL_RASHOD > 0);
            }
            set => base.IsCanSaveData = value;
        }

        public WaybillRow CurrentNomenklRow
        {
            get => myCurrentNomenklRow;
            set
            {
                if (myCurrentNomenklRow != null && myCurrentNomenklRow.Equals(value)) return;
                myCurrentNomenklRow = value;
                RaisePropertyChanged();
            }
        }

        private void CreateReports()
        {
            ReportManager.Reports.Add("Экспорт", new WaybillTorg12(this)
            {
                PrintOptions = new KursReportA4PrintOptions(),
                ShowType = ReportShowType.Spreadsheet,
                XlsFileName = "torg12"
            });
            ReportManager.Reports.Add("Торг12", new WaybillTorg12(this)
            {
                PrintOptions = new KursReportLandscapeA4PrintOptions
                {
                    FitToWidth = 1
                },
                ShowType = ReportShowType.Report,
                XlsFileName = "torg12"
            });
        }

        private void LoadByWhoom()
        {
            ByWhoomLicoList.Clear();
            foreach (var item in GlobalOptions.GetEntities()
                .Database.SqlQuery<string>("SELECT DISTINCT DD_KOMU_PEREDANO from sd_24 (nolock)")
                .ToList())
                ByWhoomLicoList.Add(item);
        }

        private void Refresh(decimal dc)
        {
            LoadByWhoom();
            using (var ctx = GlobalOptions.GetEntities())
            {
                try
                {
                    var data = ctx.SD_24
                        .Include(_ => _.TD_24)
                        .Include(_ => _.SD_84)
                        .AsNoTracking()
                        .SingleOrDefault(_ => _.DOC_CODE == dc);
                    if (data == null) return;
                    Document = new Waybill(data) {State = RowStatus.NotEdited};
                    Document.DeletedRows.Clear();
                    foreach (var r in Document.Rows)
                    {
                        var lr = ctx.TD_84.FirstOrDefault(_ =>
                            _.DOC_CODE == r.DDT_SFACT_DC && _.CODE == r.DDT_SFACT_ROW_CODE);
                        if (lr != null) r.SchetLinkedRow = new InvoiceClientRow(lr);
                        r.State = RowStatus.NotEdited;
                    }
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }
            }
        }

        #region Command

        public override bool IsRedoAllow
        {
            get => Document.DeletedRows != null && Document.DeletedRows.Count > 0;
            set => base.IsRedoAllow = value;
        }

        public Command ExportCommand
        {
            get { return new Command(ExportWayBill, param => true); }
        }

        public void ExportWayBill(object obj)
        {
            ReportManager.Reports["Экспорт"].Show();
        }

        public Command PrintWaybillCommand
        {
            get { return new Command(PrintWaybill, param => true); }
        }

        public void PrintWaybill(object obj)
        {
            ReportManager.Reports["Торг12"].Show();
        }

        public Command SFClientSelectCommand
        {
            get { return new Command(SFClientSelect, _ => true); }
        }

        private void SFClientSelect(object obj)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        #endregion
    }
}