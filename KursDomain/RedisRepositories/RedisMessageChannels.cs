namespace KursAM2.Repositories.RedisRepository;

/// <summary>
///     Каналы для обмена сообщений с редис
/// </summary>
public static class RedisMessageChannels
{
    public const string InvoiceProvider = "InvoiceProvider";
    public const string InvoiceClient = "InvoiceClient";
    public const string WayBill = "WayBill";
    public const string WarehouseOrderIn = "WarehouseOrderIn";
    public const string WarehouseOrderOut = "WarehouseOrderOut";
    public const string StartLogin = "StartLogin";

    public const string CashIn = "CashIn";
    public const string CashOut = "CashOut";
    public const string Bank = "BankTransaction";
    public const string MutualAccounting = "MutualAccounting";
    public const string MutualAccountingConvert = "MutualAccountingConvert";

    public const string NomenklReturnOfClient = "NomenklReturnOfClient";
    public const string NomenklReturnToProvider = "NomenklReturnToProvider";

    public const string BankReference = "Cache:Bank";
    public const string RegionReference = "Cache:Region";
    public const string BankAccountReference = "Cache:BankAccount";
    public const string CashBoxReference = "Cache:CashBox";
    public const string CentrResponsibilityReference = "Cache:CentrResponsibility";
    public const string ClientCategoryReference = "Cache:ClientCategory";
    public const string ContractTypeReference = "Cache:ContractType";
    public const string CountryReference = "Cache:Country";
    public const string CurrencyReference = "Cache:Currency";
    public const string DeliveryConditionReference = "Cache:DeliveryCondition";
    public const string EmployeeReference = "Cache:Employee";
    public const string KontragentGroupReference = "Cache:KontragentGroup";
    public const string MutualSettlementTypeReference = "Cache:MutualSettlementType";
    public const string NomenklGroupReference = "Cache:NomenklGroup";
    public const string NomenklMainReference = "Cache:NomenklMain";
    public const string NomenklProductTypeReference = "Cache:NomenklProductType";
    public const string NomenklTypeReference = "Cache:NomenklType";
    public const string PayConditionReference = "Cache:PayCondition";
    public const string PayFormReference = "Cache: PayForm";
    public const string ProjectReference = "Cache:Project";
    public const string ProductTypeReference = "Cache:ProductType";
    public const string SDRSchetReference = "Cache:SDRSchet";
    public const string SDRStateReference = "Cache:SDRState";
    public const string WarehouseReference = "Cache:Warehouse";
    public const string UnitReference = "Cache:Unit";
    public const string KontragentReference = "Cache:Kontragent";
    public const string NomenklReference = "Cache:Nomenkl";
}
