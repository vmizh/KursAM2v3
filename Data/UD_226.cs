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
    
    public partial class UD_226
    {
        public decimal DOC_CODE { get; set; }
        public int CODE { get; set; }
        public decimal ACTRU_NOMENKL_DC { get; set; }
        public double ACTRU_KOL { get; set; }
        public Nullable<decimal> ACTRU_UCH_CRS_CENA { get; set; }
        public string ACTRU_NOTES { get; set; }
    
        public virtual SD_226 SD_226 { get; set; }
        public virtual SD_83 SD_83 { get; set; }
    }
}
