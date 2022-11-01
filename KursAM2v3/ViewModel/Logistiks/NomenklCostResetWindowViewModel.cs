using Core.ViewModel.Base;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklCostResetWindowViewModel : RSWindowViewModelBase
    {
        public NomenklCostResetWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocRightBar(this);
        }
    }
}