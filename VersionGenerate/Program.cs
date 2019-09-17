using System;
using System.IO;
using System.Xml;

namespace VersionGenerate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var curDir = Directory.GetCurrentDirectory();
            string fVerName = $"{curDir}\\Version.xml";
            string fVerNameErr = $"{curDir}\\error.txt";
            try
            {
                if (args[0] != "Release") return;
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(fVerName);
                var nodeList = xmlDoc.DocumentElement?.SelectNodes("/Version/Version");
                if (nodeList != null)
                    foreach (XmlNode node in nodeList)
                    {

                        var ver = Convert.ToInt32(node.InnerText);
                        node.InnerText = Convert.ToString(ver + 1);
                    }
                xmlDoc.Save(fVerName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                File.WriteAllText(fVerNameErr, fVerName + " / " + ex.Message);
            }
        }
    }
}