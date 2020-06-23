using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using Core.ViewModel.Base;
using Data;

namespace KursRepozit.ViewModels
{
    public class DataSourceViewModel : KursBaseViewModel, IViewModelToEntity<DataSources>
    {
        public DataSourceViewModel(DataSources entity, RowStatus state = RowStatus.NotEdited)
        { 
            Entity = entity;
            Color c = new Color();
            if (!string.IsNullOrEmpty(entity.Color))
            {
                var conv = (SolidColorBrush)new BrushConverter().ConvertFromString(entity.Color);
                if (conv != null)
                    c = conv.Color;
            }

            Id = state == RowStatus.NewRow ? Guid.NewGuid() : entity.Id;
            if (state == RowStatus.NewRow) entity.Id = Id;
            Color = c;
            Name = entity.Name;
            DBName = entity.DBName;
            Order = entity.Order;
            Server = entity.Server;
            ShowName = entity.ShowName;
            State = state;
        }


        [DisplayName("Наименование")]
        [Display(AutoGenerateField = true)]
        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value, () =>
            {
                SetChangeStatus();
                Entity.Name = Name;
            });
        }

        [DisplayName("Имя на экране")]
        [Display(AutoGenerateField = true)]
        public string ShowName
        {
            get => GetValue<string>();
            set => SetValue(value, () =>
            {
                SetChangeStatus();
                Entity.ShowName = ShowName;
            });
        }

        [DisplayName("Порядок при выборе")]
        [Display(AutoGenerateField = true)]
        public int? Order
        {
            get => GetValue<int>();
            set => SetValue(value, () =>
            {
                SetChangeStatus();
                Entity.Order = Order;
            });
        }

        [DisplayName("Сервер")]
        [Display(AutoGenerateField = true)]
        public string Server
        {
            get => GetValue<string>();
            set => SetValue(value, () =>
            {
                SetChangeStatus();
                Entity.Server = Server;
            });
        }

        [DisplayName("База данных")]
        [Display(AutoGenerateField = true)]
        public string DBName
        {
            get => GetValue<string>();
            set => SetValue(value, () =>
            {
                SetChangeStatus();
                Entity.DBName = DBName;
            });
        }

        [DisplayName("Цвет")]
        [Display(AutoGenerateField = true)]
        public Color Color
        {
            get => GetValue<Color>();
            set => SetValue(value, () =>
            {
                SetChangeStatus();
                Entity.Color = Color.ToString();
            });
        }

        [DisplayName("Entity")]
        [Display(AutoGenerateField = false)]
        public DataSources Entity { get; set; }
    }

    public class DataSourceSelectedViewModel : DataSourceViewModel
    {
        public DataSourceSelectedViewModel(DataSources entity, RowStatus state = RowStatus.NotEdited) : base(entity, state)
        {
        }

        [DisplayName("Доступ")]
        [Display(AutoGenerateField = true)]
        public bool IsSelected
        {
            set => SetValue(value, () => SetChangeStatus());
            get => GetValue<bool>();
        }
    }
}