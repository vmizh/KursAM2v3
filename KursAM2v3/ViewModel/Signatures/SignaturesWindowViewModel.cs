using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using KursAM2.Auxiliary;
using KursAM2.View.Base;
using KursAM2.View.Signature;
using KursAM2.ViewModel.Personal;
using KursAM2.ViewModel.Reference.Dialogs;
using KursDomain.Documents.Signatures;
using KursDomain.Documents.Systems;
using KursDomain.Menu;

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
            foreach (var ds in DocumentSchemes)
            {
                if (ds.SchemesInfo.Any(_ => _.SignatureType == null)) return false;
            }
            return true;
        }

        private void LoadReference()
        {
            var ds = SystemUnitOfWork.Context.DataSources.Include(_ => _.Users)
                .Where(_ => _.Users.Any(u => u.Id == GlobalOptions.UserInfo.KursId))
                .ToList();
            foreach (var d in ds) DataSourceList.Add(new DataSourcesViewModel(d));
        }

        private void LoadSignatureTypes()
        {
            Signatures.Clear();
            DocumentSchemes.Clear();

            foreach (var s in SystemUnitOfWork.Context.SignatureType
                .Where(_ => _.DbId == CurrentDataSource.Id).ToList())
                Signatures.Add(new SignatureTypeViewModel(s));
            foreach (var ds in SystemUnitOfWork.Context.SignatureSchemes.Where(
                _ => _.DbId == CurrentDataSource.Id).ToList())
            {
                DocumentSchemes.Add(new SignatureSchemesViewModel(ds));
            }
        }

        #endregion

        #region Fields

        private DataSourcesViewModel myCurrentDataSource;
        private SignatureTypeViewModel myCurrentSignature;
        private UsersViewModel myCurrentUser;
        private KursMenuItemViewModel myCurrentDocType;
        private SignatureSchemesViewModel myCurrentSchema;
        private SignatureSchemesInfoViewModel myCurrentInfoSchema;

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

        public ObservableCollection<SignatureSchemesViewModel> DocumentSchemes { set; get; } =
            new ObservableCollection<SignatureSchemesViewModel>();


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

        public UsersViewModel CurrentUser
        {
            get => myCurrentUser;
            set
            {
                if (myCurrentUser == value) return;
                myCurrentUser = value;
                RaisePropertyChanged();
            }
        }

        public KursMenuItemViewModel CurrentDocType
        {
            get => myCurrentDocType;
            set
            {
                if (myCurrentDocType == value) return;
                myCurrentDocType = value;
                RaisePropertyChanged();
            }
        }

        public SignatureSchemesInfoViewModel CurrentInfoSchema
        {
            get => myCurrentInfoSchema;
            set
            {
                if (myCurrentInfoSchema == value) return;
                myCurrentInfoSchema = value;
                RaisePropertyChanged();
            }
        }

        public SignatureSchemesViewModel CurrentSchema
        {
            get => myCurrentSchema;
            set
            {
                if (myCurrentSchema == value) return;
                myCurrentSchema = value;
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

        public ICommand AddSchemaSignatureCommand
        {
            get { return new Command(AddSchemaSignature, _ => CurrentSchema != null); }
        }

        private void AddSchemaSignature(object obj)
        {
            var newSch = new SignatureSchemesInfo
            {
                Id = Guid.NewGuid(),
                ParentId = null,
                IsRequired = true,
                SchemeId = CurrentSchema.Id
            };
            CurrentSchema.Entity.SignatureSchemesInfo.Add(newSch);
            var newItem = new SignatureSchemesInfoViewModel(newSch);
            CurrentSchema.SchemesInfo.Add(newItem);
        }

        public ICommand AddSchemaSignature2Command
        {
            get { return new Command(AddSchemaSignature2, _ => CurrentSchema != null 
                                                               && CurrentInfoSchema != null); }
        }

        private void AddSchemaSignature2(object obj)
        {
            var newSch = new SignatureSchemesInfo
            {
                Id = Guid.NewGuid(),
                ParentId = CurrentInfoSchema.Id,
                IsRequired = true,
                SchemeId = CurrentSchema.Id
            };
            SystemUnitOfWork.Context.SignatureSchemesInfo.Add(newSch);
            var newItem = new SignatureSchemesInfoViewModel(newSch);
            CurrentSchema.SchemesInfo.Add(newItem);
        }

        public ICommand DeleteSchemaSignatureCommand
        {
            get
            {
                return new Command(DeleteSchemaSignature, _ => CurrentInfoSchema != null
                                                               && CurrentSchema.SchemesInfo.All(x =>
                                                                   x.ParentId != CurrentInfoSchema.Id));
            }
        }

        private void DeleteSchemaSignature(object obj)
        {
            SystemUnitOfWork.Context.SignatureSchemesInfo.Remove(CurrentInfoSchema.Entity);
            CurrentSchema.SchemesInfo.Remove(CurrentInfoSchema);
        }

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
            var service = this.GetService<IDialogService>("WinUIDialogService");
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
                if (CurrentSignature.Users.Any(_ => _.Id == usr.Id)) return;
                CurrentSignature.Entity.Users.Add(usr);
                CurrentSignature.Users.Add(new UsersViewModel(usr));
            }
        }

        public ICommand DeleteUserCommand
        {
            get { return new Command(DeleteUser, _ => CurrentUser != null); }
        }

        private void DeleteUser(object obj)
        {
            var service = this.GetService<IDialogService>("WinUIDialogService");
            dialogServiceText = "Вы уверены, что хотите удалить пользователя?";
            var res = service.ShowDialog(MessageButton.YesNoCancel, "Запрос", this);
            if (res != MessageResult.Yes) return;
            CurrentSignature.Entity.Users.Remove(CurrentUser.Entity);
            CurrentSignature.Users.Remove(CurrentUser);
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
                SystemUnitOfWork.Rollback();
                UnitOfWork.Rollback();
                var service = this.GetService<IDialogService>("WinUIDialogService");
                MessageManager.ErrorShow(service, ex);
            }
        }

        public ICommand DeleteDocumentCommand
        {
            get { return new Command(DeleteDocument, _ => CurrentSchema != null); }
        }

        private void DeleteDocument(object obj)
        {
            if (SystemUnitOfWork.Context.Entry(CurrentSchema.Entity).State == EntityState.Added)
            {
                SystemUnitOfWork.Context.Entry(CurrentSchema.Entity).State = EntityState.Detached;
            }
            else
            {
                foreach (var si in CurrentSchema.SchemesInfo)
                {
                    SystemUnitOfWork.Context.SignatureSchemesInfo.Remove(si.Entity);
                }
                
                SystemUnitOfWork.Context.SignatureSchemes.Remove(CurrentSchema.Entity);
            }
            DocumentSchemes.Remove(CurrentSchema);
        }

        public ICommand AddDocumentCommand
        {
            get { return new Command(AddDocument, _ => CurrentDataSource != null); }
        }

        private void AddDocument(object obj)
        {
            var ctx = new SelectKursMainMenuItemViewModel(SystemUnitOfWork.Context);
            var service = this.GetService<IDialogService>("DialogServiceUI");
            if (service.ShowDialog(MessageButton.OKCancel, "Запрос", ctx) == MessageResult.Cancel) return;
            if (ctx.CurrentMenu == null) return;
            var newEnt = new SignatureSchemes
            {
                Id = Guid.NewGuid(),
                Name = ctx.CurrentMenu.Name,
                DbId = CurrentDataSource.Id,
                DocumentTYpeId = ctx.CurrentMenu.Id,
                KursMenuItem = ctx.CurrentMenu.Entity
            };
            var schema = new SignatureSchemesViewModel(newEnt)
            {
                DataSource = CurrentDataSource,
                DocumentType = ctx.CurrentMenu

            };
            SystemUnitOfWork.Context.SignatureSchemes.Add(schema.Entity);
            DocumentSchemes.Add(schema);
            if (DocumentSchemes.Count == 1)
            {
                CurrentSchema = schema;
            }
        }

        #endregion
    }
}
