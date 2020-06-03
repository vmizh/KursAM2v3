using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Managers;
using KursAM2.View.Logistiks;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklTransferSearchViewModel : RSWindowSearchViewModelBase
    {
        private NomenklTransferSearch myCurrentRow;
        private DateTime myDateEnd;
        private DateTime myDateStart;
        private Nomenkl mySelectedTransferNomenkl;

        public NomenklTransferSearchViewModel()
        {
            WindowName = "Документы по актам таксировки товаров";
            SecondSearchName = "Номенклатура";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartSearchRightBar(this);
            DateStart = DateTime.Today.AddDays(-30);
            DateEnd = DateTime.Today;
            LoadNomenkls();
        }

        public ObservableCollection<Nomenkl> TransferNomenkls { set; get; } =
            new ObservableCollection<Nomenkl>();

        public ICommand ClearSearchNomenklCommand
        {
            get { return new Command(ClearSearchNomenkl, _ => SelectedTransferNomenkl != null); }
        }

        public DateTime DateStart
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                if (myDateStart > DateEnd)
                    myDateEnd = DateStart;
                RaisePropertyChanged();
            }
        }

        public DateTime DateEnd
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
                if (myDateEnd < myDateStart)
                    myDateStart = myDateEnd;
                RaisePropertyChanged();
            }
        }

        public Nomenkl SelectedTransferNomenkl
        {
            get => mySelectedTransferNomenkl;
            set
            {
                if (mySelectedTransferNomenkl != null && mySelectedTransferNomenkl.Equals(value)) return;
                mySelectedTransferNomenkl = value;
                RaisePropertyChanged();
            }
        }

        public override string SearchText
        {
            get => mySearchText;
            set
            {
                if (mySearchText == value) return;
                mySearchText = value;
                RaisePropertyChanged();
            }
        }

        public NomenklTransferSearch CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow == value) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsDocumentOpenAllow => CurrentRow != null;

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<NomenklTransferSearch> Data { set; get; } =
            new ObservableCollection<NomenklTransferSearch>();

        private void ClearSearchNomenkl(object obj)
        {
            SelectedTransferNomenkl = null;
        }

        private void LoadNomenkls()
        {
            TransferNomenkls.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (
                        var n in
                        ctx.NomenklTransferRow)
                        if (TransferNomenkls.All(_ => _.DocCode != n.NomenklInDC))
                            TransferNomenkls.Add(MainReferences.GetNomenkl(n.NomenklInDC));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void DocNewEmpty(object form)
        {
            var frm = new NomenklTransferView
            {
                Owner = Application.Current.MainWindow,
                DataContext = new NomenklTransferWindowViewModel(null)
            };
            frm.Show();
        }

        public override void DocumentOpen(object obj)
        {
            DocumentsOpenManager.Open(DocumentType.NomenklTransfer, CurrentRow.Id);
        }

        public override void RefreshData(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    if (string.IsNullOrEmpty(SearchText))
                    {
                        List<NomenklTransferRow> data;
                        if (SelectedTransferNomenkl == null)
                            data =
                                ctx.NomenklTransferRow.Include(_ => _.NomenklTransfer)
                                    .Include(_ => _.NomenklTransfer.SD_27)
                                    .AsNoTracking()
                                    .Where(_ => _.NomenklTransfer.Date >= DateStart
                                                && _.NomenklTransfer.Date <= DateEnd)
                                    .ToList();
                        else
                            data = (from tt in ctx.NomenklTransferRow
                                join ss in ctx.NomenklTransfer on tt.DocId equals ss.Id
                                join sd27 in ctx.SD_27 on ss.SkladDC equals sd27.DOC_CODE
                                where ss.Date >= DateStart && ss.Date <= DateEnd
                                                           && (tt.NomenklInDC == SelectedTransferNomenkl.DocCode
                                                               || tt.NomenklOutDC == SelectedTransferNomenkl.DocCode)
                                select tt).ToList();
                        Data.Clear();
                        foreach (var d in data)
                            //var firstOrDefault = ctx.NOM_PRICE.AsNoTracking()
                            //    .FirstOrDefault(
                            //        _ =>
                            //            _.NOM_DC == d.NomenklOutDC &&
                            //            _.DATE ==
                            //            ctx.NOM_PRICE.Where(
                            //                    p => p.NOM_DC == d.NomenklOutDC && _.DATE <= d.NomenklTransfer.Date)
                            //                .Max(dd => dd.DATE));
                            Data.Add(new NomenklTransferSearch
                            {
                                Id = d.NomenklTransfer.Id,
                                DocNum = d.NomenklTransfer.DucNum,
                                Date = d.NomenklTransfer.Date,
                                StoreName = d.NomenklTransfer.SD_27.SKL_NAME,
                                IsAccepted = d.IsAccepted,
                                NomenklIn = MainReferences.GetNomenkl(d.NomenklInDC),
                                NomenklOut = MainReferences.GetNomenkl(d.NomenklOutDC),
                                MaxQuantity =
                                    GetMaxQuantity(d.NomenklTransfer.SkladDC, d.NomenklOutDC, d.NomenklTransfer.Date),
                                Rate = d.Rate,
                                Quantity = d.Quantity,
                                PriceIn = d.PriceIn,
                                PriceOut = d.PriceOut,
                                SummaIn = Math.Round(d.PriceIn * d.Quantity, 2),
                                SummaOut = Math.Round(d.PriceOut * d.Quantity, 2),
                                Note = d.NomenklTransfer.Note + " / " + d.Note,
                                LastUpdate = d.LastUpdate,
                                LastUpdater = d.LastUpdater,
                                IsPriceAccepted = d.IsPriceAcepted ?? false,
                                NakladEdSumma = d.NakladEdSumma ?? 0,
                                NakladNewEdSumma = d.NakladNewEdSumma ?? 0,
                                NakladRate = d.NakladRate ?? 0
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        private decimal GetMaxQuantity(decimal? skladDC, decimal nomDC, DateTime date)
        {
            var q = Nomenkl.Quantity(skladDC, nomDC, date);
            return q < 0 ? 0 : q;
        }

        public class NomenklTransferSearch : NomenklTransferRowViewModelExt
        {
            public NomenklTransferSearch()
            {
                IsCalcConrol = false;
            }

            public int DocNum { set; get; }
            public DateTime Date { set; get; }
            public string StoreName { set; get; }
        }
    }
}