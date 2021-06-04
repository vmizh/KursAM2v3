using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Data;
using KursAM2.ViewModel.Costing;
using Reports.Base;

namespace KursAM2.ViewModel.Finance
{
    public sealed class KontragentReferenceOutWindowViewModel : RSWindowViewModelBase
    {
        private readonly List<KontragentRefOut> Deleted = new List<KontragentRefOut>();
        private KontragentRefOut myCurrentKontr;
        private KONTRAGENT_REF_OUT_REQUISITEViewModel myCurrentRequisite;

        public KontragentReferenceOutWindowViewModel()
        {
            WindowName = "Справочник контрагентов для печати";
            Result = new ObservableCollection<KontragentRefOut>();
        }

        public KontragentReferenceOutWindowViewModel(Window form) : base(form)
        {
            WindowName = "Справочник контрагентов для печати";
            Result = new ObservableCollection<KontragentRefOut>();
        }

        public KONTRAGENT_REF_OUT_REQUISITEViewModel CurrentRequisite
        {
            get => myCurrentRequisite;
            set
            {
                if (myCurrentRequisite != null && myCurrentRequisite.Equals(value)) return;
                myCurrentRequisite = value;
                RaisePropertyChanged();
            }
        }

        public KontragentRefOut CurrentKontr
        {
            get => myCurrentKontr;
            set
            {
                if (myCurrentKontr != null && myCurrentKontr.Equals(value)) return;
                myCurrentKontr = value;
                if (myCurrentKontr != null)
                {
                    IsDocNewCopyAllow = true;
                    IsDocNewCopyRequisiteAllow = true;
                    IsPrintAllow = true;
                }

                RaisePropertyChanged();
            }
        }

        public ObservableCollection<KontragentRefOut> Result { set; get; }

        public override void Print(object form)
        {
            var rep = new ExportView();
            rep.Show();
        }

        #region Commands

        public ICommand AddNewRequisiteCommand
        {
            get { return new Command(AddNewRequisite, param => true); }
        }

        public void AddNewRequisite(object form)
        {
            var newReq = new KontragentRefOutRow
            {
                Entity = new KONTRAGENT_REF_OUT_REQUISITE
                {
                    Id = Guid.NewGuid(),
                    KontrId = CurrentKontr.Id,
                    OKPO = CurrentKontr.OKPO
                }
            };
            CurrentKontr.Requisite.Add(newReq);
            CurrentRequisite = newReq;
            RaisePropertyChanged(nameof(CurrentRequisite));
        }

        public override void RefreshData(object data)
        {
            base.RefreshData(data);
            IsDocNewCopyAllow = false;
            IsDocNewCopyRequisiteAllow = false;
            IsPrintAllow = false;
            Result.Clear();
            foreach (var item in GlobalOptions.GetEntities()
                .KONTRAGENT_REF_OUT
                .Include(_ => _.KONTRAGENT_REF_OUT_REQUISITE)
                .ToList())
                Result.Add(new KontragentRefOut(item));
            RaisePropertyChanged(nameof(Result));
        }

        public override void DocNewEmpty(object form)
        {
            Result.Add(new KontragentRefOut {State = RowStatus.NewRow});
            RaisePropertyChanged(nameof(Result));
        }

        public override bool IsDocNewCopyAllow
        {
            get => CurrentKontr != null;
            set => base.IsDocNewCopyAllow = value;
        }

        public override void DocNewCopy(object form)
        {
            Result.Add(new KontragentRefOut(CurrentKontr.Entity) {State = RowStatus.NewRow});
            RaisePropertyChanged(nameof(Result));
        }

        public Command CopyFromReferenceCommand
        {
            get { return new Command(CopyFromReference, param => true); }
        }

        private void CopyFromReference(object obj)
        {
        }

        public override bool IsDocDeleteAllow
        {
            get => CurrentKontr != null;
            set => base.IsDocDeleteAllow = value;
        }

        public override void DocDelete(object form)
        {
            if (CurrentKontr.State != RowStatus.NewRow)
                Deleted.Add(CurrentKontr);
            Result.Remove(CurrentKontr);
            RaisePropertyChanged(nameof(Result));
        }

        public Command RowsUndeleteCommand
        {
            get { return new Command(RowsUndelete, param => IsDeletedExists); }
        }

        public bool IsDeletedExists => Deleted.Count > 0;

        private void RowsUndelete(object obj)
        {
            foreach (var item in Deleted)
                Result.Add(item);
            Deleted.Clear();
            RaisePropertyChanged(nameof(Result));
        }

        #endregion
    }
}