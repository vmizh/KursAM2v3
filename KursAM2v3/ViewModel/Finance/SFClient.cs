using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Data;


namespace KursAM2.ViewModel.Finance
{
    public class SFClient : SD_84ViewModel, IDocument<SFClient>
    {
        public SFClient()
        {
            DeletedRows = new List<SFClientRow>();
            Rows = new ObservableCollection<SFClientRow>();
        }

        public SFClient(SD_84 entity) : base(entity)
        {
            Rows = new ObservableCollection<SFClientRow>();
            if (entity.TD_84 == null || entity.TD_84.Count <= 0) return;
            foreach (var row in entity.TD_84)
            {
                var newRow = new SFClientRow(row);
                Rows.Add(newRow);
            }

        }

        public List<SFClientRow> DeletedRows { set; get; }
        public ObservableCollection<SFClientRow> Rows { set; get; }

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
            => $"С/ф №{Entity.SF_IN_NUM}/{Entity.SF_OUT_NUM} от {Entity.SF_DATE.ToShortDateString()}";

        public Kontragent Client
        {
            get => Entity.SF_CLIENT_DC == null ? null : MainReferences.GetKontragent(Entity.SF_CLIENT_DC.Value);
            set
            {
                if (value == null)
                {
                    Entity.SF_CLIENT_DC = null;
                }
                else
                {
                    if (Entity.SF_CLIENT_DC == value.DOC_CODE) return;
                    Entity.SF_CLIENT_DC = value.DOC_CODE;
                }
                RaisePropertyChanged();
            }
        }

        public Kontragent Diler
        {
            get
            {
                if (Entity.SF_DILER_DC == null) return null;
                return MainReferences.AllKontragents.ContainsKey(Entity.SF_DILER_DC.Value)
                    ? MainReferences.AllKontragents[Entity.SF_DILER_DC.Value]
                    : null;
            }
            set
            {
                if (value == null)
                {
                    Entity.SF_DILER_DC = null;
                }
                else
                {
                    if (Entity.SF_DILER_DC == value.DOC_CODE) return;
                    Entity.SF_DILER_DC = value.DOC_CODE;
                }
                RaisePropertyChanged();
            }
        }

        public Kontragent Receiver
        {
            get => Entity.SF_RECEIVER_KONTR_DC == null
                ? null
                : MainReferences.GetKontragent(Entity.SF_RECEIVER_KONTR_DC.Value);
            set
            {
                if (value == null)
                {
                    Entity.SF_RECEIVER_KONTR_DC = null;
                }
                else
                {
                    if (Entity.SF_RECEIVER_KONTR_DC == value.DOC_CODE) return;
                    Entity.SF_RECEIVER_KONTR_DC = value.DOC_CODE;
                }
                RaisePropertyChanged();
            }
        }

        public string COName => CO?.Name;

        public CentrOfResponsibility CO
        {
            get
            {
                if (Entity.SF_CENTR_OTV_DC == null) return null;
                if (MainReferences.COList.ContainsKey(Entity.SF_CENTR_OTV_DC.Value))
                    return MainReferences.COList[Entity.SF_CENTR_OTV_DC.Value];
                return null;
            }
            set
            {
                if (value == null)
                {
                    Entity.SF_CENTR_OTV_DC = null;
                }
                else
                {
                    if (Entity.SF_CENTR_OTV_DC == value.DOC_CODE) return;
                    Entity.SF_CENTR_OTV_DC = value.DOC_CODE;
                }
                RaisePropertyChanged();
            }
        }

