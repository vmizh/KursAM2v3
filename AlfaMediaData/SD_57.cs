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
    
    public partial class SD_57
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_57()
        {
            this.SD_43 = new HashSet<SD_43>();
        }
    
        public decimal DOC_CODE { get; set; }
        public string OTR_NAME { get; set; }
        public Nullable<decimal> OTR_PARENT_DC { get; set; }
        public Nullable<int> OP_CODE { get; set; }
        public string OTR_IERARHY_NUM { get; set; }
        public string OTR_CODE { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_43> SD_43 { get; set; }
    }
}
