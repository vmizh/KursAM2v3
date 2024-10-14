using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using KursDomain.ICommon;
using KursDomain.IReferences.Kontragent;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.IReferences;

/// <summary>
///     Кэш справочников
/// </summary>
public interface IReferencesCache
{
    void UpdateListGuid<T>(IEnumerable<T> list, DateTime? nowFix) where T : IDocGuid;
    void AddOrUpdateGuid<T>(T item) where T : IDocGuid;
    void DropAllGuid<T>() where T : IDocGuid;
    void DropGuid<T>(Guid id) where T : IDocGuid;
    T GetItemGuid<T>(Guid dc) where T : IDocGuid;
    IEnumerable<T> GetAllGuid<T>() where T : IDocGuid;

    IEnumerable<T> GetListGuid<T>(IEnumerable<Guid> ids) where T : IDocGuid;
   
    void UpdateList<T>(IEnumerable<T> list, DateTime? nowFix) where T : IDocCode;
    void AddOrUpdate<T>(T item) where T : IDocCode;
    void DropAll<T>() where T : IDocCode;
    void Drop<T>(decimal dc) where T : IDocCode;
    T GetItem<T>(decimal dc) where T : IDocCode;
    IEnumerable<T> GetAll<T>() where T : IDocCode;

    IEnumerable<T> GetList<T>(IEnumerable<decimal> dcs) where T : IDocCode;


    public bool IsChangeTrackingOn { set; get; }

    public DbContext DBContext { get; }

    //Справочник касс

    ICashBox GetCashBox(decimal? dc);
    ICashBox GetCashBox(decimal dc);
    IEnumerable<ICashBox> GetCashBoxAll();

    //Справочник базовых условий поставки

    IDeliveryCondition GetDeliveryCondition(decimal dc);
    IDeliveryCondition GetDeliveryCondition(decimal? dc);
    IEnumerable<IDeliveryCondition> GetDeliveryConditionAll();

    //Справочник типов материальных ценностей (SD_119)

    INomenklType GetNomenklType(decimal? dc);
    IEnumerable<INomenklType> GetNomenklTypeAll();

    //Справочник типов продукции (SD_77)

    INomenklProductType GetNomenklProductType(decimal? dc);
    INomenklProductType GetNomenklProductType(decimal dc);

    IEnumerable<INomenklProductType> GetNomenklProductTypesAll();

    //Справочник видов продукции (SD_50)

    IProductType GetProductType(decimal? dc);
    IProductType GetProductType(decimal dc);
    IEnumerable<IProductType> GetProductTypeAll();

    //Справочник центров ответственности (SD_40)

    ICentrResponsibility GetCentrResponsibility(decimal? dc);
    IEnumerable<ICentrResponsibility> GetCentrResponsibilitiesAll();

    // Банки (SD_44)

    IBank GetBank(decimal? dc);
    IEnumerable<IBank> GetBanksAll();

    // Банковские счета (SD_114)

    IBankAccount GetBankAccount(decimal? dc);
    IEnumerable<IBankAccount> GetBankAccountAll();


    IKontragentGroup GetKontragentGroup(int? id);
    IEnumerable<IKontragentGroup> GetKontragentCategoriesAll();

    IKontragent GetKontragent(decimal? dc);
    IKontragent GetKontragent(decimal dc);
    IKontragent GetKontragent(Guid? id);
    IEnumerable<IKontragent> GetKontragentsAll();

    INomenkl GetNomenkl(Guid? id);
    INomenkl GetNomenkl(Guid id);
    INomenkl GetNomenkl(decimal? dc);
    INomenkl GetNomenkl(decimal dc);
    IEnumerable<INomenkl> GetNomenklsAll();

    IEnumerable<INomenkl> GetNomenkls(IEnumerable<decimal> dcList);


    INomenklMain GetNomenklMain(Guid? id);
    INomenklMain GetNomenklMain(Guid id);
    IEnumerable<INomenklMain> GetNomenklMainAll();


    INomenklGroup GetNomenklGroup(decimal? dc);
    IEnumerable<INomenklGroup> GetNomenklGroupAll();

    IWarehouse GetWarehouse(decimal? dc);
    IWarehouse GetWarehouse(decimal dc);
    IWarehouse GetWarehouse(Guid? id);
    IEnumerable<IWarehouse> GetWarehousesAll();

    IEmployee GetEmployee(int? tabelNumber);
    IEmployee GetEmployee(decimal? dc);
    IEmployee GetEmployee(Guid? id);
    IEnumerable<IEmployee> GetEmployees();

    ISDRSchet GetSDRSchet(decimal? dc);
    ISDRSchet GetSDRSchet(Guid? id);
    IEnumerable<ISDRSchet> GetSDRSchetAll();

    ISDRState GetSDRState(decimal? dc);
    ISDRState GetSDRState(Guid? id);
    IEnumerable<ISDRState> GetSDRStateAll();

    IClientCategory GetClientCategory(decimal? dc);
    IClientCategory GetClientCategory(Guid? id);
    IEnumerable<IClientCategory> GetClientCategoriesAll();

    ICurrency GetCurrency(decimal? dc);
    ICurrency GetCurrency(decimal dc);
    IEnumerable<ICurrency> GetCurrenciesAll();

    ICountry GetCountry(Guid? id);
    IEnumerable<ICountry> GetCountriesAll();

    IRegion GetRegion(decimal? dc);
    IRegion GetRegion(Guid? id);
    IEnumerable<IRegion> GetRegionsAll();

    IUnit GetUnit(decimal? dc);
    IUnit GetUnit(Guid? id);
    IEnumerable<IUnit> GetUnitsAll();

    IMutualSettlementType GetMutualSettlementType(decimal? dc);
    IEnumerable<IMutualSettlementType> GetMutualSettlementTypeAll();

    IProject GetProject(Guid? id);
    IEnumerable<IProject> GetProjectsAll();

    IContractType GetContractType(decimal? dc);
    IEnumerable<IContractType> GetContractsTypeAll();

    IPayForm GetPayForm(decimal? вс);
    IEnumerable<IPayForm> GetPayFormAll();

    IPayCondition GetPayCondition(decimal? dc);
    IEnumerable<IPayCondition> GetPayConditionAll();

    void StartLoad();
}
