using System;
using System.Collections.ObjectModel;
using System.Linq;
using Core.ViewModel.Base;
using Data;
using Helper;
using KursDomain;
using KursDomain.ICommon;
using Newtonsoft.Json;

namespace KursAM2.ViewModel.Logistiks
{
    public class InventorySheetViewModel : RSViewModelBase, IEntity<SD_24>
    {
        public InventorySheetViewModel(SD_24 entity)
        {
            Entity = entity ?? DefaultValue();

            LoadReference();
        }

        public override decimal DocCode
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

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

        public ObservableCollection<InventorySheetRowViewModel> Rows { set; get; } =
            new ObservableCollection<InventorySheetRowViewModel>();


        public ObservableCollection<InventorySheetRowViewModel> DeletedRows { set; get; } =
            new ObservableCollection<InventorySheetRowViewModel>();

        public DateTime Date
        {
            get => Entity.DD_DATE;
            set
            {
                if (Entity.DD_DATE == value) return;
                Entity.DD_DATE = value;
                RaisePropertyChanged();
            }
        }

        public KursDomain.References.Warehouse Warehouse
        {
            get =>
                GlobalOptions.ReferencesCache.GetWarehouse(Entity.DD_SKLAD_POL_DC) as KursDomain.References.Warehouse;
            set
            {
                if (GlobalOptions.ReferencesCache.GetWarehouse(Entity.DD_SKLAD_POL_DC) == value) return;
                Entity.DD_SKLAD_POL_DC = value?.DocCode;
                Entity.DD_SKLAD_OTPR_DC = value?.DocCode;
                Entity.DD_POLUCH_NAME =
                    ((IName)GlobalOptions.ReferencesCache.GetWarehouse(Entity.DD_SKLAD_POL_DC)).Name.Length > 50
                        ? ((IName)GlobalOptions.ReferencesCache.GetWarehouse(Entity.DD_SKLAD_POL_DC)).Name.Substring(0,
                            50)
                        : ((IName)GlobalOptions.ReferencesCache.GetWarehouse(Entity.DD_SKLAD_POL_DC)).Name;
                RaisePropertyChanged();
            }
        }

        public KursDomain.References.Warehouse WarehouseIn
        {
            get =>
                GlobalOptions.ReferencesCache.GetWarehouse(Entity.DD_SKLAD_POL_DC) as KursDomain.References.Warehouse;
            set
            {
                if (GlobalOptions.ReferencesCache.GetWarehouse(Entity.DD_SKLAD_POL_DC) == value) return;
                Entity.DD_SKLAD_POL_DC = value?.DocCode;
                Entity.DD_SKLAD_OTPR_DC = value?.DocCode;
                RaisePropertyChanged();
            }
        }

        public KursDomain.References.Warehouse WarehouseOut
        {
            get =>
                GlobalOptions.ReferencesCache.GetWarehouse(Entity.DD_SKLAD_OTPR_DC) as KursDomain.References.Warehouse;
            set
            {
                if (GlobalOptions.ReferencesCache.GetWarehouse(Entity.DD_SKLAD_OTPR_DC) == value) return;
                Entity.DD_SKLAD_POL_DC = value?.DocCode;
                Entity.DD_SKLAD_OTPR_DC = value?.DocCode;
                RaisePropertyChanged();
            }
        }

        public int Num
        {
            get => Entity.DD_IN_NUM;
            set
            {
                if (Entity.DD_IN_NUM == value) return;
                Entity.DD_IN_NUM = value;
                RaisePropertyChanged();
            }
        }

        public bool IsClosed
        {
            get => Entity.DD_EXECUTED != 0;
            set
            {
                if (Entity.DD_EXECUTED == (short)(value ? 1 : 0)) return;
                Entity.DD_EXECUTED = (short)(value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        public string Creator
        {
            get => Entity.CREATOR;
            set
            {
                if (Entity.CREATOR == value) return;
                Entity.CREATOR = value;
                RaisePropertyChanged();
            }
        }

        public SD_24 DefaultValue()
        {
            return new SD_24
            {
                DOC_CODE = -1
            };
        }

        public SD_24 Entity { get; set; }

        private void LoadReference()
        {
            if (Entity.TD_24 != null)
                foreach (var t in Entity.TD_24)
                    Rows.Add(new InventorySheetRowViewModel(t)
                    {
                        Parent = this,
                        myState = RowStatus.NotEdited
                    });
        }

        public override string Note { get => Entity.DD_NOTES;
            set
            {
                if (Entity.DD_NOTES == value) return;
                Entity.DD_NOTES = value;
                RaisePropertyChanged();
            } }

        public override object ToJson()
        {
            var res = new
            {
                Статус = CustomFormat.GetEnumName(State),
                Id,
                DocCode,
                Номер = Num.ToString(),
                Дата = Date.ToShortDateString(),
                Склад = Warehouse.Name,
                Cоздатель = Creator,
                Примечание = Note,
                Закрыт = IsClosed ? "Да" : "Нет",
                Позиции = Rows.Select(_ => _.ToJson())
            };
            return JsonConvert.SerializeObject(res);
        }
    }
}
