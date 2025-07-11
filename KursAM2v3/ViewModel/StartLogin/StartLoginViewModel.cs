using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using Helper;
using KursAM2.Managers;
using KursAM2.Properties;
using KursAM2.Repositories.RedisRepository;
using KursAM2.ViewModel.Splash;
using KursDomain;
using KursDomain.DBContext;
using KursDomain.Documents.Employee;
using KursDomain.References;
using KursDomain.References.RedisCache;
using KursDomain.Repository;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace KursAM2.ViewModel.StartLogin
{
    public sealed class StartLoginViewModel : RSWindowViewModelBase
    {
        private readonly ConnectionMultiplexer redis;
        private readonly ISubscriber mySubscriber;


        private string myCurrentBoxItem;
        private string myCurrentPassword;
        private string myCurrentUser;
        private bool myIsConnectNotExecute;
        private bool myIsThemeAllow;
        private DataSource mySelectedDataSource;
        private string myVersionValue;

        public StartLoginViewModel(Window formWindow)
        {
            IsConnectNotExecute = true;
            try
            {
                redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
                mySubscriber = redis.GetSubscriber();
                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe(new RedisChannel(RedisMessageChannels.StartLogin, RedisChannel.PatternMode.Auto),
                        (_, message) =>
                        {
                            Console.WriteLine($@"Redis - {message}");
                            StartLoad(message);
                        });
            }
            catch
            {
                Console.WriteLine($@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
            }

            var iniFileName = Application.Current.Properties["DataPath"] + "\\User.ini";
            try
            {
                UserIniFile = new IniFileManager(iniFileName);
                InitIniFile(UserIniFile);
            }
            catch
            {
                if (File.Exists(iniFileName)) File.Delete(iniFileName);
                UserIniFile = new IniFileManager(iniFileName);
                InitIniFile(UserIniFile);
            }


            Form = formWindow;
            CurrentUser = UserIniFile.ReadINI("Start", "Login");
        }


        public IniFileManager UserIniFile { set; get; }


        public ObservableCollection<DataSource> ComboBoxItemSource { set; get; } =
            new ObservableCollection<DataSource>();

        public bool IsConnectNotExecute
        {
            get => myIsConnectNotExecute;
            set
            {
                if (value == myIsConnectNotExecute) return;
                myIsConnectNotExecute = value;
                RaisePropertyChanged();
            }
        }

        public DataSource SelectedDataSource
        {
            set
            {
                if (mySelectedDataSource == value) return;
                mySelectedDataSource = value;
                RaisePropertyChanged();
            }
            get => mySelectedDataSource;
        }

        public string VersionValue
        {
            set
            {
                if (myVersionValue == value) return;
                myVersionValue = value;
                RaisePropertyChanged();
            }
            get => myVersionValue;
        }

        public string CurrentUser
        {
            set
            {
                if (myCurrentUser == value) return;
                myCurrentUser = value;
                if (string.IsNullOrWhiteSpace(myCurrentUser) && !isUserExists(myCurrentUser)) return;
                GetDefaultCache();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ComboBoxItemSource));
            }
            get => myCurrentUser;
        }

        public string CurrentBoxItem
        {
            set
            {
                if (myCurrentBoxItem == value) return;
                myCurrentBoxItem = value;
                RaisePropertyChanged();
            }
            get => myCurrentBoxItem;
        }

        public string CurrentPassword
        {
            set
            {
                if (myCurrentPassword == value) return;
                myCurrentPassword = value;
                RaisePropertyChanged();
            }
            get => myCurrentPassword;
        }

        public ObservableCollection<string> ThemeNameList { set; get; } =
            new ObservableCollection<string>();

        private async void StartLoad(RedisValue message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            try
            {
                var msg = JsonConvert.DeserializeObject<RedisMessage>(message);
                if (msg == null) return;
                if (msg.Message == CurrentUser)
                    await bnOk_ClickAsync();
            }
            catch (JsonException ex)
            {
                Console.WriteLine(@"Ошибка десериализации в методе StartLoginViewModel.StartLoad");
                Console.WriteLine(ex.Message);
            }
        }

        private void InitIniFile(IniFileManager userIni)
        {
            if (!userIni.KeyExists("Login", "Start")) userIni.Write("Start", "Login", "");
            if (!userIni.KeyExists("LastDataBase", "Start")) userIni.Write("Start", "LastDataBase", "");
            if (!userIni.KeyExists("DefaultDataBase", "Start")) userIni.Write("Start", "DefaultDataBase", "");
            if (!userIni.KeyExists("System", "Version")) userIni.Write("Version", "System", "0");
            if (!userIni.KeyExists("Test", "Version")) userIni.Write("Version", "Test", "0");
            if (!userIni.KeyExists("Version", "Version")) userIni.Write("Version", "Version", "0");
        }

        #region command

        public ICommand bnOk_ClickCommand
        {
            get { return new Command(bnOk_Click, _ => true); }
        }

        public void bnOk_Click(object obj)
        {
            var view = Form as View.StartLogin;
            if (string.IsNullOrEmpty(CurrentUser) || CurrentUser.Trim() == string.Empty ||
                string.IsNullOrEmpty(CurrentPassword)
                || SelectedDataSource == null)
            {
                var wm = new WindowManager();
                var showMess = @"Имя пользователя и пароль должны быть обязательно заполнены.";
                var TitleText = "Ошибка входа в систему";
                var dlgRslt = wm.ShowKursDialog(showMess, TitleText, Brushes.Red,
                    WindowManager.Confirm);
                view.Dispatcher.Invoke(() => { view.pwdText.Focus(); });
                return;
            }

            UserIniFile.Write("Start", "Login", CurrentUser);
            UserIniFile.Write("Start", "LastDataBase", SelectedDataSource.ShowName);
            //SplashLoadBar();
            // ReSharper disable once InlineOutVariableDeclaration
            User newUser;
            if (!CheckAndSetUser(out newUser))
            {
                view.Dispatcher.Invoke(() => { view.pwdText.Focus(); });
                return;
            }

            using (var ctx = GlobalOptions.KursSystem())
            {
                var tileOrders = GlobalOptions.KursSystem().UserMenuOrder
                    .Where(_ => _.UserId == newUser.KursId).ToList();
                var tileItems = ctx.KursMenuGroup.ToList();
                var tileUsersItems = ctx.UserMenuRight.Where(_ => _.DBId == GlobalOptions.DataBaseId
                                                                  && _.LoginName.ToUpper() ==
                                                                  newUser.NickName.ToUpper())
                    .ToList();
                var tileGroupsTemp = new List<TileGroup>();
                var favorite = tileItems.FirstOrDefault(_ => _.Id == 11);
                if (favorite != null)
                {
                    var newfavGrp = new TileGroup
                    {
                        Id = favorite.Id,
                        Name = favorite.Name,
                        Notes = favorite.Note,
                        Picture = ImageManager.ByteToImage(favorite.Picture),
                        // ReSharper disable once PossibleInvalidOperationException
                        OrderBy = favorite.Id
                    };
                    tileGroupsTemp.Add(newfavGrp);
                }

                foreach (var grp in tileItems.OrderBy(_ => _.OrderBy))
                {
                    var grpOrd = tileOrders.FirstOrDefault(_ => _.IsGroup && _.TileId == grp.Id);
                    foreach (var t in grp.KursMenuItem)
                        if (tileUsersItems.Any(_ => _.MenuId == t.Id) && tileGroupsTemp.All(_ => _.Id != grp.Id))
                        {
                            var newGrp = new TileGroup
                            {
                                Id = grp.Id,
                                Name = grp.Name,
                                Notes = grp.Note,
                                Picture = ImageManager.ByteToImage(grp.Picture),
                                // ReSharper disable once PossibleInvalidOperationException
                                OrderBy = (int)(grpOrd != null ? grpOrd.Order : grp.Id)
                            };
                            tileGroupsTemp.Add(newGrp);
                        }
                }

                var tileGroups = new List<TileGroup>(tileGroupsTemp.OrderBy(_ => _.OrderBy));
                foreach (var grp in tileGroups)
                {
                    var tItems = new List<TileItem>();
                    foreach (var tile in ctx.KursMenuItem.Where(t => t.GroupId == grp.Id).ToList())
                    {
                        if (tileUsersItems.All(_ => _.MenuId != tile.Id)) continue;
                        var ord = tileOrders.FirstOrDefault(_ => !_.IsGroup && _.TileId == tile.Id);
                        var newTItem = new TileItem
                        {
                            Id = tile.Id,
                            Name = tile.Name,
                            Notes = tile.Note,
                            Picture = ImageManager.ByteToImage(tile.Picture),
                            GroupId = tile.GroupId,
                            // ReSharper disable once PossibleInvalidOperationException
                            OrderBy = (int)(ord != null ? ord.Order : tile.Id)
                        };
                        tItems.Add(newTItem);
                    }

                    grp.TileItems = new List<TileItem>(tItems.OrderBy(_ => _.OrderBy));
                }

                newUser.MainTileGroups = new List<TileGroup>(tileGroups.OrderBy(_ => _.OrderBy));
                newUser.Groups =
                    GlobalOptions.GetEntities().EXT_GROUPS.Select(
                            grp => new UserGroup { Id = grp.GR_ID, Name = grp.GR_NAME })
                        .ToList();
                var fav = ctx.UserMenuFavorites.Where(_ => _.DbId == GlobalOptions.DataBaseId
                                                           && _.UserId == newUser.KursId).ToList();
                foreach (var f in fav) newUser.MenuFavorites.Add(f);
                GlobalOptions.UserInfo = newUser;
                Helper.CurrentUser.UserInfo = newUser;
                GlobalOptions.SystemProfile = new SystemProfile();
            }

            
            SetUserProfile(newUser.NickName.ToUpper());
            //GlobalOptions.ReferencesCache = new ReferencesKursCache(new KursDBContext(GlobalOptions.SqlConnectionString).Context);
            GlobalOptions.ReferencesCache = new RedisCacheReferences();
            SetGlobalProfile();
            GlobalOptions.ReferencesCache.StartLoad();
            using (var dbContext = GlobalOptions.GetEntities())
            {
                var bankSql =
                    $"SELECT doc_code FROM HD_114 h WHERE h.USR_ID = {CustomFormat.DecimalToSqlDecimal(GlobalOptions.UserInfo.Id)}";
                var cashSql =
                    $"SELECT doc_code FROM HD_22 h WHERE h.USR_ID = {CustomFormat.DecimalToSqlDecimal(GlobalOptions.UserInfo.Id)}";

                foreach (var dc in dbContext.Database.SqlQuery<decimal>(bankSql)) newUser.BankAccess.Add(dc);
                foreach (var dc in dbContext.Database.SqlQuery<decimal>(cashSql)) newUser.CashAccess.Add(dc);
            }

            // ReSharper disable once PossibleNullReferenceException
            view.IsConnectSuccess = true;
            DialogResult = true;
            SaveСache(view.AvatarObj.Source);
            view.Close();
        }

        public async Task bnOk_ClickAsync()
        {
            var view = Form as View.StartLogin;
            if (string.IsNullOrEmpty(CurrentUser) || CurrentUser.Trim() == string.Empty ||
                string.IsNullOrEmpty(CurrentPassword)
                || SelectedDataSource == null)
            {
                var wm = new WindowManager();
                var showMess = @"Имя пользователя и пароль должны быть обязательно заполнены.";
                var TitleText = "Ошибка входа в систему";
                Form.Dispatcher.Invoke(() =>
                {
                    var dlgRslt = wm.ShowKursDialog(showMess, TitleText, Brushes.Red,
                        WindowManager.Confirm);
                    view.ButtonOK.Tag = "NotActive";
                    view.pwdText.Focus();
                    view.pwdText.SelectAll();
                });
                IsConnectNotExecute = true;
                return;
            }

            UserIniFile.Write("Start", "Login", CurrentUser);
            UserIniFile.Write("Start", "LastDataBase", SelectedDataSource.ShowName);
            //SplashLoadBar();
            // ReSharper disable once InlineOutVariableDeclaration
            User newUser;
            if (!CheckAndSetUser(out newUser, false))
            {
                view.Dispatcher.Invoke(() =>
                {
                    var showMess = @"Недопустимые имя пользователя или пароль.";
                    var TitleText = "Ошибка входа в систему!";

                    var wm = new WindowManager();
                    var dlgRslt = wm.ShowKursDialog(showMess, TitleText, Brushes.Red,
                        WindowManager.Confirm);
                    view.ButtonOK.Tag = "NotActive";
                    view.pwdText.Focus();
                    view.pwdText.SelectAll();
                });
                IsConnectNotExecute = true;
                return;
            }

            using (var ctx = GlobalOptions.KursSystem())
            {
                var tileOrders = await GlobalOptions.KursSystem().UserMenuOrder
                    .Where(_ => _.UserId == newUser.KursId).ToListAsync();
                var tileItems = await ctx.KursMenuGroup.ToListAsync();
                var tileUsersItems = await ctx.UserMenuRight.Where(_ => _.DBId == GlobalOptions.DataBaseId
                                                                        && _.LoginName.ToUpper() ==
                                                                        newUser.NickName.ToUpper())
                    .ToListAsync();
                view.Dispatcher.Invoke(() =>
                {
                    var tileGroupsTemp = new List<TileGroup>();
                    var favorite = tileItems.FirstOrDefault(_ => _.Id == 11);
                    if (favorite != null)
                    {
                        var newfavGrp = new TileGroup
                        {
                            Id = favorite.Id,
                            Name = favorite.Name,
                            Notes = favorite.Note,
                            Picture = ImageManager.ByteToImage(favorite.Picture),
                            // ReSharper disable once PossibleInvalidOperationException
                            OrderBy = favorite.Id
                        };
                        tileGroupsTemp.Add(newfavGrp);
                    }

                    foreach (var grp in tileItems.OrderBy(_ => _.OrderBy))
                    {
                        var grpOrd = tileOrders.FirstOrDefault(_ => _.IsGroup && _.TileId == grp.Id);
                        foreach (var t in grp.KursMenuItem)
                            if (tileUsersItems.Any(_ => _.MenuId == t.Id) && tileGroupsTemp.All(_ => _.Id != grp.Id))
                            {
                                var newGrp = new TileGroup
                                {
                                    Id = grp.Id,
                                    Name = grp.Name,
                                    Notes = grp.Note,
                                    Picture = ImageManager.ByteToImage(grp.Picture),
                                    // ReSharper disable once PossibleInvalidOperationException
                                    OrderBy = (int)(grpOrd != null ? grpOrd.Order : grp.Id)
                                };
                                tileGroupsTemp.Add(newGrp);
                            }
                    }

                    var tileGroups = new List<TileGroup>(tileGroupsTemp.OrderBy(_ => _.OrderBy));
                    foreach (var grp in tileGroups)
                    {
                        var tItems = new List<TileItem>();
                        foreach (var tile in ctx.KursMenuItem.Where(t => t.GroupId == grp.Id).ToList())
                        {
                            if (tileUsersItems.All(_ => _.MenuId != tile.Id)) continue;
                            var ord = tileOrders.FirstOrDefault(_ => !_.IsGroup && _.TileId == tile.Id);
                            var newTItem = new TileItem
                            {
                                Id = tile.Id,
                                Name = tile.Name,
                                Notes = tile.Note,
                                Picture = ImageManager.ByteToImage(tile.Picture),
                                GroupId = tile.GroupId,
                                // ReSharper disable once PossibleInvalidOperationException
                                OrderBy = (int)(ord != null ? ord.Order : tile.Id)
                            };
                            tItems.Add(newTItem);
                        }

                        grp.TileItems = new List<TileItem>(tItems.OrderBy(_ => _.OrderBy));
                    }

                    newUser.MainTileGroups = new List<TileGroup>(tileGroups.OrderBy(_ => _.OrderBy));
                    newUser.Groups = GlobalOptions.GetEntities().EXT_GROUPS.Select(
                            grp => new UserGroup { Id = grp.GR_ID, Name = grp.GR_NAME })
                        .ToList();
                    var fav = ctx.UserMenuFavorites.Where(_ => _.DbId == GlobalOptions.DataBaseId
                                                               && _.UserId == newUser.KursId).ToList();
                    foreach (var f in fav) newUser.MenuFavorites.Add(f);
                    GlobalOptions.UserInfo = newUser;
                    Helper.CurrentUser.UserInfo = newUser;
                    GlobalOptions.SystemProfile = new SystemProfile();
                });
            }

            //GlobalOptions.ReferencesCache = new ReferencesKursCache(new KursDBContext(GlobalOptions.SqlConnectionString).Context); 
            GlobalOptions.ReferencesCache = new RedisCacheReferences();
            SetGlobalProfile();
            GlobalOptions.ReferencesCache.StartLoad();
           
            SetUserProfile(newUser.NickName.ToUpper());
            using (var dbContext = GlobalOptions.GetEntities())
            {
                var bankSql =
                    $"SELECT doc_code FROM HD_114 h WHERE h.USR_ID = {CustomFormat.DecimalToSqlDecimal(GlobalOptions.UserInfo.Id)}";
                var cashSql =
                    $"SELECT doc_code FROM HD_22 h WHERE h.USR_ID = {CustomFormat.DecimalToSqlDecimal(GlobalOptions.UserInfo.Id)}";

                foreach (var dc in dbContext.Database.SqlQuery<decimal>(bankSql)) newUser.BankAccess.Add(dc);
                foreach (var dc in dbContext.Database.SqlQuery<decimal>(cashSql)) newUser.CashAccess.Add(dc);
            }

            // ReSharper disable once PossibleNullReferenceException
            Form.Dispatcher.Invoke(() =>
            {
                view.IsConnectSuccess = true;
                DialogResult = true;
                SaveСache(view.AvatarObj.Source);
                view.Close();
            });
            IsConnectNotExecute = true;
        }

        private static void SetGlobalProfile()
        {
            try
            {
                GlobalOptions.SystemProfile.Profile.Clear();
                foreach (var p in GlobalOptions.GetEntities().PROFILE)
                    GlobalOptions.SystemProfile.Profile.Add(p);
                var ownKontrDC =
                    GlobalOptions.SystemProfile.Profile.FirstOrDefault(
                        _ => _.SECTION == "COMPANY" && _.ITEM == "DOC_CODE");
                if (ownKontrDC != null)
                {
                    // ReSharper disable once InconsistentNaming
                    var DC = Convert.ToDecimal(ownKontrDC.ITEM_VALUE);
                    var ownKontr =
                        GlobalOptions.GetEntities().SD_43.Include(_ => _.TD_43)
                            .Include(_ => _.TD_43.Select(x => x.SD_44))
                            .SingleOrDefault(_ => _.DOC_CODE == DC);
                    if (ownKontr != null)
                        GlobalOptions.SystemProfile.OwnerKontragent = new Kontragent();
                    GlobalOptions.SystemProfile.OwnerKontragent.LoadFromEntity(ownKontr, GlobalOptions.ReferencesCache);
                }

                var mainCrsDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(
                    _ => _.SECTION == "CURRENCY" && _.ITEM == "MAIN");
                if (mainCrsDC != null)
                {
                    var dc = Convert.ToDecimal(mainCrsDC.ITEM_VALUE);
                    var mainCrs =
                        GlobalOptions.GetEntities().SD_301.Single(_ => _.DOC_CODE == dc);
                    var crs = new Currency();
                    crs.LoadFromEntity(mainCrs);
                    GlobalOptions.SystemProfile.MainCurrency = crs;
                }

                var nationalCrsDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(
                    _ => _.SECTION == "CURRENCY" && _.ITEM == "ОСНОВНАЯ_В_ГОСУДАРСТВЕ");
                if (nationalCrsDC != null)
                {
                    var dc = Convert.ToDecimal(nationalCrsDC.ITEM_VALUE);
                    var nationalCrs =
                        GlobalOptions.GetEntities().SD_301.Single(_ => _.DOC_CODE == dc);
                    var crs = new Currency();
                    crs.LoadFromEntity(nationalCrs);
                    GlobalOptions.SystemProfile.NationalCurrency = crs;
                }

                var employeeDefaultCurrencyDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(
                    _ => _.SECTION == "ЗАРПЛАТА" && _.ITEM == "ВАЛЮТА")?.ITEM_VALUE;
                if (employeeDefaultCurrencyDC != null)
                {
                    var dc = Convert.ToDecimal(employeeDefaultCurrencyDC);
                    var crs =
                        GlobalOptions.GetEntities().SD_301.Single(_ => _.DOC_CODE == dc);
                    var c = new Currency();
                    c.LoadFromEntity(crs);
                    GlobalOptions.SystemProfile.EmployeeDefaultCurrency = c;
                }
                else
                {
                    GlobalOptions.SystemProfile.EmployeeDefaultCurrency = GlobalOptions.SystemProfile.NationalCurrency;
                }

                var prTypeDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(
                    _ => _.SECTION == "ЗAРПЛАТА" && _.ITEM == "НАЧИСЛЕНИЕ_ПО_УМОЛЧАНИЮ")?.ITEM_VALUE;
                if (prTypeDC != null)
                {
                    var dc = Convert.ToDecimal(prTypeDC);
                    var prtype = GlobalOptions.GetEntities().EMP_PAYROLL_TYPE.FirstOrDefault(_ => _.DOC_CODE == dc);
                    GlobalOptions.SystemProfile.DafaultPayRollType =
                        prtype == null ? null : new EMP_PAYROLL_TYPEViewModel(prtype);
                }

                // ReSharper disable once PossibleNullReferenceException
                var nomCalcType = GlobalOptions.SystemProfile.Profile.FirstOrDefault(
                        _ => _.SECTION == "NOMENKL_CALC" && _.ITEM == "TYPE")
                    .ITEM_VALUE;
                switch (nomCalcType)
                {
                    case "0":
                        GlobalOptions.SystemProfile.NomenklCalcType = NomenklCalcType.Standart;
                        break;
                    case "1":
                        GlobalOptions.SystemProfile.NomenklCalcType = NomenklCalcType.NakladSeparately;
                        break;
                }
            }
            catch (Exception ex)
            {
                var errText = new StringBuilder(ex.Message);
                while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");
                MessageBox.Show("StartLoginViewModel SetGlobalProfile  error.\n" + errText);
            }
        }

        private bool CheckAndSetUser(out User newUser, bool isMessageShow = true)
        {
            var WinManager = new WindowManager();
            try
            {
                using (var kursSystemCtx = GlobalOptions.KursSystem())
                {
                    var u = kursSystemCtx.Users.Include(_ => _.DataSources).FirstOrDefault(_ => _.Name == CurrentUser);
                    if (u == null && isMessageShow)
                    {
                        WinManager.ShowMessageBox($"Пользователь {CurrentUser} в системе не зарегистрирован",
                            "Ошибка входа");
                        newUser = null;
                        return false;
                    }

                    if (u.IsDeleted && isMessageShow)
                    {
                        WinManager.ShowMessageBox(
                            $"Пользователю {CurrentUser} запрещен вход в систему.Обратитесь к администратору.",
                            "Ошибка входа");
                        newUser = null;
                        return false;
                    }

                    if (u.IsDeleted && isMessageShow)
                    {
                        WinManager.ShowMessageBox(
                            $"Пользователю {CurrentUser} не назначен доступ ни к одной базе данных. " +
                            "Обратитесь к администратору.",
                            "Ошибка входа");
                        newUser = null;
                        return false;
                    }
                }

                string hostName;
                var section = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
#if DEBUG
                hostName = section.Get("KursSystemDebugHost");
#else
            hostName = section.Get("KursSystemHost");
#endif

                GlobalOptions.DataBaseName = SelectedDataSource.ShowName;
                GlobalOptions.DataBaseId = SelectedDataSource.Id;
                GlobalOptions.RedisDBId = SelectedDataSource.RedisDBId;
                GlobalOptions.DatabaseColor = SelectedDataSource.Color;
                GlobalOptions.SqlConnectionString =
                    SelectedDataSource.GetConnectionString(CurrentUser, CurrentPassword);
                GlobalOptions.KursDBContext = new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString);
                GlobalOptions.KursDBUnitOfWork = new UnitOfWork<ALFAMEDIAEntities>(GlobalOptions.KursDBContext);
                GlobalOptions.KursSystemDBContext = new KursSystemEntities(new SqlConnectionStringBuilder
                {
                    DataSource = hostName,
                    InitialCatalog = "KursSystem",
                    UserID = "KursUser",
                    Password = "KursUser",
                    ConnectTimeout = 0,
                    ApplicationName = "KursAM"
                }.ToString());
                GlobalOptions.KursSystemDBUnitOfWork =
                    new UnitOfWork<KursSystemEntities>(GlobalOptions.KursSystemDBContext);
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var usr = ctx.EXT_USERS
                        .FirstOrDefault(_ => _.USR_NICKNAME.ToUpper() == CurrentUser.ToUpper());
                    if (usr == null)
                    {
                        WindowManager.ShowMessage(null, "Неправильный пароль или пользователь.",
                            "Ошибка",
                            MessageBoxImage.Error);
                        newUser = null;
                        return false;
                    }

                    newUser = new User
                    {
                        Id = Convert.ToInt32(usr.USR_ID),
                        Name = usr.USR_NICKNAME,
                        NickName = usr.USR_NICKNAME,
                        Notes = usr.USR_NOTES,
                        FullName = usr.USR_FULLNAME,
                        TabelNumber = usr.TABELNUMBER,
                        Phone = usr.USR_PHONE
                    };
                    using (var sysctx = GlobalOptions.KursSystem())
                    {
                        var u = sysctx.Users.FirstOrDefault(_ => _.Name == usr.USR_NICKNAME);
                        if (u != null)
                        {
                            newUser.KursId = u.Id;
                            newUser.IsAdmin = u.IsAdmin;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var exx = ex;
                var errText = new StringBuilder(exx.Message);
                while (exx.InnerException != null)
                {
                    errText.Append($"\n {exx.InnerException.Message}");
                    exx = exx.InnerException;
                }

                if (isMessageShow)
                {
                    var showMess = @"Недопустимые имя пользователя или пароль.";
                    var TitleText = "Ошибка входа в систему!";

                    var wm = new WindowManager();
                    var dlgRslt = wm.ShowKursDialog(showMess, TitleText, Brushes.Red,
                        WindowManager.Confirm);
                }

                newUser = null;
                return false;
            }

            return true;
        }

        private static void SetUserProfile(string userName)
        {
            try
            {
                GlobalOptions.UserInfo.Profile.AddRange(
                    GlobalOptions.GetEntities().UserProfile.Where(_ => _.EXT_USERS.USR_NICKNAME == userName));
            }
            catch (Exception ex)
            {
                var errText = new StringBuilder(ex.Message);
                while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");
                MessageBox.Show("SetUserProfile error.\n" + errText);
            }
        }

        private bool isUserExists(string userName)
        {
            string hostName;
            var section = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
#if DEBUG
            hostName = section.Get("KursSystemDebugHost");
#else
            hostName = section.Get("KursSystemHost");
#endif
            if (string.IsNullOrWhiteSpace(CurrentUser)) return false;

            ComboBoxItemSource.Clear();
            var connection = new SqlConnectionStringBuilder
            {
                DataSource = hostName,
                InitialCatalog = "KursSystem",
                UserID = "KursUser",
                Password = "KursUser"
            };
            try
            {
                using (var conn = new SqlConnection(connection.ConnectionString))
                {
                    conn.Open();
                    var command = new SqlCommand
                    {
                        CommandText =
                            $"SELECT d.Name AS Name FROM Users u WHERE  UPPER(u.Name) = '{CurrentUser.ToUpper()}'  ",

                        Connection = conn
                    };
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) return true;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                var errText = new StringBuilder(ex.Message);
                while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");
                MessageBox.Show("LoadDataSource error.\n" + errText);
                return false;
            }
        }

        private void LoadDataSources()
        {
            string hostName;
            var section = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
#if DEBUG
            hostName = section.Get("KursSystemDebugHost");
#else
            hostName = section.Get("KursSystemHost");
#endif
            if (CurrentUser == null) return;
            ComboBoxItemSource.Clear();
            var connection = new SqlConnectionStringBuilder
            {
                DataSource = hostName,
                InitialCatalog = "KursSystem",
                UserID = "KursUser",
                Password = "KursUser"
            };
            SqlConnection conn = null;
            try
            {
                using (conn = new SqlConnection(connection.ConnectionString))
                {
                    conn.Open();
                    var command = new SqlCommand
                    {
                        CommandText =
                            "SELECT d.Name AS Name, d.ShowName AS ShowName, d.[Order] AS 'Order', d.Server as 'Server', " +
                            "d.DBName AS 'DBName', d.Color AS 'Color', d.Id as 'Id', d.RedisDBId as 'RedisDBId'  FROM DataSources d " +
                            "INNER JOIN  UsersLinkDB link ON link.DBId = d.Id " +
                            $"INNER JOIN  Users u ON u.Id = link.UserId AND UPPER(u.Name) = '{CurrentUser.ToUpper()}'  " +
                            "WHERE isnull(IsVisible,0) = 1 " +
                            "ORDER BY 3",
                        Connection = conn
                    };
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            ComboBoxItemSource.Add(new DataSource
                            {
                                Name = (string)reader.GetValue(0),
                                ShowName = (string)reader.GetValue(1),
                                Order = (int)reader.GetValue(2),
                                Server = (string)reader.GetValue(3),
                                DBName = (string)reader.GetValue(4),
                                Color = (SolidColorBrush)new BrushConverter().ConvertFromString(
                                    (string)reader.GetValue(5)),
                                Id = reader.GetGuid(6),
                                RedisDBId =  (int?)reader.GetValue(7),
                            });
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                var errText = new StringBuilder(ex.Message);
                while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");
                MessageBox.Show("LoadDataSource error.\n" + errText);
                if (conn != null && conn.State == ConnectionState.Open) conn.Close();
            }
        }

        public void SplashLoadBar()
        {
            var vm = new DebitorCreditorCalcKontrSplashViewModel
            {
                Progress = 0,
                MaxProgress = 100,
                Minimum = 0,
                ExtendExtendedTextVisibility = Visibility.Visible
            };
            if (SplashScreenService == null) return;
            SplashScreenService.ShowSplashScreen();
            SplashScreenService.SetSplashScreenState(vm);
        }

        public void GetDefaultCache()
        {
            try
            {
                using (var ctx = GlobalOptions.KursSystem())
                {
                    var view = Form as View.StartLogin;
                    var user = ctx.Users.FirstOrDefault(_ => _.Name == CurrentUser);
                    if (user == null)
                        // ReSharper disable once PossibleNullReferenceException
                    {
                        view.AvatarObj.Source =
                            new BitmapImage(new Uri("./../Images/businessman.png", UriKind.Relative));
                    }
                    else
                    {
                        IsThemeAllow = true;
                        // ReSharper disable once PossibleNullReferenceException
                        view.AvatarObj.Source = user.Avatar != null
                            ? ImageManager.ByteToImageSource(user.Avatar)
                            : new BitmapImage(new Uri("./../Images/businessman.png", UriKind.Relative));
                        ApplicationThemeHelper.ApplicationThemeName = user.ThemeName;
                        ApplicationThemeHelper.UpdateApplicationThemeName();
                    }

                    if (!string.IsNullOrWhiteSpace(Settings.Default.LastDataBase))
                        CurrentBoxItem = Settings.Default.LastDataBase;
                }
            }
            catch (Exception ex)
            {
                var showMess = @"Нет связи с сервером. Запуск системы не возможен.";
                var TitleText = "Ошибка запуска";
                var wm = new WindowManager();
                var dlgRslt = wm.ShowKursDialog(showMess, TitleText, Brushes.Red,
                    WindowManager.Confirm);
                Environment.Exit(1);

                //var errText = new StringBuilder(ex.Message);
                //while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");
                //MessageBox.Show("KursSystem error.\n" + errText);
            }

            //Version cache
            var vers = new VersionManager(null);
            var ver = vers.CheckVersion();
            if (ver != null)
            {
                GlobalOptions.Version = $"Версия {ver.Major}.{ver.Minor}.{ver.Ver}";
                GlobalOptions.VersionType = ver.Serverpath.Contains("KursApp") ? "(бета версия)" : null;
                VersionValue = $"Версия {ver.Major}.{ver.Minor}.{ver.Ver} {GlobalOptions.VersionType}";
                if (ver.UpdateStatus == 2)
                {
                    var verCheck = vers.GetCanUpdate();
                    if (verCheck == false) return;
                    vers.KursUpdate();
                }
            }

            LoadDataSources();
            if (!string.IsNullOrWhiteSpace(UserIniFile.ReadINI("Start", "LastDataBase")))
                SelectedDataSource =
                    ComboBoxItemSource.FirstOrDefault(_ => _.ShowName == UserIniFile.ReadINI("Start", "LastDataBase"));
        }


        public void SaveСache(ImageSource data)
        {
            using (var ctx = GlobalOptions.KursSystem())
            {
                try
                {
                    var user = ctx.Users.FirstOrDefault(_ => _.Id == GlobalOptions.UserInfo.KursId);
                    if (user != null) user.Avatar = ImageManager.ImageSourceToBytes(new PngBitmapEncoder(), data);
                    ctx.SaveChanges();
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["Login"].Value = CurrentUser;
                    config.Save();
                }
                catch (Exception ex)
                {
                    var errText = new StringBuilder(ex.Message);
                    while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");
                    MessageBox.Show("LocalDbContext SaveСache error.\n" + errText);
                }
            }
        }

        public bool IsThemeAllow
        {
            get => myIsThemeAllow;
            set
            {
                if (myIsThemeAllow == value) return;
                myIsThemeAllow = value;
                RaisePropertyChanged();
            }
        }

        #endregion
    }
}
