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
    
    public partial class SD_434
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_434()
        {
            this.SD_23 = new HashSet<SD_23>();
            this.SD_437 = new HashSet<SD_437>();
        }
    
        public decimal DOC_CODE { get; set; }
        public string STR_NAME { get; set; }
        public string STR_FULL_NAME { get; set; }
        public string STR_INTERNATIONAL_NAME { get; set; }
        public string STR_NUM_KOD { get; set; }
        public string STR_ABC2 { get; set; }
        public string STR_ABC3 { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_23> SD_23 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_437> SD_437 { get; set; }
    }
}
