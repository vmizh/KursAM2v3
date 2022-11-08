using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Helper;
using KursDomain;
using KursDomain.Documents.AccruedAmount;
using KursDomain.Documents.Cash;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.NomenklManagement;
using KursDomain.Documents.Vzaimozachet;
using KursDomain.ICommon;
using KursDomain.References;
using ContractType = KursDomain.Documents.Dogovora.ContractType;
using DeliveryCondition = KursDomain.Documents.NomenklManagement.DeliveryCondition;
using Employee = KursDomain.Documents.Employee.Employee;
using Project = KursDomain.Documents.CommonReferences.Project;
using Region = KursDomain.Documents.CommonReferences.Region;
using Warehouse = KursDomain.Documents.NomenklManagement.Warehouse;

namespace Core;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
public static class MainReferences
{
    private static bool myIsNomComplete;
    private static bool myIsKontrComplete;
    private static Timer myUpdateTimer;

    static MainReferences()
    {
        ThemeNames = getThemeNames();
        AllKontragents = new Dictionary<decimal, KontragentViewModel>();
        ALLNomenkls = new Dictionary<decimal, Nomenkl>();
        ActiveKontragents = new Dictionary<decimal, KontragentViewModel>();
        ActiveNomenkls = new Dictionary<decimal, Nomenkl>();
        COList = new Dictionary<decimal, CentrResponsibility>();
        PayConditions = new Dictionary<decimal, PayCondition>();
        VzaimoraschetTypes = new Dictionary<decimal, VzaimoraschetType>();
        FormRaschets = new Dictionary<decimal, PayForm>();
        Countries = new Dictionary<decimal, CountriesViewModel>();
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
        CashsAll = new Dictionary<decimal, Cash>();
        Projects = new Dictionary<Guid, Project>();
        ContractTypes = new Dictionary<decimal, ContractType>();
        DeliveryConditions = new Dictionary<decimal, DeliveryCondition>();
        Regions = new Dictionary<decimal, Region>();
        ClientKategory = new Dictionary<decimal, CategoryClientTypeViewModel>();
        AccruedAmountTypes = new Dictionary<Guid, AccruedAmountTypeViewModel>();
    }


    public static Dictionary<decimal, Cash> CashsAll { get; set; }

    public static bool IsCheckedForUpdate { set; get; } = false;

    public static List<string> ThemeNames { set; get; }
    public static Dictionary<decimal, Region> Regions { set; get; }

    public static Dictionary<decimal, CategoryClientTypeViewModel> ClientKategory { set; get; }
    public static Dictionary<decimal, DeliveryCondition> DeliveryConditions { set; get; }
    public static Dictionary<decimal, ContractType> ContractTypes { set; get; }
    public static Dictionary<Guid, Project> Projects { set; get; }
    public static Dictionary<decimal, Cash> Cashs { set; get; }
    public static Dictionary<decimal, BankAccount> BankAccounts { set; get; }
    public static DateTime NomenklLastUpdate { set; get; } = new DateTime(1900, 1, 1);
    public static DateTime KontragentLastUpdate { set; get; } = new DateTime(1900, 1, 1);
    public static Dictionary<decimal, SD_111ViewModel> MutualTypes { get; }
    public static Dictionary<decimal, SDRSchet> SDRSchets { get; }
    public static Dictionary<decimal, SDRState> SDRStates { get; }
    public static Dictionary<decimal, CentrResponsibility> COList { get; }
    public static Dictionary<decimal, PayCondition> PayConditions { get; }
    public static Dictionary<decimal, VzaimoraschetType> VzaimoraschetTypes { get; }
    public static Dictionary<decimal, PayForm> FormRaschets { get; }
    public static Dictionary<decimal, CountriesViewModel> Countries { set; get; }
    public static Dictionary<decimal, Currency> Currencies { get; }
    public static Dictionary<decimal, Unit> Units { get; }
    public static Dictionary<decimal, NomenklGroup> NomenklGroups { get; }
    public static Dictionary<decimal, NomenklProductType> NomenklTypes { get; }
    public static Dictionary<decimal, Employee> Employees { get; }
    public static Dictionary<decimal, KontragentViewModel> AllKontragents { set; get; }
    public static Dictionary<Guid, AccruedAmountTypeViewModel> AccruedAmountTypes { set; get; }

    // ReSharper disable once InconsistentNaming
    public static Dictionary<decimal, Nomenkl> ALLNomenkls { set; get; }
    public static Dictionary<decimal, KontragentViewModel> ActiveKontragents { set; get; }
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

