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
    
    public partial class EMP_PAYROLL_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EMP_PAYROLL_TYPE()
        {
            this.EMP_PR_ROWS = new HashSet<EMP_PR_ROWS>();
        }
    
        public string ID { get; set; }
        public decimal DOC_CODE { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EMP_PR_ROWS> EMP_PR_ROWS { get; set; }
    }
}
