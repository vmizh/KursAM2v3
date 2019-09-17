using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace GenerateDataContract
{
    public class DataBaseManager
    {
        public static List<DataBaseContext> DBCollection { get; } = new List<DataBaseContext>();
        public static DataBaseContext ActiveDB { set; get; }

        public static void Open(string name, string source, string dbname, string uname, string pwd)
        {
            var db = new DataBaseContext
            {
                Name = name
            };
            db.Open(source, dbname, uname, pwd);
            if (DBCollection.Count == 0)
                ActiveDB = db;
            DBCollection.Add(db);
        }

        public static void Open(string name, DataBaseContext db)
        {
            db.Name = name;
            db.Open();
            if (DBCollection.Count == 0)
                ActiveDB = db;
            DBCollection.Add(db);
        }

        public static void Remove(string name)
        {
            var cn = DBCollection.FirstOrDefault(_ => _.Name == name);
            if (cn == null) return;
            if (cn.State != ConnectionState.Closed)
                cn.Close();
            DBCollection.Remove(cn);
        }
    }
}