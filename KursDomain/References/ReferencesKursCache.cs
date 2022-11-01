using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.References;

public class ReferencesKursCache : IReferencesCache
{
    #region fields

    private readonly KursContext.KursContext Context;

    #endregion

    #region Constructors

    public ReferencesKursCache(DbContext dbContext)
    {
        Context = dbContext as KursContext.KursContext;
    }

    #endregion


    #region Проекты

    public IProject GetProject(Guid? id)
    {
        if (id == null) return null;
        if (Projects.ContainsKey(id.Value))
            return Projects[id.Value];
        var data = Context.Projects.FirstOrDefault(_ => _.Id == id.Value);
        if (data == null) return null;
        var newItem = new Project();
        newItem.LoadFromEntity(data, this);
        Projects.Add(data.Id, newItem);
        return Projects[data.Id];
    }

    public IEnumerable<IProject> GetProjectsAll()
    {
        return Projects.Values.ToList();
    }

    #endregion

    #region Типы договоров

    public IContractType GetContractType(decimal? dc)
    {
        if (dc == null) return null;
        if (ContractTypes.ContainsKey(dc.Value))
            return ContractTypes[dc.Value];
        var data = Context.SD_102.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new ContractType();
        newItem.LoadFromEntity(data);
        ContractTypes.Add(data.DOC_CODE, newItem);
        return ContractTypes[data.DOC_CODE];
    }

    public IEnumerable<IContractType> GetContractsTypeAll()
    {
        return ContractTypes.Values.ToList();
    }

    #endregion

    #region Методы

