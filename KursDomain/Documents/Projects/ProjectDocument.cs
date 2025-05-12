using System;
using Core;
using Core.ViewModel.Base;
using KursDomain.Documents.CommonReferences;
using KursDomain.References;

namespace KursDomain.Documents.Projects;

public class ProjectDocument : IProjectDocument, IMultyWithDilerCurrency
{
    private void setCurrencyToZero()
    {
        ProfitCHF = 0;
        ProfitEUR = 0;
        ProfitGBP = 0;
        ProfitRUB = 0;
        ProfitSEK = 0;
        ProfitUSD = 0;
        ProfitCNY = 0;
        LossCHF = 0;
        LossEUR = 0;
        LossGBP = 0;
        LossRUB = 0;
        LossSEK = 0;
        LossUSD = 0;
        LossCNY = 0;
    }

    public void SetCurrency(decimal? prihod = null, decimal? rashod = null)
    {
        var sPrihod = prihod ?? SummaPrihod;
        var sRashod = rashod ?? SummaRashod;
        setCurrencyToZero();
        switch (Currency.DocCode)
        {
            case CurrencyCode.CHF:
                if (sPrihod > 0)
                    ProfitCHF = sPrihod;
                if (sRashod > 0)
                    LossCHF = sRashod;
                break;
            case CurrencyCode.USD:
                if (sPrihod > 0)
                    ProfitUSD = sPrihod;
                if (sRashod > 0)
                    LossUSD = sRashod;
                break;
            case CurrencyCode.RUB:
                if (sPrihod > 0)
                    ProfitRUB = sPrihod;
                if (sRashod > 0)
                    LossRUB = sRashod;
                break;
            case CurrencyCode.EUR:
                if (sPrihod > 0)
                    ProfitEUR = sPrihod;
                if (sRashod > 0)
                    LossEUR = sRashod;
                break;
            case CurrencyCode.GBP:
                if (sPrihod > 0)
                    ProfitGBP = sPrihod;
                if (sRashod > 0)
                    LossGBP = sRashod;
                break;
            case CurrencyCode.CNY:
                if (sPrihod > 0)
                    ProfitCNY = sPrihod;
                if (sRashod > 0)
                    LossCNY = sRashod;
                break;
            case CurrencyCode.SEK:
                if (sPrihod > 0)
                    ProfitSEK = sPrihod;
                if (sRashod > 0)
                    LossSEK = SummaRashod;
                break;

        }
    }

    #region Properties
    
    public Guid Id { get; set; }
    public Project Project { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocInfo { get; set; }
    public string Note { get; set; }
    public int? BankCode { get; set; }
    public decimal? CashInDC { get; set; }
    public decimal? CashOutDC { get; set; }
    public decimal? WarehouseOrderInDC { get; set; }
    public decimal? WaybillDC { get; set; }
    public Guid? AccruedClientRowId { get; set; }
    public Guid? AccruedSupplierRowId { get; set; }
    public References.Currency Currency { get; set; }
    public decimal SummaPrihod { get; set; }
    public decimal SummaRashod { get; set; }
    
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
    public decimal ResultCHF => ProfitCHF - LossCHF - DilerCHF;
    public decimal ResultEUR => ProfitEUR - LossEUR - DilerEUR;
    public decimal ResultGBP => ProfitGBP - LossGBP - DilerGBP;
    public decimal ResultRUB => ProfitRUB - LossRUB - DilerRUB;
    public decimal ResultSEK => ProfitSEK - LossSEK - DilerSEK;
    public decimal ResultUSD => ProfitUSD - LossUSD - DilerUSD;
    public decimal ResultCNY => ProfitCNY - LossCNY - DilerCNY;

   

    public decimal DilerCHF { get; set; }
    public decimal DilerEUR { get; set; }
    public decimal DilerGBP { get; set; }
    public decimal DilerRUB { get; set; }
    public decimal DilerSEK { get; set; }
    public decimal DilerUSD { get; set; }
    public decimal DilerCNY { get; set; } 
    
    #endregion
}
