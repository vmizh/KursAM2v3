using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference.Kontragent
{
    public class KontragentCategoryRefWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private StandartErrorManager errorManager;

        #endregion

        #region Constructors

        public KontragentCategoryRefWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #endregion

        #region Properties

        public ObservableCollection<KontragentClientCategory> CategoryCollection { set; get; } =
            new ObservableCollection<KontragentClientCategory>();

        public ObservableCollection<KontragentClientCategory> CategoryDeleteCollection { set; get; } =
            new ObservableCollection<KontragentClientCategory>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<KontragentClientCategory> SelectedCategories { set; get; } =
            new ObservableCollection<KontragentClientCategory>();

        private KontragentClientCategory myCurrentClientCategory;

        public KontragentClientCategory CurrentClientCategory
        {
            get => myCurrentClientCategory;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentClientCategory == value) return;
                myCurrentClientCategory = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override bool IsCanSaveData => CategoryCollection.Any(_ => _.State != RowStatus.NotEdited)
                                              || CategoryDeleteCollection.Count > 0;

        public override void RefreshData(object obj)
        {
            var WinManager = new WindowManager();
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("Были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                    try
                    {
                        SaveData(null);
                    }
                    catch (Exception ex)
                    {
                        WinManager.ShowWinUIMessageBox(ex.Message, "Ошибка");
                    }
            }

            CategoryCollection.Clear();
            CategoryDeleteCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var item in ctx.SD_148.ToList())
                    CategoryCollection.Add(new KontragentClientCategory(item) {myState = RowStatus.NotEdited});
            }
        }

        public override void SaveData(object data)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                errorManager = new StandartErrorManager(ctx, "KontragentCategoryRefWindow", true);
                using (var tnx = new TransactionScope())
                {
                    try
                    {
                        foreach (var d in CategoryDeleteCollection)
                        {
                            var old = ctx.SD_148.FirstOrDefault(_ => _.DOC_CODE == d.DocCode);
                            if (old != null) ctx.SD_148.Remove(old);
                        }

                        var itemsDC = new List<decimal>();
                        var newDC = ctx.SD_148.Any() ? ctx.SD_148.Max(_ => _.DOC_CODE) : 1480000001;
                        foreach (var r in CategoryCollection.Where(_ => _.State != RowStatus.NotEdited))
                            switch (r.State)
                            {
                                case RowStatus.Edited:
                                    var ed = ctx.SD_148.FirstOrDefault(_ => _.DOC_CODE == r.DocCode);
                                    if (ed == null) continue;
                                    ed.CK_NAME = r.Name;
                                    ed.CK_CREDIT_SUM = r.CK_CREDIT_SUM;
                                    ed.CK_GROUP = r.CK_GROUP;
                                    ed.CK_IMMEDIATE_PRICE_CHANGE = r.CK_IMMEDIATE_PRICE_CHANGE;
                                    ed.CK_MAX_PROSROCH_DNEY = r.CK_MAX_PROSROCH_DNEY;
                                    ed.CK_MIN_OBOROT = r.CK_MIN_OBOROT;
                                    ed.CK_NACEN_DEFAULT_KOMPL = r.CK_NACEN_DEFAULT_KOMPL;
                                    ed.CK_NACEN_DEFAULT_ROZN = r.CK_NACEN_DEFAULT_ROZN;
                                    itemsDC.Add(r.DocCode);
                                    break;
                                case RowStatus.NewRow:
                                    newDC++;
                                    ctx.SD_148.Add(new SD_148
                                        {
                                            DOC_CODE = newDC,
                                            CK_NAME = r.Name,
                                            CK_CREDIT_SUM = r.CK_CREDIT_SUM,
                                            CK_GROUP = r.CK_GROUP,
                                            CK_IMMEDIATE_PRICE_CHANGE = r.CK_IMMEDIATE_PRICE_CHANGE,
                                            CK_MAX_PROSROCH_DNEY = r.CK_MAX_PROSROCH_DNEY,
                                            CK_MIN_OBOROT = r.CK_MIN_OBOROT,
                                            CK_NACEN_DEFAULT_KOMPL = r.CK_NACEN_DEFAULT_KOMPL,
                                            CK_NACEN_DEFAULT_ROZN = r.CK_NACEN_DEFAULT_ROZN
                                        }
                                    );
                                    itemsDC.Add(newDC);
                                    break;
                            }

                        ctx.SaveChanges();
                        tnx.Complete();
                        foreach (var dc in itemsDC)
                        {
                            var ent = ctx.SD_148.FirstOrDefault(_ => _.DOC_CODE == dc);
                            if (ent == null) continue;
                            var caheItem = new ClientCategory();
                            caheItem.LoadFromEntity(ent);
                            GlobalOptions.ReferencesCache.AddOrUpdate(caheItem);
                        }
                        
                        foreach (var r in CategoryCollection)
                            r.myState = RowStatus.NotEdited;
                        CategoryDeleteCollection.Clear();
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(null, ex);
                        errorManager.WriteErrorMessage(ex);
                        return;
                    }
                }
            }

            RefreshData(null);
        }

        public ICommand AddNewItemCommand
        {
            get { return new Command(AddNewItem, _ => true); }
        }

        private void AddNewItem(object obj)
        {
            var newRow = new KontragentClientCategory
            {
                State = RowStatus.NewRow,
                Id = Guid.NewGuid()
            };
            CategoryCollection.Add(newRow);
            CurrentClientCategory = newRow;
        }

        public ICommand RemoveItemCommand
        {
            get { return new Command(RemoveItem, _ => true); }
        }

        private void RemoveItem(object obj)
        {
            var dList = new List<KontragentClientCategory>();
            foreach (var d in SelectedCategories)
                if (d.State == RowStatus.NewRow)
                    dList.Add(d);
            foreach (var r in dList) CategoryCollection.Remove(r);
            var drows = new ObservableCollection<KontragentClientCategory>(SelectedCategories);
            foreach (var d in drows)
            {
                CategoryCollection.Remove(d);
                CategoryDeleteCollection.Add(d);
            }
        }

        public override void CloseWindow(object form)
        {
            var WinManager = new WindowManager();
            var vin = form as Window;
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("Были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (res == MessageBoxResult.Cancel) return;
                if (res == MessageBoxResult.No)
                    vin?.Close();
                if (res != MessageBoxResult.Yes) return;
                try
                {
                    SaveData(null);
                    vin?.Close();
                }
                catch (Exception ex)
                {
                    WinManager.ShowWinUIMessageBox(ex.Message, "Ошибка");
                }
            }
            else
            {
                vin?.Close();
            }
        }

        #endregion
    }
}