    public void StartLoad()
    {
        Clear();
        foreach (var item in Context.SD_301.AsNoTracking().ToList())
        {
            var newItem = new Currency();
            newItem.LoadFromEntity(item);
            if (!Currencies.ContainsKey(newItem.DocCode))
                Currencies.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.Countries.AsNoTracking().ToList())
        {
            var newItem = new Country();
            newItem.LoadFromEntity(item);
            if (!Countries.ContainsKey(newItem.Id))
                Countries.Add(newItem.Id, newItem);
        }

        foreach (var item in Context.SD_23.AsNoTracking().ToList())
        {
            var newItem = new Region();
            newItem.LoadFromEntity(item);
            if (!Regions.ContainsKey(newItem.DocCode))
                Regions.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_175.AsNoTracking().ToList())
        {
            var newItem = new Unit();
            newItem.LoadFromEntity(item);
            if (!Units.ContainsKey(newItem.DocCode))
                Units.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_40.AsNoTracking().ToList())
        {
            var newItem = new CentrResponsibility();
            newItem.LoadFromEntity(item);
            if (!CentrResponsibilities.ContainsKey(newItem.DocCode))
                CentrResponsibilities.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_99.AsNoTracking().ToList())
        {
            var newItem = new SDRState();
            newItem.LoadFromEntity(item);
            if (!SDRStates.ContainsKey(newItem.DocCode))
                SDRStates.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_303.AsNoTracking().ToList())
        {
            var newItem = new SDRSchet();
            newItem.LoadFromEntity(item, this);
            if (!SDRSchets.ContainsKey(newItem.DocCode))
                SDRSchets.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_148.AsNoTracking().ToList())
        {
            var newItem = new ClientCategory();
            newItem.LoadFromEntity(item);
            if (!ClientCategories.ContainsKey(newItem.DocCode))
                ClientCategories.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.UD_43.AsNoTracking().ToList())
        {
            var newItem = new KontragentCategory();
            newItem.LoadFromEntity(item);
            if (!KontragentCategories.ContainsKey(newItem.Id))
                KontragentCategories.Add(newItem.Id, newItem);
        }

        foreach (var item in Context.SD_82.AsNoTracking().ToList())
        {
            var newItem = new NomenklCategory();
            newItem.LoadFromEntity(item);
            if (!NomenklCategories.ContainsKey(newItem.DocCode))
                NomenklCategories.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_119.AsNoTracking().ToList())
        {
            var newItem = new NomenklType();
            newItem.LoadFromEntity(item);
            if (!NomenklTypes.ContainsKey(newItem.DocCode))
                NomenklTypes.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_50.AsNoTracking().ToList())
        {
            var newItem = new ProductType();
            newItem.LoadFromEntity(item);
            if (!ProductTypes.ContainsKey(newItem.DocCode))
                ProductTypes.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_2.AsNoTracking().ToList())
        {
            var newItem = new Employee();
            newItem.LoadFromEntity(item, this);
            if (!Employees.ContainsKey(newItem.DocCode))
                Employees.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_27.AsNoTracking().ToList())
        {
            var newItem = new Warehouse();
            newItem.LoadFromEntity(item, this);
            if (!Warehouses.ContainsKey(newItem.DocCode))
                Warehouses.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_22.Include(_ => _.TD_22).AsNoTracking().ToList())
        {
            var newItem = new CashBox();
            newItem.LoadFromEntity(item, this);
            if (!CashBoxes.ContainsKey(newItem.DocCode))
                CashBoxes.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_111.AsNoTracking().ToList())
        {
            var newItem = new MutualSettlementType();
            newItem.LoadFromEntity(item);
            if (!MutualSettlementTypes.ContainsKey(newItem.DocCode))
                MutualSettlementTypes.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.Projects.AsNoTracking().ToList())
        {
            var newItem = new Project();
            newItem.LoadFromEntity(item, this);
            if (!Projects.ContainsKey(newItem.Id))
                Projects.Add(newItem.Id, newItem);
        }

        foreach (var item in Context.SD_102.AsNoTracking().ToList())
        {
            var newItem = new ContractType();
            newItem.LoadFromEntity(item);
            if (!ContractTypes.ContainsKey(newItem.DocCode))
                ContractTypes.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_44.AsNoTracking().ToList())
        {
            var newItem = new Bank();
            newItem.LoadFromEntity(item);
            if (!ContractTypes.ContainsKey(newItem.DocCode))
                Banks.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_114.AsNoTracking().ToList())
        {
            var newItem = new BankAccount();
            newItem.LoadFromEntity(item, this);
            if (!ContractTypes.ContainsKey(newItem.DocCode))
                BankAccounts.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_43.AsNoTracking().ToList().ToList())
        {
            var newItem = new Kontragent();
            newItem.LoadFromEntity(item, this);
            if (!Kontragents.ContainsKey(newItem.DocCode))
                Kontragents.Add(item.DOC_CODE, newItem);
        }

        foreach (var item in Context.SD_83.AsNoTracking().ToList().ToList())
        {
            var newItem = new Nomenkl();
            newItem.LoadFromEntity(item, this);
            if (!Nomenkls.ContainsKey(newItem.DocCode))
                Nomenkls.Add(item.DOC_CODE, newItem);
        }
    }

    private void Clear()
    {
        Currencies.Clear();
        Countries.Clear();
        Regions.Clear();
        Units.Clear();
        CentrResponsibilities.Clear();
        SDRStates.Clear();
        SDRSchets.Clear();

        ClientCategories.Clear();
        KontragentCategories.Clear();
        NomenklCategories.Clear();
        Employees.Clear();

        Warehouses.Clear();
        CashBoxes.Clear();
        MutualSettlementTypes.Clear();
        Projects.Clear();
        ContractTypes.Clear();
        Banks.Clear();
        BankAccounts.Clear();

        Kontragents.Clear();
        Nomenkls.Clear();
    }

    #endregion


    #region Аакты взаимозачета

    public IMutualSettlementType GetMutualSettlementType(decimal? dc)
    {
        if (dc == null) return null;
        if (MutualSettlementTypes.ContainsKey(dc.Value))
            return MutualSettlementTypes[dc.Value];
        var data = Context.SD_111.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new MutualSettlementType();
        newItem.LoadFromEntity(data);
        MutualSettlementTypes.Add(data.DOC_CODE, newItem);
        return MutualSettlementTypes[data.DOC_CODE];
    }

