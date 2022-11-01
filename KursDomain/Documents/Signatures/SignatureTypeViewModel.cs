using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;
using KursDomain.Documents.Systems;

namespace KursDomain.Documents.Signatures;

public class SignatureTypeViewModel : RSViewModelBase, IEntity<SignatureType>
{
    #region Fields

    private DataSourcesViewModel myDataSource;

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
    public DataSourcesViewModel DataSource
    {
        get
        {
            if (myDataSource != null) return myDataSource;
            if (Entity.DataSources != null)
            {
                myDataSource = new DataSourcesViewModel(Entity.DataSources);
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

    public ObservableCollection<UsersViewModel> Users { set; get; } = new ObservableCollection<UsersViewModel>();

    #endregion

    #region Constructors

    public SignatureTypeViewModel()
    {
        Entity = DefaultValue();
    }

    public SignatureTypeViewModel(SignatureType entity)
    {
        Entity = entity ?? DefaultValue();
        LoadReference();
    }

    #endregion

    #region Methods

    private void LoadReference()
    {
        foreach (var u in Entity.Users) Users.Add(new UsersViewModel(u));
    }

    public SignatureType DefaultValue()
    {
        return new SignatureType
        {
            Id = Guid.NewGuid()
        };
    }

    #endregion
}
