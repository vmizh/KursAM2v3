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
    
    public partial class IMP_PUBLICATIONS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public IMP_PUBLICATIONS()
        {
            this.IMP_ARTICLES = new HashSet<IMP_ARTICLES>();
            this.IMP_CURRICDETAILS = new HashSet<IMP_CURRICDETAILS>();
            this.IMP_CURRICULUM = new HashSet<IMP_CURRICULUM>();
            this.IMP_EVENTS = new HashSet<IMP_EVENTS>();
            this.IMP_PARS = new HashSet<IMP_PARS>();
            this.IMP_TURNOFF = new HashSet<IMP_TURNOFF>();
        }
    
        public int PUB_ID { get; set; }
        public string PUB_NAME { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IMP_ARTICLES> IMP_ARTICLES { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IMP_CURRICDETAILS> IMP_CURRICDETAILS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IMP_CURRICULUM> IMP_CURRICULUM { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IMP_EVENTS> IMP_EVENTS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IMP_PARS> IMP_PARS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IMP_TURNOFF> IMP_TURNOFF { get; set; }
    }
}
