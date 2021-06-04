using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core;
using Core.EntityViewModel;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.EntityViewModel.Employee;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using Data.Repository;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using Helper;
using KursAM2.Managers;
using KursAM2.Properties;
using KursAM2.ViewModel.Splash;
using User = Helper.User;

namespace KursAM2.ViewModel.StartLogin
{
    public sealed class StartLoginViewModel : RSWindowViewModelBase
    {
        private string myCurrentBoxItem;
        private string myCurrentPassword;
        private string myCurrentUser;
        private bool myIsThemeAllow;
        private DataSource mySelectedDataSource;
        private string myVersionValue;

        public StartLoginViewModel(Window formWindow)
        {
            UserIniFile = new IniFileManager(Application.Current.Properties["DataPath"] + "\\User.ini");
            InitIniFile(UserIniFile);
            Form = formWindow;
            CurrentUser = UserIniFile.ReadINI("Start", "Login");
            //GetDefaultCache();
        }

        private ISplashScreenService SplashScreenService => GetService<ISplashScreenService>();
        public IniFileManager UserIniFile { set; get; }

        public ObservableCollection<DataSource> ComboBoxItemSource { set; get; } =
            new ObservableCollection<DataSource>();

        public DataSource SelectedDataSource
        {
            set
            {
                if (mySelectedDataSource == value) return;
                mySelectedDataSource = value;
                RaisePropertiesChanged();
            }
            get => mySelectedDataSource;
        }

        public string VersionValue
        {
            set
            {
                if (myVersionValue == value) return;
                myVersionValue = value;
                RaisePropertiesChanged();
            }
            get => myVersionValue;
        }

        public string CurrentUser
        {
            set
            {
                if (myCurrentUser == value) return;
                myCurrentUser = value;
                if (string.IsNullOrWhiteSpace(myCurrentUser)) return;
                LoadDataSources();
                GetDefaultCache();
                RaisePropertiesChanged();
                RaisePropertiesChanged(nameof(ComboBoxItemSource));
            }
            get => myCurrentUser;
        }

        public string CurrentBoxItem
        {
            set
            {
                if (myCurrentBoxItem == value) return;
                myCurrentBoxItem = value;
                RaisePropertiesChanged();
            }
            get => myCurrentBoxItem;
        }

        public string CurrentPassword
        {
            set
            {
                if (myCurrentPassword == value) return;
                myCurrentPassword = value;
                RaisePropertiesChanged();
            }
            get => myCurrentPassword;
        }

        public ObservableCollection<string> ThemeNameList { set; get; } =
            new ObservableCollection<string>();

        private void InitIniFile(IniFileManager userIni)
        {
            if (!userIni.KeyExists("Login", "Start")) userIni.Write("Start", "Login", "");
            if (!userIni.KeyExists("LastDataBase", "Start")) userIni.Write("Start", "LastDataBase", "");
            if (!userIni.KeyExists("DefaultDataBase", "Start")) userIni.Write("Start", "DefaultDataBase", "");
        }

        #region command

        // ReSharper disable once InconsistentNaming
        public ICommand bnOk_ClickCommand
        {
            get { return new Command(bnOk_Click, _ => true); }
        }

        private void bnOk_Click(object obj)
        {
            var view = Form as View.StartLogin;
            if (string.IsNullOrEmpty(CurrentUser) || CurrentUser.Trim() == string.Empty ||
                string.IsNullOrEmpty(CurrentPassword)
                || SelectedDataSource == null)
            {
                WindowManager.ShowMessage(view, "Имя пользователя и пароль должны быть обязательно заполнены.",
                    "Ошибка",
                    MessageBoxImage.Question);
                return;
            }

            UserIniFile.Write("Start", "Login", CurrentUser);
            UserIniFile.Write("Start", "LastDataBase", SelectedDataSource.ShowName);
            SplashLoadBar();
            // ReSharper disable once InlineOutVariableDeclaration
            User newUser;
            if (!CheckAndSetUser(out newUser)) return;
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
                                OrderBy = (int) (grpOrd != null ? grpOrd.Order : grp.Id)
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
                        tItems.Add(new TileItem
                        {
                            Id = tile.Id,
                            Name = tile.Name,
                            Notes = tile.Note,
                            Picture = ImageManager.ByteToImage(tile.Picture),
                            GroupId = tile.GroupId,
                            // ReSharper disable once PossibleInvalidOperationException
                            OrderBy = (int) (ord != null ? ord.Order : tile.Id)
                        });
                    }

                    grp.TileItems = new List<TileItem>(tItems.OrderBy(_ => _.OrderBy));
                }

