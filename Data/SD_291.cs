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
    
    public partial class SD_291
    {
        public decimal DOC_CODE { get; set; }
        public int OZO_NUM { get; set; }
        public System.DateTime OZO_DATE { get; set; }
        public int TABELNUMBER { get; set; }
        public decimal OZO_ZAPISKA_DC { get; set; }
        public string OZO_REASON { get; set; }
        public System.DateTime OZO_FROM { get; set; }
        public int OZO_DOLZH { get; set; }
    
        public virtual SD_13 SD_13 { get; set; }
        public virtual SD_2 SD_2 { get; set; }
        public virtual TD_17 TD_17 { get; set; }
    }
}
