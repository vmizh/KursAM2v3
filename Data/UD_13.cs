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
    
    public partial class UD_13
    {
        public decimal DOC_CODE { get; set; }
        public System.DateTime OZU_MONTH { get; set; }
        public Nullable<decimal> OZU_SUMMA { get; set; }
        public Nullable<short> OZU_KOL_DAYS { get; set; }
    
        public virtual SD_13 SD_13 { get; set; }
    }
}
