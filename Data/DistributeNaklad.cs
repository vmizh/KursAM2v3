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
    
    public partial class DistributeNaklad
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DistributeNaklad()
        {
            this.DistributeNakladInvoices = new HashSet<DistributeNakladInvoices>();
            this.DistributeNakladRow = new HashSet<DistributeNakladRow>();
        }
    
        public System.Guid Id { get; set; }
        public System.DateTime DocDate { get; set; }
        public string DocNum { get; set; }
        public string Creator { get; set; }
        public string Note { get; set; }
        public Nullable<decimal> CurrencyDC { get; set; }
    
        public virtual SD_301 SD_301 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DistributeNakladInvoices> DistributeNakladInvoices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DistributeNakladRow> DistributeNakladRow { get; set; }
    }
}