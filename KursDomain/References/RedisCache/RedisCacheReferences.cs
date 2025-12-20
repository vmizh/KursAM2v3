using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Helper.Extensions;
using KursAM2.Repositories.RedisRepository;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.IReferences.Nomenkl;
using KursDomain.WindowsManager.WindowsManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceStack.Redis;
using ServiceStack.Text;
using ServiceStack.Text.Common;
using StackExchange.Redis;

namespace KursDomain.References.RedisCache;

public interface ICache
{
    DateTime UpdateDate { set; get; }
    void LoadFromCache();
}

internal static class PagingUtils
{
    public static IEnumerable<T> Page<T>(this IEnumerable<T> en, int pageSize, int page)
    {
        return en.Skip(page * pageSize).Take(pageSize);
    }

    public static IQueryable<T> Page<T>(this IQueryable<T> en, int pageSize, int page)
    {
        return en.Skip(page * pageSize).Take(pageSize);
    }
}

//public record CachKey
//{
//    public decimal? DocCode { get; set; }
//    public int? TabelNumber { set; get; }
//    public Guid? Id { get; set; }
//    public DateTime LastUpdate { set; get; }

//    public string Key { set; get; }
//}

//public class CacheKeys
//{
//    public DateTime LoadMoment { set; get; }
//    public HashSet<CachKey> CachKeys { get; set; } = new();
//}

[SuppressMessage("ReSharper", "RedundantDictionaryContainsKeyBeforeAdding")]
public class RedisCacheReferences : IReferencesCache
{
    public const int MaxTimersSec = 10800; // 3 часа
    private readonly ISubscriber mySubscriber;
    private readonly ConnectionMultiplexer redis;

    //public readonly Dictionary<string, CacheKeys> cacheKeysDict = new();

    private readonly RedisManagerPool redisManager = new(ConfigurationManager.AppSettings["redis.connection"]);

    //public bool isNomenklCacheLoad = true;

    // ReSharper disable once MemberInitializerValueIgnored
    public bool isStartLoad = true;

    public RedisCacheReferences()
    {
        isStartLoad = true;
        redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
        mySubscriber = redis.GetSubscriber();

        ThreadPool.QueueUserWorkItem(_ =>
        {
            using (var redisClient = new RedisClient(ConfigurationManager.AppSettings["redis.connection"]))
            {
                using (var subscription = redisClient.CreateSubscription())
                {
                    subscription.OnSubscribe = channel => { Console.WriteLine($"Client #{channel} Subscribed"); };
                    subscription.OnUnSubscribe = channel => { Console.WriteLine($"Client #{channel} UnSubscribed"); };
                    subscription.OnMessage = UpdateReferences;

                    subscription.SubscribeToChannelsMatching("Cache:*");
                }
            }
        });
    }

    public bool IsChangeTrackingOn { get; set; }

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public DbContext DBContext { get; }

    public ICashBox GetCashBox(decimal? dc)
    {
        if (dc is null) return null;
        return GetCashBox(dc.Value);
    }

    public ICashBox GetCashBox(decimal dc)
    {
        if (!CashBoxes.ContainsKey(dc))
            GetCashBoxAll();
        return CashBoxes[dc];
    }

