using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Xpf;
using Helper;
using KursAM2.Managers.Nomenkl;
using KursAM2.View.DialogUserControl.ViewModel;
using KursAM2.View.KursReferences;

namespace KursAM2.ViewModel.Reference.Nomenkl
{
    public sealed class ReferenceWindowViewModel : RSWindowViewModelBase
    {
        private NomenklGroup myCurrentCategory;
        private Core.EntityViewModel.Nomenkl myCurrentNomenkl;
        private NomenklMainViewModel myCurrentNomenklMain;
        private bool myIsCanChangeCurrency;
        private bool myIsCategoryEnabled;

        public ReferenceWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            LoadReferences();
            RefreshData(null);
            IsCategoryEnabled = true;
        }

        private IDialogService DialogService => GetService<IDialogService>();
        private IMessageBoxService MessageBoxService => GetService<IMessageBoxService>();

        public ObservableCollection<NomenklGroup> CategoryCollection { set; get; } =
            new ObservableCollection<NomenklGroup>();

        // ReSharper disable once CollectionNeverUpdated.Global
        public ObservableCollection<NomenklMainViewModel> NomenklMainCollection { set; get; } =
            new ObservableCollection<NomenklMainViewModel>();

        public ObservableCollection<Currency> CurrencyCollection { set; get; }

        //public bool IsSearchTextNull => 
        public bool IsCanChangeCurrency
        {
            get => myIsCanChangeCurrency;
            set
            {
                if (myIsCanChangeCurrency == value) return;
                myIsCanChangeCurrency = value;
                RaisePropertyChanged();
            }
        }

        public ICommand FocusedRowChangedCommand
        {
            get { return new Command(FocusedRowChanged, _ => true); }
        }

