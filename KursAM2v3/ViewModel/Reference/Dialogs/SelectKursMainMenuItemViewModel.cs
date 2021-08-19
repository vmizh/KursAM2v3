using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core;
using Core.Menu;
using Core.ViewModel.Base;
using DevExpress.Mvvm;
using KursAM2.View.KursReferences.UC;
using KursRepositories.ViewModels;

namespace KursAM2.ViewModel.Reference.Dialogs
{
    public sealed class SelectKursMainMenuItemViewModel : RSWindowViewModelBase
    {
        private KursMenuItemViewModel myCurrentMenu;
        private ICurrentWindowService winCurrentService;

        public SelectKursMainMenuItemViewModel()
        {
            RightMenuBar = MenuGenerator.DialogStandartBar(this);
            RefreshData(null);
        }

        public UserControl CustomDataUserControl { set; get; } = new SelectKursMainMenuItem();

        public override string LayoutName => "SelectKursMainMenuItemViewModel";

        public List<KursMenuItemViewModel> MenuItems { set; get; } = new List<KursMenuItemViewModel>();

        public KursMenuItemViewModel CurrentMenu
        {
            get => myCurrentMenu;
            set
            {
                if (myCurrentMenu == value) return;
                myCurrentMenu = value;
                RaisePropertyChanged();
            }
        }

        #region Command

        public override bool IsOkAllow()
        {
            return CurrentMenu != null;
        }

        public override void Ok(object obj)
        {
            winCurrentService = GetService<ICurrentWindowService>();
            if (winCurrentService != null) winCurrentService.Close();
        }

        public override void Cancel(object obj)
        {
            winCurrentService = GetService<ICurrentWindowService>();
            if (winCurrentService != null)
            {
                CurrentMenu = null;
                winCurrentService.Close();
            }
        }

        public override void RefreshData(object obj)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var data = ctx.KursMenuItem.ToList();
                MenuItems.Clear();
                foreach(var d in data)
                    MenuItems.Add(new KursMenuItemViewModel(d));
            }
        }

        #endregion
    }
}