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
    
    public partial class TD_222
    {
        public decimal DOC_CODE { get; set; }
        public int CODE { get; set; }
        public decimal NALT_SHPZ_DC { get; set; }
        public decimal NALT_STDP_DC { get; set; }
    
        public virtual SD_165 SD_165 { get; set; }
        public virtual SD_222 SD_222 { get; set; }
        public virtual SD_303 SD_303 { get; set; }
    }
}
