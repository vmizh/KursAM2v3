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
    
    public partial class TD_379
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TD_379()
        {
            this.SD_384 = new HashSet<SD_384>();
            this.VD_379 = new HashSet<VD_379>();
        }
    
        public decimal DOC_CODE { get; set; }
        public int CODE { get; set; }
        public decimal PPZT_NOMENKL_DC { get; set; }
        public double PPZT_PLAN_KOL { get; set; }
        public Nullable<decimal> PPZT_PLAN_CENA { get; set; }
        public Nullable<decimal> PPZT_PLAN_SUMMA { get; set; }
        public double PPZT_FACT_KOL { get; set; }
        public Nullable<decimal> PPZT_FACT_CENA { get; set; }
        public Nullable<decimal> PPZT_FACT_SUMMA { get; set; }
        public Nullable<decimal> PPZT_DELTA { get; set; }
    
        public virtual SD_379 SD_379 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_384> SD_384 { get; set; }
        public virtual SD_83 SD_83 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VD_379> VD_379 { get; set; }
    }
}
