using System;
using System.Collections.Generic;
using KursDomain.IReferences.Kontragent;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.IReferences;

/// <summary>
///     Кэш справочников
/// </summary>
public interface IReferencesCache
{
    ICashBox GetCashBox(decimal? dc);
    IEnumerable<ICashBox> GetCashBoxAll();

    INomenklType GetNomenklType(decimal? dc);
    IEnumerable<INomenklType> GetNomenklTypeAll();

    IProductType GetProductType(decimal? dc);
    IEnumerable<IProductType> GetProductTypeAll();

    ICentrResponsibility GetCentrResponsibility(decimal? dc);
    IEnumerable<ICentrResponsibility> GetCentrResponsibilitiesAll();

    IBank GetBank(decimal? dc);
    IEnumerable<IBank> GetBanksAll();

    IBankAccount GetBankAccount(decimal? dc);
    IEnumerable<IBankAccount> GetBankAccountAll();

    IKontragentGroup GetKontragentGroup(int? id);
    IEnumerable<IKontragentGroup> GetKontragentCategoriesAll();

    IKontragent GetKontragent(decimal? dc);
    IKontragent GetKontragent(decimal dc);
    IKontragent GetKontragent(Guid? id);
    IEnumerable<IKontragent> GetKontragentsAll();

    INomenkl GetNomenkl(Guid? id);
    INomenkl GetNomenkl(decimal? dc);
    INomenkl GetNomenkl(decimal dc);
    IEnumerable<INomenkl> GetNomenklsAll();

    INomenklCategory GetNomenklCategory(decimal? dc);
    IEnumerable<INomenklCategory> GetNomenklCategoriesAll();

    IWarehouse GetWarehouse(decimal? dc);
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
