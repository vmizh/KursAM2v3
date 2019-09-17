using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using Helper;
using KursAM2.Managers;
using KursAM2.ViewModel.Splash;
using SQLite.Base;
using SQLite.Base.Entity;
using User = Helper.User;

namespace KursAM2.ViewModel.StartLogin
{
    public sealed class StartLoginViewModel : RSWindowViewModelBase
    {
        private string myCurrentBoxItem;
        private string myCurrentPassword;
        private string myCurrentUser;
        private string myVersionValue;

        public StartLoginViewModel(Window formWindow)
        {
            Form = formWindow;
            GetDefaultCache();
        }

        private ISplashScreenService SplashScreenService => GetService<ISplashScreenService>();

        public ObservableCollection<DataSource> ComboBoxItemSource { set; get; } =
            new ObservableCollection<DataSource>();

        public DataSource SelectedDataSource { set; get; }

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
                if (string.IsNullOrEmpty(myCurrentUser) || string.IsNullOrEmpty(myCurrentUser)) return;
                LoadDataSources();
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

        #region command

        // ReSharper disable once InconsistentNaming
        public ICommand bnOk_ClickCommand
        {
            get { return new Command(bnOk_Click, _ => true); }
        }

        private void bnOk_Click(object obj)
        {
            try
            {
                var view = Form as View.StartLogin;
                if (string.IsNullOrEmpty(CurrentUser) || CurrentUser.Trim() == string.Empty ||
                    string.IsNullOrEmpty(CurrentPassword)
                    || SelectedDataSource == null) {
                    WindowManager.ShowMessage(view, "Имя пользователя и пароль должны быть обязательно заполнены.",
                        "Ошибка",
                        MessageBoxImage.Question);
                    return;
                }
                else
                    SplashLoadBar();
                User newUser;
                if (!CheckAndSetUser(out newUser)) return;
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var tileItems = (from item in ctx.MAIN_DOCUMENT_ITEM
                            join d in ctx.USER_FORMS_RIGHT on item.ID equals
                                d.FORM_ID
                            where Equals(d.USER_NAME.ToUpper(), newUser.NickName.ToUpper())
                            select item).OrderBy(_ => _.ID)
                        .ToList();
                    var tileGrps = new List<MAIN_DOCUMENT_GROUP>();
                    var titems = tileItems.Where(tile => !tileGrps.Contains(tile.MAIN_DOCUMENT_GROUP)).ToList();
                    tileGrps.AddRange(titems.Select(tile => tile.MAIN_DOCUMENT_GROUP));
                    var tileGroups = new List<TileGroup>();
                    foreach (var grp in tileGrps)
                    {
                        var newGrp = new TileGroup
                        {
                            Id = grp.ID,
                            Name = grp.NAME,
                            Notes = grp.NOTES,
                            Picture = ImageManager.ByteToImage(grp.PICTURE)
                        };
                        var grp1 = grp;
                        foreach (var tile in tileItems.Where(t => t.GROUP_ID == grp1.ID))
                            newGrp.TileItems.Add(new TileItem
                            {
                                Id = tile.ID,
                                Name = tile.NAME,
                                Notes = tile.NOTES,
                                Picture = ImageManager.ByteToImage(tile.PICTURE),
                                GroupId = tile.GROUP_ID
                            });
                        if (tileGroups.All(g => g.Id != newGrp.Id))
                            tileGroups.Add(newGrp);
                    }

                    newUser.MainTileGroups = tileGroups;
                    newUser.Groups =
                        ctx.EXT_GROUPS.Select(
                                grp => new UserGroup {Id = grp.GR_ID, Name = grp.GR_NAME})
                            .ToList();
                    GlobalOptions.UserInfo = newUser;
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
            catch
                (Exception ex)
            {
                var errText = new StringBuilder(ex.Message);
                while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");

                MessageBox.Show("StartLogionBnOK error.\n" + errText);
            }
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

            //GlobalOptions.MainReferences = new MainReferences();
            //GlobalOptions.MainReferences.Reset();
        }

        private bool CheckAndSetUser(out User newUser)
        {
            try
            {
                GlobalOptions.DataBaseName = SelectedDataSource.ShowName;
                GlobalOptions.DataBaseId = SelectedDataSource.Id;
                GlobalOptions.DatabaseColor = SelectedDataSource.Color;
                GlobalOptions.SqlConnectionString =
                    SelectedDataSource.GetConnectionString(CurrentUser, CurrentPassword);
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var usr = ctx.EXT_USERS
                        .FirstOrDefault(_ => _.USR_NICKNAME.ToUpper() == CurrentUser.ToUpper());
                    if (usr == null)
                    {
                        WindowManager.ShowMessage(null, "Неправильный пароль или пользователь.", "Ошибка",
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
                }
            }
            catch (Exception ex)
            {
                //HideSplash();
                //if (ex.Message == "The underlying provider failed on Open.")
                //    WindowManager.ShowMessage(
                //        Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive),
                //        "Логин или пароль не совпадают.",
                //        "Ошибка регистрации", MessageBoxImage.Error);
                //else
                //    WindowManager.ShowError(ex);
                var errText = new StringBuilder(ex.Message);
                while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");

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

//        public void LoadDataSources()
//        {
//            var xmlDoc = new XmlDocument();
//            xmlDoc.Load($"{Environment.CurrentDirectory}\\Layout\\DataSources.xml");
//            var nodeList = xmlDoc.DocumentElement?.SelectNodes("/DataSources/DataSource");
//            if (nodeList == null) return;
//            foreach (XmlNode node in nodeList)
//            {
//                var attrs = node.Attributes;
//                var ds = new DataSource();
//                if (attrs == null) continue;
//                foreach (XmlAttribute attr in attrs)
//                    switch (attr.Name)
//                    {
//                        case "Name":
//                            ds.Name = attr.Value;
//                            break;
//                        case "ShowName":
//                            ds.ShowName = attr.Value;
//                            break;
//                        case "Order":
//                            ds.Order = Convert.ToInt32(attr.Value);
//                            break;
//                        case "Server":
//                            ds.Server = attr.Value;
//                            break;
//                        case "DBName":
//                            ds.DBName = attr.Value;
//                            break;
//                        case "Color":
//                            ds.Color = (SolidColorBrush) new BrushConverter().ConvertFromString(attr.Value);
//                            break;
//                    }
//#if DEBUG
//                if (ds.Name == "EcoOndol") ds.Server = "VMIZHPC";
//                ComboBoxItemSource.Add(ds);
//#else
//                if (ds.Name == "Gokite.terifa1") continue;
//                ComboBoxItemSource.Add(ds);
//#endif
//            }
//        }

        private void LoadDataSources()
        {
            if (CurrentUser == null) return;
            ComboBoxItemSource.Clear();
            var connection = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.0.1", InitialCatalog = "KursSystem", UserID = "KursUser", Password = "KursUser"
            };
            //"Data Source=main8;Initial Catalog=KursSystem;";
            try
            {
                using (var conn = new SqlConnection(connection.ConnectionString))
                {
                    conn.Open();
                    var command = new SqlCommand();
                    command.CommandText =
                        "SELECT d.Name AS Name, d.ShowName AS ShowName, d.[Order] AS 'Order', d.Server as 'Server', " +
                        "d.DBName AS 'DBName', d.Color AS 'Color', d.Id as 'Id'  FROM DataSources d " +
                        "INNER JOIN  UsersLinkDB link ON link.DBId = d.Id " +
                        $"INNER JOIN  Users u ON u.Id = link.UserId AND UPPER(u.Name) = '{CurrentUser.ToUpper()}'";
                    command.Connection = conn;
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
                //Login cache
                using (var ctx = new LocalDbContext())
                {
                    var item = ctx.LoginStarts;
                    var view = Form as View.StartLogin;
                    if (!item.Any())
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        view.AvatarObj.Source =
                            new BitmapImage(new Uri("./../Images/businessman.png", UriKind.Relative));
                    }
                    else
                    {
                        var lastData = ctx.LoginStarts.Max(_ => _.LastDataTime);
                        var firstOrDefault = ctx.LoginStarts.FirstOrDefault(_ => _.LastDataTime == lastData);
                        if (firstOrDefault != null)
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            view.AvatarObj.Source = ImageManager.ByteToImageSource(
                                firstOrDefault.Avatar);
                            CurrentUser = firstOrDefault.Login;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errText = new StringBuilder(ex.Message);
                while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");

                MessageBox.Show("LocalDbContext error.\n" + errText);
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
        }

        public void SaveСache(ImageSource data)
        {
            using (var ctx = new LocalDbContext())
            {
                try
                {
                    var item = ctx.LoginStarts.FirstOrDefault(_ => _.Login.ToUpper() == CurrentUser.ToUpper());
                    if (item != null)
                    {
                        item.Avatar = ImageManager.ImageSourceToBytes(
                            new PngBitmapEncoder(), data);
                        item.LastDataTime = DateTime.Now;
                    }
                    else
                    {
                        var id = ctx.LoginStarts.Any() ? ctx.LoginStarts.Max(_ => _.Id) + 1 : 1;
                        ctx.LoginStarts.Add(new LoginStart
                        {
                            Id = id,
                            Avatar = ImageManager.ImageSourceToBytes(
                                new PngBitmapEncoder(), data),
                            LastDataTime = DateTime.Now,
                            Login = CurrentUser
                        });
                    }

                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    var errText = new StringBuilder(ex.Message);
                    while (ex.InnerException != null) errText.Append($"\n {ex.InnerException.Message}");

                    MessageBox.Show("LocalDbContext SaveСache error.\n" + errText);
                }
            }
        }

        public void HideSplash()
        {
            SplashScreenService.HideSplashScreen();
        }

        #endregion
    }
}