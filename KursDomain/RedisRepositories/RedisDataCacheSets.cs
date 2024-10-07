namespace KursAM2.Repositories.RedisRepository
{
    /// <summary>
    /// Множества для хранения кэша данных
    /// </summary>
    public static class RedisDataCacheSets
    {
        public const string InvoiceProviderPayment = "CacheData:InvoiceProviderPaymentShipped";
        public const string InvoiceClientPayment = "CacheData:InvoiceClientPaymentShipped";
    }
}
