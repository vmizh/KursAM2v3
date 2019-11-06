using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using Helper;

namespace Core
{
    public class MainReferences
    {
        private static bool myIsNomComplete;
        private static bool myIsKontrComplete;

        public MainReferences()
        {
            AllKontragents = new Dictionary<decimal, Kontragent>();
            ALLNomenkls = new Dictionary<decimal, Nomenkl>();
            ActiveKontragents = new Dictionary<decimal, Kontragent>();
            ActiveNomenkls = new Dictionary<decimal, Nomenkl>();
            COList = new Dictionary<decimal, CentrOfResponsibility>();
            PayConditions = new Dictionary<decimal, UsagePay>();
            VzaimoraschetTypes = new Dictionary<decimal, VzaimoraschetType>();
            FormRaschets = new Dictionary<decimal, FormPay>();
            Countries = new Dictionary<decimal, Country>();
            Currencies = new Dictionary<decimal, Currency>();
            Units = new Dictionary<decimal, Unit>();
            NomenklGroups = new Dictionary<decimal, NomenklGroup>();
            NomenklTypes = new Dictionary<decimal, NomenklProductType>();
            Employees = new Dictionary<decimal, Employee>();
            Warehouses = new Dictionary<decimal, Warehouse>();
            SDRSchets = new Dictionary<decimal, SDRSchet>();
            SDRStates = new Dictionary<decimal, SDRState>();
            MutualTypes = new Dictionary<decimal, SD_111ViewModel>();
            BankAccounts = new Dictionary<decimal, BankAccount>();
            Cashs = new Dictionary<decimal, Cash>();
            Projects = new Dictionary<Guid, Project>();
            ContractTypes = new Dictionary<decimal, ContractType>();
            DeliveryConditions = new Dictionary<decimal, DeliveryCondition>();
            Regions = new Dictionary<decimal, Region>();
        }

        public static Dictionary<decimal, Region> Regions { set; get; }
        public static Dictionary<decimal, DeliveryCondition> DeliveryConditions { set; get; }
        public static Dictionary<decimal, ContractType> ContractTypes { set; get; }
        public static Dictionary<Guid, Project> Projects { set; get; }
        public static Dictionary<decimal, Cash> Cashs { set; get; }
        public static Dictionary<decimal, BankAccount> BankAccounts { set; get; }
        public static DateTime NomenklLastUpdate { set; get; } = new DateTime(1900, 1, 1);
        public static DateTime KontragentLastUpdate { set; get; } = new DateTime(1900, 1, 1);
        public static Dictionary<decimal, SD_111ViewModel> MutualTypes { private set; get; }
        public static Dictionary<decimal, SDRSchet> SDRSchets { private set; get; }
        public static Dictionary<decimal, SDRState> SDRStates { private set; get; }
        public static Dictionary<decimal, CentrOfResponsibility> COList { private set; get; }
        public static Dictionary<decimal, UsagePay> PayConditions { private set; get; }
        public static Dictionary<decimal, VzaimoraschetType> VzaimoraschetTypes { private set; get; }
        public static Dictionary<decimal, FormPay> FormRaschets { private set; get; }
        public static Dictionary<decimal, Country> Countries { set; get; }
        public static Dictionary<decimal, Currency> Currencies { private set; get; }
        public static Dictionary<decimal, Unit> Units { private set; get; }
        public static Dictionary<decimal, NomenklGroup> NomenklGroups { private set; get; }
        public static Dictionary<decimal, NomenklProductType> NomenklTypes { private set; get; }
        public static Dictionary<decimal, Employee> Employees { private set; get; }
        public static Dictionary<decimal, Kontragent> AllKontragents { set; get; }

        // ReSharper disable once InconsistentNaming
        public static Dictionary<decimal, Nomenkl> ALLNomenkls { set; get; }
        public static Dictionary<decimal, Kontragent> ActiveKontragents { set; get; }
        public static Dictionary<decimal, Nomenkl> ActiveNomenkls { set; get; }
        public static Dictionary<decimal, Warehouse> Warehouses { set; get; }

        public static bool IsReferenceLoadComplete
        {
            get => myIsNomComplete && myIsKontrComplete;
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                myIsNomComplete = false;
                myIsKontrComplete = false;
            }
        }

        public static Nomenkl GetNomenkl(decimal dc, bool isUpdateCheck)
        {
            if (ALLNomenkls.ContainsKey(dc))
            {
                if (isUpdateCheck) UpdateNomenkl();
                return ALLNomenkls[dc];
            }

            LoadNomenkl(dc);
            if (!ALLNomenkls.ContainsKey(dc))
            {
                WindowManager.ShowMessage($"Номенклатура с кодом {dc} отсутствует.", "Ошибка", MessageBoxImage.Error);
                return null;
            }

            return ALLNomenkls[dc];
        }

