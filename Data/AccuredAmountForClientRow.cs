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
    
    public partial class AccuredAmountForClientRow
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AccuredAmountForClientRow()
        {
            this.ProjectDocuments = new HashSet<ProjectDocuments>();
        }
    
        public System.Guid Id { get; set; }
        public System.Guid DocId { get; set; }
        public decimal Summa { get; set; }
        public string Note { get; set; }
        public Nullable<decimal> CashDC { get; set; }
        public Nullable<int> BankCode { get; set; }
        public Nullable<decimal> SHPZ_DC { get; set; }
        public decimal NomenklDC { get; set; }
    
        public virtual AccruedAmountForClient AccruedAmountForClient { get; set; }
        public virtual SD_303 SD_303 { get; set; }
        public virtual TD_101 TD_101 { get; set; }
        public virtual SD_33 SD_33 { get; set; }
        public virtual SD_83 SD_83 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProjectDocuments> ProjectDocuments { get; set; }
    }
}
