using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Core.Helper;
using Core.Logger;
using Core.Repository.Base;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Utils.CommonDialogs.Internal;
using DevExpress.Xpf.Editors;
using KursRepozit.Repositories;
using KursRepozit.Views;

namespace KursRepozit.ViewModels
{
    public class UserViewModel : KursBaseViewModel, IViewModelToEntity<Users>
    {
        #region Constructors

        public UserViewModel(Users entity, RowStatus state = RowStatus.NotEdited)
        {
            Entity = entity ?? new Users
            {
                Id = Guid.NewGuid()
            };

            // ReSharper disable once VirtualMemberCallInConstructor
            Id = state == RowStatus.NewRow ? Guid.NewGuid() : Entity.Id;
            if (Entity.Avatar != null)
                AvatarSource =
                    (BitmapImage) imageConverter.Convert(Entity.Avatar, typeof(byte[]), null,
                        new CultureInfo("ru-RU"));
            State = state;
            // ReSharper disable once InvertIf
            if (Entity.DataSources.Count > 0)
                foreach (var d in Entity.DataSources)
                    DBSources.Add(new DataSourceViewModel(d));
        }

        #endregion

        #region Fields

        #endregion

        #region Properties

        [DisplayName("DBSources")]
        [Display(AutoGenerateField = false)]
        public ObservableCollection<DataSourceViewModel> DBSources { set; get; }
            = new ObservableCollection<DataSourceViewModel>();


        [DisplayName("Entity")]
        [Display(AutoGenerateField = false)]
        public Users Entity { get; set; }

