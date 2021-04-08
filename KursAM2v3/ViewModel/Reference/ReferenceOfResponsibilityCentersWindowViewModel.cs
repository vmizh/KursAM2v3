using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using KursAM2.Dialogs;
using KursAM2.View.KursReferences;

namespace KursAM2.ViewModel.Reference
{
    public class ReferenceOfResponsibilityCentersWindowViewModel : RSWindowViewModelBase
    {
        private SD_40ViewModel myCurrentCenter;
        
        public ReferenceOfResponsibilityCentersWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        public ReferenceOfResponsibilityCentersWindowViewModel(Window win) : this()
        {
            Form = win;
        }

        public ObservableCollection<SD_40ViewModel> CenterCollection { set; get; } =
            new ObservableCollection<SD_40ViewModel>();

        public ObservableCollection<SD_40ViewModel> DeletedCenterCollection { set; get; } =
            new ObservableCollection<SD_40ViewModel>();


        public SD_40ViewModel CurrentCenter
        {
            get => myCurrentCenter;
            set
            {
                if (myCurrentCenter != null && myCurrentCenter.Equals(value)) return;
                myCurrentCenter = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsCanSaveData => IsCanSave();

        public override void RefreshData(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    CenterCollection.Clear();
                    foreach (var p in ctx.SD_40.ToList())
                        CenterCollection.Add(new SD_40ViewModel(p) {State = RowStatus.NotEdited});
                    if (CenterCollection.Count == 0)
                    {
                        var newRow = new SD_40ViewModel()
                        {
                            DOC_CODE = 1,
                            CENT_FULLNAME = "Новый центр",
                            CENT_NAME = "Новый центр",
                            CENT_PARENT_DC = 2,
                            IS_DELETED = 0,
                            State = RowStatus.NotEdited
                        };
                        CenterCollection.Add(newRow);
                        CurrentCenter = newRow;
                    }
                }

                RaisePropertiesChanged(nameof(CenterCollection));
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void SaveData(object data)
        {
            try
            {
                foreach (var p in DeletedCenterCollection)
                    p.Delete();
                foreach (var p in CenterCollection.Where(_ => _.State != RowStatus.NotEdited))
                    p.Save();
                DeletedCenterCollection.Clear();
                foreach (var p in CenterCollection.Where(_ => _.State != RowStatus.NotEdited))
                    p.myState = RowStatus.NotEdited;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        private new bool IsCanSave()
        {
            if (CenterCollection.Any(_ => _.State != RowStatus.NotEdited) ||
                DeletedCenterCollection.Count > 0)
                return CenterCollection.All(p => p.Check());
            return false;
        }

       
        #region Commands

        public ICommand MoveToTopCenterCommand
        {
            get { return new Command(MoveToTopProject, _ => CurrentCenter?.DOC_CODE != null); }
        }

        private void MoveToTopProject(object obj)
        {
            CurrentCenter.DOC_CODE = new decimal(null);
            using (var ctx = GlobalOptions.GetEntities())
            {
                var tx = ctx.Database.BeginTransaction();
                try
                {
                    var centr = ctx.SD_40.FirstOrDefault(_ => _.CENT_PARENT_DC == CurrentCenter.CENT_PARENT_DC);
                    {
                        if (centr != null) centr.DOC_CODE = new decimal(null);
                    }
                    ctx.SaveChanges();
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    WindowManager.ShowError(ex);
                }
            }
        }

        public ICommand AddNewCenterCommand
        {
            get { return new Command(AddNewCenter, _ => CurrentCenter != null); }
        }

        private void AddNewCenter(object obj)
        {
            var newRow = new SD_40ViewModel()
            {
                State = RowStatus.NewRow,
                CENT_NAME = "Новый центр",
                DOC_CODE = CurrentCenter.DOC_CODE, // Где брать  doc-code для нового центра?
                IS_DELETED = 0
            };
            CenterCollection.Add(newRow);
            CurrentCenter = newRow;
        }

        private bool IsCanAddCenter()
        {
            var pitem = CenterCollection.FirstOrDefault(_ => _.CENT_PARENT_DC == CurrentCenter.DOC_CODE);
            if (pitem == null)
                return true;
            var pitem2 = CenterCollection.FirstOrDefault(_ => _.DOC_CODE == pitem.CENT_PARENT_DC);
            if (pitem2 == null)
                return true;
            var pitem3 = CenterCollection.FirstOrDefault(_ => _.CENT_PARENT_DC == pitem2.DOC_CODE);
            if (pitem3 == null)
                return true;
            var pitem4 = CenterCollection.FirstOrDefault(_ => _.CENT_PARENT_DC == pitem3.DOC_CODE);
            if (pitem4 != null) 
                return false;
            return false;
        }

        public ICommand AddNewChildCenterCommand
        {
            get { return new Command(AddNewChildCenter, _ => IsCanAddCenter() && CurrentCenter != null); }
        }

        private void AddNewChildCenter(object obj)
        {
            var newRow = new SD_40ViewModel()
            {
                State = RowStatus.NewRow,
                CENT_NAME = "Новый центр",
                CENT_PARENT_DC = CurrentCenter.DOC_CODE,
                IS_DELETED = 0
            };
            CenterCollection.Add(newRow);
            if (Form is ReferenceOfResponsibilityCentersView win)
                win.treeListView.FocusedNode.IsExpanded = true;
            CurrentCenter = newRow;
        }

        public ICommand DeleteCenterCommand
        {
            get
            {
                return new Command(DeleteCenter,
                    _ => CurrentCenter != null && CenterCollection.All(p => p.DOC_CODE != CurrentCenter.CENT_PARENT_DC));
            }
        }

        private void DeleteCenter(object obj)
        {

            if (CurrentCenter.State != RowStatus.NewRow)
                DeletedCenterCollection.Add(CurrentCenter);
            CenterCollection.Remove(CurrentCenter);
        }


        #endregion
    }
}