using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Core.WindowsManager;
using Helper;
using KursAM2.RedisRepositories;
using KursAM2.Repositories.RedisRepository;
using KursDomain;
using KursDomain.References.RedisCache;
using RedisProvider;

public class InvoiceDataCacheRepository : IInvoiceDataCacheRepository
{
    private readonly RedisConnection myRedisConnection =
        new RedisConnection(ConfigurationManager.AppSettings["redis.connection"]);

    private readonly RedisContainer myRedisContainer;

    public InvoiceDataCacheRepository()
    {
        myRedisContainer = new RedisContainer(myRedisConnection, GlobalOptions.RedisDBId ?? -1, "DataCache");
    }

    public IEnumerable<InvoicePaymentShipment> GetInvoiceClientPartialPayment(
        DateTime? dateStart = null,
        DateTime? dateEnd = null)
    {
        var dataSet = new RedisSet<InvoicePaymentShipment>(RedisDataCacheSets.InvoiceClientPayment);
        myRedisContainer.AddToContainer(dataSet);
        var d = dataSet.ToList().Result;
        return d.Where(item =>
                item.DocDate >= (dateStart ?? DateTime.MinValue) && item.DocDate <= (dateEnd ?? DateTime.MaxValue)
                && item.Payment < item.Summa)
            .ToList();
    }

    public IEnumerable<InvoicePaymentShipment> GetInvoiceClientPartialShipment(DateTime? dateStart = null,
        DateTime? dateEnd = null)
    {
        var dataSet = new RedisSet<InvoicePaymentShipment>(RedisDataCacheSets.InvoiceClientPayment);
        myRedisContainer.AddToContainer(dataSet);
        var d = dataSet.ToList().Result;
        return d.Where(item =>
                item.DocDate >= (dateStart ?? DateTime.MinValue) && item.DocDate <= (dateEnd ?? DateTime.MaxValue)
                                                                 && item.Shipment < item.Summa)
            .ToList();
    }

    public IEnumerable<InvoicePaymentShipment> GetInvoiceProviderPartialPayment(DateTime? dateStart = null,
        DateTime? dateEnd = null)
    {
        var dataSet = new RedisSet<InvoicePaymentShipment>(RedisDataCacheSets.InvoiceProviderPayment);
        myRedisContainer.AddToContainer(dataSet);
        var d = dataSet.ToList().Result;
        return d.Where(item =>
                item.DocDate >= (dateStart ?? DateTime.MinValue) && item.DocDate <= (dateEnd ?? DateTime.MaxValue)
                                                                 && item.Payment < item.Summa)
            .ToList();
    }

    public IEnumerable<InvoicePaymentShipment> GetInvoiceProviderPartialShipment(DateTime? dateStart = null,
        DateTime? dateEnd = null)
    {
        var dataSet = new RedisSet<InvoicePaymentShipment>(RedisDataCacheSets.InvoiceProviderPayment);
        myRedisContainer.AddToContainer(dataSet);
        var d = dataSet.ToList().Result;
        return d.Where(item =>
                item.DocDate >= (dateStart ?? DateTime.MinValue) && item.DocDate <= (dateEnd ?? DateTime.MaxValue)
                                                                 && item.Shipment < item.Summa)
            .ToList();
    }

