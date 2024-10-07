using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.IReferences.Nomenkl;
using ServiceStack.Redis;
using ServiceStack.Text;

namespace KursDomain.References.RedisCache;

public interface ICache
{

    void LoadFromCache();
}

public class RedisCacheReferences : IReferencesCache
{
    public const string KontargentDocCodeGuidRelateionsCacheName = "Cache:Kontragent:DCGuidRelations";
    public const string NomenklDocCodeGuidRelateionsCacheName = "Cache:Nomenkl:DCGuidRelations";
    public const string EmployeeTabnumberGuidRelateionsCacheName = "Cache:Employee:TabelnumberGuidRelations";

    private readonly RedisManagerPool redisManager =
        new RedisManagerPool(ConfigurationManager.AppSettings["redis.connection"]);


    public bool IsChangeTrackingOn { get; set; }
    public DbContext DBContext { get; }

    public ICashBox GetCashBox(decimal? dc)
    {
        return dc is null ? null : GetCashBox(dc.Value);
    }

    public ICashBox GetCashBox(decimal dc)
    {
        return GetItem<CashBox>(dc);
    }

    public IEnumerable<ICashBox> GetCashBoxAll()
    {
        return GetItems<CashBox>().ToList();
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
        return GetItems<DeliveryCondition>().ToList();
    }

    public INomenklType GetNomenklType(decimal? dc)
    {
        return dc is not null ? GetItem<NomenklType>(dc.Value) : null;
    }

    public IEnumerable<INomenklType> GetNomenklTypeAll()
    {
        return GetItems<NomenklType>();
    }

    public INomenklProductType GetNomenklProductType(decimal? dc)
    {
        return dc is not null ? GetNomenklProductType(dc.Value) : null;
    }

    public INomenklProductType GetNomenklProductType(decimal dc)
    {
        return GetItem<NomenklProductType>(dc);
    }

    public IEnumerable<INomenklProductType> GetNomenklProductTypesAll()
    {
        return GetItems<NomenklProductType>();
    }

    public IProductType GetProductType(decimal? dc)
    {
        return dc is not null ? GetProductType(dc.Value) : null;
    }

    public IProductType GetProductType(decimal dc)
    {
        return GetItem<ProductType>(dc);
    }

    public IEnumerable<IProductType> GetProductTypeAll()
    {
        return GetItems<ProductType>();
    }

    public ICentrResponsibility GetCentrResponsibility(decimal? dc)
    {
        return dc is not null ? GetItem<CentrResponsibility>(dc.Value) : null;
    }

    public IEnumerable<ICentrResponsibility> GetCentrResponsibilitiesAll()
    {
        return GetItems<CentrResponsibility>().ToList();
    }

    public IBank GetBank(decimal? dc)
    {
        return dc is not null ? GetItem<Bank>(dc.Value) : null;
    }

    public IEnumerable<IBank> GetBanksAll()
    {
        return GetItems<Bank>();
    }

    public IBankAccount GetBankAccount(decimal? dc)
    {
        return dc is not null ? GetItem<BankAccount>(dc.Value) : null;
    }

    public IEnumerable<IBankAccount> GetBankAccountAll()
    {
        return GetItems<BankAccount>();
    }

    public IKontragentGroup GetKontragentGroup(int? id)
    {
        return id is not null ? GetItem<KontragentGroup>(id.Value) : null;
    }

    public IEnumerable<IKontragentGroup> GetKontragentCategoriesAll()
    {
        return GetItems<KontragentGroup>();
    }

    public IKontragent GetKontragent(decimal? dc)
    {
        return dc is not null ? GetKontragent(dc.Value) : null;
    }

    public IKontragent GetKontragent(decimal dc)
    {
        return GetItem<Kontragent>(dc);
    }

