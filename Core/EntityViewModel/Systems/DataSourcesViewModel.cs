using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Systems
{
    public class DataSourcesViewModel : RSViewModelBase
    {
       
        public DataSourcesViewModel(DataSources entityDataSources)
        {
            Entity = entityDataSources;
        }
        public DataSourcesViewModel() {}

        #region Fields

        private bool myIsSelectedItem;

        #endregion

        #region Property

        [Display(AutoGenerateField = false)]
        public DataSources Entity { get; set; }

        [DisplayName("Статус")]
        public bool IsSelectedItem
        {
            get => myIsSelectedItem;
            set
            {
                if (myIsSelectedItem == value)
                    return;
                myIsSelectedItem = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value)
                    return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
        }
        [Display(AutoGenerateField = false)]
        [DisplayName("Наименование")]
        public override string Name
            {
                get => Entity.Name;
                set
                {
                    if (Entity.Name == value)
                        return;
                    Entity.Name = value;
                    RaisePropertyChanged();
                }
            }
        [DisplayName("Наименоване компании")]
        public string ShowName
            {
                get => Entity.ShowName;
                set
                {
                    if (Entity.ShowName == value)
                        return;
                    Entity.ShowName = value;
                    RaisePropertyChanged();
                }
            }

        [Display(AutoGenerateField = false)]
        [DisplayName("Порядок")]
        public int Order
        {
            get => Entity.Order ?? 0;
            set
            {
                if (Entity.Order == value)
                    return;
                Entity.Order = value;
                RaisePropertyChanged();
            }
        }
        [Display(AutoGenerateField = false)]
        [DisplayName("Наименование базы данных")]
        public string DBName
            {
                get => Entity.DBName;
                set
                {
                    if (Entity.DBName == value)
                        return;
                    Entity.DBName = value;
                    RaisePropertyChanged();
                }
            }

        [Display(AutoGenerateField = false)]
        [DisplayName("Цвет")]
        public string Color
        {
            get => Entity.Color;
            set
            {
                if (Entity.Color == value)
                    return;
                Entity.Color = value;
                RaisePropertyChanged();
            }
        }
        [Display(AutoGenerateField = false)]
        [DisplayName("Сервер")]
        public  string  Server
        {
            get => Entity.Server;
            set
            {
                if(Entity.Server == value)
                    return;
                Entity.Server = value;
                RaisePropertyChanged();
            }
            
        }
        
        #endregion



    }
}