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
    
    public partial class ZD_9
    {
        public decimal DOC_CODE { get; set; }
        public int CODE { get; set; }
        public Nullable<System.DateTime> ZAKZ_DATE { get; set; }
        public Nullable<decimal> ZAKZ_SUMMA { get; set; }
        public decimal ZAKZ_FORM_RASCH_DC { get; set; }
        public string ZAKZ_NOTES { get; set; }
    
        public virtual SD_189 SD_189 { get; set; }
        public virtual SD_9 SD_9 { get; set; }
    }
}
