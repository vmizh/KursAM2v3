using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Helper;
using KursDomain.Annotations;
using KursDomain.Documents.Currency;
using KursDomain.Repository.NomenklRepository;
using KursDomain.WindowsManager.WindowsManager;

namespace KursDomain.Managers
{
    public class NomenklManager2
    {
        [NotNull] private readonly ALFAMEDIAEntities Context;

        public NomenklManager2(ALFAMEDIAEntities context)
        {
            Context = context ?? GlobalOptions.GetEntities();
        }

        //TODO Доделать расчет остатков и цен по спискуц номенклатур ***

        public void RecalcPrice(List<decimal> nomDCs, ALFAMEDIAEntities context = null)
        {
            var strDC = new StringBuilder("(");
            foreach (var dc in nomDCs)
            {
                if (strDC.Length == 1)
                    strDC.Append(dc);
                else strDC.Append($",{dc}");
            }
            strDC.Append(")");

        }

        //EXEC dbo.NomenklCalculateCostsForOne @NomDC

        public void RecalcPrice(ALFAMEDIAEntities context = null)
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
            if (context != null)
                context.Database.ExecuteSqlCommand(sql);
            else

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

        public void RecalcPrice(List<decimal> nomdcs)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var n in nomdcs)
                            ctx.Database.ExecuteSqlCommand("INSERT INTO NOMENKL_RECALC (NOM_DC, OPER_DATE) " +
                                                           $"VALUES ({CustomFormat.DecimalToSqlDecimal(n)}, '20000101');");

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

        public Prices GetNomenklPrice(decimal nomdc, DateTime date)
        {
            var sql = "SELECT np.PRICE_WO_NAKLAD as Price, np.PRICE as PriceWithNaklad FROM nom_price np " +
                      $"WHERE np.nom_dc = {CustomFormat.DecimalToSqlDecimal(nomdc)} " +
                      "AND Date = (select MAX(np1.Date) " +
                      $"FROM NOM_PRICE np1 WHERE np1.NOM_DC = np.NOM_DC AND np1.DATE <= '{CustomFormat.DateToString(date)}')";
            var data1 = Context.Database.SqlQuery<Prices>(sql).ToList();
            if (data1.Count == 0)
                return new Prices
                {
                    Price = 0,
                    PriceWithNaklad = 0
                };
            return data1.First();
        }

        public List<NomenklQuantityInfo> GetNomenklQuantity(decimal skladDC, decimal nomDC, DateTime dateStart,
            DateTime dateEnd)
        {
            try
            {
                return Context.Database.SqlQuery<NomenklQuantityInfo>(
                    $"EXEC GetNomenklQuantityInfo {CustomFormat.DecimalToSqlDecimal(skladDC)},{CustomFormat.DecimalToSqlDecimal(nomDC)}," +
                    $"'{CustomFormat.DateToString(dateStart)}','{CustomFormat.DateToString(dateEnd)}'").ToList();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                return null;
            }
        }

        public List<NomenklQuantityInfo> GetNomenklStoreQuantity(decimal skladDC, DateTime dateStart,
            DateTime dateEnd, bool isZeroInclude = false)
        {
            try
            {
                var sql = $"EXEC GetNomenklAllQuantityInfo {CustomFormat.DecimalToSqlDecimal(skladDC)}," +
                          $"'{CustomFormat.DateToString(dateStart)}','{CustomFormat.DateToString(dateEnd)}'";
                var ret = Context.Database.SqlQuery<NomenklQuantityInfo>(sql)
                    .ToList();
                return isZeroInclude ? ret : ret.Where(_ => _.OstatokQuantity > 0).ToList();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                return null;
            }
        }

        public async Task<List<NomenklQuantityInfo>> GetNomenklStoreQuantityAsync(decimal skladDC, DateTime dateStart,
            DateTime dateEnd)
        {
            try
            {
                var sql = $"EXEC GetNomenklAllQuantityInfo {CustomFormat.DecimalToSqlDecimal(skladDC)}," +
                          $"'{CustomFormat.DateToString(dateStart)}','{CustomFormat.DateToString(dateEnd)}'";
                using (var ctx = GlobalOptions.GetEntities())
                {
                    return await ctx.Database.SqlQuery<NomenklQuantityInfo>(sql)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                return null;
            }
        }

        public async Task<List<NomenklMoveInfo>> GetNomenklStoreMoveAsync(decimal skladDC, DateTime dateStart,
            DateTime dateEnd)
        {
            try
            {
                var sql = $"EXEC GetNomenklAllMove {CustomFormat.DecimalToSqlDecimal(skladDC)}," +
                          $"'{CustomFormat.DateToString(dateStart)}','{CustomFormat.DateToString(dateEnd)}'";
                using (var ctx = GlobalOptions.GetEntities())
                {
                    return await ctx.Database.SqlQuery<NomenklMoveInfo>(sql)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                return null;
            }
        }

        public List<NomenklMoveInfo> GetNomenklStoreMove(decimal skladDC, DateTime dateStart,
            DateTime dateEnd)
        {
            try
            {
                var sql = $"EXEC GetNomenklAllMove {CustomFormat.DecimalToSqlDecimal(skladDC)}," +
                          $"'{CustomFormat.DateToString(dateStart)}','{CustomFormat.DateToString(dateEnd)}'";
                return Context.Database.SqlQuery<NomenklMoveInfo>(sql)
                    .ToList();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                return null;
            }
        }
    }
}
