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
    
    public partial class BD_83
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BD_83()
        {
            this.TD_194 = new HashSet<TD_194>();
            this.UD_373 = new HashSet<UD_373>();
            this.XD_24 = new HashSet<XD_24>();
            this.YD_27 = new HashSet<YD_27>();
        }
    
        public decimal DOC_CODE { get; set; }
        public int CODE { get; set; }
        public string PARTIA_NOM { get; set; }
        public Nullable<double> NOMB_VHOD_KOL { get; set; }
        public Nullable<decimal> NOMB_PASPORT_KACH_DC { get; set; }
    
        public virtual SD_435 SD_435 { get; set; }
        public virtual SD_83 SD_83 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_194> TD_194 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_373> UD_373 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<XD_24> XD_24 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YD_27> YD_27 { get; set; }
    }
}
