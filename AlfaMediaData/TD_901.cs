//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AlfaMediaData
{
    using System;
    using System.Collections.Generic;
    
    public partial class TD_901
    {
        public decimal DOC_CODE { get; set; }
        public Nullable<int> TABELNUMBER { get; set; }
        public Nullable<System.DateTime> VV_DATE { get; set; }
        public Nullable<double> VV_VALUE { get; set; }
        public Nullable<decimal> VV_CO { get; set; }
        public Nullable<decimal> VV_DEPT { get; set; }
        public Nullable<int> VV_TYPE { get; set; }
    
        public virtual SD_901 SD_901 { get; set; }
    }
}
