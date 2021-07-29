using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel.Vzaimozachet
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    [MetadataType(typeof(DataAnnotationsSD_110ViewModel))]
    public sealed class SD_110ViewModel : RSViewModelBase, IEntityDocument<SD_110, TD_110ViewModel>
    {
        private Currency myCreditorCurrency;
        private Currency myDebitorCurrency;
        private SD_110 myEntity;
        private bool myIsOld;
        private SD_111ViewModel myMutualAccountingOldType;

        //private UD_110ViewModel myMutualAccountingType;

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public SD_110ViewModel()
        {
            Entity = new SD_110 {DOC_CODE = -1};
        }

        public SD_110ViewModel(SD_110 entity)
        {
            Entity = entity ?? new SD_110 {DOC_CODE = -1};
            // ReSharper disable once VirtualMemberCallInConstructor
            DocCode = Entity.DOC_CODE;
            VZ_NUM = Entity.VZ_NUM;
            VZ_DATE = Entity.VZ_DATE;
            VZ_TYPE_DC = Entity.VZ_TYPE_DC;
            VZ_PRIBIL_UCH_CRS_SUM = Entity.VZ_PRIBIL_UCH_CRS_SUM;
            VZ_NOTES = Entity.VZ_NOTES;
            CREATOR = Entity.CREATOR;
            VZ_LEFT_UCH_CRS_SUM = Entity.VZ_LEFT_UCH_CRS_SUM;
            VZ_RIGHT_UCH_CRS_SUM = Entity.VZ_RIGHT_UCH_CRS_SUM;
            tstamp = Entity.tstamp;
            ActType = Entity.ActType;
            SD_111 = Entity.SD_111;
            MutualAccountingOldType = MainReferences.MutualTypes[Entity.SD_111.DOC_CODE];
            DebitorCurrency = Entity.CurrencyFromDC != 0 ? MainReferences.Currencies[Entity.CurrencyFromDC] : null;
            CreditorCurrency = Entity.CurrencyToDC != 0 ? MainReferences.Currencies[Entity.CurrencyToDC] : null;
            Rows.Clear();
            if (Entity.TD_110 == null) return;
            foreach (var r in Entity.TD_110)
            {
                var newrow = new TD_110ViewModel(r)
                {
                    Parent = this,
                    ParentDC = ParentDC,
                    ParentId = Id
                };
                Rows.Add(newrow);
            }
        }

        public bool IsOld
        {
            get => myIsOld;
            set
            {
                if (myIsOld == value) return;
                myIsOld = value;
                RaisePropertyChanged();
            }
        }

        public override object ToJson()
        {
            var res = new
            {
                DocCode,
                Номер = VZ_NUM,
                Дата = VZ_DATE.ToShortDateString(),
                ТипЗачета = MutualAccountingOldType.ToString(),
                Результат = VZ_PRIBIL_UCH_CRS_SUM != null ? VZ_PRIBIL_UCH_CRS_SUM.Value.ToString("n2") : "0",
                СуммаДебитор = VZ_LEFT_UCH_CRS_SUM != null
                    ? VZ_LEFT_UCH_CRS_SUM.Value.ToString("n2")
                    : "0 " +
                      DebitorCurrency,
                СуммаКредитор = VZ_RIGHT_UCH_CRS_SUM != null
                    ? VZ_RIGHT_UCH_CRS_SUM.Value.ToString("n2")
                    : "0 " +
                      DebitorCurrency,
                Создатель = CREATOR,
                Примечание = VZ_NOTES,
                Позиции = Rows.Select(_ => _.ToJson())
            };
            return JsonConvert.SerializeObject(res);      
        }

        public decimal? DebitorSumma
        {
            get { return Rows?.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1).Sum(_ => _.VZT_CRS_SUMMA ?? 0) ?? 0; }
        }

        public decimal? CreditorSumma
        {
            get { return Rows?.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0).Sum(_ => _.VZT_CRS_SUMMA ?? 0) ?? 0; }
        }

        public override string Description
        {
            get
            {
                var ds = $"Дебиторы: {DebitorSumma} {DebitorCurrency} (";
                var debList = Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0).Select(_ => _.Kontragent).Distinct()
                    .ToList();
                ds = debList.Aggregate(ds, (current, cred) => current + $" {cred},");
                ds = ds.Remove(ds.Length - 1);
                ds += ")";
                var cs = $"Кредиторы: {CreditorSumma} {CreditorCurrency} (";
                var gredList = Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0).Select(_ => _.Kontragent).Distinct()
                    .ToList();
                cs = gredList.Aggregate(cs, (current, cred) => current + $" {cred},");
                cs = cs.Remove(cs.Length - 1);
                cs += ")";
                var type = SD_111?.IsCurrencyConvert ?? false ? "Акт конвертации" : "Акт взаимозачета";
                return $"{type} №{VZ_NUM} от {VZ_DATE.ToShortDateString()} {ds} {cs} {VZ_NOTES}";
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

        public override string Note
        {
            get => Entity.VZ_NOTES;
            set
            {
                if (Entity.VZ_NOTES == value) return;
                Entity.VZ_NOTES = value;
                RaisePropertyChanged();
            }
        }

        public Currency DebitorCurrency
        {
            get => myDebitorCurrency;
            set
            {
                if (MutualAccountingOldType == null) return;
                if (Equals(myDebitorCurrency, value)) return;
                myDebitorCurrency = value;
                Entity.CurrencyFromDC = myDebitorCurrency.DocCode;
                if (!MutualAccountingOldType.IsCurrencyConvert && myDebitorCurrency != null)
                {
                    myCreditorCurrency = myDebitorCurrency;
                    Entity.CurrencyToDC = myDebitorCurrency.DocCode;
                }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CreditorCurrency));
            }
        }

        public Currency CreditorCurrency
        {
            get => myCreditorCurrency;
            set
            {
                if (MutualAccountingOldType == null) return;
                if (Equals(myCreditorCurrency, value)) return;
                myCreditorCurrency = value;
                Entity.CurrencyToDC = myCreditorCurrency.DocCode;
                if (!MutualAccountingOldType.IsCurrencyConvert && myCreditorCurrency != null)
                {
                    myDebitorCurrency = myCreditorCurrency;
                    Entity.CurrencyFromDC = myCreditorCurrency.DocCode;
                }

                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(DebitorCurrency));
            }
        }

        public int VZ_NUM
        {
            get => Entity.VZ_NUM;
            set
            {
                if (Entity.VZ_NUM == value) return;
                Entity.VZ_NUM = value;
                RaisePropertyChanged();
            }
        }

        public DateTime VZ_DATE
        {
            get => Entity.VZ_DATE;
            set
            {
                if (Entity.VZ_DATE == value) return;
                Entity.VZ_DATE = value;
                RaisePropertyChanged();
            }
        }

        public SD_111ViewModel MutualAccountingOldType
        {
            get => myMutualAccountingOldType;
            set
            {
                if (myMutualAccountingOldType != null && myMutualAccountingOldType.Equals(value)) return;
                myMutualAccountingOldType = value;
                VZ_TYPE_DC = myMutualAccountingOldType?.DocCode ?? -1;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(VZ_TYPE_DC));
            }
        }

        public decimal VZ_TYPE_DC
        {
            get => Entity.VZ_TYPE_DC;
            set
            {
                if (Entity.VZ_TYPE_DC == value) return;
                Entity.VZ_TYPE_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VZ_PRIBIL_UCH_CRS_SUM
        {
            get => Entity.VZ_PRIBIL_UCH_CRS_SUM;
            set
            {
                if (Entity.VZ_PRIBIL_UCH_CRS_SUM == value) return;
                Entity.VZ_PRIBIL_UCH_CRS_SUM = value;
                RaisePropertyChanged();
            }
        }

        public string VZ_NOTES
        {
            get => Entity.VZ_NOTES;
            set
            {
                if (Entity.VZ_NOTES == value) return;
                Entity.VZ_NOTES = value;
                RaisePropertyChanged();
            }
        }

        public string CREATOR
        {
            get => Entity.CREATOR;
            set
            {
                if (Entity.CREATOR == value) return;
                Entity.CREATOR = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VZ_LEFT_UCH_CRS_SUM
        {
            get => Entity.VZ_LEFT_UCH_CRS_SUM;
            set
            {
                if (Entity.VZ_LEFT_UCH_CRS_SUM == value) return;
                Entity.VZ_LEFT_UCH_CRS_SUM = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VZ_RIGHT_UCH_CRS_SUM
        {
            get => Entity.VZ_RIGHT_UCH_CRS_SUM;
            set
            {
                if (Entity.VZ_RIGHT_UCH_CRS_SUM == value) return;
                Entity.VZ_RIGHT_UCH_CRS_SUM = value;
                RaisePropertyChanged();
            }
        }

        public byte[] tstamp
        {
            get => Entity.tstamp;
            set
            {
                if (Entity.tstamp == value) return;
                Entity.tstamp = value;
                RaisePropertyChanged();
            }
        }

        public Guid? ActType
        {
            get => Entity.ActType;
            set
            {
                if (Entity.ActType == value) return;
                Entity.ActType = value;
                RaisePropertyChanged();
            }
        }

        public SD_111 SD_111
        {
            get => Entity.SD_111;
            set
            {
                if (Entity.SD_111 == value) return;
                Entity.SD_111 = value;
                myMutualAccountingOldType = Entity.SD_111 != null ? new SD_111ViewModel(Entity.SD_111) : null;
                RaisePropertyChanged();
            }
        }

        public UD_110 UD_110
        {
            get => Entity.UD_110;
            set
            {
                if (Entity.UD_110 == value) return;
                Entity.UD_110 = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

        public bool IsAccessRight { get; set; }


        public SD_110 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<TD_110ViewModel> Rows { get; set; } = new();

        public ObservableCollection<TD_110ViewModel> DeletedRows { get; set; } =
            new();

        public bool DeletedRow(TD_110ViewModel row)
        {
            try
            {
                Rows.Remove(row);
                DeletedRows.Add(row);
                return true;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                return false;
            }
        }

        public bool DeletedRow(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool DeletedRow(int id)
        {
            throw new NotImplementedException();
        }

        public List<SD_110> LoadList()
        {
            throw new NotImplementedException();
        }

        public SD_110 Load(decimal dc)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var doc = ctx.SD_110
                    .Include(_ => _.TD_110)
                    .Include(_ => _.SD_111)
                    .Include("TD_110.SD_26")
                    .Include("TD_110.SD_301")
                    .Include("TD_110.SD_3011")
                    .Include("TD_110.SD_303")
                    .Include("TD_110.SD_43")
                    .Include("TD_110.SD_77")
                    .Include("TD_110.SD_84")
                    //.Include(_ => _.UD_110)
                    .FirstOrDefault(_ => _.DOC_CODE == dc);
                if (doc == null) return null;
                UpdateFrom(doc);
                State = RowStatus.NotEdited;
                return doc;
            }
        }

        public SD_110 Load(Guid id)
        {
            return DefaultValue();
        }

        public void Save(SD_110 doc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    switch (State)
                    {
                        case RowStatus.NewRow:
                            var newDC = ctx.SD_110.Select(_ => _.DOC_CODE).Max() + 1;
                            DocCode = newDC;
                            var i = 1;
                            foreach (var row in Rows)
                            {
                                row.DocCode = newDC;
                                row.Code = i;
                                row.Entity.DOC_CODE = newDC;
                                row.Code = i;
                                ctx.TD_110.Add(row.Entity);
                                i++;
                            }

                            ctx.SD_110.Add(Entity);
                            ctx.SaveChanges();
                            State = RowStatus.NotEdited;
                            break;
                        case RowStatus.Edited:
                            var entity = ctx.SD_110.FirstOrDefault(_ => _.DOC_CODE == DocCode);
                            if (entity == null) return;
                            ctx.Entry(entity).CurrentValues.SetValues(Entity);
                            foreach (var r in DeletedRows)
                            {
                                var delRow =
                                    ctx.TD_110.FirstOrDefault(_ => _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                                if (delRow != null)
                                    ctx.TD_110.Remove(delRow);
                            }

                            foreach (var r in Rows)
                                switch (r.State)
                                {
                                    case RowStatus.Edited:
                                        var row =
                                            ctx.TD_110.FirstOrDefault(_ => _.DOC_CODE == DocCode && _.CODE == r.Code);
                                        if (row == null) break;
                                        ctx.Entry(row).CurrentValues.SetValues(r.Entity);
                                        break;
                                    case RowStatus.NewRow:
                                        ctx.TD_110.Add(r.Entity);
                                        break;
                                }

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
                    var rows = ctx.TD_110.Where(_ => _.DOC_CODE == dc);
                    ctx.TD_110.RemoveRange(rows);
                    var item = ctx.SD_110
                        .FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (item == null) return;
                    ctx.SD_110.Remove(item);
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public void UpdateFrom(SD_110 ent)
        {
            DocCode = ent.DOC_CODE;
            VZ_NUM = ent.VZ_NUM;
            VZ_DATE = ent.VZ_DATE;
            VZ_TYPE_DC = ent.VZ_TYPE_DC;
            VZ_PRIBIL_UCH_CRS_SUM = ent.VZ_PRIBIL_UCH_CRS_SUM;
            VZ_NOTES = ent.VZ_NOTES;
            CREATOR = ent.CREATOR;
            VZ_LEFT_UCH_CRS_SUM = ent.VZ_LEFT_UCH_CRS_SUM;
            VZ_RIGHT_UCH_CRS_SUM = ent.VZ_RIGHT_UCH_CRS_SUM;
            tstamp = ent.tstamp;
            ActType = ent.ActType;
            SD_111 = ent.SD_111;
            UD_110 = ent.UD_110;
            myMutualAccountingOldType = SD_111 != null ? MainReferences.MutualTypes[SD_111.DOC_CODE] : null;
            DebitorCurrency = ent.CurrencyFromDC != 0 ? MainReferences.Currencies[ent.CurrencyFromDC] : null;
            CreditorCurrency = ent.CurrencyToDC != 0 ? MainReferences.Currencies[ent.CurrencyToDC] : null;
            Rows.Clear();
            if (ent.TD_110 == null) return;
            foreach (var r in ent.TD_110)
            {
                var newrow = new TD_110ViewModel(r)
                {
                    Parent = this,
                    ParentDC = DocCode,
                    ParentId = Id
                };
                Rows.Add(newrow);
            }

            if (ent.DOC_CODE <= 11100025000)
                IsOld = true;
        }

        public void UpdateTo(SD_110 ent)
        {
            ent.DOC_CODE = DocCode;
            ent.VZ_NUM = VZ_NUM;
            ent.VZ_DATE = VZ_DATE;
            ent.VZ_TYPE_DC = VZ_TYPE_DC;
            ent.VZ_PRIBIL_UCH_CRS_SUM = VZ_PRIBIL_UCH_CRS_SUM;
            ent.VZ_NOTES = VZ_NOTES;
            ent.CREATOR = CREATOR;
            ent.VZ_LEFT_UCH_CRS_SUM = VZ_LEFT_UCH_CRS_SUM;
            ent.VZ_RIGHT_UCH_CRS_SUM = VZ_RIGHT_UCH_CRS_SUM;
            ent.tstamp = tstamp;
            ent.ActType = ActType;
            ent.SD_111 = SD_111;
            ent.UD_110 = UD_110;
            ent.CurrencyFromDC = DebitorCurrency.DocCode;
            ent.CurrencyToDC = CreditorCurrency.DocCode;
        }

        public SD_110 DefaultValue()
        {
            return new() {DOC_CODE = -1, CREATOR = GlobalOptions.UserInfo?.Name};
        }

        public bool DeletedRow(SD_110 row)
        {
            throw new NotImplementedException();
        }

        public bool DeletedRow(TD_110 row)
        {
            throw new NotImplementedException();
        }
    }

    public static class DataAnnotationsSD_110ViewModel
    {
        public static void BuildMetadata(MetadataBuilder<SD_110ViewModel> builder)
        {
            //builder.Property(_ => _.).DisplayName("").Description("").ReadOnly();
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Id).NotAutoGenerated();
            builder.Property(_ => _.State).NotAutoGenerated();
            builder.Property(_ => _.Note).NotAutoGenerated();
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Entity).NotAutoGenerated();
            builder.Property(_ => _.LoadCondition).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
            builder.Property(_ => _.Parent).NotAutoGenerated();
            builder.Property(_ => _.StringId).NotAutoGenerated();
            builder.Property(_ => _.ParentId).NotAutoGenerated();
            builder.Property(_ => _.ActType).NotAutoGenerated();
            //builder.Property(_ => _.MutualAccountingType).NotAutoGenerated();
            builder.Property(_ => _.VZ_PRIBIL_UCH_CRS_SUM).NotAutoGenerated();
            builder.Property(_ => _.Name).NotAutoGenerated();
            builder.Property(_ => _.VZ_TYPE_DC).NotAutoGenerated();
            builder.Property(_ => _.SD_111).NotAutoGenerated();
            builder.Property(_ => _.UD_110).NotAutoGenerated();
            builder.Property(_ => _.VZ_DATE).DisplayName("Дата");
            builder.Property(_ => _.VZ_NUM).DisplayName("Номер");
            builder.Property(_ => _.MutualAccountingOldType).DisplayName("Тип зачета");
            builder.Property(_ => _.DebitorSumma).DisplayName("Дебитор (-) к балансу");
            builder.Property(_ => _.CreditorSumma).DisplayName("Кредитор (+) к балансу");
            builder.Property(_ => _.DebitorCurrency).DisplayName("Валюта дебитора");
            builder.Property(_ => _.CreditorCurrency).DisplayName("Валюта кредитора");
            builder.Property(_ => _.VZ_NOTES).DisplayName("Примечание");
            builder.Property(_ => _.CREATOR).DisplayName("Создатель");
            builder.Property(_ => _.IsOld).DisplayName("Старый");
            builder.Property(_ => _.VZ_LEFT_UCH_CRS_SUM).NotAutoGenerated();
            builder.Property(_ => _.VZ_RIGHT_UCH_CRS_SUM).NotAutoGenerated();
            builder.Property(_ => _.IsAccessRight).NotAutoGenerated();
        }
    }
}