using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using Helper;
using KursDomain.References;
using System.Data.Entity;
using System.Diagnostics;
using KursDomain.References.RedisCache;
using KursDomain.Repository.Base;
using ServiceStack.Redis;

namespace KursDomain.Repository.NomenklRepository;

[DebuggerDisplay("{NomDC} - {NomNomenkl} Приход: {Prihod} Расход: {Rashod}")]
public class NomenklMoveInfo
{
    public decimal NomDC { set; get; }
    public string NomNomenkl { set; get; }
    public string NomName { set; get; }
    public decimal Prihod { set; get; }
    public decimal PrihodSumma { set; get; }
    public decimal PrihodNaklSumma { set; get; }
    public decimal Rashod { set; get; }
    public decimal RashodSumma { set; get; }
    public decimal RashodNaklSumma { set; get; }
}

[DebuggerDisplay("{NomDC} - {NomNomenkl} Остаток: {OstatokQuantity} Приход: {Prihod} Расход: {Rashod}")]
public class NomenklQuantityInfo
{
    public decimal NomDC { set; get; }
    public string NomNomenkl { set; get; }
    public string NomName { set; get; }
    public decimal StartQuantity { set; get; }
    public decimal StartSumma { set; get; }
    public decimal StartNaklSumma { set; get; }
    public decimal PrihodStart { set; get; }
    public decimal RashodStart { set; get; }
    public decimal Prihod { set; get; }
    public decimal Rashod { set; get; }
    public decimal OstatokQuantity { set; get; }
    public decimal OstatokSumma { set; get; }
    public decimal OstatokNaklSumma { set; get; }
    public decimal Reserved { set; get; }

}

[DebuggerDisplay("{NomDC} - {NomNomenkl} Остаток: {OstatokQuantity} Приход: {Prihod} Расход: {Rashod}")]
public class NomenklQuantityInfoExt : NomenklQuantityInfo
{
    public NomenklQuantityInfoExt(NomenklQuantityInfo item)
    {
        NomDC = item.NomDC;
        NomNomenkl = item.NomNomenkl;
        NomName = item.NomName;
        StartQuantity = item.StartQuantity;
        StartSumma = item.StartSumma;
        StartNaklSumma = item.StartNaklSumma;
        PrihodStart = item.PrihodStart;
        RashodStart = item.RashodStart;
        Prihod = item.Prihod;
        Rashod = item.Rashod;
        OstatokQuantity = item.OstatokQuantity;
        OstatokSumma = item.OstatokSumma;
        OstatokNaklSumma = item.OstatokNaklSumma;
        Nomenkl = (Nomenkl)GlobalOptions.ReferencesCache.GetNomenkl(item.NomDC);
    }

    public KursDomain.References.Nomenkl Nomenkl { set; get; }
}

