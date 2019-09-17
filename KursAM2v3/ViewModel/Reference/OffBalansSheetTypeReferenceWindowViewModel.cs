using System.Collections.ObjectModel;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;

namespace KursAM2.ViewModel.Reference
{
    public class OffBalansSheetTypeReferenceWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        // ReSharper disable once NotAccessedField.Local
        private StandartErrorManager errorManager;

        #endregion

        #region Constructors

        public OffBalansSheetTypeReferenceWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            errorManager = new StandartErrorManager(GlobalOptions.GetEntities(), Name, true);
            RefreshData(null);
        }

        #endregion

        #region Properties

        public ObservableCollection<OffBalanceSheetChargesTypeViewModel> Rows { set; get; }
            = new ObservableCollection<OffBalanceSheetChargesTypeViewModel>();

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
        }

        #endregion
    }
}