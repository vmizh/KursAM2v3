using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.Xpf;
using Helper;
using KursAM2.Managers.Nomenkl;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.DialogUserControl.ViewModel;
using KursAM2.View.KursReferences;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using Newtonsoft.Json;
using NomenklMain = KursDomain.References.NomenklMain;
using StackExchange.Redis;


namespace KursAM2.ViewModel.Reference.Nomenkl
{
    public sealed class ReferenceWindowViewModel : RSWindowViewModelBase
    {
        private readonly WindowManager WinManager = new WindowManager();
        private NomenklGroupViewModel myCurrentCategory;
        private NomenklViewModel myCurrentNomenkl;
        private NomenklMainViewModel myCurrentNomenklMain;
        private bool myIsCanChangeCurrency;
        private bool myIsCategoryEnabled;

        private readonly ISubscriber mySubscriber;
        private readonly ConnectionMultiplexer redis;


        public ReferenceWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();
            LoadReferences();
            RefreshData(null);
            IsCategoryEnabled = true;
        }

        private IMessageBoxService MessageBoxService => this.GetService<IMessageBoxService>();

        public ObservableCollection<NomenklGroupViewModel> CategoryCollection { set; get; } =
            new ObservableCollection<NomenklGroupViewModel>();

        public ObservableCollection<NomenklMainViewModel> NomenklMainCollection { set; get; } =
            new ObservableCollection<NomenklMainViewModel>();

        public ObservableCollection<NomenklViewModel> CurrentNomenklCollection { set; get; }
            = new ObservableCollection<NomenklViewModel>();

        public ObservableCollection<Currency> CurrencyCollection { set; get; }

        public ObservableCollection<Currency> CurrencyCollectionForNomenklMain { set; get; }
            = new ObservableCollection<Currency>();

        public override string LayoutName => "NomenklReferenceWindowViewModel";

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

