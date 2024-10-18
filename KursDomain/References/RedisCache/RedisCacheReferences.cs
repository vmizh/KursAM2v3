using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using KursAM2.Repositories.RedisRepository;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.IReferences.Nomenkl;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace KursDomain.References.RedisCache;

public interface ICache
{
    DateTime LastUpdateServe { set; get; }
    void LoadFromCache();
}

public class ItemReferenceUpdate
{
    public string Id { get; set; }
    public DateTime UpdateTime { get; set; }
}

public class RedisCacheReferences : IReferencesCache
{
    private readonly RedisManagerPool redisManager =
        new RedisManagerPool(ConfigurationManager.AppSettings["redis.connection"]);


    private bool isStartLoad = true;

    public RedisCacheReferences()
    {
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

    public IEnumerable<T> GetList<T>(IEnumerable<decimal> dcs) where T : IDocCode
    {
        if (dcs is null) return Enumerable.Empty<T>();
        var idList = dcs.ToList();
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var list = redis.Lists[$"Cache:{typeof(T).Name}"].GetAll().Where(_ => idList.Contains(_.DocCode));
            return list;
        }
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
        if (CashBoxes.ContainsKey(dc))
        {
            var itemCache = CashBoxes[dc] as CashBox;
            if (!IsTimerOut(itemCache)) return CashBoxes[dc];
            var item = GetItem<CashBox>(dc);
            if (item is null) return null;
            CashBoxes[dc] = item;
            return CashBoxes[dc];
        }

        var itemNew = GetItem<CashBox>(dc);
        if (itemNew is null) return null;
        CashBoxes.Add(dc, itemNew);
        return CashBoxes[dc];
    }

    public IEnumerable<ICashBox> GetCashBoxAll()
    {
        var checkData = GetIsTimerOut(CashBoxes.Values.Cast<CashBox>());
        if (!checkData.Item1) return CashBoxes.Values.ToList();
        CashBoxes.Clear();
        foreach (var item in checkData.Item2) CashBoxes.Add(item.DocCode, item);
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
        if (NomenklTypes.ContainsKey(dc.Value))
        {
            var itemCache = NomenklTypes[dc.Value] as NomenklType;
            if (!IsTimerOut(itemCache)) return NomenklTypes[dc.Value];
            var item = GetItem<NomenklType>(dc.Value);
            if (item is null) return null;
            NomenklTypes[dc.Value] = item;
            return NomenklTypes[dc.Value];
        }

        var itemNew = GetItem<NomenklType>(dc.Value);
        if (itemNew is null) return null;
        NomenklTypes.Add(dc.Value, itemNew);
        return NomenklTypes[dc.Value];
    }

    public IEnumerable<INomenklType> GetNomenklTypeAll()
    {
        var checkData = GetIsTimerOut(NomenklTypes.Values.Cast<NomenklType>());
        if (!checkData.Item1) return NomenklTypes.Values.ToList();
        NomenklTypes.Clear();
        foreach (var item in checkData.Item2) NomenklTypes.Add(item.DocCode, item);
        return NomenklTypes.Values.ToList();
    }

    public INomenklProductType GetNomenklProductType(decimal? dc)
    {
        if (dc is null) return null;
        return GetNomenklProductType(dc.Value);
    }

    public INomenklProductType GetNomenklProductType(decimal dc)
    {
        if (NomenklProductTypes.ContainsKey(dc))
        {
            var itemCache = NomenklProductTypes[dc] as NomenklProductType;
            if (!IsTimerOut(itemCache)) return NomenklProductTypes[dc];
            var item = GetItem<NomenklProductType>(dc);
            if (item is null) return null;
            NomenklProductTypes[dc] = item;
            return NomenklProductTypes[dc];
        }

        var itemNew = GetItem<NomenklProductType>(dc);
        if (itemNew is null) return null;
        NomenklProductTypes.Add(dc, itemNew);
        return NomenklProductTypes[dc];
    }

    public IEnumerable<INomenklProductType> GetNomenklProductTypesAll()
    {
        var checkData = GetIsTimerOut(NomenklProductTypes.Values.Cast<NomenklProductType>());
        if (!checkData.Item1) return NomenklProductTypes.Values.ToList();
        NomenklProductTypes.Clear();
        foreach (var item in checkData.Item2) NomenklProductTypes.Add(item.DocCode, item);
        return NomenklProductTypes.Values.ToList();
    }

    public IProductType GetProductType(decimal? dc)
    {
        return dc is null ? null : GetProductType(dc.Value);
    }

    public IProductType GetProductType(decimal dc)
    {
        if (ProductTypes.ContainsKey(dc))
        {
            var itemCache = ProductTypes[dc] as ProductType;
            if (!IsTimerOut(itemCache)) return ProductTypes[dc];
            var item = GetItem<ProductType>(dc);
            if (item is null) return null;
            ProductTypes[dc] = item;
            return ProductTypes[dc];
        }

        var itemNew = GetItem<ProductType>(dc);
        if (itemNew is null) return null;
        ProductTypes.Add(dc, itemNew);
        return ProductTypes[dc];
    }

    public IEnumerable<IProductType> GetProductTypeAll()
    {
        var checkData = GetIsTimerOut(ProductTypes.Values.Cast<ProductType>());
        if (!checkData.Item1) return ProductTypes.Values.ToList();
        ProductTypes.Clear();
        foreach (var item in checkData.Item2) ProductTypes.Add(item.DocCode, item);
        return ProductTypes.Values.ToList();
    }

    public ICentrResponsibility GetCentrResponsibility(decimal? dc)
    {
        if (dc is null) return null;
        if (CentrResponsibilities.ContainsKey(dc.Value))
        {
            var itemCache = CentrResponsibilities[dc.Value] as CentrResponsibility;
            if (!IsTimerOut(itemCache)) return CentrResponsibilities[dc.Value];
            var item = GetItem<CentrResponsibility>(dc.Value);
            if (item is null) return null;
            CentrResponsibilities[dc.Value] = item;
            return CentrResponsibilities[dc.Value];
        }

        var itemNew = GetItem<CentrResponsibility>(dc.Value);
        if (itemNew is null) return null;
        CentrResponsibilities.Add(dc.Value, itemNew);
        return CentrResponsibilities[dc.Value];
    }

    public IEnumerable<ICentrResponsibility> GetCentrResponsibilitiesAll()
    {
        var checkData = GetIsTimerOut(CentrResponsibilities.Values.Cast<CentrResponsibility>());
        if (!checkData.Item1) return CentrResponsibilities.Values.ToList();
        CentrResponsibilities.Clear();
        foreach (var item in checkData.Item2) CentrResponsibilities.Add(item.DocCode, item);
        return CentrResponsibilities.Values.ToList();
    }

