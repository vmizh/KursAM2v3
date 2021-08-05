using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Core.EntityViewModel.Systems;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Signatures
{
    public class SignatureTypeViewModel : RSViewModelBase, IEntity<SignatureType>
    {
        #region Fields

        private DataSourceViewModel myDataSource;

        #endregion

        #region Properties

        [Display(AutoGenerateField = false)] public SignatureType Entity { get; set; }

        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
        }

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

        [Display(Name = "База данных")]
        public DataSourceViewModel DataSource
        {
            get
            {
                if (myDataSource != null) return myDataSource;
                if (Entity.DataSources != null)
                {
                    myDataSource = new DataSourceViewModel(Entity.DataSources);
                    return myDataSource;
                }

                return null;

            }
            set
            {
                if (myDataSource == value) return;
                myDataSource = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<SignatureSchemesViewModel> Shemes { set; get; } =
            new ObservableCollection<SignatureSchemesViewModel>();

        public ObservableCollection<UserViewModel> Users { set; get; } = new ObservableCollection<UserViewModel>();



        #endregion

        #region Constructors

        public SignatureTypeViewModel()
        {
            Entity = DefaultValue();
        }

        public SignatureTypeViewModel(SignatureType entity)
        {
            Entity = entity ?? DefaultValue();
        }

        #endregion

        #region Methods

        public SignatureType DefaultValue()
        {
            return new SignatureType
            {
                Id = Guid.NewGuid(),
            };
        }

        #endregion
    }
}