    public IKontragent GetKontragent(Guid? id)
    {
        if (id is null) return null;
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var dcGuid = redisClient.GetAllItemsFromSet(KontargentDocCodeGuidRelateionsCacheName)
                .Select(JsonSerializer.DeserializeFromString<DCGuidRelations>)
                .FirstOrDefault(i => i.Id == id.Value);
            return dcGuid is null ? null : GetKontragent(dcGuid.DocCode);
        }
    }

    public IEnumerable<IKontragent> GetKontragentsAll()
    {
        return GetItems<Kontragent>();
    }

    public INomenkl GetNomenkl(Guid? id)
    {
        if (id is null) return null;
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var dcGuid = redisClient.GetAllItemsFromSet(NomenklDocCodeGuidRelateionsCacheName)
                .Select(JsonSerializer.DeserializeFromString<DCGuidRelations>)
                .FirstOrDefault(i => i.Id == id.Value);
            return dcGuid is null ? null : GetNomenkl(dcGuid.DocCode);
        }
    }

    public INomenkl GetNomenkl(Guid id)
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var dcGuid = redisClient.GetAllItemsFromSet(KontargentDocCodeGuidRelateionsCacheName)
                .Select(JsonSerializer.DeserializeFromString<DCGuidRelations>)
                .FirstOrDefault(i => i.Id == id);
            return dcGuid is null ? null : GetNomenkl(dcGuid.DocCode);
        }
    }

    public INomenkl GetNomenkl(decimal? dc)
    {
        return dc is not null ? GetItem<Nomenkl>(dc.Value) : null;
    }

    public INomenkl GetNomenkl(decimal dc)
    {
        return GetItem<Nomenkl>(dc);
    }

    public IEnumerable<INomenkl> GetNomenklsAll()
    {
        return GetItems<Nomenkl>();
    }

    public INomenklMain GetNomenklMain(Guid? id)
    {
        return id is not null ? GetItemGuid<NomenklMain>(id.Value) : null;
    }

    public INomenklMain GetNomenklMain(Guid id)
    {
        return GetItemGuid<NomenklMain>(id);
    }

    public IEnumerable<INomenklMain> GetNomenklMainAll()
    {
        return GetItemsGuid<NomenklMain>();
    }

    public INomenklGroup GetNomenklGroup(decimal? dc)
    {
        return dc is not null ? GetItem<NomenklGroup>(dc.Value) : null;
    }

    public IEnumerable<INomenklGroup> GetNomenklGroupAll()
    {
        return GetItems<NomenklGroup>();
    }

    public IWarehouse GetWarehouse(decimal? dc)
    {
        return dc is not null ? GetItem<Warehouse>(dc.Value) : null;
    }

    public IWarehouse GetWarehouse(decimal dc)
    {
        return GetItem<Warehouse>(dc);
    }

    public IWarehouse GetWarehouse(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IWarehouse> GetWarehousesAll()
    {
        return GetItems<Warehouse>();
    }

    public IEmployee GetEmployee(int? tabelNumber)
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var dcGuid = redisClient.GetAllItemsFromSet(EmployeeTabnumberGuidRelateionsCacheName)
                .Select(JsonSerializer.DeserializeFromString<DCIntRelations>)
                .FirstOrDefault(i => i.Id == tabelNumber);
            return dcGuid is null ? null : GetEmployee(dcGuid.DocCode);
        }
    }

    public IEmployee GetEmployee(decimal? dc)
    {
        return dc is not null ? GetItem<Employee>(dc.Value) : null;
    }

    public IEmployee GetEmployee(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IEmployee> GetEmployees()
    {
        return GetItems<Employee>();
    }

    public ISDRSchet GetSDRSchet(decimal? dc)
    {
        return dc is not null ? GetItem<SDRSchet>(dc.Value) : null;
    }

    public ISDRSchet GetSDRSchet(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISDRSchet> GetSDRSchetAll()
    {
        return GetItems<SDRSchet>();
    }

    public ISDRState GetSDRState(decimal? dc)
    {
        return dc is not null ? GetItem<SDRState>(dc.Value) : null;
    }

    public ISDRState GetSDRState(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISDRState> GetSDRStateAll()
    {
        return GetItems<SDRState>();
    }

    public IClientCategory GetClientCategory(decimal? dc)
    {
        return dc is not null ? GetItem<ClientCategory>(dc.Value) : null;
    }

    public IClientCategory GetClientCategory(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IClientCategory> GetClientCategoriesAll()
    {
        return GetItems<ClientCategory>();
    }

    public ICurrency GetCurrency(decimal? dc)
    {
        return dc is not null ? GetItem<Currency>(dc.Value) : null;
    }

    public ICurrency GetCurrency(decimal dc)
    {
        return GetItem<Currency>(dc);
    }

    public IEnumerable<ICurrency> GetCurrenciesAll()
    {
        return GetItems<Currency>();
    }

    public ICountry GetCountry(Guid? id)
    {
        return id is not null ? GetItemGuid<Country>(id.Value) : null;
    }

    public IEnumerable<ICountry> GetCountriesAll()
    {
        return GetItemsGuid<Country>();
    }

    public IRegion GetRegion(decimal? dc)
    {
        return dc is not null ? GetItem<Region>(dc.Value) : null;
    }

    public IRegion GetRegion(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IRegion> GetRegionsAll()
    {
        return GetItems<Region>();
    }

    public IUnit GetUnit(decimal? dc)
    {
        return dc is not null ? GetItem<Unit>(dc.Value) : null;
    }

    public IUnit GetUnit(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IUnit> GetUnitsAll()
    {
        return GetItems<Unit>();
    }

    public IMutualSettlementType GetMutualSettlementType(decimal? dc)
    {
        return dc is not null ? GetItem<MutualSettlementType>(dc.Value) : null;
    }

    public IEnumerable<IMutualSettlementType> GetMutualSettlementTypeAll()
    {
        return GetItems<MutualSettlementType>();
    }

    public IProject GetProject(Guid? id)
    {
        return id is not null ? GetItemGuid<Project>(id.Value) : null;
    }

    public IEnumerable<IProject> GetProjectsAll()
    {
        return GetItemsGuid<Project>();
    }

    public IContractType GetContractType(decimal? dc)
    {
        return dc is not null ? GetItem<ContractType>(dc.Value) : null;
    }

    public IEnumerable<IContractType> GetContractsTypeAll()
    {
        return GetItems<ContractType>();
    }

    public IPayForm GetPayForm(decimal? dc)
    {
        return dc is not null ? GetItem<PayForm>(dc.Value) : null;
    }

    public IEnumerable<IPayForm> GetPayFormAll()
    {
        return GetItems<PayForm>();
    }

    public IPayCondition GetPayCondition(decimal? dc)
    {
        return dc is not null ? GetItem<PayCondition>(dc.Value) : null;
    }

    public IEnumerable<IPayCondition> GetPayConditionAll()
    {
        return GetItems<PayCondition>();
    }

    public void StartLoad()
    {
        using (var redisClient = redisManager.GetClient())
        {

            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            var Context = GlobalOptions.GetEntities();
            var isUpdateInvoice = Convert.ToInt32(GlobalOptions.SystemProfile.Profile.FirstOrDefault(_ =>
                _.SECTION == "CACHE" &&
                _.ITEM == "UPDATE_ON_STATY")?.ITEM_VALUE ?? "0");
            if (isUpdateInvoice == 1)
            {
                var NomenklProductTypes
                    = new Dictionary<decimal, INomenklProductType>();

                var KontragentGroups =
                    new Dictionary<int, IKontragentGroup>();

                var DeliveryConditions =
                    new Dictionary<decimal, IDeliveryCondition>();

                var Kontragents = new Dictionary<decimal, IKontragent>();
                var Nomenkls = new Dictionary<decimal, INomenkl>();
                var NomenklMains = new Dictionary<Guid, INomenklMain>();

                var NomenklGroups =
                    new Dictionary<decimal, INomenklGroup>();

                var Warehouses = new Dictionary<decimal, IWarehouse>();
                var Employees = new Dictionary<decimal, IEmployee>();
                var Banks = new Dictionary<decimal, IBank>();
                var BankAccounts = new Dictionary<decimal, IBankAccount>();

                var CentrResponsibilities =
                    new Dictionary<decimal, ICentrResponsibility>();

                var SDRSchets = new Dictionary<decimal, ISDRSchet>();
                var SDRStates = new Dictionary<decimal, ISDRState>();
                var ClientCategories = new Dictionary<decimal, IClientCategory>();
                var Currencies = new Dictionary<decimal, ICurrency>();
                var Regions = new Dictionary<decimal, IRegion>();
                var Units = new Dictionary<decimal, IUnit>();
                var CashBoxes = new Dictionary<decimal, ICashBox>();

                var MutualSettlementTypes =
                    new Dictionary<decimal, IMutualSettlementType>();

                var Countries = new Dictionary<Guid, ICountry>();
                var Projects = new Dictionary<Guid, IProject>();
                var ContractTypes = new Dictionary<decimal, IContractType>();
                var NomenklTypes = new Dictionary<decimal, INomenklType>();
                var ProductTypes = new Dictionary<decimal, IProductType>();
                var PayForms = new Dictionary<decimal, IPayForm>();
                var PayConditions = new Dictionary<decimal, IPayCondition>();

                foreach (var item in Context.SD_301.AsNoTracking().ToList())
                {
                    var newItem = new Currency();
                    newItem.LoadFromEntity(item);
                    if (!Currencies.ContainsKey(newItem.DocCode))
                        Currencies.Add(newItem.DocCode, newItem);
                }

                DropAll<Currency>();
                UpdateList(Currencies.Values.Cast<Currency>());


                foreach (var item in Context.Countries.AsNoTracking().ToList())
                {
                    var newItem = new Country();
                    newItem.LoadFromEntity(item);
                    if (!Countries.ContainsKey(newItem.Id))
                        Countries.Add(newItem.Id, newItem);
                }

                DropAllGuid<Country>();
                UpdateListGuid(Countries.Values.Cast<Country>());

                foreach (var item in Context.SD_23.AsNoTracking().ToList())
                {
                    var newItem = new Region();
                    newItem.LoadFromEntity(item);
                    if (!Regions.ContainsKey(newItem.DocCode))
                        Regions.Add(newItem.DocCode, newItem);
                }

                DropAll<Region>();
                UpdateList(Regions.Values.Cast<Region>());

                foreach (var item in Context.SD_179.AsNoTracking().ToList())
                {
                    var newItem = new PayCondition();
                    newItem.LoadFromEntity(item);
                    if (!PayConditions.ContainsKey(newItem.DocCode))
                        PayConditions.Add(newItem.DocCode, newItem);
                }

                DropAll<PayCondition>();
                UpdateList(PayConditions.Values.Cast<PayCondition>());

                foreach (var item in Context.SD_189.AsNoTracking().ToList())
                {
                    var newItem = new PayForm();
                    newItem.LoadFromEntity(item);
                    if (!PayForms.ContainsKey(newItem.DocCode))
                        PayForms.Add(newItem.DocCode, newItem);
                }

                DropAll<PayForm>();
                UpdateList(PayForms.Values.Cast<PayForm>());

                foreach (var item in Context.SD_175.AsNoTracking().ToList())
                {
                    var newItem = new Unit();
                    newItem.LoadFromEntity(item);
                    if (!Units.ContainsKey(newItem.DocCode))
                        Units.Add(newItem.DocCode, newItem);
                }

                DropAll<Unit>();
                UpdateList(Units.Values.Cast<Unit>());

                foreach (var item in Context.SD_40.AsNoTracking().ToList())
                {
                    var newItem = new CentrResponsibility();
                    newItem.LoadFromEntity(item);
                    if (!CentrResponsibilities.ContainsKey(newItem.DocCode))
                        CentrResponsibilities.Add(newItem.DocCode, newItem);
                }

                DropAll<CentrResponsibility>();
                UpdateList(CentrResponsibilities.Values.Cast<CentrResponsibility>());

                foreach (var item in Context.SD_99.AsNoTracking().ToList())
                {
                    var newItem = new SDRState();
                    newItem.LoadFromEntity(item);
                    if (!SDRStates.ContainsKey(newItem.DocCode))
                        SDRStates.Add(newItem.DocCode, newItem);
                }

                DropAll<SDRState>();
                UpdateList(SDRStates.Values.Cast<SDRState>());

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
                UpdateList(SDRSchets.Values.Cast<SDRSchet>());

                foreach (var item in Context.SD_148.AsNoTracking().ToList())
                {
                    var newItem = new ClientCategory();
                    newItem.LoadFromEntity(item);
                    if (!ClientCategories.ContainsKey(newItem.DocCode))
                        ClientCategories.Add(newItem.DocCode, newItem);
                }

                DropAll<ClientCategory>();
                UpdateList(ClientCategories.Values.Cast<ClientCategory>());

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
                UpdateList(NomenklProductTypes.Values.Cast<NomenklProductType>());

                foreach (var item in Context.UD_43.AsNoTracking().ToList())
                {
                    var newItem = new KontragentGroup();
                    newItem.LoadFromEntity(item);
                    if (!KontragentGroups.ContainsKey(newItem.Id))
                        KontragentGroups.Add(newItem.Id, newItem);
                }

                DropAll<KontragentGroup>();
                UpdateList(KontragentGroups.Values.Cast<KontragentGroup>());

                foreach (var item in Context.SD_82.AsNoTracking().ToList())
                {
                    var newItem = new NomenklGroup();
                    newItem.LoadFromEntity(item);
                    if (!NomenklGroups.ContainsKey(newItem.DocCode))
                        NomenklGroups.Add(newItem.DocCode, newItem);
                }

                DropAll<NomenklGroup>();
                UpdateList(NomenklGroups.Values.Cast<NomenklGroup>());

                foreach (var item in Context.SD_119.AsNoTracking().ToList())
                {
                    var newItem = new NomenklType();
                    newItem.LoadFromEntity(item);
                    if (!NomenklTypes.ContainsKey(newItem.DocCode))
                        NomenklTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<NomenklType>();
                UpdateList(NomenklTypes.Values.Cast<NomenklType>());


                foreach (var item in Context.SD_50.AsNoTracking().ToList())
                {
                    var newItem = new ProductType();
                    newItem.LoadFromEntity(item);
                    if (!ProductTypes.ContainsKey(newItem.DocCode))
                        ProductTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<ProductType>();
                UpdateList(ProductTypes.Values.Cast<ProductType>());

                foreach (var item in Context.SD_2.AsNoTracking().ToList())
                {
                    var newItem = new Employee();
                    newItem.LoadFromEntity(item, this);
                    if (!Employees.ContainsKey(newItem.DocCode))
                        Employees.Add(newItem.DocCode, newItem);
                }

                foreach (var item in Employees.Values.Cast<Employee>())
                    item.CurrencyDC = ((IDocCode)item.Currency)?.DocCode;
                var erelations = new List<string>(Employees.Values.Select(_ => new DCIntRelations
                {
                    DocCode = ((IDocCode)_).DocCode,
                    Id = _.TabelNumber
                }).Select(JsonSerializer.SerializeToString));

                DropAll<Employee>();
                UpdateList(Employees.Values.Cast<Employee>());
                redisClient.Remove(EmployeeTabnumberGuidRelateionsCacheName);
                redisClient.AddRangeToSet(EmployeeTabnumberGuidRelateionsCacheName, erelations);

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
                UpdateList(Warehouses.Values.Cast<Warehouse>());

                foreach (var item in Context.SD_111.AsNoTracking().ToList())
                {
                    var newItem = new MutualSettlementType();
                    newItem.LoadFromEntity(item);
                    if (!MutualSettlementTypes.ContainsKey(newItem.DocCode))
                        MutualSettlementTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<MutualSettlementType>();
                UpdateList(MutualSettlementTypes.Values.Cast<MutualSettlementType>());

                foreach (var item in Context.Projects.AsNoTracking().ToList())
                {
                    var newItem = new Project();
                    newItem.LoadFromEntity(item, this);
                    if (!Projects.ContainsKey(newItem.Id))
                        Projects.Add(newItem.Id, newItem);
                }

                DropAllGuid<Project>();
                UpdateListGuid(Projects.Values.Cast<Project>());

                foreach (var item in Context.SD_102.AsNoTracking().ToList())
                {
                    var newItem = new ContractType();
                    newItem.LoadFromEntity(item);
                    if (!ContractTypes.ContainsKey(newItem.DocCode))
                        ContractTypes.Add(newItem.DocCode, newItem);
                }

                DropAll<ContractType>();
                UpdateList(ContractTypes.Values.Cast<ContractType>());

                foreach (var item in Context.SD_44.AsNoTracking().ToList())
                {
                    var newItem = new Bank();
                    newItem.LoadFromEntity(item);
                    if (!ContractTypes.ContainsKey(newItem.DocCode))
                        Banks.Add(newItem.DocCode, newItem);
                }

                DropAll<Bank>();
                UpdateList(Banks.Values.Cast<Bank>());

                foreach (var item in Context.SD_43.AsNoTracking().ToList().ToList())
                {
                    var newItem = new Kontragent();
                    newItem.LoadFromEntity(item, this);
                    if (!Kontragents.ContainsKey(newItem.DocCode))
                        Kontragents.Add(item.DOC_CODE, newItem);
                }

                DropAll<Kontragent>();
                foreach (var item in Kontragents.Values.Cast<Kontragent>())
                {
                    item.GroupDC = ((IDocCode)item.Group)?.DocCode;
                    item.ClientCategoryDC = ((IDocCode)item.ClientCategory)?.DocCode;
                    item.ResponsibleEmployeeDC = ((IDocCode)item.ResponsibleEmployee)?.DocCode;
                    item.RegionDC = ((IDocCode)item.Region)?.DocCode;
                    item.CurrencyDC = ((IDocCode)item.Currency)?.DocCode;
                }

                var krelations = new List<string>(Kontragents.Values.Select(_ => new DCGuidRelations
                {
                    DocCode = ((IDocCode)_).DocCode,
                    Id = ((IDocGuid)_).Id
                }).Select(JsonSerializer.SerializeToString));

                UpdateList(Kontragents.Values.Cast<Kontragent>());
                redisClient.Remove(KontargentDocCodeGuidRelateionsCacheName);
                redisClient.AddRangeToSet(KontargentDocCodeGuidRelateionsCacheName, krelations);

                foreach (var item in Context.SD_114.AsNoTracking().ToList())
                {
                    var newItem = new BankAccount();
                    newItem.LoadFromEntity(item, this);
                    if (!ContractTypes.ContainsKey(newItem.DocCode))
                        BankAccounts.Add(newItem.DocCode, newItem);
                }

                foreach (var item in BankAccounts.Values.Cast<BankAccount>())
                {
                    item.CentrResponsibilityDC = ((IDocCode)item.CentrResponsibility)?.DocCode;
                    item.CurrencyDC = ((IDocCode)item.Currency)?.DocCode;
                    item.KontragentDC = ((IDocCode)item.Kontragent)?.DocCode;
                    item.BankDC = ((IDocCode)item.Bank)?.DocCode;
                }

                DropAll<BankAccount>();
                UpdateList(BankAccounts.Values.Cast<BankAccount>());

                foreach (var item in Context.SD_22.Include(_ => _.TD_22).AsNoTracking().ToList())
                {
                    var newItem = new CashBox();
                    newItem.LoadFromEntity(item, this);
                    if (!CashBoxes.ContainsKey(newItem.DocCode))
                        CashBoxes.Add(newItem.DocCode, newItem);
                }

                foreach (var item in CashBoxes.Values.Cast<CashBox>())
                {
                    item.CentrResponsibilityDC = ((IDocCode)item.CentrResponsibility)?.DocCode;
                    item.DefaultCurrencyDC = ((IDocCode)item.DefaultCurrency)?.DocCode;
                    item.KontragentDC = ((IDocCode)item.Kontragent)?.DocCode;
                }

                DropAll<CashBox>();
                UpdateList(CashBoxes.Values.Cast<CashBox>());


                foreach (var item in Context.NomenklMain.AsNoTracking().ToList().ToList())
                {
                    var newItem = new NomenklMain();
                    newItem.LoadFromEntity(item, this);
                    if (!NomenklMains.ContainsKey(newItem.Id))
                        NomenklMains.Add(item.Id, newItem);
                }

                DropAllGuid<NomenklMain>();
                foreach (var item in NomenklMains.Values.Cast<NomenklMain>())
                {
                    item.CategoryDC = ((IDocCode)item.Category)?.DocCode;
                    item.ProductTypeDC = ((IDocCode)item.ProductType)?.DocCode;
                    item.NomenklTypeDC = ((IDocCode)item.NomenklType)?.DocCode;
                    item.UnitDC = ((IDocCode)item.Unit)?.DocCode;
                }

                UpdateListGuid(NomenklMains.Values.Cast<NomenklMain>());

                foreach (var item in Context.SD_83.Include(_ => _.NomenklMain).AsNoTracking().ToList().ToList())
                {
                    var newItem = new Nomenkl();
                    newItem.LoadFromEntity(item, this);
                    if (Nomenkls.ContainsKey(newItem.DocCode)) continue;
                    Nomenkls.Add(item.DOC_CODE, newItem);
                }

                DropAll<Nomenkl>();
                foreach (var item in Nomenkls.Values.Cast<Nomenkl>())
                {
                    item.GroupDC = ((IDocCode)item.Group)?.DocCode;
                    item.UnitDC = ((IDocCode)item.Unit)?.DocCode;
                    item.CurrencyDC = ((IDocCode)item.Currency)?.DocCode;
                    item.NomenklTypeDC = ((IDocCode)item.NomenklType)?.DocCode;
                    item.SDRSchetDC = ((IDocCode)item.SDRSchet)?.DocCode;
                    item.ProductTypeDC = ((IDocCode)item.ProductType)?.DocCode;
                    item.NomenklMainId = ((IDocGuid)item.NomenklMain)?.Id;
                }

                var nrelations = new List<string>(Nomenkls.Values.Select(_ => new DCGuidRelations
                {
                    DocCode = ((IDocCode)_).DocCode,
                    Id = ((IDocGuid)_).Id
                }).Select(JsonSerializer.SerializeToString));
                UpdateList(Nomenkls.Values.Cast<Nomenkl>());
                redisClient.Remove(NomenklDocCodeGuidRelateionsCacheName);
                redisClient.AddRangeToSet(NomenklDocCodeGuidRelateionsCacheName, nrelations);
            }
        }

    }


    #region Generic Guid Id

    public void UpdateListGuid<T>(IEnumerable<T> list) where T : IDocGuid
    {
        var items = list.ToList();
        if (!items.ToList().Any()) return;
        using (var redisClient = redisManager.GetClient())
        {
            var redis = redisClient.As<T>();
            redis.Db = GlobalOptions.RedisDBId ?? 0;
            var jsons = new Dictionary<string, string>();
            foreach (var item in items)
            {
                var j = JsonSerializer.SerializeToString(item);
                jsons[$"Cache:{typeof(T).Name}:{item.Id}"] = j;
            }

            redisClient.SetAll(jsons);
            var ids = redisClient.GetAllItemsFromSet($"ids:{typeof(T).Name}");
            foreach (var id in ids) redisClient.RemoveItemFromSet($"Cache:{typeof(T).Name}", id);
            redisClient.AddRangeToSet($"Cache:{typeof(T).Name}", items.Select(_ => _.Id.ToString()).ToList());
            redisClient.Save();
        }
    }

    public void AddOrUpdateGuid<T>(T item) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            var redis = redisClient.As<T>();
            redis.Db = GlobalOptions.RedisDBId ?? 0;
            var key = $"Cache:{typeof(T).Name}:{item.Id}";
            var old = redis.GetValue(key);
            if (old != null)
                redis.RemoveEntry(key);
            redis.SetValue(key, item);
        }
    }

    public void DropAllGuid<T>() where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            var redis = redisClient.As<T>();
            redis.Db = GlobalOptions.RedisDBId ?? 0;
            var ids = redisClient.GetAllItemsFromSet($"ids:{typeof(T).Name}");
            foreach (var id in ids) redisClient.RemoveItemFromSet($"ids:{typeof(T).Name}", id);

            redisClient.RemoveAll(redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:*"));
        }
    }

    public void DropGuid<T>(Guid id) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            redisClient.Remove($"Cache:{typeof(T).Name}:{id}");
        }
    }

    public T GetItemGuid<T>(Guid id) where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            var redis = redisClient.As<T>();
            redis.Db = GlobalOptions.RedisDBId ?? 0;
            return redis.GetValue($"Cache:{typeof(T).Name}:{id}");
        }
    }

    public IEnumerable<T> GetItemsGuid<T>() where T : IDocGuid
    {
        using (var redisClient = redisManager.GetClient())
        {
            var redis = redisClient.As<T>();
            redis.Db = GlobalOptions.RedisDBId ?? 0;
            return redis.GetValues(redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:*").ToList());
        }
    }

    #endregion

    #region Generic decimal Id

    public void UpdateList<T>(IEnumerable<T> list) where T : IDocCode
    {
        var items = list.ToList();
        if (!items.ToList().Any()) return;
        using (var redisClient = redisManager.GetClient())
        {
            var redis = redisClient.As<T>();
            redis.Db = GlobalOptions.RedisDBId ?? 0;
            var jsons = new Dictionary<string, string>();
            foreach (var item in items)
            {
                var j = JsonSerializer.SerializeToString(item);
                jsons[$"Cache:{typeof(T).Name}:{item.DocCode}"] = j;
            }

            redisClient.SetAll(jsons);
            var ids = redisClient.GetAllItemsFromSet($"ids:{typeof(T).Name}");
            foreach (var id in ids) redisClient.RemoveItemFromSet($"Cache:{typeof(T).Name}", id);
            redisClient.AddRangeToSet($"Cache:{typeof(T).Name}", items.Select(_ => _.DocCode.ToString()).ToList());
            redisClient.Save();
        }
    }

    public void AddOrUpdate<T>(T item) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            var redis = redisClient.As<T>();
            redis.Db = GlobalOptions.RedisDBId ?? 0;
            var key = $"Cache:{typeof(T).Name}:{item.DocCode}";
            var old = redis.GetValue(key);
            if (old != null)
                redis.RemoveEntry(key);
            redis.SetValue(key, item);
        }
    }

    public void DropAll<T>() where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            var redis = redisClient.As<T>();
            redis.Db = GlobalOptions.RedisDBId ?? 0;
            var ids = redisClient.GetAllItemsFromSet($"ids:{typeof(T).Name}");
            foreach (var id in ids) redisClient.RemoveItemFromSet($"ids:{typeof(T).Name}", id);
            redisClient.RemoveAll(redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:*"));
        }
    }

    public void Drop<T>(decimal dc) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            redisClient.Db = GlobalOptions.RedisDBId ?? 0;
            redisClient.Remove($"Cache:{typeof(T).Name}:{dc}");
        }
    }

    public T GetItem<T>(decimal dc) where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            var redis = redisClient.As<T>();
            redis.Db = GlobalOptions.RedisDBId ?? 0;
            var item = redis.GetValue($"Cache:{typeof(T).Name}:{dc}");
            ((ICache)item)?.LoadFromCache();
            return item;
        }
    }


    public IEnumerable<T> GetItems<T>() where T : IDocCode
    {
        using (var redisClient = redisManager.GetClient())
        {
            var redis = redisClient.As<T>();
            redis.Db = GlobalOptions.RedisDBId ?? 0;
            return redis.GetValues(redisClient.GetKeysByPattern($"Cache:{typeof(T).Name}:*").ToList());
        }
    }

    #endregion
}
