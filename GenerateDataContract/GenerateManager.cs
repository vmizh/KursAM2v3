using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GenerateDataContract
{
    public class GenerateManager
    {
        private string OutputFileName { set; get; }
        private string WrapperFile { set; get; }

        private List<string> ExcludeTableName { set; get; } = new List<string>();

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string DBName { set; get; }

        public void LoadConfig(string filename)
        {
            try
            {
                var cfg = XDocument.Load(filename);
                var d = cfg.Descendants("DataBase").FirstOrDefault();
                if (d != null)
                {
                    DataBaseManager.Open(d.Attribute("Name")?.Value, new DataBaseContext
                    {
                        Name = d.Attribute("Name")?.Value,
                        DataBaseName = d.Attribute("DataBaseName")?.Value,
                        Source = d.Attribute("Source")?.Value,
                        UserName = d.Attribute("UserName")?.Value,
                        Password = d.Attribute("Password")?.Value,
                    });
                    DBName = DataBaseManager.ActiveDB.Name;
                }
                var f = cfg.Descendants("OutputFile").FirstOrDefault();
                if (f != null)
                    OutputFileName = f.Attribute("FileName")?.Value;
                var w = cfg.Descendants("WrapperFile").FirstOrDefault();
                if (w != null)
                    WrapperFile = w.Attribute("FileName")?.Value;

                var ef = cfg.Descendants("ExcludeTable").FirstOrDefault();
                if (ef != null)
                {
                    foreach (var efn in ef.Descendants("TableName"))
                    {
                        ExcludeTableName.Add(efn.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void LoadConfig()
        {
            try
            {
                var cfg = XDocument.Load("GenerateConfig.xml");
                var d = cfg.Descendants("DataBase").FirstOrDefault();
                if (d != null)
                {
                    DataBaseManager.Open(d.Attribute("Name")?.Value, new DataBaseContext
                    {
                        Name = d.Attribute("Name")?.Value,
                        DataBaseName = d.Attribute("DataBaseName")?.Value,
                        Source = d.Attribute("Source")?.Value,
                        UserName = d.Attribute("UserName")?.Value,
                        Password = d.Attribute("Password")?.Value,
                    });
                    DBName = DataBaseManager.ActiveDB.Name;
                }
                var f = cfg.Descendants("OutputFile").FirstOrDefault();
                if (f != null)
                    OutputFileName = f.Attribute("FileName")?.Value;
                var w = cfg.Descendants("WrapperFile").FirstOrDefault();
                if (w != null)
                    WrapperFile = w.Attribute("FileName")?.Value;
                var ef = cfg.Descendants("ExcludeTable").FirstOrDefault();
                if (ef != null)
                {
                    foreach (var efn in ef.Descendants("TableName"))
                    {
                        ExcludeTableName.Add(efn.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        public void Generate()
        {
            try
            {
                var dbs = new DBStructure(DataBaseManager.ActiveDB);
                using (var f = File.Create(OutputFileName))
                {
                    {
                        using (StreamWriter sw = new StreamWriter(f))
                        {
                            Console.WriteLine("Начало генерации контрактов");
                            sw.Write(dbs.Generate());
                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
                var wrap = new GenerateWrapper(dbs.SqlTables, ExcludeTableName);
                using (var w = File.Create(WrapperFile))
                {
                    {
                        using (StreamWriter sw2 = new StreamWriter(w))
                        {
                            sw2.Write(wrap.Generate());
                            sw2.Flush();
                            sw2.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}