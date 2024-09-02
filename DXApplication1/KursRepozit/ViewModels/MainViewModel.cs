using System.Collections.ObjectModel;
using System.Windows;
using Core.WindowsManager;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using KursAM2.View.Base;
using KursRepozit.Auxiliary;


namespace KursRepozit.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        #endregion

        #region Constructors

        public MainViewModel()
        {
            GenerateTileList();
        }

        #endregion

        #region Properties

        public ObservableCollection<TileItemViewModel> TileList { set; get; }
            = new ObservableCollection<TileItemViewModel>();

        #endregion

        #region Methods

        private void GenerateTileList()
        {
            TileList.Add(new TileItemViewModel
            {
                Text = "Управление пользователями",
                Id = MainMenuEnum.UserControl
            });
            TileList.Add(new TileItemViewModel
            {
                Text = "Настройка типов документов",
                Id = MainMenuEnum.TypeDocumentControl
            });
            TileList.Add(new TileItemViewModel
            {
                Text = "Права доступа к объектам",
                Id = MainMenuEnum.UserRight
            });
            TileList.Add(new TileItemViewModel
            {
                Text = "Управление источниками баз данных",
                Id = MainMenuEnum.DataSourcesControl
            });
            TileList.Add(new TileItemViewModel
            {
                Text = "Просмотр ошибок и логов",
                Id = MainMenuEnum.ErrorShow
            });
        }

        #endregion

        #region Commands

        [Command]
        public void Click(TileItemViewModel vm)
        {
            switch (vm.Id)
            {
                case MainMenuEnum.DataSourcesControl:
                    var dsForm = new KursBaseWindow
                    {
                        Owner = Application.Current.MainWindow
                    };
                    var dsDataContext = new DataSourcesViewModel();
                    dsForm.DataContext = dsDataContext;
                    dsDataContext.Form = dsForm;
                    dsForm.Show();
                    break;
                case MainMenuEnum.ErrorShow:
                    WindowManager.ShowFunctionNotReleased();
                    break;
                case MainMenuEnum.TypeDocumentControl:
                    WindowManager.ShowFunctionNotReleased();
                    break;
                case MainMenuEnum.UserControl:
                    var userForm = new KursBaseWindow
                    {
                        Owner = Application.Current.MainWindow
                    };
                    var userDataContext = new UsersViewModel
                    {
                        Form = userForm
                    };

                    userForm.DataContext = userDataContext;
                    userForm.Show();
                    break;
                case MainMenuEnum.UserRight:
                    WindowManager.ShowFunctionNotReleased();
                    break;
            }
        }

        [Command]
        public void MouseEnter(TileItemViewModel vm)
        {
        }

        #endregion
    }
}
