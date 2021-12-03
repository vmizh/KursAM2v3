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

        public ObservableCollection<Tile> CurrentDocumentTiles { set; get; }
            = new ObservableCollection<Tile>();

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

        public override bool IsCanSearch => !string.IsNullOrWhiteSpace(SearchText);

        public override void Search(object obj)
        {
            if (Form is MainWindow frm)
            {
                frm.tileDocumentItems.Children.Clear();
                if (!string.IsNullOrWhiteSpace(SearchText) && SearchText.Length >= 3)
                {
                    
                    foreach (var tile in CurrentDocumentTiles)
                    {
                        if (((string)tile.Header).ToUpper().Contains(SearchText.ToUpper()))
                        {
                            frm.tileDocumentItems.Children.Add(tile);
                        }
                    }
                }
                else
                {
                    foreach (var tile in CurrentDocumentTiles)
                    {
                        frm.tileDocumentItems.Children.Add(tile);
                    }
                }
            }
        }

        public override void SearchClear(object obj)
        {
            SearchText = null;
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