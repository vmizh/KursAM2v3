using Core.ViewModel.Base;
using KursDomain.Menu;

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