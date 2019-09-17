using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Text;
using Microsoft.SqlServer.Server;
using ServerCalculate;


// ReSharper disable once CheckNamespace
public partial class StoredProcedures
{
    [SqlProcedure()]
    public static void CalcNomenklCostAndQuantity(SqlString ds, SqlString dbase, SqlString usr, SqlString pwd)
    {
        var clc = new MaterialCostCalculate((string) ds, (string) dbase, (string) usr, (string) pwd);
        try
        {
            clc.CalcMaterialAll();
        }
        catch (Exception ex)
        {
            using (var fs = File.Create("Error.NomCalc.txt"))
            {
                // Add some text to file
                var title = new UTF8Encoding(true).GetBytes(ex.Message);
                fs.Write(title, 0, title.Length);
            }
        }
    }
}
