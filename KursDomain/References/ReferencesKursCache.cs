using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.References;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public class ReferencesKursCache : IReferencesCache
{
    #region Constructors

    public ReferencesKursCache(DbContext dbContext)
    {
        Context = dbContext as KursContext.KursContext;
        if (Context != null) sqlConnect = new SqlConnection(Context.Database.Connection.ConnectionString);
    }

    #endregion

    #region fields

    private readonly KursContext.KursContext Context;
    private readonly SqlConnection sqlConnect;
    private DateTime lastTimeCheckTrackerId;
    private readonly int diffSecondsForCheckTracker = 0;

    #endregion

    #region Properties

    public bool IsChangeTrackingOn { set; get; }
    public DbContext DBContext => Context;

    #endregion

    #region Tracking

    private class ChangeTrackingResultDC
    {
        public string Operation { set; get; }
        public decimal DocCode { set; get; }
    }

    private class ChangeTrackingResultInt
    {
        public string Operation { set; get; }
        public int Code { set; get; }
    }

    private class ChangeTrackingResultGuid
    {
        public string Operation { set; get; }
        public Guid Id { set; get; }
    }

    private long GetCurrentChangeTrackingId()
    {
        using (var sqlConn = new SqlConnection(GlobalOptions.SqlConnectionString))
        {
            try
            {
                sqlConn.Open();
                long id = 0;
                var cmd = sqlConn.CreateCommand();
                cmd.CommandText = "SELECT change_tracking_current_version();";
                cmd.CommandType = CommandType.Text;
                var res = cmd.ExecuteScalar();
                id = (long)res;
                sqlConn.Close();
                lastTimeCheckTrackerId = DateTime.Now;
                return id;
            }
            catch (Exception ex)
            {
                saveError(ex, "ReferenceKursCache.GetCurrentChangeTrackingId()");
            }
        }

        return 0;
    }

    private void saveError(Exception ex, string methodName)
    {
        using (var sqlConn = new SqlConnection(GlobalOptions.SqlSystemConnectionString))
        {
            sqlConn.Open();
            var cmd = sqlConn.CreateCommand();
            cmd.CommandText =
                @"INSERT INTO dbo.Errors(Id, DbId, UserId, Host, ErrorText, Note, Moment)"
                + $"VALUES('{Helper.CustomFormat.GuidToSqlString(Guid.NewGuid())}', " +
                $"'{Helper.CustomFormat.GuidToSqlString(GlobalOptions.DataBaseId)}'," +
                $"'{Helper.CustomFormat.GuidToSqlString(GlobalOptions.UserInfo.KursId)}'," +
                $"'{Environment.MachineName}'," +
                $"'{Helper.CustomFormat.GetFullExceptionTextMessage(ex, methodName)}'," +
                $"'', '{Helper.CustomFormat.DateToString(DateTime.Now)}')";
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();
            sqlConn.Close();

        }
    }

    private IEnumerable<ChangeTrackingResultDC> GetChangeData(string tabName, long trackingId)
    {
        var sql =
            $"SELECT sys_change_operation as Operation, doc_code as DocCode FROM CHANGETABLE(CHANGES {tabName}, {trackingId}) as Cht";

        using (var sqlConn = new SqlConnection(GlobalOptions.SqlConnectionString))
        {
            try
            {
                sqlConn.Open();
                var cmd = sqlConn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                var ret = new List<ChangeTrackingResultDC>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        ret.Add(new ChangeTrackingResultDC
                        {
                            Operation = reader[0] as string,
                            DocCode = (decimal)reader[1]
                        });
                }

                sqlConn.Close();
                return ret;
            }
            catch (Exception ex)
            {
                saveError(ex, "ReferenceKursCache.GetCurrentChangeTrackingId()");
                return new List<ChangeTrackingResultDC>();
            }
        }
    }

    private IEnumerable<ChangeTrackingResultInt> GetChangeDataInt(string tabName, long trackingId, string keyFieldName)
    {
        var sql =
            $"SELECT sys_change_operation as Operation, {keyFieldName} as Code FROM CHANGETABLE(CHANGES {tabName}, {trackingId}) as Cht";

        using (var sqlConn = new SqlConnection(GlobalOptions.SqlConnectionString))
        {
            try
            {
                sqlConn.Open();
                var cmd = sqlConn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                var ret = new List<ChangeTrackingResultInt>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        ret.Add(new ChangeTrackingResultInt
                        {
                            Operation = reader[0] as string,
                            Code = (int)reader[1]
                        });
                }

                sqlConn.Close();
                return ret;
            }
            catch (Exception ex)
            {
                saveError(ex, "ReferenceKursCache.GetCurrentChangeTrackingId()");
                return new List<ChangeTrackingResultInt>();
            }
        }
    }

    //ChangeTrackingResultGuid
    private IEnumerable<ChangeTrackingResultGuid> GetChangeDataGuid(string tabName, long trackingId,
        string keyFieldName)
    {
        var sql =
            $"SELECT sys_change_operation as Operation, {keyFieldName} as Id FROM CHANGETABLE(CHANGES {tabName}, {trackingId}) as Cht";
        try
        {
            using (var sqlConn = new SqlConnection(GlobalOptions.SqlConnectionString))
            {
                sqlConn.Open();
                var cmd = sqlConn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                var ret = new List<ChangeTrackingResultGuid>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        ret.Add(new ChangeTrackingResultGuid
                        {
                            Operation = reader[0] as string,
                            Id = (Guid)reader[1]
                        });
                }

                sqlConn.Close();
                return ret;
            }
        }
        catch (Exception ex)
        {
            saveError(ex, "ReferenceKursCache.GetCurrentChangeTrackingId()");
            return new List<ChangeTrackingResultGuid>();
        }
    }

    /// <summary>
    ///     SD_77
    /// </summary>
    private long NomenklProductTypesTrackingId;

    /// <summary>
    ///     UD_43
    /// </summary>
    private long KontragentGroupsTrackingId;

    /// <summary>
    ///     sd_103
    /// </summary>
    private long DeliveryConditionsTrackingId;

    /// <summary>
    ///     sd_43
    /// </summary>
    private long KontragentsTrackingId;

    /// <summary>
    ///     sd_83
    /// </summary>
    private long NomenklsTrackingId;

    /// <summary>
    ///     NomenklMain
    /// </summary>
    private long NomenklMainTrackingId;

    /// <summary>
    ///     sd_82
    /// </summary>
    private long NomenklGroupsTrackingId;

    /// <summary>
    ///     sd_27
    /// </summary>
    private long WarehousesTrackingId;

    /// <summary>
    ///     sd_2
    /// </summary>
    private long EmployeesTrackingId;

    /// <summary>
    ///     SD_44
    /// </summary>
    private long BanksTrackingId;

    /// <summary>
    ///     SD_114
    /// </summary>
    private long BankAccountssTrackingId;

    /// <summary>
    ///     SD_40
    /// </summary>
    private long CentrResponsibilitiesTrackingId;

    /// <summary>
    ///     SD_303
    /// </summary>
    private long SDRSchetsTrackingId;

    /// <summary>
    ///     sd_99
    /// </summary>
    private long SDRStatesTrackingId;

    /// <summary>
    ///     SD_148
    /// </summary>
    private long ClientCategoriesTrackingId;

    /// <summary>
    ///     SD_301
    /// </summary>
    private long CurrenciesTrackingId;

    /// <summary>
    ///     SD_23
    /// </summary>
    private long RegionsTrackingId;

    /// <summary>
    ///     SD_175
    /// </summary>
    private long UnitsTrackingId;

    /// <summary>
    ///     SD_22
    /// </summary>
    private long CashBoxesTrackingId;

    /// <summary>
    ///     SD_111
    /// </summary>
    private long MutualSettlementTypesTrackingId;

    /// <summary>
    ///     Countries
    /// </summary>
    private long CountriesTrackingId;

    /// <summary>
    ///     Projects
    /// </summary>
    private long ProjectsTrackingId;

    /// <summary>
    ///     sd_102
    /// </summary>
    private long ContractTypesTrackingId;

    /// <summary>
    ///     sd_119
    /// </summary>
    private long NomenklTypesTrackingId;

    /// <summary>
    ///     SD_50
    /// </summary>
    private long ProductTypesTrackingId;

    /// <summary>
    ///     SD_189
    /// </summary>
    private long PayFormsTrackingId;

    /// <summary>
    ///     SD_179
    /// </summary>
    private long PayConditionsTrackingId;

    #endregion

    #region Форма оплаты SD_189

    private void UpdateCachePayForm()
    {
        var changed = GetChangeData("SD_189", CashBoxesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (PayForms.ContainsKey(ch.DocCode)) PayForms.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_189.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newCB = new PayForm();
                                newCB.LoadFromEntity(item);
                                PayForms.Add(newCB.DocCode, newCB);
                            }
                            else
                            {
                                if (PayForms.ContainsKey(ch.DocCode))
                                    ((PayForm) PayForms[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        PayFormsTrackingId = GetCurrentChangeTrackingId();
    }

    public IPayForm GetPayForm(decimal? dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCachePayForm();
        if (dc == null) return null;
        if (PayForms.ContainsKey(dc.Value))
            return PayForms[dc.Value];
        var data = Context.SD_189.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new PayForm();
        newItem.LoadFromEntity(data);
        PayForms.Add(data.DOC_CODE, newItem);
        return PayForms[data.DOC_CODE];
    }

    public IEnumerable<IPayForm> GetPayFormAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCachePayForm();
        return PayForms.Values.ToList();
    }

    #endregion

    #region Условия оплаты SD_179

    private void UpdateCachePayConditions()
    {
        var changed = GetChangeData("SD_179", PayConditionsTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (PayConditions.ContainsKey(ch.DocCode)) PayConditions.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_179.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new PayCondition();
                                newItem.LoadFromEntity(item);
                                PayConditions.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (PayConditions.ContainsKey(ch.DocCode))
                                    ((PayCondition) PayConditions[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        PayConditionsTrackingId = GetCurrentChangeTrackingId();
    }

    public IPayCondition GetPayCondition(decimal? dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCachePayConditions();
        if (dc == null) return null;
        if (PayConditions.ContainsKey(dc.Value))
            return PayConditions[dc.Value];
        var data = Context.SD_179.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new PayCondition();
        newItem.LoadFromEntity(data);
        PayConditions.Add(data.DOC_CODE, newItem);
        return PayConditions[data.DOC_CODE];
    }

    public IEnumerable<IPayCondition> GetPayConditionAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCachePayConditions();
        return PayConditions.Values.ToList();
    }

    #endregion

    #region Проекты

    public IProject GetProject(Guid? id)
    {
        if (id == null) return null;
        if (Projects.ContainsKey(id.Value))
            return Projects[id.Value];
        var data = Context.Projects.FirstOrDefault(_ => _.Id == id.Value);
        if (data == null) return null;
        var newItem = new Project();
        newItem.LoadFromEntity(data, this);
        Projects.Add(data.Id, newItem);
        return Projects[data.Id];
    }

    public IEnumerable<IProject> GetProjectsAll()
    {
        return Projects.Values.ToList();
    }

    #endregion

    #region Типы договоров

    private void UpdateCacheContractType()
    {
        var changed = GetChangeData("SD_102", ContractTypesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (ContractTypes.ContainsKey(ch.DocCode)) ContractTypes.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_102.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new ContractType();
                                newItem.LoadFromEntity(item);
                                ContractTypes.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (ContractTypes.ContainsKey(ch.DocCode))
                                    ((ContractType) ContractTypes[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        ContractTypesTrackingId = GetCurrentChangeTrackingId();
    }

    public IContractType GetContractType(decimal? dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheContractType();
        if (dc == null) return null;
        if (ContractTypes.ContainsKey(dc.Value))
            return ContractTypes[dc.Value];
        var data = Context.SD_102.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new ContractType();
        newItem.LoadFromEntity(data);
        ContractTypes.Add(data.DOC_CODE, newItem);
        return ContractTypes[data.DOC_CODE];
    }

    public IEnumerable<IContractType> GetContractsTypeAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheContractType();
        return ContractTypes.Values.ToList();
    }

    #endregion

    #region Методы

    public void StartLoad()
    {
        Clear();
        var currentTrackingId = Context.Database.SqlQuery<long>("SELECT change_tracking_current_version();");
        KontragentGroupsTrackingId = DeliveryConditionsTrackingId =
            KontragentsTrackingId = NomenklsTrackingId = NomenklGroupsTrackingId = WarehousesTrackingId =
                EmployeesTrackingId = BanksTrackingId = BankAccountssTrackingId = CentrResponsibilitiesTrackingId =
                    SDRSchetsTrackingId = SDRStatesTrackingId = ClientCategoriesTrackingId =
                        CurrenciesTrackingId = RegionsTrackingId = UnitsTrackingId = CashBoxesTrackingId =
                            MutualSettlementTypesTrackingId = CountriesTrackingId = ProjectsTrackingId =
                                ContractTypesTrackingId =
                                    NomenklTypesTrackingId = ProductTypesTrackingId = PayFormsTrackingId =
                                        PayConditionsTrackingId = NomenklProductTypesTrackingId =
                                            NomenklMainTrackingId =
                                                currentTrackingId.First();

        foreach (var item in Context.SD_301.AsNoTracking().ToList())
        {
            var newItem = new Currency();
            newItem.LoadFromEntity(item);
            if (!Currencies.ContainsKey(newItem.DocCode))
                Currencies.Add(newItem.DocCode, newItem);
        }


        foreach (var item in Context.Countries.AsNoTracking().ToList())
        {
            var newItem = new Country();
            newItem.LoadFromEntity(item);
            if (!Countries.ContainsKey(newItem.Id))
                Countries.Add(newItem.Id, newItem);
        }

        foreach (var item in Context.SD_23.AsNoTracking().ToList())
        {
            var newItem = new Region();
            newItem.LoadFromEntity(item);
            if (!Regions.ContainsKey(newItem.DocCode))
                Regions.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_179.AsNoTracking().ToList())
        {
            var newItem = new PayCondition();
            newItem.LoadFromEntity(item);
            if (!PayConditions.ContainsKey(newItem.DocCode))
                PayConditions.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_189.AsNoTracking().ToList())
        {
            var newItem = new PayForm();
            newItem.LoadFromEntity(item);
            if (!PayForms.ContainsKey(newItem.DocCode))
                PayForms.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_175.AsNoTracking().ToList())
        {
            var newItem = new Unit();
            newItem.LoadFromEntity(item);
            if (!Units.ContainsKey(newItem.DocCode))
                Units.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_40.AsNoTracking().ToList())
        {
            var newItem = new CentrResponsibility();
            newItem.LoadFromEntity(item);
            if (!CentrResponsibilities.ContainsKey(newItem.DocCode))
                CentrResponsibilities.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_99.AsNoTracking().ToList())
        {
            var newItem = new SDRState();
            newItem.LoadFromEntity(item);
            if (!SDRStates.ContainsKey(newItem.DocCode))
                SDRStates.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_303.AsNoTracking().ToList())
        {
            var newItem = new SDRSchet();
            newItem.LoadFromEntity(item, this);
            if (!SDRSchets.ContainsKey(newItem.DocCode))
                SDRSchets.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_148.AsNoTracking().ToList())
        {
            var newItem = new ClientCategory();
            newItem.LoadFromEntity(item);
            if (!ClientCategories.ContainsKey(newItem.DocCode))
                ClientCategories.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_77.AsNoTracking().ToList())
        {
            var newItem = new NomenklProductType();
            newItem.LoadFromEntity(item, this);
            if (!NomenklProductTypes.ContainsKey(newItem.DocCode))
                NomenklProductTypes.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.UD_43.AsNoTracking().ToList())
        {
            var newItem = new KontragentGroup();
            newItem.LoadFromEntity(item);
            if (!KontragentGroups.ContainsKey(newItem.Id))
                KontragentGroups.Add(newItem.Id, newItem);
        }

        foreach (var item in Context.SD_82.AsNoTracking().ToList())
        {
            var newItem = new NomenklGroup();
            newItem.LoadFromEntity(item);
            if (!NomenklGroups.ContainsKey(newItem.DocCode))
                NomenklGroups.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_119.AsNoTracking().ToList())
        {
            var newItem = new NomenklType();
            newItem.LoadFromEntity(item);
            if (!NomenklTypes.ContainsKey(newItem.DocCode))
                NomenklTypes.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_50.AsNoTracking().ToList())
        {
            var newItem = new ProductType();
            newItem.LoadFromEntity(item);
            if (!ProductTypes.ContainsKey(newItem.DocCode))
                ProductTypes.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_2.AsNoTracking().ToList())
        {
            var newItem = new Employee();
            newItem.LoadFromEntity(item, this);
            if (!Employees.ContainsKey(newItem.DocCode))
                Employees.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_27.AsNoTracking().ToList())
        {
            var newItem = new Warehouse();
            newItem.LoadFromEntity(item, this);
            if (!Warehouses.ContainsKey(newItem.DocCode))
                Warehouses.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_22.Include(_ => _.TD_22).AsNoTracking().ToList())
        {
            var newItem = new CashBox();
            newItem.LoadFromEntity(item, this);
            if (!CashBoxes.ContainsKey(newItem.DocCode))
                CashBoxes.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_111.AsNoTracking().ToList())
        {
            var newItem = new MutualSettlementType();
            newItem.LoadFromEntity(item);
            if (!MutualSettlementTypes.ContainsKey(newItem.DocCode))
                MutualSettlementTypes.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.Projects.AsNoTracking().ToList())
        {
            var newItem = new Project();
            newItem.LoadFromEntity(item, this);
            if (!Projects.ContainsKey(newItem.Id))
                Projects.Add(newItem.Id, newItem);
        }

        foreach (var item in Context.SD_102.AsNoTracking().ToList())
        {
            var newItem = new ContractType();
            newItem.LoadFromEntity(item);
            if (!ContractTypes.ContainsKey(newItem.DocCode))
                ContractTypes.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_44.AsNoTracking().ToList())
        {
            var newItem = new Bank();
            newItem.LoadFromEntity(item);
            if (!ContractTypes.ContainsKey(newItem.DocCode))
                Banks.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_114.AsNoTracking().ToList())
        {
            var newItem = new BankAccount();
            newItem.LoadFromEntity(item, this);
            if (!ContractTypes.ContainsKey(newItem.DocCode))
                BankAccounts.Add(newItem.DocCode, newItem);
        }

        foreach (var item in Context.SD_43.AsNoTracking().ToList().ToList())
        {
            var newItem = new Kontragent();
            newItem.LoadFromEntity(item, this);
            if (!Kontragents.ContainsKey(newItem.DocCode))
                Kontragents.Add(item.DOC_CODE, newItem);
        }

        foreach (var item in Context.NomenklMain.AsNoTracking().ToList().ToList())
        {
            var newItem = new NomenklMain();
            newItem.LoadFromEntity(item, this);
            if (!NomenklMains.ContainsKey(newItem.Id))
                NomenklMains.Add(item.Id, newItem);
        }

        foreach (var item in Context.SD_83.AsNoTracking().ToList().ToList())
        {
            var newItem = new Nomenkl();
            newItem.LoadFromEntity(item, this);
            if (!Nomenkls.ContainsKey(newItem.DocCode))
                Nomenkls.Add(item.DOC_CODE, newItem);
        }

        IsChangeTrackingOn = true;
    }

    private void Clear()
    {
        Currencies.Clear();
        Countries.Clear();
        Regions.Clear();
        Units.Clear();
        CentrResponsibilities.Clear();
        SDRStates.Clear();
        SDRSchets.Clear();
        DeliveryConditions.Clear();
        NomenklTypes.Clear();
        ProductTypes.Clear();
        PayForms.Clear();
        PayConditions.Clear();

        ClientCategories.Clear();
        KontragentGroups.Clear();
        NomenklGroups.Clear();
        Employees.Clear();

        Warehouses.Clear();
        CashBoxes.Clear();
        MutualSettlementTypes.Clear();
        Projects.Clear();
        ContractTypes.Clear();
        Banks.Clear();
        BankAccounts.Clear();

        Kontragents.Clear();
        Nomenkls.Clear();
        NomenklProductTypes.Clear();
    }

    #endregion

    #region Аакты взаимозачета

    private void UpdateCacheMutualSettlementType()
    {
        var changed = GetChangeData("SD_111", MutualSettlementTypesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (MutualSettlementTypes.ContainsKey(ch.DocCode)) MutualSettlementTypes.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_111.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new MutualSettlementType();
                                newItem.LoadFromEntity(item);
                                MutualSettlementTypes.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (MutualSettlementTypes.ContainsKey(ch.DocCode))
                                    ((MutualSettlementType) MutualSettlementTypes[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        MutualSettlementTypesTrackingId = GetCurrentChangeTrackingId();
    }

    public IMutualSettlementType GetMutualSettlementType(decimal? dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheMutualSettlementType();
        if (dc == null) return null;
        if (MutualSettlementTypes.ContainsKey(dc.Value))
            return MutualSettlementTypes[dc.Value];
        var data = Context.SD_111.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new MutualSettlementType();
        newItem.LoadFromEntity(data);
        MutualSettlementTypes.Add(data.DOC_CODE, newItem);
        return MutualSettlementTypes[data.DOC_CODE];
    }

    public IEnumerable<IMutualSettlementType> GetMutualSettlementTypeAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheMutualSettlementType();
        return MutualSettlementTypes.Values.ToList();
    }

    #endregion

    #region Счета и статьи расходов и доходов

    private void UpdateCacheSDRSchet()
    {
        var changed = GetChangeData("SD_303", SDRSchetsTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (SDRSchets.ContainsKey(ch.DocCode)) SDRSchets.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_303.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new SDRSchet();
                                newItem.LoadFromEntity(item, this);
                                SDRSchets.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (SDRSchets.ContainsKey(ch.DocCode))
                                    ((SDRSchet) SDRSchets[ch.DocCode]).LoadFromEntity(item, this);
                            }
                        }

                        break;
                }

        SDRSchetsTrackingId = GetCurrentChangeTrackingId();
    }

    private void UpdateCacheSDRSTate()
    {
        var changed = GetChangeData("SD_99", SDRStatesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (SDRStates.ContainsKey(ch.DocCode)) SDRStates.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_99.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new SDRState();
                                newItem.LoadFromEntity(item);
                                SDRStates.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (SDRStates.ContainsKey(ch.DocCode))
                                    ((SDRState) SDRStates[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        SDRStatesTrackingId = GetCurrentChangeTrackingId();
    }

    public ISDRSchet GetSDRSchet(decimal? dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheSDRSchet();
        if (dc == null) return null;
        if (SDRSchets.ContainsKey(dc.Value))
            return SDRSchets[dc.Value];
        var data = Context.SD_303.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new SDRSchet();
        newItem.LoadFromEntity(data, this);
        SDRSchets.Add(data.DOC_CODE, newItem);
        return SDRSchets[data.DOC_CODE];
    }

    public ISDRSchet GetSDRSchet(Guid? id)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheSDRSchet();
        throw new NotImplementedException();
    }

    public IEnumerable<ISDRSchet> GetSDRSchetAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheSDRSchet();
        return SDRSchets.Values.ToList();
    }

    public ISDRState GetSDRState(decimal? dc)
    {
        if (dc == null) return null;
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheSDRSTate();
        if (SDRStates.ContainsKey(dc.Value))
            return SDRStates[dc.Value];
        var data = Context.SD_99.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new SDRState();
        newItem.LoadFromEntity(data);
        SDRStates.Add(data.DOC_CODE, newItem);
        return SDRStates[data.DOC_CODE];
    }

    public ISDRState GetSDRState(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISDRState> GetSDRStateAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheSDRSTate();
        return SDRStates.Values.ToList();
    }

    #endregion

    #region Категория клиентов

    private void UpdateCacheClientCategory()
    {
        var changed = GetChangeData("SD_148", ClientCategoriesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (ClientCategories.ContainsKey(ch.DocCode)) ClientCategories.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_148.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new ClientCategory();
                                newItem.LoadFromEntity(item);
                                ClientCategories.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (ClientCategories.ContainsKey(ch.DocCode))
                                    ((ClientCategory) ClientCategories[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        ClientCategoriesTrackingId = GetCurrentChangeTrackingId();
    }

    public IClientCategory GetClientCategory(decimal? dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheClientCategory();
        if (dc == null) return null;
        if (ClientCategories.ContainsKey(dc.Value))
            return ClientCategories[dc.Value];
        var data = Context.SD_148.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new ClientCategory();
        newItem.LoadFromEntity(data);
        ClientCategories.Add(data.DOC_CODE, newItem);
        return ClientCategories[data.DOC_CODE];
    }

    public IClientCategory GetClientCategory(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IClientCategory> GetClientCategoriesAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheClientCategory();
        return ClientCategories.Values.ToList();
    }

    #endregion

    #region Валюта

    private void UpdateCacheCurrency()
    {
        var changed = GetChangeData("SD_301", CurrenciesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (Currencies.ContainsKey(ch.DocCode)) Currencies.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_301.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new Currency();
                                newItem.LoadFromEntity(item);
                                Currencies.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (Currencies.ContainsKey(ch.DocCode))
                                    ((Currency) Currencies[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        CurrenciesTrackingId = GetCurrentChangeTrackingId();
    }

    public ICurrency GetCurrency(decimal? dc)
    {
        if (dc == null) return null;
        return GetCurrency(dc.Value);
    }

    public ICurrency GetCurrency(decimal dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheCurrency();
        if (Currencies.ContainsKey(dc))
            return Currencies[dc];
        var data = Context.SD_301.FirstOrDefault(_ => _.DOC_CODE == dc);
        if (data == null) return null;
        var newItem = new Currency();
        newItem.LoadFromEntity(data);
        Currencies.Add(data.DOC_CODE, newItem);
        return Currencies[data.DOC_CODE];
    }

    public IEnumerable<ICurrency> GetCurrenciesAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheCurrency();
        return Currencies.Values.ToList();
    }

    #endregion

    #region Страны

    public ICountry GetCountry(Guid? id)
    {
        if (id == null) return null;
        if (Countries.ContainsKey(id.Value))
            return Countries[id.Value];
        var data = Context.Countries.FirstOrDefault(_ => _.Id == id.Value);
        if (data == null) return null;
        var newItem = new Country();
        newItem.LoadFromEntity(data);
        Countries.Add(data.Id, newItem);
        return Countries[data.Id];
    }

    public IEnumerable<ICountry> GetCountriesAll()
    {
        return Countries.Values.ToList();
    }

    #endregion

    #region Регионы

    private void UpdateCacheRegion()
    {
        var changed = GetChangeData("SD_23", RegionsTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (Regions.ContainsKey(ch.DocCode)) Regions.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_23.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new Region();
                                newItem.LoadFromEntity(item);
                                Regions.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (Regions.ContainsKey(ch.DocCode))
                                    ((Region) Regions[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        RegionsTrackingId = GetCurrentChangeTrackingId();
    }

    public IRegion GetRegion(decimal? dc)
    {
        if (dc == null) return null;
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheRegion();
        if (Regions.ContainsKey(dc.Value))
            return Regions[dc.Value];
        var data = Context.SD_23.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Region();
        newItem.LoadFromEntity(data);
        Regions.Add(data.DOC_CODE, newItem);
        return Regions[data.DOC_CODE];
    }

    public IRegion GetRegion(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IRegion> GetRegionsAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheRegion();
        return Regions.Values.ToList();
    }

    #endregion

    #region Единицы измерения

    private void UpdateCacheUnit()
    {
        var changed = GetChangeData("SD_175", UnitsTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (Units.ContainsKey(ch.DocCode)) Units.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_175.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new Unit();
                                newItem.LoadFromEntity(item);
                                Units.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (Units.ContainsKey(ch.DocCode))
                                    ((Unit) Units[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        UnitsTrackingId = GetCurrentChangeTrackingId();
    }

    public IUnit GetUnit(decimal? dc)
    {
        if (dc == null) return null;
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheUnit();
        if (Units.ContainsKey(dc.Value))
            return Units[dc.Value];
        var data = Context.SD_175.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Unit();
        newItem.LoadFromEntity(data);
        Units.Add(data.DOC_CODE, newItem);
        return Units[data.DOC_CODE];
    }

    public IUnit GetUnit(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IUnit> GetUnitsAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheUnit();
        return Units.Values.ToList();
    }

    #endregion

    #region Kontragent

    private void UpdateCacheKontragents()
    {
        var changed = GetChangeData("SD_43", KontragentsTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (Kontragents.ContainsKey(ch.DocCode)) Kontragents.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_43.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new Kontragent();
                                newItem.LoadFromEntity(item, this);
                                Kontragents.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (Kontragents.ContainsKey(ch.DocCode))
                                    ((Kontragent) Kontragents[ch.DocCode]).LoadFromEntity(item, this);
                            }
                        }

                        break;
                }

        KontragentsTrackingId = GetCurrentChangeTrackingId();
    }

    private void UpdateCacheKontragentGroup()
    {
        var changed = GetChangeDataInt("UD_43", KontragentGroupsTrackingId, "EG_ID");
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (KontragentGroups.ContainsKey(ch.Code)) KontragentGroups.Remove(ch.Code);
                        break;
                    case "I":
                    case "U":
                        var item = Context.UD_43.Find(ch.Code);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new KontragentGroup();
                                newItem.LoadFromEntity(item);
                                KontragentGroups.Add(newItem.Id, newItem);
                            }
                            else
                            {
                                if (KontragentGroups.ContainsKey(ch.Code))
                                    ((KontragentGroup) KontragentGroups[ch.Code]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        KontragentGroupsTrackingId = GetCurrentChangeTrackingId();
    }

    public IKontragentGroup GetKontragentGroup(int? id)
    {
        if (id == null) return null;
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheKontragentGroup();
        if (KontragentGroups.ContainsKey(id.Value))
            return KontragentGroups[id.Value];
        var data = Context.UD_43.FirstOrDefault(_ => _.EG_ID == id.Value);
        if (data == null) return null;
        var newItem = new KontragentGroup();
        newItem.LoadFromEntity(data);
        KontragentGroups.Add(data.EG_ID, newItem);
        return KontragentGroups[data.EG_ID];
    }

    public IEnumerable<IKontragentGroup> GetKontragentCategoriesAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheKontragentGroup();
        return KontragentGroups.Values.ToList();
    }

    public IKontragent GetKontragent(decimal? dc)
    {
        return dc == null ? null : GetKontragent(dc.Value);
    }

    public IKontragent GetKontragent(decimal dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheKontragents();
        if (Kontragents.ContainsKey(dc))
            return Kontragents[dc];
        var data = Context.SD_43.FirstOrDefault(_ => _.DOC_CODE == dc);
        if (data == null) return null;
        var newItem = new Kontragent();
        newItem.LoadFromEntity(data, this);
        Kontragents.Add(data.DOC_CODE, newItem);
        return Kontragents[data.DOC_CODE];
    }

    public IKontragent GetKontragent(Guid? id)
    {
        if (id == null) return null;
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheKontragents();
        var k = Kontragents.Values.Cast<Kontragent>().FirstOrDefault(_ => _.Id == id.Value);
        if (k != null)
            return k;
        var data = Context.SD_43.FirstOrDefault(_ => _.Id == id.Value);
        if (data == null) return null;
        var newItem = new Kontragent();
        newItem.LoadFromEntity(data, this);
        Kontragents.Add(data.DOC_CODE, newItem);
        return Kontragents[data.DOC_CODE];
    }

    public IEnumerable<IKontragent> GetKontragentsAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheKontragents();
        return Kontragents.Values.ToList();
    }

    #endregion

    #region Warehouse

    private void UpdateCacheWarehouse()
    {
        var changed = GetChangeData("SD_27", WarehousesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (Warehouses.ContainsKey(ch.DocCode)) Warehouses.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_27.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new Warehouse();
                                newItem.LoadFromEntity(item, this);
                                Warehouses.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (Warehouses.ContainsKey(ch.DocCode))
                                    ((Warehouse) Warehouses[ch.DocCode]).LoadFromEntity(item, this);
                            }
                        }

                        break;
                }

        WarehousesTrackingId = GetCurrentChangeTrackingId();
    }

    public IWarehouse GetWarehouse(decimal? dc)
    {
        return dc == null ? null : GetWarehouse(dc.Value);
    }

    public IWarehouse GetWarehouse(decimal dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheWarehouse();
        if (Warehouses.ContainsKey(dc))
            return Warehouses[dc];
        var data = Context.SD_27.FirstOrDefault(_ => _.DOC_CODE == dc);
        if (data == null) return null;
        var newItem = new Warehouse();
        newItem.LoadFromEntity(data, this);
        Warehouses.Add(data.DOC_CODE, newItem);
        return Warehouses[data.DOC_CODE];
    }

    public IWarehouse GetWarehouse(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IWarehouse> GetWarehousesAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheWarehouse();
        return Warehouses.Values.ToList();
    }

    #endregion

    #region Employee

    private void UpdateCacheEmployee()
    {
        var changed = GetChangeDataInt("SD_2", EmployeesTrackingId, "TABELNUMBER").ToList();
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (Employees.ContainsKey(ch.Code)) Employees.Remove(ch.Code);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_2.Find(ch.Code);
                        if (item != null)
                        {
                            if (Employees.ContainsKey(item.DOC_CODE))
                                ((Employee) Employees[item.DOC_CODE]).LoadFromEntity(item, this);
                            else
                            {
                                var newItem = new Employee();
                                newItem.LoadFromEntity(item, this);
                                Employees.Add(newItem.DocCode, newItem);
                            }
                        }
                        break;
                }

        EmployeesTrackingId = GetCurrentChangeTrackingId();
    }

    public IEmployee GetEmployee(int? tabelNumber)
    {
        if (tabelNumber == null) return null;
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheEmployee();
        var emp = Employees.Values.FirstOrDefault(_m => _m.TabelNumber == tabelNumber.Value);
        if (emp != null) return emp;
        var data = Context.SD_2.FirstOrDefault(_ => _.TABELNUMBER == tabelNumber.Value);
        if (data == null) return null;
        var newItem = new Employee();
        newItem.LoadFromEntity(data, this);
        Employees.Add(data.DOC_CODE, newItem);
        return Employees[data.DOC_CODE];
    }

    public IEmployee GetEmployee(decimal? dc)
    {
        if (dc == null) return null;
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheEmployee();
        if (Employees.ContainsKey(dc.Value))
            return Employees[dc.Value];
        var data = Context.SD_2.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Employee();
        newItem.LoadFromEntity(data, this);
        Employees.Add(data.DOC_CODE, newItem);
        return Employees[data.DOC_CODE];
    }

    public IEmployee GetEmployee(Guid? id)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IEmployee> GetEmployees()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheEmployee();
        return Employees.Values.ToList();
    }

    #endregion

    #region Nomenkl

    private void UpdateCacheNomenkl()
    {
        var changed = GetChangeData("SD_83", NomenklsTrackingId).ToList();
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (Nomenkls.ContainsKey(ch.DocCode)) Nomenkls.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_83.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I" && !Nomenkls.ContainsKey(ch.DocCode))
                            {
                                var newItem = new Nomenkl();
                                newItem.LoadFromEntity(item, this);
                                Nomenkls.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (Nomenkls.ContainsKey(ch.DocCode))
                                    ((Nomenkl) Nomenkls[ch.DocCode]).LoadFromEntity(item, this);
                            }
                        }

                        break;
                }

        NomenklsTrackingId = GetCurrentChangeTrackingId();
    }

    public INomenkl GetNomenkl(Guid id)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheNomenkl();
        var k = Nomenkls.Values.Cast<Nomenkl>().FirstOrDefault(_ => _.Id == id);
        if (k != null)
            return k;
        var data = Context.SD_83.FirstOrDefault(_ => _.Id == id);
        if (data == null) return null;
        var newItem = new Nomenkl();
        newItem.LoadFromEntity(data, this);
        Nomenkls.Add(data.DOC_CODE, newItem);
        return Nomenkls[data.DOC_CODE];
    }

    public INomenkl GetNomenkl(Guid? id)
    {
        if (id == null) return null;
        return GetNomenkl(id.Value);
    }

    public INomenkl GetNomenkl(decimal? dc)
    {
        if (dc == null) return null;
        return GetNomenkl(dc.Value);
    }

    public INomenkl GetNomenkl(decimal dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheNomenkl();
        if (Nomenkls.ContainsKey(dc))
            return Nomenkls[dc];
        var data = Context.SD_83.FirstOrDefault(_ => _.DOC_CODE == dc);
        if (data == null) return null;
        var newItem = new Nomenkl();
        newItem.LoadFromEntity(data, this);
        Nomenkls.Add(data.DOC_CODE, newItem);
        return Nomenkls[data.DOC_CODE];
    }

    public IEnumerable<INomenkl> GetNomenklsAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheNomenkl();
        return Nomenkls.Values.ToList();
    }

    public INomenklMain GetNomenklMain(Guid? id)
    {
        if (id == null) return null;
        return GetNomenklMain(id.Value);
    }

    public INomenklMain GetNomenklMain(Guid id)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheNomenklMain();
        if (NomenklMains.ContainsKey(id))
            return NomenklMains[id];
        var data = Context.NomenklMain.FirstOrDefault(_ => _.Id == id);
        if (data == null) return null;
        var newItem = new NomenklMain();
        newItem.LoadFromEntity(data, this);
        NomenklMains.Add(data.Id, newItem);
        return NomenklMains[data.Id];
    }

    private void UpdateCacheNomenklMain()
    {
        var changed = GetChangeDataGuid("NomenklMain", NomenklMainTrackingId, "Id").ToList();
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (NomenklMains.ContainsKey(ch.Id)) NomenklMains.Remove(ch.Id);
                        break;
                    case "I":
                    case "U":
                        var item = Context.NomenklMain.Find(ch.Id);
                        if (item != null)
                        { 
                            if (NomenklMains.ContainsKey(ch.Id))
                                    ((NomenklMain) NomenklMains[ch.Id]).LoadFromEntity(item, this);
                            else 
                            {
                                var newItem = new NomenklMain();
                                newItem.LoadFromEntity(item, this);
                                NomenklMains.Add(newItem.Id, newItem);
                            }
                        }

                        break;
                }

        NomenklsTrackingId = GetCurrentChangeTrackingId();
    }

    public IEnumerable<INomenklMain> GetNomenklMainAll()
    {
        throw new NotImplementedException();
    }

    private void UpdateCacheNomenklGroup()
    {
        var changed = GetChangeData("SD_82", NomenklGroupsTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (NomenklGroups.ContainsKey(ch.DocCode)) NomenklGroups.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_82.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new NomenklGroup();
                                newItem.LoadFromEntity(item);
                                NomenklGroups.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (NomenklGroups.ContainsKey(ch.DocCode))
                                    ((NomenklGroup) NomenklGroups[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        NomenklGroupsTrackingId = GetCurrentChangeTrackingId();
    }

    public INomenklGroup GetNomenklGroup(decimal? dc)
    {
        if (dc == null) return null;
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheNomenklGroup();
        if (NomenklGroups.ContainsKey(dc.Value))
            return NomenklGroups[dc.Value];
        var data = Context.SD_82.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new NomenklGroup();
        newItem.LoadFromEntity(data);
        NomenklGroups.Add(data.DOC_CODE, newItem);
        return NomenklGroups[data.DOC_CODE];
    }

    public IEnumerable<INomenklGroup> GetNomenklGroupAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheNomenklGroup();
        return NomenklGroups.Values.ToList();
    }

    #endregion

    #region Bank && CashBox

    public ICashBox GetCashBox(decimal? dc)
    {
        if (dc == null) return null;
        return GetCashBox(dc.Value);
    }

    private void UpdateCacheCashBox()
    {
        var changed = GetChangeData("SD_22", CashBoxesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (CashBoxes.ContainsKey(ch.DocCode)) CashBoxes.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_22.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newCB = new CashBox();
                                newCB.LoadFromEntity(item, this);
                                CashBoxes.Add(newCB.DocCode, newCB);
                            }
                            else
                            {
                                if (CashBoxes.ContainsKey(ch.DocCode))
                                    ((CashBox) CashBoxes[ch.DocCode]).LoadFromEntity(item, this);
                            }
                        }

                        break;
                }

        CashBoxesTrackingId = GetCurrentChangeTrackingId();
    }

    private void UpdateCacheBank()
    {
        var changed = GetChangeData("SD_44", BanksTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (Banks.ContainsKey(ch.DocCode)) Banks.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_44.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newitem = new Bank();
                                newitem.LoadFromEntity(item);
                                Banks.Add(newitem.DocCode, newitem);
                            }
                            else
                            {
                                if (Banks.ContainsKey(ch.DocCode))
                                    ((Bank) Banks[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        CashBoxesTrackingId = GetCurrentChangeTrackingId();
    }

    public ICashBox GetCashBox(decimal dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheCashBox();
        if (CashBoxes.ContainsKey(dc))
            return CashBoxes[dc];
        var data = Context.SD_22.Include(_ => _.TD_22).FirstOrDefault(_ => _.DOC_CODE == dc);
        if (data == null) return null;
        var newItem = new CashBox();
        newItem.LoadFromEntity(data, this);
        CashBoxes.Add(data.DOC_CODE, newItem);
        return CashBoxes[data.DOC_CODE];
    }

    public IEnumerable<ICashBox> GetCashBoxAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheCashBox();
        return CashBoxes.Values.ToList();
    }

    public IBank GetBank(decimal? dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheBank();
        if (dc == null) return null;
        if (Banks.ContainsKey(dc.Value))
            return Banks[dc.Value];
        var data = Context.SD_44.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new Bank();
        newItem.LoadFromEntity(data);
        Banks.Add(data.DOC_CODE, newItem);
        return Banks[data.DOC_CODE];
    }

    public IEnumerable<IBank> GetBanksAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheBank();
        return Banks.Values.ToList();
    }

    public IBankAccount GetBankAccount(decimal? dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheBankAccount();
        if (dc == null) return null;
        if (BankAccounts.ContainsKey(dc.Value))
            return BankAccounts[dc.Value];
        var data = Context.SD_114.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new BankAccount();
        newItem.LoadFromEntity(data, this);
        BankAccounts.Add(data.DOC_CODE, newItem);
        return BankAccounts[data.DOC_CODE];
    }

    public IEnumerable<IBankAccount> GetBankAccountAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheBankAccount();
        return BankAccounts.Values.ToList();
    }

    private void UpdateCacheBankAccount()
    {
        var changed = GetChangeData("SD_114", CashBoxesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (BankAccounts.ContainsKey(ch.DocCode)) BankAccounts.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_114.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newitem = new BankAccount();
                                newitem.LoadFromEntity(item, this);
                                BankAccounts.Add(newitem.DocCode, newitem);
                            }
                            else
                            {
                                if (BankAccounts.ContainsKey(ch.DocCode))
                                    ((BankAccount) BankAccounts[ch.DocCode]).LoadFromEntity(item, this);
                            }
                        }

                        break;
                }

        CashBoxesTrackingId = GetCurrentChangeTrackingId();
    }

    #endregion

    #region Тип продукции

    private void UpdateCacheProductType()
    {
        var changed = GetChangeData("SD_50", ProductTypesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (ProductTypes.ContainsKey(ch.DocCode)) ProductTypes.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_50.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new ProductType();
                                newItem.LoadFromEntity(item);
                                ProductTypes.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (ProductTypes.ContainsKey(ch.DocCode))
                                    ((ProductType) ProductTypes[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        ProductTypesTrackingId = GetCurrentChangeTrackingId();
    }

    public IProductType GetProductType(decimal dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheProductType();
        if (ProductTypes.ContainsKey(dc))
            return ProductTypes[dc];
        var data = Context.SD_50.FirstOrDefault(_ => _.DOC_CODE == dc);
        if (data == null) return null;
        var newItem = new ProductType();
        newItem.LoadFromEntity(data);
        ProductTypes.Add(data.DOC_CODE, newItem);
        return ProductTypes[data.DOC_CODE];
    }

    public IProductType GetProductType(decimal? dc)
    {
        return dc == null ? null : GetProductType(dc.Value);
    }

    public IEnumerable<IProductType> GetProductTypeAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheProductType();
        return ProductTypes.Values.ToList();
    }

    #endregion

    #region Тип товара

    private void UpdateCacheNomenklType()
    {
        var changed = GetChangeData("SD_119", NomenklTypesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (NomenklTypes.ContainsKey(ch.DocCode)) NomenklTypes.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_119.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new NomenklType();
                                newItem.LoadFromEntity(item);
                                NomenklTypes.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (NomenklTypes.ContainsKey(ch.DocCode))
                                    ((NomenklType) NomenklTypes[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        NomenklTypesTrackingId = GetCurrentChangeTrackingId();
    }

    public INomenklType GetNomenklType(decimal? dc)
    {
        if (dc == null) return null;
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheNomenklType();
        if (NomenklTypes.ContainsKey(dc.Value))
            return NomenklTypes[dc.Value];
        var data = Context.SD_119.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new NomenklType();
        newItem.LoadFromEntity(data);
        NomenklTypes.Add(data.DOC_CODE, newItem);
        return NomenklTypes[data.DOC_CODE];
    }

    public IEnumerable<INomenklType> GetNomenklTypeAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId())
            UpdateCacheNomenklType();
        return NomenklTypes.Values.ToList();
    }

    #endregion

    #region Центр ответственности

    private void UpdateCacheCentrResponsibility()
    {
        var changed = GetChangeData("SD_40", CentrResponsibilitiesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (CentrResponsibilities.ContainsKey(ch.DocCode)) CentrResponsibilities.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_40.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new CentrResponsibility();
                                newItem.LoadFromEntity(item);
                                CentrResponsibilities.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (CentrResponsibilities.ContainsKey(ch.DocCode))
                                    ((CentrResponsibility) CentrResponsibilities[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        CentrResponsibilitiesTrackingId = GetCurrentChangeTrackingId();
    }


    public ICentrResponsibility GetCentrResponsibility(decimal? dc)
    {
        if (dc == null) return null;
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheCentrResponsibility();
        if (CentrResponsibilities.ContainsKey(dc.Value))
            return CentrResponsibilities[dc.Value];
        var data = Context.SD_40.FirstOrDefault(_ => _.DOC_CODE == dc.Value);
        if (data == null) return null;
        var newItem = new CentrResponsibility();
        newItem.LoadFromEntity(data);
        CentrResponsibilities.Add(data.DOC_CODE, newItem);
        return CentrResponsibilities[data.DOC_CODE];
    }

    public IEnumerable<ICentrResponsibility> GetCentrResponsibilitiesAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheCentrResponsibility();
        return CentrResponsibilities.Values.ToList();
    }

    #endregion

    #region Условия поставки sd_103

    private void UpdateCacheDeliveryCondition()
    {
        var changed = GetChangeData("SD_103", DeliveryConditionsTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (DeliveryConditions.ContainsKey(ch.DocCode)) DeliveryConditions.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_103.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (ch.Operation == "I")
                            {
                                var newItem = new DeliveryCondition();
                                newItem.LoadFromEntity(item);
                                DeliveryConditions.Add(newItem.DocCode, newItem);
                            }
                            else
                            {
                                if (DeliveryConditions.ContainsKey(ch.DocCode))
                                    ((DeliveryCondition) DeliveryConditions[ch.DocCode]).LoadFromEntity(item);
                            }
                        }

                        break;
                }

        DeliveryConditionsTrackingId = GetCurrentChangeTrackingId();
    }

    public IDeliveryCondition GetDeliveryCondition(decimal dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheDeliveryCondition();
        if (DeliveryConditions.ContainsKey(dc))
            return DeliveryConditions[dc];
        var data = Context.SD_103.FirstOrDefault(_ => _.DOC_CODE == dc);
        if (data == null) return null;
        var newItem = new DeliveryCondition();
        newItem.LoadFromEntity(data);
        DeliveryConditions.Add(data.DOC_CODE, newItem);
        return DeliveryConditions[data.DOC_CODE];
    }

    public IDeliveryCondition GetDeliveryCondition(decimal? dc)
    {
        if (dc == null) return null;
        return GetDeliveryCondition(dc.Value);
    }

    public IEnumerable<IDeliveryCondition> GetDeliveryConditionAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheDeliveryCondition();
        return DeliveryConditions.Values.ToList();
    }

    #endregion

    #region Тип продукции для счетов SD_77

    private void UpdateCacheNomenklProductType()
    {
        var changed = GetChangeData("SD_77", NomenklProductTypesTrackingId);
        if (changed.Any())
            foreach (var ch in changed)
                switch (ch.Operation)
                {
                    case "D":
                        if (NomenklProductTypes.ContainsKey(ch.DocCode)) NomenklProductTypes.Remove(ch.DocCode);
                        break;
                    case "I":
                    case "U":
                        var item = Context.SD_77.Find(ch.DocCode);
                        if (item != null)
                        {
                            if (NomenklProductTypes.ContainsKey(ch.DocCode))
                                ((NomenklProductType) NomenklProductTypes[ch.DocCode]).LoadFromEntity(item, this);
                            else
                            {
                                var newItem = new NomenklProductType();
                                newItem.LoadFromEntity(item, this);
                                NomenklProductTypes.Add(newItem.DocCode, newItem);
                            }
                        }

                        break;
                }

        NomenklProductTypesTrackingId = GetCurrentChangeTrackingId();
    }

    public INomenklProductType GetNomenklProductType(decimal? dc)
    {
        if (dc == null) return null;
        return GetNomenklProductType(dc.Value);
    }

    public INomenklProductType GetNomenklProductType(decimal dc)
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheNomenklProductType();
        if (NomenklProductTypes.ContainsKey(dc))
            return NomenklProductTypes[dc];
        var data = Context.SD_77.FirstOrDefault(_ => _.DOC_CODE == dc);
        if (data == null) return null;
        var newItem = new NomenklProductType();
        newItem.LoadFromEntity(data, this);
        NomenklProductTypes.Add(data.DOC_CODE, newItem);
        return NomenklProductTypes[data.DOC_CODE];
    }

    public IEnumerable<INomenklProductType> GetNomenklProductTypesAll()
    {
        if ((DateTime.Now - lastTimeCheckTrackerId).TotalSeconds > diffSecondsForCheckTracker && IsChangeTrackingOn &&
            NomenklsTrackingId != GetCurrentChangeTrackingId()) UpdateCacheNomenklProductType();
        return NomenklProductTypes.Values.ToList();
    }

    #endregion

    #region Dictionaries

    private readonly Dictionary<decimal, INomenklProductType> NomenklProductTypes
        = new Dictionary<decimal, INomenklProductType>();

    private readonly Dictionary<int, IKontragentGroup> KontragentGroups =
        new Dictionary<int, IKontragentGroup>();

    private readonly Dictionary<decimal, IDeliveryCondition> DeliveryConditions =
        new Dictionary<decimal, IDeliveryCondition>();

    private readonly Dictionary<decimal, IKontragent> Kontragents = new Dictionary<decimal, IKontragent>();
    private readonly Dictionary<decimal, INomenkl> Nomenkls = new Dictionary<decimal, INomenkl>();
    private readonly Dictionary<Guid, INomenklMain> NomenklMains = new Dictionary<Guid, INomenklMain>();

    private readonly Dictionary<decimal, INomenklGroup> NomenklGroups =
        new Dictionary<decimal, INomenklGroup>();

    private readonly Dictionary<decimal, IWarehouse> Warehouses = new Dictionary<decimal, IWarehouse>();
    private readonly Dictionary<decimal, IEmployee> Employees = new Dictionary<decimal, IEmployee>();
    private readonly Dictionary<decimal, IBank> Banks = new Dictionary<decimal, IBank>();
    private readonly Dictionary<decimal, IBankAccount> BankAccounts = new Dictionary<decimal, IBankAccount>();

    private readonly Dictionary<decimal, ICentrResponsibility> CentrResponsibilities =
        new Dictionary<decimal, ICentrResponsibility>();

    private readonly Dictionary<decimal, ISDRSchet> SDRSchets = new Dictionary<decimal, ISDRSchet>();
    private readonly Dictionary<decimal, ISDRState> SDRStates = new Dictionary<decimal, ISDRState>();
    private readonly Dictionary<decimal, IClientCategory> ClientCategories = new Dictionary<decimal, IClientCategory>();
    private readonly Dictionary<decimal, ICurrency> Currencies = new Dictionary<decimal, ICurrency>();
    private readonly Dictionary<decimal, IRegion> Regions = new Dictionary<decimal, IRegion>();
    private readonly Dictionary<decimal, IUnit> Units = new Dictionary<decimal, IUnit>();
    private readonly Dictionary<decimal, ICashBox> CashBoxes = new Dictionary<decimal, ICashBox>();

    private readonly Dictionary<decimal, IMutualSettlementType> MutualSettlementTypes =
        new Dictionary<decimal, IMutualSettlementType>();

    private readonly Dictionary<Guid, ICountry> Countries = new Dictionary<Guid, ICountry>();
    private readonly Dictionary<Guid, IProject> Projects = new Dictionary<Guid, IProject>();
    private readonly Dictionary<decimal, IContractType> ContractTypes = new Dictionary<decimal, IContractType>();
    private readonly Dictionary<decimal, INomenklType> NomenklTypes = new Dictionary<decimal, INomenklType>();
    private readonly Dictionary<decimal, IProductType> ProductTypes = new Dictionary<decimal, IProductType>();
    private readonly Dictionary<decimal, IPayForm> PayForms = new Dictionary<decimal, IPayForm>();
    private readonly Dictionary<decimal, IPayCondition> PayConditions = new Dictionary<decimal, IPayCondition>();

    #endregion
}
