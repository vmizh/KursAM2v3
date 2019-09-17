//using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace KursAM2.Tests.EntityViewModels
{
    [TestFixture]
    public class SD_111Tests : TestBase
    {
        [Test]
        public void CRUDSD_111()
        {
            var item1 = new SD_111ViewModel
            {
                State = RowStatus.NewRow,
                Name = "Test1",
                Note = "Note1",
                IsCurrencyConvert = false
            };
            item1.UpdateTo(item1.Entity);
            item1.Save(item1.Entity);
            var item1Old = new SD_111ViewModel(item1.Load(item1.DocCode)) {State = RowStatus.NotEdited};
            Assert.AreEqual(item1.Name, item1Old.Name,
                "Тип акта взаимозачета не загружен или не сохранен в базе данных");
            item1Old.Name = "Test2";
            item1Old.IsCurrencyConvert = true;
            item1Old.Save();
            var item2Old = new SD_111ViewModel(item1Old.Load(item1Old.DocCode));
            Assert.AreNotEqual(item1.Name, item2Old.Name, "Тип акта взаимозачета не обновился");
            item2Old.Delete();
            var item3Old = item1.Load(item1.DocCode);
            Assert.Null(item3Old, "Тип акта взаимозачета не удален из базы данных");
        }
    }
}