using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Windows.Input;
using Core;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Reference
{
    public class StoreReferenceWindowViewModel : RSWindowViewModelBase
    {
        private readonly StandartErrorManager errorManager;
        private Warehouse myCurrentWarehouse;

        public StoreReferenceWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            errorManager = new StandartErrorManager(GlobalOptions.GetEntities(), Name, true);
            RefreshData(null);
        }

        public override string LayoutName => "StoreReferenceWindowViewModel";
        public ObservableCollection<Warehouse> StoreCollection { set; get; } = new ObservableCollection<Warehouse>();
        public override bool IsCanSaveData => StoreCollection.Any(_ => _.State != RowStatus.NotEdited);
        public ObservableCollection<SelectUser> SelectedUsers { set; get; } = new ObservableCollection<SelectUser>();

        public ICommand AddNewStore
        {
            get { return new Command(addNewStore, _ => true); }
        }

        public Warehouse CurrentWarehouse
        {
            get => myCurrentWarehouse;
            set
            {
                if (myCurrentWarehouse != null && myCurrentWarehouse.Equals(value)) return;
                myCurrentWarehouse = value;
                RaisePropertyChanged();
            }
        }

        public sealed override void RefreshData(object obj)
        {
            StoreCollection.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var s in ctx.SD_27.Include(_ => _.SD_23).ToList())
                        StoreCollection.Add(new Warehouse(s) {State = RowStatus.NotEdited});
                }
            }
            catch (Exception ex)
            {
                errorManager.WriteErrorMessage(ex);
            }
        }

        // ReSharper disable once InconsistentNaming
        private void addNewStore(object obj)
        {
            var newStore = new Warehouse
            {
                IsDeleted = false,
                IsCanNegativeRest = false,
                State = RowStatus.NewRow
            };
            StoreCollection.Add(newStore);
            CurrentWarehouse = newStore;
            RaisePropertyChanged(nameof(CurrentWarehouse));
        }

        public override void SaveData(object data)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = new TransactionScope())
                {
                    try
                    {
                        foreach (var r in StoreCollection.Where(_ => _.State != RowStatus.NotEdited))
                            switch (r.State)
                            {
                                case RowStatus.Edited:
                                    var ed = ctx.SD_27.FirstOrDefault(_ => _.DOC_CODE == r.DocCode);
                                    if (ed == null) continue;
                                    ed.SKL_NEGATIVE_REST = r.SKL_NEGATIVE_REST;
                                    ed.IsDeleted = r.IsDeleted;
                                    ed.SKL_NAME = r.Name;
                                    break;
                                case RowStatus.NewRow:
                                    var newDC = ctx.SD_27.Max(_ => _.DOC_CODE) + 1;
                                    ctx.SD_27.Add(new SD_27
                                        {
                                            DOC_CODE = newDC,
                                            SKL_NAME = r.Name,
                                            SKL_NEGATIVE_REST = r.SKL_NEGATIVE_REST,
                                            IsDeleted = r.IsDeleted
                                        }
                                    );
                                    break;
                            }

                        ctx.SaveChanges();
                        tnx.Complete();
                        foreach (var r in StoreCollection)
                            r.State = RowStatus.NotEdited;
                    }
                    catch (Exception ex)
                    {
                        errorManager.WriteErrorMessage(ex);
                    }
                }
            }
        }
    }
}
