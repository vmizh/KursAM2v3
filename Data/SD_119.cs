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
    
    public partial class SD_119
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_119()
        {
            this.NomenklMain = new HashSet<NomenklMain>();
            this.SD_390 = new HashSet<SD_390>();
            this.SD_83 = new HashSet<SD_83>();
            this.UD_17 = new HashSet<UD_17>();
            this.UD_83 = new HashSet<UD_83>();
        }
    
        public decimal DOC_CODE { get; set; }
        public string MC_NAME { get; set; }
        public short MC_DELETED { get; set; }
        public Nullable<double> MC_PROC_OTKL { get; set; }
        public Nullable<short> MC_TARA { get; set; }
        public Nullable<short> MC_TRANSPORT { get; set; }
        public Nullable<short> MC_PREDOPLATA { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NomenklMain> NomenklMain { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_390> SD_390 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_83> SD_83 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_17> UD_17 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_83> UD_83 { get; set; }
    }
}
