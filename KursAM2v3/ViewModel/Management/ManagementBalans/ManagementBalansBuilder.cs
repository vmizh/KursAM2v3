using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Data;

namespace KursAM2.ViewModel.Management.ManagementBalans
{
    public static class ManagemenentBalansStructrue
    {
        public static Guid Root = Guid.Parse("{9DC33178-1DAA-4A65-88BD-E1AD617B12D9}");
        public static Guid MoneyInPah = Guid.Parse("{CF0D1836-59BF-4749-969A-63135522B2D0}");
        public static Guid TovarInPath = Guid.Parse("{47741975-8DAA-4B15-AA4D-209E6373AD5E}");
    }

    public class ManagementBalansBuilder
    {
        public readonly ObservableCollection<ManagementBalanceGroupViewModel> Structure;

        // ReSharper disable once UnusedMember.Local
        private List<CURRENCY_RATES_CB> myRates = new List<CURRENCY_RATES_CB>();

        public ManagementBalansBuilder()
        {
            Structure = new ObservableCollection<ManagementBalanceGroupViewModel>(GetBalansStructure());
        }

        private List<ManagementBalanceGroupViewModel> GetBalansStructure()
        {
            var ret = new List<ManagementBalanceGroupViewModel>
            {
                new ManagementBalanceGroupViewModel
                {
                    Id = ManagemenentBalansStructrue.Root,
                    ParentId = null,
                    Name = "Управленческий баланс",
                    Order = 0,
                    Summa = 0,
                    SummaEUR = 0,
                    SummaRUB = 0,
                    SummaUSD = 0,
                    Tag = BalansSection.Head
                },
                new ManagementBalanceGroupViewModel
                {
                    Id = Guid.Parse("{893EEF60-1CBF-4F4A-8AE6-97BA710610BE}"),
                    ParentId = Guid.Parse("{9DC33178-1DAA-4A65-88BD-E1AD617B12D9}"),
                    Name = "Основные средства",
                    Order = 1,
                    Summa = 0,
                    SummaEUR = 0,
                    SummaRUB = 0,
                    SummaUSD = 0,
                    Tag = BalansSection.MainInventory
                },
                new ManagementBalanceGroupViewModel
                {
                    Id = Guid.Parse("{9322A87D-EFAF-47BB-9E14-B7C3DE6D89DA}"),
                    ParentId = ManagemenentBalansStructrue.Root,
                    Name = "Касса",
                    Order = 1,
                    Summa = 0,
                    SummaEUR = 0,
                    SummaRUB = 0,
                    SummaUSD = 0,
                    Tag = BalansSection.Cash
                },
                new ManagementBalanceGroupViewModel
                {
                    Id = Guid.Parse("{A5FE741F-159B-43C6-9FF0-87368B7B27E9}"),
                    ParentId = ManagemenentBalansStructrue.Root,
                    Name = "Банк",
                    Order = 1,
                    Summa = 0,
                    SummaEUR = 0,
                    SummaRUB = 0,
                    SummaUSD = 0,
                    Tag = BalansSection.Bank
                },
                new ManagementBalanceGroupViewModel
                {
                    Id = Guid.Parse("{324DED20-90E1-4D7B-8A8A-017A6261A659}"),
                    ParentId = ManagemenentBalansStructrue.Root,
                    Name = "Дебиторы",
                    Order = 1,
                    Summa = 0,
                    SummaEUR = 0,
                    SummaRUB = 0,
                    SummaUSD = 0,
                    Tag = BalansSection.Debitors
                },
                new ManagementBalanceGroupViewModel
                {
                    Id = Guid.Parse("{5B7AE408-9A46-4A5A-B476-E3716880CB4E}"),
                    ParentId = ManagemenentBalansStructrue.Root,
                    Name = "Кредиторы",
                    Order = 1,
                    Summa = 0,
                    SummaEUR = 0,
                    SummaRUB = 0,
                    SummaUSD = 0,
                    Tag = BalansSection.Creditors
                },
                new ManagementBalanceGroupViewModel
                {
                    Id = Guid.Parse("{8B02D6AA-C65E-4D93-8415-7C48EC9E9691}"),
                    ParentId = ManagemenentBalansStructrue.Root,
                    Name = "Материалы на складах",
                    Order = 1,
                    Summa = 0,
                    SummaEUR = 0,
                    SummaRUB = 0,
                    SummaUSD = 0,
                    Tag = BalansSection.Store
                },
                new ManagementBalanceGroupViewModel
                {
                    Id = Guid.Parse("{0A7270FC-C521-4997-986F-BD10955E31B5}"),
                    ParentId = ManagemenentBalansStructrue.Root,
                    Name = "Неотфактурованные поставки",
                    Order = 1,
                    Summa = 0,
                    SummaEUR = 0,
                    SummaRUB = 0,
                    SummaUSD = 0,
                    Tag = BalansSection.NotFactPrihod
                },
                new ManagementBalanceGroupViewModel
                {
                    Id = Guid.Parse("{8706DE77-5D81-489F-8800-29B535E32C0B}"),
                    ParentId = ManagemenentBalansStructrue.Root,
                    Name = "Подотчет",
                    Order = 1,
                    Summa = 0,
                    SummaEUR = 0,
                    SummaRUB = 0,
                    SummaUSD = 0,
                    Tag = BalansSection.UnderReport
                },
                new ManagementBalanceGroupViewModel
                {
                    Id = Guid.Parse("{4E88F0DF-BC0A-46CB-8038-912615DC4488}"),
                    ParentId = ManagemenentBalansStructrue.Root,
                    Name = "Заработная плата",
                    Order = 1,
                    Summa = 0,
                    SummaEUR = 0,
                    SummaRUB = 0,
                    SummaUSD = 0,
                    Tag = BalansSection.Salary
                },
                new ManagementBalanceGroupViewModel
                {
                    Id = ManagemenentBalansStructrue.MoneyInPah,
                    ParentId = ManagemenentBalansStructrue.Root,
                    Name = "Деньги в пути",
                    Order = 1,
                    Summa = 0,
                    SummaEUR = 0,
                    SummaRUB = 0,
                    SummaUSD = 0,
                    Tag = BalansSection.MoneyInPath
                }
            };
            return ret;
        }
    }
}