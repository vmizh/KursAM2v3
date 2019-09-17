using System;
using Core.EntityViewModel;
using Data;

namespace KursAM2.ViewModel.Logistiks
{
    public class KontragentGruzoInfoViewModel : SD_43_GRUZOViewModel
    {
        public KontragentGruzoInfoViewModel(SD_43_GRUZO ent)
        {
            Entity = ent;
        }

        //public string OKPO
        //{
        //    get => Entity.OKPO;
        //    set
        //    {
        //        if (Entity.OKPO == value) return;
        //        Entity.OKPO = value;
        //        RaisePropertyChanged();
        //    }
        //}
        public string GruzoTextForSchetFactura
        {
            get => Entity.GRUZO_TEXT_SF;
            set
            {
                if (Entity.GRUZO_TEXT_SF == value) return;
                Entity.GRUZO_TEXT_SF = value;
                RaisePropertyChanged();
            }
        }

        public string GruzoTextForNaklad
        {
            get => Entity.GRUZO_TEXT_NAKLAD;
            set
            {
                if (Entity.GRUZO_TEXT_NAKLAD == value) return;
                Entity.GRUZO_TEXT_NAKLAD = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? DateChanged
        {
            get => Entity.CHANGED_DATE;
            set
            {
                if (Entity.CHANGED_DATE == value) return;
                Entity.CHANGED_DATE = value;
                RaisePropertyChanged();
            }
        }

        //public SD_43_GRUZO Entity
        //{
        //    get => Entity;
        //    set
        //    {
        //        Entity = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public new void UpdateFrom(SD_43_GRUZO ent)
        {
            GRUZO_TEXT_SF = ent.GRUZO_TEXT_SF;
            OKPO = ent.OKPO;
            CHANGED_DATE = ent.CHANGED_DATE;
            GRUZO_TEXT_NAKLAD = ent.GRUZO_TEXT_NAKLAD;
        }

        public new void UpdateTo(SD_43_GRUZO ent)
        {
            ent.GRUZO_TEXT_SF = GRUZO_TEXT_SF;
            ent.OKPO = OKPO;
            ent.CHANGED_DATE = CHANGED_DATE;
            ent.GRUZO_TEXT_NAKLAD = GRUZO_TEXT_NAKLAD;
        }

        public new SD_43_GRUZO Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public new SD_43_GRUZO Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public new void Save(SD_43_GRUZO doc)
        {
            throw new NotImplementedException();
        }
    }
}