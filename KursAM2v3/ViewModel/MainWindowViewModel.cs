using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using DevExpress.Xpf.LayoutControl;
using KursAM2.Managers;
using KursAM2.View;
using KursAM2.ViewModel.StartLogin;
using KursDomain;
using KursDomain.Documents.Systems;

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

        public ObservableCollection<LastDocumentViewModel> LastDocuments { set; get; } = new();

        public ObservableCollection<Tile> CurrentDocumentTiles { set; get; } = new();

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

        public bool IsCanVersionUpdate
        {
            get
            {
                var Vers = new VersionManager();
                var Ver = Vers.GetCanUpdate(1);
                if (Ver == 0) return false;
                return true;
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

        public ICommand VersionUpdateCommand
        {
            get
            {
                return new Command(VersionUpdate, _ =>
                {
                    var Vers = new VersionManager();
                    var Ver = Vers.GetCanUpdate(1);
                    if (Ver == 0) return false;
                    return true;
                });
            }
        }

        public ObservableCollection<UserMenuFavoriteViewModel> FavoritesMenuItems { set; get; } = new();

        public ICommand OpenLastDocumentDialogCommand
        {
            get { return new Command(OpenLastDocumentDialog, _ => true); }
        }

        public ICommand ExpandDocumentListCommand
        {
            get { return new Command(ExpandDocumentList, _ => true); }
        }

        public override bool IsCanSearch => !string.IsNullOrWhiteSpace(SearchText);

        private void VersionUpdate(object obj)
        {
            var Vers = new VersionManager();
            var Ver = Vers.CheckVersion(1);
        }

        public override void Search(object obj)
        {
            if (Form is MainWindow frm)
            {
                frm.tileDocumentItems.Children.Clear();
                if (!string.IsNullOrWhiteSpace(SearchText) && SearchText.Length >= 3)
                {
                    foreach (var tile in CurrentDocumentTiles)
                        if (((string)tile.Header).ToUpper().Contains(SearchText.ToUpper()))
                            frm.tileDocumentItems.Children.Add(tile);
                }
                else
                {
                    foreach (var tile in CurrentDocumentTiles) frm.tileDocumentItems.Children.Add(tile);
                }
            }
        }

        public override void SearchClear(object obj)
        {
            SearchText = null;
            if (Form is MainWindow frm)
            {
                frm.tileDocumentItems.Children.Clear();
                foreach (var tile in CurrentDocumentTiles) frm.tileDocumentItems.Children.Add(tile);
            }
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