using System;
using NUnit.Framework;

namespace GenerateDataContract.Tests
{
    [TestFixture]
    public class DBStructureTests
    {
        private DataBaseContext myDBContext;
        [SetUp]
        public void Init()
        {
            DataBaseManager.Open("KURSLocal", "172.16.1.1", "ALFAMEDIA", "sa", ",juk.,bnyfc");
            myDBContext = DataBaseManager.ActiveDB;
        }
        [Test]
        public void GetXmlStructureTest()
        {
            var dbs = new DBStructure(myDBContext);
            var s = dbs.GetDBStructure();
            Assert.AreNotEqual(s,null);
            Console.WriteLine(s);
        }

        [Test]
        public void GetTablesTest()
        {
            var dbs = new DBStructure(myDBContext);
            var tbls = dbs.GetDBTables();
            Assert.AreNotEqual(tbls.Count,0);
            Console.WriteLine(tbls.Count);
        }

        [Test]
        public void GenerateDtoTest()
        {
            var dbs = new DBStructure(myDBContext);
            dbs.Generate();
        }
    }
}