using System;
using System.Diagnostics;
using System.Linq;
using KursDomain.References;
using KursDomain.Tests.Fakes;
using Xunit;
using Xunit.Abstractions;

namespace KursDomain.Tests.ReferenceCache;

public class ReferencesCacheTests
{
    private readonly ITestOutputHelper myTestOutputHelper;

    public ReferencesCacheTests(ITestOutputHelper testOutputHelper)
    {
        myTestOutputHelper = testOutputHelper;
    }

    [Fact]
    public void LoadCacheTest()
    {
        var context = new LocalKursDBContext();
        var cache = new ReferencesKursCache(context.Context);
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        cache.StartLoad();
        stopWatch.Stop();

        TimeSpan ts = stopWatch.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        myTestOutputHelper.WriteLine("RunTime " + elapsedTime);

        Assert.Equal(7, cache.GetCurrenciesAll().Count());
        Assert.Equal(240, cache.GetCountriesAll().Count());
        Assert.Equal(17, cache.GetRegionsAll().Count());
        Assert.Equal(12, cache.GetUnitsAll().Count());
        Assert.Equal(21, cache.GetCentrResponsibilitiesAll().Count());
        Assert.Equal(12, cache.GetUnitsAll().Count());
        Assert.Equal(13, cache.GetSDRStateAll().Count());
        Assert.Equal(36, cache.GetSDRSchetAll().Count());
        Assert.Equal(6, cache.GetClientCategoriesAll().Count());
        Assert.Equal(64, cache.GetKontragentCategoriesAll().Count());
        Assert.Equal(953, cache.GetNomenklGroupAll().Count());
        Assert.Equal(62, cache.GetEmployees().Count());
        Assert.Equal(34, cache.GetWarehousesAll().Count());
        Assert.Equal(4, cache.GetCashBoxAll().Count());
        Assert.Equal(6, cache.GetMutualSettlementTypeAll().Count());
        Assert.Equal(8, cache.GetProjectsAll().Count());
        Assert.Equal(8, cache.GetContractsTypeAll().Count());
        Assert.Equal(405, cache.GetBanksAll().Count());
        Assert.Equal(27, cache.GetBankAccountAll().Count());

        Assert.Equal(1172, cache.GetKontragentsAll().Count());
        Assert.Equal(27352, cache.GetNomenklsAll().Count());
    }
    
}
