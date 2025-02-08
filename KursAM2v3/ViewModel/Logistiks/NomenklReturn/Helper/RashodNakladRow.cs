using System;
using System.ComponentModel.DataAnnotations;
using KursDomain.References;

namespace KursAM2.ViewModel.Logistiks.NomenklReturn.Helper
{
    public class RashodNakladRow
    {
        /// <summary>
        ///     DocCode расходной накладной (SD_24)
        /// </summary>
        [Display(AutoGenerateField = false)]
        public decimal DocCode { set; get; }


        /// <summary>
        ///     Code расходной накладной (TD_24)
        /// </summary>
        [Display(AutoGenerateField = false)]
        public int Code { set; get; }

        /// <summary>
        ///     Id расходной накладной (TD_24)
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

        [Display(AutoGenerateField = true, Name = "Дата",GroupName = "Расх.накладная")]
        public DateTime WayBillDate { set; get; }

        [Display(AutoGenerateField = true, Name = "№",GroupName = "Расх.накладная")]
        public string WayBillNumber { set; get; }

        [Display(AutoGenerateField = true, Name = "Создатель",GroupName = "Расх.накладная")]

        public string WayBillCreator { set; get; }

        [Display(AutoGenerateField = true, Name = "Примечание",GroupName = "Расх.накладная")]

        public string WaybillNote { set; get; }

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
    }
}
