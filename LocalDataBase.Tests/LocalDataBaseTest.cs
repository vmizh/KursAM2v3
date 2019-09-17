using System;
using System.Linq;
using NUnit.Framework;

namespace LocalDataBase.Tests
{
    [TestFixture]
    public class LocalDataBaseTest
    {
        [Test]
        public void GetDatabase()
        {
            try
            {
                using (var db = new LocalDBContext("LocalDB.db"))
                {
                    var d = db.KontragentCashes.FirstOrDefault(_ => _.Id == 2);
                    if (d != null)
                    {
                        d.Count = d.Count + 1;
                        d.LastUpdate = DateTime.Now;
                    }
                    else
                    {
                        db.KontragentCashes.Add(new KontragentCash
                        {
                            Id = 2,
                            DocCode = 10430000014,
                            Count = 5,
                            LastUpdate = DateTime.Now
                        });
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}