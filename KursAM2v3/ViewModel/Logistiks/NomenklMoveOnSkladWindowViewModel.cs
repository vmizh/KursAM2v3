using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.NomenklManagement;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using KursAM2.Managers;
using KursAM2.Managers.Nomenkl;
using KursAM2.View.Base;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklMoveOnSkladWindowViewModel : RSWindowViewModelBase
    {
        private readonly NomenklManager2 nomenklManager = new NomenklManager2(GlobalOptions.GetEntities());
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
            IsShowAll = true;
        }

        public NomenklMoveOnSkladWindowViewModel(Window form) : base(form)
        {
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            StartDate = DateTime.Today;
            IsShowAll = true;
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
                IsShowAll = myCurrentSklad == null;
                NomenklMoveList.Clear();
                RaisePropertyChanged(nameof(NomenklMoveList));
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
            get { return new Command(DocumentOpen, _ => IsDocumentOpenAllow); }
        }

        public override bool IsDocumentOpenAllow => CurrentDocument != null;

        private bool MustExclude(List<NOMENKL_PRIH_EXCLUDE> list, TD_24 doc)
        {
            foreach (var l in list)
                if (l.DOC_CODE == doc.DOC_CODE && l.CODE == doc.CODE)
                    return true;
            return false;
        }

        private void LoadDocuments()
        {
            DocumentList.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var exclude = ctx.NOMENKL_PRIH_EXCLUDE.ToList();
                var docs = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                            _.SD_24.DD_DATE >= StartDate && _.SD_24.DD_DATE <= EndDate &&
                            _.SD_24.DD_TYPE_DC == 2010000005 && _.DDT_KOL_PRIHOD > 0)
                    .ToList();
                foreach (var doc in docs)
                {
                    if (MustExclude(exclude, doc)) continue;
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
                }

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
                    if (MustExclude(exclude, doc)) continue;
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
                            : MainReferences.GetWarehouse(doc.SD_24.DD_SKLAD_OTPR_DC).Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = MainReferences.GetWarehouse(doc.SD_24.DD_SKLAD_POL_DC).Name
                    };
                    if (doc.TD_26 != null)
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        newItem.SummaIn = (decimal)(doc.TD_26.SFT_SUMMA_K_OPLATE * doc.DDT_KOL_PRIHOD) /
                                          doc.TD_26.SFT_KOL;
                        newItem.SummaOut = 0;
                        newItem.SummaDelta =
