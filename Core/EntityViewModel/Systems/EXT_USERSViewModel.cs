using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel.Systems
{
    [MetadataType(typeof(DataAnnotationsEXT_USERSViewModel))]
    public class EXT_USERSViewModel : RSViewModelBase, IEntity<EXT_USERS>
    {
        private EXT_USERS myEntity;

        public EXT_USERSViewModel()
        {
            Entity = new EXT_USERS();
        }

        public EXT_USERSViewModel(EXT_USERS entity)
        {
            Entity = entity ?? new EXT_USERS();
        }

        public int USR_ID
        {
            get => Entity.USR_ID;
            set
            {
                if (Entity.USR_ID == value) return;
                Entity.USR_ID = value;
                RaisePropertyChanged();
            }
        }

        [ReadOnly(true)]
        public string USR_NICKNAME
        {
            get => Entity.USR_NICKNAME;
            set
            {
                if (Entity.USR_NICKNAME == value) return;
                Entity.USR_NICKNAME = value;
                RaisePropertyChanged();
            }
        }

        [ReadOnly(true)]
        public string USR_FULLNAME
        {
            get => Entity.USR_FULLNAME;
            set
            {
                if (Entity.USR_FULLNAME == value) return;
                Entity.USR_FULLNAME = value;
                RaisePropertyChanged();
            }
        }

        [ReadOnly(true)]
        public int? TABELNUMBER
        {
            get => Entity.TABELNUMBER;
            set
            {
                if (Entity.TABELNUMBER == value) return;
                Entity.TABELNUMBER = value;
                RaisePropertyChanged();
            }
        }

        public string USR_PHONE
        {
            get => Entity.USR_PHONE;
            set
            {
                if (Entity.USR_PHONE == value) return;
                Entity.USR_PHONE = value;
                RaisePropertyChanged();
            }
        }

        public string USR_NOTES
        {
            get => Entity.USR_NOTES;
            set
            {
                if (Entity.USR_NOTES == value) return;
                Entity.USR_NOTES = value;
                RaisePropertyChanged();
            }
        }

        public string USR_PASSWORD
        {
            get => Entity.USR_PASSWORD;
            set
            {
                if (Entity.USR_PASSWORD == value) return;
                Entity.USR_PASSWORD = value;
                RaisePropertyChanged();
            }
        }

        public short? USR_PROVODKY
        {
            get => Entity.USR_PROVODKY;
            set
            {
                if (Entity.USR_PROVODKY == value) return;
                Entity.USR_PROVODKY = value;
                RaisePropertyChanged();
            }
        }

        public int? USR_ABORT_CONNECT
        {
            get => Entity.USR_ABORT_CONNECT;
            set
            {
                if (Entity.USR_ABORT_CONNECT == value) return;
                Entity.USR_ABORT_CONNECT = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(UserBlock));
            }
        }

        public bool UserBlock
        {
            set
            {
                if (Entity.USR_ABORT_CONNECT == (value ? 1 : 0)) return;
                Entity.USR_ABORT_CONNECT = value ? 1 : 0;
                RaisePropertyChanged();
            }
            get => Entity.USR_ABORT_CONNECT == 1;
        }

        public EXT_USERS Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

        public List<EXT_USERS> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public EXT_USERS Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(EXT_USERS doc)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var entity = ctx.EXT_USERS.FirstOrDefault(_ => _.USR_ID == USR_ID);
                ctx.Entry(entity).CurrentValues.SetValues(Entity);
                ctx.SaveChanges();
            }
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Delete(decimal dc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(EXT_USERS ent)
        {
            USR_ID = ent.USR_ID;
            USR_NICKNAME = ent.USR_NICKNAME;
            USR_FULLNAME = ent.USR_FULLNAME;
            TABELNUMBER = ent.TABELNUMBER;
            USR_PHONE = ent.USR_PHONE;
            USR_NOTES = ent.USR_NOTES;
            USR_PASSWORD = ent.USR_PASSWORD;
            USR_PROVODKY = ent.USR_PROVODKY;
            USR_ABORT_CONNECT = ent.USR_ABORT_CONNECT;
        }

        public void UpdateTo(EXT_USERS ent)
        {
            ent.USR_ID = USR_ID;
            ent.USR_NICKNAME = USR_NICKNAME;
            ent.USR_FULLNAME = USR_FULLNAME;
            ent.TABELNUMBER = TABELNUMBER;
            ent.USR_PHONE = USR_PHONE;
            ent.USR_NOTES = USR_NOTES;
            ent.USR_PASSWORD = USR_PASSWORD;
            ent.USR_PROVODKY = USR_PROVODKY;
            ent.USR_ABORT_CONNECT = USR_ABORT_CONNECT;
        }

        public EXT_USERS DefaultValue()
        {
            return new EXT_USERS();
            //throw new NotImplementedException();
        }

        public virtual EXT_USERS Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual EXT_USERS Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public EXT_USERS Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }

    public static class DataAnnotationsEXT_USERSViewModel
    {
        public static void BuildMetadata(MetadataBuilder<EXT_USERSViewModel> builder)
        {
            //builder.Property(_ => _.).DisplayName("").Description("").ReadOnly();
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Id).NotAutoGenerated();
            builder.Property(_ => _.State).NotAutoGenerated();
            builder.Property(_ => _.Note).NotAutoGenerated();
            builder.Property(_ => _.Entity).NotAutoGenerated();
            builder.Property(_ => _.LoadCondition).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.Parent).NotAutoGenerated();
            builder.Property(_ => _.StringId).NotAutoGenerated();
            builder.Property(_ => _.ParentId).NotAutoGenerated();
            builder.Property(_ => _.USR_ABORT_CONNECT).NotAutoGenerated();
            builder.Property(_ => _.Name).NotAutoGenerated();
            builder.Property(_ => _.USR_ID).NotAutoGenerated();
            builder.Property(_ => _.USR_PHONE).NotAutoGenerated();
            builder.Property(_ => _.USR_PASSWORD).NotAutoGenerated();
            builder.Property(_ => _.USR_PROVODKY).NotAutoGenerated();
            builder.Property(_ => _.USR_FULLNAME).DisplayName("Имя");
            builder.Property(_ => _.USR_NICKNAME).DisplayName("Логин");
            builder.Property(_ => _.TABELNUMBER).DisplayName("Табельный номер");
            builder.Property(_ => _.UserBlock).DisplayName("Заблокирован");
            builder.Property(_ => _.USR_NOTES).DisplayName("Анотация");
        }
    }
}