    public IEnumerable<ICashBox> GetCashBoxAll()
    {
        //using (var redisClient = redisManager.GetClient())
        //{
        //    var keys = redisClient.GetKeysByPattern("Cache:CashBox:*").ToList();
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        var cnt = ctx.SD_22.Count();
        //        if (keys.Count != cnt || cnt != CashBoxes.Count)
        //        {
        //            CashBoxes.Clear();
        //            var data = ctx.SD_22.ToList();
        //            foreach (var entity in data)
        //            {
        //                var newItem = new CashBox();
        //                newItem.LoadFromEntity(entity, this);
        //                AddOrUpdate(newItem);
        //                CashBoxes[newItem.DocCode] = newItem;
        //            }
        //        }
        //        else
        //        {
        //            var maxUpdate = CashBoxes.Values.Cast<ICache>().Max(_ => _.UpdateDate);
        //            var oldUpdate = ctx.SD_22.Max(_ => _.UpdateDate);
        //            if (!(oldUpdate > maxUpdate)) return CashBoxes.Values.ToList();
        //            using var pipe = redisClient.As<CashBox>().CreatePipeline();
        //            foreach (var key in keys)
        //                pipe.QueueCommand(redisTypedClient => redisTypedClient.GetValue(key),
        //                    x =>
        //                    {
        //                        x.LoadFromCache();
        //                        if (CashBoxes.ContainsKey(x.DocCode))
        //                            CashBoxes[x.DocCode] = x;
        //                        else
        //                            CashBoxes.Add(x.DocCode, x);
        //                    });

        //            pipe.Flush();
        //        }
        //    }
        //}

        //return CashBoxes.Values.ToList();
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = CashBoxes.Any() ? CashBoxes.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.SD_22.All(_ => _.UpdateDate <= mDate))
                return CashBoxes.Values.ToList();
            {
                var d = ctx.SD_22.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new CashBox();
                    newItem.LoadFromEntity(item,this);
                    CashBoxes.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }
        return CashBoxes.Values.ToList();
    }

    public IDeliveryCondition GetDeliveryCondition(decimal dc)
    {
        if (DeliveryConditions.ContainsKey(dc)) return DeliveryConditions[dc];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:DeliveryCondition:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<DeliveryCondition>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_103.FirstOrDefault(_ => _.DOC_CODE == dc);
                        if (ent is null) return null;
                        var newItem = new DeliveryCondition();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<DeliveryCondition>([newItem]));
                        DeliveryConditions.AddOrUpdate(dc, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    DeliveryConditions.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_103.FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (ent is null) return null;
                    var newItem = new DeliveryCondition();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<DeliveryCondition>([newItem]));
                    DeliveryConditions.AddOrUpdate(dc, newItem);
                }
            }
        }

        return DeliveryConditions[dc];
    }

    public IDeliveryCondition GetDeliveryCondition(decimal? dc)
    {
        return dc is not null ? GetDeliveryCondition(dc.Value) : null;
    }

    public IEnumerable<IDeliveryCondition> GetDeliveryConditionAll()
    {
        return GetAll<DeliveryCondition>().ToList();
    }

    public INomenklType GetNomenklType(decimal? dc)
    {
        if (dc is null) return null;
        if (NomenklTypes.ContainsKey(dc.Value)) return NomenklTypes[dc.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:NomenklType:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<NomenklType>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_119.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                        if (ent is null) return null;
                        var newItem = new NomenklType();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<NomenklType>(new[] { newItem }));
                        NomenklTypes.AddOrUpdate(dc.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    NomenklTypes.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_119.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                    if (ent is null) return null;
                    var newItem = new NomenklType();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<NomenklType>(new[] { newItem }));
                    NomenklTypes.AddOrUpdate(dc.Value, newItem);
                }
            }
        }

        return NomenklTypes[dc.Value];
    }

    public IEnumerable<INomenklType> GetNomenklTypeAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = NomenklTypes.Any()
                ? NomenklTypes.Values.Cast<ICache>().Max(_ => _.UpdateDate)
                : DateTime.MinValue;
            if (ctx.SD_119.All(_ => _.UpdateDate <= mDate))
                return NomenklTypes.Values.ToList();
            {
                var d = ctx.SD_119.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new NomenklType();
                    newItem.LoadFromEntity(item);
                    NomenklTypes.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return NomenklTypes.Values.ToList();
    }

    public INomenklProductType GetNomenklProductType(decimal? dc)
    {
        if (dc is null) return null;
        return GetNomenklProductType(dc.Value);
    }

    public INomenklProductType GetNomenklProductType(decimal dc)
    {
        //if (dc is null) return null;
        if (NomenklProductTypes.ContainsKey(dc)) return NomenklProductTypes[dc];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:NomenklProductType:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<NomenklProductType>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_77.FirstOrDefault(_ => _.DOC_CODE == dc);
                        if (ent is null) return null;
                        var newItem = new NomenklProductType();
                        newItem.LoadFromEntity(ent, this);
                        UpdateList(new List<NomenklProductType>(new[] { newItem }));
                        NomenklProductTypes.AddOrUpdate(newItem.DocCode, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    NomenklProductTypes.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_77.FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (ent is null) return null;
                    var newItem = new NomenklProductType();
                    newItem.LoadFromEntity(ent, this);
                    UpdateList(new List<NomenklProductType>(new[] { newItem }));
                    NomenklProductTypes.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return NomenklProductTypes[dc];
    }

    public IEnumerable<INomenklProductType> GetNomenklProductTypesAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = NomenklProductTypes.Any()
                ? NomenklProductTypes.Values.Cast<ICache>().Max(_ => _.UpdateDate)
                : DateTime.MinValue;
            if (ctx.SD_77.All(_ => _.UpdateDate <= mDate))
                return NomenklProductTypes.Values.ToList();
            {
                var d = ctx.SD_77.AsNoTracking().Where(_ =>
                        _.UpdateDate > mDate)
                    .ToList();
                foreach (var item in d)
                {
                    var newItem = new NomenklProductType();
                    newItem.LoadFromEntity(item, this);
                    NomenklProductTypes.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return NomenklProductTypes.Values.ToList();
    }

    public IProductType GetProductType(decimal? dc)
    {
        return dc is null ? null : GetProductType(dc.Value);
    }

    public IProductType GetProductType(decimal dc)
    {
        if (ProductTypes.ContainsKey(dc)) return ProductTypes[dc];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:ProductType:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<ProductType>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_50.FirstOrDefault(_ => _.DOC_CODE == dc);
                        if (ent is null) return null;
                        var newItem = new ProductType();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<ProductType>([newItem]));
                        ProductTypes.AddOrUpdate(newItem.DocCode, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    ProductTypes.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_50.FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (ent is null) return null;
                    var newItem = new ProductType();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<ProductType>(new[] { newItem }));
                    ProductTypes.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return ProductTypes[dc];
    }

    public IEnumerable<IProductType> GetProductTypeAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = ProductTypes.Any() ? ProductTypes.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.SD_50.All(_ => _.UpdateDate <= mDate))
                return ProductTypes.Values.ToList();
            {
                var d = ctx.SD_50.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new ProductType();
                    newItem.LoadFromEntity(item);
                    ProductTypes.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return ProductTypes.Values.ToList();
    }

    public ICentrResponsibility GetCentrResponsibility(decimal? dc)
    {
        if (dc is null) return null;
        if (CentrResponsibilities.ContainsKey(dc.Value)) return CentrResponsibilities[dc.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:CentrResponsibility:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<CentrResponsibility>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_40.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                        if (ent is null) return null;
                        var newItem = new CentrResponsibility();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<CentrResponsibility>([newItem]));
                        CentrResponsibilities.AddOrUpdate(dc.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    CentrResponsibilities.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_40.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                    if (ent is null) return null;
                    var newItem = new CentrResponsibility();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<CentrResponsibility>(new[] { newItem }));
                    CentrResponsibilities.AddOrUpdate(dc.Value, newItem);
                }
            }
        }

        return CentrResponsibilities[dc.Value];
    }

    public IEnumerable<ICentrResponsibility> GetCentrResponsibilitiesAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = CentrResponsibilities.Any()
                ? CentrResponsibilities.Values.Cast<ICache>().Max(_ => _.UpdateDate)
                : DateTime.MinValue;
            if (ctx.SD_40.All(_ => _.UpdateDate <= mDate))
                if (!ctx.SD_40.Any(_ =>
                        _.UpdateDate > mDate))
                    return CentrResponsibilities.Values.ToList();
            {
                var d = ctx.SD_40.AsNoTracking().Where(_ =>
                        _.UpdateDate > mDate)
                    .ToList();
                foreach (var item in d)
                {
                    var newItem = new CentrResponsibility();
                    newItem.LoadFromEntity(item);
                    CentrResponsibilities.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return CentrResponsibilities.Values.ToList();
    }

    public IBank GetBank(decimal? dc)
    {
        //if (dc is null) return null;

        //if (!cacheKeysDict.ContainsKey("Bank"))
        //    LoadCacheKeys("Bank");

        //var key = cacheKeysDict["Bank"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        //Bank itemNew;
        //if (key is not null)
        //{
        //    if (Banks.TryGetValue(dc.Value, out var Bank))
        //        return Bank;
        //    itemNew = GetItem<Bank>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        var entity = ctx.SD_44.FirstOrDefault(_ => _.DOC_CODE == dc);
        //        if (entity is null) return null;
        //        var newItem = new Bank
        //        {
        //            DocCode = entity.DOC_CODE
        //        };
        //        newItem.LoadFromEntity(entity);
        //        UpdateList(new List<Bank>(new[] { newItem }));
        //        Banks.AddOrUpdate(dc.Value, newItem);
        //        return Banks[dc.Value];
        //    }
        //}

        //if (Banks.ContainsKey(dc.Value))
        //    Banks[dc.Value] = itemNew;
        //else
        //    Banks.Add(dc.Value, itemNew);

        //return Banks[dc.Value];
        if (dc is null) return null;
        if (Banks.ContainsKey(dc.Value)) return Banks[dc.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:Bank:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<Bank>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_44.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                        if (ent is null) return null;
                        var newItem = new Bank();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<Bank>([newItem]));
                        Banks.AddOrUpdate(dc.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    Banks.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_44.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                    if (ent is null) return null;
                    var newItem = new Bank();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<Bank>(new[] { newItem }));
                    Banks.AddOrUpdate(dc.Value, newItem);
                }
            }
        }

        return Banks[dc.Value];
    }

    public IEnumerable<IBank> GetBanksAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = Banks.Any() ? Banks.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.SD_44.All(_ => _.UpdateDate <= mDate))
                return Banks.Values.ToList();
            {
                var d = ctx.SD_44.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new Bank();
                    newItem.LoadFromEntity(item);
                    Banks.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return Banks.Values.ToList();
    }

    public IBankAccount GetBankAccount(decimal? dc)
    {
        if (dc is null) return null;
        if (BankAccounts.ContainsKey(dc.Value)) return BankAccounts[dc.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:BankAccount:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<BankAccount>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_114.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                        if (ent is null) return null;
                        var newItem = new BankAccount();
                        newItem.LoadFromEntity(ent, this);
                        UpdateList(new List<BankAccount>([newItem]));
                        BankAccounts.AddOrUpdate(dc.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    BankAccounts.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_114.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                    if (ent is null) return null;
                    var newItem = new BankAccount();
                    newItem.LoadFromEntity(ent, this);
                    UpdateList(new List<BankAccount>([newItem]));
                    BankAccounts.AddOrUpdate(dc.Value, newItem);
                }
            }
        }

        return BankAccounts[dc.Value];
    }

    public IEnumerable<IBankAccount> GetBankAccountAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            if (BankAccounts.Count != ctx.SD_114.Count())
            {
                var d = ctx.SD_114.AsNoTracking().ToList();
                foreach (var item in d)
                {
                    var newItem = new BankAccount();
                    newItem.LoadFromEntity(item, this);
                    BankAccounts.AddOrUpdate(newItem.DocCode, newItem);
                }
                return BankAccounts.Values.ToList();
            }
            var mDate = BankAccounts.Any()
                ? BankAccounts.Values.Cast<ICache>().Max(_ => _.UpdateDate)
                : DateTime.MinValue;
            if (ctx.SD_114.All(_ => _.UpdateDate <= mDate))
                return BankAccounts.Values.ToList();
            if (!ctx.SD_114.Any(_ => _.UpdateDate > mDate))
            {
                var d = ctx.SD_114.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new BankAccount();
                    newItem.LoadFromEntity(item, this);
                    BankAccounts.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return BankAccounts.Values.ToList();
    }

    public IKontragentGroup GetKontragentGroup(int? id)
    {
        if (id is null) return null;
        if (KontragentGroups.ContainsKey(id.Value)) return KontragentGroups[id.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:KontragentGroup:{id}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<KontragentGroup>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.UD_43.FirstOrDefault(_ => _.EG_ID == id.Value);
                        if (ent is null) return null;
                        var newItem = new KontragentGroup();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<KontragentGroup>([newItem]));
                        KontragentGroups.AddOrUpdate(id.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    KontragentGroups.AddOrUpdate(id.Value, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.UD_43.FirstOrDefault(_ => _.EG_ID == id.Value);
                    if (ent is null) return null;
                    var newItem = new KontragentGroup();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<KontragentGroup>([newItem]));
                    KontragentGroups.AddOrUpdate(id.Value, newItem);
                }
            }
        }

        return KontragentGroups[id.Value];
    }

    public IEnumerable<IKontragentGroup> GetKontragentCategoriesAll()
    {
        
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = KontragentGroups.Any()
                ? KontragentGroups.Values.Cast<ICache>().Max(_ => _.UpdateDate)
                : DateTime.MinValue;
            if (ctx.UD_43.All(_ => _.UpdateDate <= mDate))
                if (!ctx.UD_43.Any(_ =>
                        _.UpdateDate > mDate))
                    return KontragentGroups.Values.ToList();
            {
                var d = ctx.UD_43.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new KontragentGroup();
                    newItem.LoadFromEntity(item);
                    KontragentGroups.AddOrUpdate(newItem.Id, newItem);
                }
            }
        }

        return KontragentGroups.Values.ToList();
    }

    public IKontragent GetKontragent(decimal? dc)
    {
        if (dc is null) return null;
        return GetKontragent(dc.Value);
    }

    public IKontragent GetKontragent(decimal dc)
    {
        //if (!cacheKeysDict.ContainsKey("Kontragent"))
        //    LoadCacheKeys("Kontragent");

        //var key = cacheKeysDict["Kontragent"].CachKeys.FirstOrDefault(_ => _.DocCode == dc);
        //Kontragent itemNew;
        //if (key is not null)
        //{
        //    if (Kontragents.TryGetValue(dc, out var Kontragent))
        //        return Kontragent;
        //    itemNew = GetItem<Kontragent>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        var entity = ctx.SD_43.Include(_ => _.SD_2).FirstOrDefault(_ => _.DOC_CODE == dc);
        //        if (entity is null) return null;
        //        var newItem = new Kontragent
        //        {
        //            DocCode = entity.DOC_CODE,
        //            StartBalans = entity.START_BALANS ?? new DateTime(2000, 1, 1),
        //            Name = entity.NAME,
        //            Notes = entity.NOTES,
        //            IsBalans = entity.FLAG_BALANS == 1,
        //            IsDeleted = entity.DELETED == 1,
        //            Id = entity.Id,
        //            CurrencyDC = entity.VALUTA_DC,
        //            GroupDC = entity.EG_ID,
        //            ResponsibleEmployeeDC = entity.SD_2?.DOC_CODE
        //        };
        //        newItem.LoadFromEntity(entity, this);
        //        UpdateList2(new List<Kontragent>(new[] { newItem }));
        //        Kontragents.AddOrUpdate(dc, newItem);
        //        return Kontragents[dc];
        //    }
        //}

        //Kontragents.AddOrUpdate(dc, itemNew);
        //return Kontragents[dc];

        if (Kontragents.ContainsKey(dc)) return Kontragents[dc];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:Kontragent:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<Kontragent>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_43.FirstOrDefault(_ => _.DOC_CODE == dc);
                        if (ent is null) return null;
                        var newItem = new Kontragent();
                        newItem.LoadFromEntity(ent, this);
                        UpdateList(new List<Kontragent>([newItem]));
                        Kontragents.AddOrUpdate(dc, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    Kontragents.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_43.FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (ent is null) return null;
                    var newItem = new Kontragent();
                    newItem.LoadFromEntity(ent, this);
                    UpdateList(new List<Kontragent>([newItem]));
                    Kontragents.AddOrUpdate(dc, newItem);
                }
            }
        }

        return Kontragents[dc];
    }

    public IKontragent GetKontragent(Guid? id)
    {
        if (id is null) return null;
        decimal dc;
        var old = Kontragents.Values.FirstOrDefault(_ => ((IDocGuid)_).Id == id.Value);
        if (old is null)
            using (var redisClient = redisManager.GetClient())
            {
                redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                var keys = redisClient.GetKeysByPattern($"*{id.Value}*");
                var enumerable = keys.ToList();
                if (!enumerable.Any()) return default;
                dc = Convert.ToDecimal(enumerable.First().Split('@')[0].Split(':')[2]);
            }
        else dc = ((IDocCode)old).DocCode;

        return GetKontragent(dc);
    }

    public IEnumerable<IKontragent> GetKontragentsAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            if (Kontragents.Count != ctx.SD_43.Count())
                using (var redisClient = redisManager.GetClient())
                {
                    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                    var redisLocal = redisClient.As<Kontragent>();
                    Kontragents.Clear();
                    var keys = redisClient.GetKeysByPattern("Cache:Kontragent:*").ToList();
                    using (var pipe = redisLocal.CreatePipeline())
                    {
                        foreach (var key in keys)
                            pipe.QueueCommand(r => r.GetValue(key),
                                x =>
                                {
                                    x.LoadFromCache();
                                    Kontragents.AddOrUpdate(x.DocCode, x);
                                });

                        pipe.Flush();
                    }
                }
            else
            {
                var mDate = Kontragents.Values.Cast<ICache>().Max(_ => _.UpdateDate);

                if (!(mDate < ctx.SD_43.Max(_ => _.UpdateDate))) return Kontragents.Values.ToList();
                {
                    var data = ctx.SD_43.Where(_ => _.UpdateDate > mDate).ToList();
                    foreach (var kontr in data)
                    {
                        var item = new Kontragent();
                        item.LoadFromEntity(kontr, this);
                        Kontragents.AddOrUpdate(item.DocCode, item);
                    }
                }
            }
        }
        return Kontragents.Values.ToList();
        
    }
    
    public INomenkl GetNomenkl(Guid? id)
    {
        return id is null ? null : GetNomenkl(id.Value);
    }

    public INomenkl GetNomenkl(Guid id)
    {
        decimal dc;
        var old = Nomenkls.Values.FirstOrDefault(_ => ((IDocGuid)_).Id == id);
        if (old is null)
            using (var redisClient = redisManager.GetClient())
            {
                redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                //var redis = redisClient.As<Nomenkl>();
                var keys = redisClient.GetKeysByPattern($"*{id}*");
                var enumerable = keys.ToList();
                if (!enumerable.Any()) return default;
                dc = Convert.ToDecimal(enumerable.First().Split('@')[0].Split(':')[2]);
            }
        else dc = ((IDocCode)old).DocCode;

        return GetNomenkl(dc);
    }

    public INomenkl GetNomenkl(decimal? dc)
    {
        if (dc is null) return null;
        return GetNomenkl(dc.Value);
    }

    public INomenkl GetNomenkl(decimal dc)
    {
        //while (isNomenklCacheLoad) Thread.Sleep(new TimeSpan(0, 0, 5));
        //Nomenkl itemNew;

        //if (!cacheKeysDict.ContainsKey("Nomenkl") ||
        //    (DateTime.Now - cacheKeysDict["Nomenkl"].LoadMoment).TotalSeconds > MaxTimersSec)
        //    LoadCacheKeys("Nomenkl");
        //CachKey key = null;
        //var keys = cacheKeysDict["Nomenkl"].CachKeys.Where(_ => _.DocCode == dc).ToList();
        //if (keys.Any())
        //{
        //    if (keys.Count > 1)
        //    {
        //        key = keys.First();
        //        foreach (var k in keys.Where(_ => _.Key != key.Key))
        //        {
        //            var d1 = Convert.ToDateTime(key.Key.Split('@')[1]);
        //            var d2 = Convert.ToDateTime(k.Key.Split('@')[1]);
        //            if (d2 > d1)
        //                key = k;
        //        }
        //    }
        //    else
        //    {
        //        key = keys.First();
        //    }
        //}


        //if (key is not null)
        //{
        //    if (Nomenkls.TryGetValue(dc, out var Nomenkl))
        //        return Nomenkl;
        //    itemNew = GetItem<Nomenkl>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        var entity = ctx.SD_83.FirstOrDefault(_ => _.DOC_CODE == dc);
        //        if (entity is null) return null;
        //        var nMain = GetNomenklMain(entity.MainId);
        //        var newItem = new Nomenkl
        //        {
        //            DocCode = entity.DOC_CODE,
        //            Id = entity.Id,
        //            Name = entity.NOM_NAME,
        //            FullName =
        //                entity.NOM_FULL_NAME,
        //            Notes = entity.NOM_NOTES,
        //            IsUsluga =
        //                entity.NOM_0MATER_1USLUGA == 1,
        //            IsProguct = entity.NOM_1PROD_0MATER == 1,
        //            IsNakladExpense =
        //                entity.NOM_1NAKLRASH_0NO == 1,
        //            DefaultNDSPercent = (decimal?)entity.NOM_NDS_PERCENT,
        //            IsDeleted =
        //                entity.NOM_DELETED == 1,
        //            IsUslugaInRentabelnost =
        //                entity.IsUslugaInRent ?? false,
        //            UpdateDate =
        //                entity.UpdateDate ?? DateTime.MinValue,
        //            MainId =
        //                entity.MainId ?? Guid.Empty,
        //            IsCurrencyTransfer = nMain.IsCurrencyTransfer,
        //            NomenklNumber =
        //                entity.NOM_NOMENKL,
        //            NomenklTypeDC =
        //                ((IDocCode)nMain.NomenklType).DocCode,
        //            ProductTypeDC = ((IDocCode)nMain.ProductType)?.DocCode,
        //            UnitDC = entity.NOM_ED_IZM_DC,
        //            CurrencyDC = entity.NOM_SALE_CRS_DC,
        //            GroupDC = entity.NOM_CATEG_DC
        //        };
        //        newItem.LoadFromEntity(entity, this);
        //        UpdateList2(new List<Nomenkl>(new[] { newItem }));
        //        Nomenkls.AddOrUpdate(dc, newItem);
        //        return Nomenkls[dc];
        //    }
        //}

        //Nomenkls.AddOrUpdate(dc, itemNew);
        //return Nomenkls[dc];

        if (Nomenkls.ContainsKey(dc)) return Nomenkls[dc];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:Nomenkl:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<Nomenkl>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_83.FirstOrDefault(_ => _.DOC_CODE == dc);
                        if (ent is null) return null;
                        var newItem = new Nomenkl();
                        newItem.LoadFromEntity(ent, this);
                        UpdateList(new List<Nomenkl>([newItem]));
                        Nomenkls.AddOrUpdate(dc, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    Nomenkls.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_83.FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (ent is null) return null;
                    var newItem = new Nomenkl();
                    newItem.LoadFromEntity(ent, this);
                    UpdateList(new List<Nomenkl>([newItem]));
                    Nomenkls.AddOrUpdate(dc, newItem);
                }
            }
        }

        return Nomenkls[dc];
    }

    public IEnumerable<INomenkl> GetNomenklsAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            if (Nomenkls.Count != ctx.SD_83.Count())
            {
                Nomenkls.Clear();
                using (var redisClient = redisManager.GetClient())
                {
                    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                    var redisLocal = redisClient.As<Nomenkl>();
                    var keys = redisClient.GetKeysByPattern("Cache:Nomenkl:*").ToList();
                    using (var pipe = redisLocal.CreatePipeline())
                    {
                        foreach (var key in keys)
                            pipe.QueueCommand(r => r.GetValue(key),
                                x =>
                                {
                                    x.LoadFromCache();
                                    Nomenkls.AddOrUpdate(x.DocCode, x);
                                });

                        pipe.Flush();
                    }
                }
            }
            else
            {
                var mDate = Nomenkls.Values.Cast<ICache>().Max(_ => _.UpdateDate);
                if (!(mDate < ctx.SD_83.Max(_ => _.UpdateDate))) return Nomenkls.Values.ToList();
                {
                    var data = ctx.SD_83.Where(_ => _.UpdateDate > mDate).ToList();
                    foreach (var nom in data)
                    {
                        var item = new Nomenkl();
                        item.LoadFromEntity(nom, this);
                        Nomenkls.AddOrUpdate(item.DocCode, item);
                    }
                }
            }
        }

        return Nomenkls.Values.ToList();
    }

    public INomenklMain GetNomenklMain(Guid? id)
    {
        return id is null ? null : GetNomenklMain(id.Value);
    }

    public INomenklMain GetNomenklMain(Guid id)
    {
        if (NomenklMains.ContainsKey(id))
            return NomenklMains[id];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:NomenklMain:{id}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItemGuid<NomenklMain>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.NomenklMain.FirstOrDefault(_ => _.Id == id);
                        if (ent is null) return null;
                        var newItem = new NomenklMain();
                        newItem.LoadFromEntity(ent, this);
                        UpdateListGuid(new List<NomenklMain>([newItem]));
                        NomenklMains.AddOrUpdate(id, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    NomenklMains.AddOrUpdate(itemNew.Id, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.NomenklMain.FirstOrDefault(_ => _.Id == id);
                    if (ent is null) return null;
                    var newItem = new NomenklMain();
                    newItem.LoadFromEntity(ent, this);
                    UpdateListGuid(new List<NomenklMain>([newItem]));
                    NomenklMains.AddOrUpdate(id, newItem);
                }
            }
        }

        return NomenklMains[id];
    }

    public IEnumerable<INomenklMain> GetNomenklMainAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            if (NomenklMains.Count != ctx.NomenklMain.Count())
            {
                NomenklMains.Clear();
                using (var redisClient = redisManager.GetClient())
                {
                    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                    var redisLocal = redisClient.As<NomenklMain>();
                    var keys = redisClient.GetKeysByPattern("Cache:NomenklMain:*").ToList();
                    using (var pipe = redisLocal.CreatePipeline())
                    {
                        foreach (var key in keys)
                            pipe.QueueCommand(r => r.GetValue(key),
                                x =>
                                {
                                    x.LoadFromCache();
                                    NomenklMains.AddOrUpdate(x.Id, x);
                                });

                        pipe.Flush();
                    }
                }
            }
            else
            {
                var mDate = NomenklMains.Values.Cast<ICache>().Max(_ => _.UpdateDate);
                if (!(mDate < ctx.NomenklMain.Max(_ => _.UpdateDate))) return NomenklMains.Values.ToList();
                {
                    var data = ctx.NomenklMain.Where(_ => _.UpdateDate > mDate).ToList();
                    foreach (var kontr in data)
                    {
                        var item = new NomenklMain();
                        item.LoadFromEntity(kontr, this);
                        NomenklMains.AddOrUpdate(item.Id, item);
                    }
                }
            }
        }

        return NomenklMains.Values.ToList();

    }

    public IEnumerable<INomenkl> GetNomenkl(IEnumerable<decimal> dcList)
    {
        //var cacheName = "Nomenkl";
        //var list = dcList.ToList();
        //UpdateNomenkl(list);
        //if (!list.Any()) return new List<Nomenkl>();
        //using (var redisClient = redisManager.GetClient())
        //{
        //    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
        //    if (cacheKeysDict.ContainsKey(cacheName))
        //        LoadCacheKeys(cacheName);
        //    var redis = redisClient.As<Nomenkl>();
        //    var keys = (from dc in list.Where(_ => !Nomenkls.ContainsKey(_))
        //        select cacheKeysDict[cacheName].CachKeys.FirstOrDefault(_ => _.DocCode == dc)
        //        into t
        //        where t != null
        //        select t.Key).ToList();
        //    if (!keys.Any()) return Nomenkls.Keys.Where(list.Contains).Select(n => Nomenkls[n] as Nomenkl).ToList();
        //    using (var pipe = redis.CreatePipeline())
        //    {
        //        foreach (var key in keys)
        //            pipe.QueueCommand(r => r.GetValue(key),
        //                x =>
        //                {
        //                    x.LoadFromCache();
        //                    if (Nomenkls.ContainsKey(x.DocCode)) Nomenkls[x.DocCode] = x;
        //                    else Nomenkls.Add(x.DocCode, x);
        //                });

        //        pipe.Flush();
        //    }

        return GetNomenklsAll().Cast<Nomenkl>().Where(_ => dcList.Contains(_.DocCode)).ToList();
    }


    public INomenklGroup GetNomenklGroup(decimal? dc)
    {
        //if (dc is null) return null;

        //if (!cacheKeysDict.ContainsKey("NomenklGroup"))
        //    LoadCacheKeys("NomenklGroup");

        //var key = cacheKeysDict["NomenklGroup"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        //NomenklGroup itemNew;
        //if (key is not null)
        //{
        //    if (NomenklGroups.TryGetValue(dc.Value, out var NomenklGroup))
        //        return NomenklGroup;
        //    itemNew = GetItem<NomenklGroup>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        var entity = ctx.SD_82.FirstOrDefault(_ => _.DOC_CODE == dc);
        //        if (entity is null) return null;
        //        var newItem = new NomenklGroup
        //        {
        //            DocCode = entity.DOC_CODE
        //        };
        //        newItem.LoadFromEntity(entity);
        //        UpdateList(new List<NomenklGroup>(new[] { newItem }));
        //        NomenklGroups.AddOrUpdate(dc.Value, newItem);
        //        return NomenklGroups[dc.Value];
        //    }
        //}

        //NomenklGroups.AddOrUpdate(dc.Value, itemNew);
        //return NomenklGroups[dc.Value];

        if (dc is null) return null;
        if (NomenklGroups.ContainsKey(dc.Value)) return NomenklGroups[dc.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:NomenklGroup:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<NomenklGroup>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_82.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                        if (ent is null) return null;
                        var newItem = new NomenklGroup();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<NomenklGroup>([newItem]));
                        NomenklGroups.AddOrUpdate(dc.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    NomenklGroups.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_82.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                    if (ent is null) return null;
                    var newItem = new NomenklGroup();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<NomenklGroup>([newItem]));
                    NomenklGroups.AddOrUpdate(dc.Value, newItem);
                }
            }
        }

        return NomenklGroups[dc.Value];
    }

    public IEnumerable<INomenklGroup> GetNomenklGroupAll()
    {

        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = NomenklGroups.Any()
                ? NomenklGroups.Values.Cast<ICache>().Max(_ => _.UpdateDate)
                : DateTime.MinValue;
            if (ctx.SD_82.All(_ => _.UpdateDate <= mDate))
                return NomenklGroups.Values.ToList();
            {
                var d = ctx.SD_82.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new NomenklGroup();
                    newItem.LoadFromEntity(item);
                    NomenklGroups.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return NomenklGroups.Values.ToList();
    }

    public IWarehouse GetWarehouse(decimal? dc)
    {
        return dc is null ? null : GetWarehouse(dc.Value);
    }

    public IWarehouse GetWarehouse(decimal dc)
    {
        //if (!cacheKeysDict.ContainsKey("Warehouse"))
        //    LoadCacheKeys("Warehouse");

        //var key = cacheKeysDict["Warehouse"].CachKeys.SingleOrDefault(_ => _.DocCode == dc);
        //Warehouse itemNew;
        //if (key is not null)
        //{
        //    if (Warehouses.TryGetValue(dc, out var Warehouse))
        //        return Warehouse;
        //    itemNew = GetItem<Warehouse>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        var entity = ctx.SD_27.FirstOrDefault(_ => _.DOC_CODE == dc);
        //        if (entity is null) return null;
        //        var newItem = new Warehouse
        //        {
        //            DocCode = entity.DOC_CODE,
        //            Id = entity.Id
        //        };
        //        newItem.LoadFromEntity(entity, this);
        //        UpdateList(new List<Warehouse>(new[] { newItem }));
        //        Warehouses.AddOrUpdate(dc, newItem);
        //        return Warehouses[dc];
        //    }
        //}

        //if (Warehouses.ContainsKey(dc))
        //    Warehouses[dc] = itemNew;
        //else
        //    Warehouses.Add(dc, itemNew);

        //return Warehouses[dc];

        if (Warehouses.ContainsKey(dc)) return Warehouses[dc];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:Warehouse:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<Warehouse>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_27.FirstOrDefault(_ => _.DOC_CODE == dc);
                        if (ent is null) return null;
                        var newItem = new Warehouse();
                        newItem.LoadFromEntity(ent, this);
                        UpdateList(new List<Warehouse>([newItem]));
                        Warehouses.AddOrUpdate(dc, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    Warehouses.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_27.FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (ent is null) return null;
                    var newItem = new Warehouse();
                    newItem.LoadFromEntity(ent, this);
                    UpdateList(new List<Warehouse>([newItem]));
                    Warehouses.AddOrUpdate(dc, newItem);
                }
            }
        }

        return Warehouses[dc];
    }

    public IWarehouse GetWarehouse(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IWarehouse> GetWarehousesAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = Warehouses.Any() ? Warehouses.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.SD_27.All(_ => _.UpdateDate <= mDate))
                return Warehouses.Values.ToList();
            {
                var d = ctx.SD_27.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new Warehouse();
                    newItem.LoadFromEntity(item, this);
                    Warehouses.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return Warehouses.Values.ToList();
    }

    public IEmployee GetEmployee(int? tabelNumber)
    {
        if (tabelNumber is null) return null;
        if (Employees.ContainsKey(tabelNumber.Value))
            return Employees[tabelNumber.Value];
        using (var ctx = GlobalOptions.GetEntities())
        {
            var emp = ctx.SD_2.FirstOrDefault(_ => _.TABELNUMBER == tabelNumber.Value);
            if (emp == null) return null;
            var item = new Employee();
            item.LoadFromEntity(emp, this);
            Employees.AddOrUpdate(item.TabelNumber, item);
        }

        return Employees[tabelNumber.Value];
    }

    public IEmployee GetEmployee(decimal? dc)
    {
        if (dc is null) return null;
        if (Employees.Count == 0)
            GetEmployees();
        
        if (Employees.ContainsKey(dc.Value))
            return Employees[dc.Value];
        var ret = Employees.Values.Cast<Employee>().FirstOrDefault(_ => _.DocCode == dc.Value);
        return ret ?? null;
    }

    public IEmployee GetEmployee(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IEmployee> GetEmployees()
    {
        
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = Employees.Any() ? Employees.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.SD_2.All(_ => _.UpdateDate <= mDate) || ctx.SD_2.Count() == Employees.Count)
                return Employees.Values.ToList();
            {
                var d = ctx.SD_2.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new Employee();
                    newItem.LoadFromEntity(item, this);
                    Employees.AddOrUpdate(newItem.DocCode, newItem);
                }

                if (ctx.SD_2.Count() != Employees.Count)
                {

                    var d2 = ctx.SD_2.AsNoTracking().Where(_ => !Employees.Keys.Contains(_.DOC_CODE))
                      .ToList();
                    foreach (var item in d2)
                    {
                        var newItem = new Employee();
                        newItem.LoadFromEntity(item, this);
                        Employees.AddOrUpdate(newItem.DocCode, newItem);
                    }
                }
            }
        }

        return Employees.Values.ToList();
    }

    public ISDRSchet GetSDRSchet(decimal? dc)
    {
        if (dc is null) return null;
        if (SDRSchets.Count == 0)
            GetSDRSchetAll();
        return SDRSchets.ContainsKey(dc.Value) ? SDRSchets[dc.Value] : null;
    }

    public ISDRSchet GetSDRSchet(Guid? id)
    {
        throw new NotImplementedException();
    }

    #region TimeOut

    private const int secondForCheckUpdate = 300;

    private DateTime checkSDRSchetUpdate = DateTime.MinValue;

    #endregion


    public IEnumerable<ISDRSchet> GetSDRSchetAll()
    {
        DateTime mDate = DateTime.MinValue;
        using (var ctx = GlobalOptions.GetEntities())
        {
            if (ctx.SD_303.Count() != SDRSchets.Count)
            {
                var dd = ctx.SD_303.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in dd)
                {
                    var newItem = new SDRSchet();
                    newItem.LoadFromEntity(item, this);
                    SDRSchets.AddOrUpdate(newItem.DocCode, newItem);
                }
                return SDRSchets.Values.ToList();
            }
            if (checkSDRSchetUpdate < DateTime.Now.AddSeconds(-secondForCheckUpdate))
            {
                checkSDRSchetUpdate = DateTime.Now;
                mDate = SDRSchets.Any()
                    ? SDRSchets.Values.Cast<ICache>().Max(_ => _.UpdateDate)
                    : DateTime.MinValue;
                if (ctx.SD_303.All(_ => _.UpdateDate <= mDate))
                    return SDRSchets.Values.ToList();
            }
            else
            {
                return SDRSchets.Values.ToList();
            }

            if (ctx.SD_303.Any(_ => _.UpdateDate > mDate)) return SDRSchets.Values.ToList();

            var d = ctx.SD_303.AsNoTracking().Where(_ =>
                _.UpdateDate > mDate).ToList();
            foreach (var item in d)
            {
                var newItem = new SDRSchet();
                newItem.LoadFromEntity(item, this);
                SDRSchets.AddOrUpdate(newItem.DocCode, newItem);
            }
        }

        return SDRSchets.Values.ToList();
    }

    public ISDRState GetSDRState(decimal? dc)
    {
        if (dc is null) return null;
        if (SDRStates.Count == 0)
            GetSDRStateAll();
        return SDRStates.ContainsKey(dc.Value) ? SDRStates[dc.Value] : null;
    }

    public ISDRState GetSDRState(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISDRState> GetSDRStateAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = SDRStates.Any() ? SDRStates.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.SD_99.All(_ => _.UpdateDate <= mDate))
                return SDRStates.Values.ToList();
            {
                var d = ctx.SD_99.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new SDRState();
                    newItem.LoadFromEntity(item);
                    SDRStates.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return SDRStates.Values.ToList();
    }

    public IClientCategory GetClientCategory(decimal? dc)
    {
        //if (dc is null) return null;
        //if (!cacheKeysDict.ContainsKey("ClientCategory"))
        //    LoadCacheKeys("ClientCategory");

        //var key = cacheKeysDict["ClientCategory"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        //ClientCategory itemNew;
        //if (key is not null)
        //{
        //    if (ClientCategories.TryGetValue(dc.Value, out var ClientCategory))
        //        return ClientCategory;
        //    itemNew = GetItem<ClientCategory>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    return null;
        //}

        //if (ClientCategories.ContainsKey(dc.Value))
        //    ClientCategories[dc.Value] = itemNew;
        //else
        //    ClientCategories.Add(dc.Value, itemNew);

        //return ClientCategories[dc.Value];
        if (dc is null) return null;
        if (ClientCategories.ContainsKey(dc.Value)) return ClientCategories[dc.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:ClientCategory:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<ClientCategory>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_148.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                        if (ent is null) return null;
                        var newItem = new ClientCategory();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<ClientCategory>([newItem]));
                        ClientCategories.AddOrUpdate(dc.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    ClientCategories.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_148.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                    if (ent is null) return null;
                    var newItem = new ClientCategory();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<ClientCategory>([newItem]));
                    ClientCategories.AddOrUpdate(dc.Value, newItem);
                }
            }
        }

        return ClientCategories[dc.Value];
    }

    public IClientCategory GetClientCategory(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IClientCategory> GetClientCategoriesAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = ClientCategories.Any()
                ? ClientCategories.Values.Cast<ICache>().Max(_ => _.UpdateDate)
                : DateTime.MinValue;
            if (ctx.SD_148.All(_ => _.UpdateDate <= mDate))
                return ClientCategories.Values.ToList();
            {
                var d = ctx.SD_148.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new ClientCategory();
                    newItem.LoadFromEntity(item);
                    ClientCategories.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return ClientCategories.Values.ToList();
    }

    public ICurrency GetCurrency(decimal? dc)
    {
        return dc is not null ? GetCurrency(dc.Value) : null;
    }

    public ICurrency GetCurrency(decimal dc)
    {
        //if (!cacheKeysDict.ContainsKey("Currency"))
        //    LoadCacheKeys("Currency");

        //var key = cacheKeysDict["Currency"].CachKeys.SingleOrDefault(_ => _.DocCode == dc);
        //Currency itemNew;
        //if (key is not null)
        //{
        //    if (Currencies.TryGetValue(dc, out var Currency))
        //        return Currency;
        //    itemNew = GetItem<Currency>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        var entity = ctx.SD_301.FirstOrDefault(_ => _.DOC_CODE == dc);
        //        if (entity is null) return null;
        //        var newItem = new Currency
        //        {
        //            DocCode = entity.DOC_CODE
        //        };
        //        newItem.LoadFromEntity(entity);
        //        UpdateList(new List<Currency>(new[] { newItem }));
        //        Currencies.AddOrUpdate(dc, newItem);
        //        return Currencies[dc];
        //    }
        //}

        //Currencies.AddOrUpdate(dc, itemNew);
        //return Currencies[dc];

        if (Currencies.ContainsKey(dc)) return Currencies[dc];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:Currency:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<Currency>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_301.FirstOrDefault(_ => _.DOC_CODE == dc);
                        if (ent is null) return null;
                        var newItem = new Currency();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<Currency>([newItem]));
                        Currencies.AddOrUpdate(dc, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    Currencies.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_301.FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (ent is null) return null;
                    var newItem = new Currency();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<Currency>([newItem]));
                    Currencies.AddOrUpdate(dc, newItem);
                }
            }
        }

        return Currencies[dc];
    }

    public IEnumerable<ICurrency> GetCurrenciesAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = Currencies.Any() ? Currencies.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.SD_301.All(_ => _.UpdateDate <= mDate))
                return Currencies.Values.ToList();
            {
                var d = ctx.SD_301.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new Currency();
                    newItem.LoadFromEntity(item);
                    Currencies.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return Currencies.Values.ToList();
    }

    public ICountry GetCountry(Guid? id)
    {
        //if (id is null) return null;
        //if (!cacheKeysDict.ContainsKey("Country"))
        //    LoadCacheKeys("Country");

        //var key = cacheKeysDict["Country"].CachKeys.SingleOrDefault(_ => _.Id == id.Value);
        //Country itemNew;
        //if (key is not null)
        //{
        //    if (Countries.TryGetValue(id.Value, out var Country))
        //        return Country;
        //    itemNew = GetItemGuid<Country>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    return null;
        //}

        //if (Countries.ContainsKey(id.Value))
        //    Countries[id.Value] = itemNew;
        //else
        //    Countries.Add(id.Value, itemNew);

        //return Countries[id.Value];
        if (id == null) return null;
        if (Countries.ContainsKey(id.Value))
            return Countries[id.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:Country:{id}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItemGuid<Country>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.Countries.FirstOrDefault(_ => _.Id == id);
                        if (ent is null) return null;
                        var newItem = new Country();
                        newItem.LoadFromEntity(ent);
                        UpdateListGuid(new List<Country>([newItem]));
                        Countries.AddOrUpdate(id.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    Countries.AddOrUpdate(itemNew.Id, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.Countries.FirstOrDefault(_ => _.Id == id);
                    if (ent is null) return null;
                    var newItem = new Country();
                    newItem.LoadFromEntity(ent);
                    UpdateListGuid(new List<Country>([newItem]));
                    Countries.AddOrUpdate(id.Value, newItem);
                }
            }
        }

        return Countries[id.Value];
    }

    public IEnumerable<ICountry> GetCountriesAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = Countries.Any() ? Countries.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.Countries.All(_ => _.UpdateDate <= mDate))
                return Countries.Values.ToList();
            {
                var d = ctx.Countries.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new Country();
                    newItem.LoadFromEntity(item);
                    Countries.AddOrUpdate(newItem.Id, newItem);
                }
            }
        }

        return Countries.Values.ToList();
    }

    public IRegion GetRegion(decimal? dc)
    {
        //if (dc is null) return null;
        //if (!cacheKeysDict.ContainsKey("Region"))
        //    LoadCacheKeys("Region");

        //var key = cacheKeysDict["Region"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        //Region itemNew;
        //if (key is not null)
        //{
        //    if (Regions.TryGetValue(dc.Value, out var Region))
        //        return Region;
        //    itemNew = GetItem<Region>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        var entity = ctx.SD_23.FirstOrDefault(_ => _.DOC_CODE == dc);
        //        if (entity is null) return null;
        //        var newItem = new Region
        //        {
        //            DocCode = entity.DOC_CODE
        //        };
        //        newItem.LoadFromEntity(entity);
        //        UpdateList(new List<Region>(new[] { newItem }));
        //        Regions.AddOrUpdate(dc.Value, newItem);
        //        return Regions[dc.Value];
        //    }
        //}

        //if (Regions.ContainsKey(dc.Value))
        //    Regions[dc.Value] = itemNew;
        //else
        //    Regions.Add(dc.Value, itemNew);

        //return Regions[dc.Value];

        if (dc is null) return null;
        if (Regions.ContainsKey(dc.Value)) return Regions[dc.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:Region:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<Region>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_23.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                        if (ent is null) return null;
                        var newItem = new Region();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<Region>([newItem]));
                        Regions.AddOrUpdate(dc.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    Regions.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_23.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                    if (ent is null) return null;
                    var newItem = new Region();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<Region>([newItem]));
                    Regions.AddOrUpdate(dc.Value, newItem);
                }
            }
        }

        return Regions[dc.Value];
    }

    public IRegion GetRegion(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IRegion> GetRegionsAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = Countries.Any() ? Countries.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.SD_23.All(_ => _.UpdateDate <= mDate))
                return Regions.Values.ToList();
            {
                var d = ctx.SD_23.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new Region();
                    newItem.LoadFromEntity(item);
                    Regions.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return Regions.Values.ToList();
    }

    public IUnit GetUnit(decimal? dc)
    {
        //if (dc is null) return null;
        //if (!cacheKeysDict.ContainsKey("Unit"))
        //    LoadCacheKeys("Unit");

        //var key = cacheKeysDict["Unit"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        //Unit itemNew;
        //if (key is not null)
        //{
        //    if (Units.TryGetValue(dc.Value, out var Unit))
        //        return Unit;
        //    itemNew = GetItem<Unit>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        var entity = ctx.SD_175.FirstOrDefault(_ => _.DOC_CODE == dc);
        //        if (entity is null) return null;
        //        var newItem = new Unit
        //        {
        //            DocCode = entity.DOC_CODE
        //        };
        //        newItem.LoadFromEntity(entity);
        //        UpdateList(new List<Unit>(new[] { newItem }));
        //        Units.AddOrUpdate(dc.Value, newItem);
        //        return Units[dc.Value];
        //    }
        //}

        //if (Units.ContainsKey(dc.Value))
        //    Units[dc.Value] = itemNew;
        //else
        //    Units.Add(dc.Value, itemNew);

        //return Units[dc.Value];

        if (dc is null) return null;
        if (Units.ContainsKey(dc.Value)) return Units[dc.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:Unit:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<Unit>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_175.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                        if (ent is null) return null;
                        var newItem = new Unit();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<Unit>([newItem]));
                        Units.AddOrUpdate(dc.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    Units.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_175.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                    if (ent is null) return null;
                    var newItem = new Unit();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<Unit>([newItem]));
                    Units.AddOrUpdate(dc.Value, newItem);
                }
            }
        }

        return Units[dc.Value];
    }

    public IUnit GetUnit(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IUnit> GetUnitsAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = Countries.Any() ? Countries.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.SD_175.All(_ => _.UpdateDate <= mDate))
                return Units.Values.ToList();
            {
                var d = ctx.SD_175.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new Unit();
                    newItem.LoadFromEntity(item);
                    Units.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return Units.Values.ToList();
    }

    public IMutualSettlementType GetMutualSettlementType(decimal? dc)
    {
        //if (dc is null) return null;

        //if (!cacheKeysDict.ContainsKey("MutualSettlementType"))
        //    LoadCacheKeys("MutualSettlementType");

        //var key = cacheKeysDict["MutualSettlementType"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        //MutualSettlementType itemNew;
        //if (key is not null)
        //{
        //    if (MutualSettlementTypes.TryGetValue(dc.Value, out var MutualSettlementType))
        //        return MutualSettlementType;
        //    itemNew = GetItem<MutualSettlementType>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    return null;
        //}

        //if (MutualSettlementTypes.ContainsKey(dc.Value))
        //    MutualSettlementTypes[dc.Value] = itemNew;
        //else
        //    MutualSettlementTypes.Add(dc.Value, itemNew);

        //return MutualSettlementTypes[dc.Value];

        if (dc is null) return null;
        if (MutualSettlementTypes.ContainsKey(dc.Value)) return MutualSettlementTypes[dc.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:MutualSettlementType:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<MutualSettlementType>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_111.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                        if (ent is null) return null;
                        var newItem = new MutualSettlementType();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<MutualSettlementType>([newItem]));
                        MutualSettlementTypes.AddOrUpdate(dc.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    MutualSettlementTypes.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_111.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                    if (ent is null) return null;
                    var newItem = new MutualSettlementType();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<MutualSettlementType>([newItem]));
                    MutualSettlementTypes.AddOrUpdate(dc.Value, newItem);
                }
            }
        }

        return MutualSettlementTypes[dc.Value];
    }

    public IEnumerable<IMutualSettlementType> GetMutualSettlementTypeAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = MutualSettlementTypes.Any()
                ? MutualSettlementTypes.Values.Cast<ICache>().Max(_ => _.UpdateDate)
                : DateTime.MinValue;
            if (ctx.SD_111.All(_ => _.UpdateDate <= mDate))
                return MutualSettlementTypes.Values.ToList();
            {
                var d = ctx.SD_111.AsNoTracking().Where(_ =>
                        _.UpdateDate > mDate)
                    .ToList();
                foreach (var item in d)
                {
                    var newItem = new MutualSettlementType();
                    newItem.LoadFromEntity(item);
                    MutualSettlementTypes.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return MutualSettlementTypes.Values.ToList();
    }

    public IProject GetProject(Guid? id)
    {
        //if (id is null) return null;

        //var keys = redisClient.GetKeysByPattern("Cache:Project:*").ToList();
        //Project itemNew;
        //if (key is not null)
        //{
        //    if (Projects.TryGetValue(id.Value, out var Project))
        //        return Project;
        //    itemNew = GetItemGuid<Project>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    return null;
        //}

        //if (Projects.ContainsKey(id.Value))
        //    Projects[id.Value] = itemNew;
        //else
        //    Projects.Add(id.Value, itemNew);

        //return Projects[id.Value];
        if (id == null) return null;
        if (Projects.ContainsKey(id.Value))
            return Projects[id.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:Project:{id.Value}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItemGuid<Project>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.Projects.FirstOrDefault(_ => _.Id == id);
                        if (ent is null) return null;
                        var newItem = new Project();
                        newItem.LoadFromEntity(ent, this);
                        UpdateListGuid(new List<Project>([newItem]));
                        Projects.AddOrUpdate(id.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    Projects.AddOrUpdate(itemNew.Id, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.Projects.FirstOrDefault(_ => _.Id == id);
                    if (ent is null) return null;
                    var newItem = new Project();
                    newItem.LoadFromEntity(ent, this);
                    UpdateListGuid(new List<Project>([newItem]));
                    Projects.AddOrUpdate(id.Value, newItem);
                }
            }
        }

        return Projects[id.Value];
    }

    public IEnumerable<IProject> GetProjectsAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = Projects.Any() ? Projects.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.Projects.All(_ => _.UpdateDate <= mDate))
                return Projects.Values.ToList();
            {
                var d = ctx.Projects.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new Project();
                    newItem.LoadFromEntity(item, this);
                    Projects.AddOrUpdate(newItem.Id, newItem);
                }
            }
        }

        return Projects.Values.ToList();
    }

    public IContractType GetContractType(decimal? dc)
    {
        //if (dc is null) return null;

        //if (!cacheKeysDict.ContainsKey("ContractType"))
        //    LoadCacheKeys("ContractType");

        //var key = cacheKeysDict["ContractType"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        //ContractType itemNew;
        //if (key is not null)
        //{
        //    if (ContractTypes.TryGetValue(dc.Value, out var ContractType))
        //        return ContractType;
        //    itemNew = GetItem<ContractType>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    return null;
        //}

        //if (ContractTypes.ContainsKey(dc.Value))
        //    ContractTypes[dc.Value] = itemNew;
        //else
        //    ContractTypes.Add(dc.Value, itemNew);

        //return ContractTypes[dc.Value];
        if (dc is null) return null;
        if (ContractTypes.ContainsKey(dc.Value)) return ContractTypes[dc.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:ContractType:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<ContractType>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_102.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                        if (ent is null) return null;
                        var newItem = new ContractType();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<ContractType>([newItem]));
                        ContractTypes.AddOrUpdate(dc.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    ContractTypes.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_102.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                    if (ent is null) return null;
                    var newItem = new ContractType();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<ContractType>([newItem]));
                    ContractTypes.AddOrUpdate(dc.Value, newItem);
                }
            }
        }

        return ContractTypes[dc.Value];
    }

    public IEnumerable<IContractType> GetContractsTypeAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = ContractTypes.Any()
                ? ContractTypes.Values.Cast<ICache>().Max(_ => _.UpdateDate)
                : DateTime.MinValue;
            if (ctx.SD_102.All(_ => _.UpdateDate <= mDate))
                return ContractTypes.Values.ToList();
            {
                var d = ctx.SD_102.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new ContractType();
                    newItem.LoadFromEntity(item);
                    ContractTypes.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return ContractTypes.Values.ToList();
    }

    public IPayForm GetPayForm(decimal? dc)
    {
        //if (dc is null) return null;
        //if (!cacheKeysDict.ContainsKey("PayForm"))
        //    LoadCacheKeys("PayForm");

        //var key = cacheKeysDict["PayForm"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        //PayForm itemNew;
        //if (key is not null)
        //{
        //    if (PayForms.TryGetValue(dc.Value, out var PayForm))
        //        return PayForm;
        //    itemNew = GetItem<PayForm>(key.Key);
        //    if (itemNew is null) return null;
        //    itemNew.LoadFromCache();
        //}
        //else
        //{
        //    return null;
        //}

        //if (PayForms.ContainsKey(dc.Value))
        //    PayForms[dc.Value] = itemNew;
        //else
        //    PayForms.Add(dc.Value, itemNew);

        //return PayForms[dc.Value];

        if (dc is null) return null;
        if (PayForms.ContainsKey(dc.Value)) return PayForms[dc.Value];
        using (var redisClient = redisManager.GetClient())
        {
            var allKeys = redisClient.GetKeysByPattern($"Cache:PayForm:{dc}*").ToList();
            if (allKeys.Count > 0)
            {
                var itemNew = GetItem<PayForm>(allKeys.Last());

                if (itemNew is null)
                {
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_189.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                        if (ent is null) return null;
                        var newItem = new PayForm();
                        newItem.LoadFromEntity(ent);
                        UpdateList(new List<PayForm>([newItem]));
                        PayForms.AddOrUpdate(dc.Value, newItem);
                    }
                }
                else
                {
                    itemNew.LoadFromCache();
                    PayForms.AddOrUpdate(itemNew.DocCode, itemNew);
                }
            }
            else
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var ent = ctx.SD_189.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
                    if (ent is null) return null;
                    var newItem = new PayForm();
                    newItem.LoadFromEntity(ent);
                    UpdateList(new List<PayForm>([newItem]));
                    PayForms.AddOrUpdate(dc.Value, newItem);
                }
            }
        }

        return PayForms[dc.Value];
    }

    public IEnumerable<IPayForm> GetPayFormAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = PayForms.Any() ? PayForms.Values.Cast<ICache>().Max(_ => _.UpdateDate) : DateTime.MinValue;
            if (ctx.SD_189.All(_ => _.UpdateDate <= mDate))
                return PayForms.Values.ToList();
            {
                var d = ctx.SD_189.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new PayForm();
                    newItem.LoadFromEntity(item);
                    PayForms.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return PayForms.Values.ToList();
    }

    public IPayCondition GetPayCondition(decimal? dc)
    {
        if (dc is null) return null;
        if (PayConditions.Count == 0)
            GetPayConditionAll();
        return PayConditions.ContainsKey(dc.Value) ? PayConditions[dc.Value] : null;
    }

    public IEnumerable<IPayCondition> GetPayConditionAll()
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var mDate = PayConditions.Any()
                ? PayConditions.Values.Cast<ICache>().Max(_ => _.UpdateDate)
                : DateTime.MinValue;
            if (ctx.SD_179.All(_ => _.UpdateDate <= mDate))
                return PayConditions.Values.ToList();
            {
                var d = ctx.SD_179.AsNoTracking().Where(_ =>
                    _.UpdateDate > mDate).ToList();
                foreach (var item in d)
                {
                    var newItem = new PayCondition();
                    newItem.LoadFromEntity(item);
                    PayConditions.AddOrUpdate(newItem.DocCode, newItem);
                }
            }
        }

        return PayConditions.Values.ToList();
    }

    public void StartLoad()
    {
        var now = DateTime.Now;
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var Context = GlobalOptions.GetEntities();
            var isUpdateInvoice = Convert.ToInt32(GlobalOptions.SystemProfile.Profile.FirstOrDefault(_ =>
                _.SECTION == "CACHE" &&
                _.ITEM == "UPDATE_ON_STATY")?.ITEM_VALUE ?? "0");
            if (isUpdateInvoice == 1)
            {
                foreach (var item in Context.SD_301.OrderBy(_ => _.ORDER_IMPOTANCE).AsNoTracking().ToList())
                {
                    var newItem = new Currency();
                    newItem.LoadFromEntity(item);
                    if (!Currencies.ContainsKey(newItem.DocCode))
                        Currencies.Add(newItem.DocCode, newItem);
                }

                DropAll<Currency>();
                UpdateList(Currencies.Values.Cast<Currency>(), now);
                GetCurrenciesAll();

                foreach (var item in Context.Countries.AsNoTracking().ToList())
                {
                    var newItem = new Country();
                    newItem.LoadFromEntity(item);
                    if (!Countries.ContainsKey(newItem.Id))
                        Countries.Add(newItem.Id, newItem);
                }

                DropAllGuid<Country>();
                UpdateListGuid(Countries.Values.Cast<Country>(), now);
                GetCountriesAll();


                foreach (var item in Context.SD_23.AsNoTracking().ToList())
                {
                    var newItem = new Region();
                    newItem.LoadFromEntity(item);
                    if (!Regions.ContainsKey(newItem.DocCode))
                        Regions.Add(newItem.DocCode, newItem);
                }

                DropAll<Region>();
                UpdateList(Regions.Values.Cast<Region>(), now);
                GetRegionsAll();

                foreach (var item in Context.SD_179.AsNoTracking().ToList())
                {
                    var newItem = new PayCondition();
                    newItem.LoadFromEntity(item);
                    if (!PayConditions.ContainsKey(newItem.DocCode))
                        PayConditions.Add(newItem.DocCode, newItem);
                }

                DropAll<PayCondition>();
                UpdateList(PayConditions.Values.Cast<PayCondition>(), now);
                GetPayConditionAll();

                foreach (var item in Context.SD_189.AsNoTracking().ToList())
                {
                    var newItem = new PayForm();
                    newItem.LoadFromEntity(item);
                    if (!PayForms.ContainsKey(newItem.DocCode))
                        PayForms.Add(newItem.DocCode, newItem);
                }

                DropAll<PayForm>();
                UpdateList(PayForms.Values.Cast<PayForm>(), now);
                GetPayFormAll();

                foreach (var item in Context.SD_175.AsNoTracking().ToList())
                {
                    var newItem = new Unit();
                    newItem.LoadFromEntity(item);
                    if (!Units.ContainsKey(newItem.DocCode))
                        Units.Add(newItem.DocCode, newItem);
                }

                DropAll<Unit>();
                UpdateList(Units.Values.Cast<Unit>(), now);
                GetUnitsAll();

                foreach (var item in Context.SD_40.AsNoTracking().ToList())
                {
                    var newItem = new CentrResponsibility();
                    newItem.LoadFromEntity(item);
                    if (!CentrResponsibilities.ContainsKey(newItem.DocCode))
                        CentrResponsibilities.Add(newItem.DocCode, newItem);
                }

                DropAll<CentrResponsibility>();
                UpdateList(CentrResponsibilities.Values.Cast<CentrResponsibility>(), now);
                GetCentrResponsibilitiesAll();

                foreach (var item in Context.SD_99.AsNoTracking().ToList())
                {
                    var newItem = new SDRState();
                    newItem.LoadFromEntity(item);
                    if (!SDRStates.ContainsKey(newItem.DocCode))
                        SDRStates.Add(newItem.DocCode, newItem);
                }

                DropAll<SDRState>();
                UpdateList(SDRStates.Values.Cast<SDRState>(), now);
                GetSDRStateAll();

                foreach (var item in Context.SD_303.AsNoTracking().ToList())
                {
                    var newItem = new SDRSchet();
                    newItem.LoadFromEntity(item, this);
                    if (!SDRSchets.ContainsKey(newItem.DocCode))
                        SDRSchets.Add(newItem.DocCode, newItem);
                }

                foreach (var item in SDRSchets.Values.Cast<SDRSchet>())
                    item.SDRStateDC = ((IDocCode)item.SDRState)?.DocCode;
                DropAll<SDRSchet>();
                UpdateList(SDRSchets.Values.Cast<SDRSchet>(), now);
                GetSDRSchetAll();

                foreach (var item in Context.SD_148.AsNoTracking().ToList())
                {
                    var newItem = new ClientCategory();
                    newItem.LoadFromEntity(item);
                    if (!ClientCategories.ContainsKey(newItem.DocCode))
                        ClientCategories.Add(newItem.DocCode, newItem);
                }

                DropAll<ClientCategory>();
                UpdateList(ClientCategories.Values.Cast<ClientCategory>(), now);
                GetClientCategoriesAll();

                foreach (var item in Context.SD_77.AsNoTracking().ToList())
                {
                    var newItem = new NomenklProductType();
                    newItem.LoadFromEntity(item, this);
                    if (!NomenklProductTypes.ContainsKey(newItem.DocCode))
                        NomenklProductTypes.Add(newItem.DocCode, newItem);
                }

                foreach (var item in NomenklProductTypes.Values)
                    ((NomenklProductType)item).SDRSchetDC = ((IDocCode)item.SDRSchet)?.DocCode;
                DropAll<NomenklProductType>();
                UpdateList(NomenklProductTypes.Values.Cast<NomenklProductType>(), now);
                GetNomenklProductTypesAll();

                foreach (var item in Context.UD_43.AsNoTracking().ToList())
                {
                    var newItem = new KontragentGroup();
                    newItem.LoadFromEntity(item);
                    if (!KontragentGroups.ContainsKey(newItem.Id))
                        KontragentGroups.Add(newItem.Id, newItem);
                }

                DropAll<KontragentGroup>();
                UpdateList(KontragentGroups.Values.Cast<KontragentGroup>(), now);
                GetKontragentCategoriesAll();

                foreach (var item in Context.SD_119.AsNoTracking().ToList())
                {
                    var newItem = new NomenklType();
                    newItem.LoadFromEntity(item);
                    if (!NomenklTypes.ContainsKey(newItem.DocCode))
                        NomenklTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<NomenklType>();
                UpdateList(NomenklTypes.Values.Cast<NomenklType>(), now);
                GetNomenklTypeAll();

                foreach (var item in Context.SD_50.AsNoTracking().ToList())
                {
                    var newItem = new ProductType();
                    newItem.LoadFromEntity(item);
                    if (!ProductTypes.ContainsKey(newItem.DocCode))
                        ProductTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<ProductType>();
                UpdateList(ProductTypes.Values.Cast<ProductType>(), now);
                GetProductTypeAll();

                //foreach (var item in Context.SD_2.AsNoTracking().ToList())
                //{
                //    var newItem = new Employee();
                //    newItem.LoadFromEntity(item, this);
                //    if (!Employees.ContainsKey(newItem.DocCode))
                //        Employees.Add(newItem.DocCode, newItem);
                //    else Employees[newItem.DocCode] = newItem;
                //}

                //foreach (var item in Employees.Values.Cast<Employee>())
                //    item.CurrencyDC = ((IDocCode)item.Currency)?.DocCode;
                //DropAll<Employee>();
                //UpdateList2(Employees.Values.Cast<Employee>(), now);
                GetEmployees();


                foreach (var item in Context.SD_27.AsNoTracking().ToList())
                {
                    var newItem = new Warehouse();
                    newItem.LoadFromEntity(item, this);
                    if (!Warehouses.ContainsKey(newItem.DocCode))
                        Warehouses.Add(newItem.DocCode, newItem);
                }

                foreach (var item in Warehouses.Values.Cast<Warehouse>())
                {
                    item.RegionDC = ((IDocCode)item.Region)?.DocCode;
                    item.StoreKeeperDC = ((IDocCode)item.StoreKeeper)?.DocCode;
                }

                DropAll<Warehouse>();
                UpdateList(Warehouses.Values.Cast<Warehouse>(), now);
                GetWarehousesAll();

                foreach (var item in Context.SD_111.AsNoTracking().ToList())
                {
                    var newItem = new MutualSettlementType();
                    newItem.LoadFromEntity(item);
                    if (!MutualSettlementTypes.ContainsKey(newItem.DocCode))
                        MutualSettlementTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<MutualSettlementType>();
                UpdateList(MutualSettlementTypes.Values.Cast<MutualSettlementType>(), now);
                GetMutualSettlementTypeAll();

                foreach (var item in Context.Projects.AsNoTracking().ToList())
                {
                    var newItem = new Project();
                    newItem.LoadFromEntity(item, this);
                    if (!Projects.ContainsKey(newItem.Id))
                        Projects.Add(newItem.Id, newItem);
                }

                DropAllGuid<Project>();
                UpdateListGuid(Projects.Values.Cast<Project>(), now);
                GetProjectsAll();

                foreach (var item in Context.SD_102.AsNoTracking().ToList())
                {
                    var newItem = new ContractType();
                    newItem.LoadFromEntity(item);
                    if (!ContractTypes.ContainsKey(newItem.DocCode))
                        ContractTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<ContractType>();
                UpdateList(ContractTypes.Values.Cast<ContractType>(), now);
                GetContractsTypeAll();

                foreach (var item in Context.SD_44.AsNoTracking().ToList())
                {
                    var newItem = new Bank();
                    newItem.LoadFromEntity(item);
                    if (!ContractTypes.ContainsKey(newItem.DocCode))
                        Banks.Add(newItem.DocCode, newItem);
                }

                DropAll<Bank>();
                UpdateList(Banks.Values.Cast<Bank>(), now);
                GetBanksAll();
                Kontragents.Clear();
                foreach (var entity in Context.SD_43.Include(_ => _.SD_2))
                {
                    var newItem = new Kontragent
                    {
                        DocCode = entity.DOC_CODE,
                        StartBalans = entity.START_BALANS ?? new DateTime(2000, 1, 1),
                        Name = entity.NAME,
                        Notes = entity.NOTES,
                        IsBalans = entity.FLAG_BALANS == 1,
                        IsDeleted = entity.DELETED == 1,
                        Id = entity.Id,
                        CurrencyDC = entity.VALUTA_DC,
                        GroupDC = entity.EG_ID,
                        ResponsibleEmployeeTN = entity.OTVETSTV_LICO
                    };
                    newItem.LoadFromEntity(entity, this);
                    newItem.ResponsibleEmployeeDC = ((IDocCode)GetEmployee(newItem.ResponsibleEmployeeTN))?.DocCode;
                    Kontragents.Add(newItem.DocCode, newItem);
                }

                DropAll<Kontragent>();
                UpdateList2(Kontragents.Values.Cast<Kontragent>().ToList(), now);

                BankAccounts = Context.SD_114.AsNoTracking()
                    .ToList()
                    .Select(entity => new BankAccount
                    {
                        RashAccCode = entity.BA_RASH_ACC_CODE,
                        RashAccount = entity.BA_RASH_ACC,
                        BACurrency = entity.BA_CURRENCY,
                        IsTransit = entity.BA_TRANSIT == 1,
                        IsNegativeRests = entity.BA_NEGATIVE_RESTS == 1,
                        BABankAccount = entity.BA_BANK_ACCOUNT,
                        ShortName = entity.BA_ACC_SHORTNAME,
                        StartDate = entity.StartDate,
                        StartSumma = entity.StartSumma,
                        DateNonZero = entity.DateNonZero,
                        DocCode = entity.DOC_CODE,
                        KontragentDC = entity.BA_BANK_AS_KONTRAGENT_DC,
                        CentrResponsibilityDC = entity.BA_CENTR_OTV_DC,
                        CurrencyDC = entity.CurrencyDC,
                        BankDC = entity.BA_BANKDC,
                        IsDeleted = entity.IsDeleted ?? false
                    })
                    .ToDictionary<BankAccount, decimal, IBankAccount>(newItem => newItem.DocCode, newItem => newItem);
                foreach (var k in BankAccounts.Values.Cast<BankAccount>()) k.LoadFromCache();

                DropAll<BankAccount>();
                UpdateList(BankAccounts.Values.Cast<BankAccount>(), now);
                GetBankAccountAll();

                CashBoxes = Context.SD_22.Include(_ => _.TD_22)
                    .AsNoTracking()
                    .ToList()
                    .Select(entity => new CashBox
                    {
                        IsNegativeRests = entity.CA_NEGATIVE_RESTS == 1,
                        IsNoBalans = entity.CA_NEGATIVE_RESTS == 1,
                        DocCode = entity.DOC_CODE,
                        Name = entity.CA_NAME,
                        DefaultCurrencyDC = entity.CA_CRS_DC,
                        KontragentDC = entity.CA_KONTR_DC,
                        CentrResponsibilityDC = entity.CA_CENTR_OTV_DC
                    })
                    .ToDictionary<CashBox, decimal, ICashBox>(newItem => newItem.DocCode, newItem => newItem);
                foreach (var k in CashBoxes.Values.Cast<CashBox>()) k.LoadFromCache();

                DropAll<CashBox>();
                UpdateList(CashBoxes.Values.Cast<CashBox>(), now);
                GetCashBoxAll();

                var cnts = Context.Database.SqlQuery<NomGroupCount>(
                    "SELECT NOM_CATEG_DC AS DocCode, count(*) AS Count from sd_83 GROUP BY NOM_CATEG_DC").ToList();
                var dictCount = cnts.ToDictionary(cnt => cnt.DocCode, cnt => cnt.Count);
                foreach (var item in Context.SD_82.AsNoTracking().ToList())
                {
                    var newItem = new NomenklGroup();
                    newItem.LoadFromEntity(item);
                    if (!NomenklGroups.ContainsKey(newItem.DocCode))
                        NomenklGroups.Add(newItem.DocCode, newItem);
                }

                foreach (var grp in NomenklGroups.Values.Cast<NomenklGroup>())
                    grp.NomenklCount = getCountNomenklGroup(grp, dictCount);

                DropAll<NomenklGroup>();
                UpdateList(NomenklGroups.Values.Cast<NomenklGroup>(), now);
                GetNomenklGroupAll();

                foreach (var entity in Context.NomenklMain.Include(_ => _.SD_83).AsNoTracking().ToList())
                {
                    var item = new NomenklMain
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        Notes = entity.Note,
                        NomenklNumber = entity.NomenklNumber,
                        FullName = entity.FullName,
                        IsUsluga = entity.IsUsluga,
                        IsProguct = entity.IsComplex,
                        IsNakladExpense = entity.IsNakladExpense,
                        IsOnlyState = entity.IsOnlyState ?? false,
                        IsCurrencyTransfer = entity.IsCurrencyTransfer ?? false,
                        IsRentabelnost = entity.IsRentabelnost ?? false,
                        UnitDC = entity.UnitDC,
                        CategoryDC = entity.CategoryDC,
                        NomenklTypeDC = entity.TypeDC,
                        ProductTypeDC = entity.ProductDC,
                        Nomenkls = new List<decimal>(entity.SD_83.Select(_ => _.DOC_CODE))
                    };
                    //item.LoadFromEntity(entity,this);
                    NomenklMains.AddOrUpdate(item.Id, item);
                }

                DropAllGuid<NomenklMain>();
                UpdateListGuid(NomenklMains.Values.Cast<NomenklMain>(), now);

                //var noms = new Dictionary<decimal, INomenkl>();
                foreach (var entity in Context.SD_83.Include(_ => _.NomenklMain).AsNoTracking())
                {
                    var item = new Nomenkl
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
                    };
                    Nomenkls.Add(item.DocCode, item);
                }

                DropAll<Nomenkl>();
                UpdateList2(Nomenkls.Values.Cast<Nomenkl>(), now);
                //isNomenklCacheLoad = false;
            }
            else
            {
                GetCurrenciesAll();
                GetClientCategoriesAll();
                GetKontragentCategoriesAll();
                GetCentrResponsibilitiesAll();
                GetSDRStateAll();
                GetSDRSchetAll();
                Task.Run(() =>
                {
                    GetEmployees();
                    GetKontragentsAll();
                });

                Task.Run(() =>
                {
                    GetCountriesAll();
                    GetUnitsAll();
                    GetCashBoxAll();
                    GetContractsTypeAll();
                    GetMutualSettlementTypeAll();
                    GetNomenklGroupAll();
                    GetNomenklProductTypesAll();
                    GetNomenklTypeAll();
                    GetPayConditionAll();
                    GetPayFormAll();
                    GetProductTypeAll();
                    GetProjectsAll();
                    GetRegionsAll();
                    GetWarehousesAll();
                    GetDeliveryConditionAll();
                    GetKontragentsAll();
                    GetNomenklMainAll();
                    GetNomenklsAll();
                });
            }
        }

        //ClearAll();
    }

    public void UpdateNomeklMain(IEnumerable<Guid> ids)
    {
        var pageSize = 500;
        using (var ctx = GlobalOptions.GetEntities())
        {
            var load_ids = ids.Where(id => !NomenklMains.ContainsKey(id)).ToList();

            var maxIdx = load_ids.Count / pageSize;
            for (var i = 0; i < maxIdx; i++)
            {
                var list = load_ids.Page(pageSize, i);
                var data = ctx.NomenklMain.Where(_ => list.Contains(_.Id)).ToList();
                var loadItems = new List<NomenklMain>();
                foreach (var item in data.Select(entity => new NomenklMain
                         {
                             Id = entity.Id,
                             Name = entity.Name,
                             Notes = entity.Note,
                             NomenklNumber = entity.NomenklNumber,
                             FullName = entity.FullName,
                             IsUsluga = entity.IsUsluga,
                             IsProguct = entity.IsComplex,
                             IsNakladExpense = entity.IsNakladExpense,
                             IsOnlyState = entity.IsOnlyState ?? false,
                             IsCurrencyTransfer = entity.IsCurrencyTransfer ?? false,
                             IsRentabelnost = entity.IsRentabelnost ?? false,
                             UnitDC = entity.UnitDC,
                             CategoryDC = entity.CategoryDC,
                             NomenklTypeDC = entity.TypeDC,
                             ProductTypeDC = entity.ProductDC
                         }))
                {
                    NomenklMains.AddOrUpdate(item.Id, item);
                    loadItems.Add(item);
                }

                UpdateListGuid(loadItems);
            }
        }
    }

    public void UpdateNomenkl(IEnumerable<decimal> docDCs)
    {
        var pageSize = 500;
        var out_dcs = new List<decimal>();
        var in_dcs = new List<string>();
        try
        {
            //while (isNomenklCacheLoad) Thread.Sleep(new TimeSpan(0, 0, 5));
            //foreach (var dc in docDCs.ToList())
            //{
            //    //var key = cacheKeysDict["Nomenkl"].CachKeys.SingleOrDefault(_ => _.DocCode == dc);
            //    if (key is null) out_dcs.Add(dc);
            //    else
            //        in_dcs.Add(key.Key);
            //}

            using (var redisClient = redisManager.GetClient())
            {
                redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                var redis = redisClient.As<Nomenkl>();
                var noms = redis.GetValues(in_dcs);
                foreach (var n in noms) ((ICache)n).LoadFromCache();

                foreach (var n in noms) Nomenkls.AddOrUpdate(n.DocCode, n);
            }


            using (var ctx = GlobalOptions.GetEntities())
            {
                var load_ids = out_dcs.Where(dc => !Nomenkls.ContainsKey(dc)).ToList();
                var maxIdx = load_ids.Count / pageSize;
                for (var i = 0; i < maxIdx; i++)
                {
                    var list = load_ids.Page(pageSize, i);
                    var data = ctx.SD_83.Where(_ => list.Contains(_.DOC_CODE)).Include(sd83 => sd83.NomenklMain)
                        .ToList();
                    var loadItems = new List<Nomenkl>();
                    foreach (var item in data.Select(entity => new Nomenkl
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
                        ((ICache)item).LoadFromCache();
                        Nomenkls.AddOrUpdate(item.DocCode, item);
                        loadItems.Add(item);
                    }

                    UpdateList2(loadItems);
                }
            }
        }
        catch (Exception ex)
        {
            var win = new WindowManager();
            win.ShowMessageBox(ex.Message, "Ошибка");
        }
    }

    private int getCountNomenklGroup(NomenklGroup grp, Dictionary<decimal, int> noms)
    {
        var subGroups =
            NomenklGroups.Values.Cast<NomenklGroup>().Where(_ => _.ParentDC == grp.DocCode).ToList();
        if (!subGroups.Any()) return noms.ContainsKey(grp.DocCode) ? noms[grp.DocCode] : 0;

        var cnt = noms.ContainsKey(grp.DocCode) ? noms[grp.DocCode] : 0;
        foreach (var sg in subGroups)
        {
            sg.NomenklCount = getCountNomenklGroup(sg, noms);
            cnt += sg.NomenklCount;
        }

        return cnt;
    }


    //private void LoadCacheKeys(string name)
    //{
    //    var resultList = new ConcurrentBag<CachKey>();
    //    if (!cacheKeysDict.ContainsKey(name))
    //        cacheKeysDict.Add(name, new CacheKeys());
    //    using (var redisClient = redisManager.GetClient())
    //    {
    //        redisClient.Db = GlobalOptions.RedisDBId ?? 0;
    //        var keys = redisClient.GetKeysByPattern($"Cache:{name}:*").ToList();
    //        cacheKeysDict[name].CachKeys.Clear();
    //        Parallel.ForEach(keys, key =>
    //        {
    //            switch (name)
    //            {
    //                case "Kontragent":
    //                case "Nomenkl":
    //                    resultList.Add(new CachKey
    //                    {
    //                        DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
    //                        Id = Guid.Parse(key.Split('@')[0].Split(':')[3]),
    //                        LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
    //                        Key = key
    //                    });
    //                    break;
    //                case "Employee":
    //                    resultList.Add(new CachKey
    //                    {
    //                        DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
    //                        TabelNumber = Convert.ToInt32(key.Split('@')[0].Split(':')[3]),
    //                        LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
    //                        Key = key
    //                    });
    //                    break;
    //                case "NomenklMain":
    //                case "Country":
    //                case "Project":
    //                    resultList.Add(new CachKey
    //                    {
    //                        Id = Guid.Parse(key.Split('@')[0].Split(':')[2]),
    //                        LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
    //                        Key = key
    //                    });
    //                    break;
    //                default:
    //                    resultList.Add(new CachKey
    //                    {
    //                        DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
    //                        LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
    //                        Key = key
    //                    });
    //                    break;
    //            }
    //        });


    //        cacheKeysDict[name].CachKeys = [];
    //        foreach (var cKey in resultList)
    //        {
    //            cacheKeysDict[name].CachKeys.AddCacheKey(cKey);
    //        }
    //        cacheKeysDict[name].LoadMoment = DateTime.Now;
    //        if (name == "Nomenkl")
    //            isNomenklCacheLoad = false;
    //    }
    //}

    private void UpdateReferences(string channel, string msg)
    {
        if (string.IsNullOrWhiteSpace(msg)) return;
        var message = JsonConvert.DeserializeObject<RedisMessage>(msg);
        if (message == null || message.DbId != GlobalOptions.DataBaseId) return;

        switch (channel)
        {
            case RedisMessageChannels.BankReference:
                if (message.DocCode is null) return;
                var b_item = GetItem<Bank>((string)message.ExternalValues["RedisKey"]);
                b_item.LoadFromCache();
                if (Banks.ContainsKey(message.DocCode.Value))
                    Banks[message.DocCode.Value] = b_item;
                else Banks.Add(message.DocCode.Value, b_item);
                break;
            case RedisMessageChannels.RegionReference:
                if (message.DocCode is null) return;
                var r_item = GetItem<Region>((string)message.ExternalValues["RedisKey"]);
                r_item.LoadFromCache();
                if (Regions.ContainsKey(message.DocCode.Value))
                    Regions[message.DocCode.Value] = r_item;
                else Regions.Add(message.DocCode.Value, r_item);
                break;
            case RedisMessageChannels.BankAccountReference:
                if (message.DocCode is null) return;
                if (message.OperationType == RedisMessageDocumentOperationTypeEnum.Delete)
                    BankAccounts.Remove(message.DocCode.Value);
                else
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var ent = ctx.SD_114.SingleOrDefault(_ => _.DOC_CODE == message.DocCode);
                        if (ent is not null)
                        {
                            var newItem = new BankAccount();
                            newItem.LoadFromEntity(ent, this);
                            AddOrUpdate(newItem);
                            BankAccounts[newItem.DocCode] = newItem;
                        }
                    }

                var jsonSerializerSettings1 = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var redisMessage1 = new RedisMessage
                {
                    Message =
                        $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник банков.",
                    ExternalValues =
                    {
                        ["Type"] = "BankAccount"
                    }
                };
                var json1 = JsonConvert.SerializeObject(redisMessage1, jsonSerializerSettings1);
                mySubscriber.Publish(
                    new RedisChannel(RedisMessageChannels.ReferenceUpdate,
                        RedisChannel.PatternMode.Auto), json1);
                break;
            case RedisMessageChannels.CashBoxReference:
                if (message.DocCode is null) return;
                var cb_item = GetItem<CashBox>((string)message.ExternalValues["RedisKey"]);
                cb_item.LoadFromCache();
                if (CashBoxes.ContainsKey(message.DocCode.Value))
                    CashBoxes[message.DocCode.Value] = cb_item;
                else CashBoxes.Add(message.DocCode.Value, cb_item);
                break;
            case RedisMessageChannels.CentrResponsibilityReference:
                using (var redisClient = redisManager.GetClient())
                {
                    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                    if (!message.ExternalValues.ContainsKey("DocCodes")) return;
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var lst = (message.ExternalValues["DocCodes"] as JArray).ToObject<List<decimal>>();

                        var entities = ctx.SD_40.Where(_ => lst.Contains(_.DOC_CODE));
                        foreach (var entity in entities)
                        {
                            var newItem = new CentrResponsibility
                            {
                                DocCode = entity.DOC_CODE,
                                Name = entity.CENT_NAME,
                                FullName = entity.CENT_FULLNAME,
                                ParentDC = entity.CENT_PARENT_DC,
                                UpdateDate = entity.UpdateDate ?? DateTime.Now
                            };
                            newItem.LoadFromEntity(entity);
                            AddOrUpdate(newItem);
                            if (CentrResponsibilities.ContainsKey(newItem.DocCode))
                                CentrResponsibilities[newItem.DocCode] = newItem;
                            else CentrResponsibilities.Add(newItem.DocCode, newItem);
                        }
                    }

                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var redisMessage = new RedisMessage
                    {
                        Message =
                            $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник центров ответственности."
                    };
                    redisMessage.ExternalValues["Type"] = "CentrResponsibility";
                    var json = JsonConvert.SerializeObject(redisMessage, jsonSerializerSettings);
                    mySubscriber.Publish(
                        new RedisChannel(RedisMessageChannels.ReferenceUpdate,
                            RedisChannel.PatternMode.Auto), json);
                }

                break;
            case RedisMessageChannels.ClientCategoryReference:
                if (message.DocCode is null) return;
                var cc_item = GetItem<ClientCategory>((string)message.ExternalValues["RedisKey"]);
                cc_item.LoadFromCache();
                if (ClientCategories.ContainsKey(message.DocCode.Value))
                    ClientCategories[message.DocCode.Value] = cc_item;
                else ClientCategories.Add(message.DocCode.Value, cc_item);
                break;
            case RedisMessageChannels.ContractTypeReference:
                if (message.DocCode is null) return;
                var ct_item = GetItem<ContractType>((string)message.ExternalValues["RedisKey"]);
                ct_item.LoadFromCache();

                if (ContractTypes.ContainsKey(message.DocCode.Value))
                    ContractTypes[message.DocCode.Value] = ct_item;
                else ContractTypes.Add(message.DocCode.Value, ct_item);
                break;
            case RedisMessageChannels.CountryReference:
                if (message.Id is null) return;
                GetCountry(message.Id);
                break;
            case RedisMessageChannels.CurrencyReference:
                if (message.DocCode is null) return;
                var crs_item = GetItem<Currency>((string)message.ExternalValues["RedisKey"]);
                crs_item.LoadFromCache();
                if (Currencies.ContainsKey(message.DocCode.Value))
                    Currencies[message.DocCode.Value] = crs_item;
                else Currencies.Add(message.DocCode.Value, crs_item);
                break;
            case RedisMessageChannels.DeliveryConditionReference:
                if (message.DocCode is null) return;
                GetDeliveryCondition(message.DocCode);
                break;
            case RedisMessageChannels.EmployeeReference:
                using (var redisClient = redisManager.GetClient())
                {
                    if (!message.ExternalValues.ContainsKey("Updates") ||
                        !message.ExternalValues.ContainsKey("Deletes")) return;
                    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                    var lstUpdates = (message.ExternalValues["Updates"] as JArray).ToObject<List<decimal>>();
                    var lstDels = (message.ExternalValues["Deletes"] as JArray).ToObject<List<decimal>>();
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        if (lstDels.Count > 0)
                        {
                            var delItems = ctx.SD_2.Where(_ => lstDels.Contains(_.DOC_CODE)).ToList();
                            ctx.SD_2.RemoveRange(delItems);
                            foreach (var dc in lstDels.Where(dc => Employees.ContainsKey(dc))) Employees.Remove(dc);
                        }

                        if (lstUpdates.Count > 0)
                        {
                            var updItems = ctx.SD_2.Where(_ => lstUpdates.Contains(_.DOC_CODE)).ToList();
                            foreach (var ent in updItems)
                            {
                                var newItem = new Employee();
                                newItem.LoadFromEntity(ent, this);
                                AddOrUpdate(newItem);
                                Employees[newItem.DocCode] = newItem;
                            }
                        }
                    }

                    var jsonSerializerSettings2 = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var redisMessage2 = new RedisMessage
                    {
                        Message =
                            $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник сотрудников.",
                        ExternalValues =
                        {
                            ["Type"] = "Employee"
                        }
                    };

                    var json2 = JsonConvert.SerializeObject(redisMessage2, jsonSerializerSettings2);
                    mySubscriber.Publish(
                        new RedisChannel(RedisMessageChannels.ReferenceUpdate,
                            RedisChannel.PatternMode.Auto), json2);
                }

                //if (message.DocCode is null) return;
                //LoadCacheKeys("Employee");
                //var emp_item = GetItem<Employee>((string)message.ExternalValues["RedisKey"]);
                //emp_item.LoadFromCache();
                //var emp_oldKey = cacheKeysDict["Employee"].CachKeys
                //    .SingleOrDefault(_ => _.DocCode == message.DocCode);
                //if (emp_oldKey is null)
                //    cacheKeysDict["Employee"].CachKeys.AddCacheKey(new CachKey
                //    {
                //        Key = (string)message.ExternalValues["RedisKey"],
                //        DocCode = message.DocCode,
                //        LastUpdate =
                //            Convert.ToDateTime(((string)message.ExternalValues["RedisKey"]).Split("@"[1]))
                //    });

                //if (Employees.ContainsKey(message.DocCode.Value))
                //    Employees[message.DocCode.Value] = emp_item;
                //else Employees.Add(message.DocCode.Value, emp_item);
                break;
            case RedisMessageChannels.KontragentGroupReference:
                if (message.DocCode is null) return;
                var kg_item = GetItem<KontragentGroup>((string)message.ExternalValues["RedisKey"]);
                kg_item.LoadFromCache();
                if (KontragentGroups.ContainsKey((int)message.DocCode.Value))
                    KontragentGroups[(int)message.DocCode.Value] = kg_item;
                else KontragentGroups.Add((int)message.DocCode.Value, kg_item);
                break;
            case RedisMessageChannels.MutualSettlementTypeReference:
                if (message.DocCode is null) return;
                var mst_item = GetItem<MutualSettlementType>((string)message.ExternalValues["RedisKey"]);
                mst_item.LoadFromCache();
                MutualSettlementTypes.AddOrUpdate(message.DocCode.Value, mst_item);
                break;
            case RedisMessageChannels.NomenklGroupReference:
                if (message.DocCode is null) return;
                var ng_item = GetItem<NomenklGroup>((string)message.ExternalValues["RedisKey"]);
                ng_item.LoadFromCache();
                NomenklGroups.AddOrUpdate(message.DocCode.Value, ng_item);
                break;
            case RedisMessageChannels.NomenklMainReference:
                if (message.Id is null) return;
                using (var redisClient = redisManager.GetClient())
                {
                    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                    //if (message.DocCode is null) return;
                    NomenklMain nm_item = null;
                    var nomenkls = new List<SD_83>();
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var nm = ctx.NomenklMain.Include(_ => _.SD_83).FirstOrDefault(_ => _.Id == message.Id);
                        if (nm is null) return;
                        nm_item = new NomenklMain();
                        nm_item.LoadFromEntity(nm, this);

                        AddOrUpdateGuid(nm_item);

                        if (NomenklMains.ContainsKey(nm_item.Id))
                            NomenklMains[nm_item.Id] = nm_item;
                        else NomenklMains.Add(nm_item.Id, nm_item);

                        foreach (var nom in ctx.SD_83.Include(_ => _.NomenklMain)
                                     .Where(_ => nm_item.Nomenkls.Contains(_.DOC_CODE)).AsNoTracking())
                        {
                            var nomItem = new Nomenkl
                            {
                                DocCode = nom.DOC_CODE,
                                Id = nom.Id,
                                Name = nm_item.Name,
                                FullName =
                                    nm_item.FullName,
                                Notes = nom.NOM_NOTES,
                                IsUsluga =
                                    nom.NOM_0MATER_1USLUGA == 1,
                                IsProguct = nom.NOM_1PROD_0MATER == 1,
                                IsNakladExpense =
                                    nom.NOM_1NAKLRASH_0NO == 1,
                                DefaultNDSPercent = (decimal?)nom.NOM_NDS_PERCENT,
                                IsDeleted =
                                    nom.NOM_DELETED == 1,
                                IsUslugaInRentabelnost =
                                    nom.IsUslugaInRent ?? false,
                                UpdateDate =
                                    nm_item.UpdateDate == default ? DateTime.Now : nm_item.UpdateDate,
                                MainId =
                                    nom.MainId ?? Guid.Empty,
                                IsCurrencyTransfer = nom.NomenklMain.IsCurrencyTransfer ?? false,
                                NomenklNumber =
                                    nom.NOM_NOMENKL,
                                NomenklTypeDC =
                                    nom.NomenklMain.TypeDC,
                                ProductTypeDC = nom.NomenklMain.ProductDC,
                                UnitDC = nom.NOM_ED_IZM_DC,
                                CurrencyDC = nom.NOM_SALE_CRS_DC,
                                GroupDC = nom.NOM_CATEG_DC
                            };
                            nomItem.LoadFromEntity(nom, this);
                            AddOrUpdate(nomItem);
                            //UpdateList2(new List<Nomenkl>(new[] { nomItem }));
                            Nomenkls.AddOrUpdate(nomItem.DocCode, nomItem);
                        }
                    }

                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var redisMessage = new RedisMessage
                    {
                        Message =
                            $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник номенклатур."
                    };
                    redisMessage.ExternalValues["Type"] = "Nomenkl";
                    var json = JsonConvert.SerializeObject(redisMessage, jsonSerializerSettings);
                    mySubscriber.Publish(
                        new RedisChannel(RedisMessageChannels.ReferenceUpdate,
                            RedisChannel.PatternMode.Auto), json);
                }

                break;
            case RedisMessageChannels.NomenklProductTypeReference:
                if (message.DocCode is null) return;
                var npt_item = GetItem<NomenklProductType>((string)message.ExternalValues["RedisKey"]);
                npt_item.LoadFromCache();
                if (NomenklProductTypes.ContainsKey(message.DocCode.Value))
                    NomenklProductTypes[message.DocCode.Value] = npt_item;
                else NomenklProductTypes.Add(message.DocCode.Value, npt_item);
                break;
            case RedisMessageChannels.NomenklTypeReference:
                if (message.DocCode is null) return;
                var nt_item = GetItem<NomenklType>((string)message.ExternalValues["RedisKey"]);
                nt_item.LoadFromCache();
                if (NomenklTypes.ContainsKey(message.DocCode.Value))
                    NomenklTypes[message.DocCode.Value] = nt_item;
                else NomenklTypes.Add(message.DocCode.Value, nt_item);
                break;
            case RedisMessageChannels.PayConditionReference:
                using (var redisClient = redisManager.GetClient())
                {
                    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                    if (!message.ExternalValues.ContainsKey("DocCodes")) return;
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var lst = (message.ExternalValues["DocCodes"] as JArray).ToObject<List<decimal>>();

                        var entities = ctx.SD_179.Where(_ => lst.Contains(_.DOC_CODE));
                        foreach (var entity in entities)
                        {
                            var newItem = new PayCondition
                            {
                                DocCode = entity.DOC_CODE,
                                Name = entity.PT_NAME,
                                IsDefault = entity.DEFAULT_VALUE == 1,
                                UpdateDate = entity.UpdateDate ?? DateTime.Now
                            };
                            newItem.LoadFromEntity(entity);
                            AddOrUpdate(newItem);
                            if (PayConditions.ContainsKey(newItem.DocCode))
                                PayConditions[newItem.DocCode] = newItem;
                            else PayConditions.Add(newItem.DocCode, newItem);
                        }
                    }

                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var redisMessage = new RedisMessage
                    {
                        Message =
                            $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник условий оплаты."
                    };
                    redisMessage.ExternalValues["Type"] = "PayCondition";
                    var json = JsonConvert.SerializeObject(redisMessage, jsonSerializerSettings);
                    mySubscriber.Publish(
                        new RedisChannel(RedisMessageChannels.ReferenceUpdate,
                            RedisChannel.PatternMode.Auto), json);
                }

                break;
            case RedisMessageChannels.PayFormReference:
                if (message.DocCode is null) return;
                var pf_item = GetItem<PayForm>((string)message.ExternalValues["RedisKey"]);
                pf_item.LoadFromCache();
                if (PayForms.ContainsKey(message.DocCode.Value))
                    PayForms[message.DocCode.Value] = pf_item;
                else PayForms.Add(message.DocCode.Value, pf_item);
                break;
            case RedisMessageChannels.ProjectReference:
                Projects.Clear();

                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var itemPrj in ctx.Projects.AsNoTracking().ToList())
                    {
                        var newItem = new Project();
                        newItem.LoadFromEntity(itemPrj, this);
                        if (!Projects.ContainsKey(newItem.Id))
                            Projects.Add(newItem.Id, newItem);
                    }

                    DropAllGuid<Project>();
                    UpdateListGuid(Projects.Values.Cast<Project>(), DateTime.Now);
                    GetProjectsAll();
                }

                break;
            case RedisMessageChannels.ProductTypeReference:
                if (message.DocCode is null) return;
                var pt_item = GetItem<ProductType>((string)message.ExternalValues["RedisKey"]);
                pt_item.LoadFromCache();
                if (ProductTypes.ContainsKey(message.DocCode.Value))
                    ProductTypes[message.DocCode.Value] = pt_item;
                else ProductTypes.Add(message.DocCode.Value, pt_item);
                break;
            case RedisMessageChannels.SDRSchetReference:
                using (var redisClient = redisManager.GetClient())
                {
                    if (!message.ExternalValues.ContainsKey("States") ||
                        !message.ExternalValues.ContainsKey("Schets")) return;
                    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                    //if (!message.ExternalValues.ContainsKey("DocCodes")) return;
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var lstStates = (message.ExternalValues["States"] as JArray).ToObject<List<decimal>>();
                        var lstSchets = (message.ExternalValues["Schets"] as JArray).ToObject<List<decimal>>();

                        var e_states = ctx.SD_99.Where(_ => lstStates.Contains(_.DOC_CODE));
                        foreach (var entity in e_states.Include(_ => _.SD_303))
                        {
                            foreach (var schet in entity.SD_303)
                            {
                                if (lstSchets.Contains(schet.DOC_CODE)) continue;
                                lstSchets.Add(schet.DOC_CODE);
                            }

                            var newItem = new SDRState
                            {
                                DocCode = entity.DOC_CODE,
                                Name = entity.SZ_NAME,
                                ParentDC = entity.SZ_PARENT_DC,
                                IsDohod = entity.SZ_1DOHOD_0_RASHOD == 1,
                                Shifr = entity.SZ_SHIFR,
                                UpdateDate = entity.UpdateDate ?? DateTime.Now
                            };
                            newItem.LoadFromEntity(entity);
                            AddOrUpdate(newItem);
                            if (SDRStates.ContainsKey(newItem.DocCode))
                                SDRStates[newItem.DocCode] = newItem;
                            else SDRStates.Add(newItem.DocCode, newItem);
                        }

                        var e_schets = ctx.SD_303.Where(_ => lstSchets.Contains(_.DOC_CODE));
                        foreach (var entity in e_schets)
                        {
                            var newItem = new SDRSchet
                            {
                                DocCode = entity.DOC_CODE,
                                Name = entity.SHPZ_NAME,
                                SDRStateDC = entity.SHPZ_STATIA_DC,
                                IsDeleted = entity.SHPZ_DELETED == 1,
                                UpdateDate = entity.UpdateDate ?? DateTime.Now
                            };
                            newItem.LoadFromEntity(entity, this);
                            AddOrUpdate(newItem);
                            if (SDRSchets.ContainsKey(newItem.DocCode))
                                SDRSchets[newItem.DocCode] = newItem;
                            else SDRSchets.Add(newItem.DocCode, newItem);
                        }
                    }

                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var redisMessage = new RedisMessage
                    {
                        Message =
                            $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник счетов и статей доходов/расъодов.",
                        ExternalValues =
                        {
                            ["Type"] = "SDRSchet"
                        }
                    };
                    var json = JsonConvert.SerializeObject(redisMessage, jsonSerializerSettings);
                    mySubscriber.Publish(
                        new RedisChannel(RedisMessageChannels.ReferenceUpdate,
                            RedisChannel.PatternMode.Auto), json);
                }

                break;
            case RedisMessageChannels.SDRStateReference:
                if (message.DocCode is null) return;
                var sdt_item = GetItem<SDRState>((string)message.ExternalValues["RedisKey"]);
                sdt_item.LoadFromCache();
                if (SDRStates.ContainsKey(message.DocCode.Value))
                    SDRStates[message.DocCode.Value] = sdt_item;
                else SDRStates.Add(message.DocCode.Value, sdt_item);
                break;
            case RedisMessageChannels.WarehouseReference:
                if (message.DocCode is null) return;
                var w_item = GetItem<Warehouse>((string)message.ExternalValues["RedisKey"]);
                w_item.LoadFromCache();
                if (Warehouses.ContainsKey(message.DocCode.Value))
                    Warehouses[message.DocCode.Value] = w_item;
                else Warehouses.Add(message.DocCode.Value, w_item);
                break;
            case RedisMessageChannels.UnitReference:
                if (message.DocCode is null) return;
                var u_item = GetItem<Unit>((string)message.ExternalValues["RedisKey"]);
                u_item.LoadFromCache();
                if (Units.ContainsKey(message.DocCode.Value))
                    Units[message.DocCode.Value] = u_item;
                else Units.Add(message.DocCode.Value, u_item);
                break;
            case RedisMessageChannels.KontragentReference:
                using (var redisClient = redisManager.GetClient())
                {
                    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                    if (message.DocCode is null) return;
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var entity = ctx.SD_43.Include(_ => _.SD_2).FirstOrDefault(_ => _.DOC_CODE == message.DocCode);
                        var newItem = new Kontragent
                        {
                            DocCode = entity.DOC_CODE,
                            StartBalans = entity.START_BALANS ?? new DateTime(2000, 1, 1),
                            Name = entity.NAME,
                            Notes = entity.NOTES,
                            IsBalans = entity.FLAG_BALANS == 1,
                            IsDeleted = entity.DELETED == 1,
                            Id = entity.Id,
                            CurrencyDC = entity.VALUTA_DC,
                            GroupDC = entity.EG_ID,
                            ResponsibleEmployeeDC = entity.SD_2?.DOC_CODE
                        };
                        newItem.LoadFromEntity(entity, GlobalOptions.ReferencesCache);
                        AddOrUpdate(newItem);
                        if (Kontragents.ContainsKey(message.DocCode.Value))
                            Kontragents[message.DocCode.Value] = newItem;
                        else Kontragents.Add(message.DocCode.Value, newItem);
                    }

                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var redisMessage = new RedisMessage
                    {
                        Message =
                            $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник контрагентов."
                    };
                    redisMessage.ExternalValues["Type"] = "Kontragent";
                    var json = JsonConvert.SerializeObject(redisMessage, jsonSerializerSettings);
                    mySubscriber.Publish(
                        new RedisChannel(RedisMessageChannels.ReferenceUpdate,
                            RedisChannel.PatternMode.Auto), json);
                }

                break;
            case RedisMessageChannels.NomenklReference:
                using (var redisClient = redisManager.GetClient())
                {
                    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                    if (message.DocCode is null) return;
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var nom = ctx.SD_83.Include(_ => _.NomenklMain)
                            .FirstOrDefault(_ => _.DOC_CODE == message.DocCode);
                        var nomItem = new Nomenkl
                        {
                            DocCode = nom.DOC_CODE,
                            Id = nom.Id,
                            Name = nom.NOM_NAME,
                            FullName =
                                nom.NOM_FULL_NAME,
                            Notes = nom.NOM_NOTES,
                            IsUsluga =
                                nom.NOM_0MATER_1USLUGA == 1,
                            IsProguct = nom.NOM_1PROD_0MATER == 1,
                            IsNakladExpense =
                                nom.NOM_1NAKLRASH_0NO == 1,
                            DefaultNDSPercent = (decimal?)nom.NOM_NDS_PERCENT,
                            IsDeleted =
                                nom.NOM_DELETED == 1,
                            IsUslugaInRentabelnost =
                                nom.IsUslugaInRent ?? false,
                            UpdateDate =
                                nom.UpdateDate ?? DateTime.MinValue,
                            MainId =
                                nom.MainId ?? Guid.Empty,
                            IsCurrencyTransfer = nom.NomenklMain.IsCurrencyTransfer ?? false,
                            NomenklNumber =
                                nom.NOM_NOMENKL,
                            NomenklTypeDC =
                                nom.NomenklMain.TypeDC,
                            ProductTypeDC = nom.NomenklMain.ProductDC,
                            UnitDC = nom.NOM_ED_IZM_DC,
                            CurrencyDC = nom.NOM_SALE_CRS_DC,
                            GroupDC = nom.NOM_CATEG_DC
                        };
                        nomItem.LoadFromEntity(nom, GlobalOptions.ReferencesCache);
                        AddOrUpdate(nomItem);
                        if (Nomenkls.ContainsKey(nomItem.DocCode))
                            Nomenkls[nomItem.DocCode] = nomItem;
                        else Nomenkls.Add(nomItem.DocCode, nomItem);
                    }
                }

                break;
            default:
                Console.WriteLine($"{channel} - не обработан");
                break;
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class NomGroupCount
    {
        public decimal DocCode { get; set; }
        public int Count { get; set; }
    }


    #region Dictionaries

    private readonly Dictionary<decimal, INomenklProductType> NomenklProductTypes = new();

    private readonly Dictionary<int, IKontragentGroup> KontragentGroups = new();

    // ReSharper disable once UnusedMember.Local
    private readonly Dictionary<decimal, IDeliveryCondition> DeliveryConditions = new();

    public Dictionary<decimal, IKontragent> Kontragents = new();
    public Dictionary<decimal, INomenkl> Nomenkls = new();
    public readonly Dictionary<Guid, INomenklMain> NomenklMains = new();

    private readonly Dictionary<decimal, INomenklGroup> NomenklGroups = new();

    private readonly Dictionary<decimal, IWarehouse> Warehouses = new();
    private readonly Dictionary<decimal, IEmployee> Employees = new();
    private readonly Dictionary<decimal, IBank> Banks = new();
    private Dictionary<decimal, IBankAccount> BankAccounts = new();

    private readonly Dictionary<decimal, ICentrResponsibility> CentrResponsibilities = new();

    private readonly Dictionary<decimal, ISDRSchet> SDRSchets = new();
    private readonly Dictionary<decimal, ISDRState> SDRStates = new();
    private readonly Dictionary<decimal, IClientCategory> ClientCategories = new();
    private readonly Dictionary<decimal, ICurrency> Currencies = new();
    private readonly Dictionary<decimal, IRegion> Regions = new();
    private readonly Dictionary<decimal, IUnit> Units = new();
    private Dictionary<decimal, ICashBox> CashBoxes = new();

    private readonly Dictionary<decimal, IMutualSettlementType> MutualSettlementTypes = new();

    private readonly Dictionary<Guid, ICountry> Countries = new();
    private readonly Dictionary<Guid, IProject> Projects = new();
    private readonly Dictionary<decimal, IContractType> ContractTypes = new();
    private readonly Dictionary<decimal, INomenklType> NomenklTypes = new();
    private readonly Dictionary<decimal, IProductType> ProductTypes = new();
    private readonly Dictionary<decimal, IPayForm> PayForms = new();
    private readonly Dictionary<decimal, IPayCondition> PayConditions = new();

    #endregion

    #region Generic Guid Id

    public void UpdateListGuid<T>(IEnumerable<T> list, DateTime? nowFix = null) where T : IDocGuid
    {
        ITypeSerializer<T> serializer = new JsonSerializer<T>();

        var items = list.Where(_ => _ is not null).ToList();
        if (!items.ToList().Any()) return;
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            using (var trans = redisClient.CreateTransaction())
            {
                foreach (var item in items)
                {
                    var strValue = serializer.SerializeToString(item);
                    trans.QueueCommand(r =>
                        r.SetValue($"Cache:{typeof(T).Name}:{item.Id}@{((ICache)item).UpdateDate}", strValue));
                }

                trans.Commit();
            }
        }
    }

    public void AddOrUpdateGuid<T>(T item) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var now = DateTime.Now;
            ((ICache)item).UpdateDate = now;
            var name = $"{typeof(T).Name}";
            var key = name switch
            {
                "Kontragent" or "Nomenkl" => $"Cache:{typeof(T).Name}:{((IDocCode)item).DocCode}:{item.Id}@{now}",
                _ => $"Cache:{typeof(T).Name}:{item.Id}@{now}"
            };
            var olds = redisClient.GetKeysByPattern($"{key.Split('@')[0]}*");
            foreach (var oldKey in olds) redisClient.Remove(oldKey);
            redisClient.Save();
            redis.SetValue(key, item);
            redisClient.Save();
            //var message = new RedisMessage
            //{
            //    DocumentType = DocumentType.None,
            //    Id = item.Id,
            //    DocDate = DateTime.Now,
            //    IsDocument = false,
            //    OperationType = RedisMessageDocumentOperationTypeEnum.Execute,
            //    Message =
            //        $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник {typeof(T).Name} '{item.Id}'"
            //};
            //message.ExternalValues.Add("RedisKey", key);
            //var jsonSerializerSettings = new JsonSerializerSettings
            //{
            //    TypeNameHandling = TypeNameHandling.All
            //};
            //redisClient.PublishMessage(getChannelName(typeof(T).Name),
            //    JsonConvert.SerializeObject(message, jsonSerializerSettings));
        }
    }

    public void DropAllGuid<T>() where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var keys = redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:*")
                .ToList();
            using (var trans = redisClient.CreateTransaction())
            {
                foreach (var key in keys) trans.QueueCommand(r => r.Remove(key));
                trans.Commit();
            }
        }
    }

    public void DropGuid<T>(Guid id) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var keys = redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:{id}*");
            var enumerable = keys.ToList();
            if (enumerable.Any())
                redisClient.Remove(enumerable.First());
        }
    }

    public T GetItemGuid<T>(Guid id) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var keys = redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:{id}*");
            var enumerable = keys.ToList();
            if (!enumerable.Any()) return default;
            var item = redis.GetValue(enumerable.First());
            var key = enumerable.First();
            var name = $"{typeof(T).Name}";
            switch (name)
            {
                case "Kontragent":
                case "Nomenkl":
                    //cacheKeysDict[name].CachKeys.AddCacheKey(new CachKey
                    //{
                    //    DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                    //    Id = Guid.Parse(key.Split('@')[0].Split(':')[3]),
                    //    LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                    //    Key = key
                    //});
                    break;
                case "Employee":
                    //cacheKeysDict[name].CachKeys.AddCacheKey(new CachKey
                    //{
                    //    DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                    //    TabelNumber = Convert.ToInt32(key.Split('@')[0].Split(':')[3]),
                    //    LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                    //    Key = key
                    //});
                    break;
                case "NomenklMain":
                case "Country":
                case "Project":
                    //cacheKeysDict[name].CachKeys.AddCacheKey(new CachKey
                    //{
                    //    Id = Guid.Parse(key.Split('@')[0].Split(':')[2]),
                    //    LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                    //    Key = key
                    //});
                    break;
            }


            ((ICache)item).UpdateDate = Convert.ToDateTime(enumerable.First().Split('@')[1]);
            if (!isStartLoad)
                ((ICache)item)?.LoadFromCache();
            return item;
        }
    }


    public T GetItemGuid<T>(string key) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var item = redis.GetValue(key) ?? GetItemGuid<T>(Guid.Parse(key.Split('@')[0].Split(':')[2]));
            ((ICache)item).UpdateDate = Convert.ToDateTime(key.Split('@')[1]);
            if (!isStartLoad)
                ((ICache)item)?.LoadFromCache();
            return item;
        }
    }

    public IEnumerable<T> GetAllGuid<T>() where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var keys = redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:*");
            var list = keys.Select(key => redis.GetValue(key)).ToList();

            if (isStartLoad) return list;
            foreach (var item in list)
                ((ICache)item).LoadFromCache();
            return list;
        }
    }

    public IEnumerable<T> GetListGuid<T>(IEnumerable<Guid> ids) where T : IDocGuid
    {
        if (ids is null) return Enumerable.Empty<T>();
        var idList = ids.ToList();
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var allKeys = redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:*").ToList();
            var keys = (from key in allKeys from id in idList where key.Contains(id.ToString()) select id.ToString())
                .ToList();
            var list = keys.Select(key => redis.GetValue(key.ToString())).ToList();
            return list;
        }
    }

    public IEnumerable<T> GetListDocCode<T>(IEnumerable<decimal> ids) where T : IDocGuid
    {
        if (ids is null) return Enumerable.Empty<T>();
        var idList = ids.ToList();
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var allKeys = redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:*").ToList();
            var keys = (from key in allKeys from id in idList where key.Contains(id.ToString()) select id.ToString())
                .ToList();
            var list = keys.Select(key => redis.GetValue(key.ToString())).ToList();
            return list;
        }
    }

    #endregion

    #region Generic decimal Id

    public void UpdateList<T>(IEnumerable<T> list, DateTime? nowFix = null) where T : IDocCode
    {
        ITypeSerializer<T> serializer = new JsonSerializer<T>();

        var items = list.Where(_ => _ is not null).ToList();
        if (!items.ToList().Any()) return;
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            using (var trans = redisClient.CreateTransaction())
            {
                foreach (var item in items)
                {
                    var strValue = serializer.SerializeToString(item);
                    trans.QueueCommand(r =>
                        r.SetValue($"Cache:{typeof(T).Name}:{item.DocCode}@{((ICache)item).UpdateDate}", strValue));
                }

                trans.Commit();
            }
        }
    }

    public bool IsKeyExists(string key)
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            return redisClient.ContainsKey(key);
        }
    }

    public void UpdateList2<T>(IEnumerable<T> list, DateTime? nowFix = null)
    {
        ITypeSerializer<T> serializer = new JsonSerializer<T>();

        var items = list.Where(_ => _ is not null).ToList();
        if (!items.ToList().Any()) return;
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            using (var trans = redisClient.CreateTransaction())
            {
                foreach (var item in items)
                {
                    var strValue = serializer.SerializeToString(item);
                    switch (typeof(T).Name)
                    {
                        case "Kontragent":
                        case "Nomenkl":
                            trans.QueueCommand(r =>
                                r.SetValue(
                                    $"Cache:{typeof(T).Name}:{((IDocCode)item).DocCode}:{((IDocGuid)item).Id}@{((ICache)item).UpdateDate}",
                                    strValue));
                            break;
                        case "Employee":
                            trans.QueueCommand(r =>
                                r.SetValue(
                                    $"Cache:{typeof(T).Name}:{((IDocCode)item).DocCode}:{((IEmployee)item).TabelNumber}@{((ICache)item).UpdateDate}",
                                    strValue));
                            break;
                    }
                }

                trans.Commit();
            }
        }
    }

    public void AddOrUpdate<T>(T item) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var now = DateTime.Now;
            ((ICache)item).UpdateDate = now;
            now = item switch
            {
                Nomenkl nom => nom.UpdateDate == default ? now : nom.UpdateDate,
                Kontragent kontr => kontr.UpdateDate == default ? now : kontr.UpdateDate,
                _ => now
            };
            var name = $"{typeof(T).Name}";

            var key = name switch
            {
                "Kontragent" => $"Cache:{typeof(T).Name}:{item.DocCode}:{((IDocGuid)item).Id}@{now}",
                "Nomenkl" => $"Cache:{typeof(T).Name}:{item.DocCode}:{((IDocGuid)item).Id}@{now}",
                "Employee" => $"Cache:{typeof(T).Name}:{item.DocCode}:{((IEmployee)item).TabelNumber}@{now}",
                _ => $"Cache:{typeof(T).Name}:{item.DocCode}@{now}"
            };
            var olds = redisClient.GetKeysByPattern($"{key.Split('@')[0]}*");
            foreach (var oldKey in olds) redisClient.Remove(oldKey);
            redisClient.Save();
            redis.SetValue(key, item);
        }
    }

    private string getChannelName(string typeName)
    {
        return typeName switch
        {
            "Kontragent" => RedisMessageChannels.KontragentReference,
            "CashBox" => RedisMessageChannels.CashBoxReference,
            "DeliveryCondition" => RedisMessageChannels.DeliveryConditionReference,
            "NomenklType" => RedisMessageChannels.NomenklTypeReference,
            "NomenklProductType" => RedisMessageChannels.NomenklProductTypeReference,
            "ProductType" => RedisMessageChannels.ProductTypeReference,
            "CentrResponsibility" => RedisMessageChannels.CentrResponsibilityReference,
            "Bank" => RedisMessageChannels.BankReference,
            "BankAccount" => RedisMessageChannels.BankAccountReference,
            "KontragentGroup" => RedisMessageChannels.KontragentGroupReference,
            "Nomenkl" => RedisMessageChannels.NomenklReference,
            "NomenklMain" => RedisMessageChannels.NomenklMainReference,
            "NomenklGroup" => RedisMessageChannels.NomenklGroupReference,
            "Warehouse" => RedisMessageChannels.WarehouseReference,
            "Employee" => RedisMessageChannels.EmployeeReference,
            "SDRSchet" => RedisMessageChannels.SDRSchetReference,
            "SDRState" => RedisMessageChannels.SDRStateReference,
            "Currency" => RedisMessageChannels.CurrencyReference,
            "Country" => RedisMessageChannels.CountryReference,
            "Region" => RedisMessageChannels.RegionReference,
            "Unit" => RedisMessageChannels.UnitReference,
            "MutualSettlementType" => RedisMessageChannels.MutualSettlementTypeReference,
            "Project" => RedisMessageChannels.ProjectReference,
            "PayForm" => RedisMessageChannels.PayFormReference,
            "PayCondition" => RedisMessageChannels.PayConditionReference,
            _ => string.Empty
        };
    }

    public void DropAll<T>() where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var keys = redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:*")
                .ToList();
            using (var trans = redisClient.CreateTransaction())
            {
                foreach (var key in keys) trans.QueueCommand(r => r.Remove(key));
                trans.Commit();
            }
        }
    }

    public void Drop<T>(decimal dc) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var keys = redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:{dc}*");
            var enumerable = keys.ToList();
            if (enumerable.Any())
                redisClient.Remove(enumerable.First());
        }
    }

    public T GetItem<T>(decimal dc) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var keys = redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:{dc}*");
            var enumerable = keys.ToList();
            if (!enumerable.Any()) return default;
            var item = redis.GetValue(enumerable.First());
            //var oldKey = cacheKeysDict[$"{typeof(T).Name}"].CachKeys.FirstOrDefault(_ => _.DocCode == dc);
            //if (oldKey is not null)
            //{
            //    oldKey.Key = enumerable.First();
            //}
            //else

            var key = enumerable.First();
            var name = $"Cache:{typeof(T).Name}";
            switch (name)
            {
                case "Kontragent":
                case "Nomenkl":
                    //cacheKeysDict[name].CachKeys.AddCacheKey(new CachKey
                    //{
                    //    DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                    //    Id = Guid.Parse(key.Split('@')[0].Split(':')[3]),
                    //    LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                    //    Key = key
                    //});
                    break;
                case "Employee":
                    //cacheKeysDict[name].CachKeys.AddCacheKey(new CachKey
                    //{
                    //    DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                    //    TabelNumber = Convert.ToInt32(key.Split('@')[0].Split(':')[3]),
                    //    LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                    //    Key = key
                    //});
                    break;
                case "NomenklMain":
                case "Country":
                case "Project":
                    //cacheKeysDict[name].CachKeys.AddCacheKey(new CachKey
                    //{
                    //    Id = Guid.Parse(key.Split('@')[0].Split(':')[2]),
                    //    LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                    //    Key = key
                    //});
                    break;
            }


            ((ICache)item).UpdateDate = Convert.ToDateTime(enumerable.First().Split('@')[1]);
            if (!isStartLoad)
                ((ICache)item)?.LoadFromCache();
            return item;
        }
    }

    public T GetItem<T>(string key) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var item = redis.GetValue(key) ?? GetItem<T>(Convert.ToDecimal(key.Split('@')[0].Split(':')[2]));
            ((ICache)item).UpdateDate = Convert.ToDateTime(key.Split('@')[1]);
            if (!isStartLoad)
                ((ICache)item)?.LoadFromCache();
            return item;
        }
    }

    public IEnumerable<T> GetAll<T>() where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var keys = redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:*");
            var list = keys.Select(key => redis.GetValue(key)).ToList();

            if (isStartLoad) return list;
            foreach (var item in list)
                ((ICache)item).LoadFromCache();
            return list;
        }
    }

    #endregion
}