// ReSharper disable once PossibleInvalidOperationException
                            (decimal)(doc.TD_26.SFT_SUMMA_K_OPLATE / doc.TD_26.SFT_KOL * doc.DDT_KOL_PRIHOD);
                        newItem.Note = doc.SD_24.DD_NOTES + " / " + doc.TD_26.SD_26.SF_NOTES;
                    }
                    else
                    {
                        if (doc.SD_24.DD_SKLAD_OTPR_DC == null && doc.TD_26 == null)
                            newItem.DocumentName += "(неоттаксированный приход)";
                        else
                            newItem.DocumentName += doc.SD_24.DD_VOZVRAT == 1
                                ? " (Возврат товара)"
                                : " (Внутреннее перемещение)";
                        newItem.SummaIn = 0;
                        newItem.SummaOut = 0;
                        newItem.SummaDelta = 0;
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
                {
                    if (MustExclude(exclude, doc)) continue;
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
                            _.SD_24.DD_TYPE_DC == 2010000003)
                    .ToList();
                foreach (var doc in docs)
                {
                    if (MustExclude(exclude, doc)) continue;
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        DocCode = doc.DOC_CODE,
                        DocumentName = doc.SD_24.SD_201.D_NAME +
                                       (doc.TD_84 == null ? "  (внутреннее перемещение)" : " (отгрузка)"),
                        DocumentNum = doc.SD_24.DD_IN_NUM + "/" + doc.SD_24.DD_EXT_NUM,
                        DocumentDate = doc.SD_24.DD_DATE,
                        QuantityIn = 0,
                        QuantityOut = doc.DDT_KOL_RASHOD,
                        QuantityDelta = -doc.DDT_KOL_RASHOD,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.GetWarehouse(doc.SD_24.DD_SKLAD_OTPR_DC).Name,
                        To = doc.SD_24.DD_KONTR_POL_DC != null
                            ? MainReferences.GetKontragent(doc.SD_24.DD_KONTR_POL_DC)?.Name
                            : MainReferences.GetWarehouse(doc.SD_24.DD_SKLAD_POL_DC)?.Name,
                        SummaIn = 0,
                        SummaOut = doc.DDT_KOL_RASHOD * CurrentNomenklMoveItem.PriceStart,
                        SummaDelta = -doc.DDT_KOL_RASHOD * CurrentNomenklMoveItem.PriceEnd,
                        Note = doc.SD_24.DD_NOTES + " / " + (doc.TD_84 != null ? doc.TD_84.SD_84.SF_NOTE : string.Empty)
                    });
                }

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
                    if (MustExclude(exclude, doc)) continue;
                    var prc = nomenklManager.GetNomenklPrice(doc.DDT_NOMENKL_DC, doc.SD_24.DD_DATE).Price;
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
                        From = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_OTPR_DC.Value].Name,
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
                    var prc = nomenklManager.GetNomenklPrice(doc.Nomenkl_DC, doc.AktSpisaniyaNomenkl_Title.Date_Doc)
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
                var exclude = ctx.NOMENKL_PRIH_EXCLUDE.ToList();
                var docs = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                            _.SD_24.DD_DATE >= StartDate && _.SD_24.DD_DATE <= EndDate &&
                            _.SD_24.DD_TYPE_DC == 2010000005 && _.DDT_KOL_PRIHOD > 0
                            && (_.SD_24.DD_SKLAD_POL_DC == storeDC || _.SD_24.DD_SKLAD_OTPR_DC == storeDC))
                    .ToList();
                foreach (var doc in docs)
                {
                    if (MustExclude(exclude, doc)) continue;
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
                }

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
                            && _.SD_24.DD_SKLAD_POL_DC == storeDC)
                    .ToList();
                foreach (var doc in docs)
                {
                    if (MustExclude(exclude, doc)) continue;
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
                            : MainReferences.GetWarehouse(doc.SD_24.DD_SKLAD_OTPR_DC).Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = MainReferences.GetWarehouse(doc.SD_24.DD_SKLAD_POL_DC).Name
                    };
                    if (doc.TD_26 != null)
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        newItem.SummaIn = (decimal)(doc.TD_26.SFT_SUMMA_K_OPLATE * doc.DDT_KOL_PRIHOD) /
                                          doc.TD_26.SFT_KOL;
                        newItem.SummaOut = 0;
                        newItem.SummaDelta =
