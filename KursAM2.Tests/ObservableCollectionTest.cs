using System.Collections.ObjectModel;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace KursAM2.Tests
{
    public class Data
    {
        public int Id { set; get; }
        public string Name { set; get; } 
    }
    [TestFixture]
    public class ObservableCollectionTest
    {
        public ObservableCollection<Data> DataList { set; get; } = new ObservableCollection<Data>(); 
        [Test]
        public void ObservableCollectionAddTest()
        {

            DataList.Clear();
            DataList.Add(new Data
            {
                Id =  1,
                Name = "Name 1"
            });
            Assert.AreEqual(DataList.Count,1);

        }
    }
}