using System;
using Core;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using NUnit.Framework;

namespace KursAM2.Tests.EntityViewModels
{
    [TestFixture]
    public class ProjectsTests : TestBase
    {
        [Test]
        public void CRUDProjectTest()
        {
            GlobalOptions.GetEntities();
            var prj1Id = Guid.NewGuid();
            var prj1 = new Project()
            {
                State = RowStatus.NewRow,
                Id = prj1Id,
                Name = "Test1",
                Note = "Note1",
                DateEnd = new DateTime(2017,1,1),
                IsClosed = false,
                IsDeleted = false
            };
            prj1.UpdateTo(prj1.Entity);
            prj1.Save(prj1.Entity);
            var prj1Old = new Project(prj1.Load(prj1.Id)) {State = RowStatus.NotEdited};
            Assert.AreEqual(prj1.Name,prj1Old.Name,"Проект не загружен или не сохранен в базе данных");
            prj1Old.Name = "Test2";
            prj1Old.DateEnd = DateTime.Today;
            prj1Old.IsClosed = true;
            var prj2Old = new Project(prj1.Load(prj1.Id));
            Assert.AreNotEqual(prj1.Name, prj1Old.Name, "Проект не обновился");
            prj2Old.Delete();
            var prj3Old = prj1.Load(prj1.Id);
            Assert.Null(prj3Old,"Проект не удален из базы данных");
        }
    }
}