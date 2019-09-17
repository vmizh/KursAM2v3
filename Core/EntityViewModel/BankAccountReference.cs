using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel
{
    /// <summary>
    ///     Банковский счет предприятия
    /// </summary>
    [MetadataType(typeof(DataAnnotationsBankAccountReference))]
    public class BankAccountReference : RSViewModelBase, IEntity<SD_114>
    {
        private Bank myBank;
        private CentrOfResponsibility myCO;
        private SD_114 myEntity;
        private bool myIsNegative;

        public BankAccountReference()
        {
            Entity = new SD_114 {DOC_CODE = -1};
        }

        public BankAccountReference(SD_114 entity)
        {
            Entity = entity ?? new SD_114 {DOC_CODE = -1};
            myIsNegative = Entity.BA_NEGATIVE_RESTS == 1;
            myCO = MainReferences.GetCO(Entity.BA_CENTR_OTV_DC);
        }

        public SD_114 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }
        public decimal DOC_CODE
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }
        public override decimal DocCode
        {
            get => DOC_CODE;
            set
            {
                if (DOC_CODE == value) return;
                DOC_CODE = value;
                RaisePropertyChanged();
            }
        }
        public int BA_RASH_ACC_CODE
        {
            get => Entity.BA_RASH_ACC_CODE;
            set
            {
                if (Entity.BA_RASH_ACC_CODE == value) return;
                Entity.BA_RASH_ACC_CODE = value;
                RaisePropertyChanged();
            }
        }
        public string BA_RASH_ACC
        {
            get => Entity.BA_RASH_ACC;
            set
            {
                if (Entity.BA_RASH_ACC == value) return;
                Entity.BA_RASH_ACC = value;
                RaisePropertyChanged();
            }
        }
        public short BA_CURRENCY
        {
            get => Entity.BA_CURRENCY;
            set
            {
                if (Entity.BA_CURRENCY == value) return;
                Entity.BA_CURRENCY = value;
                RaisePropertyChanged();
            }
        }
        public decimal? BA_BANK_AS_KONTRAGENT_DC
        {
            get => Entity.BA_BANK_AS_KONTRAGENT_DC;
            set
            {
                if (Entity.BA_BANK_AS_KONTRAGENT_DC == value) return;
                Entity.BA_BANK_AS_KONTRAGENT_DC = value;
                RaisePropertyChanged();
            }
        }
        public short BA_TRANSIT
        {
            get => Entity.BA_TRANSIT;
            set
            {
                if (Entity.BA_TRANSIT == value) return;
                Entity.BA_TRANSIT = value;
                RaisePropertyChanged();
            }
        }
        public decimal BA_BANKDC
        {
            get => Entity.BA_BANKDC;
            set
            {
                if (Entity.BA_BANKDC == value) return;
                Entity.BA_BANKDC = value;
                RaisePropertyChanged();
            }
        }
        public Bank Bank
        {
            get => myBank;
            set
            {
                if (myBank != null && myBank.Equals(value)) return;
                myBank = value;
                if (myBank != null)
                    BA_BANKDC = myBank.DocCode;
                RaisePropertyChanged();
            }
        }
        public string BA_BANK_NAME
        {
            get => Entity.BA_BANK_NAME;
            set
            {
                if (Entity.BA_BANK_NAME == value) return;
                Entity.BA_BANK_NAME = value;
                RaisePropertyChanged();
            }
        }

        public override string Name
        {
            get => BA_ACC_SHORTNAME;
            set
            {
                if (BA_ACC_SHORTNAME == value) return;
                BA_ACC_SHORTNAME = value;
                RaisePropertyChanged();
            }
        }

        public short? BA_NEGATIVE_RESTS
        {
            get => Entity.BA_NEGATIVE_RESTS;
            set
            {
                if (Entity.BA_NEGATIVE_RESTS == value) return;
                Entity.BA_NEGATIVE_RESTS = value;
                RaisePropertyChanged();
            }
        }
        public bool IsNegative
        {
            get => myIsNegative;
            set
            {
                if (myIsNegative == value) return;
                myIsNegative = value;
                BA_NEGATIVE_RESTS = (short?) (myIsNegative ? 1 : 0);
                RaisePropertyChanged();
            }
        }
        public short? BA_BANK_ACCOUNT
        {
            get => Entity.BA_BANK_ACCOUNT;
            set
            {
                if (Entity.BA_BANK_ACCOUNT == value) return;
                Entity.BA_BANK_ACCOUNT = value;
                RaisePropertyChanged();
            }
        }
        public string BA_ACC_SHORTNAME
        {
            get => Entity.BA_ACC_SHORTNAME;
            set
            {
                if (Entity.BA_ACC_SHORTNAME == value) return;
                Entity.BA_ACC_SHORTNAME = value;
                RaisePropertyChanged();
            }
        }
        public decimal? BA_CENTR_OTV_DC
        {
            get => Entity.BA_CENTR_OTV_DC;
            set
            {
                if (Entity.BA_CENTR_OTV_DC == value) return;
                Entity.BA_CENTR_OTV_DC = value;
                RaisePropertyChanged();
            }
        }
        public CentrOfResponsibility CO
        {
            get => myCO;
            set
            {
                if (myCO != null && myCO.Equals(value)) return;
                myCO = value;
                BA_CENTR_OTV_DC = myCO?.DocCode;
                RaisePropertyChanged();
            }
        }
        public SD_44 SD_44
        {
            get => Entity.SD_44;
            set
            {
                if (Entity.SD_44 == value) return;
                Entity.SD_44 = value;
                RaisePropertyChanged();
            }
        }
        public TD_43 TD_43
        {
            get => Entity.TD_43;
            set
            {
                if (Entity.TD_43 == value) return;
                Entity.TD_43 = value;
                RaisePropertyChanged();
            }
        }
        public SD_40 SD_40
        {
            get => Entity.SD_40;
            set
            {
                if (Entity.SD_40 == value) return;
                Entity.SD_40 = value;
                RaisePropertyChanged();
            }
        }
        public bool IsAccessRight { get; set; }

        public List<SD_114> LoadList()
        {
            throw new NotImplementedException();
        }

        public virtual SD_114 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_114 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_114 doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(SD_114 ent)
        {
            BA_RASH_ACC_CODE = ent.BA_RASH_ACC_CODE;
            BA_RASH_ACC = ent.BA_RASH_ACC;
            BA_CURRENCY = ent.BA_CURRENCY;
            BA_BANK_AS_KONTRAGENT_DC = ent.BA_BANK_AS_KONTRAGENT_DC;
            BA_TRANSIT = ent.BA_TRANSIT;
            BA_BANKDC = ent.BA_BANKDC;
            BA_BANK_NAME = ent.BA_BANK_NAME;
            BA_NEGATIVE_RESTS = ent.BA_NEGATIVE_RESTS;
            BA_BANK_ACCOUNT = ent.BA_BANK_ACCOUNT;
            BA_ACC_SHORTNAME = ent.BA_ACC_SHORTNAME;
            BA_CENTR_OTV_DC = ent.BA_CENTR_OTV_DC;
            SD_44 = ent.SD_44;
            TD_43 = ent.TD_43;
            SD_40 = ent.SD_40;
        }

        public void UpdateTo(SD_114 ent)
        {
            ent.BA_RASH_ACC_CODE = BA_RASH_ACC_CODE;
            ent.BA_RASH_ACC = BA_RASH_ACC;
            ent.BA_CURRENCY = BA_CURRENCY;
            ent.BA_BANK_AS_KONTRAGENT_DC = BA_BANK_AS_KONTRAGENT_DC;
            ent.BA_TRANSIT = BA_TRANSIT;
            ent.BA_BANKDC = BA_BANKDC;
            ent.BA_BANK_NAME = BA_BANK_NAME;
            ent.BA_NEGATIVE_RESTS = BA_NEGATIVE_RESTS;
            ent.BA_BANK_ACCOUNT = BA_BANK_ACCOUNT;
            ent.BA_ACC_SHORTNAME = BA_ACC_SHORTNAME;
            ent.BA_CENTR_OTV_DC = BA_CENTR_OTV_DC;
            ent.SD_44 = SD_44;
            ent.TD_43 = TD_43;
            ent.SD_40 = SD_40;
        }
    }

    public class DataAnnotationsBankAccountReference : DataAnnotationForFluentApiBase,
        IMetadataProvider<BankAccountReference>
    {
        void IMetadataProvider<BankAccountReference>.BuildMetadata(MetadataBuilder<BankAccountReference> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Bank).NotAutoGenerated();
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Короткое наименование").MaxLength(80);
            builder.Property(_ => _.BA_BANK_NAME).AutoGenerated().DisplayName("Наименование");
            builder.Property(_ => _.BA_RASH_ACC).AutoGenerated().DisplayName("Р/счет");
            builder.Property(_ => _.IsNegative).AutoGenerated().DisplayName("Отр.остатки");
            builder.Property(_ => _.CO).AutoGenerated().DisplayName("Центр ответственности");

        }
    }
}