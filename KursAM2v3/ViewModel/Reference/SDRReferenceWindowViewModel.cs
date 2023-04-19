using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference
{
    public sealed class SDRReferenceWindowViewModel : RSWindowViewModelBase
    {
        #region Constructors

        public SDRReferenceWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            RefreshData(null);
        }

        #endregion

        #region Fields

        private decimal newDC = -1;
        private decimal newDC2 = -1;

        #endregion

        #region Properties

        public ObservableCollection<SDRStateViewModel> SDRStateCollection { set; get; } =
            new ObservableCollection<SDRStateViewModel>();

        public ObservableCollection<SDRSchetViewModel> SDRSchetCollection { set; get; } =
            new ObservableCollection<SDRSchetViewModel>();

        public ObservableCollection<SDRSchetViewModel> SDRSchetCollectionSelection { set; get; } =
            new ObservableCollection<SDRSchetViewModel>();

        public ObservableCollection<SDRStateViewModel> SDRStateCollectionDeleted { set; get; } =
            new ObservableCollection<SDRStateViewModel>();

        public ObservableCollection<SDRSchetViewModel> SDRSchetCollectionDeleted { set; get; } =
            new ObservableCollection<SDRSchetViewModel>();

        private SDRStateViewModel myCurrentSDRState;

        public SDRStateViewModel CurrentSDRState
        {
            get => myCurrentSDRState;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentSDRState == value) return;
                myCurrentSDRState = value;
                LoadSchetsForCurrentState();
                RaisePropertyChanged();
            }
        }

        private SDRSchetViewModel myCurrentSDRSchet;

        public SDRSchetViewModel CurrentSDRSchet
        {
            get => myCurrentSDRSchet;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentSDRSchet == value) return;
                myCurrentSDRSchet = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override void RefreshData(object obj)
        {
            SDRStateCollection.Clear();
            SDRSchetCollection.Clear();
            SDRStateCollectionDeleted.Clear();
            SDRSchetCollectionDeleted.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var d in ctx.SD_99.ToList())
                    SDRStateCollection.Add(new SDRStateViewModel(d)
                    {
                        State = RowStatus.NotEdited
                    });
                foreach (var d in ctx.SD_303.ToList())
                    SDRSchetCollection.Add(new SDRSchetViewModel(d)
                    {
                        State = RowStatus.NotEdited
                    });
            }
        }

        public override bool IsCanSaveData => SDRStateCollection.Any(_ => _.State != RowStatus.NotEdited) ||
                                              SDRSchetCollection.Any(_ => _.State != RowStatus.NotEdited) ||
                                              SDRSchetCollectionDeleted.Count > 0 ||
                                              SDRStateCollectionDeleted.Count > 0;

        public ICommand AddNewSDRStateCommand
        {
            get { return new Command(AddNewSDRState, _ => true); }
        }

        private void AddNewSDRState(object obj)
        {
            var newItem = new SDRStateViewModel
            {
                DocCode = newDC,
                SZ_PARENT_DC = CurrentSDRState?.SZ_PARENT_DC,
                State = RowStatus.NewRow
            };
            SDRStateCollection.Add(newItem);
            CurrentSDRState = newItem;
            newDC--;
        }

        public ICommand AddNewChildSDRStateCommand
        {
            get
            {
                return new Command(AddNewChildSDRState,
                    _ => CurrentSDRState != null && CurrentSDRState.State != RowStatus.NewRow);
            }
        }

        private void AddNewChildSDRState(object obj)
        {
            var newItem = new SDRStateViewModel
            {
                DocCode = newDC,
                SZ_PARENT_DC = CurrentSDRState?.DocCode,
                State = RowStatus.NewRow
            };
            SDRStateCollection.Add(newItem);
            CurrentSDRState = newItem;
            newDC--;
        }

        public ICommand DeleteSDRStateCommand
        {
            get
            {
                return new Command(DeleteSDRState,
                    _ => CurrentSDRState != null && !IsHasChilds(CurrentSDRState));
            }
        }

        private void DeleteSDRState(object obj)
        {
            if (CurrentSDRState.State == RowStatus.NewRow)
            {
                SDRSchetCollection.Clear();
                SDRStateCollection.Remove(CurrentSDRState);
                return;
            }

            foreach (var r in SDRSchetCollection.Where(_ => _.SHPZ_STATIA_DC == CurrentSDRState.DocCode)) 
                SDRSchetCollectionDeleted.Add(r);
            SDRSchetCollection.Clear();
            SDRStateCollectionDeleted.Add(CurrentSDRState);
            SDRStateCollection.Remove(CurrentSDRState);
        }

        public ICommand MoveToTopDRStateCommand
        {
            get
            {
                return new Command(MoveToTopDRState, _ => CurrentSDRState != null && CurrentSDRState.ParentDC != null);
            }
        }

        private void MoveToTopDRState(object obj)
        {
            if (CurrentSDRState == null) return;
            var parent = SDRStateCollection.FirstOrDefault(_ => _.DocCode == CurrentSDRState.SZ_PARENT_DC);
            CurrentSDRState.SZ_PARENT_DC = parent?.SZ_PARENT_DC;
        }

        public ICommand AddNewItemCommand
        {
            get { return new Command(AddNewItem, _ => CurrentSDRState != null); }
        }

        private void AddNewItem(object obj)
        {
            var newItem = new SDRSchetViewModel
            {
                DocCode = newDC2,
                SHPZ_STATIA_DC = CurrentSDRState.DocCode,
                State = RowStatus.NewRow
            };
            SDRSchetCollection.Add(newItem);
            SDRSchetCollectionSelection.Add(newItem);
            CurrentSDRSchet = newItem;
            newDC2--;
        }

        public ICommand RemoveItemCommand
        {
            get { return new Command(RemoveItem, _ => CurrentSDRSchet != null); }
        }

        private void RemoveItem(object obj)
        {
            if (CurrentSDRSchet.State == RowStatus.NewRow)
            {
                SDRSchetCollection.Remove(CurrentSDRSchet);
                SDRSchetCollectionSelection.Remove(CurrentSDRSchet);
                return;
            }

            SDRSchetCollectionDeleted.Add(CurrentSDRSchet);
            SDRSchetCollectionSelection.Remove(CurrentSDRSchet);
            SDRSchetCollection.Remove(CurrentSDRSchet);
        }

        public override void SaveData(object data)
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var changedDC = new Dictionary<decimal, decimal>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var schet in SDRSchetCollectionDeleted)
                        {
                            var sdel = ctx.SD_303.FirstOrDefault(_ => _.DOC_CODE == schet.DocCode);
                            if (sdel != null)
                                ctx.SD_303.Remove(sdel);
                        }

                        foreach (var schet in SDRStateCollectionDeleted)
                        {
                            var sdel = ctx.SD_99.FirstOrDefault(_ => _.DOC_CODE == schet.DocCode);
                            if (sdel != null)
                                ctx.SD_99.Remove(sdel);
                        }

                        if (SDRStateCollection.Any(_ => _.State == RowStatus.NewRow))
                        {
                            var stateNewDC = ctx.SD_99.Any() ? ctx.SD_99.Max(_ => _.DOC_CODE) + 1 : 10990000001;
                            foreach (var item in SDRStateCollection.Where(_ => _.State == RowStatus.NewRow))
                            {
                                List<decimal> updateList = SDRSchetCollection
                                    .Where(_ => _.SHPZ_STATIA_DC == item.DocCode).Select(d => d.DocCode).ToList();
                                foreach (var u in updateList
                                             .Select(d => SDRSchetCollection.FirstOrDefault(_ => _.DocCode == d))
                                             .Where(u => u != null))
                                {
                                    u.SHPZ_STATIA_DC = stateNewDC;
                                }
                                item.DocCode = stateNewDC;
                                stateNewDC++;
                            }
                        }

                        foreach (var item in SDRStateCollection.Where(_ => _.State != RowStatus.NotEdited))
                            switch (item.State)
                            {
                                case RowStatus.NewRow:
                                    
                                    var newItem = new SD_99
                                    {
                                        DOC_CODE = item.DocCode,
                                        SZ_NAME = item.Name,
                                        SZ_SHIFR = item.SZ_SHIFR,
                                        SZ_PARENT_DC = item.SZ_PARENT_DC,
                                        SZ_1DOHOD_0_RASHOD = item.SZ_1DOHOD_0_RASHOD
                                    };
                                    ctx.SD_99.Add(newItem);
                                    break;
                                case RowStatus.Edited:
                                    var editItem = ctx.SD_99.FirstOrDefault(_ => _.DOC_CODE == item.DocCode);
                                    if (editItem != null)
                                    {
                                        editItem.SZ_NAME = item.Name;
                                        editItem.SZ_SHIFR = item.SZ_SHIFR;
                                        editItem.SZ_PARENT_DC = item.SZ_PARENT_DC;
                                        editItem.SZ_1DOHOD_0_RASHOD = item.SZ_1DOHOD_0_RASHOD;
                                    }

                                    break;
                            }

                        var newSchetDC = ctx.SD_303.Any() ? ctx.SD_303.Max(_ => _.DOC_CODE) + 1 : 13030000001;
                        foreach (var sch in SDRSchetCollection)
                        {
                            switch (sch.State)
                            {
                                case RowStatus.NewRow:
                                    //decimal newStateDC;
                                    //if (sch.SHPZ_STATIA_DC < 0)
                                    //    newStateDC = changedDC[sch.DocCode];
                                    //else
                                    //    // ReSharper disable once PossibleInvalidOperationException
                                    //    newStateDC = sch.SHPZ_STATIA_DC.Value;
                                    ctx.SD_303.Add(new SD_303
                                    {
                                        DOC_CODE = newSchetDC,
                                        SHPZ_NAME = sch.Name,
                                        SHPZ_DELETED = sch.Entity.SHPZ_DELETED,
                                        OP_CODE = sch.Entity.OP_CODE,
                                        SHPZ_SHIRF = sch.Entity.SHPZ_SHIRF,
                                        SHPZ_1OSN_0NO = sch.Entity.SHPZ_1OSN_0NO,
                                        SHPZ_STATIA_DC = sch.Entity.SHPZ_STATIA_DC,
                                        SHPZ_ELEMENT_DC = sch.Entity.SHPZ_ELEMENT_DC,
                                        SHPZ_PODOTCHET = sch.Entity.SHPZ_PODOTCHET,
                                        SHPZ_1DOHOD_0_RASHOD = sch.Entity.SHPZ_1DOHOD_0_RASHOD,
                                        SHPZ_1TARIFIC_0NO = sch.Entity.SHPZ_1TARIFIC_0NO,
                                        SHPZ_1ZARPLATA_0NO = sch.Entity.SHPZ_1ZARPLATA_0NO,
                                        SHPZ_NOT_USE_IN_OTCH_DDS = sch.Entity.SHPZ_NOT_USE_IN_OTCH_DDS
                                    });
                                    newSchetDC++;
                                    break;
                                case RowStatus.Edited:
                                    var edSch = ctx.SD_303.FirstOrDefault(_ => _.DOC_CODE == sch.DocCode);
                                    if (edSch == null) continue;
                                    edSch.SHPZ_NAME = sch.Name;
                                    edSch.SHPZ_SHIRF = sch.SHPZ_SHIRF;
                                    break;
                            }
                        }
                        ctx.SaveChanges();
                        tnx.Commit();
                        RefreshData(null);
                    }
                    catch (Exception ex)
                    {
                        tnx.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void LoadSchetsForCurrentState()
        {
            SDRSchetCollectionSelection.Clear();
            if(CurrentSDRState == null) return;
            foreach (var s in SDRSchetCollection.Where(_ => _.SHPZ_STATIA_DC == CurrentSDRState.DocCode))
            {
                SDRSchetCollectionSelection.Add(s);
            }
        }
        private bool IsHasChilds(SDRStateViewModel sdr)
        {
            return SDRStateCollection.Any(_ => _.SZ_PARENT_DC == sdr.DocCode) 
                   || SDRSchetCollection.Any(_ => _.SHPZ_STATIA_DC == sdr.DocCode);
        }

        #endregion
    }
}
