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
    
    public partial class SD_307
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_307()
        {
            this.SD_310 = new HashSet<SD_310>();
            this.SD_61 = new HashSet<SD_61>();
            this.TD_307 = new HashSet<TD_307>();
        }
    
        public decimal DOC_CODE { get; set; }
        public string METH_NAME { get; set; }
        public short METH_DELETED { get; set; }
        public short METH_100 { get; set; }
        public Nullable<int> OP_CODE { get; set; }
        public string METH_INCLUDE { get; set; }
        public Nullable<decimal> METH_STAJ_DC { get; set; }
    
        public virtual SD_208 SD_208 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_310> SD_310 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SD_61> SD_61 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_307> TD_307 { get; set; }
    }
}
