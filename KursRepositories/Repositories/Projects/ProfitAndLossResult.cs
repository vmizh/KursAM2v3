using System.Collections.Generic;
using KursRepositories.Repositories.ProfitAndLoss;

namespace KursRepositories.Repositories.Projects
{
    public class ProfitAndLossResult
    {
        public List<ProfitAndLossesMainRowViewModel> Main { set; get; }
        public List<ProfitAndLossesExtendRowViewModel> Extend { set; get; }
    }
}
