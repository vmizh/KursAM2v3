using System.Collections.ObjectModel;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Common;
using Data;

namespace KursAM2.ViewModel.Dogovora
{
    public class DogovorClientHead : SD_9ViewModel
    {
        public DogovorClientHead()
        {
        }

        public DogovorClientHead(SD_9 entity) : base(entity)
        {
            Rows = new ObservableCollection<DogovorClientRow>();
            if (entity.TD_9 == null || entity.TD_9.Count <= 0) return;
            foreach (var row in entity.TD_9)
                Rows.Add(new DogovorClientRow(row));
        }

        public ObservableCollection<DogovorClientRow> Rows { set; get; }

        public Kontragent Client
        {
            get
            {
                if (Entity.ZAK_CLIENT_DC == null) return null;
                return MainReferences.AllKontragents.ContainsKey(Entity.ZAK_CLIENT_DC.Value)
                    ? MainReferences.AllKontragents[Entity.ZAK_CLIENT_DC.Value]
                    : null;
            }
            set
            {
                if (value == null)
                {
                    Entity.ZAK_CLIENT_DC = null;
                }
                else
                {
                    if (Entity.ZAK_CLIENT_DC == value.DOC_CODE) return;
                    Entity.ZAK_CLIENT_DC = value.DOC_CODE;
                }

                RaisePropertyChanged();
            }
        }

        public Kontragent Diler
        {
            get
            {
                if (Entity.ZAK_DILER_DC == null) return null;
                return MainReferences.AllKontragents.ContainsKey(Entity.ZAK_DILER_DC.Value)
                    ? MainReferences.AllKontragents[Entity.ZAK_DILER_DC.Value]
                    : null;
            }
            set
            {
                if (value == null)
                {
                    Entity.ZAK_DILER_DC = null;
                }
                else
                {
                    if (Entity.ZAK_DILER_DC == value.DOC_CODE) return;
                    Entity.ZAK_DILER_DC = value.DOC_CODE;
                }

                RaisePropertyChanged();
            }
        }

        public Currency Currency
        {
            get => !MainReferences.Currencies.ContainsKey(Entity.ZAK_CRS_DC)
                ? null
                : MainReferences.Currencies[Entity.ZAK_CRS_DC];
            set
            {
                if (Entity.ZAK_CRS_DC == value.DOC_CODE) return;
                Entity.ZAK_CRS_DC = value.DOC_CODE;
                RaisePropertyChanged();
            }
        }
    }
}