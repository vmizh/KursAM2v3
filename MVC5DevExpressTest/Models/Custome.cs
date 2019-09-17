using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC5DevExpressTest.Models
{
    public class Custome
    {
        public Guid Id { set; get; }
        public string ContactName { set; get; }
        public string CompanyName { set; get; }
        public string City { set; get; }
        public string Region { set; get; }
        public string Country { set; get; }
    }
}