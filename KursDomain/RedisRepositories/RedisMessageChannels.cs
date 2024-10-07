namespace KursAM2.Repositories.RedisRepository
{
    /// <summary>
    /// Каналы для обмена сообщений с редис
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
    }
}
