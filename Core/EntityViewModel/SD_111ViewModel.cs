using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.MutualAccounting;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    [MetadataType(typeof(DataAnnotationsSD_111ViewModel))]
    public class SD_111ViewModel : RSViewModelBase, IEntity<SD_111>
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public ObservableCollection<SD_110ViewModel> MutualAccountings = new ObservableCollection<SD_110ViewModel>();
        private SD_111 myEntity;

        public SD_111ViewModel()
        {
            Entity = DefaultValue();
        }

        public SD_111ViewModel(SD_111 entity)
        {
            Entity = entity ?? DefaultValue();
            UpdateFrom(Entity);
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
        public override string Name
        {
            get => Entity.ZACH_NAME;
            set
            {
                if (Entity.ZACH_NAME == value) return;
                Entity.ZACH_NAME = value;
                RaisePropertyChanged();
            }
        }
        public bool IsCurrencyConvert
        {
            get => Entity.IsCurrencyConvert;
            set
            {
                if (Entity.IsCurrencyConvert == value) return;
                Entity.IsCurrencyConvert = value;
                RaisePropertyChanged();
            }
        }
        public SD_111 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }
        public EntityLoadCodition LoadCondition { get; set; } = new EntityLoadCodition();

        public List<SD_111> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual SD_111 Load(decimal dc)
        {
            SD_111 item = null;
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    if (LoadCondition.IsShort)
                        item = ctx.SD_111
                            .FirstOrDefault(_ => _.DOC_CODE == dc);
                    else
                        item = ctx.SD_111
                            .Include(_ => _.SD_110)
                            .FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (item != null) UpdateFrom(item);
                    return item;
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
            return item;
        }

        public virtual SD_111 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_111 doc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    switch (State)
                    {
                        case RowStatus.NewRow:
                            var newDC = ctx.SD_111.Select(_ => _.DOC_CODE).Max() + 1;
                            DocCode = newDC;
                            ctx.SD_111.Add(new SD_111
                            {
                                DOC_CODE = newDC,
                                ZACH_NAME = Name,
                                IsCurrencyConvert = IsCurrencyConvert
                            });
                            ctx.SaveChanges();
                            State = RowStatus.NotEdited;
                            break;
                        case RowStatus.Edited:
                            var entity = ctx.SD_111.FirstOrDefault(_ => _.DOC_CODE == DocCode);
                            if (entity == null) return;
                            ctx.Entry(entity).CurrentValues.SetValues(Entity);
                            ctx.SaveChanges();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public void Save()
        {
            Save(Entity);
        }

        public void Delete()
        {
            Delete(DocCode);
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Delete(decimal dc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var item = ctx.SD_111
                        .FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (item == null) return;
                    ctx.SD_111.Remove(item);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public void UpdateFrom(SD_111 ent)
        {
            if (ent == null) return;
            IsCurrencyConvert = ent.IsCurrencyConvert;
            //if (ent.SD_110 != null)
            //{
            //    foreach (var d in ent.SD_110)
            //    {
            //        MutualAccountings.Add(new SD_110ViewModel(d));
            //    }
            //}
            //SD_110 = ent.SD_110;
        }

        public void UpdateTo(SD_111 ent)
        {
            ent.IsCurrencyConvert = IsCurrencyConvert;
            if (MutualAccountings != null)
                foreach (var d in MutualAccountings)
                    ent.SD_110.Add(d.Entity);
            //ent.SD_110 = SD_110;
        }

        public SD_111 DefaultValue()
        {
            return new SD_111 {DOC_CODE = -1};
        }
    }

    public class DataAnnotationsSD_111ViewModel : DataAnnotationForFluentApiBase, IMetadataProvider<SD_111ViewModel>
    {
        void IMetadataProvider<SD_111ViewModel>.BuildMetadata(MetadataBuilder<SD_111ViewModel> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Id).NotAutoGenerated();
            builder.Property(_ => _.State).NotAutoGenerated();
            builder.Property(_ => _.Note).NotAutoGenerated();
            builder.Property(_ => _.Entity).NotAutoGenerated();
            builder.Property(_ => _.LoadCondition).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.Parent).NotAutoGenerated();
            builder.Property(_ => _.StringId).NotAutoGenerated();
            builder.Property(_ => _.ParentId).NotAutoGenerated();
            builder.Property(_ => _.IsCurrencyConvert).DisplayName("Конвертация");
            builder.Property(_ => _.Name).DisplayName("Наименование");
        }
    }
}