public class NomenklRepository : KursGenericRepository<SD_83, ALFAMEDIAEntities, decimal>,
    INomenklRepository
{
    private readonly RedisManagerPool redisManager =
        new RedisManagerPool(ConfigurationManager.AppSettings["redis.connection"]);

    public NomenklRepository(ALFAMEDIAEntities context) : base(context)
    {
    }

    private readonly DateTime startDate = new DateTime(2000, 1, 1);
    private readonly string sql = "DECLARE @NomDC NUMERIC(15, 0); " +
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

    public async Task RecalcPriceAsync()
    {
        await Context.Database.ExecuteSqlCommandAsync(sql);

    }

    public async Task RecalcPriceAsync(List<decimal> docCodes)
    {
            List<NOMENKL_RECALC> rows = docCodes
                .Select(dc => new NOMENKL_RECALC() { NOM_DC = dc, OPER_DATE = startDate })
                .ToList();
            foreach (var r in from r in rows
                     let old = Context.Set<NOMENKL_RECALC>().FirstOrDefault(_ => _.NOM_DC == r.NOM_DC)
                     where old == null
                     select r)
            {
                await Context.Database.ExecuteSqlCommandAsync($"INSERT INTO dbo.NOMENKL_RECALC(NOM_DC, OPER_DATE) " +
                                                              $"VALUES ({CustomFormat.DecimalToSqlDecimal(r.NOM_DC)}," +
                                                              $"'{CustomFormat.DateToString(r.OPER_DATE)}');");
            }

            await SaveAsync();
            await Context.Database.ExecuteSqlCommandAsync(sql);
    }

    public void RecalcPrice(List<decimal> docCodes)
    {
        List<NOMENKL_RECALC> rows = docCodes
            .Select(dc => new NOMENKL_RECALC() { NOM_DC = dc, OPER_DATE = startDate })
            .ToList();
        foreach (var r in from r in rows
                 let old = Context.Set<NOMENKL_RECALC>().FirstOrDefault(_ => _.NOM_DC == r.NOM_DC)
                 where old == null
                 select r)
        {
            Context.Database.ExecuteSqlCommand($"INSERT INTO dbo.NOMENKL_RECALC(NOM_DC, OPER_DATE) " +
                                               $"VALUES ({CustomFormat.DecimalToSqlDecimal(r.NOM_DC)}," +
                                               $"'{CustomFormat.DateToString(r.OPER_DATE)}');");
        }

        Context.SaveChanges();
        Context.Database.ExecuteSqlCommandAsync(sql);
    }

    public async Task<List<NomenklQuantityInfo>> GetNomenklQuantityAsync(decimal skladDC, decimal nomDC, DateTime dateStart, DateTime dateEnd)
    {
        {
            try
            {
                return await Context.Database.SqlQuery<NomenklQuantityInfo>(
                    $"EXEC GetNomenklQuantityInfo {CustomFormat.DecimalToSqlDecimal(skladDC)},{CustomFormat.DecimalToSqlDecimal(nomDC)}," +
                    $"'{CustomFormat.DateToString(dateStart)}','{CustomFormat.DateToString(dateEnd)}'").ToListAsync();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                throw;
            }
        }
    }

  
    public IEnumerable<Nomenkl> FindByName(string name)
    {
        var n = name.ToLower();
        using (var ctx = GlobalOptions.GetEntities())
        {
            var data = ctx.SD_83.AsNoTracking()
                .Where(_ => (_.NOM_NOMENKL + _.NOM_FULL_NAME + _.NOM_POLNOE_IMIA + _.NOM_NOTES).ToLower().Contains(n.ToLower())).ToList();
           
            if (data.Select(_ => _.DOC_CODE).Any())
                ((RedisCacheReferences)GlobalOptions.ReferencesCache).UpdateList2(data.Select(_ => _.DOC_CODE).ToList());
            var r = GlobalOptions.ReferencesCache.GetNomenkl(data.Select(_ => _.DOC_CODE));
            return GlobalOptions.ReferencesCache.GetNomenkl(data.Select(_ => _.DOC_CODE)).Cast<Nomenkl>();
        }
    }

    private void updateNomenklCache(List<decimal> list)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            foreach (var item in ctx.SD_83.Include(_ => _.NomenklMain).AsNoTracking().ToList().Select(entity =>
                         new Nomenkl
                         {
                             DocCode = entity.DOC_CODE,
                             Id = entity.Id,
                             Name = entity.NOM_NAME,
                             FullName =
                                 entity.NOM_FULL_NAME,
                             Notes = entity.NOM_NOTES,
                             IsUsluga =
                                 entity.NOM_0MATER_1USLUGA == 1,
                             IsProguct = entity.NOM_1PROD_0MATER == 1,
                             IsNakladExpense =
                                 entity.NOM_1NAKLRASH_0NO == 1,
                             DefaultNDSPercent = (decimal?)entity.NOM_NDS_PERCENT,
                             IsDeleted =
                                 entity.NOM_DELETED == 1,
                             IsUslugaInRentabelnost =
                                 entity.IsUslugaInRent ?? false,
                             UpdateDate =
                                 entity.UpdateDate ?? DateTime.MinValue,
                             MainId =
                                 entity.MainId ?? Guid.Empty,
                             IsCurrencyTransfer = entity.NomenklMain.IsCurrencyTransfer ?? false,
                             NomenklNumber =
                                 entity.NOM_NOMENKL,
                             NomenklTypeDC =
                                 entity.NomenklMain.TypeDC,
                             ProductTypeDC = entity.NomenklMain.ProductDC,
                             UnitDC = entity.NOM_ED_IZM_DC,
                             CurrencyDC = entity.NOM_SALE_CRS_DC,
                             GroupDC = entity.NOM_CATEG_DC
                         }))
            {
                GlobalOptions.ReferencesCache.AddOrUpdate(item);
            }
        }
    }

    public IEnumerable<Nomenkl> GetByGroupDC(decimal groupDC)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var dc_list = ctx.SD_83.Where(_ => _.NOM_CATEG_DC == groupDC).Select(x => x.DOC_CODE).ToList();
            return GlobalOptions.ReferencesCache.GetNomenkl(dc_list).Cast<Nomenkl>();
        }
    }

    public IEnumerable<Nomenkl> GetUsluga()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var dc_list = ctx.SD_83.Where(_ => _.NOM_0MATER_1USLUGA == 1).Select(x => x.DOC_CODE).ToList();
            return GlobalOptions.ReferencesCache.GetNomenkl(dc_list).Cast<Nomenkl>();
        }
    }
}
