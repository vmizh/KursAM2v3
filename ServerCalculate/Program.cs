using System;
using System.IO;
using System.Text;

namespace ServerCalculate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //string ds, string dbase, string usr, string pw
            var clc = new MaterialCostCalculate(args[0], args[1], args[2], args[3]);
            //var clc = new MaterialCostCalculate("172.16.0.1", "AlfaNew", "sa", ",juk.,bnyfc");
            //try
            {
                clc.CalcMaterialAll();
            }
            //catch (Exception ex)
            //{
            //    using (var fs = File.Create("Error.NomCalc.txt"))
            //    {
            //        // Add some text to file
            //        var title = new UTF8Encoding(true).GetBytes(ex.Message);
            //        fs.Write(title, 0, title.Length);
            //    }
            //}
        }
    }
}