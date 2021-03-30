using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using Core;
using Core.EntityViewModel;
using Core.WindowsManager;
using Data;
using Data.Repository;
using Helper;
using KursAM2.Managers;

namespace JSONGenerate
{
    internal class Program
    {


        private static void Main(string[] args)
        {
            ALFAMEDIAEntities entities;
            UnitOfWork<ALFAMEDIAEntities> UnitOfWork = new UnitOfWork<ALFAMEDIAEntities>();

            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = "127.0.0.1",
                InitialCatalog = "AlfaTest",
                UserID = "sysadm",
                Password = "19655691"
            }.ConnectionString;
            GlobalOptions.SqlConnectionString = sqlConnectionString;
            GlobalOptions.UserInfo = new User
            {
                Name = "sysadm",
                NickName = "sysadm"
            };
            GlobalOptions.MainReferences = new MainReferences();
            GlobalOptions.MainReferences.Reset();
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

            GlobalOptions.SystemProfile = new SystemProfile
            {
                NationalCurrency = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR"),
                MainCurrency = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR")
            };
            entities = GlobalOptions.GetEntities();
            UnitOfWork = new UnitOfWork<ALFAMEDIAEntities>(entities);
            var manager = new WarehouseManager(new StandartErrorManager(GlobalOptions.GetEntities(),"Test"));
            var order = manager.GetOrderIn(10240036770);
            var s = JsonSerializer.Serialize<WarehouseOrderIn>(order);
            Console.Write(s);
            Console.ReadLine();

        }
    }
}