using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using GenerateDataContract.SqlTable;
using JetBrains.Annotations;

namespace GenerateDataContract
{
    public class DBStructure
    {
        private readonly DataBaseContext myDBContext;
        public List<Table> SqlTables { set; get; } 

        public DBStructure(DataBaseContext dbctx)
        {
            myDBContext = dbctx;
        }
        private string GenerateHeader()
        {
            var sb = new StringBuilder();
            sb.Append("using System; \n");
            sb.Append("using System.Collections.Generic; \n");
            sb.Append("using System.Runtime.Serialization; \n");
            sb.Append("using System.Web.UI.WebControls; \n");
            sb.Append("\n namespace DataContract \n { \n");
            return sb.ToString();
        }
        private string GenerateFooter()
        {
            var sb = new StringBuilder();
            sb.Append("} \n");
            return sb.ToString();
        }
        public XElement GetDBStructure()
        {
            if (myDBContext == null)
                throw new NullReferenceException("DataBaseContext never be null");
            return GetDBStructure(myDBContext);
        }
        private XElement GetDBStructure(DataBaseContext dbctx)
        {
            XElement ret = null;
            var cmd = new SqlCommand
            {
                CommandText = "SELECT   s.name AS '@Schema',   t.name AS '@Name',   t.object_id AS '@Id',   " +
                              "(SELECT     c.name AS '@Name',     c.column_id AS '@Id', 	c.system_type_id as '@TypeId', 	" +
                              "systyp.name as '@TypeName',     IIF(ic.object_id IS NOT NULL, 1, 0) AS '@IsPrimaryKey',    " +
                              " c.is_nullable AS '@IsCanNull',     c.max_length AS '@Length',     c.scale AS '@Scale',     " +
                              "c.precision AS '@Precision',     fkc.referenced_object_id AS '@ColumnReferencesTableId',     " +
                              "fkc.referenced_column_id AS '@ColumnReferencesTableColumnId',tbl.name as '@ParentTableName'   " +
                              "FROM sys.columns AS c   " +
                              "inner join sys.types as typ on c.user_type_id= typ.user_type_id " +
                              "inner join sys.types as systyp on c.system_type_id = systyp.system_type_id and systyp.is_user_defined=0 and systyp.user_type_id = systyp.system_type_id   " +
                              "LEFT OUTER JOIN sys.index_columns AS ic     ON c.object_id = ic.object_id AND c.column_id = ic.column_id AND ic.index_id = 1   " +
                              "LEFT OUTER JOIN sys.foreign_key_columns AS fkc ON c.object_id = fkc.parent_object_id     AND c.column_id = fkc.parent_column_id   " +
                              "Left outer join sys.tables tbl on tbl.[object_id] = fkc.referenced_object_id " +
                              "WHERE c.object_id = t.object_id   FOR XML PATH ('Column'), TYPE) " +
                              "FROM sys.schemas AS s " +
                              "INNER JOIN sys.tables AS t ON s.schema_id = t.schema_id " +
                              "FOR XML PATH ('Table'), ROOT ('Tables')",
                Connection = dbctx.Connect
            };
            try
            {
                using (var rdr = cmd.ExecuteReader())
                {
                    var sb = new StringBuilder();
                    while (rdr.Read())
                    {
                        sb.Append(rdr[0]);
                    }
                    var doc = XElement.Parse(sb.ToString());
                    ret = doc;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ret;
        }
        public List<Table> GetDBTables()
        {
            SqlTables = new List<Table>();
            var xml = GetDBStructure();
            foreach (var tbl in xml.Descendants("Table"))
            {
                var tb = new Table
                {
                    Name = tbl.Attribute("Name")?.Value
                };

                foreach (var col in tbl.Descendants("Column"))
                {
                    var fk = col.Attribute("ParentTableName")?.Value;
                    if (fk != null && tb.Keys.All(_ => _.Name != fk))
                        tb.Keys.Add(new ForeignKey
                        {
                            Name = fk,
                            ColumnName = col.Attribute("Name")?.Value
                        });
                    if (tb.Columns.Any(_ => _.Name == col.Attribute("Name")?.Value)) continue;
                    tb.Columns.Add(new Column
                    {
                        Name = col.Attribute("Name")?.Value,
                        IsNull = Convert.ToBoolean(col.Attribute("IsCanNull")?.Value != "0"),
                        TypeName = col.Attribute("TypeName")?.Value.ToLower()
                    });
                }
                //if (tb.Name == "TD_212")
                //{
                //    var i = 1;
                //}
                SqlTables.Add(tb);
            }
            foreach (var t in SqlTables)
            {
                foreach (var tt in SqlTables)
                {
                    foreach (var fk in tt.Keys.Where(_ => _.Name == t.Name))
                    {
                        t.ChildKeys.Add(new ForeignKey
                        {
                            Name = tt.Name,
                            ColumnName = tt.Name == t.Name ? "Parent" : fk.ColumnName
                        });
                    }
                }
            }
            return SqlTables;
        }
        public static string GetCTypeFromSqlType(string sqltype, bool isnull = true)
        {
            switch (sqltype)
            {
                case "numeric":
                case "decimal":
                case "money":
                case "smallmoney":
                    return isnull ? "decimal?" : "decimal";
                case "varchar":
                case "nvarchar":
                case "char":
                case "text":
                case "nchar":
                case "ntext":
                    return "string";
                case "uniqueidentifier":
                    return isnull ? "Guid?" : "Guid";
                case "binary":
                case "image":
                case "rowversion":
                case "timestamp":
                case "varbinary":
                    return "byte[]";
                case "bigint":
                    return isnull ? "long?" : "long";
                case "int":
                    return isnull ? "int?" : "int";
                case "smallint":
                    return isnull ? "Int16?" : "Int16";
                case "bit":
                    return isnull ? "bool?" : "bool";
                case "tinyint":
                    return isnull ? "byte?" : "byte";
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return isnull ? "DateTime?" : "DateTime";
                case "float":
                    return isnull ? "double?" : "double";
                case "real":
                    return isnull ? "Single?" : "Single";
                case "xml":
                    return "Xml";
                default:
                    return "unknown";
            }
        }
        private string GenerateColumn([NotNull] Column col)
        {
            var sb = new StringBuilder();
            sb.Append("[DataMember] \n");
            sb.Append($"public {GetCTypeFromSqlType(col.TypeName, col.IsNull)} {col.Name} {{ set; get; }} \n");
            return sb.ToString();
        }
        private string GenerateParent(ForeignKey fkey, bool isColNameAccept = false)
        {
            var sb = new StringBuilder();
            sb.Append("[DataMember] \n");
            var pname = isColNameAccept ? $"{fkey.ColumnName}_{fkey.Name}" : $"{fkey.Name}";
            sb.Append($"public virtual {fkey.Name} {pname} {{set; get;}}  \n");
            return sb.ToString();
        }
        private string GenerateChild(ForeignKey fkey, bool isColNameAccept = false)
        {
            var sb = new StringBuilder();
            sb.Append("[DataMember] \n");
            var pname = isColNameAccept ? $"{fkey.ColumnName}_{fkey.Name}" : $"{fkey.Name}";
            sb.Append($"public virtual List<{fkey.Name}> {pname} {{set; get;}} = new List<{fkey.Name}>(); \n");
            return sb.ToString();
        }
        public void GenerateDto([NotNull] Table tbl)
        {
            var sb = new StringBuilder();
            sb.Append("[DataContract] \n");
            sb.Append($"public partial class {tbl.Name} \n {{ \n");
            foreach (var col in tbl.Columns)
            {
                sb.Append(GenerateColumn(col));
            }
            if (tbl.Keys.Count > 0)
            {
                var kgrps = (tbl.Keys.GroupBy(t => t.Name).Select(g => new {g, count = g.Count()}).Select(@t1 => new
                {
                    @t1.g.Key,
                    Count = @t1.count
                })).ToList();
                foreach (var k in tbl.Keys)
                {
                    if (k.Name == tbl.Name)
                    {
                        sb.Append(GenerateParent(k, true));
                        continue;
                    }
                    sb.Append(kgrps.Any(_ => _.Key == k.Name && _.Count > 1)
                        ? GenerateParent(k, true)
                        : GenerateParent(k));
                }
            }
            if (tbl.ChildKeys.Count > 0)
            {
                var kgrps =
                    (tbl.ChildKeys.GroupBy(t => t.Name).Select(g => new {g, count = g.Count()}).Select(@t1 => new
                    {
                        @t1.g.Key,
                        Count = @t1.count
                    })).ToList();
                foreach (var k in tbl.ChildKeys)
                {
                    if (k.Name == tbl.Name)
                    {
                        sb.Append(GenerateChild(k, true));
                        continue;
                    }
                    sb.Append(kgrps.Any(_ => _.Key == k.Name && _.Count > 1)
                        ? GenerateChild(k, true)
                        : GenerateChild(k));
                }
            }
            sb.Append(" }");
            tbl.TextClassDto = sb.ToString();
        }
        public string Generate()
        {
            var sb = new StringBuilder();
            Console.WriteLine("Контракты: Заголовок");
            sb.Append(GenerateHeader());
            sb.Append(GenerateAllDto(GetDBTables()));
            Console.WriteLine("Контракты: Конец");
            sb.Append(GenerateFooter());
            return sb.ToString();
        }
        public string GenerateAllDto(List<Table> tbls)
        {
            var sb = new StringBuilder();
            foreach (var tbl in tbls)
            {
                Console.WriteLine("Контракты: {0}",tbl.Name);
                GenerateDto(tbl);
                sb.Append($"{tbl.TextClassDto} \n");
            }
            return sb.ToString();
        }

    }
}