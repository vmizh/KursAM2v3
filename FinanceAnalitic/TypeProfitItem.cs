using System;

namespace FinanceAnalitic
{
    public class TypeProfitItem
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
    }

    public class BankAccount : TypeProfitItem
    {
        public BankAccount()
        {
            Id = Guid.Parse("{0AD95635-A46D-49F2-AE78-CBDF52BD6E27}");
            Name = @"Банковские счета";

        }
    }

    public class CashAccount : TypeProfitItem
    {
        public CashAccount()
        {
            Id = Guid.Parse("{A084B37A-D942-4B7F-9AE9-3C3AAA0F4475}");
            Name = @"Банковские счета";
        }
    }
    
}