    public async Task UpdateInvoiceClientPaymentShipmentAsync(decimal invoiceDC)
    {
        var sql = @$"SELECT DISTINCT
                        t.DocCode          AS DocCode,
                        t.DocDate          AS DocDate,
                        t.Summa            AS Summa,
                        SUM(t.PaySumma)    AS Payment,
                        SUM(t.SummaOtgruz) AS Shipment
                    FROM InvoiceClientQuery t
                    WHERE t.DocCode = {CustomFormat.DecimalToSqlDecimal(invoiceDC)}
                    GROUP BY t.DocCode, t.DocDate, t.Summa";
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = await ctx.Database.SqlQuery<InvoicePaymentShipment>(sql).FirstOrDefaultAsync();
                if (data is null) return;
                var dataSet = new RedisSet<InvoicePaymentShipment>(RedisDataCacheSets.InvoiceClientPayment);
                myRedisContainer.AddToContainer(dataSet);
                if (data.Summa == data.Payment && data.Summa == data.Shipment)
                    foreach (var d in dataSet.Cast<InvoicePaymentShipment>())
                    {
                        if (d.DocCode != data.DocCode) continue;
                        await dataSet.Remove(d);
                        return;
                    }
                else
                    await dataSet.Add(data);
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public async Task UpdateInvoiceProviderPaymentShipmentAsync(decimal invoiceDC)
    {
        var sql = @$"SELECT DISTINCT
                        t.DocCode          AS DocCode,
                        t.Date             AS DocDate,
                        t.Summa            AS Summa,
                        SUM(t.PaySumma)    AS Payment,
                        SUM(t.ShippedSumma) AS Shipment
                    FROM InvoicePostQuery t
                    WHERE t.DocCode = {CustomFormat.DecimalToSqlDecimal(invoiceDC)}
                    GROUP BY t.DocCode, t.Date, t.Summa";
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = await ctx.Database.SqlQuery<InvoicePaymentShipment>(sql).FirstOrDefaultAsync();
                if (data is null) return;
                var dataSet = new RedisSet<InvoicePaymentShipment>(RedisDataCacheSets.InvoiceProviderPayment);
                myRedisContainer.AddToContainer(dataSet);
                if (data.Summa == data.Payment && data.Summa == data.Shipment)
                    foreach (var d in dataSet.Cast<InvoicePaymentShipment>())
                    {
                        if (d.DocCode != data.DocCode) continue;
                        await dataSet.Remove(d);
                        return;
                    }
                else
                    await dataSet.Add(data);
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public Task UpdateInvoiceClientPaymentShipmentAsync(IEnumerable<decimal> invoiceDCList)
    {
        throw new NotImplementedException();
    }

    public Task UpdateInvoiceProviderPaymentShipmentAsync(IEnumerable<decimal> invoiceDCList)
    {
        throw new NotImplementedException();
    }

    public async Task ResetInvoiceProviderPaymentShipmentAsync()
    {
        var sql = @"SELECT DISTINCT
                        t.DocCode          AS DocCode,
                        T.Date             AS DocDate,
                        t.Summa            AS Summa,
                        SUM(t.PaySumma)    AS Payment,
                        SUM(t.ShippedSumma) AS Shipment
                    FROM InvoicePostQuery t
                    GROUP BY t.DocCode, t.Date, t.Summa
                    HAVING SUM(t.ShippedSumma) < t.Summa OR SUM(t.PaySumma) < t.Summa";
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = await ctx.Database.SqlQuery<InvoicePaymentShipment>(sql).ToListAsync();
                var dataSet = new RedisSet<InvoicePaymentShipment>(RedisDataCacheSets.InvoiceProviderPayment);
                myRedisContainer.AddToContainer(dataSet);
                foreach (var d in data) d.DocType = PaymentShipmentInvoiceTypeEnum.Provider;
                await dataSet.AddRange(data);
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public async Task ResetInvoiceClientPaymentShipmentAsync()
    {
        var sql = @"SELECT DISTINCT
                        t.DocCode          AS DocCode,
                        t.DocDate          AS DocDate,
                        t.Summa            AS Summa,
                        SUM(t.PaySumma)    AS Payment,
                        SUM(t.SummaOtgruz) AS Shipment
                    FROM InvoiceClientQuery t
                    GROUP BY t.DocCode, t.DocDate, t.Summa
                    HAVING SUM(t.SummaOtgruz) < t.Summa OR SUM(t.PaySumma) < t.Summa";
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = await ctx.Database.SqlQuery<InvoicePaymentShipment>(sql).ToListAsync();
                var dataSet = new RedisSet<InvoicePaymentShipment>(RedisDataCacheSets.InvoiceClientPayment);
                myRedisContainer.AddToContainer(dataSet);
                foreach (var d in data) d.DocType = PaymentShipmentInvoiceTypeEnum.Client;
                await dataSet.AddRange(data);
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }
}
