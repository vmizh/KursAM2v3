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
    
    public partial class TD_373
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TD_373()
        {
            this.UD_373 = new HashSet<UD_373>();
        }
    
        public decimal DOC_CODE { get; set; }
        public int CODE { get; set; }
        public decimal SFT_NOMENKL_DC { get; set; }
        public double SFT_KOL { get; set; }
        public Nullable<decimal> SFT_CENA { get; set; }
        public Nullable<decimal> SFT_ACCIZ_V_CENE { get; set; }
        public Nullable<decimal> SFT_SUMMA_BEZ { get; set; }
        public Nullable<decimal> SFT_ACCIZ_V_SUMME { get; set; }
        public double SFT_NDS_PROC { get; set; }
        public Nullable<decimal> SFT_NDS_SUMMA { get; set; }
        public Nullable<decimal> SFT_NP_SUMMA { get; set; }
        public Nullable<decimal> SFT_SUMMA_K_OPLATE { get; set; }
        public string SFT_NOTES { get; set; }
        public string SFT_STRANA_PROIS { get; set; }
        public string SFT_N_GRUZ_DECLAR { get; set; }
        public string SFT_HS_CODE { get; set; }
    
        public virtual SD_373 SD_373 { get; set; }
        public virtual SD_83 SD_83 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_373> UD_373 { get; set; }
    }
}
