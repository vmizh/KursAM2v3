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
    
    public partial class SD_287
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_287()
        {
            this.SD_269 = new HashSet<SD_269>();
            this.YD_2 = new HashSet<YD_2>();
        }
    
        public decimal DOC_CODE { get; set; }
        public string PRF_NAME { get; set; }
        public short PRF_DELETED { get; set; }
        public string PRF_CODE { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_269> SD_269 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YD_2> YD_2 { get; set; }
    }
}