    private static void Clear()
    {
        AllKontragents.Clear();
        ALLNomenkls.Clear();
        ActiveKontragents.Clear();
        ActiveNomenkls.Clear();
        COList.Clear();
        PayConditions.Clear();
        VzaimoraschetTypes.Clear();
        FormRaschets.Clear();
        Countries.Clear();
        Currencies.Clear();
        Units.Clear();
        NomenklGroups.Clear();
        NomenklTypes.Clear();
        Employees.Clear();
        Warehouses.Clear();
        SDRSchets.Clear();
        SDRStates.Clear();
        MutualTypes.Clear();
        BankAccounts.Clear();
        Cashs.Clear();
        CashsAll.Clear();
        Projects.Clear();
        ContractTypes.Clear();
        DeliveryConditions.Clear();
        Regions.Clear();
        ClientKategory.Clear();
        AccruedAmountTypes.Clear();
    }

    public static AccruedAmountTypeViewModel GetAccruedAmountType(Guid id)
    {
        if (AccruedAmountTypes.ContainsKey(id))
            return AccruedAmountTypes[id];
        return null;
    }

    public static List<AccruedAmountTypeViewModel> GetAllAccruedAmountType()
    {
        return new List<AccruedAmountTypeViewModel>(AccruedAmountTypes.Values);
    }

    public static Employee GetEmployee(decimal dc)
    {
        if (Employees.ContainsKey(dc))
            return Employees[dc];
        return null;
    }

    public static Employee GetEmployee(int? tabelnumber)
    {
        if (tabelnumber == null) return null;
        return Employees.Values.FirstOrDefault(_ => _.TabelNumber == tabelnumber);
    }

    public static Employee GetEmployee(decimal? dc)
    {
        if (dc == null) return null;
        if (Employees.ContainsKey(dc.Value))
            return Employees[dc.Value];
        return null;
    }

    public static DeliveryCondition GetDeliveryCondition(decimal dc)
    {
        if (DeliveryConditions.ContainsKey(dc))
            return DeliveryConditions[dc];
        return null;
    }

    public static DeliveryCondition GetDeliveryCondition(decimal? dc)
    {
        if (dc == null) return null;
        if (DeliveryConditions.ContainsKey(dc.Value))
            return DeliveryConditions[dc.Value];
        return null;
    }

    public static Unit GetUnit(decimal dc)
    {
        if (Units.ContainsKey(dc))
            return Units[dc];
        return null;
    }

    public static Unit GetUnit(decimal? dc)
    {
        if (dc == null) return null;
        if (Units.ContainsKey(dc.Value))
            return Units[dc.Value];
        return null;
    }

    public static NomenklProductType GetNomenklProductType(decimal dc)
    {
        if (NomenklTypes.ContainsKey(dc))
            return NomenklTypes[dc];
        return null;
    }

    public static NomenklProductType GetNomenklProductType(decimal? dc)
    {
        if (dc == null) return null;
        if (NomenklTypes.ContainsKey(dc.Value))
            return NomenklTypes[dc.Value];
        return null;
    }

    public static NomenklGroup GetNomenklGroup(decimal dc)
    {
        if (NomenklGroups.ContainsKey(dc))
            return NomenklGroups[dc];
        return null;
    }


    public static NomenklGroup GetNomenklGroup(decimal? dc)
    {
        if (dc == null) return null;
        if (NomenklGroups.ContainsKey(dc.Value))
            return NomenklGroups[dc.Value];
        return null;
    }

    public static Warehouse GetWarehouse(decimal dc)
    {
        if (Warehouses.ContainsKey(dc))
            return Warehouses[dc];
        return null;
    }

    public static Warehouse GetWarehouse(decimal? dc)
    {
        if (dc == null) return null;
        if (Warehouses.ContainsKey(dc.Value))
            return Warehouses[dc.Value];
        return null;
    }

    public static ContractType GetContractType(decimal dc)
    {
        if (ContractTypes.ContainsKey(dc))
            return ContractTypes[dc];
        return null;
    }

    public static ContractType GetContractType(decimal? dc)
    {
        if (dc == null) return null;
        if (ContractTypes.ContainsKey(dc.Value))
            return ContractTypes[dc.Value];
        return null;
    }

    public static PayCondition GetPayCondition(decimal dc)
    {
        if (PayConditions.ContainsKey(dc))
            return PayConditions[dc];
        return null;
    }

    public static PayCondition GetPayCondition(decimal? dc)
    {
        if (dc == null) return null;
        if (PayConditions.ContainsKey(dc.Value))
            return PayConditions[dc.Value];
        return null;
    }

    public static PayForm GetPayForm(decimal dc)
    {
        if (FormRaschets.ContainsKey(dc))
            return FormRaschets[dc];
        return null;
    }

    public static VzaimoraschetType GetVzaimoraschetType(decimal? dc)
    {
        if (dc == null) return null;
        if (VzaimoraschetTypes.ContainsKey(dc.Value))
            return VzaimoraschetTypes[dc.Value];
        return null;
    }


    public static VzaimoraschetType GetVzaimoraschetType(decimal dc)
    {
        if (VzaimoraschetTypes.ContainsKey(dc))
            return VzaimoraschetTypes[dc];
        return null;
    }

