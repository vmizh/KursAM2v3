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
using Core.ViewModel.Common;
using KursAM2.Managers;
using KursAM2.View.Base;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklMoveOnSkladWindowViewModel : RSWindowViewModelBase
    {
        private NomPriceDocumentViewModel myCurrentDocument;
        private NomenklMoveOnSkladViewModel myCurrentNomenklMoveItem;
        private Core.EntityViewModel.Warehouse myCurrentSklad;
        private DateTime myEndDate;
        private bool myIsShowAll;
        private DateTime myStartDate;

        public NomenklMoveOnSkladWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            IsShowAll = false;
        }

        public NomenklMoveOnSkladWindowViewModel(Window form) : base(form)
        {
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            StartDate = DateTime.Today;
            LoadReferences();
        }

        public NomPriceDocumentViewModel CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument != null && myCurrentDocument.Equals(value)) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<NomenklMoveOnSkladViewModel> NomenklMoveList { set; get; } =
            new ObservableCollection<NomenklMoveOnSkladViewModel>();

        public ObservableCollection<NomPriceDocumentViewModel> DocumentList { get; set; } =
            new ObservableCollection<NomPriceDocumentViewModel>();

        public List<Core.EntityViewModel.Warehouse> Sklads { set; get; } = new List<Core.EntityViewModel.Warehouse>();

        public NomenklMoveOnSkladViewModel CurrentNomenklMoveItem
        {
            get => myCurrentNomenklMoveItem;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentNomenklMoveItem == value) return;
                myCurrentNomenklMoveItem = value;
                RaisePropertyChanged();
                if (myCurrentNomenklMoveItem != null && CurrentSklad.DOC_CODE == 0)
                    LoadDocuments();
                else
                    LoadDocuments(CurrentSklad.DocCode);
            }
        }

        public bool IsShowAll
        {
            get => myIsShowAll;
            set
            {
                if (myIsShowAll == value) return;
                myIsShowAll = value;
                NomenklMoveList.Clear();
                RefreshData(null);
                RaisePropertyChanged();
            }
        }

        public Core.EntityViewModel.Warehouse CurrentSklad
        {
            get => myCurrentSklad;
            set
            {
                if (myCurrentSklad != null && myCurrentSklad.Equals(value)) return;
                myCurrentSklad = value;
                RefreshData(null);
                RaisePropertyChanged();
            }
        }

        public ICommand NomenklCalcOpenCommand
        {
            get { return new Command(NomenklCalcOpen, _ => CurrentNomenklMoveItem != null); }
        }

        public DateTime StartDate
        {
            set
            {
                if (Equals(value, myStartDate)) return;
                myStartDate = value;
                if (myStartDate > EndDate)
                    EndDate = myStartDate;
                RaisePropertyChanged();
            }
            get
            {
                if (myStartDate == DateTime.MinValue)
                    myStartDate = DateTime.Today;
                return myStartDate;
            }
        }

        public DateTime EndDate
        {
            set
            {
                if (Equals(value, myEndDate)) return;
                myEndDate = value;
                if (myEndDate < StartDate)
                    StartDate = myEndDate;
                RaisePropertyChanged();
            }
            get
            {
                if (myEndDate == DateTime.MinValue)
                    myEndDate = DateTime.Today;
                return myEndDate;
            }
        }

        public new Command DocumentOpenCommand
        {
            get { return new Command(DocumentOpen, param => IsDocumentOpenAllow); }
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        private void LoadDocuments()
        {
            DocumentList.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var docs = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                            _.SD_24.DD_DATE >= StartDate && _.SD_24.DD_DATE <= EndDate &&
                            _.SD_24.DD_TYPE_DC == 2010000005)
                    .ToList();
                foreach (var doc in docs)
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        DocCode = doc.DOC_CODE,
                        DocumentName = doc.SD_24.SD_201.D_NAME,
                        DocumentNum = doc.SD_24.DD_IN_NUM + "/" + doc.SD_24.DD_EXT_NUM,
                        DocumentDate = doc.SD_24.DD_DATE,
                        QuantityIn = doc.DDT_KOL_PRIHOD,
                        QuantityOut = doc.DDT_KOL_RASHOD,
                        QuantityDelta = doc.DDT_KOL_PRIHOD - doc.DDT_KOL_RASHOD,
                        // ReSharper disable once PossibleInvalidOperationException
                        // ReSharper disable once AssignNullToNotNullAttribute
                        From = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_POL_DC.Value].Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_POL_DC.Value].Name,
                        SummaIn = (doc.DDT_KOL_PRIHOD > 0 ? doc.DDT_TAX_CRS_CENA ?? 0 : 0) * doc.DDT_KOL_PRIHOD,
                        SummaOut = (doc.DDT_KOL_RASHOD > 0 ? doc.DDT_TAX_CRS_CENA ?? 0 : 0) * doc.DDT_KOL_RASHOD,
                        SummaDelta = (doc.DDT_KOL_PRIHOD > 0 ? doc.DDT_TAX_CRS_CENA ?? 0 : 0) * doc.DDT_KOL_PRIHOD -
                                     (doc.DDT_KOL_RASHOD > 0 ? doc.DDT_TAX_CRS_CENA ?? 0 : 0) * doc.DDT_KOL_RASHOD
                    });
                docs = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Include(_ => _.TD_26)
                    .Include(_ => _.TD_26.SD_26)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                            _.SD_24.DD_DATE >= StartDate && _.SD_24.DD_DATE <= EndDate &&
                            _.SD_24.DD_TYPE_DC == 2010000001)
                    .ToList();
                foreach (var doc in docs)
                {
                    var newItem = new NomPriceDocumentViewModel
                    {
                        DocCode = doc.DOC_CODE,
                        DocumentName = doc.SD_24.SD_201.D_NAME,
                        DocumentNum = doc.SD_24.DD_IN_NUM + "/" + doc.SD_24.DD_EXT_NUM,
                        DocumentDate = doc.SD_24.DD_DATE,
                        QuantityIn = doc.DDT_KOL_PRIHOD,
                        QuantityOut = doc.DDT_KOL_RASHOD,
                        QuantityDelta = doc.DDT_KOL_PRIHOD - doc.DDT_KOL_RASHOD,
                        From = MainReferences.GetKontragent(doc.SD_24.DD_KONTR_OTPR_DC).Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_POL_DC.Value].Name
                    };
                    if (doc.TD_26 != null)
                    {
                        newItem.SummaIn = (decimal) (doc.TD_26.SFT_SUMMA_K_OPLATE * doc.DDT_KOL_PRIHOD) /
                                          doc.TD_26.SFT_KOL;
                        newItem.SummaOut = 0;
                        newItem.SummaDelta =
                            (decimal) (doc.TD_26.SFT_SUMMA_K_OPLATE / doc.TD_26.SFT_KOL * doc.DDT_KOL_PRIHOD);
                        newItem.Note = doc.SD_24.DD_NOTES + " / " + doc.TD_26.SD_26.SF_NOTES;
                    }
                    else
                    {
                        newItem.DocumentName = newItem.DocumentName + " (Возврат товара)";
                        newItem.SummaIn = Nomenkl.PriceWithOutNaklad(doc.DDT_NOMENKL_DC, doc.SD_24.DD_DATE) *
                                          doc.DDT_KOL_PRIHOD;
                        newItem.SummaOut = 0;
                        newItem.SummaDelta = Nomenkl.PriceWithOutNaklad(doc.DDT_NOMENKL_DC, doc.SD_24.DD_DATE) *
                                             doc.DDT_KOL_PRIHOD;
                        newItem.Note = doc.SD_24.DD_NOTES;
                    }

                    DocumentList.Add(newItem);
                }

                docs = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Include(_ => _.TD_84)
                    .Include(_ => _.TD_84.SD_84)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                            _.SD_24.DD_DATE >= StartDate && _.SD_24.DD_DATE <= EndDate &&
                            _.SD_24.DD_TYPE_DC == 2010000012)
                    .ToList();
                foreach (var doc in docs)
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        DocCode = doc.DOC_CODE,
                        DocumentName = doc.SD_24.SD_201.D_NAME,
                        DocumentNum = doc.SD_24.DD_IN_NUM + "/" + doc.SD_24.DD_EXT_NUM,
                        DocumentDate = doc.SD_24.DD_DATE,
                        QuantityIn = doc.DDT_KOL_PRIHOD,
                        QuantityOut = doc.DDT_KOL_RASHOD,
                        QuantityDelta = doc.DDT_KOL_PRIHOD - doc.DDT_KOL_RASHOD,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_OTPR_DC.Value].Name,
                        To = MainReferences.GetKontragent(doc.SD_24.DD_KONTR_POL_DC).Name,
                        SummaIn = 0,
                        SummaOut = doc.DDT_KOL_RASHOD * CurrentNomenklMoveItem.PriceStart,
                        SummaDelta = -doc.DDT_KOL_RASHOD * CurrentNomenklMoveItem.PriceEnd,
                        Note = doc.SD_24.DD_NOTES + " / " + doc.TD_84.SD_84.SF_NOTE
                    });
                var docs2 = ctx.NomenklTransferRow
                    .Include(_ => _.NomenklTransfer)
                    .Where(_ => _.NomenklOutDC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                                _.NomenklTransfer.Date >= StartDate && _.NomenklTransfer.Date <= EndDate).ToList();
                foreach (var doc in docs2)
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        Id = doc.DocId,
                        DocumentName = "Акт валютной конвертации товара",
                        DocumentNum = doc.NomenklTransfer.DucNum.ToString(),
                        DocumentDate = doc.NomenklTransfer.Date,
                        QuantityIn = 0,
                        QuantityOut = doc.Quantity,
                        QuantityDelta = -doc.Quantity,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[doc.NomenklTransfer.SkladDC.Value].Name,
                        To = MainReferences.Warehouses[doc.NomenklTransfer.SkladDC.Value].Name,
                        SummaIn = 0,
                        SummaOut = doc.PriceOut * doc.Quantity,
                        SummaDelta = -doc.PriceOut * doc.Quantity,
                        Note = doc.NomenklTransfer.Note + "/" + doc.Note
                    });
                docs2 = ctx.NomenklTransferRow
                    .Include(_ => _.NomenklTransfer)
                    .Where(_ => _.NomenklInDC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                                _.NomenklTransfer.Date >= StartDate && _.NomenklTransfer.Date <= EndDate).ToList();
                foreach (var doc in docs2)
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        Id = doc.DocId,
                        DocumentName = "Акт валютной конвертации товара",
                        DocumentNum = doc.NomenklTransfer.DucNum.ToString(),
                        DocumentDate = doc.NomenklTransfer.Date,
                        QuantityIn = doc.Quantity,
                        QuantityOut = 0,
                        QuantityDelta = doc.Quantity,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[doc.NomenklTransfer.SkladDC.Value].Name,
                        To = MainReferences.Warehouses[doc.NomenklTransfer.SkladDC.Value].Name,
                        SummaIn = doc.PriceIn * doc.Quantity,
                        SummaOut = 0,
                        SummaDelta = doc.PriceIn * doc.Quantity,
                        Note = doc.NomenklTransfer.Note + "/" + doc.Note
                    });
            }
        }

        private void LoadDocuments(decimal storeDC)
        {
            DocumentList.Clear();
            if (CurrentNomenklMoveItem == null || storeDC == 0) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var docs = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                            _.SD_24.DD_DATE >= StartDate && _.SD_24.DD_DATE <= EndDate &&
                            _.SD_24.DD_TYPE_DC == 2010000005
                            && (_.SD_24.DD_SKLAD_POL_DC == storeDC || _.SD_24.DD_SKLAD_OTPR_DC == storeDC))
                    .ToList();
                foreach (var doc in docs)
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        DocCode = doc.DOC_CODE,
                        DocumentName = doc.SD_24.SD_201.D_NAME,
                        DocumentNum = doc.SD_24.DD_IN_NUM + "/" + doc.SD_24.DD_EXT_NUM,
                        DocumentDate = doc.SD_24.DD_DATE,
                        QuantityIn = doc.DDT_KOL_PRIHOD,
                        QuantityOut = doc.DDT_KOL_RASHOD,
                        QuantityDelta = doc.DDT_KOL_PRIHOD - doc.DDT_KOL_RASHOD,
                        // ReSharper disable once PossibleInvalidOperationException
                        // ReSharper disable once AssignNullToNotNullAttribute
                        From = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_POL_DC.Value].Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_POL_DC.Value].Name,
                        SummaIn = (doc.DDT_KOL_PRIHOD > 0 ? doc.DDT_TAX_CRS_CENA ?? 0 : 0) * doc.DDT_KOL_PRIHOD,
                        SummaOut = (doc.DDT_KOL_RASHOD > 0 ? doc.DDT_TAX_CRS_CENA ?? 0 : 0) * doc.DDT_KOL_RASHOD,
                        SummaDelta = (doc.DDT_KOL_PRIHOD > 0 ? doc.DDT_TAX_CRS_CENA ?? 0 : 0) * doc.DDT_KOL_PRIHOD -
                                     (doc.DDT_KOL_RASHOD > 0 ? doc.DDT_TAX_CRS_CENA ?? 0 : 0) * doc.DDT_KOL_RASHOD
                    });
                docs = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Include(_ => _.TD_26)
                    .Include(_ => _.TD_26.SD_26)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                            _.SD_24.DD_DATE >= StartDate && _.SD_24.DD_DATE <= EndDate &&
                            _.SD_24.DD_TYPE_DC == 2010000001
                            && (_.SD_24.DD_SKLAD_POL_DC == storeDC || _.SD_24.DD_SKLAD_OTPR_DC == storeDC))
                    .ToList();
                foreach (var doc in docs)
                {
                    var newItem = new NomPriceDocumentViewModel
                    {
                        DocCode = doc.DOC_CODE,
                        DocumentName = doc.SD_24.SD_201.D_NAME,
                        DocumentNum = doc.SD_24.DD_IN_NUM + "/" + doc.SD_24.DD_EXT_NUM,
                        DocumentDate = doc.SD_24.DD_DATE,
                        QuantityIn = doc.DDT_KOL_PRIHOD,
                        QuantityOut = doc.DDT_KOL_RASHOD,
                        QuantityDelta = doc.DDT_KOL_PRIHOD - doc.DDT_KOL_RASHOD,
                        From = MainReferences.GetKontragent(doc.SD_24.DD_KONTR_OTPR_DC).Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_POL_DC.Value].Name
                    };
                    if (doc.TD_26 != null)
                    {
                        newItem.SummaIn = (decimal) (doc.TD_26.SFT_SUMMA_K_OPLATE * doc.DDT_KOL_PRIHOD) /
                                          doc.TD_26.SFT_KOL;
                        newItem.SummaOut = 0;
                        newItem.SummaDelta =
                            (decimal) (doc.TD_26.SFT_SUMMA_K_OPLATE / doc.TD_26.SFT_KOL * doc.DDT_KOL_PRIHOD);
                        newItem.Note = doc.SD_24.DD_NOTES + " / " + doc.TD_26.SD_26.SF_NOTES;
                    }
                    else
                    {
                        newItem.DocumentName = newItem.DocumentName + " (Возврат товара)";
                        newItem.SummaIn = Nomenkl.PriceWithOutNaklad(doc.DDT_NOMENKL_DC, doc.SD_24.DD_DATE) *
                                          doc.DDT_KOL_PRIHOD;
                        newItem.SummaOut = 0;
                        newItem.SummaDelta = Nomenkl.PriceWithOutNaklad(doc.DDT_NOMENKL_DC, doc.SD_24.DD_DATE) *
                                             doc.DDT_KOL_PRIHOD;
                        newItem.Note = doc.SD_24.DD_NOTES;
                    }

                    DocumentList.Add(newItem);
                }

                docs = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Include(_ => _.TD_84)
                    .Include(_ => _.TD_84.SD_84)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                            _.SD_24.DD_DATE >= StartDate && _.SD_24.DD_DATE <= EndDate &&
                            _.SD_24.DD_TYPE_DC == 2010000012
                            && (_.SD_24.DD_SKLAD_POL_DC == storeDC || _.SD_24.DD_SKLAD_OTPR_DC == storeDC))
                    .ToList();
                foreach (var doc in docs)
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        DocCode = doc.DOC_CODE,
                        DocumentName = doc.SD_24.SD_201.D_NAME,
                        DocumentNum = doc.SD_24.DD_IN_NUM + "/" + doc.SD_24.DD_EXT_NUM,
                        DocumentDate = doc.SD_24.DD_DATE,
                        QuantityIn = doc.DDT_KOL_PRIHOD,
                        QuantityOut = doc.DDT_KOL_RASHOD,
                        QuantityDelta = doc.DDT_KOL_PRIHOD - doc.DDT_KOL_RASHOD,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_OTPR_DC.Value].Name,
                        To = MainReferences.GetKontragent(doc.SD_24.DD_KONTR_POL_DC).Name,
                        SummaIn = 0,
                        SummaOut = doc.DDT_KOL_RASHOD * CurrentNomenklMoveItem.PriceStart,
                        SummaDelta = -doc.DDT_KOL_RASHOD * CurrentNomenklMoveItem.PriceEnd,
                        Note = doc.SD_24.DD_NOTES + " / " + doc.TD_84.SD_84.SF_NOTE
                    });
                var docs2 = ctx.NomenklTransferRow
                    .Include(_ => _.NomenklTransfer)
                    .Where(_ => _.NomenklOutDC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                                _.NomenklTransfer.Date >= StartDate && _.NomenklTransfer.Date <= EndDate).ToList();
                foreach (var doc in docs2)
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        Id = doc.DocId,
                        DocumentName = "Акт валютной конвертации товара",
                        DocumentNum = doc.NomenklTransfer.DucNum.ToString(),
                        DocumentDate = doc.NomenklTransfer.Date,
                        QuantityIn = 0,
                        QuantityOut = doc.Quantity,
                        QuantityDelta = -doc.Quantity,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[doc.NomenklTransfer.SkladDC.Value].Name,
                        To = MainReferences.Warehouses[doc.NomenklTransfer.SkladDC.Value].Name,
                        SummaIn = 0,
                        SummaOut = doc.PriceOut * doc.Quantity,
                        SummaDelta = -doc.PriceOut * doc.Quantity,
                        Note = doc.NomenklTransfer.Note + "/" + doc.Note
                    });
                docs2 = ctx.NomenklTransferRow
                    .Include(_ => _.NomenklTransfer)
                    .Where(_ => _.NomenklInDC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                                _.NomenklTransfer.Date >= StartDate && _.NomenklTransfer.Date <= EndDate).ToList();
                foreach (var doc in docs2)
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        Id = doc.DocId,
                        DocumentName = "Акт валютной конвертации товара",
                        DocumentNum = doc.NomenklTransfer.DucNum.ToString(),
                        DocumentDate = doc.NomenklTransfer.Date,
                        QuantityIn = doc.Quantity,
                        QuantityOut = 0,
                        QuantityDelta = doc.Quantity,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[doc.NomenklTransfer.SkladDC.Value].Name,
                        To = MainReferences.Warehouses[doc.NomenklTransfer.SkladDC.Value].Name,
                        SummaIn = doc.PriceIn * doc.Quantity,
                        SummaOut = 0,
                        SummaDelta = doc.PriceIn * doc.Quantity,
                        Note = doc.NomenklTransfer.Note + "/" + doc.Note
                    });
            }
        }

        private void NomenklCalcOpen(object obj)
        {
            if (CurrentNomenklMoveItem?.Nomenkl.DocCode == null) return;
            var ctx = new NomPriceWindowViewModel((decimal) CurrentNomenklMoveItem?.Nomenkl.DocCode);
            var dlg = new SelectDialogView {DataContext = ctx};
            dlg.ShowDialog();
        }

        private void LoadForCurrentSklad()
        {
            List<decimal> NomDCs;
            NomenklMoveList.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var dd = StartDate.AddDays(-1);
                var prcData = ctx.NOM_PRICE.ToList();
                var data = ctx.NomenklMoveForCalc.Where(_ => _.StoreDC == CurrentSklad.DocCode).ToList();
                NomDCs = data.Select(_ => _.NomDC).Distinct().ToList();
                foreach (var dc in NomDCs)
                {
                    var dsstart = prcData.Where(_ => _.NOM_DC == dc && _.DATE <= dd);
                    var dstart = dsstart.Any() ? dsstart.Max(_ => _.DATE) : new DateTime(2000, 1, 1);
                    var dsend = prcData.Where(_ => _.NOM_DC == dc && _.DATE <= EndDate);
                    var dend = dsend.Any() ? dsend.Max(_ => _.DATE) : new DateTime(2000, 1, 1);
                    var dstart1 = dstart;
                    var prcstart = prcData.FirstOrDefault(_ => _.NOM_DC == dc && _.DATE == dstart1);
                    var dend1 = dend;
                    var prcend = prcData.FirstOrDefault(_ => _.NOM_DC == dc && _.DATE == dend1);
                    var prcsum =
                        data.Where(_ => _.NomDC == dc && _.Date >= StartDate && _.Date <= EndDate)
                            .ToList();
                    var nomStart = data.Where(_ => _.NomDC == dc && _.Date < StartDate).Sum(s => s.Prihod - s.Rashod);
                    var nomEnd = data.Where(_ => _.NomDC == dc && _.Date <= EndDate).Sum(s => s.Prihod - s.Rashod);
                    var nomIn = data.Where(_ => _.NomDC == dc && _.Date >= StartDate && _.Date <= EndDate)
                        .Sum(s => s.Prihod);
                    var nomOut = data.Where(_ => _.NomDC == dc && _.Date >= StartDate && _.Date <= EndDate)
                        .Sum(s => s.Rashod);
                    var newitem = new NomenklMoveOnSkladViewModel
                    {
                        Nomenkl = MainReferences.GetNomenkl(dc),
                        PriceStart = prcstart?.PRICE_WO_NAKLAD ?? 0,
                        PriceEnd = prcend?.PRICE_WO_NAKLAD ?? 0,
                        QuantityEnd = nomEnd,
                        QuantityStart = nomStart,
                        QuantityIn = nomIn,
                        QuantityOut = nomOut
                    };
                    if (newitem.QuantityStart == 0 && newitem.QuantityEnd == 0 && newitem.QuantityIn == 0 &&
                        newitem.QuantityOut == 0) continue;
                    if (newitem.CurrencyName == "RUR" || newitem.CurrencyName == "RUB")
                    {
                        newitem.SummaRUBStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaRUBEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaRUBIn = prcsum.Where(_ => _.OperType == "InnerMove")
                                                 .Sum(_ => _.Prihod * newitem.PriceEnd)
                                             + prcsum.Where(_ => _.OperType == "Prihod")
                                                 .Sum(_ => _.Prihod * newitem.PriceEnd);
                        newitem.SummaRUBOut = prcsum.Sum(_ => _.Prihod * newitem.PriceEnd);
                    }

                    if (newitem.CurrencyName == "USD")
                    {
                        newitem.SummaUSDStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaUSDEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaUSDIn = prcsum.Where(_ => _.OperType == "InnerMove")
                                                 .Sum(_ => _.Prihod * newitem.PriceEnd)
                                             + prcsum.Where(_ => _.OperType == "Prihod")
                                                 .Sum(_ => _.Prihod * newitem.PriceEnd);
                        newitem.SummaUSDOut = prcsum.Sum(_ => _.Prihod * newitem.PriceEnd);
                    }

                    if (newitem.CurrencyName == "EUR")
                    {
                        newitem.SummaEURStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaEUREnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaEURIn = prcsum.Where(_ => _.OperType == "InnerMove")
                                                 .Sum(_ => _.Prihod * newitem.PriceEnd)
                                             + prcsum.Where(_ => _.OperType == "Prihod")
                                                 .Sum(_ => _.Prihod * newitem.PriceEnd);
                        newitem.SummaEUROut = prcsum.Sum(_ => _.Prihod * newitem.PriceEnd);
                    }

                    if (newitem.CurrencyName != "RUR" && newitem.CurrencyName != "RUB" &&
                        newitem.CurrencyName != "USD" &&
                        newitem.CurrencyName != "EUR")
                    {
                        newitem.SummaAllStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaAllEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaAllIn = prcsum.Where(_ => _.OperType == "InnerMove")
                                                 .Sum(_ => _.Prihod * newitem.PriceEnd)
                                             + prcsum.Where(_ => _.OperType == "Prihod")
                                                 .Sum(_ => _.Prihod * newitem.PriceEnd);
                        newitem.SummaAllOut = prcsum.Sum(_ => _.Prihod * newitem.PriceEnd);
                    }

                    NomenklMoveList.Add(newitem);
                }
            }
        }

        private void LoadForAllSklads()
        {
            NomenklMoveList.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var dd = StartDate.AddDays(-1);
                var prcData = ctx.NOM_PRICE.ToList();
                var NomDCs = prcData.Select(_ => _.NOM_DC).Distinct().ToList();
                foreach (var dc in NomDCs)
                {
                    var dsstart = prcData.Where(_ => _.NOM_DC == dc && _.DATE <= dd);
                    var dstart = dsstart.Any() ? dsstart.Max(_ => _.DATE) : new DateTime(2000, 1, 1);
                    var dsend = prcData.Where(_ => _.NOM_DC == dc && _.DATE <= EndDate);
                    var dend = dsend.Any() ? dsend.Max(_ => _.DATE) : new DateTime(2000, 1, 1);
                    var dstart1 = dstart;
                    var prcstart = prcData.FirstOrDefault(_ => _.NOM_DC == dc && _.DATE == dstart1);
                    var dend1 = dend;
                    var prcend = prcData.FirstOrDefault(_ => _.NOM_DC == dc && _.DATE == dend1);
                    var prcsum =
                        prcData.Where(_ => _.NOM_DC == dc && _.DATE >= StartDate && _.DATE <= EndDate)
                            .ToList();
                    var newitem = new NomenklMoveOnSkladViewModel
                    {
                        Nomenkl = MainReferences.GetNomenkl(dc),
                        PriceStart = prcstart?.PRICE_WO_NAKLAD ?? 0,
                        PriceEnd = prcend?.PRICE_WO_NAKLAD ?? 0,
                        QuantityEnd = prcend?.NAKOPIT ?? 0,
                        QuantityStart = prcstart?.NAKOPIT ?? 0,
                        QuantityIn = prcsum.Sum(_ => _.KOL_IN),
                        QuantityOut = prcsum.Sum(_ => _.KOL_OUT)
                    };
                    if (newitem.QuantityStart == 0 && newitem.QuantityEnd == 0 && newitem.QuantityIn == 0 &&
                        newitem.QuantityOut == 0) continue;
                    if (newitem.CurrencyName == "RUR" || newitem.CurrencyName == "RUB")
                    {
                        newitem.SummaRUBStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaRUBEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaRUBIn = prcsum.Sum(_ => _.SUM_IN_WO_NAKLAD);
                        newitem.SummaRUBOut = prcsum.Sum(_ => _.SUM_OUT_WO_NAKLAD);
                    }

                    if (newitem.CurrencyName == "USD")
                    {
                        newitem.SummaUSDStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaUSDEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaUSDIn = prcsum.Sum(_ => _.SUM_IN_WO_NAKLAD);
                        newitem.SummaUSDOut = prcsum.Sum(_ => _.SUM_OUT_WO_NAKLAD);
                    }

                    if (newitem.CurrencyName == "EUR")
                    {
                        newitem.SummaEURStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaEUREnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaEURIn = prcsum.Sum(_ => _.SUM_IN_WO_NAKLAD);
                        newitem.SummaEUROut = prcsum.Sum(_ => _.SUM_OUT_WO_NAKLAD);
                    }

                    if (newitem.CurrencyName != "RUR" && newitem.CurrencyName != "RUB" &&
                        newitem.CurrencyName != "USD" &&
                        newitem.CurrencyName != "EUR")
                    {
                        newitem.SummaAllStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaAllEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaAllIn = prcsum.Sum(_ => _.SUM_IN_WO_NAKLAD);
                        newitem.SummaAllOut = prcsum.Sum(_ => _.SUM_OUT_WO_NAKLAD);
                    }

                    if (IsShowAll)
                        NomenklMoveList.Add(newitem);
                    else if (newitem.QuantityStart != 0 || newitem.QuantityEnd != 0)
                        NomenklMoveList.Add(newitem);
                }
            }
        }

        private void LoadReferences()
        {
            Sklads.Add(new Core.EntityViewModel.Warehouse
            {
                DOC_CODE = 0,
                Name = "Все склады"
            });
            foreach (var s in MainReferences.Warehouses.Values.OrderBy(_ => _.Name).ToList())
                Sklads.Add(s);
            RaisePropertiesChanged(nameof(Sklads));
        }

        public override void RefreshData(object obj)
        {
            DocumentList.Clear();
            NomenklMoveList.Clear();
            if (CurrentSklad == null) return;
            if (CurrentSklad.DocCode == 0)
                LoadForAllSklads();
            else
                LoadForCurrentSklad();
        }

        public override void DocumentOpen(object obj)
        {
            switch (CurrentDocument.DocumentName)
            {
                case "Приходный складской ордер":
                    DocumentsOpenManager.Open(DocumentType.StoreOrderIn, CurrentDocument.DocCode);
                    break;
                case "Расходный складской ордер":
                    DocumentsOpenManager.Open(DocumentType.StoreOrderIn, CurrentDocument.DocCode);
                    break;
                case "Акт валютной конвертации товара":
                    DocumentsOpenManager.Open(DocumentType.NomenklTransfer, CurrentDocument.DocCode);
                    break;
            }
        }
    }
}