// ReSharper disable once PossibleInvalidOperationException
                            (decimal)(doc.TD_26.SFT_SUMMA_K_OPLATE / doc.TD_26.SFT_KOL * doc.DDT_KOL_PRIHOD);
                        newItem.Note = doc.SD_24.DD_NOTES + " / " + doc.TD_26.SD_26.SF_NOTES;
                    }
                    else
                    {
                        if (doc.SD_24.DD_SKLAD_OTPR_DC == null && doc.TD_26 == null)
                            newItem.DocumentName += "(неоттаксированный приход)";
                        else
                            newItem.DocumentName += doc.SD_24.DD_VOZVRAT == 1
                                ? " (Возврат товара)"
                                : " (Внутреннее перемещение)";

                        newItem.SummaIn = 0;
                        newItem.SummaOut = 0;
                        newItem.SummaDelta = 0;
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

                docs = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Include(_ => _.TD_84)
                    .Include(_ => _.TD_84.SD_84)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentNomenklMoveItem.Nomenkl.DocCode &&
                            _.SD_24.DD_DATE >= StartDate && _.SD_24.DD_DATE <= EndDate &&
                            _.SD_24.DD_SKLAD_OTPR_DC == storeDC &&
                            _.SD_24.DD_TYPE_DC == 2010000003)
                    .ToList();
                foreach (var doc in docs)
                {
                    if (MustExclude(exclude, doc)) continue;
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        DocCode = doc.DOC_CODE,
                        DocumentName = doc.SD_24.SD_201.D_NAME +
                                       (doc.TD_84 == null ? "  (внутреннее перемещение)" : " (отгрузка)"),
                        DocumentNum = doc.SD_24.DD_IN_NUM + "/" + doc.SD_24.DD_EXT_NUM,
                        DocumentDate = doc.SD_24.DD_DATE,
                        QuantityIn = 0,
                        QuantityOut = doc.DDT_KOL_RASHOD,
                        QuantityDelta = -doc.DDT_KOL_RASHOD,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.GetWarehouse(doc.SD_24.DD_SKLAD_OTPR_DC).Name,
                        To = doc.SD_24.DD_KONTR_POL_DC != null
                            ? MainReferences.GetKontragent(doc.SD_24.DD_KONTR_POL_DC)?.Name
                            : MainReferences.GetWarehouse(doc.SD_24.DD_SKLAD_POL_DC)?.Name,
                        SummaIn = 0,
                        SummaOut = doc.DDT_KOL_RASHOD * CurrentNomenklMoveItem.PriceStart,
                        SummaDelta = -doc.DDT_KOL_RASHOD * CurrentNomenklMoveItem.PriceEnd,
                        Note = doc.SD_24.DD_NOTES + " / " + (doc.TD_84 != null ? doc.TD_84.SD_84.SF_NOTE : string.Empty)
                    });
                }

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
                    var prc = nomenklManager.GetNomenklPrice(doc.DDT_NOMENKL_DC, doc.SD_24.DD_DATE).Price;
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
                    var prc = nomenklManager.GetNomenklPrice(doc.Nomenkl_DC, doc.AktSpisaniyaNomenkl_Title.Date_Doc)
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
            var ctx = new NomPriceWindowViewModel((decimal)CurrentNomenklMoveItem?.Nomenkl.DocCode);
            var dlg = new SelectDialogView { DataContext = ctx };
            dlg.ShowDialog();
        }

        private void LoadForCurrentSklad4()
        {
            var quanData = nomenklManager.GetNomenklStoreQuantity(CurrentSklad.DocCode, StartDate, EndDate);
            var moveDate = nomenklManager.GetNomenklStoreMove(CurrentSklad.DocCode, StartDate, EndDate);
            var listTemp = new List<NomenklMoveOnSkladViewModel>();
            foreach (var n in quanData)
            {
                var old = listTemp.FirstOrDefault(_ => _.Nomenkl.DocCode == n.NomDC);
                if (old != null)
                {
                    old.QuantityStart += n.StartQuantity;
                    old.QuantityEnd += n.OstatokQuantity;
                    old.QuantityIn += moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.Prihod);
                    old.QuantityOut += moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.Rashod);

                    switch (old.CurrencyName)
                    {
                        case CurrencyCode.RUBName:
                        case CurrencyCode.RURName:
                            old.SummaRUBStart += n.StartSumma;
                            old.SummaRUBEnd += n.OstatokSumma;
                            old.SummaRUBIn += moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.PrihodSumma);
                            old.SummaRUBOut += moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.RashodSumma);
                            break;
                        case CurrencyCode.USDName:
                            old.SummaUSDStart += n.StartSumma;
                            old.SummaUSDEnd += n.OstatokSumma;
                            old.SummaUSDIn += moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.PrihodSumma);
                            old.SummaUSDOut += moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.RashodSumma);
                            break;
                        case CurrencyCode.EURName:
                            old.SummaEURStart += n.StartSumma;
                            old.SummaEUREnd += n.OstatokSumma;
                            old.SummaEURIn += moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.PrihodSumma);
                            old.SummaEUROut += moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.RashodSumma);
                            break;
                        default:
                            old.SummaAllStart += n.StartSumma;
                            old.SummaAllEnd += n.OstatokSumma;
                            old.SummaAllIn += moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.PrihodSumma);
                            old.SummaAllOut += moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.RashodSumma);
                            break;
                    }
                }
                else
                {
                    var newitem = new NomenklMoveOnSkladViewModel
                    {
                        Nomenkl = MainReferences.GetNomenkl(n.NomDC),
                        PriceEnd = n.OstatokQuantity != 0 ? Math.Round(n.OstatokSumma / n.OstatokQuantity, 2) : 0,
                        PriceStart = n.StartQuantity != 0 ? Math.Round(n.StartSumma / n.StartQuantity, 2) : 0,
                        QuantityEnd = n.OstatokQuantity,
                        QuantityIn = moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.Prihod),
                        QuantityOut = moveDate.Where(_ => _.NomDC == n.NomDC).Sum(s => s.Rashod),
                        QuantityStart = n.StartQuantity
                    };
                    switch (newitem.CurrencyName)
                    {
                        case CurrencyCode.RUBName:
                        case CurrencyCode.RURName:
                            newitem.SummaRUBStart = n.StartSumma;
                            newitem.SummaRUBEnd = n.OstatokSumma;
                            newitem.SummaRUBIn = moveDate.Where(_ => _.NomDC == n.NomDC)
                                .Sum(s => s.PrihodSumma);
                            newitem.SummaRUBOut = moveDate.Where(_ => _.NomDC == n.NomDC)
                                .Sum(s => s.RashodSumma);
                            break;
                        case CurrencyCode.USDName:
                            newitem.SummaUSDStart = n.StartSumma;
                            newitem.SummaUSDEnd = n.OstatokSumma;
                            newitem.SummaUSDIn = moveDate.Where(_ => _.NomDC == n.NomDC)
                                .Sum(s => s.PrihodSumma);
                            newitem.SummaUSDOut = moveDate.Where(_ => _.NomDC == n.NomDC)
                                .Sum(s => s.RashodSumma);
                            break;
                        case CurrencyCode.EURName:
                            newitem.SummaEURStart = n.StartSumma;
                            newitem.SummaEUREnd = n.OstatokSumma;
                            newitem.SummaEURIn = moveDate.Where(_ => _.NomDC == n.NomDC)
                                .Sum(s => s.PrihodSumma);
                            newitem.SummaEUROut = moveDate.Where(_ => _.NomDC == n.NomDC)
                                .Sum(s => s.RashodSumma);
                            break;
                        default:
                            newitem.SummaAllStart = n.StartSumma;
                            newitem.SummaAllEnd = n.OstatokSumma;
                            newitem.SummaAllIn = moveDate.Where(_ => _.NomDC == n.NomDC)
                                .Sum(s => s.PrihodSumma);
                            newitem.SummaAllOut = moveDate.Where(_ => _.NomDC == n.NomDC)
                                .Sum(s => s.RashodSumma);
                            break;
                    }

                    listTemp.Add(newitem);
                }
            }

            NomenklMoveList = new ObservableCollection<NomenklMoveOnSkladViewModel>(listTemp.Where(nl =>
                nl.QuantityStart != 0
                || nl.QuantityIn != 0 || nl.QuantityOut != 0 || nl.QuantityEnd != 0));
            RaisePropertyChanged(nameof(NomenklMoveList));
        }

        private void LoadForAllSklads4()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sklDCList = ctx.NomenklMoveForCalc.Select(_ => _.StoreDC).Distinct();
                var skladsInfo = new Dictionary<decimal, List<NomenklQuantityInfo>>();
                var moveInfo = new Dictionary<decimal, List<NomenklMoveInfo>>();
                foreach (var dc in sklDCList)
                    // ReSharper disable once PossibleInvalidOperationException
                    skladsInfo.Add((decimal)dc, new List<NomenklQuantityInfo>());
                foreach (var sklDC in sklDCList)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    skladsInfo[(decimal)sklDC] =
                        nomenklManager.GetNomenklStoreQuantity((decimal)sklDC, StartDate, EndDate);
                    moveInfo[(decimal)sklDC] = nomenklManager.GetNomenklStoreMove((decimal)sklDC, StartDate, EndDate);
                }

                var listTemp = new List<NomenklMoveOnSkladViewModel>();
                foreach (var sklDC in skladsInfo.Keys)
                foreach (var n in skladsInfo[sklDC])
                {
                    var old = listTemp.FirstOrDefault(_ => _.Nomenkl.DocCode == n.NomDC);
                    if (old != null)
                    {
                        old.QuantityStart += n.StartQuantity;
                        old.QuantityEnd += n.OstatokQuantity;
                        old.QuantityIn += moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC).Sum(s => s.Prihod);
                        old.QuantityOut += moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC).Sum(s => s.Rashod);

                        switch (old.CurrencyName)
                        {
                            case CurrencyCode.RUBName:
                            case CurrencyCode.RURName:
                                old.SummaRUBStart += n.StartSumma;
                                old.SummaRUBEnd += n.OstatokSumma;
                                old.SummaRUBIn += moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.PrihodSumma);
                                old.SummaRUBOut += moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.RashodSumma);
                                break;
                            case CurrencyCode.USDName:
                                old.SummaUSDStart += n.StartSumma;
                                old.SummaUSDEnd += n.OstatokSumma;
                                old.SummaUSDIn += moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.PrihodSumma);
                                old.SummaUSDOut += moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.RashodSumma);
                                break;
                            case CurrencyCode.EURName:
                                old.SummaEURStart += n.StartSumma;
                                old.SummaEUREnd += n.OstatokSumma;
                                old.SummaEURIn += moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.PrihodSumma);
                                old.SummaEUROut += moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.RashodSumma);
                                break;
                            default:
                                old.SummaAllStart += n.StartSumma;
                                old.SummaAllEnd += n.OstatokSumma;
                                old.SummaAllIn += moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.PrihodSumma);
                                old.SummaAllOut += moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.RashodSumma);
                                break;
                        }
                    }
                    else
                    {
                        var newitem = new NomenklMoveOnSkladViewModel
                        {
                            Nomenkl = MainReferences.GetNomenkl(n.NomDC),
                            PriceEnd = n.OstatokQuantity != 0 ? Math.Round(n.OstatokSumma / n.OstatokQuantity, 2) : 0,
                            PriceStart = n.StartQuantity != 0 ? Math.Round(n.StartSumma / n.StartQuantity, 2) : 0,
                            QuantityEnd = n.OstatokQuantity,
                            QuantityIn = moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC).Sum(s => s.Prihod),
                            QuantityOut = moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC).Sum(s => s.Rashod),
                            QuantityStart = n.StartQuantity
                        };
                        switch (newitem.CurrencyName)
                        {
                            case CurrencyCode.RUBName:
                            case CurrencyCode.RURName:
                                newitem.SummaRUBStart = n.StartSumma;
                                newitem.SummaRUBEnd = n.OstatokSumma;
                                newitem.SummaRUBIn = moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.PrihodSumma);
                                newitem.SummaRUBOut = moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.RashodSumma);
                                break;
                            case CurrencyCode.USDName:
                                newitem.SummaUSDStart = n.StartSumma;
                                newitem.SummaUSDEnd = n.OstatokSumma;
                                newitem.SummaUSDIn = moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.PrihodSumma);
                                newitem.SummaUSDOut = moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.RashodSumma);
                                break;
                            case CurrencyCode.EURName:
                                newitem.SummaEURStart = n.StartSumma;
                                newitem.SummaEUREnd = n.OstatokSumma;
                                newitem.SummaEURIn = moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.PrihodSumma);
                                newitem.SummaEUROut = moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.RashodSumma);
                                break;
                            default:
                                newitem.SummaAllStart = n.StartSumma;
                                newitem.SummaAllEnd = n.OstatokSumma;
                                newitem.SummaAllIn = moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.PrihodSumma);
                                newitem.SummaAllOut = moveInfo[sklDC].Where(_ => _.NomDC == n.NomDC)
                                    .Sum(s => s.RashodSumma);
                                break;
                        }

                        listTemp.Add(newitem);
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
                LoadForAllSklads4();
            else
                LoadForCurrentSklad4();
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
