using System;
using System.Collections.Generic;
using System.Linq;
using Calculates.Materials;
using Core;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using KursAM2.View.Base;
using KursAM2.ViewModel.Logistiks;

namespace KursAM2.Managers
{
    public class NomenklManager
    {
        public static List<Nomenkl> SelectNomenklsDialog()
        {
            var ctx = new NomenklSelectedDialogViewModel();
            var dlg = new SelectDialogView {DataContext = ctx};
            dlg.ShowDialog();
            if (!ctx.DialogResult) return null;
            return new List<Nomenkl>(ctx.SelectedNomenkls);
        }

        public List<NomenklGroup> SelectNomenklGroups()
        {
            var ret = new List<NomenklGroup>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                ret.AddRange(ctx.SD_82.Select(g => new NomenklGroup
                {
                    DocCode = g.DOC_CODE,
                    Name = g.CAT_NAME,
                    ParentDC = g.CAT_PARENT_DC
                }));
            }
            return ret;
        }

        public static List<NomenklTransferRowViewModelExt> SelectNomenklsWithRemainsDialog(Store store)
        {
            var ctxTransf = new NomTransferAddForSklad(store);
            var dlg = new SelectDialogView {DataContext = ctxTransf};
            dlg.ShowDialog();
            if (!ctxTransf.DialogResult) return null;
            var ret =
                ctxTransf.Rows.Where(_ => _.IsAccepted).Select(row => new NomenklTransferRowViewModelExt
                {
                    Id = Guid.NewGuid(),
                    //DocId = Document.Id,
                    NomenklOut = row.Nomenkl,
                    MaxQuantity = row.Quantity,
                    Quantity = row.Quantity,
                    State = RowStatus.NewRow,
                    PriceOut = row.Price,
                    SummaOut = row.Quantity*row.Price,
                    NakladEdSumma = row.PriceWithNaklad-row.Price,
                    NakladRate = 0
                    
                    //Parent = Document
                }).ToList();
            return ret;
        }

        public static List<NomenklTransferRowViewModelExt> SelectNomenklsWithRemainsDialog(Store store, DateTime date)
        {
            var ctxTransf = new NomTransferAddForSklad(store, date);
            var dlg = new SelectDialogView { DataContext = ctxTransf };
            dlg.ShowDialog();
            if (!ctxTransf.DialogResult) return null;
            var ret =
                ctxTransf.Rows.Where(_ => _.IsAccepted).Select(row => new NomenklTransferRowViewModelExt
                {
                    Id = Guid.NewGuid(),
                    //DocId = Document.Id,
                    NomenklOut = row.Nomenkl,
                    MaxQuantity = row.Quantity,
                    Quantity = row.Quantity,
                    State = RowStatus.NewRow,
                    PriceOut = row.Price,
                    SummaOut = row.Quantity * row.Price,
                    NakladEdSumma = row.PriceWithNaklad - row.Price,
                    NakladRate = 0

                    //Parent = Document
                }).ToList();
            return ret;
        }

        public static List<Nomenkl> GetNomenklsSearch(string searchText, bool isDeleted = false)
        {
            var ret = new List<Nomenkl>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var n in ctx.SD_83.Where(_ => (_.NOM_NAME.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_NOMENKL.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_FULL_NAME.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_NOTES.ToUpper().Contains(searchText.ToUpper())) &&
                                                       (_.NOM_DELETED == (isDeleted ? 1 : 0))))
                    ret.Add(new Nomenkl
                    {
                        DocCode = n.DOC_CODE,
                        Name = n.NOM_NAME,
                        NameFull = n.NOM_FULL_NAME,
                        Note = n.NOM_NOTES,
                        NomenklNumber = n.NOM_NOMENKL,
                        Group = MainReferences.NomenklGroups[n.NOM_CATEG_DC],
                        Currency = MainReferences.Currencies[n.NOM_SALE_CRS_DC.Value]
                    });
            }
            return ret;
        }

        public decimal GetNomenklCount(decimal nomDC, decimal storeDC)
        {
            var item = NomenklCalculationManager.GetNomenklStoreRemain(DateTime.Today, nomDC, storeDC);
            return item?.Remain ?? 0;
        }

        public decimal GetNomenklCount(DateTime date, decimal nomDC, decimal storeDC)
        {
            var item = NomenklCalculationManager.GetNomenklStoreRemain(date, nomDC, storeDC);
            return item?.Remain ?? 0;
        }
    }
}