using System;
using Core;
using KursDomain.Documents.CommonReferences;
using KursDomain.References;

namespace KursAM2.ViewModel.Management.Projects;

public class ProjectDocumentInfo : IMultyWithDilerCurrency
{
    public Guid Id { set; get; }
    public int? BankCode { set; get; }
    public decimal? CashInDC { set; get; }
    public decimal? CashOutDC { set; get; }
    public decimal? WarehouseOrderInDC { set; get; }
    public decimal? WaybillDC { set; get; }
    public Guid? AccruedClientRowId { set; get; }
    public Guid? AccruedSupplierRowId { set; get; }
    public DocumentType DocumentType { set; get; }
    public int InnerNumber { set; get; }
    public string ExtNumber { set; get; }
    public DateTime DocDate { set; get; }

    public Kontragent Kontragent { set; get; }
    public Kontragent Diler { set; get; }
    public Warehouse Warehouse { set; get; }
    public string BankAccount { set; get; }
    public CashBox CashBox { set; get; }
    public Currency Currency { set; get; }
    public decimal SummaIn { set; get; }
    public decimal SummaOut { set; get; }

    public string Note { set; get; }
    public string ProjectNote { set; get; }

    #region Currencies

    public decimal ProfitCHF { get; set; }
    public decimal ProfitEUR { get; set; }
    public decimal ProfitGBP { get; set; }
    public decimal ProfitRUB { get; set; }
    public decimal ProfitSEK { get; set; }
    public decimal ProfitUSD { get; set; }
    public decimal ProfitCNY { get; set; }
    public decimal LossCHF { get; set; }
    public decimal LossEUR { get; set; }
    public decimal LossGBP { get; set; }
    public decimal LossRUB { get; set; }
    public decimal LossSEK { get; set; }
    public decimal LossUSD { get; set; }
    public decimal LossCNY { get; set; }
    public decimal ResultCHF => ProfitCHF - DilerCHF - LossCHF;
    public decimal ResultEUR => ProfitEUR - DilerEUR - LossEUR;
    public decimal ResultGBP => ProfitGBP - DilerGBP - LossGBP;
    public decimal ResultRUB => ProfitRUB - DilerRUB - LossRUB;
    public decimal ResultSEK => ProfitSEK - DilerSEK - LossSEK;
    public decimal ResultUSD => ProfitUSD - DilerUSD - LossUSD;
    public decimal ResultCNY => ProfitCNY - DilerCNY - LossCNY;
    public decimal DilerCHF { get; set; }
    public decimal DilerEUR { get; set; }
    public decimal DilerGBP { get; set; }
    public decimal DilerRUB { get; set; }
    public decimal DilerSEK { get; set; }
    public decimal DilerUSD { get; set; }
    public decimal DilerCNY { get; set; }

    #endregion
}