    public IBank GetBank(decimal? dc)
    {
        if (dc is null) return null;
        if (Banks.ContainsKey(dc.Value))
        {
            var itemCache = Banks[dc.Value] as Bank;
            if (!IsTimerOut(itemCache)) return Banks[dc.Value];
            var item = GetItem<Bank>(dc.Value);
            if (item is null) return null;
            Banks[dc.Value] = item;
            return Banks[dc.Value];
        }

        var itemNew = GetItem<Bank>(dc.Value);
        if (itemNew is null) return null;
        Banks.Add(dc.Value, itemNew);
        return Banks[dc.Value];
    }

    public IEnumerable<IBank> GetBanksAll()
    {
        var checkData = GetIsTimerOut(Banks.Values.Cast<Bank>());
        if (!checkData.Item1) return Banks.Values.ToList();
        Banks.Clear();
        foreach (var item in checkData.Item2) Banks.Add(item.DocCode, item);
        return Banks.Values.ToList();
    }

    public IBankAccount GetBankAccount(decimal? dc)
    {
        if (dc is null) return null;
        if (BankAccounts.ContainsKey(dc.Value))
        {
            var itemCache = BankAccounts[dc.Value] as BankAccount;
            if (!IsTimerOut(itemCache)) return BankAccounts[dc.Value];
            var item = GetItem<BankAccount>(dc.Value);
            if (item is null) return null;
            BankAccounts[dc.Value] = item;
            return BankAccounts[dc.Value];
        }

        var itemNew = GetItem<BankAccount>(dc.Value);
        if (itemNew is null) return null;
        BankAccounts.Add(dc.Value, itemNew);
        return BankAccounts[dc.Value];
    }

    public IEnumerable<IBankAccount> GetBankAccountAll()
    {
        var checkData = GetIsTimerOut(BankAccounts.Values.Cast<BankAccount>());
        if (!checkData.Item1) return BankAccounts.Values.ToList();
        BankAccounts.Clear();
        foreach (var item in checkData.Item2) BankAccounts.Add(item.DocCode, item);
        return BankAccounts.Values.ToList();
    }

    public IKontragentGroup GetKontragentGroup(int? id)
    {
        if (id is null) return null;
        var dc = id.Value;
        if (KontragentGroups.ContainsKey(dc))
        {
            var itemCache = KontragentGroups[dc] as KontragentGroup;
            if (!IsTimerOut(itemCache)) return KontragentGroups[dc];
            var item = GetItem<KontragentGroup>(dc);
            if (item is null) return null;
            KontragentGroups[dc] = item;
            return KontragentGroups[dc];
        }

        var itemNew = GetItem<KontragentGroup>(dc);
        if (itemNew is null) return null;
        KontragentGroups.Add(dc, itemNew);
        return KontragentGroups[dc];
    }

    public IEnumerable<IKontragentGroup> GetKontragentCategoriesAll()
    {
        var checkData = GetIsTimerOut(KontragentGroups.Values.Cast<KontragentGroup>());
        if (!checkData.Item1) return KontragentGroups.Values.ToList();
        KontragentGroups.Clear();
        foreach (var item in checkData.Item2) KontragentGroups.Add((int)item.DocCode, item);
        return KontragentGroups.Values.ToList();
    }

    public IKontragent GetKontragent(decimal? dc)
    {
        if (dc is null) return null;
        return GetKontragent(dc.Value);
    }

    public IKontragent GetKontragent(decimal dc)
    {
        if (Kontragents.ContainsKey(dc))
        {
            var itemCache = Kontragents[dc] as Kontragent;
            if (!IsTimerOut(itemCache)) return Kontragents[dc];
            var item = GetItem<Kontragent>(dc);
            if (item is null) return null;
            Kontragents[dc] = item;
            return Kontragents[dc];
        }

        var itemNew = GetItem<Kontragent>(dc);
        if (itemNew is null) return null;
        Kontragents.Add(dc, itemNew);
        return Kontragents[dc];
    }

    public IKontragent GetKontragent(Guid? id)
    {
        throw new NotImplementedException();
    }

    // TODO Переписать с учетом, обновлять не все а только измененные и не попавшие в кэш, с удалением лишних
    public IEnumerable<IKontragent> GetKontragentsAll()
    {
        var checkData = GetIsTimerOut(Kontragents.Values.Cast<Kontragent>());
        if (!checkData.Item1) return Kontragents.Values.ToList();
        Kontragents.Clear();
        foreach (var item in checkData.Item2) Kontragents.Add(item.DocCode, item);
        return Kontragents.Values.ToList();
    }

    public INomenkl GetNomenkl(Guid? id)
    {
        return id is null ? null : GetNomenkl(id.Value);
    }