    public IEnumerable<IMutualSettlementType> GetMutualSettlementTypeAll()
    {
        return MutualSettlementTypes.Values.ToList();
    }

    #endregion

    #region Счета и статьи расходов и доходов

    public ISDRSchet GetSDRSchet(decimal? dc)
    {
        if (dc == null) return null;
        if (SDRSchets.ContainsKey(dc.Value))
            return SDRSchets[dc.Value];
        var data = Context.SD_303.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new SDRSchet();
        newItem.LoadFromEntity(data, this);
        SDRSchets.Add(data.DOC_CODE, newItem);
        return SDRSchets[data.DOC_CODE];
    }

    public ISDRSchet GetSDRSchet(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISDRSchet> GetSDRSchetAll()
    {
        return SDRSchets.Values.ToList();
    }

    public ISDRState GetSDRState(decimal? dc)
    {
        if (dc == null) return null;
        if (SDRStates.ContainsKey(dc.Value))
            return SDRStates[dc.Value];
        var data = Context.SD_99.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new SDRState();
        newItem.LoadFromEntity(data);
        SDRStates.Add(data.DOC_CODE, newItem);
        return SDRStates[data.DOC_CODE];
    }

    public ISDRState GetSDRState(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISDRState> GetSDRStateAll()
    {
        return SDRStates.Values.ToList();
    }

    #endregion

    #region Категория клиентов

    public IClientCategory GetClientCategory(decimal? dc)
    {
        if (dc == null) return null;
        if (ClientCategories.ContainsKey(dc.Value))
            return ClientCategories[dc.Value];
        var data = Context.SD_148.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new ClientCategory();
        newItem.LoadFromEntity(data);
        ClientCategories.Add(data.DOC_CODE, newItem);
        return ClientCategories[data.DOC_CODE];
    }

    public IClientCategory GetClientCategory(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IClientCategory> GetClientCategoriesAll()
    {
        return ClientCategories.Values.ToList();
    }

    #endregion

    #region Валюта

    public ICurrency GetCurrency(decimal? dc)
    {
        if (dc == null) return null;
        if (Currencies.ContainsKey(dc.Value))
            return Currencies[dc.Value];
        var data = Context.SD_301.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Currency();
        newItem.LoadFromEntity(data);
        Currencies.Add(data.DOC_CODE, newItem);
        return Currencies[data.DOC_CODE];
    }

    public IEnumerable<ICurrency> GetCurrenciesAll()
    {
        return Currencies.Values.ToList();
    }

    #endregion

    #region Страны

    public ICountry GetCountry(Guid? id)
    {
        if (id == null) return null;
        if (Countries.ContainsKey(id.Value))
            return Countries[id.Value];
        var data = Context.Countries.FirstOrDefault(_ => _.Id == id.Value);
        if (data == null) return null;
        var newItem = new Country();
        newItem.LoadFromEntity(data);
        Countries.Add(data.Id, newItem);
        return Countries[data.Id];
    }

    public IEnumerable<ICountry> GetCountriesAll()
    {
        return Countries.Values.ToList();
    }

    #endregion

    #region Регионы

    public IRegion GetRegion(decimal? dc)
    {
        if (dc == null) return null;
        if (Regions.ContainsKey(dc.Value))
            return Regions[dc.Value];
        var data = Context.SD_23.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Region();
        newItem.LoadFromEntity(data);
        Regions.Add(data.DOC_CODE, newItem);
        return Regions[data.DOC_CODE];
    }

    public IRegion GetRegion(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IRegion> GetRegionsAll()
    {
        return Regions.Values.ToList();
    }

    #endregion

    #region Единицы измерения

    public IUnit GetUnit(decimal? dc)
    {
        if (dc == null) return null;
        if (Units.ContainsKey(dc.Value))
            return Units[dc.Value];
        var data = Context.SD_175.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Unit();
        newItem.LoadFromEntity(data);
        Units.Add(data.DOC_CODE, newItem);
        return Units[data.DOC_CODE];
    }

    public IUnit GetUnit(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IUnit> GetUnitsAll()
    {
        return Units.Values.ToList();
    }

    #endregion

    #region Kontragent

    public IKontragentCategory GetKontragentCategory(int? id)
    {
        if (id == null) return null;
        if (KontragentCategories.ContainsKey(id.Value))
            return KontragentCategories[id.Value];
        var data = Context.UD_43.FirstOrDefault(_ => _.EG_ID == id.Value);
        if (data == null) return null;
        var newItem = new KontragentCategory();
        newItem.LoadFromEntity(data);
        KontragentCategories.Add(data.EG_ID, newItem);
        return KontragentCategories[data.EG_ID];
    }

    public IEnumerable<IKontragentCategory> GetKontragentCategoriesAll()
    {
        return KontragentCategories.Values.ToList();
    }

    public IKontragent GetKontragent(decimal? dc)
    {
        if (dc == null) return null;
        if (Kontragents.ContainsKey(dc.Value))
            return Kontragents[dc.Value];
        var data = Context.SD_43.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Kontragent();
        newItem.LoadFromEntity(data, this);
        Kontragents.Add(data.DOC_CODE, newItem);
        return Kontragents[data.DOC_CODE];
    }

    public IKontragent GetKontragent(Guid? id)
    {
        if (id == null) return null;
        var k = Kontragents.Values.Cast<Kontragent>().FirstOrDefault(_ => _.Id == id.Value);
        if (k != null)
            return k;
        var data = Context.SD_43.FirstOrDefault(_ => _.Id == id.Value);
        if (data == null) return null;
        var newItem = new Kontragent();
        newItem.LoadFromEntity(data, this);
        Kontragents.Add(data.DOC_CODE, newItem);
        return Kontragents[data.DOC_CODE];
    }

    public IEnumerable<IKontragent> GetKontragentsAll()
    {
        return Kontragents.Values.ToList();
    }

    #endregion

    #region Warehouse

    public IWarehouse GetWarehouse(decimal? dc)
    {
        if (dc == null) return null;
        if (Warehouses.ContainsKey(dc.Value))
            return Warehouses[dc.Value];
        var data = Context.SD_27.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Warehouse();
        newItem.LoadFromEntity(data, this);
        Warehouses.Add(data.DOC_CODE, newItem);
        return Warehouses[data.DOC_CODE];
    }

    public IWarehouse GetWarehouse(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IWarehouse> GetWarehousesAll()
    {
        return Warehouses.Values.ToList();
    }

    #endregion

    #region Employee

    public IEmployee GetEmployee(int? tabelNumber)
    {
        if (tabelNumber == null) return null;
        var emp = Employees.Values.FirstOrDefault(_m => _m.TabelNumber == tabelNumber.Value);
        if (emp != null) return emp;
        var data = Context.SD_2.FirstOrDefault(_ => _.TABELNUMBER == tabelNumber.Value);
        if (data == null) return null;
        var newItem = new Employee();
        newItem.LoadFromEntity(data, this);
        Employees.Add(data.DOC_CODE, newItem);
        return Employees[data.DOC_CODE];
    }

    public IEmployee GetEmployee(decimal? dc)
    {
        if (dc == null) return null;
        if (Employees.ContainsKey(dc.Value))
            return Employees[dc.Value];
        var data = Context.SD_2.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Employee();
        newItem.LoadFromEntity(data, this);
        Employees.Add(data.DOC_CODE, newItem);
        return Employees[data.DOC_CODE];
    }

    public IEmployee GetEmployee(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IEmployee> GetEmployees()
    {
        return Employees.Values.ToList();
    }

    #endregion

    #region Nomenkl

    public INomenkl GetNomenkl(Guid? id)
    {
        if (id == null) return null;
        var k = Nomenkls.Values.Cast<Nomenkl>().FirstOrDefault(_ => _.Id == id.Value);
        if (k != null)
            return k;
        var data = Context.SD_83.FirstOrDefault(_ => _.Id == id.Value);
        if (data == null) return null;
        var newItem = new Nomenkl();
        newItem.LoadFromEntity(data, this);
        Nomenkls.Add(data.DOC_CODE, newItem);
        return Nomenkls[data.DOC_CODE];
    }

    public INomenkl GetNomenkl(decimal? dc)
    {
        if (dc == null) return null;
        if (Kontragents.ContainsKey(dc.Value))
            return Nomenkls[dc.Value];
        var data = Context.SD_83.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Nomenkl();
        newItem.LoadFromEntity(data, this);
        Nomenkls.Add(data.DOC_CODE, newItem);
        return Nomenkls[data.DOC_CODE];
    }

    public IEnumerable<INomenkl> GetNomenklsAll()
    {
        return Nomenkls.Values.ToList();
    }

    public INomenklCategory GetNomenklCategory(decimal? dc)
    {
        if (dc == null) return null;
        if (NomenklCategories.ContainsKey(dc.Value))
            return NomenklCategories[dc.Value];
        var data = Context.SD_82.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new NomenklCategory();
        newItem.LoadFromEntity(data);
        NomenklCategories.Add(data.DOC_CODE, newItem);
        return NomenklCategories[data.DOC_CODE];
    }

    public IEnumerable<INomenklCategory> GetNomenklCategoriesAll()
    {
        return NomenklCategories.Values.ToList();
    }

    #endregion

    #region Bank && CashBox

    public ICashBox GetCashBox(decimal? dc)
    {
        if (dc == null) return null;
        if (CashBoxes.ContainsKey(dc.Value))
            return CashBoxes[dc.Value];
        var data = Context.SD_22.Include(_ => _.TD_22).FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new CashBox();
        newItem.LoadFromEntity(data, this);
        CashBoxes.Add(data.DOC_CODE, newItem);
        return CashBoxes[data.DOC_CODE];
    }

    public IEnumerable<ICashBox> GetCashBoxAll()
    {
        return CashBoxes.Values.ToList();
    }

   public IBank GetBank(decimal? dc)
    {
        if (dc == null) return null;
        if (Banks.ContainsKey(dc.Value))
            return Banks[dc.Value];
        var data = Context.SD_44.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Bank();
        newItem.LoadFromEntity(data);
        Banks.Add(data.DOC_CODE, newItem);
        return Banks[data.DOC_CODE];
    }

    public IEnumerable<IBank> GetBanksAll()
    {
        return Banks.Values.ToList();
    }

    public IBankAccount GetBankAccount(decimal? dc)
    {
        if (dc == null) return null;
        if (BankAccounts.ContainsKey(dc.Value))
            return BankAccounts[dc.Value];
        var data = Context.SD_114.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new BankAccount();
        newItem.LoadFromEntity(data, this);
        BankAccounts.Add(data.DOC_CODE, newItem);
        return BankAccounts[data.DOC_CODE];
    }

    public IEnumerable<IBankAccount> GetBankAccountAll()
    {
        return BankAccounts.Values.ToList();
    }

    #endregion

    #region Тип продукции

    public IProductType GetProductType(decimal? dc)
    {
        if (dc == null) return null;
        if (ProductTypes.ContainsKey(dc.Value))
            return ProductTypes[dc.Value];
        var data = Context.SD_50.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new ProductType();
        newItem.LoadFromEntity(data);
        ProductTypes.Add(data.DOC_CODE, newItem);
        return ProductTypes[data.DOC_CODE];
    }

    public IEnumerable<IProductType> GetProductTypeAll()
    {
        return ProductTypes.Values.ToList();
    }

    #endregion

    #region Тип товара

    public INomenklType GetNomenklType(decimal? dc)
    {
        if (dc == null) return null;
        if (NomenklTypes.ContainsKey(dc.Value))
            return NomenklTypes[dc.Value];
        var data = Context.SD_119.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new NomenklType();
        newItem.LoadFromEntity(data);
        NomenklTypes.Add(data.DOC_CODE, newItem);
        return NomenklTypes[data.DOC_CODE];
    }

    public IEnumerable<INomenklType> GetNomenklTypeAll()
    {
        return NomenklTypes.Values.ToList();
    }

    #endregion

    #region Центр ответственности

    public ICentrResponsibility GetCentrResponsibility(decimal? dc)
    {
        if (dc == null) return null;
        if (CentrResponsibilities.ContainsKey(dc.Value))
            return CentrResponsibilities[dc.Value];
        var data = Context.SD_40.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new CentrResponsibility();
        newItem.LoadFromEntity(data);
        CentrResponsibilities.Add(data.DOC_CODE, newItem);
        return CentrResponsibilities[data.DOC_CODE];
    }

    public IEnumerable<ICentrResponsibility> GetCentrResponsibilitiesAll()
    {
        return CentrResponsibilities.Values.ToList();
    }

    #endregion

    #region Dictionaries

    private readonly Dictionary<int, IKontragentCategory> KontragentCategories =
        new Dictionary<int, IKontragentCategory>();

    private readonly Dictionary<decimal, IKontragent> Kontragents = new Dictionary<decimal, IKontragent>();
    private readonly Dictionary<decimal, INomenkl> Nomenkls = new Dictionary<decimal, INomenkl>();

    private readonly Dictionary<decimal, INomenklCategory> NomenklCategories =
        new Dictionary<decimal, INomenklCategory>();

    private readonly Dictionary<decimal, IWarehouse> Warehouses = new Dictionary<decimal, IWarehouse>();
    private readonly Dictionary<decimal, IEmployee> Employees = new Dictionary<decimal, IEmployee>();
    private readonly Dictionary<decimal, IBank> Banks = new Dictionary<decimal, IBank>();
    private readonly Dictionary<decimal, IBankAccount> BankAccounts = new Dictionary<decimal, IBankAccount>();

    private readonly Dictionary<decimal, ICentrResponsibility> CentrResponsibilities =
        new Dictionary<decimal, ICentrResponsibility>();

    private readonly Dictionary<decimal, ISDRSchet> SDRSchets = new Dictionary<decimal, ISDRSchet>();
    private readonly Dictionary<decimal, ISDRState> SDRStates = new Dictionary<decimal, ISDRState>();
    private readonly Dictionary<decimal, IClientCategory> ClientCategories = new Dictionary<decimal, IClientCategory>();
    private readonly Dictionary<decimal, ICurrency> Currencies = new Dictionary<decimal, ICurrency>();
    private readonly Dictionary<decimal, IRegion> Regions = new Dictionary<decimal, IRegion>();
    private readonly Dictionary<decimal, IUnit> Units = new Dictionary<decimal, IUnit>();
    private readonly Dictionary<decimal, ICashBox> CashBoxes = new Dictionary<decimal, ICashBox>();

    private readonly Dictionary<decimal, IMutualSettlementType> MutualSettlementTypes =
        new Dictionary<decimal, IMutualSettlementType>();

    private readonly Dictionary<Guid, ICountry> Countries = new Dictionary<Guid, ICountry>();
    private readonly Dictionary<Guid, IProject> Projects = new Dictionary<Guid, IProject>();
    private readonly Dictionary<decimal, IContractType> ContractTypes = new Dictionary<decimal, IContractType>();
    private readonly Dictionary<decimal,INomenklType> NomenklTypes = new Dictionary<decimal, INomenklType>();
    private readonly Dictionary<decimal,IProductType> ProductTypes = new Dictionary<decimal, IProductType>();

    #endregion
}
