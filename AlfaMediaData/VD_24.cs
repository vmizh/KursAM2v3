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
    
    public partial class VD_24
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VD_24()
        {
            this.TD_394 = new HashSet<TD_394>();
        }
    
        public decimal DOC_CODE { get; set; }
        public int CODE { get; set; }
        public Nullable<decimal> DDV_NOMENKL_DC { get; set; }
        public int DDV_RASH_ROW_CODE { get; set; }
        public decimal DDV_PRIH_DC { get; set; }
        public int DDV_PRIH_ROW_CODE { get; set; }
        public double DDV_KOL { get; set; }
        public Nullable<decimal> DDV_NOM_CRS_PRICE { get; set; }
        public Nullable<decimal> DDV_UCH_CRS_PRICE { get; set; }
    
        public virtual SD_83 SD_83 { get; set; }
        public virtual TD_24 TD_24 { get; set; }
        public virtual TD_24 TD_241 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_394> TD_394 { get; set; }
    }
}
