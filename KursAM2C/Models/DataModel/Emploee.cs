using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data;

namespace KursAM2C.Models.DataModel
{
    public class Emploee
    {
        public int Tablenumber { set; get; }
        public string FirstName { set; get;  }
        public string SecondName { set; get; }

        public string LastName { set; get; }
    }
}