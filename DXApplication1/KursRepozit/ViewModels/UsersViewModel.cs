using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core.Logger;
using Core.Repository.Base;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using KursAM2.View.Base;
using KursRepozit.Auxiliary;
using KursRepozit.Repositories;
using KursRepozit.Views;

namespace KursRepozit.ViewModels
{
    public class UsersViewModel : KursBaseControlViewModel, IDocumentOperation
    {
        #region Constructors

        public UsersViewModel()
        {
            kursSystemRepository = new GenericKursSystemDBRepository<Users>(unitOfWork);
            //If you want to use Specific KursSystemRepository with Unit of work
            userRepository = new UserKursSystemRepository(unitOfWork);
            ModelView = new UsersView();
            WindowName = "Управление пользователями";
            LeftMenuBar = WindowMenuGenerator.BaseLeftBar(this);
            RightMenuBar = WindowMenuGenerator.ReferenceRightBar(this);
            Load(null);
        }

        #endregion

        #region Methods

        private void UpdateDataSources()
        {
            if (CurrentUser == null) return;
            foreach (var d in DataSources)
            {
                d.IsSelected = CurrentUser.DBSources.Any(_ => _.Id == d.Id);
                d.State = RowStatus.NotEdited;
            }
        }

        #endregion

        #region Fields

        private readonly UnitOfWork<KursSystemEntities> unitOfWork = new UnitOfWork<KursSystemEntities>();

        // ReSharper disable once NotAccessedField.Local
        private readonly GenericKursSystemDBRepository<Users> kursSystemRepository;

        // ReSharper disable once NotAccessedField.Local
        private readonly IUsersRepository userRepository;

        #endregion

        #region Properties

        public ObservableCollection<UserViewModel> Users { set; get; } =
            new ObservableCollection<UserViewModel>();

        public ObservableCollection<DataSourceSelectedViewModel> DataSources { set; get; } =
            new ObservableCollection<DataSourceSelectedViewModel>();

        public UserViewModel CurrentUser
        {
            get => GetValue<UserViewModel>();
            set => SetValue(value, () =>
            {
                UpdateDataSources();
                RaisePropertiesChanged(nameof(IsDataSourcesEnabled));
            });
        }

        public DataSourceSelectedViewModel CurrentDataSource
        {
            get => GetValue<DataSourceSelectedViewModel>();
            set => SetValue(value);
        }

        public bool IsDataSourcesEnabled => CurrentUser != null;

        #endregion

        #region Commands

        [Command]
        public sealed override void Load(object o)
        {
            Users.Clear();
            foreach (var u in userRepository.GetAllWithDataSources().OrderBy(_ => _.Name))
                Users.Add(new UserViewModel(u));
            DataSources.Clear();
            foreach (var d in userRepository.GetAllDataSources())
                DataSources.Add(new DataSourceSelectedViewModel(d));
        }

        public override bool CanLoad(object o)
        {
            return true;
        }

        [Command]
        public override void Save()
        {
            try
            {
                unitOfWork.CreateTransaction();
                foreach (var d in Users.Where(_ => _.State != RowStatus.NotEdited))
                    switch (d.State)
                    {
                        case RowStatus.NewRow:
                            userRepository.Insert(d.Entity);
                            break;
                        case RowStatus.Deleted:
                            userRepository.Delete(d.Entity);
                            break;
                        case RowStatus.Edited:
                            userRepository.Update(d.Entity);
                            break;
                    }

                unitOfWork.Save();
                unitOfWork.Commit();
                Users.ForEach(_ => _.State = RowStatus.NotEdited);
                Load(null);
            }

            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                LoggerHelper.WriteError(ex);
                unitOfWork.Rollback();
            }
        }

        public override bool IsCanRefresh { set; get; } = true;

        public override bool CanSave()
        {
            return Users.Any(_ => _.State != RowStatus.NotEdited);
        }

        [Command]
        public void AddUser()
        {
        }

        public bool CanAddUser()
        {
            return true;
        }

        [Command]
        public void DataSourceSelectChange()
        {
            if (CurrentUser == null) return;
            var d = CurrentUser.DBSources.SingleOrDefault(_ => _.Id == CurrentDataSource.Id);
            if (CurrentDataSource.IsSelected)
            {
                if (d == null)
                {
                    CurrentUser.Entity.DataSources.Add(CurrentDataSource.Entity);
                    var newItem = new DataSourceViewModel(CurrentDataSource.Entity);
                    newItem.SetChangeStatus(RowStatus.NewRow);
                    CurrentUser.DBSources.Add(newItem);
                }
                else
                {
                    switch (d.State)
                    {
                        case RowStatus.Deleted:
                            d.State = RowStatus.NotEdited;
                            break;
                        case RowStatus.NewRow:
                            CurrentUser.DBSources.Remove(d);
                            CurrentUser.Entity.DataSources.Add(d.Entity);
                            break;
                    }
                }
            }
            else
            {
                if (d == null)
                    return;

                CurrentUser.DBSources.Remove(d);
                CurrentUser.Entity.DataSources.Remove(d.Entity);
            }

            UpdateDataSources();
            if (CurrentUser.State == RowStatus.NotEdited)
                CurrentUser.State = RowStatus.Edited;
        }

        public bool CanDataSourceSelectChange()
        {
            return true;
        }

        [Command]
        public void ChangeUser()
        {
            var dsForm = new KursBaseDialog
            {
                Owner = Application.Current.MainWindow
            };
            var dsDataContext = new UserFormViewModel(CurrentUser)
            {
                Form = dsForm,
                KursSystemRepository = kursSystemRepository,
                UserRepository = userRepository,
                UnitOfWork = unitOfWork
            };
            dsDataContext.Form = dsForm;
            dsForm.DataContext = dsDataContext;
            dsForm.ShowDialog();
        }

        public bool CanChangeUser()
        {
            return CurrentUser != null;
        }

        #endregion
    }
}