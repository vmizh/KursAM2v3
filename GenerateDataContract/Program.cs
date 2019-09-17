using System;
using System.Linq;

namespace GenerateDataContract
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                //DataBaseManager.Open("KURSLocal", "localhost", "KURS", "sa", ",juk.,bnyfc");
                var gen = new GenerateManager();
                if (args != null && args.Any())
                {
                    gen.LoadConfig(args[0]);
                }
                else
                    gen.LoadConfig();
                gen.Generate();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}