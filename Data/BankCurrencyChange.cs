//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class BankCurrencyChange
    {
        public System.Guid Id { get; set; }
        public System.DateTime DocDate { get; set; }
        public decimal BankFromDC { get; set; }
        public decimal BankToDC { get; set; }
        public decimal SummaFrom { get; set; }
        public decimal SummaTo { get; set; }
        public Nullable<decimal> Rate { get; set; }
        public decimal CrsFromDC { get; set; }
        public decimal CrsToDC { get; set; }
        public string Note { get; set; }
        public decimal DocFromDC { get; set; }
        public int DocRowFromCode { get; set; }
        public decimal DocToDC { get; set; }
        public int DocRowToCode { get; set; }
        public int DocNum { get; set; }
        public string CREATOR { get; set; }
    
        public virtual SD_114 SD_114 { get; set; }
        public virtual SD_114 SD_1141 { get; set; }
        public virtual SD_301 SD_301 { get; set; }
        public virtual SD_301 SD_3011 { get; set; }
    }
}
