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
    
    public partial class TD_116
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TD_116()
        {
            this.PD_116 = new HashSet<PD_116>();
            this.SD_122 = new HashSet<SD_122>();
            this.SD_1221 = new HashSet<SD_122>();
            this.SD_161 = new HashSet<SD_161>();
            this.TD_122 = new HashSet<TD_122>();
            this.TD_309 = new HashSet<TD_309>();
            this.TD_3091 = new HashSet<TD_309>();
            this.WD_116 = new HashSet<WD_116>();
        }
    
        public int CODE { get; set; }
        public string AMT_NAME { get; set; }
        public short AMT_DELETED { get; set; }
        public short AMT_TYPE { get; set; }
        public string AMT_SHIFR { get; set; }
        public double AMT_PERCENT { get; set; }
        public decimal DOC_CODE { get; set; }
        public Nullable<short> AMT_TRANSPORT_FLAG { get; set; }
        public Nullable<short> AMT_NONMAT_FLAG { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PD_116> PD_116 { get; set; }
        public virtual SD_116 SD_116 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_122> SD_122 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_122> SD_1221 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_161> SD_161 { get; set; }
        public virtual UD_116 UD_116 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_122> TD_122 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_309> TD_309 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_309> TD_3091 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WD_116> WD_116 { get; set; }
    }
}
