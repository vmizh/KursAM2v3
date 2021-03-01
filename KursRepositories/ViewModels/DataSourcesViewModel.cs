using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;

namespace KursRepositories.ViewModels
{
    public class DataSourcesViewModel : RSViewModelBase
    {
        public DataSourcesViewModel(DataSources entityDataSources)
        {
            Entity = entityDataSources;
        }
        public DataSourcesViewModel() {}

        #region Property

        public DataSources Entity { get; set; }

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

         [DisplayName("Наименование")]
        public string Name
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
        [DisplayName("Наименование для вывода")]
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

        [DisplayName("Сервер")]
        public  string  Server
        {
            get => Entity.Server;
            
        }

            #endregion


           
    }
}