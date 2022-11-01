using Core.ViewModel.Base;
using KursDomain.Menu;

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