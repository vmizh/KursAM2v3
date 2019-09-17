using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Linq;
using Calculates.Materials;
using Core;
using Data;

namespace StoreProcedures
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
                GlobalOptions.MainReferences = new MainReferences();
                GlobalOptions.MainReferences.Reset();
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

            var calc = new NomenklCostMediumSliding();
            using (var ctx = new ALFAMEDIAEntities())
            {
                var nomDCs = ctx.NOMENKL_RECALC.Select(_ => _.NOM_DC).Distinct()
                    .ToList();
                var noms = nomDCs.Select(MainReferences.GetNomenkl).Where(newNom => !newNom.IsUsluga).ToList();
                //Console.WriteLine("Кол-во = {0}", noms.Count);
                if (!noms.Any()) return;
                foreach (var op in noms.Select(_ => _.DocCode))
                {
                    var ops = calc.GetOperations(op);
                    calc.Save(ops);
                }
            }
            //NomenklCostCalc.CalcAll();
        }
    }
}