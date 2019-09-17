using Core.Menu;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomenklInnerMoveWindowViewModel : RSWindowViewModelBase
    {
        public NomenklInnerMoveWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocRightBar(this);
        }
    }
}