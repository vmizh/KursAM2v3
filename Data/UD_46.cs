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
    
    public partial class UD_46
    {
        public decimal DOC_CODE { get; set; }
        public short NCODE { get; set; }
        public decimal SHPZ_DC { get; set; }
        public decimal STDP_DC { get; set; }
    
        public virtual SD_303 SD_303 { get; set; }
        public virtual SD_46 SD_46 { get; set; }
        public virtual TD_46 TD_46 { get; set; }
    }
}