        public Core.EntityViewModel.Nomenkl CurrentNomenkl
        {
            get => myCurrentNomenkl;
            set
            {
                //SaveData(CurrentNomenklMain.NomenklCollection);
                if (myCurrentNomenkl != null && myCurrentNomenkl.Equals(value)) return;
                myCurrentNomenkl = value;
                if (myCurrentNomenkl != null)
                    if (myCurrentNomenkl.State == RowStatus.NewRow)
                        IsCanChangeCurrency = false;
                    else
                        try
                        {
                            IsCanChangeCurrency = true;
                            using (var ctx = GlobalOptions.GetEntities())
                            {
                                if (ctx.TD_24.Any(_ => _.DDT_NOMENKL_DC == myCurrentNomenkl.DocCode))
                                {
                                    IsCanChangeCurrency = false;
                                }
                                else
                                {
                                    if (ctx.TD_26.Any(_ => _.SFT_NEMENKL_DC == myCurrentNomenkl.DocCode))
                                    {
                                        IsCanChangeCurrency = false;
                                    }
                                    else
                                    {
                                        if (ctx.TD_84.Any(_ => _.SFT_NEMENKL_DC == myCurrentNomenkl.DocCode))
                                            IsCanChangeCurrency = false;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WindowManager.ShowError(ex);
                        }

                RaisePropertyChanged();
            }
        }

        public NomenklMainViewModel CurrentNomenklMain
        {
            get => myCurrentNomenklMain;
            set
            {
                if (myCurrentNomenklMain != null && myCurrentNomenklMain.Equals(value)) return;
                myCurrentNomenklMain = value;
                if (myCurrentNomenklMain != null)
                    LoadNomenklForMain(myCurrentNomenklMain);
                RaisePropertyChanged();
            }
        }

        public NomenklGroup CurrentCategory
        {
            get => myCurrentCategory;
            set
            {
                if (myCurrentCategory != null && myCurrentCategory.Equals(value)) return;
                if (myCurrentCategory != null && myCurrentCategory.State == RowStatus.Edited)
                    CategorySave(myCurrentCategory);
                myCurrentCategory = value;
                if (myCurrentCategory == null)
                {
                    NomenklMainCollection.Clear();
                    RaisePropertiesChanged(nameof(NomenklMainCollection));
                }
                else
                {
                    LoadNomMainForCategory(myCurrentCategory);
                }

                RaisePropertyChanged();
            }
        }

        public override bool IsCanSaveData => CategoryCollection.Any(_ => _.State != RowStatus.NotEdited) ||
                                              NomenklMainCollection.Any(_ => _.State != RowStatus.NotEdited)
                                              ||
                                              CurrentNomenklMain != null &&
                                              CurrentNomenklMain.NomenklCollection.Any(_ =>
                                                  _.State != RowStatus.NotEdited) &&
                                              CurrentNomenklMain.NomenklCollection.All(_ => _.Check());

        private void FocusedRowChanged(object obj)
        {
            var d = obj as Core.EntityViewModel.Nomenkl;
            if (d == null) return;
            if (d.State != RowStatus.NotEdited)
                SaveNomenkl(d);
        }

        private void LoadReferences()
        {
            if (CurrencyCollection == null)
                CurrencyCollection = new ObservableCollection<Currency>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                CurrencyCollection.Clear();
                foreach (var c in ctx.SD_301.ToList())
                    CurrencyCollection.Add(new Currency(c));
            }
        }

        private void LoadNomenklForMain(NomenklMainViewModel main)
        {
            main.NomenklCollection.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var noms = (from n in ctx.SD_83
                        join sd301 in ctx.SD_301 on n.NOM_SALE_CRS_DC equals sd301.DOC_CODE
                        where n.MainId == main.Id
                        select new Core.EntityViewModel.Nomenkl
                        {
                            DocCode = n.DOC_CODE,
                            Name = n.NOM_NAME,
                            NomenklNumber = n.NOM_NOMENKL,
                            NameFull = n.NOM_FULL_NAME,
                            Currency = new Currency {DocCode = sd301.DOC_CODE, CRS_SHORTNAME = sd301.CRS_SHORTNAME},
                            Note = n.NOM_NOTES,
                            IsRentabelnost =  n.IsUslugaInRent ?? false
                        }).ToList();
                    foreach (var nom in noms)
                    {
                        nom.Currency = CurrencyCollection.SingleOrDefault(_ => _.DocCode == nom.Currency.DocCode);
                        nom.Parent = CurrentNomenklMain;
                        main.NomenklCollection.Add(nom);
                        nom.State = RowStatus.NotEdited;
                    }

                    CurrentNomenklMain.State = RowStatus.NotEdited;
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public void LoadNomMainForCategory(NomenklGroup nomenklGroup)
        {
            var nomCat = nomenklGroup ?? CurrentCategory;
            if (nomCat == null) return;
            NomenklMainCollection.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.NomenklMain
                        .Include(_ => _.Countries)
                        .Include(_ => _.SD_119)
                        .Include(_ => _.SD_175)
                        .Include(_ => _.SD_82)
                        .Include(_ => _.SD_83)
                        .Where(_ => _.CategoryDC == nomCat.DocCode)
                        .ToList();
                    foreach (var d in data)
                        NomenklMainCollection.Add(new NomenklMainViewModel(d)
                        {
                            State = RowStatus.NotEdited
                        });
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void RefreshData(object obj)
        {
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            CategoryCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var grp in ctx.SD_82.ToList())
                    CategoryCollection.Add(new NomenklGroup(grp)
                    {
                        State = RowStatus.NotEdited
                    });
            }
        }

        private void SaveNomenkl(Core.EntityViewModel.Nomenkl nom)
        {
            decimal newDC = 0;
            var state = nom.State;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        switch (nom.State)
                        {
                            case RowStatus.NewRow:
                                newDC = ctx.SD_83.Max(_ => _.DOC_CODE) + 1;
                                var newItem = new SD_83
                                {
                                    DOC_CODE = newDC,
                                    NOM_NAME = nom.Name,
                                    NOM_NOMENKL = nom.NomenklNumber,
                                    NOM_NOTES = nom.Note,
                                    NOM_SALE_CRS_DC = nom.Currency.DocCode,
                                    NOM_FULL_NAME = nom.NameFull,
                                    NOM_ED_IZM_DC = CurrentNomenklMain.Unit.DocCode,
                                    NOM_CATEG_DC = CurrentNomenklMain.NomenklCategory.DocCode,
                                    NOM_0MATER_1USLUGA = CurrentNomenklMain.IsUsluga ? 1 : 0,
                                    NOM_1PROD_0MATER = 0,
                                    NOM_1NAKLRASH_0NO = CurrentNomenklMain.IsNakladExpense ? 1 : 0,
                                    NOM_PRODUCT_DC = CurrentNomenklMain.ProductType.DOC_CODE,
                                    Id = nom.Id,
                                    MainId = nom.MainId,
                                    IsUslugaInRent = nom.IsRentabelnost
                                };
                                ctx.SD_83.Add(newItem);
                                break;
                            case RowStatus.Edited:
                                var d = ctx.SD_83.FirstOrDefault(_ => _.DOC_CODE == nom.DocCode);
                                if (d == null) return;
                                d.NOM_NAME = nom.Name;
                                d.NOM_NOMENKL = nom.NomenklNumber;
                                d.NOM_NOTES = nom.Note;
                                d.NOM_SALE_CRS_DC = nom.Currency.DocCode;
                                d.NOM_FULL_NAME = nom.NameFull ?? nom.Name;
                                break;
                        }

                        ctx.SaveChanges();
                        tnx.Commit();
                        nom.myState = RowStatus.NotEdited;
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                        tnx.Rollback();
                        return;
                    }
                }
            }

