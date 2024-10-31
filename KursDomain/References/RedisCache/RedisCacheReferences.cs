using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using KursAM2.Repositories.RedisRepository;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.IReferences.Nomenkl;
using Newtonsoft.Json;
using ServiceStack.Redis;
using ServiceStack.Text;
using ServiceStack.Text.Common;

namespace KursDomain.References.RedisCache;

public interface ICache
{
    DateTime UpdateDate { set; get; }
    void LoadFromCache();
}

public record CachKey
{
    public decimal? DocCode { get; set; }
    public int? TabelNumber { set; get; }
    public Guid? Id { get; set; }
    public DateTime LastUpdate { set; get; }

    public string Key { set; get; }
}

public class CacheKeys
{
    public DateTime LoadMoment { set; get; }
    public List<CachKey> CachKeys { get; set; } = new List<CachKey>();
}

[SuppressMessage("ReSharper", "RedundantDictionaryContainsKeyBeforeAdding")]
public class RedisCacheReferences : IReferencesCache
{
    public const int MaxTimersSec = 10800; // 3 часа

    public readonly Dictionary<string, CacheKeys> cacheKeysDict = new Dictionary<string, CacheKeys>();

    private readonly RedisManagerPool redisManager =
        new RedisManagerPool(ConfigurationManager.AppSettings["redis.connection"]);

    public bool isStartLoad = true;

