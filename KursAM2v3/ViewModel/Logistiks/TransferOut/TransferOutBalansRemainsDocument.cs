using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using KursDomain.IDocuments.TransferOut;
using KursDomain.References;
using KursDomain.Wrapper;

namespace KursAM2.ViewModel.Logistiks.TransferOut
{
    public class TransferOutBalansRemainsDocument : ITransferOutBalansRows
    {
        [Display(AutoGenerateField = true, Name = "№")]
        public int DocNum => TransferOutBalans.DocNum;

        [Display(AutoGenerateField = true, Name = "Дата")]
        public DateTime DocDate => TransferOutBalans.DocDate;

        [Display(AutoGenerateField = true, Name = "Место хранения")]
        public StorageLocationsWrapper StorageLocations { get; set; }

        [Display(AutoGenerateField = true, Name = "Ном.№")]
        public string NomenklNumber => Nomenkl?.NomenklNumber;

        [Display(AutoGenerateField = false, Name = "Id")]
        public Guid Id { get; set; }

        [Display(AutoGenerateField = false, Name = "Id")]
        public Guid DocId { get; set; }

        [Display(AutoGenerateField = true, Name = "Номенклатура")]
        [ReadOnly(true)]
        public Nomenkl Nomenkl { get; set; }

        [Display(AutoGenerateField = true, Name = "Кол-во")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Quatntity { get; set; }

        [Display(AutoGenerateField = true, Name = "Цена")]
        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Price { get; set; }

        [Display(AutoGenerateField = true, Name = "Себес-ть(ед)")]
        public decimal CostPrice  { get; set; }
 
        [Display(AutoGenerateField = true, Name = "Сумма")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal Summa => Price * Quatntity;

        [Display(AutoGenerateField = true, Name = "Себ-ть (сумма)")]
        [DisplayFormat(DataFormatString = "n2")]
        public decimal CostSumma => Quatntity*CostPrice;

        [Display(AutoGenerateField = true, Name = "Примечание")]
        [ReadOnly(true)]
        public string Note { get; set; }

        [Display(AutoGenerateField = false, Name = "Id")]
        public ITransferOutBalans TransferOutBalans { get; set; }
    }
}
