using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Data.Tests
{
    [TestFixture]
    public class EntityTests
    {
        [Test]
        public void TestNomenklMove()
        {
            using (var ent = new ALFAMEDIAEntities2())
            {
                var d = ent.H184000_DVIZH_LIC_SCHET_KONTR_TABLE(Convert.ToDecimal(10430000005), new DateTime(2015,1,1), new DateTime(2015,10,1)).ToList();
                Assert.AreNotEqual(d.Count, 0);
            }
        }
    }
}
