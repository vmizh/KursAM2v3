using Data;

namespace Core.EntityViewModel.Systems
{
    public class User : EXT_USERSViewModel
    {
        public User(EXT_USERS entity) : base(entity)
        {
        }

        public User()
        {
            Entity = DefaultValue();
        }

        public override decimal DocCode => USR_ID;

        public string NickName
        {
            get => Entity.USR_NICKNAME;
            set
            {
                if (Entity.USR_NICKNAME == value) return;
                Entity.USR_NICKNAME = value;
                RaisePropertyChanged();
            }
        }

        public string FullName
        {
            get => Entity.USR_FULLNAME;
            set
            {
                if (Entity.USR_FULLNAME == value) return;
                Entity.USR_FULLNAME = value;
                RaisePropertyChanged();
            }
        }

        public override string Note
        {
            get => Entity.USR_NOTES;
            set
            {
                if (Entity.USR_NOTES == value) return;
                Entity.USR_NOTES = value;
                RaisePropertyChanged();
            }
        }

        public int? TabelNumber
        {
            get => Entity.TABELNUMBER;
            set
            {
                if (Entity.TABELNUMBER == value) return;
                Entity.TABELNUMBER = value;
                RaisePropertyChanged();
            }
        }
    }
}