        public VzaimoraschetType VzaimoraschetType
        {
            get
            {
                if (Entity.SF_VZAIMOR_TYPE_DC == null) return null;
                if (MainReferences.VzaimoraschetTypes.ContainsKey(Entity.SF_VZAIMOR_TYPE_DC.Value))
                    return MainReferences.VzaimoraschetTypes[Entity.SF_VZAIMOR_TYPE_DC.Value];
                return null;
            }
            set
            {
                if (value == null)
                {
                    Entity.SF_VZAIMOR_TYPE_DC = null;
                }
                else
                {
                    if (Entity.SF_VZAIMOR_TYPE_DC == value.DOC_CODE) return;
                    Entity.SF_VZAIMOR_TYPE_DC = value.DOC_CODE;
                }
                RaisePropertyChanged();
            }
        }

        public SD_189ViewModel FormRaschet
        {
            get
            {
                if (Entity.SF_FORM_RASCH_DC == null) return null;
                if (MainReferences.FormRaschets.ContainsKey(Entity.SF_FORM_RASCH_DC.Value))
                    return MainReferences.FormRaschets[Entity.SF_FORM_RASCH_DC.Value];
                return null;
            }
            set
            {
                if (value == null)
                {
                    Entity.SF_FORM_RASCH_DC = null;
                }
                else
                {
                    if (Entity.SF_FORM_RASCH_DC == value.DOC_CODE) return;
                    Entity.SF_FORM_RASCH_DC = value.DOC_CODE;
                }
                RaisePropertyChanged();
            }
        }

        public PayCondition PayCondition
        {
            get
            {
                if (MainReferences.PayConditions.ContainsKey(Entity.SF_PAY_COND_DC))
                    return MainReferences.PayConditions[Entity.SF_PAY_COND_DC];
                return null;
            }
            set
            {
                if (Entity.SF_PAY_COND_DC == value?.DOC_CODE) return;
                if (value != null) Entity.SF_PAY_COND_DC = value.DOC_CODE;
                RaisePropertyChanged();
            }
        }

        private decimal mySummaOtgruz;
        public decimal SummaOtgruz
        {
            get => mySummaOtgruz;
            set
            {
                if (mySummaOtgruz == value) return;
                mySummaOtgruz = value;
                RaisePropertyChanged();
            }
        }

        public decimal Summa => SF_CRS_SUMMA_K_OPLATE ?? 0;


        public bool IsAccepted
        {
            get => SF_ACCEPTED == 1;
            set
            {
                if (SF_ACCEPTED == 1 == value) return;
                SF_ACCEPTED = (short) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }


        public Currency Currency
        {
            get => !MainReferences.Currencies.ContainsKey(Entity.SF_CRS_DC)
                ? null
                : MainReferences.Currencies[Entity.SF_CRS_DC];
            set
            {
                if (Entity.SF_CRS_DC == value.DOC_CODE) return;
                Entity.SF_CRS_DC = value.DOC_CODE;
                RaisePropertyChanged();
            }
        }

        public void RefreshData()
        {
            throw new NotImplementedException();
        }

        public new void Save()
        {
            //throw new NotImplementedException();
        }

        public bool IsCanSave { get; set; }

        public bool Check()
        {
            return false;
            //throw new NotImplementedException();
        }

        public SFClient NewDocument()
        {
            throw new NotImplementedException();
        }

        public SFClient CopyDocument()
        {
            throw new NotImplementedException();
        }

        public SFClient CopyRequisite()
        {
            throw new NotImplementedException();
        }

        public void UnDeleteRows()
        {
            throw new NotImplementedException();
        }

        public void DeleteRow(TD_84ViewModel row)
        {
            var r = Rows.FirstOrDefault(_ => _.CODE == row.CODE);
            if (r == null) return;
            if (r.State != RowStatus.NewRow)
                DeletedRows.Add(r);
            Rows.Remove(r);
            RaisePropertyChanged(nameof(Rows));
        }

        public void DeleteRow(int code)
        {
            var r = Rows.FirstOrDefault(_ => _.CODE == code);
            if (r == null) return;
            if (r.State != RowStatus.NewRow)
                DeletedRows.Add(r);
            Rows.Remove(r);
            RaisePropertyChanged(nameof(Rows));
        }
    }
}