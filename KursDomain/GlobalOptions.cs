using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using Core.EntityViewModel.CommonReferences;
using Data;
using Data.Repository;
using Helper;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Employee;
using KursDomain.IReferences;
using KursDomain.References;
using Brush = System.Windows.Media.Brush;
using Kontragent = KursDomain.Documents.CommonReferences.Kontragent.Kontragent;

namespace Core;

public static class GlobalOptions
{
    private static readonly string HostName;
    public static ALFAMEDIAEntities KursDBContext;
    public static UnitOfWork<ALFAMEDIAEntities> KursDBUnitOfWork;

    public static KursSystemEntities KursSystemDBContext;
    public static UnitOfWork<KursSystemEntities> KursSystemDBUnitOfWork;

    public static IReferencesCache ReferencesCache;

    public static string SqlConnectionString;
    public static string SqlSystemConnectionString;

    static GlobalOptions()
    {
        var section = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
#if DEBUG
        HostName = section.Get("KursSystemDebugHost");
#else
            HostName = section.Get("KursSystemHost");
#endif
    }

    public static User UserInfo { set; get; }

    public static string DataBaseName { set; get; }
    public static Guid DataBaseId { set; get; }
    public static Brush DatabaseColor { set; get; }

    public static SystemProfile SystemProfile { set; get; }

    //public static MainReferences MainReferences { set; get; }
    public static string Version { set; get; }
    public static string VersionType { set; get; }

    public static KursSystemEntities KursSystem()
    {
        SqlSystemConnectionString = new SqlConnectionStringBuilder
        {
            DataSource = HostName,
            InitialCatalog = "KursSystem",
            UserID = "sa",
            Password = "CbvrfFhntvrf65"
        }.ToString();
        return new KursSystemEntities(SqlSystemConnectionString);
    }

    public static ALFAMEDIAEntities GetEntities()
    {
        if (string.IsNullOrEmpty(SqlConnectionString))
            SqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = HostName,
                InitialCatalog = "EcoOndol",
                UserID = "sysadm",
                Password = "19655691"
            }.ToString();
        var ret = new ALFAMEDIAEntities(SqlConnectionString);
        ret.Database.CommandTimeout = 0;
        return ret;
    }

    public static ALFAMEDIAEntities GetTestEntities()
    {
        var ret = new ALFAMEDIAEntities(SqlConnectionString);
        SqlConnectionString = new SqlConnectionStringBuilder
        {
            DataSource = HostName,
            InitialCatalog = "AlfaMedia",
            UserID = "sa",
            Password = ",juk.,bnyfc"
        }.ConnectionString;
        ret.Database.Connection.ConnectionString = SqlConnectionString;
        ret.Database.CommandTimeout = 0;
        if (MainReferences.IsReferenceLoadComplete)
            return ret;
        MainReferences.Reset();
        while (!MainReferences.IsReferenceLoadComplete)
        {
        }

        return ret;
    }

    public static void Update<TEntity>(TEntity entity, DbContext context)
        where TEntity : class
    {
        // Настройки контекста
        context.Database.Log = s => Debug.WriteLine(s);
        context.Entry(entity).State = EntityState.Modified;
        context.SaveChanges();
    }

    public static void Insert<TEntity>(TEntity entity, DbContext context) where TEntity : class
    {
        // Настройки контекста
        context.Database.Log = s => Debug.WriteLine(s);
        context.Entry(entity).State = EntityState.Added;
        context.SaveChanges();
    }

    /// <summary>
    ///     Запись нескольких сущностей в БД
    /// </summary>
    public static void Inserts<TEntity>(IEnumerable<TEntity> entities, DbContext context) where TEntity : class
    {
        // Настройки контекста
        // Отключаем отслеживание и проверку изменений для оптимизации вставки множества полей
        context.Configuration.AutoDetectChangesEnabled = false;
        context.Configuration.ValidateOnSaveEnabled = false;
        context.Database.Log = s => Debug.WriteLine(s);
        foreach (var entity in entities)
            context.Entry(entity).State = EntityState.Added;
        context.SaveChanges();
        context.Configuration.AutoDetectChangesEnabled = true;
        context.Configuration.ValidateOnSaveEnabled = true;
    }
}

/// <summary>
///     глобальные параметры для системы
/// </summary>
public class SystemProfile
{
    /// <summary>
    ///     учетная валюта
    /// </summary>
    public Currency MainCurrency { set; get; }

    /// <summary>
    ///     Государственная валюта
    /// </summary>
    public Currency NationalCurrency { set; get; }

    /// <summary>
    ///     контрагент - владелей тккущей БД
    /// </summary>
    public Kontragent OwnerKontragent { set; get; }

    public NomenklCalcType NomenklCalcType { set; get; }
    public List<PROFILE> Profile { set; get; } = new List<PROFILE>();
    public Currency EmployeeDefaultCurrency { set; get; }
    public EMP_PAYROLL_TYPEViewModel DafaultPayRollType { set; get; }
}

public enum NomenklCalcType
{
    //Накладные расходы в себестоимости товара
    Standart = 0,

    //Накладные расходы отдельно от цены
    NakladSeparately = 1
}
