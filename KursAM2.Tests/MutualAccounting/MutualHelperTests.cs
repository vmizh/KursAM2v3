using System.Collections.ObjectModel;
using System.Linq;
using Core.ViewModel.MutualAccounting;
using KursAM2.ViewModel.Finance;
using NUnit.Framework;

namespace KursAM2.Tests.MutualAccounting
{
    [TestFixture]
    public class MutualHelperTests
    {
        [Test]
        public void EqualityTest()
        {
            var first = new TD_110ViewModel
            {
                DocCode = 1000,
                Code = 1
            };

            var second = new TD_110ViewModel
            {
                DocCode = 1000,
                Code = 1
            };
            Assert.AreEqual(first, second, "Не прошло сравнение по эквивалентности");

            var f = new MutualAccountingDebitorCreditors.MutualAccountingDebitorViewModel(first);
            var s = new MutualAccountingDebitorCreditors.MutualAccountingDebitorViewModel(second);
            Assert.AreEqual(f, s, "Не прошло сравнение по эквивалентности");
            f.Code = 1;
            second.Code = 2;
            Assert.AreNotEqual(first, second, "Не прошло сравнение по эквивалентности");
            Assert.AreNotEqual(f, s, "Не прошло сравнение по эквивалентности");

            var col = new ObservableCollection<TD_110ViewModel>
            {
                new TD_110ViewModel
                {
                    DocCode = 1000,
                    Code = 1
                },
                new TD_110ViewModel
                {
                    DocCode = 1000,
                    Code = 2
                },
                new TD_110ViewModel
                {
                    DocCode = 1000,
                    Code = 3
                },
                new TD_110ViewModel
                {
                    DocCode = 1000,
                    Code = 4
                }
            };

            var delrow = col.FirstOrDefault(_ => _.Code == 3);
            col.Remove(delrow);
            Assert.AreEqual(col.Count, 3, "Не удалилась запись");
            Assert.AreEqual(col.Contains(delrow), false, "Удалилась не та запись");
        }
    }
}