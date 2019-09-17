using System;
using System.ComponentModel.DataAnnotations;

namespace LocalDataBase
{
    public class KontragentCash
    {
        [Key]
        public int Id { set; get; }
        public decimal DocCode { set; get; }
        public int Count { set; get; }
        public DateTime LastUpdate { set; get; }
        public string DBName { set; get; }
    }
}