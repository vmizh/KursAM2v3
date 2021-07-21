using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.NomenklManagement;
using Core.Menu;
using Core.ViewModel.Base;
using Helper;
using KursAM2.Managers;
using KursAM2.Managers.Nomenkl;
using KursAM2.View.Base;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklMoveOnSkladWindowViewModel : RSWindowViewModelBase
    {
        private NomPriceDocumentViewModel myCurrentDocument;
        private NomenklMoveOnSkladViewModel myCurrentNomenklMoveItem;
        private Core.EntityViewModel.NomenklManagement.Warehouse myCurrentSklad;
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

        public ObservableCollection<NomenklMoveOnSkladViewModel> NomenklMoveListTemp { set; get; } =
            new ObservableCollection<NomenklMoveOnSkladViewModel>();

        public ObservableCollection<NomPriceDocumentViewModel> DocumentList { get; set; } =
            new ObservableCollection<NomPriceDocumentViewModel>();

        public List<Core.EntityViewModel.NomenklManagement.Warehouse> Sklads { set; get; } =
            new List<Core.EntityViewModel.NomenklManagement.Warehouse>();

        public NomenklMoveOnSkladViewModel CurrentNomenklMoveItem
        {
            get => myCurrentNomenklMoveItem;
            set
            {
                while (!MainReferences.IsReferenceLoadComplete)
                {
                }

                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentNomenklMoveItem == value) return;
                myCurrentNomenklMoveItem = value;
                RaisePropertyChanged();
                if (myCurrentNomenklMoveItem != null && CurrentSklad == null)
                    LoadDocuments();
                else if (CurrentSklad != null)
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
                if (myIsShowAll)
                {
                    CurrentSklad = null;
                    RaisePropertyChanged(nameof(CurrentSklad));
                }

                NomenklMoveList.Clear();
                //RefreshData(null);
                RaisePropertyChanged();
            }
        }

        public Core.EntityViewModel.NomenklManagement.Warehouse CurrentSklad
        {
            get => myCurrentSklad;
            set
            {
                if (myCurrentSklad != null && myCurrentSklad.Equals(value)) return;
                myCurrentSklad = value;
                //RefreshData(null);
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
                        From = doc.SD_24.DD_KONTR_OTPR_DC != null
                            ? MainReferences.GetKontragent(doc.SD_24.DD_KONTR_OTPR_DC).Name
                            // ReSharper disable once PossibleInvalidOperationException
                            : MainReferences.Warehouses[doc.SD_24.DD_SKLAD_OTPR_DC.Value].Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_POL_DC.Value].Name
                    };
                    if (doc.TD_26 != null)
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        newItem.SummaIn = (decimal) (doc.TD_26.SFT_SUMMA_K_OPLATE * doc.DDT_KOL_PRIHOD) /
                                          doc.TD_26.SFT_KOL;
                        newItem.SummaOut = 0;
                        newItem.SummaDelta =
