using System;

namespace DevExpress.Models.DataModel
{
    public class Customer
    {
        public Guid Id { set; get; }
        public string ContactName { set; get; }
        public string CompanyName { set; get; }
        public string City { set; get; }
        public string Region { set; get; }
        public string Country { set; get; }
    }
}