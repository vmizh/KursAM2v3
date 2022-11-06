using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Calculates.Materials;
using Core;
using KursDomain.References;

namespace ServerCalculate
{
    public class MaterialCostCalculate
    {
        public MaterialCostCalculate(string ds, string dbase, string usr, string pwd)
        {
            try
            {
                var sqlConnectionString = new SqlConnectionStringBuilder
                {
                    DataSource = ds, //"172.16.1.1",
                    InitialCatalog = dbase, //"AlfaMedia",
                    UserID = usr, //"sa",
                    Password = pwd //",juk.,bnyfc"
                }.ConnectionString;
                GlobalOptions.SqlConnectionString = sqlConnectionString;
                //if (MainReferences != null && MainReferences.IsReferenceLoadComplete)
                //    return ret; 
                GlobalOptions.SystemProfile = new SystemProfile();
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
                    var kontr = new Kontragent();
                    kontr.LoadFromEntity(ownKontr, GlobalOptions.ReferencesCache);
                    GlobalOptions.SystemProfile.OwnerKontragent = kontr;

                }

                var mainCrsDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(
                    _ => _.SECTION == "CURRENCY" && _.ITEM == "MAIN");
                if (mainCrsDC != null)
                {
                    var DC = Convert.ToDecimal(mainCrsDC.ITEM_VALUE);
                    var mainCrs =
                        GlobalOptions.GetEntities().SD_301.Single(_ => _.DOC_CODE == DC);
                    var crs = new Currency();
                    crs.LoadFromEntity(mainCrs);
                    GlobalOptions.SystemProfile.MainCurrency =crs;
                }

                var nationalCrsDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(
                    _ => _.SECTION == "CURRENCY" && _.ITEM == "ОСНОВНАЯ_В_ГОСУДАРСТВЕ");
                if (nationalCrsDC != null)
                {
                    var DC = Convert.ToDecimal(nationalCrsDC.ITEM_VALUE);
                    var nationalCrs =
                        GlobalOptions.GetEntities().SD_301.Single(_ => _.DOC_CODE == DC);
                    var crs = new Currency();
                    crs.LoadFromEntity(nationalCrs);
                    GlobalOptions.SystemProfile.NationalCurrency = crs;
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

                MainReferences.Reset();
                while (!MainReferences.IsReferenceLoadComplete)
                {
                }
            }
            catch (Exception ex)
            {
                using (var fs = File.Create("Error.txt"))
                {
                    // Add some text to file
                    var title = new UTF8Encoding(true).GetBytes(ex.Message);
                    fs.Write(title, 0, title.Length);
                }
            }
        }

        public void CalcMaterialAll()
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var calc = new NomenklCostMediumSliding(ctx);
                var nomDCs = ctx.NOMENKL_RECALC.Select(_ => _.NOM_DC).Distinct()
                    .ToList();
                //nomDCs.Clear();
                //nomDCs.Add(10830000041);
                var noms = nomDCs.Select(MainReferences.GetNomenkl).Where(newNom => !newNom.IsUsluga).ToList();
                Console.WriteLine("Кол-во = {0}", noms.Count);
                if (!noms.Any()) return;
                foreach (var op in noms.Select(_ => _.DocCode))
                {
                    var ops = calc.GetOperations(op);
                    var s = $"DELETE FROM NOMENKL_RECALC WHERE NOM_DC={op}";
                    ctx.Database.ExecuteSqlCommand(s);
                    Console.WriteLine($"Сохранение для {op}");
                    if (ops != null && ops.Count > 0)
                        calc.Save(ops);
                }
            }
            //NomenklCostCalc.CalcAll();
        }
    }
}