                newUser.MainTileGroups = new List<TileGroup>(tileGroups.OrderBy(_ => _.OrderBy));
                newUser.Groups =
                    GlobalOptions.GetEntities().EXT_GROUPS.Select(
                            grp => new UserGroup {Id = grp.GR_ID, Name = grp.GR_NAME})
                        .ToList();
                GlobalOptions.UserInfo = newUser;
                Helper.CurrentUser.UserInfo = newUser;
                GlobalOptions.SystemProfile = new SystemProfile();
            }

            SetUserProfile(newUser.NickName.ToUpper());
            SetGlobalProfile();
            // ReSharper disable once PossibleNullReferenceException
            view.IsConnectSuccess = true;
            DialogResult = true;
            SaveСache(view.AvatarObj.Source);
            view.Close();
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
                        GlobalOptions.GetEntities().SD_43.Single(_ => _.DOC_CODE == DC);
                    GlobalOptions.SystemProfile.OwnerKontragent =
                        new Kontragent(ownKontr);
                }

                var mainCrsDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(
                    _ => _.SECTION == "CURRENCY" && _.ITEM == "MAIN");
                if (mainCrsDC != null)
                {
                    var dc = Convert.ToDecimal(mainCrsDC.ITEM_VALUE);
                    var mainCrs =
                        GlobalOptions.GetEntities().SD_301.Single(_ => _.DOC_CODE == dc);
                    GlobalOptions.SystemProfile.MainCurrency = new Currency(mainCrs);
                }

                var nationalCrsDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(
                    _ => _.SECTION == "CURRENCY" && _.ITEM == "ОСНОВНАЯ_В_ГОСУДАРСТВЕ");
                if (nationalCrsDC != null)
                {
                    var dc = Convert.ToDecimal(nationalCrsDC.ITEM_VALUE);
                    var nationalCrs =
                        GlobalOptions.GetEntities().SD_301.Single(_ => _.DOC_CODE == dc);
                    GlobalOptions.SystemProfile.NationalCurrency =
                        new Currency(nationalCrs);
                }

                var employeeDefaultCurrencyDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(
                    _ => _.SECTION == "ЗАРПЛАТА" && _.ITEM == "ВАЛЮТА")?.ITEM_VALUE;
                if (employeeDefaultCurrencyDC != null)
                {
                    var dc = Convert.ToDecimal(employeeDefaultCurrencyDC);
                    var crs =
                        GlobalOptions.GetEntities().SD_301.Single(_ => _.DOC_CODE == dc);
                    GlobalOptions.SystemProfile.EmployeeDefaultCurrency = new Currency(crs);
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

        private bool CheckAndSetUser(out User newUser)
        {
            try
            {
                using (var kursSystemCtx = GlobalOptions.KursSystem())
                {
                    var u = kursSystemCtx.Users.Include(_ => _.DataSources).FirstOrDefault(_ => _.Name == CurrentUser);
                    if (u == null)
                    {
                        WinManager.ShowMessageBox($"Пользователь {CurrentUser} в системе не зарегистрирован",
                            "Ошибка входа");
                        newUser = null;
                        return false;
                    }

                    if (u.IsDeleted)
                    {
                        WinManager.ShowMessageBox(
                            $"Пользователю {CurrentUser} запрещен вход в систему.Обратитесь к администратору.",
                            "Ошибка входа");
                        newUser = null;
                        return false;
                    }

                    if (u.IsDeleted)
                    {
                        WinManager.ShowMessageBox(
                            $"Пользователю {CurrentUser} не назначен доступ ни к одной базе данных. " +
                            "Обратитесь к администратору.",
                            "Ошибка входа");
                        newUser = null;
                        return false;
                    }
                }

                GlobalOptions.DataBaseName = SelectedDataSource.ShowName;
                GlobalOptions.DataBaseId = SelectedDataSource.Id;
                GlobalOptions.DatabaseColor = SelectedDataSource.Color;
                GlobalOptions.SqlConnectionString =
                    SelectedDataSource.GetConnectionString(CurrentUser, CurrentPassword);
                GlobalOptions.KursDBContext = new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString);
                GlobalOptions.KursDBUnitOfWork = new UnitOfWork<ALFAMEDIAEntities>(GlobalOptions.KursDBContext);
                GlobalOptions.KursSystemDBContext = new KursSystemEntities(new SqlConnectionStringBuilder
                {
                    DataSource = "172.16.0.1",
                    InitialCatalog = "KursSystem",
                    UserID = "sa",
                    Password = "CbvrfFhntvrf65"
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
                            newUser.KursId = u.Id;
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

                MessageBox.Show("CheckAndSetUser error.\n" + errText);
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

        private void LoadDataSources()
        {
            if (CurrentUser == null) return;
            ComboBoxItemSource.Clear();
            var connection = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.0.1",
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
                            "SELECT d.Name AS Name, d.ShowName AS ShowName, d.[Order] AS 'Order', d.Server as 'Server', " +
                            "d.DBName AS 'DBName', d.Color AS 'Color', d.Id as 'Id'  FROM DataSources d " +
                            "INNER JOIN  UsersLinkDB link ON link.DBId = d.Id " +
                            $"INNER JOIN  Users u ON u.Id = link.UserId AND UPPER(u.Name) = '{CurrentUser.ToUpper()}' " +
                            "ORDER BY 3",
                        Connection = conn
                    };
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            ComboBoxItemSource.Add(new DataSource
                            {
                                Name = (string) reader.GetValue(0),
                                ShowName = (string) reader.GetValue(1),
                                Order = (int) reader.GetValue(2),
                                Server = (string) reader.GetValue(3),
                                DBName = (string) reader.GetValue(4),
                                Color = (SolidColorBrush) new BrushConverter().ConvertFromString(
                                    (string) reader.GetValue(5)),
                                Id = reader.GetGuid(6)
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                var errText = new StringBuilder(ex.Message);
                while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");
                MessageBox.Show("LoadDataSource error.\n" + errText);
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
                var errText = new StringBuilder(ex.Message);
                while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");
                MessageBox.Show("KursSystem error.\n" + errText);
            }

            //Version cache
            var vers = new VersionManager();
            var ver = vers.CheckVersion();
            if (ver != null)
            {
                GlobalOptions.Version = $"Версия {ver.Major}.{ver.Minor}.{ver.Ver}";
                VersionValue = $"Версия {ver.Major}.{ver.Minor}.{ver.Ver}";
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
                RaisePropertiesChanged();
            }
        }

        #endregion
    }
}