using System.Linq;
using Data;
using KursDomain.ICommon;
using KursDomain.References;
using KursDomain.Tests.Fakes;
using Xunit;

namespace KursDomain.Tests.ReferenceCache;

public class CashBoxCacheTests
{
    //private readonly ITestOutputHelper myTestOutputHelper;

    //public CashBoxCacheTests(ITestOutputHelper testOutputHelper)
    //{
    //    myTestOutputHelper = testOutputHelper;
    //}

    [Fact]
    public void CashBoxCacheTest()
    {
        var context = new LocalKursDBContext();
        var cache = new ReferencesKursCache(context.Context);
        cache.StartLoad();
        var cashBoxcount = context.Context.Database.SqlQuery<int>("SELECT COUNT(*) FROM SD_22").First();
        Assert.Equal(cashBoxcount, cache.GetCashBoxAll().Count());

        var dc = context.Context.SD_22.Max(_ => _.DOC_CODE);
        dc = dc == 0 ? 10220000001 : dc + 1;
        var newCashBox = new SD_22
        {
            DOC_CODE = dc,
            CA_NAME = "Test 1",
            CA_CRS_DC = ((IDocCode) cache.GetCurrenciesAll().First()).DocCode,
            CA_NEGATIVE_RESTS = 0
        };
        context.Context.SD_22.Add(newCashBox);
        context.Context.SaveChanges();
        Assert.Equal(cashBoxcount + 1, cache.GetCashBoxAll().Count());
        var cb = context.Context.SD_22.FirstOrDefault(_ => _.DOC_CODE == dc);
        if (cb != null)
        {
            context.Context.SD_22.Remove(cb);
            context.Context.SaveChanges();
        }

        Assert.Equal(cashBoxcount, cache.GetCashBoxAll().Count());
        var cacheBox = cache.GetCashBoxAll().First();
        var oldItem = context.Context.SD_22.First(_ => _.DOC_CODE == ((IDocCode) cacheBox).DocCode);
        var oldName = ((IName) cacheBox).Name;
        oldItem.CA_NAME = "Новое название";
        context.Context.SaveChanges();
        var cacheBox2 = cache.GetCashBox(((IDocCode) cacheBox).DocCode);
        Assert.Equal(((IName) cacheBox2).Name, "Новое название");
        oldItem = context.Context.SD_22.First(_ => _.DOC_CODE == ((IDocCode) cacheBox).DocCode);
        oldItem.CA_NAME = oldName;
        context.Context.SaveChanges();
    }
}
