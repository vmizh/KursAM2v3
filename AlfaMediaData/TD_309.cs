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
    
    public partial class TD_309
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TD_309()
        {
            this.TD_122 = new HashSet<TD_122>();
        }
    
        public int CODE { get; set; }
        public decimal DOC_CODE { get; set; }
        public Nullable<System.DateTime> OSDT_PREV_ACT_DATE { get; set; }
        public decimal OSDT_OS_DC { get; set; }
        public decimal OSDT_OLD_COST { get; set; }
        public decimal OSDT_CRS_OLD_COST { get; set; }
        public decimal OSDT_NEW_COST { get; set; }
        public decimal OSDT_CRS_NEW_COST { get; set; }
        public decimal OSDT_OLD_AMORT { get; set; }
        public decimal OSDT_CRS_OLD_AMOR { get; set; }
        public decimal OSDT_NEW_AMOR { get; set; }
        public decimal OSDT_CRS_NEW_AMOR { get; set; }
        public short OSDT_1SAVERATIO_0NO { get; set; }
        public Nullable<short> OSDT_1UVELICH_0UMENSH { get; set; }
        public Nullable<int> OSDT_OLD_SHIFR { get; set; }
        public Nullable<int> OSDT_NEW_SHIFR { get; set; }
    
        public virtual SD_122 SD_122 { get; set; }
        public virtual SD_309 SD_309 { get; set; }
        public virtual TD_116 TD_116 { get; set; }
        public virtual TD_116 TD_1161 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_122> TD_122 { get; set; }
    }
}