        public static Nomenkl GetNomenkl(decimal dc)
        {
            if (ALLNomenkls.ContainsKey(dc))
                return ALLNomenkls[dc];
            LoadNomenkl(dc);
            if (!ALLNomenkls.ContainsKey(dc))
            {
                WindowManager.ShowMessage($"Номенклатура с кодом {dc} отсутствует.", "Ошибка", MessageBoxImage.Error);
                return null;
            }

            return ALLNomenkls[dc];
        }

        public static Nomenkl GetNomenkl(decimal? dc)
        {
            if (dc == null) return null;
            return GetNomenkl(dc.Value);
        }

        private static void CheckUpdateKontragentAndLoad()
        {
            try
            {
                using (var ent = GlobalOptions.GetEntities())
                {
                    if (!ent.SD_43.Any(_ => _.UpdateDate > KontragentLastUpdate)) return;
                    foreach (
                        var newItem in
                        ent.SD_43.Where(_ => _.UpdateDate > KontragentLastUpdate)
                            .AsNoTracking()
                            .Include(_ => _.SD_301)
                            .ToList()
                            .Select(item => new Kontragent(item)))
                    {
                        if (AllKontragents.ContainsKey(newItem.DOC_CODE))
                            AllKontragents[newItem.DOC_CODE] = newItem;
                        else
                            AllKontragents.Add(newItem.DOC_CODE, newItem);
                        if ((newItem.DELETED ?? 0) != 0) continue;
                        if (!ActiveKontragents.ContainsKey(newItem.DOC_CODE))
                            ActiveKontragents.Add(newItem.DOC_CODE, newItem);
                        else
                            ActiveKontragents[newItem.DOC_CODE] = newItem;
                    }

                    // ReSharper disable once PossibleInvalidOperationException
                    KontragentLastUpdate = (DateTime) AllKontragents.Values.Select(_ => _.UpdateDate).Max();
                }
            }
            catch (Exception)
            {
                //WindowManager.ShowError(null, ex);
            }
        }

        public static Kontragent GetKontragent(decimal dc)
        {
            if (AllKontragents.ContainsKey(dc))
                return AllKontragents[dc];
            return null;
        }

        public static Kontragent GetKontragent(decimal? dc)
        {
            if (dc == null) return null;
            return GetKontragent(dc.Value);
        }

        public static CentrOfResponsibility GetCO(decimal dc)
        {
            if (COList.ContainsKey(dc))
                return COList[dc];
            return new CentrOfResponsibility
            {
                Entity = new SD_40
                {
                    DOC_CODE = 0,
                    CENT_NAME = "ЦО не указан"
                }
            };
        }

        public static CentrOfResponsibility GetCO(decimal? dc)
        {
            if (dc == null)
                return
                    new CentrOfResponsibility
                    {
                        Entity = new SD_40
                        {
                            DOC_CODE = 0,
                            CENT_NAME = "ЦО не указан"
                        }
                    };
            return GetCO(dc.Value);
        }

        public static void Refresh()
        {
           try
            {
                using (var ent = GlobalOptions.GetEntities())
                {
                    #region центр ответственности SD_40

                    var s40 = ent.SD_40.Include(_ => _.SD_402).AsNoTracking().ToList();
                    foreach (var item in s40)
                    {   if(COList.ContainsKey(item.DOC_CODE))
                        {
                            var d =  COList[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else {
                            COList.Add(item.DOC_CODE, new CentrOfResponsibility(item) {myState = RowStatus.NotEdited});
                        }
                    }
                    foreach (var k in COList.Keys)
                    {
                        if (s40.Any(_ => _.DOC_CODE == k)) continue;
                        COList.Remove(k);
                    }

                    #endregion

                    #region  тип актов взаимозачета
                    var s111 = ent.SD_111.AsNoTracking().ToList();
                    foreach (var item in s111)
                    {
                        if (MutualTypes.ContainsKey(item.DOC_CODE))
                        {
                            var d = MutualTypes[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            MutualTypes.Add(item.DOC_CODE, new SD_111ViewModel(item) {myState = RowStatus.NotEdited});
                        }
                       
                    }

                    var keys = MutualTypes.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s111.Any(_ => _.DOC_CODE == k)) continue;
                        MutualTypes.Remove(k);
                    }
                    #endregion

                    #region Счет доходов/расходов sd_303
                    var s303 = ent.SD_303.Include(_ => _.SD_99).Include(_ => _.SD_97).AsNoTracking().ToList();
                    foreach (var l in s303)
                    {
                        if (SDRSchets.ContainsKey(l.DOC_CODE))
                        {
                            var d = SDRSchets[l.DOC_CODE];
                            d.UpdateFrom(l);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            SDRSchets.Add(l.DOC_CODE, new SDRSchet(l) {myState = RowStatus.NotEdited});
                        }
                    }

                    keys = SDRSchets.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s303.Any(_ => _.DOC_CODE == k)) continue;
                        SDRSchets.Remove(k);
                    }
                    #endregion

                    #region статьи доходов SD_99
                    var s99 = ent.SD_99.Include(_ => _.SD_992).AsNoTracking().ToList();
                    foreach (var item in s99)
                    {
                        if (SDRStates.ContainsKey(item.DOC_CODE))
                        {
                            var d = SDRStates[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            SDRStates.Add(item.DOC_CODE, new SDRState(item) {myState = RowStatus.NotEdited});
                        }
                    }

                    keys = SDRStates.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s99.Any(_ => _.DOC_CODE == k)) continue;
                        SDRStates.Remove(k);
                    }
                    #endregion

                    #region Условия оплаты SD_179

                    var s179 = ent.SD_179.AsNoTracking().ToList();
                    foreach (var item in s179)
                    {
                        if (PayConditions.ContainsKey(item.DOC_CODE))
                        {
                            var d = PayConditions[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            PayConditions.Add(item.DOC_CODE, new UsagePay(item) {myState = RowStatus.NotEdited});
                        }
                    }

                    keys = PayConditions.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s179.Any(_ => _.DOC_CODE == k)) continue;
                        PayConditions.Remove(k);
                    }
                    #endregion

