using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLite.Base.Entity
{
    public class KontragentCash : IEntity
    {
        [Key]
        public int Id { set; get; }

        [Required]
        [Index("IX_KontragentCash_DocCode", IsUnique = true)]
        public decimal DocCode { set; get; }

        [Required]
        public int Count { set; get; }

        [Required]
        public DateTime LastUpdate { set; get; }

        [MaxLength(50)]
        [Required]
        public string DBName { set; get; }
    }
}