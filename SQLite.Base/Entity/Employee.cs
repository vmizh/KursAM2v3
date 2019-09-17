using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLite.Base.Entity
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        public decimal DocCode { set; get; }

        [Required]
        [Index("IX_Employee_DocCode", IsUnique = true)]
        public int TabelNumber { set; get; }

        [MaxLength(100)]
        public string NameFirst { set; get; }
        [MaxLength(100)]
        public string NameSecond { set; get; }
        [MaxLength(100)]
        public string NameLast { set; get; }

        public decimal CrsDC { set; get; }
        public string CrsName { set; get; }
        public string StatusNotes { set; get; }
    }
}