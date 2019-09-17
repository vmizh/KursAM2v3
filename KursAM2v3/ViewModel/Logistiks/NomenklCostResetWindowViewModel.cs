using Core.Menu;
using Core.ViewModel.Base;

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