        public NomenklViewModel CurrentNomenkl
        {
            get => myCurrentNomenkl;
            set
            {
                //SaveData(CurrentNomenklMain.NomenklCollection);
                if (Equals(myCurrentNomenkl, value)) return;
                myCurrentNomenkl = value;
                reloadCurrencies();
                if (myCurrentNomenkl != null)
                    if (myCurrentNomenkl.State == RowStatus.NewRow)
                        IsCanChangeCurrency = false;
                    else
                        try
                        {
                            IsCanChangeCurrency = true;
                            using (var ctx = GlobalOptions.GetEntities())
                            {
                                if (ctx.TD_24.Any(_ => _.DDT_NOMENKL_DC == myCurrentNomenkl.DocCode) ||
                                    ctx.TD_26.Any(_ => _.SFT_NEMENKL_DC == myCurrentNomenkl.DocCode) ||
                                    ctx.TD_84.Any(_ => _.SFT_NEMENKL_DC == myCurrentNomenkl.DocCode))
                                {
                                    IsCanChangeCurrency = false;
                                    myCurrentNomenkl.myState = RowStatus.NotEdited;
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
                if (Equals(myCurrentNomenklMain, value)) return;
                myCurrentNomenklMain = value;
                if (myCurrentNomenklMain != null)
                    LoadNomenklForMain();
                reloadCurrencies();
                if (myCurrentNomenklMain != null)
                    foreach (var n in myCurrentNomenklMain.NomenklCollection)
                        n.State = RowStatus.NotEdited;
                myCurrentNomenklMain?.myState = RowStatus.NotEdited;
                myCurrentNomenklMain?.RaisePropertyAllChanged();
                RaisePropertyChanged();
            }
        }

        public NomenklGroupViewModel CurrentCategory
        {
            get => myCurrentCategory;
            set
            {
                if (myCurrentCategory != null && myCurrentCategory == value) return;
                if (myCurrentCategory != null && myCurrentCategory.State == RowStatus.Edited)
                    CategorySave(myCurrentCategory);
                myCurrentCategory = value;
                if (myCurrentCategory == null)
                {
                    if (NomenklMainCollection != null)
                    {
                        NomenklMainCollection.Clear();
                        RaisePropertyChanged(nameof(NomenklMainCollection));
                    }
                }
                else
                {
                    LoadNomMainForCategory(myCurrentCategory);
                }

                RaisePropertyChanged();
            }
        }

        public override bool IsCanSaveData => CategoryCollection.Any(_ => _.State != RowStatus.NotEdited) ||
                                              (CurrentNomenklMain != null &&
                                               CurrentNomenklMain.NomenklCollection.Any(_ =>
                                                   _.State != RowStatus.NotEdited) &&
                                               CurrentNomenklMain.NomenklCollection.All(_ => _.Check()));

        private void reloadCurrencies()
        {
            CurrencyCollectionForNomenklMain.Clear();
            if (myCurrentNomenklMain != null)
                foreach (var crs in CurrencyCollection)
                    if (myCurrentNomenklMain.NomenklCollection.Where(_ => _.Currency != null)
                        .Select(_ => _.Currency.DocCode)
                        .All(d => d != crs.DocCode))
                        CurrencyCollectionForNomenklMain.Add(crs);
        }

        private void FocusedRowChanged(object obj)
        {
            var d = obj as NomenklViewModel;
            if (d == null) return;
            if (d.State != RowStatus.NotEdited && d.Currency != null)
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
                {
                    var crs = new Currency();
                    crs.LoadFromEntity(c);
                    CurrencyCollection.Add(crs);
                }
            }
        }

        private void LoadNomenklForMain()
        {
            CurrentNomenklCollection.Clear();
            try
            {
                foreach (var nom in CurrentNomenklMain.NomenklCollection) CurrentNomenklCollection.Add(nom);
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public void LoadNomMainForCategory(NomenklGroupViewModel nomenklGroup)
        {
            var nomCat = nomenklGroup ?? CurrentCategory;
            if (nomCat == null) return;
            NomenklMainCollection.Clear();
            CurrentNomenklCollection.Clear();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.NomenklMain
                        .Include(_ => _.SD_119)
                        .Include(_ => _.SD_175)
                        .Include(_ => _.SD_82)
                        .Include(_ => _.SD_83)
                        .Include(_ => _.SD_50)
                        .Include(_ => _.Countries)
                        .Where(_ => _.CategoryDC == nomCat.DocCode)
                        .ToList();
                    foreach (var d in data)
                        NomenklMainCollection.Add(new NomenklMainViewModel(d)
                        {
                            myState = RowStatus.NotEdited
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
                foreach (var newItem in ctx.SD_82.ToList().Select(grp => new NomenklGroupViewModel(grp)
                         {
                             State = RowStatus.NotEdited
                         }))
                {
                    newItem.NomenklCount = GlobalOptions.ReferencesCache.GetNomenklGroup(newItem.DocCode).NomenklCount;
                    CategoryCollection.Add(newItem);
                }
            }

        }
        
        private void SaveNomenkl(NomenklViewModel nom)
        {
            decimal newDC = 0;
            var state = nom.State;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        SD_83 d = null;
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
                                    NOM_SALE_CRS_DC = ((IDocCode)nom.Currency)?.DocCode,
                                    NOM_FULL_NAME = nom.NameFull,
                                    NOM_ED_IZM_DC = CurrentNomenklMain.Unit.DocCode,
                                    NOM_CATEG_DC = CurrentNomenklMain.NomenklCategory.DocCode,
                                    NOM_0MATER_1USLUGA = CurrentNomenklMain.IsUsluga ? 1 : 0,
                                    NOM_1PROD_0MATER = 0,
                                    NOM_1NAKLRASH_0NO = CurrentNomenklMain.IsNakladExpense ? 1 : 0,
                                    NOM_PRODUCT_DC = ((IDocCode)CurrentNomenklMain.ProductType).DocCode,
                                    Id = nom.Id,
                                    MainId = nom.MainId,
                                    IsUslugaInRent = nom.IsRentabelnost
                                };
                                ctx.SD_83.Add(newItem);
                                break;
                            case RowStatus.Edited:
                                d = ctx.SD_83.FirstOrDefault(_ => _.DOC_CODE == nom.DocCode);
                                if (d == null) return;
                                d.NOM_NAME = nom.Name;
                                d.NOM_NOMENKL = nom.NomenklNumber;
                                d.NOM_NOTES = nom.Note;
                                d.NOM_SALE_CRS_DC = ((IDocCode)nom.Currency).DocCode;
                                d.NOM_FULL_NAME = nom.NameFull ?? nom.Name;
                                break;
                        }

                        ctx.SaveChanges();
                        tnx.Commit();
                        SD_83 entity = null;
                        entity = nom.State == RowStatus.NewRow
                            ? ctx.SD_83.Include(_ => _.NomenklMain).FirstOrDefault(_ => _.DOC_CODE == newDC)
                            : d;
                        if (entity != null)
                        {
                            var updItem = new KursDomain.References.Nomenkl
                            {
                                DocCode = entity.DOC_CODE,
                                Id = entity.Id,
                                Name = entity.NOM_NAME,
                                FullName =
                                    entity.NOM_FULL_NAME,
                                Notes = entity.NOM_NOTES,
                                IsUsluga =
                                    entity.NOM_0MATER_1USLUGA == 1,
                                IsProguct = entity.NOM_1PROD_0MATER == 1,
                                IsNakladExpense =
                                    entity.NOM_1NAKLRASH_0NO == 1,
                                DefaultNDSPercent = (decimal?)entity.NOM_NDS_PERCENT,
                                IsDeleted =
                                    entity.NOM_DELETED == 1,
                                IsUslugaInRentabelnost =
                                    entity.IsUslugaInRent ?? false,
                                UpdateDate =
                                    entity.UpdateDate ?? DateTime.MinValue,
                                MainId =
                                    entity.MainId ?? Guid.Empty,
                                IsCurrencyTransfer = entity.NomenklMain.IsCurrencyTransfer ?? false,
                                NomenklNumber =
                                    entity.NOM_NOMENKL,
                                NomenklTypeDC =
                                    entity.NomenklMain.TypeDC,
                                ProductTypeDC = entity.NomenklMain.ProductDC,
                                UnitDC = entity.NOM_ED_IZM_DC,
                                CurrencyDC = entity.NOM_SALE_CRS_DC,
                                GroupDC = entity.NOM_CATEG_DC
                            };
                            updItem.LoadFromEntity(entity, GlobalOptions.ReferencesCache);
                            //GlobalOptions.ReferencesCache.AddOrUpdate(updItem);
                        }

                        var mentity = ctx.NomenklMain.Include(_ => _.SD_83).FirstOrDefault(_ => _.Id == nom.MainId);
                        if (mentity is not null)
                        {
                            var updMain = new NomenklMain
                            {
                                Id = mentity.Id,
                                Name = mentity.Name,
                                Notes = mentity.Note,
                                NomenklNumber = mentity.NomenklNumber,
                                FullName = mentity.FullName,
                                IsUsluga = mentity.IsUsluga,
                                IsProguct = mentity.IsComplex,
                                IsNakladExpense = mentity.IsNakladExpense,
                                IsOnlyState = mentity.IsOnlyState ?? false,
                                IsCurrencyTransfer = mentity.IsCurrencyTransfer ?? false,
                                IsRentabelnost = mentity.IsRentabelnost ?? false,
                                UnitDC = mentity.UnitDC,
                                CategoryDC = mentity.CategoryDC,
                                NomenklTypeDC = mentity.TypeDC,
                                ProductTypeDC = mentity.ProductDC,
                                Nomenkls = new List<decimal>(mentity.SD_83.Select(_ => _.DOC_CODE))
                            };
                            updMain.LoadFromEntity(mentity, GlobalOptions.ReferencesCache);
                            //GlobalOptions.ReferencesCache.AddOrUpdateGuid(updMain);
                            if (mySubscriber != null && mySubscriber.IsConnected())
                            {
                                var str = "обновление справочника NomenklMain";
                                var message = new RedisMessage
                                {
                                    DocumentType = DocumentType.None,
                                    DocCode = null,
                                    Id = updMain.Id,
                                    DocDate = DateTime.Now,
                                    IsDocument = false,
                                    OperationType = RedisMessageDocumentOperationTypeEnum.Update,
                                    Message =
                                        $"Пользователь '{GlobalOptions.UserInfo.Name}' {str} номенклатура: {updMain.Id} {updMain.Name}"
                                };
                                message.ExternalValues.Add("NomenklDC", updMain.Nomenkls);
                                message.ExternalValues.Add("RedisKey", $"Cache:NomenklMain:{updMain.Id}@{updMain.UpdateDate}");
                                var jsonSerializerSettings = new JsonSerializerSettings
                                {
                                    TypeNameHandling = TypeNameHandling.All
                                };
                                var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                                mySubscriber.Publish(
                                    new RedisChannel(RedisMessageChannels.NomenklMainReference,
                                        RedisChannel.PatternMode.Auto), json);
                            }
                        }

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
                GlobalOptions.ReferencesCache.GetNomenkl(newDC);
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
                                    nom.DocCode = newDC;
                                    var newItem = new SD_83
                                    {
                                        DOC_CODE = newDC,
                                        NOM_NAME = nom.Name,
                                        NOM_NOMENKL = nom.NomenklNumber,
                                        NOM_NOTES = nom.Note,
                                        NOM_SALE_CRS_DC = ((IDocCode)nom.Currency).DocCode,
                                        NOM_FULL_NAME = nom.NameFull,
                                        NOM_ED_IZM_DC = CurrentNomenklMain.Unit.DocCode,
                                        NOM_CATEG_DC = CurrentNomenklMain.NomenklCategory.DocCode,
                                        NOM_0MATER_1USLUGA = CurrentNomenklMain.IsUsluga ? 1 : 0,
                                        NOM_1PROD_0MATER = 0,
                                        NOM_1NAKLRASH_0NO = CurrentNomenklMain.IsNakladExpense ? 1 : 0,
                                        NOM_PRODUCT_DC = ((IDocCode)CurrentNomenklMain.ProductType).DocCode,
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
                                    d.NOM_SALE_CRS_DC = ((IDocCode)nom.Currency).DocCode;
                                    d.NOM_FULL_NAME = nom.NameFull ?? nom.Name;
                                    break;
                            }

                        foreach (var nom in CurrentNomenklMain.NomenklCollection)
                        {
                            SD_83 entity = null;
                            entity = ctx.SD_83.Include(_ => _.NomenklMain)
                                .FirstOrDefault(_ => _.DOC_CODE == nom.DOC_CODE);
                            if (entity != null)
                            {
                                var updItem = new KursDomain.References.Nomenkl
                                {
                                    DocCode = entity.DOC_CODE,
                                    Id = entity.Id,
                                    Name = entity.NOM_NAME,
                                    FullName =
                                        entity.NOM_FULL_NAME,
                                    Notes = entity.NOM_NOTES,
                                    IsUsluga =
                                        entity.NOM_0MATER_1USLUGA == 1,
                                    IsProguct = entity.NOM_1PROD_0MATER == 1,
                                    IsNakladExpense =
                                        entity.NOM_1NAKLRASH_0NO == 1,
                                    DefaultNDSPercent = (decimal?)entity.NOM_NDS_PERCENT,
                                    IsDeleted =
                                        entity.NOM_DELETED == 1,
                                    IsUslugaInRentabelnost =
                                        entity.IsUslugaInRent ?? false,
                                    UpdateDate =
                                        entity.UpdateDate ?? DateTime.MinValue,
                                    MainId =
                                        entity.MainId ?? Guid.Empty,
                                    IsCurrencyTransfer = entity.NomenklMain.IsCurrencyTransfer ?? false,
                                    NomenklNumber =
                                        entity.NOM_NOMENKL,
                                    NomenklTypeDC =
                                        entity.NomenklMain.TypeDC,
                                    ProductTypeDC = entity.NomenklMain.ProductDC,
                                    UnitDC = entity.NOM_ED_IZM_DC,
                                    CurrencyDC = entity.NOM_SALE_CRS_DC,
                                    GroupDC = entity.NOM_CATEG_DC
                                };
                                updItem.LoadFromEntity(entity, GlobalOptions.ReferencesCache);
                                GlobalOptions.ReferencesCache.AddOrUpdate(updItem);
                            }
                        }

                        ctx.SaveChanges();
                        tnx.Commit();

                        var mentity = ctx.NomenklMain.Include(_ => _.SD_83)
                            .FirstOrDefault(_ => _.Id == CurrentNomenklMain.Id);
                        if (mentity is not null)
                        {
                            var updMain = new NomenklMain
                            {
                                Id = mentity.Id,
                                Name = mentity.Name,
                                Notes = mentity.Note,
                                NomenklNumber = mentity.NomenklNumber,
                                FullName = mentity.FullName,
                                IsUsluga = mentity.IsUsluga,
                                IsProguct = mentity.IsComplex,
                                IsNakladExpense = mentity.IsNakladExpense,
                                IsOnlyState = mentity.IsOnlyState ?? false,
                                IsCurrencyTransfer = mentity.IsCurrencyTransfer ?? false,
                                IsRentabelnost = mentity.IsRentabelnost ?? false,
                                UnitDC = mentity.UnitDC,
                                CategoryDC = mentity.CategoryDC,
                                NomenklTypeDC = mentity.TypeDC,
                                ProductTypeDC = mentity.ProductDC,
                                Nomenkls = new List<decimal>(mentity.SD_83.Select(_ => _.DOC_CODE))
                            };
                            updMain.LoadFromEntity(mentity, GlobalOptions.ReferencesCache);
                            GlobalOptions.ReferencesCache.AddOrUpdateGuid(updMain);
                        }
                        if (mySubscriber != null && mySubscriber.IsConnected())
                        {
                            var str = "обновление справочника NomenklMain";
                            var message = new RedisMessage
                            {
                                DocumentType = DocumentType.None,
                                DocCode = null,
                                Id = CurrentNomenklMain.Id,
                                DocDate = DateTime.Now,
                                IsDocument = false,
                                OperationType = RedisMessageDocumentOperationTypeEnum.Update,
                                Message =
                                    $"Пользователь '{GlobalOptions.UserInfo.Name}' {str} номенклатура: {CurrentNomenklMain.Id} {CurrentNomenklMain.Name}"
                            };
                            message.ExternalValues.Add("NomenklDC", CurrentNomenklMain.NomenklCollection);
                            message.ExternalValues.Add("RedisKey", $"Cache:NomenklMain:{CurrentNomenklMain.Id}@{DateTime.Now}");
                            var jsonSerializerSettings = new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.All
                            };
                            var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                            mySubscriber.Publish(
                                new RedisChannel(RedisMessageChannels.NomenklMainReference,
                                    RedisChannel.PatternMode.Auto), json);
                        }
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

        private bool isNomenklCanDelete()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                return CurrentNomenkl != null && !(ctx.TD_24.Any(_ => _.DDT_NOMENKL_DC == CurrentNomenkl.DocCode)
                                                   || ctx.TD_26.Any(_ => _.SFT_NEMENKL_DC == CurrentNomenkl.DocCode)
                                                   || ctx.TD_84.Any(_ => _.SFT_NEMENKL_DC == CurrentNomenkl.DocCode)
                                                   || ctx.TD_26_CurrencyConvert.Any(_ =>
                                                       _.NomenklId == CurrentNomenkl.Id));
            }
        }

        public ICommand NomenklDeleteCommand
        {
            get { return new Command(NomenklDelete, _ => isNomenklCanDelete()); }
        }

        private void NomenklDelete(object obj)
        {
            var res = WinManager.ShowWinUIMessageBox("По данной номенклатуре операций нет, удалить?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (res == MessageBoxResult.No) return;

            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        if (ctx.TD_24.Any(_ => _.DDT_NOMENKL_DC == CurrentNomenkl.DocCode)
                            || ctx.TD_26.Any(_ => _.SFT_NEMENKL_DC == CurrentNomenkl.DocCode)
                            || ctx.TD_84.Any(_ => _.SFT_NEMENKL_DC == CurrentNomenkl.DocCode)
                            || ctx.TD_26_CurrencyConvert.Any(_ => _.NomenklId == CurrentNomenkl.Id))
                        {
                            WindowManager.ShowMessage(Application.Current.MainWindow,
                                $"По номенклатуре №{CurrentNomenkl.NomenklNumber} {CurrentNomenkl.Name} есть операции. Удаление не возможно",
                                "Предупреждение.", MessageBoxImage.Warning);
                            return;
                        }

                        var prices = ctx.NOM_PRICE.Where(_ => _.NOM_DC == CurrentNomenkl.DocCode);
                        foreach (var np in prices) ctx.NOM_PRICE.Remove(np);
                        var wd27 = ctx.WD_27.Where(_ => _.SKLW_NOMENKL_DC == CurrentNomenkl.DocCode);
                        foreach (var w in wd27) ctx.WD_27.Remove(w);
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
            get
            {
                return new Command(NomenklAdd,
                    _ => CurrentNomenklMain != null && !CurrentNomenklMain.IsOnlyState);
            }
        }

        private void NomenklAdd(object obj)
        {
            var newItem = new NomenklViewModel
            {
                DOC_CODE = -1,
                Name = CurrentNomenklMain.Name,
                NomenklNumber = CurrentNomenklMain.NomenklNumber,
                NameFull = CurrentNomenklMain.Name,
                MainId = CurrentNomenklMain.Id,
                Id = Guid.NewGuid(),
                IsDeleted = false,
                Group =
                    GlobalOptions.ReferencesCache.GetNomenklGroup(CurrentNomenklMain.NomenklCategory?.DocCode) as
                        NomenklGroup,
                IsUsluga = CurrentNomenklMain.IsUsluga,
                IsNaklRashod = CurrentNomenklMain.IsNakladExpense,
                IsRentabelnost = CurrentNomenklMain.IsRentabelnost,
                State = RowStatus.NewRow,
                NOM_PRODUCT_DC = ((IDocCode)CurrentNomenklMain.ProductType).DocCode,
                IsCurrencyTransfer = CurrentNomenklMain.IsCurrencyTransfer
            };
            CurrentNomenklMain.NomenklCollection.Add(newItem);
            LoadNomenklForMain();
        }

        public ICommand NomenklMainAddCommand
        {
            get { return new Command(NomenklMainNew, _ => CurrentCategory != null); }
        }

        private void NomenklMainNew(object obj)
        {
            var ctx = new MainCardWindowViewModel(Guid.Empty, CurrentCategory)
            {
                ParentReference = this
            };
            // ReSharper disable once UseObjectOrCollectionInitializer
            var form = new NomenklMainCardView { Owner = Application.Current.MainWindow, DataContext = ctx };
            //form.DataContext = ctx;
            form.Show();
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void SearchNomenkls(object obj)
        {
            NomenklMainCollection.Clear();
            //var noms = new List<CurrentNomenklMain>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var nn in from n in ctx.NomenklMain.Include(_ => _.Countries)
                                 .Include(_ => _.SD_119)
                                 .Include(_ => _.SD_175)
                                 .Include(_ => _.SD_82)
                                 .Include(_ => _.SD_83)
                                 .ToList()
                             where n.Name.ToUpper().Contains(SearchText.ToUpper())
                                   || n.NomenklNumber.ToUpper().Contains(SearchText.ToUpper())
                                   || n.FullName.ToUpper().Contains(SearchText.ToUpper())
                             select new NomenklMainViewModel(n) { State = RowStatus.NotEdited })
                        NomenklMainCollection.Add(nn);
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public ICommand NomenklMainEditCommand
        {
            get { return new Command(NomenklMainEdit, _ => CurrentNomenklMain != null); }
        }

        public void NomenklMainEdit(object obj)
        {
            CurrentNomenklMain.State = RowStatus.NotEdited;
            CurrentNomenklMain.RaisePropertyAllChanged();
            var ctx = new MainCardWindowViewModel
            {
                ParentReference = this,
                NomenklMain = CurrentNomenklMain
            };
            // ReSharper disable once UseObjectOrCollectionInitializer
            var form = new NomenklMainCardView { Owner = Application.Current.MainWindow };
            form.DataContext = ctx;
            form.Show();
            ctx.NomenklMain.State = RowStatus.NotEdited;
            ctx.RaisePropertyAllChanged();
            ctx.NomenklMain.RaisePropertyAllChanged();

        }

        public ICommand NomenklMainCopyCommand
        {
            get { return new Command(NomenklMainCopy, _ => CurrentNomenklMain != null); }
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
            var form = new NomenklMainCardView { Owner = Application.Current.MainWindow, DataContext = ctx };
            //form.DataContext = ctx;
            form.Show();
        }

        public ICommand NomenklMainDeleteCommand
        {
            get { return new Command(NomenklMainDelete, _ => CurrentNomenklMain != null); }
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

                            var prices = ctx.NOM_PRICE.Where(_ => _.NOM_DC == nom.DocCode);
                            foreach (var np in prices) ctx.NOM_PRICE.Remove(np);
                            var wd27 = ctx.WD_27.Where(_ => _.SKLW_NOMENKL_DC == nom.DocCode);
                            foreach (var w in wd27) ctx.WD_27.Remove(w);
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
            get { return new Command(TreeAddInRoot, _ => true); }
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
            var c = new NomenklGroup();
            c.LoadFromEntity(CurrentCategory.Entity);
            GlobalOptions.ReferencesCache.AddOrUpdate(c);
        }

        public ICommand TreeAddChildCommand
        {
            get { return new Command(TreeAddChild, _ => true); }
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
            var c = new NomenklGroup();
            c.LoadFromEntity(CurrentCategory.Entity);
            GlobalOptions.ReferencesCache.AddOrUpdate(c);
        }

        public ICommand TreeAddOneLevelCommand
        {
            get { return new Command(TreeAddOneLevel, _ => true); }
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
            var c = new NomenklGroup();
            c.LoadFromEntity(CurrentCategory.Entity);
            GlobalOptions.ReferencesCache.AddOrUpdate(c);
        }

        public ICommand TreeEditCommand
        {
            get { return new Command(TreeEdit, _ => true); }
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
            var oldCat = new NomenklGroupViewModel(CurrentCategory.Entity);
            CurrentCategory.Name = category.Name;
            CurrentCategory.Note = category.Note;
            if (CategorySave(CurrentCategory)) return;
            CurrentCategory.Name = oldCat.Name;
            CurrentCategory.Note = oldCat.Note;
            CurrentCategory.State = RowStatus.NotEdited;
            var c = new NomenklGroup();
            c.LoadFromEntity(CurrentCategory.Entity);
            GlobalOptions.ReferencesCache.AddOrUpdate(c);
        }

        private bool CategorySave(NomenklGroupViewModel cat)
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
            CurrentNomenklCollection.Clear();
            NomenklMainCollection.Clear();
            //return;
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.NomenklMain
                        .Include(_ => _.Countries)
                        .Include(_ => _.SD_119)
                        .Include(_ => _.SD_175)
                        .Include(_ => _.SD_82)
                        .Include(_ => _.SD_82)
                        .Include(_ => _.SD_50)
                        .AsNoTracking()
                        .Where(_ => _.FullName.ToUpper().Contains(SearchText.ToUpper()) ||
                                    _.Name.ToUpper().Contains(SearchText.ToUpper()) ||
                                    _.NomenklNumber.ToUpper().Contains(SearchText.ToUpper()) ||
                                    (_.SD_119 != null && _.SD_119.MC_NAME
                                        .ToUpper()
                                        .Contains(
                                            SearchText
                                                .ToUpper()))
                                    || _.SD_175.ED_IZM_NAME.ToUpper().Contains(SearchText.ToUpper())
                                    || (_.SD_82 != null && _.SD_82.CAT_NAME.ToUpper().Contains(SearchText.ToUpper()))
                        )
                        .ToList();
                    foreach (var nom in data)
                    {
                        var item = new NomenklMainViewModel(nom)
                        {
                            State = RowStatus.NotEdited
                        };
                        NomenklMainCollection.Add(item);
                    }
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
            CurrentNomenklCollection.Clear();
            NomenklMainCollection.Clear();
        }

        #endregion
    }
}
