using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Entity;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using KursAM2.View;

namespace KursAM2.ViewModel
{
    public class SearchMenuItem
    {
        public string Name { set; get; }
        public ICommand Command { set; get; }
        public int Id { set; get; }
    }

    public class MenuSearchWindowViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public MenuSearchWindowViewModel()
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                var items = ctx.LastMenuUserSearch
                    .Include(_ => _.KursMenuItem)
                    .Where(_ => _.DbId == GlobalOptions.DataBaseId
                    && _.UserId == GlobalOptions.UserInfo.KursId)
                    .OrderByDescending(_ => _.OpenCount).Take(5);
                foreach (var item in items)
                {
                    OftenUsedMenuItems.Add(new SearchMenuItem
                    {
                        Id = item.MenuId,
                        Name = item.KursMenuItem.Name,
                        Command = OpenCommand
                    });
                }
            }
        }

        #endregion

        #region Fields

        #endregion

        #region Properties

        public override string LayoutName => "MenuSearchWindow";

        public ObservableCollection<SearchMenuItem> OftenUsedMenuItems { set; get; } =
            new ObservableCollection<SearchMenuItem>();
        public ObservableCollection<SearchMenuItem> SearchMenuItems { set; get; } =
            new ObservableCollection<SearchMenuItem>();

        #endregion

        #region Methods

        #endregion


        #region Commands

        public ICommand ClearSearchMenuCommand
        {
            get { return new Command(ClearSearchMenu, _ => !string.IsNullOrWhiteSpace(SearchText)); }
        }

        private void ClearSearchMenu(object obj)
        {
            SearchText = null;
        }

        public ICommand SearchMenuCommand
        {
            get
            {
                return new Command(SearchMenu, _ => !string.IsNullOrWhiteSpace(SearchText));
            }
        }

        private void SearchMenu(object obj)
        {
            if (SearchText.Length >= 3)
            {
                SearchMenuItems.Clear();
                foreach (var grp in GlobalOptions.UserInfo.MainTileGroups)
                {
                    foreach (var menu in grp.TileItems)
                    {
                        if (menu.Name.ToUpper().Contains(SearchText.ToUpper()))
                        {
                            if (SearchMenuItems.Any(_ => _.Id == menu.Id)) continue;
                            SearchMenuItems.Add(new SearchMenuItem
                            {
                                Id = menu.Id,
                                Name = menu.Name,
                                Command = OpenCommand
                            });
                        }
                    }
                }
            }
            else
            {
                SearchMenuItems.Clear();
            }
        }

        public ICommand OpenCommand
        {
            get { return new Command(Open, _ => true); }
        }

        private void Open(object obj)
        {
            MainWindow.OpenWindow((string)obj);
        }

        #endregion
    }
}