// ReSharper disable once PossibleInvalidOperationException
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
                var docs3 = from cc in ctx.TD_26_CurrencyConvert
                    // ReSharper disable once AccessToDisposedClosure
                    from td26 in ctx.TD_26
                    // ReSharper disable once AccessToDisposedClosure
                    from sd26 in ctx.SD_26
                    where cc.DOC_CODE == td26.DOC_CODE && cc.CODE == td26.CODE
                                                       && sd26.DOC_CODE == td26.DOC_CODE &&
                                                       cc.NomenklId == CurrentNomenklMoveItem.Nomenkl.Id
                                                       && cc.Date >= StartDate && cc.Date <= EndDate
                    select cc;
                foreach (var doc in docs3.ToList())
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        Id = doc.TD_26.SD_26.Id,
                        DocCode = doc.TD_26.DOC_CODE,
                        DocumentName = "Акт валютной конвертации (по счету)",
                        DocumentNum = doc.TD_26.SD_26.SF_IN_NUM + "/" + doc.TD_26.SD_26.SF_POSTAV_NUM,
                        DocumentDate = doc.Date,
                        QuantityIn = doc.Quantity,
                        QuantityOut = 0,
                        QuantityDelta = doc.Quantity,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[doc.StoreDC].Name,
                        To = MainReferences.Warehouses[doc.StoreDC].Name,
                        SummaIn = doc.Price * doc.Quantity,
                        SummaOut = 0,
                        SummaDelta = doc.Price * doc.Quantity,
                        Note = doc.TD_26.SD_26.SF_NOTES + "/" + doc.Note
                    });

                var docs4 = from cc in ctx.TD_26_CurrencyConvert
                    // ReSharper disable once AccessToDisposedClosure
                    from td26 in ctx.TD_26
                    // ReSharper disable once AccessToDisposedClosure
                    from sd26 in ctx.SD_26
                    where cc.DOC_CODE == td26.DOC_CODE && cc.CODE == td26.CODE
                                                       && sd26.DOC_CODE == td26.DOC_CODE &&
                                                       td26.SFT_NEMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DOC_CODE
                                                       && cc.Date >= StartDate && cc.Date <= EndDate
                    select cc;

                foreach (var doc in docs4.ToList())
                {
                    decimal prc = 0;
                    var last = ctx.NOM_PRICE.Where(_ => _.NOM_DC == CurrentNomenklMoveItem.Nomenkl.DOC_CODE &&
                                                        _.DATE <= doc.Date).OrderBy(_ => _.DATE).ToList();
                    if (last.Any())
                        prc = last.Last().PRICE_WO_NAKLAD;
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        Id = doc.TD_26.SD_26.Id,
                        DocCode = doc.TD_26.DOC_CODE,
                        DocumentName = "Акт валютной конвертации (по счету)",
                        DocumentNum = doc.TD_26.SD_26.SF_IN_NUM + "/" + doc.TD_26.SD_26.SF_POSTAV_NUM,
                        DocumentDate = doc.Date,
                        QuantityIn = 0,
                        QuantityOut = doc.Quantity,
                        QuantityDelta = -doc.Quantity,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[doc.StoreDC].Name,
                        To = MainReferences.Warehouses[doc.StoreDC].Name,
                        SummaIn = 0,
                        SummaOut = prc,
                        SummaDelta = -prc,
                        Note = doc.TD_26.SD_26.SF_NOTES + "/" + doc.Note
                    });
                }

                var docs5 = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                            _.SD_24.DD_DATE >= StartDate && _.SD_24.DD_DATE <= EndDate &&
                            _.SD_24.DD_TYPE_DC == 2010000014
                    )
                    .ToList();
                foreach (var doc in docs5)
                {
                    var prc = NomenklManager.NomenklPrice(doc.DDT_NOMENKL_DC, doc.SD_24.DD_DATE, ctx).Price;
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
                        SummaIn = doc.DDT_KOL_PRIHOD * prc,
                        SummaOut = doc.DDT_KOL_RASHOD * prc,
                        SummaDelta = (doc.DDT_KOL_PRIHOD - doc.DDT_KOL_RASHOD) * prc
                    });
                }
                var docs6 = ctx.AktSpisaniya_row
                    .Include(_ => _.AktSpisaniyaNomenkl_Title)
                    .Where(_ => _.Nomenkl_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                                _.AktSpisaniyaNomenkl_Title.Date_Doc >= StartDate
                                && _.AktSpisaniyaNomenkl_Title.Date_Doc <= EndDate).ToList();
                foreach (var doc in docs6)
                {
                    var prc = NomenklManager.NomenklPrice(doc.Nomenkl_DC, doc.AktSpisaniyaNomenkl_Title.Date_Doc, ctx)
                        .Price;
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        DocCode = 0,
                        DocumentName = "Акт списания номенклатур",
                        DocumentNum = doc.AktSpisaniyaNomenkl_Title.Num_Doc.ToString(),
                        DocumentDate = doc.AktSpisaniyaNomenkl_Title.Date_Doc,
                        QuantityIn = 0,
                        QuantityOut = doc.Quantity,
                        QuantityDelta = -doc.Quantity,
                        // ReSharper disable once PossibleInvalidOperationException
                        // ReSharper disable once AssignNullToNotNullAttribute
                        From = MainReferences.Warehouses[doc.AktSpisaniyaNomenkl_Title.Warehouse_DC].Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = "Списание",
                        SummaIn = 0,
                        SummaOut = doc.Quantity * prc,
                        SummaDelta = -doc.Quantity * prc
                    });
                }
            }

            CalcNakopit();
        }

        private void LoadDocuments(decimal storeDC)
        {
            DocumentList.Clear();
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

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
                        From = doc.SD_24.DD_KONTR_OTPR_DC != null
                            ? MainReferences.GetKontragent(doc.SD_24.DD_KONTR_OTPR_DC).Name
                            // ReSharper disable once PossibleInvalidOperationException
                            : MainReferences.Warehouses[doc.SD_24.DD_SKLAD_OTPR_DC.Value].Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_POL_DC.Value].Name
                    };
                    if (doc.TD_26 != null)
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        newItem.SummaIn = (decimal) (doc.TD_26.SFT_SUMMA_K_OPLATE * doc.DDT_KOL_PRIHOD) /
                                          doc.TD_26.SFT_KOL;
                        newItem.SummaOut = 0;
                        newItem.SummaDelta =
