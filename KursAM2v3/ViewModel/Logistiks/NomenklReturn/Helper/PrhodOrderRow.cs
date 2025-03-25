using System;
using System.ComponentModel.DataAnnotations;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks.NomenklReturn.Helper
{
    public class PrhodOrderRow
    {
        /// <summary>
        ///     DocCode приходного ордера (SD_24)
        /// </summary>
        [Display(AutoGenerateField = false)]
        public decimal DocCode { set; get; }


        /// <summary>
        ///     Code приходного ордера (TD_24)
        /// </summary>
        [Display(AutoGenerateField = false)]
        public int Code { set; get; }

        /// <summary>
        ///     Id строки приходного ордера (TD_24)
        /// </summary>
        [Display(AutoGenerateField = false)]
        public Guid Id { set; get; }

        [Display(AutoGenerateField = false)] public decimal NomenklDC { set; get; }

        [Display(AutoGenerateField = true, Name = "Номенклатура",GroupName = "Основные данные")]
        public Nomenkl Nomenkl { set; get; }

        [Display(AutoGenerateField = true, Name = "Ном.№",GroupName = "Основные данные")]
        public string NomenklNumber => Nomenkl?.NomenklNumber;

        [Display(AutoGenerateField = true, Name = "Ед.изм.", GroupName = "Основные данные")]
        public Unit NomUnit => Nomenkl?.Unit as Unit;

        [Display(AutoGenerateField = true, Name = "Кол-во",GroupName = "Основные данные")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Quantity { set; get; }

        [Display(AutoGenerateField = true, Name = "Цена", GroupName = "Основные данные")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Price { set; get; }

        [Display(AutoGenerateField = true, Name = "Сумма", GroupName = "Основные данные")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Summa => Quantity * Price;

        [Display(AutoGenerateField = true, Name = "Дата",GroupName = "Прих.одный ордер")]
        public DateTime PrihOrderDate { set; get; }

        [Display(AutoGenerateField = true, Name = "№",GroupName = "Прих.одный ордер")]
        public string PrihOrderNumber { set; get; }

        [Display(AutoGenerateField = true, Name = "Создатель",GroupName = "Прих.одный ордер")]

        public string PrihOrderCreator { set; get; }

        [Display(AutoGenerateField = true, Name = "Примечание",GroupName = "Прих.одный ордер")]

        public string PrihOrderNote { set; get; }

        [Display(AutoGenerateField = true, Name = "Дата",GroupName = "Счет-фактура")]
        public DateTime? InvoiceDate { set; get; }

        [Display(AutoGenerateField = true, Name = "№",GroupName = "Счет-фактура")]
        public string InvoiceNumber { set; get; }

        [Display(AutoGenerateField = true, Name = "Создатель",GroupName = "Счет-фактура")]
        public string InvoiceCreator { set; get; }

        [Display(AutoGenerateField = true, Name = "Примечание",GroupName = "Счет-фактура")]
        public string InvoiceNote { set; get; }

        [Display(AutoGenerateField = false)] public decimal? InvoiceDC { set; get; }

        [Display(AutoGenerateField = false)] public int? InvoiceCode { set; get; }

        [Display(AutoGenerateField = false)] public Guid? InvoiceRowId { set; get; }
        [Display(AutoGenerateField = false)] public Guid? PrihodOrderRowId { set; get; }
        [Display(AutoGenerateField = false)] public int? PrihodOrderCode { set; get; }
        [Display(AutoGenerateField = false)] public decimal? PrihodOrderDC { set; get; }
    }
}
