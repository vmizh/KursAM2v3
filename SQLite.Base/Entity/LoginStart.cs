using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;


namespace SQLite.Base.Entity
{
    public class LoginStart : IEntity
    {
        [Key]
        public int Id { get; set; }
        public byte[] Avatar { get; set; }
        [Index("UIX_LoginStart_Login", IsUnique = true)]
        public string Login { set; get; }
        public DateTime LastDataTime { set; get; }
    }
}