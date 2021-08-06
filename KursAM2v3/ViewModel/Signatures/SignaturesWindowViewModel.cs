using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.Signatures;
using Core.Menu;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using KursAM2.Auxiliary;
using KursAM2.View.Base;
using KursAM2.View.Signature;
using KursAM2.ViewModel.Personal;
using KursRepositories.ViewModels;

namespace KursAM2.ViewModel.Signatures
{
    public class SignaturesWindowViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public SignaturesWindowViewModel()
        {
            GenericRepository = new GenericKursDBRepository<SignatureType>(UnitOfWork);
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            LoadReference();
            DocumentUserControl = new SignatureReferenceUC();
        }

        #endregion

        #region Methods

        private bool IsDataCorrect()
        {
            if (Signatures.Any(_ => string.IsNullOrWhiteSpace(_.Name))) return false;
            if (Signatures.Select(_ => _.Name).Distinct().Count() < Signatures.Count) return false;
            return true;
        }

        private void LoadReference()
        {
            var ds = SystemUnitOfWork.Context.DataSources.Include(_ => _.Users).AsNoTracking()
                .Where(_ => _.Users.Any(u => u.Id == GlobalOptions.UserInfo.KursId))
                .ToList();
            foreach (var d in ds) DataSourceList.Add(new DataSourcesViewModel(d));
        }

        private void LoadKursMenuItems()
        {
            KursMenuItems.Clear();

            var menuitems = SystemUnitOfWork.Context.UserMenuRight.Include(_ => _.KursMenuItem).Where(_ =>
                    _.DBId == CurrentDataSource.Id && _.LoginName == GlobalOptions.UserInfo.NickName)
                .Select(x => x.KursMenuItem).ToList();
            foreach (var m in menuitems) KursMenuItems.Add(new KursMenuItemViewModel(m));
        }

        private void LoadSignatureTypes()
        {
            Signatures.Clear();

            var signs = SystemUnitOfWork.Context.SignatureType
                .Where(_ => _.DbId == CurrentDataSource.Id);
            foreach (var s in signs.ToList()) 
                Signatures.Add(new SignatureTypeViewModel(s));
        }

        #endregion

        #region Fields

        private DataSourcesViewModel myCurrentDataSource;
        private SignatureTypeViewModel myCurrentSignature;

        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new UnitOfWork<ALFAMEDIAEntities>(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        public readonly UnitOfWork<KursSystemEntities> SystemUnitOfWork =
            new UnitOfWork<KursSystemEntities>(new KursSystemEntities(GlobalOptions.SqlSystemConnectionString));

        public readonly GenericKursDBRepository<SignatureType> GenericRepository;

        #endregion

        #region Properties

        public ObservableCollection<DataSourcesViewModel> DataSourceList { set; get; } =
            new ObservableCollection<DataSourcesViewModel>();

        public ObservableCollection<SignatureTypeViewModel> Signatures { set; get; } =
            new ObservableCollection<SignatureTypeViewModel>();

        public ObservableCollection<KursMenuItemViewModel> KursMenuItems { set; get; } =
            new ObservableCollection<KursMenuItemViewModel>();

        public SignatureReferenceUC DocumentUserControl { set; get; }

        public override string LayoutName => "SignaturesWindowViewModel";

        public override string WindowName => "Управление подписями для документов";

        public DataSourcesViewModel CurrentDataSource
        {
            get => myCurrentDataSource;
            set
            {
                if (myCurrentDataSource == value) return;
                myCurrentDataSource = value;
                LoadSignatureTypes();
                RaisePropertyChanged();
            }
        }


        public SignatureTypeViewModel CurrentSignature
        {
            get => myCurrentSignature;
            set
            {
                if (myCurrentSignature == value) return;
                myCurrentSignature = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override bool IsCanSaveData => (SystemUnitOfWork.Context.ChangeTracker.HasChanges() ||
                                               UnitOfWork.Context.ChangeTracker.HasChanges()) && IsDataCorrect();

        public ICommand AddSignatureCommand
        {
            get { return new Command(AddSignature, _ => CurrentDataSource != null); }
        }

        private void AddSignature(object obj)
        {
            var newSign = new SignatureType
            {
                Id = Guid.NewGuid(),
                DbId = CurrentDataSource.Id
            };
            SystemUnitOfWork.Context.SignatureType.Add(newSign);
            var newItem = new SignatureTypeViewModel(newSign);
            Signatures.Add(newItem);
            CurrentSignature = newItem;
            RaisePropertyChanged(nameof(IsCanSaveData));
        }

        public ICommand DeleteSignatureCommand
        {
            get { return new Command(DeleteSignature, _ => CurrentSignature != null); }
        }

        private void DeleteSignature(object obj)
        {
            var service = GetService<IDialogService>("WinUIDialogService");
            dialogServiceText = "Вы уверены, что хотите удалить подпись?";
            var res = service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this);
            if (res != MessageResult.Yes) return;
            SystemUnitOfWork.Context.SignatureType.Remove(CurrentSignature.Entity);
            Signatures.Remove(CurrentSignature);
        }

        public ICommand AddUserCommand
        {
            get { return new Command(AddUser, _ => CurrentSignature != null); }
        }

        private void AddUser(object obj)
        {
            var ctxnew = new PersonaAddUserForRights();
            var dlg = new SelectDialogView {DataContext = ctxnew};
            dlg.ShowDialog();
            if (!ctxnew.DialogResult) return;
            var usr = SystemUnitOfWork.Context.Users.SingleOrDefault(_ => _.Name == ctxnew.CurrentRow.NickName);
            if (usr != null)
            {
                CurrentSignature.Entity.Users.Add(usr);
                CurrentSignature.Users.Add(new UsersViewModel(usr));
            }
        }

        public override void SaveData(object data)
        {
            try
            {
                if (SystemUnitOfWork.Context.ChangeTracker.HasChanges())
                {
                    SystemUnitOfWork.CreateTransaction();
                    SystemUnitOfWork.Save();
                    SystemUnitOfWork.Commit();
                }

                if (UnitOfWork.Context.ChangeTracker.HasChanges())
                {
                    UnitOfWork.CreateTransaction();
                    UnitOfWork.Save();
                    UnitOfWork.Commit();
                }

                RaisePropertyChanged(nameof(IsCanSaveData));
            }
            catch (Exception ex)
            {
                var service = GetService<IDialogService>("WinUIDialogService");
                MessageManager.ErrorShow(service, ex);
            }
        }
        #endregion
    }
}