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
    
    public partial class TD_71
    {
        public decimal DOC_CODE { get; set; }
        public int CODE { get; set; }
        public decimal VVMT_NOMENKL_DC { get; set; }
        public double VVMT_KOL_VIDACHI { get; set; }
        public short VVMT_TAX_EXECUTED { get; set; }
        public double VVMT_KOL_VOZVRAT { get; set; }
        public System.DateTime VVMT_VIDACH_DATE { get; set; }
        public Nullable<System.DateTime> VVMT_VOZVRAT_DATE { get; set; }
        public decimal VVMT_SHPZ_DC { get; set; }
        public Nullable<int> TABELNUMBER { get; set; }
    
        public virtual SD_2 SD_2 { get; set; }
        public virtual SD_303 SD_303 { get; set; }
        public virtual SD_71 SD_71 { get; set; }
        public virtual SD_83 SD_83 { get; set; }
    }
}
