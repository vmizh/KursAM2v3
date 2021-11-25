using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.EntityViewModel.Systems;
using Core.ViewModel.Base;
using DevExpress.Xpf.LayoutControl;
using KursAM2.View;
using KursAM2.ViewModel.StartLogin;

namespace KursAM2.ViewModel
{
    public class MainWindowViewModel : RSWindowViewModelBase
    {
        private Tile myCurrentFavoriteMenu;
        private LastDocumentViewModel myCurrentLastDocument;


        private string mySearchMenuString;

        // ReSharper disable once EmptyConstructor
        public MainWindowViewModel()
        {
        }

        public ObservableCollection<LastDocumentViewModel> LastDocuments { set; get; }
            = new ObservableCollection<LastDocumentViewModel>();

        public LastDocumentViewModel CurrentLastDocument
        {
            get => myCurrentLastDocument;
            set
            {
                if (myCurrentLastDocument == value) return;
                myCurrentLastDocument = value;
                RaisePropertyChanged();
            }
        }

        public string SearchMenuString
        {
            get => mySearchMenuString;
            set
            {
                if (mySearchMenuString == value) return;
                mySearchMenuString = value;
                RaisePropertyChanged();
            }
        }

        public ICommand SearchMenuCommand
        {
            get { return new Command(SearchMenu, _ => !string.IsNullOrWhiteSpace(SearchMenuString)); }
        }

        public Command ClearSearchCommand
        {
            get { return new Command(ClearSearchMenu, _ => !string.IsNullOrWhiteSpace(SearchMenuString)); }
        }

        public Tile CurrentFavoriteMenu
        {
            get => myCurrentFavoriteMenu;
            set
            {
                if (myCurrentFavoriteMenu == value) return;
                myCurrentFavoriteMenu = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<UserMenuFavoriteViewModel> FavoritesMenuItems { set; get; }
            = new ObservableCollection<UserMenuFavoriteViewModel>();

        public ICommand OpenLastDocumentDialogCommand
        {
            get { return new Command(OpenLastDocumentDialog, _ => true); }
        }

        public ICommand ExpandDocumentListCommand
        {
            get { return new Command(ExpandDocumentList, _ => true); }
        }

        private void SearchMenu(object obj)
        {
        }

        private void ClearSearchMenu(object obj)
        {
            SearchMenuString = null;
        }

        private void OpenLastDocumentDialog(object obj)
        {
            var dlg = new LastUsersDocumentView();
            var ctx = new LastDocumentWindowViewModel
            {
                Form = dlg
            };
            dlg.DataContext = ctx;
            dlg.Show();
        }

        private void ExpandDocumentList(object obj)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                LastDocuments.Clear();
                foreach (var h in ctx.LastDocument.Where(_ => _.UserId == GlobalOptions.UserInfo.KursId
                                                              && _.DbId == GlobalOptions.DataBaseId)
                    .OrderByDescending(_ => _.LastOpen))
                    LastDocuments.Add(new LastDocumentViewModel(h));
            }
        }
    }
}