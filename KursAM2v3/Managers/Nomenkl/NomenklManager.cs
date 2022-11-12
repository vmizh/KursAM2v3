using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Core.WindowsManager;
using Data;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl.ViewModel;
using KursAM2.ViewModel.Logistiks;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursAM2.Managers.Nomenkl
{
    public class NomenklManager
    {
        public List<NomenklGroupViewModel> SelectNomenklGroups()
        {
            var ret = new List<NomenklGroupViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                ret.AddRange(ctx.SD_82.Select(g => new NomenklGroupViewModel
                {
                    DocCode = g.DOC_CODE,
                    Name = g.CAT_NAME,
                    ParentDC = g.CAT_PARENT_DC
                }));
            }

            return ret;
        }

        public static List<NomenklTransferRowViewModelExt> SelectNomenklsWithRemainsDialog(Warehouse warehouse,
            DateTime date)
        {
            var ctxTransf = new NomTransferAddForSklad(warehouse, date);
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

        public static List<KursDomain.References.Nomenkl> GetNomenklsSearch(string searchText,
            bool isDeleted = false)
        {
            var ret = new List<KursDomain.References.Nomenkl>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var n in ctx.SD_83.Where(_ => (_.NOM_NAME.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_NOMENKL.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_FULL_NAME.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_NOTES.ToUpper().Contains(searchText.ToUpper())) &&
                                                       _.NOM_DELETED == (isDeleted ? 1 : 0)))
                    ret.Add(new KursDomain.References.Nomenkl
                    {
                        DocCode = n.DOC_CODE,
                        Name = n.NOM_NAME,
                        FullName = n.NOM_FULL_NAME,
                        Notes = n.NOM_NOTES,
                        NomenklNumber = n.NOM_NOMENKL,
                        Group = GlobalOptions.ReferencesCache.GetNomenklGroup(n.NOM_CATEG_DC),
                        // ReSharper disable once PossibleInvalidOperationException
                        Currency = GlobalOptions.ReferencesCache.GetCurrency(n.NOM_SALE_CRS_DC) as Currency
                    });
            }

            return ret;
        }

        public static NomenklGroupViewModel CategoryAdd(NameNoteViewModel cat, decimal? parentDC)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = new TransactionScope())
                {
                    try
                    {
                        var newDC = ctx.SD_82.Any() ? ctx.SD_82.Max(_ => _.DOC_CODE) + 1 : 10820000001;
                        var newCat = new SD_82
                        {
                            DOC_CODE = newDC,
                            CAT_NAME = cat.Name,
                            CAT_PATH_NAME = cat.Name,
                            CAT_PARENT_DC = parentDC
                        };
                        ctx.SD_82.Add(newCat);
                        ctx.SaveChanges();
                        tnx.Complete();
                        // ReSharper disable once InconsistentNaming
                        var newCatVM = new NomenklGroupViewModel(newCat)
                        {
                            State = RowStatus.NotEdited
                        };
                        return newCatVM;
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                        return null;
                    }
                }
            }
        }
    }
}