    public static PayForm GetPayForm(decimal? dc)
    {
        if (dc == null) return null;
        if (FormRaschets.ContainsKey(dc.Value))
            return FormRaschets[dc.Value];
        return null;
    }

    private static List<string> getThemeNames()
    {
        return new List<string>(new[]
        {
            "MetropolisLight",
            "MetropolisDark", "Office2019Black", "Office2019Colorful",
            "Office2019DarkGray", "Office2019White", "Office2019HighContrast",
            "VS2019Blue", "VS2019Dark", "VS2019Light", "VS2017Blue",
            "VS2017Dark", "VS2017Light", "Office2016WhiteSE", "Office2016BlackSE",
            "Office2016ColorfulSE", "Office2016DarkGraySE",
            "Office2016White", "Office2016Black", "Office2016Colorful",
            "Office2013", "Office2013DarkGray", "Office2013LightGray",
            "Office2010Black", "Office2010Blue", "Office2010Silver",
            "Office2010Silver", "Office2007Black", "Office2007Blue", "Office2007Silver",
            "TouchlineDark", "DeepBlue", "DXStyle", "LightGray", "Seven", "VS2010"
        });
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
            //WindowManager.ShowMessage($"Номенклатура с кодом {dc} отсутствует.", "Ошибка", MessageBoxImage.Error);
            return null;

        return ALLNomenkls[dc];
    }

    private static bool IsNomMustUpdate(Nomenkl nom)
    {
        if (IsCheckedForUpdate)
        {
            var sql = "SELECT UpdateDate FROM SD_83 " +
                      $"WHERE DOC_CODE = {CustomFormat.DecimalToSqlDecimal(nom.DocCode)}";
            var d = GlobalOptions.GetEntities().Database.SqlQuery<DateTime>(sql).ToList();
            if (d.Count == 0 || d[0] != nom.UpdateDate) return true;
        }

        return false;
    }

