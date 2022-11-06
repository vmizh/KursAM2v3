using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Entity;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Data;
using KursAM2.View;
using KursDomain;

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
                    .OrderByDescending(_ => _.OpenCount).Take(5).ToList();
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

        public override void SearchClear(object obj)
        {
            SearchText = null;
        }

        public override bool IsCanSearch => !string.IsNullOrWhiteSpace(SearchText);

        public override void Search(object obj)
        {
            if (SearchText.Length >= 3)
            {
                SearchMenuItems.Clear();
                foreach (var menu in from grp in GlobalOptions.UserInfo.MainTileGroups
                    from menu in grp.TileItems
                    where menu.Name.ToUpper().Contains(SearchText.ToUpper())
                    where SearchMenuItems.All(_ => _.Id != menu.Id)
                    select menu)
                {
                    SearchMenuItems.Add(new SearchMenuItem
                    {
                        Id = menu.Id,
                        Name = menu.Name,
                        Command = OpenCommand
                    });
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
            using (var ctx = GlobalOptions.KursSystem())
            {
                var menuItem = ctx.LastMenuUserSearch
                    .Include(_ => _.KursMenuItem)
                    .FirstOrDefault(_ => _.UserId == GlobalOptions.UserInfo.KursId
                                         && _.DbId == GlobalOptions.DataBaseId 
                                         && _.KursMenuItem.Name == (string)obj);
                if (menuItem == null)
                {
                    var newMenu = ctx.KursMenuItem.FirstOrDefault(_ => _.Name == (string)obj);
                    if (newMenu != null)
                    {
                        ctx.LastMenuUserSearch.Add(new LastMenuUserSearch
                        {
                            Id = Guid.NewGuid(),
                            DbId = GlobalOptions.DataBaseId,
                            UserId = GlobalOptions.UserInfo.KursId,
                            MenuId = newMenu.Id,
                            LastOpen = DateTime.Now,
                            OpenCount = 1
                        });
                    }
                }
                else
                {
                    menuItem.OpenCount++;
                }

                ctx.SaveChanges();
            }
            MainWindow.OpenWindow((string)obj);
        }

        #endregion
    }
}
