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
    
    public partial class SD_141
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_141()
        {
            this.TD_141 = new HashSet<TD_141>();
        }
    
        public decimal DOC_CODE { get; set; }
        public System.DateTime ZAY_DATE { get; set; }
        public decimal ZAY_ZA_DC { get; set; }
        public decimal ZAY_SKLAD_DC { get; set; }
        public short ZAY_EXECUTED { get; set; }
        public string CREATOR { get; set; }
    
        public virtual SD_25 SD_25 { get; set; }
        public virtual SD_27 SD_27 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_141> TD_141 { get; set; }
    }
}
