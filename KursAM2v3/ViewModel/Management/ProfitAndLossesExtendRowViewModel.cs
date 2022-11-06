using System;
using System.Collections.ObjectModel;
using Core.ViewModel.Base;
using FinanceAnalitic;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Management;
using KursDomain.References;

namespace KursAM2.ViewModel.Management
{
    public class ProfitAndLossesExtendRowViewModel : RSViewModelBase, IProfitAndLossesBase
    {
        public DocumentType DocTypeCode { set; get; }
        public string KontragentName { get; set; }
        public decimal? AktZachetResult { set; get; }

        public SDRSchet SDRSchet { set; get; }
        public SDRState SDRState { set; get; }
        public string AktZachet { set; get; }

        public ObservableCollection<VzaimozachetRow> VzaimozachetInfo { set; get; } =
            new ObservableCollection<VzaimozachetRow>();

        public decimal? DocumentDC { set; get; }
        public decimal ProfitGBP { get; set; }
        public decimal LossGBP { get; set; }
        public decimal ResultGBP { get; set; }
        public decimal ProfitCHF { get; set; }
        public decimal LossCHF { get; set; }
        public decimal ResultCHF { get; set; }
        public decimal ProfitSEK { get; set; }
        public decimal LossSEK { get; set; }
        public decimal ResultSEK { get; set; }

        public TypeProfitAndLossCalc CalcType { get; set; }
        public Guid GroupId { set; get; }
        public decimal Quantity { set; get; }
        public decimal Price { set; get; }
        public string CurrencyName { set; get; }
        public Currency Currency { get; set; }
        public decimal ProfitRUB { set; get; }
        public decimal LossRUB { set; get; }
        public decimal ResultRUB { set; get; }
        public decimal ProfitUSD { set; get; }
        public decimal LossUSD { set; get; }
        public decimal ResultUSD { set; get; }
        public decimal ProfitEUR { set; get; }
        public decimal LossEUR { set; get; }
        public decimal ResultEUR { set; get; }
        public decimal ProfitOther { get; set; }
        public decimal LossOther { get; set; }
        public decimal ResultOther { get; set; }
        public decimal ProfitCNY { get; set; }
        public decimal LossCNY { get; set; }
        public decimal ResultCNY { get; set; }
        public Nomenkl Nomenkl { set; get; }
        public Kontragent KontragentBase { get; set; }
        public string Kontragent { set; get; }
        public DateTime Docdate { set; get; }
        public DateTime Date { set; get; }
        public DateTime RowDate { set; get; }
        public string DocNum { set; get; }

        public static ProfitAndLossesExtendRowViewModel GetCopy(ProfitAndLossesExtendRowViewModel d)
        {
            var ret = new ProfitAndLossesExtendRowViewModel
            {
                DocTypeCode = d.DocTypeCode,
                KontragentName = d.KontragentName,
                AktZachetResult = d.AktZachetResult,
                ProfitGBP = d.ProfitGBP,
                LossGBP = d.LossGBP,
                ResultGBP = d.ResultGBP,
                ProfitCHF = d.ProfitCHF,
                LossCHF = d.LossCHF,
                ResultCHF = d.ResultCHF,
                ProfitSEK = d.ProfitSEK,
                LossSEK = d.LossSEK,
                ResultSEK = d.ResultSEK,
                SDRSchet = d.SDRSchet,
                SDRState = d.SDRState,
                AktZachet = d.AktZachet,
                CalcType = d.CalcType,
                GroupId = d.GroupId,
                Quantity = d.Quantity,
                Price = d.Price,
                CurrencyName = d.CurrencyName,
                Currency = d.Currency,
                ProfitRUB = d.ProfitRUB,
                LossRUB = d.LossRUB,
                ResultRUB = d.ResultRUB,
                ProfitUSD = d.ProfitUSD,
                LossUSD = d.LossUSD,
                ResultUSD = d.ResultUSD,
                ProfitEUR = d.ProfitEUR,
                LossEUR = d.LossEUR,
                ResultEUR = d.ResultEUR,
                ProfitCNY = d.ProfitCNY,
                LossCNY = d.LossCNY,
                ResultCNY = d.ResultCNY,
                ProfitOther = d.ProfitOther,
                LossOther = d.LossOther,
                ResultOther = d.ResultOther,
                Nomenkl = d.Nomenkl,
                KontragentBase = d.KontragentBase,
                Kontragent = d.Kontragent,
                Docdate = d.Docdate,
                Date = d.Date,
                RowDate = d.RowDate,
                DocNum = d.DocNum,
                Id = d.Id,
                DocCode = d.DocCode
            };
            if (d.VzaimozachetInfo != null && d.VzaimozachetInfo.Count > 0)
                ret.VzaimozachetInfo = new ObservableCollection<VzaimozachetRow>(d.VzaimozachetInfo);
            return ret;
        }
    }
}
