using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace KursRepositories.Repositories.Projects
{
    public interface IProjectProfitAndLossRepository
    {
        Guid ProjectId { set; get; }
        DateTime DateStart { set; get; }
        DateTime DateEnd { set; get; }
        Task<ProfitAndLossResult> CalcTovar();
        Task<ProfitAndLossResult> CalcStartKontragentBalans();
        Task<ProfitAndLossResult> CalcOutCach();
        Task<ProfitAndLossResult> CalcStartCash();
        Task<ProfitAndLossResult> CalcStartBank();
        Task<ProfitAndLossResult> CalcNomInRounding();
        Task<ProfitAndLossResult> CalcDilers();
        Task<ProfitAndLossResult> CalcUslugiDilers();
        Task<ProfitAndLossResult> CalcVozvrat();
        Task<ProfitAndLossResult> CalcNomenklReturn();
        Task<ProfitAndLossResult> CalcSpisanie();
        Task<ProfitAndLossResult> CalcTovarTransfer();
        Task<ProfitAndLossResult> CalcUslugi();
        Task<ProfitAndLossResult> CalcFinance();
        Task<ProfitAndLossResult> CalcOutBalans();
        Task<ProfitAndLossResult> SpisanieTovar();
        Task<ProfitAndLossResult> CalcCurrencyChange();
        Task<ProfitAndLossResult> CalcZarplata();
        Task<ProfitAndLossResult> CalcZarplataNach();
        Task<ProfitAndLossResult> CalcNomenklCurrencyChanged();
        Task<ProfitAndLossResult> CalcAccruedAmmount();
        Task<ProfitAndLossResult> CalcStockHolders();
        Task<ProfitAndLossResult> CalcTransferOutBalans();

    }
}
