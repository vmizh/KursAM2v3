using System.Collections.Generic;
using KursAM2.ViewModel.Management;
using KursRepositories.Repositories.ProfitAndLoss;

namespace KursAM2.Repositories.ProfitAndLossRepository;

public class ProfitLossRepositoryResult
{
    public List<ProfitAndLossesMainRowViewModel> Main { set; get; }
    public List<ProfitAndLossesMainRowViewModel> MainNach { set; get; }
    public List<ProfitAndLossesExtendRowViewModel> Extend { set; get; }
    public List<ProfitAndLossesExtendRowViewModel> ExtendNach { set; get; }
}