        [DisplayName("Имя")]
        [Display(AutoGenerateField = true)]
        public string Name
        {
            get => Entity.Name;
            set
            {
                if (Entity.Name == value) return;
                Entity.Name = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Примечания")]
        [Display(AutoGenerateField = true)]
        public string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Администратор")]
        [Display(AutoGenerateField = true)]
        public bool IsAdmin
        {
            get => Entity.IsAdmin;
            set
            {
                if (Entity.IsAdmin == value) return;
                Entity.IsAdmin = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Тестер")]
        [Display(AutoGenerateField = true)]
        public bool IsTester
        {
            get => Entity.IsTester;
            set
            {
                if (Entity.IsTester == value) return;
                Entity.IsTester = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Удален")]
        [Display(AutoGenerateField = true)]
        public bool IsDeleted
        {
            get => Entity.IsDeleted;
            set
            {
                if (Entity.IsDeleted == value) return;
                Entity.IsDeleted = value;
                SetChangeStatus();
            }
        }

        [DisplayName("Аватарка")]
        [Display(AutoGenerateField = false)]
        public byte[] Avatar
        {
            get => Entity.Avatar;
            set => SetValue(value, () => { SetChangeStatus(); });
        }

        private readonly BytesToImageSourceConverter imageConverter = new BytesToImageSourceConverter();

        [DisplayName("Аватарка")]
        [Display(AutoGenerateField = true)]
        public BitmapImage AvatarSource
        {
            get => GetValue<BitmapImage>();
            set => SetValue(value, () =>
            {
                SetChangeStatus();
                if (AvatarSource != null)
                {
                    var encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(AvatarSource));
                    using (var ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        Entity.Avatar = ms.ToArray();
                    }
                }
                else
                {
                    Entity.Avatar = null;
                }
            });
        }

        [DisplayName("Полное имя")]
        [Display(AutoGenerateField = true)]
        public string FullName
        {
            get => Entity.FullName;
            set
            {
                if (Entity.FullName == value) return;
                Entity.FullName = value;
                SetChangeStatus();
            }
        }

        #endregion

        #region Methods

        #endregion

        #region Commands

        #endregion
    }


    [MetadataType(typeof(DataAnnotationsUserFormViewModel))]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class UserFormViewModel : UserViewModel, IKursDialog
    {
        // ReSharper disable once NotAccessedField.Local
        private UserViewModel source;

        public UserFormViewModel(Users entity,
            RowStatus state = RowStatus.NotEdited) : base(entity, state)
        {
            ModelView = new UserFormView();
        }

        public UserFormViewModel(UserViewModel source) : base(source.Entity, source.State)
        {
            this.source = source;
            ModelView = new UserFormView();
        }

        [Display(AutoGenerateField = false)] public UserFormViewModel Current => this;

        [Display(AutoGenerateField = false)] public UnitOfWork<KursSystemEntities> UnitOfWork { set; get; }

        [DisplayName("Репозиторий пользователя")]
        [Display(AutoGenerateField = false)]
        public IUsersRepository UserRepository { set; get; }

        public GenericKursSystemDBRepository<Users> KursSystemRepository { set; get; }

        [DisplayName("Пароль")]
        [Display(AutoGenerateField = true)]
        public string Password
        {
            get => GetValue<string>();
            set => SetValue(value, () => { SetChangeStatus(); });
        }

        [DisplayName("Повтор пароля")]
        [Display(AutoGenerateField = true)]
        public string ReplayPassword
        {
            get => GetValue<string>();
            set => SetValue(value, () => { SetChangeStatus(); });
        }

        [Display(AutoGenerateField = false)] public DialogResult Result { get; set; }

        [Command]
        public void Ok()
        {
            if (State != RowStatus.NotEdited)
            {
                try
                {
                    UnitOfWork.CreateTransaction();
                    KursSystemRepository.Update(Entity);
                    UnitOfWork.Save();
                    UnitOfWork.Commit();
                    Result = DialogResult.OK;
                    Form.Close();
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                    LoggerHelper.WriteError(ex);
                    UnitOfWork.Rollback();
                }
            }
            else
            {
                Result = DialogResult.OK;
                Form.Close();
            }
        }

        [Command]
        public void Cancel()
        {
            Result = DialogResult.Cancel;
            Form.Close();
        }

        public Window Form { get; set; }
        public UserControl ModelView { get; set; }

        public bool CanOk()
        {
            return !string.IsNullOrEmpty(Name);
        }
    }


    public class DataAnnotationsUserFormViewModel : DataAnnotationForFluentApiBase, IMetadataProvider<UserFormViewModel>
    {
        void IMetadataProvider<UserFormViewModel>.BuildMetadata(MetadataBuilder<UserFormViewModel> builder)
        {
            //SetNotAutoGenerated(builder);
            builder.Property(_ => _.KursSystemRepository).NotAutoGenerated();
            builder.Property(_ => _.UserRepository).NotAutoGenerated();
            builder.Property(_ => _.UnitOfWork).NotAutoGenerated();
            builder.Property(_ => _.Result).NotAutoGenerated();
            builder.Property(_ => _.Current).NotAutoGenerated();
            builder.Property(_ => _.Form).NotAutoGenerated();
            builder.Property(_ => _.ModelView).NotAutoGenerated();

            #region Form Layout

            // @formatter:off
            builder.DataFormLayout()
                .Group("g0", Orientation.Vertical)
                    .Group("a0")
                        .ContainsProperty(_ => _.AvatarSource)
                    .EndGroup()
                    .GroupBox("Данные пользователя",Orientation.Vertical)
                        .Group("g1", Orientation.Horizontal)
                            .ContainsProperty(_ => _.Name)
                            .ContainsProperty(_ => _.FullName)
                            .ContainsProperty(_ => _.Password)
                            .ContainsProperty(_ => _.ReplayPassword)
                        .EndGroup()
                        .Group("g2",Orientation.Vertical)
                            .ContainsProperty(_ => _.IsAdmin)
                            .ContainsProperty(_ => _.IsTester)
                            .ContainsProperty(_ => _.IsDeleted)
                        .EndGroup()
                    .EndGroup()
                .EndGroup();
            // @formatter:on

            #endregion
        }
    }
}