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
    
    public partial class SD_7
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SD_7()
        {
            this.TD_46 = new HashSet<TD_46>();
            this.UD_235 = new HashSet<UD_235>();
        }
    
        public decimal DOC_CODE { get; set; }
        public string SPD_CODE { get; set; }
        public string SPD_NAME { get; set; }
        public short SPD_TYPE { get; set; }
        public Nullable<int> SPD_KOL { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TD_46> TD_46 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UD_235> UD_235 { get; set; }
    }
}
