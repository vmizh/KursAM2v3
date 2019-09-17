using System.Data;
using NUnit.Framework;

namespace GenerateDataContract.Tests
{
    [TestFixture]
    public class DataBaseCoreTests
    {
       
        [Test]
        public void DataBaseOpenCloseTest()
        {
            DataBaseManager.Open("KURSLocal","localhost","KURS","sa",",juk.,bnyfc");
            Assert.AreEqual(DataBaseManager.DBCollection.Count,1);
            DataBaseManager.Open("KURSLocal2", "localhost", "KURS", "sa", ",juk.,bnyfc");
            Assert.AreNotEqual(DataBaseManager.ActiveDB,null);
            Assert.AreEqual(DataBaseManager.ActiveDB.State,ConnectionState.Open);
            DataBaseManager.ActiveDB.Close();
            Assert.AreEqual(DataBaseManager.ActiveDB.State, ConnectionState.Closed);
            DataBaseManager.Remove("KURSLocal2");
            Assert.AreEqual(DataBaseManager.DBCollection.Count, 1);
        }
    }
}