    public RedisCacheReferences()
    {
        isStartLoad = true;
        ThreadPool.QueueUserWorkItem(x =>
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
    public DbContext DBContext { get; }

    public ICashBox GetCashBox(decimal? dc)
    {
        if (dc is null) return null;
        return GetCashBox(dc.Value);
    }

    public ICashBox GetCashBox(decimal dc)
    {
        if (!cacheKeysDict.ContainsKey("CashBox"))
            LoadCacheKeys("CashBox");

        var key = cacheKeysDict["CashBox"].CachKeys.SingleOrDefault(_ => _.DocCode == dc);
        CashBox itemNew = null;
        if (key is not null)
        {
            if (CashBoxes.TryGetValue(dc, out var CashBox)) return CashBox;

            itemNew = GetItem<CashBox>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (CashBoxes.ContainsKey(dc))
            CashBoxes[dc] = itemNew;
        else
            CashBoxes.Add(dc, itemNew);

        return CashBoxes[dc];
    }

    public IEnumerable<ICashBox> GetCashBoxAll()
    {
        var cacheName = "CashBox";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return CashBoxes.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<CashBox>();
            CashBoxes.Clear();
            var keys = redisClient.GetKeysByPattern("Cache:CashBox:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in keys)
                    pipe.QueueCommand(r => r.GetValue(key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (CashBoxes.ContainsKey(x.DocCode))
                                CashBoxes[x.DocCode] = x;
                            else
                                CashBoxes.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return CashBoxes.Values.ToList();
    }

    public IDeliveryCondition GetDeliveryCondition(decimal dc)
    {
        return GetItem<DeliveryCondition>(dc);
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

        if (!cacheKeysDict.ContainsKey("NomenklType"))
            LoadCacheKeys("NomenklType");

        var key = cacheKeysDict["NomenklType"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        NomenklType itemNew = null;
        if (key is not null)
        {
            if (NomenklTypes.TryGetValue(dc.Value, out var NomenklType))
                return NomenklType;
            itemNew = GetItem<NomenklType>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (NomenklTypes.ContainsKey(dc.Value))
            NomenklTypes[dc.Value] = itemNew;
        else
            NomenklTypes.Add(dc.Value, itemNew);

        return NomenklTypes[dc.Value];
    }

    public IEnumerable<INomenklType> GetNomenklTypeAll()
    {
        var cacheName = "NomenklType";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return NomenklTypes.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<NomenklType>();
            NomenklTypes.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:NomenklType:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (NomenklTypes.ContainsKey(x.DocCode)) NomenklTypes[x.DocCode] = x;
                            else NomenklTypes.Add(x.DocCode, x);
                        });

                pipe.Flush();
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
        if (!cacheKeysDict.ContainsKey("NomenklProductType"))
            LoadCacheKeys("NomenklProductType");

        var key = cacheKeysDict["NomenklProductType"].CachKeys.SingleOrDefault(_ => _.DocCode == dc);
        NomenklProductType itemNew = null;
        if (key is not null)
        {
            if (NomenklProductTypes.TryGetValue(dc, out var NomenklProductType))
                return NomenklProductType;
            itemNew = GetItem<NomenklProductType>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (NomenklProductTypes.ContainsKey(dc))
            NomenklProductTypes[dc] = itemNew;
        else
            NomenklProductTypes.Add(dc, itemNew);

        return NomenklProductTypes[dc];
    }

    public IEnumerable<INomenklProductType> GetNomenklProductTypesAll()
    {
        var cacheName = "NomenklProductType";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return NomenklProductTypes.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<NomenklProductType>();
            NomenklProductTypes.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:NomenklProductType:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (NomenklProductTypes.ContainsKey(x.DocCode)) NomenklProductTypes[x.DocCode] = x;
                            else NomenklProductTypes.Add(x.DocCode, x);
                        });

                pipe.Flush();
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
        if (!cacheKeysDict.ContainsKey("ProductType"))
            LoadCacheKeys("ProductType");

        var key = cacheKeysDict["ProductType"].CachKeys.SingleOrDefault(_ => _.DocCode == dc);
        ProductType itemNew = null;
        if (key is not null)
        {
            if (ProductTypes.TryGetValue(dc, out var ProductType))
                return ProductType;
            itemNew = GetItem<ProductType>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (ProductTypes.ContainsKey(dc))
            ProductTypes[dc] = itemNew;
        else
            ProductTypes.Add(dc, itemNew);

        return ProductTypes[dc];
    }

    public IEnumerable<IProductType> GetProductTypeAll()
    {
        var cacheName = "ProductType";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return ProductTypes.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<ProductType>();
            ProductTypes.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:ProductType:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (ProductTypes.ContainsKey(x.DocCode)) ProductTypes[x.DocCode] = x;
                            else ProductTypes.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return ProductTypes.Values.ToList();
    }

    public ICentrResponsibility GetCentrResponsibility(decimal? dc)
    {
        if (dc is null) return null;

        if (!cacheKeysDict.ContainsKey("CentrResponsibility"))
            LoadCacheKeys("CentrResponsibility");

        var key = cacheKeysDict["CentrResponsibility"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        CentrResponsibility itemNew = null;
        if (key is not null)
        {
            if (CentrResponsibilities.TryGetValue(dc.Value, out var CentrResponsibility))
                return CentrResponsibility;
            itemNew = GetItem<CentrResponsibility>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (CentrResponsibilities.ContainsKey(dc.Value))
            CentrResponsibilities[dc.Value] = itemNew;
        else
            CentrResponsibilities.Add(dc.Value, itemNew);

        return CentrResponsibilities[dc.Value];
    }

    public IEnumerable<ICentrResponsibility> GetCentrResponsibilitiesAll()
    {
        var cacheName = "CentrResponsibility";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return CentrResponsibilities.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<CentrResponsibility>();
            CentrResponsibilities.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:CentrResponsibility:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (CentrResponsibilities.ContainsKey(x.DocCode)) CentrResponsibilities[x.DocCode] = x;
                            else CentrResponsibilities.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return CentrResponsibilities.Values.ToList();
    }

    public IBank GetBank(decimal? dc)
    {
        if (dc is null) return null;

        if (!cacheKeysDict.ContainsKey("Bank"))
            LoadCacheKeys("Bank");

        var key = cacheKeysDict["Bank"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        Bank itemNew = null;
        if (key is not null)
        {
            if (Banks.TryGetValue(dc.Value, out var Bank))
                return Bank;
            itemNew = GetItem<Bank>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (Banks.ContainsKey(dc.Value))
            Banks[dc.Value] = itemNew;
        else
            Banks.Add(dc.Value, itemNew);

        return Banks[dc.Value];
    }

    public IEnumerable<IBank> GetBanksAll()
    {
        var cacheName = "Bank";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return Banks.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<Bank>();
            Banks.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:Bank:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (Banks.ContainsKey(x.DocCode)) Banks[x.DocCode] = x;
                            else Banks.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return Banks.Values.ToList();
    }

    public IBankAccount GetBankAccount(decimal? dc)
    {
        if (dc is null) return null;

        if (!cacheKeysDict.ContainsKey("BankAccount"))
            LoadCacheKeys("BankAccount");

        var key = cacheKeysDict["BankAccount"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        BankAccount itemNew = null;
        if (key is not null)
        {
            if (BankAccounts.TryGetValue(dc.Value, out var BankAccount))
                return BankAccount;
            itemNew = GetItem<BankAccount>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (BankAccounts.ContainsKey(dc.Value))
            BankAccounts[dc.Value] = itemNew;
        else
            BankAccounts.Add(dc.Value, itemNew);

        return BankAccounts[dc.Value];
    }

    public IEnumerable<IBankAccount> GetBankAccountAll()
    {
        var cacheName = "BankAccount";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return BankAccounts.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<BankAccount>();
            BankAccounts.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:BankAccount:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (BankAccounts.ContainsKey(x.DocCode)) BankAccounts[x.DocCode] = x;
                            else BankAccounts.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return BankAccounts.Values.ToList();
    }

    public IKontragentGroup GetKontragentGroup(int? id)
    {
        if (id is null) return null;
        if (!cacheKeysDict.ContainsKey("KontragentGroup"))
            LoadCacheKeys("KontragentGroup");

        var key = cacheKeysDict["KontragentGroup"].CachKeys.SingleOrDefault(_ => _.DocCode == id.Value);
        KontragentGroup itemNew = null;
        if (key is not null)
        {
            if (KontragentGroups.TryGetValue(id.Value, out var KontragentGroup))
                return KontragentGroup;
            itemNew = GetItem<KontragentGroup>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (KontragentGroups.ContainsKey(id.Value))
            KontragentGroups[id.Value] = itemNew;
        else
            KontragentGroups.Add(id.Value, itemNew);

        return KontragentGroups[id.Value];
    }

    public IEnumerable<IKontragentGroup> GetKontragentCategoriesAll()
    {
        var cacheName = "KontragentGroup";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return KontragentGroups.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<KontragentGroup>();
            KontragentGroups.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:KontragentGroup:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            if (KontragentGroups.ContainsKey(x.Id))
                                KontragentGroups[x.Id] = x;
                            else
                                KontragentGroups.Add(x.Id, x);
                        });

                pipe.Flush();
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
        if (!cacheKeysDict.ContainsKey("Kontragent"))
            LoadCacheKeys("Kontragent");

        var key = cacheKeysDict["Kontragent"].CachKeys.SingleOrDefault(_ => _.DocCode == dc);
        Kontragent itemNew = null;
        if (key is not null)
        {
            if (Kontragents.TryGetValue(dc, out var Kontragent))
                return Kontragent;
            itemNew = GetItem<Kontragent>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (Kontragents.ContainsKey(dc))
            Kontragents[dc] = itemNew;
        else
            Kontragents.Add(dc, itemNew);

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
        var cacheName = "Kontragent";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (!cacheKeysDict.ContainsKey(cacheName))
                LoadCacheKeys(cacheName);

            if((Kontragents.Any() || (DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec)
               && Kontragents.Count == cacheKeysDict[cacheName].CachKeys.Count)
                return Kontragents.Values.ToList();
            
            var redis = redisClient.As<Kontragent>();
            Kontragents.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:Kontragent:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (Kontragents.ContainsKey(x.DocCode)) Kontragents[x.DocCode] = x;
                            else Kontragents.Add(x.DocCode, x);
                        });

                pipe.Flush();
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
                var redis = redisClient.As<Nomenkl>();
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
        if (!cacheKeysDict.ContainsKey("Nomenkl") || (DateTime.Now - cacheKeysDict["Nomenkl"].LoadMoment).TotalSeconds > MaxTimersSec)
            LoadCacheKeys("Nomenkl");
        var key = cacheKeysDict["Nomenkl"].CachKeys.SingleOrDefault(_ => _.DocCode == dc);
        Nomenkl itemNew = null;
        if (key is not null)
        {
            if (Nomenkls.TryGetValue(dc, out var Nomenkl))
                return Nomenkl;
            itemNew = GetItem<Nomenkl>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (Nomenkls.ContainsKey(dc))
            Nomenkls[dc] = itemNew;
        else
            Nomenkls.Add(dc, itemNew);

        return Nomenkls[dc];
    }

    public IEnumerable<INomenkl> GetNomenklsAll()
    {
        var cacheName = "Nomenkl";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return Nomenkls.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<Nomenkl>();
            Nomenkls.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:Nomenkl:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (Nomenkls.ContainsKey(x.DocCode)) Nomenkls[x.DocCode] = x;
                            else Nomenkls.Add(x.DocCode, x);
                        });

                pipe.Flush();
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
        if (!cacheKeysDict.ContainsKey("NomenklMain"))
            LoadCacheKeys("NomenklMain");

        var key = cacheKeysDict["NomenklMain"].CachKeys.SingleOrDefault(_ => _.Id == id);
        NomenklMain itemNew = null;
        if (key is not null)
        {
            if (NomenklMains.TryGetValue(id, out var NomenklMain))
                return NomenklMain;
            itemNew = GetItemGuid<NomenklMain>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (NomenklMains.ContainsKey(id))
            NomenklMains[id] = itemNew;
        else
            NomenklMains.Add(id, itemNew);

        return NomenklMains[id];
    }

    public IEnumerable<INomenklMain> GetNomenklMainAll()
    {
        var cacheName = "NomenklMain";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return NomenklMains.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<NomenklMain>();
            NomenklMains.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:NomenklMain:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (NomenklMains.ContainsKey(x.Id)) NomenklMains[x.Id] = x;
                            else NomenklMains.Add(x.Id, x);
                        });

                pipe.Flush();
            }
        }

        return NomenklMains.Values.ToList();
    }

    public IEnumerable<INomenkl> GetNomenkl(IEnumerable<decimal> dcList)
    {
        var cacheName = "Nomenkl";
        var list = dcList.ToList();
        if (!list.Any()) return new List<Nomenkl>();
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName))
                 LoadCacheKeys(cacheName);
            var redis = redisClient.As<Nomenkl>();
            var keys = (from dc in list.Where(_ => !Nomenkls.ContainsKey(_))
                select cacheKeysDict[cacheName].CachKeys.FirstOrDefault(_ => _.DocCode == dc)
                into t
                where t != null
                select t.Key).ToList();
            if (!keys.Any()) return Nomenkls.Keys.Where(list.Contains).Select(n => Nomenkls[n] as Nomenkl).ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in keys)
                    pipe.QueueCommand(r => r.GetValue(key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (Nomenkls.ContainsKey(x.DocCode)) Nomenkls[x.DocCode] = x;
                            else Nomenkls.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }

            return Nomenkls.Keys.Where(list.Contains).Select(n => Nomenkls[n] as Nomenkl).ToList();
        }
    }

    public INomenklGroup GetNomenklGroup(decimal? dc)
    {
        if (dc is null) return null;

        if (!cacheKeysDict.ContainsKey("NomenklGroup"))
            LoadCacheKeys("NomenklGroup");

        var key = cacheKeysDict["NomenklGroup"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        NomenklGroup itemNew = null;
        if (key is not null)
        {
            if (NomenklGroups.TryGetValue(dc.Value, out var NomenklGroup))
                return NomenklGroup;
            itemNew = GetItem<NomenklGroup>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (NomenklGroups.ContainsKey(dc.Value))
            NomenklGroups[dc.Value] = itemNew;
        else
            NomenklGroups.Add(dc.Value, itemNew);

        return NomenklGroups[dc.Value];
    }

    public IEnumerable<INomenklGroup> GetNomenklGroupAll()
    {
        var cacheName = "NomenklGroup";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return NomenklGroups.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<NomenklGroup>();
            NomenklGroups.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:NomenklGroup:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (NomenklGroups.ContainsKey(x.DocCode)) NomenklGroups[x.DocCode] = x;
                            else NomenklGroups.Add(x.DocCode, x);
                        });

                pipe.Flush();
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
        if (!cacheKeysDict.ContainsKey("Warehouse"))
            LoadCacheKeys("Warehouse");

        var key = cacheKeysDict["Warehouse"].CachKeys.SingleOrDefault(_ => _.DocCode == dc);
        Warehouse itemNew = null;
        if (key is not null)
        {
            if (Warehouses.TryGetValue(dc, out var Warehouse))
                return Warehouse;
            itemNew = GetItem<Warehouse>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (Warehouses.ContainsKey(dc))
            Warehouses[dc] = itemNew;
        else
            Warehouses.Add(dc, itemNew);

        return Warehouses[dc];
    }

    public IWarehouse GetWarehouse(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IWarehouse> GetWarehousesAll()
    {
        var cacheName = "Warehouse";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return Warehouses.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<Warehouse>();
            Warehouses.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:Warehouse:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (Warehouses.ContainsKey(x.DocCode))
                                Warehouses[x.DocCode] = x;
                            else
                                Warehouses.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return Warehouses.Values.ToList();
    }

    public IEmployee GetEmployee(int? tabelNumber)
    {
        if (tabelNumber is null) return null;
        decimal dc;
        var old = Employees.Values.FirstOrDefault(_ => _.TabelNumber == tabelNumber.Value);
        if (old is null)
        {
            if(!cacheKeysDict.ContainsKey("Employee"))
                LoadCacheKeys("Employee");
            var key = cacheKeysDict["Employee"].CachKeys.FirstOrDefault(_ => _.TabelNumber == tabelNumber);
            if (key is null)
            {
                using (var redisClient = redisManager.GetClient())
                {
                    redisClient.Db = GlobalOptions.RedisDBId ?? 0;
                    var redis = redisClient.As<Employee>();
                    var keys = redisClient.GetKeysByPattern($"Cache:Employee:*:{tabelNumber}@*");
                    var enumerable = keys.ToList();
                    if (!enumerable.Any()) return default;
                    dc = Convert.ToDecimal(enumerable.First().Split('@')[0].Split(':')[2]);
                }
            }
            else
            {
                var itemNew = GetItem<Employee>(key.Key);
                if (itemNew is null) return null;
                itemNew.LoadFromCache();
                if (Employees.ContainsKey(itemNew.DocCode))
                    Employees[itemNew.DocCode] = itemNew;
                else
                    Employees.Add(itemNew.DocCode, itemNew);

                return Employees[itemNew.DocCode];
            }
        }
        else
        {
            dc = ((IDocCode)old).DocCode;
        }

        return GetEmployee(dc);
    }

    public IEmployee GetEmployee(decimal? dc)
    {
        if (dc is null) return null;
        if (!cacheKeysDict.ContainsKey("Employee"))
            LoadCacheKeys("Employee");

        var key = cacheKeysDict["Employee"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        Employee itemNew = null;
        if (key is not null)
        {
            if (Employees.TryGetValue(dc.Value, out var Employee))
                return Employee;
            itemNew = GetItem<Employee>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (Employees.ContainsKey(dc.Value))
            Employees[dc.Value] = itemNew;
        else
            Employees.Add(dc.Value, itemNew);

        return Employees[dc.Value];
    }

    public IEmployee GetEmployee(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IEmployee> GetEmployees()
    {
        var cacheName = "Employee";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return Employees.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<Employee>();
            Employees.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:Employee:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (Employees.ContainsKey(x.DocCode))
                                Employees[x.DocCode] = x;
                            else
                                Employees.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return Employees.Values.ToList();
    }

    public ISDRSchet GetSDRSchet(decimal? dc)
    {
        if (dc is null) return null;
        if (!cacheKeysDict.ContainsKey("SDRSchet"))
            LoadCacheKeys("SDRSchet");

        var key = cacheKeysDict["SDRSchet"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        SDRSchet itemNew = null;
        if (key is not null)
        {
            if (SDRSchets.TryGetValue(dc.Value, out var SDRSchet))
                return SDRSchet;
            itemNew = GetItem<SDRSchet>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (SDRSchets.ContainsKey(dc.Value))
            SDRSchets[dc.Value] = itemNew;
        else
            SDRSchets.Add(dc.Value, itemNew);

        return SDRSchets[dc.Value];
    }

    public ISDRSchet GetSDRSchet(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISDRSchet> GetSDRSchetAll()
    {
        var cacheName = "SDRSchet";
        if (cacheKeysDict.ContainsKey(cacheName) &&
            !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
            return SDRSchets.Values.ToList();
        LoadCacheKeys(cacheName);
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;

            var redis = redisClient.As<SDRSchet>();
            SDRSchets.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:SDRSchet:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key), x =>
                    {
                        x.LoadFromCache();
                        if (SDRSchets.ContainsKey(x.DocCode))
                            SDRSchets[x.DocCode] = x;
                        else
                            SDRSchets.Add(x.DocCode, x);
                    });

                pipe.Flush();
            }
        }

        return SDRSchets.Values.ToList();
    }

    public ISDRState GetSDRState(decimal? dc)
    {
        if (dc is null) return null;
        if (!cacheKeysDict.ContainsKey("SDRState"))
            LoadCacheKeys("SDRState");

        var key = cacheKeysDict["SDRState"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        SDRState itemNew = null;
        if (key is not null)
        {
            if (SDRStates.TryGetValue(dc.Value, out var SDRState))
                return SDRState;
            itemNew = GetItem<SDRState>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (SDRStates.ContainsKey(dc.Value))
            SDRStates[dc.Value] = itemNew;
        else
            SDRStates.Add(dc.Value, itemNew);

        return SDRStates[dc.Value];
    }

    public ISDRState GetSDRState(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISDRState> GetSDRStateAll()
    {
        var cacheName = "SDRState";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return SDRStates.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<SDRState>();
            SDRStates.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:SDRState:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (SDRStates.ContainsKey(x.DocCode))
                                SDRStates[x.DocCode] = x;
                            else
                                SDRStates.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return SDRStates.Values.ToList();
    }

    public IClientCategory GetClientCategory(decimal? dc)
    {
        if (dc is null) return null;
        if (!cacheKeysDict.ContainsKey("ClientCategory"))
            LoadCacheKeys("ClientCategory");

        var key = cacheKeysDict["ClientCategory"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        ClientCategory itemNew = null;
        if (key is not null)
        {
            if (ClientCategories.TryGetValue(dc.Value, out var ClientCategory))
                return ClientCategory;
            itemNew = GetItem<ClientCategory>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (ClientCategories.ContainsKey(dc.Value))
            ClientCategories[dc.Value] = itemNew;
        else
            ClientCategories.Add(dc.Value, itemNew);

        return ClientCategories[dc.Value];
    }

    public IClientCategory GetClientCategory(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IClientCategory> GetClientCategoriesAll()
    {
        var cacheName = "ClientCategory";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return ClientCategories.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<ClientCategory>();
            ClientCategories.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:ClientCategory:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (ClientCategories.ContainsKey(x.DocCode))
                                ClientCategories[x.DocCode] = x;
                            else
                                ClientCategories.Add(x.DocCode, x);
                        });

                pipe.Flush();
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
        if (!cacheKeysDict.ContainsKey("Currency"))
            LoadCacheKeys("Currency");

        var key = cacheKeysDict["Currency"].CachKeys.SingleOrDefault(_ => _.DocCode == dc);
        Currency itemNew = null;
        if (key is not null)
        {
            if (Currencies.TryGetValue(dc, out var Currency))
                return Currency;
            itemNew = GetItem<Currency>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (Currencies.ContainsKey(dc))
            Currencies[dc] = itemNew;
        else
            Currencies.Add(dc, itemNew);

        return Currencies[dc];
    }

    public IEnumerable<ICurrency> GetCurrenciesAll()
    {
        var cacheName = "Currency";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return Currencies.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<Currency>();
            Currencies.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:Currency:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict["Currency"].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (Currencies.ContainsKey(x.DocCode))
                                Currencies[x.DocCode] = x;
                            else
                                Currencies.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return Currencies.Values.ToList();
    }

    public ICountry GetCountry(Guid? id)
    {
        if (id is null) return null;
        if (!cacheKeysDict.ContainsKey("Country"))
            LoadCacheKeys("Country");

        var key = cacheKeysDict["Country"].CachKeys.SingleOrDefault(_ => _.Id == id.Value);
        Country itemNew = null;
        if (key is not null)
        {
            if (Countries.TryGetValue(id.Value, out var Country))
                return Country;
            itemNew = GetItemGuid<Country>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (Countries.ContainsKey(id.Value))
            Countries[id.Value] = itemNew;
        else
            Countries.Add(id.Value, itemNew);

        return Countries[id.Value];
    }

    public IEnumerable<ICountry> GetCountriesAll()
    {
        var cacheName = "Country";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return Countries.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<Country>();
            Countries.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:Country:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (Countries.ContainsKey(x.Id))
                                Countries[x.Id] = x;
                            else
                                Countries.Add(x.Id, x);
                        });

                pipe.Flush();
            }
        }

        return Countries.Values.ToList();
    }

    public IRegion GetRegion(decimal? dc)
    {
        if (dc is null) return null;
        if (!cacheKeysDict.ContainsKey("Region"))
            LoadCacheKeys("Region");

        var key = cacheKeysDict["Region"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        Region itemNew = null;
        if (key is not null)
        {
            if (Regions.TryGetValue(dc.Value, out var Region))
                return Region;
            itemNew = GetItem<Region>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (Regions.ContainsKey(dc.Value))
            Regions[dc.Value] = itemNew;
        else
            Regions.Add(dc.Value, itemNew);

        return Regions[dc.Value];
    }

    public IRegion GetRegion(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IRegion> GetRegionsAll()
    {
        var cacheName = "Region";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return Regions.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<Region>();
            Regions.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:Region:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (Regions.ContainsKey(x.DocCode)) Regions[x.DocCode] = x;
                            else Regions.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return Regions.Values.ToList();
    }

    public IUnit GetUnit(decimal? dc)
    {
        if (dc is null) return null;
        if (!cacheKeysDict.ContainsKey("Unit"))
            LoadCacheKeys("Unit");

        var key = cacheKeysDict["Unit"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        Unit itemNew = null;
        if (key is not null)
        {
            if (Units.TryGetValue(dc.Value, out var Unit))
                return Unit;
            itemNew = GetItem<Unit>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (Units.ContainsKey(dc.Value))
            Units[dc.Value] = itemNew;
        else
            Units.Add(dc.Value, itemNew);

        return Units[dc.Value];
    }

    public IUnit GetUnit(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IUnit> GetUnitsAll()
    {
        var cacheName = "Unit";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return Units.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<Unit>();
            Units.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:Unit:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (Units.ContainsKey(x.DocCode))
                                Units[x.DocCode] = x;
                            else
                                Units.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return Units.Values.ToList();
    }

    public IMutualSettlementType GetMutualSettlementType(decimal? dc)
    {
        if (dc is null) return null;

        if (!cacheKeysDict.ContainsKey("MutualSettlementType"))
            LoadCacheKeys("MutualSettlementType");

        var key = cacheKeysDict["MutualSettlementType"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        MutualSettlementType itemNew = null;
        if (key is not null)
        {
            if (MutualSettlementTypes.TryGetValue(dc.Value, out var MutualSettlementType))
                return MutualSettlementType;
            itemNew = GetItem<MutualSettlementType>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (MutualSettlementTypes.ContainsKey(dc.Value))
            MutualSettlementTypes[dc.Value] = itemNew;
        else
            MutualSettlementTypes.Add(dc.Value, itemNew);

        return MutualSettlementTypes[dc.Value];
    }

    public IEnumerable<IMutualSettlementType> GetMutualSettlementTypeAll()
    {
        var cacheName = "MutualSettlementType";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return MutualSettlementTypes.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<MutualSettlementType>();
            MutualSettlementTypes.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:MutualSettlementType:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (MutualSettlementTypes.ContainsKey(x.DocCode))
                                MutualSettlementTypes[x.DocCode] = x;
                            else
                                MutualSettlementTypes.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return MutualSettlementTypes.Values.ToList();
    }

    public IProject GetProject(Guid? id)
    {
        if (id is null) return null;

        if (!cacheKeysDict.ContainsKey("Project"))
            LoadCacheKeys("Project");

        var key = cacheKeysDict["Project"].CachKeys.SingleOrDefault(_ => _.Id == id.Value);
        Project itemNew = null;
        if (key is not null)
        {
            if (Projects.TryGetValue(id.Value, out var Project))
                return Project;
            itemNew = GetItemGuid<Project>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (Projects.ContainsKey(id.Value))
            Projects[id.Value] = itemNew;
        else
            Projects.Add(id.Value, itemNew);

        return Projects[id.Value];
    }

    public IEnumerable<IProject> GetProjectsAll()
    {
        var cacheName = "Project";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if(!cacheKeysDict.ContainsKey(cacheName))
                LoadCacheKeys(cacheName);
            if (!((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return Projects.Values.ToList();
            var redis = redisClient.As<Project>();
            Projects.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:Project:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (Projects.ContainsKey(x.Id)) Projects[x.Id] = x;
                            else Projects.Add(x.Id, x);
                        });

                pipe.Flush();
            }
        }

        return Projects.Values.ToList();
    }

    public IContractType GetContractType(decimal? dc)
    {
        if (dc is null) return null;

        if (!cacheKeysDict.ContainsKey("ContractType"))
            LoadCacheKeys("ContractType");

        var key = cacheKeysDict["ContractType"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        ContractType itemNew = null;
        if (key is not null)
        {
            if (ContractTypes.TryGetValue(dc.Value, out var ContractType))
                return ContractType;
            itemNew = GetItem<ContractType>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (ContractTypes.ContainsKey(dc.Value))
            ContractTypes[dc.Value] = itemNew;
        else
            ContractTypes.Add(dc.Value, itemNew);

        return ContractTypes[dc.Value];
    }

    public IEnumerable<IContractType> GetContractsTypeAll()
    {
        var cacheName = "ContractType";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return ContractTypes.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<ContractType>();
            ContractTypes.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:ContractType:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (ContractTypes.ContainsKey(x.DocCode))
                                ContractTypes[x.DocCode] = x;
                            else
                                ContractTypes.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return ContractTypes.Values.ToList();
    }

    public IPayForm GetPayForm(decimal? dc)
    {
        if (dc is null) return null;
        if (!cacheKeysDict.ContainsKey("PayForm"))
            LoadCacheKeys("PayForm");

        var key = cacheKeysDict["PayForm"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        PayForm itemNew = null;
        if (key is not null)
        {
            if (PayForms.TryGetValue(dc.Value, out var PayForm))
                return PayForm;
            itemNew = GetItem<PayForm>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (PayForms.ContainsKey(dc.Value))
            PayForms[dc.Value] = itemNew;
        else
            PayForms.Add(dc.Value, itemNew);

        return PayForms[dc.Value];
    }

    public IEnumerable<IPayForm> GetPayFormAll()
    {
        var cacheName = "PayForm";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return PayForms.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<PayForm>();
            PayForms.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:PayForm:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (PayForms.ContainsKey(x.DocCode))
                                PayForms[x.DocCode] = x;
                            else
                                PayForms.Add(x.DocCode, x);
                        });

                pipe.Flush();
            }
        }

        return PayForms.Values.ToList();
    }

    public IPayCondition GetPayCondition(decimal? dc)
    {
        if (dc is null) return null;
        if (!cacheKeysDict.ContainsKey("PayCondition"))
            LoadCacheKeys("PayCondition");

        var key = cacheKeysDict["PayCondition"].CachKeys.SingleOrDefault(_ => _.DocCode == dc.Value);
        PayCondition itemNew = null;
        if (key is not null)
        {
            if (PayConditions.TryGetValue(dc.Value, out var PayCondition))
                return PayCondition;
            itemNew = GetItem<PayCondition>(key.Key);
            if (itemNew is null) return null;
            itemNew.LoadFromCache();
        }
        else
        {
            return null;
        }

        if (PayConditions.ContainsKey(dc.Value))
            PayConditions[dc.Value] = itemNew;
        else
            PayConditions.Add(dc.Value, itemNew);

        return PayConditions[dc.Value];
    }

    public IEnumerable<IPayCondition> GetPayConditionAll()
    {
        var cacheName = "PayCondition";
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (cacheKeysDict.ContainsKey(cacheName) &&
                !((DateTime.Now - cacheKeysDict[cacheName].LoadMoment).TotalSeconds > MaxTimersSec))
                return PayConditions.Values.ToList();
            LoadCacheKeys(cacheName);
            var redis = redisClient.As<PayCondition>();
            PayConditions.Clear();
            //var keys = redisClient.GetKeysByPattern("Cache:PayCondition:*").ToList();
            using (var pipe = redis.CreatePipeline())
            {
                foreach (var key in cacheKeysDict[cacheName].CachKeys)
                    pipe.QueueCommand(r => r.GetValue(key.Key),
                        x =>
                        {
                            x.LoadFromCache();
                            if (PayConditions.ContainsKey(x.DocCode))
                                PayConditions[x.DocCode] = x;
                            else
                                PayConditions.Add(x.DocCode, x);
                        });

                pipe.Flush();
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
                foreach (var item in Context.SD_301.AsNoTracking().ToList())
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

                foreach (var item in Context.SD_2.AsNoTracking().ToList())
                {
                    var newItem = new Employee();
                    newItem.LoadFromEntity(item, this);
                    if (!Employees.ContainsKey(newItem.DocCode))
                        Employees.Add(newItem.DocCode, newItem);
                }

                foreach (var item in Employees.Values.Cast<Employee>())
                    item.CurrencyDC = ((IDocCode)item.Currency)?.DocCode;
                DropAll<Employee>();
                UpdateList2(Employees.Values.Cast<Employee>(), now);
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

                Kontragents = (from entity in Context.SD_43.Include(_ => _.SD_2)
                        select new Kontragent
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
                            ResponsibleEmployeeDC = entity.SD_2.DOC_CODE
                        })
                    .ToDictionary<Kontragent, decimal, IKontragent>(newItem => newItem.DocCode, newItem => newItem);
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
                        BankDC = entity.BA_BANKDC
                    })
                    .ToDictionary<BankAccount, decimal, IBankAccount>(newItem => newItem.DocCode, newItem => newItem);

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
                {
                    grp.NomenklCount = getCountNomenklGroup(grp, dictCount);
                }

                DropAll<NomenklGroup>();
                UpdateList(NomenklGroups.Values.Cast<NomenklGroup>(), now);
                GetNomenklGroupAll();

                foreach (var entity in Context.NomenklMain.AsNoTracking().ToList())
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
                        ProductTypeDC = entity.ProductDC
                    };
                    //item.LoadFromEntity(entity,this);
                    NomenklMains.Add(item.Id, item);
                }

                DropAllGuid<NomenklMain>();
                UpdateListGuid(NomenklMains.Values.Cast<NomenklMain>(), now);

                Nomenkls = new Dictionary<decimal, INomenkl>();
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
                    //item.LoadFromEntity(entity,this);
                    Nomenkls.Add(item.DocCode, item);
                }

                DropAll<Nomenkl>();
                UpdateList2(Nomenkls.Values.Cast<Nomenkl>(), now);
            }
            else
            {
                Task.Run(() =>
                {
                    GetCurrenciesAll();
                    GetClientCategoriesAll();
                    GetKontragentCategoriesAll();
                    GetCentrResponsibilitiesAll();
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
                    GetSDRStateAll();
                    GetSDRSchetAll();
                    GetWarehousesAll();
                    GetDeliveryConditionAll();
                   
                    //GetNomenklMainAll();
                    //GetNomenklsAll();
                });
            }
        }

        //ClearAll();
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

    private void LoadCacheKeysAll()
    {
        LoadCacheKeys("Kontragent");
        LoadCacheKeys("CashBox");
        LoadCacheKeys("DeliveryCondition");
        LoadCacheKeys("NomenklType");
        LoadCacheKeys("NomenklProductType");
        LoadCacheKeys("ProductType");
        LoadCacheKeys("CentrResponsibility");
        LoadCacheKeys("Bank");
        LoadCacheKeys("BankAccount");
        LoadCacheKeys("KontragentGroup");
        LoadCacheKeys("Nomenkl");
        LoadCacheKeys("NomenklMain");
        LoadCacheKeys("NomenklGroup");
        LoadCacheKeys("Warehouse");
        LoadCacheKeys("Employee");
        LoadCacheKeys("SDRSchet");
        LoadCacheKeys("SDRState");
        LoadCacheKeys("Currency");
        LoadCacheKeys("Country");
        LoadCacheKeys("Region");
        LoadCacheKeys("Unit");
        LoadCacheKeys("MutualSettlementType");
        LoadCacheKeys("Project");
        LoadCacheKeys("PayForm");
        LoadCacheKeys("PayCondition");
    }

    private void LoadCacheKeys(string name)
    {
        if (!cacheKeysDict.ContainsKey(name))
            cacheKeysDict.Add(name, new CacheKeys());
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var keys = redisClient.GetKeysByPattern($"Cache:{name}:*").ToList();
            cacheKeysDict[name].CachKeys.Clear();
            foreach (var key in keys)
                switch (name)
                {
                    case "Kontragent":
                    case "Nomenkl":
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                            Id = Guid.Parse(key.Split('@')[0].Split(':')[3]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                    case "Employee":
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                            TabelNumber = Convert.ToInt32(key.Split('@')[0].Split(':')[3]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                    case "NomenklMain":
                    case "Country":
                    case "Project":
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            Id = Guid.Parse(key.Split('@')[0].Split(':')[2]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                    default:
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                }

            cacheKeysDict[name].LoadMoment = DateTime.Now;
        }
    }

    private void UpdateReferences(string channel, string msg)
    {
        if (string.IsNullOrWhiteSpace(msg)) return;
        var message = JsonConvert.DeserializeObject<RedisMessage>(msg);
        if (message == null || message.DbId != GlobalOptions.DataBaseId) return;

        using (var Context = GlobalOptions.GetEntities())
        {
            switch (channel)
            {
                case RedisMessageChannels.BankReference:
                    if (message.DocCode is null) return;
                    GetBank(message.DocCode);
                    break;
                case RedisMessageChannels.RegionReference:
                    if (message.DocCode is null) return;
                    GetRegion(message.DocCode);
                    break;
                case RedisMessageChannels.BankAccountReference:
                    if (message.DocCode is null) return;
                    GetBankAccount(message.DocCode);
                    break;
                case RedisMessageChannels.CashBoxReference:
                    if (message.DocCode is null) return;
                    GetCashBox(message.DocCode);
                    break;
                case RedisMessageChannels.CentrResponsibilityReference:
                    if (message.DocCode is null) return;
                    GetCentrResponsibility(message.DocCode);
                    break;
                case RedisMessageChannels.ClientCategoryReference:
                    if (message.DocCode is null) return;
                    GetClientCategory(message.DocCode);
                    break;
                case RedisMessageChannels.ContractTypeReference:
                    if (message.DocCode is null) return;
                    GetContractType(message.DocCode);
                    break;
                case RedisMessageChannels.CountryReference:
                    if (message.Id is null) return;
                    GetCountry(message.Id);
                    break;
                case RedisMessageChannels.CurrencyReference:
                    if (message.DocCode is null) return;
                    GetCurrency(message.DocCode);
                    break;
                case RedisMessageChannels.DeliveryConditionReference:
                    if (message.DocCode is null) return;
                    GetDeliveryCondition(message.DocCode);
                    break;
                case RedisMessageChannels.EmployeeReference:
                    if (message.DocCode is null) return;
                    GetEmployee(message.DocCode);
                    break;
                case RedisMessageChannels.KontragentGroupReference:
                    if (message.DocCode is null) return;
                    GetKontragentGroup(Convert.ToInt32(message.DocCode));
                    break;
                case RedisMessageChannels.MutualSettlementTypeReference:
                    if (message.DocCode is null) return;
                    GetMutualSettlementType(message.DocCode);
                    break;
                case RedisMessageChannels.NomenklGroupReference:
                    if (message.DocCode is null) return;
                    GetNomenklGroup(message.DocCode);
                    break;
                case RedisMessageChannels.NomenklMainReference:
                    if (message.Id is null) return;
                    var nm_item = GetItemGuid<NomenklMain>((string)message.ExternalValues["RedisKey"]);
                    nm_item.LoadFromCache();
                    var nm_oldKey = cacheKeysDict["NomenklMain"].CachKeys.SingleOrDefault(_ => _.Id == message.Id);
                    if (nm_oldKey is null)
                    {
                        cacheKeysDict["NomenklMain"].CachKeys.Add(new CachKey()
                        {
                            Key = (string)message.ExternalValues["RedisKey"],
                            //DocCode = message.DocCode,
                            Id = nm_item.Id,
                            LastUpdate = Convert.ToDateTime(((string)message.ExternalValues["RedisKey"]).Split("@"[1]))
                        });
                    }

                    if (NomenklMains.ContainsKey(message.Id.Value))
                        NomenklMains[message.Id.Value] = nm_item;
                    else NomenklMains.Add(message.Id.Value,nm_item);
                    break;
                case RedisMessageChannels.NomenklProductTypeReference:
                    if (message.DocCode is null) return;
                    GetNomenklProductType(message.DocCode);
                    break;
                case RedisMessageChannels.NomenklTypeReference:
                    if (message.DocCode is null) return;
                    GetNomenklType(message.DocCode);
                    break;
                case RedisMessageChannels.PayConditionReference:
                    if (message.DocCode is null) return;
                    GetPayCondition(message.DocCode);
                    break;
                case RedisMessageChannels.PayFormReference:
                    if (message.DocCode is null) return;
                    GetPayForm(message.DocCode);
                    break;
                case RedisMessageChannels.ProjectReference:
                    if (message.Id is null) return;
                    GetProject(message.Id);
                    break;
                case RedisMessageChannels.ProductTypeReference:
                    if (message.DocCode is null) return;
                    GetProductType(message.DocCode);
                    break;
                case RedisMessageChannels.SDRSchetReference:
                    if (message.DocCode is null) return;
                    GetSDRSchet(message.DocCode);
                    break;
                case RedisMessageChannels.SDRStateReference:
                    if (message.DocCode is null) return;
                    GetSDRState(message.DocCode);
                    break;
                case RedisMessageChannels.WarehouseReference:
                    if (message.DocCode is null) return;
                    GetWarehouse(message.DocCode);
                    break;
                case RedisMessageChannels.UnitReference:
                    if (message.DocCode is null) return;
                    GetUnit(message.DocCode);
                    break;
                case RedisMessageChannels.KontragentReference:
                    if (message.DocCode is null) return;
                    var k_item = GetItem<Kontragent>((string)message.ExternalValues["RedisKey"]);
                    k_item.LoadFromCache();
                    var k_oldKey = cacheKeysDict["Kontragent"].CachKeys.SingleOrDefault(_ => _.DocCode == message.DocCode);
                    if (k_oldKey is null)
                    {
                        cacheKeysDict["Kontragent"].CachKeys.Add(new CachKey()
                        {
                            Key = (string)message.ExternalValues["RedisKey"],
                            DocCode = message.DocCode,
                            Id = k_item.Id,
                            LastUpdate = Convert.ToDateTime(((string)message.ExternalValues["RedisKey"]).Split("@"[1]))
                        });
                    }

                    if (Kontragents.ContainsKey(message.DocCode.Value))
                        Kontragents[message.DocCode.Value] = k_item;
                    else Kontragents.Add(message.DocCode.Value,k_item);
                    break;
                case RedisMessageChannels.NomenklReference:
                    if (message.DocCode is null) return;
                    if(!cacheKeysDict.ContainsKey("Nomenkl"))
                        LoadCacheKeys("Nomenkl");
                    var item = GetItem<Nomenkl>((string)message.ExternalValues["RedisKey"]);
                    item.LoadFromCache();
                    var oldKey = cacheKeysDict["Nomenkl"].CachKeys.SingleOrDefault(_ => _.DocCode == message.DocCode);
                    if (oldKey is null)
                    {
                        cacheKeysDict["Nomenkl"].CachKeys.Add(new CachKey()
                        {
                            Key = (string)message.ExternalValues["RedisKey"],
                            DocCode = message.DocCode,
                            Id = item.Id,
                            LastUpdate = Convert.ToDateTime(((string)message.ExternalValues["RedisKey"]).Split("@"[1]))
                        });
                    }

                    if (Nomenkls.ContainsKey(message.DocCode.Value))
                        Nomenkls[message.DocCode.Value] = item;
                    else Nomenkls.Add(message.DocCode.Value,item);
                    break;
                default:
                    Console.WriteLine($"{channel} - не обработан");
                    break;
            }
        }
    }

    private void ClearAll()
    {
        /*CashBoxes.Clear();
        CentrResponsibilities.Clear();
        ClientCategories.Clear();
        ContractTypes.Clear();
        Countries.Clear();
        Currencies.Clear();
        Employees.Clear();
        KontragentGroups.Clear();
        MutualSettlementTypes.Clear();
        NomenklGroups.Clear();
        NomenklProductTypes.Clear();
        NomenklTypes.Clear();
        PayConditions.Clear();
        PayForms.Clear();
        ProductTypes.Clear();
        Projects.Clear();
        Regions.Clear();
        SDRSchets.Clear();
        SDRStates.Clear();
        Units.Clear();
        Warehouses.Clear();
        DeliveryConditions.Clear();*/
        Kontragents.Clear();
        NomenklMains.Clear();
        Nomenkls.Clear();
    }

    private bool IsTimerOut<T>(T item) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (!cacheKeysDict.ContainsKey(typeof(T).Name) ||
                (DateTime.Now - cacheKeysDict[typeof(T).Name].LoadMoment).TotalSeconds > MaxTimersSec)
                LoadCacheKeys(typeof(T).Name);
            var s = cacheKeysDict[typeof(T).Name].CachKeys.FirstOrDefault(_ => _.DocCode == item.DocCode);
            return ((ICache)item).UpdateDate < (s?.LastUpdate ?? DateTime.MaxValue);
        }
    }

    private bool IsTimerOutGuid<T>(T item) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            if (!cacheKeysDict.ContainsKey(typeof(T).Name) ||
                (DateTime.Now - cacheKeysDict[typeof(T).Name].LoadMoment).TotalSeconds > MaxTimersSec)
                LoadCacheKeys(typeof(T).Name);
            var s = cacheKeysDict[typeof(T).Name].CachKeys.FirstOrDefault(_ => _.Id == item.Id);
            return ((ICache)item).UpdateDate < (s?.LastUpdate ?? DateTime.MaxValue);
        }
    }


    private class NomGroupCount
    {
        public decimal DocCode { get; set; }
        public int Count { get; set; }
    }

    #region Dictionaries

    private readonly Dictionary<decimal, INomenklProductType> NomenklProductTypes
        = new Dictionary<decimal, INomenklProductType>();

    private readonly Dictionary<int, IKontragentGroup> KontragentGroups =
        new Dictionary<int, IKontragentGroup>();

    private readonly Dictionary<decimal, IDeliveryCondition> DeliveryConditions =
        new Dictionary<decimal, IDeliveryCondition>();

    public Dictionary<decimal, IKontragent> Kontragents = new Dictionary<decimal, IKontragent>();
    public Dictionary<decimal, INomenkl> Nomenkls = new Dictionary<decimal, INomenkl>();
    public readonly Dictionary<Guid, INomenklMain> NomenklMains = new Dictionary<Guid, INomenklMain>();

    private readonly Dictionary<decimal, INomenklGroup> NomenklGroups =
        new Dictionary<decimal, INomenklGroup>();

    private readonly Dictionary<decimal, IWarehouse> Warehouses = new Dictionary<decimal, IWarehouse>();
    private readonly Dictionary<decimal, IEmployee> Employees = new Dictionary<decimal, IEmployee>();
    private readonly Dictionary<decimal, IBank> Banks = new Dictionary<decimal, IBank>();
    private Dictionary<decimal, IBankAccount> BankAccounts = new Dictionary<decimal, IBankAccount>();

    private readonly Dictionary<decimal, ICentrResponsibility> CentrResponsibilities =
        new Dictionary<decimal, ICentrResponsibility>();

    private readonly Dictionary<decimal, ISDRSchet> SDRSchets = new Dictionary<decimal, ISDRSchet>();
    private readonly Dictionary<decimal, ISDRState> SDRStates = new Dictionary<decimal, ISDRState>();
    private readonly Dictionary<decimal, IClientCategory> ClientCategories = new Dictionary<decimal, IClientCategory>();
    private readonly Dictionary<decimal, ICurrency> Currencies = new Dictionary<decimal, ICurrency>();
    private readonly Dictionary<decimal, IRegion> Regions = new Dictionary<decimal, IRegion>();
    private readonly Dictionary<decimal, IUnit> Units = new Dictionary<decimal, IUnit>();
    private Dictionary<decimal, ICashBox> CashBoxes = new Dictionary<decimal, ICashBox>();

    private readonly Dictionary<decimal, IMutualSettlementType> MutualSettlementTypes =
        new Dictionary<decimal, IMutualSettlementType>();

    private readonly Dictionary<Guid, ICountry> Countries = new Dictionary<Guid, ICountry>();
    private readonly Dictionary<Guid, IProject> Projects = new Dictionary<Guid, IProject>();
    private readonly Dictionary<decimal, IContractType> ContractTypes = new Dictionary<decimal, IContractType>();
    private readonly Dictionary<decimal, INomenklType> NomenklTypes = new Dictionary<decimal, INomenklType>();
    private readonly Dictionary<decimal, IProductType> ProductTypes = new Dictionary<decimal, IProductType>();
    private readonly Dictionary<decimal, IPayForm> PayForms = new Dictionary<decimal, IPayForm>();
    private readonly Dictionary<decimal, IPayCondition> PayConditions = new Dictionary<decimal, IPayCondition>();

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
            var name = $"{ typeof(T).Name}";
            string key = name switch
            {
                "Kontragent" or "Nomenkl" => $"Cache:{typeof(T).Name}:{((IDocCode)item).DocCode}:{item.Id}@{now}",
                _ => $"Cache:{typeof(T).Name}:{item.Id}@{now}"
            };
            var olds = redisClient.GetKeysByPattern($"{key.Split('@')[0]}*");
            foreach (var oldKey in olds)
            {
                redisClient.Remove(oldKey);
            }
            redis.SetValue(key, item);
            var message = new RedisMessage
            {
                DocumentType = DocumentType.None,
                Id = item.Id,
                DocDate = DateTime.Now,
                IsDocument = false,
                OperationType = RedisMessageDocumentOperationTypeEnum.Execute,
                Message =
                    $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник {typeof(T).Name} '{item.Id}'"
            };
            message.ExternalValues.Add("RedisKey",key);
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            redisClient.PublishMessage(getChannelName(typeof(T).Name),
                JsonConvert.SerializeObject(message, jsonSerializerSettings));
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
            var oldKey = cacheKeysDict[$"{typeof(T).Name}"].CachKeys.FirstOrDefault(_ => _.Id == id);
            if (oldKey is not null)
            {
                oldKey.Key = enumerable.First();
            }
            else
            {
                var key = enumerable.First();
                var name = $"{typeof(T).Name}";
                switch (name)
                {
                    case "Kontragent":
                    case "Nomenkl":
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                            Id = Guid.Parse(key.Split('@')[0].Split(':')[3]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                    case "Employee":
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                            TabelNumber = Convert.ToInt32(key.Split('@')[0].Split(':')[3]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                    case "NomenklMain":
                    case "Country":
                    case "Project":
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            Id = Guid.Parse(key.Split('@')[0].Split(':')[2]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                    default:
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                }
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
            var name = $"{typeof(T).Name}";

            string key = name switch
            {
                "Kontragent" or "Nomenkl" => $"Cache:{typeof(T).Name}:{item.DocCode}:{((IDocGuid)item).Id}@{now}",
                "Employee" => $"Cache:{typeof(T).Name}:{item.DocCode}:{((IEmployee)item).TabelNumber}@{now}",
                _ => $"Cache:{typeof(T).Name}:{item.DocCode}@{now}"
            };
            var olds = redisClient.GetKeysByPattern($"{key.Split('@')[0]}*");
            foreach (var oldKey in olds)
            {
                redisClient.Remove(oldKey);
            }
            redis.SetValue(key, item);
            var message = new RedisMessage
            {
                DocumentType = DocumentType.None,
                DocCode = item.DocCode,
                DocDate = DateTime.Now,
                IsDocument = false,
                OperationType = RedisMessageDocumentOperationTypeEnum.Execute,
                Message =
                    $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник {typeof(T).Name} '{item.DocCode}'"
            };
            message.ExternalValues.Add("RedisKey",key);
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            redisClient.PublishMessage(getChannelName(typeof(T).Name),
                JsonConvert.SerializeObject(message, jsonSerializerSettings));
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
            var oldKey = cacheKeysDict[$"{typeof(T).Name}"].CachKeys.FirstOrDefault(_ => _.DocCode == dc);
            if (oldKey is not null)
            {
                oldKey.Key = enumerable.First();
            }
            else
            {
                var key = enumerable.First();
                var name = $"Cache:{typeof(T).Name}";
                switch (name)
                {
                    case "Kontragent":
                    case "Nomenkl":
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                            Id = Guid.Parse(key.Split('@')[0].Split(':')[3]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                    case "Employee":
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                            TabelNumber = Convert.ToInt32(key.Split('@')[0].Split(':')[3]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                    case "NomenklMain":
                    case "Country":
                    case "Project":
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            Id = Guid.Parse(key.Split('@')[0].Split(':')[2]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                    default:
                        cacheKeysDict[name].CachKeys.Add(new CachKey
                        {
                            DocCode = Convert.ToDecimal(key.Split('@')[0].Split(':')[2]),
                            LastUpdate = Convert.ToDateTime(key.Split('@')[1]),
                            Key = key
                        });
                        break;
                }
            }

            var c = enumerable.First().Split(':');
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
