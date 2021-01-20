using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Calculates.Materials;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Data;
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
        
        public ObservableCollection<NomenklMoveOnSkladViewModel> NomenklMoveListTemp { set; get; } =
            new ObservableCollection<NomenklMoveOnSkladViewModel>();

        public ObservableCollection<NomPriceDocumentViewModel> DocumentList { get; set; } =
            new ObservableCollection<NomPriceDocumentViewModel>();

        public List<Core.EntityViewModel.Warehouse> Sklads { set; get; } = new List<Core.EntityViewModel.Warehouse>();

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

        public Core.EntityViewModel.Warehouse CurrentSklad
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
                        From = doc.SD_24.DD_KONTR_OTPR_DC != null ? MainReferences.GetKontragent(doc.SD_24.DD_KONTR_OTPR_DC).Name
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
                    var prc = NomenklManager.NomenklPrice(doc.DDT_NOMENKL_DC, doc.SD_24.DD_DATE, ctx).Item1;
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
                        From = doc.SD_24.DD_KONTR_OTPR_DC != null ? MainReferences.GetKontragent(doc.SD_24.DD_KONTR_OTPR_DC).Name 
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
                        {
                            newItem.DocumentName = newItem.DocumentName + " (Возврат товара)";
                        }

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
                    var prc = NomenklManager.NomenklPrice(doc.DDT_NOMENKL_DC, doc.SD_24.DD_DATE, ctx).Item1;
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
                        SummaDelta = doc.SD_24.DD_SKLAD_POL_DC == storeDC ? doc.DDT_KOL_PRIHOD * prc :
                            -doc.DDT_KOL_RASHOD * prc
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

        private void LoadForCurrentSklad2()
        {
            NomenklMoveListTemp.Clear();
            NomenklMoveList.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql1 =
                    "SELECT  NomDC ,Date ,StoreDC ,Start ,Prihod ,Rashod ,Nakopit," +
                           "SummaIn ,SummaNakladIn ,SummaOut ,SummaNakladOut, Price ," +
                           "PriceWithNaklad " +
                           "FROM dbo.NomenklMoveStore n1 " +
                           $"WHERE StoreDC = {CustomFormat.DecimalToSqlDecimal(CurrentSklad.DOC_CODE)} " +
                           "AND n1.Date = (SELECT MAX(n2.Date) FROM NomenklMoveForCalc n2  " +
                           "WHERE n2.OperTypeDC != 2010000014 and n1.NomDC = n2.NomDC  AND n1.StoreDC = n2.StoreDC  " +
                           $"AND n2.Date <= '{CustomFormat.DateToString(StartDate)}') " +
                           "AND n1.Nakopit != 0 " +
                           "UNION ALL " +
                           "SELECT  NomDC ,Date ,StoreDC ,Start ,Prihod ,Rashod ," +
                           "Nakopit ,SummaIn ,SummaNakladIn ," +
                           "SummaOut ,SummaNakladOut ,Price ,PriceWithNaklad " +
                           "FROM NomenklMoveStore " +
                           $"WHERE StoreDC = {CustomFormat.DecimalToSqlDecimal(CurrentSklad.DOC_CODE)} " +
                           $"AND Date >= '{CustomFormat.DateToString(StartDate)}' " +
                           $"AND Date <= '{CustomFormat.DateToString(EndDate)}'";
                var dataStart = NomenklCalculationManager.GetNomenklStoreRemains(StartDate, CurrentSklad.DOC_CODE).ToList();
                var dataEnd = NomenklCalculationManager.GetNomenklStoreRemains(EndDate, CurrentSklad.DOC_CODE).ToList();
                //var data = ctx.Database.SqlQuery<NomenklMoveStore>(sql1).ToList();
                var nomList = dataStart.Where(_ => _.Remain != 0).Select(_ => _.NomenklDC).Distinct().ToList();
                foreach (var ddc in dataEnd)
                {
                    if (nomList.Any(_ => _ == ddc.NomenklDC))
                        continue;
                    nomList.Add(ddc.NomenklDC);
                }

                var data = NomenklCalculationManager.NomenklMoveSum2(StartDate, EndDate, CurrentSklad.DOC_CODE);
                foreach (var d in data)
                {
                    var newitem = new NomenklMoveOnSkladViewModel
                    {
                        Nomenkl =  MainReferences.GetNomenkl(d.NomDC),
                        PriceStart = d.PriceStart,
                        PriceEnd = d.PriceEnd,
                        QuantityEnd = d.End,
                        QuantityStart = d.Start,
                        QuantityIn = d.In,
                        QuantityOut = d.Out
                    };
                    if (newitem.CurrencyName == "RUR" || newitem.CurrencyName == "RUB")
                    {
                        newitem.SummaRUBStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaRUBEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaRUBIn = d.SumIn;
                        newitem.SummaRUBOut = d.SumOut;
                    }

                    if (newitem.CurrencyName == "USD")
                    {
                        newitem.SummaUSDStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaUSDEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaUSDIn = d.SumIn;
                        newitem.SummaUSDOut = d.SumOut;
                    }

                    if (newitem.CurrencyName == "EUR")
                    {
                        newitem.SummaEURStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaEUREnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaEURIn = d.SumIn;
                        newitem.SummaEUROut = d.SumOut;
                    }

                    if (newitem.CurrencyName != "RUR" && newitem.CurrencyName != "RUB" &&
                        newitem.CurrencyName != "USD" &&
                        newitem.CurrencyName != "EUR")
                    {
                        newitem.SummaAllStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaAllEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaAllIn = d.SumIn;
                        newitem.SummaAllOut = d.SumOut;
                    }
                    NomenklMoveListTemp.Add(newitem);
                }
                //var nomList = data.Select(_ => _.NomDC).Distinct().ToList();
                //foreach (var dc in nomList)
                //{
                //    var dtemp = data.Where(_ => _.NomDC == dc && _.StoreDC == CurrentSklad.DOC_CODE).ToList();
                //    // ReSharper disable once PossibleInvalidOperationException
                //    var kolIn = (decimal) dtemp.Where(_ => _.Date >= StartDate && _.Date <= EndDate
                //    && _.NomDC == dc && _.StoreDC == CurrentSklad.DOC_CODE).Sum(_ => _.Prihod);
                //    // ReSharper disable once PossibleInvalidOperationException
                //    var kolOut = (decimal) dtemp.Where(_ => _.Date >= StartDate && _.Date <= EndDate
                //        && _.NomDC == dc && _.StoreDC == CurrentSklad.DOC_CODE)
                //        .Sum(_ => _.Rashod);
                //    var dStartrows = dtemp.Where(_ => _.Date <= StartDate);
                //    var dEnd = dtemp.Where(_ => _.Date <= EndDate).Max(_ => _.Date);
                //    var datarow = dtemp.First(_ => _.Date == dEnd);
                //    decimal start, pricestart = 0;
                //    // ReSharper disable once PossibleMultipleEnumeration
                //    if (dStartrows.Any())
                //    {
                //        // ReSharper disable once PossibleMultipleEnumeration
                //        var dt = dStartrows.Max(d => d.Date);
                //        if (dt < StartDate)
                //        {
                //            // ReSharper disable once PossibleInvalidOperationException
                //            start = (decimal) dtemp.First(_ => _.Date == dt).Nakopit;
                //            pricestart = dtemp.First(_ => _.Date == dt).Price;
                //        }
                //        else
                //        {
                //            // ReSharper disable once PossibleInvalidOperationException
                //            start = (decimal) dtemp.First(_ => _.Date == dt).Start;
                //        }
                //    }
                //    else
                //    {
                //        start = 0;
                //    }

                //    var summaIn = (decimal) dtemp.Where(_ => _.Date >= StartDate && _.Date <= EndDate)
                //        .Sum(_ => _.SummaIn);
                //    var summaOut = (decimal) dtemp.Where(_ => _.Date >= StartDate && _.Date <= EndDate)
                //        .Sum(_ => _.SummaOut);

                //    if (start == 0 && kolIn == 0 && kolOut == 0 && datarow.Nakopit == 0) continue;
                //    var newitem = new NomenklMoveOnSkladViewModel
                //    {
                //        Nomenkl = MainReferences.GetNomenkl(dc),
                //        PriceStart = pricestart,
                //        PriceEnd = datarow.Price,
                //        QuantityEnd = (decimal) datarow.Nakopit,
                //        QuantityStart = start,
                //        QuantityIn = kolIn,
                //        QuantityOut = kolOut
                //    };
                //    if (newitem.CurrencyName == "RUR" || newitem.CurrencyName == "RUB")
                //    {
                //        newitem.SummaRUBStart = newitem.PriceStart * newitem.QuantityStart;
                //        newitem.SummaRUBEnd = newitem.PriceEnd * newitem.QuantityEnd;
                //        newitem.SummaRUBIn = summaIn;
                //        newitem.SummaRUBOut = summaOut;
                //    }

                //    if (newitem.CurrencyName == "USD")
                //    {
                //        newitem.SummaUSDStart = newitem.PriceStart * newitem.QuantityStart;
                //        newitem.SummaUSDEnd = newitem.PriceEnd * newitem.QuantityEnd;
                //        newitem.SummaUSDIn = summaIn;
                //        newitem.SummaUSDOut = summaOut;
                //    }

                //    if (newitem.CurrencyName == "EUR")
                //    {
                //        newitem.SummaEURStart = newitem.PriceStart * newitem.QuantityStart;
                //        newitem.SummaEUREnd = newitem.PriceEnd * newitem.QuantityEnd;
                //        newitem.SummaEURIn = summaIn;
                //        newitem.SummaEUROut = summaOut;
                //    }

                //    if (newitem.CurrencyName != "RUR" && newitem.CurrencyName != "RUB" &&
                //        newitem.CurrencyName != "USD" &&
                //        newitem.CurrencyName != "EUR")
                //    {
                //        newitem.SummaAllStart = newitem.PriceStart * newitem.QuantityStart;
                //        newitem.SummaAllEnd = newitem.PriceEnd * newitem.QuantityEnd;
                //        newitem.SummaAllIn = summaIn;
                //        newitem.SummaAllOut = summaOut;
                //    }

                //    NomenklMoveListTemp.Add(newitem);
                //}
                foreach (var nl in NomenklMoveListTemp)
                {
                    if (nl.NomenklNumber == "53713")
                    {
                        var i = 1;
                    }
                    if (nl.QuantityStart != 0 || nl.QuantityIn != 0 || nl.QuantityOut != 0 || nl.QuantityEnd != 0)
                    {
                        NomenklMoveList.Add(nl);
                    }
                }
            }
        }

        private void LoadForAllSklads2()
        {
            NomenklMoveListTemp.Clear();
            NomenklMoveList.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var sql = "SELECT  NomDC ,Date ,Start ,Prihod ,Rashod ,Nakopit ,SummaIn ," +
                          "SummaNakladIn ,SummaOut ,SummaNakladOut ,Price ," +
                          "PriceWithNaklad " +
                          "FROM dbo.NomenklMove n1 " +
                          "WHERE n1.Date = (SELECT MAX(n2.Date)  " +
                          "FROM NomenklMoveForCalc n2  " +
                          $"WHERE n2.OperTypeDC != 2010000014 and  n1.NomDC = n2.NomDC  AND n2.Date <= '{CustomFormat.DateToString(StartDate)}') " +
                          "AND n1.Nakopit != 0 " +
                          "UNION " +
                          "SELECT  NomDC ,Date ,Start ,Prihod ,Rashod ,Nakopit ," +
                          "SummaIn ,SummaNakladIn ,SummaOut ,SummaNakladOut ,Price , PriceWithNaklad " +
                          "FROM NomenklMove " +
                          $"WHERE Date >= '{CustomFormat.DateToString(StartDate)}' " +
                          $"AND Date <= '{CustomFormat.DateToString(EndDate)}'";
                var data = ctx.Database.SqlQuery<NomenklMove>(sql).ToList();
                var nomList = data.Select(_ => _.NomDC).Distinct().ToList();
                foreach (var dc in nomList)
                {
                    var dtemp = data.Where(_ => _.NomDC == dc).ToList();
                    var kolIn = (decimal) dtemp.Where(_ => _.Date >= StartDate && _.Date <= EndDate).Sum(_ => _.Prihod);
                    var kolOut = (decimal) dtemp.Where(_ => _.Date >= StartDate && _.Date <= EndDate)
                        .Sum(_ => _.Rashod);
                    var dStartrows = dtemp.Where(_ => _.Date <= StartDate);
                    var dEnd = dtemp.Where(_ => _.Date <= EndDate).Max(_ => _.Date);
                    var datarow = dtemp.Last(_ => _.Date == dEnd);
                    decimal start, pricestart = 0;
                    if (dStartrows.Any())
                    {
                        var dt = dStartrows.Max(d => d.Date);
                        if (dt < StartDate)
                        {
                            start = (decimal) dtemp.First(_ => _.Date == dt).Nakopit;
                            pricestart = dtemp.First(_ => _.Date == dt).Price;
                        }
                        else
                        {
                            start = (decimal) dtemp.First(_ => _.Date == dt).Start;
                        }
                    }
                    else
                    {
                        start = 0;
                    }

                    var summaIn = (decimal) dtemp.Where(_ => _.Date >= StartDate && _.Date <= EndDate)
                        .Sum(_ => _.SummaIn);
                    var summaOut = (decimal) dtemp.Where(_ => _.Date >= StartDate && _.Date <= EndDate)
                        .Sum(_ => _.SummaOut);
                    //if (start == 0 && kolIn == 0 && kolOut == 0 && datarow.Nakopit == 0) continue;
                    var newitem = new NomenklMoveOnSkladViewModel
                    {
                        Nomenkl = MainReferences.GetNomenkl(dc),
                        PriceStart = pricestart,
                        PriceEnd = datarow.Price,
                        QuantityEnd = (decimal) datarow.Nakopit,
                        QuantityStart = start,
                        QuantityIn = kolIn,
                        QuantityOut = kolOut
                    };
                    if (newitem.CurrencyName == "RUR" || newitem.CurrencyName == "RUB")
                    {
                        newitem.SummaRUBStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaRUBEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaRUBIn = summaIn;
                        newitem.SummaRUBOut = summaOut;
                    }

                    if (newitem.CurrencyName == "USD")
                    {
                        newitem.SummaUSDStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaUSDEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaUSDIn = summaIn;
                        newitem.SummaUSDOut = summaOut;
                    }

                    if (newitem.CurrencyName == "EUR")
                    {
                        newitem.SummaEURStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaEUREnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaEURIn = summaIn;
                        newitem.SummaEUROut = summaOut;
                    }

                    if (newitem.CurrencyName != "RUR" && newitem.CurrencyName != "RUB" &&
                        newitem.CurrencyName != "USD" &&
                        newitem.CurrencyName != "EUR")
                    {
                        newitem.SummaAllStart = newitem.PriceStart * newitem.QuantityStart;
                        newitem.SummaAllEnd = newitem.PriceEnd * newitem.QuantityEnd;
                        newitem.SummaAllIn = summaIn;
                        newitem.SummaAllOut = summaOut;
                    }

                    NomenklMoveListTemp.Add(newitem);
                }
                foreach (var nl in NomenklMoveListTemp)
                {
                    if (nl.QuantityStart != 0 || nl.QuantityIn != 0 || nl.QuantityOut != 0 || nl.QuantityEnd != 0)
                    {
                        NomenklMoveList.Add(nl);
                    }
                }
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
                LoadForAllSklads2();
            else
                LoadForCurrentSklad2();
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