    public INomenkl GetNomenkl(Guid id)
    {
        var nom = Nomenkls.Values.Cast<Nomenkl>().FirstOrDefault(_ => _.Id == id);
        if (nom is not null) return GetNomenkl(nom.DocCode);
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<Nomenkl>();
            var list = redis.Lists["Cache:Nomenkl"];
            var item = list.FirstOrDefault(_ => _.Id == id);
            if (item is null) return null;
            ((ICache)item)?.LoadFromCache();
            Nomenkls[item.DocCode] = item;
            return item;
        }
    }

    public INomenkl GetNomenkl(decimal? dc)
    {
        if (dc is null) return null;
        return GetNomenkl(dc.Value);
    }

    public INomenkl GetNomenkl(decimal dc)
    {
        if (Nomenkls.ContainsKey(dc))
        {
            var itemCache = Nomenkls[dc] as Nomenkl;
            if (!IsTimerOut(itemCache)) return Nomenkls[dc];
            var item = GetItem<Nomenkl>(dc);
            if (item is null) return null;
            Nomenkls[dc] = item;
            return Nomenkls[dc];
        }

        var itemNew = GetItem<Nomenkl>(dc);
        if (itemNew is null) return null;
        Nomenkls.Add(dc, itemNew);
        return Nomenkls[dc];
    }

    // TODO Переписать с учетом, обновлять не все а только измененные и не попавшие в кэш, с удалением лишних
    public IEnumerable<INomenkl> GetNomenklsAll()
    {
        var checkData = GetIsTimerOut(Nomenkls.Values.Cast<Nomenkl>());
        if (!checkData.Item1) return Nomenkls.Values.ToList();
        Nomenkls.Clear();
        foreach (var item in checkData.Item2) Nomenkls.Add(item.DocCode, item);
        return Nomenkls.Values.ToList();
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public IEnumerable<INomenkl> GetNomenkls(IEnumerable<decimal> dcList)
    {
        if (dcList == null || !dcList.Any()) return new List<INomenkl>();
        var list = dcList.ToList();

        var timers = GetChangeTracker<Nomenkl>();
        var nomenkls = GetRawAll<Nomenkl>().Where(_ => list.Contains(_.DocCode)).ToList();
        var newItems = list.Where(dc => !Nomenkls.ContainsKey(dc)).ToList();
        foreach (var dc in newItems)
        {
            var d = nomenkls.FirstOrDefault(_ => _.DocCode == dc);
            if (d is null) continue;
            ((ICache)d).LoadFromCache();
            Nomenkls.Add(dc, d);
        }

        var updItems = (from dc in list.Except(newItems)
            let track = timers.FirstOrDefault(_ => _.Id == dc.ToString())
            where track != null && ((ICache)Nomenkls[dc]).LastUpdateServe < track.UpdateTime
            select dc).ToList();
        foreach (var dc in updItems)
        {
            var d = nomenkls.FirstOrDefault(_ => _.DocCode == dc);
            if (d is null) continue;
            ((ICache)d).LoadFromCache();
            Nomenkls[dc] = d;
        }

        return list.Select(dc => Nomenkls[dc] as Nomenkl).ToList();
    }

    public INomenklMain GetNomenklMain(Guid? id)
    {
        if (id is null) return null;
        return GetNomenklMain(id.Value);
    }

    public INomenklMain GetNomenklMain(Guid id)
    {
        if (NomenklMains.ContainsKey(id))
        {
            var itemCache = NomenklMains[id] as NomenklMain;
            if (!IsTimerOutGuid(itemCache)) return NomenklMains[id];
            var item = GetItemGuid<NomenklMain>(id);
            if (item is null) return null;
            NomenklMains[id] = item;
            return NomenklMains[id];
        }

        var itemNew = GetItemGuid<NomenklMain>(id);
        if (itemNew is null) return null;
        NomenklMains.Add(id, itemNew);
        return NomenklMains[id];
    }

    public IEnumerable<INomenklMain> GetNomenklMainAll()
    {
        var checkData = GetIsTimerOutGuid(NomenklMains.Values.Cast<NomenklMain>());
        if (!checkData.Item1) return NomenklMains.Values.ToList();
        NomenklMains.Clear();
        foreach (var item in checkData.Item2) NomenklMains.Add(item.Id, item);
        return NomenklMains.Values.ToList();
    }

    public INomenklGroup GetNomenklGroup(decimal? dc)
    {
        if (dc is null) return null;
        if (NomenklGroups.ContainsKey(dc.Value))
        {
            var itemCache = NomenklGroups[dc.Value] as NomenklGroup;
            if (!IsTimerOut(itemCache)) return NomenklGroups[dc.Value];
            var item = GetItem<NomenklGroup>(dc.Value);
            if (item is null) return null;
            NomenklGroups[dc.Value] = item;
            return NomenklGroups[dc.Value];
        }

        var itemNew = GetItem<NomenklGroup>(dc.Value);
        if (itemNew is null) return null;
        NomenklGroups.Add(dc.Value, itemNew);
        return NomenklGroups[dc.Value];
    }

    public IEnumerable<INomenklGroup> GetNomenklGroupAll()
    {
        var checkData = GetIsTimerOut(NomenklGroups.Values.Cast<NomenklGroup>());
        if (!checkData.Item1) return NomenklGroups.Values.ToList();
        NomenklGroups.Clear();
        foreach (var item in checkData.Item2) NomenklGroups.Add(item.DocCode, item);
        return NomenklGroups.Values.ToList();
    }

    public IWarehouse GetWarehouse(decimal? dc)
    {
        return dc is null ? null : GetWarehouse(dc.Value);
    }

    public IWarehouse GetWarehouse(decimal dc)
    {
        if (Warehouses.ContainsKey(dc))
        {
            var itemCache = Warehouses[dc] as Warehouse;
            if (!IsTimerOut(itemCache)) return Warehouses[dc];
            var item = GetItem<Warehouse>(dc);
            if (item is null) return null;
            Warehouses[dc] = item;
            return Warehouses[dc];
        }

        var itemNew = GetItem<Warehouse>(dc);
        if (itemNew is null) return null;
        Warehouses.Add(dc, itemNew);
        return Warehouses[dc];
    }

    public IWarehouse GetWarehouse(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IWarehouse> GetWarehousesAll()
    {
        var checkData = GetIsTimerOut(Warehouses.Values.Cast<Warehouse>());
        if (!checkData.Item1) return Warehouses.Values.ToList();
        Warehouses.Clear();
        foreach (var item in checkData.Item2) Warehouses.Add(item.DocCode, item);
        return Warehouses.Values.ToList();
    }


    public IEmployee GetEmployee(int? tabelNumber)
    {
        if (tabelNumber is null) return null;
        var emp = Employees.Values.FirstOrDefault(_ => _.TabelNumber == tabelNumber.Value) as Employee;
        if (emp is not null) return GetEmployee(emp.DocCode);
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<Employee>();
            var list = redis.Lists["Cache:Employee"];
            var item = list.FirstOrDefault(_ => _.TabelNumber == tabelNumber);
            if (item is null) return null;
            ((ICache)item)?.LoadFromCache();
            Employees[item.DocCode] = item;
            return item;
        }
    }

    public IEmployee GetEmployee(decimal? dc)
    {
        if (dc is null) return null;
        if (Employees.ContainsKey(dc.Value))
        {
            var itemCache = Employees[dc.Value] as Employee;
            if (!IsTimerOut(itemCache)) return Employees[dc.Value];
            var item = GetItem<Employee>(dc.Value);
            if (item is null) return null;
            Employees[dc.Value] = item;
            return Employees[dc.Value];
        }

        var itemNew = GetItem<Employee>(dc.Value);
        if (itemNew is null) return null;
        Employees.Add(dc.Value, itemNew);
        return Employees[dc.Value];
    }

    public IEmployee GetEmployee(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IEmployee> GetEmployees()
    {
        var checkData = GetIsTimerOut(Employees.Values.Cast<Employee>());
        if (!checkData.Item1) return Employees.Values.ToList();
        Employees.Clear();
        foreach (var item in checkData.Item2) Employees.Add(item.DocCode, item);
        return Employees.Values.ToList();
    }

    public ISDRSchet GetSDRSchet(decimal? dc)
    {
        if (dc is null) return null;
        if (SDRSchets.ContainsKey(dc.Value))
        {
            var itemCache = SDRSchets[dc.Value] as SDRSchet;
            if (!IsTimerOut(itemCache)) return SDRSchets[dc.Value];
            var item = GetItem<SDRSchet>(dc.Value);
            if (item is null) return null;
            SDRSchets[dc.Value] = item;
            return SDRSchets[dc.Value];
        }

        var itemNew = GetItem<SDRSchet>(dc.Value);
        if (itemNew is null) return null;
        SDRSchets.Add(dc.Value, itemNew);
        return SDRSchets[dc.Value];
    }

    public ISDRSchet GetSDRSchet(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISDRSchet> GetSDRSchetAll()
    {
        var checkData = GetIsTimerOut(SDRSchets.Values.Cast<SDRSchet>());
        if (!checkData.Item1) return SDRSchets.Values.ToList();
        SDRSchets.Clear();
        foreach (var item in checkData.Item2) SDRSchets.Add(item.DocCode, item);
        return SDRSchets.Values.ToList();
    }

    public ISDRState GetSDRState(decimal? dc)
    {
        if (dc is null) return null;
        if (SDRStates.ContainsKey(dc.Value))
        {
            var itemCache = SDRStates[dc.Value] as SDRState;
            if (!IsTimerOut(itemCache)) return SDRStates[dc.Value];
            var item = GetItem<SDRState>(dc.Value);
            if (item is null) return null;
            SDRStates[dc.Value] = item;
            return SDRStates[dc.Value];
        }

        var itemNew = GetItem<SDRState>(dc.Value);
        if (itemNew is null) return null;
        SDRStates.Add(dc.Value, itemNew);
        return SDRStates[dc.Value];
    }

    public ISDRState GetSDRState(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISDRState> GetSDRStateAll()
    {
        var checkData = GetIsTimerOut(SDRStates.Values.Cast<SDRState>());
        if (!checkData.Item1) return SDRStates.Values.ToList();
        SDRStates.Clear();
        foreach (var item in checkData.Item2) SDRStates.Add(item.DocCode, item);
        return SDRStates.Values.ToList();
    }

    public IClientCategory GetClientCategory(decimal? dc)
    {
        if (dc is null) return null;
        if (ClientCategories.ContainsKey(dc.Value))
        {
            var itemCache = ClientCategories[dc.Value] as ClientCategory;
            if (!IsTimerOut(itemCache)) return ClientCategories[dc.Value];
            var item = GetItem<ClientCategory>(dc.Value);
            if (item is null) return null;
            ClientCategories[dc.Value] = item;
            return ClientCategories[dc.Value];
        }

        var itemNew = GetItem<ClientCategory>(dc.Value);
        if (itemNew is null) return null;
        ClientCategories.Add(dc.Value, itemNew);
        return ClientCategories[dc.Value];
    }

    public IClientCategory GetClientCategory(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IClientCategory> GetClientCategoriesAll()
    {
        var checkData = GetIsTimerOut(ClientCategories.Values.Cast<ClientCategory>());
        if (!checkData.Item1) return ClientCategories.Values.ToList();
        ClientCategories.Clear();
        foreach (var item in checkData.Item2) ClientCategories.Add(item.DocCode, item);
        return ClientCategories.Values.ToList();
    }

    public ICurrency GetCurrency(decimal? dc)
    {
        return dc is not null ? GetCurrency(dc.Value) : null;
    }

    public ICurrency GetCurrency(decimal dc)
    {
        if (Currencies.ContainsKey(dc))
        {
            var itemCache = Currencies[dc] as Currency;
            if (!IsTimerOut(itemCache)) return Currencies[dc];
            var item = GetItem<Currency>(dc);
            if (item is null) return null;
            Currencies[dc] = item;
            return Currencies[dc];
        }

        var itemNew = GetItem<Currency>(dc);
        if (itemNew is null) return null;
        Currencies.Add(dc, itemNew);
        return Currencies[dc];
    }

    public IEnumerable<ICurrency> GetCurrenciesAll()
    {
        var checkData = GetIsTimerOut(Currencies.Values.Cast<Currency>());
        if (!checkData.Item1) return Currencies.Values.ToList();
        Currencies.Clear();
        foreach (var item in checkData.Item2) Currencies.Add(item.DocCode, item);
        return Currencies.Values.ToList();
    }

    public ICountry GetCountry(Guid? id)
    {
        return id is not null ? GetItemGuid<Country>(id.Value) : null;
    }

    public IEnumerable<ICountry> GetCountriesAll()
    {
        return GetAllGuid<Country>();
    }

    public IRegion GetRegion(decimal? dc)
    {
        if (dc is null) return null;
        if (Regions.ContainsKey(dc.Value))
        {
            var itemCache = Regions[dc.Value] as Region;
            if (!IsTimerOut(itemCache)) return Regions[dc.Value];
            var item = GetItem<Region>(dc.Value);
            if (item is null) return null;
            Regions[dc.Value] = item;
            return Regions[dc.Value];
        }

        var itemNew = GetItem<Region>(dc.Value);
        if (itemNew is null) return null;
        Regions.Add(dc.Value, itemNew);
        return Regions[dc.Value];
    }

    public IRegion GetRegion(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IRegion> GetRegionsAll()
    {
        var checkData = GetIsTimerOut(Regions.Values.Cast<Region>());
        if (!checkData.Item1) return Regions.Values.ToList();
        Regions.Clear();
        foreach (var item in checkData.Item2) Regions.Add(item.DocCode, item);

        return Regions.Values.ToList();
    }

    public IUnit GetUnit(decimal? dc)
    {
        if (dc is null) return null;
        if (Units.ContainsKey(dc.Value))
        {
            var itemCache = Units[dc.Value] as Unit;
            if (!IsTimerOut(itemCache)) return Units[dc.Value];
            var item = GetItem<Unit>(dc.Value);
            if (item is null) return null;
            Units[dc.Value] = item;
            return Units[dc.Value];
        }

        var itemNew = GetItem<Unit>(dc.Value);
        if (itemNew is null) return null;
        Units.Add(dc.Value, itemNew);
        return Units[dc.Value];
    }

    public IUnit GetUnit(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IUnit> GetUnitsAll()
    {
        var checkData = GetIsTimerOut(Units.Values.Cast<Unit>());
        if (!checkData.Item1) return Units.Values.ToList();
        Units.Clear();
        foreach (var item in checkData.Item2) Units.Add(item.DocCode, item);
        return Units.Values.ToList();
    }

    public IMutualSettlementType GetMutualSettlementType(decimal? dc)
    {
        if (dc is null) return null;
        if (MutualSettlementTypes.ContainsKey(dc.Value))
        {
            var itemCache = MutualSettlementTypes[dc.Value] as MutualSettlementType;
            if (!IsTimerOut(itemCache)) return MutualSettlementTypes[dc.Value];
            var item = GetItem<MutualSettlementType>(dc.Value);
            MutualSettlementTypes[dc.Value] = item;
            return MutualSettlementTypes[dc.Value];
        }

        var itemNew = GetItem<MutualSettlementType>(dc.Value);
        MutualSettlementTypes.Add(dc.Value, itemNew);
        return MutualSettlementTypes[dc.Value];
    }

    public IEnumerable<IMutualSettlementType> GetMutualSettlementTypeAll()
    {
        var checkData = GetIsTimerOut(MutualSettlementTypes.Values.Cast<MutualSettlementType>());
        if (!checkData.Item1) return MutualSettlementTypes.Values.ToList();
        MutualSettlementTypes.Clear();
        foreach (var item in checkData.Item2) MutualSettlementTypes.Add(item.DocCode, item);
        return MutualSettlementTypes.Values.ToList();
    }

    public IProject GetProject(Guid? id)
    {
        if (id is null) return null;
        if (Projects.ContainsKey(id.Value))
        {
            var itemCache = Projects[id.Value] as Project;
            if (!IsTimerOutGuid(itemCache)) return Projects[id.Value];
            var item = GetItemGuid<Project>(id.Value);
            if (item is null) return null;
            Projects[id.Value] = item;
            return Projects[id.Value];
        }

        var itemNew = GetItemGuid<Project>(id.Value);
        if (itemNew is null) return null;
        Projects.Add(id.Value, itemNew);
        return Projects[id.Value];
    }

    public IEnumerable<IProject> GetProjectsAll()
    {
        var checkData = GetIsTimerOutGuid(Projects.Values.Cast<Project>());
        if (!checkData.Item1) return Projects.Values.ToList();
        Projects.Clear();
        foreach (var item in checkData.Item2) Projects.Add(item.Id, item);
        return Projects.Values.ToList();
    }

    public IContractType GetContractType(decimal? dc)
    {
        if (dc is null) return null;
        if (ContractTypes.ContainsKey(dc.Value))
        {
            var itemCache = ContractTypes[dc.Value] as ContractType;
            if (!IsTimerOut(itemCache)) return ContractTypes[dc.Value];
            var item = GetItem<ContractType>(dc.Value);
            if (item is null) return null;
            ContractTypes[dc.Value] = item;
            return ContractTypes[dc.Value];
        }

        var itemNew = GetItem<ContractType>(dc.Value);
        if (itemNew is null) return null;
        ContractTypes.Add(dc.Value, itemNew);
        return ContractTypes[dc.Value];
    }

    public IEnumerable<IContractType> GetContractsTypeAll()
    {
        var checkData = GetIsTimerOut(ContractTypes.Values.Cast<ContractType>());
        if (!checkData.Item1) return ContractTypes.Values.ToList();
        ContractTypes.Clear();
        foreach (var item in checkData.Item2) ContractTypes.Add(item.DocCode, item);
        return ContractTypes.Values.ToList();
    }

    public IPayForm GetPayForm(decimal? dc)
    {
        if (dc is null) return null;
        if (PayForms.ContainsKey(dc.Value))
        {
            var itemCache = PayForms[dc.Value] as PayForm;
            if (!IsTimerOut(itemCache)) return PayForms[dc.Value];
            var item = GetItem<PayForm>(dc.Value);
            if (item is null) return null;
            PayForms[dc.Value] = item;
            return PayForms[dc.Value];
        }

        var itemNew = GetItem<PayForm>(dc.Value);
        if (itemNew is null) return null;
        PayForms.Add(dc.Value, itemNew);
        return PayForms[dc.Value];
    }

    public IEnumerable<IPayForm> GetPayFormAll()
    {
        var checkData = GetIsTimerOut(PayForms.Values.Cast<PayForm>());
        if (!checkData.Item1) return PayForms.Values.ToList();
        PayForms.Clear();
        foreach (var item in checkData.Item2) PayForms.Add(item.DocCode, item);
        return PayForms.Values.ToList();
    }

    public IPayCondition GetPayCondition(decimal? dc)
    {
        if (dc is null) return null;
        if (PayConditions.ContainsKey(dc.Value))
        {
            var itemCache = PayConditions[dc.Value] as PayCondition;
            if (!IsTimerOut(itemCache)) return PayConditions[dc.Value];
            var item = GetItem<PayCondition>(dc.Value);
            if (item is null) return null;
            PayConditions[dc.Value] = item;
            return PayConditions[dc.Value];
        }

        var itemNew = GetItem<PayCondition>(dc.Value);
        if (itemNew is null) return null;
        PayConditions.Add(dc.Value, itemNew);
        return PayConditions[dc.Value];
    }

    public IEnumerable<IPayCondition> GetPayConditionAll()
    {
        var checkData = GetIsTimerOut(PayConditions.Values.Cast<PayCondition>());
        if (!checkData.Item1) return PayConditions.Values.ToList();
        PayConditions.Clear();
        foreach (var item in checkData.Item2) PayConditions.Add(item.DocCode, item);
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

                foreach (var item in Context.Countries.AsNoTracking().ToList())
                {
                    var newItem = new Country();
                    newItem.LoadFromEntity(item);
                    if (!Countries.ContainsKey(newItem.Id))
                        Countries.Add(newItem.Id, newItem);
                }

                DropAllGuid<Country>();
                UpdateListGuid(Countries.Values.Cast<Country>(), now);

                foreach (var item in Context.SD_23.AsNoTracking().ToList())
                {
                    var newItem = new Region();
                    newItem.LoadFromEntity(item);
                    if (!Regions.ContainsKey(newItem.DocCode))
                        Regions.Add(newItem.DocCode, newItem);
                }

                DropAll<Region>();
                UpdateList(Regions.Values.Cast<Region>(), now);

                foreach (var item in Context.SD_179.AsNoTracking().ToList())
                {
                    var newItem = new PayCondition();
                    newItem.LoadFromEntity(item);
                    if (!PayConditions.ContainsKey(newItem.DocCode))
                        PayConditions.Add(newItem.DocCode, newItem);
                }

                DropAll<PayCondition>();
                UpdateList(PayConditions.Values.Cast<PayCondition>(), now);

                foreach (var item in Context.SD_189.AsNoTracking().ToList())
                {
                    var newItem = new PayForm();
                    newItem.LoadFromEntity(item);
                    if (!PayForms.ContainsKey(newItem.DocCode))
                        PayForms.Add(newItem.DocCode, newItem);
                }

                DropAll<PayForm>();
                UpdateList(PayForms.Values.Cast<PayForm>(), now);

                foreach (var item in Context.SD_175.AsNoTracking().ToList())
                {
                    var newItem = new Unit();
                    newItem.LoadFromEntity(item);
                    if (!Units.ContainsKey(newItem.DocCode))
                        Units.Add(newItem.DocCode, newItem);
                }

                DropAll<Unit>();
                UpdateList(Units.Values.Cast<Unit>(), now);

                foreach (var item in Context.SD_40.AsNoTracking().ToList())
                {
                    var newItem = new CentrResponsibility();
                    newItem.LoadFromEntity(item);
                    if (!CentrResponsibilities.ContainsKey(newItem.DocCode))
                        CentrResponsibilities.Add(newItem.DocCode, newItem);
                }

                DropAll<CentrResponsibility>();
                UpdateList(CentrResponsibilities.Values.Cast<CentrResponsibility>(), now);

                foreach (var item in Context.SD_99.AsNoTracking().ToList())
                {
                    var newItem = new SDRState();
                    newItem.LoadFromEntity(item);
                    if (!SDRStates.ContainsKey(newItem.DocCode))
                        SDRStates.Add(newItem.DocCode, newItem);
                }

                DropAll<SDRState>();
                UpdateList(SDRStates.Values.Cast<SDRState>(), now);

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

                foreach (var item in Context.SD_148.AsNoTracking().ToList())
                {
                    var newItem = new ClientCategory();
                    newItem.LoadFromEntity(item);
                    if (!ClientCategories.ContainsKey(newItem.DocCode))
                        ClientCategories.Add(newItem.DocCode, newItem);
                }

                DropAll<ClientCategory>();
                UpdateList(ClientCategories.Values.Cast<ClientCategory>(), now);

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

                foreach (var item in Context.UD_43.AsNoTracking().ToList())
                {
                    var newItem = new KontragentGroup();
                    newItem.LoadFromEntity(item);
                    if (!KontragentGroups.ContainsKey(newItem.Id))
                        KontragentGroups.Add(newItem.Id, newItem);
                }

                DropAll<KontragentGroup>();
                UpdateList(KontragentGroups.Values.Cast<KontragentGroup>(), now);

                foreach (var item in Context.SD_119.AsNoTracking().ToList())
                {
                    var newItem = new NomenklType();
                    newItem.LoadFromEntity(item);
                    if (!NomenklTypes.ContainsKey(newItem.DocCode))
                        NomenklTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<NomenklType>();
                UpdateList(NomenklTypes.Values.Cast<NomenklType>(), now);


                foreach (var item in Context.SD_50.AsNoTracking().ToList())
                {
                    var newItem = new ProductType();
                    newItem.LoadFromEntity(item);
                    if (!ProductTypes.ContainsKey(newItem.DocCode))
                        ProductTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<ProductType>();
                UpdateList(ProductTypes.Values.Cast<ProductType>(), now);

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
                UpdateList(Employees.Values.Cast<Employee>(), now);


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

                foreach (var item in Context.SD_111.AsNoTracking().ToList())
                {
                    var newItem = new MutualSettlementType();
                    newItem.LoadFromEntity(item);
                    if (!MutualSettlementTypes.ContainsKey(newItem.DocCode))
                        MutualSettlementTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<MutualSettlementType>();
                UpdateList(MutualSettlementTypes.Values.Cast<MutualSettlementType>(), now);

                foreach (var item in Context.Projects.AsNoTracking().ToList())
                {
                    var newItem = new Project();
                    newItem.LoadFromEntity(item, this);
                    if (!Projects.ContainsKey(newItem.Id))
                        Projects.Add(newItem.Id, newItem);
                }

                DropAllGuid<Project>();
                UpdateListGuid(Projects.Values.Cast<Project>(), now);

                foreach (var item in Context.SD_102.AsNoTracking().ToList())
                {
                    var newItem = new ContractType();
                    newItem.LoadFromEntity(item);
                    if (!ContractTypes.ContainsKey(newItem.DocCode))
                        ContractTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<ContractType>();
                UpdateList(ContractTypes.Values.Cast<ContractType>(), now);

                foreach (var item in Context.SD_44.AsNoTracking().ToList())
                {
                    var newItem = new Bank();
                    newItem.LoadFromEntity(item);
                    if (!ContractTypes.ContainsKey(newItem.DocCode))
                        Banks.Add(newItem.DocCode, newItem);
                }

                DropAll<Bank>();
                UpdateList(Banks.Values.Cast<Bank>(), now);

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
                UpdateList(Kontragents.Values.Cast<Kontragent>().ToList(), now);

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

                NomenklMains = Context.NomenklMain.AsNoTracking()
                    .Select(entity => new NomenklMain
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
                    })
                    .ToDictionary<NomenklMain, Guid, INomenklMain>(newItem => newItem.Id, newItem => newItem);


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
                    Nomenkls.Add(item.DocCode, item);
                }

                DropAll<Nomenkl>();
                UpdateList(Nomenkls.Values.Cast<Nomenkl>(), now);

                foreach (var item in Context.SD_82.AsNoTracking().ToList())
                {
                    var newItem = new NomenklGroup();
                    newItem.LoadFromEntity(item);
                    if (!NomenklGroups.ContainsKey(newItem.DocCode))
                        NomenklGroups.Add(newItem.DocCode, newItem);
                }

                DropAll<NomenklGroup>();
                using (var nomenklClient = redisManager.GetClient())
                {
                    nomenklClient.Db = GlobalOptions.RedisDBId ?? 0;
                    var noms = nomenklClient.As<Nomenkl>().Lists["Cache:Nomenkl"].GetAll();
                    foreach (var g in NomenklGroups.Values.Cast<NomenklGroup>().Where(_ => _.ParentDC is null))
                        g.NomenklCount = getCountNomenklGroup(g, noms);
                }

                UpdateList(NomenklGroups.Values.Cast<NomenklGroup>(), now);
            }
        }

        ClearAll();
        isStartLoad = false;
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
                case RedisMessageChannels.RegionReference:
                case RedisMessageChannels.BankAccountReference:
                case RedisMessageChannels.CashBoxReference:
                case RedisMessageChannels.CentrResponsibilityReference:
                case RedisMessageChannels.ClientCategoryReference:
                case RedisMessageChannels.ContractTypeReference:
                case RedisMessageChannels.CountryReference:
                case RedisMessageChannels.CurrencyReference:
                case RedisMessageChannels.DeliveryConditionReference:
                case RedisMessageChannels.EmployeeReference:
                case RedisMessageChannels.KontragentGroupReference:
                case RedisMessageChannels.MutualSettlementTypeReference:
                case RedisMessageChannels.NomenklGroupReference:
                case RedisMessageChannels.NomenklMainReference:
                case RedisMessageChannels.NomenklProductTypeReference:
                case RedisMessageChannels.NomenklTypeReference:
                case RedisMessageChannels.PayConditionReference:
                case RedisMessageChannels.PayFormReference:
                case RedisMessageChannels.ProjectReference:
                case RedisMessageChannels.ProductTypeReference:
                case RedisMessageChannels.SDRSchetReference:
                case RedisMessageChannels.SDRStateReference:
                case RedisMessageChannels.WarehouseReference:
                case RedisMessageChannels.UnitReference:
                    break;
                case RedisMessageChannels.KontragentReference:
                    if (message.DocCode is null) return;
                    if (Kontragents.ContainsKey(message.DocCode.Value))
                    {
                        var entity = Context.SD_43.Include(_ => _.SD_2).FirstOrDefault();
                        if (entity is null) return;
                        var kontr = new Kontragent
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
                        };
                        Kontragents[message.DocCode.Value] = kontr;
                    }

                    break;
                default:
                    Console.WriteLine($"{channel} - не обработан");
                    break;
            }
        }
    }

    private int getCountNomenklGroup(NomenklGroup grp, List<Nomenkl> noms)
    {
        var subGroups =
            NomenklGroups.Values.Cast<NomenklGroup>().Where(_ => _.ParentDC == grp.DocCode).ToList();
        if (!subGroups.Any()) return noms.Count(_ => _.GroupDC == grp.DocCode);

        var cnt = noms.Count(_ => _.GroupDC == grp.DocCode);
        foreach (var sg in subGroups)
        {
            sg.NomenklCount = getCountNomenklGroup(sg, noms);
            cnt += sg.NomenklCount;
        }

        return cnt;
    }

    private void ClearAll()
    {
        //CashBoxes.Clear();
        //CentrResponsibilities.Clear();
        //ClientCategories.Clear();
        //ContractTypes.Clear();
        //Countries.Clear();
        //Currencies.Clear();
        //Employees.Clear();
        //KontragentGroups.Clear();
        //MutualSettlementTypes.Clear();
        //NomenklGroups.Clear();
        //NomenklProductTypes.Clear();
        //NomenklTypes.Clear();
        //PayConditions.Clear();
        //PayForms.Clear();
        //ProductTypes.Clear();
        //Projects.Clear();
        //Regions.Clear();
        //SDRSchets.Clear();
        //SDRStates.Clear();
        //Units.Clear();
        //Warehouses.Clear();
        //DeliveryConditions.Clear();
        Kontragents.Clear();
        NomenklMains.Clear();
        Nomenkls.Clear();
    }


    private bool IsTimerOut<T>(T item) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var timers = redisClient.As<ItemReferenceUpdate>();
            var itemTimer = timers.Lists[$"Cache:{typeof(T).Name}:Timers"].GetAll()
                .FirstOrDefault(_ => _.Id == item.DocCode.ToString());
            return ((ICache)item).LastUpdateServe < (itemTimer?.UpdateTime ?? DateTime.MaxValue);
        }
    }

    private bool IsTimerOutGuid<T>(T item) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var timers = redisClient.As<ItemReferenceUpdate>();
            var itemTimer = timers.Lists[$"Cache:{typeof(T).Name}:Timers"].GetAll()
                .FirstOrDefault(_ => _.Id == item.Id.ToString());
            return ((ICache)item).LastUpdateServe < (itemTimer?.UpdateTime ?? DateTime.MaxValue);
        }
    }

    /// <summary>
    ///     Получения списка из редиса, если кол-во в кэше не совпадает, либо в справочник были внесены изменения
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    private Tuple<bool, IEnumerable<T>> GetIsTimerOut<T>(IEnumerable<T> items) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            Tuple<bool, IEnumerable<T>> ret;
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var timers = redisClient.As<ItemReferenceUpdate>();
            var itemsTimer = timers.Lists[$"Cache:{typeof(T).Name}:Timers"].GetAll();
            var itemsList = items.ToList();
            if (itemsTimer.Count != itemsList.Count()) return new Tuple<bool, IEnumerable<T>>(true, GetAll<T>());

            return (from itemTimer in itemsTimer
                let old = itemsList.FirstOrDefault(_ => _.DocCode == Convert.ToDecimal(itemTimer.Id))
                where old is null || itemTimer.UpdateTime > ((ICache)old).LastUpdateServe
                select itemTimer).Any()
                ? new Tuple<bool, IEnumerable<T>>(true, GetAll<T>())
                : new Tuple<bool, IEnumerable<T>>(false, null);
        }
    }

    private Tuple<bool, IEnumerable<T>> GetIsTimerOutGuid<T>(IEnumerable<T> items) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            Tuple<bool, IEnumerable<T>> ret;
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var timers = redisClient.As<ItemReferenceUpdate>();
            var itemsTimer = timers.Lists[$"Cache:{typeof(T).Name}:Timers"].GetAll();
            var itemsList = items.ToList();
            if (itemsTimer.Count != itemsList.Count()) return new Tuple<bool, IEnumerable<T>>(true, GetAllGuid<T>());

            return (from itemTimer in itemsTimer
                let old = itemsList.FirstOrDefault(_ => _.Id == Guid.Parse(itemTimer.Id))
                where old is null || itemTimer.UpdateTime > ((ICache)old).LastUpdateServe
                select itemTimer).Any()
                ? new Tuple<bool, IEnumerable<T>>(true, GetAllGuid<T>())
                : new Tuple<bool, IEnumerable<T>>(false, null);
        }
    }


    #region Dictionaries

    private readonly Dictionary<decimal, INomenklProductType> NomenklProductTypes
        = new Dictionary<decimal, INomenklProductType>();

    private readonly Dictionary<int, IKontragentGroup> KontragentGroups =
        new Dictionary<int, IKontragentGroup>();

    private readonly Dictionary<decimal, IDeliveryCondition> DeliveryConditions =
        new Dictionary<decimal, IDeliveryCondition>();

    private Dictionary<decimal, IKontragent> Kontragents = new Dictionary<decimal, IKontragent>();
    private Dictionary<decimal, INomenkl> Nomenkls = new Dictionary<decimal, INomenkl>();
    private Dictionary<Guid, INomenklMain> NomenklMains = new Dictionary<Guid, INomenklMain>();

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
        var items = list.Where(_ => _ is not null).ToList();
        if (!items.ToList().Any()) return;
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var now = nowFix ?? DateTime.Now;
            var redis = redisClient.As<T>();
            var timers = redisClient.As<ItemReferenceUpdate>();
            var timerItems = items.Select(item => new ItemReferenceUpdate
                { Id = item.Id.ToString(), UpdateTime = now }).ToList();
            redis.Lists[$"Cache:{typeof(T).Name}"].AddRange(items);
            timers.Lists[$"Cache:{typeof(T).Name}:Timers"].AddRange(timerItems);
        }
    }

    public void AddOrUpdateGuid<T>(T item) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var timers = redisClient.As<ItemReferenceUpdate>();
            var list = redis.Lists[$"Cache:{typeof(T).Name}"].GetAll();
            var listTimer = timers.Lists[$"Cache:{typeof(T).Name}:Timers"].GetAll();
            var now = DateTime.Now;
            ((ICache)item).LastUpdateServe = now;
            var old = list.FirstOrDefault(_ => _.Id == item.Id);
            if (old is null)
            {
                list.Add(item);
                redis.Lists[$"Cache:{typeof(T).Name}"].Add(item);
            }
            else
            {
                redis.Lists[$"Cache:{typeof(T).Name}"].RemoveValue(old);
                redis.Lists[$"Cache:{typeof(T).Name}"].Add(item);
            }

            var oldTimer = listTimer.FirstOrDefault(_ => _.Id == item.Id.ToString());
            var newTimer = new ItemReferenceUpdate
            {
                Id = item.Id.ToString(),
                UpdateTime = now
            };
            if (oldTimer is not null)
                timers.Lists[$"Cache:{typeof(T).Name}:Timers"].RemoveValue(oldTimer);
            timers.Lists[$"Cache:{typeof(T).Name}:Timers"].Add(newTimer);
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
            var redis = redisClient.As<T>();
            var timers = redisClient.As<ItemReferenceUpdate>();
            redis.Lists[$"Cache:{typeof(T).Name}"].RemoveAll();
            timers.Lists[$"Cache:{typeof(T).Name}:Timers"].RemoveAll();
        }
    }

    public void DropGuid<T>(Guid id) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var timers = redisClient.As<ItemReferenceUpdate>();
            var list = redis.Lists[$"Cache:{typeof(T).Name}"].GetAll();
            var listTimer = timers.Lists[$"Cache:{typeof(T).Name}:Timers"].GetAll();
            var old = list.FirstOrDefault(_ => _.Id == id);
            if (old is null) return;

            redis.Lists[$"Cache:{typeof(T).Name}"].RemoveValue(old);
            var oldTimer = listTimer.FirstOrDefault(_ => _.Id == id.ToString());
            if (oldTimer is not null) timers.Lists[$"Cache:{typeof(T).Name}:Timers"].Remove(oldTimer);
        }
    }

    public T GetItemGuid<T>(Guid id) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var list = redis.Lists[$"Cache:{typeof(T).Name}"].GetAll();
            var item = list.FirstOrDefault(_ => _.Id == id);
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
            var list = redis.Lists[$"Cache:{typeof(T).Name}"].GetAll();
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
            var list = redis.Lists[$"Cache:{typeof(T).Name}"].GetAll().Where(_ => idList.Contains(_.Id));
            return list;
        }
    }

    private IEnumerable<ItemReferenceUpdate> GetChangeTracker<T>()
    {
        using (var redisClient = redisManager.GetClient())
        {
            var timers = redisClient.As<ItemReferenceUpdate>();
            return timers.Lists[$"Cache:{typeof(T).Name}:Timers"].GetAll();
        }
    }

    private IEnumerable<T> GetRawAll<T>()
    {
        using (var redisClient = redisManager.GetClient())
        {
            var data = redisClient.As<T>();
            return data.Lists[$"Cache:{typeof(T).Name}"].GetAll();
        }
    }

    #endregion

    #region Generic decimal Id

    public void UpdateList<T>(IEnumerable<T> list, DateTime? nowFix = null) where T : IDocCode
    {
        var items = list.Where(_ => _ is not null).ToList();
        if (!items.ToList().Any()) return;
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var now = nowFix ?? DateTime.Now;
            var redis = redisClient.As<T>();
            var timers = redisClient.As<ItemReferenceUpdate>();
            var timerItems = items.Select(item => new ItemReferenceUpdate
                { Id = item.DocCode.ToString(), UpdateTime = now }).ToList();
            foreach (var item in items) ((ICache)item).LastUpdateServe = now;
            redis.Lists[$"Cache:{typeof(T).Name}"].AddRange(items);
            timers.Lists[$"Cache:{typeof(T).Name}:Timers"].AddRange(timerItems);
        }
    }


    public void AddOrUpdate<T>(T item) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var timers = redisClient.As<ItemReferenceUpdate>();
            var list = redis.Lists[$"Cache:{typeof(T).Name}"].GetAll();
            var listTimer = timers.Lists[$"Cache:{typeof(T).Name}:Timers"].GetAll();
            var now = DateTime.Now;
            ((ICache)item).LastUpdateServe = now;
            var old = list.FirstOrDefault(_ => _.DocCode == item.DocCode);
            if (old is null)
            {
                list.Add(item);
                redis.Lists[$"Cache:{typeof(T).Name}"].Add(item);
            }
            else
            {
                list.Remove(old);
                list.Add(item);
                redis.Lists[$"Cache:{typeof(T).Name}"].RemoveValue(old);
                redis.Lists[$"Cache:{typeof(T).Name}"].Add(item);
            }

            var oldTimer = listTimer.FirstOrDefault(_ => _.Id == item.DocCode.ToString());
            var newTimer = new ItemReferenceUpdate
            {
                Id = item.DocCode.ToString(),
                UpdateTime = now
            };
            if (oldTimer is not null)
                timers.Lists[$"Cache:{typeof(T).Name}:Timers"].RemoveValue(oldTimer);
            timers.Lists[$"Cache:{typeof(T).Name}:Timers"].Add(newTimer);
            var message = new RedisMessage
            {
                DocumentType = DocumentType.None,
                DocCode = item.DocCode,
                DocDate = DateTime.Now,
                IsDocument = false,
                OperationType = RedisMessageDocumentOperationTypeEnum.Execute,
                Message =
                    $"Пользователь '{GlobalOptions.UserInfo.Name}' обновил справочник {typeof(T).Name} {item.DocCode}"
            };
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
            var redis = redisClient.As<T>();
            var timers = redisClient.As<ItemReferenceUpdate>();
            redis.Lists[$"Cache:{typeof(T).Name}"].RemoveAll();
            timers.Lists[$"Cache:{typeof(T).Name}:Timers"].RemoveAll();
        }
    }

    public void Drop<T>(decimal dc) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var timers = redisClient.As<ItemReferenceUpdate>();
            var list = redis.Lists[$"Cache:{typeof(T).Name}"].GetAll();
            var listTimer = timers.Lists[$"Cache:{typeof(T).Name}:Timers"].GetAll();
            var old = list.FirstOrDefault(_ => _.DocCode == dc);
            if (old is null) return;

            redis.Lists[$"Cache:{typeof(T).Name}"].RemoveValue(old);
            var oldTimer = listTimer.FirstOrDefault(_ => _.Id == dc.ToString());
            if (oldTimer is not null) timers.Lists[$"Cache:{typeof(T).Name}:Timers"].Remove(oldTimer);
        }
    }

    public T GetItem<T>(decimal dc) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var redis = redisClient.As<T>();
            var list = redis.Lists[$"Cache:{typeof(T).Name}"].GetAll();
            var item = list.FirstOrDefault(_ => _.DocCode == dc);
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
            var list = redis.Lists[$"Cache:{typeof(T).Name}"].GetAll();
            if (!isStartLoad)
                foreach (var item in list)
                    ((ICache)item).LoadFromCache();
            return list;
        }
    }

    #endregion
}
