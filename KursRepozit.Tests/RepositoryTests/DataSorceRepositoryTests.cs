using System;
using System.Linq;
using Core.Repository.Base;
using Data;
using KursRepozit.Repositories;
using NUnit.Framework;

namespace KursRepozit.Tests.RepositoryTests
{
    [TestFixture]
    public class DataSorceRepositoryTests
    {
        private readonly UnitOfWork<KursSystemEntities> 
            unitOfWork = new UnitOfWork<KursSystemEntities>();
        private GenericKursSystemRepository<DataSources> kursSystemRepository;
        private IDataSourcesRepository dataSourceRepository;
        [SetUp]
        public void SetUp()
        {
            kursSystemRepository = new GenericKursSystemRepository<DataSources>(unitOfWork);
            //If you want to use Specific KursSystemRepository with Unit of work
            dataSourceRepository = new DataSourcesKursSystemRepository(unitOfWork);
        }

        [Test]
        public void DataSourceCRUDTest()
        {
            try
            {
                unitOfWork.CreateTransaction();
                var findDS = dataSourceRepository.GetByName("Test1");
                if (findDS == null)
                {
                    var ds = new DataSources
                    {
                        Name = "Test1",
                        Color = "Black",
                        DBName = "DBB",
                        Order = 12,
                        Server = "Main8",
                        ShowName = "Новая база"
                    };
                    kursSystemRepository.Insert(ds);
                    unitOfWork.Save();
                }
                
                var order = dataSourceRepository.GetByName("Test1");
                Console.WriteLine($@"Name = {order.Name}, Order = {order.Order}");

                kursSystemRepository.Delete(order);
                unitOfWork.Commit();

            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public void DataSourceGetTest()
        {
            try
            {
                var data = kursSystemRepository.GetAll().ToList();
                    Assert.AreNotEqual(data.Count,0,"Не нашел ничего");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
