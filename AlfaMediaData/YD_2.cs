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
    
    public partial class YD_2
    {
        public int TABELNUMBER { get; set; }
        public int CODE { get; set; }
        public decimal DOC_CODE { get; set; }
        public string EDU_ALMAMATER { get; set; }
        public Nullable<short> EDU_YEAR { get; set; }
        public Nullable<decimal> EDU_SPECIALITY_DC { get; set; }
        public string EDU_SPECIALITY { get; set; }
        public Nullable<short> EDU_NOW_FLAG { get; set; }
        public Nullable<short> EDU_NOW_TYPE { get; set; }
        public string EDU_EDUC { get; set; }
        public string EDU_DEGREE { get; set; }
        public string EDU_QUALIF { get; set; }
        public Nullable<decimal> EDU_QUALIF_DC { get; set; }
        public Nullable<System.DateTime> EDU_DIPLOM_DATE { get; set; }
        public string EDU_DIPLOM_NUM { get; set; }
        public Nullable<short> EDU_MAIN_PROFILE { get; set; }
    
        public virtual SD_2 SD_2 { get; set; }
        public virtual SD_287 SD_287 { get; set; }
        public virtual SD_288 SD_288 { get; set; }
    }
}