                    #region Тип взаиморасчетов SD_77

                    var s77 = ent.SD_77.AsNoTracking().ToList();
                    foreach (var item in s77)
                    {
                        if (VzaimoraschetTypes.ContainsKey(item.DOC_CODE))
                        {
                            var d = VzaimoraschetTypes[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            VzaimoraschetTypes.Add(item.DOC_CODE,
                                new VzaimoraschetType(item) {myState = RowStatus.NotEdited});
                        }
                    }

                    keys = VzaimoraschetTypes.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s77.Any(_ => _.DOC_CODE == k)) continue;
                        VzaimoraschetTypes.Remove(k);
                    }
                    #endregion

                    #region Форма расчетов

                    var s189 = ent.SD_189.Include(_ => _.SD_179).AsNoTracking().ToList();
                    foreach (var item in s189)
                    {
                        if (FormRaschets.ContainsKey(item.DOC_CODE))
                        {
                            var d = FormRaschets[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            FormRaschets.Add(item.DOC_CODE, new FormPay(item) {myState = RowStatus.NotEdited});
                        }
                    }

                    keys = FormRaschets.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s189.Any(_ => _.DOC_CODE == k)) continue;
                        FormRaschets.Remove(k);
                    }
                    #endregion

                    #region Страны countries
                    var countries = ent.Countries.AsNoTracking().OrderBy(_ => _.Name).AsNoTracking().ToList();
                    foreach (var item in countries)
                    {
                        if (Countries.ContainsKey(item.Iso))
                        {
                            var d = Countries[item.Iso];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            Countries.Add(item.Iso, new Country(item) {myState = RowStatus.NotEdited});
                        }
                    }

                    keys = Countries.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (countries.Any(_ => _.Iso == k)) continue;
                        Countries.Remove(k);
                    }
                    #endregion

                    #region Валюты sd_301

                    var s301 = ent.SD_301.AsNoTracking().ToList();
                    foreach (var item in s301)
                    {
                        if (Currencies.ContainsKey(item.DOC_CODE))
                        {
                            var d = Currencies[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            Currencies.Add(item.DOC_CODE, new Currency(item) {myState = RowStatus.NotEdited});
                        }
                    }
                    Currencies.Add(0m,new Currency
                    {
                        DocCode = 0m,
                        CRS_NAME = "Валюта не указана",
                        CRS_SHORTNAME = "Валюта не указана",
                        myState = RowStatus.NotEdited
                    });

                    keys = Currencies.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s301.Any(_ => _.DOC_CODE == k)) continue;
                        Currencies.Remove(k);
                    }
                    #endregion

                    #region Единицы измерения SD_175

                    var s175 = ent.SD_175.AsNoTracking().ToList();
                    foreach (var item in s175)
                    {
                        if (Units.ContainsKey(item.DOC_CODE))
                        {
                            var d = Units[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            Units.Add(item.DOC_CODE, new Unit(item) {myState = RowStatus.NotEdited});
                        }
                    }

                    keys = Units.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s175.Any(_ => _.DOC_CODE == k)) continue;
                        Units.Remove(k);
                    }
                    #endregion

                    #region группы номенклатур sd_82

                    var s82 = ent.SD_82.Include(_ => _.SD_822).AsNoTracking().ToList();
                    foreach (var item in s82)
                    {
                        if (NomenklGroups.ContainsKey(item.DOC_CODE))
                        {
                            var d = NomenklGroups[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            NomenklGroups.Add(item.DOC_CODE, new NomenklGroup(item) {myState = RowStatus.NotEdited});
                        }
                    }

                    keys = NomenklGroups.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s82.Any(_ => _.DOC_CODE == k)) continue;
                        NomenklGroups.Remove(k);
                    }
                    #endregion

                    #region тип номенклатур sd_119

                    var s119 = ent.SD_119.AsNoTracking().ToList();
                    foreach (var item in s119)
                    {
                        if (NomenklTypes.ContainsKey(item.DOC_CODE))
                        {
                            var d = NomenklTypes[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            NomenklTypes.Add(item.DOC_CODE,
                                new NomenklProductType(item) {myState = RowStatus.NotEdited});
                        }
                    }
                    keys = NomenklTypes.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s119.Any(_ => _.DOC_CODE == k)) continue;
                        NomenklTypes.Remove(k);
                    }
                    #endregion

                    #region персонал sd_2
                    var s2 = ent.SD_2.Include(_ => _.SD_301).Include(_ => _.SD_269).AsNoTracking().ToList();
                    foreach (var item in s2)
                    {
                        if (Employees.ContainsKey(item.DOC_CODE))
                        {
                            var d = Employees[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            Employees.Add(item.DOC_CODE, new Employee(item) {myState = RowStatus.NotEdited});
                        }
                    }
                    keys = Employees.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s2.Any(_ => _.DOC_CODE == k)) continue;
                        Employees.Remove(k);
                    }
                    #endregion

                    #region склады sd_27

                    var s27 = ent.SD_27.AsNoTracking().ToList();
                    foreach (var item in s27)
                    {
                        if (Warehouses.ContainsKey(item.DOC_CODE))
                        {
                            var d = Warehouses[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            Warehouses.Add(item.DOC_CODE, new Warehouse(item) {myState = RowStatus.NotEdited});
                        }
                    }
                    keys = Warehouses.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s27.Any(_ => _.DOC_CODE == k)) continue;
                        Warehouses.Remove(k);
                    }
                    #endregion

                    #region Проекты Projects

                    // ReSharper disable once IdentifierTypo
                    var prjs = ent.Projects.Include(_ => _.ProjectsDocs).AsNoTracking().ToList();
                    foreach (var item in prjs)
                    {
                        if (Projects.ContainsKey(item.Id))
                        {
                            var d = Projects[item.Id];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            Projects.Add(item.Id, new Project(item) {myState = RowStatus.NotEdited});
                        }
                    }
                    var keys1 = Projects.Keys.ToList();
                    foreach (var i in keys1)
                    {
                        if (prjs.Any(_ => _.Id == i)) continue;
                        Projects.Remove(i);
                    }
                    #endregion

                    #region Банковские счета sd_114

                    var s114 = ent.SD_114.AsNoTracking().Include(_ => _.SD_44).AsNoTracking().ToList();
                    foreach (var item in s114)
                    {
                        if (BankAccounts.ContainsKey(item.DOC_CODE))
                        {
                            var d = BankAccounts[item.DOC_CODE];
                            d.Name = item.BA_ACC_SHORTNAME;
                            d.BIK = item.SD_44.POST_CODE;
                            d.Account = item.BA_RASH_ACC;
                            d.CorrAccount = item.SD_44.CORRESP_ACC;
                            d.BankName = item.BA_BANK_NAME;
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            BankAccounts.Add(item.DOC_CODE, new BankAccount
                            {
                                DocCode = item.DOC_CODE,
                                BankDC = item.DOC_CODE,
                                Name = item.BA_ACC_SHORTNAME,
                                BIK = item.SD_44.POST_CODE,
                                Account = item.BA_RASH_ACC,
                                CorrAccount = item.SD_44.CORRESP_ACC,
                                BankName = item.BA_BANK_NAME,
                                myState = RowStatus.NotEdited
                            });
                        }
                    }
                    keys = BankAccounts.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s114.Any(_ => _.DOC_CODE == k)) continue;
                        BankAccounts.Remove(k);
                    }
                    #endregion

                    #region Кассы SD_22

                    var s22 = ent.SD_22.Include(_ => _.SD_40).AsNoTracking().ToList();
                    var cashAcc = ent.Database.SqlQuery<AccessRight>(
                            $"SELECT DOC_CODE AS DocCode, USR_ID as UserId FROM HD_22 WHERE USR_ID = {GlobalOptions.UserInfo.Id}")
                        .ToList();
                    foreach (var item in s22)
                    {
                        if (Cashs.ContainsKey(item.DOC_CODE))
                        {
                            var d = Cashs[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            Cashs.Add(item.DOC_CODE,
                                new Cash(item)
                                {
                                    IsAccessRight = cashAcc.Any(_ => item.DOC_CODE == _.DocCode),
                                    myState = RowStatus.Deleted
                                });
                        }
                    }
                    keys = Cashs.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s22.Any(_ => _.DOC_CODE == k) && cashAcc.Any(_ => _.DocCode == k)) continue;
                        Cashs.Remove(k);
                    }
                    #endregion

                    #region Регионы SD_23

                    var s23 = ent.SD_23.Include(_ => _.SD_232).Include(_ => _.SD_434).AsNoTracking().ToList();
                    foreach (var item in s23)
                    {
                        if (Regions.ContainsKey(item.DOC_CODE))
                        {
                            var d = Regions[item.DOC_CODE];
                            d.UpdateFrom(item);
                            d.myState = RowStatus.NotEdited;
                        }
                        else
                        {
                            Regions.Add(item.DOC_CODE, new Region(item) {myState = RowStatus.NotEdited});
                        }
                    }
                    keys = Regions.Keys.ToList();
                    foreach (var k in keys)
                    {
                        if (s23.Any(_ => _.DOC_CODE == k)) continue;
                        Regions.Remove(k);
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private void LoadReference()
        {
            try
            {
                using (var ent = GlobalOptions.GetEntities())
                {
                    foreach (var item in ent.SD_40.AsNoTracking().ToList())
                        COList.Add(item.DOC_CODE, new CentrOfResponsibility(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_111.AsNoTracking().ToList())
                        MutualTypes.Add(item.DOC_CODE, new SD_111ViewModel(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_303.AsNoTracking().ToList())
                        SDRSchets.Add(item.DOC_CODE, new SDRSchet(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_99.AsNoTracking().ToList())
                        SDRStates.Add(item.DOC_CODE, new SDRState(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_179.AsNoTracking().ToList())
                        PayConditions.Add(item.DOC_CODE, new UsagePay(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_77.AsNoTracking().ToList())
                        VzaimoraschetTypes.Add(item.DOC_CODE,
                            new VzaimoraschetType(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_189.AsNoTracking().ToList())
                        FormRaschets.Add(item.DOC_CODE, new FormPay(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.Countries.AsNoTracking().OrderBy(_ => _.Name).AsNoTracking().ToList())
                        Countries.Add(item.Iso, new Country(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_301.AsNoTracking().ToList())
                        Currencies.Add(item.DOC_CODE, new Currency(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_175.AsNoTracking().ToList())
                        Units.Add(item.DOC_CODE, new Unit(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_82.AsNoTracking().ToList())
                        NomenklGroups.Add(item.DOC_CODE, new NomenklGroup(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_119.AsNoTracking().ToList())
                        NomenklTypes.Add(item.DOC_CODE, new NomenklProductType(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_2.AsNoTracking().ToList())
                        Employees.Add(item.DOC_CODE, new Employee(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_27.AsNoTracking().ToList())
                        Warehouses.Add(item.DOC_CODE, new Warehouse(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.Projects.AsNoTracking().ToList())
                        Projects.Add(item.Id, new Project(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_114.AsNoTracking().Include(_ => _.SD_44).AsNoTracking())
                        BankAccounts.Add(item.DOC_CODE, new BankAccount
                        {
                            DocCode = item.DOC_CODE,
                            BankDC = item.DOC_CODE,
                            Name = item.BA_ACC_SHORTNAME,
                            BIK = item.SD_44.POST_CODE,
                            Account = item.BA_RASH_ACC,
                            CorrAccount = item.SD_44.CORRESP_ACC,
                            BankName = item.BA_BANK_NAME,
                            myState = RowStatus.NotEdited
                        });
                    var cashAcc = ent.Database.SqlQuery<AccessRight>(
                            $"SELECT DOC_CODE AS DocCode, USR_ID as UserId FROM HD_22 WHERE USR_ID = {GlobalOptions.UserInfo.Id}")
                        .ToList();
                    foreach (var item in ent.SD_22)
                        Cashs.Add(item.DOC_CODE,
                            new Cash(item)
                            {
                                IsAccessRight = cashAcc.Any(_ => item.DOC_CODE == _.DocCode),
                                myState = RowStatus.NotEdited
                            });
                    foreach (var item in ent.SD_102)
                        ContractTypes.Add(item.DOC_CODE, new ContractType(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_103)
                        DeliveryConditions.Add(item.DOC_CODE,
                            new DeliveryCondition(item) {myState = RowStatus.NotEdited});
                    foreach (var item in ent.SD_23.AsNoTracking().ToList())
                        Regions.Add(item.DOC_CODE, new Region(item) {myState = RowStatus.NotEdited});
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        private static void UpdateNomenkl()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var sql = " SELECT SD_83.DOC_CODE as DOC_CODE, NOM_NAME, NOM_NOMENKL, NOM_CATEG_DC, " +
                          " SD_175.DOC_CODE as UNIT_DC, SD_175.ED_IZM_NAME AS UNIT_NAME, SD_175.ED_IZM_OKEI_CODE as UNIT_CODE, " +
                          " NOM_SALE_CRS_DC as NOM_SALE_CRS_DC, Id as Id, UpdateDate as UpdateDate, NOM_0MATER_1USLUGA, MainId as MainId, " +
                          " isnull(sd_83.NOM_PRODUCT_DC,0) as NOM_PRODUCT_DC " +
                          " FROM SD_83 " +
                          " INNER JOIN SD_175 ON SD_175.DOC_CODE = SD_83.NOM_ED_IZM_DC " +
                          $" and SD_83.UpdateDate > '{CustomFormat.DateWithTimeToString(NomenklLastUpdate)}'";
                var data = ent.Database.SqlQuery<NomenklShort>(sql).ToList();
                if (data.Count == 0) return;
                foreach (var d in data)
                    if (ALLNomenkls.ContainsKey(d.DOC_CODE))
                    {
                        var n = ALLNomenkls[d.DOC_CODE];
                        n.DOC_CODE = d.DOC_CODE;
                        n.NOM_NAME = d.NOM_NAME;
                        n.NOM_NOMENKL = d.NOM_NOMENKL;
                        n.NOM_SALE_CRS_DC = d.NOM_SALE_CRS_DC;
                        n.Id = d.Id;
                        n.MainId = (Guid) d.MainId;
                        n.UpdateDate = d.UpdateDate;
                        n.NOM_0MATER_1USLUGA = d.NOM_0MATER_1USLUGA;
                        n.NOM_CATEG_DC = d.NOM_CATEG_DC;
                        n.NOM_PRODUCT_DC = d.NOM_PRODUCT_DC;
                    }
                    else
                    {
                        ALLNomenkls.Add(d.DOC_CODE, new Nomenkl
                        {
                            Entity = new SD_83
                            {
                                DOC_CODE = d.DOC_CODE,
                                NOM_NAME = d.NOM_NAME,
                                NOM_NOMENKL = d.NOM_NOMENKL,
                                NOM_SALE_CRS_DC = d.NOM_SALE_CRS_DC,
                                Id = d.Id,
                                MainId = d.MainId,
                                UpdateDate = d.UpdateDate,
                                NOM_0MATER_1USLUGA = d.NOM_0MATER_1USLUGA,
                                NOM_CATEG_DC = d.NOM_CATEG_DC,
                                NOM_PRODUCT_DC = d.NOM_PRODUCT_DC
                            },
                            Unit = Units[d.UNIT_DC],
                            Currency = Currencies[d.NOM_SALE_CRS_DC],
                            Group = NomenklGroups[d.NOM_CATEG_DC]
                        });
                    }

                NomenklLastUpdate = data.Max(_ => _.UpdateDate);
            }
        }

        public static void LoadNomenkl(decimal dc)
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                var data = ent.Database.SqlQuery<NomenklShort>(
                    " SELECT SD_83.DOC_CODE as DOC_CODE, NOM_NAME, NOM_NOMENKL, NOM_CATEG_DC, " +
                    " SD_175.DOC_CODE as UNIT_DC, SD_175.ED_IZM_NAME AS UNIT_NAME, SD_175.ED_IZM_OKEI_CODE as UNIT_CODE, " +
                    " NOM_SALE_CRS_DC as NOM_SALE_CRS_DC, Id as Id, UpdateDate as UpdateDate, NOM_0MATER_1USLUGA, MainId as MainId, " +
                    " isnull(sd_83.NOM_PRODUCT_DC,0) as NOM_PRODUCT_DC, isnull(sd_83.IsUslugaInRent,0) as IsRentabelnost " +
                    " FROM SD_83 " +
                    " INNER JOIN SD_175 ON SD_175.DOC_CODE = SD_83.NOM_ED_IZM_DC " +
                    " and SD_83.DOC_CODE = '" + CustomFormat.DecimalToSqlDecimal(dc) + "'").ToList();
                if (data.Count == 0) return;
                var d = data[0];
                ALLNomenkls.Add(d.DOC_CODE, new Nomenkl
                {
                    Entity = new SD_83
                    {
                        DOC_CODE = d.DOC_CODE,
                        NOM_NAME = d.NOM_NAME,
                        NOM_NOMENKL = d.NOM_NOMENKL,
                        NOM_SALE_CRS_DC = d.NOM_SALE_CRS_DC,
                        Id = d.Id,
                        MainId = d.MainId,
                        UpdateDate = d.UpdateDate,
                        NOM_0MATER_1USLUGA = d.NOM_0MATER_1USLUGA,
                        NOM_CATEG_DC = d.NOM_CATEG_DC,
                        NOM_PRODUCT_DC = d.NOM_PRODUCT_DC,
                        IsUslugaInRent = d.IsRentabelnost
                    },
                    Unit = Units[d.UNIT_DC],
                    Currency = Currencies[d.NOM_SALE_CRS_DC],
                    Group = NomenklGroups[d.NOM_CATEG_DC]
                });
            }
        }

        public static void LoadNomenkl()
        {
            ALLNomenkls.Clear();
            try
            {
                using (var ent = GlobalOptions.GetEntities())
                {
                    var data = ent.Database.SqlQuery<NomenklShort>(
                            " SELECT SD_83.DOC_CODE as DOC_CODE, NOM_NAME, NOM_NOMENKL, NOM_CATEG_DC, " +
                            " SD_175.DOC_CODE as UNIT_DC, SD_175.ED_IZM_NAME AS UNIT_NAME, SD_175.ED_IZM_OKEI_CODE as UNIT_CODE, " +
                            " NOM_SALE_CRS_DC as NOM_SALE_CRS_DC, Id as Id, UpdateDate as UpdateDate, NOM_0MATER_1USLUGA, MainId as MainId, " +
                            " sd_83.NOM_PRODUCT_DC as NOM_PRODUCT_DC " +
                            " FROM SD_83 " +
                            " INNER JOIN SD_175 ON SD_175.DOC_CODE = SD_83.NOM_ED_IZM_DC " +
                            " and isnull(UpdateDate,getdate()) > '" + CustomFormat.DateToString(NomenklLastUpdate) +
                            "'")
                        .ToList();
                    foreach (var d in data)
                        ALLNomenkls.Add(d.DOC_CODE, new Nomenkl
                        {
                            Entity = new SD_83
                            {
                                DOC_CODE = d.DOC_CODE,
                                NOM_NAME = d.NOM_NAME,
                                NOM_NOMENKL = d.NOM_NOMENKL,
                                NOM_SALE_CRS_DC = d.NOM_SALE_CRS_DC,
                                Id = d.Id,
                                MainId = d.MainId,
                                UpdateDate = d.UpdateDate,
                                NOM_0MATER_1USLUGA = d.NOM_0MATER_1USLUGA,
                                NOM_CATEG_DC = d.NOM_CATEG_DC,
                                NOM_PRODUCT_DC = d.NOM_PRODUCT_DC
                            },
                            Unit = Units[d.UNIT_DC],
                            Currency = Currencies[d.NOM_SALE_CRS_DC],
                            Group = NomenklGroups[d.NOM_CATEG_DC]
                        });
                    // ReSharper disable once PossibleInvalidOperationException
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }

            var dm = ALLNomenkls.Values.Select(_ => _.UpdateDate);
            var dateTimes = dm as IList<DateTime?> ?? dm.ToList();
            if (dateTimes.Any())
            {
                var dateTime = dateTimes.Max();
                if (dateTime != null) NomenklLastUpdate = (DateTime) dateTime;
            }
            else
            {
                NomenklLastUpdate = DateTime.Now;
            }
        }

        private static void LoadKontragents()
        {
            AllKontragents.Clear();
            try
            {
                using (var ent = GlobalOptions.GetEntities())
                {
                    foreach (
                        var item in
                        ent.SD_43.Where(_ => _.UpdateDate > KontragentLastUpdate)
                            .AsNoTracking()
                            .Include(_ => _.SD_301)
                            .ToList())
                    {
                        var newItem = new Kontragent(item);
                        AllKontragents.Add(newItem.DOC_CODE, newItem);
                        if ((item.DELETED ?? 0) == 0)
                            ActiveKontragents.Add(newItem.DOC_CODE, newItem);
                    }

                    // ReSharper disable once PossibleInvalidOperationException
                    KontragentLastUpdate = (DateTime) AllKontragents.Values.Select(_ => _.UpdateDate).Max();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(null, ex);
            }
        }

        public void Reset(LoadReferenceStrategy strategy = LoadReferenceStrategy.Full)
        {
            Refresh();
            switch (strategy)
            {
                case LoadReferenceStrategy.Full:
                    Task.Run(() => LoadNomenkl()).ContinueWith(_ => SetNomComplete());
                    Task.Run(() => LoadKontragents()).ContinueWith(_ => SetKontrComplete());
                    break;
                case LoadReferenceStrategy.WithoutKontragent:
                    Task.Run(() => LoadNomenkl()).ContinueWith(_ => SetNomComplete());
                    break;
                case LoadReferenceStrategy.WithoutNomenkl:
                    Task.Run(() => LoadKontragents()).ContinueWith(_ => SetKontrComplete());
                    break;
                case LoadReferenceStrategy.WithoutKontragentAndNomenkl:
                    IsReferenceLoadComplete = true;
                    break;
            }
        }

        private Timer myUpdateTimer;

        private void SetNomComplete()
        {
            myIsNomComplete = true;
            if (myIsKontrComplete && myUpdateTimer == null)
            {

                myUpdateTimer = new Timer(_ =>
                    {
                        //Refresh();
                        UpdateNomenkl();
                        CheckUpdateKontragentAndLoad();
                    }, null, 1000 * 60, Timeout.Infinite);
            }
        }

        private void SetKontrComplete()
        {
            myIsKontrComplete = true;
            if (myIsNomComplete && myUpdateTimer == null)
            {
                myUpdateTimer = new Timer(_ =>
                {
                    //Refresh();
                    UpdateNomenkl();
                    CheckUpdateKontragentAndLoad();
                }, null, 1000 * 60, Timeout.Infinite);
            }
        }

        public static Dictionary<decimal, Kontragent> GetAllKontragents()
        {
            if (AllKontragents.Count == 0)
            {
                try
                {
                    using (var ent = GlobalOptions.GetEntities())
                    {
                        foreach (var item in ent.SD_43.AsNoTracking().Include(_ => _.SD_301).ToList())
                        {
                            var newItem = new Kontragent(item);
                            AllKontragents.Add(newItem.DOC_CODE, newItem);
                            if ((item.DELETED ?? 0) == 0)
                                ActiveKontragents.Add(newItem.DOC_CODE, newItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(null, ex);
                }
            }
            else
            {
                var dateTime = AllKontragents.Values.Max(_ => _.Entity.UpdateDate);
                if (dateTime == null) return AllKontragents;
                var date = (DateTime) dateTime;
                try
                {
                    using (var ent = GlobalOptions.GetEntities())
                    {
                        var c =
                            ent.Database.SqlQuery<int>("Select count(*) from SD_43 where UpdateDate > '" +
                                                       CustomFormat.DateToString(date) + "'").ToList();
                        if (c.Count == 0 || c[0] == 0) return AllKontragents;
                        foreach (
                            var item in
                            ent.SD_43.Include(_ => _.SD_301).AsNoTracking().Where(_ => _.UpdateDate > date).ToList()
                        )
                            if (AllKontragents.Keys.Contains(item.DOC_CODE))
                            {
                                var newItem = new Kontragent(item);
                                AllKontragents.Add(newItem.DOC_CODE, newItem);
                                if ((item.DELETED ?? 0) == 0)
                                    ActiveKontragents.Add(newItem.DOC_CODE, newItem);
                            }
                            else
                            {
                                AllKontragents.Remove(item.DOC_CODE);
                                ActiveKontragents.Remove(item.DOC_CODE);
                                var newItem = new Kontragent(item);
                                AllKontragents.Add(item.DOC_CODE, newItem);
                                if ((item.DELETED ?? 0) == 0)
                                    ActiveKontragents.Add(item.DOC_CODE, newItem);
                            }
                    }
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(null, ex);
                }
            }

            return AllKontragents;
        }

        public static Dictionary<decimal, Nomenkl> GetAllNomenkls()
        {
            if (ALLNomenkls.Count == 0)
            {
                try
                {
                    using (var ent = GlobalOptions.GetEntities())
                    {
                        foreach (var item in ent.SD_83.AsNoTracking().Include(_ => _.SD_175).ToList())
                        {
                            var newItem = new Nomenkl(item);
                            ALLNomenkls.Add(item.DOC_CODE, newItem);
                            if ((item.NOM_DELETED ?? 0) == 0)
                                ActiveNomenkls.Add(item.DOC_CODE, newItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(null, ex);
                }
            }
            else
            {
                var dateTime = ALLNomenkls.Values.Max(_ => _.Entity.UpdateDate);
                if (dateTime == null) return ALLNomenkls;
                var date = (DateTime) dateTime;
                try
                {
                    using (var ent = GlobalOptions.GetEntities())
                    {
                        var c =
                            ent.Database.SqlQuery<int>("Select count(*) from SD_83 where UpdateDate > '" +
                                                       CustomFormat.DateToString(date) + "'");
                        if (c == null || c.First() == 0) return ALLNomenkls;
                        foreach (
                            var item in
                            ent.SD_83.Include(_ => _.SD_175)
                                .AsNoTracking()
                                .ToList()
                                .Where(_ => _.UpdateDate > date)
                                .ToList())
                            if (ALLNomenkls.Keys.Contains(item.DOC_CODE))
                            {
                                var newItem = new Nomenkl(item);
                                ALLNomenkls.Add(item.DOC_CODE, newItem);
                                if ((item.NOM_DELETED ?? 0) == 0)
                                    ActiveNomenkls.Add(item.DOC_CODE, newItem);
                            }
                            else
                            {
                                ALLNomenkls.Remove(item.DOC_CODE);
                                ActiveNomenkls.Remove(item.DOC_CODE);
                                var newItem = new Nomenkl(item);
                                ALLNomenkls.Add(item.DOC_CODE, newItem);
                                if ((item.NOM_DELETED ?? 0) == 0)
                                    ActiveNomenkls.Add(item.DOC_CODE, newItem);
                            }
                    }
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(null, ex);
                }
            }

            return ALLNomenkls;
        }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public class AccessRight
        {
            public decimal DocCode { set; get; }
            public int UserId { set; get; }
        }
    }

    public enum LoadReferenceStrategy
    {
        Full = 0,
        WithoutKontragentAndNomenkl = 1,
        WithoutKontragent = 2,
        WithoutNomenkl = 3
    }

    public class NomenklShort
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public decimal DOC_CODE { set; get; }
        public decimal NOM_SALE_CRS_DC { set; get; }
        public string NOM_NAME { set; get; }
        public string NOM_NOMENKL { set; get; }
        public decimal UNIT_DC { set; get; }
        public string UNIT_NAME { set; get; }
        public string UNIT_CODE { set; get; }
        public DateTime UpdateDate { set; get; }
        public Guid Id { set; get; }
        public Guid? MainId { set; get; }
        public int NOM_0MATER_1USLUGA { set; get; }
        public decimal NOM_CATEG_DC { set; get; }

        public decimal NOM_PRODUCT_DC { set; get; }
        public bool IsRentabelnost { set; get; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore InconsistentNaming
    }
}