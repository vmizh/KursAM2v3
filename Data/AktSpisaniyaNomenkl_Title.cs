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
    
    public partial class AktSpisaniyaNomenkl_Title
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AktSpisaniyaNomenkl_Title()
        {
            this.AktSpisaniya_row = new HashSet<AktSpisaniya_row>();
        }
    
        public System.Guid Id { get; set; }
        public decimal Warehouse_DC { get; set; }
        public int Num_Doc { get; set; }
        public System.DateTime Date_Doc { get; set; }
        public string Creator { get; set; }
        public string Reason_Creation { get; set; }
        public string Note { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AktSpisaniya_row> AktSpisaniya_row { get; set; }
        public virtual SD_27 SD_27 { get; set; }
    }
}