// ReSharper disable once PossibleInvalidOperationException
                            (decimal) (doc.TD_26.SFT_SUMMA_K_OPLATE / doc.TD_26.SFT_KOL * doc.DDT_KOL_PRIHOD);
                        newItem.Note = doc.SD_24.DD_NOTES + " / " + doc.TD_26.SD_26.SF_NOTES;
                    }
                    else
                    {
                        if (doc.SD_24.DD_SKLAD_OTPR_DC == null)
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
                        Note = doc.SD_24.DD_NOTES + " / " + doc.TD_84?.SD_84.SF_NOTE
                    });
                var docs2 = ctx.NomenklTransferRow
                    .Include(_ => _.NomenklTransfer)
                    .Where(_ => _.NomenklOutDC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                                _.NomenklTransfer.Date >= StartDate && _.NomenklTransfer.Date <= EndDate
                                && _.NomenklTransfer.SkladDC == storeDC).ToList();
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
                                _.NomenklTransfer.Date >= StartDate && _.NomenklTransfer.Date <= EndDate
                                && _.NomenklTransfer.SkladDC == storeDC).ToList();
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
                var docs3 = from cc in ctx.TD_26_CurrencyConvert
                    // ReSharper disable once AccessToDisposedClosure
                    from td26 in ctx.TD_26
                    // ReSharper disable once AccessToDisposedClosure
                    from sd26 in ctx.SD_26
                    where cc.DOC_CODE == td26.DOC_CODE && cc.CODE == td26.CODE
                                                       && sd26.DOC_CODE == td26.DOC_CODE &&
                                                       cc.NomenklId == CurrentNomenklMoveItem.Nomenkl.Id
                                                       && cc.Date >= StartDate && cc.Date <= EndDate
                                                       && cc.StoreDC == storeDC
                    select cc;
                foreach (var doc in docs3.ToList())
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        Id = doc.TD_26.SD_26.Id,
                        DocCode = doc.TD_26.DOC_CODE,
                        DocumentName = "Акт валютной конвертации (по счету)",
                        DocumentNum = doc.TD_26.SD_26.SF_IN_NUM + "/" + doc.TD_26.SD_26.SF_POSTAV_NUM,
                        DocumentDate = doc.Date,
                        QuantityIn = doc.Quantity,
                        QuantityOut = 0,
                        QuantityDelta = doc.Quantity,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[doc.StoreDC].Name,
                        To = MainReferences.Warehouses[doc.StoreDC].Name,
                        SummaIn = doc.Price * doc.Quantity,
                        SummaOut = 0,
                        SummaDelta = doc.Price * doc.Quantity,
                        Note = doc.TD_26.SD_26.SF_NOTES + "/" + doc.Note
                    });

                var docs4 = from cc in ctx.TD_26_CurrencyConvert
                    // ReSharper disable once AccessToDisposedClosure
                    from td26 in ctx.TD_26
                    // ReSharper disable once AccessToDisposedClosure
                    from sd26 in ctx.SD_26
                    where cc.DOC_CODE == td26.DOC_CODE && cc.CODE == td26.CODE
                                                       && sd26.DOC_CODE == td26.DOC_CODE &&
                                                       td26.SFT_NEMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DOC_CODE
                                                       && cc.Date >= StartDate && cc.Date <= EndDate
                                                       && cc.StoreDC == storeDC
                    select cc;

                foreach (var doc in docs4.ToList())
                {
                    decimal prc = 0;
                    var last = ctx.NOM_PRICE.Where(_ => _.NOM_DC == CurrentNomenklMoveItem.Nomenkl.DOC_CODE &&
                                                        _.DATE <= doc.Date).OrderBy(_ => _.DATE).ToList();
                    if (last.Any())
                        prc = last.Last().PRICE_WO_NAKLAD;
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        Id = doc.TD_26.SD_26.Id,
                        DocCode = doc.TD_26.DOC_CODE,
                        DocumentName = "Акт валютной конвертации (по счету)",
                        DocumentNum = doc.TD_26.SD_26.SF_IN_NUM + "/" + doc.TD_26.SD_26.SF_POSTAV_NUM,
                        DocumentDate = doc.Date,
                        QuantityIn = 0,
                        QuantityOut = doc.Quantity,
                        QuantityDelta = -doc.Quantity,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[doc.StoreDC].Name,
                        To = MainReferences.Warehouses[doc.StoreDC].Name,
                        SummaIn = 0,
                        SummaOut = prc,
                        SummaDelta = -prc,
                        Note = doc.TD_26.SD_26.SF_NOTES + "/" + doc.Note
                    });
                }

                var docs5 = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                            _.SD_24.DD_DATE >= StartDate && _.SD_24.DD_DATE <= EndDate &&
                            _.SD_24.DD_TYPE_DC == 2010000014
                            && (_.SD_24.DD_SKLAD_POL_DC == storeDC || _.SD_24.DD_SKLAD_OTPR_DC == storeDC))
                    .ToList();
                foreach (var doc in docs5)
                {
                    var prc = NomenklManager.NomenklPrice(doc.DDT_NOMENKL_DC, doc.SD_24.DD_DATE, ctx).Price;
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        DocCode = doc.DOC_CODE,
                        DocumentName = doc.SD_24.SD_201.D_NAME,
                        DocumentNum = doc.SD_24.DD_IN_NUM + "/" + doc.SD_24.DD_EXT_NUM,
                        DocumentDate = doc.SD_24.DD_DATE,
                        QuantityIn = doc.SD_24.DD_SKLAD_POL_DC == storeDC ? doc.DDT_KOL_PRIHOD : 0,
                        QuantityOut = doc.SD_24.DD_SKLAD_OTPR_DC == storeDC ? doc.DDT_KOL_RASHOD : 0,
                        QuantityDelta = doc.SD_24.DD_SKLAD_POL_DC == storeDC ? doc.DDT_KOL_PRIHOD : -doc.DDT_KOL_RASHOD,
                        // ReSharper disable once PossibleInvalidOperationException
                        // ReSharper disable once AssignNullToNotNullAttribute
                        From = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_OTPR_DC.Value].Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_POL_DC.Value].Name,
                        SummaIn = doc.SD_24.DD_SKLAD_POL_DC == storeDC ? doc.DDT_KOL_PRIHOD * prc : 0,
                        SummaOut = doc.SD_24.DD_SKLAD_OTPR_DC == storeDC ? doc.DDT_KOL_RASHOD * prc : 0,
                        SummaDelta = doc.SD_24.DD_SKLAD_POL_DC == storeDC
                            ? doc.DDT_KOL_PRIHOD * prc
                            : -doc.DDT_KOL_RASHOD * prc
                    });
                }

                var docs6 = ctx.AktSpisaniya_row
                    .Include(_ => _.AktSpisaniyaNomenkl_Title)
                    .Where(_ => _.Nomenkl_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                                _.AktSpisaniyaNomenkl_Title.Warehouse_DC == storeDC &&
                                _.AktSpisaniyaNomenkl_Title.Date_Doc >= StartDate
                                && _.AktSpisaniyaNomenkl_Title.Date_Doc <= EndDate).ToList();
                foreach (var doc in docs6)
                {
                    var prc = NomenklManager.NomenklPrice(doc.Nomenkl_DC, doc.AktSpisaniyaNomenkl_Title.Date_Doc, ctx)
                        .Price;
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        DocCode = 0,
                        DocumentName = "Акт списания номенклатур",
                        DocumentNum = doc.AktSpisaniyaNomenkl_Title.Num_Doc.ToString(),
                        DocumentDate = doc.AktSpisaniyaNomenkl_Title.Date_Doc,
                        QuantityIn = 0,
                        QuantityOut = doc.Quantity,
                        QuantityDelta = -doc.Quantity,
                        // ReSharper disable once PossibleInvalidOperationException
                        // ReSharper disable once AssignNullToNotNullAttribute
                        From = MainReferences.Warehouses[doc.AktSpisaniyaNomenkl_Title.Warehouse_DC].Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = "Списание",
                        SummaIn = 0,
                        SummaOut = doc.Quantity * prc,
                        SummaDelta = -doc.Quantity * prc
                    });
                }
            }

            CalcNakopit();
        }

        private void CalcNakopit()
        {
            var start = CurrentNomenklMoveItem.QuantityStart;
            var tempList = new List<NomPriceDocumentViewModel>(DocumentList)
                .OrderBy(_ => _.DocumentDate).ToList();

            foreach (var item in tempList)
            {
                item.Nakopit = start + item.QuantityIn - item.QuantityOut;
                start = item.Nakopit;
            }
        }

        private void NomenklCalcOpen(object obj)
        {
            if (CurrentNomenklMoveItem?.Nomenkl.DocCode == null) return;
            var ctx = new NomPriceWindowViewModel((decimal) CurrentNomenklMoveItem?.Nomenkl.DocCode);
            var dlg = new SelectDialogView {DataContext = ctx};
            dlg.ShowDialog();
        }

        private void LoadForCurrentSklad3()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql = "DECLARE @StartPrice NUMERIC(18, 4) " +
                          ",@StartPriceWithNaklad NUMERIC(18, 4) " +
                          ",@StartKol NUMERIC(18,4) " +
                          ",@nomDC NUMERIC(18, 0) " +
                          ",@startDate DATETIME " +
                          $",@DateStart DATETIME = '{CustomFormat.DateToString(StartDate)}' " +
                          $",@DateEnd DATETIME = '{CustomFormat.DateToString(EndDate)}' " +
                          $",@StoreDC NUMERIC(18, 0) = {CustomFormat.DecimalToSqlDecimal(CurrentSklad.DocCode)}; " +
                          " " +
                          "DROP TABLE IF EXISTS #startprices; " +
                          "DROP TABLE IF EXISTS #tab; " +
                          " " +
                          "CREATE TABLE #startprices ( " +
                          "  NomDC NUMERIC(18, 0) " +
                          " ,Price NUMERIC(18, 4) " +
                          " ,PriceWithnaklad NUMERIC(18, 4) " +
                          " ,Ostatok NUMERIC(18,4) " +
                          "); " +
                          "CREATE NONCLUSTERED INDEX ix_tempstartprices ON #startprices (NomDC); " +
                          " " +
                          "CREATE TABLE #tab ( " +
                          "NomDC NUMERIC(18, 0) " +
                          ",Date DATETIME " +
                          ",StartQuantity NUMERIC(18,4) " +
                          ",PriceStart NUMERIC(18, 4) " +
                          ",PiceStartWithPrice NUMERIC(18, 4) " +
                          ",Prihod NUMERIC(18, 4) " +
                          ",MoneyPrihod NUMERIC(18, 4) " +
                          ",MoneyPrihodWithNaklad NUMERIC(18, 4) " +
                          ",Rashod NUMERIC(18, 4) " +
                          ",MoneyRashod NUMERIC(18, 4) " +
                          ",MoneyRashodWithNaklad NUMERIC(18, 4) " +
                          ",OrderBy INT " +
                          ",SumPrihod NUMERIC(18, 4) " +
                          ",SumRashod NUMERIC(18, 4) " +
                          "); " +
                          "CREATE NONCLUSTERED INDEX ix_temptab ON #tab (NomDC,Date); " +
                          "INSERT INTO #tab " +
                          "SELECT " +
                          "NomDC " +
                          ",Date " +
                          ",0 " +
                          ",0 " +
                          ",0 " +
                          ",Prihod " +
                          ",CASE WHEN Prihod > 0 THEN isnull(SUM(Prihod * Price) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0)" +
                          "ELSE 0 END AS MoneyPrihod " +
                          ",CASE WHEN Prihod > 0 THEN isnull(SUM(Prihod * PriceWithNaklad) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0) " +
                          "ELSE 0 END AS MoneyPrihodWithNaklad " +
                          ",Rashod " +
                          ",isnull(SUM(Rashod * Price) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0) AS MoneyRashod " +
                          ",isnull(SUM(Rashod * PriceWithNaklad) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0) AS MoneyRashodWithNaklad " +
                          ",CASE WHEN Prihod > 0 THEN 0 ELSE 1 END OrderBy " +
                          ",SUM(Prihod) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS SumPrihod " +
                          ",SUM(Rashod) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS SumRashod " +
                          "FROM NomenklMoveForCalc " +
                          "WHERE Date <= @DateEnd --AND OperType != 'Накладная на внутренее перемещение' \n " +
                          "AND StoreDC = @StoreDC " +
                          "ORDER BY NomDC, OrderBy; " +
                          "INSERT INTO #startprices " +
                          "SELECT " +
                          "np.NOM_DC " +
                          ",np.PRICE_WO_NAKLAD " +
                          ",np.PRICE " +
                          ",ISNULL((SELECT SUM(Prihod-Rashod) FROM #tab tt WHERE tt.NomDC = np.NOM_DC AND tt.Date < @DateStart),0) " +
                          "FROM NOM_PRICE np " +
                          "WHERE np.NOM_DC IN (SELECT DISTINCT " +
                          "t.NomDC " +
                          "FROM #tab t) " +
                          "AND np.Date = (SELECT " +
                          "MAX(np1.DATE) " +
                          "FROM NOM_PRICE np1 " +
                          "WHERE np1.NOM_DC = np.NOM_DC " +
                          "AND np1.Date < @DateStart); " +
                          " " +
                          "SELECT * FROM ( " +
                          "SELECT " +
                          "t.NomDC " +
                          ",NOM_NOMENKL AS NomNumber " +
                          ",NOM_NAME AS NomName " +
                          ",t.Date " +
                          ",Prihod " +
                          ",t.MoneyPrihod MoneyPrihod " +
                          ",MoneyPrihodWithNaklad MoneyPrihodWithNaklad " +
                          ",Rashod " +
                          ",t.Rashod * np.PRICE_WO_NAKLAD MoneyRashod " +
                          ",t.Rashod * np.PRICE MoneyRashodWithNaklad " +
                          ",SUM(Prihod - Rashod) OVER (PARTITION BY t.NomDC ORDER BY t.Date, OrderBy ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Ostatok " +
                          ",np.PRICE_WO_NAKLAD AS Price " +
                          ",np.PRICE AS PriceWithNaklad " +
                          ",ISNULL(s.Price, 0) StartPrice " +
                          ",ISNULL(s.PriceWithnaklad, 0) StartPriceWithNaklad " +
                          ",ISNULL(s.Ostatok,0) Start " +
                          "FROM #tab t " +
                          "INNER JOIN sd_83 " +
                          "  ON NomDC = SD_83.DOC_CODE " +
                          "INNER JOIN NOM_PRICE np " +
                          "  ON SD_83.DOC_CODE = np.NOM_DC " +
                          "    AND np.DATE = (SELECT " +
                          " MAX(np1.DATE) " +
                          "      FROM NOM_PRICE np1 " +
                          "      WHERE np1.NOM_DC = SD_83.DOC_CODE " +
                          "      AND np1.Date <= t.Date) " +
                          "LEFT OUTER JOIN #startprices s " +
                          "  ON t.NomDC = s.NomDC) tab " +
                          "WHERE tab.Date >= @DateStart " +
                          "ORDER BY tab.NomDC, tab.Date " +
                          " " +
                          "DROP TABLE #startprices; " +
                          "DROP TABLE #tab; ";
                var data = ctx.Database.SqlQuery<NomenklCalcMove>(sql).ToList();
                var nnomlist = (from item in data
                    group item by item.NomDC
                    into g
                    select new
                    {
                        NomDC = g.Key,
                        SummaIn = g.Sum(item => item.MoneyPrihod),
                        SummaOut = g.Sum(item => item.MoneyRashod),
                        Prihod = g.Sum(item => item.Prihod),
                        Rashod = g.Sum(item => item.Rashod),
                        LastOp = g.Last()
                    }).ToList();
                var listTemp = new List<NomenklMoveOnSkladViewModel>();
                foreach (var item in nnomlist)
                {
                    var summaIn = item.SummaIn;
                    var summaOut = item.SummaOut;
                    var newitem = new NomenklMoveOnSkladViewModel
                    {
                        Nomenkl = MainReferences.GetNomenkl(item.NomDC),
                        PriceStart = item.LastOp.StartPrice,
                        PriceEnd = item.LastOp.Price,
                        QuantityEnd = item.LastOp.Ostatok,
                        QuantityStart = item.LastOp.Start,
                        QuantityIn = item.Prihod,
                        QuantityOut = item.Rashod
                    };
                    listTemp.Add(newitem);
                    switch (newitem.CurrencyName)
                    {
                        case CurrencyCode.RUBName:
                        case CurrencyCode.RURName:
                            newitem.SummaRUBStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaRUBEnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaRUBIn = summaIn;
                            newitem.SummaRUBOut = summaOut;
                            continue;
                        case CurrencyCode.USDName:
                            newitem.SummaUSDStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaUSDEnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaUSDIn = summaIn;
                            newitem.SummaUSDOut = summaOut;
                            continue;
                        case CurrencyCode.EURName:
                            newitem.SummaEURStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaEUREnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaEURIn = summaIn;
                            newitem.SummaEUROut = summaOut;
                            continue;
                        default:
                            newitem.SummaAllStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaAllEnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaAllIn = summaIn;
                            newitem.SummaAllOut = summaOut;
                            continue;
                    }
                }

                var delList = new List<NomenklMoveOnSkladViewModel>(listTemp.Where(nl => nl.QuantityStart == 0
                    && nl.QuantityIn == 0 && nl.QuantityOut == 0 && nl.QuantityEnd == 0));
                foreach (var nl in delList) listTemp.Remove(nl);

                NomenklMoveList = new ObservableCollection<NomenklMoveOnSkladViewModel>(listTemp);
                RaisePropertyChanged(nameof(NomenklMoveList));
            }
        }

        private void LoadForAllSklads3()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql = "DECLARE @StartPrice NUMERIC(18, 4) " +
                          ",@StartPriceWithNaklad NUMERIC(18, 4) " +
                          ",@StartKol NUMERIC(18,4) " +
                          ",@nomDC NUMERIC(18, 0) " +
                          ",@startDate DATETIME " +
                          $",@DateStart DATETIME = '{CustomFormat.DateToString(StartDate)}' " +
                          $",@DateEnd DATETIME = '{CustomFormat.DateToString(EndDate)}'; " +
                          " " +
                          "DROP TABLE IF EXISTS #startprices; " +
                          "DROP TABLE IF EXISTS #tab; " +
                          " " +
                          "CREATE TABLE #startprices ( " +
                          "  NomDC NUMERIC(18, 0) " +
                          " ,Price NUMERIC(18, 4) " +
                          " ,PriceWithnaklad NUMERIC(18, 4) " +
                          " ,Ostatok NUMERIC(18,4) " +
                          "); " +
                          "CREATE NONCLUSTERED INDEX ix_tempstartprices ON #startprices (NomDC); " +
                          " " +
                          "CREATE TABLE #tab ( " +
                          "NomDC NUMERIC(18, 0) " +
                          ",Date DATETIME " +
                          ",StartQuantity NUMERIC(18,4) " +
                          ",PriceStart NUMERIC(18, 4) " +
                          ",PiceStartWithPrice NUMERIC(18, 4) " +
                          ",Prihod NUMERIC(18, 4) " +
                          ",MoneyPrihod NUMERIC(18, 4) " +
                          ",MoneyPrihodWithNaklad NUMERIC(18, 4) " +
                          ",Rashod NUMERIC(18, 4) " +
                          ",MoneyRashod NUMERIC(18, 4) " +
                          ",MoneyRashodWithNaklad NUMERIC(18, 4) " +
                          ",OrderBy INT " +
                          ",SumPrihod NUMERIC(18, 4) " +
                          ",SumRashod NUMERIC(18, 4) " +
                          "); " +
                          "CREATE NONCLUSTERED INDEX ix_temptab ON #tab (NomDC,Date); " +
                          "INSERT INTO #tab " +
                          "SELECT " +
                          "NomDC " +
                          ",Date " +
                          ",0 " +
                          ",0 " +
                          ",0 " +
                          ",Prihod " +
                          ",isnull(SUM(Prihod * Price) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0) AS MoneyPrihod " +
                          ",isnull(SUM(Prihod * PriceWithNaklad) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0) AS MoneyPrihodWithNaklad " +
                          ",Rashod " +
                          ",isnull(SUM(Rashod * Price) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0) AS MoneyRashod " +
                          ",isnull(SUM(Rashod * PriceWithNaklad) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),0) AS MoneyRashodWithNaklad " +
                          ",CASE " +
                          "WHEN Prihod > 0 THEN 0 ELSE 1 END OrderBy " +
                          ",SUM(Prihod) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS SumPrihod " +
                          ",SUM(Rashod) OVER (PARTITION BY NomDC ORDER BY Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS SumRashod " +
                          "FROM NomenklMoveForCalc " +
                          "WHERE Date <= @DateEnd --AND OperType != 'Накладная на внутренее перемещение' \n" +
                          "ORDER BY NomDC, OrderBy; " +
                          "INSERT INTO #startprices " +
                          "SELECT " +
                          "np.NOM_DC " +
                          ",np.PRICE_WO_NAKLAD " +
                          ",np.PRICE " +
                          ",ISNULL((SELECT SUM(Prihod-Rashod) FROM #tab tt WHERE tt.NomDC = np.NOM_DC AND tt.Date < @DateStart),0) " +
                          "FROM NOM_PRICE np " +
                          "WHERE np.NOM_DC IN (SELECT DISTINCT " +
                          "t.NomDC " +
                          "FROM #tab t) " +
                          "AND np.Date = (SELECT " +
                          "MAX(np1.DATE) " +
                          "FROM NOM_PRICE np1 " +
                          "WHERE np1.NOM_DC = np.NOM_DC " +
                          "AND np1.Date < @DateStart); " +
                          " " +
                          "SELECT * FROM ( " +
                          "SELECT " +
                          "t.NomDC " +
                          ",NOM_NOMENKL AS NomNumber " +
                          ",NOM_NAME AS NomName " +
                          ",t.Date " +
                          ",Prihod " +
                          ",t.MoneyPrihod MoneyPrihod " +
                          ",MoneyPrihodWithNaklad MoneyPrihodWithNaklad " +
                          ",Rashod " +
                          ",t.Rashod * np.PRICE_WO_NAKLAD MoneyRashod " +
                          ",t.Rashod * np.PRICE MoneyRashodWithNaklad " +
                          ",SUM(Prihod - Rashod) OVER (PARTITION BY t.NomDC ORDER BY t.Date, OrderBy ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Ostatok " +
                          ",np.PRICE_WO_NAKLAD AS Price " +
                          ",np.PRICE AS PriceWithNaklad " +
                          ",ISNULL(s.Price, 0) StartPrice " +
                          ",ISNULL(s.PriceWithnaklad, 0) StartPriceWithNaklad " +
                          ",ISNULL(s.Ostatok,0) Start " +
                          "FROM #tab t " +
                          "INNER JOIN sd_83 " +
                          "  ON NomDC = SD_83.DOC_CODE " +
                          "INNER JOIN NOM_PRICE np " +
                          "  ON SD_83.DOC_CODE = np.NOM_DC " +
                          "    AND np.DATE = (SELECT " +
                          " MAX(np1.DATE) " +
                          "      FROM NOM_PRICE np1 " +
                          "      WHERE np1.NOM_DC = SD_83.DOC_CODE " +
                          "      AND np1.Date <= t.Date) " +
                          "LEFT OUTER JOIN #startprices s " +
                          "  ON t.NomDC = s.NomDC) tab " +
                          "WHERE tab.Date >= @DateStart " +
                          "ORDER BY tab.NomDC, tab.Date " +
                          " " +
                          "DROP TABLE #startprices; " +
                          "DROP TABLE #tab; ";
                var data = ctx.Database.SqlQuery<NomenklCalcMove>(sql).ToList();
                var nnomlist = (from item in data
                    group item by item.NomDC
                    into g
                    select new
                    {
                        NomDC = g.Key,
                        SummaIn = g.Sum(item => item.MoneyPrihod),
                        SummaOut = g.Sum(item => item.MoneyRashod),
                        Prihod = g.Sum(item => item.Prihod),
                        Rashod = g.Sum(item => item.Rashod),
                        LastOp = g.Last()
                    }).ToList();
                var listTemp = new List<NomenklMoveOnSkladViewModel>();
                foreach (var item in nnomlist)
                {
                    var summaIn = item.SummaIn;
                    var summaOut = item.SummaOut;
                    var newitem = new NomenklMoveOnSkladViewModel
                    {
                        Nomenkl = MainReferences.GetNomenkl(item.NomDC),
                        PriceStart = item.LastOp.StartPrice,
                        PriceEnd = item.LastOp.Price,
                        QuantityEnd = item.LastOp.Ostatok,
                        QuantityStart = item.LastOp.Start,
                        QuantityIn = item.Prihod,
                        QuantityOut = item.Rashod
                    };
                    listTemp.Add(newitem);
                    switch (newitem.CurrencyName)
                    {
                        case CurrencyCode.RUBName:
                        case CurrencyCode.RURName:
                            newitem.SummaRUBStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaRUBEnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaRUBIn = summaIn;
                            newitem.SummaRUBOut = summaOut;
                            continue;
                        case CurrencyCode.USDName:
                            newitem.SummaUSDStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaUSDEnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaUSDIn = summaIn;
                            newitem.SummaUSDOut = summaOut;
                            continue;
                        case CurrencyCode.EURName:
                            newitem.SummaEURStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaEUREnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaEURIn = summaIn;
                            newitem.SummaEUROut = summaOut;
                            continue;
                        default:
                            newitem.SummaAllStart = newitem.PriceStart * newitem.QuantityStart;
                            newitem.SummaAllEnd = newitem.PriceEnd * newitem.QuantityEnd;
                            newitem.SummaAllIn = summaIn;
                            newitem.SummaAllOut = summaOut;
                            continue;
                    }
                }

                var delList = new List<NomenklMoveOnSkladViewModel>(listTemp.Where(nl => nl.QuantityStart == 0
                    && nl.QuantityIn == 0 && nl.QuantityOut == 0 && nl.QuantityEnd == 0));
                foreach (var nl in delList) listTemp.Remove(nl);

                NomenklMoveList = new ObservableCollection<NomenklMoveOnSkladViewModel>(listTemp);
                RaisePropertyChanged(nameof(NomenklMoveList));
            }
        }

        private void LoadReferences()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql = "SELECT DISTINCT storeDC FROM NomenklMoveForCalc nmfc " +
                          "INNER JOIN HD_27 h ON h.DOC_CODE = nmfc.StoreDC " +
                          "INNER JOIN  EXT_USERS U ON U.USR_ID = H.USR_ID " +
                          $"AND UPPER(U.USR_NICKNAME) = UPPER('{GlobalOptions.UserInfo.NickName}')";
                var skls = ctx.Database.SqlQuery<decimal>(sql);
                foreach (var s in skls) Sklads.Add(MainReferences.Warehouses[s]);
            }

            RaisePropertiesChanged(nameof(Sklads));
        }

        public override void RefreshData(object obj)
        {
            DocumentList.Clear();
            NomenklMoveList.Clear();
            if (CurrentSklad == null)
                LoadForAllSklads3();
            else
                LoadForCurrentSklad3();
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
                    DocumentsOpenManager.Open(DocumentType.NomenklTransfer, CurrentDocument.DocCode,
                        CurrentDocument.Id);
                    break;
                case "Расходная накладная (без требования)":
                    DocumentsOpenManager.Open(DocumentType.Waybill, CurrentDocument.DocCode,
                        CurrentDocument.Id);
                    break;
                case "Акт валютной конвертации (по счету)":
                    DocumentsOpenManager.Open(DocumentType.InvoiceProvider, CurrentDocument.DocCode);
                    break;
            }
        }
    }
}