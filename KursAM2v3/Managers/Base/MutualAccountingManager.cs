using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Core;
using Core.EntityViewModel.Vzaimozachet;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;

namespace KursAM2.Managers.Base
{
    public class MutualAccountingManager : DocumentWithRowManager<SD_110ViewModel, TD_110ViewModel>
    {
        private readonly decimal maxOldDC;

        public MutualAccountingManager()
        {
            maxOldDC = GetMaxOldDC();
        }

        public override SD_110ViewModel Load()
        {
            throw new NotImplementedException();
        }

        public override SD_110ViewModel Load(decimal dc)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var ent = ctx.SD_110
                    .Include(_ => _.TD_110)
                    .Include(_ => _.SD_111)
                    .Include("TD_110.SD_26")
                    .Include("TD_110.SD_301")
                    .Include("TD_110.SD_3011")
                    .Include("TD_110.SD_303")
                    .Include("TD_110.SD_43")
                    .Include("TD_110.SD_77")
                    .Include("TD_110.SD_84")
                    .AsNoTracking()
                    .FirstOrDefault(_ => _.DOC_CODE == dc);
                if (ent == null) return new SD_110ViewModel();
                return new SD_110ViewModel(ent) { State = RowStatus.NotEdited };
            }
        }

        private decimal GetMaxOldDC()
        {
            var d = GlobalOptions.SystemProfile.Profile
                .FirstOrDefault(_ => _.SECTION == "MUTUAL_ACCOUNTING" && _.ITEM == "MAX_OLD_DC");
            return d != null ? Convert.ToDecimal(d.ITEM_VALUE) : 0;
        }

        public bool CheckDocumentForOld(decimal dc)
        {
            return dc <= maxOldDC;
        }

        public override SD_110ViewModel Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public override SD_110ViewModel Save(SD_110ViewModel doc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var isConvert = MainReferences.MutualTypes[doc.VZ_TYPE_DC].IsCurrencyConvert;
                    switch (doc.State)
                    {
                        case RowStatus.NewRow:
                            var newDC = ctx.SD_110.Any() ? ctx.SD_110.Max(_ => _.DOC_CODE) + 1 : 11100000001;
                            doc.VZ_NUM = ctx.SD_110.Any() ? ctx.SD_110.Max(_ => _.VZ_NUM) + 1 : 1;
                            doc.DocCode = newDC;
                            ctx.SD_110.Add(new SD_110
                            {
                                DOC_CODE = newDC,
                                VZ_NUM = doc.Entity.VZ_NUM,
                                VZ_DATE = doc.Entity.VZ_DATE,
                                VZ_TYPE_DC = doc.Entity.VZ_TYPE_DC,
                                VZ_PRIBIL_UCH_CRS_SUM = doc.Entity.VZ_PRIBIL_UCH_CRS_SUM,
                                VZ_NOTES = doc.Entity.VZ_NOTES,
                                CREATOR = doc.Entity.CREATOR,
                                VZ_LEFT_UCH_CRS_SUM = doc.Entity.VZ_LEFT_UCH_CRS_SUM,
                                VZ_RIGHT_UCH_CRS_SUM = doc.Entity.VZ_RIGHT_UCH_CRS_SUM,
                                CurrencyFromDC = doc.Entity.CurrencyFromDC,
                                CurrencyToDC = doc.Entity.CurrencyToDC
                            });
                            foreach (var r in doc.Rows)
                            {
                                r.DocCode = newDC;
                                ctx.TD_110.Add(new TD_110
                                {
                                    DOC_CODE = newDC,
                                    CODE = r.Entity.CODE,
                                    VZT_CRS_SUMMA = r.Entity.VZT_CRS_SUMMA,
                                    VZT_CRS_DC = r.Entity.VZT_CRS_DC,
                                    VZT_SPOST_DC = r.Entity.VZT_SPOST_DC,
                                    VZT_SFACT_DC = r.Entity.VZT_SFACT_DC,
                                    VZT_DOC_DATE = isConvert ? doc.Entity.VZ_DATE : r.Entity.VZT_DOC_DATE,
                                    VZT_DOC_NUM = r.Entity.VZT_DOC_NUM,
                                    VZT_DOC_NOTES = r.Entity.VZT_DOC_NOTES,
                                    VZT_KONTR_DC = r.Entity.VZT_KONTR_DC,
                                    VZT_CRS_POGASHENO = r.Entity.VZT_CRS_POGASHENO,
                                    VZT_UCH_CRS_POGASHENO = r.Entity.VZT_UCH_CRS_POGASHENO,
                                    VZT_UCH_CRS_RATE = r.Entity.VZT_UCH_CRS_RATE,
                                    VZT_VZAIMOR_TYPE_DC = r.Entity.VZT_VZAIMOR_TYPE_DC,
                                    VZT_1MYDOLZH_0NAMDOLZH = r.Entity.VZT_1MYDOLZH_0NAMDOLZH,
                                    VZT_KONTR_CRS_DC = r.Entity.VZT_KONTR_CRS_DC,
                                    VZT_KONTR_CRS_RATE = r.Entity.VZT_KONTR_CRS_RATE,
                                    VZT_KONTR_CRS_SUMMA = r.Entity.VZT_KONTR_CRS_SUMMA,
                                    VZT_SHPZ_DC = r.Entity.VZT_SHPZ_DC
                                });
                            }

                            break;
                        case RowStatus.Edited:
                        case RowStatus.NotEdited:
                            var entity = ctx.SD_110.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                            if (entity == null) return null;
                            ctx.Entry(entity).CurrentValues.SetValues(doc.Entity);
                            foreach (var r in doc.DeletedRows)
                            {
                                var delRow =
                                    ctx.TD_110.FirstOrDefault(_ => _.DOC_CODE == r.DocCode && _.CODE == r.Code);
                                if (delRow != null)
                                    ctx.TD_110.Remove(delRow);
                            }

                            foreach (var r in doc.Rows)
                            {
                                r.VZT_DOC_DATE = isConvert ? doc.Entity.VZ_DATE : r.Entity.VZT_DOC_DATE;
                                switch (r.State)
                                {
                                    case RowStatus.Edited:
                                    case RowStatus.NotEdited:
                                        var row =
                                            ctx.TD_110.FirstOrDefault(
                                                _ => _.DOC_CODE == doc.DocCode && _.CODE == r.Code);
                                        if (row == null) break;
                                        ctx.Entry(row).CurrentValues.SetValues(r.Entity);
                                        break;
                                    case RowStatus.NewRow:
                                        ctx.TD_110.Add(new TD_110
                                        {
                                            DOC_CODE = r.DocCode,
                                            CODE = r.Entity.CODE,
                                            VZT_CRS_SUMMA = r.Entity.VZT_CRS_SUMMA,
                                            VZT_CRS_DC = r.Entity.VZT_CRS_DC,
                                            VZT_SPOST_DC = r.Entity.VZT_SPOST_DC,
                                            VZT_SFACT_DC = r.Entity.VZT_SFACT_DC,
                                            VZT_DOC_DATE = isConvert ? doc.Entity.VZ_DATE : r.Entity.VZT_DOC_DATE,
                                            VZT_DOC_NUM = r.Entity.VZT_DOC_NUM,
                                            VZT_DOC_NOTES = r.Entity.VZT_DOC_NOTES,
                                            VZT_KONTR_DC = r.Entity.VZT_KONTR_DC,
                                            VZT_CRS_POGASHENO = r.Entity.VZT_CRS_POGASHENO,
                                            VZT_UCH_CRS_POGASHENO = r.Entity.VZT_UCH_CRS_POGASHENO,
                                            VZT_UCH_CRS_RATE = r.Entity.VZT_UCH_CRS_RATE,
                                            VZT_VZAIMOR_TYPE_DC = r.Entity.VZT_VZAIMOR_TYPE_DC,
                                            VZT_1MYDOLZH_0NAMDOLZH = r.Entity.VZT_1MYDOLZH_0NAMDOLZH,
                                            VZT_KONTR_CRS_DC = r.Entity.VZT_KONTR_CRS_DC,
                                            VZT_KONTR_CRS_RATE = r.Entity.VZT_KONTR_CRS_RATE,
                                            VZT_KONTR_CRS_SUMMA = r.Entity.VZT_KONTR_CRS_SUMMA,
                                            VZT_SHPZ_DC = r.Entity.VZT_SHPZ_DC
                                        });
                                        break;
                                }
                            }

                            break;
                    }

                    ctx.SaveChanges();
                    doc.myState = RowStatus.NotEdited;
                    foreach (var r in doc.Rows)
                    {
                        r.myState = RowStatus.NotEdited;
                        r.RaisePropertyChanged("State");
                    }

                    doc.RaisePropertyChanged("State");
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
                return null;
            }

            return doc;
        }

        public override bool IsChecked(SD_110ViewModel doc)
        {
            CheckedInfo = null;
            if (doc == null)
            {
                CheckedInfo = "Пустой документ";
                return false;
            }

            var info = new StringBuilder();
            var ret = true;
            if (doc.VZ_DATE == DateTime.MinValue)
            {
                info.Append("Не установлена дата документа.\n");
                ret = false;
            }

            if (doc.CreditorCurrency == null)
            {
                info.Append("Не установлена валюта документа.\n");
                ret = false;
            }

            if (doc.DebitorCurrency == null)
            {
                info.Append("Не установлена валюта документа.\n");
                ret = false;
            }

            if (doc.VZ_TYPE_DC < 1111000001)
            {
                info.Append("Не выбран тип взаимозачета.\n");
                ret = false;
            }

            if (string.IsNullOrWhiteSpace(doc.CREATOR))
            {
                info.Append("Не установлен создатель документа.\n");
                ret = false;
            }

            if (doc.Rows != null && doc.Rows.Count > 0)
                foreach (var r in doc.Rows)
                {
                    if (r.DocCode != doc.DocCode)
                    {
                        info.Append("Doc-Code строки не равен коду документа.\n");
                        ret = false;
                    }

                    var res = IsRowChecked(r);
                    if (res.Item1) continue;
                    info.Append(res.Item2);
                    ret = false;
                }

            CheckedInfo = info.ToString();
            return ret;
        }

        public override SD_110ViewModel New()
        {
            var ret = new SD_110ViewModel
            {
                DocCode = -1,
                VZ_NUM = -1,
                VZ_DATE = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.NickName,
                Rows = new ObservableCollection<TD_110ViewModel>(),
                IsOld = false,
                State = RowStatus.NewRow
            };
            return ret;
        }

        public override SD_110ViewModel NewFullCopy(SD_110ViewModel doc)
        {
            var newDoc = NewRequisity(doc);
            foreach (var r in doc.Rows)
            {
                var newRow = new TD_110ViewModel(r.Entity)
                {
                    DocCode = -1,
                    Code = r.Code,
                    VZT_DOC_DATE = DateTime.Today,
                    State = RowStatus.NewRow
                };
                newDoc.Rows.Add(newRow);
            }

            newDoc.State = RowStatus.NewRow;
            return newDoc;
        }

        public override SD_110ViewModel NewRequisity(SD_110ViewModel doc)
        {
            var newDoc = New();
            newDoc.DocCode = -1;
            newDoc.VZ_NUM = -1;
            newDoc.CREATOR = GlobalOptions.UserInfo.NickName;
            newDoc.MutualAccountingOldType = doc.MutualAccountingOldType;
            newDoc.DebitorCurrency = doc.DebitorCurrency;
            newDoc.CreditorCurrency = doc.CreditorCurrency;
            return newDoc;
        }

        public override void Delete(SD_110ViewModel doc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var d = ctx.SD_110.Include(_ => _.TD_110).FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                    if (d != null)
                    {
                        ctx.SD_110.Remove(d);
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void Delete(decimal dc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var doc = ctx.SD_110.Include(_ => _.TD_110).FirstOrDefault(_ => _.DOC_CODE == dc);
                    if (doc != null)
                    {
                        ctx.SD_110.Remove(doc);
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public override TD_110ViewModel AddRow(ObservableCollection<TD_110ViewModel> rows, TD_110ViewModel row,
            // ReSharper disable once OptionalParameterHierarchyMismatch
            short dolg = 0)
        {
            if (rows == null) return null;
            if (row == null)
                row = new TD_110ViewModel
                {
                    VZT_DOC_DATE = DateTime.Today,
                    Code = rows.Count + 1,
                    VZT_DOC_NUM = (rows.Count + 1).ToString(),
                    VZT_1MYDOLZH_0NAMDOLZH = dolg,
                    VZT_CRS_POGASHENO = 0,
                    VZT_UCH_CRS_POGASHENO = 0,
                    VZT_CRS_SUMMA = 0,
                    VZT_KONTR_CRS_RATE = 1,
                    VZT_KONTR_CRS_SUMMA = 0,
                    VZT_UCH_CRS_RATE = 1,
                    State = RowStatus.NewRow
                };
            else
                row = new TD_110ViewModel
                {
                    VZT_DOC_DATE = row.VZT_DOC_DATE,
                    Code = rows.Count + 1,
                    VZT_DOC_NUM = (rows.Count + 1).ToString(),
                    VZT_1MYDOLZH_0NAMDOLZH = row.VZT_1MYDOLZH_0NAMDOLZH,
                    VZT_CRS_POGASHENO = row.VZT_CRS_POGASHENO,
                    VZT_UCH_CRS_POGASHENO = row.VZT_UCH_CRS_POGASHENO,
                    VZT_CRS_SUMMA = row.VZT_CRS_SUMMA,
                    VZT_KONTR_CRS_RATE = row.VZT_KONTR_CRS_RATE,
                    VZT_KONTR_CRS_SUMMA = row.VZT_KONTR_CRS_SUMMA,
                    VZT_UCH_CRS_RATE = row.VZT_UCH_CRS_RATE,
                    State = RowStatus.NewRow
                };
            rows.Add(row);
            return row;
        }

        public override TD_110ViewModel DeleteRow(ObservableCollection<TD_110ViewModel> rows,
            ObservableCollection<TD_110ViewModel> deletedrows, TD_110ViewModel row)
        {
            //var drow = Rows.FirstOrDefault(_ => _.Code == row.Code);
            if (row != null)
            {
                if (row.State == RowStatus.NewRow)
                {
                    rows.Remove(row);
                    return row;
                }

                deletedrows.Add(row);
                rows.Remove(row);
                if (row.Parent is RSViewModelBase prnt)
                    prnt.State = prnt.State == RowStatus.NewRow ? RowStatus.NewRow : RowStatus.Edited;
            }

            return row;
        }

        public override Tuple<bool, string> IsRowChecked(TD_110ViewModel r)
        {
            var ret = true;
            var info = new StringBuilder();
            if (r.Code < 1)
            {
                info.Append("Не установлен код строки.\n");
                ret = false;
            }

            if (r.VZT_CRS_DC < 3010000001)
            {
                info.Append("Не выбрана валюта строки.\n");
                ret = false;
            }

            if (r.VZT_DOC_DATE == DateTime.MinValue)
            {
                info.Append("Не установлена дата строки.\n");
                ret = false;
            }

            if (r.VZT_VZAIMOR_TYPE_DC < 10770000001)
            {
                info.Append("Не выбран тип взаиморасчета строки.\n");
                ret = false;
            }

            if (r.VZT_1MYDOLZH_0NAMDOLZH < 0 || r.VZT_1MYDOLZH_0NAMDOLZH > 1)
            {
                info.Append("Неправильно вставлен флаг для проводки в строке.\n");
                ret = false;
            }

            CheckedInfo = "\n" + info;
            return Tuple.Create(ret, info.ToString());
        }

        public override List<SD_110ViewModel> GetDocuments()
        {
            throw new NotImplementedException();
        }

        public override List<SD_110ViewModel> GetDocuments(DateTime dateStart, DateTime dateEnd)
        {
            throw new NotImplementedException();
        }

        public override List<SD_110ViewModel> GetDocuments(DateTime dateStart, DateTime dateEnd, string searchText)
        {
            throw new NotImplementedException();
        }
    }
}
