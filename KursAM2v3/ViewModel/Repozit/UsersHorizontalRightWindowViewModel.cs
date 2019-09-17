using Core.Menu;
using Core.ViewModel.Base;

namespace KursAM2.ViewModel.Repozit
{
    public class UsersHorizontalRightWindowViewModel : RSWindowViewModelBase
    {
        public UsersHorizontalRightWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocRightBar(this);
        }
    }
}