            if (state == RowStatus.NewRow)
                MainReferences.GetNomenkl(newDC);
        }

        public override void SaveData(object data)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var nom in CurrentNomenklMain.NomenklCollection)
                            switch (nom.State)
                            {
                                case RowStatus.NewRow:
                                    var newDC = ctx.SD_83.Any() ? ctx.SD_83.Max(_ => _.DOC_CODE) + 1 : 10830000001;
                                    var newItem = new SD_83
                                    {
                                        DOC_CODE = newDC,
                                        NOM_NAME = nom.Name,
                                        NOM_NOMENKL = nom.NomenklNumber,
                                        NOM_NOTES = nom.Note,
                                        NOM_SALE_CRS_DC = nom.Currency.DocCode,
                                        NOM_FULL_NAME = nom.NameFull,
                                        NOM_ED_IZM_DC = CurrentNomenklMain.Unit.DocCode,
                                        NOM_CATEG_DC = CurrentNomenklMain.NomenklCategory.DocCode,
                                        NOM_0MATER_1USLUGA = CurrentNomenklMain.IsUsluga ? 1 : 0,
                                        NOM_1PROD_0MATER = 0,
                                        NOM_1NAKLRASH_0NO = CurrentNomenklMain.IsNakladExpense ? 1 : 0,
                                        NOM_PRODUCT_DC = CurrentNomenklMain.ProductType.DOC_CODE,
                                        Id = nom.Id,
                                        MainId = nom.MainId,
                                        IsUslugaInRent = CurrentNomenklMain.IsRentabelnost
                                    };
                                    ctx.SD_83.Add(newItem);
                                    break;
                                case RowStatus.Edited:
                                    var d = ctx.SD_83.FirstOrDefault(_ => _.DOC_CODE == nom.DocCode);
                                    if (d == null) continue;
                                    d.NOM_NAME = nom.Name;
                                    d.NOM_NOMENKL = nom.NomenklNumber;
                                    d.NOM_NOTES = nom.Note;
                                    d.NOM_SALE_CRS_DC = nom.Currency.DocCode;
                                    d.NOM_FULL_NAME = nom.NameFull ?? nom.Name;
                                    break;
                            }

                        ctx.SaveChanges();
                        tnx.Commit();
                        foreach (var nm in NomenklMainCollection)
                            nm.myState = RowStatus.NotEdited;
                        foreach (var nom in CurrentNomenklMain.NomenklCollection)
                            nom.myState = RowStatus.NotEdited;
                    }
                    catch (Exception ex)
                    {
                        tnx.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        #region Command

        public ICommand NomenklDeleteCommand
        {
            get { return new Command(NomenklDelete, _ => CurrentNomenkl != null); }
        }

        private void NomenklDelete(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (ctx.TD_24.Any(_ => _.DDT_NOMENKL_DC == CurrentNomenkl.DocCode)
                            || ctx.TD_26.Any(_ => _.SFT_NEMENKL_DC == CurrentNomenkl.DocCode)
                            || ctx.TD_84.Any(_ => _.SFT_NEMENKL_DC == CurrentNomenkl.DocCode))
                        {
                            WindowManager.ShowMessage(Application.Current.MainWindow,
                                $"По номенклатуре №{CurrentNomenkl.NomenklNumber} {CurrentNomenkl.Name} есть операции. Удаление не возможно",
                                "Предупреждение.", MessageBoxImage.Warning);
                            return;
                        }

                        var oldItem = ctx.SD_83.FirstOrDefault(_ => _.DOC_CODE == CurrentNomenkl.DocCode);
                        if (oldItem != null)
                            ctx.SD_83.Remove(oldItem);
                        ctx.SaveChanges();
                        tnx.Commit();
                        CurrentNomenklMain.NomenklCollection.Remove(CurrentNomenkl);
                    }
                    catch (Exception ex)
                    {
                        tnx.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public ICommand NomenklAddCommand
        {
            get { return new Command(NomenklAdd, _ => true); }
        }

        private void NomenklAdd(object obj)
        {
            var newItem = new Core.EntityViewModel.Nomenkl
            {
                DOC_CODE = -1,
                Name = CurrentNomenklMain.Name,
                NomenklNumber = CurrentNomenklMain.NomenklNumber,
                NameFull = CurrentNomenklMain.Name,
                MainId = CurrentNomenklMain.Id,
                Id = Guid.NewGuid(),
                IsDeleted = false,
                Group = CurrentNomenklMain.NomenklCategory,
                IsUsluga = CurrentNomenklMain.IsUsluga,
                IsNaklRashod = CurrentNomenklMain.IsNakladExpense,
                IsRentabelnost = CurrentNomenklMain.IsRentabelnost,
                State = RowStatus.NewRow,
                NOM_PRODUCT_DC = CurrentNomenklMain.ProductType.DOC_CODE
            };
            CurrentNomenklMain.NomenklCollection.Add(newItem);
        }

        public ICommand NomenklMainAddCommand
        {
            get { return new Command(NomenklMainNew, param => CurrentCategory != null); }
        }

        private void NomenklMainNew(object obj)
        {
            var ctx = new MainCardWindowViewModel(Guid.Empty, CurrentCategory)
            {
                ParentReference = this
            };
            // ReSharper disable once UseObjectOrCollectionInitializer
            var form = new NomenklMainCardView {Owner = Application.Current.MainWindow, DataContext = ctx};
            //form.DataContext = ctx;
            form.Show();
        }

        private void SearchNomenkls(object obj)
        {
            NomenklMainCollection.Clear();
            //var noms = new List<CurrentNomenklMain>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var n in ctx.NomenklMain.Include(_ => _.Countries)
                        .Include(_ => _.SD_119)
                        .Include(_ => _.SD_175)
                        .Include(_ => _.SD_82)
                        .Include(_ => _.SD_83)
                        .ToList())
                        if (n.Name.ToUpper().Contains(SearchText.ToUpper())
                            || n.NomenklNumber.ToUpper().Contains(SearchText.ToUpper())
                            || n.FullName.ToUpper().Contains(SearchText.ToUpper()))
                        {
                            var nn = new NomenklMainViewModel(n) {State = RowStatus.NotEdited};
                            NomenklMainCollection.Add(nn);
                        }
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public ICommand NomenklMainEditCommand
        {
            get { return new Command(NomenklMainEdit, param => CurrentNomenklMain != null); }
        }

        public void NomenklMainEdit(object obj)
        {
            var ctx = new MainCardWindowViewModel(CurrentNomenklMain.Id, null)
            {
                ParentReference = this,
                NomenklMain = CurrentNomenklMain
            };
            // ReSharper disable once UseObjectOrCollectionInitializer
            var form = new NomenklMainCardView {Owner = Application.Current.MainWindow, DataContext = ctx};
            //form.DataContext = ctx;
            form.Show();
            //ctx.RefreshData(null);
        }

        public ICommand NomenklMainCopyCommand
        {
            get { return new Command(NomenklMainCopy, param => CurrentNomenklMain != null); }
        }

        private void NomenklMainCopy(object obj)
        {
            var newNom = new NomenklMainViewModel
            {
                State = RowStatus.NewRow,
                NomenklNumber = null,
                Id = Guid.NewGuid(),
                Name = CurrentNomenklMain.Name,
                FullName = CurrentNomenklMain.FullName,
                NomenklCategory = CurrentNomenklMain.NomenklCategory,
                NomenklType = CurrentNomenklMain.NomenklType,
                Country = CurrentNomenklMain.Country,
                Note = CurrentNomenklMain.Note,
                IsComplex = false,
                IsNakladExpense = CurrentNomenklMain.IsNakladExpense,
                IsUsluga = CurrentNomenklMain.IsUsluga,
                Unit = CurrentNomenklMain.Unit,
                ProductType = CurrentNomenklMain.ProductType,
                IsRentabelnost = CurrentNomenklMain.IsRentabelnost
            };
            var ctx = new MainCardWindowViewModel
            {
                ParentReference = this,
                NomenklMain = newNom
            };
            // ReSharper disable once UseObjectOrCollectionInitializer
            var form = new NomenklMainCardView {Owner = Application.Current.MainWindow, DataContext = ctx};
            //form.DataContext = ctx;
            form.Show();
        }

        public ICommand NomenklMainDeleteCommand
        {
            get { return new Command(NomenklMainDelete, param => CurrentNomenklMain != null); }
        }

        private void NomenklMainDelete(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var nom in CurrentNomenklMain.NomenklCollection)
                        {
                            if (ctx.TD_24.Any(_ => _.DDT_NOMENKL_DC == nom.DocCode)
                                || ctx.TD_26.Any(_ => _.SFT_NEMENKL_DC == nom.DocCode)
                                || ctx.TD_84.Any(_ => _.SFT_NEMENKL_DC == nom.DocCode))
                            {
                                WindowManager.ShowMessage(Application.Current.MainWindow,
                                    $"По номенклатуре №{nom.NomenklNumber} {nom.Name} есть операции. Удаление не возможно",
                                    "Предупреждение.", MessageBoxImage.Warning);
                                return;
                            }

                            var oldItem = ctx.SD_83.FirstOrDefault(_ => _.DOC_CODE == nom.DocCode);
                            if (oldItem != null)
                                ctx.SD_83.Remove(oldItem);
                        }

                        var oldMain = ctx.NomenklMain.FirstOrDefault(_ => _.Id == CurrentNomenklMain.Id);
                        if (oldMain != null)
                            ctx.NomenklMain.Remove(oldMain);
                        ctx.SaveChanges();
                        tnx.Commit();
                        NomenklMainCollection.Remove(CurrentNomenklMain);
                    }
                    catch (Exception ex)
                    {
                        tnx.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public ICommand TreeAddInRootCommand
        {
            get { return new Command(TreeAddInRoot, param => true); }
        }

        private void TreeAddInRoot(object obj)
        {
            var category = new NameNoteViewModel
            {
                HeaderName = "Наименование",
                HeaderNote = "Примечания",
                Name = "",
                Note = ""
            };
            var res = DialogService.ShowDialog(MessageBoxButton.OKCancel, "Новая категория", "NameNoteUC", category);
            if (res != MessageBoxResult.OK) return;
            var newgrp = NomenklManager.CategoryAdd(category, null);
            CategoryCollection.Add(newgrp);
            CurrentCategory = newgrp;
        }

        public ICommand TreeAddChildCommand
        {
            get { return new Command(TreeAddChild, param => true); }
        }

        private void TreeAddChild(object obj)
        {
            var category = new NameNoteViewModel
            {
                HeaderName = "Наименование",
                HeaderNote = "Примечания",
                Name = "",
                Note = ""
            };
            var res = DialogService.ShowDialog(MessageBoxButton.OKCancel, "Новая категория", "NameNoteUC", category);
            if (res != MessageBoxResult.OK) return;
            var newgrp = NomenklManager.CategoryAdd(category, CurrentCategory.DocCode);
            if (newgrp == null) return;
            CategoryCollection.Add(newgrp);
            CurrentCategory = newgrp;
        }

        public ICommand TreeAddOneLevelCommand
        {
            get { return new Command(TreeAddOneLevel, param => true); }
        }

        private void TreeAddOneLevel(object obj)
        {
            var category = new NameNoteViewModel
            {
                HeaderName = "Наименование",
                HeaderNote = "Примечания",
                Name = "",
                Note = ""
            };
            var res = DialogService.ShowDialog(MessageBoxButton.OKCancel, "Новая категория", "NameNoteUC", category);
            if (res != MessageBoxResult.OK) return;
            var newgrp = NomenklManager.CategoryAdd(category, CurrentCategory.ParentDC);
            CategoryCollection.Add(newgrp);
            CurrentCategory = newgrp;
        }

        public ICommand TreeEditCommand
        {
            get { return new Command(TreeEdit, param => true); }
        }

        private void TreeEdit(object obj)
        {
            var category = new NameNoteViewModel
            {
                HeaderName = "Наименование",
                HeaderNote = "Примечания",
                Name = CurrentCategory?.Name,
                Note = CurrentCategory?.Note
            };
            var res = DialogService.ShowDialog(MessageBoxButton.OKCancel, "Редактирование категории", "NameNoteUC",
                category);
            if (res != MessageBoxResult.OK) return;
            if (CurrentCategory == null) return;
            var oldCat = new NomenklGroup(CurrentCategory.Entity);
            CurrentCategory.Name = category.Name;
            CurrentCategory.Note = category.Note;
            if (CategorySave(CurrentCategory)) return;
            CurrentCategory.Name = oldCat.Name;
            CurrentCategory.Note = oldCat.Note;
            CurrentCategory.State = RowStatus.NotEdited;
        }

        private bool CategorySave(NomenklGroup cat)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = new TransactionScope())
                {
                    try
                    {
                        var oldcat = ctx.SD_82.SingleOrDefault(_ => _.DOC_CODE == cat.DocCode);
                        if (oldcat == null) return false;
                        oldcat.CAT_NAME = cat.Name;
                        ctx.SaveChanges();
                        tnx.Complete();
                        CurrentCategory.State = RowStatus.NotEdited;
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                        return false;
                    }
                }
            }

            return true;
        }

        public ICommand TreeDeleteCommand
        {
            get
            {
                return new Command(TreeDelete, param => CurrentCategory != null && NomenklMainCollection.Count == 0
                                                                                &&
                                                                                CategoryCollection.All(
                                                                                    _ => _.ParentDC !=
                                                                                         CurrentCategory.DocCode));
            }
        }

        private void TreeDelete(object obj)
        {
            var res = MessageBoxService.Show($"Вы уверены, что хотите удалить категорию {CurrentCategory}?",
                "Предупреждение", MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (res != MessageBoxResult.OK) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var oldcat = ctx.SD_82.SingleOrDefault(_ => _.DOC_CODE == CurrentCategory.DocCode);
                        if (oldcat == null) return;
                        ctx.Database.ExecuteSqlCommand(
                            $"DELETE FROM HD_82 WHERE DOC_CODE = {CustomFormat.DecimalToSqlDecimal(CurrentCategory.DocCode)}");
                        ctx.SD_82.Remove(oldcat);
                        ctx.SaveChanges();
                        tnx.Commit();
                        CategoryCollection.Remove(CurrentCategory);
                    }
                    catch (Exception ex)
                    {
                        tnx.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public override void Search(object obj)
        {
            NomenklMainCollection.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.NomenklMain
                        .Include(_ => _.Countries)
                        .Include(_ => _.SD_119)
                        .Include(_ => _.SD_175)
                        .Include(_ => _.SD_82)
                        .Where(_ => _.FullName.ToUpper().Contains(SearchText.ToUpper()) ||
                                    _.Name.ToUpper().Contains(SearchText.ToUpper()) ||
                                    _.NomenklNumber.ToUpper().Contains(SearchText.ToUpper()) ||
                                    _.SD_119 != null && _.SD_119.MC_NAME
                                        .ToUpper()
                                        .Contains(
                                            SearchText
                                                .ToUpper())
                                    || _.SD_175.ED_IZM_NAME.ToUpper().Contains(SearchText.ToUpper())
                                    || _.SD_82 != null && _.SD_82.CAT_NAME.ToUpper().Contains(SearchText.ToUpper())
                        )
                        .ToList();
                    foreach (var nom in data)
                        NomenklMainCollection.Add(new NomenklMainViewModel(nom)
                        {
                            State = RowStatus.NotEdited
                        });
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override string SearchText
        {
            get => mySearchText;
            set
            {
                if (mySearchText == value) return;
                mySearchText = value;
                RaisePropertyChanged();
                IsCategoryEnabled = string.IsNullOrEmpty(mySearchText);
                CurrentCategory = null;
            }
        }

        public bool IsCategoryEnabled
        {
            get => myIsCategoryEnabled;
            set
            {
                if (myIsCategoryEnabled == value) return;
                myIsCategoryEnabled = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsCanSearch => !string.IsNullOrEmpty(SearchText);

        public override void SearchClear(object obj)
        {
            SearchText = null;
            RefreshData(null);
        }

        #endregion
    }
}