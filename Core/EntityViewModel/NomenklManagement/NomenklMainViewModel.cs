using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.NomenklManagement
{
    public class NomenklMainViewModel : RSViewModelBase, IEntity<NomenklMain>, IDataErrorInfo
    {
        private CountriesViewModel myCountry;
        private NomenklMain myEntity;
        private NomenklGroup myNomenklCategory;
        public NomenklProductType myNomenklType;
        public NomenklProductKind myProductType;
        private Unit myUnit;

        public NomenklMainViewModel()
        {
            Entity = new NomenklMain {Id = Guid.NewGuid()};
        }

        public NomenklMainViewModel(NomenklMain entity)
        {
            Entity = entity ?? new NomenklMain {Id = Guid.NewGuid()};
            LoadReference();
        }

        public ObservableCollection<Nomenkl> NomenklCollection { set; get; } = new();

        public NomenklMain Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
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

        public override string Name
        {
            get => Entity.Name;
            set
            {
                if (Entity.Name == value) return;
                Entity.Name = value;
                RaisePropertyChanged();
            }
        }

        public string NomenklNumber
        {
            get => Entity.NomenklNumber;
            set
            {
                if (Entity.NomenklNumber == value) return;
                Entity.NomenklNumber = value;
                RaisePropertyChanged();
            }
        }

        public override string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCurrencyTransfer
        {
            get => Entity.IsCurrencyTransfer ?? false;
            set
            {
                if (Entity.IsCurrencyTransfer == value) return;
                Entity.IsCurrencyTransfer = value;
                RaisePropertyChanged();
            }
        }

        public bool IsUsluga
        {
            get => Entity.IsUsluga;
            set
            {
                if (Entity.IsUsluga == value) return;
                if (CheckResetIsUsluga())
                {
                    Entity.IsUsluga = value;
                    if (!value)
                    {
                        IsNakladExpense = false;
                        IsRentabelnost = false;
                    }

                    RaisePropertyChanged();
                }
            }
        }

        public bool IsRentabelnost
        {
            get => Entity.IsRentabelnost ?? false;
            set
            {
                if (Entity.IsRentabelnost == value) return;
                Entity.IsRentabelnost = value;
                RaisePropertyChanged();
            }
        }

        public decimal CategoryDC
        {
            get => Entity.CategoryDC;
            set
            {
                if (Entity.CategoryDC == value) return;
                Entity.CategoryDC = value;
                RaisePropertyChanged();
            }
        }

        public NomenklGroup NomenklCategory
        {
            get => myNomenklCategory;
            set
            {
                if (myNomenklCategory != null && myNomenklCategory.Equals(value)) return;
                myNomenklCategory = value;
                if (myNomenklCategory != null)
                    CategoryDC = myNomenklCategory.DocCode;
                RaisePropertyChanged();
            }
        }

        public string FullName
        {
            get => Entity.FullName;
            set
            {
                if (Entity.FullName == value) return;
                Entity.FullName = value;
                RaisePropertyChanged();
            }
        }

        public bool IsNakladExpense
        {
            get => Entity.IsNakladExpense;
            set
            {
                if (Entity.IsNakladExpense == value) return;
                Entity.IsNakladExpense = value;
                RaisePropertyChanged();
            }
        }

        public Guid? CountryId
        {
            get => Entity.CountryId;
            set
            {
                if (Entity.CountryId == value) return;
                Entity.CountryId = value;
                RaisePropertyChanged();
            }
        }

        public CountriesViewModel Country
        {
            get => myCountry;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCountry == value) return;
                myCountry = value;
                CountryId = myCountry?.Id;
                RaisePropertyChanged();
            }
        }

        public decimal UnitDC
        {
            get => Entity.UnitDC;
            set
            {
                if (Entity.UnitDC == value) return;
                Entity.UnitDC = value;
                RaisePropertyChanged();
            }
        }

        public Unit Unit
        {
            get => myUnit;
            set
            {
                if (myUnit != null && myUnit.Equals(value)) return;
                myUnit = value;
                if (myUnit != null)
                    UnitDC = myUnit.DocCode;
                RaisePropertyChanged();
            }
        }

        public bool IsComplex
        {
            get => Entity.IsComplex;
            set
            {
                if (Entity.IsComplex == value) return;
                Entity.IsComplex = value;
                RaisePropertyChanged();
            }
        }

        public decimal NomenklTypeDC
        {
            get => Entity.TypeDC;
            set
            {
                if (Entity.TypeDC == value) return;
                Entity.TypeDC = value;
                RaisePropertyChanged();
            }
        }

        public NomenklProductType NomenklType
        {
            get => myNomenklType;
            set
            {
                if (myNomenklType != null && myNomenklType.Equals(value)) return;
                myNomenklType = value;
                if (myNomenklType != null)
                    NomenklTypeDC = myNomenklType.DocCode;
                RaisePropertyChanged();
            }
        }

        public NomenklProductKind ProductType
        {
            get => myProductType;
            set
            {
                if (myProductType != null && myProductType.Equals(value)) return;
                myProductType = value;
                if (myProductType != null)
                    ProductTypeDC = myProductType.DocCode;
                RaisePropertyChanged();
            }
        }

        public decimal ProductTypeDC
        {
            get => Entity.ProductDC;
            set
            {
                if (Entity.ProductDC == value) return;
                Entity.ProductDC = value;
                RaisePropertyChanged();
            }
        }

        public bool? IsDelete
        {
            get => Entity.IsDelete;
            set
            {
                if (Entity.IsDelete == value) return;
                Entity.IsDelete = value;
                RaisePropertyChanged();
            }
        }

        public decimal ProductDC
        {
            get => Entity.ProductDC;
            set
            {
                if (Entity.ProductDC == value) return;
                Entity.ProductDC = value;
                RaisePropertyChanged();
            }
        }

        public Countries Countries
        {
            get => Entity.Countries;
            set
            {
                if (Entity.Countries == value) return;
                Entity.Countries = value;
                RaisePropertyChanged();
            }
        }

        public SD_50 SD_50
        {
            get => Entity.SD_50;
            set
            {
                if (Entity.SD_50 == value) return;
                Entity.SD_50 = value;
                RaisePropertyChanged();
            }
        }

        public SD_119 SD_119
        {
            get => Entity.SD_119;
            set
            {
                if (Entity.SD_119 == value) return;
                Entity.SD_119 = value;
                RaisePropertyChanged();
            }
        }

        public SD_175 SD_175
        {
            get => Entity.SD_175;
            set
            {
                if (Entity.SD_175 == value) return;
                Entity.SD_175 = value;
                RaisePropertyChanged();
            }
        }

        public SD_82 SD_82
        {
            get => Entity.SD_82;
            set
            {
                if (Entity.SD_82 == value) return;
                Entity.SD_82 = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAccessRight { get; set; }

        List<NomenklMain> IEntity<NomenklMain>.LoadList()
        {
            throw new NotImplementedException();
        }

        private bool CheckResetIsUsluga()
        {
            if (State == RowStatus.NewRow) return true;
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (Entity.IsUsluga)
                {
                    if (ctx.TD_26.Any(_ => _.SFT_NEMENKL_DC == DocCode) ||
                        ctx.TD_84.Any(_ => _.SFT_NEMENKL_DC == DocCode))
                        return false;
                    return true;
                }

                if (ctx.TD_24.Any(_ => _.DDT_NOMENKL_DC == DocCode)) return false;
                return true;
            }
        }

        private void LoadReference()
        {
            myCountry = new CountriesViewModel(Entity.Countries);
            myNomenklType = new NomenklProductType(Entity.SD_119);
            myNomenklCategory = new NomenklGroup(Entity.SD_82);
            myUnit = new Unit(Entity.SD_175);
            if (Entity.SD_83 != null && Entity.SD_83.Count > 0)
                foreach (var n in Entity.SD_83)
                    NomenklCollection.Add(new Nomenkl(n));
            ProductType = new NomenklProductKind(Entity.SD_50);
        }

        public virtual NomenklMain Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual NomenklMain Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(NomenklMain doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(NomenklMain ent)
        {
            Id = ent.Id;
            Name = ent.Name;
            NomenklNumber = ent.NomenklNumber;
            Note = ent.Note;
            IsUsluga = ent.IsUsluga;
            CategoryDC = ent.CategoryDC;
            FullName = ent.FullName;
            IsNakladExpense = ent.IsNakladExpense;
            CountryId = ent.CountryId;
            UnitDC = ent.UnitDC;
            IsComplex = ent.IsComplex;
            NomenklTypeDC = ent.TypeDC;
            IsDelete = ent.IsDelete;
            ProductDC = ent.ProductDC;
            Countries = ent.Countries;
            SD_50 = ent.SD_50;
            SD_119 = ent.SD_119;
            SD_175 = ent.SD_175;
            SD_82 = ent.SD_82;
        }

        public void UpdateTo(NomenklMain ent)
        {
            ent.Id = Id;
            ent.Name = Name;
            ent.NomenklNumber = NomenklNumber;
            ent.Note = Note;
            ent.IsUsluga = IsUsluga;
            ent.CategoryDC = CategoryDC;
            ent.FullName = FullName;
            ent.IsNakladExpense = IsNakladExpense;
            ent.CountryId = CountryId;
            ent.UnitDC = UnitDC;
            ent.IsComplex = IsComplex;
            ent.TypeDC = NomenklTypeDC;
            ent.IsDelete = IsDelete;
            ent.ProductDC = ProductDC;
            ent.Countries = Countries;
            ent.SD_50 = SD_50;
            ent.SD_119 = SD_119;
            ent.SD_175 = SD_175;
            ent.SD_82 = SD_82;
        }

        public List<NomenklMain> LoadList()
        {
            throw new NotImplementedException();
        }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Name):
                        return Name == null ? ValidationError.FieldNotNull : null;
                    case nameof(NomenklCategory):
                        return NomenklCategory == null ? ValidationError.FieldNotNull : null;

                }

                return null;
            }
        }

        public string Error => null;
    }
}