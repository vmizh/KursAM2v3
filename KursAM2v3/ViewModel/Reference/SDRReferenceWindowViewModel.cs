using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;

namespace KursAM2.ViewModel.Reference
{
    public class SDRReferenceWindowViewModel : RSWindowViewModelBase
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

        public ObservableCollection<SDRState> SDRStateCollection { set; get; } =
            new ObservableCollection<SDRState>();

        public ObservableCollection<SDRSchet> SDRSchetCollection { set; get; } =
            new ObservableCollection<SDRSchet>();

        public ObservableCollection<SDRState> SDRStateCollectionDeleted { set; get; } =
            new ObservableCollection<SDRState>();

        public ObservableCollection<SDRSchet> SDRSchetCollectionDeleted { set; get; } =
            new ObservableCollection<SDRSchet>();

        public ObservableCollection<SDRSchet> SDRSchetCollectionAdded { set; get; } =
            new ObservableCollection<SDRSchet>();

        public ObservableCollection<SDRSchet> SDRSchetCollectionEdited { set; get; } =
            new ObservableCollection<SDRSchet>();

        private SDRState myCurrentSDRSTate;

        public SDRState CurrentSDRSTate
        {
            get => myCurrentSDRSTate;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentSDRSTate == value) return;
                myCurrentSDRSTate = value;
                LoadSchetsForCurrentState();
                RaisePropertyChanged();
            }
        }

        private SDRSchet myCurrentSDRSchet;

        public SDRSchet CurrentSDRSchet
        {
            get => myCurrentSDRSchet;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentSDRSchet == value) return;
                if (myCurrentSDRSchet != null)
                    if (myCurrentSDRSchet.State == RowStatus.Edited)
                    {
                        var old = SDRSchetCollectionEdited.FirstOrDefault(_ => _.DocCode == myCurrentSDRSchet.DocCode);
                        if (old != null)
                        {
                            SDRSchetCollectionEdited.Remove(old);
                            SDRSchetCollectionEdited.Add(myCurrentSDRSchet);
                        }
                    }

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
            SDRSchetCollectionAdded.Clear();
            SDRSchetCollectionEdited.Clear();
            SDRStateCollectionDeleted.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var d in ctx.SD_99.ToList())
                    SDRStateCollection.Add(new SDRState(d)
                    {
                        State = RowStatus.NotEdited
                    });
            }
        }

        public override bool IsCanSaveData => SDRStateCollection.Any(_ => _.State != RowStatus.NotEdited) ||
                                              SDRSchetCollection.Any(_ => _.State != RowStatus.NotEdited) ||
                                              SDRSchetCollectionDeleted.Count > 0 ||
                                              SDRStateCollectionDeleted.Count > 0 ||
                                              SDRSchetCollectionAdded.Count > 0 ||
                                              SDRSchetCollectionEdited.Count > 0;

        public ICommand AddNewSDRStateCommand
        {
            get { return new Command(AddNewSDRState, _ => true); }
        }

        private void AddNewSDRState(object obj)
        {
            var newItem = new SDRState
            {
                DocCode = newDC,
                SZ_PARENT_DC = CurrentSDRSTate?.SZ_PARENT_DC,
                State = RowStatus.NewRow
            };
            SDRStateCollection.Add(newItem);
            CurrentSDRSTate = newItem;
            newDC--;
        }

        public ICommand AddNewChildSDRStateCommand
        {
            get { return new Command(AddNewChildSDRState, _ => CurrentSDRSTate != null && CurrentSDRSTate.State != RowStatus.NewRow); }
        }

        private void AddNewChildSDRState(object obj)
        {
            var newItem = new SDRState
            {
                DocCode = newDC,
                SZ_PARENT_DC = CurrentSDRSTate?.DocCode,
                State = RowStatus.NewRow
            };
            SDRStateCollection.Add(newItem);
            CurrentSDRSTate = newItem;
            newDC--;
        }

        public ICommand DeleteSDRStateCommand
        {
            get { return new Command(DeleteSDRState, _ => CurrentSDRSTate != null && !IsHasChilds(CurrentSDRSTate)); }
        }

        private void DeleteSDRState(object obj)
        {
            if (CurrentSDRSTate.State == RowStatus.NewRow)
            {
                SDRSchetCollection.Clear();
                SDRStateCollection.Remove(CurrentSDRSTate);
                return;
            }

            foreach (var r in SDRSchetCollection) SDRSchetCollectionDeleted.Add(r);
            SDRSchetCollection.Clear();
            SDRStateCollectionDeleted.Add(CurrentSDRSTate);
            SDRStateCollection.Remove(CurrentSDRSTate);
        }

        public ICommand MoveToTopDRStateCommand
        {
            get { return new Command(MoveToTopDRState, _ => CurrentSDRSTate != null && CurrentSDRSTate.ParentDC != null); }
        }

        private void MoveToTopDRState(object obj)
        {
            if (CurrentSDRSTate == null) return;
            var parent = SDRStateCollection.FirstOrDefault(_ => _.DocCode == CurrentSDRSTate.SZ_PARENT_DC);
            CurrentSDRSTate.SZ_PARENT_DC = parent?.SZ_PARENT_DC;
        }

        public ICommand AddNewItemCommand
        {
            get { return new Command(AddNewItem, _ => CurrentSDRSTate != null); }
        }

        private void AddNewItem(object obj)
        {
            var newItem = new SDRSchet
            {
                DocCode = newDC2,
                SHPZ_STATIA_DC = CurrentSDRSTate.DocCode,
                State = RowStatus.NewRow
            };
            SDRSchetCollectionAdded.Add(newItem);
            SDRSchetCollection.Add(newItem);
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
                SDRSchetCollectionAdded.Remove(CurrentSDRSchet);
                SDRSchetCollection.Remove(CurrentSDRSchet);
                return;
            }

            SDRSchetCollectionDeleted.Add(CurrentSDRSchet);
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

                        foreach (var item in SDRStateCollection.Where(_ => _.State != RowStatus.NotEdited))
                            switch (item.State)
                            {
                                case RowStatus.NewRow:
                                    var stateNewDC = ctx.SD_99.Any() ? ctx.SD_99.Max(_ => _.DOC_CODE) + 1 : 10990000001;
                                    //foreach (var s in SDRSchetCollectionAdded
                                    //    .Where(_ => _.SHPZ_STATIA_DC == item.DocCode)
                                    //    .OrderBy(_ => _.DocCode)) changedDC.Add(item.DocCode, stateNewDC)
                                    var newItem = new SD_99
                                    {
                                        DOC_CODE = stateNewDC,
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
                        foreach (var sch in SDRSchetCollectionAdded)
                        {
                            decimal newStateDC;
                            if (sch.SHPZ_STATIA_DC < 0)
                                newStateDC = changedDC[sch.DocCode];
                            else
                                newStateDC = sch.SHPZ_STATIA_DC.Value;
                            ctx.SD_303.Add(new SD_303
                            {
                                DOC_CODE = newSchetDC,
                                SHPZ_NAME = sch.Name,
                                SHPZ_DELETED = sch.Entity.SHPZ_DELETED,
                                OP_CODE = sch.Entity.OP_CODE,
                                SHPZ_SHIRF = sch.Entity.SHPZ_SHIRF,
                                SHPZ_1OSN_0NO = sch.Entity.SHPZ_1OSN_0NO,
                                SHPZ_STATIA_DC = newStateDC,
                                SHPZ_ELEMENT_DC = sch.Entity.SHPZ_ELEMENT_DC,
                                SHPZ_PODOTCHET = sch.Entity.SHPZ_PODOTCHET,
                                SHPZ_1DOHOD_0_RASHOD = sch.Entity.SHPZ_1DOHOD_0_RASHOD,
                                SHPZ_1TARIFIC_0NO = sch.Entity.SHPZ_1TARIFIC_0NO,
                                SHPZ_1ZARPLATA_0NO = sch.Entity.SHPZ_1ZARPLATA_0NO,
                                SHPZ_NOT_USE_IN_OTCH_DDS = sch.Entity.SHPZ_NOT_USE_IN_OTCH_DDS
                            });
                            newSchetDC++;
                        }

                        //foreach (var sch in SDRSchetCollection.Where(_ => _.State == RowStatus.Edited))
                        //{
                        //    var edSch = ctx.SD_303.FirstOrDefault(_ => _.DOC_CODE == sch.DocCode);
                        //    if (edSch == null) continue;
                        //}
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
            SDRSchetCollection.Clear();
            if (CurrentSDRSTate == null)
                return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var d in ctx.SD_303.Where(_ => _.SHPZ_STATIA_DC == CurrentSDRSTate.DocCode).ToList())
                    SDRSchetCollection.Add(new SDRSchet(d)
                    {
                        State = RowStatus.NotEdited
                    });
                foreach (var d in SDRSchetCollectionAdded.Where(_ => _.SHPZ_STATIA_DC == CurrentSDRSTate.DocCode)
                    .ToList()) SDRSchetCollection.Add(d);
            }
        }

        private bool IsHasChilds(SDRState sdr)
        {
            return SDRStateCollection.Any(_ => _.SZ_PARENT_DC == sdr.DocCode);
        }
        
        #endregion
    }
}