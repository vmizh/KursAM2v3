using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Data;
using DevExpress.Mvvm.DataAnnotations;

// ReSharper disable UnusedMember.Global
namespace Core.EntityViewModel
{
    [MetadataType(typeof(DataAnnotationsCash))]
    public class Cash : RSViewModelBase, IEntity<SD_22>
    {
        private CentrOfResponsibility myCO;
        private SD_22 myEntity;
        private bool myIsCanNegative;

        public Cash()
        {
            Entity = DefaultValue();
        }

        public Cash(SD_22 entity)
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
        public string CA_NAME
        {
            get => Entity.CA_NAME;
            set
            {
                if (Entity.CA_NAME == value) return;
                Entity.CA_NAME = value;
                RaisePropertyChanged();
            }
        }
        public override string Name
        {
            get => Entity.CA_NAME;
            set
            {
                if (Entity.CA_NAME == value) return;
                Entity.CA_NAME = value;
                RaisePropertyChanged();
            }
        }
        public decimal CA_CRS_DC
        {
            get => Entity.CA_CRS_DC;
            set
            {
                if (Entity.CA_CRS_DC == value) return;
                Entity.CA_CRS_DC = value;
                RaisePropertyChanged();
            }
        }
        public short? CA_NEGATIVE_RESTS
        {
            get => Entity.CA_NEGATIVE_RESTS;
            set
            {
                if (Entity.CA_NEGATIVE_RESTS == value) return;
                Entity.CA_NEGATIVE_RESTS = value;
                RaisePropertyChanged();
            }
        }
        public bool IsCanNegative
        {
            get => myIsCanNegative;
            set
            {
                if (myIsCanNegative == value) return;
                myIsCanNegative = value;
                CA_NEGATIVE_RESTS = (short?) (myIsCanNegative ? 1 : 0);
                RaisePropertyChanged();
            }
        }
        public decimal? CA_KONTRAGENT_DC
        {
            get => Entity.CA_KONTRAGENT_DC;
            set
            {
                if (Entity.CA_KONTRAGENT_DC == value) return;
                Entity.CA_KONTRAGENT_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? CA_CENTR_OTV_DC
        {
            get => Entity.CA_CENTR_OTV_DC;
            set
            {
                if (Entity.CA_CENTR_OTV_DC == value) return;
                Entity.CA_CENTR_OTV_DC = value;
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
                if (myCO != null)
                    CA_CENTR_OTV_DC = myCO.DocCode;
                RaisePropertyChanged();
            }
        }
        public decimal? CA_KONTR_DC
        {
            get => Entity.CA_KONTR_DC;
            set
            {
                if (Entity.CA_KONTR_DC == value) return;
                Entity.CA_KONTR_DC = value;
                RaisePropertyChanged();
            }
        }
        public short? CA_NO_BALANS
        {
            get => Entity.CA_NO_BALANS;
            set
            {
                if (Entity.CA_NO_BALANS == value) return;
                Entity.CA_NO_BALANS = value;
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
        public SD_22 Entity
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

        public List<SD_22> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public SD_22 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_22 doc)
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

        public void UpdateFrom(SD_22 ent)
        {
            CA_NAME = ent.CA_NAME;
            CA_CRS_DC = ent.CA_CRS_DC;
            CA_NEGATIVE_RESTS = ent.CA_NEGATIVE_RESTS;
            CA_KONTRAGENT_DC = ent.CA_KONTRAGENT_DC;
            CA_CENTR_OTV_DC = ent.CA_CENTR_OTV_DC;
            CA_KONTR_DC = ent.CA_KONTR_DC;
            CA_NO_BALANS = ent.CA_NO_BALANS;
            SD_40 = ent.SD_40;
        }

        public void UpdateTo(SD_22 ent)
        {
            ent.CA_NAME = CA_NAME;
            ent.CA_CRS_DC = CA_CRS_DC;
            ent.CA_NEGATIVE_RESTS = CA_NEGATIVE_RESTS;
            ent.CA_KONTRAGENT_DC = CA_KONTRAGENT_DC;
            ent.CA_CENTR_OTV_DC = CA_CENTR_OTV_DC;
            ent.CA_KONTR_DC = CA_KONTR_DC;
            ent.CA_NO_BALANS = CA_NO_BALANS;
            ent.SD_40 = SD_40;
        }

        public SD_22 DefaultValue()
        {
            return new SD_22
            {
                DOC_CODE = -1
            };
        }

        public virtual SD_22 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_22 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_22 Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }

    [MetadataType(typeof(DataAnnotationsCashReference))]
    public class CashReference : Cash
    {
        private Currency myDefaultCurrency;

        public CashReference()
        {
            if (MainReferences.Currencies.ContainsKey(CA_CRS_DC))
                DefaultCurrency = MainReferences.Currencies[CA_CRS_DC];
        }

        public CashReference(SD_22 entity) : base(entity)
        {
            if (MainReferences.Currencies.ContainsKey(CA_CRS_DC))
                DefaultCurrency = MainReferences.Currencies[CA_CRS_DC];
            CO = MainReferences.GetCO(CA_CENTR_OTV_DC);
            if (Entity.TD_22 != null && Entity.TD_22.Count > 0)
                foreach (var r in Entity.TD_22)
                {
                    var newItem = new CashStartRemains(r)
                    {
                        Cash = this,
                        Parent = this,
                        ParentDC = DOC_CODE,
                        State = RowStatus.NotEdited
                    };
                    StartRemains.Add(newItem);
                }
        }

        public Currency DefaultCurrency
        {
            get => myDefaultCurrency;
            set
            {
                if (Equals(myDefaultCurrency, value)) return;
                myDefaultCurrency = value;
                if (myDefaultCurrency != null)
                    CA_CRS_DC = myDefaultCurrency.DocCode;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<CashStartRemains> StartRemains { set; get; } =
            new ObservableCollection<CashStartRemains>();
    }

    public class DataAnnotationsCash : DataAnnotationForFluentApiBase, IMetadataProvider<Cash>
    {
        void IMetadataProvider<Cash>.BuildMetadata(MetadataBuilder<Cash> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
        }
    }

    public class DataAnnotationsCashReference : DataAnnotationForFluentApiBase, IMetadataProvider<CashReference>
    {
        void IMetadataProvider<CashReference>.BuildMetadata(MetadataBuilder<CashReference> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.StartRemains).NotAutoGenerated();
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
            builder.Property(_ => _.DefaultCurrency).AutoGenerated().DisplayName("Валюта по умолчанию");
            builder.Property(_ => _.IsCanNegative).AutoGenerated().DisplayName("Отр.остатки");
            builder.Property(_ => _.CO).AutoGenerated().DisplayName("Центр ответственности");
        }
    }
}