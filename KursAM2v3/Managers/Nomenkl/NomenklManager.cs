using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Calculates.Materials;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Helper;
using KursAM2.View.Base;
using KursAM2.View.DialogUserControl.ViewModel;
using KursAM2.ViewModel.Logistiks;

namespace KursAM2.Managers.Nomenkl
{
    public class NomenklManager
    {
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

        public static List<NomenklTransferRowViewModelExt> SelectNomenklsWithRemainsDialog(Warehouse warehouse)
        {
            var ctxTransf = new NomTransferAddForSklad(warehouse);
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
                    SummaOut = row.Quantity * row.Price,
                    NakladEdSumma = row.PriceWithNaklad - row.Price,
                    NakladRate = 0

                    //Parent = Document
                }).ToList();
            return ret;
        }

        public static List<NomenklTransferRowViewModelExt> SelectNomenklsWithRemainsDialog(Warehouse warehouse,
            DateTime date)
        {
            var ctxTransf = new NomTransferAddForSklad(warehouse, date);
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
                    SummaOut = row.Quantity * row.Price,
                    NakladEdSumma = row.PriceWithNaklad - row.Price,
                    NakladRate = 0

                    //Parent = Document
                }).ToList();
            return ret;
        }

        public static List<Core.EntityViewModel.Nomenkl> GetNomenklsSearch(string searchText, bool isDeleted = false)
        {
            var ret = new List<Core.EntityViewModel.Nomenkl>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var n in ctx.SD_83.Where(_ => (_.NOM_NAME.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_NOMENKL.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_FULL_NAME.ToUpper().Contains(searchText.ToUpper())
                                                        || _.NOM_NOTES.ToUpper().Contains(searchText.ToUpper())) &&
                                                       _.NOM_DELETED == (isDeleted ? 1 : 0)))
                    ret.Add(new Core.EntityViewModel.Nomenkl
                    {
                        DocCode = n.DOC_CODE,
                        Name = n.NOM_NAME,
                        NameFull = n.NOM_FULL_NAME,
                        Note = n.NOM_NOTES,
                        NomenklNumber = n.NOM_NOMENKL,
                        Group = MainReferences.NomenklGroups[n.NOM_CATEG_DC],
                        // ReSharper disable once PossibleInvalidOperationException
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

        public static NomenklGroup CategoryAdd(NameNoteViewModel cat, decimal? parentDC)
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
                        if (!MainReferences.NomenklGroups.ContainsKey(newDC))
                        {
                            MainReferences.NomenklGroups.Add(newDC, new NomenklGroup
                            {
                                DocCode = newDC,
                                Name = newCat.CAT_NAME,
                                ParentDC = newCat.CAT_PARENT_DC
                            });
                        }

                        // ReSharper disable once InconsistentNaming
                        var newCatVM = new NomenklGroup(newCat)
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

        public static void RecalcPrice()
        {
            var sql = "DECLARE @NomDC NUMERIC(15, 0); " +
                      " DECLARE NomenklList CURSOR FOR " +
                      " SELECT DISTINCT NOM_DC " +
                      " FROM NOMENKL_RECALC; " +
                      " OPEN NomenklList " +
                      " FETCH NEXT FROM NomenklList INTO @NomDC " +
                      " WHILE @@fetch_status = 0 " +
                      " BEGIN " +
                      " EXEC dbo.NomenklCalculateCostsForOne @NomDC " +
                      " FETCH NEXT FROM NomenklList INTO @NomDC " +
                      " END" +
                      " CLOSE NomenklList; " +
                      " DEALLOCATE NomenklList; " +
                      " DELETE FROM WD_27 " +
                      " INSERT INTO dbo.WD_27 (DOC_CODE, SKLW_NOMENKL_DC, SKLW_DATE, SKLW_KOLICH) " +
                      " SELECT n.StoreDC, n.nomdc, n.DATE, SUM(n.prihod) - SUM(n.rashod) Quantity " +
                      " FROM NomenklMoveWithPrice n " +
                      " WHERE n.DATE = (SELECT MAX(n1.DATE) " +
                      " FROM NomenklMoveWithPrice n1 " +
                      " WHERE n1.nomdc = n.nomdc AND n.storedc = n1.storedc) " +
                      " GROUP BY n.nomdc, n.StoreDc, n.DATE " +
                      " HAVING SUM(n.prihod) - SUM(n.rashod) > 0; " +
                      " DELETE FROM NOMENKL_RECALC;";
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        ctx.Database.ExecuteSqlCommand(sql);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        if (transaction.UnderlyingTransaction.Connection != null)
                            transaction.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public static void RecalcPrice(List<decimal> nomdcs, ALFAMEDIAEntities ent = null )
        {
            if (ent != null)
            {
                try
                {
                    foreach (var n in nomdcs)
                    {
                        ent.Database.ExecuteSqlCommand($"INSERT INTO NOMENKL_RECALC (NOM_DC, OPER_DATE) " +
                                                       $"VALUES ({CustomFormat.DecimalToSqlDecimal(n)}, '20000101');");
                    }
                    
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    using (var transaction = ctx.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var n in nomdcs)
                            {
                                ctx.Database.ExecuteSqlCommand($"INSERT INTO NOMENKL_RECALC (NOM_DC, OPER_DATE) " +
                                                               $"VALUES ({CustomFormat.DecimalToSqlDecimal(n)}, '20000101');");
                            }

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            if (transaction.UnderlyingTransaction.Connection != null)
                                transaction.Rollback();
                            WindowManager.ShowError(ex);
                        }
                    }

                    RecalcPrice();
                }
            }
        }
    }
}