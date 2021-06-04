using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.NomenklManagement;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using KursAM2.View.Logistiks.UC;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomPriceWindowViewModel : RSWindowViewModelBase, IDataUserControl
    {
        private NomPriceViewModel myCurrentCalc;
        private NomPriceDocumentViewModel myCurrentDocvument;
        private NomenklMoveWithCalcPriceUC myDataUserControl;
        private string myNomenklCurrency;
        private string myNomenklName;
        private string myNomenklNumber;

        public NomPriceWindowViewModel()
        {
            myDataUserControl = new NomenklMoveWithCalcPriceUC();
            WindowName = "Расчет себестоимости товара";
        }

        public NomPriceWindowViewModel(decimal nomDC) : this()
        {
            RefreshData(nomDC);
            var nom = MainReferences.GetNomenkl(nomDC);
            if (nom == null) return;
            NomenklNumber = nom.NomenklNumber;
            NomenklName = nom.Name;
            NomenklCurrency = nom.Currency.Name;
        }

        public ObservableCollection<NomPriceViewModel> CalcList { get; set; } =
            new ObservableCollection<NomPriceViewModel>();

        public ObservableCollection<NomPriceDocumentViewModel> DocumentList { get; set; } =
            new ObservableCollection<NomPriceDocumentViewModel>();

        public string NomenklNumber
        {
            get => myNomenklNumber;
            set
            {
                if (myNomenklNumber == value) return;
                myNomenklNumber = value;
                RaisePropertyChanged();
            }
        }

        public string NomenklName
        {
            get => myNomenklName;
            set
            {
                if (myNomenklName == value) return;
                myNomenklName = value;
                RaisePropertyChanged();
            }
        }

        public string NomenklCurrency
        {
            get => myNomenklCurrency;
            set
            {
                if (myNomenklCurrency == value) return;
                myNomenklCurrency = value;
                RaisePropertyChanged();
            }
        }

        public NomPriceDocumentViewModel CurrentDocvument
        {
            get => myCurrentDocvument;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentDocvument == value) return;
                myCurrentDocvument = value;
                RaisePropertyChanged();
            }
        }

        public NomenklMoveWithCalcPriceUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (Equals(myDataUserControl, value)) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }

        public NomPriceViewModel CurrentCalc
        {
            get => myCurrentCalc;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentCalc == value) return;
                myCurrentCalc = value;
                LoadDocuments();
                RaisePropertyChanged();
            }
        }

        public DependencyObject LayoutControl
        {
            get
            {
                var ctrl = DataUserControl?.LayoutManager.LayoutControl;
                return ctrl;
            }
        }

        public override void RefreshData(object obj)
        {
            if (!(obj is decimal)) return;
            var dc = Convert.ToDecimal(obj);
            CalcList.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var clc in ctx.NOM_PRICE.Where(_ => _.NOM_DC == dc).OrderBy(_ => _.DATE))
                    CalcList.Add(new NomPriceViewModel(clc));
            }
        }

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
                            _.DDT_NOMENKL_DC == CurrentCalc.NOM_DC && _.SD_24.DD_DATE == CurrentCalc.DATE &&
                            _.SD_24.DD_TYPE_DC == 2010000005)
                    .ToList();
                foreach (var doc in docs)
                {
                    var newItem = new NomPriceDocumentViewModel
                    {
                        DocumentName = doc.SD_24.SD_201.D_NAME,
                        DocumentNum = doc.SD_24.DD_IN_NUM + "/" + doc.SD_24.DD_EXT_NUM,
                        DocumentDate = doc.SD_24.DD_DATE,
                        QuantityIn = doc.DDT_KOL_PRIHOD,
                        QuantityOut = doc.DDT_KOL_RASHOD,
                        QuantityDelta = doc.DDT_KOL_PRIHOD - doc.DDT_KOL_RASHOD,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_OTPR_DC.Value].Name,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        To = MainReferences.Warehouses[doc.SD_24.DD_SKLAD_POL_DC.Value].Name
                    };
                    newItem.SummaIn = (decimal) (doc.DDT_KOL_PRIHOD * doc.DDT_TAX_CRS_CENA);
                    newItem.SummaOut = doc.DDT_KOL_PRIHOD *
                                       Nomenkl.PriceWithOutNaklad(doc.DDT_NOMENKL_DC, doc.SD_24.DD_DATE);
                    newItem.SummaDelta = newItem.SummaIn - newItem.SummaOut;
                    newItem.Note = doc.SD_24.DD_NOTES;
                    DocumentList.Add(newItem);
                }

                docs = ctx.TD_24
                    .Include(_ => _.SD_24)
                    .Include(_ => _.SD_24.SD_201)
                    .Include(_ => _.TD_26)
                    .Include(_ => _.TD_26.SD_26)
                    .Where(
                        _ =>
                            _.DDT_NOMENKL_DC == CurrentCalc.NOM_DC && _.SD_24.DD_DATE == CurrentCalc.DATE &&
                            _.SD_24.DD_TYPE_DC == 2010000001)
                    .ToList();
                foreach (var doc in docs)
                {
                    var newItem = new NomPriceDocumentViewModel
                    {
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
                            _.DDT_NOMENKL_DC == CurrentCalc.NOM_DC && _.SD_24.DD_DATE == CurrentCalc.DATE &&
                            _.SD_24.DD_TYPE_DC == 2010000012)
                    .ToList();
                foreach (var doc in docs)
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
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
                        SummaOut = doc.DDT_KOL_RASHOD * CurrentCalc.PRICE_WO_NAKLAD,
                        SummaDelta = -doc.DDT_KOL_RASHOD * CurrentCalc.PRICE_WO_NAKLAD,
                        Note = doc.SD_24.DD_NOTES + " / " + doc.TD_84.SD_84.SF_NOTE
                    });

                //Загрузка валютного трансфера
                var transferin = ctx.NomenklTransferRow.Include(_ => _.NomenklTransfer)
                    .Where(
                        _ =>
                            _.IsAccepted && _.NomenklInDC == CurrentCalc.NOM_DC &&
                            _.NomenklTransfer.Date == CurrentCalc.DATE)
                    .ToList();
                var transferout = ctx.NomenklTransferRow.Include(_ => _.NomenklTransfer)
                    .Where(
                        _ =>
                            _.IsAccepted && _.NomenklOutDC == CurrentCalc.NOM_DC &&
                            _.NomenklTransfer.Date == CurrentCalc.DATE)
                    .ToList();
                foreach (var t in transferin)
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        DocumentName = "Акт валютной таксировки номенклатур",
                        DocumentNum = t.NomenklTransfer.DucNum.ToString(),
                        DocumentDate = t.NomenklTransfer.Date,
                        QuantityIn = t.Quantity,
                        QuantityOut = 0,
                        QuantityDelta = t.Quantity,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[t.NomenklTransfer.SkladDC.Value].Name,
                        To = MainReferences.Warehouses[t.NomenklTransfer.SkladDC.Value].Name,
                        SummaIn = t.PriceIn * t.Quantity,
                        SummaOut = 0,
                        SummaDelta = t.PriceIn * t.Quantity,
                        Note = t.Note
                    });
                foreach (var t in transferout)
                    DocumentList.Add(new NomPriceDocumentViewModel
                    {
                        DocumentName = "Акт валютной таксировки номенклатур",
                        DocumentNum = t.NomenklTransfer.DucNum.ToString(),
                        DocumentDate = t.NomenklTransfer.Date,
                        QuantityIn = 0,
                        QuantityOut = t.Quantity,
                        QuantityDelta = -t.Quantity,
                        // ReSharper disable once AssignNullToNotNullAttribute
                        // ReSharper disable once PossibleInvalidOperationException
                        From = MainReferences.Warehouses[t.NomenklTransfer.SkladDC.Value].Name,
                        To = MainReferences.Warehouses[t.NomenklTransfer.SkladDC.Value].Name,
                        SummaIn = 0,
                        SummaOut = Nomenkl.PriceWithOutNaklad(t.NomenklOutDC, t.NomenklTransfer.Date) * t.Quantity,
                        SummaDelta = -Nomenkl.PriceWithOutNaklad(t.NomenklOutDC, t.NomenklTransfer.Date) * t.Quantity,
                        Note = t.Note
                    });
            }
        }
    }
}