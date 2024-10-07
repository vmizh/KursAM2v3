using System;

namespace KursDomain.References.RedisCache;

public class InvoicePaymentShipment
{
    public PaymentShipmentInvoiceTypeEnum? DocType { get; set; }
    public decimal DocCode { get; set; }
    public DateTime DocDate { get; set; }
    public decimal Summa { get; set; }
    public decimal Payment { get; set; }
    public decimal Shipment { get; set; }

}
