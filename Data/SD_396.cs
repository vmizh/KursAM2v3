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
    
    public partial class SD_396
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_396()
        {
            this.EXT_USERS = new HashSet<EXT_USERS>();
            this.SD_43 = new HashSet<SD_43>();
        }
    
        public decimal DOC_CODE { get; set; }
        public string GRD_GROUP_NAME { get; set; }
        public short GRD_DEFINE { get; set; }
        public Nullable<short> GRD_SKIDKI { get; set; }
        public Nullable<int> GRD_BALANS { get; set; }
        public Nullable<short> GRD_ADD_NEW { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EXT_USERS> EXT_USERS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_43> SD_43 { get; set; }
    }
}