    public static void UpdateNomenklForMain(Guid id)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var noms = ctx.SD_83.Where(_ => _.MainId == id);
            foreach (var n in noms)
            {
                if (ALLNomenkls.ContainsKey(n.DOC_CODE)) ALLNomenkls.Remove(n.DOC_CODE);
                var newNom = new Nomenkl();
                newNom.LoadFromEntity(n, null);
                ALLNomenkls.Add(n.DOC_CODE, newNom);
            }
        }
    }

    public static Nomenkl GetNomenkl(decimal dc)
    {
        if (ALLNomenkls.ContainsKey(dc))
        {
            if (IsNomMustUpdate(ALLNomenkls[dc])) LoadNomenkl(dc);
            return ALLNomenkls[dc];
        }

        LoadNomenkl(dc);
        if (!ALLNomenkls.ContainsKey(dc))
            if (dc != 0)
                //WindowManager.ShowMessage($"Номенклатура с кодом {dc} отсутствует.", "Ошибка", MessageBoxImage.Error);
                return null;

        return ALLNomenkls[dc];
    }

    public static Nomenkl GetNomenkl(Guid id)
    {
        var n = ALLNomenkls.Values.FirstOrDefault(_ => _.Id == id);
        if (n != null)
        {
            if (IsNomMustUpdate(n)) LoadNomenkl(n.DocCode);
            return ALLNomenkls[n.DocCode];
        }

        LoadNomenkl(id);
        if (ALLNomenkls.Values.All(_ => _.Id != id))
            //WindowManager.ShowMessage($"Номенклатура с кодом {id} отсутствует.", "Ошибка", MessageBoxImage.Error);
            return null;

        return null;
    }

    public static Nomenkl GetNomenkl(decimal? dc)
    {
        if (dc == null) return null;
        return GetNomenkl(dc.Value);
    }

    public static Currency GetCurrency(decimal? dc)
    {
        if (dc == null) return null;
        return GetCurrency(dc.Value);
    }

    public static Currency GetCurrency(decimal dc)
    {
        if (Currencies.ContainsKey(dc)) return Currencies[dc];
        return null;
    }

    private static void CheckUpdateKontragentAndLoad()
    {
        try
        {
            var ent = GlobalOptions.GetEntities();

            if (!ent.SD_43.Any(_ => _.UpdateDate > KontragentLastUpdate)) return;
            foreach (
                var newItem in
                ent.SD_43.Where(_ => _.UpdateDate > KontragentLastUpdate)
                    .AsNoTracking()
                    .Include(_ => _.SD_301)
                    .ToList()
                    .Select(item => new KontragentViewModel(item)))
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
        catch (Exception)
        {
            //WindowManager.ShowError(null, ex);
        }
    }

    //TODO Вставить проверку на обновление контрагента по дате обновления
    public static KontragentViewModel GetKontragent(decimal dc)
    {
        return AllKontragents.ContainsKey(dc) ? AllKontragents[dc] : null;
    }

    public static KontragentViewModel GetKontragent(decimal? dc)
    {
        return dc == null ? null : GetKontragent(dc.Value);
    }

    public static CentrResponsibility GetCO(decimal dc)
    {
        return COList.ContainsKey(dc) ? COList[dc] : null;
    }

    public static CentrResponsibility GetCO(decimal? dc)
    {
        return dc == null ? null : GetCO(dc.Value);
    }

    public static void Refresh()
    {
        try
        {
            var ent = GlobalOptions.GetEntities();

            #region Типы договоров/контрактов sd_102

            var s102 = ent.SD_102.AsNoTracking().ToList();
            foreach (var item in s102)
                if (ContractTypes.ContainsKey(item.DOC_CODE))
                {
                    var d = ContractTypes[item.DOC_CODE];
                    d.UpdateFrom(item);
                    d.myState = RowStatus.NotEdited;
                }
                else
                {
                    ContractTypes.Add(item.DOC_CODE, new ContractType(item) {myState = RowStatus.NotEdited});
                }

            #endregion

            #region Категории контрагентов

            var s148 = ent.SD_148.AsNoTracking().ToList();
            foreach (var item in s148)
                if (ClientKategory.ContainsKey(item.DOC_CODE))
                {
                    var d = ClientKategory[item.DOC_CODE];
                    d.UpdateFrom(item);
                    d.myState = RowStatus.NotEdited;
                }
                else
                {
                    ClientKategory.Add(item.DOC_CODE,
                        new CategoryClientTypeViewModel(item) {myState = RowStatus.NotEdited});
                }

            #endregion

            #region центр ответственности SD_40

            var s40 = ent.Set<SD_40>().AsNoTracking().ToList();
            if (!COList.ContainsKey(0))
                COList.Add(0, new CentrResponsibility
                {
                    DocCode = 0,
                    Name = "Центр ответественности не указан"
                });

            foreach (var item in s40)
                if (COList.ContainsKey(item.DOC_CODE))
                {
                    var d = COList[item.DOC_CODE];
                    d.LoadFromEntity(item);
                }
                else
                {
                    var co = new CentrResponsibility();
                    co.LoadFromEntity(item);
                    COList.Add(item.DOC_CODE, co);
                }

            foreach (var k in COList.Keys)
            {
                if (s40.Any(_ => _.DOC_CODE == k) || k == 0) continue;
                COList.Remove(k);
            }

            #endregion

            #region тип актов взаимозачета

            var s111 = ent.SD_111.AsNoTracking().ToList();
            foreach (var item in s111)
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
                if (SDRSchets.ContainsKey(l.DOC_CODE))
                {
                    var d = SDRSchets[l.DOC_CODE];
                }
                else
                {
                    var newItem = new SDRSchet();
                    newItem.LoadFromEntity(l, GlobalOptions.ReferencesCache);
                    SDRSchets.Add(l.DOC_CODE, newItem);
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
                if (SDRStates.ContainsKey(item.DOC_CODE))
                {
                    var d = SDRStates[item.DOC_CODE];
                }
                else
                {
                    var newItem = new SDRState();
                    newItem.LoadFromEntity(item);
                    SDRStates.Add(item.DOC_CODE, newItem);
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
                if (PayConditions.ContainsKey(item.DOC_CODE))
                {
                    var d = PayConditions[item.DOC_CODE];
                    d.LoadFromEntity(item);
                }
                else
                {
                    var newItem = new PayCondition();
                    newItem.LoadFromEntity(item);
                    PayConditions.Add(item.DOC_CODE, newItem);
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

            keys = VzaimoraschetTypes.Keys.ToList();
            foreach (var k in keys)
            {
                if (s77.Any(_ => _.DOC_CODE == k)) continue;
                VzaimoraschetTypes.Remove(k);
            }

            #endregion

            #region Форма расчетов sd_189

            var s189 = ent.SD_189.Include(_ => _.SD_179).AsNoTracking().ToList();
            foreach (var item in s189)
                if (FormRaschets.ContainsKey(item.DOC_CODE))
                {
                    var d = FormRaschets[item.DOC_CODE];
                    d.LoadFromEntity(item);
                }
                else
                {
                    var newItem = new PayForm();
                    newItem.LoadFromEntity(item);
                    FormRaschets.Add(item.DOC_CODE, newItem);
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
                if (Countries.ContainsKey(item.Iso))
                {
                    var d = Countries[item.Iso];
                    d.UpdateFrom(item);
                    d.myState = RowStatus.NotEdited;
                }
                else
                {
                    Countries.Add(item.Iso, new CountriesViewModel(item) {myState = RowStatus.NotEdited});
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
                if (Currencies.ContainsKey(item.DOC_CODE))
                {
                    var d = Currencies[item.DOC_CODE];
                    d.LoadFromEntity(item);
                }
                else
                {
                    var newItem = new Currency();
                    newItem.LoadFromEntity(item);
                    Currencies.Add(item.DOC_CODE, newItem);
                }

            Currencies.Add(0m, new Currency
            {
                DocCode = 0m,
                Name = "Валюта не указана"
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
                if (Units.ContainsKey(item.DOC_CODE))
                {
                    var d = Units[item.DOC_CODE];
                    d.LoadFromEntity(item);
                }
                else
                {
                    var newUnit = new Unit();
                    newUnit.LoadFromEntity(item);
                    Units.Add(item.DOC_CODE, newUnit);
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
                if (NomenklGroups.ContainsKey(item.DOC_CODE))
                {
                    var d = NomenklGroups[item.DOC_CODE];
                    d.LoadFromEntity(item);
                }
                else
                {
                    var newItem = new NomenklGroup();
                    newItem.LoadFromEntity(item);
                    NomenklGroups.Add(item.DOC_CODE, newItem);
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
                if (BankAccounts.ContainsKey(item.DOC_CODE))
                {
                    var bank = new Bank();
                    bank.LoadFromEntity(item.SD_44);
                    var d = BankAccounts[item.DOC_CODE];
                    d.Name = $"{bank.Name} Cч.№{item.BA_RASH_ACC} {Currencies[(decimal) item.CurrencyDC]}";
                    //d.BankDC = item.BA_BANKDC;
                    d.Bank = bank;
                    //d.Account = item.BA_RASH_ACC;
                    //d.myState = RowStatus.NotEdited;
                    d.Currency = item.CurrencyDC != null ? Currencies[(decimal) item.CurrencyDC] : null;
                }
                else
                {
                    var bank = new Bank();
                    bank.LoadFromEntity(item.SD_44);
                    BankAccounts.Add(item.DOC_CODE, new BankAccount
                    {
                        DocCode = item.DOC_CODE,
                        //BankDC = item.BA_BANKDC,
                        Bank = bank,
                        Name = $"{bank.Name} Cч.№{item.BA_RASH_ACC} {Currencies[(decimal) item.CurrencyDC]}",
                        //Account = item.BA_RASH_ACC,
                        //myState = RowStatus.NotEdited,
                        Currency = item.CurrencyDC != null ? Currencies[(decimal) item.CurrencyDC] : null
                    });
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
                if (CashsAll.ContainsKey(item.DOC_CODE))
                {
                    var d = CashsAll[item.DOC_CODE];
                    d.UpdateFrom(item);
                    d.myState = RowStatus.NotEdited;
                }
                else
                {
                    var ch = new Cash(item)
                    {
                        IsAccessRight = cashAcc.Any(_ => item.DOC_CODE == _.DocCode),
                        myState = RowStatus.Deleted
                    };
                    Cashs.Add(item.DOC_CODE, ch);
                    CashsAll.Add(item.DOC_CODE, ch);
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

            keys = Regions.Keys.ToList();
            foreach (var k in keys)
            {
                if (s23.Any(_ => _.DOC_CODE == k)) continue;
                Regions.Remove(k);
            }

            #endregion

            #region AccruedAmountTypes

            var acct = ent.AccruedAmountType.ToList();
            foreach (var a in acct)
                if (!AccruedAmountTypes.ContainsKey(a.Id))
                {
                    AccruedAmountTypes.Add(a.Id, new AccruedAmountTypeViewModel(a));
                }
                else
                {
                    var a1 = AccruedAmountTypes[a.Id];
                    a1.Name = a.Name;
                    a1.IsClient = a.IsClient;
                    a1.IsSupplier = a.IsSupplier;
                    a1.Note = a.Note;
                }

            #endregion
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(null, ex);
        }
    }

    private static void UpdateNomenkl()
    {
        var ent = GlobalOptions.GetEntities();
        var sql = " SELECT SD_83.DOC_CODE as DOC_CODE, NOM_NAME, NOM_NOMENKL, NOM_CATEG_DC, " +
                  " SD_175.DOC_CODE as UNIT_DC, SD_175.ED_IZM_NAME AS UNIT_NAME, SD_175.ED_IZM_OKEI_CODE as UNIT_CODE, " +
                  " NOM_SALE_CRS_DC as NOM_SALE_CRS_DC, SD_83.Id as Id, UpdateDate as UpdateDate, NOM_0MATER_1USLUGA, MainId as MainId, " +
                  " isnull(sd_83.NOM_PRODUCT_DC,0) as NOM_PRODUCT_DC, " +
                  " ISNULL(IsCurrencyTransfer,0) as IsCurrencyTransfer " +
                  " FROM SD_83 (nolock) " +
                  " INNER JOIN SD_175 (nolock) ON SD_175.DOC_CODE = SD_83.NOM_ED_IZM_DC " +
                  $" and SD_83.UpdateDate > '{CustomFormat.DateWithTimeToString(NomenklLastUpdate)}'";
        var data = ent.Database.SqlQuery<NomenklShort>(sql).ToList();
        if (data.Count == 0) return;
        foreach (var d in data)
            if (ALLNomenkls.ContainsKey(d.DOC_CODE))
            {
                var n = ALLNomenkls[d.DOC_CODE];
                n.DocCode = d.DOC_CODE;
                n.Name = d.NOM_NAME;
                n.NomenklNumber = d.NOM_NOMENKL;
                n.Currency = GetCurrency(d.NOM_SALE_CRS_DC);
                n.Id = d.Id;
                // ReSharper disable once PossibleInvalidOperationException
                n.MainId = (Guid) d.MainId;
                n.UpdateDate = d.UpdateDate;
                n.IsUsluga = d.NOM_0MATER_1USLUGA == 1;
                n.Group = GlobalOptions.ReferencesCache.GetNomenklGroup(d.NOM_CATEG_DC);
                n.ProductType = GlobalOptions.ReferencesCache.GetProductType(d.NOM_PRODUCT_DC);
                n.IsCurrencyTransfer = d.IsCurrencyTransfer;
            }
            else
            {
                var newNom = new Nomenkl();
                newNom.LoadFromEntity(new SD_83
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
                    IsCurrencyTransfer = d.IsCurrencyTransfer
                }, null);
                newNom.Unit = Units[d.UNIT_DC];
                newNom.Currency = Currencies[d.NOM_SALE_CRS_DC];
                newNom.Group = GlobalOptions.ReferencesCache.GetNomenklGroup(d.NOM_CATEG_DC);
                ALLNomenkls.Add(d.DOC_CODE, newNom);
            }

        NomenklLastUpdate = data.Max(_ => _.UpdateDate);
    }

    public static void LoadNomenkl(decimal dc)
    {
        var ent = GlobalOptions.GetEntities();
        var data = ent.Database.SqlQuery<NomenklShort>(
            " SELECT SD_83.DOC_CODE as DOC_CODE, NOM_NAME, NOM_NOMENKL, NOM_CATEG_DC, " +
            " SD_175.DOC_CODE as UNIT_DC, SD_175.ED_IZM_NAME AS UNIT_NAME, SD_175.ED_IZM_OKEI_CODE as UNIT_CODE, " +
            " NOM_SALE_CRS_DC as NOM_SALE_CRS_DC, SD_83.Id as Id, UpdateDate as UpdateDate, NOM_0MATER_1USLUGA, MainId as MainId, " +
            " isnull(sd_83.NOM_PRODUCT_DC,0) as NOM_PRODUCT_DC, isnull(sd_83.IsUslugaInRent,0) as IsRentabelnost, " +
            " ISNULL(IsCurrencyTransfer,0) as IsCurrencyTransfer " +
            " FROM SD_83 " +
            " INNER JOIN SD_175 ON SD_175.DOC_CODE = SD_83.NOM_ED_IZM_DC " +
            " and SD_83.DOC_CODE = '" + CustomFormat.DecimalToSqlDecimal(dc) + "'").ToList();
        if (data.Count == 0) return;
        var d = data[0];
        if (ALLNomenkls.ContainsKey(d.DOC_CODE)) ALLNomenkls.Remove(d.DOC_CODE);
        var newNom = new Nomenkl();
        newNom.LoadFromEntity(new SD_83
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
            IsCurrencyTransfer = d.IsCurrencyTransfer
        }, null);
        newNom.Unit = Units[d.UNIT_DC];
        newNom.Currency = Currencies[d.NOM_SALE_CRS_DC];
        newNom.Group = GlobalOptions.ReferencesCache.GetNomenklGroup(d.NOM_CATEG_DC);
        ALLNomenkls.Add(d.DOC_CODE, newNom);
    }


    public static void LoadNomenkl(Guid id)
    {
        var ent = GlobalOptions.GetEntities();
        var data = ent.Database.SqlQuery<NomenklShort>(
            " SELECT SD_83.DOC_CODE as DOC_CODE, NOM_NAME, NOM_NOMENKL, NOM_CATEG_DC, " +
            " SD_175.DOC_CODE as UNIT_DC, SD_175.ED_IZM_NAME AS UNIT_NAME, SD_175.ED_IZM_OKEI_CODE as UNIT_CODE, " +
            " NOM_SALE_CRS_DC as NOM_SALE_CRS_DC, SD_83.Id as Id, UpdateDate as UpdateDate, NOM_0MATER_1USLUGA, MainId as MainId, " +
            " isnull(sd_83.NOM_PRODUCT_DC,0) as NOM_PRODUCT_DC, isnull(sd_83.IsUslugaInRent,0) as IsRentabelnost, " +
            " ISNULL(IsCurrencyTransfer,0) as IsCurrencyTransfer " +
            " FROM SD_83 " +
            " INNER JOIN SD_175 ON SD_175.DOC_CODE = SD_83.NOM_ED_IZM_DC " +
            $" and SD_83.Id = '{id}'").ToList();
        if (data.Count == 0) return;
        var d = data[0];
        if (ALLNomenkls.ContainsKey(d.DOC_CODE)) ALLNomenkls.Remove(d.DOC_CODE);
        var newNom = new Nomenkl();
        newNom.LoadFromEntity(new SD_83
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
            IsCurrencyTransfer = d.IsCurrencyTransfer
        }, null);
        newNom.Unit = Units[d.UNIT_DC];
        newNom.Currency = Currencies[d.NOM_SALE_CRS_DC];
        newNom.Group = GlobalOptions.ReferencesCache.GetNomenklGroup(d.NOM_CATEG_DC);
        ALLNomenkls.Add(d.DOC_CODE, newNom);
    }

    public static void LoadNomenkl()
    {
        //ALLNomenkls.Clear();
        try
        {
            var ent = GlobalOptions.GetEntities();
            var data = ent.Database.SqlQuery<NomenklShort>(
                    " SELECT SD_83.DOC_CODE as DOC_CODE, NOM_NAME, NOM_NOMENKL, NOM_CATEG_DC, " +
                    " SD_175.DOC_CODE as UNIT_DC, SD_175.ED_IZM_NAME AS UNIT_NAME, SD_175.ED_IZM_OKEI_CODE as UNIT_CODE, " +
                    " NOM_SALE_CRS_DC as NOM_SALE_CRS_DC, SD_83.Id as Id, UpdateDate as UpdateDate, NOM_0MATER_1USLUGA, MainId as MainId, " +
                    " sd_83.NOM_PRODUCT_DC as NOM_PRODUCT_DC, " +
                    " ISNULL(IsCurrencyTransfer,0) as IsCurrencyTransfer " +
                    " FROM SD_83 " +
                    " INNER JOIN SD_175 ON SD_175.DOC_CODE = SD_83.NOM_ED_IZM_DC " +
                    " and isnull(UpdateDate,getdate()) > '" + CustomFormat.DateToString(NomenklLastUpdate) +
                    "'")
                .ToList();
            foreach (var d in data)
            {
                var newNom = new Nomenkl();
                newNom.LoadFromEntity(new SD_83
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
                    IsCurrencyTransfer = d.IsCurrencyTransfer
                }, null);
                newNom.Unit = Units[d.UNIT_DC];
                newNom.Currency = Currencies[d.NOM_SALE_CRS_DC];
                newNom.Group = GlobalOptions.ReferencesCache.GetNomenklGroup(d.NOM_CATEG_DC);
                ALLNomenkls.Add(d.DOC_CODE, newNom);
            }
            // ReSharper disable once PossibleInvalidOperationException
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(null, ex);
        }

        var dm = ALLNomenkls.Values.Select(_ => _.UpdateDate).ToList();
        // var dateTimes = dm as IList<DateTime?> ?? dm.ToList();
        if (dm.Any())
        {
            var dateTime = dm.Max();
            if (dateTime != null) NomenklLastUpdate = dateTime;
        }
        else
        {
            NomenklLastUpdate = DateTime.Now;
        }
    }

    public static void LoadKontragents()
    {
        //AllKontragents.Clear();
        try
        {
            var ent = GlobalOptions.GetEntities();
            foreach (
                var item in
                ent.SD_43.Where(_ => _.UpdateDate > KontragentLastUpdate)
                    .AsNoTracking()
                    .Include(_ => _.SD_301)
                    .ToList())
            {
                var newItem = new KontragentViewModel(item);
                if (AllKontragents.ContainsKey(newItem.DOC_CODE))
                {
                    AllKontragents[newItem.DOC_CODE] = newItem;
                    if ((newItem.DELETED ?? 0) == 0)
                        ActiveKontragents[newItem.DOC_CODE] = newItem;
                }
                else
                {
                    AllKontragents.Add(newItem.DOC_CODE, newItem);
                    if ((newItem.DELETED ?? 0) == 0)
                        ActiveKontragents.Add(newItem.DOC_CODE, newItem);
                }
            }

            // ReSharper disable once PossibleInvalidOperationException
            if (AllKontragents.Count > 0)
                KontragentLastUpdate = (DateTime) AllKontragents.Values.Select(_ => _.UpdateDate).Max();
            else
                KontragentLastUpdate = DateTime.Now;
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(null, ex);
        }
    }

    public static void Reset(LoadReferenceStrategy strategy = LoadReferenceStrategy.Full, bool isRefresh = true)
    {
        if (isRefresh)
            Refresh();
        switch (strategy)
        {
            case LoadReferenceStrategy.Full:
                ALLNomenkls.Clear();
                AllKontragents.Clear();
                Task.Run(() => LoadNomenkl()).ContinueWith(_ => SetNomComplete());
                Task.Run(() => LoadKontragents()).ContinueWith(_ => SetKontrComplete());
                break;
            case LoadReferenceStrategy.WithoutKontragent:
                ALLNomenkls.Clear();
                Task.Run(() => LoadNomenkl()).ContinueWith(_ => SetNomComplete());
                break;
            case LoadReferenceStrategy.WithoutNomenkl:
                AllKontragents.Clear();
                Task.Run(() => LoadKontragents()).ContinueWith(_ => SetKontrComplete());
                break;
            case LoadReferenceStrategy.WithoutKontragentAndNomenkl:
                IsReferenceLoadComplete = true;
                break;
        }
    }

    private static void SetNomComplete()
    {
        myIsNomComplete = true;
        if (myIsKontrComplete && myUpdateTimer == null)
            myUpdateTimer = new Timer(_ =>
            {
                //Refresh();
                UpdateNomenkl();
                CheckUpdateKontragentAndLoad();
            }, null, 1000 * 60, Timeout.Infinite);
    }

    private static void SetKontrComplete()
    {
        myIsKontrComplete = true;
        if (myIsNomComplete && myUpdateTimer == null)
            myUpdateTimer = new Timer(_ =>
            {
                //Refresh();
                UpdateNomenkl();
                CheckUpdateKontragentAndLoad();
            }, null, 1000 * 60, Timeout.Infinite);
    }

    public static Dictionary<decimal, KontragentViewModel> GetAllKontragents()
    {
        if (AllKontragents.Count == 0)
        {
            try
            {
                var ent = GlobalOptions.GetEntities();
                foreach (var item in ent.SD_43.AsNoTracking().Include(_ => _.SD_301).ToList())
                {
                    var newItem = new KontragentViewModel(item);
                    AllKontragents.Add(newItem.DOC_CODE, newItem);
                    if ((item.DELETED ?? 0) == 0)
                        ActiveKontragents.Add(newItem.DOC_CODE, newItem);
                }
            }

            catch (Exception ex)
            {
                //WindowManager.ShowError(null, ex);
            }
        }
        else
        {
            var dateTime = AllKontragents.Values.Max(_ => _.Entity.UpdateDate);
            if (dateTime == null) return AllKontragents;
            var date = (DateTime) dateTime;
            try
            {
                var ent = GlobalOptions.GetEntities();
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
                        var newItem = new KontragentViewModel(item);
                        AllKontragents.Add(newItem.DOC_CODE, newItem);
                        if ((item.DELETED ?? 0) == 0)
                            ActiveKontragents.Add(newItem.DOC_CODE, newItem);
                    }
                    else
                    {
                        AllKontragents.Remove(item.DOC_CODE);
                        ActiveKontragents.Remove(item.DOC_CODE);
                        var newItem = new KontragentViewModel(item);
                        AllKontragents.Add(item.DOC_CODE, newItem);
                        if ((item.DELETED ?? 0) == 0)
                            ActiveKontragents.Add(item.DOC_CODE, newItem);
                    }
            }
            catch (Exception ex)
            {
                //WindowManager.ShowError(null, ex);
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
                        var newItem = new Nomenkl();
                        newItem.LoadFromEntity(item, GlobalOptions.ReferencesCache);
                        ALLNomenkls.Add(item.DOC_CODE, newItem);
                        if ((item.NOM_DELETED ?? 0) == 0)
                            ActiveNomenkls.Add(item.DOC_CODE, newItem);
                    }
                }
            }
            catch (Exception ex)
            {
                //WindowManager.ShowError(null, ex);
            }
        }
        else
        {
            var dateTime = ALLNomenkls.Values.Max(_ => _.UpdateDate);
            if (dateTime == null) return ALLNomenkls;
            var date = dateTime;
            try
            {
                var ent = GlobalOptions.GetEntities();
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
                        var newItem = new Nomenkl();
                        newItem.LoadFromEntity(item, GlobalOptions.ReferencesCache);
                        ALLNomenkls.Add(item.DOC_CODE, newItem);
                        if ((item.NOM_DELETED ?? 0) == 0)
                            ActiveNomenkls.Add(item.DOC_CODE, newItem);
                    }
                    else
                    {
                        ALLNomenkls.Remove(item.DOC_CODE);
                        ActiveNomenkls.Remove(item.DOC_CODE);
                        var newItem = new Nomenkl();
                        newItem.LoadFromEntity(item, GlobalOptions.ReferencesCache);
                        ALLNomenkls.Add(item.DOC_CODE, newItem);
                        if ((item.NOM_DELETED ?? 0) == 0)
                            ActiveNomenkls.Add(item.DOC_CODE, newItem);
                    }
            }
            catch (Exception ex)
            {
                //WindowManager.ShowError(null, ex);
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

    public bool IsCurrencyTransfer { set; get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore InconsistentNaming
}
