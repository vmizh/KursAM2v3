using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel.CommonReferences
{
    [MetadataType(typeof(DataAnnotationsCurrencyRef))]
    public class CurrencyRef : Currency
    {

        public CurrencyRef(SD_301 ent) : base(ent)
        {

        }

        public bool IsMain
        {
            get => Entity.CRS_MAIN_CURRENCY == 1;
            set
            {
                if (Entity.CRS_MAIN_CURRENCY == (value ? 1 : 0)) return;
                Entity.CRS_MAIN_CURRENCY = (short)(value ? 1 : 0);
                RaisePropertyChanged();
            }
        }
    }

    [MetadataType(typeof(DataAnnotationsCurrency))]
    public class Currency : RSViewModelBase, IEntity<SD_301>, IEquatable<Currency>
    {
        private SD_301 myEntity;

        public Currency()
        {
            Entity = DefaultValue();
        }

        public Currency(SD_301 entity)
        {
            Entity = entity ?? DefaultValue();
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

        public string CRS_CODE
        {
            get => Entity.CRS_CODE;
            set
            {
                if (Entity.CRS_CODE == value) return;
                Entity.CRS_CODE = value;
                RaisePropertyChanged();
            }
        }

        public string CRS_NAME
        {
            get => Entity.CRS_NAME;
            set
            {
                if (Entity.CRS_NAME == value) return;
                Entity.CRS_NAME = value;
                RaisePropertyChanged();
            }
        }

        public string CRS_SHORTNAME
        {
            get => Entity.CRS_SHORTNAME;
            set
            {
                if (Entity.CRS_SHORTNAME == value) return;
                Entity.CRS_SHORTNAME = value;
                RaisePropertyChanged();
            }
        }

        public short CRS_MAIN_CURRENCY
        {
            get => Entity.CRS_MAIN_CURRENCY;
            set
            {
                if (Entity.CRS_MAIN_CURRENCY == value) return;
                Entity.CRS_MAIN_CURRENCY = value;
                RaisePropertyChanged();
            }
        }

        public string CRS_BIG_SYMBOL
        {
            get => Entity.CRS_BIG_SYMBOL;
            set
            {
                if (Entity.CRS_BIG_SYMBOL == value) return;
                Entity.CRS_BIG_SYMBOL = value;
                RaisePropertyChanged();
            }
        }

        public string CRS_SMALL_SYMBOL
        {
            get => Entity.CRS_SMALL_SYMBOL;
            set
            {
                if (Entity.CRS_SMALL_SYMBOL == value) return;
                Entity.CRS_SMALL_SYMBOL = value;
                RaisePropertyChanged();
            }
        }

        public int? CRS_ACTIVE
        {
            get => Entity.CRS_ACTIVE;
            set
            {
                if (Entity.CRS_ACTIVE == value) return;
                Entity.CRS_ACTIVE = value;
                RaisePropertyChanged();
            }
        }

        public new string Name
        {
            get => Entity.CRS_SHORTNAME;
            set
            {
                if (Entity.CRS_SHORTNAME == value) return;
                Entity.CRS_SHORTNAME = value;
                RaisePropertyChanged();
            }
        }

        public override decimal DocCode
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                base.DocCode = value;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

        public string FullName
        {
            get => Entity.CRS_NAME;
            set
            {
                if (Entity.CRS_NAME == value) return;
                Entity.CRS_NAME = value;
                RaisePropertyChanged();
            }
        }

        public string CrsCode
        {
            get => Entity.CRS_CODE;
            set
            {
                if (Entity.CRS_CODE == value) return;
                Entity.CRS_CODE = value;
                RaisePropertyChanged();
            }
        }

        public byte[] SMALL_SUMBOL
        {
            get => Entity.SMALL_SUMBOL;
            set
            {
                if (Entity.SMALL_SUMBOL == value) return;
                Entity.SMALL_SUMBOL = value;
                RaisePropertyChanged();
            }
        }

        public string NalogCode
        {
            get => Entity.NalogCode;
            set
            {
                if (Entity.NalogCode == value) return;
                Entity.NalogCode = value;
                RaisePropertyChanged();
            }
        }

        public string NalogName
        {
            get => Entity.NalogName;
            set
            {
                if (Entity.NalogName == value) return;
                Entity.NalogName = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

        public bool IsAccessRight { get; set; }

        public SD_301 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public bool Equals(Currency other)
        {
            return DocCode == other?.DocCode;
        }

        public List<SD_301> LoadList()
        {
            throw new NotImplementedException();
        }

        public IList<Currency> GetAllFromDataBase()
        {
            return GlobalOptions.GetEntities().SD_301.Select(item => new Currency(item)).ToList();
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Currency)obj);
        }

        public override int GetHashCode()
        {
            return DocCode.GetHashCode() ^ 397;
        }

        public void Save(SD_301 doc)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
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

        public void UpdateFrom(SD_301 ent)
        {
            CRS_CODE = ent.CRS_CODE;
            CRS_NAME = ent.CRS_NAME;
            CRS_SHORTNAME = ent.CRS_SHORTNAME;
            CRS_MAIN_CURRENCY = ent.CRS_MAIN_CURRENCY;
            CRS_BIG_SYMBOL = ent.CRS_BIG_SYMBOL;
            CRS_SMALL_SYMBOL = ent.CRS_SMALL_SYMBOL;
            CRS_ACTIVE = ent.CRS_ACTIVE;
            Id = ent.Id;
            SMALL_SUMBOL = ent.SMALL_SUMBOL;
        }

        public void UpdateTo(SD_301 ent)
        {
            ent.CRS_CODE = CRS_CODE;
            ent.CRS_NAME = CRS_NAME;
            ent.CRS_SHORTNAME = CRS_SHORTNAME;
            ent.CRS_MAIN_CURRENCY = CRS_MAIN_CURRENCY;
            ent.CRS_BIG_SYMBOL = CRS_BIG_SYMBOL;
            ent.CRS_SMALL_SYMBOL = CRS_SMALL_SYMBOL;
            ent.CRS_ACTIVE = CRS_ACTIVE;
            ent.Id = Id;
            ent.SMALL_SUMBOL = SMALL_SUMBOL;
        }

        public SD_301 DefaultValue()
        {
            return new SD_301
            {
                DOC_CODE = -1,
                Id = Guid.NewGuid(),
                CRS_ACTIVE = 1
            };
        }

        // ReSharper disable once MethodOverloadWithOptionalParameter
        public SD_301 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        // ReSharper disable once MethodOverloadWithOptionalParameter
        public SD_301 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_301 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public SD_301 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }

    public class DataAnnotationsCurrency : DataAnnotationForFluentApiBase, IMetadataProvider<Currency>
    {
        void IMetadataProvider<Currency>.BuildMetadata(MetadataBuilder<Currency> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
        }
    }

    public class DataAnnotationsCurrencyRef : DataAnnotationForFluentApiBase, IMetadataProvider<CurrencyRef>
    {
        void IMetadataProvider<CurrencyRef>.BuildMetadata(MetadataBuilder<CurrencyRef> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.CRS_SHORTNAME).AutoGenerated().DisplayName("Межд.наименование").MaxLength(3);
            builder.Property(_ => _.CRS_CODE).AutoGenerated().DisplayName("Код валюты").MaxLength(3);
            builder.Property(_ => _.CRS_NAME).AutoGenerated().DisplayName("Наименование");
            builder.Property(_ => _.IsMain).AutoGenerated().DisplayName("Валюта компании");
        }
    }
}