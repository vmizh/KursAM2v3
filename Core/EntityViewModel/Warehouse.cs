using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel
{
    [MetadataType(typeof(DataAnnotationsStore))]
    public class Warehouse : RSViewModelBase, IEntity<SD_27>
    {
        private Employee myEmployee;
        private SD_27 myEntity;
        private bool myIsCanNegativeRest;
        private Region myRegion;

        public Warehouse()
        {
            Entity = DefaultValue();
        }

        public Warehouse(SD_27 entity)
        {
            Entity = entity;
            LoadReference();
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

        public override Guid? ParentId
        {
            get => Entity.ParentId;
            set
            {
                if (Entity.ParentId == value) return;
                Entity.ParentId = value;
                RaisePropertyChanged();
            }
        }

        public string SKL_NAME
        {
            get => Entity.SKL_NAME;
            set
            {
                if (Entity.SKL_NAME == value) return;
                Entity.SKL_NAME = value;
                RaisePropertyChanged();
            }
        }

        public override string Name
        {
            get => SKL_NAME;
            set
            {
                if (SKL_NAME == value) return;
                SKL_NAME = value;
                RaisePropertyChanged();
            }
        }

        public int TABELNUMBER
        {
            get => Entity.TABELNUMBER;
            set
            {
                if (Entity.TABELNUMBER == value) return;
                Entity.TABELNUMBER = value;
                RaisePropertyChanged();
            }
        }

        public Employee Employee
        {
            get => myEmployee;
            set
            {
                if (myEmployee != null && myEmployee.Equals(value)) return;
                myEmployee = value;
                if (myEmployee != null)
                    TABELNUMBER = myEmployee.TabelNumber;
                RaisePropertyChanged();
            }
        }

        public decimal SKL_REGION_DC
        {
            get => Entity.SKL_REGION_DC;
            set
            {
                if (Entity.SKL_REGION_DC == value) return;
                Entity.SKL_REGION_DC = value;
                RaisePropertyChanged();
            }
        }

        public Region Region
        {
            get => myRegion;
            set
            {
                if (myRegion != null && myRegion.Equals(value)) return;
                myRegion = value;
                if (Region != null)
                    SKL_REGION_DC = Region.DocCode;
                RaisePropertyChanged();
            }
        }

        public short? SKL_NEGATIVE_REST
        {
            get => Entity.SKL_NEGATIVE_REST;
            set
            {
                if (Entity.SKL_NEGATIVE_REST == value) return;
                Entity.SKL_NEGATIVE_REST = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCanNegativeRest
        {
            get => myIsCanNegativeRest;
            set
            {
                if (myIsCanNegativeRest == value) return;
                myIsCanNegativeRest = value;
                SKL_NEGATIVE_REST = (short?) (myIsCanNegativeRest ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        public short? SKL_PROIZVODSTVO
        {
            get => Entity.SKL_PROIZVODSTVO;
            set
            {
                if (Entity.SKL_PROIZVODSTVO == value) return;
                Entity.SKL_PROIZVODSTVO = value;
                RaisePropertyChanged();
            }
        }

        public short? SKL_NOTAXTO_MUSTTAXFROM
        {
            get => Entity.SKL_NOTAXTO_MUSTTAXFROM;
            set
            {
                if (Entity.SKL_NOTAXTO_MUSTTAXFROM == value) return;
                Entity.SKL_NOTAXTO_MUSTTAXFROM = value;
                RaisePropertyChanged();
            }
        }

        public short? SKL_GOTOV_PRODUC
        {
            get => Entity.SKL_GOTOV_PRODUC;
            set
            {
                if (Entity.SKL_GOTOV_PRODUC == value) return;
                Entity.SKL_GOTOV_PRODUC = value;
                RaisePropertyChanged();
            }
        }

        public short? SKL_TORGOVY_ZAL
        {
            get => Entity.SKL_TORGOVY_ZAL;
            set
            {
                if (Entity.SKL_TORGOVY_ZAL == value) return;
                Entity.SKL_TORGOVY_ZAL = value;
                RaisePropertyChanged();
            }
        }

        public short? SKL_OTK
        {
            get => Entity.SKL_OTK;
            set
            {
                if (Entity.SKL_OTK == value) return;
                Entity.SKL_OTK = value;
                RaisePropertyChanged();
            }
        }

        public short? SKL_UDAL_TORG_POINT
        {
            get => Entity.SKL_UDAL_TORG_POINT;
            set
            {
                if (Entity.SKL_UDAL_TORG_POINT == value) return;
                Entity.SKL_UDAL_TORG_POINT = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SKL_KONTR_TO_SPIS_DC
        {
            get => Entity.SKL_KONTR_TO_SPIS_DC;
            set
            {
                if (Entity.SKL_KONTR_TO_SPIS_DC == value) return;
                Entity.SKL_KONTR_TO_SPIS_DC = value;
                RaisePropertyChanged();
            }
        }

        public bool IsDeleted
        {
            get => Entity.IsDeleted ?? false;
            set
            {
                if (Entity.IsDeleted == value) return;
                Entity.IsDeleted = value;
                RaisePropertyChanged();
            }
        }

        public SD_27 Entity
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

        public List<SD_27> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        private void LoadReference()
        {
            if (MainReferences.Employees.Values.Any(_ => _.TabelNumber == Entity.TABELNUMBER))
                Employee = MainReferences.Employees.Values.FirstOrDefault(_ => _.TabelNumber == Entity.TABELNUMBER);
            if (MainReferences.Regions.ContainsKey(Entity.SKL_REGION_DC))
                Region = MainReferences.Regions[Entity.SKL_REGION_DC];
        }

        public virtual void Save(SD_27 doc)
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

        public void UpdateFrom(SD_27 ent)
        {
            SKL_NAME = ent.SKL_NAME;
            TABELNUMBER = ent.TABELNUMBER;
            SKL_REGION_DC = ent.SKL_REGION_DC;
            SKL_NEGATIVE_REST = ent.SKL_NEGATIVE_REST;
            SKL_PROIZVODSTVO = ent.SKL_PROIZVODSTVO;
            SKL_NOTAXTO_MUSTTAXFROM = ent.SKL_NOTAXTO_MUSTTAXFROM;
            SKL_GOTOV_PRODUC = ent.SKL_GOTOV_PRODUC;
            SKL_TORGOVY_ZAL = ent.SKL_TORGOVY_ZAL;
            SKL_OTK = ent.SKL_OTK;
            SKL_UDAL_TORG_POINT = ent.SKL_UDAL_TORG_POINT;
            SKL_KONTR_TO_SPIS_DC = ent.SKL_KONTR_TO_SPIS_DC;
        }

        public void UpdateTo(SD_27 ent)
        {
            ent.SKL_NAME = SKL_NAME;
            ent.TABELNUMBER = TABELNUMBER;
            ent.SKL_REGION_DC = SKL_REGION_DC;
            ent.SKL_NEGATIVE_REST = SKL_NEGATIVE_REST;
            ent.SKL_PROIZVODSTVO = SKL_PROIZVODSTVO;
            ent.SKL_NOTAXTO_MUSTTAXFROM = SKL_NOTAXTO_MUSTTAXFROM;
            ent.SKL_GOTOV_PRODUC = SKL_GOTOV_PRODUC;
            ent.SKL_TORGOVY_ZAL = SKL_TORGOVY_ZAL;
            ent.SKL_OTK = SKL_OTK;
            ent.SKL_UDAL_TORG_POINT = SKL_UDAL_TORG_POINT;
            ent.SKL_KONTR_TO_SPIS_DC = SKL_KONTR_TO_SPIS_DC;
        }

        public SD_27 DefaultValue()
        {
            return new SD_27
            {
                Id = Guid.NewGuid(),
                DOC_CODE = -1
            };
        }

        public SD_27 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_27 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_27 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_27 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }

    public class DataAnnotationsStore : DataAnnotationForFluentApiBase, IMetadataProvider<Warehouse>
    {
        void IMetadataProvider<Warehouse>.BuildMetadata(MetadataBuilder<Warehouse> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
            builder.Property(_ => _.IsCanNegativeRest).AutoGenerated().DisplayName("Отрицат.остатки");
            builder.Property(_ => _.Region).AutoGenerated().DisplayName("Регион");
            builder.Property(_ => _.Employee).AutoGenerated().DisplayName("Ответственный");
        }
    }
}