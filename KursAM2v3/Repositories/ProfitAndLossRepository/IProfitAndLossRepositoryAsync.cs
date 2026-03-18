using Data;
using KursAM2.ViewModel.Management;
using KursDomain.Documents.Currency;
using KursDomain.References;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursAM2.Repositories.ProfitAndLossRepository;

public interface IProfitAndLossRepositoryAsync
{
    Task<ProfitLossRepositoryResult> CalcTovar(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcStartKontragentBalans(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcOutCach(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcStartCash(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcStartBank(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcNomInRounding(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcDilers(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcUslugiDilers(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcVozvrat(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcNomenklReturn(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcSpisanie(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcTovarTransfer(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcUslugi(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcFinance(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcOutBalans(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> SpisanieTovar(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcCurrencyChange(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcZarplata(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcNomenklCurrencyChanged(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcAccruedAmmount(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcStockHolders(DateTime dateStart, DateTime dateEnd);
    Task<ProfitLossRepositoryResult> CalcTransferOutBalans(DateTime dateStart, DateTime dateEnd);

    Task<Prices> GetNomenklPriceAsync(decimal nomdc, DateTime date, ALFAMEDIAEntities context);


}
