using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Core.EntityViewModel.Systems;
using Core.ViewModel.Base;
using Data;
using KursRepositories.ViewModels;

namespace Core.EntityViewModel.Signatures
{
    public class SignatureSchemesViewModel : RSViewModelBase, IEntity<SignatureSchemes>
    {
        #region Methods

        public SignatureSchemes DefaultValue()
        {
            return new()
            {
                Id = Guid.NewGuid()
            };
        }

        private void LoadReference()
        {
            Name = Entity.KursMenuItem.Name;
            if (Entity.SignatureSchemesInfo is {Count: > 0})
            {
                foreach (var info in Entity.SignatureSchemesInfo)
                {
                    SchemesInfo.Add(new SignatureSchemesInfoViewModel(info));
                }
            }
        }

        #endregion

        #region Fields

        private DataSourcesViewModel myDataSource;
        private KursMenuItemViewModel myDocumentType;

        #endregion

        #region Properties

        public ObservableCollection<SignatureSchemesInfoViewModel> SchemesInfo { set; get; }
            = new ();

        [Display(AutoGenerateField = false)] public SignatureSchemes Entity { get; set; }

        [Display(AutoGenerateField = false)]
        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                base.Id = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public DataSourcesViewModel DataSource
        {
            get => myDataSource;
            set
            {
                if (myDataSource == value) return;
                myDataSource = value;
                Entity.DataSources = myDataSource.Entity;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public KursMenuItemViewModel DocumentType
        {
            get => myDocumentType;
            set
            {
                if (myDocumentType == value) return;
                myDocumentType = value;
                Entity.KursMenuItem = myDocumentType.Entity;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Тип документа")] 
        public string MenuItemName => Entity.KursMenuItem?.Name;

        [Display(Name = "Наименование")]
        public override string Name
        {
            get => Entity.Name;
            set
            {
                if (Entity.Name == value) return;
                Entity.Name = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Примечания")]
        public override string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Constructors

        public SignatureSchemesViewModel()
        {
            Entity = DefaultValue();
        }

        public SignatureSchemesViewModel(SignatureSchemes entity)
        {
            Entity = entity ?? DefaultValue();
            LoadReference();
        }

        #endregion
    }
}